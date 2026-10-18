using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Storage;

internal static class NativeReleaseChecks
{
    private sealed class Transport(Func<HttpRequestMessage,HttpResponseMessage> send):HttpMessageHandler
    {protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellation)=>Task.FromResult(send(request));}
    public static async Task<int> Run()
    {
        int checks=0;
        void Check(bool ok,string reason) {checks++;if(!ok)throw new Exception("Native releases: "+reason);}
        string directory=Path.Combine(Path.GetTempPath(),"digitactics-release-test-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);
        string cache=Path.Combine(directory,"cache.json");
        JsonObject Manifest()=>JsonSerializer.SerializeToNode(new NativeReleaseManifest(1,"1.2.3","win-x64",1,"native-1.2.3.zip",123,new string('a',64),"Fixes"),new JsonSerializerOptions {PropertyNamingPolicy=JsonNamingPolicy.CamelCase})!.AsObject();
        JsonObject Release()=>JsonNode.Parse("""
        {"tag_name":"native-v1.2.3","draft":false,"prerelease":false,"html_url":"https://github.com/owner/repo/releases/tag/native-v1.2.3","assets":[{"name":"native-latest.json","size":300,"browser_download_url":"https://github.com/owner/repo/releases/download/native-v1.2.3/native-latest.json"},{"name":"native-1.2.3.zip","size":123,"browser_download_url":"https://github.com/owner/repo/releases/download/native-v1.2.3/native-1.2.3.zip"}]}
        """)!.AsObject();
        HttpResponseMessage Response(JsonNode value)=>new(HttpStatusCode.OK) {Content=new StringContent(value.ToJsonString())};
        try
        {
            int apiRequests=0;
            using(var discovery=new NativeReleaseDiscovery(new Transport(request=>
            {
                Check(request.Headers.Authorization is null,"anonymous release check");
                if(request.RequestUri!.Host=="api.github.com")
                {
                    apiRequests++;
                    if(apiRequests>1) {Check(request.Headers.IfNoneMatch.Single().Tag=="\"revision\"","cached ETag sent");return new(HttpStatusCode.NotModified);}
                    var response=Response(Release());response.Headers.ETag=new EntityTagHeaderValue("\"revision\"");return response;
                }
                return Response(Manifest());
            })))
            {
                var available=await discovery.Check("owner","repo","1.0.0",1,cache);
                Check(available?.Expected.Version=="1.2.3" && !available.RequiresLauncherUpdate,"native release discovered");
                available=await discovery.Check("owner","repo","1.0.0",1,cache);
                Check(available?.Manifest.Notes=="Fixes","304 restores release metadata");
                Check(await discovery.Check("owner","repo","1.2.3",1,cache) is null,"current version ignored");
            }
            foreach(string mode in new[]{"legacy","prerelease","draft","launcher","tag","platform","size","digest","duplicate","downgrade","oversized"})
            {
                var release=Release();var manifest=Manifest();
                switch(mode)
                {
                    case "legacy": release["assets"]![0]!["name"]="latest.json";break;
                    case "prerelease": release[mode]=true;break;
                    case "draft": release[mode]=true;break;
                    case "launcher": manifest["minimumLauncher"]=2;break;
                    case "tag": release["tag_name"]="v9.9.9";break;
                    case "platform": manifest["platform"]="electron";break;
                    case "size": manifest["size"]=124;break;
                    case "digest": manifest["sha256"]="invalid";break;
                    case "duplicate": release["assets"]!.AsArray().Add(release["assets"]![0]!.DeepClone());break;
                    case "downgrade": release["assets"]![0]!["browser_download_url"]="http://insecure.invalid/manifest";break;
                    case "oversized": manifest["notes"]=new string('x',70000);break;
                }
                using var discovery=new NativeReleaseDiscovery(new Transport(request=>Response(request.RequestUri!.Host=="api.github.com"?release:manifest)));
                try
                {
                    var found=await discovery.Check("owner","repo","1.0.0",1,Path.Combine(directory,mode+".json"));
                    if(mode is "legacy" or "prerelease" or "draft")Check(found is null,mode+" excluded");
                    else if(mode=="launcher")Check(found?.RequiresLauncherUpdate==true,"full launcher update required");
                    else throw new Exception("Accepted invalid release "+mode);
                }
                catch(InvalidDataException) when(mode is not ("legacy" or "prerelease" or "draft" or "launcher")) {checks++;}
            }
        }
        finally {Directory.Delete(directory,true);}
        Console.WriteLine($"Native release discovery: {checks} metadata, compatibility and ETag checks passed.");return checks;
    }
}
