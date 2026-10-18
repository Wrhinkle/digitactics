using System.Diagnostics;
using DigiTactics.Networking;

namespace DigiTactics.Desktop;

internal static class SteamBootstrap
{
    // Valve requires Steam initialization before the OpenGL device is created so
    // its overlay can hook device creation. Smoke/diagnostic entry points opt out.
    public static SteamRoomSession? Start(string? library, bool enabled, out string? error)
    {
        error=null;if(!enabled)return null;
        var processes=Process.GetProcessesByName("steam");
        bool running=processes.Length>0;foreach(var process in processes)process.Dispose();
        if(!running)return null;
        string path=library??Path.Combine(AppContext.BaseDirectory,"steam_api64.dll");
        if(!File.Exists(path)){error="Steam support is missing from this installation.";return null;}
        try{return new(path,480);}
        catch(Exception failure) when(failure is IOException or InvalidOperationException or EntryPointNotFoundException or DllNotFoundException or BadImageFormatException)
        {error="Could not connect to Steam: "+failure.Message;return null;}
    }
}
