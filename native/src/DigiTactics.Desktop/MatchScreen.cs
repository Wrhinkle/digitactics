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
    private readonly FarmProgress? farm;
    private Task<ProfileImport?>? profilePreview;
    private ProfileImport? profileCandidate;
    private string? profileMessage;
    private MatchHistoryRecorder? history;
    private readonly AbilityElements abilityElements = new(Path.Combine(AppContext.BaseDirectory, "content/ability-elements.json"));
    private CombatAudioCues? combatAudio;
    private readonly DisplayText text = new();
    private PlanningView planning = new();
    private BattleView? battle;
    private MatchBattle? watching;
    private int variant, viewed;
    private bool duos, showAudio;
    private int? selectedUnit, selectedItem;
    private string status = "Select a unit, then a hex. Select an item, then its holder.";
    private double backgroundTime;
    private int2 size;
    public MatchPlanningSession? Session { get; private set; }
    public Dictionary<string, float2> Buttons { get; } = [];
    public int BattleDrawCalls => battle?.DrawCalls ?? 0;
    public int PlanningDrawCalls => planning.DrawCalls;
    public bool WatchingMirrored => battle?.Mirrored ?? false;
    public int BattleColumns => battle?.Combat.Columns ?? 0;
    public int CompletedRounds { get; private set; }
    private MatchPlayer Human => Session!.State.Players[0];
    private Ruleset Rules => Session?.State.Rules ?? catalog.Variants[variant];
    private int Left => 230;
    private int Top => 118;
    private int Width => Math.Max(300, size.X - 470);
    private int Height => Math.Max(240, size.Y - 335);
    private const ImGuiWindowFlags Fixed = ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings;
    public MatchScreen(ContentCatalog catalog, GameAudio audio, uint? seed = null, bool duos = false, NativeProfileStore? profile = null) { this.catalog = catalog; this.audio = audio; this.seed = seed; this.duos = duos; this.profile = profile; farm = profile is null ? null : new(profile); }
    private void Window(string name, float x, float y, float width, float height, ImGuiWindowFlags flags = 0)
    {
        ImGui.SetNextWindowPos(new(x, y)); ImGui.SetNextWindowSize(new(width, height)); ImGui.Begin(name, Fixed | flags);
    }
    private bool Button(string key, string label, float width = 0, float height = 0, bool active = false)
    {
        if (active) ImGui.PushStyleColor(ImGuiCol.Button, new ImColor8(32, 131, 113));
        bool pressed = ImGui.Button(label + "##" + key, new float2(width, height));
        if (active) ImGui.PopStyleColor();
        var min = ImGui.GetItemRectMin(); var max = ImGui.GetItemRectMax(); Buttons[key] = new((min.X + max.X) / 2, (min.Y + max.Y) / 2);
        return pressed;
    }
    private string Name(string id) => text.Get(Rules.Id, id).Name;
    private void Tooltip(string id)
    {
        if (!ImGui.IsItemHovered()) return;
        var entry = text.Get(Rules.Id, id); ImGui.BeginTooltip(); ImGui.Text(entry.Name); ImGui.TextWrapped(entry.Description); ImGui.EndTooltip();
    }
    private string UnitName(PlanningUnit unit) => Rules.Lines.Concat(Rules.ExtraLines).Single(l => l.Id == unit.Line).Forms[unit.Stage];
    private void UnitDetails(string lineId, int stage)
    {
        var line = Rules.Lines.Concat(Rules.ExtraLines).Single(l => l.Id == lineId);
        var stats = UnitValues.Stats(line, stage);
        ImGui.TextWrapped($"{line.Forms[stage]}  {new string('*', stage + 1)}");
        ImGui.TextWrapped(string.Join(" / ", line.Traits.Select(Name)));
        ImGui.TextWrapped($"Base stats: {stats.MaxHealth} HP, {stats.AttackDamage} attack, {stats.AttackSpeed:0.##} attacks/s");
        ImGui.TextWrapped($"{stats.Armor} armor / {stats.MagicResist} magic resist / {stats.Range} range");
        ImGui.TextWrapped($"Mana {stats.StartMana}/{stats.MaxMana}");
        ImGui.TextWrapped(line.Ability.Names.ElementAtOrDefault(stage) ?? line.Ability.Kind);
    }
    private void UnitTooltip(string line, int stage)
    {
        if (!ImGui.IsItemHovered()) return;
        ImGui.BeginTooltip(); UnitDetails(line, stage); ImGui.EndTooltip();
    }
    private MatchActionResult? Act(string action, object? args = null)
    {
        var result = Session!.Actions.Apply(0, action, JsonSerializer.SerializeToElement(args ?? new { }));
        history?.Observe();
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
        return result;
    }
    private static object Spot(UnitSpot spot) => spot.IsBoard ? new { type = "board", c = spot.Column, r = spot.Row } : (object)new { type = "bench", i = spot.Index };
    private void Select(UnitSpot spot)
    {
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
        if (Session is null) return;
        if (Session.Rounds.Phase == MatchPhase.Combat)
        {
            battle?.Advance(delta); backgroundTime += Math.Min(delta, .25);
            combatAudio?.Update(audio.Play);
            while (backgroundTime >= CombatSimulation.Tick)
            {
                foreach (var match in Session.Battles) if (match != watching) match.Combat.Step();
                backgroundTime -= CombatSimulation.Tick;
            }
            if (Session.Battles.All(m => m.Combat.Done)) Finish();
        }
        else if (Session.Rounds.Phase == MatchPhase.Result) battle?.Advance(delta);
        planning.Update(Rules, Session.State.Players[viewed], delta);
    }
    private void Watch(int player)
    {
        viewed = player;
        if (Session is null || Session.Rounds.Phase == MatchPhase.Planning) return;
        var next = Session.Battles.FirstOrDefault(m => m.A == player || !m.Ghost && m.B == player ||
            m.TeamA is { } ta && Session.State.Teams[ta].Members.Any(p => p.Id == player) ||
            !m.Ghost && m.TeamB is { } tb && Session.State.Teams[tb].Members.Any(p => p.Id == player)) ?? Session.Battles.FirstOrDefault();
        if (next == watching) return;
        battle?.Dispose(); battle = null; watching = next;
        combatAudio = next is null ? null : new(next.Combat, Rules.Id, abilityElements);
        if (next is not null)
        {
            bool mirror = next.B == player || next.TeamB is { } team && Session.State.Teams[team].Members.Any(p => p.Id == player);
            battle = new(Rules, next.Combat, mirror);
        }
    }
    private void Fight()
    {
        selectedUnit = null; selectedItem = null; backgroundTime = 0;
        status = "Battle in progress.";
        Session!.StartCombat(); Watch(0);
        history?.Capture();
        if (Session.Battles.Count > 0) { audio.Play("fight"); audio.Music("combat"); }
        if (Session.Battles.Count == 0) Finish();
    }
    private void Finish()
    {
        history?.Capture();
        Session!.FinishCombat(); CompletedRounds++; status = "Round complete.";
        history?.Complete(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        if (history?.Error is { } saveError) { status = saveError; profileMessage = saveError; }
        var own = Session.Battles.FirstOrDefault(m => m.A == 0 || m.B == 0 ||
            m.TeamA is { } a && Session.State.Teams[a].Members.Any(p => p.Id == 0) ||
            m.TeamB is { } b && Session.State.Teams[b].Members.Any(p => p.Id == 0));
        bool sideB = own?.B == 0 || own?.TeamB is { } team && Session.State.Teams[team].Members.Any(p => p.Id == 0);
        audio.Play(Session.State.Over ? Human.Placement == 1 ? "champion" : "gameover" :
            own?.Combat.Winner is null or -1 ? "round" : own.Combat.Winner == (sideB ? 1 : 0) ? "win" : "loss");
        audio.Music(Session.State.Over ? null : "planning");
    }
    private void Next()
    {
        battle?.Dispose(); battle = null; watching = null; combatAudio = null; viewed = 0;
        Session!.Advance(); selectedUnit = null; selectedItem = null;
        audio.Play("round");
        status = "Select a unit, then a hex. Select an item, then its holder.";
    }
    public void Draw(int2 windowSize)
    {
        size = windowSize; Buttons.Clear();
        if (Session is null) { if (showFarm) FarmPanel(); else { Menu(); if (showCollection) Collection(); } return; }
        Header(); if (Session is null) return;
        Roster(); Sidebar();
        if (battle is null) Board();
        else battle.Labels(Width, Height, size.Y, Left, size.Y - Top - Height);
        Choices();
        if (showAudio)
        {
            Window("Sound settings", size.X / 2 - 210, Top + 20, 420, 250);
            AudioControls();
            if (Button("closeAudio", "Close")) showAudio = false;
            ImGui.End();
        }
    }
    private void Menu()
    {
        Window("DIGI-TACTICS", size.X * .25f, size.Y * .12f, size.X * .5f, 560);
        ImGui.Text("Build your team. Digivolve. Be the last Tamer standing."u8); ImGui.Separator();
        ProfileControls();
        if (profile is not null && Button("collection", "Collection and store")) showCollection = true;
        if (farm is not null && Button("farm", "Digi-Farm")) { farm.Refresh(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()); showFarm = true; }
        if (farm?.Error is { } farmError) CollectionText(farmError);
        if (ImGui.BeginCombo("Ruleset / pace"u8, $"{Rules.Name} / {Rules.Pace}"))
        {
            for (int i = 0; i < catalog.Variants.Length; i++) if (ImGui.Selectable($"{catalog.Variants[i].Name} / {catalog.Variants[i].Pace}", variant == i)) variant = i;
            ImGui.EndCombo();
        }
        if (ImGui.BeginCombo("Format"u8, duos ? "Duos" : "Solo"))
        {
            if (ImGui.Selectable("Solo"u8, !duos)) duos = false;
            if (ImGui.Selectable("Duos"u8, duos)) duos = true;
            ImGui.EndCombo();
        }
        ImGui.TextWrapped(duos ? "Team up with an AI partner. Share health and send units or items through your portal." : "Face seven AI Tamers. Your choices decide the team.");
        AudioControls();
        if (Button("start", "Start match", 220, 48))
        {
            Session = new(Rules, seed ?? (uint)Random.Shared.Next(1, int.MaxValue), Enumerable.Range(0, 8).Select(i => new MatchSeat(i == 0 ? profile?.Progress.Name ?? "You" : $"Tamer {i}", i == 0)).ToArray(), duos);
            history = profile is null ? null : new(Session, profile);
            viewed = 0; CompletedRounds = 0;
            selectedUnit = null; selectedItem = null; backgroundTime = 0;
            audio.Music("planning");
            status = "Select a unit, then a hex. Select an item, then its holder.";
        }
        ImGui.End();
    }
    private void Header()
    {
        var session = Session!;
        Window("Match", 12, 12, size.X - 24, 96, ImGuiWindowFlags.NoTitleBar);
        ImGui.Text($"{Rules.Name}  /  {Rules.Pace}  /  {(session.State.Duos ? "Duos" : "Solo")}     Round {session.Rounds.Position.Label}  -  {session.Rounds.RoundType.ToUpperInvariant()}");
        ImGui.Text($"{Human.Health} HP     {Human.Economy.Gold} gold     Level {Human.Economy.Level}     Streak {Human.Economy.Streak}");
        ImGui.SameLine(); if (Button("mute", audio.Muted ? "Unmute" : "Mute")) audio.Muted = !audio.Muted;
        ImGui.SameLine(); if (Button("audioSettings", "Sound settings")) showAudio = !showAudio;
        if (session.State.Over)
        {
            ImGui.Text($"Match complete - placement #{Human.Placement}"); ImGui.SameLine();
            if (Button("menu", "Main menu")) { Session = null; audio.Music(null); battle?.Dispose(); battle = null; watching = null; planning.Dispose(); planning = new(); }
        }
        else if (session.Rounds.Phase == MatchPhase.Planning) { if (Button("fight", session.Rounds.RoundType == "carousel" ? "Finish carousel" : "Ready for battle")) Fight(); }
        else if (session.Rounds.Phase == MatchPhase.Result) { if (Button("next", "Next round")) Next(); }
        else { if (Button("finish", "Skip to results")) Finish(); ImGui.SameLine(); ImGui.Text($"{battle?.Combat.Time ?? 0:F1}s"); }
        ImGui.End();
    }
    private void Board()
    {
        Window("Arena", Left, Top, Width, Height, ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
        ImGui.Text(viewed == 0 ? "Choose a unit or item, then a hex." : $"Scouting {Session!.State.Players[viewed].Name}");
        for (int row = 4; row < 8; row++) for (int column = 0; column < 7; column++)
        {
            var point = PlanningView.Cell(column, row, Width, Height); var spot = UnitSpot.Board(column, row);
            var unit = Session!.State.Players[viewed].Roster.At(spot);
            ImGui.SetCursorScreenPos(new(Left + point.X - 30, Top + point.Y + 10));
            ImGui.BeginDisabled(viewed != 0);
            if (Button($"hex/{column}/{row}", unit is null ? "+" : new string('*', unit.Stage + 1), 60, 25, selectedUnit is not null && selectedUnit == unit?.Id)) Select(spot);
            ImGui.EndDisabled();
            if (unit is not null) UnitTooltip(unit.Line, unit.Stage);
        }
        ImGui.End();
    }
    private void AudioControls()
    {
        float effects = audio.EffectsVolume, music = audio.MusicVolume;
        if (ImGui.SliderFloat("Effects volume"u8, ref effects, 0, 1)) audio.EffectsVolume = effects;
        if (ImGui.SliderFloat("Music volume"u8, ref music, 0, 1)) audio.MusicVolume = music;
        bool musicOff = audio.MusicOff, muted = audio.Muted;
        if (ImGui.Checkbox("Music off"u8, ref musicOff)) audio.MusicOff = musicOff;
        ImGui.SameLine(); if (ImGui.Checkbox("Mute all"u8, ref muted)) audio.Muted = muted;
        if (audio.Error is { } error) ImGui.TextWrapped(error);
        if (audio.SettingsError is { } settingsError) ImGui.TextWrapped(settingsError);
    }
    private void ProfileControls()
    {
        if (profile is null) return;
        static string Number(long? value) => value?.ToString("N0") ?? "Unavailable";
        var saved = profile.Progress;
        ImGui.Text($"{saved.Name}  |  {Number(saved.Bits)} Bits");
        ImGui.Text($"Saved record: {Number(saved.Games)} games, {Number(saved.Wins)} wins, {Number(saved.TopFour)} top-four finishes");
        if (profile.Error is { } error) ImGui.TextWrapped(error);
        if (!profile.Exists)
        {
            if (profilePreview is not null) ImGui.Text("Reading backup..."u8);
            else if (profileCandidate is { } candidate)
            {
                ImGui.TextWrapped($"Import {candidate.Progress.Name}'s saved profile? {candidate.Summary.StorageEntries} saved entries and {candidate.Summary.MusicTracks} music files will be preserved.");
                ImGui.TextWrapped("Identity and saved totals are available now. Cosmetics, music-library playback and other migrated features are still being completed."u8);
                if (Button("importProfile", "Import profile"))
                {
                    try { profile.Import(candidate); profileCandidate = null; profileMessage = "Profile imported. Your original backup is preserved."; }
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
        if (profileMessage is not null) ImGui.TextWrapped(profileMessage);
        ImGui.Separator();
    }
    private void Sidebar()
    {
        Window("Team", 12, Top, 208, Height);
        var player = Session!.State.Players[viewed];
        ImGui.Text($"{player.Name}  {player.Roster.BoardCount}/{new MatchPlacement(Session.State).Capacity(player, Session.Rounds.Position.Stage)} deployed");
        var counts = TraitRules.Count(Rules, Session.CombatProjection.Board(player), player.Augments.ToArray());
        foreach (var count in counts.OrderByDescending(c => c.Value))
        {
            var trait = Rules.Traits.FirstOrDefault(t => t.Id == count.Key);
            ImGui.TextWrapped($"{Name(count.Key)}  {count.Value}  ({string.Join('/', trait?.Breakpoints ?? [])})"); Tooltip(count.Key);
        }
        ImGui.SeparatorText("Selected"u8);
        var selected = Human.Roster.Entries.FirstOrDefault(e => e.Unit.Id == selectedUnit);
        if (selected is not null)
        {
            UnitDetails(selected.Unit.Line, selected.Unit.Stage);
            foreach (string item in selected.Unit.Items) { ImGui.Text(Name(item)); Tooltip(item); }
            if (Button("sell", "Sell unit")) { Act("sell", new { uid = selected.Unit.Id }); selectedUnit = null; }
            if (Session.State.Duos && Button("sendUnit", "Send to partner")) { Act("portal", new { uid = selected.Unit.Id }); selectedUnit = null; }
        }
        if (Human.Wisp is { } wisp && Button("wisp", $"{Name(wisp.Id)} ({wisp.Cost}g)")) Act("wisp");
        if (Human.Power is { } power) ImGui.TextWrapped($"{Name(power.Id)} - rank {power.Rank}");
        foreach (string id in Human.Augments) { ImGui.TextWrapped(Name(id)); Tooltip(id); }
        foreach (string id in Human.Globals) { ImGui.TextWrapped(Name(id)); Tooltip(id); }
        ImGui.End();
        Window("Standings", size.X - 228, Top, 216, Height);
        foreach (var p in Session.State.Players.OrderByDescending(p => p.Alive).ThenByDescending(p => p.Health))
            if (Button("view/" + p.Id, $"{p.Name}  {(p.Alive ? p.Health + " HP" : "#" + p.Placement)}", 192, 28)) Watch(p.Id);
        if (Session.State.Duos) ImGui.TextWrapped("Your partner: " + Session.Units.Portal.Partner(Human)?.Name);
        foreach (var orb in Human.Orbs.ToArray()) if (Button("orb/" + orb.Id, $"Collect {orb.Kind} loot")) Act("orb", new { id = orb.Id });
        if (history?.Award is { } award)
        {
            ImGui.Separator(); ImGui.Text($"Earned {award.Total} Bits");
            foreach (var line in award.Lines) ImGui.TextWrapped($"{line.Label}: +{line.N}");
            if (history.LegendAward is { } legend) ImGui.TextWrapped($"{legend.Legend}: +{legend.Data} Digi-Data (level {legend.Level})");
        }
        if (history?.Error is not null && Button("retryHistory", "Retry saving progress"))
        {
            bool saved = history.Complete(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            status = saved ? "Match progress saved." : history.Error!;
            profileMessage = saved ? null : history.Error;
        }
        ImGui.Separator(); ImGui.TextWrapped(status); ImGui.End();
    }
    private void Roster()
    {
        Window("Shop and bench", 12, size.Y - 207, size.X - 24, 195, ImGuiWindowFlags.NoTitleBar);
        ImGui.BeginDisabled(Session!.State.Over || !Human.Alive);
        if (Button("reroll", $"Refresh ({(Human.Shop.FreeRolls > 0 ? "free" : Rules.RerollCost + "g")})")) Act("reroll");
        ImGui.SameLine(); if (Button("lock", Human.Shop.Locked ? "Unlock shop" : "Lock shop")) Act("lock");
        ImGui.SameLine(); if (Rules.Economy!.Xp is { } xp && Button("xp", $"Buy XP ({xp.BuyCost}g)")) Act("buyXp");
        ImGui.SameLine(); ImGui.Text($"XP {Human.Economy.Xp}     {Human.Items.Count} items");
        float card = (size.X - 64f) / Human.Shop.Slots.Count;
        for (int i = 0; i < Human.Shop.Slots.Count; i++)
        {
            if (i > 0) ImGui.SameLine(); string? id = Human.Shop.Slots[i];
            string label = id is null ? "Empty" : $"{Rules.Lines.Single(l => l.Id == id).Forms[Human.Shop.Metadata[i]?.Unit?.Stage ?? 0]}\n{Human.Shop.Cost(i)} gold {Human.Shop.Metadata[i]?.Tag}";
            if (Button("buy/" + i, label, card, 48)) Act("buy", new { slot = i });
            if (id is not null) UnitTooltip(id, Human.Shop.Metadata[i]?.Unit?.Stage ?? 0);
        }
        for (int i = 0; i < 9; i++)
        {
            if (i > 0) ImGui.SameLine(); var unit = Human.Roster.Bench[i];
            if (Button("bench/" + i, unit is null ? "-" : UnitName(unit) + "\n" + new string('*', unit.Stage + 1), (size.X - 96f) / 9, 40, selectedUnit is not null && unit?.Id == selectedUnit)) Select(UnitSpot.Bench(i));
            if (unit is not null) UnitTooltip(unit.Line, unit.Stage);
        }
        for (int i = 0; i < Human.Items.Count; i++)
        {
            if (i % 6 != 0) ImGui.SameLine();
            if (Button("item/" + i, Name(Human.Items[i]), (size.X - 78f) / 6, 24, selectedItem == i))
            {
                if (selectedItem is { } old && old != i) { Act("swapItems", new { a = old, b = i }); selectedItem = null; }
                else { selectedItem = selectedItem == i ? null : i; selectedUnit = null; status = "Select a holder to equip, or another item to swap positions."; }
            }
            Tooltip(Human.Items[i]);
        }
        if (selectedItem is { } item && Session.State.Duos && Button("sendItem", "Send selected item")) { Act("portal", new { item }); selectedItem = null; }
        ImGui.EndDisabled(); ImGui.End();
    }
    private void Choices()
    {
        if (Session!.Units.Carousel.Active)
        {
            Window("Carousel", Left, Top + 15, Width, 180);
            ImGui.Text($"Picking: {Session.Units.Carousel.Turn?.Name ?? "Complete"}");
            for (int i = 0; i < Session.Units.Carousel.Options.Count; i++)
            {
                var o = Session.Units.Carousel.Options[i]; if (o.PickedBy is not null) continue;
                if (Button("carousel/" + i, $"{Rules.Lines.Single(l => l.Id == o.Line).Forms[0]} + {Name(o.Item!)}")) Act("carousel", new { i });
            }
            ImGui.End();
        }
        else if (Human.Offer is { } offer)
        {
            var heading = text.Offer(Rules.Id, offer.Kind);
            Window(heading.Name + "##reward", Left + 12, Top + 12, Width - 24, Math.Min(Height - 24, 440));
            ImGui.TextWrapped(heading.Description);
            ImGui.Separator();
            for (int i = 0; i < offer.Options.Length; i++)
            {
                var choice = text.Choice(Rules, offer.Kind, offer.Options[i]);
                if (Button("offer/" + i, choice.Name, Width - 65, 34)) { Act("offer", new { i }); break; }
                ImGui.TextWrapped(choice.Description);
                ImGui.Separator();
            }
            if (offer.Rerolls > 0 && Button("offerReroll", $"Reroll choices ({offer.Rerolls})")) Act("offerReroll");
            ImGui.End();
        }
    }
    public void Render(int2 framebuffer)
    {
        if (Session is null && showFarm && training is null && farmViewError is null && size.X > 0 && size.Y > 0)
        {
            float scaleX = (float)framebuffer.X / size.X, scaleY = (float)framebuffer.Y / size.Y;
            if (farmView is not null) farmView.Selected = farmPet;
            farmView?.Draw((int)(12 * scaleX), (int)((size.Y - 70 - FarmSceneHeight) * scaleY), (int)(FarmSceneWidth * scaleX), (int)(FarmSceneHeight * scaleY));
            return;
        }
        if (Session is null || framebuffer.X <= 0 || framebuffer.Y <= 0 || size.X <= 0 || size.Y <= 0) return;
        // GLFW logical and framebuffer sizes can differ on high-DPI displays.
        float sx = (float)framebuffer.X / size.X, sy = (float)framebuffer.Y / size.Y;
        int x = (int)(Left * sx), y = (int)((size.Y - Top - Height) * sy), w = (int)(Width * sx), h = (int)(Height * sy);
        if (battle is not null) battle.Draw(w, h, x, y); else planning.Draw(Rules, Session.State.Players[viewed], x, y, w, h);
    }
    public void Dispose() { farmView?.Dispose(); battle?.Dispose(); planning.Dispose(); }
}
