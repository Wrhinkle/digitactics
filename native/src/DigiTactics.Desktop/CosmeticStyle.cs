using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

/// <summary>
/// Collection, Store, Loot, Farm and Codex drawing primitives from the original style.css and js/theme.js:
/// the --lc-*/--c-* palette, CSS font sizes with letter-spacing, rarity badges, Bits gems and price tags.
/// ImGui sizes fonts by ascent-descent height while CSS sizes them by the em square, so CSS pixel sizes
/// are converted with each typeface's vertical metrics (Chakra Petch 1.3, Inter 1.21).
/// </summary>
internal static class CosmeticStyle
{
    internal static readonly ImColor8 Gold1=new(240,230,210),Gold2=new(200,170,110),Gold3=new(200,155,60),Gold4=new(120,90,40),Gold5=new(70,55,20);
    internal static readonly ImColor8 LcText=new(205,190,145),LcMute=new(160,155,140),Navy0=new(1,10,19);
    internal static readonly ImColor8 Text1=new(238,242,250),Text2=new(167,178,199),Text3=new(116,129,154),TextInv=new(20,13,5);
    internal static readonly ImColor8 Accent=new(63,214,255),Success=new(79,224,143),Bits=new(79,200,255),Brand=new(255,138,61),Brand2=new(255,194,74);
    internal static readonly ImColor8 Border1=new(34,48,73);

    internal readonly record struct Type(ImFontPtr Font,float Size,float Tracking,float Ratio)
    {
        public float Width(string text)
        {
            if(Tracking==0)return Font.CalcTextSizeA(Size,float.MaxValue,0,text).X;
            float width=0;int count=0;
            foreach(var rune in text.EnumerateRunes()){width+=Font.CalcTextSizeA(Size,float.MaxValue,0,rune.ToString()).X;count++;}
            return width+Tracking*Math.Max(0,count-1);
        }
        public float Height=>Size;
    }

    /// <summary>Chakra Petch at a CSS pixel size, tracking in em (CSS letter-spacing).</summary>
    internal static Type Display(float css,float em=0,bool bold=true)=>new(bold?NativeUiTheme.DisplayBoldFont:NativeUiTheme.DisplayFont,css*1.3f,css*em,1.3f);
    /// <summary>Inter at a CSS pixel size; weights of 700 and above use the static bold face.</summary>
    internal static Type Body(float css,bool bold=false,float em=0)=>new(bold?NativeUiTheme.BodyBoldFont:NativeUiTheme.BodyFont,css*1.21f,css*em,1.21f);

