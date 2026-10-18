using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Assets;
using DigiTactics.Presentation;
using DigiTactics.Rules;

internal static class BoardBurstChecks
{
    public static int Run(string root)
    {
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/board-bursts.json.gz"));
        using var gzip=new GZipStream(file,CompressionMode.Decompress);
        using var fixture=JsonDocument.Parse(gzip); int checks=0;
        void Check(bool ok) { checks++; if(!ok) throw new InvalidOperationException("Combat board burst source mismatch."); }
        foreach(var row in fixture.RootElement.EnumerateArray())
        {
            var e=row.GetProperty("event"); int columns=row.GetProperty("columns").GetInt32(); bool mirror=row.GetProperty("mirror").GetBoolean();
            var projector=new CombatBoardBursts(columns,mirror); var flashes=new List<CombatTileFlash>();
            var events=new List<CombatEvent> { new(0,"damage",0),new(e.GetProperty("t").GetDouble(),"burst",-1,Cell:new(e.GetProperty("c").GetInt32(),e.GetProperty("r").GetInt32()),Radius:e.GetProperty("radius").GetInt32(),Team:e.GetProperty("team").GetInt32()) };
            projector.Update(events,flashes.Add); var expected=row.GetProperty("flashes"); Check(flashes.Count==expected.GetArrayLength());
            for(int i=0;i<flashes.Count;i++)
            {
                var actual=flashes[i]; var target=expected[i];
                Check(actual.Column==target.GetProperty("c").GetInt32() && actual.Row==target.GetProperty("r").GetInt32());
                Check(actual.Until==target.GetProperty("until").GetDouble());
                var color=BoardLighting.Linear(actual.ViewTeam==0?"#6fd3ff":"#ff7a6a"); var rgb=target.GetProperty("color");
                Check(Math.Abs(color.X-rgb[0].GetSingle())<2e-6 && Math.Abs(color.Y-rgb[1].GetSingle())<2e-6 && Math.Abs(color.Z-rgb[2].GetSingle())<2e-6);
            }
            int before=flashes.Count; projector.Update(events,flashes.Add); Check(flashes.Count==before && projector.FlashCount==before);
            events.Add(events[1] with { Time=4 }); projector.Update(events,flashes.Add);
            Check(flashes.Count==before*2 && flashes.Skip(before).All(f=>f.Until==4.45));
        }
        Console.WriteLine($"Board bursts: {checks:N0} source checks passed for Solo/Duos radii, both teams, mirrored cells, expiry timestamps and incremental event consumption.");
        return checks;
    }
}
