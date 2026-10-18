using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

/// <summary>Self-launch fixtures preserve the entry assembly when the parent was started with dotnet.</summary>
internal static class NativeTestProcess
{
    private static string EntryAssembly => Assembly.GetEntryAssembly()?.Location is {Length:>0} location
        ? Path.GetFullPath(location) : throw new IOException("Test entry assembly is unavailable.");

    public static ProcessStartInfo Start(params string[] arguments)
    {
        string host=Environment.ProcessPath??throw new IOException("Test process executable is unavailable.");
        var start=new ProcessStartInfo(host){UseShellExecute=false,CreateNoWindow=true};
        if(Path.GetFileNameWithoutExtension(host).Equals("dotnet",StringComparison.OrdinalIgnoreCase))start.ArgumentList.Add(EntryAssembly);
        start.Environment["DOTNET_ROOT"]=Path.GetFullPath(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(),"../../.."));
        foreach(string argument in arguments)start.ArgumentList.Add(argument);
        return start;
    }

    // Launcher fixtures require a real exe because production launches Desktop.exe directly.
    // The build produces this companion apphost even when tests are invoked as `dotnet Tests.dll`.
    public static string AppHost
    {
        get
        {
            string path=Path.ChangeExtension(EntryAssembly,".exe");
            return File.Exists(path)?path:throw new FileNotFoundException("The launcher fixture requires the test build's companion apphost executable.",path);
        }
    }
}
