using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Networking;

internal static class ReplicatedLogChecks
{
    public static int Run(string root)
    {
        int checks = 0;
        void Check(bool ok, string why) { checks++; if (!ok) throw new InvalidOperationException("Replicated log: " + why); }
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/replicated-log.json.gz"));
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        foreach (var scenario in fixture.RootElement.EnumerateArray())
        {
            var log = new ReplicatedLog("999");
            foreach (var step in scenario.EnumerateArray())
            {
                var update = log.Receive("123", "123", JsonSerializer.Serialize(new { l = "999", t = "log", n = step.GetProperty("n").GetInt32(), e = step.GetProperty("e") }), step.GetProperty("now").GetInt64());
                Check(JsonElement.DeepEquals(JsonSerializer.SerializeToElement(log.Entries), step.GetProperty("log")), "ordered entries match original");
                Check(log.PendingCount == step.GetProperty("pending").GetInt32(), "pending bound/drain");
                Check(update.Reset == step.GetProperty("reset").GetBoolean(), "conflict reset");
                Check(update.RequestFrom == (step.GetProperty("request").ValueKind == JsonValueKind.Null ? null : step.GetProperty("request").GetInt32()), "gap request/throttle");
            }
        }
        var replica = new ReplicatedLog("999");
        string Entry(int n, int seat = 0) => JsonSerializer.Serialize(new { l = "999", t = "log", n, e = new { k = "vote", s = seat, at = 1 } });
        JsonObject Snapshot(int start, int total, params int[] seats) => new()
        {
            ["l"] = "999", ["t"] = "snap", ["rid"] = 1, ["start"] = start, ["total"] = total,
            ["entries"] = new JsonArray(seats.Select(s => (JsonNode)new JsonObject { ["k"] = "vote", ["s"] = s, ["at"] = 1 }).ToArray()),
            ["lobby"] = new JsonObject { ["host"] = "123", ["name"] = "Room", ["hostName"] = "Host", ["status"] = "playing", ["slots"] = new JsonObject() },
            ["presence"] = new JsonObject(), ["init"] = null, ["now"] = 1000
        };
        replica.Receive("123", "123", Entry(0), 1);
        Check(!replica.Receive("456", "123", Entry(1), 2).Accepted && replica.Count == 1, "nonhost cannot append");
        Check(!replica.Receive("123", "123", Entry(1).Replace("999", "998"), 2).Accepted, "wrong room rejected");
        replica.BeginSnapshot(1);
        replica.Receive("123", "123", Entry(3, 3), 3);
        Check(replica.Count == 1 && replica.PendingCount == 1, "live log waits for snapshot");
        var second = Snapshot(1, 3, 1, 2);
        Check(replica.Receive("123", "123", second.ToJsonString(), 4).Snapshot is null && replica.Count == 1, "partial snapshot retains old state");
        replica.Receive("123", "123", second.ToJsonString(), 5);
        Check(replica.Count == 1 && replica.Syncing, "duplicate chunk does not complete snapshot");
        var wrongSize = Snapshot(0, 2, 0);
        Check(!replica.Receive("123", "123", wrongSize.ToJsonString(), 6).Accepted, "inconsistent totals rejected");
        var first = Snapshot(0, 3, 0); first["rid"] = 2;
        Check(!replica.Receive("123", "123", first.ToJsonString(), 6).Accepted, "stale request rejected"); first["rid"] = 1;
        Check(!replica.Receive("456", "123", first.ToJsonString(), 6).Accepted, "nonhost snapshot rejected");
        var complete = replica.Receive("123", "123", first.ToJsonString(), 7);
        Check(complete.Snapshot?.HostTime == 1000 && replica.Count == 4 && !replica.Syncing && replica.PendingCount == 0, "complete snapshot plus concurrent entry");
        Check(replica.Entries.Select(e => e.GetProperty("s").GetInt32()).SequenceEqual(new[] { 0, 1, 2, 3 }), "chunk order restored");
        replica.BeginSnapshot(2); replica.Receive("123", "123", Entry(4, 4), 8);
        Check(!replica.ExpireSnapshot(1) && replica.Syncing, "old timeout cannot cancel new request");
        Check(replica.ExpireSnapshot(2) && replica.Count == 5 && !replica.Syncing, "timeout uses current copy and drains live entries");
        var head = replica.Receive("123", "123", "{\"l\":\"999\",\"t\":\"head\",\"n\":2}", 9);
        Check(head.Reset && replica.Count == 2, "shorter new host history triggers replay");
        Check(replica.Receive("123", "123", "{\"l\":\"999\",\"t\":\"head\",\"n\":4}", 10).RequestFrom == 2, "longer head requests missing entries");
        Check(!replica.Receive("123", "123", Entry(0).Replace("\"at\":1", "\"at\":1.0"), 11).Reset, "equivalent JSON numbers do not reset history");
        replica.BeginSnapshot(1);
        Check(replica.Receive("123", "123", Snapshot(0, 0).ToJsonString(), 12).Snapshot is not null && replica.Count == 0, "empty snapshot completes");
        var bounded = new ReplicatedLog("999"); bounded.BeginSnapshot(1);
        for (int i = 0; i < 512; i++) bounded.Receive("123", "123", Entry(i), 20);
        Check(bounded.PendingCount == 512 && !bounded.Receive("123", "123", Entry(512), 21).Accepted, "pending buffer cannot exceed original bound");
        bounded.ExpireSnapshot(1);
        Check(bounded.Count == 512 && bounded.PendingCount == 0, "bounded buffer drains on snapshot timeout");
        var switched = new ReplicatedLog("999"); switched.BeginSnapshot(1);
        switched.Receive("123", "123", Snapshot(1, 2, 1).ToJsonString(), 1);
        var replacement = Snapshot(0, 1, 7); replacement["lobby"]!["host"] = "456";
        Check(switched.Receive("456", "456", replacement.ToJsonString(), 2).Snapshot is not null && switched.Count == 1 && switched.Entries[0].GetProperty("s").GetInt32() == 7, "new expected host replaces incomplete old-host snapshot");
        Console.WriteLine($"Replicated log: {checks} original transitions, authorization, snapshot and recovery checks passed.");
        return checks;
    }
}
