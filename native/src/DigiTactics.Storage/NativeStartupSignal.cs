using System.IO.Pipes;
using System.Text;

namespace DigiTactics.Storage;

/// <summary>One-use, current-user named-pipe acknowledgement of a rendered startup.</summary>
public sealed class NativeStartupSignal : IDisposable
{
    public const string PipeVariable="DIGITACTICS_STARTUP_PIPE";
    public const string TokenVariable="DIGITACTICS_STARTUP_TOKEN";
    private readonly string pipeName,token;
    private readonly CancellationTokenSource cancellation=new();
    private bool sent,disposed;
    private NativeStartupSignal(string pipeName,string token) {this.pipeName=pipeName;this.token=token;}

    public static NativeStartupSignal? FromEnvironment()
    {
        string? pipe=Environment.GetEnvironmentVariable(PipeVariable),token=Environment.GetEnvironmentVariable(TokenVariable);
        // Do not pass the acknowledgement capability to subprocesses spawned by the game.
        Environment.SetEnvironmentVariable(PipeVariable,null);Environment.SetEnvironmentVariable(TokenVariable,null);
        if(pipe is null && token is null)return null;
        if(pipe is null || !pipe.StartsWith("digitactics-startup-",StringComparison.Ordinal) || !Guid.TryParseExact(pipe["digitactics-startup-".Length..],"N",out _) || !Guid.TryParseExact(token,"N",out _))throw new InvalidDataException("Invalid launcher startup signal.");
        return new(pipe,token!);
    }

    /// <summary>Call after the first successfully presented game frame. Never blocks rendering.</summary>
    public Task ReportReady()
    {
        ObjectDisposedException.ThrowIf(disposed,this);
        if(sent)return Task.CompletedTask;
        sent=true;return Send();
    }
    private async Task Send()
    {
        try
        {
            cancellation.CancelAfter(TimeSpan.FromSeconds(5));
            CancellationToken token=cancellation.Token;
            using var pipe=new NamedPipeClientStream(".",pipeName,PipeDirection.Out,PipeOptions.Asynchronous|PipeOptions.CurrentUserOnly);
            await pipe.ConnectAsync(token).ConfigureAwait(false);
            await pipe.WriteAsync(Encoding.ASCII.GetBytes(this.token),token).ConfigureAwait(false);
            await pipe.FlushAsync(token).ConfigureAwait(false);
        }
        catch(Exception error) when(error is IOException or OperationCanceledException or UnauthorizedAccessException)
        {Console.Error.WriteLine("Launcher startup acknowledgement failed: "+error.Message);}
    }
    public void Dispose() {if(disposed)return;disposed=true;cancellation.Cancel();cancellation.Dispose();}
}

public sealed class NativeStartupListener : IDisposable
{
    public string PipeName {get;}="digitactics-startup-"+Guid.NewGuid().ToString("N");
    private readonly NamedPipeServerStream pipe;
    public NativeStartupListener()=>pipe=new NamedPipeServerStream(PipeName,PipeDirection.In,1,PipeTransmissionMode.Byte,PipeOptions.Asynchronous|PipeOptions.CurrentUserOnly);

    public async Task WaitReady(string token,CancellationToken cancellation)
    {
        if(!Guid.TryParseExact(token,"N",out _))throw new ArgumentException("Invalid startup token.",nameof(token));
        await pipe.WaitForConnectionAsync(cancellation).ConfigureAwait(false);
        byte[] received=new byte[32];
        await pipe.ReadExactlyAsync(received,cancellation).ConfigureAwait(false);
        if(!System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(received,Encoding.ASCII.GetBytes(token)))throw new InvalidDataException("Startup acknowledgement token differs.");
    }
    public void Dispose()=>pipe.Dispose();
}
