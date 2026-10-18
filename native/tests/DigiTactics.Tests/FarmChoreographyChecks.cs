using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Presentation;
using DigiTactics.Assets;
using DigiTactics.Storage;
using System.Text.Json.Nodes;

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
        checks += CareTransitions();
        checks += RoomMotion(root);
        checks += Snow(root);
        checks += Lighting(root);
        Console.WriteLine($"Farm choreography: {checks} movement, social, sleep, care and animation-command checks passed.");
        return checks;
    }
    private static int RoomMotion(string root)
    {
        int checks = 0;
        foreach (string theme in new[] { "meadow", "tropical", "snowy", "sewer", "toy", "grid" })
        {
            using var file = File.OpenRead(Path.Combine(root, "content/farm-rooms", theme + ".json.gz"));
            using var gzip = new GZipStream(file, CompressionMode.Decompress);
            using var doc = JsonDocument.Parse(gzip);
            var meshes = doc.RootElement.GetProperty("meshes");
            foreach (var sample in doc.RootElement.GetProperty("motionChecks").EnumerateArray())
            foreach (var expected in sample.GetProperty("meshes").EnumerateArray())
            {
                int index = expected.GetProperty("index").GetInt32();
                var motion = JsonSerializer.Deserialize<FarmRoomMotion>(meshes[index].GetProperty("motion"), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                double[] actual = new double[16]; motion.Evaluate(sample.GetProperty("time").GetDouble()).CopyTo(actual);
                int component = 0;
                foreach (var value in expected.GetProperty("matrix").EnumerateArray())
                {
                    checks++;
                    if (Math.Abs(actual[component++] - value.GetDouble()) > 1e-9) throw new InvalidOperationException($"Farm room motion {theme}/{index} at {sample.GetProperty("time")}: matrix component {component}");
                }
            }
        }
        Console.WriteLine($"Farm room animation: {checks} original world-transform comparisons passed.");
        return checks;
    }
    private static int Snow(string root)
    {
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/farm-snow.json.gz"));
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var doc = JsonDocument.Parse(gzip);
        var snow = doc.RootElement.GetProperty("initial").EnumerateArray().Select(x => new FarmSnow(x.EnumerateArray().Select(v => v.GetSingle()).ToArray())).ToArray();
        int checks = 0, frame = 0;
        foreach (var sample in doc.RootElement.GetProperty("frames").EnumerateArray())
        {
            for (int i = 0; i < snow.Length; i++)
            {
                snow[i].Update(sample.GetProperty("delta").GetDouble(), sample.GetProperty("time").GetDouble());
                int component = 0;
                foreach (var expected in sample.GetProperty("positions")[i].EnumerateArray())
                {
                    checks++;
                    if (snow[i].Positions[component++] != expected.GetSingle()) throw new InvalidOperationException($"Snow drift mismatch frame {frame}, component {component}");
                }
            }
            frame++;
        }
        Console.WriteLine($"Farm snow: {checks} original float32 particle comparisons passed.");
        return checks;
    }
    private static int Lighting(string root)
    {
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/farm-lighting.json.gz"));
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var doc = JsonDocument.Parse(gzip);
        var light = new FarmLighting(); int checks = 0;
        foreach (var frame in doc.RootElement.EnumerateArray())
        {
            light.Night = frame.GetProperty("night").GetBoolean();
            light.Update(frame.GetProperty("delta").GetDouble(), frame.GetProperty("theme").GetString()!);
            foreach (var (name, value) in new[] { ("hemisphere", light.Hemisphere), ("sun", light.Sun), ("lamp", light.Lamp), ("bulb", light.BulbEmission) })
            {
                checks++;
                if (Math.Abs(value - frame.GetProperty(name).GetDouble()) > 1e-12) throw new InvalidOperationException("Farm lighting transition: " + name);
            }
        }
        Console.WriteLine($"Farm lighting: {checks} original transition comparisons passed."); return checks;
    }
    private static int CareTransitions()
    {
        int checks = 0;
        void Check(bool condition, string name) { checks++; if (!condition) throw new InvalidOperationException("Farm care choreography: " + name); }
        var events = new List<string[]>(); var scene = new FarmChoreography(() => .5);
        var a = scene.Add("a", new View("a", true, events), -2, 0);
        var b = scene.Add("b", new View("b", true, events), 2, 0);
        FarmChoreography.WalkTo(a, 1, 1); scene.Update(.1); scene.Feed("a");
        double x = a.X, z = a.Z; scene.Update(.5);
        Check(a.X == x && a.Z == z && a.Mode == "idle" && a.Busy > 1, "feeding holds position during eating");
        Check(events.Any(e => e.SequenceEqual(new[] { "a", "play", "eat" })), "eat animation");
        a.Busy = 2; a.Needs = a.Needs with { Tantrum = true }; scene.Pet("a");
        Check(a.Busy == 2 && events.Last()[2] == "taunt", "petting retains longer busy period and tantrum reaction");
        Check(Math.Abs(a.View.Yaw - Math.Atan2(-a.X, 8 - a.Z)) < 1e-10, "pet faces camera");
        a.Needs = a.Needs with { Asleep = true }; int count = events.Count; scene.Pet("a");
        Check(events.Count == count, "sleeping pet remains undisturbed");
        a.Needs = a.Needs with { Asleep = false, Tantrum = false }; a.Busy = 0;
        scene.Play("a", "b");
        double dx = b.TargetX - a.TargetX, dz = b.TargetZ - a.TargetZ;
        Check(a.Mode == "walk" && b.Mode == "walk" && Math.Abs(Math.Sqrt(dx * dx + dz * dz) - .8) < 1e-10, "pair approaches opposite sides of midpoint");
        for (int i = 0; i < 100 && a.Busy <= 0; i++) scene.Update(.05);
        Check(a.Busy > 0 && b.Busy > 0 && a.Mode == "idle" && b.Mode == "idle", "arrival starts both play animations");
        scene.Play("a", null); Check(a.Busy == 2.4 && events.Last()[2] == "dance", "solo play");
        a.Busy = 0; scene.Remove("b"); scene.StartTraining("a");
        Check(a.TargetX == 3 && a.TargetZ == 1.8 && a.Mode == "walk", "training station destination");
        for (int i = 0; i < 200 && a.Mode == "walk"; i++) scene.Update(.05);
        Check(a.Mode == "idle" && a.View.Yaw == .6, "training arrival faces equipment");
        scene.FinishTraining("a", .6); Check(a.Busy == 1.5 && events.Last()[2] == "happy", "successful training reaction");
        scene.FinishTraining("a", .59); Check(events.Last()[2] == "sad", "low score training reaction");
        var pairScene = new FarmChoreography(() => .5);
        pairScene.Add("center",new View("center",true,events),0,0);
        var left = pairScene.Add("left",new View("left",true,events),-1,0);
        var right = pairScene.Add("right",new View("right",true,events),1,0);
        Check(pairScene.NearestPlaymate("center") == "right", "equal-distance playmate uses original last-resident tie rule");
        right.Needs = right.Needs with { Asleep = true };
        Check(pairScene.NearestPlaymate("center") == "left", "sleeping playmate excluded");
        left.Needs = left.Needs with { Sick = true };
        Check(pairScene.NearestPlaymate("center") is null, "solo play when all others unavailable");
        right.Needs = right.Needs with { Asleep = false }; right.Busy = 2;
        Check(pairScene.NearestPlaymate("center") == "right", "manual play permits busy healthy partner");
        foreach (var (action, receipt, sound) in new (FarmCareAction, string, string?)[] {
            (FarmCareAction.Feed,"{\"ok\":true}","pick"),
            (FarmCareAction.Feed,"{\"ok\":false,\"refused\":true}","error"),
            (FarmCareAction.Feed,"{\"ok\":false}",null),
            (FarmCareAction.Clean,"{\"ok\":true}","sell"),
            (FarmCareAction.Cure,"{\"ok\":true}","heal"),
            (FarmCareAction.Lights,"{\"ok\":true,\"lights\":true}","equip"),
            (FarmCareAction.Lights,"{\"ok\":true,\"lights\":false}","place"),
            (FarmCareAction.Scold,"{\"ok\":true,\"unfair\":true}","error"),
            (FarmCareAction.Scold,"{\"ok\":true}","place"),
            (FarmCareAction.Play,"{\"ok\":true}","emote"),
            (FarmCareAction.Play,"{\"ok\":false}",null),
            (FarmCareAction.Pet,"{\"ok\":false}","emote")
        }) Check(FarmFeedback.Sound(action, JsonNode.Parse(receipt)!.AsObject()) == sound, "care sound " + action + receipt);
        return checks;
    }
}
