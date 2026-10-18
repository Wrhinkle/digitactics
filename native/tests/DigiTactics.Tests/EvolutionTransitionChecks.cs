using System.Text.Json;
using DigiTactics.Presentation;
internal static class EvolutionTransitionChecks
{
    public static int Run(string root)
    {
        using var data=JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"tests/fixtures/evolution-transition.json")));int count=0;
        foreach(var row in data.RootElement.EnumerateArray())
        {
            var state=new EvolutionTransition();state.Advance(row.GetProperty("time").GetDouble());
            if(state.White!=row.GetProperty("white").GetBoolean() || state.ShowNew!=row.GetProperty("showNew").GetBoolean() || Math.Abs(state.Scale-row.GetProperty("scale").GetDouble())>1e-10 || Math.Abs(state.Yaw-row.GetProperty("yaw").GetDouble())>1e-10)throw new InvalidOperationException("Evolution transition differs from source.");count++;
        }
        var effect=new EvolutionTransition();
        if(!effect.Advance(2.7)||effect.Complete||effect.FlashOpacity!=0)throw new InvalidOperationException("Transformation flash did not start once.");
        if(effect.Advance(.26)||!effect.Complete||!effect.ShowNew||effect.White||effect.Scale!=1||effect.Yaw!=0||effect.FlashOpacity!=1)throw new InvalidOperationException("Transformation did not settle after flash delay.");
        effect.Advance(1);if(effect.FlashOpacity!=0)throw new InvalidOperationException("Transformation flash did not fade.");
        Console.WriteLine($"Evolution transformation: {count} source samples and flash/settlement boundaries passed.");return count+3;
    }
}
