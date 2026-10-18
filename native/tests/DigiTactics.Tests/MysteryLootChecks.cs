using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Presentation;
using DigiTactics.Storage;

internal static class MysteryLootChecks
{
    public static int Run(string root)
    {
        int checks = 0;
        void Check(bool value, string message) { checks++; if (!value) throw new InvalidOperationException("Mystery loot: " + message); }
        using var fixture = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "tests/fixtures/mystery-loot.json")));
        string? Initial(JsonElement initial, string field) => initial.TryGetProperty(field, out var value) && value.ValueKind != JsonValueKind.Null
            ? value.ValueKind == JsonValueKind.String ? value.GetString() : value.GetRawText() : null;
        void Verify(JsonElement expected, string? loot, string? wallet, string label)
        {
            Check(JsonNode.DeepEquals(JsonNode.Parse(loot!), JsonNode.Parse(expected.GetProperty("state").GetRawText())), label + " complete source save including ledger and pity");
            var inventory = MysteryLoot.Inventory(loot); var source = expected.GetProperty("inventory");
            Check(inventory.Daily == source.GetProperty("daily").GetInt64() && inventory.Vault == source.GetProperty("vault").GetInt64() &&
                inventory.Keys == source.GetProperty("keys").GetInt64() && inventory.Day == source.GetProperty("day").GetString() && inventory.DayProgress == source.GetProperty("dayProgress").GetInt32(), label + " original inventory and active-day progress");
            var collection = new CosmeticCollection(wallet);
            foreach (string kind in CosmeticCollection.Kinds)
                Check(CosmeticCollection.Items(kind).Where(item => collection.Owns(kind, item.Id)).Select(item => item.Id)
                    .SequenceEqual(expected.GetProperty("owned").GetProperty(kind).EnumerateArray().Select(item => item.GetString())), label + " original ownership " + kind);
        }
        foreach (string pool in new[] { "daily", "vault" })
        {
            var expected = fixture.RootElement.GetProperty("pools").GetProperty(pool).EnumerateArray().Select(item =>
                new MysteryReward(item.GetProperty("kind").GetString()!, item.GetProperty("id").GetString()!)).ToArray();
            Check(MysteryLoot.Available(null, pool).SequenceEqual(expected), pool + " all source pool entries and category order");
            foreach (string kind in CosmeticCollection.Kinds)
                Check(expected.Count(item => item.Kind == kind) == (kind == "board" ? 5 : 20), pool + " original category count " + kind);
        }
        foreach (var sample in fixture.RootElement.GetProperty("cases").EnumerateArray())
        {
            string name = sample.GetProperty("name").GetString()!;
            string? loot = Initial(sample.GetProperty("initial"), "loot"), wallet = Initial(sample.GetProperty("initial"), "cosmetics");
            if (name == "malformed-save-recovers")
            {
                // Native saves deliberately retain unreadable data instead of the browser's silent reset.
                bool rejected = false; try { MysteryLoot.Inventory(loot); } catch (JsonException) { rejected = true; }
                Check(rejected, "malformed JSON is preserved for recovery rather than reset"); continue;
            }
            bool started = false;
            foreach (var step in sample.GetProperty("steps").EnumerateArray())
            {
                var random = step.GetProperty("random").EnumerateArray().Select(value => value.GetUInt32()).ToArray(); int used = 0;
                double Roll() { Check(used < random.Length, name + " random stream not exhausted"); return random[used++] / 4294967296.0; }
                string operation = step.GetProperty("op").GetString()!;
                void Apply(MysteryLootUpdate update) { loot = update.Loot; if (update.Wallet is not null) wallet = update.Wallet; }
                if (!started || operation == "reload") { Apply(MysteryLoot.Settle(loot, wallet)); started = true; }
                var expected = step.GetProperty("expected");
                if (operation == "round")
                {
                    var game = step.GetProperty("game"); string participation = game.GetProperty("participation").GetString()!;
                    bool participates = participation is "a" or "a2" or "b" or "b2" or "ghost-a" or "ghost-a2";
                    var round = new MysteryRound(game.GetProperty("setId").GetString()!, game.GetProperty("duo").GetBoolean(), game.GetProperty("seed").GetUInt32(),
                        game.GetProperty("humanId").GetInt32(), game.GetProperty("stage").GetInt32(), game.GetProperty("round").GetInt32(), participates);
                    var update = MysteryLoot.RoundEnd(loot, round, DateOnly.ParseExact(step.GetProperty("day").GetString()!, "yyyy-MM-dd", CultureInfo.InvariantCulture), Roll);
                    Apply(update);
                    Check(update.Drops.SequenceEqual(expected.GetProperty("result").EnumerateArray().Select(value => value.GetString())), name + " exact drop order");
                }
                else if (operation == "open")
                {
                    Apply(MysteryLoot.Settle(loot, wallet));
                    var update = MysteryLoot.PrepareOpen(loot, wallet, step.GetProperty("pool").GetString()!, Roll); Apply(update);
                    if (update.Changed) Apply(MysteryLoot.Settle(loot, wallet));
                    var result = expected.GetProperty("result");
                    Check(result.ValueKind == JsonValueKind.Null ? update.Reward is null : update.Reward == new MysteryReward(result.GetProperty("kind").GetString()!, result.GetProperty("id").GetString()!), name + " source selected discovery");
                }
                Verify(expected, loot, wallet, name);
                Check(used == expected.GetProperty("randomUsed").GetInt32(), name + " exact random consumption including short-circuited pity");
            }
        }
        // The browser persists debit, grant and pending-clear separately. Native settles grant and clear
        // atomically, while retaining the same imported pending record and recovery endpoint.
        foreach (var sample in fixture.RootElement.GetProperty("failureCases").EnumerateArray())
        {
            string name = sample.GetProperty("name").GetString()!;
            if (name is not ("failed-cosmetic-grant-recovers" or "failed-pending-clear-recovers" or "startup-grant-failure-preserves-pending")) continue;
            var steps = sample.GetProperty("steps"); var interrupted = steps[0].GetProperty("expected");
            string loot = interrupted.GetProperty("state").GetRawText();
            string wallet = "{\"owned\":" + interrupted.GetProperty("owned").GetRawText() + "}";
            var recovered = MysteryLoot.Settle(loot, wallet);
            Check(recovered.Changed && recovered.Reward is not null && MysteryLoot.Inventory(recovered.Loot).Pending is null, name + " pending recovery resolves original selected reward");
            Verify(steps[1].GetProperty("expected"), recovered.Loot, recovered.Wallet, name);
            Check(!MysteryLoot.Settle(recovered.Loot, recovered.Wallet).Changed, name + " settled recovery is idempotent");
        }
        var date = new DateOnly(2026, 10, 18); var liveRound = new MysteryRound("reckoning", false, 143, 0, 2, 1, true);
        foreach (var excluded in new[] { liveRound with { Live = false }, liveRound with { Participated = false }, liveRound with { Seat = -1 } })
        {
            var update = MysteryLoot.RoundEnd(null, excluded, date, () => throw new InvalidOperationException("Excluded round consumed local random."));
            Check(!update.Changed && update.Drops.Length == 0 && MysteryLoot.Inventory(update.Loot).Daily == 0, "replay, spectator or absent local seat cannot award rewards");
        }
        foreach (double invalid in new[] { -0.01, 1, double.NaN, double.PositiveInfinity })
        {
            bool rejected = false; try { MysteryLoot.PrepareOpen("{\"daily\":1}", null, "daily", () => invalid); } catch (InvalidDataException) { rejected = true; }
            Check(rejected, "invalid entropy cannot select or consume a reward");
        }
        string directory = Path.Combine(Path.GetTempPath(), "digitactics-mystery-check-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string path = Path.Combine(directory, "profile.json"); var profile = new NativeProfileStore(directory); var progress = new MysteryLootProgress(profile);
            using (var guard = new FileStream(path + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                Check(progress.RecordRound(liveRound, date, () => 0).Length == 0 && progress.Error is not null && !profile.Exists, "failed round transaction does not display or persist rewards");
            Check(progress.RecordRound(liveRound, date, () => 0).SequenceEqual(new[] { "Cipher Box", "Cipher Key" }) && progress.Inventory.DayProgress == 1, "failed round can retry drops and first active-day progress");
            string before = File.ReadAllText(path);
            Check(progress.RecordRound(liveRound, date, () => throw new InvalidOperationException("Duplicate reward rerolled.")).Length == 0 && File.ReadAllText(path) == before, "same completed round never rerolls or writes");
            Check(progress.RecordRound(liveRound with { Round = 2 }, date.AddDays(9), () => .9999).SequenceEqual(new[] { "Daybreak Box" }) && progress.Inventory.DayProgress == 0, "second nonconsecutive active day awards one chest");
            before = File.ReadAllText(path);
            using (var guard = new FileStream(path + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                Check(progress.Open("daily", () => 0) is null && progress.Inventory.Daily == 1 && File.ReadAllText(path) == before, "failed debit retains box and grants nothing");
            var opened = progress.Open("daily", () => 0);
            Check(opened is not null && progress.Inventory.Daily == 0 && progress.Inventory.Pending is null && new CosmeticCollection(profile.Storage.GetValueOrDefault("dt.cosmetics.v1")).Owns(opened.Kind, opened.Id), "successful retry durably debits and grants exactly once");
            before = File.ReadAllText(path);
            Check(progress.Open("daily", () => throw new InvalidOperationException("Empty box rerolled.")) is null && File.ReadAllText(path) == before, "empty pool inventory never spends or rerolls");
            var pending = MysteryLoot.PrepareOpen(profile.Storage.GetValueOrDefault(MysteryLoot.StorageKey), profile.Storage.GetValueOrDefault("dt.cosmetics.v1"), "vault", () => 0);
            Check(pending.Changed && profile.UpdateStorage(new Dictionary<string, string> { [MysteryLoot.StorageKey] = pending.Loot }), "interrupted opening persists selected reward before grant");
            before = File.ReadAllText(path); string? walletBefore = profile.Storage.GetValueOrDefault("dt.cosmetics.v1");
            using (var guard = new FileStream(path + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                Check(!progress.Recover() && progress.Inventory.Pending == pending.Reward && File.ReadAllText(path) == before && profile.Storage.GetValueOrDefault("dt.cosmetics.v1") == walletBefore, "failed atomic settlement preserves pending and previous wallet together");
            profile = new NativeProfileStore(directory); progress = new MysteryLootProgress(profile);
            Check(progress.Recover() && progress.Inventory.Pending is null && progress.Inventory.Vault == 0 && progress.Inventory.Keys == 0 && new CosmeticCollection(profile.Storage.GetValueOrDefault("dt.cosmetics.v1")).Owns(pending.Reward!.Kind, pending.Reward.Id), "process restart recovers selected reward without another box or key");
            before = File.ReadAllText(path);
            Check(progress.Recover() && File.ReadAllText(path) == before, "repeated startup settlement makes no further writes");
            Check(profile.UpdateStorage(new Dictionary<string, string> { [MysteryLoot.StorageKey] = "{broken" }), "fixture writes preserved unreadable loot entry"); before = File.ReadAllText(path);
            Check(!progress.Recover() && progress.Error is not null && File.ReadAllText(path) == before, "unreadable loot is reported and profile bytes remain preserved");
        }
        finally
        {
            string resolved = Path.GetFullPath(directory);
            if (!resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(resolved).StartsWith("digitactics-mystery-check-", StringComparison.Ordinal))
                throw new InvalidOperationException("Unsafe mystery test cleanup target.");
            Directory.Delete(resolved, true);
        }
        checks += MysteryLootIntegrationChecks.Run(root);
        Console.WriteLine($"Mystery loot: {checks:N0} original drop, pool, pity, recovery and transaction checks passed."); return checks;
    }
}
