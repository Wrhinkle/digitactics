using System.Text.Json;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;
using DigiTactics.Presentation;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen : IDisposable
{
    private readonly ContentCatalog catalog;
    private readonly uint? seed;
    private readonly GameAudio audio;
    private readonly NativeProfileStore? profile;
    private FarmProgress? farm;
    private Task<ProfileImport?>? profilePreview;
    private ProfileImport? profileCandidate;
    private string? profileMessage;
    private MatchHistoryRecorder? history;
    private readonly AbilityElements abilityElements = new(Path.Combine(AppContext.BaseDirectory, "content/ability-elements.json"));
    private CombatAudioCues? combatAudio;
    private readonly DisplayText text = new();
    private readonly ArenaEnvironment arenaEnvironment = new();
    private PlanningView planning;
    private BattleView? battle;
    private MatchBattle? watching;
    private int variant, viewed, localSeat;
    private bool duos, showAudio;
    private int? selectedUnit, selectedItem;
    private string status = "Select a unit, then a hex. Select an item, then its holder.";
    private double backgroundTime;
    private int2 size;
    public MatchPlanningSession? Session { get; private set; }
    public Dictionary<string, float2> Buttons { get; } = [];
    public int BattleDrawCalls => battle?.DrawCalls ?? 0;
    public int PlanningDrawCalls => planning.DrawCalls;
    internal IReadOnlySet<int> PlanningRenderedUnits => planning.RenderedUnits;
    internal IReadOnlySet<int>? BattleRenderedBenchUnits => battle?.RenderedBenchUnits;
    internal int ViewedPlayer => viewed;
    internal int ItemShelfDrawCalls => arenaEnvironment.Items.DrawCalls;
    public bool WatchingMirrored => battle?.Mirrored ?? false;
    public int BattleColumns => battle?.Combat.Columns ?? 0;
    public int CompletedRounds { get; private set; }
    private MatchPlayer Human => Session!.State.Players[localSeat >= 0 ? localSeat : viewed];
    private Ruleset Rules => Session?.State.Rules ?? catalog.Variants[variant];
    private int Left => 230;
    private int Top => 118;
    private int Width => Math.Max(300, size.X - 470);
    private int Height => Math.Max(240, size.Y - 383);
    private const ImGuiWindowFlags Fixed = ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings;
    public MatchScreen(ContentCatalog catalog, GameAudio audio, uint? seed = null, bool duos = false, NativeProfileStore? profile = null) { planning=new(arenaEnvironment); this.catalog = catalog; this.audio = audio; this.seed = seed; this.duos = duos; this.profile = profile; farm = profile is null ? null : new(profile); InitializeSetSelection(duos); }
    private void Window(string name, float x, float y, float width, float height, ImGuiWindowFlags flags = 0)
    {
        ImGui.SetNextWindowPos(new(x, y)); ImGui.SetNextWindowSize(new(width, height)); ImGui.Begin(name, Fixed | flags);
    }
    private bool Button(string key, string label, float width = 0, float height = 0, bool active = false)
    {
        if (active) ImGui.PushStyleColor(ImGuiCol.Button, new ImColor8(32, 131, 113));
        bool pressed = ImGui.Button(label + "##" + key, new float2(width, height));
        if(ImGui.IsItemFocused())KeyboardFocusedButton=key;
        if (active) ImGui.PopStyleColor();
        var min = ImGui.GetItemRectMin(); var max = ImGui.GetItemRectMax(); Buttons[key] = new((min.X + max.X) / 2, (min.Y + max.Y) / 2);
        ClientButtonSound(key, pressed);
        return pressed;
    }
    private string Name(string id) => text.Get(Rules.Id, id).Name;
    private void Tooltip(string id)
    {
        if (!ImGui.IsItemHovered()) return;
        if (Rules.Items.Any(item => item.Id == id)) { ItemTooltip(id); return; }
        var entry = text.Get(Rules.Id, id); ImGui.SetNextWindowSize(new(340, 0)); ImGui.BeginTooltip(); ImGui.Text(entry.Name); UiText.Wrapped(entry.Description); ImGui.EndTooltip();
    }
    private string UnitName(PlanningUnit unit) => Rules.Lines.Concat(Rules.ExtraLines).Single(l => l.Id == unit.Line).Forms[unit.Stage];
    private MatchActionResult? Act(string action, object? args = null)
    {
        if(viewed!=localSeat&&MatchKeyboard.RequiresOwnBoard(action)){status="Return to your board first (Space).";return null;}
        var arguments = JsonSerializer.SerializeToElement(args ?? new { });
        if (networkRoom is not null) { SendNetworkAction(action, arguments); return null; }
        var result = Session!.Actions.Apply(localSeat, action, arguments);
        history?.Observe();
        ActionFeedback(action, result); return result;
    }
    private void ActionFeedback(string action, MatchActionResult? result)
    {
        string? sound = result switch
        {
            MatchActionResult.Flag { Value: false } when action != "lock" => "error",
            MatchActionResult.Code when action == "offer" => "pick",
            MatchActionResult.Code code when action == "equip" && Rules.Items.Any(item => item.Id == code.Value) => "equip",
            MatchActionResult.Evolutions { Units.Count: > 0 } => "digivolve",
            MatchActionResult.Evolutions => "buy",
            MatchActionResult.Orb => "orb",
            MatchActionResult.Transfer => "pick",
            MatchActionResult.Flag { Value: true } => action switch
            {
                "sell" => "sell", "move" => "place", "reroll" => "reroll", "buyXp" => "levelup",
                "offer" or "carousel" or "wisp" => "pick", "equip" or "swapItems" => "equip", _ => null
            },
            _ => null
        };
        if (sound is not null) audio.Play(sound);
        status = result switch
        {
            MatchActionResult.Flag { Value: true } => "Done.",
            MatchActionResult.Flag => "That action is not available right now.",
            MatchActionResult.Evolutions { Units.Count: > 0 } e => "Digivolved: " + string.Join(", ", e.Units.Select(UnitName)),
            MatchActionResult.Evolutions => "Unit purchased.",
            MatchActionResult.Transfer e => e.Kind == "unit" ? "Unit sent to your partner." : "Item sent to your partner.",
            MatchActionResult.Orb => "Loot collected.",
            MatchActionResult.Code e => e.Value switch
            {
                "locked" => "Board units cannot be changed during combat.",
                "cap" => "Your board is full. Swap a unit or increase your level.",
                "gold" => "Not enough gold.",
                "phase" => "This action is available during planning.",
                "full" => "This unit cannot hold another item.",
                "hasTrait" => "This unit already has that trait.",
                "none" => "No valid item or target selected.",
                _ => Name(e.Value)
            },
            _ => "No action taken."
        };
    }
    private static object Spot(UnitSpot spot) => spot.IsBoard ? new { type = "board", c = spot.Column, r = spot.Row } : (object)new { type = "bench", i = spot.Index };
    private void Select(UnitSpot spot)
    {
        if(viewed!=localSeat){status="Return to your board first (Space).";return;}
        var unit = Human.Roster.At(spot);
        if (selectedItem is { } item && unit is not null) { Act("equip", new { idx = item, uid = unit.Id }); selectedItem = null; return; }
        if (selectedUnit is { } id && Human.Roster.Entries.FirstOrDefault(e => e.Unit.Id == id) is { } from)
        {
            if (from.Spot != spot) Act("move", new { from = Spot(from.Spot), to = Spot(spot) });
            selectedUnit = null; return;
        }
        selectedUnit = unit?.Id;
    }
    public void Update(double delta, double? wallDelta = null)
    {
        UpdateClientPreferences();
        if (!IsPaused) boardSceneTime+=Math.Clamp(wallDelta??delta,0,.1);
        UpdateSteamLobby();
        UpdateNetworkMatch();
        UpdateNetworkCountdown();
        UpdatePostgameAward();
        UpdateSaveData();
        UpdateMysteryLoot();
        UpdateEliminationCinematic(wallDelta ?? delta);
        if (farm is not null) farm.ResidentPosition = id => showFarm && training is null ? farmView?.Position(id) : null;
        if (training is null) farm?.Update(delta, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        else UpdateTraining(delta, wallDelta ?? delta);
        UpdateFarmView(delta);
        if (profilePreview is { IsCompleted: true })
        {
            try { profileCandidate = profilePreview.GetAwaiter().GetResult(); profileMessage = null; }
            catch (Exception error) { profileMessage = "Could not read this backup: " + error.Message; }
            profilePreview = null;
        }
        if (Session is null || IsPaused) return;
        bool wasResult = Session.Rounds.Phase == MatchPhase.Result;
        if (Session.Rounds.Phase == MatchPhase.Combat)
        {
            battle?.Advance(delta, ReplaySpeed); backgroundTime += Math.Clamp(delta, 0, .1) * ReplaySpeed;
            combatAudio?.Update(audio.Play);
            while (backgroundTime >= CombatSimulation.Tick)
            {
                foreach (var match in Session.Battles) if (match != watching) match.Combat.Step();
                backgroundTime -= CombatSimulation.Tick;
            }
            if (Session.Battles.All(m => m.Combat.Done)) { if (networkRoom is null) Finish(); else NetworkReplayDone(); }
        }
        else if (Session.Rounds.Phase == MatchPhase.Result) battle?.Advance(delta, ReplaySpeed);
        if (wasResult) UpdateResultCountdown(wallDelta ?? delta);
        else UpdatePlanningCountdown(delta);
        planning.Update(Rules, Session.State.Players[viewed], delta);
        UpdateBattleBench(delta);
        UpdateAiEmotes();
        UpdateArenaCompanion(delta);
        UpdateLootCollection();
    }
    private void Watch(int player)
    {
        cameraDrag = null;
        CancelArenaDrag();
        selectedUnit=selectedItem=null;hoveredOwnedUnit=null;
        viewed = player;
        if (Session is null || Session.Rounds.Phase == MatchPhase.Planning) return;
        var next = Session.Battles.FirstOrDefault(m => m.A == player || !m.Ghost && m.B == player ||
            m.TeamA is { } ta && Session.State.Teams[ta].Members.Any(p => p.Id == player) ||
            !m.Ghost && m.TeamB is { } tb && Session.State.Teams[tb].Members.Any(p => p.Id == player)) ?? Session.Battles.FirstOrDefault();
        bool mirror = next is not null && (next.B == player || next.TeamB is { } team && Session.State.Teams[team].Members.Any(p => p.Id == player));
        if (next == watching && battle?.Mirrored == mirror) { UpdateBattleBench(0); return; }
        battle?.Dispose(); battle = null; watching = next;
        combatAudio = next is null ? null : new(next.Combat, Rules.Id, abilityElements);
        if (next is not null)
        {
            battle = new(Rules, next.Combat, mirror,arenaEnvironment);
            UpdateBattleBench(0);
        }
    }
    private void UpdateBattleBench(double delta)
    {
        if (battle is null || Session is null) return;
        var player = Session.State.Players[viewed];
        var partner = player.Team is { } team ? Session.State.Teams[team].Members.FirstOrDefault(p => p.Id != player.Id) : null;
        battle.UpdateBench(player, partner, delta);
    }
    private void Fight()
    {
        if (networkRoom is not null) { SendNetworkAction("ready"); return; }
        Session!.StartCombat(); CombatStarted();
    }
    private void CombatStarted()
    {
        selectedUnit = null; selectedItem = null; backgroundTime = 0;
        status = "Battle in progress.";
        Watch(WatchingAfterElimination ? LivingWatchSeat() : localSeat >= 0 ? localSeat : viewed);
        history?.Capture();
        if (Session!.Battles.Count > 0) { audio.Play("fight"); audio.Music("combat"); }
        if (Session.Battles.Count == 0 && networkRoom is null) Finish();
    }
    private void Finish()
    {
        if (networkRoom is not null) { foreach (var match in Session!.Battles) while (!match.Combat.Done) match.Combat.Step(); NetworkReplayDone(); return; }
        history?.Capture();
        Session!.FinishCombat(); RoundFinished();
    }
    private void RoundFinished()
    {
        QueueEliminations();
        resultCountdown = 2.2;
        ScheduleAiEmotes();
        CompletedRounds++; status = "Round complete.";
        if (networkRoom is null) history?.Complete(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        if (history?.Error is { } saveError) { status = saveError; profileMessage = saveError; }
        AwardMysteryRound();
        var own = PlayerRoundResult.Find(Session!.Rounds.Results, Human);
        audio.Play(IsSpectating ? "round" : Session.State.Over ? Human.Placement == 1 ? "champion" : "gameover" : own?.Sound ?? "round");
        audio.Music(Session.State.Over ? null : "planning");
    }
    private void Next()
    {
        if (networkRoom is not null) return;
        Session!.Advance(); RoundAdvanced();
    }
    private void RoundAdvanced()
    {
        resultCountdown = null;
        battle?.Dispose(); battle = null; watching = null; combatAudio = null; viewed = WatchingAfterElimination ? LivingWatchSeat() : localSeat >= 0 ? localSeat : viewed;
        selectedUnit = null; selectedItem = null;
        audio.Play("round");
        status = WatchingAfterElimination ? $"Placement #{Human.Placement}. Watching the remaining teams." : "Select a unit, then a hex. Select an item, then its holder.";
    }
    public void Draw(int2 windowSize)
    {
        BeginClientMenuFrame();
        size = ClientContentSize(windowSize); Buttons.Clear(); emoteArtwork.BeginFrame();
        KeyboardFocusedButton=null;
        try
        {
        if((!showCollection || evolutionLegend is null) && evolutionPreview is not null) { ClearEvolutionPreview(); }
        portraits.BeginFrame(); itemArtwork.BeginFrame(); CarouselCardsDrawn = 0; InventoryArtworkDrawCalls = 0; ItemTooltipRecipes = 0; DetailedUnit = null; DetailedItems = []; ItemDropFeedback = null;
        hoveredOwnedUnit = null; sellDropHovered = partnerDropHovered = false;
        itemDropSlot = -1;
        CodexShortcuts();
        OverlayShortcuts();
        ReserveMatchSpace();
        if(codexFull){DrawCodexFull();return;}
        if (Session is null) { CancelArenaDrag(); cameraDrag = null; }
        if(showSettings && ClientHomeVisible) {ClientSettingsPanel();return;}
        if(showSaveData) {SaveDataPanel();return;}
        if(showProfile) {ProfilePanel();DrawEliminationCinematic();return;}
        if (ClientHomeVisible)
        {
            if (showMultiplayer) { MultiplayerMenu(); PlayersPanel(); }
            else if (showFarm) FarmPanel();
            else { Menu(); if (showCollection) Collection(); MusicLibraryPanel(); }
            if(Session is not null)DrawEliminationCinematic();
            return;
        }
        if (showGameMenu) { PausePanel(); if(Session is not null)DrawEliminationCinematic(); return; }
        if (DrawPostgame()) { if(Session is not null)DrawEliminationCinematic(); return; }
        Header(); if (Session is null) return;
        if (showGameMenu) { PausePanel(); if(Session is not null)DrawEliminationCinematic(); return; }
        Roster(); Sidebar();
        if (battle is null) Board();
        else
        {
            Window("Arena", Left, Top, Width, Height, ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
            ArenaPointer(); ImGui.End();
            battle.Labels(Width, Height, size.Y, Left, size.Y - Top - Height);
        }
        ResultBanner();
        CompanionClickMark();
        ArenaEmotes();
        Choices(); Soundboard(); EmotePanel(); PlayersPanel();
        if (showAudio)
        {
            Window("Sound settings", size.X / 2 - 210, Top + 20, 420, audio.MusicController is null ? 250 : 390);
            AudioControls();
            if (Button("closeAudio", "Close")) showAudio = false;
            ImGui.End();
        }
        MusicLibraryPanel();DrawCodexSide();
        if(!showMusicLibrary && !EliminationCinematicActive && !CodexVisible && !codexConsumedShortcut) MatchShortcuts();
        DrawEliminationCinematic();
        }
        finally { DrawClientSocial(); DrawMysteryLootNotice(); keyboardPopupLastFrame=KeyboardPopup; }
    }
    private void Menu()=>HomePanel();
    private void Header()
    {
        var session = Session!;
        Window("Match", 12, 12, size.X - 24, 96, ImGuiWindowFlags.NoTitleBar);
        ImGui.Text($"{MatchSetTitle}  /  {MatchPaceTitle}  /  {(session.State.Duos ? "Duos" : "Solo")}     Round {session.Rounds.Position.Label}  -  {session.Rounds.RoundType.ToUpperInvariant()}");
        ImGui.Text(WatchingAfterElimination ? $"Placement #{Human.Placement} - spectating {session.State.Players[viewed].Name}" : IsSpectating ? $"Spectating {Human.Name}" : $"{Human.Health} HP     {Human.Economy.Gold} gold     Level {Human.Economy.Level}     Streak {Human.Economy.Streak}");
        if (Human.Alive) IncomeDisplay();
        ImGui.SameLine(); if (Button("mute", audio.Muted ? "Unmute" : "Mute")) audio.Muted = !audio.Muted;
        ImGui.SameLine(); if (Button("audioSettings", "Sound settings")) showAudio = !showAudio;
        ImGui.SameLine(); if (Button("soundboard", "Soundboard")) { showSoundboard = !showSoundboard; if (showSoundboard) showEmotes = false; }
        ImGui.SameLine(); if (Button("emotes", "Emotes")) { showEmotes = !showEmotes; if (showEmotes) showSoundboard = false; }
        if (networkRoom is not null) { ImGui.SameLine(); if (Button("players", "Players")) showPlayers = !showPlayers; }
        if (session.State.Over)
        {
            ImGui.Text(CanWatchAfterElimination ? $"Eliminated - placement #{Human.Placement}" : IsSpectating ? "Match complete" : $"Match complete - placement #{Human.Placement}"); ImGui.SameLine();
            if (Button("menu", "Main menu")) LeaveMatch();
            if (Session is not null && CanWatchAfterElimination)
            {
                ImGui.SameLine(); if (Button("spectateRemaining", "Spectate remaining teams")) ContinueAfterElimination();
            }
        }
        else if (WatchingAfterElimination && session.Rounds.Phase == MatchPhase.Planning) ImGui.Text($"Next battle in {Math.Ceiling(PlanningRemaining)}s");
        else if (IsSpectating) ImGui.Text($"{session.Rounds.Phase} - watching the shared match");
        else if (session.Rounds.Phase == MatchPhase.Planning) { if (networkRoom is not null && session.Rounds.RoundType == "carousel") ImGui.Text("Waiting for carousel picks..."u8); else if (Button("fight", session.Rounds.RoundType == "carousel" ? "Finish carousel" : "Ready for battle")) Fight(); }
        else if (session.Rounds.Phase == MatchPhase.Result) { if (networkRoom is not null) ImGui.Text("Next round starts shortly..."u8); else if (Button("next", "Next round")) Next(); }
        else { if (Button("finish", networkRoom is null ? "Skip to results" : "Finish my replay")) Finish(); ImGui.SameLine(); ImGui.Text($"{battle?.Combat.Time ?? 0:F1}s"); }
        if (Session is null) { ImGui.End(); return; }
        ImGui.SameLine(); if (Button("gameMenu", "Menu")) OpenGameMenu();
        ImGui.SameLine(); if(Button("codex","Codex (C)"))ToggleCodex();
        PlaybackSpeedControl();
        PlanningTimerControls();
        if (networkRoom is null && !session.State.Over) { ImGui.SameLine(); if (Button("pause", "Pause")) PauseLocal(); }
        if (networkRoom is not null) { ImGui.SameLine(); NetworkSeatControls(); ImGui.SameLine(); if (Button("leaveOnline", "Leave match")) { OpenGameMenu(); confirmLeave = true; } }
        ImGui.End();
    }
    private void Board()
    {
        Window("Arena", Left, Top, Width, Height, ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
        ArenaPointer();
        ImGui.Text(viewed == localSeat ? "Drag units to place. Wheel: zoom. Middle-drag: pan. Right-click: walk/collect loot. Right-drag: orbit. R: reset." : $"Scouting {Session!.State.Players[viewed].Name}");
        for (int row = 4; row < 8; row++) for (int column = 0; column < 7; column++)
        {
            var point = planning.Cell(column, row, Width, Height);
            Buttons[$"hex/{column}/{row}"] = new(Left + point.X, Top + point.Y);
        }
        for (int i = 0; i < 9; i++)
        {
            var point = planning.Bench(i, Width, Height);
            Buttons[$"arenaBench/{i}"] = new(Left + point.X, Top + point.Y);
        }
        ImGui.End();
    }
    private void AudioControls()
    {
        float effects = audio.EffectsVolume, music = audio.MusicVolume;
        if (ImGui.SliderFloat("Effects volume"u8, ref effects, 0, 1)) audio.EffectsVolume = effects;
        if (ClientMenuActive && ImGui.IsItemDeactivatedAfterEdit()) audio.PlayClient("buy", false);
        if (ImGui.SliderFloat("Music volume"u8, ref music, 0, 1)) audio.MusicVolume = music;
        float board = audio.BoardVolume;
        if (ImGui.SliderFloat("Soundboard volume"u8, ref board, 0, 1)) audio.BoardVolume = board;
        bool muteOthers = audio.MuteOthers;
        if (ImGui.Checkbox("Mute other players' soundboard"u8, ref muteOthers)) audio.MuteOthers = muteOthers;
        bool musicOff = audio.MusicOff, muted = audio.Muted;
        if (ImGui.Checkbox("Music off"u8, ref musicOff)) audio.MusicOff = musicOff;
        ImGui.SameLine(); if (ImGui.Checkbox("Mute all"u8, ref muted)) audio.Muted = muted;
        if (audio.Error is { } error) UiText.Wrapped(error);
        if (Button("reconnectAudio", "Reconnect audio")) audio.Reconnect();
        if (audio.MusicController is { } controller)
        {
            if(Button("musicLibrary","Manage music...")) showMusicLibrary=true;
            bool own=audio.MusicSource=="own";
            if (ImGui.Checkbox("Use my music"u8, ref own)) audio.MusicSource=own?"own":"builtin";
            if(controller.Library is { } library)
            {
                UiText.Wrapped($"{library.Tracks.Count} saved music tracks");
                if(controller.CurrentTrackId is { } id && library.Tracks.FirstOrDefault(t=>t.Id==id) is { } track) UiText.Wrapped(track.Name);
                if(own && library.Tracks.Count>0 && Button("nextMusic", "Next track")) controller.Next();
            }
            if(controller.Loading) ImGui.Text("Loading music..."u8);
            if(controller.Error is { } musicError) UiText.Wrapped(musicError);
        }
        if (audio.SettingsError is { } settingsError) UiText.Wrapped(settingsError);
    }
    private void ProfileControls()
    {
        if (profile is null) return;
        static string Number(long? value) => value?.ToString("N0") ?? "Unavailable";
        var saved = profile.Progress;
        ImGui.Text($"{saved.Name}  |  {Number(saved.Bits)} Bits");
        ImGui.Text($"Saved record: {Number(saved.Games)} games, {Number(saved.Wins)} wins, {Number(saved.TopFour)} top-four finishes");
        if (profile.Error is { } error) UiText.Wrapped(error);
        if (profile.CanImport)
        {
            if (profilePreview is not null) ImGui.Text("Reading backup..."u8);
            else if (profileCandidate is { } candidate)
            {
                UiText.Wrapped($"Import {candidate.Progress.Name}'s saved profile? {candidate.Summary.StorageEntries} saved entries and {candidate.Summary.MusicTracks} music files will be preserved.");
                UiText.Wrapped("Your progress, collection, settings and music files will be imported. Your original backup will be preserved."u8);
                if (Button("importProfile", "Import profile"))
                {
                    try { profile.Import(candidate); audio.MusicController?.Reload(); profileCandidate = null; profileMessage = "Profile imported. Your original backup is preserved."; }
                    catch (Exception failure) { profileMessage = "Profile was not imported: " + failure.Message; }
                }
                ImGui.SameLine(); if (Button("cancelProfile", "Cancel")) profileCandidate = null;
            }
            else if (Button("chooseProfile", "Import save backup..."))
            {
                try
                {
                    var selection = ProfileFilePicker.Open();
                    profilePreview = Task.Run(async () => { string? path = await selection; return path is null ? null : new ProfileImport(path); });
                }
                catch (Exception failure) { profileMessage = failure.Message; }
            }
        }
        if (profileMessage is not null) UiText.Wrapped(profileMessage);
        if (Button("saveData", "Save data...")) OpenSaveData();
        ImGui.Separator();
    }
    private void Sidebar()
    {
        Window("Team", 12, Top, 208, Height);
        PartnerDropZone();
        var player = Session!.State.Players[viewed];
        ImGui.Text($"{player.Name}  {player.Roster.BoardCount}/{new MatchPlacement(Session.State).Capacity(player, Session.Rounds.Position.Stage)} deployed");
        TraitSidebar(player);
        ImGui.SeparatorText("Selected"u8);
        var selected = Human.Roster.Entries.FirstOrDefault(e => e.Unit.Id == selectedUnit);
        if (selected is not null)
        {
            UnitDetails(selected.Unit);

            if (Button("sell", "Sell unit")) { Act("sell", new { uid = selected.Unit.Id }); selectedUnit = null; }
            if (Session.State.Duos && Button("sendUnit", "Send to partner")) { Act("portal", new { uid = selected.Unit.Id }); selectedUnit = null; }
        }
        if (!IsSpectating && Human.Wisp is { } wisp && Button("wisp", $"{Name(wisp.Id)} ({wisp.Cost}g)")) Act("wisp");
        DamageChart();
        AugmentSidebar(player);
        ModeBonusSidebar(player);
        ImGui.End();
        Window("Standings", size.X - 228, Top, 216, Height);
        EnsureSoundboard(); var soundLabels = soundboard.Labels(Environment.TickCount64);
        foreach (var seat in ScoutOrder())
        {
            var p=Session.State.Players[seat.Id];
            if (Button("view/" + p.Id, $"{p.Name}  {(p.Alive ? p.Health + " HP" : "#" + p.Placement)}", 192, 28)) Watch(p.Id);
            if (soundLabels.FirstOrDefault(label => label.Seat == p.Id) is { } clip) UiText.Wrapped("Playing: " + DigiTactics.Storage.CosmeticCollection.Item("sound", clip.Id)!.Name);
            EmoteRow(p.Id);
        }
        if (Session.State.Duos && !IsSpectating) UiText.Wrapped("Your partner: " + Session.Units.Portal.Partner(Human)?.Name);
        if (!IsSpectating) foreach (var orb in Human.Orbs.ToArray()) if (Button("orb/" + orb.Id, $"Collect {orb.Kind} loot")) Act("orb", new { id = orb.Id });
        if (history?.Award is { } award)
        {
            ImGui.Separator(); ImGui.Text($"Earned {award.Total} Bits");
            foreach (var line in award.Lines) UiText.Wrapped($"{line.Label}: +{line.N}");
            if (history.LegendAward is { } legend) UiText.Wrapped($"{legend.Legend}: +{legend.Data} Digi-Data (level {legend.Level})");
        }
        if (history?.Error is not null && Button("retryHistory", "Retry saving progress"))
        {
            bool saved = history.Complete(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            status = saved ? "Match progress saved." : history.Error!;
            profileMessage = saved ? null : history.Error;
        }
        ImGui.Separator(); UiText.Wrapped(status); ImGui.End();
    }
    private void Roster()
    {
        Window("Shop and bench", 12, size.Y - 255, size.X - 24, 243, ImGuiWindowFlags.NoTitleBar);
        if (IsSpectating) { UiText.Wrapped("Spectating. Take an available seat to control a team."u8); ImGui.End(); return; }
        if (SellDropZone()) { ImGui.End(); return; }
        ImGui.BeginDisabled(Session!.State.Over || !Human.Alive);
        ShopControls();
        float card = (size.X - 64f) / Human.Shop.Slots.Count;
        for (int i = 0; i < Human.Shop.Slots.Count; i++)
        {
            if (i > 0) ImGui.SameLine();
            ShopCard(i,card);
        }
        for (int i = 0; i < 9; i++)
        {
            if (i > 0) ImGui.SameLine(); var unit = Human.Roster.Bench[i];
            if (Button("bench/" + i, unit is null ? "-" : UnitName(unit) + "\n" + new string('*', unit.Stage + 1), (size.X - 96f) / 9, 40, selectedUnit is not null && unit?.Id == selectedUnit)) Select(UnitSpot.Bench(i));
            if (unit is not null) { if (viewed==localSeat&&ImGui.IsItemHovered()) hoveredOwnedUnit = unit.Id; UnitTooltip(unit); }
        }
        for (int i = 0; i < Human.Items.Count; i++)
        {
            if (i % 6 != 0) ImGui.SameLine();
            if (ItemButton(i, (size.X - 78f) / 6) && !itemDragMoved)
            {
                if (selectedItem is { } old && old != i) { Act("swapItems", new { a = old, b = i }); selectedItem = null; }
                else { selectedItem = selectedItem == i ? null : i; selectedUnit = null; status = "Select a holder to equip, or another item to swap positions."; }
            }
            ItemDragSource(i);
            if (!itemDragMoved) Tooltip(Human.Items[i]);
        }
        if (selectedItem is { } item && Session.State.Duos && Button("sendItem", "Send selected item")) { Act("portal", new { item }); selectedItem = null; }
        ImGui.EndDisabled(); ImGui.End();
    }
    public void Render(int2 framebuffer)
    {
        if (ClientHomeVisible && showFarm && training is null && farmViewError is null && size.X > 0 && size.Y > 0)
        {
            float scaleX = (float)framebuffer.X / Math.Max(1,socialWindowSize.X), scaleY = (float)framebuffer.Y / size.Y;
            if (farmView is not null) farmView.Selected = farmPet;
            farmView?.Draw((int)(12 * scaleX), (int)((size.Y - 70 - FarmSceneHeight) * scaleY), (int)(FarmSceneWidth * scaleX), (int)(FarmSceneHeight * scaleY));
            return;
        }
        if (Session is null || ClientHomeVisible || framebuffer.X <= 0 || framebuffer.Y <= 0 || size.X <= 0 || size.Y <= 0) return;
        // GLFW logical and framebuffer sizes can differ on high-DPI displays.
        float sx = (float)framebuffer.X / size.X, sy = (float)framebuffer.Y / size.Y;
        int x = (int)(Left * sx), y = (int)((size.Y - Top - Height) * sy), w = (int)(Width * sx), h = (int)(Height * sy);
        SyncBoardTheme();
        arenaEnvironment.Items.Ruleset = Rules.Id;
        arenaEnvironment.Items.Items = Session.State.Players[viewed].Items;
        arenaCompanion?.BeginFrame(); opponentCompanion?.BeginFrame();
        var sceneCamera = arenaEnvironment.Camera.Matrix(w, h, battle?.Combat.Columns ?? 7);
        var sceneEye = arenaEnvironment.Camera.Eye(w, h, battle?.Combat.Columns ?? 7);
        arenaCompanion?.PrepareScene(sceneCamera, sceneEye); opponentCompanion?.PrepareScene(sceneCamera, sceneEye);
        lootOrbs.Sync(Human.Orbs,boardSceneTime,!IsSpectating && viewed == localSeat);
        arenaEnvironment.Actors = (renderer,camera,transparent) => { DrawCompanions(renderer,camera,transparent); lootOrbs.Draw(renderer,camera,transparent); };
        if (battle is not null) battle.Draw(w, h, x, y); else planning.Draw(Rules, Session.State.Players[viewed], x, y, w, h);
    }
    public void Dispose() { DisposeHome(); ClearEliminationCinematics(); DisposeCollectionPreview(); postgameArtwork.Dispose(); ClearEvolutionPreview(); lootOrbs.Dispose(); itemArtwork.Dispose(); portraits.Dispose(); DetachNetworkMatch(); opponentCompanion?.Dispose(); arenaCompanion?.Dispose(); emoteArtwork.Dispose(); ClearLobbyAvatars(); DisposeSteamConnection(); farmMoodTexture?.Dispose(); farmSymbolTexture?.Dispose(); farmView?.Dispose(); battle?.Dispose(); planning.Dispose(); arenaEnvironment.Dispose(); }
}
