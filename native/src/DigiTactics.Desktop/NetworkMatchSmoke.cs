using System.Text.Json;
using System.Text.Json.Nodes;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Networking;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Two room controllers, one real rendered guest screen in seat one.</summary>
internal sealed class NetworkMatchSmoke : IDisposable
{
    private readonly NetworkRoom host, guest;
    private readonly MatchReplica replica;
    private readonly MatchHostController controller;
    private readonly Queue<(string Sender, RoomPacket Packet)> packets = [];
    private long now = 10000;
    private int stage, cooldown = 3;
    private bool down, combatRendered, planningRendered;
    private readonly bool spectator;
    private bool spectatorVerified;
    public string Pending => "network UI stage " + stage;
    public NetworkMatchSmoke(ContentCatalog catalog, MatchScreen screen, bool duos = false, bool spectator = false)
    {
        this.spectator = spectator; if (spectator) stage = -2;
        var lobby = JsonNode.Parse("{\"host\":\"123\",\"name\":\"Test room\",\"hostName\":\"Host\",\"status\":\"playing\",\"slots\":{\"1\":{\"uid\":\"234\",\"name\":\"Guest\"}}}"u8)!.AsObject();
        if (spectator) { lobby["slots"]!.AsObject().Remove("1"); lobby["slots"]!["0"] = new JsonObject { ["uid"] = "123", ["name"] = "Host" }; }
        host = new("999", "123", lobby); guest = new("999", "234", lobby);
        host.Members(["123", "234"], "123", now); guest.Members(["123", "234"], "123", now);
        host.Send += p => packets.Enqueue(("123", p)); guest.Send += p => packets.Enqueue(("234", p));
        host.PublishInit(JsonSerializer.SerializeToElement(new { seed = 143, at = now, planTime = 30, setId = "faction-wars", pace = "hyper", format = duos ? "duos" : "solo", slots = Enumerable.Range(0, 8).Select(i => new { name = i == 1 ? "Guest" : "Tamer " + i, human = i == (spectator ? 0 : 1) }).ToArray() }));
        PumpPackets(); replica = new(host, catalog); controller = new(host, replica, () => now); replica.Pump(now);
        guest.SetPresence(new JsonObject { ["name"] = "Guest" }, false, now); PumpPackets();
        screen.AttachNetworkMatch(guest, () => now);
    }
    private void PumpPackets()
    {
        while (packets.TryDequeue(out var p)) (p.Packet.Recipient == "123" ? host : guest).Receive(p.Sender, p.Packet.Text, now);
    }
    public void Input(MatchScreen screen)
    {
        now += 200; PumpPackets(); controller.Pump(now); PumpPackets();
        if (cooldown-- > 0) return;
        if (down) { ImGui.GetIO().AddMouseButtonEvent(0, false); down = false; stage++; cooldown = 3; return; }
        string? key = stage switch { -2 => "joinSeat", 0 when screen.Session?.Rounds.RoundType != "carousel" => "lock", 2 => "fight", 4 => "spectate", _ => null };
        if (key is not null && screen.Buttons.TryGetValue(key, out var point)) { ImGui.GetIO().AddMousePosEvent(point.X, point.Y); ImGui.GetIO().AddMouseButtonEvent(0, true); down = true; }
    }
    public bool Verify(MatchScreen screen, int2 size, string? capture)
    {
        planningRendered |= screen.PlanningDrawCalls > 0;
        if (stage == -2 && screen.IsSpectating)
        {
            if (screen.Buttons.ContainsKey("lock") || screen.Buttons.ContainsKey("fight") || screen.Buttons.ContainsKey("buy/0")) throw new InvalidOperationException("Spectator sees player controls.");
            spectatorVerified = true;
        }
        if (stage == -1 && !screen.IsSpectating) stage = 0;
        if (stage == 1 && replica.Match!.Session.State.Players[1].Shop.Locked)
        {
            if (!screen.Session!.State.Players[1].Shop.Locked) return false;
            if (host.Log.Entries.Last(e => e.GetProperty("k"u8).GetString() == "a").GetProperty("s"u8).GetDouble() != 1) throw new InvalidOperationException("Online action used the wrong seat.");
            stage = 2;
        }
        if (screen.Session?.Rounds.Phase == MatchPhase.Combat && screen.BattleDrawCalls > 0)
        {
            if (!combatRendered && capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "combat.bmp"), size.X, size.Y);
            combatRendered = true;
        }
        if (stage < 3 || screen.CompletedRounds < 1 || screen.Session?.Rounds.Phase != MatchPhase.Planning) return false;
        if (spectator && stage == 3) { stage = 4; return false; }
        if (spectator && (stage < 5 || !screen.IsSpectating)) return false;
        if (spectator && (!spectatorVerified || screen.Buttons.ContainsKey("lock") || screen.Buttons.ContainsKey("fight"))) throw new InvalidOperationException("Returning to spectating did not remove player controls.");
        if (!planningRendered || !combatRendered || screen.Session.State.Seed != replica.Match!.Session.State.Seed) throw new InvalidOperationException("Online match did not render planning and combat.");
        if (screen.NetworkStateHash != replica.Match.Check()) throw new InvalidOperationException("Rendered guest state differs from host after settlement.");
        if (capture is not null) FrameCapture.SaveBitmap(capture, size.X, size.Y);
        Console.WriteLine(spectator ? "Spectator UI smoke passed: read-only view, take seat, replicated controls/combat, return to spectating and matching host state."
            : "Network match UI smoke passed: guest seat-one input, replicated shop lock, ready vote, rendered 3D planning/combat, host settlement and next round."); return true;
    }
    public void Dispose() { controller.Dispose(); replica.Dispose(); }
}
