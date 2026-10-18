using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Storage;

internal static class RecentPlayerChecks
{
    public static int Run(string root)
    {
        int checks = 0;
        void Check(bool ok, string label) { checks++; if (!ok) throw new InvalidOperationException("Recent players: " + label); }
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/recent-players.json.gz"));
        using var gzip = new System.IO.Compression.GZipStream(file, System.IO.Compression.CompressionMode.Decompress);
        using var document = JsonDocument.Parse(gzip);
        string? saved = document.RootElement.GetProperty("initial").GetRawText();
        foreach (var test in document.RootElement.GetProperty("cases").EnumerateArray())
        {
            var members = test.GetProperty("members").EnumerateArray().Select(m => new EncounteredPlayer(
                m.TryGetProperty("name", out var name) ? name.GetString() : null, m.TryGetProperty("me", out var me) && me.GetBoolean()));
            string? next = RecentPlayers.Remember(saved, "Me", members, test.GetProperty("at").GetInt64());
            Check(JsonNode.DeepEquals(JsonNode.Parse(next!), JsonNode.Parse(test.GetProperty("expected").GetRawText())), "original source parity");
            Check((next != saved) == test.GetProperty("changed").GetBoolean(), "write only when changed");
            Check(RecentPlayers.Read(next).Count <= 20, "bounded readable history");
            saved = next;
        }
        Check(RecentPlayers.Remember(null, "Me", [], 0) is null, "empty encounters do not create progress");
        Check(RecentPlayers.Read(RecentPlayers.Remember(null, "Me", [new("Peer")], 1)).Single() == new RecentPlayer("Peer", 1), "fresh profile");
        foreach (string bad in new[] { "[]", "{\"recent\":{}}", "{\"recent\":[null]}", "{\"recent\":[{\"name\":\"Peer\",\"at\":\"oops\"}]}" })
        {
            bool rejected = false;
            try { RecentPlayers.Remember(bad, "Me", [new("Peer")], 1); } catch (InvalidDataException) { rejected = true; }
            Check(rejected, "malformed progress rejected");
        }
        Console.WriteLine($"Recent players: {checks:N0} checks passed.");
        return checks;
    }
}
