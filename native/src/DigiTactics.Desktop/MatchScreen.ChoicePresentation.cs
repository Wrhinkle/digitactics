using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private readonly PostgameArtwork choiceArtwork=new("choice-art");
    internal string? DisplayedOfferTitle {get;private set;}
    internal string? DisplayedOfferDescription {get;private set;}
    internal Dictionary<int,ChoiceArt?> DisplayedChoiceArt {get;}=[];
    internal Dictionary<int,(float2 Min,float2 Max)> ChoiceCardBounds {get;}=[];
    internal (float2 Min,float2 Max) ChoiceViewportBounds {get;private set;}
    internal float ChoiceScrollMaxY {get;private set;}
    internal bool ChoiceTextFits {get;private set;}
    private readonly record struct ChoiceCardLayout(float TitleY,float DescriptionY,float DescriptionHeight,float RecipeY,float Height);
    private static ChoiceCardLayout MeasureChoice(DisplayEntry choice,bool artwork,float width,string? recipe)
    {
        float bodyWidth=width-28,titleY=artwork?98:18;
        float titleHeight=ChoiceLines(choice.Name,20,bodyWidth).Length*24;
        float descriptionY=titleY+titleHeight+8;
        float descriptionHeight=ChoiceLines(choice.Description,16,bodyWidth).Length*21;
        float recipeY=descriptionY+descriptionHeight+8;
        float bottom=recipe is null?descriptionY+descriptionHeight:recipeY+ChoiceLines(recipe,16,bodyWidth).Length*21;
        return new(titleY,descriptionY,descriptionHeight,recipeY,Math.Max(180,bottom+16));
    }
    // The original CSS centers every line of augment/carousel text, rather than
    // centering one left-aligned text block. Measure and draw the same wrapping.
    private static string[] ChoiceLines(string value,float fontSize,float width)
    {
        if(string.IsNullOrEmpty(value))return [];
        var lines=new List<string>();var font=ImGui.GetFont();
        foreach(string paragraph in value.Replace("\r","").Split('\n'))
        {
            string line="";
            foreach(string word in paragraph.Split(' ',StringSplitOptions.RemoveEmptyEntries))
            {
                string next=line.Length==0?word:line+" "+word;
                if(line.Length>0&&font.CalcTextSizeA(fontSize,float.MaxValue,0,next).X>width){lines.Add(line);line="";}
                string remainder=word;
                while(font.CalcTextSizeA(fontSize,float.MaxValue,0,remainder).X>width&&remainder.Length>1)
                {
                    int count=1;
                    while(count<remainder.Length&&font.CalcTextSizeA(fontSize,float.MaxValue,0,remainder[..(count+1)]).X<=width)count++;
                    lines.Add(remainder[..count]);remainder=remainder[count..];
                }
                line=line.Length==0?remainder:line+" "+remainder;
            }
            lines.Add(line);
        }
        return lines.ToArray();
    }
    private static void ChoiceText(ImDrawListPtr draw,float2 at,float width,string value,float fontSize,float lineHeight,ImColor8 color,bool centered)
    {
        foreach(string line in ChoiceLines(value,fontSize,width))
        {
            float x=centered?Math.Max(0,(width-ImGui.GetFont().CalcTextSizeA(fontSize,float.MaxValue,0,line).X)/2):0;
            draw.AddText(ImGui.GetFont(),fontSize,at+new float2(x,0),color,line);at.Y+=lineHeight;
        }
    }
    private static void ChoiceCenteredLabel(string value,float font=16,ImColor8? color=null)
    {
        var at=ImGui.GetCursorScreenPos();float width=ImGui.GetContentRegionAvail().X;
        float height=ChoiceLines(value,font,width).Length*(font+5);
        ChoiceText(ImGui.GetWindowDrawList(),at,width,value,font,font+5,color??HudMuted,true);
        ImGui.Dummy(new(width,height));
    }
    private void BeginChoicePanel(string name,float width,float height,bool peek=false)
    {
        float x=Math.Clamp(Left+(Width-width)/2,24,size.X-width-24);
        float y=peek?Top+10:Math.Clamp(Top+(Height-height)/2,24,size.Y-height-24);
        ImGui.PushStyleColor(ImGuiCol.WindowBg,new ImColor8(0,0,0,0));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize,0);
        Window(name,peek?x:0,peek?y:0,peek?width:size.X,peek?height:size.Y,
            ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse);
        ImGui.PopStyleVar();ImGui.PopStyleColor();
        // Source #modal belongs to the arena: the match rails, top bar and shop
        // remain bright. Keep the full window as an input barrier while a choice
        // is open; only the painted scrim is limited to the arena rectangle.
        if(!peek)ImGui.GetWindowDrawList().AddRectFilled(new(Left,Top),new(Left+Width,Top+Height),new ImColor8(5,7,12,153));
        ImGui.SetCursorScreenPos(new(x,y));
        ImGui.PushStyleColor(ImGuiCol.ChildBg,new ImColor8(0,0,0,0));
        ImGui.PushStyleColor(ImGuiCol.Border,new ImColor8(120,90,40));
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding,14);ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(18,16));
        ImGui.BeginChild((name+" panel"),new(width,height),ImGuiChildFlags.Borders,ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse);
        ImGui.PopStyleVar(2);ImGui.PopStyleColor(2);
        var draw=ImGui.GetWindowDrawList();var min=ImGui.GetWindowPos();var max=min+ImGui.GetWindowSize();
        draw.AddRectFilledMultiColor(min+new float2(1,1),max-new float2(1,1),new ImColor8(10,20,40,250),new ImColor8(10,20,40,250),new ImColor8(1,10,19,250),new ImColor8(1,10,19,250));
        draw.AddRect(min,max,new ImColor8(120,90,40),14);
    }
    private static void EndChoicePanel(){ImGui.EndChild();ImGui.End();}
    private static void ChoiceCardSurface(ImDrawListPtr draw,float2 min,float2 max,ImColor8 border,bool hovered)
    {
        draw.AddRectFilledMultiColor(min,max,hovered?new ImColor8(28,39,62):new ImColor8(21,30,49),hovered?new ImColor8(28,39,62):new ImColor8(21,30,49),new ImColor8(15,22,37),new ImColor8(15,22,37));
        draw.AddRect(min+new float2(1,1),max-new float2(1,1),border,12,thickness:2);
    }
    private static string ChoiceTierName(int tier)=>tier switch{1=>"Silver",2=>"Gold",3=>"Prismatic",_=>"Silver"};
    private static ImColor8 ChoiceTierColor(int tier)=>tier switch{2=>new(255,207,74),3=>new(229,139,255),_=>new(201,209,220)};
    private DisplayEntry LiveOfferHeading(PlayerOffer offer)
    {
        var heading=text.Offer(Rules.Id,offer.Kind);
        if(offer.Kind=="augment")
        {
            int tier=offer.Options.Length>0?ChoiceArtwork.Resolve(Rules.Id,"augment",offer.Options[0])?.Tier??1:1;
            return heading with{Name=ChoiceTierName(tier)+" Augment",Description="Pick one augment. It lasts the whole game."};
        }
        if(offer.Kind=="power")return heading with{Description="Pick your Tamer power. It ranks up later in the game."};
        if(offer.Kind is "armory" or "itemTier")return heading with{Description="Pick one item. It goes to your item bench."};
        if(offer.Kind=="upgrade"&&Session!.State.GizmosGates is {Length:>0} gates)
            return heading with{Description=heading.Description.TrimEnd()+" Gates: "+string.Join(", ",gates.Select(ChoiceTierName))+"."};
        return heading;
    }
}
