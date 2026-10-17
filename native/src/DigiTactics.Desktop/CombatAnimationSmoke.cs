using DigiTactics.Assets;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal static class CombatAnimationSmoke
{
    public static void Run(ContentCatalog content)
    {
        var models = ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/models.json"));
        int poses = 0;
        foreach (var rules in content.Variants)
        {
            CombatEntry[] side = rules.Lines.Take(3).Select((line, i) => new CombatEntry(line.Id, i % 3, i, 4, Items: [])).ToArray();
            var combat = new CombatSimulation(rules, side, side, 74182, setup: new(new(), new()));
            var driver = new CombatModelAnimations(models, Path.Combine(AppContext.BaseDirectory, "assets"), rules.Id, () => .17);
            var choreography = new CombatChoreography(combat, driver);
            choreography.Update(0);
            int ticks = 0;
            while (!combat.Done && ticks++ < 1500)
            {
                combat.Step(); choreography.Update(CombatSimulation.Tick);
                foreach (var view in choreography.Views)
                {
                    var pose = driver.Pose(view.UnitId);
                    if (pose is null) continue;
                    if (pose.World.Any(matrix => !matrix.IsFinite)) throw new InvalidOperationException($"Nonfinite combat animation: {rules.Id}/{view.UnitId}.");
                    poses++;
                }
            }
            if (!combat.Done) throw new InvalidOperationException($"Animation smoke battle did not finish: {rules.Id}.");
            choreography.Update(.1);
        }
        if (poses == 0) throw new InvalidOperationException("No native combat model poses were evaluated.");
        Console.WriteLine($"Native combat animation smoke passed: {content.Variants.Length} battles, {poses} finite model poses.");
    }
}
