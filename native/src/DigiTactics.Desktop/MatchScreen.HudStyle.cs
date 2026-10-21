using System.Text;
using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

/// <summary>
/// The original match HUD's type and box metrics. style.css sets the top bar, rails and shop
/// in the system UI face (Segoe UI on Windows, 14px/1.35); theme.js switches #modal, #tooltip
/// and #banner to Inter with Chakra Petch headings. CSS sizes are em sizes, while ImGui sizes a
/// face by its ascent-to-descent height, so every face carries its own conversion factor.
/// </summary>
internal sealed partial class MatchScreen
{
    // Ascent and Descent are hhea metrics in em; Factor (their sum) converts a CSS em size to ImGui's pixel height.
    private readonly record struct HudFace(ImFontPtr Font,float Ascent,float Descent)
    {
        public float Factor=>Ascent+Descent;
    }
    private static HudFace hudRegular,hudSemibold,hudBold,hudBlack,modalRegular,modalBold,modalDisplay;
    private static bool hudFontsLoaded;

    internal static void LoadHudFonts()
    {
        string system=Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
        string bundled=Path.Combine(AppContext.BaseDirectory,"content","fonts");
        modalRegular=Face(NativeUiTheme.BodyFont,Path.Combine(bundled,"inter","Inter[opsz,wght].ttf"));
        modalBold=Face(NativeUiTheme.BodyBoldFont,Path.Combine(bundled,"inter","Inter-Bold.ttf"));
        modalDisplay=Face(NativeUiTheme.DisplayBoldFont,Path.Combine(bundled,"chakrapetch","ChakraPetch-Bold.ttf"));
        HudFace System(string file,HudFace fallback)
        {
            string path=Path.Combine(system,file);
            if(!File.Exists(path))return fallback;
            var font=AddHudFont(path);
            return font.IsNull()?fallback:Face(font,path);
        }
        // Chromium resolves CSS weight 800 to Segoe UI Black, the next heavier installed face.
        hudRegular=System("segoeui.ttf",modalRegular);
        hudSemibold=System("seguisb.ttf",modalBold);
        hudBold=System("segoeuib.ttf",modalBold);
        hudBlack=System("seguibl.ttf",modalBold);
        hudFontsLoaded=true;
    }

    // Chromium draws this text with ClearType; a heavier rasterizer keeps grayscale ImGui text as bright and crisp.
    private static unsafe ImFontPtr AddHudFont(string path)
    {
        var config=new ImFontConfig{FontDataOwnedByAtlas=true,GlyphMaxAdvanceX=float.MaxValue,RasterizerMultiply=1.3f,RasterizerDensity=1};
        return ImGui.GetIO().Fonts.AddFontFromFileTTF(path,NativeUiTheme.BodySize,new ImFontConfigPtr(&config),default);
    }
    // hhea ascender and descender over unitsPerEm: stb_truetype sizes a face by their sum.
    private static HudFace Face(ImFontPtr font,string path)
    {
        try
        {
            byte[] data=File.ReadAllBytes(path);
            static int U16(byte[] b,int at)=>b[at]<<8|b[at+1];
            static int U32(byte[] b,int at)=>b[at]<<24|b[at+1]<<16|b[at+2]<<8|b[at+3];
            int head=-1,hhea=-1;
            for(int i=0,count=U16(data,4);i<count;i++)
            {
                int record=12+16*i;string tag=Encoding.ASCII.GetString(data,record,4);
                if(tag=="head")head=U32(data,record+8);else if(tag=="hhea")hhea=U32(data,record+8);
            }
            int unitsPerEm=head<0?0:U16(data,head+18);
            if(hhea<0||unitsPerEm==0)return new(font,.97f,.24f);
            return new(font,(short)U16(data,hhea+4)/(float)unitsPerEm,-(short)U16(data,hhea+6)/(float)unitsPerEm);
        }
        catch(IOException){return new(font,.97f,.24f);}
    }

    private static HudFace HudWeight(int weight)
    {
        if(!hudFontsLoaded)return new(NativeUiTheme.BodyFont,.97f,.24f);
        return weight>=800?hudBlack:weight>=700?hudBold:weight>=600?hudSemibold:hudRegular;
    }
    private static HudFace ModalWeight(int weight)
    {
        if(!hudFontsLoaded)return new(weight>=600?NativeUiTheme.BodyBoldFont:NativeUiTheme.BodyFont,.97f,.24f);
        return weight>=600?modalBold:modalRegular;
    }
    private static HudFace ModalDisplay=>hudFontsLoaded?modalDisplay:new(NativeUiTheme.DisplayBoldFont,1f,.3f);

