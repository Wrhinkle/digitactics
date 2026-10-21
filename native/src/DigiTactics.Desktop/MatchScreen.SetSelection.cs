using Brutal.ImGuiApi;
using DigiTactics.Networking;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private static readonly Lazy<SetSelection> setChoices=new(()=>SetSelection.Load(Path.Combine(AppContext.BaseDirectory,"content/set-selection.json")));
    private string selectedSet="faction-wars",selectedPace="hyper",lobbyPaceChoice="hyper";
    private bool lobbyDuos;
    private (string? Set,string? Pace,string? Format) savedSelection;
    private SetSelection SetChoices=>setChoices.Value;
    internal string SelectedSet=>selectedSet;
    internal string SelectedPace=>selectedPace;
    internal bool SelectedDuos=>duos;
    internal string? LobbySetId=>steam?.Room?.Authority.Lobby?["setId"]?.GetValue<string>();
    internal string? LobbyPaceId=>steam?.Room?.Authority.Lobby?["pace"]?.GetValue<string>();
    internal string? LobbyFormatId=>steam?.Room?.Authority.Lobby?["format"]?.GetValue<string>();
    internal System.Text.Json.JsonElement? SteamMatchInit=>steam?.Room?.Init;
    private string MatchSetTitle=>Session?.State.Chaos==true?"Chaos: "+Rules.Name:Rules.Name;
    private string MatchPaceTitle=>SetSelection.PaceLabel(Rules.Pace)+(Session?.State.ChaosPace==true?" (Chaos pace)":"");
    private (string? Set,string? Pace,string? Format) ReadSavedSelection()=>
        (profile?.Storage.GetValueOrDefault("dt.set.v1"),profile?.Storage.GetValueOrDefault("dt.pace.v1"),profile?.Storage.GetValueOrDefault("dt.format.v1"));
    private void InitializeSetSelection(bool forceDuos)
    {
        savedSelection=ReadSavedSelection();selectedSet=SetChoices.SavedSet(savedSelection.Set);selectedPace=SetSelection.SavedPace(savedSelection.Pace);lobbyPaceChoice=selectedPace;
        // A seeded smoke explicitly supplies its format; normal launches restore the user's choice.
        duos=forceDuos || seed is null && SetSelection.SavedFormat(savedSelection.Format)=="duos";lobbyDuos=duos;
        UpdateSelectionPreview();
    }
    private void RefreshSetSelection()
    {
        var saved=ReadSavedSelection();if(saved==savedSelection)return;
        selectedSet=SetChoices.SavedSet(saved.Set);selectedPace=SetSelection.SavedPace(saved.Pace);duos=SetSelection.SavedFormat(saved.Format)=="duos";
        if(saved.Pace!=savedSelection.Pace)lobbyPaceChoice=selectedPace;
        if(saved.Format!=savedSelection.Format)lobbyDuos=duos;
        savedSelection=saved;UpdateSelectionPreview();
    }
    private void SaveSelection(string key,string value)
    {
        if(profile is not null && !profile.UpdateStorage(new Dictionary<string,string>{{key,value}}))profileMessage=profile.Error??"Your selection could not be saved.";
        savedSelection=ReadSavedSelection();
    }
    private void UpdateSelectionPreview()
    {
        // Like the original codex, selecting Chaos keeps the last concrete preview until a game starts.
        if(Session is null && selectedSet!="chaos")variant=Array.IndexOf(catalog.Variants,SetSelection.Rules(catalog,SetChoices.Resolve(selectedSet,selectedPace,0)));
    }
    private void SelectSet(string id)
    {
        if(!SetChoices.Pickable(id))return;
        selectedSet=id;SaveSelection("dt.set.v1",id);UpdateSelectionPreview();
    }
    private void SelectPace(string pace,bool lobby=false)
    {
        if(lobby)lobbyPaceChoice=SetSelection.LobbyPace(pace);
        if(pace is not ("hyper" or "normal"))return;
        selectedPace=lobbyPaceChoice=pace;SaveSelection("dt.pace.v1",pace);UpdateSelectionPreview();
    }
    private void SelectFormat(string format)
    {lobbyDuos=duos=SetSelection.SavedFormat(format)=="duos";SaveSelection("dt.format.v1",duos?"duos":"solo");}
    private void SelectionButton(string key)
    {var min=ImGui.GetItemRectMin();var max=ImGui.GetItemRectMax();Buttons[key]=(min+max)*.5f;}
    private void DrawSetChoice(string key,string current,Action<string> choose)
    {
        if(ImGui.BeginCombo("##"+key,SetChoices.Label(current),ImGuiComboFlags.HeightLargest))
        {
            foreach(string id in SetChoices.Options.Select(s=>s.Id).Concat(SetChoices.HasChaos?["chaos"]:Array.Empty<string>()))
            {
                bool picked=ImGui.Selectable(SetChoices.Label(id),current==id);SelectionButton(key+"/"+id);
                if(picked)choose(id);ClientItemSound(key+"/"+id,picked,"levelup",false);
            }
            ImGui.EndCombo();
        }
        SelectionButton(key);
    }
    private void DrawPaceChoice(string key,string current,bool chaos,Action<string> choose)
    {
        if(ImGui.BeginCombo("##"+key,SetSelection.PaceLabel(current)))
        {
            foreach(string pace in chaos?new[]{"hyper","normal","chaos"}:new[]{"hyper","normal"})
            {bool picked=ImGui.Selectable(SetSelection.PaceLabel(pace),current==pace);SelectionButton(key+"/"+pace);if(picked)choose(pace);ClientItemSound(key+"/"+pace,picked);}
            ImGui.EndCombo();
        }
        SelectionButton(key);
    }
    private void DrawFormatChoice(string key,bool duo,Action<string> choose)
    {
        if(ImGui.BeginCombo("##"+key,duo?"Duos · 2v2":"Free for all"))
        {
            foreach(string format in new[]{"solo","duos"})
            {bool picked=ImGui.Selectable(format=="duos"?"Duos · 2v2":"Free for all",duo==(format=="duos"));SelectionButton(key+"/"+format);if(picked)choose(format);ClientItemSound(key+"/"+format,picked);}
            ImGui.EndCombo();
        }
        SelectionButton(key);
    }
    private void LobbySetChoices(NetworkRoom room)
    {
        var lobby=room.Authority.Lobby!;
        ImGui.Text("Set / pace / format"u8);ImGui.SetNextItemWidth(235);
        DrawSetChoice("steamRoomSet",SetChoices.SavedSet(lobby["setId"]?.GetValue<string>()),id=>{if(room.UpdateLobby(new(){["setId"]=id}))SelectSet(id);});
        ImGui.SameLine();ImGui.SetNextItemWidth(165);
        DrawPaceChoice("steamRoomPace",SetSelection.LobbyPace(lobby["pace"]?.GetValue<string>()),true,pace=>{if(room.UpdateLobby(new(){["pace"]=pace}))SelectPace(pace,true);});
        ImGui.SameLine();ImGui.SetNextItemWidth(115);
        DrawFormatChoice("steamRoomFormat",lobby["format"]?.GetValue<string>()=="duos",format=>{if(room.UpdateLobby(new(){["format"]=format}))lobbyDuos=format=="duos";});
    }
}
