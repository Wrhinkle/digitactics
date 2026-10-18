using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Storage;

internal static class ProfileSummaryChecks
{
    public static int Run(string root)
    {
        int checks=0;
        void Check(bool value,string label) {checks++;if(!value)throw new InvalidOperationException("Profile summary: "+label);}
        using var source=JsonDocument.Parse(File.ReadAllBytes(Path.Combine(root,"tests/fixtures/profile-summary.json")));
        foreach(var row in source.RootElement.GetProperty("cases").EnumerateArray())
        {
            var summary=ProfileSummary.Read(row.GetProperty("saved").GetString(),null);var expected=row.GetProperty("stats");var rank=row.GetProperty("rank");
            Check(summary.Games==expected.GetProperty("games").GetInt64() && summary.Wins==expected.GetProperty("wins").GetInt64() && summary.TopFour==expected.GetProperty("top4").GetInt64(),"lifetime totals");
            Check(Math.Abs(summary.Average-expected.GetProperty("avg").GetDouble())<1e-12,"average placement");
            Check(summary.Rank.Id==rank.GetProperty("id").GetString() && summary.Rank.Label==rank.GetProperty("label").GetString() && summary.Rank.Detail==rank.GetProperty("sub").GetString(),"source rank thresholds/detail");
            foreach(var (field,values) in new[]{("traits",summary.Traits),("units",summary.Units)})
            {
                var list=expected.GetProperty(field).EnumerateArray().ToArray();
                Check(values.Length==list.Length && values.Select((v,i)=>v.Id==list[i][0].GetString() && v.Games==list[i][1].GetInt64()).All(x=>x),"stable favourite order and limit");
            }
            Check(summary.Placements.SequenceEqual(row.GetProperty("placements").EnumerateArray().Select(x=>x.GetInt32())),"source placement histogram");
            var trend=row.GetProperty("trend").EnumerateArray().ToArray();
            Check(summary.Trend.Length<2?trend.Length==0:summary.Trend.Length==trend.Length && summary.Trend.Select((m,i)=>m.Place==trend[i].GetProperty("place").GetInt32() && m.Mode==trend[i].GetProperty("mode").GetString()).All(x=>x),"last twenty oldest-first trend");
        }
        foreach(var row in source.RootElement.GetProperty("names").EnumerateArray())Check(ProfileSummary.CleanName(row.GetProperty("value").GetString()!)==row.GetProperty("expected").GetString(),"source name sanitizer");
        string saved="{\"future\":{\"keep\":true},\"profile\":{\"futureAvatar\":9},\"matches\":[],\"totals\":{\"games\":2,\"wins\":1,\"top4\":1}}";
        var collection=new CosmeticCollection(null);string free=CosmeticCollection.Items("legend").First(x=>x.Free).Id;
        string changed=ProfileSummary.Customize(ProfileSummary.Customize(saved,"banner","profile-4",collection),"avatar",free,collection);
        var value=JsonNode.Parse(changed)!;
        Check(value["future"]!["keep"]!.GetValue<bool>() && value["profile"]!["futureAvatar"]!.GetValue<int>()==9 && value["totals"]!["games"]!.GetValue<int>()==2,"customization preserves unknown progress");
        Check(ProfileSummary.Read(changed,null).Banner=="profile-4" && ProfileSummary.Read(changed,null).Avatar==free,"profile customization independent of equipment");
        void Invalid(Action action,string label) {try {action();}catch(InvalidDataException) {Check(true,label);return;}throw new InvalidOperationException(label);}
        Invalid(()=>ProfileSummary.Customize(saved,"banner","../../unknown",collection),"unknown banner rejected");
        Invalid(()=>ProfileSummary.Customize(saved,"avatar","unowned",collection),"unowned avatar rejected");
        Invalid(()=>ProfileSummary.Customize(saved,"totals","new",collection),"progress-field edit rejected");
        string wallet=JsonSerializer.Serialize(new {history=Enumerable.Range(0,20).Select(i=>new {n=i,reason=i==0?"":"Match "+i,at=1760000000000L+i})});
        var bits=ProfileSummary.Read(null,wallet).BitsHistory;
        Check(bits.Length==14 && bits[0].Reason=="Bits" && bits[^1].N==13,"source Bits history count, ordering and fallback");
        Check(ProfileSummary.Read(null,null).Rank.Unranked && ProfileSummary.Read(null,null).Banner=="profile-1","new profile defaults");
        Console.WriteLine($"Profile summary checks passed: {checks} source projections and preservation checks.");return checks;
    }
}
