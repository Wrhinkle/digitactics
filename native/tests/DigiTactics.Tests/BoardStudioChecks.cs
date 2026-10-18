using System.IO.Compression;
using System.Text.Json;
using System.Numerics;
using DigiTactics.Assets;
using DigiTactics.Presentation;

internal static class BoardStudioChecks
{
    public static int Run(string root)
    {
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/board-studio.json.gz"));using var gzip=new GZipStream(file,CompressionMode.Decompress);using var fixture=JsonDocument.Parse(gzip);
        var options=new JsonSerializerOptions {PropertyNameCaseInsensitive=true};int checks=0;
        void Near(double a,double b){checks++;if(Math.Abs(a-b)>3e-6)throw new InvalidOperationException($"Studio mismatch: {a:R} vs {b:R}");}
        void Matrix(Matrix4D a,JsonElement b){double[] values=new double[16];a.CopyTo(values);for(int k=0;k<16;k++)Near(values[k],b[k].GetDouble());}
        void Color(Vector3 a,JsonElement b){Near(a.X,b[0].GetDouble());Near(a.Y,b[1].GetDouble());Near(a.Z,b[2].GetDouble());}
        foreach(var scenario in fixture.RootElement.EnumerateArray())
        {
            var state=new BoardStudioMotion();var confetti=new BoardConfetti(220,scenario.GetProperty("initial").GetBoolean());
            var motions=scenario.GetProperty("motions").EnumerateArray().Select(e=>e.GetProperty("motion").Deserialize<BoardMotion>(options)!).ToArray();
            if(motions.Length!=20)throw new InvalidOperationException($"Missing studio effects: {motions.Length}.");
            foreach(var frame in scenario.GetProperty("frames").EnumerateArray())
            {
                double time=frame.GetProperty("time").GetDouble(),delta=frame.GetProperty("delta").GetDouble();state.Advance(delta,frame.GetProperty("combat").GetBoolean());confetti.Advance(delta,frame.GetProperty("won").GetBoolean());
                for(int i=0;i<motions.Length;i++)
                {
                    var motion=motions[i];var obj=frame.GetProperty("objects")[i];string kind=motion.Kind;
                    switch(kind)
                    {
                        case "studio-beam":case "studio-pool":
                            Matrix(state.Light((int)motion.Speed,kind=="studio-beam",time),obj.GetProperty("matrix"));Color(state.LightColor((int)motion.Speed),obj.GetProperty("color"));break;
                        case "studio-crane":Matrix(BoardStudioMotion.Crane(motion,time),obj.GetProperty("matrix"));break;
                        case "studio-strip":Color(BoardStudioMotion.Strip((int)motion.Speed,time),obj.GetProperty("color"));break;
                        case "studio-led":
                            var uv=new float[GltfPrimitive.Stride];BoardSceneMotion.ScrollingUv(uv,new float[GltfPrimitive.Stride],time,kind);Near(uv[6],obj.GetProperty("offset")[0].GetDouble());Near(uv[7],obj.GetProperty("offset")[1].GetDouble());break;
                        case "studio-sticks":
                            Color(state.StickColor,obj.GetProperty("color"));var vertices=new float[motion.Crowd!.Length*GltfPrimitive.Stride];BoardStudioMotion.Sticks(vertices,motion.Crowd,time);
                            for(int j=0;j<motion.Crowd.Length;j++)for(int k=0;k<3;k++)Near(vertices[j*GltfPrimitive.Stride+k],obj.GetProperty("positions")[j*3+k].GetDouble());break;
                        case "studio-audience":
                            foreach(var row in obj.GetProperty("matrices").EnumerateArray())Matrix(BoardStudioMotion.Audience(motion.Crowd![row.GetProperty("index").GetInt32()],time),row.GetProperty("matrix"));break;
                        case "studio-confetti":
                            checks++;if(confetti.Visible!=obj.GetProperty("visible").GetBoolean())throw new InvalidOperationException("Studio confetti visibility mismatch.");
                            if(confetti.Visible)foreach(var row in obj.GetProperty("matrices").EnumerateArray())
                            {var p=confetti.Poses[row.GetProperty("index").GetInt32()];Matrix(new CompanionTransform(new(p.X,p.Y,p.Z),new(p.Rx,0,p.Rz),new(1,1,1)).Matrix,row.GetProperty("matrix"));}break;
                        default:throw new InvalidOperationException("Unknown studio effect.");
                    }
                }
            }
        }
        Console.WriteLine($"Studio effects: {checks:N0} original spotlight, audience, crane, LED, color and confetti comparisons passed.");return checks;
    }
}
