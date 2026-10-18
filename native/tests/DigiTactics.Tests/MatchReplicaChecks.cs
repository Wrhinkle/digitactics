using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Networking;
using DigiTactics.Presentation;
using DigiTactics.Rules;

internal static class MatchReplicaChecks
{
    public static int Run(ContentCatalog catalog)
    {
        int checks=0;
        void Check(bool ok,string why) { checks++; if(!ok) throw new InvalidOperationException("Match replica: "+why); }
        var lobby=JsonNode.Parse("{\"host\":\"123\",\"name\":\"Room\",\"hostName\":\"Host\",\"status\":\"lobby\",\"slots\":{\"0\":{\"uid\":\"123\",\"name\":\"Host\"}}}")!.AsObject();
        var host=new NetworkRoom("999","123",lobby); var guest=new NetworkRoom("999","234",lobby);
        host.Members(["123","234"],"123",10000); guest.Members(["123","234"],"123",10000);
        var packets=new Queue<(string Sender,RoomPacket Packet)>();
        host.Send+=p=>packets.Enqueue(("123",p)); guest.Send+=p=>packets.Enqueue(("234",p));
        long now=10000;
        void Network() { while(packets.TryDequeue(out var p)) (p.Packet.Recipient=="123"?host:guest).Receive(p.Sender,p.Packet.Text,now); }
        var init=JsonSerializer.SerializeToElement(new { seed=143, at=now, planTime=30,setId="faction-wars",pace="hyper",format="solo",slots=Enumerable.Range(0,8).Select(i=>new {name="Tamer "+i,human=i<2}).ToArray() });
        using var a=new MatchReplica(host,catalog); using var b=new MatchReplica(guest,catalog);
        int live=0,rebuild=0,desync=0; b.Applied+=(_,_)=>live++; b.Rebuilt+=_=>rebuild++; b.Desync+=_=>desync++;
        host.PublishInit(init); Network(); a.Pump(now); b.Pump(now);
        Check(a.Match!.Check()==b.Match!.Check()&&rebuild==1&&live==0,"initial seed builds equal matches without live effects");
        void Append(object entry) { Check(host.Append(host.Log.Count,JsonSerializer.SerializeToElement(entry),now),"append accepted"); Network(); a.Pump(now); b.Pump(now); }
        Append(new {k="vote",s=0,at=++now});
        Check(live==1&&b.Match.Votes.Contains(0),"incremental entry emits one live effect");
        using(var reconnect=new MatchReplica(guest,catalog)) { int effects=0; reconnect.Applied+=(_,_)=>effects++; reconnect.Pump(now); Check(reconnect.Match!.Check()==a.Match.Check()&&reconnect.Match.Sequence==1&&effects==0,"reconnect rebuilds full log without replaying effects"); }
        // Tamper only the guest's derived state, then stamp the host hash on a valid next entry.
        b.Match.Session.State.Players[0].Health--;
        Append(new {k="next",at=++now,d=a.Match.Check()});
        Check(desync==1&&rebuild==2&&a.Match.Check()==b.Match.Check()&&live==1,"live hash mismatch repaired from complete log");
        now+=60001;
        Append(new {k="next",at=now,d="deadbeef"});
        Check(desync==3&&b.LastDesync is {AfterReplay:true}&&b.Match.Sequence==host.Log.Count,"persistent replay disagreement reported without rebuild loop");
        int rebuilt=b.Rebuilds; b.Pump(++now); Check(b.Rebuilds==rebuilt,"persistent mismatch does not repeatedly replay completed log");
        guest.RequestSnapshot(now); Network(); a.Pump(now); b.Pump(now);
        Check(b.Match.Check()==a.Match.Check(),"replacement snapshot keeps deterministic state");
        // A conflicting historical entry must rebuild even within the desync cooldown.
        string wire=JsonSerializer.Serialize(new {l="999",t="log",n=0,e=new {k="vote",s=1,at=10001}});
        Check(guest.Receive("123",wire,++now),"replacement history accepted from host"); b.Pump(now);
        Check(b.Match.Sequence==1&&b.Match.Votes.Contains(1)&&!b.Match.Votes.Contains(0)&&b.Rebuilds>rebuilt,"forced reset discards derived state and replays replacement history");
        guest.RequestSnapshot(now); b.Pump(now);
        Check(b.Match.Sequence==1,"incomplete snapshot does not mutate match"); Network(); b.Pump(now);
        Check(b.Match.Sequence==host.Log.Count&&b.Match.Check()==a.Match.Check(),"complete snapshot rebuild restores full history");
        int before=b.Rebuilds;
        Append(new {k="next",at=++now,d="deadbeef"});
        Check(b.Rebuilds==before&&b.Match.Sequence==host.Log.Count-1,"desync cooldown pauses before suspect entry");
        now+=60001; b.Pump(now);
        Check(b.Rebuilds==before+1&&b.Match.Sequence==host.Log.Count,"cooldown expiry permits a fresh full replay");
        var compatible=new NetworkRoom("888","123",lobby);
        using var compatibleReplica=new MatchReplica(compatible,catalog);
        string numeric=init.GetRawText().Replace("\"seed\":143","\"seed\":143.0").Replace("\"format\":\"solo\"","\"format\":false");
        Check(compatible.PublishInit(JsonDocument.Parse(numeric).RootElement),"protocol accepts equivalent JSON numeric and legacy falsy format");
        compatibleReplica.Pump(now);
        Check(compatibleReplica.Match is not null&&!compatibleReplica.Match.Session.State.Duos,"compatible initialization builds solo match");
        Check(compatible.Append(0,JsonDocument.Parse("{\"k\":\"vote\",\"s\":1.0,\"at\":10000}").RootElement,now),"numeric seat representation accepted");
        compatibleReplica.Pump(now); Check(compatibleReplica.Match!.Votes.Contains(1),"numeric seat applied consistently with protocol");
        Console.WriteLine($"Match replica: {checks} replay, desync, effects and snapshot checks passed."); return checks;
    }
}
