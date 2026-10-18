using System.Diagnostics;
using System.Text.Json;
using DigiTactics.Networking;

namespace DigiTactics.Desktop;

/// <summary>Opt-in live Steam acceptance with one already signed-in account. Never joins another
/// player's room, invites anyone, or sends a message to another identity.</summary>
internal static class SteamLiveSmoke
{
    public static int Run(string library)
    {
        if(Process.GetProcessesByName("steam").Length==0)
        {
            Console.WriteLine("Steam live smoke skipped: Steam is not running; no checks performed.");
            return 2;
        }
        ulong self=0,lobby=0;int checks=0;
        void Check(bool value,string description)
        {
            if(!value)throw new InvalidOperationException("Steam live smoke: "+description);
            checks++;Console.WriteLine("Steam live: "+description+" passed.");
        }
        using var client=new SteamClient(library,id=>self!=0 && id==self,480);self=client.SteamId;
        void PumpFor(int milliseconds,Func<bool>? done=null)
        {
            var timer=Stopwatch.StartNew();
            while(timer.ElapsedMilliseconds<milliseconds && !(done?.Invoke()??false)) {client.PumpCallbacks();Thread.Sleep(10);}
        }
        T Complete<T>(Task<T> operation)
        {
            PumpFor(35000,()=>operation.IsCompleted);
            if(!operation.IsCompleted)throw new TimeoutException("Steam live smoke request did not complete.");
            return operation.GetAwaiter().GetResult();
        }
        SteamAvatar? LoadAvatar(ulong id)
        {
            SteamAvatar? avatar=client.Friends.Avatar(id);
            PumpFor(3000,()=>{avatar=client.Friends.Avatar(id);return avatar is not null;});
            if(avatar is not null)Check(avatar.Width>0 && avatar.Height>0 && avatar.Rgba.Length==avatar.Width*avatar.Height*4,"avatar pixel dimensions");
            return avatar;
        }
        try
        {
            Check(client.AppId==480 && !string.IsNullOrWhiteSpace(client.Name),"application identity and local persona");
            PumpFor(500);
            Check(client.Messages.Poll(Environment.TickCount64,(_,_)=>{})==0,"initial self receive queue empty");
            lobby=Complete(client.Matchmaking.Create(SteamLobbyVisibility.Private));
            Check(lobby!=0,"private lobby creation");
            Check(client.Matchmaking.SetData(lobby,new Dictionary<string,string>{{"dt_game","digitactics"},{"dt_ver","native-live-test"},{"name","Native single-account verification"}}),"private lobby metadata write");
            var data=client.Matchmaking.Read(lobby);
            Check(data.Data["name"]=="Native single-account verification" && data.Data["dt_ver"]=="native-live-test","private lobby metadata read");
            Check(client.Matchmaking.Owner(lobby)==self && client.Matchmaking.Members(lobby).SequenceEqual(new[]{self}),"private lobby owner and sole membership");
            Check(!string.IsNullOrWhiteSpace(client.Friends.Name(self)),"local persona lookup");
            bool ownAvatar=LoadAvatar(self) is not null;
            var friends=client.Friends.ListFriends();bool friendAvatar=false;
            if(friends.Count>0)
            {
                Check(!string.IsNullOrWhiteSpace(client.Friends.Name(friends[0].Id)),"friend persona lookup");
                friendAvatar=LoadAvatar(friends[0].Id) is not null;
            }
            Console.WriteLine($"Steam live: friends enumerated={friends.Count}; own avatar={(ownAvatar?"loaded":"not available")}; first friend avatar={(friends.Count==0?"not applicable":friendAvatar?"loaded":"not available")}.");
            string payload=JsonSerializer.Serialize(new {kind="digitactics-native-self-check",nonce=Guid.NewGuid().ToString("N"),text="Unicode \u03a9 \u65e5\u672c 100%"});
            bool accepted=client.Messages.Send(self,payload,true);string? received=null;
            PumpFor(4000,()=>
            {
                client.Messages.Poll(Environment.TickCount64,(sender,text)=>
                {
                    if(sender!=self)throw new InvalidOperationException("Steam live transport admitted a nonlocal identity.");
                    received=text;
                });
                return received is not null;
            });
            if(received is not null)Check(received==payload,"self loopback payload intact");
            Console.WriteLine($"Steam live: self-send accepted={accepted}; loopback={(received is null?"not delivered (optional Steam behavior)":"delivered intact")}. No remote peer tested.");
            var friendRooms=Complete(client.Friends.ListRooms());
            var publicRooms=Complete(client.Matchmaking.ListCompatible());
            Check(friendRooms.All(r=>r.Lobby.Data["dt_game"]=="digitactics" && r.Lobby.Data["dt_ver"]==NetworkProtocol.Version),"compatible friend-room discovery");
            Check(publicRooms.All(r=>r.Data["dt_game"]=="digitactics" && r.Data["dt_ver"]==NetworkProtocol.Version),"compatible public-room discovery");
            Console.WriteLine($"Steam live: compatible friend rooms={friendRooms.Count}; public rooms={publicRooms.Count}; none joined.");
            client.Matchmaking.Leave(lobby);lobby=0;client.Messages.ClosePeer(self);PumpFor(100);
            Console.WriteLine($"Steam one-account live smoke passed: {checks} checks; temporary private lobby left. This verifies local Steam APIs and optional self loopback, not remote multiplayer/reconnect/host migration.");
            return 0;
        }
        finally {if(lobby!=0)client.Matchmaking.Leave(lobby);}
    }
}
