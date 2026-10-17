using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;
internal static class ComposedAiParity
{
    public static int Run(string root,ContentCatalog catalog)
    {
        using var input=File.OpenRead(Path.Combine(root,"tests/fixtures/composed-ai.json.gz"));using var gzip=new GZipStream(input,CompressionMode.Decompress);using var fixture=JsonDocument.Parse(gzip);
        int checks=0;void Check(bool ok,string c){checks++;if(!ok)throw new InvalidOperationException("Composed AI: "+c);}
        static string[] Strings(JsonElement e)=>e.EnumerateArray().Select(x=>x.GetString()!).ToArray();
        foreach(var trace in fixture.RootElement.EnumerateArray())
        {
            var basis=catalog.Variants.Single(v=>v.Id==trace.GetProperty("variant").GetString());var rules=basis with {Offers=basis.Offers! with {Schedule=[new("armory",1,1)]}};
            var currentRound=RoundPosition.First;var phase=MatchPhase.Planning;
            var state=MatchStart.Create(rules,trace.GetProperty("seed").GetUInt32(),Enumerable.Range(0,8).Select(i=>new MatchSeat($"Player {i}",i==0)).ToArray(),()=>currentRound,trace.GetProperty("duo").GetBoolean());
            var units=state.Units!;var offers=new MatchOffers(state,units,()=>phase);var portal=units.Portal;var ai=new MatchAi(state,offers,units);int n=0;
            foreach(var p in state.Players){p.Augments.UnionWith(["duo_proud","duo_wide_portal"]);if(rules.Id.StartsWith("gizmos/",StringComparison.Ordinal))p.Augments.UnionWith(["giz_a_highend","giz_a_ticket","giz_a_recruit"]);}
            foreach(var step in trace.GetProperty("steps").EnumerateArray())
            {
                string c=$"{rules.Id}/{state.Duos}/{trace.GetProperty("seed")}/{n}";var round=new RoundPosition(step.GetProperty("stage").GetInt32(),step.GetProperty("round").GetInt32());currentRound=round;phase=Enum.Parse<MatchPhase>(step.GetProperty("phase").GetString()!,true);rules.Offers.Schedule[0]=new(step.GetProperty("kind").GetString()!,round.Stage,round.Round);
                for(int i=0;i<8;i++){var p=state.Players[i];p.Economy.Credit(n%4==0?30:10);p.Shop.FreeRolls=n%3;portal.Reset(p);foreach(string id in Strings(step.GetProperty("inputs")[i]))p.GiveItem(id);if(rules.Wisps!.Length>0){var w=rules.Wisps[(n+p.Id)%rules.Wisps.Length];p.Wisp=new(w.Id,w.Base+round.Stage);}offers.Make(p,round);}
                ai.PlanPlayers(round,Enum.Parse<MatchPhase>(step.GetProperty("phase").GetString()!,true),step.GetProperty("roundType").GetString()!);
                var after=step.GetProperty("after");
                foreach(var pair in after.GetProperty("pool").EnumerateObject())Check(state.Pool.Remaining(pair.Name)==pair.Value.GetInt32(),c+" pool "+pair.Name);
                for(int i=0;i<8;i++)
                {
                    var p=state.Players[i];var e=after.GetProperty("players")[i];string pc=c+" player "+i;
                    Check(p.Economy.Gold==e.GetProperty("gold").GetInt32()&&p.Economy.Level==e.GetProperty("level").GetInt32()&&p.Economy.Xp==e.GetProperty("xp").GetInt32(),pc+" economy");
                    Check(p.Shop.FreeRolls==e.GetProperty("freeRolls").GetInt32()&&p.Shop.Slots.SequenceEqual(e.GetProperty("shop").EnumerateArray().Select(x=>x.GetString())),pc+" shop");
                    Check(p.Items.SequenceEqual(Strings(e.GetProperty("items")))&&p.Globals.SequenceEqual(Strings(e.GetProperty("globals"))),pc+" inventory");Check(p.PortalSends==e.GetProperty("portal").GetInt32(),pc+" sends");
                    Check(p.Health==e.GetProperty("hp").GetInt32()&&p.TeamBonus==e.GetProperty("teamBonus").GetInt32(),pc+" health/capacity");
                    Check(p.Augments.SequenceEqual(Strings(e.GetProperty("augs"))),pc+" augments");
                    var power=e.GetProperty("power");Check(power.ValueKind==JsonValueKind.Null?p.Power is null:p.Power is not null&&p.Power.Id==power.GetProperty("id").GetString()&&p.Power.Rank==power.GetProperty("rank").GetInt32(),pc+" power");
                    var wisp=e.GetProperty("wisp");Check(wisp.ValueKind==JsonValueKind.Null?p.Wisp is null:p.Wisp is not null&&p.Wisp.Id==wisp.GetProperty("id").GetString()&&p.Wisp.Cost==wisp.GetProperty("cost").GetInt32(),pc+" Wisp");
                    var buffs=e.GetProperty("buffs");Check((p.NextBuffs is null)==(buffs.ValueKind==JsonValueKind.Null),pc+" buffs presence");
                    if(p.NextBuffs is {} buff){double B(string key)=>buffs.TryGetProperty(key,out var v)?v.GetDouble():0;Check(buff.AttackSpeed==B("as")&&buff.Armor==B("armor")&&buff.Mana==B("mana"),pc+" buffs");}
                    Check(p.Mechanics.ColiseumCores==e.GetProperty("cores").GetInt32(),pc+" stored cores");
                    var offer=e.GetProperty("offer");Check(offer.ValueKind==JsonValueKind.Null?p.Offer is null:p.Offer is not null&&p.Offer.Options.SequenceEqual(Strings(offer.GetProperty("options"))),pc+" offer");
                    void Unit(PlanningUnit? u,JsonElement expected){Check(expected.ValueKind==JsonValueKind.Null?u is null:u is not null&&u.Id==expected.GetProperty("uid").GetInt32()&&u.Line==expected.GetProperty("line").GetString()&&u.Stage==expected.GetProperty("stage").GetInt32()&&u.Items.SequenceEqual(Strings(expected.GetProperty("items"))),pc+" unit");if(u is not null && expected.TryGetProperty("mods",out var mods))Check(u.Mods is { } actual && JsonElement.DeepEquals(actual,mods),pc+" modifiers");if(u is not null && expected.TryGetProperty("bonusTraits",out var bonus))Check(JsonElement.DeepEquals(JsonSerializer.SerializeToElement(u.BonusTraits),bonus),pc+" bonus traits");}
                    var board=e.GetProperty("board").EnumerateObject().ToArray();Check(board.Length==p.Roster.Board.Count,pc+" board count");for(int j=0;j<board.Length;j++){var u=p.Roster.Board[j];Check($"{u.Spot.Column},{u.Spot.Row}"==board[j].Name,pc+" board order");Unit(u.Unit,board[j].Value);}for(int j=0;j<9;j++)Unit(p.Roster.Bench[j],e.GetProperty("bench")[j]);
                    var incoming=e.GetProperty("incoming");Check(p.Incoming.Count==incoming.GetArrayLength(),pc+" arrivals");for(int j=0;j<p.Incoming.Count;j++){var a=p.Incoming[j];var b=incoming[j];Check(a.From==b.GetProperty("from").GetInt32()&&a.Kind==b.GetProperty("kind").GetString()&&(a.Kind=="unit"?a.Uid==b.GetProperty("uid").GetInt32():a.Item==b.GetProperty("item").GetString()),pc+" arrival payload");}
                }
                n++;
            }
            Check(state.CreateUnit(rules.Lines[0].Id,0).Id==trace.GetProperty("nextId").GetInt32(),rules.Id+" next ID");
            Check(state.Random.Next()==trace.GetProperty("nextRandom").GetDouble(),rules.Id+" RNG");
        }
        Console.WriteLine($"Composed AI parity: {checks:N0} checks across {fixture.RootElement.GetArrayLength()} Solo/Duos traces.");return checks;
    }
}
