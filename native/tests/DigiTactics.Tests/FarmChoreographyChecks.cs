using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Presentation;

internal static class FarmChoreographyChecks
{
    private sealed class View(string id, bool hasRun, List<string[]> events) : IFarmCompanionView
    {
        public bool HasRun => hasRun;
        public double Yaw { get; set; }
        public bool Moving { get; private set; }
        public void SetMoving(bool moving) { Moving = moving; events.Add([id, "moving", moving ? "true" : "false"]); }
        public void Play(string role) => events.Add([id, "play", role]);
        public void Update(double delta) { }
    }
    public static int Run(string root)
    {
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/farm-choreography.json.gz"));
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int checks = 0;
        foreach (var test in fixture.RootElement.EnumerateArray())
        {
            long seed = test.GetProperty("seed").GetInt64(); var events = new List<string[]>();
            var scene = new FarmChoreography(() => { seed = seed * 16807 % 2147483647; return seed / 2147483647.0; });
            foreach (var initial in test.GetProperty("initial").EnumerateArray())
            {
                string id = initial.GetProperty("id").GetString()!;
                var actor = scene.Add(id, new View(id, initial.GetProperty("hasRun").GetBoolean(), events));
                actor.Needs = JsonSerializer.Deserialize<FarmResidentNeeds>(initial.GetProperty("needs"), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            }
            int frame = 0;
            foreach (var expected in test.GetProperty("frames").EnumerateArray())
            {
                if (frame is 100 or 400) foreach (var actor in scene.Residents.Values) actor.Needs = actor.Needs with { Asleep = frame == 100 };
                events.Clear(); scene.Update(test.GetProperty("delta").GetDouble());
                void Check(bool ok, string field) { checks++; if (!ok) throw new InvalidOperationException($"Farm movement count {test.GetProperty("count")} / delta {test.GetProperty("delta")} / frame {frame}: {field}"); }
                Check(JsonSerializer.Serialize(events) == expected.GetProperty("events").GetRawText(), "animation commands");
                foreach (var value in expected.GetProperty("actors").EnumerateArray())
                {
                    var actor = scene.Residents[value.GetProperty("id").GetString()!];
                    void Near(double number, string field) => Check(Math.Abs(number - value.GetProperty(field).GetDouble()) < 1e-8, actor.Id + "/" + field + " " + number + " vs " + value.GetProperty(field));
                    Near(actor.X,"x"); Near(actor.Z,"z"); Near(actor.TargetX,"targetX"); Near(actor.TargetZ,"targetZ"); Near(actor.Wait,"wait"); Near(actor.Busy,"busy"); Near(actor.Tilt,"tilt"); Near(actor.Height,"height"); Near(actor.Breathe,"breathe"); Near(actor.View.Yaw,"yaw");
                    Check(actor.Mode == value.GetProperty("mode").GetString(), "mode"); Check(actor.GoingToBed == value.GetProperty("goingToBed").GetBoolean(), "bed routing"); Check(actor.BedSlot == value.GetProperty("bedSlot").GetInt32(), "bed slot"); Check(((View)actor.View).Moving == value.GetProperty("moving").GetBoolean(), "moving");
                }
                frame++;
            }
        }
        Console.WriteLine($"Farm choreography: {checks} original movement, social, sleep and animation-command checks passed.");
        return checks;
    }
}
