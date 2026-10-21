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
    private readonly record struct HudFace(ImFontPtr Font,float Factor);
    private static HudFace hudRegular,hudSemibold,hudBold,hudBlack,modalRegular,modalBold,modalDisplay;
    private static bool hudFontsLoaded;

    internal static void LoadHudFonts()
    {
        string system=Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
        string bundled=Path.Combine(AppContext.BaseDirectory,"content","fonts");
        modalRegular=new(NativeUiTheme.BodyFont,FaceFactor(Path.Combine(bundled,"inter","Inter[opsz,wght].ttf")));
        modalBold=new(NativeUiTheme.BodyBoldFont,FaceFactor(Path.Combine(bundled,"inter","Inter-Bold.ttf")));
        modalDisplay=new(NativeUiTheme.DisplayBoldFont,FaceFactor(Path.Combine(bundled,"chakrapetch","ChakraPetch-Bold.ttf")));
        HudFace System(string file,HudFace fallback)
        {
            string path=Path.Combine(system,file);
            if(!File.Exists(path))return fallback;
            var font=ImGui.GetIO().Fonts.AddFontFromFileTTF(path,NativeUiTheme.BodySize);
            return font.IsNull()?fallback:new(font,FaceFactor(path));
        }
        // Chromium resolves CSS weight 800 to Segoe UI Black, the next heavier installed face.
        hudRegular=System("segoeui.ttf",modalRegular);
        hudSemibold=System("seguisb.ttf",modalBold);
        hudBold=System("segoeuib.ttf",modalBold);
        hudBlack=System("seguibl.ttf",modalBold);
        hudFontsLoaded=true;
    }

    // hhea ascender minus descender over unitsPerEm: the factor stb_truetype applies to a pixel height.
    private static float FaceFactor(string path)
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
            if(hhea<0||unitsPerEm==0)return 1.2f;
            return ((short)U16(data,hhea+4)-(short)U16(data,hhea+6))/(float)unitsPerEm;
        }
        catch(IOException){return 1.2f;}
    }

    private static HudFace HudWeight(int weight)
    {
        if(!hudFontsLoaded)return new(NativeUiTheme.BodyFont,1.21f);
        return weight>=800?hudBlack:weight>=700?hudBold:weight>=600?hudSemibold:hudRegular;
    }
    private static HudFace ModalWeight(int weight)
    {
        if(!hudFontsLoaded)return new(weight>=600?NativeUiTheme.BodyBoldFont:NativeUiTheme.BodyFont,1.21f);
        return weight>=600?modalBold:modalRegular;
    }
    private static HudFace ModalDisplay=>hudFontsLoaded?modalDisplay:new(NativeUiTheme.DisplayBoldFont,1.3f);

    private static float HudWidth(HudFace face,float css,string value,float spacing=0)
        =>value.Length==0?0:face.Font.CalcTextSizeA(css*face.Factor,float.MaxValue,0,value).X+spacing*value.Length;

    /// <summary>Draws one CSS text run whose line box starts at <paramref name="lineTop"/>.</summary>
    private static float HudRun(ImDrawListPtr draw,HudFace face,float css,float x,float lineTop,float lineHeight,string value,ImColor8 color,float maxWidth=0,float spacing=0)
    {
        float size=css*face.Factor,y=lineTop+(lineHeight-size)/2;
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
        float r=diameter/2;
        draw.AddCircleFilled(center,r,HudHex("c48a12"),24);
        draw.AddCircleFilled(center-new float2(r*.12f,r*.12f),r*.82f,HudHex("ffcf4a"),24);
        HudRadial(draw,center-new float2(r*.3f,r*.3f),r*.62f,HudHex("fff2b0"),.95f,8);
        draw.AddCircle(center,r-.5f,HudHex("8a5f08"),24);
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
