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
    private Func<long> networkClock = () => Environment.TickCount64;
    private bool networkDone;
    internal string? NetworkStateHash => networkReplica?.Match?.Check();
    public void AttachNetworkMatch(NetworkRoom room, Func<long>? clock = null)
    {
        DetachNetworkMatch(); networkRoom = room; networkClock = clock ?? (() => Environment.TickCount64);
        localSeat = FindNetworkSeat();
        if (localSeat < 0) { networkRoom = null; localSeat = 0; throw new InvalidOperationException("Claim a seat before entering the match."); }
        history = null;
        networkReplica = new(room, catalog); networkReplica.Rebuilt += NetworkRebuilt; networkReplica.Applied += NetworkApplied;
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
        battle?.Dispose(); battle = null; watching = null; combatAudio = null;
        Session = match.Session; selectedItem = selectedUnit = null; viewed = localSeat; backgroundTime = 0; networkDone = false;
        CompletedRounds = networkRoom!.Log.Entries.Count(e => e.GetProperty("k"u8).GetString() == "end");
        Watch(localSeat); audio.Music(Session.Rounds.Phase == MatchPhase.Combat ? "combat" : "planning");
        status = "Connected. Your actions are confirmed by the host.";
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
        if (networkRoom is null || Session is null || FindNetworkSeat() != localSeat) return false;
        bool sent = networkRoom.SubmitIntent(localSeat, action, arguments.ValueKind == JsonValueKind.Undefined ? JsonSerializer.SerializeToElement(new { }) : arguments,
            Session.Rounds.Position.Stage * 100 + Session.Rounds.Position.Round);
        status = sent ? "Waiting for the host..." : "That action could not be sent."; return sent;
    }
    private void NetworkReplayDone() { if (!networkDone && SendNetworkAction("done")) networkDone = true; }
    private void UpdateNetworkMatch()
    {
        if (networkRoom is null) return;
        if (networkRoom.IsClosed || FindNetworkSeat() != localSeat)
        {
            DetachNetworkMatch(); Session = null; battle?.Dispose(); battle = null; watching = null; combatAudio = null; audio.Music(null); return;
        }
        networkHost!.Pump(networkClock());
    }
    public void DetachNetworkMatch()
    {
        networkHost?.Dispose(); networkHost = null; networkReplica?.Dispose(); networkReplica = null;
        networkRoom = null; networkDone = false;
    }
}
