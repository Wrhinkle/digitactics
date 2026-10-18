using System.Diagnostics;
using DigiTactics.Storage;

internal static class NativeSteamLaunchChecks
{
    public static async Task Child(string[] args)
    {
        string directory=args[Array.IndexOf(args,"--native-launch-instance-child")+1];
        int delayAt=Array.IndexOf(args,"--instance-delay");
        if(delayAt>=0)await Task.Delay(int.Parse(args[delayAt+1]));
        using var instance=NativeGameInstance.Open(NativeGameInstance.ProfileDirectory(args),NativeGameInstance.ParseLobbyArguments(args));
        if(!instance.IsPrimary){Environment.ExitCode=NativeGameInstance.ForwardedExitCode;return;}
        // A headless fixture acknowledgement exercises launcher plumbing only;
        // this is deliberately not evidence of a rendered game startup.
        using var signal=NativeStartupSignal.FromEnvironment();
        if(signal is not null)await signal.ReportReady();
        string events=Path.Combine(directory,"events.txt");
        File.AppendAllText(events,"PRIMARY\n");
        var timer=Stopwatch.StartNew();
        while(!File.Exists(Path.Combine(directory,"stop")) && timer.Elapsed<TimeSpan.FromSeconds(20))
        {
            while(instance.TryTake(out var lobby))File.AppendAllText(events,lobby?.ToString(System.Globalization.CultureInfo.InvariantCulture)+"\n");
            await Task.Delay(10);
        }
    }

    public static async Task<int> Run(string launcher,string desktop)
    {
        int checks=0;void Check(bool value,string reason){checks++;if(!value)throw new InvalidOperationException("Native Steam launch: "+reason);}
        string root=Path.Combine(Path.GetTempPath(),"digitactics-steam-launch-"+Guid.NewGuid().ToString("N"));
        string bundle=Path.Combine(root,"bundle"),profile=Path.Combine(root,"profile"),state=Path.Combine(root,"state"),events=Path.Combine(root,"events.txt");
        Directory.CreateDirectory(bundle);
        foreach(string file in Directory.GetFiles(AppContext.BaseDirectory))File.Copy(file,Path.Combine(bundle,Path.GetFileName(file)));
        File.Copy(Path.Combine(bundle,"DigiTactics.Tests.exe"),Path.Combine(bundle,"DigiTactics.Desktop.exe"));
        var processes=new List<Process>();
        Process Start(string executable,IEnumerable<string> arguments)
        {
            var start=new ProcessStartInfo(Path.GetFullPath(executable)){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
            start.Environment["DOTNET_ROOT"]=Path.GetFullPath(Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(),"../../.."));
            foreach(string argument in arguments)start.ArgumentList.Add(argument);
            var process=Process.Start(start)??throw new IOException("Launch fixture failed to start.");processes.Add(process);return process;
        }
        string[] Arguments(string lobby,int delay=0)=>["+connect_lobby",lobby,"--bundle",bundle,"--version","1.0.0","--state",state,"--no-updates","--","--native-launch-instance-child",root,"--profile-directory",profile,"--instance-delay",delay.ToString(System.Globalization.CultureInfo.InvariantCulture)];
        string[] Events(){try{return File.Exists(events)?File.ReadAllLines(events):[];}catch(IOException){return [];}}
        async Task Until(Func<bool> condition)
        {var timer=Stopwatch.StartNew();while(!condition()){if(timer.Elapsed>TimeSpan.FromSeconds(12))throw new TimeoutException("Launch fixture condition timed out.");await Task.Delay(20);}}
        async Task Exit(Process process,int code)
        {
            Task<string> output=process.StandardOutput.ReadToEndAsync(),error=process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(12));
            Check(process.ExitCode==code,$"process exit expected {code}: "+await output+await error);
        }
        try
        {
            var first=Start(launcher,Arguments("109775240000000001",1200));
            await Until(()=>File.Exists(Path.Combine(state,"launcher.lock")));
            Check(!Directory.Exists(profile),"first launch state lease precedes profile/broker creation");
            var second=Start(launcher,Arguments("109775240000000002"));
            await Exit(second,0);
            await Until(()=>Events().Contains("109775240000000002"));
            Check(Events().Count(line=>line=="PRIMARY")==1 && Events().Count(line=>line=="109775240000000001")==1 && Events().Count(line=>line=="109775240000000002")==1,"raw Steam arguments and startup-racing launcher reach one game exactly once");
            var direct=Start(desktop,["--profile-directory",profile,"--steam-api",Path.Combine(bundle,"missing-steam-fixture.dll"),"+connect_lobby","109775240000000003"]);
            await Exit(direct,NativeGameInstance.ForwardedExitCode);
            await Until(()=>Events().Contains("109775240000000003"));
            Check(Events().Count(line=>line=="PRIMARY")==1,"direct Desktop launch forwards before graphics or Steam initialization");
            int focus=Events().Count(line=>line.Length==0);
            var malformed=Start(launcher,Arguments("not-a-lobby"));await Exit(malformed,0);
            await Until(()=>Events().Count(line=>line.Length==0)==focus+1);
            Check(!Events().Contains("not-a-lobby"),"malformed Steam identity becomes focus only");
            File.WriteAllText(Path.Combine(root,"stop"),"");await Exit(first,0);
            using var lease=new NativeLaunchState(state,"1.0.0");
            Check(lease.CurrentVersion=="1.0.0","normal forwarded launches preserve launcher version and release its lease");
        }
        finally
        {
            foreach(var process in processes){try{if(!process.HasExited){process.Kill(entireProcessTree:true);await process.WaitForExitAsync();}}finally{process.Dispose();}}
            Directory.Delete(root,true);
        }
        Console.WriteLine($"Native Steam launch routing: {checks} headless launcher/Desktop process checks passed; no Steam invites, remote joins, or GPU used.");return checks;
    }
}
