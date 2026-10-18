using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Assets;
using DigiTactics.Presentation;

internal static class BoardGraveyardChecks
{
    public static int Run(string root)
    {
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/board-graveyard.json.gz"));using var gzip=new GZipStream(file,CompressionMode.Decompress);using var fixture=JsonDocument.Parse(gzip);
        var options=new JsonSerializerOptions {PropertyNameCaseInsensitive=true};int checks=0;
        void Near(double a,double b){checks++;if(Math.Abs(a-b)>3e-6)throw new InvalidOperationException($"Graveyard mismatch: {a:R} vs {b:R}");}
        void Matrix(Matrix4D a,JsonElement b){double[] values=new double[16];a.CopyTo(values);for(int k=0;k<16;k++)Near(values[k],b[k].GetDouble());}
        foreach(var scenario in fixture.RootElement.EnumerateArray())
        {
            var c=scenario.GetProperty("config").Deserialize<BoardLightningData>(options)!;
            var lightning=new BoardLightning(c.Spots,c.Flash,c.DelaySeed,c.DelayCalls,c.BoltCalls,c.FirstDelay,c.DelayBase,c.DelayRange,c.StrikeOnCombat);
            lightning.Advance(0,0,scenario.GetProperty("initialCombat").GetBoolean());
            var state=new BoardGraveyardMotion();var ghosts=scenario.GetProperty("ghosts").Deserialize<BoardGhostItem[]>(options)!;
            if(ghosts.Length!=7)throw new InvalidOperationException("Missing graveyard ghosts.");
            foreach(var frame in scenario.GetProperty("frames").EnumerateArray())
            {
                double time=frame.GetProperty("time").GetDouble(),delta=frame.GetProperty("delta").GetDouble();bool combat=frame.GetProperty("combat").GetBoolean();state.Advance(delta,combat);lightning.Advance(delta,time,combat);
                Near(state.FogNear,frame.GetProperty("fog")[0].GetDouble());Near(state.FogFar,frame.GetProperty("fog")[1].GetDouble());Near(lightning.Flash,frame.GetProperty("flash").GetDouble());
                for(int i=0;i<ghosts.Length;i++)Matrix(state.Ghost(ghosts[i],time),frame.GetProperty("matrices")[i]);
                int index=0;
                foreach(var bolt in frame.GetProperty("bolts").EnumerateArray())
                {
                    bool visible=lightning.Visible&&lightning.Active==index++;checks++;if(visible!=bolt.GetProperty("visible").GetBoolean())throw new InvalidOperationException("Graveyard lightning visibility mismatch.");
                    if(visible)Matrix(new CompanionTransform(new(lightning.X,lightning.Y,lightning.Z),new(0,lightning.Yaw,0),new(1,1,1)).Matrix,bolt.GetProperty("matrix"));
                }
            }
        }
        Console.WriteLine($"Graveyard effects: {checks:N0} original ghost, fog and lightning comparisons passed.");return checks;
    }
}
