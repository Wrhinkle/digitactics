using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Assets;

internal static class BoardCastleChecks
{
    public static int Run(string root)
    {
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/board-castle.json.gz"));
        using var gzip=new GZipStream(file,CompressionMode.Decompress);
        using var fixture=JsonDocument.Parse(gzip);
        var options=new JsonSerializerOptions {PropertyNameCaseInsensitive=true};int checks=0;
        void Near(double actual,double expected) {checks++;if(Math.Abs(actual-expected)>1e-6)throw new InvalidOperationException($"Castle motion mismatch: {actual:R} vs {expected:R}");}
        foreach(var scenario in fixture.RootElement.EnumerateArray())
        {
            var state=new BoardCastleMotion(scenario.GetProperty("initialCombat").GetBoolean());
            var motions=scenario.GetProperty("motions").EnumerateArray().Select(m=>m.Deserialize<BoardMotion>(options)!).ToDictionary(m=>m.Kind);
            foreach(var frame in scenario.GetProperty("frames").EnumerateArray())
            {
                double time=frame.GetProperty("time").GetDouble();
                state.Advance(frame.GetProperty("delta").GetDouble(),time,frame.GetProperty("combat").GetBoolean(),frame.GetProperty("stage").GetInt32());
                foreach(var obj in frame.GetProperty("objects").EnumerateArray())
                {
                    string kind=obj.GetProperty("kind").GetString()!;
                    if(kind=="castle-moon") {var c=state.MoonColor;Near(c.X,obj.GetProperty("color")[0].GetDouble());Near(c.Y,obj.GetProperty("color")[1].GetDouble());Near(c.Z,obj.GetProperty("color")[2].GetDouble());}
                    else if(kind=="castle-halo") Near(state.HaloOpacity(time),obj.GetProperty("opacity").GetDouble());
                    else
                    {
                        var items=motions[kind].Castle!;
                        for(int i=0;i<items.Length;i++)
                        {
                            double[] values=new double[16];state.Instance(kind,items[i],i,time).CopyTo(values);
                            for(int j=0;j<16;j++)Near(values[j],obj.GetProperty("matrices")[i][j].GetDouble());
                        }
                    }
                }
            }
        }
        Console.WriteLine($"Castle effects: {checks:N0} original bat, flame, moon and halo comparisons passed.");return checks;
    }
}
