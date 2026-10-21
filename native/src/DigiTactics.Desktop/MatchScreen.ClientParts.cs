using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

/// <summary>theme.js building blocks shared by the client pages: layers, tiles, eyebrows, badges, paragraphs, progress bars, tile links.</summary>
internal sealed partial class MatchScreen
{
    /// <summary>A borderless, transparent, unpadded window for pages that paint their own surfaces.</summary>
    private void ClientLayer(string id,float2 at,float2 extent,bool scroll=false,bool bringToFront=false)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(0,0));ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize,0);
        Window(id,at.X,at.Y,extent.X,extent.Y,ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoBackground|(bringToFront?ImGuiWindowFlags.None:ImGuiWindowFlags.NoBringToFrontOnFocus)
            |(scroll?ImGuiWindowFlags.None:ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse));
        ImGui.PopStyleVar(2);
    }

    /// <summary>.tile-card / .box surface: navy gradient, gold-5 frame (gold-4 when hovered).</summary>
    private static void PaintTile(ImDrawListPtr draw,float2 lo,float2 hi,bool hovered=false,bool box=false)
    {
        // .box: box-shadow var(--sh-2) = 0 6px 18px rgba(0,0,0,.45).
        if(box)NativeUiPaint.BoxGlow(draw,lo+new float2(0,6),hi+new float2(0,6),new ImColor8(0,0,0,115),18);
        NativeUiPaint.VerticalGradient(draw,lo,hi,new ImColor8(10,20,40,(byte)(box?240:235)),new ImColor8(1,10,19,242));
        draw.AddRect(lo,hi,NativeUiPaint.Fade(hovered?NativeUiTheme.GoldBorder:NativeUiTheme.GoldDim));
        // .box::before: a gold-3 highlight fading out toward both ends of the top edge.
        if(box)NativeUiPaint.FadedLine(draw,new(lo.X-1,lo.Y-1),new(hi.X+1,lo.Y),NativeUiTheme.GoldDeep);
    }

    /// <summary>.eyebrow: Chakra Petch 700, 11px, .16em, uppercase, gold-2.</summary>
    private static float Eyebrow(ImDrawListPtr draw,float2 at,string text,ImColor8? color=null,float px=11)
    {
        string upper=text.ToUpperInvariant();
        NativeUiPaint.TrackedIn(draw,NativeUiTheme.DisplayBoldFont,px,at,NativeUiTheme.TextLine(px),color??NativeUiTheme.Gold,upper,.16f);
        return NativeUiTheme.TextLine(px);
    }

    internal enum BadgeStyle { Plain, New, Owned, Equipped }
    /// <summary>.badge: 2px 8px, 10.5px/800, .08em, uppercase, line-height 1.5. Returns the badge size.</summary>
    private static float2 Badge(ImDrawListPtr draw,float2 at,string text,BadgeStyle style=BadgeStyle.Plain)
    {
        string upper=text.ToUpperInvariant();
        float width=NativeUiPaint.TrackedWidth(NativeUiTheme.BodyExtraBoldFont,10.5f,upper,.08f)+16+2,height=10.5f*1.5f+4+2;
        var (fill,border,ink)=style switch
        {
            BadgeStyle.New=>(NativeUiTheme.Accent,new ImColor8(168,240,255),NativeUiTheme.PlayText),
            BadgeStyle.Owned=>(new ImColor8(79,224,143,26),new ImColor8(79,224,143,115),NativeUiTheme.Success),
            BadgeStyle.Equipped=>(NativeUiTheme.Gold,NativeUiTheme.GoldBright,NativeUiTheme.PlayText),
            _=>(new ImColor8(1,10,19,217),NativeUiTheme.GoldBorder,NativeUiTheme.LcText)
        };
        var hi=at+new float2(width,height);
        draw.AddRectFilled(at,hi,NativeUiPaint.Fade(fill));draw.AddRect(at,hi,NativeUiPaint.Fade(border));
        var text_=NativeUiPaint.TrackedSize(NativeUiTheme.BodyExtraBoldFont,10.5f,upper,.08f);
        NativeUiPaint.Tracked(draw,NativeUiTheme.BodyExtraBoldFont,10.5f,new(at.X+9,MathF.Round(at.Y+(height-text_.Y)/2)),ink,upper,.08f);
        return new(width,height);
    }

    /// <summary>Greedy word wrap with an optional line clamp; the last kept line ends in an ellipsis when text is cut (-webkit-line-clamp).</summary>
    private static List<string> WrapText(ImFontPtr font,float px,string text,float width,int maxLines=int.MaxValue,float tracking=0)
    {
        var lines=new List<string>();
        foreach(var paragraph in text.Split('\n'))
        {
            string line="";
            foreach(var word in paragraph.Split(' ',StringSplitOptions.RemoveEmptyEntries))
            {
                // Break opportunities: before each word, and after each hyphen inside a word.
                var parts=new List<string>();int start=0;
                for(int i=0;i<word.Length-1;i++)if(word[i]=='-'&&i>0){parts.Add(word[start..(i+1)]);start=i+1;}
                parts.Add(word[start..]);
                for(int p=0;p<parts.Count;p++)
                {
                    string next=line.Length==0?parts[p]:p==0?line+" "+parts[p]:line+parts[p];
                    if(line.Length>0&&NativeUiPaint.TrackedWidth(font,px,next,tracking)>width){lines.Add(line);line=parts[p];}
                    else line=next;
                }
            }
            lines.Add(line);
        }
        if(lines.Count>maxLines)
        {
            lines.RemoveRange(maxLines,lines.Count-maxLines);
            string last=lines[^1];
            while(last.Length>0&&NativeUiPaint.TrackedWidth(font,px,last+"…",tracking)>width)last=last[..^1];
            lines[^1]=last.TrimEnd()+"…";
        }
        return lines;
    }

    /// <summary>Draws wrapped text at a CSS line-height multiple and returns its height.</summary>
    private static float CssParagraph(ImDrawListPtr draw,ImFontPtr font,float px,float2 at,float width,ImColor8 color,string text,float lineHeight=1.45f,int maxLines=int.MaxValue,float tracking=0,ImColor8? shadow=null)
    {
        var lines=WrapText(font,px,text,width,maxLines,tracking);
        float step=px*lineHeight,inset=(step-NativeUiTheme.LineHeight(font,px))/2;
        for(int i=0;i<lines.Count;i++)
        {
            var position=new float2(at.X,MathF.Round(at.Y+i*step+inset));
            if(shadow is {} s)NativeUiPaint.Tracked(draw,font,px,position+new float2(0,1),s,lines[i],tracking);
            NativeUiPaint.Tracked(draw,font,px,position,color,lines[i],tracking);
        }
        return lines.Count*step;
    }

    /// <summary>.progress: navy track with a gold-5 frame and a glowing teal-to-cyan fill (green when done).</summary>
    private static void Progress(ImDrawListPtr draw,float2 lo,float2 hi,float fraction,bool done=false)
    {
        draw.AddRectFilled(lo,hi,NativeUiPaint.Fade(new ImColor8(1,10,19,217)));
        fraction=Math.Clamp(fraction,0,1);
        if(fraction>0)
        {
            var fillLo=lo+new float2(1,1);var fillHi=new float2(lo.X+1+(hi.X-lo.X-2)*fraction,hi.Y-1);
            NativeUiPaint.BoxGlow(draw,fillLo,fillHi,done?new ImColor8(79,224,143,128):new ImColor8(63,214,255,128),10);
            NativeUiPaint.HorizontalGradient(draw,fillLo,fillHi,done?new ImColor8(47,184,108):new ImColor8(10,111,143),done?NativeUiTheme.Success:NativeUiTheme.Accent);
        }
        draw.AddRect(lo,hi,NativeUiPaint.Fade(NativeUiTheme.GoldDim));
    }

    /// <summary>.tile-link: Chakra 700 11px .1em uppercase cyan text button with a hover glow. "›" is drawn as a chevron.</summary>
    private bool TileLink(string key,string label,float2 at)
    {
        string text=label.ToUpperInvariant();
        var size=NativeUiPaint.TrackedSize(NativeUiTheme.DisplayBoldFont,11,text,.1f);
        float width=size.X+NativeUiPaint.TrackedWidth(NativeUiTheme.DisplayBoldFont,11," ",.1f)+5;
        ImGui.SetCursorScreenPos(at);
        bool clicked=ClientHitButton(key,width,size.Y);bool hot=ImGui.IsItemHovered();
        var draw=ImGui.GetWindowDrawList();var ink=hot?NativeUiTheme.AccentInk:NativeUiTheme.Accent;
        if(hot)NativeUiPaint.TextGlow(draw,NativeUiTheme.DisplayBoldFont,11,at,text,.1f,new ImColor8(63,214,255,160),8);
        NativeUiPaint.Tracked(draw,NativeUiTheme.DisplayBoldFont,11,at,ink,text,.1f);
        var chevron=at+new float2(width-4,size.Y*.55f);var color=NativeUiPaint.Fade(ink);
        draw.AddLine(chevron+new float2(-2,-3),chevron+new float2(1,0),color,1.6f);draw.AddLine(chevron+new float2(1,0),chevron+new float2(-2,3),color,1.6f);
        return clicked;
    }

    /// <summary>.tile-h / .card-h heading row with an optional muted note on the right.</summary>
    private static void TileHeading(ImDrawListPtr draw,float2 at,float width,string title,string? note=null,float px=12.5f)
    {
        string upper=title.ToUpperInvariant();
        float noteWidth=note is null?0:NativeUiPaint.TrackedWidth(NativeUiTheme.BodyFont,11,note,0);
        float line=NativeUiTheme.TextLine(px);
        NativeUiPaint.TrackedIn(draw,NativeUiTheme.DisplayBoldFont,px,at,line,NativeUiTheme.GoldBright,SocialEllipsis(upper,NativeUiTheme.DisplayBoldFont,px,width-noteWidth-8),.1f);
        // align-items: center puts the 11px note's line box in the middle of the heading's.
        if(note is not null)NativeUiPaint.TrackedIn(draw,NativeUiTheme.BodyFont,11,new(at.X+width-noteWidth,at.Y+(line-NativeUiTheme.TextLine(11))/2),NativeUiTheme.TextLine(11),NativeUiTheme.LcMute,note);
    }
}
