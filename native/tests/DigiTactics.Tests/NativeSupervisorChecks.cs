using System.Diagnostics;
using DigiTactics.Storage;

internal static class NativeSupervisorChecks
{
    public static async Task Child(string mode)
    {
        if(mode=="oneshot")
        {
            if(Environment.GetEnvironmentVariable(NativeStartupSignal.PipeVariable) is not null || Environment.GetEnvironmentVariable(NativeStartupSignal.TokenVariable) is not null)throw new InvalidOperationException("A diagnostic inherited a startup acknowledgement.");
            using var output=new FileStream(Environment.GetEnvironmentVariable("DIGITACTICS_DIAGNOSTIC_TEST_PATH")!,FileMode.CreateNew,FileAccess.Write,FileShare.None);
            output.WriteByte(42);return;
        }
        if(mode=="exit") {Environment.ExitCode=17;return;}
        if(mode=="forwarded") {Environment.ExitCode=NativeGameInstance.ForwardedExitCode;return;}
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
            return NativeTestProcess.Start("--native-supervisor-child",mode);
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
            foreach(bool trial in new[]{false,true})
            {
                string folder=Path.Combine(root,trial?"forwarded-trial":"forwarded-normal");
                var active=new NativeReleaseReference("1.1.0-"+Guid.NewGuid().ToString("N"),"1.1.0");
                var pending=new NativeReleaseReference("1.2.0-"+Guid.NewGuid().ToString("N"),"1.2.0");
                using var state=new NativeLaunchState(folder,"1.0.0");
                state.Queue(active);state.Confirm(state.BeginLaunch().TrialToken!);
                if(trial)state.Queue(pending);
                int starts=0;
                var result=await NativeProcessSupervisor.Run(state,_=>Start("forwarded"),started:()=>starts++);
                Check(result.ExitCode==0 && !result.RolledBack && result.Failure is null,"forwarded launch exits successfully without rollback");
                Check(starts==0 && state.CurrentVersion=="1.1.0","forwarded launch neither starts downloader nor activates candidate");
                using var saved=System.Text.Json.JsonDocument.Parse(File.ReadAllText(Path.Combine(folder,"launcher-state.json")));
                Check(saved.RootElement.GetProperty("trial").ValueKind==System.Text.Json.JsonValueKind.Null && saved.RootElement.GetProperty("badVersions").GetArrayLength()==0,"forwarded launch leaves no trial or quarantine");
                Check((saved.RootElement.GetProperty("pending").ValueKind!=System.Text.Json.JsonValueKind.Null)==trial,"forwarded candidate remains pending only when a trial was deferred");
                var selected=state.BeginLaunch();
                Check(selected.Release==(trial?pending:active) && (selected.TrialToken is not null)==trial,"next real launch retries deferred candidate with readiness required");
                if(selected.TrialToken is not null)state.DeferTrial(selected.TrialToken);
            }
            using(var state=new NativeLaunchState(Path.Combine(root,"normal-exit"),"1.0.0"))
            {
                int starts=0;
                var result=await NativeProcessSupervisor.Run(state,_=>Start("exit"),started:()=>starts++);
                Check(result.ExitCode==17 && !result.RolledBack && result.Failure is not null && starts==0,"normal pre-frame failure preserves exit code and skips downloader without quarantine");
            }
            foreach(string diagnostic in new[]{"oneshot","exit","prepare-failure"})
            {
                string folder=Path.Combine(root,"diagnostic-"+diagnostic),effect=Path.Combine(folder,"imported-once.json");
                using var state=new NativeLaunchState(folder,"1.0.0");
                var active=new NativeReleaseReference("1.1.0-"+Guid.NewGuid().ToString("N"),"1.1.0");
                var pending=new NativeReleaseReference("1.2.0-"+Guid.NewGuid().ToString("N"),"1.2.0");
                state.Queue(active);state.Confirm(state.BeginLaunch().TrialToken!);state.Queue(pending);
                string saved=File.ReadAllText(Path.Combine(folder,"launcher-state.json"));int launches=0,started=0;
                try
                {
                    var result=await NativeProcessSupervisor.Run(state,selection=>
                    {
                        launches++;Check(selection==new NativeLaunchSelection(active,null),"diagnostic uses current active release only");
                        if(diagnostic=="prepare-failure")throw new InvalidDataException("Expected diagnostic verification failure.");
                        var start=Start(diagnostic);start.Environment["DIGITACTICS_DIAGNOSTIC_TEST_PATH"]=effect;
                        start.Environment[NativeStartupSignal.PipeVariable]="stale";start.Environment[NativeStartupSignal.TokenVariable]="stale";
                        return start;
                    },started:()=>started++,mode:NativeProcessMode.Diagnostic);
                    Check(result.ExitCode==(diagnostic=="exit"?17:0) && !result.RolledBack && result.Failure is null,"diagnostic returns its own exit status without false startup failure");
                    if(diagnostic=="oneshot")Check(File.ReadAllBytes(effect).SequenceEqual(new byte[]{42}),"one-shot import side effect executes exactly once");
                }
                catch(InvalidDataException) when(diagnostic=="prepare-failure") {checks++;}
                Check(launches==1 && started==0,"diagnostic has no retry or update task");
                Check(File.ReadAllText(Path.Combine(folder,"launcher-state.json"))==saved,"diagnostic preserves pending update and quarantine state byte-for-byte");
                var trial=state.BeginLaunch();Check(trial.Release==pending && trial.TrialToken is not null,"next real launch still trials the queued update");state.DeferTrial(trial.TrialToken!);
            }
            using(var state=new NativeLaunchState(Path.Combine(root,"explicit-verification"),"1.0.0"))
            {
                var candidate=new NativeReleaseReference("1.1.0-"+Guid.NewGuid().ToString("N"),"1.1.0");state.Queue(candidate);
                var result=await NativeProcessSupervisor.Run(state,_=>Start("ready"),mode:NativeProcessMode.StartupVerification);
                Check(result.ExitCode==0 && state.CurrentVersion==candidate.Version,"explicit startup verification still promotes acknowledged candidate");
            }
        }
        finally {Directory.Delete(root,true);}
        Console.WriteLine($"Native process supervisor: {checks} real-child checks passed.");return checks;
    }
}
