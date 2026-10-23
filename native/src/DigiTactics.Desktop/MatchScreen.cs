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
    private bool duos, showAudio, audioToggleHovered;
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
    // style.css --sideL/--sideR, the #app grid rows and the #shopbar card row.
    private int Left => HudNarrow ? 178 : (int)Math.Clamp(size.X * .155f, 208, 258);
    private int RightRail => HudNarrow ? 190 : (int)Math.Clamp(size.X * .17f, 224, 276);
    private int Top => CurrentTopBar.Height;
    private int ShopHeight => HudCardHeight + (HudNarrow ? 13 : 17);
    private int Width => Math.Max(300, size.X - Left - RightRail);
    private int Height => Math.Max(120, size.Y - Top - ShopHeight);
    // The original fits its canvas while the shop bar holds only the economy column (measured 87 px, 77 px compact,
    // plus bar padding), so the canvas overhangs the arena by the later card height difference.
    private readonly ArenaFrameTarget arenaFrame = new();
    private int ArenaCanvasOverhang => Math.Max(0, ShopHeight - ((HudCompact ? 77 : 87) + (HudNarrow ? 13 : 17)));
    private const ImGuiWindowFlags Fixed = ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings;
    public MatchScreen(ContentCatalog catalog, GameAudio audio, uint? seed = null, bool duos = false, NativeProfileStore? profile = null) { planning=new(arenaEnvironment); this.catalog = catalog; this.audio = audio; this.seed = seed; this.duos = duos; this.profile = profile; farm = profile is null ? null : new(profile); InitializeSetSelection(duos); }
    private void Window(string name, float x, float y, float width, float height, ImGuiWindowFlags flags = 0)
    {
        if(drawingPauseBackdrop)flags|=ImGuiWindowFlags.NoInputs|ImGuiWindowFlags.NoBringToFrontOnFocus;
        // The narrow-layout friends drawer floats above client modules while
        // those modules can still accept keyboard focus and input beside it.
        if(ClientHomeVisible && socialDrawer && name!="Friends")flags|=ImGuiWindowFlags.NoBringToFrontOnFocus;
        ImGui.SetNextWindowPos(new(x, y)); ImGui.SetNextWindowSize(new(width, height)); ImGui.Begin(name, Fixed | flags);
    }
    private bool Button(string key, string label, float width = 0, float height = 0, bool active = false)
    {
        if (active)
        {
            ImGui.PushStyleColor(ImGuiCol.Button,new ImColor8(30,35,40));
            ImGui.PushStyleColor(ImGuiCol.Border,NativeUiTheme.Gold);
            ImGui.PushStyleColor(ImGuiCol.Text,NativeUiTheme.GoldBright);
        }
        bool pressed = ImGui.Button(label + "##" + key, new float2(width, height));
        if(key=="emotes")emotesToggleHovered=ImGui.IsItemHovered();
        if(key=="soundboard")soundboardToggleHovered=ImGui.IsItemHovered();
        if(ImGui.IsItemFocused())KeyboardFocusedButton=key;
        if (active) ImGui.PopStyleColor(3);
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
        UpdateMatchLoading(wallDelta ?? delta);
        if (!IsPaused) boardSceneTime+=Math.Clamp(wallDelta??delta,0,.1);
        UpdateSteamLobby();
        UpdateNetworkMatch();
        UpdateNetworkCountdown();
        UpdatePostgameAward();
        UpdateSaveData();
        UpdateMusicSelection();
        UpdateMysteryLoot();
        UpdateLootAwardNotice(wallDelta??delta);
        castFeed?.Advance(wallDelta??delta);
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
            SyncCutIns(); battle?.Advance(delta, ReplaySpeed); backgroundTime += Math.Clamp(delta, 0, .1) * ReplaySpeed;
            castFeed?.Observe(ReplaySpeed);
            combatAudio?.Update(audio.Play);
            while (backgroundTime >= CombatSimulation.Tick)
            {
                foreach (var match in Session.Battles) if (match != watching) match.Combat.Step();
                backgroundTime -= CombatSimulation.Tick;
            }
            if (Session.Battles.All(m => m.Combat.Done)) { if (networkRoom is null) Finish(); else NetworkReplayDone(); }
        }
        else if (Session.Rounds.Phase == MatchPhase.Result) {SyncCutIns();battle?.Advance(delta, ReplaySpeed);castFeed?.Observe(ReplaySpeed);}
        if (wasResult) UpdateResultCountdown(wallDelta ?? delta);
        else UpdatePlanningCountdown(delta);
        planning.Update(Rules, Session.State.Players[viewed], delta, PlanningPreviewSide());
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
        castFeed=next is null?null:new(next.Combat,Rules.Id,abilityElements,mirror);
        if (next is not null)
        {
            battle = new(Rules, next.Combat, mirror,arenaEnvironment);
            battle.CastColor = unit => CombatCastFeed.ElementColor(abilityElements.Get(Rules.Id, unit.Line.Id, unit.Stage));
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
    private void Fight(bool autoResolveOffers=false)
    {
        if(Session is null||Session.State.Over||Session.Rounds.Phase!=MatchPhase.Planning)return;
        if(!autoResolveOffers&&Session.Rounds.RoundType=="carousel")return;
        if(!autoResolveOffers&&Human.Offer is not null){peekChoices=false;status="Make your pick first.";return;}
        if (networkRoom is not null) { SendNetworkAction("ready"); return; }
        Session.StartCombat(); CombatStarted();
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
    private void SkipCombat(){Finish();castFeed?.Clear();}
    private void RoundFinished()
    {
        matchHud.Capture(Session!);
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
        castFeed=null;
        battle?.Dispose(); battle = null; watching = null; combatAudio = null; viewed = WatchingAfterElimination ? LivingWatchSeat() : localSeat >= 0 ? localSeat : viewed;
        selectedUnit = null; selectedItem = null;
        audio.Play("round");
        status = WatchingAfterElimination ? $"Placement #{Human.Placement}. Watching the remaining teams." : "Select a unit, then a hex. Select an item, then its holder.";
    }
    public void Draw(int2 windowSize)
    {
        // Previous frame draw data has consumed these preview textures.
        ReleaseRetiredCollectionPreviews();
        BeginClientMenuFrame();
        PreparePostgameClient();
        size = ClientContentSize(windowSize); Buttons.Clear(); emoteArtwork.BeginFrame();
        UpdateTopBarLayout();
        arenaEnvironment.Camera.CanvasStretch = (double)(Height + ArenaCanvasOverhang) / Height;
        CastFeedDraws=0;
        KeyboardFocusedButton=null;
        try
        {
        if((!showCollection || evolutionLegend is null) && evolutionPreview is not null) { ClearEvolutionPreview(); }
        portraits.BeginFrame(); itemArtwork.BeginFrame(); CarouselCardsDrawn = 0; InventoryArtworkDrawCalls = 0; ItemTooltipRecipes = 0; DetailedUnit = null; DetailedItems = []; ItemDropFeedback = null;
        hoveredOwnedUnit = null; sellDropHovered = partnerDropHovered = false;
        itemDropSlot = -1;
        // Entrance cards own the presentation until their transition ends.
        // ImGui modal windows otherwise render above the entrance window and
        // allow picks against a carousel that should still be hidden.
        if (MatchEntranceActive) { ReserveMatchSpace(); return; }
        CodexShortcuts();
        OverlayShortcuts();
        ReserveMatchSpace();
        TickHomeHero();
        if(codexFull){DrawCodexFull();return;}
        if (Session is null) { CancelArenaDrag(); cameraDrag = null; }
        if(showSettings && ClientHomeVisible) {ClientSettingsPanel();return;}
        if(showHomePlay && ClientHomeVisible) {HomePlayPanel();return;}
        if(showSaveData) {SaveDataPanel();return;}
        if(showProfile) {ProfilePanel();DrawEliminationCinematic();return;}
        if (ClientHomeVisible)
        {
            if (showMultiplayer) { MultiplayerMenu(); PlayersPanel(); }
            else if (showFarm) FarmPanel();
            // Collection, Loot and Store cover Home entirely; Home only keeps drawing under them for a retained result.
            else { if (!showCollection || PostgamePending) Menu(); if (showCollection) Collection(); MusicLibraryPanel(); }
            if(Session is not null)DrawEliminationCinematic();
            return;
        }
        if (showGameMenu) { PausePanel(); if(Session is not null)DrawEliminationCinematic(); return; }
        if (MatchCosmeticsVisible) { DrawMatchCosmeticsBackdrop(); DrawMatchCosmetics(); DrawEliminationCinematic(); return; }
        if (DrawPostgame()) { if(Session is not null)DrawEliminationCinematic(); return; }
        Header(); if (Session is null) return;
        if (MatchCosmeticsVisible) { DrawMatchCosmeticsBackdrop(); DrawMatchCosmetics(); DrawEliminationCinematic(); return; }
        if (showGameMenu) { PausePanel(); if(Session is not null)DrawEliminationCinematic(); return; }
        Roster(); Sidebar();
        if (battle is null) Board();
        else
        {
            Window("Arena", Left, Top, Width, Height, ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
            ArenaPointer();
            CombatLabels();
            ImGui.End();
            DrawCastFeed();
        }
        DrawMatchStatus();
        ResultBanner();
        CompanionClickMark();
        ArenaEmotes();
        Choices(); Soundboard(); EmotePanel(); PlayersPanel();
        if (showAudio)
        {
            float soundWidth=Math.Min(380,size.X-32),soundHeight=Math.Min(audio.MusicController is null?270:audio.MusicController.Library?.Tracks.Count>0?540:440,size.Y-Top-16);
            var anchor=Buttons.GetValueOrDefault("audioSettings",new float2(size.X-32,Top-20));
            float soundX=Math.Clamp(anchor.X-soundWidth+30,8,size.X-soundWidth-8);
            Window("Sound settings",soundX,Top+6,soundWidth,soundHeight,ImGuiWindowFlags.NoTitleBar);
            AudioSettingsBounds=(ImGui.GetWindowDrawList().GetClipRectMin(),ImGui.GetWindowDrawList().GetClipRectMax());
            ImGui.BeginChild("Match sound controls"u8,new(0,-52));AudioControls();ImGui.EndChild();
            ImGui.Separator();
            if(audio.Error is not null){if(HudButton("reconnectAudio","Reconnect audio",190,32))audio.Reconnect();ImGui.SameLine();}
            if(HudButton("closeAudio","Close",90,32))showAudio=false;
            ImGui.End();
            if(!KeyboardPopup&&!showMusicLibrary&&PressedOutsideSocialPanel(new(soundX,Top+6),new(soundX+soundWidth,Top+6+soundHeight),audioToggleHovered))showAudio=false;
        }
        MusicLibraryPanel();DrawCodexSide();
        if(!showMusicLibrary && !EliminationCinematicActive && !CodexVisible && !codexConsumedShortcut) MatchShortcuts();
        DrawEliminationCinematic();
        }
        finally { DrawClientNavigation(); DrawClientSocial(); UpdatePanel(); DrawMysteryLootNotice(); DrawFileOperationClose(); DrawMatchLoading(windowSize); keyboardPopupLastFrame=KeyboardPopup; }
    }
    private void Menu()=>HomePanel();
    private void DrawMatchCosmeticsBackdrop()
    {
        // The original cosmetic overlay retains the live match chrome beneath
        // its shade. Disabled controls cannot consume input or become targets.
        drawingPauseBackdrop=true;ImGui.BeginDisabled();
        try {if(!Buttons.ContainsKey("gameMenu"))Header();Roster();Sidebar();}
        finally {ImGui.EndDisabled();drawingPauseBackdrop=false;Buttons.Clear();}
    }
    private void Board()
    {
        Window("Arena", Left, Top, Width, Height, ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
        ArenaPointer();
        if(Session!.Rounds.Phase!=MatchPhase.Result&&viewed!=localSeat)
            HudText($"Scouting {Session.State.Players[viewed].Name}",14,HudGold);
        for (int row = 4; row < 8; row++) for (int column = 0; column < 7; column++)
        {
            var point = planning.Cell(column, row, Width, Height);
            Buttons[$"hex/{column}/{row}"] = new(Left + point.X, Top + point.Y);
        }
        for (int i = 0; i < 9; i++)
        {
            var point = planning.Bench(i, Width, Height);
            Buttons[$"arenaBench/{i}"] = new(Left + point.X, Top + point.Y);
            Buttons[$"bench/{i}"] = Buttons[$"arenaBench/{i}"];
        }
        PlanningLabels();
        ImGui.End();
    }
    private void ProfileControls(bool showSaveLink=true)
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
        if (showSaveLink && Button("saveData", "Save data...")) NavigateClient("savedata");
        ImGui.Separator();
    }
    private void Sidebar()
    {
        // #traits and #right: #0e121b rails, 10px padding, a 1px line toward the arena.
        float railPadding=HudNarrow?6:10;
        ImGui.PushStyleColor(ImGuiCol.WindowBg,CssRail);ImGui.PushStyleColor(ImGuiCol.Border,new ImColor8(0,0,0,0));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(railPadding,railPadding));
        // The rail's 1px border belongs to the rail box, outside its padding.
        Window("Team",0,Top,Left-1,Height,ImGuiWindowFlags.NoTitleBar);
        ImGui.PopStyleColor(2);ImGui.PopStyleVar();
        ImGui.GetBackgroundDrawList().AddLine(new(Left-.5f,Top),new(Left-.5f,Top+Height),CssLine);
        TeamSidebarBounds=(ImGui.GetWindowDrawList().GetClipRectMin(),ImGui.GetWindowDrawList().GetClipRectMax());
        ImGui.SetCursorScreenPos(new(railPadding,Top+railPadding));
        PartnerDropZone();
        var player = Session!.State.Players[viewed];
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(8,0));
        AugmentSidebar(player);
        TraitSidebar(player);
        ImGui.PopStyleVar();
        var selected = Human.Roster.Entries.FirstOrDefault(e => e.Unit.Id == selectedUnit);
        if (selected is not null)
        {
            ImGui.SeparatorText("Selected"u8);
            UnitDetails(selected.Unit);

            if (Button("sell", "Sell unit")) { Act("sell", new { uid = selected.Unit.Id }); selectedUnit = null; }
            if (Session.State.Duos && Button("sendUnit", "Send to partner")) { Act("portal", new { uid = selected.Unit.Id }); selectedUnit = null; }
        }
        ModeBonusSidebar(player);
        ImGui.End();
        ImGui.PushStyleColor(ImGuiCol.WindowBg,CssRail);ImGui.PushStyleColor(ImGuiCol.Border,new ImColor8(0,0,0,0));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(railPadding,railPadding));
        Window("Standings",size.X-RightRail+1,Top,RightRail-1,Height,ImGuiWindowFlags.NoTitleBar);
        ImGui.PopStyleColor(2);ImGui.PopStyleVar();
        ImGui.GetBackgroundDrawList().AddLine(new(size.X-RightRail+.5f,Top),new(size.X-RightRail+.5f,Top+Height),CssLine);
        ScoutingBounds=(ImGui.GetWindowDrawList().GetClipRectMin(),ImGui.GetWindowDrawList().GetClipRectMax());
        ImGui.SetCursorScreenPos(new(size.X-RightRail+1+railPadding,Top+railPadding));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(8,0));
        StandingsCards();
        RoundHistoryStrip();
        DamageChart();
        ImGui.PopStyleVar();
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
        ImGui.End();
    }
    public void Render(int2 framebuffer)
    {
        if ((ClientHomeVisible||MatchCosmeticsVisible) && showFarm && farmViewError is null && size.X > 0 && size.Y > 0)
        {
            float scaleX = (float)framebuffer.X / Math.Max(1,socialWindowSize.X), scaleY = (float)framebuffer.Y / size.Y;
            if (farmView is not null) farmView.Selected = farmPet;
            farmView?.Draw((int)(FarmSceneX * scaleX), (int)((size.Y - FarmSceneTop - FarmSceneHeight) * scaleY), (int)(FarmSceneWidth * scaleX), (int)(FarmSceneHeight * scaleY));
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
        bool multisampled = arenaFrame.Begin(w, h);
        int left = multisampled ? 0 : x, bottom = multisampled ? 0 : y;
        try { if (battle is not null) battle.Draw(w, h, left, bottom); else planning.Draw(Rules, Session.State.Players[viewed], left, bottom, w, h); }
        finally { if (multisampled) arenaFrame.End(x, y); }
        RenderSpecialCutIn();
    }
    public void Dispose() { DisposeHome(); galaxyIntroArtwork.Dispose(); choiceArtwork.Dispose(); ClearEliminationCinematics(); DisposeCollectionPreview(); ReleaseRetiredCollectionPreviews(); eggArtwork.Dispose(); collectionArtwork.Dispose(); postgameArtwork.Dispose(); ClearEvolutionPreview(); lootOrbs.Dispose(); itemArtwork.Dispose(); portraits.Dispose(); DetachNetworkMatch(); opponentCompanion?.Dispose(); arenaCompanion?.Dispose(); emoteArtwork.Dispose(); ClearLobbyAvatars(); DisposeSteamConnection(); farmMoodTexture?.Dispose(); farmSymbolTexture?.Dispose(); farmUiIconTexture?.Dispose(); farmView?.Dispose(); battle?.Dispose(); planning.Dispose(); arenaEnvironment.Dispose(); arenaFrame.Dispose(); }
}
