using System.Text.Json;
using DigiTactics.Presentation;
using DigiTactics.Rules;

internal static class LegacyCombatEffectChecks
{
    public static int Run(string root)
    {
        using var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(root, "tests/fixtures/legacy-combat-effects.json")));
        var fixture = document.RootElement;
        int checks = 0;
        string context = "palette";
        void Check(bool valid, string message)
        {
            checks++;
            if (!valid) throw new InvalidOperationException($"Legacy combat effects {context}: {message}");
        }
        void Near(double actual, double expected, string message)
            => Check(double.IsFinite(actual) && Math.Abs(actual - expected) <= 1e-10 * Math.Max(1, Math.Abs(expected)), $"{message}: {actual:R} vs {expected:R}");
        void Vector(MoveVector value, JsonElement expected, string message)
        {
            Near(value.X, expected[0].GetDouble(), message + " x");
            Near(value.Y, expected[1].GetDouble(), message + " y");
            Near(value.Z, expected[2].GetDouble(), message + " z");
        }
        foreach (var entry in fixture.GetProperty("palette").EnumerateObject())
        {
            var expected = entry.Value.GetProperty("linear");
            var color = LegacyCombatEffects.AttributeColor(entry.Name);
            Near(color.R, expected[0].GetDouble(), entry.Name + " red");
            Near(color.G, expected[1].GetDouble(), entry.Name + " green");
            Near(color.B, expected[2].GetDouble(), entry.Name + " blue");
        }
        foreach (var row in fixture.GetProperty("cases").EnumerateArray())
        {
            string name = row.GetProperty("name").GetString()!;
            context = name + " initial";
            var effects = new MoveEffects();
            var legacy = new LegacyCombatEffects(effects);
            foreach (var cue in row.GetProperty("events").EnumerateArray())
            {
                double time = cue.GetProperty("time").GetDouble();
                switch (cue.GetProperty("kind").GetString())
                {
                    case "beam":
                        legacy.Beam(Point(cue.GetProperty("from")), Point(cue.GetProperty("to")), LegacyCombatEffects.AttributeColor(cue.GetProperty("attribute").GetString()!), time);
                        break;
                    case "burst":
                        legacy.Burst(Point(cue.GetProperty("center")), cue.GetProperty("radius").GetDouble(), cue.GetProperty("team").GetInt32(), time);
                        break;
                    case "shield": legacy.Shield(Point(cue.GetProperty("center")), time); break;
                }
            }
            Snapshot(row.GetProperty("initial"));
            Check(effects.TaskCount == 0, "legacy cues must not schedule wall-clock tasks");
            int frame = 0;
            foreach (var expected in row.GetProperty("frames").EnumerateArray())
            {
                context = name + " frame " + frame++;
                // Independent wall-clock advancement must neither expire nor animate
                // these simulation-clock cues, including paused/repeated timestamps.
                effects.Update(2.5);
                legacy.Update(expected.GetProperty("time").GetDouble());
                Snapshot(expected.GetProperty("meshes"));
            }
            void Snapshot(JsonElement expected)
            {
                var meshes = effects.Meshes.Where(m => m.Visible).ToArray();
                Check(meshes.Length == expected.GetArrayLength(), "visible mesh count");
                Check(legacy.ActiveCount == expected.GetArrayLength(), "active cue count");
                for (int i = 0; i < meshes.Length; i++)
                {
                    var actual = meshes[i]; var reference = expected[i];
                    Check(actual.Kind == reference.GetProperty("kind").GetString(), "mesh kind");
                    Check(actual.RotationOrder == reference.GetProperty("order").GetString(), "Euler rotation order");
                    Vector(actual.Position, reference.GetProperty("position"), "position");
                    Vector(actual.Scale, reference.GetProperty("scale"), "scale");
                    Vector(actual.Rotation, reference.GetProperty("rotation"), "rotation");
                    var color = reference.GetProperty("color");
                    Near(actual.Color.R, color[0].GetDouble(), "red");
                    Near(actual.Color.G, color[1].GetDouble(), "green");
                    Near(actual.Color.B, color[2].GetDouble(), "blue");
                    Near(actual.Opacity, reference.GetProperty("opacity").GetDouble(), "opacity");
                    Check(!actual.Billboard && actual.LookTarget is null, "source Euler orientation remains authoritative");
                }
            }
        }

        context = "pooled lifecycle";
        var pooled = new MoveEffects(); var controller = new LegacyCombatEffects(pooled);
        for (int i = 0; i < 250; i++)
        {
            double time = i * 2;
            controller.Beam(new(0, 0, 0), new(1, 0, 1), LegacyCombatEffects.AttributeColor("Free"), time);
            controller.Burst(new(0, 0, 0), 2, 0, time);
            controller.Shield(new(0, 0, 0), time);
            controller.Update(time + 1);
            Check(controller.ActiveCount == 0 && pooled.Meshes.All(m => !m.Visible), "expired cues return to pool");
        }
        Check(pooled.Meshes.Count == 3, "expired cues reuse one mesh per kind");
        var signature = pooled.Mesh("sphere");
        pooled.Run(10, 1, _ => { });
        controller.Shield(new(0, 0, 0), 0);
        controller.Clear();
        Check(controller.ActiveCount == 0 && signature.Visible && pooled.TaskCount == 1, "legacy clear preserves signature work");
        Check(pooled.Meshes.Where(m => m.Kind.StartsWith("legacy", StringComparison.Ordinal)).All(m => !m.Visible), "clear hides all legacy meshes");

        context = "exported source geometry";
        using var geometry = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(root, "content/move-effects/legacy-geometry.json")));
        foreach (var entry in fixture.GetProperty("geometry").EnumerateObject())
        {
            var exported = geometry.RootElement.GetProperty(entry.Name);
            var vertices = exported.GetProperty("vertices"); var indices = exported.GetProperty("indices");
            int count = entry.Value.GetProperty("vertices").GetInt32();
            Check(vertices.GetArrayLength() == count * 16, entry.Name + " source vertex count and stride");
            Check(indices.GetArrayLength() == entry.Value.GetProperty("indices").GetInt32(), entry.Name + " source triangle index count");
            foreach (var vertex in vertices.EnumerateArray()) Check(double.IsFinite(vertex.GetDouble()), entry.Name + " finite vertex");
            foreach (var index in indices.EnumerateArray()) Check(index.GetInt32() >= 0 && index.GetInt32() < count, entry.Name + " valid triangle index");
        }
        checks += IntegrationChecks(root);
        Console.WriteLine($"Legacy combat effects: {checks:N0} source and dispatch checks across 13 timing cases passed (equipment cues, ultimate suppression, mirroring, pause/rewind and pooled lifetime).");
        return checks;
    }

    private static WorldPosition Point(JsonElement point) => new(point[0].GetDouble(), point[1].GetDouble(), point[2].GetDouble());

    private static int IntegrationChecks(string root)
    {
        var catalog = ContentCatalog.Load(Path.Combine(root, "content/catalog.json"));
        var rules = catalog.Variants[0]; var line = rules.Lines[0];
        var library = new MoveLibrary(Path.Combine(root, "content/move-effects/specs.json"));
        int checks = 0;
        string context = "dispatch";
        void Check(bool valid, string message)
        {
            checks++;
            if (!valid) throw new InvalidOperationException($"Legacy combat integration {context}: {message}");
        }
        void Near(double actual, double expected, string message)
            => Check(Math.Abs(actual - expected) < 1e-10, $"{message}: {actual:R} vs {expected:R}");
        CombatSimulation Battle() => new(rules, [new(line.Id, 0, 0, 7)], [new(line.Id, 0, 0, 7)], 97121);
        static string State(CombatSimulation battle) => JsonSerializer.Serialize(new
        {
            battle.Time, battle.Done, battle.Winner,
            Units = battle.Units.Select(u => new
            {
                u.Id, u.Stage, u.Alive, u.Cell, u.MoveOrigin, u.MoveElapsed, u.MoveDuration,
                u.Health, u.MaxHealth, u.Mana, u.Shield, u.AttackDamage, u.AbilityPower,
                u.AttackCooldown, u.MoveCooldown, u.CastLock, u.Stun, u.Dealt, u.Taken,
                u.Healed, u.Attacks, Target = u.Target?.Id
            })
        });
        static MoveMesh[] Legacy(MovePlayback playback) => playback.Effects.Meshes.Where(m => m.Visible && m.Kind.StartsWith("legacy", StringComparison.Ordinal)).ToArray();
        var center = new HexCell(1, 2);
        foreach (bool mirror in new[] { false, true })
        {
            context = mirror ? "mirrored equipment" : "equipment";
            var battle = Battle(); var control = Battle();
            var source = battle.Units[0]; var target = battle.Units[1];
            var cue = battle.Hooks.Context(CombatHookSource.Mechanic, "legacy-dispatch-test");
            var controlCue = control.Hooks.Context(CombatHookSource.Mechanic, "legacy-control-test");
            cue.Effect("beam", source, target); cue.Burst(center, 2, source.Team); cue.Effect("shield", source);
            string state = State(battle); var events = battle.Events.ToArray();
            var playback = new MovePlayback(library, rules.Id);
            var dispatch = new CombatMovePlayback(battle, playback, _ => null);
            dispatch.Consume(mirror);
            var meshes = Legacy(playback);
            Check(dispatch.EquipmentCues == 3 && dispatch.Casts == 0 && dispatch.Basics == 0, "three non-ultimate event types dispatch exactly once");
            Check(meshes.Length == 3 && playback.Effects.TaskCount == 0, "equipment events use simulation-clock meshes without signature tasks");
            Check(meshes.Select(m => m.Kind).SequenceEqual(new[] { "legacyBeam", "legacyBurst", "legacyShield" }), "source event order survives dispatch");
            var from = CombatLayout.Unit(source, battle.Columns, mirror); var to = CombatLayout.Unit(target, battle.Columns, mirror);
            Near(meshes[0].Position.X, (from.X + to.X) / 2, "beam midpoint x");
            Near(meshes[0].Position.Z, (from.Z + to.Z) / 2, "beam midpoint z");
            Near(meshes[0].Scale.Y, Math.Sqrt(Math.Pow(to.X - from.X, 2) + Math.Pow(to.Z - from.Z, 2)), "beam endpoint distance");
            Check(meshes[0].Color == LegacyCombatEffects.AttributeColor(source.Line.Attribute), "beam uses the firing unit's attribute");
            var burst = CombatLayout.Cell(center, battle.Columns, mirror);
            Near(meshes[1].Position.X, burst.X, "burst source or mirrored x");
            Near(meshes[1].Position.Z, burst.Z, "burst source or mirrored z");
            Check(meshes[1].Color == MoveColor.Hex(mirror ? "#ff7a6a" : "#6fd3ff"), "burst color follows viewer-relative team");
            Near(meshes[2].Position.X, from.X, "shield recipient x");
            Near(meshes[2].Position.Z, from.Z, "shield recipient z");
            Near(meshes[0].Scale.X, 3.5, "consume applies initial combat-time beam update");
            Near(meshes[1].Scale.X, .4, "consume applies initial combat-time burst update");
            Near(meshes[2].Opacity, .45, "consume applies initial combat-time shield update");
            for (int i = 0; i < 4; i++) { playback.Update(1); dispatch.Consume(mirror); }
            Check(dispatch.EquipmentCues == 3 && Legacy(playback).SequenceEqual(meshes), "repeated consumes do not recreate cues");
            Near(meshes[0].Opacity, 1, "paused combat clock freezes beam despite elapsed visual time");
            Near(meshes[1].Opacity, 1, "paused combat clock freezes burst");
            Near(meshes[2].Opacity, .45, "paused combat clock freezes shield");
            Check(state == State(battle) && events.SequenceEqual(battle.Events), "presentation does not mutate authoritative state or event log");
            for (int i = 0; i < 8; i++) Near(cue.Random(), controlCue.Random(), "presentation does not consume simulation RNG");
            dispatch.Clear(); dispatch.Consume(mirror);
            Check(Legacy(playback).Length == 0 && playback.Effects.TaskCount == 0, "clear removes cues and does not replay the consumed log");
            cue.Effect("shield", target); dispatch.Consume(mirror);
            Check(dispatch.EquipmentCues == 4 && Legacy(playback) is [{ Kind: "legacyShield" }], "new post-clear events remain consumable");
            for (int i = 0; i < 23; i++) battle.Step();
            dispatch.Consume(mirror);
            Check(Legacy(playback).Length == 0, "consume expires old cues as combat advances without replacement events");
        }

        context = "ultimate suppression";
        {
            var battle = Battle(); var source = battle.Units[0]; var other = battle.Units[1];
            var cue = battle.Hooks.Context(CombatHookSource.Mechanic, "legacy-ultimate-test");
            var playback = new MovePlayback(library, rules.Id);
            var dispatch = new CombatMovePlayback(battle, playback, _ => null);
            cue.Effect("cast", source);
            cue.Effect("beam", source, other); cue.Burst(center, 2, source.Team); cue.Effect("shield", source);
            dispatch.Consume(false);
            Check(dispatch.Casts == 1 && dispatch.EquipmentCues == 0 && Legacy(playback).Length == 0, "cast-owned same-time beam, burst and shield cues are suppressed");
            Check(playback.Effects.TaskCount > 0, "suppressed cues still contribute to the signature move composition");
            int tasks = playback.Effects.TaskCount;
            dispatch.Consume(false);
            Check(dispatch.Casts == 1 && playback.Effects.TaskCount == tasks, "repeated consume does not schedule the ultimate twice");
            cue.Effect("beam", other, source); cue.Effect("shield", other);
            dispatch.Consume(false);
            Check(dispatch.EquipmentCues == 2 && Legacy(playback).Select(m => m.Kind).SequenceEqual(new[] { "legacyBeam", "legacyShield" }), "another unit's same-time equipment events remain visible");
            battle.Step();
            cue.Effect("beam", source, other); cue.Burst(center, 2, source.Team); cue.Effect("shield", source);
            dispatch.Consume(false);
            Check(dispatch.EquipmentCues == 5, "later-tick equipment cues are not suppressed by stale cast context");
            dispatch.Clear(); dispatch.Consume(false);
            Check(Legacy(playback).Length == 0 && playback.Effects.TaskCount == 0 && playback.Effects.Meshes.All(m => !m.Visible), "clear removes legacy and signature work together");
        }
        return checks;
    }
}
