using DigiTactics.Networking;
using DigiTactics.Presentation;
using DigiTactics.Storage;

internal static class SocialRosterChecks
{
    public static int Run()
    {
        int checks=0;
        void Check(bool value,string why){checks++;if(!value)throw new InvalidOperationException("Social roster: "+why);}
        SteamFriendRoom Room(ulong lobby,ulong friend,string state="lobby",string version=NetworkProtocol.Version)=>
            new(new(lobby,new Dictionary<string,string>{{"dt_game","digitactics"},{"dt_ver",version},{"state",state},{"name","Friend room"},{"round","2-3"}},2,8),[new(friend,"Same name")]);
        SteamFriendPresence[] friends=[
            new(1,"Self",SteamPersonaState.Online,480,10),new(2,"Party friend",SteamPersonaState.Online,480,10),
            new(3,"Same name",SteamPersonaState.Online,480,20),new(4,"Same name",SteamPersonaState.Offline,480,30),
            new(5,"Away friend",SteamPersonaState.Away,0,0),new(6,"Playing friend",SteamPersonaState.Online,480,30),
            new(7,"Other game",SteamPersonaState.Online,999,40),new(0,"Invalid",SteamPersonaState.Online,0,0),
            new(3,"Duplicate id",SteamPersonaState.Online,480,20)];
        var rooms=new[]{Room(20,3),Room(30,6,"playing"),Room(40,7,"lobby","old")};
        SocialPartyMember[] party=[new(1,"Self",true,true,true),new(2,"Party friend",false,false,false),new(2,"Duplicate",false,false,true)];
        var recent=new[]{new RecentPlayer("Self",0),new RecentPlayer("Same name",0),new RecentPlayer("Recent tamer",0),new RecentPlayer("Recent tamer",1)};
        var rows=SocialRoster.Build(1,"Self",10,false,party,friends,rooms,recent);
        Check(rows.Count(row=>row.Group==SocialGroup.Party)==2,"party identity deduplication");
        Check(rows.Single(row=>row.Id==1).Note=="Host / you","party host/self labels");
        Check(rows.Single(row=>row.Id==2).Note=="Member / offline"&&!rows.Single(row=>row.Id==2).Online,"disconnected party member");
        Check(rows.Count(row=>row.Name=="Same name")==2,"distinct Steam identities may share names");
        Check(rows.Single(row=>row.Id==4).Group==SocialGroup.Offline&&rows.Single(row=>row.Id==4).JoinLobby is null,"offline state wins over stale game/lobby");
        Check(rows.Single(row=>row.Id==5).Note=="Away"&&rows.Single(row=>row.Id==5).Group==SocialGroup.Online,"away remains online");
        Check(rows.Single(row=>row.Id==6).Note=="In game / 2-3"&&rows.Single(row=>row.Id==6).Group==SocialGroup.InGame,"compatible match round");
        Check(rows.Single(row=>row.Id==7).JoinLobby is null&&rows.Single(row=>row.Id==7).Note=="Playing another game","incompatible game is not joinable");
        Check(rows.Single(row=>row.Id==3).JoinLobby==20&&!rows.Single(row=>row.Id==3).CanJoin,"leave current party before joining another");
        Check(rows.Count(row=>row.Group==SocialGroup.Recent)==1,"recent names exclude self and known friends");
        var idle=SocialRoster.Build(1,"Self",0,false,[],friends,rooms,[]);
        Check(idle.Single(row=>row.Id==3).CanJoin,"idle client can join compatible friend lobby");
        Check(idle.All(row=>row.Id!=1&&row.Id!=0),"self and invalid identities excluded");
        var playing=SocialRoster.Build(1,"Self",0,true,[],friends,rooms,[]);
        Check(playing.All(row=>!row.CanJoin),"active match cannot join another lobby");
        var current=SocialRoster.Build(1,"Self",20,false,[],friends,rooms,[]);
        Check(current.Single(row=>row.Id==3).JoinLobby is null,"current lobby is not offered again");
        var stale=SocialRoster.Build(1,"Self",0,false,[],friends,[Room(20,99),Room(30,6,"over")],[]);
        Check(stale.All(row=>row.JoinLobby is null),"room association and finished-room join safety");
        Check(SocialRoster.Clean("\0 Friend\n") == "Friend"&&SocialRoster.Clean(new string('x',100)).Length==48,"bounded control-free names");
        Check(SocialRoster.Build(1,"Self",0,false,[],[],[],[new("Past",long.MaxValue)]).Single().Note=="Played together previously","corrupt recent timestamp does not break sidebar");
        Console.WriteLine($"Social roster: {checks} party, presence, identity, recent-player and safe-join checks passed.");return checks;
    }
}
