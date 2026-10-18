using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Assets;

internal static class BoardMotionChecks
{
    public static int Run(string root)
    {
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/board-motion.json.gz"));
        using var gzip=new GZipStream(file,CompressionMode.Decompress);
        using var fixture=JsonDocument.Parse(gzip); int checks=0;
        var options=new JsonSerializerOptions { PropertyNameCaseInsensitive=true };
        void Near(double a,double b,double tolerance) { checks++; if(Math.Abs(a-b)>tolerance) throw new InvalidOperationException($"Board motion mismatch: {a:R} vs {b:R}."); }
        foreach(var row in fixture.RootElement.EnumerateArray())
        {
            double time=row.GetProperty("time").GetDouble(); var motion=row.GetProperty("motion").Deserialize<BoardMotion>(options)!;
            if(motion.Kind=="sea")
            {
                var samples=row.GetProperty("vertices"); var vertices=new float[samples.GetArrayLength()*GltfPrimitive.Stride]; int i=0;
                foreach(var sample in samples.EnumerateArray()) { vertices[i]=sample[0].GetSingle(); vertices[i+2]=sample[1].GetSingle(); i+=GltfPrimitive.Stride; }
                BoardSceneMotion.Sea(vertices,time,motion); i=0;
                foreach(var sample in samples.EnumerateArray()) { Near(vertices[i+1],sample[2].GetSingle(),1e-7); Near(vertices[i],sample[0].GetSingle(),0); Near(vertices[i+2],sample[1].GetSingle(),0); i+=GltfPrimitive.Stride; }
            }
            else if(motion.Kind is "desert-haze" or "grid-floor")
            {
                var samples=row.GetProperty("samples"); var vertices=new float[samples.GetArrayLength()*GltfPrimitive.Stride]; int at=0;
                foreach(var sample in samples.EnumerateArray())
                {
                    vertices[at+6]=sample.GetProperty("source")[0].GetSingle()*(motion.Kind=="grid-floor"?60:8);
                    vertices[at+7]=sample.GetProperty("source")[1].GetSingle()*(motion.Kind=="grid-floor"?60:1); at+=GltfPrimitive.Stride;
                }
                var initial=(float[])vertices.Clone();
                BoardSceneMotion.ScrollingUv(vertices,initial,time,motion.Kind); at=0;
                foreach(var sample in samples.EnumerateArray())
                {
                    Near(vertices[at+6],sample.GetProperty("expected")[0].GetDouble(),motion.Kind=="grid-floor"?1e-5:1e-6);
                    Near(vertices[at+7],sample.GetProperty("expected")[1].GetDouble(),motion.Kind=="grid-floor"?1e-5:1e-7); at+=GltfPrimitive.Stride;
                }
                if(motion.Kind=="desert-haze") Near(BoardSceneMotion.Haze(time).Opacity,row.GetProperty("opacity").GetDouble(),1e-7);
                BoardSceneMotion.ScrollingUv(vertices,initial,0,motion.Kind);
                for(int i=0;i<vertices.Length;i++) Near(vertices[i],initial[i],0);
            }
            else if(motion.Kind is "mountain-stars" or "grid-rings")
                Near(BoardSceneMotion.Opacity(motion.Kind,time)!.Value,row.GetProperty("opacity").GetDouble(),1e-7);
            else if(motion.Kind=="rotating-parent")
            {
                double[] values=new double[16]; BoardSceneMotion.RotatingParent(motion,time).CopyTo(values);
                for(int j=0;j<16;j++) Near(values[j],row.GetProperty("matrix")[j].GetDouble(),1e-10);
            }
            else if(motion.Kind=="beach-surf")
            {
                var surf=BoardSceneMotion.Surf(motion,time); double[] values=new double[16]; surf.Matrix.CopyTo(values);
                for(int j=0;j<16;j++) Near(values[j],row.GetProperty("matrix")[j].GetDouble(),1e-10);
                Near(surf.Opacity,row.GetProperty("opacity").GetDouble(),1e-7);
            }
            else
            {
                var expected=row.GetProperty("matrices");
                for(int i=0;i<(motion.Props?.Length??(motion.Kind=="beach-palms"?motion.Palms!.Length:motion.Items!.Length));i++)
                {
                    double[] values=new double[16];
                    var matrix=motion.Props is {} props?BoardSceneMotion.Prop(motion.Kind,props[i],time):motion.Kind=="beach-palms"?BoardSceneMotion.Palm(motion.Palms![i],time):BoardSceneMotion.Crystal(motion.Items![i],time);
                    matrix.CopyTo(values);
                    for(int j=0;j<16;j++) Near(values[j],expected[i][j].GetDouble(),1e-6);
                }
            }
        }
        using var spiralFile=File.OpenRead(Path.Combine(root,"tests/fixtures/board-spiral.json.gz"));
        using var spiralGzip=new GZipStream(spiralFile,CompressionMode.Decompress);
        using var spiralFixture=JsonDocument.Parse(spiralGzip);
        var states=spiralFixture.RootElement.GetProperty("motions").EnumerateArray()
            .Select(row=>row.Deserialize<BoardMotion>(options)!).ToDictionary(m=>m.Kind,m=>new BoardSpiralMotion(m));
        foreach(var frame in spiralFixture.RootElement.GetProperty("frames").EnumerateArray())
        {
            foreach(var state in states.Values) state.Advance(frame.GetProperty("delta").GetDouble(),frame.GetProperty("time").GetDouble(),frame.GetProperty("combat").GetBoolean());
            foreach(var obj in frame.GetProperty("objects").EnumerateArray())
            {
                var state=states[obj.GetProperty("kind").GetString()!]; var expected=obj.GetProperty("matrices");
                for(int i=0;i<state.Matrices.Length;i++)
                {
                    double[] values=new double[16];state.Matrices[i].CopyTo(values);
                    for(int j=0;j<16;j++) Near(values[j],expected[i][j].GetDouble(),1e-6);
                }
            }
        }
        using var lightningFile=File.OpenRead(Path.Combine(root,"tests/fixtures/board-lightning.json.gz"));
        using var lightningGzip=new GZipStream(lightningFile,CompressionMode.Decompress);
        using var lightningFixture=JsonDocument.Parse(lightningGzip);
        foreach(var scenario in lightningFixture.RootElement.EnumerateArray())
        {
        var config=scenario.GetProperty("config").Deserialize<BoardLightningData>(options)!;
        var lightning=new DigiTactics.Presentation.BoardLightning(config.Spots,config.Flash,config.DelaySeed,config.DelayCalls,config.BoltCalls);
        lightning.Advance(0,0,scenario.GetProperty("initialCombat").GetBoolean());
        foreach(var frame in scenario.GetProperty("frames").EnumerateArray())
        {
            lightning.Advance(frame.GetProperty("delta").GetDouble(),frame.GetProperty("time").GetDouble(),frame.GetProperty("combat").GetBoolean());
            Near(lightning.Flash,frame.GetProperty("flash").GetDouble(),1e-12);
            int index=0;
            foreach(var bolt in frame.GetProperty("bolts").EnumerateArray())
            {
                bool visible=lightning.Visible && lightning.Active==index++;checks++;
                if(visible!=bolt.GetProperty("visible").GetBoolean()) throw new InvalidOperationException("Lightning flicker/selection mismatch.");
                if(!visible) continue;
                double[] values=new double[16];new CompanionTransform(new(lightning.X,lightning.Y,lightning.Z),new(0,lightning.Yaw,0),new(1,1,1)).Matrix.CopyTo(values);
                for(int j=0;j<16;j++) Near(values[j],bolt.GetProperty("matrix")[j].GetDouble(),1e-10);
            }
        }
        }
        Console.WriteLine($"Board motion: {checks:N0} original wave, crystal, palm, surf, prop, spiral and lightning comparisons passed."); return checks+BoardCastleChecks.Run(root)+BoardKowloonChecks.Run(root)+BoardCircuitChecks.Run(root)+BoardTempleChecks.Run(root)+BoardUnderwaterChecks.Run(root)+BoardFreezeChecks.Run(root)+BoardSewerChecks.Run(root)+BoardLibraryChecks.Run(root)+BoardKnightsChecks.Run(root)+BoardStudioChecks.Run(root)+BoardTrainChecks.Run(root)+BoardGraveyardChecks.Run(root)+BoardPyramidChecks.Run(root);
    }
}
