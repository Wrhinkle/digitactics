using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using DigiTactics.Storage;

internal static class NativeReleaseArtifactChecks
{
    private sealed class Transport(Func<HttpRequestMessage,HttpResponseMessage> send):HttpMessageHandler
    {protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellation)=>Task.FromResult(send(request));}

    // Consumes files made by create-native-release.ps1 through the production discovery,
    // download and package-verification paths. No GitHub release or profile is modified.
    public static async Task<int> Run(string directory)
    {
        directory=Path.GetFullPath(directory);
        int checks=0;
        void Check(bool value,string reason) {checks++;if(!value)throw new Exception("Native release artifacts: "+reason);}
        var json=new JsonSerializerOptions {PropertyNamingPolicy=JsonNamingPolicy.CamelCase,UnmappedMemberHandling=JsonUnmappedMemberHandling.Disallow};
        byte[] metadata=File.ReadAllBytes(Path.Combine(directory,"native-latest.json"));
        var manifest=JsonSerializer.Deserialize<NativeReleaseManifest>(metadata,json)??throw new InvalidDataException("Missing native release manifest.");
        string fullName=$"DigiTactics-native-{manifest.Version}-win-x64.zip";
        string update=Path.Combine(directory,manifest.File);
        string temporary=Path.Combine(Path.GetTempPath(),"digitactics-release-artifacts-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporary);
        try
        {
            string tag="native-v"+manifest.Version;
            Uri baseUri=new($"https://github.com/test/native-artifacts/releases/download/{tag}/");
            string release=JsonSerializer.Serialize(new {tag_name=tag,draft=false,prerelease=false,html_url=$"https://github.com/test/native-artifacts/releases/tag/{tag}",assets=new[] {
                new {name="native-latest.json",size=(long)metadata.Length,browser_download_url=new Uri(baseUri,"native-latest.json").AbsoluteUri},
                new {name=manifest.File,size=new FileInfo(update).Length,browser_download_url=new Uri(baseUri,manifest.File).AbsoluteUri}
            }});
            HttpResponseMessage Send(HttpRequestMessage request)
            {
                Check(request.Headers.Authorization is null,"anonymous discovery and download");
                if(request.RequestUri==new Uri("https://api.github.com/repos/test/native-artifacts/releases/latest"))return new(HttpStatusCode.OK) {Content=new StringContent(release)};
                if(request.RequestUri==new Uri(baseUri,"native-latest.json"))return new(HttpStatusCode.OK) {Content=new ByteArrayContent(metadata)};
                if(request.RequestUri==new Uri(baseUri,manifest.File))return new(HttpStatusCode.OK) {Content=new StreamContent(File.OpenRead(update))};
                throw new Exception("Unexpected release transport request: "+request.RequestUri);
            }
            using var discovery=new NativeReleaseDiscovery(new Transport(Send));
            var available=await discovery.Check("test","native-artifacts","0.0.0",int.MaxValue,Path.Combine(temporary,"cache.json"));
            Check(manifest.Version=="0.0.0"?available is null:available?.Manifest==manifest,"generated manifest accepted by production discovery");
            if(available is not null)
            {
                Check(available.Download==new Uri(baseUri,manifest.File) && !available.RequiresLauncherUpdate,"update asset and compatibility resolve");
                using var downloader=new NativeUpdateDownloader(new Transport(Send));
                string downloaded=await downloader.Download(available.Download,available.Expected,Path.Combine(temporary,"downloads"),int.MaxValue);
                Check(new FileInfo(downloaded).Length==manifest.Size,"production downloader accepts generated archive");
            }
            var package=NativeUpdatePackage.Inspect(update,new(manifest.Version,manifest.Size,manifest.Sha256),int.MaxValue);
            Check(package.MinimumLauncher==manifest.MinimumLauncher && package.Platform==manifest.Platform,"release/package compatibility matches");
            using var full=ZipFile.OpenRead(Path.Combine(directory,fullName));
            using(var configuration=full.GetEntry("native-launcher.json")!.Open())
            using(var config=JsonDocument.Parse(configuration))
                Check(config.RootElement.GetProperty("bundledVersion").GetString()==manifest.Version,"full installation has the release version");
            Check(full.GetEntry("DigiTactics.Launcher.exe") is not null && full.GetEntry("START-HERE.txt") is not null,"full archive contains the launcher entry point");
            var appEntries=full.Entries.Where(e=>e.FullName.StartsWith("app/",StringComparison.Ordinal)).ToDictionary(e=>e.FullName,StringComparer.Ordinal);
            Check(appEntries.Count==package.Files.Length,"full installation and update contain the same application files");
            foreach(var file in package.Files)
            {
                Check(appEntries.TryGetValue("app/"+file.Path,out var entry) && entry.Length==file.Size,"matching full installation file "+file.Path);
                using var contents=entry!.Open();
                Check(Convert.ToHexStringLower(SHA256.HashData(contents))==file.Sha256,"matching full installation bytes "+file.Path);
            }
            using var report=JsonDocument.Parse(File.ReadAllBytes(Path.Combine(directory,"native-release-artifacts.json")));
            Check(report.RootElement.GetProperty("tag").GetString()==tag,"assembly report advertises a supported release tag");
            Check(!report.RootElement.GetProperty("releaseApproved").GetBoolean(),"local integrity checks do not approve public release");
            foreach(var material in report.RootElement.GetProperty("mediaMaterials").EnumerateArray())
            {
                using var archive=ZipFile.OpenRead(Path.Combine(directory,material.GetProperty("file").GetString()!));
                using var entry=archive.GetEntry("materials-manifest.json")!.Open();
                using var provenance=JsonDocument.Parse(entry);
                Check(material.GetProperty("runtimeSha256").GetString()==provenance.RootElement.GetProperty("runtimeSha256").GetString() && material.GetProperty("distributionReady").GetBoolean()==provenance.RootElement.GetProperty("distributionReady").GetBoolean(),"source attachment provenance and incomplete closure status preserved");
            }
            var checksums=File.ReadAllLines(Path.Combine(directory,"SHA256SUMS.txt"));
            foreach(var artifact in report.RootElement.GetProperty("artifacts").EnumerateArray())
            {
                string name=artifact.GetProperty("file").GetString()!;
                using var contents=File.OpenRead(Path.Combine(directory,name));
                string digest=Convert.ToHexStringLower(SHA256.HashData(contents));
                Check(contents.Length==artifact.GetProperty("size").GetInt64() && digest==artifact.GetProperty("sha256").GetString(),"assembly report matches "+name);
                Check(checksums.Contains(digest+"  "+name,StringComparer.Ordinal),"checksum file matches "+name);
            }
        }
        finally {Directory.Delete(temporary,true);}
        Console.WriteLine($"Native release artifacts: {checks} production discovery/download, ZIP content and metadata checks passed.");
        return checks;
    }
}
