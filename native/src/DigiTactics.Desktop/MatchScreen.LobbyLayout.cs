using System.Text.Json.Nodes;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Networking;
using DigiTactics.Rules;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private readonly ImInputString lobbyRoomName=new(160);
    private static readonly string[] LobbyAiNames=["Tai","Matt","Sora","Izzy","Mimi","Joe","Kari","T.K."];
    internal int LobbySeatCards {get;private set;}
    internal float LobbyBodyFontSize {get;private set;}
    internal bool LobbyLayoutFits {get;private set;}
    internal string? DisplayedLobbyName=>steam?.Room?.Authority.Lobby?["name"]?.GetValue<string>();
    private string LobbyRoomName
    {
        get
        {
            string name=new(lobbyRoomName.ToString().Where(c=>c>=32&&c is not ('<' or '>' or '&' or '"' or '\\')).Take(32).ToArray());
            return string.IsNullOrWhiteSpace(name)?SteamName+"'s lobby":name.Trim();
        }
    }
    private static string LobbyEllipsis(string value,float width)
    {
        if(ImGui.CalcTextSize(value).X<=width)return value;
        int length=value.Length;while(length>0&&ImGui.CalcTextSize(value[..length]+"…").X>width)length--;
        return value[..length]+"…";
    }
    private void DrawLobbyAvatar(string? uid,float diameter=40,bool sameLine=true)
    {
        var at=ImGui.GetCursorScreenPos();var draw=ImGui.GetWindowDrawList();
        draw.AddCircleFilled(at+new float2(diameter*.5f),diameter*.5f,new ImColor8(25,43,65));
        if(LobbyAvatarTexture(uid) is {} texture)
            draw.AddImageRounded(new ImTextureRef{_TexID=new((nint)texture.Id)},at,at+new float2(diameter),new(0,0),new(1,1),new ImColor8(255,255,255),diameter*.5f);
        draw.AddCircle(at+new float2(diameter*.5f),diameter*.5f,HomeLine,0,1);
        ImGui.Dummy(new(diameter,diameter));if(sameLine)ImGui.SameLine(0,10);
    }
    private void LobbyHeading(string eyebrow,string title,string description)
    {
        HomeLabel(eyebrow.ToUpperInvariant(),16,HomeGold);HomeLabel(title,32,HomeInk);
        ImGui.PushStyleColor(ImGuiCol.Text,HomeMute);UiText.Wrapped(description);ImGui.PopStyleColor();ImGui.Spacing();
    }
    private void LobbyStatus()
    {
        string message=recentPlayersError.Length>0?recentPlayersError:lobbyMessage;
        if(message.Length==0)return;
        ImGui.PushStyleColor(ImGuiCol.Text,HomeMute);UiText.Wrapped(message);ImGui.PopStyleColor();
    }
    private void MultiplayerMenu()
    {
        RefreshSetSelection();LobbySeatCards=0;LobbyBodyFontSize=17;
        float width=size.X-48,height=size.Y-ClientPageTop-24;LobbyLayoutFits=width>=700&&height>=540;
        HomeCard("Steam multiplayer",new(24,ClientPageTop),new(width,height),false);
        ImGui.PushFont(NativeUiTheme.BodyFont,17);LobbyBodyFontSize=ImGui.GetFontSize();ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,new float2(12,10));ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(10,8));
        if(steam is null)
        {
            LobbyHeading("Multiplayer · Steam","Lobbies","Every lobby has 8 Tamers. Seats you and your friends don't take are played by the AI.");
            ImGui.Spacing();UiText.Wrapped("Connect to Steam to create a lobby, find your friends and join a game."u8);
            if(HomeButton("steamConnect","Connect to Steam",230,44,true))ConnectSteam();
            LobbyStatus();
            ImGui.SetCursorPos(new(12,height-60));if(HomeButton("steamBack","Back",120,40)){if(!PostgameHomeVisible)LeaveSteamRoom();showMultiplayer=false;}
        }
        else if(steam.Room is {} room)DrawLobbyRoom(room,height);
        else DrawLobbyBrowser(height);
        ImGui.PopStyleVar(2);ImGui.PopFont();ImGui.End();
    }
    private void DrawLobbyBrowser(float height)
    {
        LobbyHeading("Multiplayer · Steam","Lobbies","Every lobby has 8 Tamers. Seats you and your friends don't take are played by the AI.");
        if(lobbyRoomName.ToString().Length==0)lobbyRoomName.SetValue((SteamName+"'s lobby").AsSpan());
        ImGui.BeginDisabled(enteringRoom is not null);
        float width=ImGui.GetContentRegionAvail().X;var start=ImGui.GetCursorScreenPos();
        ImGui.BeginGroup();HomeLabel("YOUR NAME",16);HomeLabel(SteamName,18,HomeInk);ImGui.EndGroup();
        ImGui.SetCursorScreenPos(start+new float2(220,0));ImGui.BeginGroup();HomeLabel("LOBBY NAME",16);ImGui.SetNextItemWidth(Math.Max(220,width-440));
        ImGui.InputText("##steamRoomName"u8,lobbyRoomName);SelectionButton("steamRoomName");ImGui.EndGroup();
        ImGui.SetCursorScreenPos(start+new float2(width-200,0));ImGui.BeginGroup();HomeLabel("WHO CAN JOIN",16);ImGui.SetNextItemWidth(200);
        if(ImGui.BeginCombo("##steamVisibility"u8,visibility switch{SteamLobbyVisibility.Public=>"Public",SteamLobbyVisibility.Friends=>"Friends only",_=>"Private"}))
        {
            foreach(var item in Enum.GetValues<SteamLobbyVisibility>())
                if(ImGui.Selectable(item switch{SteamLobbyVisibility.Public=>"Public",SteamLobbyVisibility.Friends=>"Friends only",_=>"Private"},visibility==item))visibility=item;
            ImGui.EndCombo();
        }
        SelectionButton("steamVisibility");ImGui.EndGroup();
        start+=new float2(0,76);ImGui.SetCursorScreenPos(start);ImGui.BeginGroup();HomeLabel("SET",16);ImGui.SetNextItemWidth(width*.34f);DrawSetChoice("steamSet",selectedSet,SelectSet);ImGui.EndGroup();
        ImGui.SetCursorScreenPos(start+new float2(width*.36f,0));ImGui.BeginGroup();HomeLabel("PACE",16);ImGui.SetNextItemWidth(width*.19f);DrawPaceChoice("steamPace",lobbyPaceChoice,true,pace=>SelectPace(pace,true));ImGui.EndGroup();
        ImGui.SetCursorScreenPos(start+new float2(width*.57f,0));ImGui.BeginGroup();HomeLabel("FORMAT",16);ImGui.SetNextItemWidth(width*.16f);DrawFormatChoice("steamFormat",lobbyDuos,format=>lobbyDuos=format=="duos");ImGui.EndGroup();
        ImGui.SetCursorScreenPos(start+new float2(width*.75f,0));ImGui.BeginGroup();HomeLabel("ROUND PLANNING",16);
        foreach(int seconds in new[]{30,45,60})
        {if(seconds!=30)ImGui.SameLine(0,6);if(HomeButton("planningSeconds/"+seconds,seconds+" s",(width*.25f-12)/3,38,lobbyPlanningSeconds==seconds))lobbyPlanningSeconds=seconds;}
        ImGui.EndGroup();
        ImGui.SetCursorScreenPos(start+new float2(0,76));
        if(HomeButton("steamCreate",enteringRoom is null?"Create lobby":"Creating lobby…",210,44,true))CreateSteamRoom();
        ImGui.SameLine();if(HomeButton("steamRefresh",listingRooms is null&&listingFriendRooms is null?"Refresh lobbies":"Searching…",200,44))RefreshSteamRooms();
        ImGui.EndDisabled();ImGui.Spacing();ImGui.Separator();
        var displayed=friendRooms.Select(r=>r.Lobby).Concat(availableRooms).DistinctBy(r=>r.Id).Where(r=>r.Data.GetValueOrDefault("state")!="over").ToArray();
        HomeLabel($"OPEN LOBBIES  ({displayed.Length})",18,HomeGold);
        float listHeight=Math.Max(80,height-ImGui.GetCursorPosY()-95);
        ImGui.BeginChild("Open Steam lobbies"u8,new(0,listHeight));
        foreach(var listed in displayed)DrawListedLobby(listed);
        if(displayed.Length==0)UiText.Wrapped(listingRooms is null&&listingFriendRooms is null?"No compatible lobbies found. Create one and invite your friends.":"Searching Steam for open lobbies…");
        ImGui.EndChild();LobbyStatus();
        ImGui.SetCursorPos(new(12,height-56));if(HomeButton("steamBack","Back",120,40)){if(!PostgameHomeVisible)LeaveSteamRoom();showMultiplayer=false;}
    }
    private void DrawListedLobby(SteamLobby listed)
    {
        var at=ImGui.GetCursorScreenPos();float width=ImGui.GetContentRegionAvail().X;const float height=90;var draw=ImGui.GetWindowDrawList();
        bool friend=friendRooms.Any(room=>room.Lobby.Id==listed.Id);
        draw.AddRectFilled(at,at+new float2(width,height),new ImColor8(8,22,34));draw.AddRect(at,at+new float2(width,height),friend?new ImColor8(63,214,255):HomeLine);
        if(friend)draw.AddRectFilled(at,at+new float2(3,height),new ImColor8(63,214,255));
        ImGui.SetCursorScreenPos(at+new float2(14,10));ImGui.BeginGroup();HomeLabel(LobbyEllipsis(listed.Data["name"],Math.Max(180,width-340)),21,HomeInk);
        HomeLabel(LobbyEllipsis(SetChoices.Label(listed.Data["setId"])+" · "+SetSelection.PaceLabel(listed.Data["pace"])+" · "+(listed.Data["format"]=="duos"?"Duos":"Solo"),Math.Max(180,width-340)),16,HomeMute);
        if(friendRooms.FirstOrDefault(room=>room.Lobby.Id==listed.Id) is {} friends)HomeLabel(LobbyEllipsis("Friends: "+string.Join(", ",friends.Friends.Select(person=>person.Name)),width-340),16,new ImColor8(63,214,255));
        ImGui.EndGroup();
        var pips=at+new float2(width-280,22);
        for(int i=0;i<8;i++){var center=pips+new float2(i*20,0);if(i<listed.Members)draw.AddCircleFilled(center,7,HomeGold);else draw.AddCircle(center,7,HomeLine);}
        ImGui.SetCursorScreenPos(at+new float2(width-280,43));HomeLabel($"{listed.Members}/{listed.Capacity} Tamers",16,HomeMute);
        ImGui.SetCursorScreenPos(at+new float2(width-110,24));ImGui.BeginDisabled(enteringRoom is not null);
        if(HomeButton("steamJoin/"+listed.Id,"Join",96,40,true))JoinSteamRoom(listed.Id);
        ImGui.EndDisabled();ImGui.SetCursorScreenPos(at+new float2(0,height));ImGui.Dummy(new(width,6));
    }
    private void DrawLobbyRoom(NetworkRoom room,float height)
    {
        if(room.Authority.Lobby is not {} lobby)
        {
            LobbyHeading("Multiplayer","Joining lobby","Waiting for the host's lobby details…");LobbyStatus();
            ImGui.SetCursorPos(new(12,height-62));
            if(HomeButton("steamLeave","Cancel join",160,44))LeaveSteamRoom();
            ImGui.SameLine();if(HomeButton("steamBack","Back",120,44)){LeaveSteamRoom();showMultiplayer=false;}
            return;
        }
        bool playing=room.Init is not null,host=room.Authority.IsHost,duo=lobby["format"]?.GetValue<string>()=="duos";
        var slots=lobby["slots"] as JsonObject;int seated=slots?.Count??0;
        LobbyHeading(playing?"Game in progress":"Lobby",lobby["name"]?.GetValue<string>()??"Digi-Tactics lobby",playing?"Take an AI seat that is still alive to join.":$"{seated}/8 players · {lobby["planTime"]?.GetValue<int>()??30} s rounds · empty seats are played by the AI.");
        if(host&&!playing)DrawLobbyRoomChoices(room);
        else HomeLabel(SetChoices.Label(lobby["setId"]?.GetValue<string>())+" · "+SetSelection.PaceLabel(lobby["pace"]?.GetValue<string>())+" · "+(duo?"Duos":"Solo"),18,HomeInk);
        HomeLabel(duo?"Seats with the same team number are partners. Move to an empty seat to choose your team.":"Free for all",16,HomeMute);
        float gridHeight=Math.Max(280,height-ImGui.GetCursorPosY()-120);var grid=ImGui.GetCursorScreenPos();float gap=10,cardWidth=(ImGui.GetContentRegionAvail().X-gap*3)/4,cardHeight=(gridHeight-gap)/2;
        for(int i=0;i<8;i++)DrawLobbySeat(room,i,grid+new float2((i%4)*(cardWidth+gap),(i/4)*(cardHeight+gap)),new(cardWidth,cardHeight));
        ImGui.SetCursorScreenPos(grid+new float2(0,gridHeight));ImGui.Dummy(new(0,4));LobbyStatus();
        ImGui.SetCursorPos(new(12,height-62));
        if(HomeButton("steamLeave","Leave lobby",150,44))LeaveSteamRoom();
        ImGui.SameLine();if(HomeButton("players","Players",110,44))showPlayers=!showPlayers;
        ImGui.SameLine();InviteFriendsButton("steamInvite");
        ImGui.SameLine();if(HomeButton("steamBack","Back",90,44)){if(!PostgameHomeVisible)LeaveSteamRoom();showMultiplayer=false;}
        if(host&&!playing)
        {
            bool yours=slots?.Any(slot=>slot.Value?["uid"]?.GetValue<string>()==room.Authority.LocalId)==true;
            ImGui.SameLine();ImGui.BeginDisabled(!yours);if(HomeButton("steamStart","START GAME",190,44,true))StartSteamMatch(room);ImGui.EndDisabled();
            if(!yours&&ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))ResultTooltip("Take a seat before starting the match.");
        }
        else if(!playing)
        {ImGui.SameLine();HomeLabel("Waiting for the host",16,HomeMute);}
    }
    private void DrawLobbyRoomChoices(NetworkRoom room)
    {
        var lobby=room.Authority.Lobby!;float width=ImGui.GetContentRegionAvail().X;var at=ImGui.GetCursorScreenPos();
        ImGui.BeginGroup();HomeLabel("SET",16);ImGui.SetNextItemWidth(width*.44f);
        DrawSetChoice("steamRoomSet",SetChoices.SavedSet(lobby["setId"]?.GetValue<string>()),id=>{if(room.UpdateLobby(new(){["setId"]=id}))SelectSet(id);});
        ImGui.EndGroup();ImGui.SetCursorScreenPos(at+new float2(width*.46f,0));ImGui.BeginGroup();HomeLabel("PACE",16);ImGui.SetNextItemWidth(width*.27f);
        DrawPaceChoice("steamRoomPace",SetSelection.LobbyPace(lobby["pace"]?.GetValue<string>()),true,pace=>{if(room.UpdateLobby(new(){["pace"]=pace}))SelectPace(pace,true);});
        ImGui.EndGroup();ImGui.SetCursorScreenPos(at+new float2(width*.75f,0));ImGui.BeginGroup();HomeLabel("FORMAT",16);ImGui.SetNextItemWidth(width*.25f);
        DrawFormatChoice("steamRoomFormat",lobby["format"]?.GetValue<string>()=="duos",format=>{if(room.UpdateLobby(new(){["format"]=format}))lobbyDuos=format=="duos";});
        ImGui.EndGroup();
    }
    private void DrawLobbySeat(NetworkRoom room,int index,float2 at,float2 extent)
    {
        var lobby=room.Authority.Lobby!;var seat=lobby["slots"]?[index.ToString()];string? uid=seat?["uid"]?.GetValue<string>();
        bool yours=uid==room.Authority.LocalId,host=uid==lobby["host"]?.GetValue<string>(),playing=room.Init is not null;
        bool available=uid is null&&(!playing||lobby["alive"]?[index]?.GetValue<bool>()!=false);
        string name=seat?["name"]?.GetValue<string>()??("AI: "+LobbyAiNames[index]);var draw=ImGui.GetWindowDrawList();
        draw.AddRectFilledMultiColor(at,at+extent,new ImColor8(11,58,74,uid is null?(byte)70:(byte)190),new ImColor8(11,58,74,uid is null?(byte)70:(byte)190),new ImColor8(1,10,19,245),new ImColor8(1,10,19,245));
        draw.AddRect(at,at+extent,yours?HomeGold:HomeLine,0,ImDrawFlags.None,yours?2:1);
        string number=lobby["format"]?.GetValue<string>()=="duos"?"T"+(index/2+1):(index+1).ToString();
        draw.AddText(at+new float2(extent.X-30,8),HomeMute,number);
        float avatar=Math.Clamp(extent.Y-110,40,76);var portraitAt=at+new float2((extent.X-avatar)/2,8);
        draw.AddCircleFilled(portraitAt+new float2(avatar*.5f),avatar*.5f,new ImColor8(25,43,65));
        string? key=uid is not null&&room.Authority.Presence[uid]?["look"] is JsonObject look?CosmeticLoadouts.CompanionKey(look):null;
        if(!portraits.DrawCompanion(key,portraitAt,avatar))
        {string initials=uid is null?"AI":new(name.Where(char.IsLetterOrDigit).Take(2).ToArray());var measured=ImGui.CalcTextSize(initials);draw.AddText(portraitAt+new float2((avatar-measured.X)/2,(avatar-measured.Y)/2),HomeGold,initials);}
        draw.AddCircle(portraitAt+new float2(avatar*.5f),avatar*.5f,yours?HomeGold:HomeLine,0,2);
        ImGui.SetCursorScreenPos(at+new float2(10,avatar+14));HomeLabel(LobbyEllipsis(name,extent.X-20),18,yours?new ImColor8(255,255,255):HomeInk);
        string tags=yours?(host?"You · Host":"You"):host?"Host":uid is null?(available?"AI Tamer":"Eliminated"):room.Authority.Presence.ContainsKey(uid)?"Ready":"Offline";
        ImGui.SetCursorScreenPos(at+new float2(10,avatar+37));HomeLabel(tags,16,HomeMute);
        LobbyLayoutFits &= avatar+57<=extent.Y-48 && at.Y>=ClientPageTop && at.Y+extent.Y<size.Y-68;
        ImGui.SetCursorScreenPos(at+new float2(10,extent.Y-44));
        bool seated=(lobby["slots"] as JsonObject)?.Any(slot=>slot.Value?["uid"]?.GetValue<string>()==room.Authority.LocalId)==true;
        if(available&&HomeButton("steamSeat/"+index,seated?"Move here":"Take seat",extent.X-20,36))room.ClaimSeat(index,SteamName,Environment.TickCount64);
        else if(yours&&!playing&&HomeButton("steamFree/"+index,"Leave seat",extent.X-20,36))room.FreeSeat(index);
        else if(uid is not null&&!yours)
        {
            bool canRemove=room.Authority.IsHost;float buttonWidth=canRemove?(extent.X-26)/2:extent.X-20;
            if(HomeButton("lobbyMute/"+uid,PlayerMuted(uid)?"Unmute":"Mute",buttonWidth,36))TogglePlayerMute(uid);
            if(canRemove){ImGui.SameLine(0,6);ImGui.BeginDisabled(!room.Authority.Member(uid));if(HomeButton("lobbyRemove/"+uid,"Remove",buttonWidth,36))playerMessage=room.Ban(uid)?"Player removed from this room.":"Player could not be removed.";ImGui.EndDisabled();}
        }
        LobbySeatCards++;
    }
}
