using System.Text.Json.Nodes;

namespace DigiTactics.Storage;

public sealed record EvolutionTree(string Root,string[][] Tiers,string[] Levels,string[][] Edges);
public sealed record EvolutionDiscovery(string Current,string[] Path,string[] Dex);

public static partial class LegendEvolution
{
    public static EvolutionDiscovery Discovery(string? saved,string id)
    {
        var (_,record)=LegendRewards.Snapshot(saved,id);
        string[] Read(string key)=>record[key]!.AsArray().Select(n=>n!.GetValue<string>()).ToArray();
        return new(record["form"]!.GetValue<string>(),Read("path"),Read("dex"));
    }
    public static EvolutionTree Tree(string id)
    {
        if(!Content.Forms.ContainsKey(id)) throw new ArgumentException("Unknown evolution root.",nameof(id));
        var order=new List<string>{id}; var seen=new HashSet<string>{id};
        var edges=new List<string[]>(); var parents=new Dictionary<string,List<string>>();
        for(int i=0;i<order.Count;i++) foreach(var child in Content.Children.GetValueOrDefault(order[i],[]))
        {
            if(!Content.Forms.ContainsKey(child.Form)) continue;
            edges.Add([order[i],child.Form]);
            if(!parents.TryGetValue(child.Form,out var list)) parents[child.Form]=list=[];
            list.Add(order[i]); if(seen.Add(child.Form)) order.Add(child.Form);
        }
        var positions=new Dictionary<string,double>(); var rows=new List<string[]>();
        foreach(var group in order.GroupBy(f=>f==id?-1:Math.Max(Content.Forms[f].Rank,Content.Forms[id].Rank+1)).OrderBy(g=>g.Key))
        {
            double At(string f)
            {
                var known=parents.GetValueOrDefault(f,[]).Where(positions.ContainsKey).ToArray();
                return known.Length==0?0:known.Average(p=>positions[p]);
            }
            var row=group.OrderBy(At).ToArray();
            for(int i=0;i<row.Length;i++) positions[row[i]]=row.Length>1?(double)i/(row.Length-1):.5;
            rows.Add(row);
        }
        return new(id,rows.ToArray(),rows.Select(r=>Content.Forms[r[0]].Level).ToArray(),edges.ToArray());
    }
}
