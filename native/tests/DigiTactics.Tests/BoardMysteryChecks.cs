using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Assets;

internal static class BoardMysteryChecks
{
    public static int Run(string root)
    {
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/board-mystery.json.gz"));using var gzip=new GZipStream(file,CompressionMode.Decompress);using var fixture=JsonDocument.Parse(gzip);
        var options=new JsonSerializerOptions {PropertyNameCaseInsensitive=true};int checks=0;
        void Matrix(Matrix4D a,JsonElement b){double[] values=new double[16];a.CopyTo(values);for(int k=0;k<16;k++){checks++;if(Math.Abs(values[k]-b[k].GetDouble())>1e-6)throw new InvalidOperationException($"Mystery transform mismatch: {values[k]:R} vs {b[k].GetDouble():R}");}}
        if(fixture.RootElement.GetArrayLength()!=10)throw new InvalidOperationException("Missing mystery themes.");
        foreach(var scenario in fixture.RootElement.EnumerateArray())
        {
            var motions=scenario.GetProperty("motions").EnumerateArray().Select(e=>e.Deserialize<BoardMotion>(options)!).ToArray();
            if(motions.Count(m=>m.Kind=="mystery-monument")!=1 || motions.Count(m=>m.Kind=="mystery-satellites")!=1)throw new InvalidOperationException("Missing mystery landmarks.");
            var states=motions.Select(m=>m.Kind=="mystery-satellites"?new BoardMysteryMotion(m.Props!):null).ToArray();
            foreach(var frame in scenario.GetProperty("frames").EnumerateArray())
            {
                double time=frame.GetProperty("time").GetDouble(),delta=frame.GetProperty("delta").GetDouble();
                for(int i=0;i<motions.Length;i++)
                {
                    var m=motions[i];var expected=frame.GetProperty("objects")[i];
                    if(states[i] is {} state){state.Advance(delta);for(int j=0;j<m.Props!.Length;j++)Matrix(state.Satellite(j,(int)m.Speed,time),expected.GetProperty("matrices")[j]);}
                    else Matrix(BoardMysteryMotion.World(m,time),expected.GetProperty("matrix"));
                }
            }
        }
        Console.WriteLine($"Mystery boards: {checks:N0} original landmark, satellite and hierarchy comparisons passed across all ten themes.");return checks;
    }
}
