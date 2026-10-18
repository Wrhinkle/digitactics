using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Assets;

internal static class BoardUnderwaterChecks
{
    public static int Run(string root)
    {
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/board-underwater.json.gz"));using var gzip=new GZipStream(file,CompressionMode.Decompress);using var fixture=JsonDocument.Parse(gzip);
        var options=new JsonSerializerOptions {PropertyNameCaseInsensitive=true};int checks=0;
        void Near(double a,double b) {checks++;if(Math.Abs(a-b)>2e-6)throw new InvalidOperationException($"Underwater mismatch: {a:R} vs {b:R}");}
        foreach(var scenario in fixture.RootElement.EnumerateArray())
        {
            var state=new BoardUnderwaterMotion(scenario.GetProperty("initialCombat").GetBoolean());
            var motions=scenario.GetProperty("motions").EnumerateArray().Select(m=>m.Deserialize<BoardMotion>(options)!).ToDictionary(m=>m.Kind);
            if(motions.Count!=6)throw new InvalidOperationException("Missing underwater source effects.");
            foreach(var frame in scenario.GetProperty("frames").EnumerateArray())
            {
                double time=frame.GetProperty("time").GetDouble();state.Advance(frame.GetProperty("delta").GetDouble(),time,frame.GetProperty("combat").GetBoolean());
                foreach(var obj in frame.GetProperty("objects").EnumerateArray())
                {
                    string kind=obj.GetProperty("kind").GetString()!;
                    if(motions[kind].Underwater is {} items)
                    {
                        for(int i=0;i<items.Length;i++)
                        {
                            double[] values=new double[16];state.Instance(kind,items[i],time).CopyTo(values);
                            for(int j=0;j<16;j++)Near(values[j],obj.GetProperty("matrices")[i][j].GetDouble());
                        }
                    }
                    else
                    {
                        var uv=new float[GltfPrimitive.Stride];BoardUnderwaterMotion.Scroll(uv,new float[GltfPrimitive.Stride],kind,time);
                        Near(uv[6],obj.GetProperty("offset")[0].GetDouble());Near(uv[7],obj.GetProperty("offset")[1].GetDouble());
                    }
                }
            }
        }
        Console.WriteLine($"Underwater effects: {checks:N0} original caustic, shaft, kelp, clam and fish comparisons passed.");return checks;
    }
}