    internal static float Text(ImDrawListPtr draw,float2 at,ImColor8 color,string text,Type type)
    {
        if(type.Tracking==0){draw.AddText(type.Font,type.Size,at,color,text);return type.Width(text);}
        float x=at.X;
        foreach(var rune in text.EnumerateRunes())
        {
            string glyph=rune.ToString();draw.AddText(type.Font,type.Size,new(x,at.Y),color,glyph);
            x+=type.Font.CalcTextSizeA(type.Size,float.MaxValue,0,glyph).X+type.Tracking;
        }
        return x-type.Tracking-at.X;
    }
    /// <summary>Draws text truncated with an ellipsis to fit the width (CSS text-overflow: ellipsis).</summary>
    internal static float TextFit(ImDrawListPtr draw,float2 at,ImColor8 color,string text,Type type,float width)=>Text(draw,at,color,Fit(text,type,width),type);
    internal static string Fit(string text,Type type,float width)
    {
        if(type.Width(text)<=width)return text;
        while(text.Length>0&&type.Width(text+"…")>width)text=text[..^1];
        return text.TrimEnd()+"…";
    }
    /// <summary>Text vertically centred on a line box of the given height.</summary>
    internal static float TextIn(ImDrawListPtr draw,float2 at,float lineHeight,ImColor8 color,string text,Type type)=>Text(draw,new(at.X,at.Y+(lineHeight-type.Size)*.5f),color,text,type);
    internal static List<string> Wrap(string text,Type type,float width)
    {
        var lines=new List<string>();
        foreach(string paragraph in text.Split('\n'))
        {
            string line="";
            foreach(string word in paragraph.Split(' ',StringSplitOptions.RemoveEmptyEntries))
            {
                string next=line.Length==0?word:line+" "+word;
                if(line.Length>0&&type.Width(next)>width){lines.Add(line);line=word;}else line=next;
            }
            lines.Add(line);
        }
        return lines;
    }
    /// <summary>Wrapped paragraph; returns its height. lineHeight is the CSS line-height multiple.</summary>
    internal static float Paragraph(ImDrawListPtr draw,float2 at,ImColor8 color,string text,Type type,float width,float lineHeight=1.45f,int maxLines=int.MaxValue)
    {
        float step=type.Size/type.Ratio*lineHeight;
        var lines=Wrap(text,type,width);int count=Math.Min(maxLines,lines.Count);
        for(int i=0;i<count;i++)
        {
            string line=i==count-1&&count<lines.Count?Fit(lines[i]+" …",type,width):lines[i];
            Text(draw,new(at.X,at.Y+i*step+(step-type.Size)*.5f),color,line,type);
        }
        return count*step;
    }
    internal static float ParagraphHeight(string text,Type type,float width,float lineHeight=1.45f)
    {
        float step=type.Size/type.Ratio*lineHeight;
        return Wrap(text,type,width).Count*step;
    }

    internal static ImColor8 Rarity(string? rarity)=>rarity switch
    {
        "uncommon"=>new(95,211,106),"rare"=>new(79,179,255),"epic"=>new(199,125,255),"legendary"=>new(255,179,71),_=>new(154,166,187)
    };
    internal static string RarityName(string? rarity)=>rarity is "uncommon" or "rare" or "epic" or "legendary"?rarity:"common";
    /// <summary>CSS color-mix(in srgb, a p%, b).</summary>
    internal static ImColor8 Mix(ImColor8 a,float amount,ImColor8 b)
    {
        var x=a.AsByte4();var y=b.AsByte4();
        byte L(byte p,byte q)=>(byte)Math.Round(p*amount+q*(1-amount));
        return new(L(x.X,y.X),L(x.Y,y.Y),L(x.Z,y.Z),L(x.W,y.W));
    }
    internal static ImColor8 Alpha(ImColor8 color,float alpha){var c=color.AsByte4();return new(c.X,c.Y,c.Z,(byte)Math.Round(c.W*Math.Clamp(alpha,0,1)));}
    internal static ImColor8 Faded(ImColor8 color)=>Alpha(color,ImGui.GetStyle().Alpha);

