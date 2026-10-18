using Brutal.ImGuiApi;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private int? hoveredOwnedUnit;
    internal string? KeyboardFocusedButton {get;private set;}
    internal bool ViewingOwnBoard=>localSeat>=0&&viewed==localSeat;
    private ScoutSeat[] ScoutOrder()=>MatchKeyboard.Order(Session!.State.Players.Select(p=>new ScoutSeat(p.Id,p.Alive,p.Health,p.Placement,p.Team)),Session.State.Duos);
    private bool ShortcutModifiers=>ImGui.GetIO().KeyCtrl||ImGui.GetIO().KeyAlt||ImGui.GetIO().KeySuper;
    private bool KeyboardPopup=>ImGui.IsPopupOpen(""u8,ImGuiPopupFlags.AnyPopup);
    private bool MatchKeyboardBlocked=>ImGui.GetIO().WantTextInput||ShortcutModifiers||KeyboardPopup||overlayShortcutConsumed||CodexVisible||codexConsumedShortcut||showAudio||showPlayers||showMusicLibrary||showGameMenu||showProfile||showCollection||showFarm||showSoundboard||showEmotes||showSettings||showSaveData||EliminationCinematicActive;
    private static readonly (ImGuiKey Key,string Value)[] matchKeys=[(ImGuiKey.Escape,"escape"),(ImGuiKey.Space," "),(ImGuiKey.D,"d"),(ImGuiKey.F,"f"),(ImGuiKey.Q,"q"),(ImGuiKey.E,"e"),..Enumerable.Range(0,8).SelectMany(i=>new[]{((ImGuiKey)((int)ImGuiKey._1+i),(i+1).ToString()),((ImGuiKey)((int)ImGuiKey.Keypad1+i),(i+1).ToString())})];
    private void ReserveMatchSpace()
    {
        if(Session is null||MatchKeyboardBlocked||!ImGui.IsKeyDown(ImGuiKey.Space))return;
        // ImGui prepares button activation in NewFrame. Space belongs to the
        // match (return from scouting / ready), while Tab and Enter keep normal
        // widget navigation, so suppress only this Space-generated activation.
        var context=ImGui.GetCurrentContext();
        context.NavActivateId=default;context.NavActivateDownId=default;context.NavActivatePressedId=default;
    }
    private void MatchShortcuts()
    {
        if(Session is null||MatchKeyboardBlocked)return;
        foreach(var (key,value) in matchKeys)
        {
            if(!ImGui.IsKeyPressed(key,false))continue;
            var action=MatchKeyboard.Resolve(value,new(ScoutOrder(),localSeat,viewed,Session.State.Duos,Session.State.Over,Session.Rounds.Phase==MatchPhase.Planning,hoveredOwnedUnit));
            if(action is null)continue;
            switch(action.Kind)
            {
                case "scout":Watch(action.Seat!.Value);break;
                case "menu":OpenGameMenu();break;
                // Source readyBtn is disabled during carousel (ui.js). Space
                // must never skip the player's pick; Escape/scouting still work.
                case "fight":if(!Session.Units.Carousel.Active)Fight();break;
                case "sell":if(!arenaDragMoved&&!itemDragMoved){Act("sell",new{uid=action.Unit});selectedUnit=null;hoveredOwnedUnit=null;}break;
                default:Act(action.Kind);break;
            }
            return;
        }
        CameraShortcuts();
    }
}
