using System.Net;
using DigiTactics.Storage;

internal static class NativeDownloadChecks
{
    private sealed class Transport(Func<HttpRequestMessage,CancellationToken,Task<HttpResponseMessage>> send):HttpMessageHandler
    {protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellation)=>send(request,cancellation);}
    private sealed class ProgressSink: IProgress<NativeDownloadProgress>
    {public readonly List<NativeDownloadProgress> Values=[];public void Report(NativeDownloadProgress value)=>Values.Add(value);}
    private sealed class Body(byte[] bytes,bool stall=false):Stream
    {
        private int position;
        public override bool CanRead=>true;public override bool CanSeek=>false;public override bool CanWrite=>false;
        public override long Length=>throw new NotSupportedException();
        public override long Position {get=>position;set=>throw new NotSupportedException();}
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer,CancellationToken cancellation=default)
        {
            if(stall && position>0)await Task.Delay(Timeout.Infinite,cancellation);
            cancellation.ThrowIfCancellationRequested();int length=Math.Min(Math.Min(buffer.Length,17),bytes.Length-position);
            bytes.AsMemory(position,length).CopyTo(buffer);position+=length;return length;
        }
        public override int Read(byte[] buffer,int offset,int count)=>throw new NotSupportedException();
        public override void Flush()=>throw new NotSupportedException();
        public override long Seek(long offset,SeekOrigin origin)=>throw new NotSupportedException();
        public override void SetLength(long value)=>throw new NotSupportedException();
        public override void Write(byte[] buffer,int offset,int count)=>throw new NotSupportedException();
    }
    public static async Task<int> Run()
    {
        int checks=0;
        void Check(bool ok,string reason) {checks++;if(!ok)throw new Exception("Native download: "+reason);}
        async Task Reject(Func<Task> action,string reason)
        {
            try {await action();}catch(Exception error) when(error is InvalidDataException or HttpRequestException or OperationCanceledException) {checks++;return;}
            throw new Exception("Native download accepted "+reason);
        }
        string root=Path.Combine(Path.GetTempPath(),"digitactics-download-test-"+Guid.NewGuid().ToString("N"));
        string source=Path.Combine(root,"source"),downloads=Path.Combine(root,"downloads");Directory.CreateDirectory(source);
        try
        {
            File.WriteAllBytes(Path.Combine(source,"DigiTactics.Desktop.exe"),[1,2,3]);
            string archive=Path.Combine(root,"fixture.zip");var expected=NativeUpdatePackage.Create(source,archive,"1.1.0",1);byte[] data=File.ReadAllBytes(archive);
            var uri=new Uri("https://updates.example.invalid/native.zip");
            using(var downloader=new NativeUpdateDownloader(new Transport((request,_)=>
            {
                Check(request.Headers.Authorization is null,"anonymous request");
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) {Content=new ByteArrayContent(data)});
            })))
            {
                var progress=new ProgressSink();string complete=await downloader.Download(uri,expected,downloads,1,progress);
                Check(File.ReadAllBytes(complete).SequenceEqual(data),"verified archive bytes");
                Check(progress.Values.Last()==new NativeDownloadProgress(expected.Size,expected.Size),"download progress");
                Check(Directory.GetFiles(downloads,"*.part").Length==0,"success removes partial name");
                await Reject(()=>downloader.Download(uri,expected with {Sha256=new string('0',64)},downloads,1),"wrong digest");
                await Reject(()=>downloader.Download(uri,expected with {Size=expected.Size+1},downloads,1),"header size mismatch");
                await Reject(()=>downloader.Download(new Uri("http://updates.example.invalid/a"),expected,downloads,1),"insecure origin");
                await Reject(()=>downloader.Download(new Uri("https://secret:password@updates.example.invalid/a"),expected,downloads,1),"URL credentials");
            }
            foreach(string mode in new[]{"redirect","downgrade","loop","truncated","overflow","status","stall","stall-body"})
            {
                int requests=0;
                using var downloader=new NativeUpdateDownloader(new Transport(async (request,cancellation)=>
                {
                    requests++;
                    if(mode=="stall")await Task.Delay(Timeout.Infinite,cancellation);
                    if(mode is "downgrade" or "loop" || mode=="redirect" && requests==1)
                    {
                        var redirected=new HttpResponseMessage(HttpStatusCode.Redirect);
                        redirected.Headers.Location=new Uri(mode=="downgrade"?"http://updates.example.invalid/a":"https://cdn.example.invalid/a");return redirected;
                    }
                    var bytes=mode=="truncated"?data[..^1]:mode=="overflow"?data.Concat(new byte[]{1}).ToArray():data;
                    var response=new HttpResponseMessage(mode=="status"?HttpStatusCode.ServiceUnavailable:HttpStatusCode.OK) {Content=new StreamContent(new Body(bytes,mode=="stall-body"))};
                    return response;
                }));
                if(mode=="redirect")
                {string complete=await downloader.Download(uri,expected,downloads,1);Check(requests==2 && File.Exists(complete),"HTTPS CDN redirect");}
                else await Reject(()=>downloader.Download(uri,expected,downloads,1,idleTimeout:TimeSpan.FromMilliseconds(100)),mode);
                Check(Directory.GetFiles(downloads,"*.part").Length==0,mode+" cleanup");
            }
            byte[] invalidArchive=[1,2,3,4];
            using(var downloader=new NativeUpdateDownloader(new Transport((_,_)=>Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) {Content=new ByteArrayContent(invalidArchive)}))))
            {
                var invalidExpected=new NativePackageExpectation("1.1.0",invalidArchive.Length,Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(invalidArchive)));
                await Reject(()=>downloader.Download(uri,invalidExpected,downloads,1),"matching outer hash but invalid native archive");
                using var cancelled=new CancellationTokenSource();cancelled.Cancel();
                await Reject(()=>downloader.Download(uri,expected,downloads,1,cancellation:cancelled.Token),"caller cancellation");
            }
            Check(Directory.GetFiles(downloads,"*.part").Length==0,"invalid archive and cancellation cleanup");
            Check(Directory.GetFiles(downloads,"*.zip").Length==2,"only verified downloads published");
        }
        finally {Directory.Delete(root,true);}
        Console.WriteLine($"Native download: {checks} transport, integrity and cleanup checks passed.");return checks;
    }
}
