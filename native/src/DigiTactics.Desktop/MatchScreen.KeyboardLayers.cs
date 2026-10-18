using Brutal.ImGuiApi;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool overlayShortcutConsumed,keyboardPopupLastFrame;
    private static bool SlotKeyPressed(int slot)=>ImGui.IsKeyPressed((ImGuiKey)((int)ImGuiKey._1+slot),false)||ImGui.IsKeyPressed((ImGuiKey)((int)ImGuiKey.Keypad1+slot),false);
    private bool SocialKeyboardAllowed=>!ImGui.GetIO().WantTextInput&&!ShortcutModifiers&&!KeyboardPopup&&!overlayShortcutConsumed&&!CodexVisible&&!codexConsumedShortcut&&!showAudio&&!showPlayers&&!showMusicLibrary&&!showGameMenu&&!showProfile&&!showCollection&&!showFarm&&!showSettings&&!showSaveData&&!EliminationCinematicActive;
    private void OverlayShortcuts()
    {
        overlayShortcutConsumed=false;
        if(!ImGui.IsKeyPressed(ImGuiKey.Escape,false)||ShortcutModifiers||EliminationCinematicActive||CodexVisible||codexConsumedShortcut)return;
        // NewFrame may already have closed a combo popup for this Escape.
        // Its enclosing panel must retain the same one-layer dismissal rule.
        if(keyboardPopupLastFrame||KeyboardPopup){overlayShortcutConsumed=true;return;}
        // Close one top layer, retaining the screen beneath it. Farm training has
        // its own Escape/result handler and never shares keys with a live match.
        if(showMusicLibrary)
        {
            if(clearMusic||removeMusicId is not null){clearMusic=false;removeMusicId=null;}
            else showMusicLibrary=false;
        }
        else if(showAudio)showAudio=false;
        else if(showPlayers)showPlayers=false;
        else if(showEmotes)showEmotes=false;
        else if(showSoundboard)showSoundboard=false;
        else if(evolutionLegend is not null)CloseEvolution();
        else if(showCollection)
        {
            if(mysteryReward is not null)mysteryReward=null;
            else if(CollectionSearchEscape) { }
            else if(purchase is not null)purchase=null;
            else if(eggPurchase is not null)eggPurchase=null;
            else if(hatching is not null)hatching=null;
            else if(boughtCosmetic is not null)boughtCosmetic=null;
            else {showCollection=false;DisposeCollectionPreview();}
        }
        else if(showFarm)
        {
            if(training is not null){if(trainingScore is not null&&trainingSaved)training=null;}
            else if(farmManagement)farmManagement=false;
            else showFarm=false;
        }
        else return;
        overlayShortcutConsumed=true;
    }
}
