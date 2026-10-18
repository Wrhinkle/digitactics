using System.Diagnostics;
using DigiTactics.Storage;

internal static class NativeSupervisorChecks
{
    public static async Task Child(string mode)
    {
        if(mode=="exit") {Environment.ExitCode=17;return;}
        if(mode=="hang") {await Task.Delay(TimeSpan.FromSeconds(30));return;}
        if(mode=="wrong")Environment.SetEnvironmentVariable(NativeStartupSignal.TokenVariable,Guid.NewGuid().ToString("N"));
        using var signal=NativeStartupSignal.FromEnvironment();
        if(signal is not null)await signal.ReportReady();
        if(mode=="ready-error")Environment.ExitCode=23;
    }
    public static async Task<int> Run()
    {
        int checks=0;
        void Check(bool ok,string reason) {checks++;if(!ok)throw new Exception("Native supervisor: "+reason);}
        string root=Path.Combine(Path.GetTempPath(),"digitactics-supervisor-test-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        ProcessStartInfo Start(string mode)
        {
            var result=new ProcessStartInfo(Environment.ProcessPath!) {UseShellExecute=false,CreateNoWindow=true};
            result.Environment["DOTNET_ROOT"]=Path.GetFullPath(Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(),"../../.."));
            result.ArgumentList.Add("--native-supervisor-child");result.ArgumentList.Add(mode);return result;
        }
        try
        {
            foreach(string mode in new[]{"ready","ready-error","exit","hang","wrong","missing","prepare-failure"})
            {
                string folder=Path.Combine(root,mode);
                var release=new NativeReleaseReference("1.1.0-"+Guid.NewGuid().ToString("N"),"1.1.0");
                int launches=0;
                using(var state=new NativeLaunchState(folder,"1.0.0"))
                {
                    state.Queue(release);
                    var result=await NativeProcessSupervisor.Run(state,selection=>
                    {
                        launches++;
                        if(selection.TrialToken is null)return Start("fallback");
                        if(mode=="prepare-failure")throw new InvalidDataException("Candidate verification failed.");
                        var start=Start(mode);
                        if(mode=="missing")start.FileName=Path.Combine(folder,"missing.exe");
                        return start;
                    },mode=="hang"?TimeSpan.FromMilliseconds(400):TimeSpan.FromSeconds(10));
                    bool healthy=mode is "ready" or "ready-error";
                    Check(result.RolledBack!=healthy,mode+" rollback selection");
                    Check(launches==(healthy?1:2),mode+" launch count");
                    Check(result.ExitCode==(mode=="ready-error"?23:0),mode+" exit code");
                    Check((result.Failure is null)==healthy,mode+" failure reporting");
                }
                using(var restarted=new NativeLaunchState(folder,"1.0.0"))
                    Check(restarted.BeginLaunch().Release==(mode is "ready" or "ready-error"?release:null),mode+" persisted selection");
            }
            string cancelled=Path.Combine(root,"cancelled");
            using(var state=new NativeLaunchState(cancelled,"1.0.0"))
            using(var cancel=new CancellationTokenSource(TimeSpan.FromMilliseconds(400)))
            {
                state.Queue(new("1.1.0-"+Guid.NewGuid().ToString("N"),"1.1.0"));
                try {await NativeProcessSupervisor.Run(state,_=>Start("hang"),cancellation:cancel.Token);throw new Exception("Ignored cancellation");}
                catch(OperationCanceledException) {checks++;}
            }
            using(var restarted=new NativeLaunchState(cancelled,"1.0.0"))
                Check(restarted.BeginLaunch().Release is null,"cancelled startup recovers safely on next launch");
        }
        finally {Directory.Delete(root,true);}
        Console.WriteLine($"Native process supervisor: {checks} real-child checks passed.");return checks;
    }
}
