using System.Net;
using System.Text.Json;
using DigiTactics.Storage;

internal static class NativeUpdateSessionChecks
{
    private sealed class Transport(Func<HttpRequestMessage,HttpResponseMessage> send):HttpMessageHandler
    {protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellation)=>Task.FromResult(send(request));}
    public static async Task<int> Run()
    {
        int checks=0;
        void Check(bool ok,string reason) {checks++;if(!ok)throw new Exception("Automatic updates: "+reason);}
        string root=Path.Combine(Path.GetTempPath(),"digitactics-auto-update-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
        try
        {
            string source=Path.Combine(root,"source");Directory.CreateDirectory(source);File.WriteAllBytes(Path.Combine(source,"DigiTactics.Desktop.exe"),[1,2,3]);
            string archive=Path.Combine(root,"update.zip");var expected=NativeUpdatePackage.Create(source,archive,"1.1.0",1);byte[] bytes=File.ReadAllBytes(archive);
            var json=new JsonSerializerOptions {PropertyNamingPolicy=JsonNamingPolicy.CamelCase};
            foreach(string mode in new[]{"ready","manual","offline","corrupt","current","quarantined","pending"})
            {
                string directory=Path.Combine(root,mode);using var state=new NativeLaunchState(directory,mode=="current"?"1.1.0":"1.0.0");
                if(mode is "quarantined" or "pending")
                {
                    state.Queue(new("1.1.0-"+Guid.NewGuid().ToString("N"),"1.1.0"));
                    if(mode=="quarantined") {var trial=state.BeginLaunch();state.Rollback(trial.TrialToken!);}
                }
                var session=new NativeUpdateSession(directory);
                using var discovery=new NativeReleaseDiscovery(new Transport(request=>
                {
                    if(mode=="offline")throw new HttpRequestException("offline fixture");
                    object payload=request.RequestUri!.Host=="api.github.com"
                        ? new {tag_name="native-v1.1.0",draft=false,prerelease=false,html_url="https://github.com/owner/repo",assets=new[]{new {name="native-latest.json",size=1L,browser_download_url="https://example.invalid/native-latest.json"},new {name="native.zip",size=expected.Size,browser_download_url="https://example.invalid/native.zip"}}}
                        : new NativeReleaseManifest(1,"1.1.0","win-x64",mode=="manual"?2:1,"native.zip",expected.Size,expected.Sha256,null);
                    return new(HttpStatusCode.OK) {Content=new StringContent(JsonSerializer.Serialize(payload,json))};
                }));
                int downloads=0;
                using var downloader=new NativeUpdateDownloader(new Transport(_=>{downloads++;return new(HttpStatusCode.OK) {Content=new ByteArrayContent(mode=="corrupt"?[1]:bytes)};}));
                await session.Run(state,"owner","repo",directory,1,discovery,downloader,CancellationToken.None);
                var client=new NativeUpdateClient(session.DirectoryPath,session.Id);client.Refresh();
                string phase=mode is "offline" or "corrupt"?"unavailable":mode is "quarantined" or "pending"?"current":mode;
                Check(client.Status?.Phase==phase,mode+" status");
                Check(downloads==(mode is "ready" or "corrupt"?1:0),mode+" download count");
                Check(!session.RestartRequested(),mode+" no unsolicited restart");
                if(mode=="ready")
                {
                    Check(state.CurrentVersion=="1.0.0","update never activates mid-session");
                    Check(client.RequestRestart() && session.RestartRequested(),"matching explicit restart accepted");
                    var stale=new NativeUpdateClient(session.DirectoryPath,Guid.NewGuid().ToString("N"));stale.Refresh();
                    Check(stale.Status is null && !stale.RequestRestart(),"stale client cannot request restart");
                    Check(state.BeginLaunch().Release?.Version=="1.1.0","queued update selected only on next launch");
                }
                else Check(!client.RequestRestart(),mode+" has no restart action");
                try {await session.Run(state,"owner","repo",directory,1,discovery,downloader,CancellationToken.None);throw new Exception("Repeated automatic check");}
                catch(InvalidOperationException) {checks++;}
            }
        }
        finally {Directory.Delete(root,true);}
        Console.WriteLine($"Automatic update sessions: {checks} workflow and restart checks passed.");return checks;
    }
}
