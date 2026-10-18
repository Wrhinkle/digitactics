using System.IO.Compression;
using System.Numerics;
using System.Text.Json;
using DigiTactics.Assets;

internal static class BoardFeedbackChecks
{
    public static int Run(string root)
    {
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/board-feedback.json.gz"));
        using var gzip=new GZipStream(file,CompressionMode.Decompress);
        using var fixture=JsonDocument.Parse(gzip);
        var themes=new Dictionary<string,BoardSurfaces>(); int checks=0;
        void Near(float a,float b) { checks++; if(Math.Abs(a-b)>2e-6) throw new InvalidOperationException($"Board feedback mismatch: {a} vs {b}."); }
        void Vector(Vector3 value,JsonElement expected) { Near(value.X,expected[0].GetSingle()); Near(value.Y,expected[1].GetSingle()); Near(value.Z,expected[2].GetSingle()); }
        foreach(var row in fixture.RootElement.EnumerateArray())
        {
            string id=row.GetProperty("id").GetString()!;
            if(!themes.TryGetValue(id,out var theme)) themes[id]=theme=BoardSurfaces.From(BoardScene.Load(Path.Combine(root,"content/boards",id+".json.gz")));
            var ui=row.GetProperty("ui"); var cell=ui.GetProperty("hoverCell"); var drag=ui.GetProperty("itemDrag");
            var feedback=new BoardSurfaceFeedback { Time=row.GetProperty("time").GetDouble(),UnitDragMoved=ui.GetProperty("drag").GetProperty("moved").GetBoolean(),
                HoverCell=(cell.GetProperty("c").GetInt32(),cell.GetProperty("r").GetInt32()),HoverBench=ui.GetProperty("hoverBench").GetInt32(),HoverItem=ui.GetProperty("hoverItem").GetInt32(),
                ItemDragging=drag.ValueKind!=JsonValueKind.Null,ItemDragOver=drag.ValueKind!=JsonValueKind.Null?drag.GetProperty("over").GetInt32():-1 };
            bool planning=row.GetProperty("planning").GetBoolean();
            for(int i=0;i<56;i++)
            {
                int c=i%7,r=i/7;
                feedback.Flash(c,r,7+i%7*.15,BoardLighting.Linear(i%2!=0?"#6fd3ff":"#ff7a6a"));
                var material=r>=4?theme.Mine:planning?theme.EnemyPlanning:theme.Enemy;
                Vector(feedback.Tile(c,r,planning,material),row.GetProperty("tiles")[i]);
            }
            for(int i=0;i<9;i++) Vector(feedback.Bench(i),row.GetProperty("slots")[i]);
            foreach(var (ped,i) in row.GetProperty("peds").EnumerateArray().Select((p,i)=>(p,i)))
                if(ped.GetProperty("visible").GetBoolean()) Vector(feedback.Item(i),ped.GetProperty("emissive"));
            feedback.ClearFlashes(); feedback.UnitDragMoved=false;
            var actual=feedback.Tile(0,0,false,theme.Enemy);
            Near(actual.X,theme.Enemy.Emissive[0]); Near(actual.Y,theme.Enemy.Emissive[1]); Near(actual.Z,theme.Enemy.Emissive[2]);
        }
        Console.WriteLine($"Board feedback: {checks:N0} source comparisons passed for hover priority, flash fade/expiry, planning suppression, bench/item highlights and reset.");
        return checks;
    }
}
