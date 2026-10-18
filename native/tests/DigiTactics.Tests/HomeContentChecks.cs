using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Presentation;

internal static class HomeContentChecks
{
    internal static int Run(string root)
    {
        using var fixture=JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"tests/fixtures/home-content.json")));
        var content=HomeContent.Load(Path.Combine(root,"content/home/content.json"));int checks=0;
        void Check(bool value,string message){if(!value)throw new InvalidOperationException(message);checks++;}
        Check(JsonNode.DeepEquals(JsonSerializer.SerializeToNode(content,new JsonSerializerOptions{PropertyNamingPolicy=JsonNamingPolicy.CamelCase}),JsonNode.Parse(fixture.RootElement.GetProperty("content").GetRawText())),"Native Home copy differs from original client rendering.");
        Check(content.Patch.Length==7&&content.Patch[1].Description.Contains("Chaos plays a random set every game",StringComparison.Ordinal),"Home patch notes lost dynamic set rotation copy.");
        foreach(var row in fixture.RootElement.GetProperty("rotation").EnumerateArray())
        {
            var now=row.GetProperty("now").GetDateTimeOffset();var zone=TimeZoneInfo.FindSystemTimeZoneById(row.GetProperty("zone").GetString()!);
            var remaining=HomeRotation.UntilMidnight(now,zone);
            Check(Math.Abs(remaining.TotalMilliseconds-row.GetProperty("milliseconds").GetDouble())<.01,"Daily reset differs from JavaScript across a local midnight/DST transition.");
            Check(HomeRotation.Label(remaining)==row.GetProperty("text").GetString(),"Rotation hours/minutes differ from original Home Events.");
        }
        Console.WriteLine($"Home content: {checks} source-copy and local-midnight/DST countdown checks passed.");return checks;
    }
}
