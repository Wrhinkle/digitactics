using System.IO.Compression;
using System.Text.Json;
using System.Numerics;
using DigiTactics.Assets;
using DigiTactics.Presentation;

internal static class BoardFireworkChecks
{
    public static int Run(string root)
    {
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/board-fireworks.json.gz"));using var gzip=new GZipStream(file,CompressionMode.Decompress);using var fixture=JsonDocument.Parse(gzip);
        var options=new JsonSerializerOptions {PropertyNameCaseInsensitive=true};int checks=0;
        void Near(double a,double b){checks++;if(Math.Abs(a-b)>2e-6)throw new InvalidOperationException($"Firework mismatch: {a:R} vs {b:R}");}
        foreach(var scenario in fixture.RootElement.EnumerateArray())
        {
            var c=scenario.GetProperty("config").Deserialize<BoardFireworkData>(options)!;
            if(c.Max!=6 || c.Count!=70)throw new InvalidOperationException("Missing original carnival fireworks.");
            var state=new BoardFireworks(c.Count,c.Max,c.Colors.Length,c.Seed,c.Calls,scenario.GetProperty("initialWon").GetBoolean());var colors=c.Colors.Select(BoardLighting.Linear).ToArray();
            foreach(var frame in scenario.GetProperty("frames").EnumerateArray())
            {
                state.Advance(frame.GetProperty("delta").GetDouble(),frame.GetProperty("time").GetDouble(),frame.GetProperty("won").GetBoolean());
                for(int i=0;i<c.Max;i++)
                {
                    var b=state.Bursts[i];var e=frame.GetProperty("bursts")[i];checks++;if(b.Visible!=e.GetProperty("visible").GetBoolean())throw new InvalidOperationException("Firework visibility mismatch.");
                    Near(b.Opacity,e.GetProperty("opacity").GetDouble());var color=b.ColorIndex<0?Vector3.One:colors[b.ColorIndex];Near(color.X,e.GetProperty("color")[0].GetDouble());Near(color.Y,e.GetProperty("color")[1].GetDouble());Near(color.Z,e.GetProperty("color")[2].GetDouble());
                    for(int k=0;k<b.Positions.Length;k++)Near(b.Positions[k],e.GetProperty("positions")[k].GetDouble());
                }
            }
        }
        Console.WriteLine($"Fireworks: {checks:N0} original scheduled/victory burst, color, trajectory and expiry comparisons passed.");return checks;
    }
}
