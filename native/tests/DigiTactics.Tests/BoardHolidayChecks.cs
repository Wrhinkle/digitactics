using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Assets;

internal static class BoardHolidayChecks
{
    public static int Run(string root)
    {
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/board-holiday.json.gz"));using var gzip=new GZipStream(file,CompressionMode.Decompress);using var fixture=JsonDocument.Parse(gzip);
        var options=new JsonSerializerOptions {PropertyNameCaseInsensitive=true};int checks=0;
        void Near(double a,double b){checks++;if(Math.Abs(a-b)>2e-7)throw new InvalidOperationException($"Holiday mismatch: {a:R} vs {b:R}");}
        foreach(var scenario in fixture.RootElement.EnumerateArray())
        {
            var state=new BoardHolidayMotion(scenario.GetProperty("initialWon").GetBoolean());
            var motions=scenario.GetProperty("motions").EnumerateArray().Select(e=>e.Deserialize<BoardMotion>(options)!).ToArray();
            if(motions.Length!=4)throw new InvalidOperationException("Missing holiday effects.");
            foreach(var frame in scenario.GetProperty("frames").EnumerateArray())
            {
                double time=frame.GetProperty("time").GetDouble();state.Advance(time,frame.GetProperty("won").GetBoolean());
                for(int i=0;i<motions.Length;i++)
                {
                    var m=motions[i];var expected=frame.GetProperty("objects")[i];
                    if(m.Phases is {} phases)for(int j=0;j<phases.Length;j++)
                    {
                        var color=state.Color(m.Kind,j,phases[j],time);var e=expected.GetProperty("colors")[j];Near(color.X,e[0].GetDouble());Near(color.Y,e[1].GetDouble());Near(color.Z,e[2].GetDouble());
                    }
                    else if(m.HolidaySnow)Near(state.SnowSize,expected.GetProperty("size").GetDouble());
                    else if(m.HolidaySparkle)Near(state.SparkleOpacity,expected.GetProperty("opacity").GetDouble());
                }
            }
        }
        Console.WriteLine($"Holiday effects: {checks:N0} original tree, string-light, sparkle and snow comparisons passed.");return checks;
    }
}
