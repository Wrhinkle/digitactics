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
    private static readonly string InstalledClientVersion = ClientUpdateDisplay.InstalledVersion(
        typeof(MatchScreen).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
        typeof(MatchScreen).Assembly.GetName().Version,
        typeof(MatchScreen).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration);
    // RENDER.settings: .settings (max 1280px, 16px 24px 32px padding, 16px gaps) with a page head, a three-column
    // .set-grid of .box cards (two columns with Keys spanning both at 1500 px and below) and the About box.
    private void ClientSettingsPanel()
    {
        bool escape = !overlayShortcutConsumed && !ImGui.GetIO().WantTextInput && !ShortcutModifiers && ImGui.IsKeyPressed(ImGuiKey.Escape, false);
        if (escape && confirmResetSave) { confirmResetSave = false; resetSaveText.SetValue("".AsSpan()); escape = false; }
        if (escape) { confirmResetSave = false; BackClient(); return; }
        ClientLayer("Settings",new(0,ClientPageTop),new(size.X,size.Y-ClientPageTop),scroll:true,bringToFront:false);
        SettingsScrollMin=ImGui.GetWindowPos();SettingsScrollMax=SettingsScrollMin+ImGui.GetWindowSize();
        var draw=ImGui.GetWindowDrawList();var origin=ImGui.GetCursorScreenPos();
        float boxWidth=Math.Min(1280,size.X),left=origin.X+MathF.Round((size.X-boxWidth)/2)+24,width=boxWidth-48,y=origin.Y+16;
        Eyebrow(draw,new(left,y),"Settings");y+=NativeUiTheme.TextLine(11)+2;
        // .page-title: Chakra 700 26px, line-height 1.1, .04em, uppercase, gold-1.
        float titleLine=26*1.1f;
        NativeUiPaint.Tracked(draw,NativeUiTheme.DisplayBoldFont,26,new(left,MathF.Round(y+(titleLine-NativeUiTheme.LineHeight(NativeUiTheme.DisplayBoldFont,26))/2)),NativeUiTheme.GoldBright,"SETTINGS",.04f);
        y+=titleLine+12;
        bool threeColumns=socialWindowSize.X>1500;
        float column=threeColumns?(width-32)/3:(width-16)/2;
        float soundHeight=SettingsSoundCard(new(left,y),column,true),interfaceHeight=SettingsInterfaceCard(new(left+column+16,y),column,true);
        float keysLeft=threeColumns?left+(column+16)*2:left,keysTop=threeColumns?y:y+Math.Max(soundHeight,interfaceHeight)+16,keysWidth=threeColumns?column:width;
        float keysHeight=SettingsKeysCard(new(keysLeft,keysTop),keysWidth,true);
        float gridHeight=threeColumns?Math.Max(Math.Max(soundHeight,interfaceHeight),keysHeight):Math.Max(soundHeight,interfaceHeight)+16+keysHeight;
        // Paint cards behind their contents: each card's surface first, then its controls.
        draw.ChannelsSplit(2);draw.ChannelsSetCurrent(1);
        SettingsSoundCard(new(left,y),column,false);
        SettingsInterfaceCard(new(left+column+16,y),column,false);
        SettingsKeysCard(new(keysLeft,keysTop),keysWidth,false);
        draw.ChannelsSetCurrent(0);
        PaintTile(draw,new(left,y),new(left+column,y+soundHeight),box:true);
        PaintTile(draw,new(left+column+16,y),new(left+column*2+16,y+interfaceHeight),box:true);
        PaintTile(draw,new(keysLeft,keysTop),new(keysLeft+keysWidth,keysTop+keysHeight),box:true);
        draw.ChannelsMerge();
        y+=gridHeight+16;
        // .set-about
        float aboutTop=y;y+=17;
        float cardHeading=NativeUiTheme.TextLine(14);
        float aboutHeight=2+16+cardHeading+12+NativeUiTheme.TextLine(12.5f)+6+NativeUiTheme.TextLine(12.5f)+6+16;
        PaintTile(draw,new(left,aboutTop),new(left+width,aboutTop+aboutHeight),box:true);
        TileHeading(draw,new(left+17,y),width-34,"About",null,14);y+=cardHeading+12;
        NativeUiPaint.TrackedIn(draw,NativeUiTheme.BodyFont,12.5f,new(left+17,y),NativeUiTheme.TextLine(12.5f),NativeUiTheme.MutedNote,"Digi-Tactics is an unofficial, non-commercial fan project. See Credits for asset sources and acknowledgements.");
        y+=NativeUiTheme.TextLine(12.5f)+6;
        if(SettingsLink("settingsCredits","Credits & acknowledgements",new(left+17,y)))NavigateClient("about");
        y=aboutTop+aboutHeight+32;
        ImGui.SetCursorScreenPos(new(origin.X,y));ImGui.Dummy(new(1,0));
        DrawResetSaveModal();
        ImGui.End();MusicLibraryPanel();
    }
    /// <summary>A plain link (a, color --c-accent, underline on hover).</summary>
    private bool SettingsLink(string key,string label,float2 at,float px=12.5f)
    {
        var size=NativeUiPaint.TrackedSize(NativeUiTheme.BodyFont,px,label,0);
        ImGui.SetCursorScreenPos(at);bool clicked=ClientHitButton(key,size.X,size.Y);bool hot=ImGui.IsItemHovered();
        var draw=ImGui.GetWindowDrawList();NativeUiPaint.Tracked(draw,NativeUiTheme.BodyFont,px,at,NativeUiTheme.Accent,label);
        if(hot)draw.AddLine(at+new float2(0,size.Y-3),at+new float2(size.X,size.Y-3),NativeUiPaint.Fade(NativeUiTheme.Accent));
        return clicked;
    }
    /// <summary>.toggle: a 40x20 switch and a 14px/700 label with an optional 11px note; the whole row toggles.</summary>
    private float SettingsToggle(string key,string label,string? caption,ref bool value,float2 at,float width,bool measure)
    {
        float labelHeight=NativeUiTheme.TextLine(14),captionHeight=caption is null?0:NativeUiTheme.TextLine(11);
        float rowHeight=8+Math.Max(20,labelHeight+captionHeight)+8;
        if(measure)return rowHeight;
        ImGui.SetCursorScreenPos(at);
        bool pressed=ClientHitButton(key,width,rowHeight);if(pressed)value=!value;
        var draw=ImGui.GetWindowDrawList();float middle=at.Y+rowHeight/2;
        var lo=new float2(at.X,MathF.Round(middle-10));var hi=lo+new float2(40,20);
        if(value)NativeUiPaint.HorizontalGradient(draw,lo,hi,new ImColor8(10,95,122),new ImColor8(11,58,74));
        else draw.AddRectFilled(lo,hi,NativeUiPaint.Fade(new ImColor8(1,10,19,230)));
        draw.AddRect(lo,hi,NativeUiPaint.Fade(value?NativeUiTheme.Gold:NativeUiTheme.GoldBorder));
        var knob=lo+new float2(value?23:3,3);
        if(value)NativeUiPaint.BoxGlow(draw,knob,knob+new float2(14,14),new ImColor8(63,214,255,170),8);
        draw.AddRectFilled(knob,knob+new float2(14,14),NativeUiPaint.Fade(value?NativeUiTheme.AccentInk:NativeUiTheme.LcMute));
        float textTop=MathF.Round(middle-(labelHeight+captionHeight)/2);
        NativeUiPaint.TrackedIn(draw,NativeUiTheme.BodyBoldFont,14,new(at.X+52,textTop),labelHeight,NativeUiTheme.Text1,label);
        if(caption is not null)NativeUiPaint.TrackedIn(draw,NativeUiTheme.BodyFont,11,new(at.X+52,textTop+labelHeight),captionHeight,NativeUiTheme.Text3,SocialEllipsis(caption,NativeUiTheme.BodyFont,11,width-52));
        return rowHeight;
    }
    /// <summary>.srow: 130px label | range (accent-color gold-2) | 36px value, 6px vertical padding.</summary>
    private float SettingsSlider(string id,string label,ref float value,float2 at,float width,bool measure,out bool changed)
    {
        changed=false;const float rowHeight=32;
        if(measure)return rowHeight;
        var draw=ImGui.GetWindowDrawList();float middle=at.Y+rowHeight/2;
        float labelHeight=NativeUiTheme.TextLine(12.5f);
        NativeUiPaint.Tracked(draw,NativeUiTheme.BodyFont,12.5f,new(at.X,MathF.Round(middle-labelHeight/2)),NativeUiTheme.Muted,label);
        float trackLeft=at.X+130+12,trackRight=at.X+width-36-12;
        ImGui.SetCursorScreenPos(new(trackLeft,middle-10));ImGui.SetNextItemWidth(trackRight-trackLeft);
        float percent=value*100;
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,new float2(0,2));ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,0);ImGui.PushStyleVar(ImGuiStyleVar.GrabMinSize,16);
        foreach(var color in new[]{ImGuiCol.FrameBg,ImGuiCol.FrameBgHovered,ImGuiCol.FrameBgActive,ImGuiCol.SliderGrab,ImGuiCol.SliderGrabActive})ImGui.PushStyleColor(color,new ImColor8(0,0,0,0));
        ImGui.PushFont(NativeUiTheme.BodyFont,16);
        changed=ImGui.SliderFloat("##"+id,ref percent,0,100,""u8);
        ImGui.PopFont();ImGui.PopStyleColor(5);ImGui.PopStyleVar(3);
        if(changed)value=MathF.Round(percent)/100;
        var min=ImGui.GetItemRectMin();var max=ImGui.GetItemRectMax();Buttons[id]=(min+max)*.5f;
        bool released=ImGui.IsItemDeactivatedAfterEdit(),hot=ImGui.IsItemHovered()||ImGui.IsItemActive();
        // Chromium's accent-colored range: an 8px rounded track, gold fill, dark remainder, 16px round thumb.
        float x=trackLeft+8+(trackRight-trackLeft-16)*value;
        draw.AddRectFilled(new(trackLeft,middle-4),new(trackRight,middle+4),NativeUiPaint.Fade(new ImColor8(49,49,49)),4);
        draw.AddRect(new(trackLeft,middle-4),new(trackRight,middle+4),NativeUiPaint.Fade(new ImColor8(118,118,118,120)),4);
        draw.AddRectFilled(new(trackLeft,middle-4),new(x,middle+4),NativeUiPaint.Fade(hot?new ImColor8(214,188,136):NativeUiTheme.Gold),4);
        draw.AddCircleFilled(new(x,middle),8,NativeUiPaint.Fade(hot?new ImColor8(214,188,136):NativeUiTheme.Gold),24);
        string number=((int)MathF.Round(value*100)).ToString();float numberWidth=NativeUiPaint.TrackedWidth(NativeUiTheme.BodyFont,12.5f,number,0);
        NativeUiPaint.Tracked(draw,NativeUiTheme.BodyFont,12.5f,new(at.X+width-numberWidth,MathF.Round(middle-labelHeight/2)),NativeUiTheme.Muted,number);
        if(id=="settingsEffectsVolume"&&released)audio.PlayClient("buy",false);
        return rowHeight;
    }
    /// <summary>Wrapped 12.5px muted paragraph with the default 1em (12.5px) margins collapsed to the given top margin.</summary>
    private static float SettingsNote(ImDrawListPtr draw,float2 at,float width,string text,bool measure,ImColor8? color=null)
    {
        float lineHeight=NativeUiTheme.TextLine(12.5f);
        var lines=WrapText(NativeUiTheme.BodyFont,12.5f,text,width);
        if(!measure)for(int i=0;i<lines.Count;i++)NativeUiPaint.Tracked(draw,NativeUiTheme.BodyFont,12.5f,at+new float2(0,i*lineHeight),color??NativeUiTheme.MutedNote,lines[i]);
        return lines.Count*lineHeight;
    }
    private float SettingsSoundCard(float2 at,float width,bool measure)
    {
        var draw=ImGui.GetWindowDrawList();float inner=width-34,x=at.X+17,y=at.Y+17;
        if(!measure)TileHeading(draw,new(x,y),inner,"Sound",null,14);
        y+=NativeUiTheme.TextLine(14)+12;
        bool muted=audio.Muted,musicOn=!audio.MusicOff,others=audio.MuteOthers;
        y+=SettingsToggle("settingsMute","Mute all",null,ref muted,new(x,y),inner,measure);if(!measure&&muted!=audio.Muted)audio.Muted=muted;
        y+=SettingsToggle("settingsMusicOn","Music",null,ref musicOn,new(x,y),inner,measure);if(!measure&&musicOn==audio.MusicOff)audio.MusicOff=!musicOn;
        float music=audio.MusicVolume,effects=audio.EffectsVolume,board=audio.BoardVolume;
        y+=SettingsSlider("settingsMusicVolume","Music volume",ref music,new(x,y),inner,measure,out bool musicChanged);if(musicChanged)audio.MusicVolume=music;
        y+=SettingsSlider("settingsEffectsVolume","Effects volume",ref effects,new(x,y),inner,measure,out bool effectsChanged);if(effectsChanged)audio.EffectsVolume=effects;
        y+=SettingsSlider("settingsBoardVolume","Soundboard volume",ref board,new(x,y),inner,measure,out bool boardChanged);if(boardChanged)audio.BoardVolume=board;
        y+=SettingsToggle("settingsMuteOthers","Mute other players' soundboard",null,ref others,new(x,y),inner,measure);if(!measure&&others!=audio.MuteOthers)audio.MuteOthers=others;
        // .set-music: a gold-5 rule, then the soundtrack note and "Your music files..." on one row.
        y+=12;if(!measure)draw.AddLine(new(x,y+.5f),new(x+inner,y+.5f),NativeUiPaint.Fade(NativeUiTheme.GoldDim));y+=1+12;
        float buttonHeight=MathF.Round(12.5f*1.2f+12),noteHeight=NativeUiTheme.TextLine(12.5f),rowHeight=Math.Max(buttonHeight,noteHeight);
        if(!measure)
        {
            NativeUiPaint.Tracked(draw,NativeUiTheme.BodyFont,12.5f,new(x,MathF.Round(y+(rowHeight-noteHeight)/2)),NativeUiTheme.MutedNote,audio.MusicSource=="own"?"Playing your own music files.":"Built-in soundtrack.");
            // The library needs the music controller, which only diagnostic sessions run without.
            const string label="YOUR MUSIC FILES...";float buttonWidth=NativeUiPaint.TrackedWidth(NativeUiTheme.DisplayFont,12.5f,label,.08f)+22+2;
            ImGui.SetCursorScreenPos(new(x+inner-buttonWidth,y+(rowHeight-buttonHeight)/2));ImGui.BeginDisabled(audio.MusicController is null);
            if(ClientButton("musicLibrary",label,buttonWidth,buttonHeight,ClientButtonKind.Secondary,12.5f))showMusicLibrary=true;
            ImGui.EndDisabled();
        }
        y+=rowHeight;
        if(audio.Error is{} error)
        {
            y+=12;y+=SettingsNote(draw,new(x,y),inner,error,measure,NativeUiTheme.Danger)+8;
            if(!measure){ImGui.SetCursorScreenPos(new(x,y));if(ClientButton("reconnectAudio","Reconnect audio",170,34))audio.Reconnect();}
            y+=34;
        }
        if(audio.SettingsError is{} settingsError){y+=12;y+=SettingsNote(draw,new(x,y),inner,settingsError,measure,NativeUiTheme.Danger);}
        return y+17-at.Y;
    }
    private float SettingsInterfaceCard(float2 at,float width,bool measure)
    {
        var draw=ImGui.GetWindowDrawList();float inner=width-34,x=at.X+17,y=at.Y+17,heading=NativeUiTheme.TextLine(14);
        if(!measure)TileHeading(draw,new(x,y),inner,"Interface",null,14);
        y+=heading+12;
        bool motion=clientPreferences.Motion,sound=clientPreferences.UiSfx,social=clientPreferences.Social;
        y+=SettingsToggle("setMotion","Animations","Page transitions and the turning pedestal",ref motion,new(x,y),inner,measure);
        if(!measure&&motion!=clientPreferences.Motion)SaveClientPreferences(clientPreferences with{Motion=motion});
        y+=SettingsToggle("setUiSfx","Menu sounds","Clicks and hovers in the menus",ref sound,new(x,y),inner,measure);
        if(!measure&&sound!=clientPreferences.UiSfx)SaveClientPreferences(clientPreferences with{UiSfx=sound});
        y+=SettingsToggle("setSocial","Friends list","The social column on the right",ref social,new(x,y),inner,measure);
        if(!measure&&social!=clientPreferences.Social){SaveClientPreferences(clientPreferences with{Social=social});audio.PlayClient("tick");}
        if(clientPreferenceStore?.Error is {} preferenceError){y+=6;y+=SettingsNote(draw,new(x,y),inner,preferenceError,measure,NativeUiTheme.Danger);}
        if(profile is not null)
        {
            // .card-h.sp "Data": 24px above; then p, the two buttons, p (1em margins).
            y+=24;if(!measure)TileHeading(draw,new(x,y),inner,"Data",null,14);y+=heading;
            y+=12.5f;y+=SettingsNote(draw,new(x,y),inner,"Your Bits, cosmetics, Little Legend levels, Digi-Farm, match history and settings are saved on this computer.",measure);y+=12.5f;
            float buttonHeight=MathF.Round(14*1.2f+16+2);
            if(!measure)
            {
                float resetWidth=NativeUiPaint.TrackedWidth(NativeUiTheme.BodySemiBoldFont,14,"Reset save...",0)+32+2;
                ImGui.SetCursorScreenPos(new(x,y));ImGui.BeginDisabled(!CanResetSave);
                if(ClientButton("resetSave","Reset save...",resetWidth,buttonHeight,ClientButtonKind.Danger)){confirmResetSave=true;resetSaveText.SetValue("".AsSpan());ImGui.OpenPopup("Reset confirmation"u8);}
                ImGui.EndDisabled();
                const string export="EXPORT SAVE BACKUP";float exportWidth=NativeUiPaint.TrackedWidth(NativeUiTheme.DisplayFont,14,export,.08f)+32+2;
                ImGui.SetCursorScreenPos(new(x+resetWidth+4,y));ImGui.BeginDisabled(FileOperationsPending||closeAfterFileOperations);
                if(ClientButton("exportSave",export,exportWidth,buttonHeight,ClientButtonKind.Secondary))
                {
                    try { backupSelection = BackupPicker($"digi-tactics-profile-{DateTime.UtcNow:yyyy-MM-dd}.json"); saveDataMessage = null; }
                    catch (Exception error) { saveDataMessage = "The file chooser could not open: " + error.Message; }
                }
                ImGui.EndDisabled();
            }
            y+=buttonHeight;
            y+=12.5f;y+=SettingsNote(draw,new(x,y),inner,"Includes progress, settings and your music files. Keep this backup private.",measure);
            if(!CanResetSave&&!SaveDataBusy){y+=6;y+=SettingsNote(draw,new(x,y),inner,"Leave your match or lobby and finish any file operation before resetting your save.",measure);}
            if(SaveDataBusy){y+=6;y+=SettingsNote(draw,new(x,y),inner,backupSelection is not null?"Choose where to save the backup...":"Preparing backup...",measure);}
            if(saveDataMessage is not null){y+=6;y+=SettingsNote(draw,new(x,y),inner,saveDataMessage,measure,NativeUiTheme.Text1);}
            // Native save management (import, profile backups) has no web counterpart; it stays one link away.
            y+=8;if(!measure&&SettingsLink("saveData","Manage save / import backup",new(x,y)))NavigateClient("savedata");
            y+=NativeUiTheme.TextLine(12.5f);
            y+=12.5f;
        }
        // With the desktop updater (UPD in RENDER.settings) an About heading shows the installed version.
        UpdateClient?.Refresh();
        if(UpdateClient is not null)
        {
            y+=24-12.5f;if(!measure)TileHeading(draw,new(x,y),inner,"About",null,14);y+=heading;
            y+=12.5f;y+=SettingsNote(draw,new(x,y),inner,InstalledClientVersion,measure);
            y+=4;y+=SettingsNote(draw,new(x,y),inner,ClientUpdateDisplay.Status(true, UpdateClient.Status),measure);
            y+=12.5f;
        }
        return y+17-at.Y;
    }
    // client.js KEYS, verbatim.
    private static readonly (string Key, string Action)[] ClientKeys =
    [
        ("D", "Reroll the shop"), ("E", "Sell the Digimon under the cursor"), ("Space", "Start the fight"), ("T", "Emote wheel"), ("G", "Soundboard"),
        ("C", "Codex"), ("1-8, Q / E", "Scout another Tamer between rounds"), ("R", "Reset the camera"), ("+ / -", "Zoom"), ("Right-click", "Move your Little Legend"),
        ("Esc", "Back / close the top window"), ("Tab", "Move between buttons")
    ];
    /// <summary>.keytable: 12.5px rows (6px 4px cells, c-border-1 rules); keys as kbd chips in a shrink-to-fit first column.</summary>
    private float SettingsKeysCard(float2 at,float width,bool measure)
    {
        var draw=ImGui.GetWindowDrawList();float inner=width-34,x=at.X+17,y=at.Y+17;
        if(!measure)TileHeading(draw,new(x,y),inner,"Keys",null,14);
        y+=NativeUiTheme.TextLine(14)+12;
        float Kbd(string key)=>NativeUiPaint.TrackedWidth(NativeUiTheme.BodyFont,11,key,0)+12+2;
        float CellWidth(string keys)=>keys.Split(", ").Sum(Kbd)+(keys.Split(", ").Length-1)*NativeUiPaint.TrackedWidth(NativeUiTheme.BodyFont,12.5f," ",0);
        float keyColumn=ClientKeys.Max(k=>CellWidth(k.Key))+4+12;
        float lineHeight=NativeUiTheme.TextLine(12.5f),kbdHeight=NativeUiTheme.LineHeight(NativeUiTheme.BodyFont,11)+3;
        foreach(var (keys,action) in ClientKeys)
        {
            var lines=WrapText(NativeUiTheme.BodyFont,12.5f,action,inner-keyColumn-8);
            float rowHeight=12+Math.Max(lines.Count*lineHeight,kbdHeight)+1;
            if(!measure)
            {
                float kx=x+4,middle=y+6+(rowHeight-13)/2;
                foreach(var key in keys.Split(", "))
                {
                    float kw=Kbd(key);var lo=new float2(kx,MathF.Round(middle-kbdHeight/2));var hi=lo+new float2(kw,kbdHeight);
                    draw.AddRectFilled(lo,hi,NativeUiPaint.Fade(new ImColor8(49,66,95)),4);
                    draw.AddRectFilled(lo,hi-new float2(0,2),NativeUiPaint.Fade(new ImColor8(28,39,62)),4,ImDrawFlags.RoundCornersTop);
                    draw.AddRect(lo,hi,NativeUiPaint.Fade(new ImColor8(49,66,95)),4);
                    NativeUiPaint.Tracked(draw,NativeUiTheme.BodyFont,11,lo+new float2(7,1),NativeUiTheme.Text1,key);
                    kx+=kw+NativeUiPaint.TrackedWidth(NativeUiTheme.BodyFont,12.5f," ",0);
                }
                for(int i=0;i<lines.Count;i++)NativeUiPaint.Tracked(draw,NativeUiTheme.BodyFont,12.5f,new(x+keyColumn+4,MathF.Round(y+6+(rowHeight-13-lines.Count*lineHeight)/2+i*lineHeight)),NativeUiTheme.Muted,lines[i]);
                draw.AddLine(new(x,y+rowHeight-.5f),new(x+inner,y+rowHeight-.5f),NativeUiPaint.Fade(new ImColor8(34,48,73)));
            }
            y+=rowHeight;
        }
        return y+17-at.Y;
    }
}
