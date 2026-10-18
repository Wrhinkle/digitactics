using System.Text.Json;
using DigiTactics.Assets;

internal static class RetroParticleChecks
{
    public static int Run(string root)
    {
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/retro-particles.json.gz"));
        using var gzip=new System.IO.Compression.GZipStream(file,System.IO.Compression.CompressionMode.Decompress);
        using var fixture=JsonDocument.Parse(gzip);int checks=0;
        foreach(var row in fixture.RootElement.EnumerateArray())
        {
            string action=row.GetProperty("act").GetString()!;int randomIndex=0;
            var particles=new RetroParticles(action,row.GetProperty("glyph").GetBoolean(),()=>((randomIndex++*73+19)%997)/997d);
            foreach(var step in row.GetProperty("steps").EnumerateArray())
            {
                double seconds=step.GetProperty("seconds").GetDouble();var at=step.GetProperty("companion");
                var motion=RetroEmoteMotion.Evaluate(action,seconds,.7,.37,new(at[0].GetDouble(),at[1].GetDouble(),at[2].GetDouble()),new(0,16,18),new(1,0,0));
                particles.Update(seconds,motion);var expected=step.GetProperty("particles");
                void Check(bool ok,string why){checks++;if(!ok)throw new InvalidOperationException($"Retro particles {action}/{seconds}: {why}");}
                void Near(double a,double e)=>Check(Math.Abs(a-e)<1e-9,$"{a:R} vs {e:R}");
                Check(particles.Active.Count==expected.GetArrayLength(),"particle count");
                for(int i=0;i<particles.Active.Count;i++)
                {
                    var p=particles.Active[i];var e=expected[i];Check(p.Puff==e.GetProperty("puff").GetBoolean(),"type");
                    var position=e.GetProperty("position");Near(p.Position.X,position[0].GetDouble());Near(p.Position.Y,position[1].GetDouble());Near(p.Position.Z,position[2].GetDouble());
                    Near(p.Opacity,e.GetProperty("opacity").GetDouble());Near(p.Scale,e.GetProperty("scale").GetDouble());
                }
            }
        }
        Console.WriteLine($"Retro particles: {checks:N0} original spawning, movement, opacity, size and expiry checks passed.");return checks;
    }
}
