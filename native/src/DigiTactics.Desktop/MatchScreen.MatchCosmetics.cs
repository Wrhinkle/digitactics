using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool matchCosmeticsActive;
    private bool matchCosmeticsFocus;
    internal bool MatchCosmeticsVisible=>matchCosmeticsActive&&Session is not null&&!ClientHomeVisible&&(showCollection||showFarm);
    internal bool MatchStoreEntryAvailable=>Session is not null&&(Session.State.Over||IsSpectating||!Human.Alive||Session.Rounds.Phase==MatchPhase.Planning);
    private bool MatchLockerVisible=>MatchCosmeticsVisible&&showCollection&&!collectionStore&&!showEggs;
    internal (float2 Min,float2 Max) MatchCosmeticsBounds {get;private set;}
    internal DailyMissionProgress[] CollectionMissions {get;private set;}=[];
    internal int CollectionBitsHistoryRows {get;private set;}

    private void MatchCosmeticsControl()
    {
        if(profile is null)return;
        if(MatchStoreEntryAvailable)
        {
            if(Button("matchStore","Store",62,34))OpenMatchCosmetics(true);
            ImGui.SameLine();
        }
        if(Button("matchLocker","Locker",62,34))OpenMatchCosmetics(false);
        ImGui.SameLine();
        if(Button("matchLegend","Little Legend",100,34))OpenMatchCosmetics(false,"legend");
    }
    private void OpenMatchCosmetics(bool store,string? kind=null)
    {
        if(profile is null||Session is null)return;
        CancelArenaDrag();cameraDrag=null;
        showAudio=showPlayers=showEmotes=showSoundboard=showMusicLibrary=false;
        showFarm=false;matchCosmeticsActive=true;matchCosmeticsFocus=true;
        OpenCollectionPage(store:store,kind:kind);
        if(!store)collectionOwnership="owned";
    }
    private bool DrawMatchCosmetics()
    {
        if(!MatchCosmeticsVisible){if(!showCollection&&!showFarm)matchCosmeticsActive=false;return false;}
        if(showFarm)
        {
            HomeCard("Match cosmetic navigation",new(16,16),new(size.X-32,72),false,true);
            ImGui.BeginDisabled(training is not null);
            if(Button("closeMatchCosmetics","Return to match",174,38))showFarm=false;
            ImGui.SameLine();if(Button("matchCollectionBack","Back to cosmetics",190,38)){showFarm=false;showCollection=true;matchCosmeticsFocus=true;}
            ImGui.EndDisabled();ImGui.SameLine();ImGui.Text("The match continues while this panel is open."u8);ImGui.End();
            if(showFarm)FarmPanel();
        }
        else Collection();
        return true;
    }
    private bool MatchCollectionHeader(CosmeticCollection collection)
    {
        var origin=ImGui.GetWindowPos();float width=ImGui.GetWindowSize().X;var draw=ImGui.GetWindowDrawList();
        var daily=MatchRewards.LiveProgress(profile!.Storage.GetValueOrDefault("dt.shop.v1"),history?.LiveStats,DateOnly.FromDateTime(DateTime.Now));CollectionMissions=daily.Missions;CollectionBitsHistoryRows=0;
        bool loot=collectionStore||showEggs;string bits=collection.Bits.ToString("N0");
        ImGui.PushFont(NativeUiTheme.BodyFont,15);
        string[] labels=daily.Missions.Select(mission=>mission.Name+"  "+mission.Bits+" Bits"+(mission.Done?"  DONE":mission.Goal>1?$"  {mission.Current}/{mission.Goal}":"")).ToArray();
        float walletWidth=ImGui.CalcTextSize(bits).X+128;
        float missionWidth=ImGui.CalcTextSize("Daily missions:"u8).X+12+labels.Sum(label=>ImGui.CalcTextSize(label).X+26);
        float right=width-16-76-14-walletWidth-(loot?76:0),missionStart=232;
        bool inline=right-missionStart-14>=missionWidth;float height=inline?64:104;
        ImGui.PopFont();
        collectionArtwork.Draw("store-hero",origin,new(width,height),255,true,.55f);
        draw.AddRectFilledMultiColor(origin,origin+new float2(width,height),new ImColor8(1,10,19,220),new ImColor8(1,10,19,140),new ImColor8(1,10,19,190),new ImColor8(1,10,19,235));draw.AddLine(origin+new float2(0,height),origin+new float2(width,height),HomeLine);
        var modes=origin+new float2(16,12);draw.AddRectFilled(modes,modes+new float2(200,40),new ImColor8(1,10,19,225));draw.AddRect(modes,modes+new float2(200,40),HomeLine);
        ImGui.PushFont(NativeUiTheme.DisplayBoldFont,17);ImGui.PushStyleColor(ImGuiCol.Button,new ImColor8(0,0,0,0));ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,0);
        ImGui.SetCursorScreenPos(modes+new float2(3,3));
        if(Button("matchCollectionStore","STORE",96,34,collectionStore&&!showEggs))OpenMatchCosmetics(true);
        if(collectionStore&&!showEggs)draw.AddRect(ImGui.GetItemRectMin(),ImGui.GetItemRectMax(),HomeGold);
        ImGui.SetCursorScreenPos(modes+new float2(101,3));
        if(Button("matchCollectionLocker","LOCKER",96,34,!collectionStore&&!showEggs))OpenMatchCosmetics(false);
        if(!collectionStore&&!showEggs)draw.AddRect(ImGui.GetItemRectMin(),ImGui.GetItemRectMax(),HomeGold);
        ImGui.PopStyleVar();ImGui.PopStyleColor();ImGui.PopFont();
        float missionX=inline?missionStart:16,missionY=inline?20:64;
        ImGui.PushFont(NativeUiTheme.BodyFont,15);draw.AddText(origin+new float2(missionX,missionY+6),HomeMute,"Daily missions:"u8);missionX+=ImGui.CalcTextSize("Daily missions:"u8).X+12;
        for(int i=0;i<daily.Missions.Length;i++)
        {
            var mission=daily.Missions[i];float badgeWidth=ImGui.CalcTextSize(labels[i]).X+16;var at=origin+new float2(missionX,missionY);
            ImGui.SetCursorScreenPos(at);ImGui.Dummy(new(badgeWidth,28));draw.AddRectFilled(at,at+new float2(badgeWidth,28),new ImColor8(1,10,19,215));draw.AddRect(at,at+new float2(badgeWidth,28),mission.Done?new ImColor8(56,121,87):HomeLine);
            draw.AddText(at+new float2(8,6),mission.Done?HomeGold:HomeInk,labels[i]);
            if(ImGui.IsItemHovered())ImGui.SetTooltip(mission.Done?"Complete":mission.Goal>1?$"This game: {mission.Current}/{mission.Goal}. Complete it in one game today.":"Complete it in one game today.");
            missionX+=badgeWidth+10;
        }
        ImGui.PopFont();
        float walletX=width-16-76-14-walletWidth;
        if(loot){ImGui.SetCursorScreenPos(origin+new float2(walletX-76,16));if(Button("toggleEggs","Loot",64,34,showEggs))OpenCollectionPage(loot:true);}
        var walletAt=origin+new float2(walletX,12);draw.AddRectFilled(walletAt,walletAt+new float2(walletWidth,40),new ImColor8(1,10,19,235));draw.AddRect(walletAt,walletAt+new float2(walletWidth,40),HomeGold);
        var diamond=walletAt+new float2(14,20);draw.AddTriangleFilled(diamond+new float2(0,-7),diamond+new float2(7,0),diamond+new float2(0,7),new ImColor8(46,181,229));draw.AddTriangleFilled(diamond+new float2(0,-7),diamond+new float2(-7,0),diamond+new float2(0,7),new ImColor8(145,233,255));
        ImGui.SetCursorScreenPos(walletAt+new float2(26,10));HomeLabel(bits,18,HomeInk);
        ImGui.SetCursorScreenPos(walletAt+new float2(walletWidth-92,5));ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,new float2(8,6));ImGui.PushFont(NativeUiTheme.BodyFont,15);CollectionHistoryControl(86);ImGui.PopFont();ImGui.PopStyleVar();
        ImGui.SetCursorScreenPos(origin+new float2(width-92,16));bool close=CollectionActionButton("closeCollection","Close",76,34);if(ImGui.IsItemHovered())ImGui.SetTooltip("Return to match"u8);
        ImGui.SetCursorScreenPos(origin+new float2(16,height));ImGui.Dummy(new(0,0));return close;
    }
    private void CollectionHistoryControl(float width=110)
    {
        ImGui.SetNextItemWidth(width);
        ImGui.SetNextWindowSize(new(500,Math.Min(420,size.Y-160)));
        bool historyOpen=ImGui.BeginCombo("##collectionBitsHistory"u8,"History"u8,ImGuiComboFlags.NoArrowButton);
        CollectionFilterAnchor("collectionBitsHistory");
        if(historyOpen)
        {
            HomeLabel("RECENT BITS",21,HomeGold);
            var entries=ProfileSummary.Read(null,profile!.Storage.GetValueOrDefault("dt.cosmetics.v1")).BitsHistory.Take(12).ToArray();
            if(entries.Length==0)CollectionText("No Bits earned yet. Finish a game to earn some.");
            foreach(var entry in entries)
            {
                CollectionText($"{entry.Reason} / +{entry.N} Bits / {DateTimeOffset.FromUnixTimeMilliseconds(entry.At).LocalDateTime:d}");
                ImGui.Separator();CollectionBitsHistoryRows++;
            }
            ImGui.EndCombo();
        }
    }
    private void CollectionRewardControls()
    {
        CollectionMissions=[];CollectionBitsHistoryRows=0;
        if(evolutionLegend is not null||!collectionStore&&!MatchCosmeticsVisible)return;
        ImGui.SameLine();CollectionHistoryControl();
        var daily=MatchRewards.LiveProgress(profile!.Storage.GetValueOrDefault("dt.shop.v1"),MatchCosmeticsVisible?history?.LiveStats:null,DateOnly.FromDateTime(DateTime.Now));
        CollectionMissions=daily.Missions;
        float remaining=ImGui.GetWindowPos().X+ImGui.GetWindowSize().X-ImGui.GetStyle().WindowPadding.X-(Buttons["collectionBitsHistory"].X+55+18);
        bool inline=!MatchCosmeticsVisible&&remaining>480;
        float width=inline?(remaining-24)/2:(ImGui.GetContentRegionAvail().X-16)/2;
        if(inline)ImGui.SameLine(0,18);else ImGui.Spacing();
        for(int i=0;i<daily.Missions.Length;i++)
        {
            if(i>0)ImGui.SameLine(0,16);
            var mission=daily.Missions[i];ImGui.BeginGroup();
            ImGui.PushFont(NativeUiTheme.BodyFont,15);
            ImGui.PushTextWrapPos(ImGui.GetCursorPosX()+width);
            ImGui.Text($"{mission.Name} / {mission.Bits} Bits"+(mission.Done?" / DONE":""));
            ImGui.PushStyleColor(ImGuiCol.Text,mission.Done?HomeGold:HomeMute);
            if(MatchCosmeticsVisible)ImGui.Text(mission.Done?"Complete":mission.Goal>1?$"This game: {mission.Current}/{mission.Goal}":"Complete it in one game today");
            ImGui.PopStyleColor();ImGui.PopTextWrapPos();ImGui.PopFont();ImGui.EndGroup();
            if(ImGui.IsItemHovered())ImGui.SetTooltip("Complete it in one game today"u8);
        }
        ImGui.Spacing();
    }
}
