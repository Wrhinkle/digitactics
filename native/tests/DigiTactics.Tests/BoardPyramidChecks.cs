using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Assets;

internal static class BoardPyramidChecks
{
    public static int Run(string root)
    {
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/board-pyramid.json.gz"));using var gzip=new GZipStream(file,CompressionMode.Decompress);using var fixture=JsonDocument.Parse(gzip);
        var options=new JsonSerializerOptions {PropertyNameCaseInsensitive=true};int checks=0;
        void Near(double a,double b){checks++;if(Math.Abs(a-b)>2e-6)throw new InvalidOperationException($"Pyramid mismatch: {a:R} vs {b:R}");}
        void Matrix(Matrix4D a,JsonElement b){double[] values=new double[16];a.CopyTo(values);for(int k=0;k<16;k++)Near(values[k],b[k].GetDouble());}
        var scene=BoardScene.Load(Path.Combine(root,"content/boards/server-pyramid.json.gz"));
        foreach(var scenario in fixture.RootElement.EnumerateArray())
        {
            var state=new BoardPyramidMotion(scenario.GetProperty("initialCombat").GetBoolean());
            var motions=scenario.GetProperty("motions").EnumerateArray().Select(e=>e.Deserialize<BoardMotion>(options)!).ToArray();
            if(motions.Length!=9)throw new InvalidOperationException($"Missing pyramid effects: {motions.Length}.");
            foreach(var frame in scenario.GetProperty("frames").EnumerateArray())
            {
                double time=frame.GetProperty("time").GetDouble();state.Advance(frame.GetProperty("delta").GetDouble(),time,frame.GetProperty("combat").GetBoolean(),frame.GetProperty("stage").GetInt32());
                for(int i=0;i<motions.Length;i++)
                {
                    var m=motions[i];var expected=frame.GetProperty("objects")[i];
                    if(m.Props is {} flames)for(int j=0;j<flames.Length;j++)Matrix(BoardPyramidMotion.Flame(flames[j],time),expected.GetProperty("matrices")[j]);
                    else
                    {
                        Near(state.Opacity(m.Kind,(int)m.Speed,time),expected.GetProperty("opacity").GetDouble());
                        if(m.Kind=="pyramid-beams")Matrix(state.Beam(scene.Objects.Single(o=>o.Motion?.Kind==m.Kind && o.Motion.Speed==m.Speed).Matrix),expected.GetProperty("matrix"));
                        else
                        {
                            var uv=new float[GltfPrimitive.Stride];BoardSceneMotion.ScrollingUv(uv,new float[GltfPrimitive.Stride],time,m.Kind);Near(uv[6],expected.GetProperty("offset")[0].GetDouble());Near(uv[7],expected.GetProperty("offset")[1].GetDouble());
                        }
                    }
                }
            }
        }
        Console.WriteLine($"Pyramid effects: {checks:N0} original beam, flame, stage-band and glyph-scroll comparisons passed.");return checks;
    }
}
