using System.Runtime.InteropServices;
using DigiTactics.Networking;

internal static class SteamFriendsChecks
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void ModeFn(int mode);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int CountFn();
    public static int Run(string root)
    {
        int checks=0;void Check(bool ok,string why){checks++;if(!ok)throw new InvalidOperationException("Steam friends: "+why);}
        string path=Path.GetFullPath(Path.Combine(root,"artifacts/steam-native/fake_steam_api.dll"));
        nint module=NativeLibrary.Load(path);
        try
        {
            var mode=Marshal.GetDelegateForFunctionPointer<ModeFn>(NativeLibrary.GetExport(module,"FakeSteam_DataRequestMode"));
            using var client=new SteamClient(path,_=>false,480);
            var imageMode=Marshal.GetDelegateForFunctionPointer<ModeFn>(NativeLibrary.GetExport(module,"FakeSteam_ImageMode"));
            var imageReads=Marshal.GetDelegateForFunctionPointer<CountFn>(NativeLibrary.GetExport(module,"FakeSteam_ImageReads"));
            Check(client.Friends.Avatar(0) is null,"missing user has no image");
            imageMode(1);Check(client.Friends.Avatar(client.SteamId) is null&&imageReads()==0,"oversized image rejected before allocation/read");
            imageMode(3);Check(client.Friends.Avatar(client.SteamId) is null&&imageReads()==0,"empty image rejected before read");
            imageMode(2);Check(client.Friends.Avatar(client.SteamId) is null,"failed pixel read returns no avatar");
            imageMode(0);Check(client.Friends.Avatar(76561190000000002) is null,"pending avatar download retried later");
            var avatar=client.Friends.Avatar(76561190000000002);
            Check(avatar is {Width:64,Height:64}&&avatar.Rgba.Length==16384&&avatar.Rgba.Take(4).SequenceEqual(new byte[]{200,100,50,255}),"native RGBA layout and color channels preserved");
            int reads=imageReads();Check(ReferenceEquals(avatar,client.Friends.Avatar(client.SteamId))&&imageReads()==reads,"same image handle reuses decoded pixels");
            var created=client.Matchmaking.Create(SteamLobbyVisibility.Private);client.PumpCallbacks();ulong id=created.Result;
            client.Matchmaking.SetData(id,new Dictionary<string,string>{["dt_game"]="digitactics",["dt_ver"]=NetworkProtocol.Version,["name"]="Friend room"});
            var list=client.Friends.ListRooms();
            Check(!list.IsCompleted&&ReferenceEquals(list,client.Friends.ListRooms()),"metadata wait and concurrent refresh coalescing");
            client.PumpCallbacks();
            Check(list.Result.Count==1&&list.Result[0].Lobby.Id==id&&list.Result[0].Lobby.Data["name"]=="Friend room","friend game/lobby struct and asynchronous metadata");
            Check(list.Result[0].Friends.Count==1&&list.Result[0].Friends[0].Id==76561190000000002&&list.Result[0].Friends[0].Name=="Tamer 2","friend identity and UTF8 name binding");
            Check(client.Friends.Name(0)=="","invalid identity is not sent to native persona API");
            client.Matchmaking.SetData(id,new Dictionary<string,string>{["dt_ver"]="old"});
            list=client.Friends.ListRooms();client.PumpCallbacks();Check(list.Result.Count==0,"incompatible friend room hidden");
            client.Matchmaking.SetData(id,new Dictionary<string,string>{["dt_ver"]=NetworkProtocol.Version});
            mode(1);list=client.Friends.ListRooms();Check(list.IsCompletedSuccessfully&&list.Result.Count==0,"failed metadata request does not return stale cached room");
            mode(2);list=client.Friends.ListRooms();Thread.Sleep(1550);client.PumpCallbacks();Check(list.IsCompletedSuccessfully&&list.Result.Count==0,"missing callback has bounded wait");
            list=client.Friends.ListRooms();client.Friends.Dispose();Check(list.IsFaulted&&list.Exception!.InnerException is ObjectDisposedException,"dispose terminates pending discovery");
            mode(0);
        }
        finally {NativeLibrary.Free(module);}
        Console.WriteLine($"Steam friends: {checks} native discovery, metadata, timeout and lifecycle checks passed.");return checks;
    }
}
