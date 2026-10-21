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
    internal bool MultiplayerVisible=>showMultiplayer;
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
        RefreshSetSelection();LobbySeatCards=0;
        // .play-mp inside .play: the same framed lobby painting as the solo party (theme.js).
        float viewWidth=socialWindowSize.X,viewHeight=socialWindowSize.Y;bool shortView=viewHeight<=820;
        float pageWidth=Math.Min(size.X,1720),pad=viewWidth<=1500?16:24,padTop=shortView?8:12,padBottom=shortView?10:16;
        var at=new float2((size.X-pageWidth)/2+pad,ClientPageTop+padTop);float width=pageWidth-2*pad,height=size.Y-ClientPageTop-padTop-padBottom;
        float padX=shortView?16:24,padY=shortView?12:16;
        LobbyLayoutFits=width>=700&&height>=440;LobbyArtworkVisible=false;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(1+padX,1+padY));
        HomeCard("Steam multiplayer",at,new(width,height),false);
        var draw=ImGui.GetWindowDrawList();var max=at+new float2(width,height);
        draw.PushClipRect(at+new float2(1),max-new float2(1),true);
        homeArtwork.Draw("lobby-bg",at+new float2(1),new(width-2,height-2),255,true,.4f);LobbyArtworkVisible=true;
        float fadeEnd=at.Y+1+(height-2)*.7f;
        ClientCss.Gradient(draw,at+new float2(1),new(max.X-1,fadeEnd),new ImColor8(1,10,19,140),new ImColor8(1,10,19,235));
        draw.AddRectFilled(new(at.X+1,fadeEnd),max-new float2(1),new ImColor8(1,10,19,235));
        draw.PopClipRect();ClientCss.Border(draw,at,max,ClientCss.Gold4);
        LobbyContentBounds=(at+new float2(1+padX,1+padY),max-new float2(1+padX,1+padY));
        // .mpfoot sits at the bottom of the panel: 16px top padding, then the 34.8px ghost button.
        LobbyFooterBounds=(new(LobbyContentBounds.Min.X,LobbyContentBounds.Max.Y-34.8f),LobbyContentBounds.Max);
        ImGui.PushFont(NativeUiTheme.BodyFont,ClientCss.Size(ClientCss.Face.Body,14));LobbyBodyFontSize=ImGui.GetFontSize();
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,new float2(12,10));ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(10,7));
        if(steam is null)
        {
            float top=LobbyHeading(LobbyContentBounds.Min,"Multiplayer · Steam","Lobbies","Every lobby has 8 Tamers. Seats you and your friends don't take are played by the AI.")+12;
            ClientCss.Text(draw,ClientCss.Face.Body,14,LobbyContentBounds.Min+new float2(0,top),ClientCss.Text2,"Connect to Steam to create a lobby, find your friends and join a game.",0,18.9f);
            if(PrimaryButton("steamConnect","Connect to Steam",LobbyContentBounds.Min+new float2(0,top+30)))ConnectSteam();
            LobbyStatus(LobbyContentBounds.Min+new float2(0,top+76),LobbyContentBounds.Max.X-LobbyContentBounds.Min.X);
            if(GhostButton("steamBack","Back",LobbyFooterBounds.Min))ReturnFromLobbies();
        }
        else if(steam.Room is {} room)DrawLobbyRoom(room,height);
        else DrawLobbyBrowser(height);
        ImGui.PopStyleVar(2);ImGui.PopFont();ImGui.End();ImGui.PopStyleVar();
    }
    /// <summary>.mp-head: eyebrow, page title and sub line; returns its height.</summary>
    private float LobbyHeading(float2 at,string eyebrow,string title,string? description)
    {
        var draw=ImGui.GetWindowDrawList();
        ClientCss.Text(draw,ClientCss.Face.DisplayBold,11,at,ClientCss.Gold2,eyebrow.ToUpperInvariant(),1.76f,14.85f);
        ClientCss.Text(draw,ClientCss.Face.DisplayBold,26,at+new float2(0,16.8f),ClientCss.Gold1,title.ToUpperInvariant(),1.04f,28.6f);
        if(description is null)return 45.4f;
        ClientCss.Text(draw,ClientCss.Face.Body,12.5f,at+new float2(0,49.4f),ClientCss.Text2,description,0,16.875f);
        return 66.3f;
    }
    private void LobbyStatus(float2 at,float width)
    {
        string message=recentPlayersError.Length>0?recentPlayersError:lobbyMessage;
        // The original keeps the connected browser quiet; only problems are reported.
        if(message.Length==0||message.StartsWith("Connected as ",StringComparison.Ordinal))return;
        ClientCss.Paragraph(ImGui.GetWindowDrawList(),ClientCss.Face.Body,12.5f,at,width,16.875f,ClientCss.Text2,message);
    }
    // .btn.btn-primary: orange gradient with a light frame.
    private bool PrimaryButton(string key,string label,float2 at)
    {
        float width=ClientCss.Width(ClientCss.Face.BodyBold,14,label)+34,height=34.8f;
        bool pressed=CssHit(key,at,new(width,height),out bool hover);var draw=ImGui.GetWindowDrawList();var max=at+new float2(width,height);
        float alpha=ImGui.GetStyle().Alpha;
        if(hover)ClientCss.Glow(draw,at,max,new ImColor8(255,138,61,89),24,2);else ClientCss.Glow(draw,at+new float2(0,2),max+new float2(0,2),new ImColor8(255,138,61,64),10,2);
        float split=at.Y+1+(height-2)*.3f;
        var top=new ImColor8(255,210,122);var middle=new ImColor8(255,194,74);var bottom=new ImColor8(255,138,61);
        if(hover){top=new ImColor8(255,227,132);middle=new ImColor8(255,209,80);bottom=new ImColor8(255,149,66);}
        ClientCss.Gradient(draw,at+new float2(1),new(max.X-1,split),ClientCss.Alpha(top,alpha),ClientCss.Alpha(middle,alpha));
        ClientCss.Gradient(draw,new(at.X+1,split),max-new float2(1),ClientCss.Alpha(middle,alpha),ClientCss.Alpha(bottom,alpha));
        draw.AddLine(at+new float2(1,1.5f),new(max.X-1,at.Y+1.5f),ClientCss.Alpha(new ImColor8(255,255,255,89),alpha));
        ClientCss.Border(draw,at,max,ClientCss.Alpha(new ImColor8(240,210,140),alpha),1,2);
        ClientCss.Text(draw,ClientCss.Face.BodyBold,14,at+new float2(17,9),ClientCss.Alpha(new ImColor8(20,13,5),alpha),label,0,16.8f);
        return pressed;
    }
    private void ReturnFromLobbies()
    {
        // Browsing the client keeps the party and any retained match result.
        // The separate Leave lobby / Cancel join controls own disconnection.
        BackClient();
    }
    // .field: uppercase label above a navy input or select.
    private static void LobbyFieldLabel(float2 at,string label)
        =>ClientCss.Text(ImGui.GetWindowDrawList(),ClientCss.Face.BodyBold,11,at,ClientCss.LcMute,label.ToUpperInvariant(),1.1f,14.85f);
    private static void LobbyFieldFrame(float2 at,float width,bool focused)
    {
        var draw=ImGui.GetWindowDrawList();var max=at+new float2(width,36.9f);
        if(focused)draw.AddRect(at-new float2(2),max+new float2(2),new ImColor8(200,170,110,46),0,ImDrawFlags.None,2);
        draw.AddRectFilled(at,max,new ImColor8(1,10,19,217));ClientCss.Border(draw,at,max,focused?ClientCss.Gold2:ClientCss.Gold5);
    }
    private static void LobbyChevron(float2 at,float width)
    {
        var draw=ImGui.GetWindowDrawList();var center=at+new float2(width-11,18.5f);
        draw.AddLine(center+new float2(-4,-2),center+new float2(0,2),ClientCss.Text1,1.5f);draw.AddLine(center+new float2(0,2),center+new float2(4,-2),ClientCss.Text1,1.5f);
    }
    private bool LobbyCombo(string key,string preview,float2 at,float width)
    {
        LobbyFieldFrame(at,width,false);
        ImGui.PushStyleColor(ImGuiCol.FrameBg,new ImColor8(0,0,0,0));ImGui.PushStyleColor(ImGuiCol.FrameBgHovered,new ImColor8(0,0,0,0));ImGui.PushStyleColor(ImGuiCol.FrameBgActive,new ImColor8(0,0,0,0));
        ImGui.PushStyleColor(ImGuiCol.Border,new ImColor8(0,0,0,0));ImGui.PushStyleColor(ImGuiCol.Text,ClientCss.Text1);
        ImGui.SetCursorScreenPos(at);ImGui.SetNextItemWidth(width);
        bool open=ImGui.BeginCombo("##"+key,preview,ImGuiComboFlags.NoArrowButton|ImGuiComboFlags.HeightLargest);
        ImGui.PopStyleColor(5);SelectionButton(key);LobbyChevron(at,width);
        return open;
    }
    private float steamListedAt=float.NegativeInfinity;
    private bool lobbyNameFocused;
    private void DrawLobbyBrowser(float height)
    {
        var content=LobbyContentBounds.Min;float contentWidth=LobbyContentBounds.Max.X-content.X;var draw=ImGui.GetWindowDrawList();
        float headHeight=LobbyHeading(content,"Multiplayer · Steam","Lobbies","Every lobby has 8 Tamers. Seats you and your friends don't take are played by the AI.");
        if(lobbyRoomName.ToString().Length==0)lobbyRoomName.SetValue((SteamName+"'s lobby").AsSpan());
        // Keep polling the room list while the browser is open, as the original does every 4 s.
        float clock=Environment.TickCount64/1000f;
        if(listingRooms is not null||listingFriendRooms is not null)steamListedAt=clock;
        else if(clock-steamListedAt>=4)RefreshSteamRooms();
        ImGui.BeginDisabled(enteringRoom is not null);
        string visibilityLabel=visibility switch{SteamLobbyVisibility.Public=>"Public",SteamLobbyVisibility.Friends=>"Friends only",_=>"Private"};
        // .mp-create: a wrapping flex row (gap 12, items aligned to the bottom); Lobby name grows from 220px.
        var items=new (string Key,string Label,float Width,bool Grow)[]
        {
            ("steamName","Your name",170,false),("steamRoomName","Lobby name",220,true),("steamSet","Set",280,false),("steamPace","Pace",123,false),
            ("steamFormat","Format",116,false),("steamPlanning","Rounds",123,false),("steamVisibility","Who can join",125,false),
            ("steamCreate","",ClientCss.Width(ClientCss.Face.BodyBold,14,enteringRoom is null?"Create lobby":"Creating…")+34,false),
        };
        float boxTop=headHeight+12+1,innerWidth=contentWidth-2-32;
        var rows=new List<List<int>>{new()};float lineWidth=0;
        for(int i=0;i<items.Length;i++)
        {
            float add=(rows[^1].Count>0?12:0)+items[i].Width;
            if(rows[^1].Count>0&&lineWidth+add>innerWidth+.5f){rows.Add([]);lineWidth=0;add=items[i].Width;}
            rows[^1].Add(i);lineWidth+=add;
        }
        var widths=items.Select(item=>item.Width).ToArray();
        foreach(var row in rows)
        {
            float used=row.Sum(i=>items[i].Width)+12*(row.Count-1);int grow=row.FirstOrDefault(i=>items[i].Grow,-1);
            if(grow>=0)widths[grow]+=innerWidth-used;
        }
        // A wrapped line is as tall as its tallest item: a field (56.8) or the lone Create button (34.8).
        float RowHeight(List<int> row)=>row.Any(i=>items[i].Key!="steamCreate")?56.8f:34.8f;
        float boxHeight=2+24+rows.Sum(RowHeight)+(rows.Count-1)*12;
        var boxAt=content+new float2(0,boxTop-1);var boxMax=boxAt+new float2(contentWidth,boxHeight);
        draw.AddRectFilled(boxAt+new float2(0,6),boxMax+new float2(0,6),new ImColor8(0,0,0,38));
        draw.AddRectFilled(boxAt,boxMax,new ImColor8(1,10,19,209));ClientCss.Border(draw,boxAt,boxMax,ClientCss.Gold5);
        // .box::before: a gold-3 highlight fading out toward both ends of the top edge.
        float mid=(boxAt.X+boxMax.X)/2;
        ClientCss.GradientX(draw,boxAt-new float2(1,0),new(mid,boxAt.Y+1),new ImColor8(200,155,60,0),ClientCss.Gold3);ClientCss.GradientX(draw,new(mid,boxAt.Y),new(boxMax.X+1,boxAt.Y+1),ClientCss.Gold3,new ImColor8(200,155,60,0));
        for(int r=0;r<rows.Count;r++)
        {
            float rowHeight=RowHeight(rows[r]),x=boxAt.X+1+16,y=boxAt.Y+1+12+rows.Take(r).Sum(row=>RowHeight(row)+12);
            foreach(int i in rows[r])
            {
                var item=items[i];float w=widths[i];var inputAt=new float2(x,y+19.9f);
                if(item.Key=="steamCreate")
                {if(PrimaryButton("steamCreate",enteringRoom is null?"Create lobby":"Creating…",new(x,y+rowHeight-34.8f)))CreateSteamRoom();}
                else
                {
                    LobbyFieldLabel(new(x,y),item.Label);
                    switch(item.Key)
                    {
                        case "steamName":
                            LobbyFieldFrame(inputAt,w,false);ClientCss.Text(draw,ClientCss.Face.Body,14,inputAt+new float2(12,0),ClientCss.Text1,ClientCss.Ellipsis(ClientCss.Face.Body,14,SteamName,w-24),0,36.9f);
                            break;
                        case "steamRoomName":
                            ImGui.SetCursorScreenPos(inputAt);LobbyFieldFrame(inputAt,w,lobbyNameFocused);
                            ImGui.PushStyleColor(ImGuiCol.FrameBg,new ImColor8(0,0,0,0));ImGui.PushStyleColor(ImGuiCol.Border,new ImColor8(0,0,0,0));ImGui.PushStyleColor(ImGuiCol.Text,ClientCss.Text1);
                            ImGui.SetNextItemWidth(w);ImGui.InputText("##steamRoomName"u8,lobbyRoomName);lobbyNameFocused=ImGui.IsItemActive();SelectionButton("steamRoomName");ImGui.PopStyleColor(3);
                            break;
                        case "steamSet":
                            if(LobbyCombo("steamSet",SetChoices.Label(selectedSet),inputAt,w))
                            {
                                foreach(string id in SetChoices.Options.Select(s=>s.Id).Concat(SetChoices.HasChaos?["chaos"]:Array.Empty<string>()))
                                {bool picked=ImGui.Selectable(SetChoices.Label(id),selectedSet==id);SelectionButton("steamSet/"+id);if(picked)SelectSet(id);ClientItemSound("steamSet/"+id,picked,"levelup",false);}
                                ImGui.EndCombo();
                            }
                            break;
                        case "steamPace":
                            if(LobbyCombo("steamPace",SetSelection.PaceLabel(lobbyPaceChoice),inputAt,w))
                            {
                                foreach(string pace in new[]{"hyper","normal","chaos"})
                                {bool picked=ImGui.Selectable(SetSelection.PaceLabel(pace),lobbyPaceChoice==pace);SelectionButton("steamPace/"+pace);if(picked)SelectPace(pace,true);ClientItemSound("steamPace/"+pace,picked);}
                                ImGui.EndCombo();
                            }
                            break;
                        case "steamFormat":
                            if(LobbyCombo("steamFormat",lobbyDuos?"Duos · 2v2":"Free for all",inputAt,w))
                            {
                                foreach(string format in new[]{"solo","duos"})
                                {bool picked=ImGui.Selectable(format=="duos"?"Duos · 2v2":"Free for all",lobbyDuos==(format=="duos"));SelectionButton("steamFormat/"+format);if(picked)lobbyDuos=format=="duos";ClientItemSound("steamFormat/"+format,picked);}
                                ImGui.EndCombo();
                            }
                            break;
                        case "steamPlanning":
                            if(LobbyCombo("steamPlanning",lobbyPlanningSeconds+" s rounds",inputAt,w))
                            {
                                foreach(int seconds in new[]{30,45,60}){if(ImGui.Selectable(seconds+" s rounds",lobbyPlanningSeconds==seconds))lobbyPlanningSeconds=seconds;SelectionButton("planningSeconds/"+seconds);}
                                ImGui.EndCombo();
                            }
                            break;
                        case "steamVisibility":
                            if(LobbyCombo("steamVisibility",visibilityLabel,inputAt,w))
                            {
                                foreach(var option in new[]{SteamLobbyVisibility.Public,SteamLobbyVisibility.Friends})if(ImGui.Selectable(option==SteamLobbyVisibility.Public?"Public":"Friends only",visibility==option))visibility=option;
                                ImGui.EndCombo();
                            }
                            break;
                    }
                }
                x+=w+12;
            }
        }
        ImGui.EndDisabled();
        // h3.mph and the list.
        var displayed=friendRooms.Select(r=>r.Lobby).Concat(availableRooms).DistinctBy(r=>r.Id).Where(r=>r.Data.GetValueOrDefault("state")!="over").ToArray();
        float headingTop=boxTop-1+boxHeight+16;
        float headingWidth=ClientCss.Text(draw,ClientCss.Face.DisplayBold,14,content+new float2(0,headingTop),ClientCss.Gold1,"OPEN LOBBIES",1.68f,18.9f);
        if(displayed.Length>0)ClientCss.Text(draw,ClientCss.Face.Body,13,content+new float2(headingWidth+8,headingTop+1),ClientCss.Muted,displayed.Length.ToString(),0,17.55f);
        float listTop=headingTop+18.9f+8;var listAt=content+new float2(0,listTop);
        float listHeight=Math.Max(64,LobbyFooterBounds.Min.Y-16-listAt.Y);
        ImGui.SetCursorScreenPos(listAt);ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(0,0));
        ImGui.BeginChild("Open Steam lobbies"u8,new(contentWidth,listHeight),ImGuiChildFlags.None,ImGuiWindowFlags.NoBackground);ImGui.PopStyleVar();
        foreach(var listed in displayed)DrawListedLobby(listed);
        if(displayed.Length==0)
        {
            var list=ImGui.GetWindowDrawList();var empty=ImGui.GetCursorScreenPos();var emptyMax=empty+new float2(contentWidth,68.9f);
            list.AddRectFilled(empty,emptyMax,new ImColor8(1,10,19,153));ClientCss.Dashed(list,empty,emptyMax-empty,ClientCss.Gold5);
            const string emptyMessage="No open lobbies right now. Create one and invite your Steam friends.";
            ClientCss.TextCentered(list,ClientCss.Face.Body,14,empty+new float2(0,25),contentWidth,ClientCss.Text3,emptyMessage,0,18.9f);
            ImGui.Dummy(new(contentWidth,68.9f));
        }
        ImGui.EndChild();
        LobbyStatus(listAt+new float2(0,Math.Min(listHeight,displayed.Length==0?68.9f:listHeight)+8),contentWidth);
        if(GhostButton("steamBack","Back",LobbyFooterBounds.Min))ReturnFromLobbies();
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
