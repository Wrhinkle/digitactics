using System.Diagnostics;
using System.Text.Json;
using DigiTactics.Storage;

internal static class NativeJobChecks
{
    private static ProcessStartInfo Start(params string[] args)
    {
        return NativeTestProcess.Start(args);
    }
    public static async Task Child(string[] args)
    {
        if(args[0]=="--native-job-echo")
        {File.WriteAllText(args[1],JsonSerializer.Serialize(new {Arguments=args[2..],Value=Environment.GetEnvironmentVariable("DT_NATIVE_ECHO"),Directory=Environment.CurrentDirectory}));return;}
        if(args[0]=="--native-job-parent")
        {
            using var child=NativeChildProcess.Start(Start("--native-job-child",args[1]));
            await Task.Delay(Timeout.Infinite);return;
        }
        using var descendant=Process.Start(Start("--native-supervisor-child","hang"))!;
        File.WriteAllText(args[1],JsonSerializer.Serialize(new[]{Environment.ProcessId,descendant.Id}));
        await Task.Delay(Timeout.Infinite);
    }
    public static async Task<int> Run()
    {
        int checks=0;
        void Check(bool ok,string reason) {checks++;if(!ok)throw new Exception("Native process job: "+reason);}
        string root=Path.Combine(Path.GetTempPath(),"digitactics-job-test-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
        using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(15));
        try
        {
            string output=Path.Combine(root,"arguments.json");
            string[] arguments=["","space value","quote\"value","C:\\trailing\\","\\\"combo","\u03a9"];
            var start=Start(new[]{"--native-job-echo",output}.Concat(arguments).ToArray());
            start.Environment["DT_NATIVE_ECHO"]="a=b \u03a9";start.WorkingDirectory=root;
            using(var child=NativeChildProcess.Start(start))
            {
                await child.Process.WaitForExitAsync(timeout.Token);Check(child.Process.ExitCode==0,"child exits normally");
            }
            using(var value=JsonDocument.Parse(File.ReadAllBytes(output)))
            {
                Check(value.RootElement.GetProperty("Arguments").EnumerateArray().Select(v=>v.GetString()).SequenceEqual(arguments),"empty, spaced, quoted, trailing-slash and Unicode arguments preserved");
                Check(value.RootElement.GetProperty("Value").GetString()=="a=b \u03a9","Unicode environment forwarded");
                Check(value.RootElement.GetProperty("Directory").GetString()==root,"working directory forwarded");
            }
            string pids=Path.Combine(root,"pids.json");
            using var parent=Process.Start(Start("--native-job-parent",pids))!;
            Process? childProcess=null,grandchild=null;
            try
            {
                int[]? ids=null;
                while(ids is null)
                {
                    timeout.Token.ThrowIfCancellationRequested();
                    try {if(File.Exists(pids))ids=JsonSerializer.Deserialize<int[]>(File.ReadAllBytes(pids));}catch(JsonException) { }
                    if(ids is null)await Task.Delay(20,timeout.Token);
                }
                childProcess=Process.GetProcessById(ids[0]);grandchild=Process.GetProcessById(ids[1]);
                Check(!childProcess.HasExited && !grandchild.HasExited,"both descendants running before parent termination");
                parent.Kill();await parent.WaitForExitAsync(timeout.Token);
                await childProcess.WaitForExitAsync(timeout.Token);await grandchild.WaitForExitAsync(timeout.Token);
                Check(childProcess.HasExited,"launcher crash kills direct child");
                Check(grandchild.HasExited,"launcher crash kills descendant without managed cleanup");
            }
            finally
            {
                if(!parent.HasExited) {parent.Kill(entireProcessTree:true);await parent.WaitForExitAsync();}
                childProcess?.Dispose();grandchild?.Dispose();
            }
        }
        finally {Directory.Delete(root,true);}
        Console.WriteLine($"Native process jobs: {checks} forwarding and abrupt-parent-exit checks passed.");return checks;
    }
}
