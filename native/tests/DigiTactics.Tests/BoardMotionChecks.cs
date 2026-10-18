using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Assets;

internal static class BoardMotionChecks
{
    public static int Run(string root)
    {
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/board-motion.json.gz"));
        using var gzip=new GZipStream(file,CompressionMode.Decompress);
        using var fixture=JsonDocument.Parse(gzip); int checks=0;
        var options=new JsonSerializerOptions { PropertyNameCaseInsensitive=true };
        void Near(double a,double b,double tolerance) { checks++; if(Math.Abs(a-b)>tolerance) throw new InvalidOperationException($"Board motion mismatch: {a:R} vs {b:R}."); }
        foreach(var row in fixture.RootElement.EnumerateArray())
        {
            double time=row.GetProperty("time").GetDouble(); var motion=row.GetProperty("motion").Deserialize<BoardMotion>(options)!;
            if(motion.Kind=="sea")
            {
                var samples=row.GetProperty("vertices"); var vertices=new float[samples.GetArrayLength()*GltfPrimitive.Stride]; int i=0;
                foreach(var sample in samples.EnumerateArray()) { vertices[i]=sample[0].GetSingle(); vertices[i+2]=sample[1].GetSingle(); i+=GltfPrimitive.Stride; }
                BoardSceneMotion.Sea(vertices,time,motion); i=0;
                foreach(var sample in samples.EnumerateArray()) { Near(vertices[i+1],sample[2].GetSingle(),1e-7); Near(vertices[i],sample[0].GetSingle(),0); Near(vertices[i+2],sample[1].GetSingle(),0); i+=GltfPrimitive.Stride; }
            }
            else if(motion.Kind is "desert-haze" or "grid-floor")
            {
                var samples=row.GetProperty("samples"); var vertices=new float[samples.GetArrayLength()*GltfPrimitive.Stride]; int at=0;
                foreach(var sample in samples.EnumerateArray())
                {
                    vertices[at+6]=sample.GetProperty("source")[0].GetSingle()*(motion.Kind=="grid-floor"?60:8);
                    vertices[at+7]=sample.GetProperty("source")[1].GetSingle()*(motion.Kind=="grid-floor"?60:1); at+=GltfPrimitive.Stride;
                }
                var initial=(float[])vertices.Clone();
                BoardSceneMotion.ScrollingUv(vertices,initial,time,motion.Kind); at=0;
                foreach(var sample in samples.EnumerateArray())
                {
                    Near(vertices[at+6],sample.GetProperty("expected")[0].GetDouble(),motion.Kind=="grid-floor"?1e-5:1e-6);
                    Near(vertices[at+7],sample.GetProperty("expected")[1].GetDouble(),motion.Kind=="grid-floor"?1e-5:1e-7); at+=GltfPrimitive.Stride;
                }
                if(motion.Kind=="desert-haze") Near(BoardSceneMotion.Haze(time).Opacity,row.GetProperty("opacity").GetDouble(),1e-7);
                BoardSceneMotion.ScrollingUv(vertices,initial,0,motion.Kind);
                for(int i=0;i<vertices.Length;i++) Near(vertices[i],initial[i],0);
            }
            else if(motion.Kind is "mountain-stars" or "grid-rings")
                Near(BoardSceneMotion.Opacity(motion.Kind,time)!.Value,row.GetProperty("opacity").GetDouble(),1e-7);
            else if(motion.Kind=="beach-surf")
            {
                var surf=BoardSceneMotion.Surf(motion,time); double[] values=new double[16]; surf.Matrix.CopyTo(values);
                for(int j=0;j<16;j++) Near(values[j],row.GetProperty("matrix")[j].GetDouble(),1e-10);
                Near(surf.Opacity,row.GetProperty("opacity").GetDouble(),1e-7);
            }
            else
            {
                var expected=row.GetProperty("matrices");
                for(int i=0;i<(motion.Props?.Length??(motion.Kind=="beach-palms"?motion.Palms!.Length:motion.Items!.Length));i++)
                {
                    double[] values=new double[16];
                    var matrix=motion.Props is {} props?BoardSceneMotion.Prop(motion.Kind,props[i],time):motion.Kind=="beach-palms"?BoardSceneMotion.Palm(motion.Palms![i],time):BoardSceneMotion.Crystal(motion.Items![i],time);
                    matrix.CopyTo(values);
                    for(int j=0;j<16;j++) Near(values[j],expected[i][j].GetDouble(),1e-6);
                }
            }
        }
        Console.WriteLine($"Board motion: {checks:N0} original wave, crystal, palm, surf and prop comparisons passed."); return checks;
    }
}
