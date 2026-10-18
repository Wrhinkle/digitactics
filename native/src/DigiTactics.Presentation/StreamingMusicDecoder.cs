using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;

namespace DigiTactics.Presentation;

/// <summary>One local media decoder with bounded PCM buffering and nonblocking reads on the mixer thread.</summary>
public sealed class StreamingMusicDecoder : IDisposable
{
    public const int SampleRate=44100, Channels=2, ChunkSamples=1764, BufferChunks=8;
    private readonly Channel<float[]> queue=Channel.CreateBounded<float[]>(new BoundedChannelOptions(BufferChunks) { SingleReader=true,SingleWriter=true,FullMode=BoundedChannelFullMode.Wait });
    private readonly CancellationTokenSource stop=new();
    private readonly TaskCompletionSource<bool> ready=new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly Task worker;
    private float[]? current;
    private int offset,disposed;
    private string? error;
    public Task<bool> Ready => ready.Task;
    public Task Completion => worker;
    public string? Error => Volatile.Read(ref error);
    public int BufferedChunks => queue.Reader.Count;
    public bool Ended => worker.IsCompleted && current is null && queue.Reader.Count==0;
    /// <param name="source">Ownership transfers to the decoder, including when starting fails.</param>
    public StreamingMusicDecoder(string executable,FileStream source)
    {
        worker=Task.Run(()=>Decode(executable,source));
    }
    public int Read(Span<float> destination)
    {
        if(destination.Length%Channels!=0) throw new ArgumentException("Read complete stereo frames.",nameof(destination));
        if(Volatile.Read(ref disposed)!=0) return 0;
        int written=0;
        while(written<destination.Length)
        {
            if(current is null && !queue.Reader.TryRead(out current)) break;
            int count=Math.Min(destination.Length-written,current.Length-offset);
            current.AsSpan(offset,count).CopyTo(destination[written..]); written+=count; offset+=count;
            if(offset==current.Length) { current=null; offset=0; }
        }
        return written;
    }
    private async Task Decode(string executable,FileStream source)
    {
        using(source)
        using(var process=new Process())
        {
            try
            {
                stop.Token.ThrowIfCancellationRequested();
                process.StartInfo=new ProcessStartInfo(Path.GetFullPath(executable)) { UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true };
                foreach(string argument in new[] { "-hide_banner","-loglevel","error","-nostdin","-protocol_whitelist","file,pipe","-i",source.Name,"-map","0:a:0","-vn","-sn","-dn","-ac","2","-ar","44100","-c:a","pcm_f32le","-f","f32le","pipe:1" }) process.StartInfo.ArgumentList.Add(argument);
                if(!process.Start()) throw new IOException("Music decoder did not start.");
                using var cancellation=stop.Token.Register(()=>Kill(process));
                Task<string> errors=ReadErrors(process.StandardError);
                try
                {
                    byte[] bytes=new byte[ChunkSamples*sizeof(float)];
                    while(true)
                    {
                        int count=0;
                        while(count<bytes.Length)
                        {
                            int read=await process.StandardOutput.BaseStream.ReadAsync(bytes.AsMemory(count),stop.Token).ConfigureAwait(false);
                            if(read==0) break;
                            count+=read;
                        }
                        if(count==0) break;
                        if(count%(Channels*sizeof(float))!=0) throw new InvalidDataException("Music decoder returned an incomplete stereo frame.");
                        var pcm=MemoryMarshal.Cast<byte,float>(bytes.AsSpan(0,count)).ToArray();
                        if(pcm.Any(value=>!float.IsFinite(value))) throw new InvalidDataException("Music decoder returned invalid samples.");
                        await queue.Writer.WriteAsync(pcm,stop.Token).ConfigureAwait(false); ready.TrySetResult(true);
                    }
                    await process.WaitForExitAsync(stop.Token).ConfigureAwait(false);
                    string details=await errors.ConfigureAwait(false);
                    if(process.ExitCode!=0) throw new InvalidDataException("Music could not be decoded. "+details.Trim());
                }
                finally
                {
                    Kill(process);
                    await process.WaitForExitAsync().ConfigureAwait(false);
                    await errors.ConfigureAwait(false);
                }
            }
            catch(OperationCanceledException) when(stop.IsCancellationRequested) { }
            catch(Exception e) when(e is IOException or InvalidDataException or InvalidOperationException or System.ComponentModel.Win32Exception or UnauthorizedAccessException or ArgumentException)
            { if(!stop.IsCancellationRequested) Volatile.Write(ref error,e.Message); }
            finally { queue.Writer.TryComplete(); ready.TrySetResult(false); }
        }
    }
    private static async Task<string> ReadErrors(StreamReader reader)
    {
        var text=new StringBuilder(); char[] buffer=new char[1024]; int count;
        while((count=await reader.ReadAsync(buffer).ConfigureAwait(false))>0) if(text.Length<4096) text.Append(buffer,0,Math.Min(count,4096-text.Length));
        return text.ToString();
    }
    private static void Kill(Process process)
    {
        try { if(!process.HasExited) process.Kill(entireProcessTree:true); }
        catch(InvalidOperationException) { } catch(System.ComponentModel.Win32Exception) { }
    }
    public void Dispose()
    {
        if(Interlocked.Exchange(ref disposed,1)!=0) return;
        stop.Cancel();
        _=worker.ContinueWith(_=>stop.Dispose(),CancellationToken.None,TaskContinuationOptions.ExecuteSynchronously,TaskScheduler.Default);
    }
}
