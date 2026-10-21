using Brutal.ImGuiApi;
using Brutal.Numerics;
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
    internal float2 SettingsScrollMin {get;private set;}
    internal float2 SettingsScrollMax {get;private set;}
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
        ClientCardHeading("INTERFACE");
        bool motion = clientPreferences.Motion, sound = clientPreferences.UiSfx, social = clientPreferences.Social;
        if(ClientSettingToggle("setMotion","Animations","Page transitions and the turning pedestal",ref motion))SaveClientPreferences(clientPreferences with{Motion=motion});
        if(ClientSettingToggle("setUiSfx","Menu sounds","Clicks and hovers in the menus",ref sound))SaveClientPreferences(clientPreferences with{UiSfx=sound});
        if(ClientSettingToggle("setSocial","Friends list","The social column on the right",ref social)){SaveClientPreferences(clientPreferences with{Social=social});audio.PlayClient("tick");}
        if (clientPreferenceStore?.Error is {} error) UiText.Wrapped(error);
    }
    private void ClientSettingsPanel()
    {
        float width=Math.Min(1280,size.X-48),height=size.Y-ClientPageTop-16;
        ClientPageWindow("Settings",(size.X-width)/2,ClientPageTop,width,height,backdrop:true);
        bool escape = !overlayShortcutConsumed && !ImGui.GetIO().WantTextInput && !ShortcutModifiers && ImGui.IsKeyPressed(ImGuiKey.Escape, false);
        if (escape && confirmResetSave) { confirmResetSave = false; resetSaveText.SetValue("".AsSpan()); escape = false; }
        ClientPageTitle("SETTINGS",15);ClientPageTitle("SETTINGS",30,ClientInk);
        ImGui.SameLine(ImGui.GetWindowWidth()-116);
        if (HomeButton("closeSettings", "Back", 84, 32) || escape) { confirmResetSave = false;BackClient(); ImGui.End(); return; }
        ImGui.Spacing();
        ImGui.BeginChild("Settings page scroll"u8,new(0,0));
        SettingsScrollMin=ImGui.GetWindowPos();SettingsScrollMax=SettingsScrollMin+ImGui.GetWindowSize();
        float available=ImGui.GetContentRegionAvail().X,gap=16;
        bool wide=available>=1100;
        float column=(available-gap*(wide?2:1))/(wide?3:2);
        // The original settings cards align at the top and size to their contents.
        // Do not force every card into the old 690px scrolling box.
        ImGui.BeginGroup();
        ClientCardBegin("Settings sound",new(column,0));ClientSettingsSound();ImGui.EndChild();
        ImGui.EndGroup();float rowBottom=ImGui.GetItemRectMax().Y;ImGui.SameLine(0,gap);
        ImGui.BeginGroup();
        ClientCardBegin("Settings interface",new(column,0));ClientPreferenceControls();ImGui.Spacing();SaveDataControls();ImGui.EndChild();
        ImGui.EndGroup();rowBottom=Math.Max(rowBottom,ImGui.GetItemRectMax().Y);
        if(wide)ImGui.SameLine(0,gap);else ImGui.SetCursorScreenPos(new(ImGui.GetWindowPos().X+ImGui.GetStyle().WindowPadding.X,rowBottom+gap));
        ClientCardBegin("Settings keys card",new(wide?column:0,0));ClientKeyboardHelp();ImGui.EndChild();
        rowBottom=Math.Max(rowBottom,ImGui.GetItemRectMax().Y);
        ImGui.SetCursorScreenPos(new(ImGui.GetWindowPos().X+ImGui.GetStyle().WindowPadding.X,rowBottom+gap));
        ClientCardBegin("Settings credits",new(0,0));ClientVersionControls();ImGui.Spacing();
        UiText.Wrapped("Digi-Tactics is an unofficial, non-commercial fan project. See Credits for asset sources and acknowledgements."u8);
        if(HomeButton("settingsCredits","Credits & acknowledgements",280,36))NavigateClient("about");
        ImGui.EndChild();ImGui.EndChild();ImGui.End();MusicLibraryPanel();
    }
    private static readonly ImColor8 ClientGold=new(200,170,110),ClientInk=new(240,230,210),ClientMuted=new(165,174,184),ClientBorder=new(70,62,46);
    private static void ClientPageTitle(string value,float font=20,ImColor8? color=null)
    {ImGui.PushFont(NativeUiTheme.DisplayFont,font);ImGui.PushStyleColor(ImGuiCol.Text,color??ClientGold);ImGui.Text(value);ImGui.PopStyleColor();ImGui.PopFont();}
    private static void ClientCardHeading(string value){ClientPageTitle(value,19,ClientInk);ImGui.Spacing();}
    private void ClientPageWindow(string name,float x,float y,float width,float height,bool backdrop=false)
    {
        ImGui.PushStyleColor(ImGuiCol.WindowBg,new ImColor8(1,10,19));ImGui.PushStyleColor(ImGuiCol.Border,ClientBorder);ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding,0);
        Window(name,x,y,width,height,ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoScrollbar|(backdrop?ImGuiWindowFlags.NoBackground:0));ImGui.PopStyleVar();ImGui.PopStyleColor(2);
    }
    private static void ClientCardBegin(string id,float2 extent)
    {
        ImGui.PushStyleColor(ImGuiCol.ChildBg,new ImColor8(3,13,24,240));ImGui.PushStyleColor(ImGuiCol.Border,ClientBorder);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding,0);ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(16,14));
        ImGui.BeginChild(id,extent,ImGuiChildFlags.Borders|(extent.Y==0?ImGuiChildFlags.AutoResizeY:ImGuiChildFlags.None));ImGui.PopStyleVar(2);ImGui.PopStyleColor(2);
        var at=ImGui.GetWindowPos();var end=at+ImGui.GetWindowSize();
        ImGui.GetWindowDrawList().AddRectFilledMultiColor(at+new float2(1,1),end-new float2(1,1),
            new ImColor8(10,20,40),new ImColor8(10,20,40),new ImColor8(1,10,19),new ImColor8(1,10,19));
    }
    private bool ClientSettingToggle(string key,string label,string? caption,ref bool value)
    {
        float width=ImGui.GetContentRegionAvail().X,height=caption is null?40:70;var at=ImGui.GetCursorScreenPos();
        ImGui.PushStyleColor(ImGuiCol.Button,new ImColor8(0,0,0,0));ImGui.PushStyleColor(ImGuiCol.ButtonHovered,new ImColor8(15,44,57,100));ImGui.PushStyleColor(ImGuiCol.ButtonActive,new ImColor8(15,44,57,180));
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,0);
        bool pressed=Button(key,"##"+key,width,height);ImGui.PopStyleVar();ImGui.PopStyleColor(3);if(pressed)value=!value;
        var draw=ImGui.GetWindowDrawList();var toggle=at+new float2(0,8);
        draw.AddRectFilled(toggle,toggle+new float2(40,22),value?new ImColor8(10,95,122):new ImColor8(1,10,19));draw.AddRect(toggle,toggle+new float2(40,22),ClientGold);
        draw.AddRectFilled(toggle+new float2(value?23:3,3),toggle+new float2(value?37:17,19),value?new ImColor8(205,250,250):ClientMuted);
        ImGui.SetCursorScreenPos(at+new float2(54,5));ImGui.BeginGroup();ImGui.PushTextWrapPos(ImGui.GetCursorPosX()+width-58);ImGui.Text(label);
        if(caption is not null){ImGui.PushStyleColor(ImGuiCol.Text,ClientMuted);ImGui.Text(caption);ImGui.PopStyleColor();}ImGui.PopTextWrapPos();ImGui.EndGroup();ImGui.SetCursorScreenPos(at+new float2(0,height+6));return pressed;
    }
    private void ClientSettingsSound()
    {
        ClientCardHeading("SOUND");bool muted=audio.Muted,musicOn=!audio.MusicOff,others=audio.MuteOthers;
        if(ClientSettingToggle("settingsMute","Mute all",null,ref muted))audio.Muted=muted;
        if(ClientSettingToggle("settingsMusicOn","Music",null,ref musicOn))audio.MusicOff=!musicOn;
        float music=audio.MusicVolume,effects=audio.EffectsVolume,board=audio.BoardVolume;
        if(ClientSettingsSlider("settingsMusicVolume","Music volume",ref music))audio.MusicVolume=music;
        if(ClientSettingsSlider("settingsEffectsVolume","Effects volume",ref effects))audio.EffectsVolume=effects;
        if(ClientSettingsSlider("settingsBoardVolume","Soundboard volume",ref board))audio.BoardVolume=board;
        if(ClientSettingToggle("settingsMuteOthers","Mute other players' soundboard",null,ref others))audio.MuteOthers=others;
        ImGui.Spacing();ImGui.Separator();ImGui.Spacing();UiText.Wrapped(audio.MusicSource=="own"?"Playing your own music files.":"Built-in soundtrack.");
        if(audio.MusicController is not null&&HomeButton("musicLibrary","Your music files...",220,36))showMusicLibrary=true;
        if(audio.Error is{} error){UiText.Wrapped(error);if(HomeButton("reconnectAudio","Reconnect audio",210,36))audio.Reconnect();}
        if(audio.SettingsError is{} settingsError)UiText.Wrapped(settingsError);
    }
    private bool ClientSettingsSlider(string id,string label,ref float value)
    {
        float available=ImGui.GetContentRegionAvail().X;
        // Source .srow: label, slider, percentage share one readable row.
        bool inline=available>=320;
        ImGui.AlignTextToFramePadding();ImGui.Text(label);
        if(inline)ImGui.SameLine(Math.Max(170,available*.44f));
        ImGui.SetNextItemWidth(Math.Max(80,ImGui.GetContentRegionAvail().X-45));float percent=value*100;
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,new float2(8,4));ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,0);
        foreach(var color in new[]{ImGuiCol.FrameBg,ImGuiCol.FrameBgHovered,ImGuiCol.FrameBgActive,ImGuiCol.SliderGrab,ImGuiCol.SliderGrabActive})ImGui.PushStyleColor(color,new ImColor8(0,0,0,0));
        bool changed=ImGui.SliderFloat("##"+id,ref percent,0,100,""u8);ImGui.PopStyleColor(5);ImGui.PopStyleVar(2);
        if(changed)value=MathF.Round(percent)/100;
        var min=ImGui.GetItemRectMin();var max=ImGui.GetItemRectMax();Buttons[id]=(min+max)*.5f;
        bool released=ImGui.IsItemDeactivatedAfterEdit();
        float center=(min.Y+max.Y)*.5f,left=min.X+8,right=max.X-8;
        var draw=ImGui.GetWindowDrawList();draw.AddRectFilled(new(left,center-2),new(right,center+2),new ImColor8(91,94,98),2);
        draw.AddRectFilled(new(left,center-2),new(left+(right-left)*value,center+2),ClientGold,2);
        draw.AddCircleFilled(new(left+(right-left)*value,center),7,ClientGold);
        ImGui.SameLine(0,8);ImGui.Text(((int)MathF.Round(value*100)).ToString());
        if(id=="settingsEffectsVolume"&&released)audio.PlayClient("buy",false);
        ImGui.Spacing();return changed;
    }
    private static readonly string InstalledClientVersion = ClientUpdateDisplay.InstalledVersion(
        typeof(MatchScreen).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
        typeof(MatchScreen).Assembly.GetName().Version,
        typeof(MatchScreen).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration);
    private void ClientVersionControls()
    {
        UpdateClient?.Refresh();
        ClientCardHeading("ABOUT");
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
        ClientCardHeading("KEYS");
        UiText.Wrapped("Match shortcuts are inactive while typing or using another window."u8);
        if (!ImGui.BeginTable("Settings keys"u8, 2, ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.SizingFixedFit)) return;
        ImGui.TableSetupColumn("Key"u8,ImGuiTableColumnFlags.WidthFixed,134);
        ImGui.TableSetupColumn("Action"u8,ImGuiTableColumnFlags.WidthStretch);
        foreach (var (key, action) in ClientKeys)
        {
            ImGui.TableNextRow(); ImGui.TableNextColumn();
            ImGui.PushFont(NativeUiTheme.BodyFont,14);
            var at=ImGui.GetCursorScreenPos();var label=ImGui.CalcTextSize(key);var extent=label+new float2(12,6);
            ImGui.Dummy(extent);var draw=ImGui.GetWindowDrawList();
            draw.AddRectFilled(at,at+extent,new ImColor8(1,10,19),3);
            draw.AddRect(at,at+extent,ClientBorder,3);
            draw.AddText(at+new float2(6,3),ClientInk,key);ImGui.PopFont();
            ImGui.TableNextColumn(); UiText.Wrapped(action);
        }
        ImGui.EndTable();
    }
}
