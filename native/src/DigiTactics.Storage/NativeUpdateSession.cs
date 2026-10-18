using System.Text.Json;

namespace DigiTactics.Storage;

public sealed record NativeUpdateStatus(string Session,string Phase,string? Version=null,long Received=0,long Total=0,string? Page=null);
public sealed record NativeRestartRequest(string Session,string Version);

/// <summary>One automatic check per game lifetime. Run only after startup confirmation and join
/// the task before the supervisor begins another launch or disposes launcher state.</summary>
public sealed class NativeUpdateSession
{
    public const string DirectoryVariable="DIGITACTICS_UPDATE_DIRECTORY";
    public const string SessionVariable="DIGITACTICS_UPDATE_SESSION";
    internal static readonly JsonSerializerOptions Json=new() {PropertyNamingPolicy=JsonNamingPolicy.CamelCase};
    public string Id {get;}=Guid.NewGuid().ToString("N");
    public string DirectoryPath {get;}
    private string? readyVersion;
    private bool started;
    public NativeUpdateSession(string updatesDirectory)
    {
        DirectoryPath=Path.Combine(Path.GetFullPath(updatesDirectory),"sessions",Id);Directory.CreateDirectory(DirectoryPath);
        Write(new(Id,"idle"));
    }
    public async Task Run(NativeLaunchState state,string owner,string repository,string updatesDirectory,int launcherVersion,
        NativeReleaseDiscovery discovery,NativeUpdateDownloader downloader,CancellationToken cancellation)
    {
        if(started)throw new InvalidOperationException("Update session already checked.");started=true;
        try
        {
            Write(new(Id,"checking"));
            var release=await discovery.Check(owner,repository,state.CurrentVersion,launcherVersion,Path.Combine(updatesDirectory,"release-cache.json"),cancellation).ConfigureAwait(false);
            if(release is null || !state.CanDownload(release.Manifest.Version)) {Write(new(Id,"current"));return;}
            if(release.RequiresLauncherUpdate) {Write(new(Id,"manual",release.Manifest.Version,Page:release.Page.AbsoluteUri));return;}
            string version=release.Manifest.Version;
            Write(new(Id,"downloading",version,Total:release.Manifest.Size));
            var progress=new Progress(value=>Write(new(Id,"downloading",version,value.Received,value.Total)));
            string archive=await downloader.Download(release.Download,release.Expected,Path.Combine(updatesDirectory,"downloads"),launcherVersion,progress,cancellation).ConfigureAwait(false);
            Write(new(Id,"verifying",version));
            string candidates=Path.Combine(updatesDirectory,"candidates");
            var staged=NativeUpdatePackage.Stage(archive,candidates,release.Expected,launcherVersion,cancellation);
            using var lease=NativeUpdatePackage.OpenCandidate(candidates,staged.Release,launcherVersion,cancellation);
            cancellation.ThrowIfCancellationRequested();state.Queue(staged.Release);
            readyVersion=version;Write(new(Id,"ready",version));
        }
        catch(OperationCanceledException) when(cancellation.IsCancellationRequested) { }
        catch(Exception error) when(error is IOException or InvalidDataException or HttpRequestException or System.Text.Json.JsonException or OperationCanceledException or ArgumentException or InvalidOperationException or UnauthorizedAccessException or KeyNotFoundException or FormatException)
        {
            // Offline/rate-limited/invalid updates must never prevent playing the installed game.
            Console.Error.WriteLine("Native update check skipped: "+error.Message);
            try {Write(new(Id,"unavailable"));}catch(IOException) { }
        }
    }
    public bool RestartRequested()
    {
        string path=Path.Combine(DirectoryPath,"restart.json");
        try
        {
            if(readyVersion is null || !File.Exists(path) || new FileInfo(path).Length>4096)return false;
            var request=JsonSerializer.Deserialize<NativeRestartRequest>(File.ReadAllBytes(path),Json);
            return request?.Session==Id && request.Version==readyVersion;
        }
        catch(Exception error) when(error is IOException or JsonException or UnauthorizedAccessException) {return false;}
    }
    private void Write(NativeUpdateStatus value)=>AtomicWrite(Path.Combine(DirectoryPath,"status.json"),value);
    internal static void AtomicWrite<T>(string path,T value)
    {
        string temporary=path+"."+Guid.NewGuid().ToString("N")+".tmp";
        try {File.WriteAllBytes(temporary,JsonSerializer.SerializeToUtf8Bytes(value,Json));File.Move(temporary,path,true);}
        finally {if(File.Exists(temporary))File.Delete(temporary);}
    }
    private sealed class Progress(Action<NativeDownloadProgress> update):IProgress<NativeDownloadProgress>
    {
        private long last;
        public void Report(NativeDownloadProgress value)
        {long now=Environment.TickCount64;if(now-last<250 && value.Received!=value.Total)return;last=now;update(value);}
    }
}

public sealed class NativeUpdateClient
{
    private readonly string directory,id;
    private long nextRead;
    public NativeUpdateStatus? Status {get;private set;}
    public NativeUpdateClient(string directory,string id)
    {
        if(!Guid.TryParseExact(id,"N",out _))throw new ArgumentException("Invalid update session.");
        this.directory=Path.GetFullPath(directory);this.id=id;
    }
    public static NativeUpdateClient? FromEnvironment()
    {
        string? directory=Environment.GetEnvironmentVariable(NativeUpdateSession.DirectoryVariable),id=Environment.GetEnvironmentVariable(NativeUpdateSession.SessionVariable);
        Environment.SetEnvironmentVariable(NativeUpdateSession.DirectoryVariable,null);Environment.SetEnvironmentVariable(NativeUpdateSession.SessionVariable,null);
        return directory is null || id is null?null:new(directory,id);
    }
    public void Refresh()
    {
        long now=Environment.TickCount64;if(now<nextRead)return;nextRead=now+500;
        string path=Path.Combine(directory,"status.json");
        try
        {
            if(!File.Exists(path) || new FileInfo(path).Length>8192) {Status=null;return;}
            var status=JsonSerializer.Deserialize<NativeUpdateStatus>(File.ReadAllBytes(path),NativeUpdateSession.Json);
            Status=status?.Session==id?status:null;
        }
        catch(Exception error) when(error is IOException or JsonException or UnauthorizedAccessException) {Status=null;}
    }
    public bool RequestRestart()
    {
        Refresh();
        if(Status is not {Phase:"ready",Version:not null} status || !NativeUpdatePackage.ValidVersion(status.Version))return false;
        NativeUpdateSession.AtomicWrite(Path.Combine(directory,"restart.json"),new NativeRestartRequest(id,status.Version));return true;
    }
}
