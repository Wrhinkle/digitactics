using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Assets;

internal static class BoardCircuitChecks
{
    public static int Run(string root)
    {
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/board-circuit.json.gz"));using var gzip=new GZipStream(file,CompressionMode.Decompress);using var fixture=JsonDocument.Parse(gzip);
        var options=new JsonSerializerOptions {PropertyNameCaseInsensitive=true};int checks=0;
        void Near(double a,double b,double tolerance=1e-6) {checks++;if(Math.Abs(a-b)>tolerance)throw new InvalidOperationException($"Circuit mismatch: {a:R} vs {b:R}");}
        foreach(var scenario in fixture.RootElement.EnumerateArray())
        {
            var state=new BoardCircuitMotion(scenario.GetProperty("initialWon").GetBoolean());
            var pulses=scenario.GetProperty("motions").EnumerateArray().Select(m=>m.Deserialize<BoardMotion>(options)!).Single(m=>m.Pulses is not null).Pulses!;
            var vertices=new float[pulses.Length*GltfPrimitive.Stride];
            foreach(var frame in scenario.GetProperty("frames").EnumerateArray())
            {
                double time=frame.GetProperty("time").GetDouble();state.Advance(frame.GetProperty("delta").GetDouble(),frame.GetProperty("combat").GetBoolean(),frame.GetProperty("won").GetBoolean());
                Near(state.LightFlash,frame.GetProperty("flash").GetDouble(),1e-12);
                foreach(var obj in frame.GetProperty("objects").EnumerateArray())
                {
                    string kind=obj.GetProperty("kind").GetString()!;
                    if(kind=="circuit-pulses")
                    {
                        state.Pulses(vertices,pulses);var expected=obj.GetProperty("positions");
                        for(int i=0;i<pulses.Length;i++)for(int j=0;j<3;j++)Near(vertices[i*GltfPrimitive.Stride+j],expected[i*3+j].GetDouble(),0);
                    }
                    else
                    {
                        Near(state.Offset(kind,time),obj.GetProperty("offset")[1].GetDouble(),1e-12);Near(state.Opacity(kind),obj.GetProperty("opacity").GetDouble());
                        float[] initial=new float[GltfPrimitive.Stride],uv=new float[GltfPrimitive.Stride];initial[7]=.5f;state.Scroll(uv,initial,kind,time);
                        Near(uv[7],.5+obj.GetProperty("offset")[1].GetDouble());
                    }
                }
            }
        }
        Console.WriteLine($"Circuit effects: {checks:N0} original pulse, code scroll and win flash comparisons passed.");return checks;
    }
}
