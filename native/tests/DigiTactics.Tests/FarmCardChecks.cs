using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Storage;

internal static class FarmCardChecks
{
    public static int Run(string root)
    {
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/farm-card.json.gz"));using var zip=new GZipStream(file,CompressionMode.Decompress);using var fixture=JsonDocument.Parse(zip);
        int count=0;
        foreach(var row in fixture.RootElement.EnumerateArray())
        {
            var expected=row.GetProperty("card");string id=row.GetProperty("id").GetString()!;
            var actual=FarmResidentDetails.Read(row.GetProperty("farm").GetRawText(),row.GetProperty("legend").GetRawText(),row.GetProperty("wallet").GetRawText(),id,row.GetProperty("time").GetInt64());
            void Check(bool value,string field){count++;if(!value)throw new InvalidOperationException("Farm card differs from source: "+id+" / "+field);}
            Check(actual.Evolution.Form==expected.GetProperty("form").GetString(),"form");Check(actual.Evolution.Level==expected.GetProperty("level").GetInt32(),"level");
            Check(actual.AgeDays==expected.GetProperty("ageDays").GetInt64(),"age");Check(actual.IdealWeight==expected.GetProperty("ideal").GetInt32(),"ideal weight");
            Check(actual.Fat==expected.GetProperty("fat").GetBoolean(),"fat");Check(actual.Thin==expected.GetProperty("thin").GetBoolean(),"thin");
            Check(actual.Mistakes==expected.GetProperty("mistakes").GetInt32(),"care mistakes");Check(actual.PoopNeglect==expected.GetProperty("poopNeglect").GetInt32(),"neglected piles");
            Check(actual.Overfeed==expected.GetProperty("overfeed").GetInt32(),"overfeeding");Check(actual.Sick==expected.GetProperty("sick").GetBoolean(),"sickness");
            Check(actual.TrainingRefusal==expected.GetProperty("canTrain").GetString(),"training refusal: "+actual.TrainingRefusal+" != "+expected.GetProperty("canTrain").GetString());
            Check(actual.Evolution.CanDigivolve==expected.GetProperty("canDigivolve").GetBoolean(),"ready");
            Check(actual.Evolution.Next==(expected.GetProperty("next").ValueKind==JsonValueKind.Null?null:expected.GetProperty("next").GetInt32()),"next level");
            foreach(string key in new[]{"bond","battle","food","wild"})Check(actual.Care.Bonus[key]==expected.GetProperty("evo").GetProperty(key).GetDouble(),key+" influence");
            var odds=expected.GetProperty("odds").EnumerateArray().ToArray();Check(actual.Evolution.Odds.Length==odds.Length,"branch count");
            for(int i=0;i<odds.Length;i++)Check(actual.Evolution.Odds[i].Form==odds[i].GetProperty("form").GetString() && actual.Evolution.Odds[i].Care==odds[i].GetProperty("care").GetString() && actual.Evolution.Odds[i].Pct==odds[i].GetProperty("pct").GetInt32(),"branch odds");
        }
        Console.WriteLine($"Farm card: {count} original age, weight, care, training and evolution checks passed.");return count;
    }
}
