using System.IO.Pipes;
using System.Text;
using DigiTactics.Storage;

internal static class NativeStartupChecks
{
    public static async Task RunDesktop(string executable)
    {
        using var listener=new NativeStartupListener();
        using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(90));
        string token=Guid.NewGuid().ToString("N");
        var start=new System.Diagnostics.ProcessStartInfo(Path.GetFullPath(executable)) {UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
        start.Environment["DOTNET_ROOT"]=Path.GetFullPath(Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(),"../../.."));
        start.ArgumentList.Add("--match-smoke");start.ArgumentList.Add("--minimum-window");
        start.Environment[NativeStartupSignal.PipeVariable]=listener.PipeName;
        start.Environment[NativeStartupSignal.TokenVariable]=token;
        Task ready=listener.WaitReady(token,timeout.Token);
        using var child=System.Diagnostics.Process.Start(start)??throw new IOException("Desktop did not launch.");
        Task<string> output=child.StandardOutput.ReadToEndAsync(),error=child.StandardError.ReadToEndAsync();
        try
        {
            Task exited=child.WaitForExitAsync(timeout.Token);
            await Task.WhenAny(ready,exited);
            if(!ready.IsCompletedSuccessfully)throw new Exception("Desktop exited or failed before rendered startup acknowledgement.");
            await ready;await exited;
            string stderr=await error;
            if(child.ExitCode!=0 || stderr.Contains("acknowledgement failed",StringComparison.Ordinal))throw new Exception("Desktop startup smoke failed: "+stderr);
            Console.WriteLine("Desktop startup handshake passed after first presented frame; minimum-window match workflow completed.");
        }
        catch
        {
            if(!child.HasExited) {child.Kill(entireProcessTree:true);await child.WaitForExitAsync();}
            Console.Error.WriteLine(await output);Console.Error.WriteLine(await error);throw;
        }
    }

    public static async Task<int> Run()
    {
        int checks=0;
        void Check(bool ok,string reason) {checks++;if(!ok)throw new Exception("Startup signal: "+reason);}
        string? savedPipe=Environment.GetEnvironmentVariable(NativeStartupSignal.PipeVariable),savedToken=Environment.GetEnvironmentVariable(NativeStartupSignal.TokenVariable);
        void EnvironmentFor(string? pipe,string? token)
        {Environment.SetEnvironmentVariable(NativeStartupSignal.PipeVariable,pipe);Environment.SetEnvironmentVariable(NativeStartupSignal.TokenVariable,token);}
        try
        {
            EnvironmentFor(null,null);Check(NativeStartupSignal.FromEnvironment() is null,"direct launch needs no signal");
            using(var listener=new NativeStartupListener())
            using(var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                string token=Guid.NewGuid().ToString("N");EnvironmentFor(listener.PipeName,token);
                using var client=NativeStartupSignal.FromEnvironment();
                Check(client is not null,"launcher capability accepted");
                Check(Environment.GetEnvironmentVariable(NativeStartupSignal.PipeVariable) is null && Environment.GetEnvironmentVariable(NativeStartupSignal.TokenVariable) is null,"capability removed from child environment");
                Task waiting=listener.WaitReady(token,timeout.Token);
                Check(!waiting.IsCompleted,"no acknowledgement before ready call");
                await client!.ReportReady();await waiting;checks++;
                await client.ReportReady();checks++;
            }
            using(var listener=new NativeStartupListener())
            using(var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                Task waiting=listener.WaitReady(Guid.NewGuid().ToString("N"),timeout.Token);
                using var bad=new NamedPipeClientStream(".",listener.PipeName,PipeDirection.Out,PipeOptions.Asynchronous);
                await bad.ConnectAsync(timeout.Token);await bad.WriteAsync(Encoding.ASCII.GetBytes(Guid.NewGuid().ToString("N")),timeout.Token);await bad.FlushAsync(timeout.Token);
                try {await waiting;throw new Exception("Accepted wrong token");}catch(InvalidDataException) {checks++;}
            }
            using(var listener=new NativeStartupListener())
            using(var timeout=new CancellationTokenSource())
            {
                Task waiting=listener.WaitReady(Guid.NewGuid().ToString("N"),timeout.Token);timeout.Cancel();
                try {await waiting;throw new Exception("Ignored watchdog cancellation");}catch(OperationCanceledException) {checks++;}
            }
            EnvironmentFor("unexpected-pipe",Guid.NewGuid().ToString("N"));
            try {using var client=NativeStartupSignal.FromEnvironment();throw new Exception("Accepted unexpected pipe");}catch(InvalidDataException) {checks++;}
        }
        finally {EnvironmentFor(savedPipe,savedToken);}
        Console.WriteLine($"Native startup signal: {checks} checks passed.");return checks;
    }
}
