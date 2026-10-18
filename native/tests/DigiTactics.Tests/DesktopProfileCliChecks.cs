using System.Diagnostics;
using DigiTactics.Storage;

internal static class DesktopProfileCliChecks
{
    public static async Task Run(string root,string executable)
    {
        string directory=Path.Combine(Path.GetTempPath(),"digitactics-profile-cli-"+Guid.NewGuid().ToString("N"));
        string backup=Path.GetFullPath(Path.Combine(root,"tests/fixtures/legacy-profile.json"));
        byte[] original=File.ReadAllBytes(backup);
        var summary=LegacyProfileArchive.Inspect(original);
        async Task<(int Code,string Output,string Error)> Invoke(string command)
        {
            var start=new ProcessStartInfo(Path.GetFullPath(executable)){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
            start.Environment["DOTNET_ROOT"]=Path.GetFullPath(Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(),"../../.."));
            foreach(string argument in new[]{command,backup,"--profile-directory",directory})start.ArgumentList.Add(argument);
            using var process=Process.Start(start)??throw new IOException("Desktop CLI did not start.");
            Task<string> output=process.StandardOutput.ReadToEndAsync(),error=process.StandardError.ReadToEndAsync();
            using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(20));
            try {await process.WaitForExitAsync(timeout.Token);}
            catch {if(!process.HasExited){process.Kill(true);await process.WaitForExitAsync();}throw;}
            return(process.ExitCode,await output,await error);
        }
        void Check(bool value,string message){if(!value)throw new InvalidOperationException("Desktop profile CLI: "+message);}
        try
        {
            Check(NativeApplicationErrorChecks.IsGuiExecutable(executable),"production game executable would open a console window.");
            var staged=await Invoke("--stage-profile");
            string archive=Path.Combine(directory,"imports",summary.Sha256+".json");
            Check(staged.Code==0&&File.Exists(archive)&&File.ReadAllBytes(archive).AsSpan().SequenceEqual(original),"staging did not honor the explicit profile directory or preserve exact backup bytes.");
            Check(staged.Output.Contains(archive,StringComparison.Ordinal)&&!File.Exists(Path.Combine(directory,"profile.json")),"staging output or non-application invariant differs.");
            var repeated=await Invoke("--stage-profile");
            Check(repeated.Code==0&&Directory.GetFiles(Path.Combine(directory,"imports")).Length==1,"staging was not idempotent.");
            var imported=await Invoke("--import-profile");
            Check(imported.Code==0&&imported.Output.Contains("Profile imported",StringComparison.Ordinal),"GUI executable lost successful CLI import or stdout.");
            string profile=Path.Combine(directory,"profile.json");byte[] saved=File.ReadAllBytes(profile);
            var duplicate=await Invoke("--import-profile");
            Check(duplicate.Code==1&&duplicate.Error.Contains("Digi-Tactics",StringComparison.Ordinal)&&File.ReadAllBytes(profile).AsSpan().SequenceEqual(saved),"duplicate import replaced progress or lost failure diagnostics.");
            Check(File.ReadAllBytes(backup).AsSpan().SequenceEqual(original),"CLI changed the original exported backup.");
            Console.WriteLine("Desktop profile CLI: 7 GUI-subsystem, real-process staging, explicit-directory, import, stdout/stderr and preservation checks passed.");
        }
        finally
        {
            string resolved=Path.GetFullPath(directory);
            if(Path.GetDirectoryName(resolved)!=Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar))throw new InvalidOperationException("Unexpected profile CLI cleanup directory.");
            if(Directory.Exists(resolved))Directory.Delete(resolved,true);
        }
    }
}