    /// <summary>The rotated square used before rarity names (.ccard-meta i, .odd i).</summary>
    internal static void Diamond(ImDrawListPtr draw,float2 center,float side,ImColor8 color)
    {
        float r=side*.7071f;draw.AddQuadFilled(center+new float2(0,-r),center+new float2(r,0),center+new float2(0,r),center+new float2(-r,0),color);
    }
    /// <summary>The .bit Bits gem: an 11px rounded square at 45 degrees with a light-to-deep-blue gradient and soft glow.</summary>
    internal static void BitsGem(ImDrawListPtr draw,float2 center,float side=11)
    {
        float r=side*.7071f;float alpha=ImGui.GetStyle().Alpha;
        draw.AddCircleFilled(center,r+1.5f,Alpha(new ImColor8(79,200,255),.08f*alpha),16);
        int first=draw.VtxBuffer.Count;
        draw.AddQuadFilled(center+new float2(0,-r),center+new float2(r,0),center+new float2(0,r),center+new float2(-r,0),new ImColor8(255,255,255,(byte)(255*alpha)));
        VerticalGradient(draw,first,center.Y-r,center.Y+r,new(200,253,255),Bits,.55f,new(27,95,208));
    }
    /// <summary>Recolours vertices added since <paramref name="first"/> along y, keeping each vertex's alpha.</summary>
    internal static unsafe void VerticalGradient(ImDrawListPtr draw,int first,float top,float bottom,ImColor8 start,ImColor8 middle,float at,ImColor8 end)
    {
        var a=start.AsByte4();var m=middle.AsByte4();var b=end.AsByte4();
        for(int i=first;i<draw.VtxBuffer.Count;i++)
        {
            ref var vertex=ref draw.VtxBuffer.Data[i];
            float t=Math.Clamp((vertex.pos.Y-top)/Math.Max(1,bottom-top),0,1);
            float r,g,bl;
            if(t<at){float u=t/at;r=a.X+(m.X-a.X)*u;g=a.Y+(m.Y-a.Y)*u;bl=a.Z+(m.Z-a.Z)*u;}
            else {float u=(t-at)/Math.Max(.001f,1-at);r=m.X+(b.X-m.X)*u;g=m.Y+(b.Y-m.Y)*u;bl=m.Z+(b.Z-m.Z)*u;}
            vertex.col=(vertex.col&0xFF000000)|((uint)Math.Round(bl)<<16)|((uint)Math.Round(g)<<8)|(uint)Math.Round(r);
        }
    }
    /// <summary>CSS filter: grayscale(g) brightness(b) applied to vertex colours drawn since <paramref name="first"/>.</summary>
    internal static unsafe void Filter(ImDrawListPtr draw,int first,float grayscale,float brightness)
    {
        for(int i=first;i<draw.VtxBuffer.Count;i++)
        {
            ref var vertex=ref draw.VtxBuffer.Data[i];uint c=vertex.col;
            float r=c&255,g=(c>>8)&255,b=(c>>16)&255,l=r*.2126f+g*.7152f+b*.0722f;
            r=(r+(l-r)*grayscale)*brightness;g=(g+(l-g)*grayscale)*brightness;b=(b+(l-b)*grayscale)*brightness;
            vertex.col=(c&0xFF000000)|((uint)Math.Clamp(Math.Round(b),0,255)<<16)|((uint)Math.Clamp(Math.Round(g),0,255)<<8)|(uint)Math.Clamp(Math.Round(r),0,255);
        }
    }

    /// <summary>.badge: 10.5px weight-800 uppercase with .08em tracking, 2px 8px padding, square frame. Returns its width.</summary>
    internal static float Badge(ImDrawListPtr draw,float2 at,string text,ImColor8 ink,ImColor8 fill,ImColor8 border,float height=19)
    {
        var type=Body(10.5f,true,.08f);string label=text.ToUpperInvariant();float width=type.Width(label)+16;
        draw.AddRectFilled(at,at+new float2(width,height),Faded(fill));draw.AddRect(at,at+new float2(width,height),Faded(border));
        TextIn(draw,at+new float2(8,0),height,Faded(ink),label,type);return width;
    }
    internal static float BadgeWidth(string text)=>Body(10.5f,true,.08f).Width(text.ToUpperInvariant())+16;
    internal static float RarityBadge(ImDrawListPtr draw,float2 at,string? rarity,float height=19)
    {
        var color=Rarity(rarity);return Badge(draw,at,RarityName(rarity),color,Mix(color,.14f,new(1,10,19,217)),Mix(color,.55f,new(color.AsByte4().X,color.AsByte4().Y,color.AsByte4().Z,0)),height);
    }
    internal static float EquippedBadge(ImDrawListPtr draw,float2 at,float height=19)=>Badge(draw,at,"Equipped",TextInv,Gold2,Gold1,height);
    internal static float OwnedBadge(ImDrawListPtr draw,float2 at,float height=19)=>Badge(draw,at,"Owned",Success,new(79,224,143,26),new(79,224,143,115),height);
    internal static float LockBadge(ImDrawListPtr draw,float2 at,string text,float height=19)=>Badge(draw,at,text,Text3,new(1,10,19,217),Border1,height);

