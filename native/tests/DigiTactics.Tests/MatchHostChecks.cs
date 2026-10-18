using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Networking;
using DigiTactics.Presentation;
using DigiTactics.Rules;

internal static class MatchHostChecks
{
    private sealed class Rig : IDisposable
    {
        public long Now=10000;
        public readonly NetworkRoom Host,Guest;
        public readonly MatchReplica A,B;
        public readonly MatchHostController HostControl,GuestControl;
        private readonly Queue<(string Sender,RoomPacket Packet)> packets=[];
        public Rig(ContentCatalog catalog)
        {
            var lobby=JsonNode.Parse("{\"host\":\"123\",\"name\":\"Room\",\"hostName\":\"Host\",\"status\":\"playing\",\"slots\":{\"0\":{\"uid\":\"123\",\"name\":\"Host\"},\"1\":{\"uid\":\"234\",\"name\":\"Guest\"}}}")!.AsObject();
            Host=new("999","123",lobby);Guest=new("999","234",lobby);
            Host.Members(["123","234"],"123",Now);Guest.Members(["123","234"],"123",Now);
            Host.Send+=p=>packets.Enqueue(("123",p));Guest.Send+=p=>packets.Enqueue(("234",p));
            A=new(Host,catalog);B=new(Guest,catalog);
            HostControl=new(Host,A,()=>Now);GuestControl=new(Guest,B,()=>Now);
            Host.PublishInit(JsonSerializer.SerializeToElement(new {seed=143,at=Now,planTime=30,setId="faction-wars",pace="hyper",format="solo",slots=Enumerable.Range(0,8).Select(i=>new {name="Tamer "+i,human=i<2}).ToArray()}));
            Network(); A.Pump(Now);B.Pump(Now);
            Guest.SetPresence(new JsonObject { ["name"]="Guest" },false,Now); Network();
            HostControl.Pump(Now);Network();
            int guard=0;
            while(A.Match!.Session.Rounds.RoundType=="carousel") { if(++guard>10)throw new InvalidOperationException("Opening carousel did not finish.");Tick(12000); }
        }
        public void Network() { while(packets.TryDequeue(out var p)) (p.Packet.Recipient=="123"?Host:Guest).Receive(p.Sender,p.Packet.Text,Now); }
        public void Tick(long delta=200) { Now+=delta;HostControl.Pump(Now);Network();GuestControl.Pump(Now);Network();A.Pump(Now);B.Pump(Now); }
        public bool Input(bool guest,string action,int? round=null) { var room=guest?Guest:Host; int r=round??(A.Match!.Session.Rounds.Position.Stage*100+A.Match.Session.Rounds.Position.Round);bool accepted=room.SubmitIntent(guest?1:0,action,JsonSerializer.SerializeToElement(new {}),r);Network();return accepted; }
        public void Dispose() { HostControl.Dispose();GuestControl.Dispose();A.Dispose();B.Dispose(); }
    }
    public static int Run(ContentCatalog catalog)
    {
        int checks=0;void Check(bool ok,string why) {checks++;if(!ok)throw new InvalidOperationException("Match host: "+why);}
        using(var rig=new Rig(catalog))
        {
            Check(rig.A.Match!.Session.Rounds.Phase==MatchPhase.Planning,"opening planning");
            int initial=rig.Host.Log.Count;
            Check(rig.Input(true,"ready"),"guest intent accepted");
            Check(rig.HostControl.Queued==1&&rig.Host.Log.Count==initial,"intent queues without mutating match");
            rig.Tick();Check(rig.A.Match.Votes.Contains(1)&&rig.B.Match!.Votes.Contains(1),"ready becomes replicated vote");
            rig.Input(false,"ready");rig.Tick();rig.Tick();
            Check(rig.A.Match.Session.Rounds.Phase==MatchPhase.Combat&&rig.B.Match!.Check()==rig.A.Match.Check(),$"all ready starts equal combats: phase={rig.A.Match.Session.Rounds.Phase} votes={string.Join(',',rig.A.Match.Votes)} logs={rig.Host.Log.Count}/{rig.Guest.Log.Count} hashes={rig.A.Match.Check()}/{rig.B.Match!.Check()} queued={rig.HostControl.Queued} dropped={rig.HostControl.Dropped}");
            rig.Input(true,"done");rig.Input(false,"done");rig.Tick();rig.Tick();
            Check(rig.A.Match.Session.Rounds.Phase==MatchPhase.Result,"all replay completions end combat");
            int before=rig.Host.Log.Count;rig.Input(true,"ready");rig.Tick(3200);
            Check(rig.Host.Log.Count==before+1&&rig.Host.Log.Entries[^1].GetProperty("k").GetString()=="next"&&rig.Host.Log.Entries[^1].TryGetProperty("d",out _),"result deadline takes precedence and stamps host hash");
            rig.Tick();Check(rig.HostControl.Queued==0&&!rig.A.Match.Votes.Contains(1),"prior round intent discarded after transition");
            Check(rig.A.Match.Check()==rig.B.Match!.Check()&&rig.B.LastDesync is null,"host/guest state remains identical");
            long dropped=rig.HostControl.Dropped;rig.Input(true,"ready",999);
            Check(rig.HostControl.Dropped==dropped+1,"stale round rejected at intake");
            Check(!rig.Input(true,"claim"),"peer cannot submit host-only seat operations");
        }
        using(var rig=new Rig(catalog))
        {
            for(int i=0;i<12;i++)rig.Input(true,"lock");
            Check(rig.HostControl.Queued==8&&rig.HostControl.Dropped==4,"per-player queue bounded at eight");
            rig.Tick(30000);
            Check(rig.Host.Log.Entries[^1].GetProperty("k").GetString()=="start"&&rig.HostControl.Queued==8,"planning deadline cannot be starved by queued actions");
            int count=rig.Host.Log.Count;rig.HostControl.Pump(rig.Now+199);
            Check(rig.Host.Log.Count==count,"host tick rate bounded at 200ms");
            rig.Host.FreeSeat(1);rig.Network();rig.Tick();
            Check(!rig.A.Match!.Session.State.Players[1].IsHuman&&rig.Host.Log.Entries[^1].GetProperty("a").GetString()=="release","vacated seat releases to AI through log");
            rig.Tick();Check(rig.HostControl.Queued==0,"queued inputs lose authority when seat is freed");
            rig.Guest.ClaimSeat(1,"Returning",rig.Now);rig.Network();rig.Tick();
            Check(rig.A.Match.Session.State.Players[1].IsHuman&&rig.A.Match.Session.State.Players[1].Name=="Returning","reclaimed seat logs human control and name");
        }
        using(var rig=new Rig(catalog))
        {
            for(int i=0;i<8;i++)rig.Input(true,"lock");
            rig.Tick(); int queued=rig.HostControl.Queued; long dropped=rig.HostControl.Dropped;
            rig.Input(true,"lock");
            Check(rig.HostControl.Queued==queued&&rig.HostControl.Dropped==dropped+1,"rate bucket rejects refill faster than four inputs per second");
            rig.Tick();rig.Input(true,"lock");
            Check(rig.HostControl.Queued==queued,"elapsed time replenishes host intake budget");
            rig.Host.RequestSnapshot(rig.Now);rig.A.Pump(rig.Now);
            Check(rig.HostControl.Queued==0,"rebuilding host match discards previously queued input");
            rig.HostControl.Dispose();rig.Input(true,"ready");
            Check(rig.HostControl.Queued==0,"disposed controller no longer captures room intents");
        }
        using(var rig=new Rig(catalog))
        {
            rig.Guest.LeavePresence();rig.Network();rig.Tick();rig.Tick(20001);
            Check(rig.Host.Authority.Lobby!["slots"]!["1"] is null,"offline seat freed after twenty seconds");
            rig.Tick();Check(!rig.A.Match!.Session.State.Players[1].IsHuman,"offline seat returns to AI");
        }
        using(var rig=new Rig(catalog))
        {
            rig.Guest.Members(["234"],"234",rig.Now);rig.Now+=6001;rig.Guest.Tick(rig.Now);rig.Network();
            Check(rig.Guest.Authority.IsHost,"surviving seated guest takes authority");
            int before=rig.Guest.Log.Count;rig.Now+=30000;rig.GuestControl.Pump(rig.Now);
            Check(rig.Guest.Log.Count==before+1&&rig.B.Match!.Sequence==rig.Guest.Log.Count,"successor schedules from existing replicated state");
        }
        Console.WriteLine($"Match host: {checks} input ordering, deadlines, AI handoff and succession checks passed.");return checks;
    }
}
