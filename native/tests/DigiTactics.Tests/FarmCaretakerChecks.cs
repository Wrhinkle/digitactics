using System.IO.Compression;
using System.Text.Json.Nodes;
using DigiTactics.Storage;
using DigiTactics.Presentation;

internal static class FarmCaretakerChecks
{
    public static int Run(string root)
    {
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/farm-caretaker.json.gz"));
        using var zip = new GZipStream(file, CompressionMode.Decompress);
        var rows = JsonNode.Parse(zip)!.AsArray(); int checks = 0;
        void Check(bool pass, string message) { checks++; if (!pass) throw new InvalidOperationException("Farm caretaker: " + message); }
        void Equal(JsonNode? actual, JsonNode? expected, string path)
        {
            if (actual is JsonObject a && expected is JsonObject b)
            {
                Check(a.Select(x => x.Key).Order().SequenceEqual(b.Select(x => x.Key).Order()), path + " fields");
                foreach (var pair in b) Equal(a[pair.Key], pair.Value, path + "/" + pair.Key);
            }
            else if (actual is JsonArray aa && expected is JsonArray bb)
            {
                Check(aa.Count == bb.Count, path + " length");
                for (int i = 0; i < aa.Count; i++) Equal(aa[i], bb[i], path + "/" + i);
            }
            else if (actual is JsonValue av && expected is JsonValue bv && av.TryGetValue<double>(out var an) && bv.TryGetValue<double>(out var bn))
                Check(Math.Abs(an - bn) < 1e-9, path + " number");
            else Check(JsonNode.DeepEquals(actual, expected), path + " value");
        }
        foreach (var row in rows)
        {
            string farm = row!["input"]!.ToJsonString(), wallet = row["wallet"]!.ToJsonString(), legends = row["legend"]!.ToJsonString();
            long time = row["time"]!.GetValue<long>(); string actual;
            if (row["kind"]!.GetValue<string>() == "hire")
            {
                var action = row["action"] is { } named ? Enum.Parse<FarmCareAction>(named.GetValue<string>()) : FarmCareAction.HireCaretaker;
                var result = FarmCare.Apply(farm, wallet, legends, time, action, row["id"]?.GetValue<string>(), row["plan"]?.GetValue<string>() ?? row["option"]?.GetValue<string>());
                actual = result.Farm;
                Equal(result.Result, row["result"], "hire result"); Equal(JsonNode.Parse(result.Wallet!), row["walletAfter"], "wallet");
                Equal(JsonNode.Parse(result.Legends!), row["legendAfter"], "legends"); Equal(result.Events, row["events"], "hire events");
            }
            else
            {
                var result = FarmSimulation.Advance(farm, wallet, legends, time); actual = result.Save;
                Equal(result.Events, row["events"], "advance events");
            }
            var after = JsonNode.Parse(actual)!; Equal(after, row["expected"], "farm");
            var again = FarmSimulation.Advance(actual, wallet, legends, time);
            Equal(JsonNode.Parse(again.Save), after, "same-time reload"); Check(again.Events.Count == 0, "reloading replayed events");
            if (FarmCaretaker.Current(after) is not null && time <= 1792260300000)
                foreach (var id in after["residents"]!.AsArray().Select(id => id!.GetValue<string>()))
                {
                    var pet = after["pets"]![id]!; var before = row["input"]!["pets"]![id]!;
                    foreach (string key in new[] { "mistakes", "poopNeglect", "goodBond", "goodFood", "str", "spd", "wis", "sta", "trained", "overfeed" })
                        Equal(pet[key], before[key], "preserved " + key);
                    Check(!pet["sick"]!.GetValue<bool>() && pet["poop"]!.AsArray().Count == 0, "upkeep left sickness or waste");
                }
        }
        // A long absence remains safe without paid staff; supply refills still use the full elapsed time.
        var healthy = rows.First(row => row!["kind"]!.GetValue<string>() == "advance" && row["input"]!["caretaker"] is null
            && row["input"]!["lights"]!.GetValue<bool>() && row["input"]!["cap"]!.GetValue<int>() == 6 && row["time"]!.GetValue<long>() == 1792346700000)!;
        var healthySave = healthy["expected"]!;
        Check(healthySave["pets"]!.AsObject().All(p => p.Value!["hunger"]!.GetValue<double>() >= 59.99 && !p.Value["sick"]!.GetValue<bool>()), "unstaffed overnight floor");
        var supplies = healthy["input"]!.DeepClone(); supplies["meat"]!["n"] = 0; supplies["bandage"]!["n"] = 0;
        supplies["meat"]!["t"] = 1792260300000; supplies["bandage"]!["t"] = 1792260300000;
        var restocked = JsonNode.Parse(FarmSimulation.Advance(supplies.ToJsonString(), healthy["wallet"]!.ToJsonString(), legendJson: null, timestamp: 1792346700000).Save)!;
        Check(restocked["meat"]!["n"]!.GetValue<double>() == 3 && restocked["bandage"]!["n"]!.GetValue<double>() == 2, "supply refill was capped with biology");
        foreach (string tier in new[] { "basic", "full" })
        {
            var split = rows.First(row => row!["kind"]!.GetValue<string>() == "advance" && row["input"]!["caretaker"]?["tier"]?.GetValue<string>() == tier
                && row["input"]!["lights"]!.GetValue<bool>() && row["time"]!.GetValue<long>() == 1792350300000)!;
            var save = split["expected"]!; double expectedHunger = tier == "full" ? 90 : 30;
            Check(FarmCaretaker.Current(save) is null && save["pets"]!.AsObject().All(p => Math.Abs(p.Value!["hunger"]!.GetValue<double>() - expectedHunger) < 1e-8), "paid/unpaid interval split at expiry");
        }

        string directory = Path.Combine(Path.GetTempPath(), "digitactics-caretaker-" + Guid.NewGuid().ToString("N"));
        try
        {
            var sample = rows.First(row => row!["kind"]!.GetValue<string>() == "hire" && row["plan"]?.GetValue<string>() == "basic" && row["input"]!["caretaker"] is null && row["wallet"]!["bits"]!.GetValue<int>() == 10000)!;
            var profile = new NativeProfileStore(directory);
            Check(profile.UpdateStorage(new Dictionary<string,string> { ["dt.farm.v1"] = sample["input"]!.ToJsonString(), ["dt.cosmetics.v1"] = sample["wallet"]!.ToJsonString(), ["dt.legendevo.v1"] = sample["legend"]!.ToJsonString(), ["dt.future"] = "preserve" }), "seed");
            var progress = new FarmProgress(profile); long time = sample["time"]!.GetValue<long>();
            string saved = File.ReadAllText(Path.Combine(directory, "profile.json"));
            using (var hold = new FileStream(Path.Combine(directory, "profile.json.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                Check(!progress.Act(time, FarmCareAction.HireCaretaker, option: "basic"), "locked hire succeeded");
                Check(File.ReadAllText(Path.Combine(directory, "profile.json")) == saved && profile.Progress.Bits == 10000, "partial charge after failed save");
            }
            Check(progress.Act(time, FarmCareAction.HireCaretaker, option: "basic") && profile.Progress.Bits == 9900, "basic24h price");
            long expiry = FarmCaretaker.Until(JsonNode.Parse(progress.Save!)!);
            Check(progress.Refresh(time) && profile.Progress.Bits == 9900, "refresh charged again");
            Check(progress.Act(time + FarmCaretaker.Day/2, FarmCareAction.HireCaretaker, option: "full") && profile.Progress.Bits == 9575, "half-day upgrade should cost325");
            Check(FarmCaretaker.Until(JsonNode.Parse(progress.Save!)!) == expiry, "upgrade erased paid time");
            Check(!progress.Act(time, FarmCareAction.HireCaretaker, option: "basic") && profile.Progress.Bits == 9575, "rollback allowed downgrade");
            Check(progress.Act(time, FarmCareAction.HireCaretaker, option: "full") && profile.Progress.Bits == 8825, "explicit renewal750");
            Check(FarmCaretaker.Until(JsonNode.Parse(progress.Save!)!) == expiry + FarmCaretaker.Day, "renewal failed to extend24h");
            string stable = progress.Save!; Check(progress.Refresh(time - FarmCaretaker.Day) && progress.Save == stable && profile.Progress.Bits == 8825, "clock rollback gained time/supplies or charged");
            var reload = new NativeProfileStore(directory); Check(reload.Progress.Bits == 8825 && reload.Storage["dt.future"] == "preserve" && FarmCaretaker.Current(JsonNode.Parse(reload.Storage["dt.farm.v1"])!)?.Id == "full", "restart lost coverage or unrelated data");
            Check(progress.Refresh(expiry + FarmCaretaker.Day + 3*3600000) && profile.Progress.Bits == 8825 && FarmCaretaker.Current(JsonNode.Parse(progress.Save!)!) is null, "expiry automatically renewed or charged");
        }
        finally
        {
            string target = Path.GetFullPath(directory);
            if (!target.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(target).StartsWith("digitactics-caretaker-", StringComparison.Ordinal)) throw new InvalidOperationException("Unsafe test cleanup.");
            if (Directory.Exists(target)) Directory.Delete(target, true);
        }
        Console.WriteLine($"Farm caretaker: {checks} source parity, offline bounds, reload, preserved progress and atomic purchase checks passed."); return checks;
    }
}
