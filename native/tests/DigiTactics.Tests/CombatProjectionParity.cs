using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class CombatProjectionParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/combat-projection.json.gz"));
        using var zip = new GZipStream(input, CompressionMode.Decompress); using var document = JsonDocument.Parse(zip);
        var json = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true };
        int checks = 0;
        void Check(bool ok, string c) { checks++; if (!ok) throw new InvalidOperationException("Combat projection: " + c); }
        static string[] Strings(JsonElement e) => e.EnumerateArray().Select(x => x.GetString()!).ToArray();
        foreach (var trace in document.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(r => r.Id == trace.GetProperty("variant").GetString());
            bool duo = trace.GetProperty("duo").GetBoolean(); string c = $"{rules.Id}/{duo}/{trace.GetProperty("n")}";
            var state = MatchStart.Create(rules, trace.GetProperty("seed").GetUInt32(), Enumerable.Range(0, 8).Select(i => new MatchSeat($"Player {i}", i == 0)).ToArray(), () => RoundPosition.First, duo);
            foreach (var p in state.Players)
            {
                var e = trace.GetProperty("players")[p.Id]; var m = e.GetProperty("mech"); var native = p.Mechanics;
                p.Augments.UnionWith(Strings(e.GetProperty("augs"))); p.Globals.AddRange(Strings(e.GetProperty("globals")));
                p.Power = e.GetProperty("power").Deserialize<CombatPower>(json);
                var buffs = e.GetProperty("buffs"); if (buffs.ValueKind != JsonValueKind.Null) p.NextBuffs = new(buffs.GetProperty("as").GetDouble(), buffs.GetProperty("armor").GetDouble(), buffs.GetProperty("mana").GetDouble());
                native.FatesDeva = m.GetProperty("deva").GetInt32(); native.FatesBlossom = m.GetProperty("blossom").GetInt32();
                native.Gizmos.GoldStart = m.GetProperty("goldStart").GetInt32(); native.DragonBank = m.GetProperty("bank").GetInt32(); native.DragonTrainingKills = m.GetProperty("ctKills").GetInt32();
                native.Arcane.SyndicateHealth = m.GetProperty("arcSynHp").GetDouble(); native.Arcane.Chests = m.GetProperty("arcChests").GetInt32(); native.Arcane.Junk.AddRange(Strings(m.GetProperty("arcJunk")));
                native.ColiseumPirate.AbilityPower = m.GetProperty("rcAP").GetDouble(); native.UnderlordsDuel = m.GetProperty("ulDuel").GetDouble(); native.UnderlordsBeastKills = m.GetProperty("ulBeastKills").GetDouble();
                if (m.TryGetProperty("arcSynAug", out var syn)) native.Arcane.SyndicateAugment = syn.GetBoolean();
                if (m.TryGetProperty("hpFull", out var hp)) native.DragonHpFull = hp.GetBoolean();
                if (m.TryGetProperty("fallen", out var fallen)) native.FatesPath = fallen.GetString();
                if (m.TryGetProperty("supreme", out var supreme)) native.ColiseumSupreme = supreme.GetString();
                if (m.TryGetProperty("bounty", out var bounty)) foreach (var pair in bounty.EnumerateObject()) native.GalaxiesBounties[pair.Name] = pair.Value.GetInt32();
            }
            foreach (var added in trace.GetProperty("added").EnumerateArray())
            {
                var p = state.Players[added.GetProperty("player").GetInt32()]; var e = added.GetProperty("unit");
                var u = state.CreateUnit(e.GetProperty("line").GetString()!, e.GetProperty("stage").GetInt32()) with
                { Items = Strings(e.GetProperty("items")), Mods = e.GetProperty("mods").Clone(), BonusTraits = e.GetProperty("bonusTraits").Deserialize<Dictionary<string, int>>() };
                Check(u.Id == e.GetProperty("uid").GetInt32(), c + " unit ID");
                int slot = p.Roster.FreeBenchSlot; p.Roster.AddToBench(u); p.Roster.Move(UnitSpot.Bench(slot), UnitSpot.Board(added.GetProperty("col").GetInt32(), added.GetProperty("row").GetInt32()), false, () => 100);
            }
            var projection = new MatchCombatProjection(state);
            var a = duo ? projection.Team(state.Teams[0]) : projection.Player(state.Players[0]);
            var b = duo ? projection.Team(state.Teams[1]) : projection.Player(state.Players[1]);
            var setup = projection.Setup(a, b, new(trace.GetProperty("stage").GetInt32(), 1));
            var actualA = JsonSerializer.SerializeToElement(a, json); var actualB = JsonSerializer.SerializeToElement(b, json); var actualSetup = JsonSerializer.SerializeToElement(setup, json);
            Check(JsonElement.DeepEquals(actualA, trace.GetProperty("a")), c + " side A");
            Check(JsonElement.DeepEquals(actualB, trace.GetProperty("b")), c + " side B");
            Check(JsonElement.DeepEquals(actualSetup, trace.GetProperty("setup")), c + " setup");
            Check(state.Random.Next() == trace.GetProperty("nextRandom").GetDouble(), c + " projection consumes no RNG");
            foreach (var p in state.Players)
            {
                foreach (var entry in p.Roster.Board) { if (entry.Unit.Items.Length > 0) entry.Unit.Items[0] = "changed"; entry.Unit.BonusTraits!["changed"] = 9; }
                p.Augments.Add("changed"); p.Globals.Add("changed"); p.Mechanics.Arcane.Junk.Add("changed"); p.Mechanics.GalaxiesBounties["changed"] = 9;
                p.NextBuffs = new(100, 100, 100);
            }
            if (state.ArcaneSponsor.Length > 0) state.ArcaneSponsor[0] = "changed";
            Check(JsonElement.DeepEquals(actualA, JsonSerializer.SerializeToElement(a, json)) && JsonElement.DeepEquals(actualB, JsonSerializer.SerializeToElement(b, json)) &&
                JsonElement.DeepEquals(actualSetup, JsonSerializer.SerializeToElement(setup, json)), c + " isolated battle inputs");
        }
        Console.WriteLine($"Combat projection parity: {checks:N0} checks across {document.RootElement.GetArrayLength()} traces."); return checks;
    }
}
