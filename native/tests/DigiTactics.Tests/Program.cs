using System.Text.Json;
using DigiTactics.Rules;

string root = args.Length > 0 ? Path.GetFullPath(args[0]) : Path.GetFullPath("native");
if (args.Contains("--steam-native-only")) { SteamMessageChecks.Run(root); SteamClientChecks.Run(root); SteamMatchmakingChecks.Run(root); SteamRoomSessionChecks.Run(root); SteamFriendsChecks.Run(root); return; }
if (args.Contains("--network-only")) { NetworkProtocolChecks.Run(root); ReplicatedLogChecks.Run(root); LobbyAuthorityChecks.Run(root); NetworkRoomChecks.Run(); return; }
if (args.Contains("--settings-only")) { AudioSettingsChecks.Run(); return; }
if (args.Contains("--soundboard-only")) { SoundboardChecks.Run(); return; }
if (args.Contains("--recent-players-only")) { RecentPlayerChecks.Run(root); return; }
if (args.Contains("--profile-only")) { LegacyProfileChecks.Run(root); return; }
if (args.Contains("--actions-only")) { ActionParity.Run(root); return; }
if (args.Contains("--meshes-only")) { MeshParity.Run(root); return; }
if (args.Contains("--materials-only")) { MaterialParity.Run(root); return; }
if (args.Contains("--assets-only")) { ModelAssetParity.Run(root); TextureParity.Run(root); MotionParity.Run(root); CompanionModelChecks.Run(root); return; }
if (args.Contains("--geometry-only")) { MaterialParity.Run(root); MeshParity.Run(root); RigParity.Run(root); return; }
if (args.Contains("--companion-motion-only")) { CompanionMotionChecks.Run(root); return; }
if (args.Contains("--companion-playback-only")) { CompanionPlaybackChecks.Run(root); return; }
if (args.Contains("--farm-choreography-only")) { FarmChoreographyChecks.Run(root); return; }
var catalog = ContentCatalog.Load(Path.Combine(root, "content/catalog.json"));
if (args.Contains("--lockstep-only")) { LockstepParity.Run(root, catalog); return; }
if (args.Contains("--match-replica-only")) { MatchReplicaChecks.Run(catalog); return; }
if (args.Contains("--match-host-only")) { MatchHostChecks.Run(catalog); return; }
if (args.Contains("--history-only")) { MatchHistoryChecks.Run(root, catalog); return; }
if (args.Contains("--network-history-only")) { NetworkHistoryChecks.Run(catalog); return; }
if (args.Contains("--rewards-only")) { MatchRewardChecks.Run(root, catalog); LegendRewardChecks.Run(root); return; }
if (args.Contains("--cosmetics-only")) { CosmeticChecks.Run(root); return; }
if (args.Contains("--eggs-only")) { EggChecks.Run(root); return; }
if (args.Contains("--evolution-only")) { EvolutionChecks.Run(root); return; }
if (args.Contains("--farm-only")) { FarmSimulationChecks.Run(root); FarmCareChecks.Run(root); return; }
if (args.Contains("--training-only")) { TrainingGameChecks.Run(root); return; }
if (args.Contains("--audio-cues-only")) { AudioCueParity.Run(root, catalog); return; }
if (args.Contains("--items-only")) { AiInventoryParity.Run(root, catalog); ColiseumCoreParity.Run(root, catalog); SpecialConsumableParity.Run(root, catalog); CopyConsumableParity.Run(root, catalog); ConsumableParity.Run(root, catalog); PlanningItemParity.Run(root, catalog); RadiantOfferParity.Run(root, catalog); return; }
if (args.Contains("--custom-offers-only")) { ColiseumChoiceParity.Run(root, catalog); CustomOfferParity.Run(root, catalog); return; }
if (args.Contains("--rounds-only")) { ColiseumRoundParity.Run(root, catalog); RoundFlowParity.Run(root, catalog); return; }
if (args.Contains("--offers-only")) { OfferParity.Run(root, catalog); return; }
if (args.Contains("--mode-loot-only")) { LootParity.Run(root, catalog, true); return; }
if (args.Contains("--loot-only")) { LootParity.Run(root, catalog); return; }
if (args.Contains("--settlement-only")) { ColiseumSettlementParity.Run(root, catalog); SettlementParity.Run(root, catalog); return; }
if (args.Contains("--match-only")) { MatchStateParity.Run(root, catalog); return; }
if (args.Contains("--pve-waves-only")) { PveWaveParity.Run(root, catalog); return; }
if (args.Contains("--match-actions-only")) { MatchActionsParity.Run(root, catalog); return; }
if (args.Contains("--full-match-only")) { FullMatchParity.Run(root, catalog); return; }
if (args.Contains("--settlement-effects-only")) { SettlementEffectsParity.Run(root, catalog); ArcaneSettlementParity.Run(root, catalog); DragonSettlementParity.Run(root, catalog); FatesSettlementParity.Run(root, catalog); UnderlordsSettlementParity.Run(root, catalog); GalaxiesSettlementParity.Run(root, catalog); ColiseumSettlementParity.Run(root, catalog); AutoChessPriestParity.Run(root, catalog); return; }
if (args.Contains("--combat-projection-only")) { CombatProjectionParity.Run(root, catalog); return; }
if (args.Contains("--planning-session-only")) { PlanningSessionParity.Run(root, catalog); return; }
if (args.Contains("--composed-ai-only")) { ComposedAiParity.Run(root, catalog); return; }
if (args.Contains("--offer-effects-only")) { OfferEffectsParity.Run(root, catalog); return; }
if (args.Contains("--faction-planning-only")) { FactionPlanningParity.Run(root, catalog); return; }
if (args.Contains("--unit-lifecycle-only")) { UnitLifecycleParity.Run(root, catalog); return; }
if (args.Contains("--reckoning-ai-only")) { ReckoningAiParity.Run(root, catalog); return; }
if (args.Contains("--reckoning-planning-only")) { ReckoningPlanningParity.Run(root, catalog); return; }
if (args.Contains("--arcane-settlement-only")) { ArcaneSettlementParity.Run(root, catalog); return; }
if (args.Contains("--arcane-ai-only")) { ArcaneAiParity.Run(root, catalog); return; }
if (args.Contains("--arcane-augments-only")) { ArcaneAugmentParity.Run(root, catalog); return; }
if (args.Contains("--arcane-rounds-only")) { ArcaneRoundParity.Run(root, catalog); return; }
if (args.Contains("--autochess-priest-only")) { AutoChessPriestParity.Run(root, catalog); return; }
if (args.Contains("--galaxy-augments-only")) { GalaxyAugmentParity.Run(root, catalog); return; }
if (args.Contains("--gizmos-augments-only")) { GizmosAugmentParity.Run(root, catalog); return; }
if (args.Contains("--gizmos-planning-only")) { GizmosPlanningParity.Run(root, catalog); return; }
if (args.Contains("--fates-augments-only")) { FatesAugmentParity.Run(root, catalog); return; }
if (args.Contains("--fates-trait-rewards-only")) { FatesTraitRewardParity.Run(root, catalog); return; }
if (args.Contains("--dragon-augments-only")) { DragonAugmentParity.Run(root, catalog); return; }
if (args.Contains("--dragon-settlement-only")) { DragonSettlementParity.Run(root, catalog); return; }
if (args.Contains("--dragon-planning-only")) { DragonPlanningParity.Run(root, catalog); return; }
if (args.Contains("--underlords-settlement-only")) { UnderlordsSettlementParity.Run(root, catalog); return; }
if (args.Contains("--underlords-planning-only")) { UnderlordsPlanningParity.Run(root, catalog); return; }
if (args.Contains("--underlords-tamers-only")) { UnderlordsTamerParity.Run(root, catalog); return; }
if (args.Contains("--match-placement-only")) { MatchPlacementParity.Run(root, catalog); return; }
if (args.Contains("--fates-settlement-only")) { FatesSettlementParity.Run(root, catalog); return; }
if (args.Contains("--fates-ai-only")) { FatesAiParity.Run(root, catalog); return; }
if (args.Contains("--galaxies-settlement-only")) { GalaxiesSettlementParity.Run(root, catalog); return; }
if (args.Contains("--galaxies-planning-only")) { GalaxiesPlanningParity.Run(root, catalog); return; }
if (args.Contains("--opening-rounds-only")) { OpeningRoundParity.Run(root, catalog); return; }
if (args.Contains("--match-start-only")) { MatchStartParity.Run(root, catalog); return; }
if (args.Contains("--match-shop-rolls-only")) { MatchShopRollParity.Run(root, catalog); return; }
if (args.Contains("--dragon-shops-only")) { DragonShopParity.Run(root, catalog); return; }
if (args.Contains("--fates-partners-only")) { FatesPartnerParity.Run(root, catalog); return; }
if (args.Contains("--shop-rolls-only")) { ShopRollParity.Run(root, catalog); return; }
if (args.Contains("--autochess-planning-only")) { AutoChessPlanningParity.Run(root, catalog); return; }
if (args.Contains("--carousel-only")) { CarouselParity.Run(root, catalog); return; }
if (args.Contains("--ai-turns-only")) { AiTurnParity.Run(root, catalog); return; }
if (args.Contains("--arcane-offers-only")) { ArcaneOfferParity.Run(root, catalog); return; }
if (args.Contains("--underlords-offers-only")) { RelicUpgradeParity.Run(root, catalog, true); return; }
if (args.Contains("--relic-upgrades-only")) { RelicUpgradeParity.Run(root, catalog); return; }
if (args.Contains("--ai-custom-offers-only")) { AiOfferParity.Run(root, catalog, true); return; }
if (args.Contains("--ai-offers-only")) { AiOfferParity.Run(root, catalog); return; }
if (args.Contains("--duos-planning-only")) { PortalParity.Run(root, catalog, true); return; }
if (args.Contains("--portals-only")) { PortalParity.Run(root, catalog); return; }
if (args.Contains("--ai-shopping-only")) { AiShoppingParity.Run(root, catalog); return; }
if (args.Contains("--ai-decisions-only")) { AiDecisionParity.Run(root, catalog); return; }
if (args.Contains("--planning-only")) { AiDecisionParity.Run(root, catalog); AiHolderParity.Run(root, catalog); ColiseumShopParity.Run(root, catalog); PlanningParity.Run(root, catalog); PlanningShopParity.Run(root, catalog); return; }
if (args.Contains("--economy-only")) { EconomyParity.Run(root, catalog); return; }
if (args.Contains("--choreography-only")) { ChoreographyParity.Run(root, catalog); return; }
using var fixture = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "tests/fixtures/parity.json")));
int assertions = SoundboardChecks.Run() + RecentPlayerChecks.Run(root) + NetworkHistoryChecks.Run(catalog) + MatchHostChecks.Run(catalog) + MatchReplicaChecks.Run(catalog) + LockstepParity.Run(root, catalog) + MatchActionsParity.Run(root, catalog) + PveWaveParity.Run(root, catalog) + FullMatchParity.Run(root, catalog) + SettlementEffectsParity.Run(root, catalog) + CombatProjectionParity.Run(root, catalog) + PlanningSessionParity.Run(root, catalog) + ComposedAiParity.Run(root, catalog) + OfferEffectsParity.Run(root, catalog) + FactionPlanningParity.Run(root, catalog) + UnitLifecycleParity.Run(root, catalog) + ReckoningAiParity.Run(root, catalog) + ReckoningPlanningParity.Run(root, catalog) + ArcaneSettlementParity.Run(root, catalog) + ArcaneAiParity.Run(root, catalog) + ArcaneAugmentParity.Run(root, catalog) + ArcaneRoundParity.Run(root, catalog) + AutoChessPriestParity.Run(root, catalog) + GalaxyAugmentParity.Run(root, catalog) + GizmosAugmentParity.Run(root, catalog) + GizmosPlanningParity.Run(root, catalog) + FatesAugmentParity.Run(root, catalog) + FatesTraitRewardParity.Run(root, catalog) + DragonAugmentParity.Run(root, catalog) + DragonSettlementParity.Run(root, catalog) + DragonPlanningParity.Run(root, catalog) + UnderlordsSettlementParity.Run(root, catalog) + UnderlordsPlanningParity.Run(root, catalog) + UnderlordsTamerParity.Run(root, catalog) + MatchPlacementParity.Run(root, catalog) + FatesSettlementParity.Run(root, catalog) + FatesAiParity.Run(root, catalog) + GalaxiesSettlementParity.Run(root, catalog) + GalaxiesPlanningParity.Run(root, catalog) + OpeningRoundParity.Run(root, catalog) + MatchStartParity.Run(root, catalog) + MatchShopRollParity.Run(root, catalog) + DragonShopParity.Run(root, catalog) + FatesPartnerParity.Run(root, catalog) + ShopRollParity.Run(root, catalog) + AutoChessPlanningParity.Run(root, catalog) + CarouselParity.Run(root, catalog) + AiTurnParity.Run(root, catalog) + ArcaneOfferParity.Run(root, catalog) + RelicUpgradeParity.Run(root, catalog, true) + RelicUpgradeParity.Run(root, catalog) + AiOfferParity.Run(root, catalog, true) + AiOfferParity.Run(root, catalog) + PortalParity.Run(root, catalog, true) + PortalParity.Run(root, catalog) + AiShoppingParity.Run(root, catalog) + AiDecisionParity.Run(root, catalog) + AiInventoryParity.Run(root, catalog) + AiHolderParity.Run(root, catalog) + ColiseumSettlementParity.Run(root, catalog) + ColiseumRoundParity.Run(root, catalog) + ColiseumShopParity.Run(root, catalog) + ColiseumChoiceParity.Run(root, catalog) + ColiseumCoreParity.Run(root, catalog) + SpecialConsumableParity.Run(root, catalog) + CopyConsumableParity.Run(root, catalog) + ConsumableParity.Run(root, catalog) + RadiantOfferParity.Run(root, catalog) + PlanningItemParity.Run(root, catalog) + CustomOfferParity.Run(root, catalog) + RoundFlowParity.Run(root, catalog) + OfferParity.Run(root, catalog) + LootParity.Run(root, catalog, true) + LootParity.Run(root, catalog) + SettlementParity.Run(root, catalog) + MatchStateParity.Run(root, catalog) + EconomyParity.Run(root, catalog) + PlanningParity.Run(root, catalog) + PlanningShopParity.Run(root, catalog);
void Check(bool condition, string message)
{
    assertions++;
    if (!condition) throw new InvalidOperationException(message);
}