    /// <summary>.progress: navy track with gold-5 frame and the cyan gradient fill with glow.</summary>
    internal static void Progress(ImDrawListPtr draw,float2 at,float2 size,float fraction)
    {
        draw.AddRectFilled(at,at+size,Faded(new ImColor8(1,10,19,217)));
        float fill=Math.Clamp(fraction,0,1)*(size.X-2);
        if(fill>0)
        {
            draw.AddRectFilled(at+new float2(1,-2),at+new float2(1+fill,size.Y+2),Faded(new ImColor8(63,214,255,30)));
            draw.AddRectFilledMultiColor(at+new float2(1,1),at+new float2(1+fill,size.Y-1),Faded(new(10,111,143)),Faded(Accent),Faded(Accent),Faded(new(10,111,143)));
        }
        draw.AddRect(at,at+size,Faded(Gold5));
    }
    /// <summary>.price: optional struck-through original, then a Bits gem and the weight-800 amount.</summary>
    internal static float Price(ImDrawListPtr draw,float2 at,float lineHeight,int price,int? original,ImColor8 ink,float css=16,bool measureOnly=false)
    {
        var strike=Body(12.5f);var amount=Body(css,true);float x=at.X;
        if(original is {} was && was!=price)
        {
            string text=was.ToString();float w=strike.Width(text);
            if(!measureOnly){TextIn(draw,new(x,at.Y),lineHeight,Faded(Text3),text,strike);draw.AddLine(new(x,at.Y+lineHeight*.5f+1),new(x+w,at.Y+lineHeight*.5f+1),Faded(Text3));}
            x+=w+8;
        }
        if(!measureOnly)BitsGem(draw,new(x+7,at.Y+lineHeight*.5f));
        x+=17;string value=price.ToString();
        if(!measureOnly)TextIn(draw,new(x,at.Y),lineHeight,Faded(ink),value,amount);
        return x+amount.Width(value)-at.X;
    }
    /// <summary>The .lc-cta hextech call to action: navy-teal fill, 2px gold gradient frame, cyan inner glow.</summary>
    internal static void Cta(ImDrawListPtr draw,float2 lo,float2 hi,bool hover)
    {
        var top=hover?new ImColor8(15,82,102):new ImColor8(11,58,74);var bottom=hover?new ImColor8(8,48,74):new ImColor8(6,32,51);
        draw.AddRectFilledMultiColor(lo,hi,Faded(top),Faded(top),Faded(bottom),Faded(bottom));
        float glow=hover?.45f:.25f;
        for(int i=0;i<6;i++)draw.AddRect(lo+new float2(2+i,2+i),hi-new float2(2+i,2+i),Faded(Alpha(Accent,glow*(1-i/6f)*.35f)));
        var frameTop=hover?Gold1:Gold2;var frameBottom=hover?Gold2:Gold4;
        int first=draw.VtxBuffer.Count;draw.AddRect(lo+new float2(1,1),hi-new float2(1,1),new ImColor8(255,255,255,(byte)(255*ImGui.GetStyle().Alpha)),thickness:2);
        VerticalGradient(draw,first,lo.Y,hi.Y,frameTop,Mix(frameTop,.5f,frameBottom),.5f,frameBottom);
    }
    /// <summary>The default theme.js button: #1e2328 to #141a22 with a gold-4 frame, gold-2 on hover.</summary>
    internal static void Secondary(ImDrawListPtr draw,float2 lo,float2 hi,bool hover)
    {
        var top=hover?new ImColor8(38,45,54):new ImColor8(30,35,40);var bottom=hover?new ImColor8(26,33,43):new ImColor8(20,26,34);
        draw.AddRectFilledMultiColor(lo,hi,Faded(top),Faded(top),Faded(bottom),Faded(bottom));draw.AddRect(lo,hi,Faded(hover?Gold2:Gold4));
    }
}
