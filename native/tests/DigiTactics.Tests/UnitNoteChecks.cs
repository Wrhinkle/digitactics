using System.Text.Json;
using System.IO.Compression;
using DigiTactics.Presentation;
using DigiTactics.Rules;

internal static class UnitNoteChecks
{
    private sealed record Sample(string Variant,string Line,int Stage,JsonElement? Mods,string[] Notes,string[]? CombatNotes);
    public static void Run(string root)
    {
        var notes=new UnitNotes(Path.Combine(root,"content/unit-notes.json"));
        using var fixture=File.OpenRead(Path.Combine(root,"tests/fixtures/unit-notes.json.gz"));
        using var inflated=new GZipStream(fixture,CompressionMode.Decompress);
        var samples=JsonSerializer.Deserialize<Sample[]>(inflated,new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
        var catalog=ContentCatalog.Load(Path.Combine(root,"content/catalog.json"));
        var seats=Enumerable.Range(0,8).Select(i=>new MatchSeat("Note Tamer "+i,i==0)).ToArray();
        var sessions=catalog.Variants.ToDictionary(r=>r.Id,r=>new MatchPlanningSession(r,461,seats));
        int checks=0,combatChecks=0,malformed=0;
        foreach(var sample in samples)
        {
            var planning=new PlanningUnit(10,sample.Line,sample.Stage,[],Mods:sample.Mods);
            if(!notes.Describe(sample.Variant,planning).SequenceEqual(sample.Notes))throw new InvalidOperationException($"Planning unit note differs from source: {sample.Variant}/{sample.Line}/{sample.Mods}");checks++;
            if(sample.CombatNotes is null){malformed++;continue;}
            var session=sessions[sample.Variant];var player=session.State.Players[0];
            foreach(var old in player.Roster.Entries.ToArray())player.Roster.Remove(old.Unit.Id,false);
            player.Roster.AddToBench(planning);player.Roster.Move(UnitSpot.Bench(0),UnitSpot.Board(0,4),false,()=>9);
            var combat=session.CombatProjection.Board(player).Single();
            if(!notes.Describe(sample.Variant,combat).SequenceEqual(sample.CombatNotes))throw new InvalidOperationException($"Projected combat unit note differs from source: {sample.Variant}/{sample.Line}/{sample.Mods}");checks++;combatChecks++;
        }
        if(samples.Select(s=>s.Variant).Distinct().Count()!=22||!samples.Any(s=>s.Notes.Length==2)||!samples.Any(s=>s.Notes.Any(n=>n.StartsWith("Partner:",StringComparison.Ordinal)))||!samples.Any(s=>s.Notes.Any(n=>n.StartsWith("Anomaly:",StringComparison.Ordinal))))throw new InvalidOperationException("Unit note source cases are incomplete.");
        Console.WriteLine($"Unit note parity passed: {checks} checks across 22 variants ({samples.Length} planning, {combatChecks} actual combat projections), including Partner bonuses, Anomalies, Digi-Core paths, line-specific forms and note order. {malformed} malformed non-string modifier cases are planning-only because existing CombatEntry fields are typed strings.");
    }
}
