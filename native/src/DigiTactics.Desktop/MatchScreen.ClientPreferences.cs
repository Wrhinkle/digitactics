using Brutal.ImGuiApi;
using DigiTactics.Presentation;
using DigiTactics.Storage;
using System.Reflection;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private ClientPreferencesStore? clientPreferenceStore;
    private ClientPreferences clientPreferences = new();
    private bool showSettings;
    private string? menuHovered;
    private bool menuHoveredLastFrame;
    private bool ClientMotion => clientPreferences.Motion;
    private bool SocialEnabled => clientPreferences.Social;
    internal bool SettingsVisible => showSettings;
    internal ClientPreferences InterfacePreferences => clientPreferences;
    // Farm, evolution and Codex own their source action sounds outside the client overlay.
    private bool ClientMenuActive => !showFarm && evolutionLegend is null && !CodexVisible && (ClientHomeVisible || showProfile || showCollection || PostgameVisible);
    private void UpdateClientPreferences()
    {
        if (profile is not null)
        {
            clientPreferenceStore ??= new(profile);
            clientPreferenceStore.Refresh(); clientPreferences = clientPreferenceStore.Settings;
        }
        audio.MenuSounds = clientPreferences.UiSfx;
    }
    private void SaveClientPreferences(ClientPreferences value)
    {
        if (clientPreferenceStore is null || clientPreferenceStore.Save(value)) clientPreferences = value;
        audio.MenuSounds = clientPreferences.UiSfx;
    }
    private void BeginClientMenuFrame()
    {
        if (!menuHoveredLastFrame) menuHovered = null;
        menuHoveredLastFrame = false;
    }
    private void ClientItemSound(string key, bool pressed, string? click = "pick", bool quiet = true)
    {
        if (!ClientMenuActive) return;
        // Click takes precedence when an input backend delivers enter and release in one frame.
        if (pressed && click is not null) audio.PlayClient(click, quiet);
        if (!ImGui.IsItemHovered()) return;
        if (menuHovered != key && !pressed) audio.PlayClient("tick");
        menuHovered = key; menuHoveredLastFrame = true;
    }
    private void ClientButtonSound(string key, bool pressed)
    {
        // These actions already dispatch their nonquiet source sound after a successful change.
        bool action = key.StartsWith("equip/", StringComparison.Ordinal) || key.StartsWith("profileBanner/", StringComparison.Ordinal)
            || key.StartsWith("profileAvatar/", StringComparison.Ordinal) || key.StartsWith("previewSound/", StringComparison.Ordinal)
            || key.StartsWith("previewKill/", StringComparison.Ordinal) || key is "start" or "confirmPurchase" or "confirmEgg" or "playAgain" or "equipEgg";
        string? click = action ? null : key.StartsWith("preview/", StringComparison.Ordinal) || key.StartsWith("egg/", StringComparison.Ordinal) ? "place" : "pick";
        ClientItemSound(key, pressed, click);
    }
    private void ClientPreferenceControls()
    {
        ImGui.SeparatorText("Interface"u8);
        bool motion = clientPreferences.Motion, sound = clientPreferences.UiSfx, social = clientPreferences.Social;
        if (ImGui.Checkbox("Animations"u8, ref motion)) SaveClientPreferences(clientPreferences with { Motion = motion });
        Buttons["setMotion"] = (ImGui.GetItemRectMin() + ImGui.GetItemRectMax()) * .5f;
        UiText.Wrapped("Menu effects and the turning pedestal"u8);
        if (ImGui.Checkbox("Menu sounds"u8, ref sound)) SaveClientPreferences(clientPreferences with { UiSfx = sound });
        Buttons["setUiSfx"] = (ImGui.GetItemRectMin() + ImGui.GetItemRectMax()) * .5f;
        UiText.Wrapped("Clicks and hovers in the menus"u8);
        if (ImGui.Checkbox("Friends list"u8, ref social)) { SaveClientPreferences(clientPreferences with { Social = social }); audio.PlayClient("tick"); }
        Buttons["setSocial"] = (ImGui.GetItemRectMin() + ImGui.GetItemRectMax()) * .5f;
        UiText.Wrapped("The social column on the right"u8);
        if (clientPreferenceStore?.Error is {} error) UiText.Wrapped(error);
    }
    private void ClientSettingsPanel()
    {
        float width = Math.Min(760, size.X - 48), height = Math.Min(820, size.Y - 48);
        Window("Settings", (size.X - width) / 2, (size.Y - height) / 2, width, height);
        bool escape = !overlayShortcutConsumed && !ImGui.GetIO().WantTextInput && !ShortcutModifiers && ImGui.IsKeyPressed(ImGuiKey.Escape, false);
        if (escape && confirmResetSave) { confirmResetSave = false; resetSaveText.SetValue("".AsSpan()); escape = false; }
        if (Button("closeSettings", "Back to Home", 145, 32) || escape) { showSettings = confirmResetSave = false; ImGui.End(); return; }
        ClientPreferenceControls();
        ImGui.SeparatorText("Audio"u8); ImGui.PushItemWidth(240); AudioControls(); ImGui.PopItemWidth();
        SaveDataControls(); ClientVersionControls(); ClientKeyboardHelp(); ImGui.End(); MusicLibraryPanel();
    }
    private static readonly string InstalledClientVersion = ClientUpdateDisplay.InstalledVersion(
        typeof(MatchScreen).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
        typeof(MatchScreen).Assembly.GetName().Version,
        typeof(MatchScreen).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration);
    private void ClientVersionControls()
    {
        UpdateClient?.Refresh();
        ImGui.SeparatorText("About"u8);
        UiText.Wrapped(InstalledClientVersion);
        UiText.Wrapped(ClientUpdateDisplay.Status(UpdateClient is not null, UpdateClient?.Status));
    }
    private static readonly (string Key, string Action)[] ClientKeys =
    [
        ("D", "Reroll the shop"),
        ("F", "Buy experience"),
        ("E", "Sell the Digimon under the cursor on your board"),
        ("Space", "Start the fight, or return to your board while scouting"),
        ("T", "Emote wheel"),
        ("G", "Soundboard"),
        ("1-6", "Choose a slot while the emote wheel or soundboard is open"),
        ("C", "Codex"),
        ("1-8, Q / E", "Scout another Tamer; E sells instead when hovering your Digimon"),
        ("R", "Reset the camera"),
        ("+ / -", "Zoom"),
        ("Right-click", "Move your Little Legend"),
        ("Esc", "Back / close the top window"),
        ("Tab / Shift+Tab", "Move between buttons"),
        ("Enter", "Activate the focused button")
    ];
    private static void ClientKeyboardHelp()
    {
        ImGui.Spacing();
        if (!ImGui.CollapsingHeader("Keys"u8)) return;
        UiText.Wrapped("Match shortcuts are inactive while typing or using another window."u8);
        if (!ImGui.BeginTable("Settings keys"u8, 2, ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.SizingFixedFit)) return;
        foreach (var (key, action) in ClientKeys)
        {
            ImGui.TableNextRow(); ImGui.TableNextColumn(); ImGui.Text(key);
            ImGui.TableNextColumn(); UiText.Wrapped(action);
        }
        ImGui.EndTable();
    }
}
