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
        foreach(var name in new[]{"background","rays"})
        {
            var image=DigiTactics.Assets.PngImage.Decode(File.ReadAllBytes(Path.Combine(root,"content/evolution-stage",name+".png")));
            int size=name=="rays"?520:384;
            if(image.Width!=size || image.Height!=size)throw new InvalidDataException("Stage artwork dimensions changed.");
            var alpha=Enumerable.Range(0,size*size).Select(i=>image.Rgba[i*4+3]);
            if(name=="rays" ? alpha.Min()!=0 || alpha.Max()!=56 : alpha.Any(a=>a!=255))throw new InvalidDataException("Stage artwork transparency changed.");
            count++;
        }
        var rays=new EvolutionTransition();rays.Advance(.2);
        if(rays.RayOpacity<=.5 || rays.RayOpacity>=1 || Math.Abs(rays.RayAngle-Math.PI/6)>1e-10)throw new InvalidOperationException("Rays did not follow source ease/rotation.");
        rays.Advance(.2);if(rays.RayOpacity!=1)throw new InvalidOperationException("Rays did not finish fading in.");
        var effect=new EvolutionTransition();
        if(!effect.Advance(2.7)||effect.Complete||effect.FlashOpacity!=0)throw new InvalidOperationException("Transformation flash did not start once.");
        if(effect.Advance(.26)||!effect.Complete||!effect.ShowNew||effect.White||effect.Scale!=1||effect.Yaw!=0||effect.FlashOpacity!=1)throw new InvalidOperationException("Transformation did not settle after flash delay.");
        effect.Advance(1);if(effect.FlashOpacity!=0 || effect.RayOpacity!=0)throw new InvalidOperationException("Transformation flash did not fade.");
        Console.WriteLine($"Evolution transformation: {count-2} source samples, stage artwork and flash/ray/settlement checks passed.");return count+5;
    }
}
