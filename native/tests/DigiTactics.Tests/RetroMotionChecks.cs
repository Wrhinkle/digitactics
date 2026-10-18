using System.Text.Json;
using DigiTactics.Assets;

internal static class RetroMotionChecks
{
    public static int Run(string root)
    {
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/retro-motion.json.gz"));
        using var gzip=new System.IO.Compression.GZipStream(file,System.IO.Compression.CompressionMode.Decompress);
        using var fixture=JsonDocument.Parse(gzip);int checks=0;
        static Vector3D V(JsonElement v)=>new(v[0].GetDouble(),v[1].GetDouble(),v[2].GetDouble());
        foreach(var row in fixture.RootElement.EnumerateArray())
        {
            var actual=RetroEmoteMotion.Evaluate(row.GetProperty("act").GetString()!,row.GetProperty("t").GetDouble(),.7,.37,V(row.GetProperty("companion")),V(row.GetProperty("camera")),V(row.GetProperty("right")));
            var expected=row.GetProperty("expected");checks++;
            if((actual is null)!=(expected.ValueKind==JsonValueKind.Null))throw new InvalidOperationException("Retro lifetime mismatch.");
            if(actual is null)continue;
            foreach(var (key,pose) in new[]{("model",actual.Model),("holder",actual.Holder)})
            {
                double[] values=[pose.Position.X,pose.Position.Y,pose.Position.Z,pose.Rotation.X,pose.Rotation.Y,pose.Rotation.Z,pose.Scale.X,pose.Scale.Y,pose.Scale.Z];
                for(int i=0;i<values.Length;i++){checks++;if(Math.Abs(values[i]-expected.GetProperty(key)[i].GetDouble())>1e-9)throw new InvalidOperationException($"Retro {row.GetProperty("act")}/{row.GetProperty("t")}/{key}/{i}: {values[i]} vs {expected.GetProperty(key)[i]}");}
            }
        }
        Console.WriteLine($"Retro motion: {checks:N0} original model, placement, pop and lifetime checks passed.");return checks;
    }
}
