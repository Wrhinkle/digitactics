using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Assets;

internal static class BoardFreezeChecks
{
    public static int Run(string root)
    {
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/board-freeze.json.gz"));using var gzip=new GZipStream(file,CompressionMode.Decompress);using var fixture=JsonDocument.Parse(gzip);
        var options=new JsonSerializerOptions {PropertyNameCaseInsensitive=true};int checks=0;
        void Near(double a,double b) {checks++;if(Math.Abs(a-b)>3e-6)throw new InvalidOperationException($"Freezeland mismatch: {a:R} vs {b:R}");}
        var initial=fixture.RootElement.GetProperty("initial").EnumerateArray().ToArray();
        if(initial.Length!=5)throw new InvalidOperationException("Missing Freezeland effects.");
        var motions=initial.Select(e=>e.GetProperty("motion").Deserialize<BoardMotion>(options)!).ToArray();
        var buffers=initial.Select(e=>{var positions=e.GetProperty("positions").EnumerateArray().Select(v=>v.GetSingle()).ToArray();var v=new float[positions.Length/3*GltfPrimitive.Stride];for(int i=0;i<positions.Length/3;i++)Array.Copy(positions,i*3,v,i*GltfPrimitive.Stride,3);return v;}).ToArray();
        var state=new BoardFreezeMotion();
        foreach(var frame in fixture.RootElement.GetProperty("frames").EnumerateArray())
        {
            double time=frame.GetProperty("time").GetDouble(),delta=frame.GetProperty("delta").GetDouble();state.Advance(delta,frame.GetProperty("combat").GetBoolean());
            var fog=frame.GetProperty("fog");Near(state.FogNear,fog.GetProperty("near").GetDouble());Near(state.FogFar,fog.GetProperty("far").GetDouble());
            var color=System.Numerics.Vector3.Lerp(BoardLighting.Linear("#24405e"),BoardLighting.Linear("#8aa6c0"),(float)state.Weather);
            Near(color.X,fog.GetProperty("color")[0].GetDouble());Near(color.Y,fog.GetProperty("color")[1].GetDouble());Near(color.Z,fog.GetProperty("color")[2].GetDouble());
            for(int i=0;i<motions.Length;i++)
            {
                var m=motions[i];var expected=frame.GetProperty("objects")[i];var vertices=buffers[i];
                if(m.Props is {} men)
                {
                    for(int j=0;j<men.Length;j++){double[] matrix=new double[16];BoardFreezeMotion.Snowman(men[j],time).CopyTo(matrix);for(int k=0;k<16;k++)Near(matrix[k],expected.GetProperty("matrices")[j][k].GetDouble());}
                    continue;
                }
                if(m.Aurora is {} aurora){BoardFreezeMotion.Aurora(vertices,aurora,time);Near(state.Opacity((int)(aurora.Ph/2)),expected.GetProperty("opacity").GetDouble());}
                else {BoardSceneMotion.Particles(vertices,delta,time,m);state.Snow(vertices,delta);Near(state.SnowSize,expected.GetProperty("size").GetDouble());}
                for(int j=0;j<vertices.Length/GltfPrimitive.Stride;j++)for(int k=0;k<3;k++)Near(vertices[j*GltfPrimitive.Stride+k],expected.GetProperty("positions")[j*3+k].GetDouble());
            }
        }
        Console.WriteLine($"Freezeland: {checks:N0} original aurora, snowman, blizzard and fog comparisons passed.");return checks;
    }
}