    private static float HudWidth(HudFace face,float css,string value,float spacing=0)
        =>value.Length==0?0:face.Font.CalcTextSizeA(css*face.Factor,float.MaxValue,0,value).X+spacing*value.Length;

    private static float HudBaseline(HudFace face,float css,float lineTop,float lineHeight)
    {
        float ascent=MathF.Round(css*face.Ascent),descent=MathF.Round(css*face.Descent);
        return MathF.Floor(lineTop+(lineHeight-ascent-descent)/2+ascent);
    }
    /// <summary>Draws one CSS text run whose line box starts at <paramref name="lineTop"/>.</summary>
    private static float HudRun(ImDrawListPtr draw,HudFace face,float css,float x,float lineTop,float lineHeight,string value,ImColor8 color,float maxWidth=0,float spacing=0)
    {
        // Chromium rounds ascent and descent to whole pixels and centres them in the line box;
        // ImGui snaps the run to a pixel and rounds its baked ascent up.
        float size=css*face.Factor,y=HudBaseline(face,css,lineTop,lineHeight)-MathF.Ceiling(css*face.Ascent);
        if(maxWidth>0&&HudWidth(face,css,value,spacing)>maxWidth)
        {
            const string ellipsis="…";
            while(value.Length>0&&HudWidth(face,css,value+ellipsis,spacing)>maxWidth)value=value[..^1];
            value+=ellipsis;
        }
        if(spacing==0){draw.AddText(face.Font,size,new(x,y),color,value);return HudWidth(face,css,value);}
        float start=x;
        foreach(var rune in value.EnumerateRunes())
        {
            string glyph=rune.ToString();draw.AddText(face.Font,size,new(x,y),color,glyph);
            x+=face.Font.CalcTextSizeA(size,float.MaxValue,0,glyph).X+spacing;
        }
        return x-start;
    }
    private static float HudRunCentered(ImDrawListPtr draw,HudFace face,float css,float centerX,float lineTop,float lineHeight,string value,ImColor8 color,float spacing=0)
        =>HudRun(draw,face,css,centerX-HudWidth(face,css,value,spacing)/2,lineTop,lineHeight,value,color,0,spacing);
    private static float HudRunRight(ImDrawListPtr draw,HudFace face,float css,float right,float lineTop,float lineHeight,string value,ImColor8 color,float spacing=0)
        =>HudRun(draw,face,css,right-HudWidth(face,css,value,spacing),lineTop,lineHeight,value,color,0,spacing);

    /// <summary>An opaque CSS colour; blends are computed here because ImColor8 exposes no channels.</summary>
    private readonly record struct Rgb(byte R,byte G,byte B)
    {
        public static implicit operator ImColor8(Rgb c)=>new(c.R,c.G,c.B);
    }
    private static ImColor8 HudAlpha(Rgb color,float alpha)=>new(color.R,color.G,color.B,(byte)Math.Clamp(MathF.Round(255*alpha),0,255));
    private static Rgb HudHex(string hex)
    {
        hex=hex.TrimStart('#');
        if(hex.Length==3)hex=string.Concat(hex.Select(c=>new string(c,2)));
        return new(Convert.ToByte(hex[..2],16),Convert.ToByte(hex[2..4],16),Convert.ToByte(hex[4..6],16));
    }
    // A translucent colour over an opaque one, as CSS opacity composites a whole element.
    private static Rgb HudOver(Rgb top,Rgb bottom,float alpha)
        =>new((byte)MathF.Round(top.R*alpha+bottom.R*(1-alpha)),(byte)MathF.Round(top.G*alpha+bottom.G*(1-alpha)),(byte)MathF.Round(top.B*alpha+bottom.B*(1-alpha)));
    private static Rgb HudMix(Rgb a,Rgb b,float amountOfA)=>HudOver(a,b,amountOfA);

