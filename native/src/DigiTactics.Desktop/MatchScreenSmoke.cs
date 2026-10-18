using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

/// <summary>Exercises visible controls through ImGui mouse events, not direct gameplay mutations.</summary>
internal sealed class MatchScreenSmoke(NativeProfileStore? collectionProfile = null, bool evolutionSmoke = false)
{
    private readonly GameMenuSmoke gameMenu = new();
    private readonly HomeSmoke home = new(collectionProfile);
    private readonly CodexSmoke codex = new();
    private readonly PlaybackSpeedSmoke playbackSpeed = new();
    private readonly CollectionPreviewSmoke collectionPreviews = new();
    private readonly CollectionFilterSmoke collectionFilters = new();
    private readonly IncubatorSmoke incubator = new();
    private bool eggWobble,eggCrack,eggBurst,eggReveal;
    private readonly EliminationSmoke elimination = new();
    private readonly LootOrbSmoke loot = new();
    private bool gameplayVerified, eliminationCaptured, spectatingCaptured;
    private readonly Dictionary<int, PauseSmoke> pauseChecks = new() { [9] = new(true), [10] = new(), [11] = new() };
    private readonly PlanningTimerSmoke planningTimer = new();
    private double automaticAdvanceWait;
    private int combatHoverFrames, combatHoverVisible;
    private bool combatHoverVerified;
    private readonly string[] collectionCommands = evolutionSmoke
        ? ["collection", "buyCosmetic/legend/bukamon", "confirmPurchase", "equip/legend/bukamon", "evolution/bukamon", "levelLegend", "levelLegend", "feedLegend", "digivolve", "revertLegend", "digivolve", "openEvolutionTree", "closeEvolutionTree", "closeEvolution", "toggleEggs", "egg/legend", "confirmEgg", "closeCollection"]
        : ["collection", "buyCosmetic/legend/bukamon", "confirmPurchase", "equip/legend/bukamon", "toggleEggs", "egg/legend", "confirmEgg", "closeCollection"];
    private readonly long initialBits = collectionProfile?.Progress.Bits ?? 0;
    private int collectionIndex;
    private bool collectionVerified,treeScrolled,transformWhite,transformFlash,settledPreview,announcementExpired,announcementFadedIn,announcementFadedOut;
    private readonly string[] farmCommands = evolutionSmoke
        ? ["farm", "farmFeed/meat", "farmPlay", "farmPraise", "farmPet", "farmLights", "farmLights", "farmRoom", "farmOut/tsunomon", "farmIn/tsunomon", "farmOut/motimon", "farmSelect/tsunomon", "farmSwap/motimon", "farmIn/tsunomon", "farmExpand", "farmTheme/sewer", "closeFarm"]
        : ["farm", "farmFeed/meat", "farmPlay", "farmPraise", "farmPet", "farmLights", "farmLights", "closeFarm"];
    private int farmIndex;
    private bool farmVerified;
    private long farmInitialBits;
    public string Pending => collectionProfile is not null && collectionIndex < collectionCommands.Length ? collectionCommands[collectionIndex] == "egg/legend" && !incubator.Done ? incubator.Pending : collectionCommands[collectionIndex]
        : collectionProfile is not null && farmIndex < farmCommands.Length ? farmCommands[farmIndex] : commands.ElementAtOrDefault(index) ?? "verification";
    private readonly string[] commands = ["start", "carousel", "fight", "next", "buy", "arenaBench/0", "hex/3/4", "bench/1", "hex/2/4", "fight", "finish", "next",
        "fight", "finish", "next", "fight", "finish", "next", "fight", "opponent", "finish", "next", "audioSettings", "reconnectAudio", "closeAudio", "mute", "mute"];
    private int index, cooldown = 3, battleFrames, pvpFrames;
    private bool down, planningCaptured, battleCaptured, pvpCaptured;
    private long emoteTime;
    private bool combatBenchVerified;
    private bool reservePurchased, purchasingReserve;
    private int scoutSwitches;
    private bool switchingScout;
    private int chartStep;
    private bool combinedTraitsVerified;
    private readonly HashSet<string> resultRounds = [];
    private int traitHoverStep;
    private string hoveredTrait = "";
    private int dragStep, draggedId;
    private readonly ArenaDropSmoke dropSmoke = new();
    private bool dropsVerified;
    private readonly OfferChoiceSmoke offerSmoke = new();
    private bool offersVerified;
    private readonly ShopControlSmoke shopControlSmoke = new();
    private bool shopControlsVerified;
    private readonly ItemDragSmoke itemDragSmoke = new();
    private bool itemDragsVerified;
    private readonly CameraSmoke cameraSmoke = new();
    private bool cameraVerified;
    private bool eliminationSkipDown;
    public void Input(MatchScreen screen)
    {
        if(!home.Done){home.Input(screen);return;}
        if(!codex.HomeDone){codex.HomeInput(screen);return;}
        if (eliminationSkipDown) { ImGui.GetIO().AddKeyEvent(ImGuiKey.Escape,false); eliminationSkipDown=false; return; }
        if (screen.EliminationCinematicActive) { ImGui.GetIO().AddKeyEvent(ImGuiKey.Escape,true); eliminationSkipDown=true; return; }
        emoteTime+=100;screen.AiEmoteClock=()=>emoteTime;screen.AiEmoteRandom=()=>.01;
        if (collectionProfile is not null && collectionIndex < collectionCommands.Length)
        {
            if (cooldown-- > 0) return;
            var input = ImGui.GetIO();
            if(collectionIndex==1 && !down && !collectionFilters.Done){collectionFilters.Input(screen,collectionProfile);return;}
            if(collectionIndex==4 && !down && !collectionPreviews.Done){collectionPreviews.Input(screen);return;}
            if(collectionCommands[collectionIndex]=="egg/legend" && !down && !incubator.Done){incubator.Input(screen);return;}
            if(collectionCommands[collectionIndex]=="closeCollection" && screen.EggHatchBusy)return;
            if(!down && (screen.EvolutionBusy || collectionIndex==11 && screen.EvolutionTransitionTime>0))return;
            if(evolutionSmoke && collectionIndex==11 && settledPreview && !down && !announcementExpired)
            {
                if(screen.EvolutionAnnouncementVisible)return;
                announcementExpired=true;cooldown=3;return;
            }
            if(evolutionSmoke && collectionIndex==12 && !down && !treeScrolled)
            {
                var anchor=screen.Buttons["closeEvolutionTree"];
                input.AddMousePosEvent(anchor.X+300,anchor.Y+220); input.AddMouseWheelEvent(0,-15);
                treeScrolled=true; cooldown=3; return;
            }
            if (down) { input.AddMouseButtonEvent(0, false); down = false; collectionIndex++; cooldown = 3; return; }
            if (!screen.Buttons.TryGetValue(collectionCommands[collectionIndex], out var target)) return;
            if(collectionCommands[collectionIndex].StartsWith("egg/",StringComparison.Ordinal) && screen.IncubatorInventoryBounds is { } inventory)
            {
                if(target.Y<inventory.Min.Y+12||target.Y>inventory.Max.Y-12)
                {
                    input.AddMousePosEvent((inventory.Min.X+inventory.Max.X)*.5f,(inventory.Min.Y+inventory.Max.Y)*.5f);
                    input.AddMouseWheelEvent(0,target.Y<inventory.Min.Y?5:-5);cooldown=3;return;
                }
            }
            if(collectionCommands[collectionIndex].StartsWith("buyCosmetic/",StringComparison.Ordinal)||collectionCommands[collectionIndex].StartsWith("evolution/",StringComparison.Ordinal))
            {
                var bounds=screen.CollectionListBounds;
                if(target.Y<bounds.Min.Y+12||target.Y>bounds.Max.Y-12)
                {
                    input.AddMousePosEvent((bounds.Min.X+bounds.Max.X)*.5f,(bounds.Min.Y+bounds.Max.Y)*.5f);
                    input.AddMouseWheelEvent(0,target.Y<bounds.Min.Y?5:-5);cooldown=3;return;
                }
            }
            input.AddMousePosEvent(target.X, target.Y); input.AddMouseButtonEvent(0, true); down = true; return;
        }
        if (collectionProfile is not null && farmIndex < farmCommands.Length)
        {
            if (cooldown-- > 0) return;
            var input = ImGui.GetIO();
            if (down) { input.AddMouseButtonEvent(0, false); down = false; farmIndex++; cooldown = 3; return; }
            if (!screen.Buttons.TryGetValue(farmCommands[farmIndex], out var target)) return;
            input.AddMousePosEvent(target.X, target.Y); input.AddMouseButtonEvent(0, true); down = true; return;
        }
        if (index >= commands.Length) { if (cooldown-- > 0) return; if (!loot.Done) loot.Input(screen); else if (!elimination.Done) elimination.Input(screen); else gameMenu.Input(screen); cooldown = 3; return; }
        if (cooldown-- > 0) return;
        if (index == 10 && battleFrames < 60) return;
        if (index == 20 && pvpFrames < 60) return;
        if (pauseChecks.TryGetValue(index, out var pauseCheck)) { if (pauseCheck.Input(screen)) pauseChecks.Remove(index); cooldown = 3; return; }
        if (index == 9 && !playbackSpeed.Done) { playbackSpeed.Input(screen); cooldown = 3; return; }
        if (index == 9 && !codex.LiveDone) { codex.LiveInput(screen); return; }
        if (index == 9 && traitHoverStep < 2)
        {
            if (traitHoverStep == 0)
            {
                hoveredTrait = screen.DisplayedTraits.Keys.First();
                var target = screen.Buttons["trait/" + hoveredTrait]; ImGui.GetIO().AddMousePosEvent(target.X, target.Y);
            }
            else if (screen.TraitTooltipLevels != screen.Session!.State.Rules.Traits.Single(trait => trait.Id == hoveredTrait).Breakpoints.Length)
                throw new InvalidOperationException("Trait hover omitted breakpoint effect text.");
            if (traitHoverStep == 1 && screen.TraitTooltipUnits.Count == 0)
                throw new InvalidOperationException("Trait hover omitted its deployed contributor portraits.");
            traitHoverStep++; cooldown = 3; return;
        }
        if (index == 20 && !combatHoverVerified)
        {
            if (screen.DetailedCombatUnit is { } inspected)
            {
                if (!inspected.Alive || !screen.ChartCombat!.Units.Contains(inspected)) throw new InvalidOperationException("Combat inspection referenced a different or dead battle unit.");
                if (++combatHoverVisible >= 3) { combatHoverVerified = true; return; }
            }
            if (++combatHoverFrames > 30) throw new InvalidOperationException("Combat unit hover did not open live details.");
            var target = screen.Buttons.First(pair => pair.Key.StartsWith("combatUnit/", StringComparison.Ordinal)).Value;
            ImGui.GetIO().AddMousePosEvent(target.X,target.Y);
            return;
        }
        if (index == 20 && chartStep < 7)
        {
            var chartIo = ImGui.GetIO();
            if (chartStep % 2 == 0 && chartStep < 6)
            {
                int metric = (chartStep / 2 + 1) % 3;
                var chartPoint = screen.Buttons["chart/" + metric];
                chartIo.AddMousePosEvent(chartPoint.X, chartPoint.Y); chartIo.AddMouseButtonEvent(0, true);
            }
            else if (chartStep < 6) chartIo.AddMouseButtonEvent(0, false);
            if (chartStep is 2 or 4 or 6 && screen.ChartMetric != chartStep / 2 % 3)
                throw new InvalidOperationException("Damage chart tab did not select its metric.");
            chartStep++; cooldown = 3; return;
        }
        if (index == 9 && !cameraVerified) { cameraVerified = cameraSmoke.Input(screen); cooldown = 3; return; }
        if (index == 22 && !itemDragsVerified) { itemDragsVerified = itemDragSmoke.Input(screen); cooldown = 3; return; }
        if (index == 22 && !dropsVerified) { dropsVerified = dropSmoke.Input(screen); cooldown = 3; return; }
        if (index == 22 && !offersVerified) { offersVerified = offerSmoke.Input(screen); cooldown = 3; return; }
        if (index == 22 && !shopControlsVerified) { shopControlsVerified = shopControlSmoke.Input(screen); cooldown = 3; return; }
        if (index == 9 && dragStep < 10)
        {
            var dragIo = ImGui.GetIO();
            var boardPoint = screen.Buttons["hex/3/4"];
            var benchPoint = screen.Buttons["arenaBench/8"];
            var roster = screen.Session!.State.Players[0].Roster;
            switch (dragStep)
            {
                case 0:
                    draggedId = roster.At(UnitSpot.Board(3, 4))!.Id;
                    dragIo.AddMousePosEvent(boardPoint.X, boardPoint.Y); dragIo.AddMouseButtonEvent(0, true); break;
                case 1: dragIo.AddMousePosEvent(benchPoint.X, benchPoint.Y); break;
                case 2:
                    if (!screen.ArenaDragging || screen.ArenaDropBench != 8) throw new InvalidOperationException("Arena drag did not start or highlight its bench destination.");
                    dragIo.AddMouseButtonEvent(0, false); break;
                case 3:
                    if (roster.At(UnitSpot.Bench(8))?.Id != draggedId) throw new InvalidOperationException("Arena drag did not move the board unit to its bench destination.");
                    dragIo.AddMouseButtonEvent(0, true); break;
                case 4: dragIo.AddMousePosEvent(boardPoint.X, boardPoint.Y); break;
                case 5: dragIo.AddMouseButtonEvent(0, false); break;
                case 6:
                    if (roster.At(UnitSpot.Board(3, 4))?.Id != draggedId || roster.At(UnitSpot.Bench(8)) is not null || screen.ArenaDragging)
                        throw new InvalidOperationException("Arena drag did not return the unit or clear pointer capture.");
                    dragIo.AddMouseButtonEvent(0, true);
                    break;
                case 7: dragIo.AddMousePosEvent(0, 0); break;
                case 8: dragIo.AddMouseButtonEvent(0, false); break;
                case 9:
                    if (roster.At(UnitSpot.Board(3, 4))?.Id != draggedId || screen.ArenaDragging || screen.ArenaDropBench != -1)
                        throw new InvalidOperationException("Dropping outside the arena moved the unit or retained drag feedback.");
                    break;
            }
            dragStep++; cooldown = 3; return;
        }
        var io = ImGui.GetIO();
        if (down)
        {
            io.AddKeyEvent(ImGuiKey.Space,false);
            io.AddMouseButtonEvent(0, false); down = false;
            if (purchasingReserve) { reservePurchased = true; purchasingReserve = false; }
            else if (switchingScout) { scoutSwitches++; switchingScout = false; }
            else index++;
            cooldown = 3; return;
        }
        if (index == 11)
        {
            if (screen.Session!.Rounds.Phase == MatchPhase.Result)
            {
                automaticAdvanceWait += CombatSimulation.Tick;
                if (automaticAdvanceWait > 2.3) throw new InvalidOperationException("Solo results did not advance automatically after the source delay.");
                return;
            }
            if (screen.Session.Rounds.Phase != MatchPhase.Planning || automaticAdvanceWait < 1.5)
                throw new InvalidOperationException("Automatic result advance skipped the result presentation delay.");
            index++; cooldown = 3; return;
        }
        if (index == 12) { if (planningTimer.Input(screen)) index++; cooldown = 3; return; }
        string command = commands[index];
        if (index == 19 && !reservePurchased)
        {
            // Buy a reserve through the real shop during combat, after automatic deployment.
            // This keeps a nonempty bench through combat instead of passing only empty-set comparisons.
            command = "buy"; purchasingReserve = true;
        }
        if (index == 20 && scoutSwitches < 2)
        {
            command = scoutSwitches == 0 ? "ally" : "opponent";
            switchingScout = true;
        }
        if (command == "carousel") command = "carousel/" + screen.Session!.Units.Carousel.Options.Select((o, i) => (o, i)).First(e => e.o.PickedBy is null).i;
        if (command == "buy") command = "buy/" + screen.Session!.State.Players[0].Shop.Slots.Select((id, i) => (id, i)).First(e => e.id is not null).i;
        if (command is "opponent" or "ally")
        {
            var match = screen.Session!.Battles[0];
            int? team = command == "ally" ? match.TeamA : match.TeamB;
            int player = team is { } t ? screen.Session.State.Teams[t].Members[0].Id : command == "ally" ? match.A : match.B!.Value;
            command = "view/" + player;
        }
        if (command == "finish" && screen.Session!.Rounds.Phase == MatchPhase.Result) { index++; return; }
        if (command == "fight") { io.AddKeyEvent(ImGuiKey.Space,true); down=true; return; }
        if (!screen.Buttons.TryGetValue(command, out var point)) return;
        io.AddMousePosEvent(point.X, point.Y); io.AddMouseButtonEvent(0, true); down = true;
    }
    public bool Verify(MatchScreen screen, int2 size, string? capture)
    {
        if(incubator.CaptureRequested is { } incubatorCapture)
        {
            if(capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,incubatorCapture+".bmp"),size.X,size.Y);
            incubator.CaptureRequested=null;
        }
        if(!home.Done){home.Verify(screen,size,capture);return false;}
        if(!codex.HomeDone){codex.Verify(screen,size,capture,false);return false;}
        if(index==9&&!codex.LiveDone)codex.Verify(screen,size,capture,true);
        if (gameplayVerified)
        {
            if (loot.Capture && capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"loot.bmp"),size.X,size.Y);
            if (!eliminationCaptured && capture is not null && screen.Buttons.ContainsKey("spectateRemaining"))
            { FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"eliminated.bmp"),size.X,size.Y); eliminationCaptured = true; }
            if (!spectatingCaptured && capture is not null && !screen.Buttons.ContainsKey("spectateRemaining") && screen.WatchingAfterElimination && screen.Session!.Rounds.Phase == MatchPhase.Planning)
            { FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"spectating.bmp"),size.X,size.Y); spectatingCaptured = true; }
            if (gameMenu.Confirming && capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"leave-confirm.bmp"),size.X,size.Y);
            return gameMenu.Done;
        }
        if (cooldown == 1 && screen.Session is { } resultSession && resultSession.Rounds.Phase == MatchPhase.Result && resultSession.Rounds.Results.Count > 0)
        {
            var player = resultSession.State.Players[0];
            var result = resultSession.Rounds.Results.First(result => result.Match.TeamA is { } a
                ? player.Team == a || !result.Match.Ghost && player.Team == result.Match.TeamB
                : result.Match.A == player.Id || !result.Match.Ghost && result.Match.B == player.Id);
            bool isA = result.Match.TeamA is { } team ? player.Team == team : result.Match.A == player.Id;
            string title = result.Match.Winner == (isA ? 0 : 1) ? "Victory" : result.Match.Winner == (isA ? 1 : 0) ? "Defeat" : "Draw";
            if (screen.ResultTitle != title || screen.ResultHealthLoss != (isA ? result.DamageA : result.DamageB))
                throw new InvalidOperationException("Result banner does not match the local player's settled outcome.");
            if (resultRounds.Add(resultSession.Rounds.Position.Label) && capture is not null)
                FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "result-" + resultSession.Rounds.Position.Label + ".bmp"), size.X, size.Y);
        }
        if (screen.IsPaused && cooldown == 1 && capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "paused.bmp"), size.X,size.Y);
        if (index == 9 && traitHoverStep == 1 && cooldown == 1 && capture is not null)
            FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "trait-tooltip.bmp"), size.X, size.Y);
        if (index == 20 && !combatHoverVerified && screen.DetailedCombatUnit is not null && capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "combat-tooltip.bmp"),size.X,size.Y);
        if (index == 20 && cooldown == 1 && screen.ChartCombat is { } chartCombat)
        {
            var session = screen.Session!;
            var player = session.State.Players[screen.ViewedPlayer];
            if (!screen.DisplayedAugments.SequenceEqual(player.Augments) || screen.AugmentPortraits != player.Augments.Count(id => ItemArtwork.Has(session.State.Rules.Id, "aug:" + id)))
                throw new InvalidOperationException("Augment sidebar did not follow the scouted player or omitted authored artwork.");
            if (screen.DisplayedPower != player.Power || !screen.DisplayedGlobals.SequenceEqual(player.Globals))
                throw new InvalidOperationException("Power/global sidebar did not follow the scouted player.");
            var soloTraits = TraitRules.Count(session.State.Rules, session.CombatProjection.Board(player), player.Augments.ToArray());
            var expectedTraits = session.State.Duos ? session.CombatProjection.Team(session.State.Teams[player.Team!.Value]).Effects.Counts! : soloTraits;
            if (screen.CombinedTraits != session.State.Duos || screen.DisplayedTraits.Count != expectedTraits.Count || expectedTraits.Any(pair => screen.DisplayedTraits.GetValueOrDefault(pair.Key) != pair.Value))
                throw new InvalidOperationException("Sidebar trait counts do not match the watched Solo/Duos team.");
            var traitBoard = session.State.Duos ? session.CombatProjection.Team(session.State.Teams[player.Team!.Value]).Units : session.CombatProjection.Board(player);
            int activeTraits = expectedTraits.Count(pair => TraitRules.Level(session.State.Rules.Traits.Single(trait => trait.Id == pair.Key), pair.Value) > 0);
            int contributors = expectedTraits.Sum(pair => TraitContributors.Select(session.State.Rules, traitBoard, pair.Key, session.State.Duos).Length);
            if (screen.InlineTraitEffects != activeTraits || screen.InlineTraitPortraits != contributors)
                throw new InvalidOperationException("Inline trait cards omitted active effects or contributor portraits.");
            combinedTraitsVerified |= session.State.Duos && expectedTraits.Any(pair => soloTraits.GetValueOrDefault(pair.Key) != pair.Value);
            var expected = chartCombat.Units.Where(unit => unit.Team == (screen.WatchingMirrored ? 1 : 0))
                .OrderByDescending(unit => screen.ChartMetric switch { 1 => unit.Taken, 2 => unit.Healed, _ => unit.Dealt })
                .Select(unit => unit.Id);
            if (!screen.ChartUnits.SequenceEqual(expected)) throw new InvalidOperationException("Damage chart omitted units, used the wrong watched team, or sorted the wrong metric.");
            if (capture is not null && chartStep is 3 or 5)
                FrameCapture.SaveBitmap(Path.ChangeExtension(capture, screen.ChartMetric == 1 ? "taken.bmp" : "healed.bmp"), size.X, size.Y);
        }
        if (index == 23 && cooldown == 1 && capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"audio-settings.bmp"),size.X,size.Y);
        if (index == 22 && cooldown == 1 && offerSmoke.AugmentCapture && capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "augment.bmp"), size.X, size.Y);
        if (index == 22 && cooldown == 1 && offerSmoke.Capture && capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "reward.bmp"), size.X, size.Y);
        if (index == 1 && cooldown == 1 && screen.Session!.Units.Carousel.Active)
        {
            if (screen.CarouselCardsDrawn != screen.Session.Units.Carousel.Options.Count)
                throw new InvalidOperationException("Carousel omitted available or taken choices.");
            if (capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "carousel.bmp"), size.X, size.Y);
        }
        if (index == 22 && cooldown == 1 && capture is not null && !itemDragsVerified && itemDragSmoke.CaptureName is { } itemCapture)
            FrameCapture.SaveBitmap(Path.ChangeExtension(capture, itemCapture + ".bmp"), size.X, size.Y);
        if (index == 22 && cooldown == 1 && capture is not null && dropSmoke.CaptureName is { } dropCapture)
            FrameCapture.SaveBitmap(Path.ChangeExtension(capture, dropCapture + ".bmp"), size.X, size.Y);
        if (index == 9 && dragStep == 2 && cooldown == 1 && capture is not null)
            FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "drag.bmp"), size.X, size.Y);
        if (collectionProfile is not null && !collectionVerified)
        {
            if(collectionIndex==1&&!collectionFilters.Done)collectionFilters.Verify(screen,collectionProfile,size,capture);
            collectionPreviews.Capture(screen,size,capture);
            double eggAge=screen.EggHatchAge;
            if(eggAge>=0)
            {
                string? stage=null;
                if(eggAge>.3&&eggAge<1.2&&!eggWobble){eggWobble=true;stage="wobble";}
                if(eggAge>1.4&&eggAge<2&&!eggCrack){eggCrack=true;stage="crack";}
                if(eggAge>2.1&&eggAge<2.4&&!eggBurst){eggBurst=true;stage="burst";}
                if(eggAge>=2.43&&!screen.EggHatchBusy&&!eggReveal){eggReveal=true;stage="reveal";if(screen.CollectionPreviewDrawCalls<=0)throw new InvalidOperationException("Egg reward omitted its cosmetic preview.");}
                if(stage is not null && capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"egg-"+stage+".bmp"),size.X,size.Y);
            }
            if(evolutionSmoke && screen.EvolutionTransitionTime>1 && screen.EvolutionTransitionTime<2.5 && !transformWhite)
            {
                if(screen.EvolutionRaysOpacity!=1)throw new InvalidOperationException("Transformation rays did not fade in.");
                if(screen.EvolutionFloorDrawn)throw new InvalidOperationException("Evolution floor remained visible during white stage.");
                var savedForm=LegendEvolution.Discovery(collectionProfile.Storage.GetValueOrDefault("dt.legendevo.v1"),"bukamon").Current;
                if(screen.DisplayedEvolutionForm!="bukamon" || savedForm=="bukamon" || screen.EvolutionAnnouncementVisible)throw new InvalidOperationException("Evolution revealed the saved result before the transformation finished.");
                if(screen.EvolutionTreeKnownPortraits!=1)throw new InvalidOperationException("Inline evolution tree revealed the new discovery before the transformation finished.");
                transformWhite=true;if(capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"transform-white.bmp"),size.X,size.Y);
            }
            if(evolutionSmoke && screen.EvolutionTransitionTime>2.87 && screen.EvolutionTransitionTime<2.95 && !transformFlash)
            {
                transformFlash=true;if(capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"transform-flash.bmp"),size.X,size.Y);
            }
            if(evolutionSmoke && collectionIndex==11 && !screen.EvolutionBusy && screen.EvolutionAnnouncementOpacity is > .1 and < .95)
            {
                if(screen.EvolutionAnnouncementSlide is <= 0 or >= 6 || screen.EvolutionAnnouncementLines<1)throw new InvalidOperationException("Evolution announcement did not slide over its stage.");
                if(!settledPreview && !announcementFadedIn) { announcementFadedIn=true;if(capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"announcement-in.bmp"),size.X,size.Y); }
                if(settledPreview && !announcementFadedOut) { announcementFadedOut=true;if(capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"announcement-out.bmp"),size.X,size.Y); }
            }
            if (collectionIndex == 4 && cooldown == 1 && capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "collection.bmp"), size.X, size.Y);
            if (collectionIndex == collectionCommands.Length - 1 && cooldown == 1 && capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "egg.bmp"), size.X, size.Y);
            if(evolutionSmoke && collectionIndex==12 && cooldown==1)
            {
                if(treeScrolled && screen.EvolutionTreeScrollMax>0 && screen.EvolutionTreeScroll<=0) throw new InvalidOperationException("Illustrated evolution tree did not scroll.");
                if(screen.EvolutionTreePortraits!=screen.EvolutionTreeNodes) throw new InvalidOperationException("Evolution tree omitted portraits.");
                if(screen.EvolutionTreeNodes!=LegendEvolution.Tree("bukamon").Tiers.Sum(t=>t.Length)) throw new InvalidOperationException("Evolution tree omitted reachable forms.");
                if(capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"evolution-tree.bmp"),size.X,size.Y);
            }
            if(evolutionSmoke && collectionIndex==11 && screen.EvolutionTransitionTime==0 && !screen.EvolutionBusy && !settledPreview)
            {
                if(screen.DisplayedEvolutionForm!=LegendEvolution.Discovery(collectionProfile.Storage.GetValueOrDefault("dt.legendevo.v1"),"bukamon").Current || !screen.EvolutionAnnouncementVisible)throw new InvalidOperationException("Evolution completion did not reveal the saved form and announcement.");
                if(screen.EvolutionAnnouncementOpacity!=1 || screen.EvolutionAnnouncementSlide!=0 || screen.EvolutionAnnouncementLines<1)throw new InvalidOperationException("Evolution completion announcement did not settle into its stage overlay.");
                if(screen.EvolutionStageWidth!=330 || screen.EvolutionStageHeight!=(size.Y<=800?196:230))throw new InvalidOperationException("Evolution stage did not use the source responsive dimensions.");
                if(screen.EvolutionTreeNodes!=LegendEvolution.Tree("bukamon").Tiers.Sum(t=>t.Length) || screen.EvolutionTreePortraits!=screen.EvolutionTreeNodes || screen.EvolutionTreePortraitSize<25)throw new InvalidOperationException("Care panel omitted its simultaneously visible adaptive evolution tree.");
                if(screen.EvolutionAnnouncementInkPixels<20 || screen.EvolutionAnnouncementShadowPixels<20)throw new InvalidOperationException("Evolution announcement omitted its native glyph mask or gold shadow.");
                if(screen.EvolutionRaysOpacity!=0)throw new InvalidOperationException("Transformation rays did not fade out.");
                settledPreview=true;
                if(!screen.EvolutionFloorDrawn)throw new InvalidOperationException("Evolution floor did not return after transformation.");
                if(screen.EvolutionPreviewDrawCalls==0 || screen.EvolutionPreviewTime<=0) throw new InvalidOperationException("Evolution preview did not render and animate.");
                if(capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"evolution.bmp"),size.X,size.Y);
            }
            if (collectionIndex < collectionCommands.Length) return false;
            var collection = new CosmeticCollection(collectionProfile.Storage.GetValueOrDefault("dt.cosmetics.v1"));
            int price = CosmeticCollection.Price("legend", CosmeticCollection.Item("legend", "bukamon")!, DateOnly.FromDateTime(DateTime.Now));
            if (!collection.Owns("legend", "bukamon") || collection.Equipped("legend")?.GetValue<string>() != "bukamon" || collection.Bits != initialBits - price - 250 - (evolutionSmoke ? 200 : 0))
                throw new InvalidOperationException("Collection purchase/equipment smoke failed.");
            var wallet = System.Text.Json.Nodes.JsonNode.Parse(collection.Json)!;
            var legends = System.Text.Json.Nodes.JsonNode.Parse(collectionProfile.Storage.GetValueOrDefault("dt.legendevo.v1", "{\"legends\":{}}"))!;
            if (evolutionSmoke)
            {
                var record = legends["legends"]!["bukamon"]!;
                if (record["xp"]!.GetValue<int>() < 200 || record["form"]!.GetValue<string>() == "bukamon" || record["nonce"]!.GetValue<int>() != 1 || record["care"]!["food"]!.GetValue<int>() != 1 || record["path"]!.AsArray().Count != 1)
                    throw new InvalidOperationException("Evolution actions did not persist the expected level, form, care and nonce.");
                if(!transformWhite || !transformFlash || !announcementExpired || !announcementFadedIn || !announcementFadedOut)throw new InvalidOperationException("Evolution transition did not traverse white/flash and announcement fade stages.");
                Console.WriteLine("Evolution UI smoke passed: buy levels, feed, digivolve, revert and digivolve again.");
            }
            if (wallet["owned"]!["legend"]!.AsArray().Count < 3 && !legends["legends"]!.AsObject().Any(x => x.Value?["xp"]?.GetValue<int>() > 0))
                throw new InvalidOperationException("Egg granted neither an item nor duplicate Digi-Data.");
            if(!eggWobble||!eggCrack||!eggBurst||!eggReveal)throw new InvalidOperationException("Egg opening skipped its original wobble/crack/burst/reveal stages.");
            collectionVerified = true;
            farmInitialBits = collection.Bits;
            Console.WriteLine("Collection UI smoke passed: purchase, equipment, egg confirmation, wallet debit and hatch reward persistence.");
        }
        if (collectionProfile is not null && !farmVerified)
        {
            if (farmIndex == 2 && cooldown == 1 && capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "farm-feed.bmp"), size.X, size.Y);
            if (farmIndex == farmCommands.Length - 1 && cooldown == 1 && (screen.FarmDrawCalls < 150 || screen.FarmTheme != (evolutionSmoke ? "sewer" : "meadow")))
                throw new InvalidOperationException("Farm scene did not render the selected theme.");
            if (farmIndex == farmCommands.Length - 1 && cooldown == 1 && capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "farm.bmp"), size.X, size.Y);
            if (farmIndex < farmCommands.Length) return false;
            var save = System.Text.Json.Nodes.JsonNode.Parse(collectionProfile.Storage["dt.farm.v1"])!;
            var pet = save["pets"]!["tokomon"]!;
            if (collectionProfile.Progress.Bits != farmInitialBits + 4 - (evolutionSmoke ? 450 : 0) || Math.Abs(pet["goodBond"]!.GetValue<double>() - .73) > 1e-9 || save["meat"]!["n"]!.GetValue<int>() != 2 || save["lights"]!.GetValue<bool>() != true)
                throw new InvalidOperationException("Farm UI care/reward persistence failed.");
            if (evolutionSmoke && (save["cap"]!.GetValue<int>() != 8 || save["theme"]!.GetValue<string>() != "sewer" || !save["residents"]!.AsArray().Any(x => x?.GetValue<string>() == "tsunomon") || !save["residents"]!.AsArray().Any(x => x?.GetValue<string>() == "motimon")))
                throw new InvalidOperationException("Farm room management persistence failed.");
            farmVerified = true; Console.WriteLine("Farm UI smoke passed: feed, play, praise, pet, lights and saved rewards.");
        }
        if (index == 0 && cooldown == 1)
        {
            if(!screen.HomeLayoutFits || screen.HomeHeroArt is null || screen.HomeLegendDrawCalls==0)
                throw new InvalidOperationException("Home omitted its selected set artwork, live Little Legend or responsive panel layout.");
            foreach(string key in new[]{"start","multiplayer","homeRuleset","reconnectAudio"})
                if(!screen.Buttons.TryGetValue(key,out var point) || point.X<0 || point.X>=size.X || point.Y<0 || point.Y>=size.Y)
                    throw new InvalidOperationException("Home control is not visible inside the supported window: "+key);
            if (capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "menu.bmp"), size.X, size.Y);
        }
        if (index == 9 && cooldown == 1 && !planningCaptured)
        {
            if (screen.Session!.State.Players[0].Roster.Board.Count != 2 || screen.PlanningDrawCalls == 0 || screen.BoardDrawCalls <= 75)
                throw new InvalidOperationException("Match UI buy/place/model smoke failed.");
            if (capture is not null) FrameCapture.SaveBitmap(capture, size.X, size.Y);
            planningCaptured = true;
        }
        if (index == 5 && cooldown == 1)
        {
            if (screen.ShopPortraitDrawCalls != screen.Session!.State.Players[0].Shop.Slots.Count(id => id is not null))
                throw new InvalidOperationException("Shop cards did not render each available unit portrait.");
            var roster = screen.Session!.State.Players[0].Roster;
            if (!roster.Entries.Any(e => !e.Spot.IsBoard) || !screen.PlanningRenderedUnits.SetEquals(roster.Entries.Select(e => e.Unit.Id)))
                throw new InvalidOperationException("Planning scene omitted a purchased bench unit.");
            if (capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "bench.bmp"), size.X, size.Y);
        }
        if (index == 10)
        {
            battleFrames++;
            if (battleFrames == 45)
            {
                if (screen.BattleDrawCalls == 0 || screen.BoardDrawCalls <= (screen.BattleColumns==14?130:75)) throw new InvalidOperationException("Match UI battle did not render units and board scenery.");
                if (capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "battle.bmp"), size.X, size.Y);
                battleCaptured = true;
            }
        }
        if (index == 20)
        {
            pvpFrames++;
            if (pvpFrames == 45)
            {
                int expectedItems = screen.BattleColumns > 7 ? 0 : Math.Min(10, screen.Session!.State.Players[screen.ViewedPlayer].Items.Count);
                if (screen.ItemShelfDrawCalls != expectedItems) throw new InvalidOperationException("Scouted combat shelf did not follow inventory or Duos visibility.");
                if (!screen.WatchingMirrored || screen.BattleColumns != (screen.Session!.State.Duos ? 14 : 7) || screen.BattleDrawCalls == 0 || screen.OpponentCompanionDrawCalls == 0 || screen.CompanionShadowDrawCalls == 0 || screen.OpponentCompanionShadowDrawCalls == 0)
                    throw new InvalidOperationException("Match UI mirrored PvP smoke failed.");
                if (capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "pvp.bmp"), size.X, size.Y);
                pvpCaptured = true;
            }
        }
        if (screen.BattleRenderedBenchUnits is { } renderedBench && cooldown == 1 && screen.Session is { } active)
        {
            var player = active.State.Players[screen.ViewedPlayer];
            var expected = player.Roster.Entries.Where(e => !e.Spot.IsBoard).Select(e => e.Unit.Id).ToHashSet();
            if (screen.BattleColumns > 7 && player.Team is { } team)
                expected.UnionWith(active.State.Teams[team].Members.Where(p => p.Id != player.Id)
                    .SelectMany(p => p.Roster.Entries.Where(e => !e.Spot.IsBoard)).Select(e => e.Unit.Id));
            if (!renderedBench.SetEquals(expected)) throw new InvalidOperationException("Combat bench does not match the viewed player and partner.");
            if (expected.Count > 0 && !combatBenchVerified && capture is not null)
                FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "reserve.bmp"), size.X, size.Y);
            combatBenchVerified |= expected.Count > 0;
        }
        if (index == 20 && cooldown == 1 && scoutSwitches > 0)
        {
            if (screen.WatchingMirrored != (scoutSwitches == 2))
                throw new InvalidOperationException("Switching sides of the same fight did not update the camera orientation.");
            if (scoutSwitches == 1 && capture is not null)
                FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "combat-bench.bmp"), size.X, size.Y);
        }
        if (index < commands.Length || cooldown > 0) return false;
        if (resultRounds.Count < 3) throw new InvalidOperationException("Result banner smoke did not observe enough settled rounds.");
        if (screen.Session!.State.Duos && !combinedTraitsVerified) throw new InvalidOperationException("Duos trait fixture did not exercise counts different from the solo board.");
        if (!planningCaptured || !battleCaptured || !pvpCaptured || !combatBenchVerified || screen.CompletedRounds != 5 || screen.Session!.Rounds.Phase != MatchPhase.Planning || screen.AiEmotesShown == 0)
            throw new InvalidOperationException($"Match UI round transition smoke failed: planning={planningCaptured}, battle={battleCaptured}, pvp={pvpCaptured}, bench={combatBenchVerified}, rounds={screen.CompletedRounds}, phase={screen.Session!.Rounds.Phase}, emotes={screen.AiEmotesShown}.");
        Console.WriteLine("Match UI smoke passed: menu, carousel, purchase, placement/drag/drop, sale/partner rules, 3D planning, combat reserve, scouting both sides, PvE, mirrored PvP, five settlements and round advances.");
        gameplayVerified = true;
        return false;
    }
}