foreach (var trace in fixture.RootElement.GetProperty("rng").EnumerateArray())
{
    var random = new DeterministicRandom(trace.GetProperty("seed").GetUInt32());
    foreach (var expected in trace.GetProperty("expected").EnumerateArray())
        Check(random.Next() == expected.GetDouble(), "RNG diverged from JavaScript.");
}
foreach (var trace in fixture.RootElement.GetProperty("rolls").EnumerateArray())
{
    var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
    var random = new DeterministicRandom(trace.GetProperty("seed").GetUInt32());
    var pool = new ShopPool(rules, random);
    var excluded = trace.GetProperty("maxed").EnumerateArray().Select(e => e.GetString()!).ToHashSet();
    int roll = 0;
    foreach (var expected in trace.GetProperty("expected").EnumerateArray())
    {
        string? actual = pool.Roll(1 + roll % (rules.Odds.Length - 1), excluded);
        Check(actual == expected.GetString(), $"Shop parity failed in {rules.Id}, roll {roll}.");
        roll++;
    }
}
foreach (var rules in catalog.Variants)
{
    var line = rules.Lines.First(l => l.Tier == 1);
    var roster = new Roster();
    int copies = line.Copies * line.Copies;
    for (int i = 0; i < copies; i++) Check(roster.Add(line), $"Could not add copy {i} in {rules.Id}.");
    Check(roster.Units.Count == 1 && roster.Units[0].Stage == 2, "Three-star merge failed.");
    Check(roster.MaxedLines().Contains(line.Id), "Three-star exclusion absent.");
    Check(!new Roster().MaxedLines().Contains(line.Id), "Exclusion leaked to another player.");
    roster.Remove(roster.Units[0].Id);
    Check(!roster.MaxedLines().Contains(line.Id), "Sold three-star still excluded.");

    var random = new DeterministicRandom(1);
    var pool = new ShopPool(rules, random);
    var all = rules.Lines.Select(l => l.Id).ToHashSet();
    Check(pool.Roll(1, all) is null, "Exhausted eligible pool should produce an empty slot.");
    Check(pool.Remaining(line.Id) == rules.PoolSizes[line.Tier], "Excluded roll consumed inventory.");

    var session = new ShopSession(rules, 143);
    Check(!session.Buy(-1) && !session.Buy(session.Slots.Count), "Invalid shop slot accepted.");
    int initialGold = session.Gold;
    int slot = Enumerable.Range(0, session.Slots.Count).First(i => session.Slots[i] is not null);
    var purchase = session.Line(session.Slots[slot]!);
    Check(session.Buy(slot), "Valid purchase failed.");
    Check(session.Gold == initialGold - purchase.Price && session.Slots[slot] is null, "Purchase not atomic.");
    Check(!session.Buy(slot), "Duplicate purchase accepted.");
}
foreach (var trace in fixture.RootElement.GetProperty("grids").EnumerateArray())
{
    var grid = new HexGrid(trace.GetProperty("cols").GetInt32(), trace.GetProperty("rows").GetInt32());
    int pair = 0;
    foreach (var distance in trace.GetProperty("distances").EnumerateArray())
    {
        Check(grid[pair / grid.Count].DistanceTo(grid[pair % grid.Count]) == distance.GetInt32(), "Hex distance parity failed.");
        pair++;
    }
    foreach (var path in trace.GetProperty("paths").EnumerateArray())
    {
        var enemies = path.GetProperty("enemies").EnumerateArray().Select(e => grid[e.GetInt32()]).ToArray();
        var occupied = path.GetProperty("occupied").EnumerateArray().Select(e => e.GetBoolean()).ToArray();
        var result = grid.StepTowardEnemy(grid[path.GetProperty("start").GetInt32()], path.GetProperty("range").GetDouble(), enemies, occupied);
        var expected = path.GetProperty("expected");
        Check(expected.ValueKind == JsonValueKind.Null ? result is null : result is { } step &&
            grid.Index(step.Cell) == expected.GetProperty("step").GetInt32() && step.TargetIndex == expected.GetProperty("target").GetInt32(),
            "Combat movement/target tie-breaking diverged from JavaScript.");
    }
}
using var unitFixtures = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "tests/fixtures/unit-stats.json")));
int forms = 0;
foreach (var trace in unitFixtures.RootElement.EnumerateArray())
{
    var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
    var line = rules.Lines.Concat(rules.ExtraLines).Single(l => l.Id == trace.GetProperty("line").GetString());
    int stage = trace.GetProperty("stage").GetInt32();
    var stats = UnitValues.Stats(line, stage);
    var expected = trace.GetProperty("stats");
    Check(stats.MaxHealth == expected.GetProperty("maxHp").GetDouble(), "Unit health scaling mismatch.");
    Check(stats.AttackDamage == expected.GetProperty("ad").GetDouble(), "Unit attack scaling mismatch.");
    Check(stats.AttackSpeed == expected.GetProperty("as").GetDouble(), "Unit attack speed mismatch.");
    Check(stats.Range == expected.GetProperty("range").GetDouble(), "Unit range mismatch.");
    Check(stats.Armor == expected.GetProperty("armor").GetDouble(), "Unit armor mismatch.");
    Check(stats.MagicResist == expected.GetProperty("mr").GetDouble(), "Unit magic resistance mismatch.");
    Check(stats.MaxMana == expected.GetProperty("maxMana").GetDouble(), "Unit mana cap mismatch.");
    Check(stats.StartMana == expected.GetProperty("mana").GetDouble(), "Unit initial mana mismatch.");
    Check(UnitValues.AbilityValue(line, stage) == trace.GetProperty("ability").GetDouble(), "Ability scaling mismatch.");
    Check(UnitValues.SellValue(line, stage) == trace.GetProperty("sell").GetInt32(), $"Sell value mismatch: {rules.Id}/{line.Id}/{stage}.");
    forms++;
}
using var damageFixtures = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "tests/fixtures/damage.json")));
foreach (var trace in damageFixtures.RootElement.EnumerateArray())
{
    var request = trace.GetProperty("request").Deserialize<DamageRequest>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    var actual = DamageRules.Resolve(request);
    var expected = trace.GetProperty("expected");
    // The reference records HP/shield differences, which can lose sub-ulp precision through subtraction.
    bool Close(double a, double b) => Math.Abs(a - b) <= 1e-10 * Math.Max(1, Math.Abs(b));
    Check(Close(actual.Amount, expected.GetProperty("amount").GetDouble()), "Damage amount mismatch.");
    Check(Close(actual.ShieldAbsorbed, expected.GetProperty("shieldAbsorbed").GetDouble()), "Shield absorption mismatch.");
    Check(Close(actual.HealthDamage, expected.GetProperty("healthDamage").GetDouble()), "Health damage mismatch.");
    Check(actual.ManaAfter == expected.GetProperty("manaAfter").GetDouble(), "Damage mana gain mismatch.");
}
assertions += ModelAssetParity.Run(root);
assertions += CompanionModelChecks.Run(root);
assertions += CompanionMotionChecks.Run(root);
assertions += CompanionPlaybackChecks.Run(root);
assertions += FarmChoreographyChecks.Run(root);
assertions += TextureParity.Run(root);
assertions += MaterialParity.Run(root);
assertions += ActionParity.Run(root);
assertions += ChoreographyParity.Run(root, catalog);
assertions += AudioCueParity.Run(root, catalog);
assertions += AudioSettingsChecks.Run();
assertions += MatchHistoryChecks.Run(root, catalog);
assertions += MatchRewardChecks.Run(root, catalog);
assertions += LegendRewardChecks.Run(root);
assertions += CosmeticChecks.Run(root);
assertions += EggChecks.Run(root);
assertions += EvolutionChecks.Run(root);
assertions += FarmSimulationChecks.Run(root);
assertions += FarmCareChecks.Run(root);
assertions += NetworkProtocolChecks.Run(root);
assertions += ReplicatedLogChecks.Run(root);
assertions += LobbyAuthorityChecks.Run(root);
assertions += NetworkRoomChecks.Run();
assertions += TrainingGameChecks.Run(root);
assertions += LegacyProfileChecks.Run(root);
assertions += MotionParity.Run(root);
assertions += MeshParity.Run(root);
assertions += RigParity.Run(root);
assertions += CombatSceneParity.Run(root, catalog);
assertions += CombatEventParity.Run(root, catalog);
assertions += await CombatParity.Run(root, catalog);
Console.WriteLine($"Passed {assertions} native checks: reference battles, RNG, shop, movement, {forms} unit forms, 1,200 damage traces, Solo/Duos geometry, and roster/pool/purchase rules across {catalog.Variants.Length} variants.");
