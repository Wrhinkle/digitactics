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
    internal bool LobbyArtworkVisible {get;private set;}
    internal (float2 Min,float2 Max) LobbyContentBounds {get;private set;}
    internal (float2 Min,float2 Max) LobbyFooterBounds {get;private set;}
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
        HomeLabel(eyebrow.ToUpperInvariant(),15,HomeGold);HomeLabel(title.ToUpperInvariant(),30,HomeInk);
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
        float width=size.X-48,height=size.Y-ClientPageTop-28;var at=new float2(24,ClientPageTop+12);
        LobbyLayoutFits=width>=700&&height>=520;LobbyArtworkVisible=false;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(24,20));
        HomeCard("Steam multiplayer",at,new(width,height),false);
        // Original .play-mp surface: the lobby painting, shaded down to navy,
        // remains visible behind the form and the eight individual seat cards.
        homeArtwork.Draw("lobby-bg",at,new(width,height),255,true,.4f);LobbyArtworkVisible=true;
        var draw=ImGui.GetWindowDrawList();draw.AddRectFilledMultiColor(at,at+new float2(width,height),new ImColor8(1,10,19,145),new ImColor8(1,10,19,145),new ImColor8(1,10,19,220),new ImColor8(1,10,19,220));
        draw.AddRect(at,at+new float2(width,height),HomeLine);
        LobbyContentBounds=(at+new float2(24,20),at+new float2(width-24,height-20));
        LobbyFooterBounds=(new(at.X+24,at.Y+height-64),new(at.X+width-24,at.Y+height-20));
        ImGui.PushFont(NativeUiTheme.BodyFont,17);LobbyBodyFontSize=ImGui.GetFontSize();ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,new float2(10,8));ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(10,7));
        if(steam is null)
        {
            LobbyHeading("Multiplayer · Steam","Lobbies","Every lobby has 8 Tamers. Seats you and your friends don't take are played by the AI.");
            ImGui.Spacing();UiText.Wrapped("Connect to Steam to create a lobby, find your friends and join a game."u8);
            if(HomeButton("steamConnect","Connect to Steam",230,44,true))ConnectSteam();
            LobbyStatus();
            ImGui.SetCursorScreenPos(LobbyFooterBounds.Min);if(ClientTextButton("steamBack","Back",120,40,false,16))ReturnFromLobbies();
        }
        else if(steam.Room is {} room)DrawLobbyRoom(room,height);
        else DrawLobbyBrowser(height);
        ImGui.PopStyleVar(2);ImGui.PopFont();ImGui.End();ImGui.PopStyleVar();
    }
    private void ReturnFromLobbies()
    {
        // Browsing the client keeps the party and any retained match result.
        // The separate Leave lobby / Cancel join controls own disconnection.
        BackClient();
    }
    private void DrawLobbyBrowser(float height)
    {
        LobbyHeading("Multiplayer · Steam","Lobbies","Every lobby has 8 Tamers. Seats you and your friends don't take are played by the AI.");
        if(lobbyRoomName.ToString().Length==0)lobbyRoomName.SetValue((SteamName+"'s lobby").AsSpan());
        ImGui.BeginDisabled(enteringRoom is not null);
        float width=ImGui.GetContentRegionAvail().X-24;var formAt=ImGui.GetCursorScreenPos();var start=formAt+new float2(12,12);
        var fields=new (string Label,float Width,Action<float> Draw)[]
        {
            ("YOUR NAME",170,w=>
            {
                var point=ImGui.GetCursorScreenPos();var list=ImGui.GetWindowDrawList();list.AddRectFilled(point,point+new float2(w,34),new ImColor8(1,10,19,220));list.AddRect(point,point+new float2(w,34),HomeLine);
                list.AddText(point+new float2(10,8),HomeInk,LobbyEllipsis(SteamName,w-20));ImGui.Dummy(new(w,34));
            }),
            ("LOBBY NAME",220,w=>{ImGui.SetNextItemWidth(w);ImGui.InputText("##steamRoomName"u8,lobbyRoomName);SelectionButton("steamRoomName");}),
            ("SET",240,w=>{ImGui.SetNextItemWidth(w);DrawSetChoice("steamSet",selectedSet,SelectSet);}),
            ("PACE",124,w=>{ImGui.SetNextItemWidth(w);DrawPaceChoice("steamPace",lobbyPaceChoice,true,pace=>SelectPace(pace,true));}),
            ("FORMAT",128,w=>{ImGui.SetNextItemWidth(w);DrawFormatChoice("steamFormat",lobbyDuos,format=>lobbyDuos=format=="duos");}),
            ("ROUNDS",132,w=>
            {
                ImGui.SetNextItemWidth(w);if(ImGui.BeginCombo("##steamPlanning"u8,lobbyPlanningSeconds+" s rounds"))
                {
                    foreach(int seconds in new[]{30,45,60}){if(ImGui.Selectable(seconds+" s rounds",lobbyPlanningSeconds==seconds))lobbyPlanningSeconds=seconds;SelectionButton("planningSeconds/"+seconds);}
                    ImGui.EndCombo();
                }
                SelectionButton("steamPlanning");
            }),
            ("WHO CAN JOIN",146,w=>
            {
                ImGui.SetNextItemWidth(w);if(ImGui.BeginCombo("##steamVisibility"u8,visibility switch{SteamLobbyVisibility.Public=>"Public",SteamLobbyVisibility.Friends=>"Friends only",_=>"Private"}))
                {
                    foreach(var item in Enum.GetValues<SteamLobbyVisibility>())if(ImGui.Selectable(item switch{SteamLobbyVisibility.Public=>"Public",SteamLobbyVisibility.Friends=>"Friends only",_=>"Private"},visibility==item))visibility=item;
                    ImGui.EndCombo();
                }
                SelectionButton("steamVisibility");
            }),
            ("",160,w=>{if(HomeButton("steamCreate",enteringRoom is null?"Create lobby":"Creating…",w,40,true))CreateSteamRoom();}),
            ("",180,w=>{if(HomeButton("steamRefresh",listingRooms is null&&listingFriendRooms is null?"Refresh lobbies":"Searching…",w,40))RefreshSteamRooms();})
        };
        var positions=new float2[fields.Length];float fieldX=0,fieldY=0;
        for(int i=0;i<fields.Length;i++){if(fieldX>0&&fieldX+fields[i].Width>width){fieldX=0;fieldY+=70;}positions[i]=start+new float2(fieldX,fieldY);fieldX+=fields[i].Width+12;}
        float formHeight=fieldY+90;var draw=ImGui.GetWindowDrawList();draw.AddRectFilled(formAt,formAt+new float2(width+24,formHeight),new ImColor8(1,10,19,210));draw.AddRect(formAt,formAt+new float2(width+24,formHeight),HomeLine);
        for(int i=0;i<fields.Length;i++)
        {
            var field=fields[i];ImGui.SetCursorScreenPos(positions[i]);if(field.Label.Length>0)HomeLabel(field.Label,15);
            ImGui.SetCursorScreenPos(positions[i]+new float2(0,24));field.Draw(field.Width);
        }
        ImGui.EndDisabled();ImGui.SetCursorScreenPos(formAt+new float2(0,formHeight+18));
        var displayed=friendRooms.Select(r=>r.Lobby).Concat(availableRooms).DistinctBy(r=>r.Id).Where(r=>r.Data.GetValueOrDefault("state")!="over").ToArray();
        HomeLabel($"OPEN LOBBIES  ({displayed.Length})",18,HomeGold);
        float listHeight=Math.Max(64,LobbyFooterBounds.Min.Y-ImGui.GetCursorScreenPos().Y-44);
        ImGui.BeginChild("Open Steam lobbies"u8,new(0,listHeight));
        foreach(var listed in displayed)DrawListedLobby(listed);
        if(displayed.Length==0)
        {
            var empty=ImGui.GetCursorScreenPos();float emptyWidth=ImGui.GetContentRegionAvail().X;
            draw.AddRectFilled(empty,empty+new float2(emptyWidth,Math.Min(92,listHeight)),new ImColor8(1,10,19,155));
            DrawLobbyDashedBorder(empty,new(emptyWidth,Math.Min(92,listHeight)),HomeLine);
            string emptyMessage=listingRooms is null&&listingFriendRooms is null?"No open lobbies right now. Create one and invite your Steam friends.":"Searching Steam for open lobbies…";
            var measured=ImGui.CalcTextSize(emptyMessage);ImGui.SetCursorScreenPos(empty+new float2(Math.Max(18,(emptyWidth-measured.X)/2),Math.Max(12,(Math.Min(92,listHeight)-measured.Y)/2)));ImGui.PushStyleColor(ImGuiCol.Text,HomeMute);
            UiText.Wrapped(emptyMessage);ImGui.PopStyleColor();
        }
        ImGui.EndChild();LobbyStatus();
        ImGui.SetCursorScreenPos(LobbyFooterBounds.Min);if(ClientTextButton("steamBack","Back",120,40,false,16))ReturnFromLobbies();
    }
    private void DrawListedLobby(SteamLobby listed)
    {
        var at=ImGui.GetCursorScreenPos();float width=ImGui.GetContentRegionAvail().X;const float height=90;var draw=ImGui.GetWindowDrawList();
        bool friend=friendRooms.Any(room=>room.Lobby.Id==listed.Id);
        draw.AddRectFilled(at,at+new float2(width,height),new ImColor8(8,22,34));draw.AddRect(at,at+new float2(width,height),friend?new ImColor8(63,214,255):HomeLine);
        if(friend)draw.AddRectFilled(at,at+new float2(3,height),new ImColor8(63,214,255));
        ImGui.SetCursorScreenPos(at+new float2(14,10));ImGui.BeginGroup();HomeLabel(LobbyEllipsis(listed.Data["name"],Math.Max(180,width-470)),21,HomeInk);
        ImGui.SameLine(0,12);HomeLabel(listed.Data.GetValueOrDefault("state")=="playing"?"IN GAME · "+listed.Data.GetValueOrDefault("round","1-1"):"WAITING",15,new ImColor8(117,204,174));
        string detail="Host "+listed.Data.GetValueOrDefault("hostName","Tamer")+" · "+listed.Members+" players · "+listed.Data.GetValueOrDefault("rounds","30")+" s rounds · "+SetChoices.Label(listed.Data["setId"])+" · "+SetSelection.PaceLabel(listed.Data["pace"]);
        HomeLabel(LobbyEllipsis(detail,Math.Max(180,width-340)),16,HomeMute);
        if(friendRooms.FirstOrDefault(room=>room.Lobby.Id==listed.Id) is {} friends)HomeLabel(LobbyEllipsis("Friends: "+string.Join(", ",friends.Friends.Select(person=>person.Name)),width-340),16,new ImColor8(63,214,255));
        ImGui.EndGroup();
        var pips=at+new float2(width-292,25);
        for(int i=0;i<8;i++){var center=pips+new float2(i*22,0);if(i<listed.Members){draw.AddCircleFilled(center,9,HomeGold);draw.AddCircle(center,9,HomeInk);}else draw.AddCircle(center,9,HomeLine);}
        ImGui.SetCursorScreenPos(at+new float2(width-292,45));HomeLabel($"{listed.Members}/{listed.Capacity} Tamers",16,HomeMute);
        ImGui.SetCursorScreenPos(at+new float2(width-110,24));ImGui.BeginDisabled(enteringRoom is not null);
        if(HomeButton("steamJoin/"+listed.Id,listed.Data.GetValueOrDefault("state")=="playing"?"View":"Join",96,40,true))JoinSteamRoom(listed.Id);
        ImGui.EndDisabled();ImGui.SetCursorScreenPos(at+new float2(0,height));ImGui.Dummy(new(width,6));
    }
    private void DrawLobbyRoom(NetworkRoom room,float height)
    {
        if(room.Authority.Lobby is not {} lobby)
        {
            LobbyHeading("Multiplayer","Joining lobby","Waiting for the host's lobby details…");LobbyStatus();
            ImGui.SetCursorScreenPos(LobbyFooterBounds.Min);
            if(HomeButton("steamLeave","Cancel join",160,44))LeaveSteamRoom();
            ImGui.SameLine();if(HomeButton("steamBack","Back",120,44))ReturnFromLobbies();
            return;
        }
        bool playing=room.Init is not null,host=room.Authority.IsHost,duo=lobby["format"]?.GetValue<string>()=="duos";
        var slots=lobby["slots"] as JsonObject;int seated=slots?.Count??0;
        var heading=ImGui.GetCursorScreenPos();float contentWidth=ImGui.GetContentRegionAvail().X;
        ImGui.PushFont(NativeUiTheme.DisplayBoldFont,30);string title=LobbyEllipsis((lobby["name"]?.GetValue<string>()??"Digi-Tactics lobby").ToUpperInvariant(),contentWidth-228);ImGui.PopFont();
        LobbyHeading(playing?"Game in progress":"Lobby",title,playing?"Take an AI seat that is still alive to join.":$"{seated}/8 players · {lobby["planTime"]?.GetValue<int>()??30} s rounds · empty seats are played by the AI.");
        var afterHeading=ImGui.GetCursorScreenPos();ImGui.SetCursorScreenPos(heading+new float2(contentWidth-200,8));InviteFriendsButton("steamInvite");ImGui.SetCursorScreenPos(afterHeading);
        if(host&&!playing)DrawLobbyRoomChoices(room);
        else HomeLabel(SetChoices.Label(lobby["setId"]?.GetValue<string>())+" · "+SetSelection.PaceLabel(lobby["pace"]?.GetValue<string>())+" · "+(duo?"Duos":"Solo"),18,HomeInk);
        HomeLabel(duo?"Seats with the same team number are partners. Move to an empty seat to choose your team.":"Free for all",16,HomeMute);
        var grid=ImGui.GetCursorScreenPos();float gridHeight=Math.Min(520,LobbyFooterBounds.Min.Y-grid.Y-42);
        float gap=8,cardWidth=(ImGui.GetContentRegionAvail().X-gap*3)/4,cardHeight=(gridHeight-gap)/2;
        for(int i=0;i<8;i++)DrawLobbySeat(room,i,grid+new float2((i%4)*(cardWidth+gap),(i/4)*(cardHeight+gap)),new(cardWidth,cardHeight));
        ImGui.SetCursorScreenPos(grid+new float2(0,gridHeight));ImGui.Dummy(new(0,4));LobbyStatus();
        var draw=ImGui.GetWindowDrawList();draw.AddLine(LobbyFooterBounds.Min-new float2(0,12),new(LobbyFooterBounds.Max.X,LobbyFooterBounds.Min.Y-12),HomeLine);
        ImGui.SetCursorScreenPos(LobbyFooterBounds.Min);
        if(ClientTextButton("steamLeave","Leave lobby",150,44,false,16))LeaveSteamRoom();
        ImGui.SameLine();if(HomeButton("players","Players",110,44))showPlayers=!showPlayers;
        ImGui.SameLine();if(ClientTextButton("steamBack","Back",90,44,false,16))ReturnFromLobbies();
        if(host&&!playing)
        {
            bool yours=slots?.Any(slot=>slot.Value?["uid"]?.GetValue<string>()==room.Authority.LocalId)==true;
            ImGui.SetCursorScreenPos(new(LobbyFooterBounds.Max.X-220,LobbyFooterBounds.Min.Y));ImGui.BeginDisabled(!yours);if(HomeButton("steamStart","START GAME",220,44,true))StartSteamMatch(room);ImGui.EndDisabled();
            if(!yours&&ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))ResultTooltip("Take a seat before starting the match.");
        }
        else if(!playing)
        {ImGui.SetCursorScreenPos(new(LobbyFooterBounds.Max.X-220,LobbyFooterBounds.Min.Y+12));HomeLabel("Waiting for the host",16,HomeMute);}
    }
    private static void DrawLobbyDashedBorder(float2 at,float2 extent,ImColor8 color)
    {
        var draw=ImGui.GetWindowDrawList();const float dash=5,gap=4;
        for(float x=0;x<extent.X;x+=dash+gap){draw.AddLine(at+new float2(x,0),at+new float2(Math.Min(x+dash,extent.X),0),color);draw.AddLine(at+new float2(x,extent.Y),at+new float2(Math.Min(x+dash,extent.X),extent.Y),color);}
        for(float y=0;y<extent.Y;y+=dash+gap){draw.AddLine(at+new float2(0,y),at+new float2(0,Math.Min(y+dash,extent.Y)),color);draw.AddLine(at+new float2(extent.X,y),at+new float2(extent.X,Math.Min(y+dash,extent.Y)),color);}
    }
    private void DrawLobbyRoomChoices(NetworkRoom room)
    {
        var lobby=room.Authority.Lobby!;float width=ImGui.GetContentRegionAvail().X;var at=ImGui.GetCursorScreenPos();
        if(size.Y>=840)
        {
            HomeLabel("SET",15);ImGui.SetNextItemWidth(240);DrawSetChoice("steamRoomSet",SetChoices.SavedSet(lobby["setId"]?.GetValue<string>()),id=>{if(room.UpdateLobby(new(){["setId"]=id}))SelectSet(id);});
            HomeLabel("PACE",15);ImGui.SetNextItemWidth(240);DrawPaceChoice("steamRoomPace",SetSelection.LobbyPace(lobby["pace"]?.GetValue<string>()),true,pace=>{if(room.UpdateLobby(new(){["pace"]=pace}))SelectPace(pace,true);});
            HomeLabel("FORMAT",15);ImGui.SetNextItemWidth(240);DrawFormatChoice("steamRoomFormat",lobby["format"]?.GetValue<string>()=="duos",format=>{if(room.UpdateLobby(new(){["format"]=format}))lobbyDuos=format=="duos";});
            return;
        }
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
        draw.AddRectFilledMultiColor(at,at+extent,new ImColor8(11,58,74,uid is null?(byte)70:(byte)150),new ImColor8(11,58,74,uid is null?(byte)70:(byte)150),new ImColor8(1,10,19,225),new ImColor8(1,10,19,225));
        if(uid is null)DrawLobbyDashedBorder(at,extent,HomeLine);
        else draw.AddRect(at,at+extent,yours?HomeInk:HomeLine,0,ImDrawFlags.None,yours?2:1);
        string number=lobby["format"]?.GetValue<string>()=="duos"?"T"+(index/2+1):(index+1).ToString();
        draw.AddText(at+new float2(extent.X-30,8),HomeMute,number);
        bool compact=extent.Y<178;
        float avatar=compact?56:Math.Clamp(extent.Y-116,56,104);var portraitAt=at+(compact?new float2(12,16):new float2((extent.X-avatar)/2,10));
        draw.AddCircleFilled(portraitAt+new float2(avatar*.5f),avatar*.5f,new ImColor8(25,43,65));
        string? key=uid is not null&&room.Authority.Presence[uid]?["look"] is JsonObject look?CosmeticLoadouts.CompanionKey(look):null;
        if(!portraits.DrawCompanion(key,portraitAt,avatar))
        {string initials=uid is null?"AI":new(name.Where(char.IsLetterOrDigit).Take(2).ToArray());var measured=ImGui.CalcTextSize(initials);draw.AddText(portraitAt+new float2((avatar-measured.X)/2,(avatar-measured.Y)/2),HomeGold,initials);}
        draw.AddCircle(portraitAt+new float2(avatar*.5f),avatar*.5f,yours?HomeGold:HomeLine,0,2);
        float nameLeft=compact?avatar+22:10,nameTop=compact?20:avatar+18,nameWidth=extent.X-nameLeft-10;
        ImGui.PushFont(NativeUiTheme.BodyFont,18);string displayedName=LobbyEllipsis(name,nameWidth);float nameTextWidth=ImGui.CalcTextSize(displayedName).X;ImGui.PopFont();
        ImGui.SetCursorScreenPos(at+new float2(nameLeft+(compact?0:(nameWidth-nameTextWidth)*.5f),nameTop));HomeLabel(displayedName,18,yours?new ImColor8(255,255,255):HomeInk);
        string tags=yours?(host?"YOU · HOST":"YOU"):host?"HOST":uid is null?(available?"AI TAMER":"ELIMINATED"):room.Authority.Presence.ContainsKey(uid)?"READY":"OFFLINE";
        ImGui.PushFont(NativeUiTheme.DisplayBoldFont,13);float tagWidth=Math.Min(nameWidth,ImGui.CalcTextSize(tags).X+14);ImGui.PopFont();
        var tagAt=at+new float2(nameLeft+(compact?0:(nameWidth-tagWidth)*.5f),nameTop+25);
        draw.AddRectFilled(tagAt,tagAt+new float2(tagWidth,20),yours?new ImColor8(8,66,81):new ImColor8(12,24,38));draw.AddRect(tagAt,tagAt+new float2(tagWidth,20),yours?new ImColor8(63,214,255):HomeLine);
        ImGui.PushFont(NativeUiTheme.DisplayBoldFont,13);draw.AddText(tagAt+new float2(7,3),yours?new ImColor8(164,236,244):HomeMute,LobbyEllipsis(tags,tagWidth-14));ImGui.PopFont();
        LobbyLayoutFits &= (compact?76:avatar+66)<=extent.Y-44 && at.X>=LobbyContentBounds.Min.X && at.X+extent.X<=LobbyContentBounds.Max.X+1 && at.Y>=LobbyContentBounds.Min.Y && at.Y+extent.Y<=LobbyFooterBounds.Min.Y-40;
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
