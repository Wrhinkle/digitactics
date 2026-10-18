using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private void EvolutionTreePanel(string id,EvolutionView view)
    {
        var tree=LegendEvolution.Tree(id);
        var discovery=LegendEvolution.Discovery(profile!.Storage.GetValueOrDefault("dt.legendevo.v1"),id);
        var known=discovery.Dex.ToHashSet(); var path=discovery.Path.ToHashSet();
        var odds=view.Odds.ToDictionary(o=>o.Form,o=>o.Pct);
        EvolutionTreeNodes=tree.Tiers.Sum(t=>t.Length);
        CollectionText($"{CosmeticCollection.Item("legend",id)!.Name}'s tree - Dex {discovery.Dex.Length}/{EvolutionTreeNodes} discovered");
        CollectionText("Green: current form. Gold: previous path. Blue: next possibilities. Dark forms are undiscovered.");
        ImGui.BeginChild("evolutionTreeCanvas"u8,new(0,0),ImGuiChildFlags.None);
        EvolutionTreeScroll=ImGui.GetScrollY();
        float width=ImGui.GetContentRegionAvail().X;
        int columns=Math.Max(1,(int)(width/140));
        float cardWidth=Math.Min(134,width),cardHeight=96;
        var origin=ImGui.GetCursorScreenPos();
        var nodes=new Dictionary<string,float2>(); float y=22;
        var headings=new List<(string Text,float Y)>();
        for(int tier=0;tier<tree.Tiers.Length;tier++)
        {
            headings.Add((tree.Levels[tier],y-20)); var row=tree.Tiers[tier];
            for(int i=0;i<row.Length;i++)
            {
                int line=i/columns,count=Math.Min(columns,row.Length-line*columns);
                float start=(width-count*140+6)/2;
                nodes[row[i]]=new(origin.X+start+(i%columns)*140,origin.Y+y+line*120);
            }
            y+=(float)Math.Ceiling((double)row.Length/columns)*120+28;
        }
        ImGui.Dummy(new(width,y)); var draw=ImGui.GetWindowDrawList();
        var chain=discovery.Path.Append(discovery.Current).ToArray();
        foreach(var edge in tree.Edges)
        {
            var from=nodes[edge[0]]; var to=nodes[edge[1]];
            bool walked=Enumerable.Range(0,Math.Max(0,chain.Length-1)).Any(i=>chain[i]==edge[0] && chain[i+1]==edge[1]);
            var color=walked?new ImColor8(225,185,80):edge[0]==view.Form?new ImColor8(80,180,230):new ImColor8(60,75,90);
            draw.AddLine(new(from.X+cardWidth/2,from.Y+cardHeight),new(to.X+cardWidth/2,to.Y),color,walked?2f:1f);
        }
        foreach(var heading in headings) draw.AddText(ImGui.GetFont(),14,new(origin.X,origin.Y+heading.Y),new ImColor8(170,190,210),heading.Text);
        foreach(var pair in nodes)
        {
            string formId=pair.Key; var at=pair.Value; var form=LegendEvolution.Form(formId);
            bool discovered=known.Contains(formId),current=formId==view.Form;
            var color=current?new ImColor8(30,110,80):path.Contains(formId)?new ImColor8(100,80,30):odds.ContainsKey(formId)?new ImColor8(35,85,120):new ImColor8(25,34,45);
            draw.AddRectFilled(at,new(at.X+cardWidth,at.Y+cardHeight),color);
            draw.PushClipRect(at,new(at.X+cardWidth,at.Y+cardHeight),true);
            portraits.DrawEvolution(formId,new(at.X+(cardWidth-52)/2,at.Y+3),52,discovered); EvolutionTreePortraits++;
            string name=discovered?form.Name:"???";
            draw.AddText(ImGui.GetFont(),12,new(at.X+6,at.Y+60),new ImColor8(235,240,245),name);
            string status=current?"Current":odds.TryGetValue(formId,out int chance)?$"{chance}%":path.Contains(formId)?"Previous form":discovered?"Discovered":"Undiscovered";
            draw.AddText(ImGui.GetFont(),12,new(at.X+6,at.Y+78),new ImColor8(170,200,220),status);
            draw.PopClipRect();
            if(ImGui.IsMouseHoveringRect(at,new(at.X+cardWidth,at.Y+cardHeight)))
            {
                ImGui.BeginTooltip(); UiText.Wrapped(discovered?$"{form.Name} ({form.Level})":$"Not discovered yet ({form.Level})"); ImGui.EndTooltip();
            }
        }
        ImGui.EndChild();
    }
}
