using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    // Every client page shares this navigation boundary, including at minimum size.
    private const float ClientPageTop = 96;
    private bool ClientNavigationBlocked=>training is not null||EliminationCinematicActive||EvolutionBusy;

    private void NavigateClient(string destination)
    {
        if (ClientNavigationBlocked) return;
        if(PostgameClientOverlay&&!PostgameHomeVisible)DismissResults();
        if (evolutionLegend is not null) CloseEvolution();
        showProfile = showFarm = showCollection = showAbout = showSettings = showSaveData = showHomePlay = showMultiplayer = false;
        codexFull = codexSide = false; codexUnit = codexModalLine = null;
        showMusicLibrary = showAudio = showPlayers = showEmotes = showSoundboard = false;
        confirmResetSave = false; resetSaveText.SetValue("");
        CancelArenaDrag(); cameraDrag = null;
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
            case "multiplayer": OpenMultiplayerView(); break;
            case "about": showAbout = true; break;
        }
    }

    private void DrawClientNavigation()
    {
        if (!ClientHomeVisible) return;
        float width = socialWindowSize.X;
        HomeCard("Client navigation", new(0, 0), new(width, 88), false, true);
        ImGui.SetCursorPos(new(16, 8));
        ImGui.BeginDisabled(ClientNavigationBlocked);
        if (HomeButton("clientPlay", "PLAY", 104, 36, true)) NavigateClient("play");
        foreach (var tab in new[] { ("homeOverview", "Home", "home", 72f), ("clientModes", "Modes", "play", 78f),
            ("collection", "Collection", "collection", 112f), ("clientLoot", "Loot", "loot", 62f), ("clientStore", "Store", "store", 70f),
            ("farm", "Digi-Farm", "farm", 104f), ("codex", "Codex", "codex", 74f) })
        {
            ImGui.SameLine(0, 6);
            ImGui.BeginDisabled(profile is null && tab.Item3 is "collection" or "loot" or "store" or "farm");
            if (HomeButton(tab.Item1, tab.Item2, tab.Item4, 36)) NavigateClient(tab.Item3);
            bool selected=tab.Item3 switch
            {
                "play"=>showHomePlay, "collection"=>showCollection&&!collectionStore&&!showEggs,
                "store"=>showCollection&&collectionStore&&!showEggs, "loot"=>showCollection&&showEggs,
                "farm"=>showFarm, "codex"=>codexFull,
                _=>!showHomePlay&&!showCollection&&!showFarm&&!codexFull&&!showMultiplayer&&!showProfile&&!showSettings&&!showSaveData&&!showAbout
            };
            if(selected){var min=ImGui.GetItemRectMin();var max=ImGui.GetItemRectMax();ImGui.GetWindowDrawList().AddRectFilled(new(min.X+8,max.Y-3),new(max.X-8,max.Y-1),HomeGold);}
            ImGui.EndDisabled();
        }
        ImGui.SetCursorPos(new(16, 48));
        if (HomeButton("multiplayer", "Steam multiplayer", 174, 36)) NavigateClient("multiplayer");
        ImGui.SameLine(0, 6); ImGui.BeginDisabled(profile is null);
        if (HomeButton("profile", "Profile", 88, 36)) NavigateClient("profile");
        ImGui.EndDisabled(); ImGui.SameLine(0, 6);
        if (HomeButton("settings", "Settings", 96, 36)) NavigateClient("settings");
        ImGui.SameLine(0, 6);
        if (HomeButton("about", "About", 80, 36)) NavigateClient("about");
        ImGui.SameLine(0, 6); SocialNavigation();
        ImGui.EndDisabled();
        string balance = $"{profile?.Progress.Bits ?? 0:N0} Bits";
        float textWidth = ImGui.CalcTextSize(balance).X;
        ImGui.SetCursorScreenPos(new(width - textWidth - 24, 58)); HomeLabel(balance, 17, HomeGold);
        ImGui.End();
    }
}
