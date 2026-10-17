using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class PortalParity
{
    private sealed class Hooks : IMatchPortalHooks
    {
        public DuosPlanning? Planning { get; set; }
        public RoundPosition Round { get; set; }
        public List<(string Name, int Player, int? Unit)> Events { get; } = [];
        public void Combined(MatchPlayer p, PlanningUnit u, IReadOnlyList<PlanningUnit> sources) => Events.Add(("onCombine", p.Id, u.Id));
        public void PartnerCombined(MatchPlayer p, MatchPlayer owner, PlanningUnit u) { Events.Add(("onPartnerCombine", p.Id, u.Id)); Planning?.PartnerCombined(p, Round); }
        public void Sent(MatchPlayer p, PortalEvent e) { Events.Add(("onPortal", p.Id, e.Unit?.Id)); Planning?.Sent(p, e); }
    }
    public static int Run(string root, ContentCatalog catalog, bool planning = false)
    {
        using var input = File.OpenRead(Path.Combine(root, planning ? "tests/fixtures/duos-planning.json.gz" : "tests/fixtures/portals.json.gz"));
        using var gzip = new GZipStream(input, CompressionMode.Decompress); using var fixture = JsonDocument.Parse(gzip);
        int checks = 0; void Check(bool ok, string c) { checks++; if (!ok) throw new InvalidOperationException("Portal parity: " + c); }
        static string[] Strings(JsonElement e) => e.EnumerateArray().Select(x => x.GetString()!).ToArray();
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
            var state = new MatchState(rules, 143, Enumerable.Range(0,8).Select(i => new MatchSeat($"AI {i}", false)).ToArray(), true);
            var hooks = new Hooks(); var portal = new MatchPortal(state, hooks); if (planning) hooks.Planning = new(state, portal); int step = 0;
            foreach (var sample in trace.GetProperty("cases").EnumerateArray())
            {
                string context = $"{rules.Id}/{step++}"; hooks.Events.Clear(); if (planning) hooks.Round = new(sample.GetProperty("stage").GetInt32(), sample.GetProperty("round").GetInt32());
                foreach (var setup in sample.GetProperty("setup").EnumerateArray())
                {
                    var p = state.Players[setup.GetProperty("id").GetInt32()]; portal.Reset(p);
                    if (planning) p.Economy.Credit((step - 1) % 60 - 10 - p.Economy.Gold);
                    foreach (var e in p.Roster.Entries.ToArray()) p.Roster.Remove(e.Unit.Id, false);
                    p.Items.Clear(); p.Items.AddRange(Strings(setup.GetProperty("items"))); p.Augments.Clear(); p.Augments.UnionWith(Strings(setup.GetProperty("augs")));
                    foreach (var u in setup.GetProperty("units").EnumerateArray())
                    {
                        var unit = state.CreateUnit(u.GetProperty("line").GetString()!, u.GetProperty("stage").GetInt32()) with { Items = Strings(u.GetProperty("items")), NoSell = u.GetProperty("noSell").GetBoolean() };
                        Check(unit.Id == u.GetProperty("uid").GetInt32(), context + " allocated ID"); p.Roster.AddToBench(unit);
                    }
                    foreach (var cell in setup.GetProperty("board").EnumerateArray())
                    {
                        int i = cell.GetInt32(); p.Roster.Move(UnitSpot.Bench(i), UnitSpot.Board(i, 4), false, () => 100);
                    }
                }
                var sender = state.Players[0]; var what = sample.GetProperty("what");
                var request = new PortalRequest(what.TryGetProperty("uid", out var uid) ? uid.GetInt32() : null, what.TryGetProperty("item", out var index) ? index.GetInt32() : null);
                var phase = Enum.Parse<MatchPhase>(sample.GetProperty("phase").GetString()!, true); string round = sample.GetProperty("roundType").GetString()!;
                Check(portal.CanSend(sender, request, phase, round) == sample.GetProperty("can").GetString(), context + " eligibility");
                var result = portal.Send(sender, request, phase, round);
                Check(result.Status == sample.GetProperty("result").GetString(), context + " result");
                Check(result.Evolved.Select(u => u.Id).SequenceEqual(sample.GetProperty("evolved").EnumerateArray().Select(x => x.GetInt32())), context + " evolved");
                portal.AiTurn(sender, phase, round);
                var income = new Dictionary<int, int>();
                if (planning) foreach (var p in new[] { sender, portal.Partner(sender)! })
                {
                    hooks.Planning!.Picked(p, sample.GetProperty("pick").GetString()!);
                    income[p.Id] = hooks.Planning.Income(p, new(0, 0, 0, 2)).Augment;
                    hooks.Planning.RoundStart(p, hooks.Round, phase, round);
                    for (int i = 0; i < 5; i++) hooks.Planning.PartnerCombined(p, hooks.Round);
                }
                var events = sample.GetProperty("events").EnumerateArray().Select(e => (e.GetProperty("name").GetString()!, e.GetProperty("player").GetInt32(), e.GetProperty("uid").ValueKind == JsonValueKind.Null ? (int?)null : e.GetProperty("uid").GetInt32()));
                Check(hooks.Events.SequenceEqual(events), context + " hook order");
                foreach (var pair in sample.GetProperty("pool").EnumerateObject()) Check(state.Pool.Remaining(pair.Name) == pair.Value.GetInt32(), context + " pool");
                foreach (var expected in sample.GetProperty("after").EnumerateArray())
                {
                    var p = state.Players[expected.GetProperty("id").GetInt32()];
                    if (planning)
                    {
                        Check(p.Economy.Gold == expected.GetProperty("gold").GetInt32(), context + " gold");
                        Check(income[p.Id] == expected.GetProperty("income").GetInt32(), context + " income");
                        var proudRound = expected.GetProperty("proudRound");
                        Check(p.ProudPartnerRound == (proudRound.ValueKind == JsonValueKind.Null ? (int?)null : proudRound.GetInt32()) && p.ProudPartnerCount == expected.GetProperty("proudCount").GetInt32(), context + " proud partner");
                    }
                    Check(p.PortalSends == expected.GetProperty("portal").GetInt32(), context + " sends");
                    Check(p.Items.SequenceEqual(Strings(expected.GetProperty("items"))), context + " inventory");
                    for (int i = 0; i < 9; i++)
                    {
                        var u = p.Roster.Bench[i]; var e = expected.GetProperty("bench")[i];
                        Check(e.ValueKind == JsonValueKind.Null ? u is null : u is not null && u.Id == e.GetProperty("uid").GetInt32() && u.Line == e.GetProperty("line").GetString() && u.Stage == e.GetProperty("stage").GetInt32() && u.Items.SequenceEqual(Strings(e.GetProperty("items"))), context + " bench");
                    }
                    Check(p.Shop.Slots.SequenceEqual(expected.GetProperty("shop").EnumerateArray().Select(e => e.GetString())), context + " shop exclusions");
                    var board = expected.GetProperty("board").EnumerateObject().ToArray(); Check(p.Roster.Board.Count == board.Length, context + " board count");
                    for (int i = 0; i < board.Length; i++)
                    {
                        var e = p.Roster.Board[i]; var u = board[i].Value;
                        Check($"{e.Spot.Column},{e.Spot.Row}" == board[i].Name && e.Unit.Id == u.GetProperty("uid").GetInt32() && e.Unit.Line == u.GetProperty("line").GetString() && e.Unit.Stage == u.GetProperty("stage").GetInt32() && e.Unit.Items.SequenceEqual(Strings(u.GetProperty("items"))), context + " board unit");
                    }
                    var incoming = expected.GetProperty("incoming"); Check(p.Incoming.Count == incoming.GetArrayLength(), context + " arrivals count");
                    for (int i = 0; i < p.Incoming.Count; i++)
                    {
                        var a = p.Incoming[i]; var e = incoming[i];
                        Check(a.Kind == e.GetProperty("kind").GetString() && a.From == e.GetProperty("from").GetInt32(), context + " arrival sender");
                        Check(a.Kind == "unit" ? a.Uid == e.GetProperty("uid").GetInt32() && a.Line == e.GetProperty("line").GetString() && a.Stage == e.GetProperty("stage").GetInt32() : a.Item == e.GetProperty("item").GetString(), context + " arrival payload");
                        Check(a.Gift == (e.TryGetProperty("gift", out var gift) && gift.GetBoolean()), context + " gift flag");
                    }
                }
            }
            Check(state.Random.Next() == trace.GetProperty("nextRandom").GetDouble(), rules.Id + " RNG");
        }
        Console.WriteLine($"{(planning ? "Duos planning" : "Portal")} parity: {checks:N0} checks across 22 variants."); return checks;
    }
}
