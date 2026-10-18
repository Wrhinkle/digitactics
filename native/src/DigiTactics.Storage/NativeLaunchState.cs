using System.Text.Json;
using System.Text.Json.Serialization;

namespace DigiTactics.Storage;

public sealed record NativeReleaseReference(string Candidate,string Version);
public sealed record NativeLaunchTrial(NativeReleaseReference Release,string Token);
public sealed record NativeLaunchSelection(NativeReleaseReference? Release,string? TrialToken);
public sealed record NativeLauncherState(int SchemaVersion,NativeReleaseReference? Active,NativeReleaseReference? Pending,NativeLaunchTrial? Trial,string[] BadVersions);

/// <summary>Durable launcher transitions. A null release selects the bundled application.
/// Own this lease for the entire child-process lifetime; verify candidate content before Queue/launch.
/// Payloads and player profiles are never moved or deleted by this state machine.</summary>
public sealed class NativeLaunchState : IDisposable
{
    private readonly string statePath;
    private readonly string bundledVersion;
    private readonly FileStream lease;
    private NativeLauncherState state;
    private bool disposed;
    private static readonly JsonSerializerOptions Json=new() {PropertyNamingPolicy=JsonNamingPolicy.CamelCase,UnmappedMemberHandling=JsonUnmappedMemberHandling.Disallow};

    public NativeLaunchState(string directory,string bundledVersion)
    {
        if(!NativeUpdatePackage.ValidVersion(bundledVersion))throw new ArgumentException("Invalid bundled version.",nameof(bundledVersion));
        this.bundledVersion=bundledVersion;
        directory=Path.GetFullPath(directory);
        for(string? path=directory;path is not null;path=Path.GetDirectoryName(path))
            if(Path.Exists(path) && (File.GetAttributes(path)&FileAttributes.ReparsePoint)!=0)throw new IOException("Launcher state cannot use linked directories.");
        Directory.CreateDirectory(directory);
        statePath=Path.Combine(directory,"launcher-state.json");
        lease=new FileStream(Path.Combine(directory,"launcher.lock"),FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);
        try
        {
            if(File.Exists(statePath))
            {
                if((File.GetAttributes(statePath)&FileAttributes.ReparsePoint)!=0 || new FileInfo(statePath).Length>1024*1024)throw new InvalidDataException("Invalid launcher state file.");
                state=JsonSerializer.Deserialize<NativeLauncherState>(File.ReadAllBytes(statePath),Json)??throw new InvalidDataException("Missing launcher state.");
                Validate(state);
            }
            else state=new(1,null,null,null,[]);
        }
        catch {lease.Dispose();throw;}
    }

    public void Queue(NativeReleaseReference release)
    {
        ObjectDisposedException.ThrowIf(disposed,this);ValidateRelease(release);
        if(state.Trial is not null)throw new InvalidOperationException("Cannot queue during a startup trial.");
        string current=EffectiveActive()?.Version??bundledVersion;
        if(Compare(release.Version,current)<=0 || state.BadVersions.Contains(release.Version,StringComparer.Ordinal))throw new InvalidOperationException("Release is older, current, or previously failed.");
        if(state.Pending is not null && Compare(release.Version,state.Pending.Version)<0)throw new InvalidOperationException("A newer release is already pending.");
        Save(state with {Pending=release});
    }

    /// <summary>Call once before spawning the child. An unconfirmed trial from a prior launcher
    /// lifetime is quarantined before selecting a release. The trial is persisted before launch.</summary>
    public NativeLaunchSelection BeginLaunch()
    {
        ObjectDisposedException.ThrowIf(disposed,this);
        if(state.Trial is not null)Rollback(state.Trial.Token);
        var active=EffectiveActive();
        var pending=state.Pending;
        if(pending is not null && Compare(pending.Version,active?.Version??bundledVersion)>0 && !state.BadVersions.Contains(pending.Version,StringComparer.Ordinal))
        {
            string token=Guid.NewGuid().ToString("N");
            Save(state with {Active=active,Pending=null,Trial=new(pending,token)});
            return new(pending,token);
        }
        if(state.Active!=active || pending is not null)Save(state with {Active=active,Pending=null});
        return new(active,null);
    }

    public bool Confirm(string token)
    {
        ObjectDisposedException.ThrowIf(disposed,this);
        if(state.Trial is null || state.Trial.Token!=token)return false;
        Save(state with {Active=state.Trial.Release,Trial=null});return true;
    }

    public bool Rollback(string token)
    {
        ObjectDisposedException.ThrowIf(disposed,this);
        if(state.Trial is null || state.Trial.Token!=token)return false;
        Save(state with {Trial=null,BadVersions=state.BadVersions.Append(state.Trial.Release.Version).Distinct(StringComparer.Ordinal).ToArray()});return true;
    }

    private NativeReleaseReference? EffectiveActive()=>state.Active is not null && Compare(state.Active.Version,bundledVersion)>0?state.Active:null;
    private static int Compare(string left,string right)=>Version.Parse(left).CompareTo(Version.Parse(right));
    private static void ValidateRelease(NativeReleaseReference release)
    {
        if(release is null || !NativeUpdatePackage.ValidVersion(release.Version) || release.Candidate is null)throw new InvalidDataException("Invalid release reference.");
        string prefix=release.Version+"-";
        if(!release.Candidate.StartsWith(prefix,StringComparison.Ordinal) || release.Candidate.Length!=prefix.Length+32 || !Guid.TryParseExact(release.Candidate[prefix.Length..],"N",out _))throw new InvalidDataException("Invalid candidate directory name.");
    }
    private static void Validate(NativeLauncherState value)
    {
        if(value.SchemaVersion!=1 || value.BadVersions is null || value.BadVersions.Length>10000 || value.BadVersions.Any(v=>!NativeUpdatePackage.ValidVersion(v)))throw new InvalidDataException("Invalid launcher state metadata.");
        if(value.Active is not null)ValidateRelease(value.Active);
        if(value.Pending is not null)ValidateRelease(value.Pending);
        if(value.Pending is not null && value.Active is not null && Compare(value.Pending.Version,value.Active.Version)<=0)throw new InvalidDataException("Pending release is not newer than active release.");
        if(value.Trial is not null)
        {
            ValidateRelease(value.Trial.Release);
            if(value.Pending is not null || value.Active is not null && Compare(value.Trial.Release.Version,value.Active.Version)<=0)throw new InvalidDataException("Inconsistent startup trial state.");
            if(!Guid.TryParseExact(value.Trial.Token,"N",out _))throw new InvalidDataException("Invalid startup trial token.");
        }
    }
    private void Save(NativeLauncherState value)
    {
        Validate(value);
        string temporary=statePath+"."+Guid.NewGuid().ToString("N")+".tmp";
        try
        {
            using(var output=new FileStream(temporary,FileMode.CreateNew,FileAccess.Write,FileShare.None))
            {JsonSerializer.Serialize(output,value,Json);output.Flush(flushToDisk:true);}
            File.Move(temporary,statePath,overwrite:true);
            state=value;
        }
        finally {if(File.Exists(temporary))File.Delete(temporary);}
    }
    public void Dispose() {if(disposed)return;disposed=true;lease.Dispose();}
}
