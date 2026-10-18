using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Networking;

internal static class NetworkRoomChecks
{
    public static int Run()
    {
        int checks = 0;
        void Check(bool ok, string why) { checks++; if (!ok) throw new InvalidOperationException("Network room: " + why); }
        var lobby = JsonNode.Parse("{\"host\":\"123\",\"name\":\"Room\",\"hostName\":\"Host\",\"status\":\"lobby\",\"slots\":{\"0\":{\"uid\":\"123\",\"name\":\"Host\"}}}")!.AsObject();
        var host = new NetworkRoom("999", "123", lobby, 10000);
        var guest = new NetworkRoom("999", "234", initialTime: 10000);
        var third = new NetworkRoom("999", "345", initialTime: 10000);
        var rooms = new Dictionary<string, NetworkRoom> { ["123"] = host, ["234"] = guest, ["345"] = third };
        var packets = new Queue<(string Sender, RoomPacket Packet)>(); long now = 10000;
        foreach (var (id, room) in rooms) room.Send += p => packets.Enqueue((id, p));
        void Pump(Func<string, RoomPacket, bool>? drop = null)
        {
            int budget = 5000;
            while (packets.TryDequeue(out var next))
            {
                if (--budget == 0) throw new InvalidOperationException("Network feedback loop.");
                if (drop?.Invoke(next.Sender, next.Packet) != true && rooms.TryGetValue(next.Packet.Recipient, out var receiver)) receiver.Receive(next.Sender, next.Packet.Text, now);
            }
        }
        foreach (var room in rooms.Values) room.Members(rooms.Keys, "123", now);
        Pump(); guest.RequestSnapshot(now); third.RequestSnapshot(now); Pump();
        Check(guest.Authority.Host == "123" && third.Authority.Host == "123" && !guest.Log.Syncing, "initial join snapshots establish host");
        bool? seated = null; guest.SeatResult += (_, ok) => seated = ok;
        guest.ClaimSeat(1, "Guest", now); third.ClaimSeat(2, "Third", now); Pump();
        Check(seated == true && rooms.Values.All(r => r.Authority.Lobby!["slots"]!["1"]!["uid"]!.GetValue<string>() == "234"), "seat request result and broadcasts agree");
        guest.SetPresence(new JsonObject { ["name"] = "Guest", ["lobbyPace"] = "normal" }, false, now); Pump();
        Check(third.Authority.Presence["234"]!["name"]!.GetValue<string>() == "Guest", "presence forwarded through host");
        int effects = 0; third.Effect += (sender, _) => { if (sender == "234") effects++; };
        Check(guest.SendEffect(JsonSerializer.SerializeToElement(new { s = 1, kind = "emote", id = "hello" })), "send seated effect"); Pump();
        Check(effects == 1 && !guest.SendEffect(JsonSerializer.SerializeToElement(new { s = 0, kind = "sound", id = "clip" })), "effect identity is bound to own seat");
        Check(host.UpdateLobby(new JsonObject { ["planTime"] = 45 }) && !guest.UpdateLobby(new JsonObject { ["planTime"] = 5 }), "only host changes settings"); Pump();
        Check(third.Authority.Lobby!["planTime"]!.GetValue<int>() == 45, "lobby setting broadcast");
        var intents = new List<RoomIntent>(); host.Intent += intents.Add;
        Check(guest.SubmitIntent(1, "ready", JsonSerializer.SerializeToElement(new { }), 1), "submit own intent"); Pump();
        Check(intents.Count == 1 && intents[0].Sender == "234" && intents[0].Seat == 1, "host receives authenticated intent");
        guest.SubmitIntent(0, "ready", JsonSerializer.SerializeToElement(new { }), 1); Pump(); Check(intents.Count == 1, "spoofed seat never delivered");
        var init = JsonSerializer.SerializeToElement(new { seed = 42, at = now, planTime = 30, setId = "base", pace = "hyper", format = "solo", slots = Enumerable.Range(0, 8).Select(i => new { name = "Tamer" + i, human = i < 3 }).ToArray() });
        Check(host.PublishInit(init), "host publishes init"); Pump(); Check(guest.Init is not null && third.Init is not null, "clients retain init");
        for (int i = 0; i < 3; i++) Check(host.Append(i, JsonSerializer.SerializeToElement(new { k = "vote", s = i, at = now }), now), "append ordered entry");
        bool lost = false;
        Pump((sender, packet) =>
        {
            using var doc = JsonDocument.Parse(packet.Text); var m = doc.RootElement;
            if (!lost && packet.Recipient == "234" && m.GetProperty("t").GetString() == "log" && m.GetProperty("n").GetInt32() == 1) { lost = true; return true; }
            return false;
        });
        Check(lost && guest.Log.Count == 3 && guest.Log.PendingCount == 0 && third.Log.Count == 3, "missing entry recovered by log request");
        now += 10000; guest.RequestSnapshot(now); Pump();
        Check(guest.Log.Count == 3 && guest.Init is not null && !guest.Log.Syncing, "reconnect snapshot includes init and history");
        rooms.Remove("123");
        foreach (var room in rooms.Values) room.Members(rooms.Keys, "345", now);
        now += 6001; foreach (var room in rooms.Values) room.Tick(now); Pump();
        Check(guest.Authority.IsHost && third.Authority.Host == "234", "seated successor claims host and broadcasts history head");
        Check(guest.Append(3, JsonSerializer.SerializeToElement(new { k = "start", at = now }), now), "new host extends existing log"); Pump();
        Check(third.Log.Count == 4, "surviving client follows new host");
        Check(guest.Ban("345"), "host bans peer"); Pump();
        Check(third.Authority.Banned("345") && guest.Authority.Lobby!["slots"]!["2"] is null, "ban announcement arrives before peer block");
        // Bounded peer traffic is independent for each sender.
        var isolated = new NetworkRoom("999", "123", lobby); isolated.Members(["123", "234", "345"], "123", now);
        string ping = "{\"l\":\"999\",\"t\":\"ping\",\"t0\":1}";
        for (int i = 0; i < 6; i++) Check(isolated.Receive("234", ping, now), "request burst permitted");
        Check(!isolated.Receive("234", ping, now) && isolated.Receive("345", ping, now), "one peer cannot consume another allowance");
        Check(isolated.Receive("234", ping, now + 1000), "tokens refill");
        var timeout = new NetworkRoom("999", "234", lobby); timeout.Members(["123", "234"], "123", now);
        int retries = 0; bool? snapshotOk = null; timeout.Send += p => { if (p.Text.Contains("snapreq")) retries++; }; timeout.SnapshotResult += ok => snapshotOk = ok;
        timeout.RequestSnapshot(now); timeout.Tick(now + 2500); timeout.Tick(now + 9000);
        Check(retries == 2 && snapshotOk == false && !timeout.Log.Syncing, "snapshot retries once then reports timeout");
        bool? seatOk = null; timeout.SeatResult += (_, ok) => seatOk = ok; timeout.ClaimSeat(1, "Guest", now + 10000); timeout.Tick(now + 16000);
        Check(seatOk == false, "lost seat response times out");
        Console.WriteLine($"Network room: {checks} multi-client routing, recovery, succession and rate checks passed.");
        return checks;
    }
}
