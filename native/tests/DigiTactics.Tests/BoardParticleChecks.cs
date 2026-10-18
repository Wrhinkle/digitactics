using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Assets;

internal static class BoardParticleChecks
{
    public static int Run(string root)
    {
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/board-particles.json.gz"));
        using var gzip=new GZipStream(file,CompressionMode.Decompress);
        using var fixture=JsonDocument.Parse(gzip); int checks=0,objects=0;
        var options=new JsonSerializerOptions { PropertyNameCaseInsensitive=true };
        foreach(var row in fixture.RootElement.EnumerateArray()) foreach(var obj in row.GetProperty("objects").EnumerateArray())
        {
            objects++; var motion=obj.GetProperty("motion").Deserialize<BoardMotion>(options)!;
            var initial=obj.GetProperty("initial").EnumerateArray().Select(v=>v.GetSingle()).ToArray();
            var vertices=new float[initial.Length/3*GltfPrimitive.Stride];
            for(int i=0;i<initial.Length/3;i++) Array.Copy(initial,i*3,vertices,i*GltfPrimitive.Stride,3);
            double time=0; int frame=0;
            foreach(var step in obj.GetProperty("steps").EnumerateArray())
            {
                int target=step.GetProperty("frame").GetInt32();
                while(frame<=target)
                {
                    double delta=(frame%6) switch {0=>1/60d,1=>.033,2=>0,_=>.1}; time+=delta;
                    BoardSceneMotion.Particles(vertices,delta,time,motion); frame++;
                }
                var expected=step.GetProperty("positions");
                for(int i=0;i<initial.Length/3;i++) for(int axis=0;axis<3;axis++)
                {
                    checks++; double error=Math.Abs(vertices[i*GltfPrimitive.Stride+axis]-expected[i*3+axis].GetSingle());
                    if(error>1e-5) throw new InvalidOperationException($"Board particles {row.GetProperty("id").GetString()}, frame {target}, particle {i}/{axis}: {error:R}.");
                }
            }
        }
        Console.WriteLine($"Board particles: {checks:N0} original position checks passed across {objects} particle systems, including wrapping and zero/delayed frames.");
        return checks;
    }
}
