using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Assets;

internal static class BoardCarnivalChecks
{
    public static int Run(string root)
    {
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/board-carnival.json.gz"));using var gzip=new GZipStream(file,CompressionMode.Decompress);using var fixture=JsonDocument.Parse(gzip);
        var options=new JsonSerializerOptions {PropertyNameCaseInsensitive=true};int checks=0;
        void Matrix(Matrix4D a,JsonElement b){double[] values=new double[16];a.CopyTo(values);for(int k=0;k<16;k++){checks++;if(Math.Abs(values[k]-b[k].GetDouble())>2e-6)throw new InvalidOperationException($"Carnival transform mismatch: {values[k]:R} vs {b[k].GetDouble():R}");}}
        var motions=fixture.RootElement.GetProperty("motions").EnumerateArray().Select(e=>e.Deserialize<BoardMotion>(options)!).ToArray();
        if(motions.Length!=5)throw new InvalidOperationException("Missing carnival ride effects.");
        foreach(var frame in fixture.RootElement.GetProperty("frames").EnumerateArray())
        {
            double time=frame.GetProperty("time").GetDouble();
            for(int i=0;i<motions.Length;i++)
            {
                var m=motions[i];var expected=frame.GetProperty("objects")[i];
                if(m.Kind=="carnival-wheel")Matrix(BoardCarnivalMotion.Wheel(time),expected.GetProperty("matrix"));
                else for(int j=0;j<m.Carnival!.Length;j++)Matrix(BoardCarnivalMotion.Instance(m.Kind,j,m.Carnival[j],time),expected.GetProperty("matrices")[j]);
            }
        }
        Console.WriteLine($"Carnival rides: {checks:N0} original wheel, gondola, bulb, horse and balloon comparisons passed.");return checks;
    }
}
