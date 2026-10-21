using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    // The original client reserves one 80-pixel navigation row on every page.
    private const float ClientPageTop = 80;
    private bool ClientNavigationBlocked=>training is not null||EliminationCinematicActive||evolutionLegend is not null||CollectionModalVisible;
    private readonly List<string> clientHistory=[];
    private bool clientNavigationDispatch;
    internal string ClientPrimaryLabel=>steam?.Room is null?"PLAY":"LOBBY";
    internal string ClientPresenceLabel=>steam?.Room is null?"Online":"In lobby";
    internal string ClientRoomLabel=>steam?.Room is {} room?$"Lobby {(room.Authority.Lobby?["slots"] as System.Text.Json.Nodes.JsonObject)?.Count??0}/8":"SOLO";
    private string CurrentClientRoute=>showProfile?"profile":showSettings?"settings":showSaveData?"savedata":showAbout?"about":showMultiplayer?"multiplayer":showHomePlay?"play":showFarm?"farm":codexFull?"codex":showCollection?(showEggs?"loot":collectionStore?"store":"collection"):"home";

    private void NavigateClient(string destination,bool remember=true)
    {
        if (ClientNavigationBlocked) return;
        if(remember)RememberClientRoute(destination);
        postgameStore = false;
        if (evolutionLegend is not null) CloseEvolution();
        showProfile = showFarm = showCollection = showAbout = showSettings = showSaveData = showHomePlay = showMultiplayer = false;
        codexFull = codexSide = false; codexUnit = codexModalLine = null;
        showMusicLibrary = showAudio = showPlayers = showEmotes = showSoundboard = false;
        confirmResetSave = false; resetSaveText.SetValue("");
        CancelArenaDrag(); cameraDrag = null;
        clientNavigationDispatch=true;
        try
        {
        switch (destination)
        {
            case "play": homePlayParty = false; showHomePlay = true; break;
            case "collection": OpenCollectionPage(); break;
            case "store": OpenCollectionPage(store: true); break;
            case "loot": OpenCollectionPage(loot: true); break;
            case "farm": if (farm is not null) { farm.Refresh(HomeClock().ToUnixTimeMilliseconds()); showFarm = true; } break;
            case "codex": OpenCodexFull(); break;
            case "profile": OpenProfile(); break;
            case "settings": showSettings = true; break;
            case "savedata": OpenSaveData(); break;
            case "multiplayer": OpenMultiplayerView(); break;
            case "about": showAbout = true; break;
        }
        }
        finally {clientNavigationDispatch=false;}
    }

    private void RememberClientRoute(string destination)
    {
        if(clientNavigationDispatch)return;
        string origin=CurrentClientRoute;
        if(destination==origin)return;
        clientHistory.Add(origin);if(clientHistory.Count>20)clientHistory.RemoveAt(0);
    }

    private void BackClient()
    {
        if(ClientNavigationBlocked)return;
        if(showHomePlay&&homePlayParty){homePlayParty=false;return;}
        string current=CurrentClientRoute;
        while(clientHistory.Count>0)
        {
            string previous=clientHistory[^1];clientHistory.RemoveAt(clientHistory.Count-1);
            if(previous==current)continue;
            NavigateClient(previous,false);return;
        }
        NavigateClient("home",false);
    }

    private bool ClientTextButton(string key,string label,float width,float height,bool selected=false,float font=15)
    {
        ImGui.PushFont(NativeUiTheme.DisplayBoldFont,font);
        ImGui.PushStyleColor(ImGuiCol.Button,new ImColor8(0,0,0,0));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered,new ImColor8(200,170,110,18));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive,new ImColor8(200,170,110,30));
        ImGui.PushStyleColor(ImGuiCol.Text,selected?HomeInk:HomeMute);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,0);ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding,0);
        bool clicked=Button(key,label,width,height);
        if(selected)
        {
            var lo=ImGui.GetItemRectMin();var hi=ImGui.GetItemRectMax();var draw=ImGui.GetWindowDrawList();
            draw.AddRectFilledMultiColor(new(lo.X,hi.Y-28),hi,new ImColor8(200,170,110,0),new ImColor8(200,170,110,0),new ImColor8(200,170,110,25),new ImColor8(200,170,110,25));
            draw.AddRectFilled(new(lo.X+10,hi.Y-2),new(hi.X-10,hi.Y),HomeInk);
        }
        ImGui.PopStyleVar(2);ImGui.PopStyleColor(4);ImGui.PopFont();return clicked;
    }

    private void DrawClientNavigation()
    {
        if (!ClientHomeVisible) return;
        DrawClientBackdrop();
        float width=socialWindowSize.X,profileWidth=SocialDocked?SocialWidth:178,profileLeft=width-profileWidth;
        bool compact=width<1280;
        HomeCard("Client navigation",new(0,0),new(width,ClientPageTop),false,true);
        var draw=ImGui.GetWindowDrawList();
        draw.AddRectFilledMultiColor(new(0,0),new(width,ClientPageTop),new ImColor8(2,12,21),new ImColor8(2,12,21),new ImColor8(8,17,31),new ImColor8(8,17,31));
        draw.AddLine(new(0,ClientPageTop-1),new(width,ClientPageTop-1),HomeLine);
        ImGui.BeginDisabled(ClientNavigationBlocked);
        ImGui.SetCursorPos(new(14,14));
        if(HomeButton("clientPlay",ClientPrimaryLabel,compact?138:162,52,true))NavigateClient(steam?.Room is null?"play":"multiplayer");
        float x=compact?164:190;
        foreach(var tab in new[]{("homeOverview","HOME","home",62f),("clientModes","MODES","play",72f),
            ("collection","COLLECTION","collection",112f),("clientLoot","LOOT","loot",58f),("clientStore","STORE","store",64f),
            ("farm","DIGI-FARM","farm",102f),("codex","CODEX","codex",72f)})
        {
            bool selected=tab.Item3 switch
            {
                "play"=>showHomePlay,"collection"=>showCollection&&!collectionStore&&!showEggs,
                "store"=>showCollection&&collectionStore&&!showEggs,"loot"=>showCollection&&showEggs,
                "farm"=>showFarm,"codex"=>codexFull,
                _=>!showHomePlay&&!showCollection&&!showFarm&&!codexFull&&!showMultiplayer&&!showProfile&&!showSettings&&!showSaveData&&!showAbout
            };
            float font=compact?16:17;ImGui.PushFont(NativeUiTheme.DisplayBoldFont,font);
            float tabWidth=Math.Max(tab.Item4*(compact?.88f:1),ImGui.CalcTextSize(tab.Item2).X+16);ImGui.PopFont();
            ImGui.SetCursorPos(new(x,0));
            ImGui.BeginDisabled(profile is null&&tab.Item3 is "collection" or "loot" or "store" or "farm");
            if(ClientTextButton(tab.Item1,tab.Item2,tabWidth,ClientPageTop,selected,font))NavigateClient(tab.Item3);
            ImGui.EndDisabled();x+=tabWidth;
        }
        string balance=$"{profile?.Progress.Bits??0:N0}";
        ImGui.PushFont(NativeUiTheme.BodyFont,16);float balanceWidth=Math.Max(54,ImGui.CalcTextSize(balance).X+30);ImGui.PopFont();
        float utilitiesLeft=profileLeft-balanceWidth-98;
        // The small mode/status control is also the persistent multiplayer entry.
        float statusWidth=steam?.Room is null?62:84;
        if(utilitiesLeft-x>=statusWidth+4)
        {
            ImGui.SetCursorPos(new(utilitiesLeft-statusWidth-4,26));
            if(ClientTextButton("clientMultiplayer",ClientRoomLabel,statusWidth,28,showMultiplayer,12))NavigateClient("multiplayer");
        }
        ImGui.SetCursorPos(new(utilitiesLeft,24));
        if(ClientTextButton("clientBalance",balance,balanceWidth,32,false,16))NavigateClient("store");
        var balanceLo=ImGui.GetItemRectMin();draw.AddRect(balanceLo,ImGui.GetItemRectMax(),HomeLine);
        var diamond=balanceLo+new float2(12,16);draw.AddTriangleFilled(diamond+new float2(0,-7),diamond+new float2(7,0),diamond+new float2(0,7),new ImColor8(63,214,255));draw.AddTriangleFilled(diamond+new float2(0,-7),diamond+new float2(0,7),diamond+new float2(-7,0),new ImColor8(63,214,255));
        ImGui.SetCursorPos(new(profileLeft-90,23));
        if(ClientTextButton("settings","",42,34,showSettings,12))NavigateClient("settings");
        DrawClientGlyph(false);
        if(ImGui.IsItemHovered())ResultTooltip("Settings");
        ImGui.SetCursorPos(new(profileLeft-46,23));
        if(SocialEnabled&&!SocialDocked)
        {if(ClientTextButton("socialToggle","",42,34,socialDrawer,12))socialDrawer=!socialDrawer;DrawClientGlyph(true);if(ImGui.IsItemHovered())ResultTooltip("Social / friends");}
        else
        {if(ClientTextButton("socialCollapse",SocialEnabled?"-":"+",42,34,false,20))SaveClientPreferences(clientPreferences with {Social=!SocialEnabled});if(ImGui.IsItemHovered())ResultTooltip(SocialEnabled?"Collapse social panel":"Show social panel");}
        DrawClientIdentity(new(profileLeft,0),new(profileWidth,ClientPageTop));
        CollectionNavigationAnchors();
        ImGui.EndDisabled();ImGui.End();
    }

    private static void DrawClientGlyph(bool people)
    {
        var draw=ImGui.GetWindowDrawList();var center=(ImGui.GetItemRectMin()+ImGui.GetItemRectMax())*.5f;
        if(people)
        {
            draw.AddCircleFilled(center+new float2(-4,-5),3,HomeGold);draw.AddCircleFilled(center+new float2(5,-3),3,HomeGold);
            draw.AddRectFilled(center+new float2(-9,0),center+new float2(1,7),HomeGold,3);draw.AddRectFilled(center+new float2(2,2),center+new float2(10,7),HomeGold,3);
        }
        else
        {
            draw.AddCircle(center,6,HomeGold,24,2);draw.AddCircle(center,2,HomeGold,16,1);
            for(int i=0;i<8;i++){float a=i*MathF.PI/4;var direction=new float2(MathF.Cos(a),MathF.Sin(a));draw.AddLine(center+direction*6,center+direction*9,HomeGold,3);}
        }
    }

    private void DrawClientBackdrop()
    {
        // The farm scene is drawn directly before ImGui; its transparent room
        // window must reveal that render rather than an opaque client backdrop.
        if(showFarm)return;
        var draw=ImGui.GetBackgroundDrawList();float width=socialWindowSize.X,height=socialWindowSize.Y;
        draw.AddRectFilledMultiColor(new(0,ClientPageTop),new(width,height),new ImColor8(5,18,28),new ImColor8(4,23,33),new ImColor8(1,10,19),new ImColor8(1,10,19));
        const float radius=24,rowHeight=41.57f;
        for(int col=0;col*36<width+radius;col++)
        for(int row=0;row*rowHeight<height;row++)
        {
            var center=new float2(col*36,ClientPageTop+row*rowHeight+(col%2)*rowHeight/2);
            for(int edge=0;edge<6;edge++)
            {
                float a=edge*MathF.PI/3,b=(edge+1)*MathF.PI/3;
                draw.AddLine(center+new float2(MathF.Cos(a),MathF.Sin(a))*radius,center+new float2(MathF.Cos(b),MathF.Sin(b))*radius,new ImColor8(74,126,143,16));
            }
        }
    }

    private void DrawClientIdentity(float2 at,float2 extent)
    {
        ImGui.SetCursorScreenPos(at);ImGui.BeginDisabled(profile is null);
        if(ClientTextButton("profile","",extent.X-30,extent.Y,showProfile))NavigateClient("profile");
        ImGui.EndDisabled();var draw=ImGui.GetWindowDrawList();draw.AddLine(at,at+new float2(0,extent.Y),HomeLine);
        var center=at+new float2(40,38);draw.AddCircleFilled(center,25,new ImColor8(10,20,40));draw.AddCircle(center,26,HomeGold,48,2);
        if(profile is not null)
        {
            var wallet=new CosmeticCollection(profile.Storage.GetValueOrDefault("dt.cosmetics.v1"));
            string? selected=ProfileSummary.Read(profile.Storage.GetValueOrDefault("dt.client.v1"),profile.Storage.GetValueOrDefault("dt.cosmetics.v1")).Avatar;
            string id=selected is not null&&wallet.Owns("legend",selected)?selected:wallet.Equipped("legend")?.GetValue<string>()??"tokomon";
            var discovered=LegendEvolution.Discovery(profile.Storage.GetValueOrDefault("dt.legendevo.v1"),id);
            portraits.DrawEvolution(discovered.Current,center-new float2(22,22),44,true);
            var level=TamerProgress.Read(profile.Storage.GetValueOrDefault("dt.client.v1"));
            draw.AddRectFilled(center+new float2(-11,16),center+new float2(11,33),new ImColor8(1,10,19));draw.AddRect(center+new float2(-11,16),center+new float2(11,33),HomeGold);
            ImGui.PushFont(NativeUiTheme.DisplayBoldFont,12);var n=level.Level.ToString();draw.AddText(center+new float2(-ImGui.CalcTextSize(n).X/2,16),HomeInk,n);ImGui.PopFont();
        }
        ImGui.SetCursorScreenPos(at+new float2(78,24));HomeLabel(LobbyEllipsis(SteamName,extent.X-86),16,HomeInk);
        var presenceColor=steam?.Room is null?new ImColor8(79,224,143):new ImColor8(100,195,255);
        draw.AddCircleFilled(at+new float2(82,53),3,presenceColor);
        ImGui.SetCursorScreenPos(at+new float2(91,45));HomeLabel(ClientPresenceLabel,12,presenceColor);
        // Keep About reachable at compact widths without adding a second nav row.
        ImGui.SetCursorScreenPos(at+new float2(extent.X-25,3));if(ClientTextButton("about","i",22,20,showAbout,13))NavigateClient("about");
    }
}
