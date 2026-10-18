using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Assets;

internal static class BoardTrainChecks
{
    public static int Run(string root)
    {
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/board-train.json.gz"));using var gzip=new GZipStream(file,CompressionMode.Decompress);using var fixture=JsonDocument.Parse(gzip);
        var scene=BoardScene.Load(Path.Combine(root,"content/boards/trailmon-station.json.gz"));
        var objects=scene.Objects.Where(o=>o.Motion?.Kind.StartsWith("train-")==true || o.Motion?.TrainSteam==true).ToDictionary(o=>o.Motion!.Kind);
        if(objects.Count!=4)throw new InvalidOperationException("Missing train effects.");int checks=0;
        void Near(double a,double b){checks++;if(Math.Abs(a-b)>2e-5)throw new InvalidOperationException($"Train mismatch: {a:R} vs {b:R}");}
        void Matrix(Matrix4D a,JsonElement b){double[] values=new double[16];a.CopyTo(values);for(int k=0;k<16;k++)Near(values[k],b[k].GetDouble());}
        foreach(var scenario in fixture.RootElement.EnumerateArray())
        {
            var state=new BoardTrainMotion(scenario.GetProperty("initialCombat").GetBoolean());
            foreach(var frame in scenario.GetProperty("frames").EnumerateArray())
            {
                double time=frame.GetProperty("time").GetDouble();state.Advance(time,frame.GetProperty("combat").GetBoolean());
                foreach(var expected in frame.GetProperty("objects").EnumerateArray())
                {
                    string kind=expected.GetProperty("kind").GetString()!;var obj=objects[kind];
                    if(obj.Motion!.TrainSteam){Matrix(state.Steam,expected.GetProperty("matrix"));Near(state.SteamOpacity,expected.GetProperty("opacity").GetDouble());}
                    else if(kind=="train-clock")Matrix(BoardTrainMotion.Clock(obj.Matrix,time),expected.GetProperty("matrix"));
                    else for(int i=0;i<obj.Instances!.Length;i++)Matrix(state.Instance(kind,i,obj.Instances[i].Matrix),expected.GetProperty("matrices")[i]);
                }
            }
        }
        // Strict cooldown and loop boundaries, including the previous-tick event timestamp.
        var boundary=new BoardTrainMotion();boundary.Advance(6.4,false);boundary.Advance(6.4,true);Near(boundary.X,80);
        boundary.Advance(6.401,false);boundary.Advance(6.5,true);Near(boundary.X,-46+.099*18);
        boundary.Advance(22.401,false);Near(boundary.X,242);boundary.Advance(22.402,false);Near(boundary.X,-46);
        Console.WriteLine($"Train effects: {checks:N0} original car, engine, steam, clock and arrival comparisons passed.");return checks;
    }
}
