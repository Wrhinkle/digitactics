using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal int EvolutionTreePortraitSize {get;private set;}
    internal int EvolutionTreeKnownPortraits {get;private set;}
    private void EvolutionTreePanel(string id,EvolutionView view)
    {
        var tree=LegendEvolution.Tree(id);
        var discovery=DisplayedEvolutionDiscovery(id);
        var known=discovery.Dex.ToHashSet();var path=discovery.Path.ToHashSet();
        var odds=view.Odds.ToDictionary(o=>o.Form,o=>o.Pct);
        EvolutionTreeNodes=tree.Tiers.Sum(t=>t.Length);
        EvolutionTreeKnownPortraits=tree.Tiers.Sum(t=>t.Count(known.Contains));
        ImGui.PushFont(default,13);CollectionText($"{CosmeticCollection.Item("legend",id)!.Name}'s tree / Dex {discovery.Dex.Length}/{EvolutionTreeNodes}");
        ImGui.PushFont(default,11);CollectionText("Gold: current and next forms. Blue: previous path. Dark forms are undiscovered.");ImGui.PopFont();ImGui.PopFont();
        ImGui.PushStyleColor(ImGuiCol.ChildBg,new ImColor8(16,25,39));
        ImGui.BeginChild("evolutionTreeCanvas"u8,new(0,0),ImGuiChildFlags.Borders);
        try
        {
            EvolutionTreeScroll=ImGui.GetScrollY();EvolutionTreeScrollMax=ImGui.GetScrollMaxY();
            var available=ImGui.GetContentRegionAvail();var origin=ImGui.GetCursorScreenPos();
            var layout=EvolutionTreeLayout.Fit(available.X,available.Y,tree.Tiers.Select(row=>row.Length).ToArray());
            EvolutionTreePortraitSize=layout.PortraitSize;
            float label=layout.ShowNames?15:0,rowHeight=layout.PortraitSize+6+label;
            float used=tree.Tiers.Sum(row=>(float)Math.Ceiling((double)row.Length/layout.Columns)*rowHeight)+tree.Tiers.Length*4;
            float extra=Math.Max(0,available.Y-14-used),tierGap=extra/tree.Tiers.Length;
            float y=7+tierGap/2;
            var nodes=new Dictionary<string,float2>();var headings=new List<(string Text,float Y,float Height)>();
            foreach(var (row,tier) in tree.Tiers.Select((row,i)=>(row,i)))
            {
                int lineCount=(int)Math.Ceiling((double)row.Length/layout.Columns);
                headings.Add((tree.Levels[tier],y,lineCount*rowHeight));
                for(int i=0;i<row.Length;i++)
                {
                    int line=i/layout.Columns,count=Math.Min(layout.Columns,row.Length-line*layout.Columns);
                    float start=62+(available.X-70-count*(layout.NodeWidth+4)+4)/2;
                    nodes[row[i]]=new(origin.X+start+(i%layout.Columns)*(layout.NodeWidth+4),origin.Y+y+line*rowHeight);
                }
                y+=lineCount*rowHeight+4+tierGap;
            }
            ImGui.Dummy(new(available.X,Math.Max(available.Y,used+14)));
            var draw=ImGui.GetWindowDrawList();var chain=discovery.Path.Append(discovery.Current).ToArray();
            foreach(var edge in tree.Edges)
            {
                var from=nodes[edge[0]];var to=nodes[edge[1]];
                bool walked=Enumerable.Range(0,Math.Max(0,chain.Length-1)).Any(i=>chain[i]==edge[0]&&chain[i+1]==edge[1]);
                bool next=edge[0]==view.Form;
                var color=walked?new ImColor8(143,208,255):next?new ImColor8(255,211,106):new ImColor8(140,160,200,70);
                var p1=new float2(from.X+layout.NodeWidth/2f,from.Y+layout.PortraitSize);
                var p4=new float2(to.X+layout.NodeWidth/2f,to.Y);float middle=(p1.Y+p4.Y)/2;
                var p2=new float2(p1.X,middle);var p3=new float2(p4.X,middle);
                if(!next || walked)draw.AddBezierCubic(p1,p2,p3,p4,color,walked?2.5f:1.5f);
                else
                {
                    var previous=p1;float distance=0;
                    for(int i=1;i<=60;i++)
                    {
                        float t=i/60f,u=1-t;var point=p1*(u*u*u)+p2*(3*u*u*t)+p3*(3*u*t*t)+p4*(t*t*t);
                        float dx=point.X-previous.X,dy=point.Y-previous.Y;
                        if(distance%9<5)draw.AddLine(previous,point,color,2.5f);
                        distance+=MathF.Sqrt(dx*dx+dy*dy);previous=point;
                    }
                }
            }
            foreach(var heading in headings)
            {
                string level=heading.Text.Replace("In-Training","In-\nTraining");
                draw.AddText(ImGui.GetFont(),10,new(origin.X,origin.Y+heading.Y+(heading.Height-20)/2),new ImColor8(150,170,197),level,52);
            }
            foreach(var pair in nodes)
            {
                string formId=pair.Key;var at=pair.Value;var form=LegendEvolution.Form(formId);
                bool discovered=known.Contains(formId),current=formId==view.Form;
                float ps=layout.PortraitSize;var imageAt=new float2(at.X+(layout.NodeWidth-ps)/2,at.Y);var center=new float2(imageAt.X+ps/2,imageAt.Y+ps/2);
                var border=current?new ImColor8(255,207,74):odds.ContainsKey(formId)?new ImColor8(255,211,106):path.Contains(formId)?new ImColor8(143,208,255):new ImColor8(49,67,94);
                if(current){draw.AddCircle(center,ps/2+5,new ImColor8(255,207,74,35),thickness:7);draw.AddCircle(center,ps/2+2,new ImColor8(255,207,74,85),thickness:3);}
                draw.AddCircleFilled(center,ps/2,new ImColor8(32,46,68));
                portraits.DrawEvolution(formId,imageAt,ps,discovered);EvolutionTreePortraits++;
                draw.AddCircle(center,ps/2,border,thickness:2);
                if(layout.ShowNames)
                {
                    string name=discovered?form.Name:"???";float textWidth=ImGui.GetFont().CalcTextSizeA(11,float.MaxValue,0,name).X;
                    draw.PushClipRect(new(at.X,at.Y+ps),new(at.X+layout.NodeWidth,at.Y+ps+18),true);
                    draw.AddText(ImGui.GetFont(),11,new(at.X+Math.Max(0,(layout.NodeWidth-textWidth)/2),at.Y+ps+2),current?new ImColor8(255,207,74):discovered?new ImColor8(229,236,247):new ImColor8(136,154,180),name);
                    draw.PopClipRect();
                }
                if(odds.TryGetValue(formId,out int chance))
                {
                    var p=new float2(at.X+layout.NodeWidth-25,at.Y-4);draw.AddRectFilled(p,new(p.X+29,p.Y+15),new ImColor8(29,20,6),7);draw.AddRect(p,new(p.X+29,p.Y+15),new ImColor8(255,211,106),7);
                    draw.AddText(ImGui.GetFont(),10,new(p.X+3,p.Y+1),new ImColor8(255,211,106),$"{chance}%");
                }
                if(ImGui.IsMouseHoveringRect(at,new(at.X+layout.NodeWidth,at.Y+ps+label)))
                {ImGui.BeginTooltip();UiText.Wrapped(discovered?$"{form.Name} ({form.Level})":$"Not discovered yet ({form.Level})");ImGui.EndTooltip();}
            }
        }
        finally{ImGui.EndChild();ImGui.PopStyleColor();}
    }
}
