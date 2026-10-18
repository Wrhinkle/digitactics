using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Storage;

internal static class EvolutionChecks
{
    public static int Run(string root)
    {
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/evolution.json.gz"));
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        int checks = 0;
        string portraitFolder=Path.Combine(root,"content/evolution-portraits");
        var portraits=JsonSerializer.Deserialize<Dictionary<string,string>>(File.ReadAllText(Path.Combine(portraitFolder,"manifest.json")))!;
        using(var content=JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"src/DigiTactics.Storage/evolution.json"))))
        foreach(var form in content.RootElement.GetProperty("forms").EnumerateObject())
        {
            if(!portraits.ContainsKey(form.Name)) throw new InvalidOperationException("Missing form portrait: "+form.Name);
            checks++;
        }
        foreach(string name in portraits.Values.Distinct())
        {
            byte[] bytes=File.ReadAllBytes(Path.Combine(portraitFolder,name));
            if(Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(bytes))+".png"!=name) throw new InvalidDataException("Evolution portrait hash mismatch.");
            var image=DigiTactics.Assets.PngImage.Decode(bytes);
            if(image.Width<32 || image.Height<32 || !Enumerable.Range(0,image.Width*image.Height).Any(i=>image.Rgba[i*4+3]>0)) throw new InvalidDataException("Empty evolution portrait.");
            checks++;
        }
        using(var trees=JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"tests/fixtures/evolution-trees.json"))))
        foreach(var tree in trees.RootElement.EnumerateArray())
        {
            var actual=JsonSerializer.SerializeToNode(LegendEvolution.Tree(tree.GetProperty("root").GetString()!),options);
            if(!JsonNode.DeepEquals(actual,JsonNode.Parse(tree.GetRawText()))) throw new InvalidOperationException("Evolution tree differs: "+tree.GetProperty("root"));
            checks++;
        }
        using(var discoveryInput=File.OpenRead(Path.Combine(root,"tests/fixtures/evolution-discovery.json.gz")))
        using(var discoveryGzip=new GZipStream(discoveryInput,CompressionMode.Decompress))
        using(var discoveries=JsonDocument.Parse(discoveryGzip))
        foreach(var row in discoveries.RootElement.EnumerateArray())
        {
            var saved=row.GetProperty("legend");
            var projection=LegendEvolution.Discovery(saved.ValueKind==JsonValueKind.Null?null:saved.GetRawText(),row.GetProperty("id").GetString()!);
            if(projection.Current!=row.GetProperty("current").GetString() || !projection.Path.SequenceEqual(row.GetProperty("path").EnumerateArray().Select(n=>n.GetString())) || !projection.Dex.SequenceEqual(row.GetProperty("dex").EnumerateArray().Select(n=>n.GetString()))) throw new InvalidOperationException("Evolution discovery differs from source.");
            checks++;
        }
        foreach (var test in fixture.RootElement.EnumerateArray())
        {
            string? Read(string name) => test.GetProperty(name).ValueKind == JsonValueKind.Null ? null : test.GetProperty(name).GetRawText();
            var view = LegendEvolution.Inspect(Read("legend"), Read("wallet"), Read("farm"), test.GetProperty("id").GetString()!);
            var actual = JsonSerializer.SerializeToNode(view, options);
            if (!JsonNode.DeepEquals(actual, JsonNode.Parse(test.GetProperty("view").GetRawText()))) throw new InvalidOperationException("Evolution mismatch: " + test + " got " + actual);
            checks++;
        }
        using(var careFile=File.OpenRead(Path.Combine(root,"tests/fixtures/evolution-care.json.gz")))
        using(var careZip=new GZipStream(careFile,CompressionMode.Decompress))
        using(var careCases=JsonDocument.Parse(careZip))
        foreach(var test in careCases.RootElement.EnumerateArray())
        {
            string? Read(string name)=>test.GetProperty(name).ValueKind==JsonValueKind.Null?null:test.GetProperty(name).GetRawText();
            var care=LegendEvolution.Care(Read("legend"),Read("farm"),test.GetProperty("id").GetString()!);
            if(!JsonNode.DeepEquals(JsonSerializer.SerializeToNode(care.Values,options),JsonNode.Parse(test.GetProperty("values").GetRawText())) || !JsonNode.DeepEquals(JsonSerializer.SerializeToNode(care.Bonus,options),JsonNode.Parse(test.GetProperty("bonus").GetRawText())))throw new InvalidOperationException("Evolution care projection differs from source.");
            checks++;
        }
        using(var layouts=JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"tests/fixtures/evolution-tree-layout.json"))))
        foreach(var test in layouts.RootElement.EnumerateArray())
        {
            var layout=DigiTactics.Presentation.EvolutionTreeLayout.Fit(test.GetProperty("width").GetDouble(),test.GetProperty("height").GetDouble(),test.GetProperty("rows").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
            if(layout.NodeWidth!=test.GetProperty("nodeWidth").GetInt32() || layout.PortraitSize!=test.GetProperty("portraitSize").GetInt32() || layout.ShowNames!=test.GetProperty("showNames").GetBoolean())throw new InvalidOperationException("Evolution tree fit differs from source.");
            checks++;
        }
        using var actionsFile = File.OpenRead(Path.Combine(root, "tests/fixtures/evolution-actions.json.gz"));
        using var actionsGzip = new GZipStream(actionsFile, CompressionMode.Decompress);
        using var actions = JsonDocument.Parse(actionsGzip);
        foreach (var test in actions.RootElement.EnumerateArray())
        {
            string? Read(string name) => test.GetProperty(name).ValueKind == JsonValueKind.Null ? null : test.GetProperty(name).GetRawText();
            var action = Enum.Parse<LegendAction>(test.GetProperty("action").GetString()!, true);
            var result = LegendEvolution.Apply(Read("legend"), Read("wallet"), Read("farm"), test.GetProperty("id").GetString()!, action);
            void Equal(JsonNode? actual, string name)
            {
                checks++;
                if (!JsonNode.DeepEquals(actual, JsonNode.Parse(test.GetProperty(name).GetRawText()))) throw new InvalidOperationException("Evolution action " + name + ": " + test + " got " + actual);
            }
            Equal(result.Result, "result"); Equal(JsonNode.Parse(result.Legends ?? "null"), "legendAfter"); Equal(JsonNode.Parse(result.Wallet!), "walletAfter");
        }
        Console.WriteLine($"Evolution: {checks} original views, farm bonuses, rolls and action/save comparisons passed.");
        return checks;
    }
}
