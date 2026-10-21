using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>
/// The original #modal: .mpanel (navy gradient, gold-4 frame, 14px radius, 16px 18px padding),
/// a Chakra Petch h2 with the theme.js button beside it, .msub, and .choice cards in Inter.
/// </summary>
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

    private static readonly Rgb ModalGold4=HudHex("785a28"),ModalGold2=HudHex("c8aa6e"),ModalGold1=HudHex("f0e6d2"),ModalButtonInk=HudHex("cdbe91");
    private const float ModalPadX=18,ModalPadY=16,ModalCardGap=12,ModalLine=1.35f;
    private float ModalMaxWidth=>Math.Min(860,Width-24);

    // The CSS line breaker for the modal's Inter text: words wrap, long words break.
    private static string[] ModalLines(HudFace face,float css,string value,float width)
    {
        if(string.IsNullOrEmpty(value))return [];
        var lines=new List<string>();
        foreach(string paragraph in value.Replace("\r","").Split('\n'))
        {
            string line="";
            foreach(string word in paragraph.Split(' ',StringSplitOptions.RemoveEmptyEntries))
            {
                string next=line.Length==0?word:line+" "+word;
                if(line.Length>0&&HudWidth(face,css,next)>width){lines.Add(line);line="";}
                string remainder=word;
                while(HudWidth(face,css,remainder)>width&&remainder.Length>1)
                {
                    int count=1;
                    while(count<remainder.Length&&HudWidth(face,css,remainder[..(count+1)])<=width)count++;
                    lines.Add(remainder[..count]);remainder=remainder[count..];
                }
                line=line.Length==0?remainder:line+" "+remainder;
            }
            lines.Add(line);
        }
        return lines.ToArray();
    }
    private static float ModalText(ImDrawListPtr draw,HudFace face,float css,float x,float y,float width,string value,ImColor8 color,bool centered)
    {
        float line=css*ModalLine;
        foreach(string text in ModalLines(face,css,value,width))
        {
            float offset=centered?Math.Max(0,(width-HudWidth(face,css,text))/2):0;
            HudRun(draw,face,css,x+offset,y,line,text,color);y+=line;
        }
        return y;
    }
    private static float ModalTextHeight(HudFace face,float css,string value,float width)=>ModalLines(face,css,value,width).Length*css*ModalLine;

    // The original CSS centers every line of augment/carousel text. GalaxyIntro still uses these.
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

    /// <summary>Opens the arena-centred .mpanel. The window covers the screen as an input barrier; the scrim covers only the arena.</summary>
    private void BeginChoicePanel(string name,float width,float height,bool peek=false)
    {
        float x=Math.Clamp(Left+(Width-width)/2,12,size.X-width-12);
        float y=peek?Top+10:Math.Clamp(Top+(Height-height)/2,Top+10,Math.Max(Top+10,size.Y-height-12));
        ImGui.PushStyleColor(ImGuiCol.WindowBg,new ImColor8(0,0,0,0));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize,0);
        Window(name,peek?x:0,peek?y:0,peek?width:size.X,peek?height:size.Y,
            ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse);
        ImGui.PopStyleVar();ImGui.PopStyleColor();
        if(!peek)ImGui.GetWindowDrawList().AddRectFilled(new(Left,Top),new(Left+Width,Top+Height),new ImColor8(5,7,12,153));
        ImGui.SetCursorScreenPos(new(x,y));
        ImGui.PushStyleColor(ImGuiCol.ChildBg,new ImColor8(0,0,0,0));
        ImGui.PushStyleColor(ImGuiCol.Border,new ImColor8(0,0,0,0));
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding,14);ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(ModalPadX+1,ModalPadY+1));
        ImGui.BeginChild(name+" panel",new(width,height),ImGuiChildFlags.Borders,ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse);
        ImGui.PopStyleVar(2);ImGui.PopStyleColor(2);
        var draw=ImGui.GetWindowDrawList();var min=ImGui.GetWindowPos();var max=min+ImGui.GetWindowSize();
        draw.PushClipRect(min-new float2(24,24),max+new float2(24,24));
        // box-shadow: var(--sh-3) = 0 18px 50px rgba(0,0,0,.6)
        for(int i=1;i<=6;i++)draw.AddRectFilled(min+new float2(-i*2,18-i*2+i),max+new float2(i*2,i*3),new ImColor8(0,0,0,(byte)(26-i*3)),14+i*2);
        HudVerticalGradient(draw,min,max,HudHex("0a1428"),HudHex("010a13"),14,.98f);
        draw.AddRect(min,max,ModalGold4,14);
        draw.PopClipRect();
    }
    private static void EndChoicePanel(){ImGui.EndChild();ImGui.End();}

    /// <summary>theme.js #modal button: Inter 600, gold-4 frame, 2px radius; .primary is the orange-gold gradient.</summary>
    private bool ModalButton(string key,string label,float2 at,bool primary=false,float minWidth=0)
    {
        var face=ModalWeight(primary?700:600);
        float width=Math.Max(minWidth,HudWidth(face,14,label)+26),height=MathF.Round(14*ModalLine+12+2);
        ImGui.SetCursorScreenPos(at);
        bool pressed=ImGui.InvisibleButton(key,new(width,height));
        bool hovered=ImGui.IsItemHovered();
        var max=at+new float2(width,height);Buttons[key]=(at+max)*.5f;
        if(ImGui.IsItemFocused())KeyboardFocusedButton=key;
        ClientButtonSound(key,pressed);
        var draw=ImGui.GetWindowDrawList();
        if(primary)
        {
            float mid=at.Y+height*.3f;
            draw.AddRectFilledMultiColor(at,new(max.X,mid),(ImColor8)HudHex("ffd27a"),(ImColor8)HudHex("ffd27a"),(ImColor8)HudHex("ffc24a"),(ImColor8)HudHex("ffc24a"));
            draw.AddRectFilledMultiColor(new(at.X,mid),max,(ImColor8)HudHex("ffc24a"),(ImColor8)HudHex("ffc24a"),(ImColor8)HudHex("ff8a3d"),(ImColor8)HudHex("ff8a3d"));
            draw.AddRect(at,max,HudHex("f0d28c"),2);
            HudRunCentered(draw,face,14,(at.X+max.X)/2,at.Y+1,height-2,label,HudHex("140d05"));
        }
        else
        {
            var top=hovered?HudHex("262d36"):HudHex("1e2328");var bottom=hovered?HudHex("1a212b"):HudHex("141a22");
            draw.AddRectFilledMultiColor(at,max,(ImColor8)top,(ImColor8)top,(ImColor8)bottom,(ImColor8)bottom);
            draw.AddRect(at,max,hovered?ModalGold2:ModalGold4,2);
            HudRunCentered(draw,face,14,(at.X+max.X)/2,at.Y+1,height-2,label,hovered?ModalGold1:ModalButtonInk);
        }
        return pressed;
    }
    private float ModalButtonWidth(string label,bool primary=false)=>HudWidth(ModalWeight(primary?700:600),14,label)+26;

    /// <summary>#modal .choice: surface gradient, 2px border, 12px radius; hover lifts 2px, brightens and turns the border gold.</summary>
    private static void ChoiceCardSurface(ImDrawListPtr draw,float2 min,float2 max,Rgb border,bool hovered,float alpha=1)
    {
        var under=HudHex("0b1626");
        Rgb Paint(Rgb c)=>HudOver(hovered?HudBright(c):c,under,alpha);
        draw.AddRectFilled(min,max,Paint(hovered?ModalGold2:border),12);
        HudVerticalGradient(draw,min+new float2(2,2),max-new float2(2,2),Paint(HudHex("151e31")),Paint(HudHex("0f1625")),10);
    }
    private static string ChoiceTierName(int tier)=>tier switch{1=>"Silver",2=>"Gold",3=>"Prismatic",_=>"Silver"};
    // DT.AUG_TIER_COLOR
    private static Rgb ChoiceTierColor(int tier)=>tier switch{2=>HudHex("ffcf4a"),3=>HudHex("e58bff"),_=>HudHex("c9d1dc")};
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
