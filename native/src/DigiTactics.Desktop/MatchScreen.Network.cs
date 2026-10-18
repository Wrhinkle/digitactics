using System.Text.Json;
using DigiTactics.Networking;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private NetworkRoom? networkRoom;
    private MatchReplica? networkReplica;
    private MatchHostController? networkHost;
    private NetworkMatchHistory? networkHistory;
    private Func<long> networkClock = () => Environment.TickCount64;
    private bool networkDone;
    internal string? NetworkStateHash => networkReplica?.Match?.Check();
    internal bool IsSpectating => networkRoom is not null && localSeat < 0;
    public void AttachNetworkMatch(NetworkRoom room, Func<long>? clock = null)
    {
        DetachNetworkMatch(); networkRoom = room; networkClock = clock ?? (() => Environment.TickCount64);
        room.Effect += NetworkEffect;
        localSeat = FindNetworkSeat();
        history = null;
        networkReplica = new(room, catalog); networkReplica.Rebuilt += NetworkRebuilt; networkReplica.Applied += NetworkApplied;
        networkReplica.Building += MatchHudBuilding; networkReplica.Advanced += MatchHudAdvanced;
        networkHistory = profile is null || localSeat < 0 ? null : new(networkReplica, profile, localSeat);
        networkReplica.Desync += _ => status = "Match state differed from the host; rebuilding from shared history.";
        networkHost = new(room, networkReplica, networkClock);
        networkReplica.Pump(networkClock());
    }
    private int FindNetworkSeat()
    {
        var lobby = networkRoom?.Authority.Lobby;
        for (int i = 0; i < 8; i++) if (lobby?["slots"]?[i.ToString()]?["uid"]?.GetValue<string>() == networkRoom?.Authority.LocalId) return i;
        return -1;
    }
    private void NetworkRebuilt(LockstepMatch match)
    {
        castFeed=null;
        battle?.Dispose(); battle = null; watching = null; combatAudio = null;
        Session = match.Session; selectedItem = selectedUnit = null; viewed = localSeat >= 0 ? localSeat : Math.Clamp(viewed, 0, 7); backgroundTime = 0; networkDone = false;
        history = networkHistory?.Recorder;
        CompletedRounds = networkRoom!.Log.Entries.Count(e => e.GetProperty("k"u8).GetString() == "end");
        Watch(viewed); audio.Music(Session.Rounds.Phase == MatchPhase.Combat ? "combat" : "planning");
        status = IsSpectating ? "Spectating. Choose a Tamer in the standings to watch." : "Connected. Your actions are confirmed by the host.";
    }
    private void NetworkApplied(JsonElement entry, LockstepResult result)
    {
        switch (entry.GetProperty("k"u8).GetString())
        {
            case "a": if ((int)entry.GetProperty("s"u8).GetDouble() == localSeat) ActionFeedback(entry.GetProperty("a"u8).GetString()!, result.Action); break;
            case "start": if (result.Applied) { networkDone = false; CombatStarted(); } break;
            case "end": if (result.Applied) RoundFinished(); break;
            case "next": if (result.Applied) { networkDone = false; RoundAdvanced(); } break;
            case "vote": if ((int)entry.GetProperty("s"u8).GetDouble() == localSeat) status = "Ready. Waiting for the other Tamers."; break;
        }
    }
    private bool SendNetworkAction(string action, JsonElement arguments = default)
    {
        if (networkRoom is null || Session is null || localSeat < 0 || FindNetworkSeat() != localSeat) return false;
        bool sent = networkRoom.SubmitIntent(localSeat, action, arguments.ValueKind == JsonValueKind.Undefined ? JsonSerializer.SerializeToElement(new { }) : arguments,
            Session.Rounds.Position.Stage * 100 + Session.Rounds.Position.Round);
        status = sent ? "Waiting for the host..." : "That action could not be sent."; return sent;
    }
    private void NetworkReplayDone() { if (!IsSpectating && !networkDone && SendNetworkAction("done")) networkDone = true; }
    private void UpdateNetworkMatch()
    {
        if (networkRoom is null) return;
        if (networkRoom.IsClosed || networkRoom.Authority.Banned(networkRoom.Authority.LocalId))
        {
            DetachNetworkMatch(); Session = null; battle?.Dispose(); battle = null; watching = null; combatAudio = null; audio.Music(null); return;
        }
        if (FindNetworkSeat() != localSeat) AttachNetworkMatch(networkRoom, networkClock);
        networkHost!.Pump(networkClock());
    }
    private void NetworkSeatControls()
    {
        if (networkRoom is null || Session is null || Session.State.Over) return;
        if (IsSpectating)
        {
            var slots = networkRoom.Authority.Lobby?["slots"];
            int available = Enumerable.Range(0, 8).FirstOrDefault(i => slots?[i.ToString()] is null && Session.State.Players[i].Alive, -1);
            if (available >= 0 && HomeButton("joinSeat", $"Take seat {available + 1}",110,36)) networkRoom.ClaimSeat(available, SteamName, networkClock());
        }
        else if (HomeButton("spectate", "Spectate",110,36)) networkRoom.FreeSeat(localSeat);
    }
    public void DetachNetworkMatch()
    {
        if (networkRoom is not null) networkRoom.Effect -= NetworkEffect;
        if(networkReplica is not null){networkReplica.Building-=MatchHudBuilding;networkReplica.Advanced-=MatchHudAdvanced;}
        networkHost?.Dispose(); networkHost = null; networkHistory?.Dispose(); networkHistory = null; networkReplica?.Dispose(); networkReplica = null;
        networkRoom = null; networkDone = false;
    }
}
