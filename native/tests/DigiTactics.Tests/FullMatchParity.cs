using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;
internal static class FullMatchParity
{
    public static int Run(string root,ContentCatalog catalog)
    {
        using var input=File.OpenRead(Path.Combine(root,"tests/fixtures/full-match.json.gz"));using var gzip=new GZipStream(input,CompressionMode.Decompress);using var fixture=JsonDocument.Parse(gzip);
        int checks=0;void Check(bool ok,string c){checks++;if(!ok)throw new InvalidOperationException("Full match: "+c);}
        static string[] Strings(JsonElement e)=>e.EnumerateArray().Select(x=>x.GetString()!).ToArray();
        foreach(var trace in fixture.RootElement.EnumerateArray())
        {
            var rules=catalog.Variants.Single(v=>v.Id==trace.GetProperty("variant").GetString());
            var session=new MatchPlanningSession(rules,trace.GetProperty("seed").GetUInt32(),Enumerable.Range(0,8).Select(i=>new MatchSeat($"Player {i}",i==0)).ToArray(),trace.GetProperty("duo").GetBoolean());
            var state=session.State; state.PlayOut=true;
            void Snapshot(JsonElement after,string c)
            {
                Check(state.Over == after.GetProperty("over").GetBoolean(), c+" over");
                Check(session.Rounds.Position==new RoundPosition(after.GetProperty("stage").GetInt32(),after.GetProperty("round").GetInt32())&&session.Rounds.RoundType==after.GetProperty("type").GetString()&&session.Rounds.Phase.ToString().ToLowerInvariant()==after.GetProperty("phase").GetString(),c+" round");
                var pairs=after.GetProperty("pairs");Check(session.Rounds.Pairs.Count==pairs.GetArrayLength(),c+" pair count");for(int j=0;j<pairs.GetArrayLength();j++)Check(session.Rounds.Pairs[j]==new MatchPair(pairs[j].GetProperty("a").GetInt32(),pairs[j].GetProperty("b").GetInt32(),pairs[j].GetProperty("ghost").GetBoolean()),c+" pair");
                var teams=after.GetProperty("teams");for(int j=0;j<state.Teams.Count;j++)Check(state.Teams[j].Health==teams[j].GetProperty("hp").GetInt32(),c+" team HP");
                var carousel=after.GetProperty("carousel");var actual=session.Units.Carousel;Check(actual.Active==(carousel.ValueKind!=JsonValueKind.Null),c+" carousel active");
                if(actual.Active){Check(actual.Index==carousel.GetProperty("idx").GetInt32()&&actual.Order.Select(p=>p.Id).SequenceEqual(carousel.GetProperty("order").EnumerateArray().Select(x=>x.GetInt32())),c+" carousel order");var options=carousel.GetProperty("opts");Check(actual.Options.Count==options.GetArrayLength(),c+" carousel size");for(int j=0;j<actual.Options.Count;j++)Check(actual.Options[j].Line==options[j].GetProperty("line").GetString()&&actual.Options[j].Item==options[j].GetProperty("item").GetString()&&actual.Options[j].PickedBy==options[j].GetProperty("by").GetString(),c+" carousel option");}
                foreach(var pair in after.GetProperty("pool").EnumerateObject())Check(state.Pool.Remaining(pair.Name)==pair.Value.GetInt32(),c+" pool "+pair.Name);
                for(int i=0;i<8;i++)
                {
                    var p=state.Players[i];var e=after.GetProperty("players")[i];string pc=c+" player "+i;
                    Check(JsonElement.DeepEquals(JsonSerializer.SerializeToElement(p.Orbs,new JsonSerializerOptions(JsonSerializerDefaults.Web)),e.GetProperty("orbs")),pc+" loot orbs");
                    Check(p.Alive==e.GetProperty("alive").GetBoolean()&&p.Placement==e.GetProperty("placement").GetInt32(),pc+" elimination");
                    Check(p.Economy.Gold==e.GetProperty("gold").GetInt32()&&p.Economy.Level==e.GetProperty("level").GetInt32()&&p.Economy.Xp==e.GetProperty("xp").GetInt32(),pc+" economy");
                    Check(p.Shop.FreeRolls==e.GetProperty("freeRolls").GetInt32()&&p.Shop.Slots.SequenceEqual(e.GetProperty("shop").EnumerateArray().Select(x=>x.GetString())),pc+" shop");
                    Check(p.Items.SequenceEqual(Strings(e.GetProperty("items")))&&p.Globals.SequenceEqual(Strings(e.GetProperty("globals"))),pc+" inventory");Check(p.PortalSends==e.GetProperty("portal").GetInt32(),pc+" sends");
                    Check(p.Health==e.GetProperty("hp").GetInt32()&&p.TeamBonus==e.GetProperty("teamBonus").GetInt32(),pc+" health/capacity");
                    Check(p.Augments.SequenceEqual(Strings(e.GetProperty("augs"))),pc+" augments");
                    var power=e.GetProperty("power");Check(power.ValueKind==JsonValueKind.Null?p.Power is null:p.Power is not null&&p.Power.Id==power.GetProperty("id").GetString()&&p.Power.Rank==power.GetProperty("rank").GetInt32(),pc+" power");
                    var wisp=e.GetProperty("wisp");Check(wisp.ValueKind==JsonValueKind.Null?p.Wisp is null:p.Wisp is not null&&p.Wisp.Id==wisp.GetProperty("id").GetString()&&p.Wisp.Cost==wisp.GetProperty("cost").GetInt32(),pc+" Wisp");
                    var buffs=e.GetProperty("buffs");Check((p.NextBuffs is null)==(buffs.ValueKind==JsonValueKind.Null),pc+" buffs presence");
                    if(p.NextBuffs is {} buff){double B(string key)=>buffs.TryGetProperty(key,out var v)?v.GetDouble():0;Check(buff.AttackSpeed==B("as")&&buff.Armor==B("armor")&&buff.Mana==B("mana"),pc+" buffs");}
                    Check(p.Economy.Streak==e.GetProperty("streak").GetInt32()&&p.Mechanics.Gizmos.GoldStart==e.GetProperty("goldStart").GetInt32()&&p.Mechanics.DragonBank==e.GetProperty("bank").GetInt32()&&p.Mechanics.DragonHpFull==e.GetProperty("hpFull").GetBoolean(),pc+" persistent economy");
                    Check(p.Mechanics.ColiseumCores==e.GetProperty("cores").GetInt32(),pc+" stored cores");
                    var offer=e.GetProperty("offer");Check(offer.ValueKind==JsonValueKind.Null?p.Offer is null:p.Offer is not null&&p.Offer.Options.SequenceEqual(Strings(offer.GetProperty("options"))),pc+" offer");
                    void Unit(PlanningUnit? u,JsonElement expected){Check(expected.ValueKind==JsonValueKind.Null?u is null:u is not null&&u.Id==expected.GetProperty("uid").GetInt32()&&u.Line==expected.GetProperty("line").GetString()&&u.Stage==expected.GetProperty("stage").GetInt32()&&u.Items.SequenceEqual(Strings(expected.GetProperty("items"))),pc+" unit");if(u is not null && expected.TryGetProperty("mods",out var mods))Check(u.Mods is { } actual && JsonElement.DeepEquals(actual,mods),pc+" modifiers");if(u is not null && expected.TryGetProperty("bonusTraits",out var bonus))Check(JsonElement.DeepEquals(JsonSerializer.SerializeToElement(u.BonusTraits),bonus),pc+" bonus traits");}
                    var board=e.GetProperty("board").EnumerateObject().ToArray();Check(board.Length==p.Roster.Board.Count,pc+" board count");for(int j=0;j<board.Length;j++){var u=p.Roster.Board[j];Check($"{u.Spot.Column},{u.Spot.Row}"==board[j].Name,pc+" board order");Unit(u.Unit,board[j].Value);}for(int j=0;j<9;j++)Unit(p.Roster.Bench[j],e.GetProperty("bench")[j]);
                    var incoming=e.GetProperty("incoming");Check(p.Incoming.Count==incoming.GetArrayLength(),pc+" arrivals");for(int j=0;j<p.Incoming.Count;j++){var a=p.Incoming[j];var b=incoming[j];Check(a.From==b.GetProperty("from").GetInt32()&&a.Kind==b.GetProperty("kind").GetString()&&(a.Kind=="unit"?a.Uid==b.GetProperty("uid").GetInt32():a.Item==b.GetProperty("item").GetString()),pc+" arrival payload");}
                }
            }
            string context=$"{rules.Id}/{state.Duos}/{trace.GetProperty("seed")}";
            Snapshot(trace.GetProperty("opening"),context+"/opening");
            int n=0;
            foreach(var step in trace.GetProperty("steps").EnumerateArray())
            {
                string c=context+"/"+n++;
                if(session.Rounds.RoundType!="carousel"&&(trace.GetProperty("seed").GetUInt32()==17||(n-1)%3!=0))session.Ai.Turn(state.Players[0],session.Rounds.Position,session.Rounds.Phase,session.Rounds.RoundType);
                session.StartCombat(); Snapshot(step.GetProperty("started"),c+" started");
                session.FinishCombat(); Snapshot(step.GetProperty("finished"),c+" finished");
                var expected=step.GetProperty("battles");Check(session.Battles.Count==expected.GetArrayLength(),c+" battles");
                for(int j=0;j<session.Battles.Count;j++)
                {
                    var battle=session.Battles[j];var e=expected[j];
                    Check(battle.A==e.GetProperty("a").GetInt32()&&battle.B==e.GetProperty("b").Deserialize<int?>()&&battle.TeamA==e.GetProperty("ta").Deserialize<int?>()&&battle.TeamB==e.GetProperty("tb").Deserialize<int?>()&&battle.Pve==e.GetProperty("pve").GetBoolean()&&battle.Ghost==e.GetProperty("ghost").GetBoolean()&&battle.Seed==e.GetProperty("seed").GetUInt32(),c+" participants/seed");
                    Check(battle.Combat.Winner==e.GetProperty("winner").GetInt32()&&Math.Abs(battle.Combat.Time-e.GetProperty("time").GetDouble())<1e-8,c+" result/time");
                    Check(JsonElement.DeepEquals(JsonSerializer.SerializeToElement(battle.TraitsA),e.GetProperty("counts")[0])&&JsonElement.DeepEquals(JsonSerializer.SerializeToElement(battle.TraitsB),e.GetProperty("counts")[1]),c+" counts");
                    var eu=e.GetProperty("units");Check(battle.Combat.Units.Count==eu.GetArrayLength(),c+" combat units");
                    for(int k=0;k<battle.Combat.Units.Count;k++) {var u=battle.Combat.Units[k];var v=eu[k];Check(u.Line.Id==v.GetProperty("line").GetString()&&u.Entry.Uid==v.GetProperty("uid").GetInt32()&&u.Entry.Owner==v.GetProperty("owner").Deserialize<int?>()&&u.Team==v.GetProperty("team").GetInt32()&&u.Stage==v.GetProperty("stage").GetInt32()&&u.Alive==v.GetProperty("alive").GetBoolean()&&u.Summoned==v.GetProperty("summoned").GetBoolean()&&Math.Abs(u.Health-v.GetProperty("hp").GetDouble())<1e-7,c+" combat unit "+k);}
                }
                if(!state.Over)Check(session.Advance(),c+" advance");Snapshot(step.GetProperty("after"),c+" after");
            }
            Check(state.CreateUnit(rules.Lines[0].Id,0).Id==trace.GetProperty("nextId").GetInt32(),context+" next ID");
            Check(state.Random.Next()==trace.GetProperty("nextRandom").GetDouble(),context+" RNG");
        }
        Console.WriteLine($"Full match parity: {checks:N0} checks across {fixture.RootElement.GetArrayLength()} Solo/Duos traces.");return checks;
    }
}
