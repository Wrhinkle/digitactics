using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DigiTactics.Storage;

public sealed record NativeReleaseManifest(int SchemaVersion,string Version,string Platform,int MinimumLauncher,string File,long Size,string Sha256,string? Notes);
public sealed record NativeAvailableRelease(NativeReleaseManifest Manifest,Uri Download,Uri Page,bool RequiresLauncherUpdate)
{public NativePackageExpectation Expected=>new(Manifest.Version,Manifest.Size,Manifest.Sha256);}

/// <summary>Public GitHub latest-release discovery. Legacy Electron manifests are never consumed.</summary>
public sealed class NativeReleaseDiscovery : IDisposable
{
    private sealed record Cache(string Url,string? Etag,string Body);
    private readonly HttpClient client;
    private static readonly JsonSerializerOptions Json=new() {PropertyNamingPolicy=JsonNamingPolicy.CamelCase,UnmappedMemberHandling=JsonUnmappedMemberHandling.Disallow};
    public NativeReleaseDiscovery(HttpMessageHandler? transport=null)
    {
        client=new HttpClient(transport??new SocketsHttpHandler {AllowAutoRedirect=false,UseCookies=false}) {Timeout=Timeout.InfiniteTimeSpan};
        client.DefaultRequestHeaders.UserAgent.ParseAdd("DigiTactics-Native-Updater/1");
    }
    public async Task<NativeAvailableRelease?> Check(string owner,string repository,string currentVersion,int launcherVersion,string cachePath,CancellationToken cancellation=default)
    {
        static bool Segment(string value)=>value.Length is >0 and <=100 && value is not ("." or "..") && value.All(c=>char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.');
        if(!Segment(owner) || !Segment(repository) || !NativeUpdatePackage.ValidVersion(currentVersion) || launcherVersion<1)throw new ArgumentException("Invalid native update source or version.");
        var endpoint=new Uri($"https://api.github.com/repos/{owner}/{repository}/releases/latest");
        Cache? cache=null;
        try
        {
            if(File.Exists(cachePath) && new FileInfo(cachePath).Length<=4*1024*1024)
            {
                var saved=JsonSerializer.Deserialize<Cache>(File.ReadAllBytes(cachePath),Json);
                if(saved?.Url==endpoint.AbsoluteUri && saved.Body is {Length:<=1024*1024} && saved.Etag is {Length:<=512} && EntityTagHeaderValue.TryParse(saved.Etag,out _))
                {using var cachedDocument=JsonDocument.Parse(saved.Body,new JsonDocumentOptions {MaxDepth=32});cache=saved;}
            }
        }
        catch(Exception error) when(error is IOException or JsonException or UnauthorizedAccessException) { }
        using var deadline=CancellationTokenSource.CreateLinkedTokenSource(cancellation);deadline.CancelAfter(TimeSpan.FromSeconds(5));
        using var request=new HttpRequestMessage(HttpMethod.Get,endpoint);
        request.Headers.Accept.ParseAdd("application/vnd.github+json");request.Headers.Add("X-GitHub-Api-Version","2026-03-10");
        if(cache is not null)request.Headers.IfNoneMatch.Add(EntityTagHeaderValue.Parse(cache.Etag!));
        using var response=await client.SendAsync(request,HttpCompletionOption.ResponseHeadersRead,deadline.Token).ConfigureAwait(false);
        string body;
        if(response.StatusCode==HttpStatusCode.NotModified && cache is not null)body=cache.Body;
        else
        {
            response.EnsureSuccessStatusCode();
            body=System.Text.Encoding.UTF8.GetString(await ReadBounded(response,1024*1024,deadline.Token).ConfigureAwait(false));
        }
        using var release=JsonDocument.Parse(body,new JsonDocumentOptions {MaxDepth=32});
        var root=release.RootElement;
        if(root.GetProperty("draft").GetBoolean() || root.GetProperty("prerelease").GetBoolean())return null;
        string tag=root.GetProperty("tag_name").GetString()??"";
        var assets=root.GetProperty("assets").EnumerateArray().ToArray();
        var manifests=assets.Where(a=>a.GetProperty("name").GetString()=="native-latest.json").ToArray();
        if(response.StatusCode==HttpStatusCode.OK)
        {
            try
            {
                string parent=Path.GetDirectoryName(Path.GetFullPath(cachePath))!;Directory.CreateDirectory(parent);
                string temporary=cachePath+"."+Guid.NewGuid().ToString("N")+".tmp";
                try {File.WriteAllBytes(temporary,JsonSerializer.SerializeToUtf8Bytes(new Cache(endpoint.AbsoluteUri,response.Headers.ETag?.ToString(),body),Json));File.Move(temporary,cachePath,true);}
                finally {if(File.Exists(temporary))File.Delete(temporary);}
            }
            catch(Exception error) when(error is IOException or UnauthorizedAccessException) { }
        }
        if(manifests.Length==0)return null;
        if(manifests.Length!=1)throw new InvalidDataException("Release contains duplicate native manifests.");
        deadline.CancelAfter(TimeSpan.FromSeconds(15));
        Uri manifestUrl=AssetUrl(manifests[0]);
        byte[] metadata=await Asset(manifestUrl,deadline.Token).ConfigureAwait(false);
        using var manifestDocument=JsonDocument.Parse(metadata,new JsonDocumentOptions {MaxDepth=16});
        NativeUpdatePackage.RejectDuplicateProperties(manifestDocument.RootElement);
        var manifest=manifestDocument.Deserialize<NativeReleaseManifest>(Json)??throw new InvalidDataException("Missing native release manifest.");
        if(manifest.SchemaVersion!=1 || manifest.Platform!="win-x64" || !NativeUpdatePackage.ValidVersion(manifest.Version) || tag!="v"+manifest.Version && tag!="native-v"+manifest.Version || manifest.MinimumLauncher<1 || manifest.Size is <=0 or >NativeUpdatePackage.MaximumArchiveBytes || manifest.Sha256 is not {Length:64} || manifest.Sha256.Any(c=>c is not (>= '0' and <= '9' or >= 'a' and <= 'f')) || manifest.Notes?.Length>16384)throw new InvalidDataException("Invalid native release metadata.");
        NativeUpdatePackage.ValidatePath(manifest.File);
        if(manifest.File.Contains('/') || !manifest.File.EndsWith(".zip",StringComparison.Ordinal))throw new InvalidDataException("Invalid native release asset name.");
        var payloads=assets.Where(a=>a.GetProperty("name").GetString()==manifest.File).ToArray();
        if(payloads.Length!=1 || payloads[0].GetProperty("size").GetInt64()!=manifest.Size)throw new InvalidDataException("Native release asset differs from manifest.");
        Uri page=new(root.GetProperty("html_url").GetString()!);Https(page);
        if(Version.Parse(manifest.Version)<=Version.Parse(currentVersion))return null;
        return new(manifest,AssetUrl(payloads[0]),page,manifest.MinimumLauncher>launcherVersion);
    }
    private async Task<byte[]> Asset(Uri address,CancellationToken cancellation)
    {
        for(int hop=0;hop<6;hop++)
        {
            Https(address);using var response=await client.GetAsync(address,HttpCompletionOption.ResponseHeadersRead,cancellation).ConfigureAwait(false);
            if(response.StatusCode is HttpStatusCode.MovedPermanently or HttpStatusCode.Redirect or HttpStatusCode.SeeOther or HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect)
            {address=new Uri(address,response.Headers.Location??throw new InvalidDataException("Missing manifest redirect."));continue;}
            response.EnsureSuccessStatusCode();return await ReadBounded(response,64*1024,cancellation).ConfigureAwait(false);
        }
        throw new HttpRequestException("Too many manifest redirects.");
    }
    private static Uri AssetUrl(JsonElement asset) {var uri=new Uri(asset.GetProperty("browser_download_url").GetString()!);Https(uri);return uri;}
    private static void Https(Uri uri) {if(!uri.IsAbsoluteUri || uri.Scheme!="https" || uri.UserInfo.Length!=0 || uri.Fragment.Length!=0)throw new InvalidDataException("Invalid release URL.");}
    private static async Task<byte[]> ReadBounded(HttpResponseMessage response,int limit,CancellationToken cancellation)
    {
        if(response.Content.Headers.ContentLength>limit)throw new InvalidDataException("Release metadata is too large.");
        using var result=new MemoryStream();await using var input=await response.Content.ReadAsStreamAsync(cancellation).ConfigureAwait(false);
        byte[] buffer=new byte[8192];int read;
        while((read=await input.ReadAsync(buffer,cancellation).ConfigureAwait(false))>0)
        {if(result.Length+read>limit)throw new InvalidDataException("Release metadata is too large.");result.Write(buffer,0,read);}
        return result.ToArray();
    }
    public void Dispose()=>client.Dispose();
}
