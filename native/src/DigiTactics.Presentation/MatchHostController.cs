using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Networking;
using DigiTactics.Rules;

namespace DigiTactics.Presentation;

/// <summary>Orders authorized player intents and host phase decisions. One entry per 200ms host tick;
/// deadlines precede queued input, and every entry is applied through the shared log.</summary>
public sealed class MatchHostController : IDisposable
{
    private static readonly string[] Npc = ["Tai","Matt","Sora","Izzy","Mimi","Joe","Kari","T.K."];
    private readonly NetworkRoom room;
    private readonly MatchReplica replica;
    private readonly Func<long> clock;
    private readonly Queue<RoomIntent> queue = [];
    private readonly HashSet<int> done = [];
    private readonly Dictionary<string,long> offline = [];
    private readonly Dictionary<string,(long At,double Tokens)> limits = [];
    private string? host;
    private long? lastTick;
    private long lastBeat;
    private bool disposed;
    public int Queued => queue.Count;
    public long Dropped { get; private set; }
    public MatchHostController(NetworkRoom room, MatchReplica replica, Func<long>? clock = null)
    {
        this.room=room; this.replica=replica; this.clock=clock??(()=>Environment.TickCount64); host=room.Authority.Host;
        room.Intent+=Intent; replica.Applied+=Applied; replica.Rebuilt+=Rebuilt;
    }
    private void Clear() { queue.Clear(); done.Clear(); offline.Clear(); limits.Clear(); lastTick=null; lastBeat=0; }
    private void Rebuilt(LockstepMatch _) => Clear();
    private void Applied(JsonElement entry,LockstepResult _) { if(entry.GetProperty("k").GetString() is "start" or "next") done.Clear(); }
    private void ObserveHost() { if(host!=room.Authority.Host) { host=room.Authority.Host; Clear(); } }
    private static int Round(LockstepMatch match) => match.Session.Rounds.Position.Stage*100+match.Session.Rounds.Position.Round;
    private bool Owned(RoomIntent intent,JsonObject? lobby) => intent.Seat is >=0 and <8 && lobby?["slots"]?[intent.Seat.ToString()]?["uid"]?.GetValue<string>()==intent.Sender && room.Authority.Member(intent.Sender);
    private void Intent(RoomIntent intent)
    {
        ObserveHost();
        if(!room.Authority.IsHost||room.IsClosed||room.Log.Syncing||replica.Match is not {} match||!Owned(intent,room.Authority.Lobby)||intent.Round!=Round(match)||!NetworkProtocol.Action(intent.Action,intent.Arguments)||queue.Count>=64||queue.Count(i=>i.Sender==intent.Sender)>=8) { Dropped++; return; }
        long now=clock(); var budget=limits.GetValueOrDefault(intent.Sender,(now,8d));
        double tokens=Math.Min(8,budget.Item2+Math.Max(0,now-budget.Item1)*.004);
        limits[intent.Sender]=(now,tokens>=1?tokens-1:tokens);
        if(tokens<1) { Dropped++; return; }
        queue.Enqueue(intent);
    }
    private void Append(JsonObject entry,LockstepMatch match,long localTime,long serverTime)
    {
        entry["at"]=serverTime;
        if(entry["k"]!.GetValue<string>()=="next") entry["d"]=match.Check();
        if(room.Append(match.Sequence,JsonSerializer.SerializeToElement(entry),localTime)) replica.Pump(localTime);
    }
    public void Pump(long localTime)
    {
        ObjectDisposedException.ThrowIf(disposed,this); ObserveHost(); replica.Pump(localTime);
        if(!room.Authority.IsHost||room.IsClosed||room.Log.Syncing||replica.Match is not {} match||match.Sequence!=room.Log.Count||room.Authority.Lobby is not {} lobby) return;
        if(lastTick is {} last&&localTime-last<200) return; lastTick=localTime;
        long now=(long)room.ServerTime(localTime);
        for(int i=0;i<8;i++)
        {
            var seat=lobby["slots"]?[i.ToString()]; var player=match.Session.State.Players[i];
            string? action=seat is not null&&!player.IsHuman&&player.Alive?"claim":seat is null&&player.IsHuman?"release":null;
            if(action is not null) { Append(new() { ["k"]="a",["s"]=i,["a"]=action,["g"]=new JsonObject { ["name"]=action=="claim"?seat!["name"]!.GetValue<string>():Npc[i] } },match,localTime,now); return; }
        }
        var presence=room.Authority.Presence; var online=new HashSet<int>();
        for(int i=0;i<8;i++)
        {
            string? uid=lobby["slots"]?[i.ToString()]?["uid"]?.GetValue<string>(); if(uid is null) continue;
            if(uid==room.Authority.LocalId||presence[uid] is not null) { offline.Remove(uid); online.Add(i); continue; }
            if(!offline.TryGetValue(uid,out long since)) offline[uid]=now;
            else if(now-since>LockstepMatch.OfflineReleaseMs) room.FreeSeat(i);
        }
        foreach(string peer in limits.Keys.Where(p=>!room.Authority.Member(p)).ToArray()) limits.Remove(peer);
        foreach(string peer in offline.Keys.Where(p=>!lobby["slots"]!.AsObject().Any(s=>s.Value?["uid"]?.GetValue<string>()==p)).ToArray()) offline.Remove(peer);
        if(match.Decide(now,online,done) is {} decision) { Append(decision,match,localTime,now); return; }
        while(queue.TryDequeue(out var intent))
        {
            if(!Owned(intent,room.Authority.Lobby)||intent.Round!=Round(match)) { Dropped++; continue; }
            if(intent.Action=="done") { done.Add(intent.Seat); continue; }
            var entry=intent.Action=="ready"?new JsonObject { ["k"]="vote",["s"]=intent.Seat }
                :new JsonObject { ["k"]="a",["s"]=intent.Seat,["a"]=intent.Action,["g"]=JsonNode.Parse(intent.Arguments.GetRawText()) };
            Append(entry,match,localTime,now); return;
        }
        if(now-lastBeat>15000||lobby["round"]?.GetValue<string>()!=match.Session.Rounds.Position.Label||match.Session.State.Over&&lobby["status"]?.GetValue<string>()!="over")
        {
            lastBeat=now;
            room.UpdateLobby(new() { ["updated"]=now,["round"]=match.Session.Rounds.Position.Label,["alive"]=new JsonArray(match.Session.State.Players.Select(p=>JsonValue.Create(p.Alive)).ToArray()),["status"]=match.Session.State.Over?"over":"playing" });
        }
    }
    public void Dispose() { if(disposed)return; room.Intent-=Intent; replica.Applied-=Applied; replica.Rebuilt-=Rebuilt; Clear(); disposed=true; }
}
