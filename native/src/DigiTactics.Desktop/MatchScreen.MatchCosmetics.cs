using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool matchCosmeticsActive;
    internal bool MatchCosmeticsVisible=>matchCosmeticsActive&&Session is not null&&!ClientHomeVisible&&(showCollection||showFarm);
    internal bool MatchStoreEntryAvailable=>Session is not null&&(Session.State.Over||IsSpectating||!Human.Alive||Session.Rounds.Phase==MatchPhase.Planning);
    private bool MatchLockerVisible=>MatchCosmeticsVisible&&showCollection&&!collectionStore&&!showEggs;
    internal (float2 Min,float2 Max) MatchCosmeticsBounds {get;private set;}
    internal DailyMissionProgress[] CollectionMissions {get;private set;}=[];
    internal int CollectionBitsHistoryRows {get;private set;}

    private void MatchCosmeticsControl()
    {
        if(profile is null)return;
        ImGui.SetCursorScreenPos(new(332,22));ImGui.SetNextItemWidth(124);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,new float2(10,8));
        bool open=ImGui.BeginCombo("##matchCosmetics"u8,"Cosmetics"u8,ImGuiComboFlags.NoArrowButton);
        CollectionFilterAnchor("matchCosmetics");ImGui.PopStyleVar();
        if(!open)return;
        if(MatchStoreEntryAvailable&&Button("matchStore","Store",200,36)){ImGui.CloseCurrentPopup();OpenMatchCosmetics(true);}
        if(Button("matchLocker","Locker",200,36)){ImGui.CloseCurrentPopup();OpenMatchCosmetics(false);}
        if(Button("matchLegend","Little Legend",200,36)){ImGui.CloseCurrentPopup();OpenMatchCosmetics(false,"legend");}
        ImGui.EndCombo();
    }
    private void OpenMatchCosmetics(bool store,string? kind=null)
    {
        if(profile is null||Session is null)return;
        CancelArenaDrag();cameraDrag=null;
        showAudio=showPlayers=showEmotes=showSoundboard=showMusicLibrary=false;
        showFarm=false;matchCosmeticsActive=true;
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
            ImGui.SameLine();if(Button("matchCollectionBack","Back to cosmetics",190,38)){showFarm=false;showCollection=true;}
            ImGui.EndDisabled();ImGui.SameLine();ImGui.Text("The match continues while this panel is open."u8);ImGui.End();
            if(showFarm)FarmPanel();
        }
        else Collection();
        return true;
    }
    private void MatchCollectionNavigation()
    {
        if(!MatchCosmeticsVisible||evolutionLegend is not null)return;
        ImGui.SameLine(0,20);
        if(Button("matchCollectionStore","Store",88,36,collectionStore))OpenMatchCosmetics(true);
        ImGui.SameLine();if(Button("matchCollectionLocker","Locker",96,36,!collectionStore))OpenMatchCosmetics(false);
    }
    private void CollectionRewardControls()
    {
        CollectionMissions=[];CollectionBitsHistoryRows=0;
        if(evolutionLegend is not null||!collectionStore&&!MatchCosmeticsVisible)return;
        ImGui.SameLine();ImGui.SetNextItemWidth(110);
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
        var daily=MatchRewards.LiveProgress(profile!.Storage.GetValueOrDefault("dt.shop.v1"),MatchCosmeticsVisible?history?.LiveStats:null,DateOnly.FromDateTime(DateTime.Now));
        CollectionMissions=daily.Missions;
        float width=(ImGui.GetContentRegionAvail().X-16)/2;
        ImGui.Spacing();
        for(int i=0;i<daily.Missions.Length;i++)
        {
            if(i>0)ImGui.SameLine(0,16);
            var mission=daily.Missions[i];ImGui.BeginGroup();
            ImGui.PushTextWrapPos(ImGui.GetCursorPosX()+width);
            ImGui.Text($"{mission.Name} / {mission.Bits} Bits");
            ImGui.PushStyleColor(ImGuiCol.Text,mission.Done?HomeGold:HomeMute);
            ImGui.Text(mission.Done?"Complete":MatchCosmeticsVisible&&mission.Goal>1?$"This game: {mission.Current}/{mission.Goal}":"Complete it in one game today");
            ImGui.PopStyleColor();ImGui.PopTextWrapPos();ImGui.EndGroup();
        }
        ImGui.Spacing();
    }
}
