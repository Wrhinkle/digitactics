using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Networking;
using DigiTactics.Presentation;
using DigiTactics.Rules;
using DigiTactics.Storage;

internal static class MysteryLootIntegrationChecks
{
    public static int Run(string root)
    {
        int checks = 0;
        void Check(bool value, string message) { checks++; if (!value) throw new InvalidOperationException("Mystery settlement: " + message); }
        var catalog = ContentCatalog.Load(Path.Combine(root, "content/catalog.json"));
        var rules = catalog.Variants.Single(rules => rules.Id == "faction-wars/hyper");
        var seats = Enumerable.Range(0, 8).Select(seat => new MatchSeat("Tamer " + seat, false)).ToArray();
        int[][] teams = [[0, 4], [1, 5], [2, 6], [3, 7]];
        var day = new DateOnly(2026, 10, 18);
        foreach (bool duos in new[] { false, true })
        {
            string format = duos ? "duos" : "solo";
            var session = new MatchPlanningSession(rules, 143, seats, duos, teams);
            Check(!MysteryRoundProjection.From(session, 0).Participated, format + " unplayed match has no participation");
            string?[] saves = new string?[8]; bool pve = false, pvp = false; int limit = 0;
            while (!(pve && pvp) && limit++ < 12)
            {
                session.StartCombat(); session.FinishCombat();
                if (session.Rounds.Results.Count > 0)
                {
                    pve |= session.Rounds.Results.Any(result => result.Match.Pve);
                    pvp |= session.Rounds.Results.Any(result => !result.Match.Pve);
                    uint combatRandom = session.State.Random.State;
                    for (int seat = 0; seat < 8; seat++)
                    {
                        var projection = MysteryRoundProjection.From(session, seat);
                        Check(projection.Participated && projection.Set == "faction-wars" && projection.Duos == duos && projection.Seed == 143 && projection.Seat == seat &&
                            projection.Stage == session.Rounds.Position.Stage && projection.Round == session.Rounds.Position.Round, format + " actual settled combat projects every participant and original ledger identity");
                        var update = MysteryLoot.RoundEnd(saves[seat], projection, day, () => 0); saves[seat] = update.Loot;
                        Check(update.Changed && MysteryLoot.Inventory(update.Loot) is { Daily: 0, DayProgress: 1 }, format + " settled combat advances only one active day per local profile");
                        Check(!MysteryLoot.RoundEnd(saves[seat], projection, day, () => throw new InvalidOperationException("Settled round rerolled.")).Changed, format + " duplicate real settlement does not re-award");
                    }
                    Check(session.State.Random.State == combatRandom, format + " local mystery rewards never consume deterministic combat randomness");
                }
                else Check(!MysteryRoundProjection.From(session, 0).Participated, format + " carousel settlement has no eligible combat");
                if (!(pve && pvp)) session.Advance();
            }
            Check(pve && pvp, format + " actual combat fixture exercised both PvE and PvP settlement");
            Check(!MysteryRoundProjection.From(session, -1).Participated && !MysteryRoundProjection.From(session, 8).Participated, format + " spectator and out-of-range seat have no player projection");
            foreach (string scenario in new[] { "ghost", "duel", "pve" })
            {
                // Isolate one settled match so a ghost donor's separate real fight cannot hide a bug.
                var partial = new MatchPlanningSession(rules, 143, seats, duos, teams);
                bool isPve = scenario == "pve", ghost = scenario == "ghost";
                partial.Rounds.BeginCombat();
                partial.Resolve([new ResolvedMatch(0, isPve ? null : 1, isPve, ghost, 0, [], new Dictionary<string, int>(), new Dictionary<string, int>(),
                    duos && !isPve ? 0 : null, duos && !isPve ? 1 : null)]);
                for (int seat = 0; seat < 8; seat++)
                {
                    bool expected = seat == 0 || duos && !isPve && seat == 4 || !isPve && !ghost && (seat == 1 || duos && seat == 5);
                    Check(MysteryRoundProjection.From(partial, seat).Participated == expected, format + " " + scenario + " settled participation excludes ghost donors and unrelated seats, includes actual partners");
                }
            }
        }
        string directory = Path.Combine(Path.GetTempPath(), "digitactics-mystery-network-" + Guid.NewGuid().ToString("N"));
        try
        {
            foreach (bool duos in new[] { false, true })
            {
                string format = duos ? "duos" : "solo", target = Path.Combine(directory, format);
                var lobby = JsonNode.Parse("{\"host\":\"123\",\"name\":\"Room\",\"hostName\":\"Host\",\"status\":\"playing\",\"slots\":{\"0\":{\"uid\":\"123\",\"name\":\"Host\"},\"1\":{\"uid\":\"234\",\"name\":\"Guest\"}}}")!.AsObject();
                var host = new NetworkRoom("999", "123", lobby); var guest = new NetworkRoom("999", "234", lobby);
                host.Members(["123", "234"], "123", 10000); guest.Members(["123", "234"], "123", 10000);
                var packets = new Queue<(string Sender, RoomPacket Packet)>();
                host.Send += packet => packets.Enqueue(("123", packet)); guest.Send += packet => packets.Enqueue(("234", packet));
                long now = 10000;
                void Network()
                {
                    while (packets.TryDequeue(out var packet))
                        Check((packet.Packet.Recipient == "123" ? host : guest).Receive(packet.Sender, packet.Packet.Text, now), format + " authenticated room packet accepted");
                }
                using var authority = new MatchReplica(host, catalog); using var live = new MatchReplica(guest, catalog);
                var profile = new NativeProfileStore(target); var progress = new MysteryLootProgress(profile);
                var observerProfile = new NativeProfileStore(target + "-spectator"); var observer = new MysteryLootProgress(observerProfile);
                int appliedEnds = 0, rejectedEnds = 0, rolls = 0, blockedEnds = 0;
                // Exercise the real transport/replica event contract used by MatchScreen.NetworkApplied.
                // This headless adapter cannot validate the GPU-owned MatchScreen callback itself.
                live.Applied += (entry, result) =>
                {
                    if (entry.GetProperty("k").GetString() != "end") return;
                    if (!result.Applied) { rejectedEnds++; return; }
                    appliedEnds++;
                    if (live.LastDesync is not null) { blockedEnds++; return; }
                    progress.RecordRound(MysteryRoundProjection.From(live.Match!.Session, 1), day, () => { rolls++; return 0; });
                    observer.RecordRound(MysteryRoundProjection.From(live.Match.Session, -1), day, () => throw new InvalidOperationException("Spectator consumed reward randomness."));
                };
                Check(host.PublishInit(JsonSerializer.SerializeToElement(new { seed = 143, at = now, planTime = 30, setId = "faction-wars", pace = "hyper", format,
                    slots = seats.Select(seat => new { name = seat.Name, human = seat.Human }).ToArray(), teams })), format + " authoritative match initialized");
                Network(); authority.Pump(now); live.Pump(now);
                Check(!profile.Exists && appliedEnds == 0 && authority.Match!.Check() == live.Match!.Check(), format + " initialization replays without local awards");
                void Append(JsonObject entry, string? digest = null)
                {
                    now += 100000; entry["at"] = now; entry["d"] = digest ?? authority.Match!.Check();
                    Check(host.Append(host.Log.Count, JsonSerializer.SerializeToElement(entry), now), format + " host appends valid phase entry");
                    Network(); authority.Pump(now); live.Pump(now);
                }
                void NextDecision()
                {
                    var entry = authority.Match!.Decide(now + 100000, new HashSet<int>(), new HashSet<int>());
                    Check(entry is not null, format + " real match produces next phase"); Append(entry!);
                }
                int limit = 0;
                while (!(live.Match!.Session.Rounds.Position.Stage >= 2 && live.Match.Session.Rounds.Phase == MatchPhase.Result) && limit++ < 50) NextDecision();
                Check(appliedEnds > 0 && progress.Inventory is { Daily: 0, DayProgress: 1, Vault: 1, Keys: 1 } && rolls == 2, format + " live authenticated stage-two settlement advances active day and awards box/key once");
                Check(!observerProfile.Exists && observerProfile.Storage.Count == 0, format + " spectator receives actual end events but never records drops");
                string path = Path.Combine(target, "profile.json"), saved = File.ReadAllText(path); int endsBefore = appliedEnds;
                Append(new JsonObject { ["k"] = "end" }); live.Pump(++now);
                Check(rejectedEnds == 1 && appliedEnds == endsBefore && rolls == 2 && File.ReadAllText(path) == saved, format + " duplicate end entry and repeated pump cannot award twice");
                var reconnectProfile = new NativeProfileStore(target + "-reconnect"); var reconnectProgress = new MysteryLootProgress(reconnectProfile);
                using (var reconnect = new MatchReplica(guest, catalog))
                {
                    int catchupEnds = 0;
                    reconnect.Applied += (entry, result) =>
                    {
                        if (entry.GetProperty("k").GetString() == "end" && result.Applied)
                        {
                            catchupEnds++;
                            reconnectProgress.RecordRound(MysteryRoundProjection.From(reconnect.Match!.Session, 1), day.AddDays(1), () => 0);
                        }
                    };
                    reconnect.Pump(now);
                    Check(reconnect.Match!.Check() == authority.Match!.Check() && catchupEnds == 0 && !reconnectProfile.Exists, format + " complete reconnect catch-up cannot grant even a new-day daily box");
                }
                int rebuilt = live.Rebuilds; guest.RequestSnapshot(now); Network(); live.Pump(now);
                Check(live.Rebuilds == rebuilt + 1 && appliedEnds == endsBefore && File.ReadAllText(path) == saved, format + " replacement snapshot rebuild replays all settlements without awards");
                while (authority.Match!.Session.Rounds.Phase != MatchPhase.Combat && limit++ < 60) NextDecision();
                Check(authority.Match!.Session.Rounds.Phase == MatchPhase.Combat, format + " next real combat reached before desync test");
                live.Match!.Session.State.Players[0].Health--;
                Append(new JsonObject { ["k"] = "end" });
                Check(live.LastDesync is null && live.Match.Check() == authority.Match!.Check() && appliedEnds == endsBefore && rolls == 2 && File.ReadAllText(path) == saved,
                    format + " live mismatch repairs through replay without granting the replayed settlement");
                Append(new JsonObject { ["k"] = "next" }, "deadbeef");
                Check(live.LastDesync is { AfterReplay: true } && File.ReadAllText(path) == saved, format + " persistent authenticated digest disagreement records no loot");
                while (authority.Match!.Session.Rounds.Phase != MatchPhase.Combat && limit++ < 70) NextDecision();
                Append(new JsonObject { ["k"] = "end" });
                Check(blockedEnds == 1 && appliedEnds == endsBefore + 1 && rolls == 2 && File.ReadAllText(path) == saved, format + " later live end remains guarded while prior disagreement is unresolved");
            }
        }
        finally
        {
            string resolved = Path.GetFullPath(directory);
            if (!resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(resolved).StartsWith("digitactics-mystery-network-", StringComparison.Ordinal))
                throw new InvalidOperationException("Unsafe mystery network cleanup target.");
            if (Directory.Exists(resolved)) Directory.Delete(resolved, true);
        }
        Console.WriteLine($"Mystery integration: {checks:N0} real settlement, ghost participation, transport, reconnect and desync checks passed.");
        return checks;
    }
}