    // Chromium snaps each laid-out box to whole pixels.
    private static float2 HudSnap(float2 point)=>new(MathF.Round(point.X),MathF.Round(point.Y));
    private static Rgb HudCss(string color)=>color.Length==7&&color[0]=='#'?HudHex(color):new(142,152,173);
    // filter: grayscale(.7) brightness(.7), the unaffordable shop card.
    private static Rgb HudPoor(Rgb color)
    {
        float gray=.2126f*color.R+.7152f*color.G+.0722f*color.B;
        static byte Channel(float value,float gray)=>(byte)Math.Clamp(MathF.Round((value*.3f+gray*.7f)*.7f),0,255);
        return new(Channel(color.R,gray),Channel(color.G,gray),Channel(color.B,gray));
    }
    // filter: brightness(1.15), the hovered card.
    private static Rgb HudBright(Rgb color,float amount=1.15f)
        =>new((byte)Math.Min(255,MathF.Round(color.R*amount)),(byte)Math.Min(255,MathF.Round(color.G*amount)),(byte)Math.Min(255,MathF.Round(color.B*amount)));

    /// <summary>text-shadow: 0 1px 2px #000 under a run.</summary>
    private static float HudShadowRun(ImDrawListPtr draw,HudFace face,float css,float x,float lineTop,float lineHeight,string value,ImColor8 color,float maxWidth=0,float spacing=0,float shadow=.75f)
    {
        HudRun(draw,face,css,x,lineTop+1,lineHeight,value,new ImColor8(0,0,0,(byte)(255*shadow*.55f)),maxWidth,spacing);
        HudRun(draw,face,css,x+.5f,lineTop+1.5f,lineHeight,value,new ImColor8(0,0,0,(byte)(255*shadow*.35f)),maxWidth,spacing);
        return HudRun(draw,face,css,x,lineTop,lineHeight,value,color,maxWidth,spacing);
    }

    /// <summary>A CSS radial-gradient from <paramref name="color"/> at <paramref name="alpha"/> to transparent at <paramref name="radius"/>.</summary>
    private static void HudRadial(ImDrawListPtr draw,float2 center,float radius,Rgb color,float alpha,int rings=18)
    {
        // Nested discs whose composite opacity falls linearly from the centre to the rim.
        float previous=0;
        for(int i=0;i<rings;i++)
        {
            float target=alpha*(i+1)/rings;
            float step=1-(1-target)/(1-previous);previous=target;
            draw.AddCircleFilled(center,radius*(1-(float)i/rings),HudAlpha(color,step),48);
        }
    }
    /// <summary>.coin: radial gold disc with a dark rim.</summary>
    private static void HudCoin(ImDrawListPtr draw,float2 center,float diameter)
    {
        // radial-gradient(circle at 35% 35%, #fff2b0, #ffcf4a 45%, #c48a12): gold reaches the rim except bottom right.
        float r=diameter/2;var light=center-new float2(r*.3f,r*.3f);
        draw.AddCircleFilled(center,r,HudMix(HudHex("ffcf4a"),HudHex("c48a12"),.55f),24);
        draw.PushClipRect(center-new float2(r,r),center+new float2(r,r),true);
        HudRadial(draw,light,r*1.25f,HudHex("ffcf4a"),1,10);
        HudRadial(draw,light,r*.7f,HudHex("fff2b0"),.9f,8);
        draw.PopClipRect();
        draw.AddCircle(center,r-.5f,HudAlpha(HudHex("8a5f08"),.85f),24);
    }
    private static void HudDashedRect(ImDrawListPtr draw,float2 min,float2 max,ImColor8 color,float thickness,float dash=6,float gap=4)
    {
        float inset=thickness/2;
        void Edge(float2 a,float2 b)
        {
            float length=MathF.Sqrt((b.X-a.X)*(b.X-a.X)+(b.Y-a.Y)*(b.Y-a.Y));var dir=(b-a)*(1/Math.Max(.001f,length));
            for(float s=0;s<length;s+=dash+gap)draw.AddLine(a+dir*s,a+dir*Math.Min(length,s+dash),color,thickness);
        }
        Edge(min+new float2(8,inset),new(max.X-8,min.Y+inset));Edge(new(min.X+8,max.Y-inset),max-new float2(8,inset));
        Edge(min+new float2(inset,8),new(min.X+inset,max.Y-8));Edge(new(max.X-inset,min.Y+8),max-new float2(inset,8));
    }
    private static void HudStar(ImDrawListPtr draw,float2 center,float radius,ImColor8 color)
    {
        Span<float2> points=stackalloc float2[10];
        for(int i=0;i<10;i++)
        {
            float angle=-MathF.PI/2+i*MathF.PI/5,r=i%2==0?radius:radius*.45f;
            points[i]=center+new float2(MathF.Cos(angle)*r,MathF.Sin(angle)*r);
        }
        for(int i=0;i<10;i+=2)draw.AddTriangleFilled(center,points[i],points[(i+1)%10],color);
        for(int i=1;i<10;i+=2)draw.AddTriangleFilled(center,points[i],points[(i+1)%10],color);
    }

