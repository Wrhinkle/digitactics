using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Assets;

internal static class BoardCityChecks
{
    public static int Run(string root)
    {
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/board-city.json.gz"));using var gzip=new GZipStream(file,CompressionMode.Decompress);using var fixture=JsonDocument.Parse(gzip);
        var options=new JsonSerializerOptions {PropertyNameCaseInsensitive=true};int checks=0;
        foreach(var scenario in fixture.RootElement.EnumerateArray())
        {
            int initialStage=scenario.GetProperty("initialStage").GetInt32();
            var motions=scenario.GetProperty("motions").EnumerateArray().Select(e=>e.Deserialize<BoardMotion>(options)!).ToArray();
            if(motions.Length!=14)throw new InvalidOperationException("Missing city effects.");
            var buildings=motions.Select(m=>m.Building is {} b?new BoardCityBuilding(b,initialStage):null).ToArray();
            void Check(JsonElement objects,int stage)
            {
                for(int i=0;i<motions.Length;i++)
                {
                    var expected=objects[i];var building=buildings[i];bool visible=building?.Visible??stage>=2;checks++;
                    if(visible!=expected.GetProperty("visible").GetBoolean())throw new InvalidOperationException("City building/crop visibility mismatch.");
                    if(building is null)continue;
                    double[] values=new double[16];building.Matrix.CopyTo(values);
                    for(int k=0;k<16;k++){checks++;if(Math.Abs(values[k]-expected.GetProperty("matrix")[k].GetDouble())>1e-10)throw new InvalidOperationException("City spring transform mismatch.");}
                }
            }
            Check(scenario.GetProperty("initial"),initialStage);
            foreach(var frame in scenario.GetProperty("frames").EnumerateArray())
            {
                int stage=frame.GetProperty("stage").GetInt32();double delta=frame.GetProperty("delta").GetDouble();
                foreach(var building in buildings)building?.Advance(delta,stage);
                Check(frame.GetProperty("objects"),stage);
            }
        }
        Console.WriteLine($"City growth: {checks:N0} original initial-stage, spring, crop and visibility comparisons passed.");return checks;
    }
}
