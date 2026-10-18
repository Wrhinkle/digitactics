using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Networking;
using DigiTactics.Presentation;
using DigiTactics.Storage;
using DigiTactics.Rules;

internal static class NetworkHistoryChecks
{
    public static int Run(ContentCatalog catalog)
    {
        int checks=0; void Check(bool ok,string why) {checks++;if(!ok)throw new InvalidOperationException("Network history: "+why);}
        string directory=Path.Combine(Path.GetTempPath(),"digitactics-network-history-"+Guid.NewGuid().ToString("N"));
        try
        {
            foreach(bool duos in new[] {false,true})
            {
                string target=Path.Combine(directory,duos?"duos":"solo");
                var profile=new NativeProfileStore(target);
                var lobby=JsonNode.Parse("{\"host\":\"123\",\"name\":\"Room\",\"hostName\":\"Host\",\"status\":\"playing\",\"slots\":{\"1\":{\"uid\":\"123\",\"name\":\"Player\"}}}")!.AsObject();
                var room=new NetworkRoom("999","123",lobby);room.Members(["123"],"123",10000);
                room.PublishInit(JsonSerializer.SerializeToElement(new {seed=143,at=10000,planTime=30,setId="faction-wars",pace="hyper",format=duos?"duos":"solo",slots=Enumerable.Range(0,8).Select(i=>new {name="Tamer "+i,human=false}).ToArray()}));
                using var replica=new MatchReplica(room,catalog);
                using var history=new NetworkMatchHistory(replica,profile,1,()=>2000);
                replica.Pump(10000);
                Check(!profile.Exists,"opening match does not save rewards");
                long now=10000;int limit=0;
                while(replica.Match!.Session.State.Players[1].Placement==0&&limit++<500)
                {
                    now+=100000;var entry=replica.Match.Decide(now,new HashSet<int>(),new HashSet<int>())!;
                    Check(entry is not null,"AI match has next phase decision");entry!["at"]=now;
                    if(entry["k"]!.GetValue<string>()=="next")entry["d"]=replica.Match.Check();
                    Check(room.Append(room.Log.Count,JsonSerializer.SerializeToElement(entry),now),"append valid match phase");replica.Pump(now);
                }
                Check(replica.Match.Session.State.Players[1].Placement>0&&profile.Exists,"seat one placement recorded");
                using var data=JsonDocument.Parse(profile.Storage["dt.client.v1"]);var record=data.RootElement.GetProperty("matches")[0];
                Check(record.GetProperty("key").GetString()=="mp:143:1"&&record.GetProperty("mode").GetString()=="mp","original multiplayer identity and mode");
                Check(record.GetProperty("place").GetInt32()==replica.Match.Session.State.Players[1].Placement&&record.GetProperty("units").GetArrayLength()>0,"correct seat placement and final living board");
                var replayProfile=new NativeProfileStore(target+"-replay");
                using(var replay=new MatchReplica(room,catalog))
                using(var reconstructed=new NetworkMatchHistory(replay,replayProfile,1,()=>2000))
                {
                    replay.Pump(now);
                    foreach(var pair in profile.Storage) Check(replayProfile.Storage.GetValueOrDefault(pair.Key)==pair.Value,"replay reconstructs saved domain "+pair.Key);
                }
                string saved=File.ReadAllText(Path.Combine(target,"profile.json"));
                using(var reconnect=new MatchReplica(room,catalog))
                using(var duplicate=new NetworkMatchHistory(reconnect,new NativeProfileStore(target),1,()=>90000000))
                {reconnect.Pump(now);Check(duplicate.Recorder!.Award is not null,"reconnect returns cached award");}
                Check(File.ReadAllText(Path.Combine(target,"profile.json"))==saved,"reconnect does not write or pay again");
                string lockedTarget=target+"-locked";Directory.CreateDirectory(lockedTarget);var locked=new NativeProfileStore(lockedTarget);
                using(var retryReplica=new MatchReplica(room,catalog))
                using(var retry=new NetworkMatchHistory(retryReplica,locked,1,()=>2000))
                {
                    using(var guard=new FileStream(Path.Combine(lockedTarget,"profile.json.lock"),FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None))
                    {retryReplica.Pump(now);Check(!locked.Exists&&locked.Storage.Count==0&&retry.Recorder!.Error is not null,"locked replay save leaves all domains untouched");}
                    Check(retry.Recorder!.Complete(90000000),"replay payout retry succeeds");
                    Check(locked.Storage["dt.client.v1"]==profile.Storage["dt.client.v1"],"retry preserves original completion time and history");
                }
                Check(room.Append(room.Log.Count,JsonSerializer.SerializeToElement(new {k="next",at=++now,d="deadbeef"}),now),"append disagreement fixture");
                var uncertain=new NativeProfileStore(target+"-desync");
                using(var replay=new MatchReplica(room,catalog))
                using(var guarded=new NetworkMatchHistory(replay,uncertain,1,()=>2000))
                {replay.Pump(now);Check(replay.LastDesync is not null&&!uncertain.Exists&&uncertain.Storage.Count==0,"replay disagreement cannot commit uncertain rewards");}
            }
        }
        finally
        {
            string resolved=Path.GetFullPath(directory);
            if(!resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()),StringComparison.OrdinalIgnoreCase)||!Path.GetFileName(resolved).StartsWith("digitactics-network-history-",StringComparison.Ordinal))throw new InvalidOperationException("Unsafe test cleanup path.");
            if(Directory.Exists(resolved))Directory.Delete(resolved,true);
        }
        Console.WriteLine($"Network history: {checks} seat, replay, payout, deduplication and retry checks passed.");return checks;
    }
}
