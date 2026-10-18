using System.Text.Json;
using DigiTactics.Assets;

internal static class BoardOrderChecks
{
    public static int Run(string root)
    {
        using var fixture=JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"tests/fixtures/board-order.json")));
        var sample=BoardScene.Load(Path.Combine(root,"content/boards/file-island.json.gz")).Objects[0];int checks=0;
        foreach(var row in fixture.RootElement.EnumerateArray())
        {
            double[] Values(JsonElement e)=>e.EnumerateArray().Select(v=>v.GetDouble()).ToArray();
            var camera=Matrix4D.From(Values(row.GetProperty("camera")));
            var items=row.GetProperty("items").EnumerateArray().Select(e=>
            {
                var obj=sample with {SortId=e.GetProperty("id").GetInt32(),RenderOrder=e.GetProperty("renderOrder").GetInt32(),SortCenter=Values(e.GetProperty("center"))};
                double depth=BoardDrawOrder.Depth(obj,Matrix4D.From(Values(e.GetProperty("world"))),camera);checks++;
                if(Math.Abs(depth-e.GetProperty("z").GetDouble())>1e-10)throw new InvalidOperationException("Transparent sort depth mismatch.");
                return (Object:obj,Depth:depth);
            }).Reverse().ToArray();
            Array.Sort(items,(a,b)=>BoardDrawOrder.Compare(a.Object,a.Depth,b.Object,b.Depth));
            checks++;if(!items.Select(i=>i.Object.SortId).SequenceEqual(row.GetProperty("expected").EnumerateArray().Select(v=>v.GetInt32())))throw new InvalidOperationException("Original Three transparent order mismatch.");
        }
        Console.WriteLine($"Board ordering: {checks:N0} original camera-depth and render-order comparisons passed.");return checks;
    }
}
