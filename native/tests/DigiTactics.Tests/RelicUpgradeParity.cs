using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;
internal static class RelicUpgradeParity
{
    private sealed class Hooks : IMatchOfferHooks
    {
        public List<string> Picks { get; } = [];
        public string[] Armory(MatchPlayer p, RoundPosition r) => throw new NotSupportedException();
        public string[] CustomOptions(MatchPlayer p, RoundPosition r, OfferDefinition d, IReadOnlyList<string> e) => throw new NotSupportedException();
        public void AugmentPicked(MatchPlayer p, string id) => Picks.Add(id);
        public void PowerPicked(MatchPlayer p, string id) => throw new NotSupportedException();
        public void CustomChosen(MatchPlayer p, PlayerOffer o, string id) => throw new NotSupportedException();
    }
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/relic-upgrades.json.gz"));
        using var gzip = new GZipStream(input, CompressionMode.Decompress); using var fixture = JsonDocument.Parse(gzip);
        int checks = 0; void Check(bool ok, string c) { checks++; if (!ok) throw new InvalidOperationException("Relic/upgrade parity: " + c); }
        static string[] Strings(JsonElement e) => e.EnumerateArray().Select(x => x.GetString()!).ToArray();
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            var basis = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
            var rules = basis with { Offers = basis.Offers! with { Schedule = [new(trace.GetProperty("kind").GetString()!,1,1)] } };
            var state = new MatchState(rules,143,[new("Player",trace.GetProperty("human").GetBoolean())]);var p=state.Players[0];
            p.Globals.Add("unrelated_global"); var hooks = new Hooks();var offers = new MatchOffers(state,hooks);int n=0;
            void Offer(JsonElement expected)
            {
                Check(expected.ValueKind==JsonValueKind.Null ? p.Offer is null : p.Offer is not null && p.Offer.Kind==expected.GetProperty("kind").GetString() && p.Offer.Options.SequenceEqual(Strings(expected.GetProperty("options"))) && p.Offer.Rerolls==expected.GetProperty("rerolls").GetInt32(), rules.Id+" offer");
            }
            foreach(var step in trace.GetProperty("steps").EnumerateArray())
            {
                string c=$"{rules.Id}/{n++}";var added=step.GetProperty("added");
                if(added.GetArrayLength()>0)
                {
                    foreach(var e in p.Roster.Entries.ToArray())p.Roster.Remove(e.Unit.Id,false);int i=0;
                    foreach(var e in added.EnumerateArray())
                    {
                        var u=state.CreateUnit(e.GetProperty("line").GetString()!,0) with {Items=Strings(e.GetProperty("items"))};Check(u.Id==e.GetProperty("uid").GetInt32(),c+" ID");
                        int slot=p.Roster.FreeBenchSlot;p.Roster.AddToBench(u);if(i<2)p.Roster.Move(UnitSpot.Bench(slot),UnitSpot.Board(i,4),false,()=>100);i++;
                    }
                }
                state.GizmosGates=step.GetProperty("gates").ValueKind==JsonValueKind.Null?null:step.GetProperty("gates").EnumerateArray().Select(e=>e.GetInt32()).ToArray();
                if(step.GetProperty("fill").GetBoolean()){p.Items.Clear();p.Items.AddRange(Enumerable.Repeat(rules.CoreItems[0],rules.Loot!.ItemCap));}
                offers.Make(p,new(1,1));Offer(step.GetProperty("made"));
                if(step.GetProperty("reroll").ValueKind!=JsonValueKind.Null)Check(offers.Reroll(p,new(1,1))==step.GetProperty("reroll").GetBoolean(),c+" reroll");Offer(step.GetProperty("rolled"));
                Check(offers.Choose(p,step.GetProperty("choice").GetInt32(),p.IsHuman&&step.GetProperty("phase").GetString()=="combat")==step.GetProperty("selected").GetString(),c+" choice");
                var after=step.GetProperty("after");Check(p.Items.SequenceEqual(Strings(after.GetProperty("items"))),c+" items");Check(p.Globals.SequenceEqual(Strings(after.GetProperty("globals"))),c+" globals");Check(p.Augments.SequenceEqual(Strings(after.GetProperty("augs"))),c+" augments");Check(p.Shop.FreeRolls==after.GetProperty("freeRolls").GetInt32(),c+" free rolls");Check(hooks.Picks.SequenceEqual(Strings(after.GetProperty("picks"))),c+" picks");Check(p.Shop.Slots.SequenceEqual(after.GetProperty("shop").EnumerateArray().Select(e=>e.GetString())),c+" shop");
                foreach(var pair in after.GetProperty("pool").EnumerateObject())Check(state.Pool.Remaining(pair.Name)==pair.Value.GetInt32(),c+" pool");
                var board=after.GetProperty("board").EnumerateObject().ToArray();Check(board.Length==p.Roster.Board.Count,c+" board count");
                void Unit(PlanningUnit? u,JsonElement e){Check(e.ValueKind==JsonValueKind.Null?u is null:u is not null&&u.Id==e.GetProperty("uid").GetInt32()&&u.Stage==e.GetProperty("stage").GetInt32()&&u.Items.SequenceEqual(Strings(e.GetProperty("items"))),c+" unit");}
                for(int i=0;i<board.Length;i++){Check($"{p.Roster.Board[i].Spot.Column},{p.Roster.Board[i].Spot.Row}"==board[i].Name,c+" board order");Unit(p.Roster.Board[i].Unit,board[i].Value);}
                for(int i=0;i<9;i++)Unit(p.Roster.Bench[i],after.GetProperty("bench")[i]);
            }
            Check(state.Random.Next()==trace.GetProperty("nextRandom").GetDouble(),rules.Id+" RNG");
        }
        Console.WriteLine($"Relic/upgrade parity: {checks:N0} checks across 8 traces.");return checks;
    }
}
