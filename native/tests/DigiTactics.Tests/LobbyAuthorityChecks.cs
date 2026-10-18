using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Networking;

internal static class LobbyAuthorityChecks
{
    public static int Run(string root)
    {
        int count = 0;
        void Check(bool condition, string message) { count++; if (!condition) throw new InvalidOperationException("Lobby authority: " + message); }
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/lobby-authority.json.gz"));
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        string[] all = ["123", "234", "345", "456", "567", "678", "789", "890"];
        foreach (var scenario in fixture.RootElement.EnumerateArray())
        {
            var authority = new LobbyAuthority("999", "123", JsonNode.Parse(scenario.GetProperty("initial").GetRawText())!.AsObject());
            foreach (var step in scenario.GetProperty("steps").EnumerateArray())
            {
                authority.SetMembers(all, "123", 1000);
                Check(authority.ClaimSeat(step.GetProperty("uid").GetString()!, step.GetProperty("seat").GetInt32(), step.GetProperty("name").GetString()) == step.GetProperty("result").GetBoolean(), "seat decision");
                Check(JsonNode.DeepEquals(authority.Lobby, JsonNode.Parse(step.GetProperty("lobby").GetRawText())), "atomic seat result");
                var members = step.GetProperty("members").EnumerateArray().Select(x => x.GetString()!).ToArray();
                if (members.Length > 0)
                {
                    authority.SetMembers(members, "123", 1000);
                    Check(authority.Successor("123") == step.GetProperty("successor").GetString(), "seat-ordered successor");
                }
            }
        }
        var lobby = JsonNode.Parse("{\"host\":\"123\",\"name\":\"Room\",\"hostName\":\"Host\",\"status\":\"lobby\",\"slots\":{\"0\":{\"uid\":\"123\",\"name\":\"Host\"},\"1\":{\"uid\":\"234\",\"name\":\"Next\"}}}")!.AsObject();
        var host = new LobbyAuthority("999", "123", lobby); host.SetMembers(all, "123", 1000);
        Check(!host.ClaimSeat("9999", 2, "Intruder"), "nonmember cannot claim seat");
        Check(!host.FreeSeat("345", 1), "member cannot free another seat");
        Check(host.SetPresence("234", new JsonObject { ["name"] = "A&B", ["at"] = 0 }, false, 2000), "presence join");
        Check(host.Presence["234"]!["at"]!.GetValue<long>() == 2000 && host.Presence["234"]!["name"]!.GetValue<string>() == "AB", "host time and sanitized name");
        Check(host.SetPresence("234", new JsonObject { ["lobbyPace"] = "normal", ["at"] = 1 }, true, 3000) && host.Presence["234"]!["at"]!.GetValue<long>() == 2000, "patch preserves join time");
        Check(!host.SetPresence("345", new JsonObject(), true, 3000), "patch cannot invent presence");
        JsonElement Intent(int s, int q) => JsonSerializer.SerializeToElement(new { l = "999", t = "intent", s, q, round = 1, a = "ready", g = new { } });
        Check(!host.AcceptIntent("345", Intent(1, 1)), "intent cannot impersonate seated player");
        Check(host.AcceptIntent("234", Intent(1, 0)) && !host.AcceptIntent("234", Intent(1, 0)), "zero sequence also suppresses replay");
        Check(host.AcceptIntent("234", Intent(1, 1)) && !host.AcceptIntent("234", Intent(1, 1)) && host.AcceptIntent("234", Intent(1, 2)), "intent sequence suppresses replay");
        Check(host.Ban("234") && host.Lobby!["slots"]!["1"] is null && host.Presence["234"] is null, "ban removes seat and presence");
        Check(!host.ClaimSeat("234", 2, "Return") && !host.AcceptIntent("234", Intent(1, 3)) && !host.Ban("123"), "banned peer and host ban refused");
        var next = new LobbyAuthority("999", "234", lobby); next.SetMembers(all, "123", 1000);
        Check(!next.TryClaimHost(9000), "live host cannot be replaced");
        next.SetMembers(["345", "234"], "345", 10000);
        Check(next.ExpectedHost("234") && !next.ExpectedHost("345"), "seat priority overrides member order");
        Check(!next.TryClaimHost(16000) && next.TryClaimHost(16001) && next.Host == "234", "six-second host grace");
        Check(next.Lobby!["hostName"]!.GetValue<string>() == "Next", "successor uses seat name");
        var altered = next.Lobby!; altered["host"] = "345";
        Check(next.Host == "234" && !next.AcceptLobby("345", altered), "snapshots are copies and peers cannot replace host");
        host.SetMembers(["123"], "123", 5000);
        Check(host.Presence.Count == 0, "departed presence pruned");
        Console.WriteLine($"Lobby authority: {count} original seat/succession and identity checks passed.");
        return count;
    }
}
