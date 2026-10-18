using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Networking;
using DigiTactics.Presentation;
using DigiTactics.Rules;

internal static class SetSelectionChecks
{
    public static int Run(string root,ContentCatalog catalog)
    {
        int checks=0;void Check(bool ok,string why){checks++;if(!ok)throw new InvalidOperationException("Set selection: "+why);}
        var choices=SetSelection.Load(Path.Combine(root,"content/set-selection.json"));
        using var fixture=JsonDocument.Parse(File.ReadAllBytes(Path.Combine(root,"tests/fixtures/set-selection.json")));
        var data=fixture.RootElement;
        Check(choices.DefaultId==data.GetProperty("defaultId").GetString(),"source default");
        Check(choices.ChaosPool.SequenceEqual(data.GetProperty("pool").EnumerateArray().Select(x=>x.GetString()!)),"source sorted eligible pool, including Knockout exclusion");
        Check(choices.Options.Select(x=>x.Id).Concat(choices.HasChaos?["chaos"]:Array.Empty<string>()).SequenceEqual(data.GetProperty("options").EnumerateArray().Select(x=>x.GetString()!)),"source choice order");
        foreach(var sample in data.GetProperty("preferences").EnumerateArray())
        {
            Check(choices.SavedSet(sample.GetProperty("set").GetString())==sample.GetProperty("expectedSet").GetString(),"stored set normalization");
            Check(SetSelection.SavedPace(sample.GetProperty("pace").GetString())==sample.GetProperty("expectedPace").GetString(),"stored pace excludes lobby-only Chaos");
            Check(SetSelection.SavedFormat(sample.GetProperty("format").GetString())==sample.GetProperty("expectedFormat").GetString(),"stored format normalization");
        }
        foreach(var sample in data.GetProperty("resolutions").EnumerateArray())
        {
            var actual=choices.Resolve(sample.GetProperty("set").GetString(),sample.GetProperty("pace").GetString(),sample.GetProperty("seed").GetUInt32());
            Check(actual==new ResolvedSet(sample.GetProperty("setId").GetString()!,sample.GetProperty("resolvedPace").GetString()!,sample.GetProperty("chaos").GetBoolean(),sample.GetProperty("chaosPace").GetBoolean()),"unsigned seed resolution");
            Check(SetSelection.Rules(catalog,actual).Id==actual.Variant,"resolved variant exists");
        }
        foreach(var sample in data.GetProperty("poolCases").EnumerateArray())
        {
            var definitions=sample.GetProperty("definitions").EnumerateArray().Select(d=>new SetChoice(d.GetProperty("id").GetString()!,"Test","","tft","hyper",d.TryGetProperty("hidden",out var hidden)&&hidden.GetBoolean(),!d.TryGetProperty("chaos",out var chaos)||chaos.GetBoolean())).ToArray();
            var synthetic=new SetSelection("default",definitions,definitions.Where(d=>!d.Hidden).Select(d=>d.Id));
            Check(synthetic.ChaosPool.SequenceEqual(sample.GetProperty("pool").EnumerateArray().Select(x=>x.GetString()!)),"hidden/excluded/empty pool eligibility");
            foreach(var pick in sample.GetProperty("picks").EnumerateArray())Check(synthetic.ResolveSet("chaos",pick.GetProperty("seed").GetUInt32())==pick.GetProperty("id").GetString(),"synthetic pool seed selection/fallback");
        }
        foreach(var sample in data.GetProperty("matches").EnumerateArray())
        {
            var local=sample.GetProperty("local");uint seed=local.GetProperty("seed").GetUInt32();
            string set=local.GetProperty("setId").GetString()!,pace=local.GetProperty("pace").GetString()!;bool duos=local.GetProperty("format").GetString()=="duos";
            var seats=local.GetProperty("slots").EnumerateArray().Select(s=>new MatchSeat(s.GetProperty("name").GetString()!,s.GetProperty("human").GetBoolean())).ToArray();
            var resolved=choices.Resolve(set,pace,seed);var rules=SetSelection.Rules(catalog,resolved);
            var match=new LockstepMatch(rules,seed,seats,30,12345,duos,chaos:resolved.Chaos,chaosPace:resolved.ChaosPace);
            Check(match.Check()==sample.GetProperty("localCheck").GetString(),"local Chaos preserves original initial RNG/state for Solo/Duos");
            Check(match.Session.State.Chaos==resolved.Chaos&&match.Session.State.ChaosPace==resolved.ChaosPace,"local selection provenance");
            var init=MatchInitialization.Host(choices,set,pace,seed,seats,duos,30,12345);
            Check(JsonNode.DeepEquals(JsonNode.Parse(init.GetRawText()),JsonNode.Parse(sample.GetProperty("hostInit").GetRawText())),"source host init carries resolved choices and provenance");
            Check(NetworkProtocol.Init(init),"Chaos init follows existing Steam protocol");
            var receiver=MatchInitialization.Load(catalog,init);
            Check(receiver.Check()==sample.GetProperty("networkCheck").GetString(),"resolved network init matches original state");
            Check(receiver.Session.State.Chaos==resolved.Chaos&&receiver.Session.State.ChaosPace==resolved.ChaosPace,"network flags retained for presentation");
            var reducedCatalog=new ContentCatalog(catalog.SchemaVersion,[rules]);
            Check(MatchInitialization.Load(reducedCatalog,init).Check()==receiver.Check(),"receivers use host resolution even with different installed set pool");
        }
        var unknown=JsonNode.Parse(data.GetProperty("matches")[0].GetProperty("hostInit").GetRawText())!;unknown["setId"]="chaos";
        try{MatchInitialization.Load(catalog,JsonSerializer.SerializeToElement(unknown));throw new Exception("Receiver rerolled unresolved Chaos.");}catch(InvalidDataException){checks++;}
        Console.WriteLine($"Set selection: {checks} original seed, preference, eligibility and Solo/Duos/Steam initialization checks passed.");return checks;
    }
}
