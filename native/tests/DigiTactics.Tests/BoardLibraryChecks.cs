using System.IO.Compression;
using System.Text.Json;
using System.Numerics;
using DigiTactics.Assets;

internal static class BoardLibraryChecks
{
    public static int Run(string root)
    {
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/board-library.json.gz"));using var gzip=new GZipStream(file,CompressionMode.Decompress);using var fixture=JsonDocument.Parse(gzip);
        var options=new JsonSerializerOptions {PropertyNameCaseInsensitive=true};int checks=0;
        void Near(double a,double b){checks++;if(Math.Abs(a-b)>2e-6)throw new InvalidOperationException($"Library mismatch: {a:R} vs {b:R}");}
        void Color(Vector3 a,JsonElement b){Near(a.X,b[0].GetDouble());Near(a.Y,b[1].GetDouble());Near(a.Z,b[2].GetDouble());}
        void Matrix(Matrix4D a,JsonElement b){double[] values=new double[16];a.CopyTo(values);for(int k=0;k<16;k++)Near(values[k],b[k].GetDouble());}
        var scene=BoardScene.Load(Path.Combine(root,"content/boards/witchelny-library.json.gz"));
        foreach(var scenario in fixture.RootElement.EnumerateArray())
        {
            var state=new BoardLibraryMotion(scenario.GetProperty("initialEvolution").GetInt32());
            var motions=scenario.GetProperty("motions").EnumerateArray().Select(e=>e.Deserialize<BoardMotion>(options)!).ToArray();
            if(motions.Length!=8)throw new InvalidOperationException("Missing original library effects.");
            foreach(var frame in scenario.GetProperty("frames").EnumerateArray())
            {
                double time=frame.GetProperty("time").GetDouble();state.Advance(frame.GetProperty("delta").GetDouble(),frame.GetProperty("evolution").GetInt32());
                Near(state.LightFlash,frame.GetProperty("flash").GetDouble());
                Color(Vector3.Lerp(BoardLighting.Linear("#ffe0b0"),BoardLighting.Linear("#ffd090"),state.TintWeight),frame.GetProperty("sunColor"));
                for(int i=0;i<motions.Length;i++)
                {
                    var motion=motions[i];var expected=frame.GetProperty("objects")[i];
                    if(motion.Library is {} items)
                        for(int j=0;j<items.Length;j++)Matrix(BoardLibraryMotion.Instance(motion.Kind,items[j],time),expected.GetProperty("matrices")[j]);
                    else if(motion.LibrarySparks)Color(state.SparkColor,expected.GetProperty("color"));
                    else
                    {
                        var obj=scene.Objects.Single(o=>o.Motion?.Kind==motion.Kind);Matrix(state.Circle(motion.Kind,obj.Matrix),expected.GetProperty("matrix"));
                        if(motion.Kind=="library-circle"){Color(state.CircleColor,expected.GetProperty("color"));Near(state.Opacity(time),expected.GetProperty("opacity").GetDouble());}
                    }
                }
            }
        }
        Console.WriteLine($"Library effects: {checks:N0} original page, candle, circle, spark and evolution-light comparisons passed.");return checks;
    }
}
