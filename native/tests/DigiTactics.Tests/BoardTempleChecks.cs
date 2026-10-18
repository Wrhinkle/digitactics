using System.IO.Compression;
using System.Text.Json;
using System.Numerics;
using DigiTactics.Assets;
using DigiTactics.Rules;

internal static class BoardTempleChecks
{
    public static int Run(string root)
    {
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/board-temple.json.gz"));using var gzip=new GZipStream(file,CompressionMode.Decompress);using var fixture=JsonDocument.Parse(gzip);
        var options=new JsonSerializerOptions {PropertyNameCaseInsensitive=true};int checks=0;string context="";
        void Near(double a,double b,double tolerance=2e-6) {checks++;if(Math.Abs(a-b)>tolerance)throw new InvalidOperationException($"Temple mismatch {context}: {a:R} vs {b:R}");}
        foreach(var scenario in fixture.RootElement.EnumerateArray())
        {
            var motions=scenario.GetProperty("motions").EnumerateArray().Select(m=>m.Deserialize<BoardMotion>(options)!).ToDictionary(m=>m.Kind);
            var rng=new DeterministicRandom(733);var state=new BoardTempleMotion(motions["temple-geysers"].Temple!,scenario.GetProperty("initialEvolution").GetInt32(),rng.Next);
            var sun=scenario.GetProperty("sun");var baseColor=BoardLighting.Linear(sun[0].GetString()!);
            foreach(var frame in scenario.GetProperty("frames").EnumerateArray())
            {
                double time=frame.GetProperty("time").GetDouble();state.Advance(frame.GetProperty("delta").GetDouble(),time,frame.GetProperty("combat").GetBoolean(),frame.GetProperty("stage").GetInt32(),frame.GetProperty("evolution").GetInt32());
                Near(state.LightFlash,frame.GetProperty("flash").GetDouble());Near(state.SunMultiplier*sun[1].GetDouble(),frame.GetProperty("sun").GetDouble());
                var tint=Vector3.Lerp(baseColor,BoardLighting.Linear("#fff6e0"),state.TintWeight);var expectedTint=frame.GetProperty("sunColor");Near(tint.X,expectedTint[0].GetDouble());Near(tint.Y,expectedTint[1].GetDouble());Near(tint.Z,expectedTint[2].GetDouble());
                foreach(var obj in frame.GetProperty("objects").EnumerateArray())
                {
                    string kind=obj.GetProperty("kind").GetString()!;
                    if(kind=="particles") {Near(state.EmberSize,obj.GetProperty("size").GetDouble());continue;}
                    if(kind=="temple-lava")
                    {
                        for(int j=0;j<3;j++)Near(state.LavaColor,obj.GetProperty("color")[j].GetDouble());
                        var uv=new float[GltfPrimitive.Stride];BoardTempleMotion.Scroll(uv,new float[GltfPrimitive.Stride],time);
                        Near(uv[6],obj.GetProperty("offset")[0].GetDouble());Near(uv[7],obj.GetProperty("offset")[1].GetDouble());continue;
                    }
                    var items=motions[kind].Temple!;
                    for(int i=0;i<items.Length;i++)
                    {
                        var matrix=kind=="temple-geysers"?state.Geysers[i]:state.Flame(items[i],kind=="temple-inner",time);double[] values=new double[16];matrix.CopyTo(values);
                        for(int j=0;j<16;j++) {context=$"{kind} time={time:R} instance={i} element={j}";Near(values[j],obj.GetProperty("matrices")[i][j].GetDouble());}
                    }
                }
            }
        }
        Console.WriteLine($"Temple effects: {checks:N0} original flame, geyser, lava, ember and evolution-light comparisons passed.");return checks;
    }
}
