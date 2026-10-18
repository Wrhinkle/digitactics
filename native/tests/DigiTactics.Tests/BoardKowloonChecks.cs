using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Assets;

internal static class BoardKowloonChecks
{
    public static int Run(string root)
    {
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/board-kowloon.json.gz"));
        using var gzip=new GZipStream(file,CompressionMode.Decompress);using var fixture=JsonDocument.Parse(gzip);
        var options=new JsonSerializerOptions {PropertyNameCaseInsensitive=true};
        var signs=fixture.RootElement.GetProperty("motions").EnumerateArray().Select(m=>m.Deserialize<BoardMotion>(options)!).Single(m=>m.Signs is not null).Signs!;
        var state=new BoardKowloonMotion();int checks=0;
        void Near(double a,double b) {checks++;if(Math.Abs(a-b)>4e-6)throw new InvalidOperationException($"Kowloon mismatch: {a:R} vs {b:R}");}
        foreach(var frame in fixture.RootElement.GetProperty("frames").EnumerateArray())
        {
            double time=frame.GetProperty("time").GetDouble();state.Advance(frame.GetProperty("delta").GetDouble(),frame.GetProperty("combat").GetBoolean());
            Near(state.FogNear,frame.GetProperty("fog")[0].GetDouble());Near(state.FogFar,frame.GetProperty("fog")[1].GetDouble());
            foreach(var obj in frame.GetProperty("objects").EnumerateArray())
            {
                string kind=obj.GetProperty("kind").GetString()!;
                if(kind=="particles") {Near(state.RainOpacity,obj.GetProperty("opacity").GetDouble());Near(state.RainSize,obj.GetProperty("size").GetDouble());}
                else if(kind=="kowloon-signs")
                {
                    for(int i=0;i<signs.Length;i++) {var c=BoardKowloonMotion.Sign(signs[i],time);var expected=obj.GetProperty("colors")[i];Near(c.X,expected[0].GetDouble());Near(c.Y,expected[1].GetDouble());Near(c.Z,expected[2].GetDouble());}
                }
                else {var scan=BoardKowloonMotion.Scan(kind,time);Near(scan.Y,obj.GetProperty("y").GetDouble());Near(scan.Opacity,obj.GetProperty("opacity").GetDouble());}
            }
        }
        Console.WriteLine($"Kowloon effects: {checks:N0} original scan, sign, rain and fog comparisons passed.");return checks;
    }
}
