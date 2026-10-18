using System.Runtime.InteropServices;
using DigiTactics.Networking;

internal static class SteamPresenceChecks
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void ModeFn(int mode);
    public static int Run(string root)
    {
        int checks=0;void Check(bool value,string why){checks++;if(!value)throw new InvalidOperationException("Steam presence: "+why);}
        string path=Path.GetFullPath(Path.Combine(root,"artifacts/steam-native/fake_steam_api.dll"));
        nint module=NativeLibrary.Load(path);
        var mode=Marshal.GetDelegateForFunctionPointer<ModeFn>(NativeLibrary.GetExport(module,"FakeSteam_PersonaMode"));
        try
        {
            mode(0);using var client=new SteamClient(path,_=>false,480);
            var people=client.Friends.ListPresence();
            Check(people.Count==2,"production native binding enumerates friends");
            Check(people[0] is {State:SteamPersonaState.Online,GameId:480,LobbyId:109775240000000001},"native persona enum and FriendGameInfo struct layout");
            Check(people[1] is {State:SteamPersonaState.Offline,GameId:0,LobbyId:0},"offline friend is retained");
            mode(1);people=client.Friends.ListPresence();
            Check(people[0] is {State:SteamPersonaState.Offline,GameId:0,LobbyId:0},"going offline drops cached game metadata");
            Check(people[1].State==SteamPersonaState.Away,"subsequent refresh sees native status changes");
            mode(2);Check(client.Friends.ListPresence().All(friend=>friend.State==SteamPersonaState.Offline&&friend.GameId==0),"unknown native enum fails closed");
            mode(3);Check(client.Friends.ListPresence().Count==2,"duplicate, self and zero IDs excluded");
            bool wrongThread=Task.Run(()=>{try{client.Friends.ListPresence();return false;}catch(InvalidOperationException){return true;}}).GetAwaiter().GetResult();
            Check(wrongThread,"owning game thread enforced before native reads");
            client.Friends.Dispose();bool disposed=false;try{client.Friends.ListPresence();}catch(ObjectDisposedException){disposed=true;}
            Check(disposed,"disposed native owner cannot be read");
        }
        finally {mode(0);NativeLibrary.Free(module);}
        Console.WriteLine($"Steam presence: {checks} native state, privacy, deduplication and lifecycle checks passed.");return checks;
    }
}