    /// <summary>.ph: an 11px/800 uppercase rail heading with 1px tracking and a muted count on the right.</summary>
    private static float HudSectionHeader(ImDrawListPtr draw,float x,float y,float width,string title,string? right)
    {
        // margin: 2px 2px 6px; line box 11px * 1.35.
        float lineTop=y+2,line=11*1.35f;
        HudRun(draw,HudWeight(800),11,x+2,lineTop,line,title.ToUpperInvariant(),HudHex("8f9ab0"),0,1);
        if(right is not null)HudRunRight(draw,HudWeight(600),11,x+width-2,lineTop,line,right,HudHex("8f9ab0"));
        return 2+line+6;
    }
    /// <summary>A 1px CSS border on the current window's outer edge, which its inner clip rectangle excludes.</summary>
    private static void HudEdgeLine(float2 from,float2 to)
    {
        var draw=ImGui.GetWindowDrawList();var min=ImGui.GetWindowPos();
        draw.PushClipRect(min,min+ImGui.GetWindowSize());draw.AddLine(from,to,CssLine);draw.PopClipRect();
    }
    /// <summary>A top-to-bottom linear-gradient inside a rounded rectangle.</summary>
    private static void HudVerticalGradient(ImDrawListPtr draw,float2 min,float2 max,Rgb top,Rgb bottom,float rounding,float alpha=1)
    {
        float height=Math.Max(1,max.Y-min.Y),cap=Math.Min(rounding,height/2);
        Rgb At(float y)=>HudMix(bottom,top,(y-min.Y)/height);
        draw.AddRectFilled(min,new(max.X,min.Y+cap),HudAlpha(At(min.Y+cap/2),alpha),rounding,ImDrawFlags.RoundCornersTop);
        draw.AddRectFilled(new(min.X,max.Y-cap),max,HudAlpha(At(max.Y-cap/2),alpha),rounding,ImDrawFlags.RoundCornersBottom);
        // The caps' anti-aliased edges would show the colour underneath, so the middle overlaps them by 1px.
        if(height>2*cap)draw.AddRectFilledMultiColor(new(min.X,min.Y+cap-1),new(max.X,max.Y-cap+1),(ImColor8)HudAlpha(At(min.Y+cap),alpha),(ImColor8)HudAlpha(At(min.Y+cap),alpha),(ImColor8)HudAlpha(At(max.Y-cap),alpha),(ImColor8)HudAlpha(At(max.Y-cap),alpha));
    }
    /// <summary>clip-path: polygon(50% 0, 100% 25%, 100% 75%, 50% 100%, 0 75%, 0 25%).</summary>
    private static void HudHexagon(ImDrawListPtr draw,float2 min,float2 extent,ImColor8 color)
    {
        float w=extent.X,h=extent.Y;
        draw.AddRectFilled(min+new float2(0,h*.25f),min+new float2(w,h*.75f),color);
        draw.AddTriangleFilled(min+new float2(0,h*.25f),min+new float2(w/2,0),min+new float2(w,h*.25f),color);
        draw.AddTriangleFilled(min+new float2(0,h*.75f),min+new float2(w,h*.75f),min+new float2(w/2,h),color);
    }
    /// <summary>.pt: a family-coloured radial tile (light at 50% 30%) behind a portrait.</summary>
    private static void HudPortraitTile(ImDrawListPtr draw,float2 min,float size,Rgb family,float rounding)
    {
        var max=min+new float2(size,size);
        draw.AddRectFilled(min,max,HudMix(family,HudHex("0b0e15"),.4f),rounding);
        draw.PushClipRect(min,max,true);
        HudRadial(draw,min+new float2(size*.5f,size*.3f),size*.69f,HudMix(family,HudHex("ffffff"),.7f),1,14);
        draw.PopClipRect();
    }

    // style.css breakpoints. Compact covers max-height 800 or max-width 1360.
    private bool HudCompact=>size.Y<=800||size.X<=1360;
    private bool HudNarrow=>size.X<=1100;
    private int HudCardHeight=>size.Y<=680?88:HudCompact?100:112;
}
