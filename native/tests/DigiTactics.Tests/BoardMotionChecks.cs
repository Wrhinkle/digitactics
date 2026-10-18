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
            else
            {
                var expected=row.GetProperty("matrices");
                for(int i=0;i<motion.Items!.Length;i++)
                {
                    double[] values=new double[16]; BoardSceneMotion.Crystal(motion.Items[i],time).CopyTo(values);
                    for(int j=0;j<16;j++) Near(values[j],expected[i][j].GetDouble(),1e-6);
                }
            }
        }
        Console.WriteLine($"Board motion: {checks:N0} original sea vertex and crystal matrix comparisons passed."); return checks;
    }
}
