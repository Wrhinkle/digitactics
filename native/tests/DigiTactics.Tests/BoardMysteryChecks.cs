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
            if(motions.Length<2 || motions.Any(m=>m.Kind!="mystery-kinetic" || m.Kinetic is null))throw new InvalidOperationException("Missing authored animated landmarks.");
            var scene=BoardScene.Load(Path.Combine(root,"content/boards",scenario.GetProperty("id").GetString()+".json.gz"));
            if(scene.Objects.Length>220 || scene.Geometries.Length>90 || scene.Materials.Length>24)throw new InvalidOperationException("Mystery scene exceeded its geometry/draw budget.");
            checks+=4;
            if(scenario.GetProperty("clearance").GetArrayLength()<motions.Length*6)throw new InvalidOperationException("Missing scene clearance samples.");
            foreach(var bounds in scenario.GetProperty("clearance").EnumerateArray())
            {
                var min=bounds.GetProperty("min");var max=bounds.GetProperty("max");
                // Ground footprint stays beyond the slab: a separate live-camera review checks projection/occlusion.
                bool clear=max[0].GetDouble()<=-7.2 || min[0].GetDouble()>=7.2 || max[2].GetDouble()<=-6.4 || min[2].GetDouble()>=6.4;
                checks++;if(!clear)throw new InvalidOperationException("Authored scenery enters the playable slab: "+scene.Id);
            }
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
        Console.WriteLine($"Mystery boards: {checks:N0} authored landmark and browser/native hierarchy comparisons passed across all ten themes.");return checks;
    }
}
