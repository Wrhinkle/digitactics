using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;
internal static class AiTurnParity
{
    private sealed class Hooks(MatchState state, bool concrete) : IMatchAiHooks, IMatchOfferHooks, IMatchShopHooks, IMatchItemHooks, IMatchPortalHooks
    {
        private readonly Ruleset rules = state.Rules;
        private readonly ColiseumPlanning coliseum = new(state);
        public List<int> Events { get; } = [];
        public void TurnStarted(MatchPlayer p, RoundPosition r) { Events.Add(p.Id); if(concrete)coliseum.AiTurn(p); }
        public int BaseCapacity(MatchPlayer p) => p.Economy.Level;
        public int Capacity(MatchPlayer p) => p.Economy.Level + p.Roster.TeamSizeBonus;
        public bool CanPlace(MatchPlayer p, PlanningUnit u, UnitSpot s) => true;
        public bool HasEquipRules(MatchPlayer p) => rules.Id.StartsWith("underlords",StringComparison.Ordinal);
        public void Reroll(MatchPlayer p) => p.Shop.Reroll();
        public string[] Armory(MatchPlayer p, RoundPosition r) => throw new NotSupportedException();
        public string[] CustomOptions(MatchPlayer p, RoundPosition r, OfferDefinition d, IReadOnlyList<string> e) => throw new NotSupportedException();
        public void AugmentPicked(MatchPlayer p, string id) => throw new NotSupportedException();
        public void PowerPicked(MatchPlayer p, string id) => throw new NotSupportedException();
        public void CustomChosen(MatchPlayer p, PlayerOffer o, string id) => throw new NotSupportedException();
        public PlanningUnit Bought(MatchPlayer p, PlanningUnit u, int slot, MatchPhase phase) => concrete ? coliseum.Bought(p,u,slot,phase) : u;
        public void Sold(MatchPlayer p, PlanningUnit u) { if(concrete)coliseum.Sold(p,u); }
        public void Combined(MatchPlayer p, PlanningUnit u, IReadOnlyList<PlanningUnit> sources) { if(concrete)coliseum.Combined(p,u,sources); }
        public void PartnerCombined(MatchPlayer p, MatchPlayer owner, PlanningUnit u) { }
        public void Sent(MatchPlayer p, PortalEvent e) { }
        public string UseConsumable(MatchPlayer p,int index,PlanningUnit u)=>throw new NotSupportedException();
        public string? EquipVeto(MatchPlayer p,PlanningUnit u,string item,string? combined)=>null;
    }
    public static int Run(string root,ContentCatalog catalog)
    {
        using var input=File.OpenRead(Path.Combine(root,"tests/fixtures/ai-turns.json.gz"));using var gzip=new GZipStream(input,CompressionMode.Decompress);using var fixture=JsonDocument.Parse(gzip);
        int checks=0;void Check(bool ok,string c){checks++;if(!ok)throw new InvalidOperationException("AI turns: "+c);}
        static string[] Strings(JsonElement e)=>e.EnumerateArray().Select(x=>x.GetString()!).ToArray();
        foreach(var trace in fixture.RootElement.EnumerateArray())
        {
            var basis=catalog.Variants.Single(v=>v.Id==trace.GetProperty("variant").GetString());var rules=basis with {Offers=basis.Offers! with {Schedule=[new("armory",1,1)]}};
            var state=new MatchState(rules,143,Enumerable.Range(0,8).Select(i=>new MatchSeat($"Player {i}",i==0)).ToArray(),trace.GetProperty("duo").GetBoolean());var hooks=new Hooks(state,trace.GetProperty("concrete").GetBoolean());var offers=new MatchOffers(state,hooks);var portal=new MatchPortal(state,hooks);var ai=new MatchAi(state,offers,new(hooks),new(state,hooks),portal,hooks);int n=0;
            foreach(var step in trace.GetProperty("steps").EnumerateArray())
            {
                string c=$"{rules.Id}/{state.Duos}/{n}";var round=new RoundPosition(step.GetProperty("stage").GetInt32(),step.GetProperty("round").GetInt32());rules.Offers.Schedule[0]=new("armory",round.Stage,round.Round);hooks.Events.Clear();
                for(int i=0;i<8;i++){var p=state.Players[i];p.Economy.Credit(n%4==0?30:10);p.Shop.FreeRolls=n%3;portal.Reset(p);foreach(string id in Strings(step.GetProperty("inputs")[i]))p.GiveItem(id);offers.Make(p,round);}
                ai.PlanPlayers(round,Enum.Parse<MatchPhase>(step.GetProperty("phase").GetString()!,true),step.GetProperty("roundType").GetString()!);
                Check(hooks.Events.SequenceEqual(step.GetProperty("events").EnumerateArray().Select(x=>x.GetInt32())),c+" turn order");var after=step.GetProperty("after");
                foreach(var pair in after.GetProperty("pool").EnumerateObject())Check(state.Pool.Remaining(pair.Name)==pair.Value.GetInt32(),c+" pool "+pair.Name);
                for(int i=0;i<8;i++)
                {
                    var p=state.Players[i];var e=after.GetProperty("players")[i];string pc=c+" player "+i;
                    Check(p.Economy.Gold==e.GetProperty("gold").GetInt32()&&p.Economy.Level==e.GetProperty("level").GetInt32()&&p.Economy.Xp==e.GetProperty("xp").GetInt32(),pc+" economy");
                    Check(p.Shop.FreeRolls==e.GetProperty("freeRolls").GetInt32()&&p.Shop.Slots.SequenceEqual(e.GetProperty("shop").EnumerateArray().Select(x=>x.GetString())),pc+" shop");
                    Check(p.Items.SequenceEqual(Strings(e.GetProperty("items")))&&p.Globals.SequenceEqual(Strings(e.GetProperty("globals"))),pc+" inventory");Check(p.PortalSends==e.GetProperty("portal").GetInt32(),pc+" sends");
                    Check(p.Mechanics.ColiseumCores==e.GetProperty("cores").GetInt32(),pc+" stored cores");
                    var offer=e.GetProperty("offer");Check(offer.ValueKind==JsonValueKind.Null?p.Offer is null:p.Offer is not null&&p.Offer.Options.SequenceEqual(Strings(offer.GetProperty("options"))),pc+" offer");
                    void Unit(PlanningUnit? u,JsonElement expected){Check(expected.ValueKind==JsonValueKind.Null?u is null:u is not null&&u.Id==expected.GetProperty("uid").GetInt32()&&u.Line==expected.GetProperty("line").GetString()&&u.Stage==expected.GetProperty("stage").GetInt32()&&u.Items.SequenceEqual(Strings(expected.GetProperty("items"))),pc+" unit");if(u is not null && expected.TryGetProperty("mods",out var mods))Check(u.Mods is { } actual && JsonElement.DeepEquals(actual,mods),pc+" modifiers");if(u is not null && expected.TryGetProperty("bonusTraits",out var bonus))Check(JsonElement.DeepEquals(JsonSerializer.SerializeToElement(u.BonusTraits),bonus),pc+" bonus traits");}
                    var board=e.GetProperty("board").EnumerateObject().ToArray();Check(board.Length==p.Roster.Board.Count,pc+" board count");for(int j=0;j<board.Length;j++){var u=p.Roster.Board[j];Check($"{u.Spot.Column},{u.Spot.Row}"==board[j].Name,pc+" board order");Unit(u.Unit,board[j].Value);}for(int j=0;j<9;j++)Unit(p.Roster.Bench[j],e.GetProperty("bench")[j]);
                    var incoming=e.GetProperty("incoming");Check(p.Incoming.Count==incoming.GetArrayLength(),pc+" arrivals");for(int j=0;j<p.Incoming.Count;j++){var a=p.Incoming[j];var b=incoming[j];Check(a.From==b.GetProperty("from").GetInt32()&&a.Kind==b.GetProperty("kind").GetString()&&(a.Kind=="unit"?a.Uid==b.GetProperty("uid").GetInt32():a.Item==b.GetProperty("item").GetString()),pc+" arrival payload");}
                }
                n++;
            }
            Check(state.Random.Next()==trace.GetProperty("nextRandom").GetDouble(),rules.Id+" RNG");
        }
        Console.WriteLine($"AI turn parity: {checks:N0} checks across {fixture.RootElement.GetArrayLength()} Solo/Duos traces.");return checks;
    }
}
