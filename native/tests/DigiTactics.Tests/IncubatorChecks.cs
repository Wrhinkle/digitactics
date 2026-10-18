using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Presentation;

internal static class IncubatorChecks
{
    public static int Run(string root)
    {
        int checks=0;
        void Check(bool condition,string label){checks++;if(!condition)throw new InvalidOperationException("Incubator: "+label);}
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/incubator.json.gz"));
        using var gzip=new GZipStream(file,CompressionMode.Decompress);
        using var fixtures=JsonDocument.Parse(gzip);
        foreach(var fixture in fixtures.RootElement.EnumerateArray())
        {
            string wallet=fixture.GetProperty("wallet").GetRawText();
            string? legends=fixture.GetProperty("legends").ValueKind==JsonValueKind.Null?null:fixture.GetProperty("legends").GetRawText();
            var actual=IncubatorOverview.Read(wallet,legends,null);var expected=fixture.GetProperty("expected");
            Check(actual.DigiData==expected.GetProperty("data").GetInt64(),"current Digi-Data total");
            Check(actual.Ready==expected.GetProperty("ready").GetInt32(),"ready total");
            Check(actual.Legends.Length==expected.GetProperty("legends").GetArrayLength(),"owned legend total");
            int index=0;
            foreach(var legend in expected.GetProperty("legends").EnumerateArray())
            {
                var item=actual.Legends[index++];
                Check(item.Item.Id==legend.GetProperty("id").GetString(),"stable ready-first, descending-level order");
                Check(item.Evolution.Form==legend.GetProperty("form").GetString()&&item.Evolution.Level==legend.GetProperty("level").GetInt32()&&item.Evolution.Data==legend.GetProperty("data").GetInt32()&&item.Evolution.CanDigivolve==legend.GetProperty("ready").GetBoolean(),"owned legend progress");
            }
            foreach(var egg in expected.GetProperty("eggs").EnumerateArray())
            {
                var item=actual.Eggs.Single(candidate=>candidate.Definition.Id==egg.GetProperty("id").GetString());
                Check(item.Total==egg.GetProperty("total").GetInt32()&&item.Owned==egg.GetProperty("owned").GetInt32(),"egg owned/pool count");
                Check(item.Exclusives.Length==egg.GetProperty("exclusives").GetArrayLength(),"egg-only pool size");
                index=0;
                foreach(var exclusive in egg.GetProperty("exclusives").EnumerateArray())
                {
                    var chance=item.Exclusives[index++];
                    Check(chance.Kind==exclusive.GetProperty("kind").GetString()&&chance.Item.Id==exclusive.GetProperty("id").GetString()&&chance.Owned==exclusive.GetProperty("owned").GetBoolean(),"egg-only identity/ownership");
                    Check(Math.Abs(chance.Percent-exclusive.GetProperty("percent").GetDouble())<1e-12&&chance.Chance==exclusive.GetProperty("chance").GetString(),"egg-specific item probability and original rounding");
                }
            }
        }
        Console.WriteLine($"Incubator: {checks} original inventory, ready ordering, progress and exclusive egg odds checks passed.");
        return checks;
    }
}
