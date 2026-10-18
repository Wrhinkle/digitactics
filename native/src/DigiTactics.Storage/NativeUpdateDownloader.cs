using System.Net;
using System.Security.Cryptography;

namespace DigiTactics.Storage;

public sealed record NativeDownloadProgress(long Received,long Total);

/// <summary>Anonymous HTTPS download with bounded streaming, manual redirects and an idle deadline.
/// A completed filename is returned only after native package verification succeeds.</summary>
public sealed class NativeUpdateDownloader : IDisposable
{
    private readonly HttpClient client;
    public NativeUpdateDownloader(HttpMessageHandler? transport=null)
    {
        client=new HttpClient(transport??new SocketsHttpHandler {AllowAutoRedirect=false,AutomaticDecompression=DecompressionMethods.None,UseCookies=false}) {Timeout=Timeout.InfiniteTimeSpan};
        client.DefaultRequestHeaders.UserAgent.ParseAdd("DigiTactics-Native-Updater/1");
    }

    public async Task<string> Download(Uri address,NativePackageExpectation expected,string downloadsDirectory,int launcherVersion,
        IProgress<NativeDownloadProgress>? progress=null,CancellationToken cancellation=default,TimeSpan? idleTimeout=null)
    {
        ValidateAddress(address);
        if(!NativeUpdatePackage.ValidVersion(expected.Version) || expected.Size is <=0 or >NativeUpdatePackage.MaximumArchiveBytes || expected.Sha256 is not {Length:64} || expected.Sha256.Any(c=>c is not (>= '0' and <= '9' or >= 'a' and <= 'f')))throw new InvalidDataException("Invalid native download descriptor.");
        TimeSpan idle=idleTimeout??TimeSpan.FromSeconds(30);
        if(idle<=TimeSpan.Zero || idle>TimeSpan.FromMinutes(10))throw new ArgumentOutOfRangeException(nameof(idleTimeout));
        string root=Path.GetFullPath(downloadsDirectory);
        for(string? path=root;path is not null;path=Path.GetDirectoryName(path))
            if(Path.Exists(path) && (File.GetAttributes(path)&FileAttributes.ReparsePoint)!=0)throw new IOException("Download directory cannot contain links.");
        Directory.CreateDirectory(root);
        string id=Guid.NewGuid().ToString("N"),partial=Path.Combine(root,id+".part"),complete=Path.Combine(root,id+".zip");
        using var deadline=CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        deadline.CancelAfter(idle);
        try
        {
            using var response=await Response(address,deadline.Token).ConfigureAwait(false);
            if(response.StatusCode!=HttpStatusCode.OK)throw new HttpRequestException("Native update download returned HTTP "+(int)response.StatusCode);
            if(response.Content.Headers.ContentLength is long declared && declared!=expected.Size)throw new InvalidDataException("Download Content-Length differs from release metadata.");
            using(var output=new FileStream(partial,FileMode.CreateNew,FileAccess.Write,FileShare.None,128*1024,FileOptions.Asynchronous))
            using(var hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
            {
                await using var input=await response.Content.ReadAsStreamAsync(deadline.Token).ConfigureAwait(false);
                byte[] buffer=new byte[128*1024];long received=0;int read;
                while((read=await input.ReadAsync(buffer,deadline.Token).ConfigureAwait(false))>0)
                {
                    received=checked(received+read);
                    if(received>expected.Size)throw new InvalidDataException("Download exceeds release size.");
                    await output.WriteAsync(buffer.AsMemory(0,read),deadline.Token).ConfigureAwait(false);
                    hash.AppendData(buffer,0,read);deadline.CancelAfter(idle);
                    progress?.Report(new(received,expected.Size));
                }
                if(received!=expected.Size || Convert.ToHexStringLower(hash.GetHashAndReset())!=expected.Sha256)throw new InvalidDataException("Downloaded size or SHA-256 differs from release metadata.");
                output.Flush(flushToDisk:true);
            }
            deadline.CancelAfter(Timeout.InfiniteTimeSpan);
            NativeUpdatePackage.Inspect(partial,expected,launcherVersion,cancellation);
            cancellation.ThrowIfCancellationRequested();
            File.Move(partial,complete,overwrite:false);return complete;
        }
        finally {if(File.Exists(partial))File.Delete(partial);}
    }
    private async Task<HttpResponseMessage> Response(Uri address,CancellationToken cancellation)
    {
        for(int redirect=0;redirect<6;redirect++)
        {
            using var request=new HttpRequestMessage(HttpMethod.Get,address);
            request.Headers.Accept.ParseAdd("application/octet-stream");
            var response=await client.SendAsync(request,HttpCompletionOption.ResponseHeadersRead,cancellation).ConfigureAwait(false);
            if(response.StatusCode is HttpStatusCode.MovedPermanently or HttpStatusCode.Redirect or HttpStatusCode.SeeOther or HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect)
            {
                try
                {
                    var location=response.Headers.Location??throw new InvalidDataException("Update redirect has no location.");
                    address=location.IsAbsoluteUri?location:new Uri(address,location);ValidateAddress(address);
                }
                finally {response.Dispose();}
                continue;
            }
            return response;
        }
        throw new HttpRequestException("Native download exceeded redirect limit.");
    }
    private static void ValidateAddress(Uri address)
    {
        if(!address.IsAbsoluteUri || address.Scheme!=Uri.UriSchemeHttps || address.UserInfo.Length!=0 || address.Fragment.Length!=0)throw new InvalidDataException("Native downloads require an HTTPS URL without credentials or fragments.");
    }
    public void Dispose()=>client.Dispose();
}
