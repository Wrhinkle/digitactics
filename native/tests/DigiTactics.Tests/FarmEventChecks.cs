using System.Text.Json.Nodes;
using DigiTactics.Storage;
using DigiTactics.Presentation;

internal static class FarmEventChecks
{
    public static int Run()
    {
        int checks = 0;
        void Check(bool value, string message) { checks++; if (!value) throw new InvalidOperationException("Farm events: " + message); }
        string directory = Path.Combine(Path.GetTempPath(), "digitactics-farm-events-" + Guid.NewGuid().ToString("N"));
        try
        {
            long time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var state = JsonNode.Parse(FarmSimulation.Advance(null, null, null, time).Save)!;
            state["pets"]!["tokomon"]!["poopClock"] = 179;
            state["pets"]!["tsunomon"]!["poopClock"] = 179;
            state["pets"]!["tsunomon"]!["disc"] = 65;
            var profile = new NativeProfileStore(directory);
            Check(profile.UpdateStorage(new Dictionary<string, string> { ["dt.farm.v1"] = state.ToJsonString() }), "seed");
            var progress = new FarmProgress(profile) { ResidentPosition = id => id == "tokomon" ? (1.12, 2.34) : null };
            using (var guard = new FileStream(Path.Combine(directory, "profile.json.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                Check(!progress.Refresh(time + 60000), "locked tick fails");
                Check(progress.DrainEvents().Length == 0, "failed tick publishes no events");
            }
            Check(progress.Refresh(time + 60000), "retry tick");
            var events = progress.DrainEvents();
            Check(events.Count(x => x["type"]!.GetValue<string>() == "poop") == 1 && events.Count(x => x["type"]!.GetValue<string>() == "toilet") == 1, "committed tick delivered");
            Check(progress.DrainEvents().Length == 0, "drain once");
            var reopened = new NativeProfileStore(directory);
            var saved = JsonNode.Parse(reopened.Storage["dt.farm.v1"])!;
            var pile = saved["pets"]!["tokomon"]!["poop"]![0]!;
            Check(Math.Abs(pile["x"]!.GetValue<double>() - 1.47) < 1e-9 && Math.Abs(pile["z"]!.GetValue<double>() - 2.54) < 1e-9, "live coordinates survive reload");
            Check(progress.Refresh(time + 60000) && progress.DrainEvents().Length == 0, "same timestamp never replays events");
            // Care can advance simulation too; its events must follow the same commit contract.
            saved["pets"]!["tokomon"]!["poopClock"] = 179;
            Check(profile.UpdateStorage(new Dictionary<string, string> { ["dt.farm.v1"] = saved.ToJsonString() }), "seed next tick");
            progress.ResidentPosition = null;
            var expected = FarmCare.Apply(saved.ToJsonString(), null, null, time + 120000, FarmCareAction.Pet, "tokomon");
            Check(progress.Act(time + 120000, FarmCareAction.Pet, "tokomon"), "care advances simulation");
            Check(progress.DrainEvents().Any(e => e["type"]!.GetValue<string>() == "poop"), "care publishes tick events");
            Check(JsonNode.DeepEquals(JsonNode.Parse(progress.Save!), JsonNode.Parse(expected.Farm)), "away coordinates remain original");
        }
        finally
        {
            string resolved = Path.GetFullPath(directory);
            if (!resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(resolved).StartsWith("digitactics-farm-events-", StringComparison.Ordinal)) throw new InvalidOperationException("Unsafe farm test cleanup.");
            if (Directory.Exists(resolved)) Directory.Delete(resolved, true);
        }
        Console.WriteLine($"Farm events: {checks} commit, retry, positioning, drain and care checks passed.");
        return checks;
    }
}
