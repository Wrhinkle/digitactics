using System.Diagnostics;
using System.Text.Json;
using DigiTactics.Storage;

/// <summary>Real launcher processes with a headless Desktop fixture. This verifies launch routing,
/// never GPU readiness; test-packaged-update.ps1 separately exercises a rendered candidate.</summary>
internal static class NativeDiagnosticLaunchChecks
{
    public const string ChildVariable="DIGITACTICS_DIAGNOSTIC_LAUNCH_CHILD";
    public static async Task Child(string[] args,string events)
    {
        File.AppendAllText(events,JsonSerializer.Serialize(new {Arguments=args,Directory=AppContext.BaseDirectory,Startup=Environment.GetEnvironmentVariable(NativeStartupSignal.PipeVariable) is not null})+"\n");
        int import=Array.IndexOf(args,"--import-profile");
        if(import>=0)new NativeProfileStore(NativeGameInstance.ProfileDirectory(args)).Import(new ProfileImport(args[import+1]));
        if(args.Contains("--match-smoke"))
        {
            using var ready=NativeStartupSignal.FromEnvironment();
            if(ready is not null)await ready.ReportReady();
        }
        if(args.Contains("--failed-diagnostic-smoke"))Environment.ExitCode=17;
    }

    public static async Task<int> Run(string launcher)
    {
        int checks=0;void Check(bool value,string reason){checks++;if(!value)throw new InvalidOperationException("Diagnostic launcher: "+reason);}
        Check(NativeApplicationErrorChecks.IsGuiExecutable(launcher),"production launcher would open a console window");
        string root=Path.Combine(Path.GetTempPath(),"digitactics-diagnostic-launch-"+Guid.NewGuid().ToString("N"));
        string bundle=Path.Combine(root,"bundle"),stateDirectory=Path.Combine(root,"state"),events=Path.Combine(root,"events.jsonl");
        Directory.CreateDirectory(bundle);
        foreach(string file in Directory.GetFiles(AppContext.BaseDirectory))File.Copy(file,Path.Combine(bundle,Path.GetFileName(file)));
        File.Copy(NativeTestProcess.AppHost,Path.Combine(bundle,"DigiTactics.Desktop.exe"),true);
        var expected=NativeUpdatePackage.Create(bundle,Path.Combine(root,"candidate.zip"),"1.1.0",1);
        var candidate=NativeUpdatePackage.Stage(Path.Combine(root,"candidate.zip"),Path.Combine(stateDirectory,"candidates"),expected,1);
        using(var state=new NativeLaunchState(stateDirectory,"1.0.0"))state.Queue(candidate.Release);
        string statePath=Path.Combine(stateDirectory,"launcher-state.json"),saved=File.ReadAllText(statePath);
        string backup=Path.Combine(root,"profile backup.json"),profile=Path.Combine(root,"profile");
        File.WriteAllText(backup,"{\"format\":\"digitactics-profile\",\"version\":1,\"storage\":{\"dt.name\":\"One import\",\"dt.future\":\"preserved\"},\"music\":[]}");
        var children=new List<Process>();
        async Task<(int Code,string Error)> Launch(string[] game,bool verify=false)
        {
            var start=new ProcessStartInfo(Path.GetFullPath(launcher)){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
            start.Environment["DOTNET_ROOT"]=Path.GetFullPath(Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(),"../../.."));
            start.Environment[ChildVariable]=events;
            foreach(string option in new[]{"run","--bundle",bundle,"--version","1.0.0","--state",stateDirectory,"--no-updates"})start.ArgumentList.Add(option);
            if(verify)start.ArgumentList.Add("--verify-update-startup");
            start.ArgumentList.Add("--");foreach(string argument in game)start.ArgumentList.Add(argument);
            var child=Process.Start(start)??throw new IOException("Launcher fixture did not start.");children.Add(child);
            Task<string> output=child.StandardOutput.ReadToEndAsync(),error=child.StandardError.ReadToEndAsync();
            await child.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(20));await output;
            return(child.ExitCode,await error);
        }
        int Events()=>File.Exists(events)?File.ReadAllLines(events).Length:0;
        try
        {
            foreach(string diagnostic in new[]{"--audio-smoke","--steam-live-smoke","--workbench","--match-smoke","--failed-diagnostic-smoke"})
            {
                int before=Events();var result=await Launch([diagnostic]);
                Check(result.Code==(diagnostic=="--failed-diagnostic-smoke"?17:0),"real launcher preserves diagnostic exit: "+diagnostic+" "+result.Error);
                Check(Events()==before+1 && File.ReadAllText(statePath)==saved,"ordinary diagnostic executes once without consuming/quarantining queued update");
                using var record=JsonDocument.Parse(File.ReadAllLines(events)[^1]);
                Check(!record.RootElement.GetProperty("Startup").GetBoolean() && Path.TrimEndingDirectorySeparator(record.RootElement.GetProperty("Directory").GetString()!)==bundle,"ordinary diagnostic uses current bundle without startup capability");
            }
            int beforeImport=Events();var imported=await Launch(["--import-profile",backup,"--profile-directory",profile]);
            Check(imported.Code==0 && Events()==beforeImport+1,"profile import command executes only once: "+imported.Error);
            Check(new NativeProfileStore(profile).Progress.Name=="One import" && new NativeProfileStore(profile).Storage["dt.future"]=="preserved","actual profile import preserves data");
            Check(File.ReadAllText(statePath)==saved,"profile import leaves pending/trial state byte-for-byte unchanged");
            int beforeInvalid=Events();var invalid=await Launch(["--import-profile",backup,"--profile-directory",profile],true);
            Check(invalid.Code==1 && invalid.Error.Contains("requires -- --match-smoke",StringComparison.Ordinal) && Events()==beforeInvalid && File.ReadAllText(statePath)==saved,"startup verification rejects an import before any execution or state mutation");
            var verified=await Launch(["--match-smoke"],true);
            Check(verified.Code==0,"explicit verification accepts fixture acknowledgement: "+verified.Error);
            using var updated=JsonDocument.Parse(File.ReadAllText(statePath));
            Check(updated.RootElement.GetProperty("active").GetProperty("version").GetString()=="1.1.0" && updated.RootElement.GetProperty("pending").ValueKind==JsonValueKind.Null && updated.RootElement.GetProperty("badVersions").GetArrayLength()==0,"explicit verification consumes pending update only after acknowledgement");
            using var verification=JsonDocument.Parse(File.ReadAllLines(events)[^1]);
            Check(verification.RootElement.GetProperty("Startup").GetBoolean() && Path.TrimEndingDirectorySeparator(verification.RootElement.GetProperty("Directory").GetString()!)==candidate.Directory,"explicit verification executes selected candidate with startup capability");
            Console.WriteLine($"Native diagnostic launcher: {checks} real process argument, one-shot import and pending-update checks passed (headless fixture; no GPU proof).");return checks;
        }
        finally
        {
            foreach(var child in children)
            {
                try{if(!child.HasExited){child.Kill(entireProcessTree:true);await child.WaitForExitAsync();}}catch(InvalidOperationException){}
                child.Dispose();
            }
            Directory.Delete(root,true);
        }
    }
}
