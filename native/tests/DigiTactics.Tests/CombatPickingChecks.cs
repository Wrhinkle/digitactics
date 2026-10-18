using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Presentation;

internal static class CombatPickingChecks
{
    public static int Run(string root)
    {
        using var input=File.OpenRead(Path.Combine(root,"tests/fixtures/combat-picking.json.gz"));
        using var gzip=new GZipStream(input,CompressionMode.Decompress);
        using var data=JsonDocument.Parse(gzip); int count=0;
        foreach(var row in data.RootElement.EnumerateArray())
        {
            var targets=row.GetProperty("targets").EnumerateArray().Select(t=>new CombatPickTarget(t.GetProperty("id").GetInt32(),t.GetProperty("x").GetDouble(),t.GetProperty("y").GetDouble(),t.GetProperty("alive").GetBoolean()));
            var expected=row.GetProperty("expected"); int? id=expected.ValueKind==JsonValueKind.Null?null:expected.GetInt32();
            if(CombatPicking.Pick(targets,row.GetProperty("x").GetDouble(),row.GetProperty("y").GetDouble())!=id) throw new InvalidOperationException($"Combat picking differs from source at case {count}.");
            count++;
        }
        Console.WriteLine($"Combat picking: {count} original nearest-body, radius, dead-unit and roster-order cases passed."); return count;
    }
}
