using DigiTactics.Storage;

internal static class NativeLaunchChecks
{
    public static int Run()
    {
        int checks=0;
        void Check(bool ok,string reason) {checks++;if(!ok)throw new Exception("Native launch: "+reason);}
        void Reject(Action action,string reason)
        {
            try {action();}catch(InvalidOperationException) {checks++;return;}catch(InvalidDataException) {checks++;return;}
            throw new Exception("Native launch accepted "+reason);
        }
        static NativeReleaseReference Release(string version)=>new(version+"-"+Guid.NewGuid().ToString("N"),version);
        string root=Path.Combine(Path.GetTempPath(),"digitactics-launch-test-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var first=Release("1.1.0");var failed=Release("1.2.0");var next=Release("1.3.0");string trial;
        try
        {
            using(var launcher=new NativeLaunchState(root,"1.0.0"))
            {
                Check(launcher.BeginLaunch()==new NativeLaunchSelection(null,null),"fresh launcher selects bundled application");
                try {using var competing=new NativeLaunchState(root,"1.0.0");throw new Exception("Second launcher acquired lock");}
                catch(IOException) {checks++;}
                Reject(()=>launcher.Queue(Release("1.0.0")),"same version");
                Reject(()=>launcher.Queue(Release("0.9.0")),"downgrade");
                Reject(()=>launcher.Queue(new("../escape","1.1.0")),"unsafe candidate");
                launcher.Queue(first);
            }
            using(var launcher=new NativeLaunchState(root,"1.0.0"))
            {
                var selected=launcher.BeginLaunch();trial=selected.TrialToken!;
                Check(selected.Release==first && trial.Length==32,"queued release starts a trial after restart");
                Check(!launcher.Confirm("wrong"),"wrong token cannot confirm");
                Check(!launcher.Rollback("wrong"),"wrong token cannot roll back");
                Reject(()=>launcher.Queue(next),"queue while startup trial is running");
                // Simulate process/power loss before a healthy startup acknowledgement.
            }
            using(var launcher=new NativeLaunchState(root,"1.0.0"))
            {
                Check(launcher.BeginLaunch()==new NativeLaunchSelection(null,null),"unconfirmed trial recovers bundled version");
                Reject(()=>launcher.Queue(first),"previously failed version");
                Check(!launcher.Confirm(trial),"late confirmation cannot resurrect failed trial");
                launcher.Queue(failed);var selected=launcher.BeginLaunch();
                Check(launcher.Confirm(selected.TrialToken!),"healthy trial commits");
                Check(!launcher.Confirm(selected.TrialToken!),"confirmation is not reusable");
            }
            using(var launcher=new NativeLaunchState(root,"1.0.0"))
            {
                Check(launcher.BeginLaunch()==new NativeLaunchSelection(failed,null),"confirmed release survives restart without a new trial");
                launcher.Queue(next);var selected=launcher.BeginLaunch();
                Check(selected.Release==next,"next release selected");
                Check(launcher.Rollback(selected.TrialToken!),"timeout or load failure can roll back");
                Check(launcher.BeginLaunch()==new NativeLaunchSelection(failed,null),"rollback restores last healthy release");
                Reject(()=>launcher.Queue(next),"failed version quarantined");
                launcher.Queue(Release("1.10.0"));Reject(()=>launcher.Queue(Release("1.9.0")),"older than pending (numeric version comparison)");
            }
            using(var launcher=new NativeLaunchState(root,"2.0.0"))
                Check(launcher.BeginLaunch()==new NativeLaunchSelection(null,null),"newer full installation supersedes older active and pending versions");
            using(var launcher=new NativeLaunchState(root,"2.0.0"))
            {
                Check(launcher.BeginLaunch()==new NativeLaunchSelection(null,null),"full installation selection persists");
                launcher.Queue(Release("2.1.0"));launcher.BeginLaunch();
            }
            using(var launcher=new NativeLaunchState(root,"2.0.0"))
                Check(launcher.BeginLaunch()==new NativeLaunchSelection(null,null),"interrupted later trial retains newer bundled fallback");
            Check(Directory.GetFiles(root,"*.tmp").Length==0,"atomic state writes leave no temporary files");
            using(var launcher=new NativeLaunchState(Path.Combine(root,"defer"),"1.0.0"))
            {
                launcher.Queue(first);launcher.Confirm(launcher.BeginLaunch().TrialToken!);
                launcher.Queue(next);var selected=launcher.BeginLaunch();
                Check(!launcher.DeferTrial("wrong"),"wrong token cannot defer a trial");
                Check(launcher.DeferTrial(selected.TrialToken!) && launcher.CurrentVersion==first.Version,"deferred trial retains previous active version");
                Check(!launcher.Confirm(selected.TrialToken!),"deferred trial cannot be confirmed by stale readiness");
                var retried=launcher.BeginLaunch();
                Check(retried.Release==next && retried.TrialToken is not null && retried.TrialToken!=selected.TrialToken,"deferred candidate retries with a fresh readiness token");
                launcher.DeferTrial(retried.TrialToken!);
            }
            string failureRoot=Path.Combine(root,"write-failure");
            using(var launcher=new NativeLaunchState(failureRoot,"1.0.0"))
            {
                launcher.Queue(first);
                string stateFile=Path.Combine(failureRoot,"launcher-state.json");byte[] saved=File.ReadAllBytes(stateFile);
                File.Delete(stateFile);Directory.CreateDirectory(stateFile);
                try {launcher.Queue(next);throw new Exception("Ignored failed state write");}catch(Exception ex) when(ex is IOException or UnauthorizedAccessException) {checks++;}
                Directory.Delete(stateFile);File.WriteAllBytes(stateFile,saved);
                Check(launcher.BeginLaunch().Release==first,"failed save does not mutate in-memory pending release");
            }

            File.WriteAllText(Path.Combine(root,"launcher-state.json"),"{bad json");
            try {using var launcher=new NativeLaunchState(root,"2.0.0");throw new Exception("Ignored corrupt state");}
            catch(System.Text.Json.JsonException) {checks++;}
            Check(File.ReadAllText(Path.Combine(root,"launcher-state.json"))=="{bad json","corrupt state preserved for recovery");
            File.Delete(Path.Combine(root,"launcher-state.json"));
            using(var launcher=new NativeLaunchState(root,"2.0.0"))
            {
                launcher.Dispose();
                try {launcher.BeginLaunch();throw new Exception("Used disposed launcher");}catch(ObjectDisposedException) {checks++;}
            }
        }
        finally {Directory.Delete(root,true);}
        Console.WriteLine($"Native launcher state: {checks} checks passed.");return checks;
    }
}
