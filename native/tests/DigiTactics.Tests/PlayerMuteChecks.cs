using System.Text.Json;
using DigiTactics.Storage;

internal static class PlayerMuteChecks
{
    public static int Run()
    {
        int checks = 0;
        void Check(bool ok, string label) { checks++; if (!ok) throw new InvalidOperationException("Player mutes: " + label); }
        var mutes = new PlayerMutes(null);
        Check(!mutes.Contains("123") && mutes.Toggle("123") == "[\"123\"]" && mutes.Contains("123"), "mute and serialize");
        Check(new PlayerMutes(mutes.Toggle("234")).Contains("123"), "restart retains prior mute");
        Check(mutes.Toggle("123") == "[\"234\"]" && !mutes.Contains("123"), "unmute preserves other players");
        var mixed = new PlayerMutes("[null,12,\"x\",\"x\",false,\"y\"]");
        Check(mixed.Toggle("z") == "[\"x\",\"y\",\"z\"]", "original string filter and ordered-set deduplication");
        var bounded = new PlayerMutes(JsonSerializer.Serialize(Enumerable.Range(0, 300).Select(i => i.ToString())));
        Check(!bounded.Contains("43") && bounded.Contains("44") && bounded.Contains("299"), "last 256 imported entries");
        bounded.Toggle("new"); Check(!bounded.Contains("44") && bounded.Contains("45"), "oldest mute evicted at capacity");
        bounded.Toggle("45"); bounded.Toggle("45"); bounded.Toggle("next");
        Check(bounded.Contains("45") && !bounded.Contains("46"), "unmute/remute moves player to end");
        Check(new PlayerMutes("{}").Toggle("123") == "[\"123\"]", "original non-array fallback");
        bool invalid = false; try { _ = new PlayerMutes("{"); } catch (JsonException) { invalid = true; }
        Check(invalid, "corrupt preference cannot be silently rewritten");
        Console.WriteLine($"Player mutes: {checks} migration, ordering, bounds and preservation checks passed.");
        return checks;
    }
}
