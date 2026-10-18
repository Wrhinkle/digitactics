using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Assets;
using DigiTactics.Presentation;

internal static class BoardKnightsChecks
{
    private static Matrix4D ConfettiMatrix(BoardConfettiPose p)=>new CompanionTransform(new(p.X,p.Y,p.Z),new(p.Rx,0,p.Rz),new(1,1,1)).Matrix;
    public static int Run(string root)
    {
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/board-knights.json.gz"));using var gzip=new GZipStream(file,CompressionMode.Decompress);using var fixture=JsonDocument.Parse(gzip);
        var options=new JsonSerializerOptions {PropertyNameCaseInsensitive=true};int checks=0;
        void Near(double a,double b){checks++;if(Math.Abs(a-b)>3e-6)throw new InvalidOperationException($"Knights mismatch: {a:R} vs {b:R}");}
        foreach(var scenario in fixture.RootElement.EnumerateArray())
        {
            bool initial=scenario.GetProperty("initial").GetBoolean();var state=new BoardCrowdMotion(initial,initial);var confetti=new BoardConfetti(260,initial);
            var motions=scenario.GetProperty("motions").EnumerateArray().Select(e=>e.Deserialize<BoardMotion>(options)!).ToDictionary(m=>m.Kind);
            if(motions.Count!=4)throw new InvalidOperationException("Missing knights effects.");
            foreach(var frame in scenario.GetProperty("frames").EnumerateArray())
            {
                double time=frame.GetProperty("time").GetDouble();bool won=frame.GetProperty("won").GetBoolean();state.Advance(time,frame.GetProperty("combat").GetBoolean(),won);confetti.Advance(frame.GetProperty("delta").GetDouble(),won);
                foreach(var obj in frame.GetProperty("objects").EnumerateArray())
                {
                    string kind=obj.GetProperty("kind").GetString()!;var motion=motions[kind];
                    if(kind=="knights-confetti")
                    {
                        checks++;if(confetti.Visible!=obj.GetProperty("visible").GetBoolean())throw new InvalidOperationException("Confetti visibility mismatch.");
                        if(!confetti.Visible)continue;
                    }
                    if(kind=="knights-flashes")
                    {
                        var vertices=new float[motion.Crowd!.Length*GltfPrimitive.Stride];state.Flashes(vertices,motion.Crowd,time);
                        for(int i=0;i<motion.Crowd.Length;i++)for(int k=0;k<3;k++)Near(vertices[i*GltfPrimitive.Stride+k],obj.GetProperty("positions")[i*3+k].GetDouble());
                    }
                    else foreach(var row in obj.GetProperty("matrices").EnumerateArray())
                    {
                        int index=row.GetProperty("index").GetInt32();var matrix=kind=="knights-confetti"?ConfettiMatrix(confetti.Poses[index]):state.Instance(kind,motion.Crowd![index],time);
                        double[] values=new double[16];matrix.CopyTo(values);for(int k=0;k<16;k++)Near(values[k],row.GetProperty("matrix")[k].GetDouble());
                    }
                }
            }
        }
        Console.WriteLine($"Knights effects: {checks:N0} original crowd, banner, camera-flash and confetti comparisons passed.");return checks;
    }
}
