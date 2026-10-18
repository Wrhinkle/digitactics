using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using DigiTactics.Storage;

const int launcherVersion=1;
var json=new JsonSerializerOptions {PropertyNamingPolicy=JsonNamingPolicy.CamelCase,UnmappedMemberHandling=JsonUnmappedMemberHandling.Disallow};
try
{
    if(args.Length>0 && args[0]=="package")
    {
        if(args.Length is not (4 or 5))throw new ArgumentException("Usage: package <published-directory> <archive.zip> <version> [minimum-launcher]");
        var descriptor=NativeUpdatePackage.Create(args[1],args[2],args[3],args.Length==5?int.Parse(args[4]):launcherVersion);
        Console.WriteLine(JsonSerializer.Serialize(descriptor,json));return 0;
    }
    int separator=Array.IndexOf(args,"--");
    string[] options=separator<0?args:args[..separator],gameArgs=separator<0?[]:args[(separator+1)..];
    int index=0;string command="run";
    if(options.Length>0 && options[0] is "run" or "stage" or "fetch" or "check")command=options[index++];
    string? owner=null,repository=null;
    if(command=="check")
    {
        if(options.Length-index<2 || gameArgs.Length!=0)throw new ArgumentException("Usage: check <owner> <repository> [launcher options]");
        owner=options[index++];repository=options[index++];
    }
    string? archive=null,descriptorPath=null;
    if(command is "stage" or "fetch")
    {
        if(options.Length-index<2 || gameArgs.Length!=0)throw new ArgumentException("Usage: stage <archive.zip> <descriptor.json>, or fetch <https-url> <descriptor.json> [--bundle directory --version version] [--state directory]");
        archive=options[index++];descriptorPath=options[index++];
    }
    string? bundle=null,version=null;
    string stateDirectory=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"DigiTactics","native-updates");
    while(index<options.Length)
    {
        string option=options[index++];if(index==options.Length)throw new ArgumentException("Missing value after "+option);
        string value=options[index++];
        switch(option)
        {
            case "--bundle": bundle=Path.GetFullPath(value);break;
            case "--version": version=value;break;
            case "--state": stateDirectory=Path.GetFullPath(value);break;
            default:throw new ArgumentException("Unknown launcher option: "+option);
        }
    }
    if((bundle is null)!=(version is null))throw new ArgumentException("Provide both --bundle and --version for a development launch.");
    if(bundle is null)
    {
        string configuration=Path.Combine(AppContext.BaseDirectory,"native-launcher.json");
        var settings=JsonSerializer.Deserialize<LauncherConfiguration>(File.ReadAllBytes(configuration),json)??throw new InvalidDataException("Missing launcher configuration.");
        if(settings.SchemaVersion!=1)throw new InvalidDataException("Unsupported launcher configuration.");
        version=settings.BundledVersion;bundle=Path.Combine(AppContext.BaseDirectory,"app");
    }
    if(!NativeUpdatePackage.ValidVersion(version))throw new InvalidDataException("Invalid bundled application version.");
    if(!File.Exists(Path.Combine(bundle,"DigiTactics.Desktop.exe")))throw new FileNotFoundException("Bundled native application is missing.");
    using var state=new NativeLaunchState(stateDirectory,version!);
    string candidates=Path.Combine(stateDirectory,"candidates");
    if(command=="check")
    {
        using var discovery=new NativeReleaseDiscovery();
        var available=await discovery.Check(owner!,repository!,state.CurrentVersion,launcherVersion,Path.Combine(stateDirectory,"release-cache.json"));
        Console.WriteLine(JsonSerializer.Serialize(available,json));return 0;
    }

    if(command is "stage" or "fetch")
    {
        if(new FileInfo(descriptorPath!).Length>16384)throw new InvalidDataException("Oversized update descriptor.");
        var expected=JsonSerializer.Deserialize<NativePackageExpectation>(File.ReadAllBytes(descriptorPath!),json)??throw new InvalidDataException("Missing native update descriptor.");
        if(command=="fetch")
        {
            using var downloader=new NativeUpdateDownloader();
            archive=await downloader.Download(new Uri(archive!,UriKind.Absolute),expected,Path.Combine(stateDirectory,"downloads"),launcherVersion);
        }
        var staged=NativeUpdatePackage.Stage(archive!,candidates,expected,launcherVersion);
        using var verified=NativeUpdatePackage.OpenCandidate(candidates,staged.Release,launcherVersion);
        state.Queue(staged.Release);
        Console.WriteLine("Native update "+expected.Version+" staged for next launch.");return 0;
    }
    using var cancellation=new CancellationTokenSource();
    Console.CancelKeyPress+=(_,e)=>{e.Cancel=true;cancellation.Cancel();};
    NativeCandidateLease? candidate=null;
    try
    {
        var result=await NativeProcessSupervisor.Run(state,selection=>
        {
            candidate?.Dispose();candidate=null;
            string executable=Path.Combine(bundle,"DigiTactics.Desktop.exe");
            if(selection.Release is not null)
            {
                candidate=NativeUpdatePackage.OpenCandidate(candidates,selection.Release,launcherVersion,cancellation.Token);
                executable=candidate.Executable;
            }
            var start=new ProcessStartInfo(executable) {UseShellExecute=false,CreateNoWindow=true,WorkingDirectory=Path.GetDirectoryName(executable)!};
            foreach(string argument in gameArgs)start.ArgumentList.Add(argument);
            return start;
        },cancellation:cancellation.Token);
        if(result.RolledBack)Console.Error.WriteLine("Update rolled back: "+result.Failure);
        return result.ExitCode;
    }
    finally {candidate?.Dispose();}
}
catch(OperationCanceledException) {return 130;}
catch(Exception error) {Console.Error.WriteLine("Digi-Tactics launcher: "+error.Message);return 1;}

internal sealed record LauncherConfiguration(int SchemaVersion,string BundledVersion);
