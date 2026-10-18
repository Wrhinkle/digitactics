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
        var recoveryLobby = lobby.DeepClone().AsObject();
        recoveryLobby["slots"]!["1"] = new JsonObject { ["uid"] = "234", ["name"] = "Successor" };
        recoveryLobby["slots"]!["2"] = new JsonObject { ["uid"] = "345", ["name"] = "Peer" };
        var recoveryRooms = new Dictionary<string, NetworkRoom>();
        var recoveryPackets = new Queue<(string Sender, RoomPacket Packet)>();
        NetworkRoom Attach(string id)
        {
            var room = new NetworkRoom("999", id, recoveryLobby, 100000);
            room.Send += p => recoveryPackets.Enqueue((id, p)); recoveryRooms[id] = room;
            room.Members(["123", "234", "345"], "123", 100000); return room;
        }
        void PumpRecovery()
        {
            int remaining = 1000;
            while (recoveryPackets.TryDequeue(out var packet))
            {
                if (--remaining == 0) throw new InvalidOperationException("Recovery feedback loop.");
                recoveryRooms[packet.Packet.Recipient].Receive(packet.Sender, packet.Packet.Text, 100000);
            }
        }
        var oldHost = Attach("123"); var successor = Attach("234"); var peer = Attach("345");
        oldHost.PublishInit(init); oldHost.Append(0, JsonSerializer.SerializeToElement(new { k = "vote", s = 1, at = 100000 }), 100000); PumpRecovery();
        Check(oldHost.RequestSnapshot(100000) == 0 && recoveryPackets.Count == 0 && oldHost.Authority.IsHost, "healthy host uses its current copy without triggering takeover");
        var restarted = Attach("123");
        Check(restarted.Log.Count == 0 && successor.Log.Count == 1, "restart loses only host memory");
        restarted.RequestSnapshot(100000); PumpRecovery();
        Check(successor.Authority.IsHost && peer.Authority.Host == "234" && restarted.Authority.Host == "234", "authorized handover works while former host is still a member");
        Check(restarted.Log.Count == 1 && restarted.Init is not null && !restarted.Log.Syncing, "restarted host restores match init and history");
        Check(successor.Append(1, JsonSerializer.SerializeToElement(new { k = "start", at = 100001 }), 100001), "recovery successor can order new entries"); PumpRecovery();
        Check(restarted.Log.Count == 2 && peer.Log.Count == 2, "all peers continue after restart");
        Check(!restarted.Close() && successor.Close(), "only current host can close room"); PumpRecovery();
        Check(recoveryRooms.Values.All(r => r.IsClosed && r.Authority.Lobby is null), "closure reaches every member");
        Check(!peer.Receive("234", JsonSerializer.Serialize(new { l = "999", t = "lobby", lobby = recoveryLobby }), 100002), "late traffic cannot reopen closed room");
        Check(!peer.SubmitIntent(2, "ready", JsonSerializer.SerializeToElement(new { }), 1), "closed room refuses new intents");
        var capped = new NetworkRoom("999", "123", lobby); capped.Members(["123", "234"], "123", 0);
        var vote = JsonSerializer.SerializeToElement(new { k = "vote", s = 0, at = 1 });
        for (int start = 0; start < NetworkProtocol.MaximumLog - 1; start += NetworkProtocol.Chunk)
        {
            int length = Math.Min(NetworkProtocol.Chunk, NetworkProtocol.MaximumLog - 1 - start);
            capped.Log.Receive("123", "123", JsonSerializer.Serialize(new { l = "999", t = "logs", start, entries = Enumerable.Repeat(vote, length).ToArray() }), 0);
        }
        Check(capped.Append(NetworkProtocol.MaximumLog - 1, vote, 0), "final permitted log entry commits");
        Check(!capped.Append(NetworkProtocol.MaximumLog, vote, 0) && capped.IsClosed && capped.Log.Count == NetworkProtocol.MaximumLog, "log limit closes room without appending beyond bound");
        Console.WriteLine($"Network room: {checks} multi-client routing, recovery, succession and rate checks passed.");
        return checks;
    }
}
