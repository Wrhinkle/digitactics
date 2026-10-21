using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

/// <summary>theme.js button variants: .btn (default), .btn-secondary, .btn-primary, .lc-cta, .btn-play, .btn-ghost, .btn-danger.</summary>
internal enum ClientButtonKind { Default, Secondary, Primary, Cta, Play, Ghost, Danger }

internal sealed partial class MatchScreen
{
    /// <summary>
    /// A client button painted like the original. <paramref name="fontPx"/> is the CSS font size; labels are drawn as given
    /// except for variants that use text-transform: uppercase.
    /// </summary>
    private bool ClientButton(string key,string label,float width,float height,ClientButtonKind kind=ClientButtonKind.Default,float fontPx=14,bool selected=false,float? tracking=null,ImFontPtr? font=null)
    {
        bool clicked=ClientHitButton(key,width,height);
        bool hovered=ImGui.IsItemHovered(),held=hovered&&ImGui.IsMouseDown(ImGuiMouseButton.Left);
        var lo=ImGui.GetItemRectMin();var hi=ImGui.GetItemRectMax();
        if(held){lo.Y+=1;hi.Y+=1;}
        PaintClientButton(ImGui.GetWindowDrawList(),lo,hi,label,kind,fontPx,hovered||selected,tracking,font);
        if(ImGui.IsItemFocused()&&ImGui.GetIO().NavVisible)ImGui.GetWindowDrawList().AddRect(lo-new float2(2,2),hi+new float2(2,2),NativeUiPaint.Fade(NativeUiTheme.Accent),0,ImDrawFlags.None,2);
        return clicked;
    }

    private static void PaintClientButton(ImDrawListPtr draw,float2 lo,float2 hi,string label,ClientButtonKind kind,float fontPx,bool hot,float? tracking=null,ImFontPtr? fontOverride=null)
    {
        ImFontPtr font;ImColor8 ink;float spacing;bool upper;
        switch(kind)
        {
            case ClientButtonKind.Primary:
                NativeUiPaint.BoxGlow(draw,lo+new float2(0,2),hi+new float2(0,2),new ImColor8(255,138,61,(byte)(hot?90:64)),hot?20:10);
                NativeUiPaint.VerticalGradient(draw,lo,hi,new ImColor8(255,210,122),NativeUiTheme.PlayMiddle,NativeUiTheme.Play,.3f);
                if(hot)draw.AddRectFilled(lo,hi,NativeUiPaint.Fade(new ImColor8(255,255,255,20)));
                draw.AddLine(lo+new float2(1,1),new(hi.X-1,lo.Y+1),NativeUiPaint.Fade(new ImColor8(255,255,255,89)));
                draw.AddRect(lo,hi,NativeUiPaint.Fade(new ImColor8(240,210,140)));
                font=NativeUiTheme.BodyBoldFont;ink=NativeUiTheme.PlayText;spacing=0;upper=false;break;
            case ClientButtonKind.Danger:
                if(hot)NativeUiPaint.BoxGlow(draw,lo,hi,new ImColor8(255,84,104,90),20);
                NativeUiPaint.VerticalGradient(draw,lo,hi,new ImColor8(255,107,124),new ImColor8(214,58,78));
                draw.AddRect(lo,hi,NativeUiPaint.Fade(new ImColor8(255,139,152)));
                font=NativeUiTheme.BodySemiBoldFont;ink=new(255,255,255);spacing=0;upper=false;break;
            case ClientButtonKind.Cta:
                if(hot)NativeUiPaint.BoxGlow(draw,lo,hi,new ImColor8(63,214,255,77),18);
                NativeUiPaint.VerticalGradient(draw,lo,hi,hot?new ImColor8(15,82,102):new ImColor8(11,58,74),hot?new ImColor8(8,48,74):new ImColor8(6,32,51));
                NativeUiPaint.InsetGlow(draw,lo+new float2(2,2),hi-new float2(2,2),new ImColor8(63,214,255,(byte)(hot?115:64)),hot?18:14);
                // border-image: linear-gradient(180deg, gold-2, gold-4) 1 at 2 px.
                var top=hot?NativeUiTheme.GoldBright:NativeUiTheme.Gold;var bottom=hot?NativeUiTheme.Gold:NativeUiTheme.GoldBorder;
                draw.AddRectFilled(lo,new(hi.X,lo.Y+2),NativeUiPaint.Fade(top));draw.AddRectFilled(new(lo.X,hi.Y-2),hi,NativeUiPaint.Fade(bottom));
                NativeUiPaint.VerticalGradient(draw,lo,new(lo.X+2,hi.Y),top,bottom);NativeUiPaint.VerticalGradient(draw,new(hi.X-2,lo.Y),hi,top,bottom);
                font=NativeUiTheme.DisplayBoldFont;ink=hot?new ImColor8(255,255,255):NativeUiTheme.AccentInk;spacing=.12f;upper=true;break;
            case ClientButtonKind.Play:
            {
                // clip-path: polygon(10px 0, 100% 0, 100% calc(100% - 10px), calc(100% - 10px) 100%, 0 100%, 0 10px).
                const float cut=10;var uv=ImGui.GetFontTexUvWhitePixel();float split=lo.Y+(hi.Y-lo.Y)*.35f;
                ImColor8 a=hot?new ImColor8(255,227,115):new ImColor8(255,210,106),b=hot?new ImColor8(255,210,80):NativeUiTheme.PlayMiddle,c=hot?new ImColor8(255,150,66):NativeUiTheme.Play;
                ImColor8 At(float y)=>y<=split?Mix(a,b,(y-lo.Y)/(split-lo.Y)):Mix(b,c,(y-split)/(hi.Y-split));
                float2[] shape=[new(lo.X+cut,lo.Y),new(hi.X,lo.Y),new(hi.X,hi.Y-cut),new(hi.X-cut,hi.Y),new(lo.X,hi.Y),new(lo.X,lo.Y+cut)];
                var centre=(lo+hi)*.5f;
                draw.PrimReserve(shape.Length*3,shape.Length*3);
                for(int i=0;i<shape.Length;i++)
                {
                    var p=shape[i];var q=shape[(i+1)%shape.Length];
                    draw.PrimVtx(centre,uv,NativeUiPaint.Fade(At(centre.Y)));draw.PrimVtx(p,uv,NativeUiPaint.Fade(At(p.Y)));draw.PrimVtx(q,uv,NativeUiPaint.Fade(At(q.Y)));
                }
                draw.AddLine(new(lo.X+cut,lo.Y+1),new(hi.X-1,lo.Y+1),NativeUiPaint.Fade(new ImColor8(255,255,255,140)));
                NativeUiPaint.VerticalGradient(draw,new(lo.X+1,hi.Y-3),new(hi.X-cut,hi.Y),new ImColor8(120,50,0,40),new ImColor8(120,50,0,70));
                // Stroke half a pixel inside so the 1px border covers whole pixels.
                float2[] edge=[new(lo.X+cut,lo.Y+.5f),new(hi.X-.5f,lo.Y+.5f),new(hi.X-.5f,hi.Y-cut),new(hi.X-cut,hi.Y-.5f),new(lo.X+.5f,hi.Y-.5f),new(lo.X+.5f,lo.Y+cut)];
                foreach(var point in edge)draw.PathLineTo(point);
                draw.PathStroke(NativeUiPaint.Fade(new ImColor8(255,224,154)),ImDrawFlags.Closed,1);
                font=NativeUiTheme.DisplayBoldFont;ink=NativeUiTheme.PlayText;spacing=.14f;upper=true;break;
            }
            case ClientButtonKind.Ghost:
                if(hot){draw.AddRectFilled(lo,hi,NativeUiPaint.Fade(new ImColor8(200,170,110,20)));draw.AddRect(lo,hi,NativeUiPaint.Fade(NativeUiTheme.GoldDim));}
                font=NativeUiTheme.BodySemiBoldFont;ink=hot?NativeUiTheme.GoldBright:NativeUiTheme.LcMute;spacing=0;upper=false;break;
            default:
                if(hot)NativeUiPaint.BoxGlow(draw,lo,hi,new ImColor8(200,170,110,46),10);
                NativeUiPaint.VerticalGradient(draw,lo,hi,hot?new ImColor8(38,45,54):new ImColor8(30,35,40),hot?new ImColor8(26,33,43):new ImColor8(20,26,34));
                draw.AddRect(lo,hi,NativeUiPaint.Fade(hot?NativeUiTheme.Gold:NativeUiTheme.GoldBorder));
                bool secondary=kind==ClientButtonKind.Secondary;
                font=secondary?NativeUiTheme.DisplayFont:NativeUiTheme.BodySemiBoldFont;ink=hot?NativeUiTheme.GoldBright:NativeUiTheme.LcText;spacing=secondary?.08f:0;upper=secondary;break;
        }
        spacing=tracking??spacing;font=fontOverride??font;
        string text=upper?label.ToUpperInvariant():label;
        var size=NativeUiPaint.TrackedSize(font,fontPx,text,spacing);
        // Letter-spacing trails the last glyph in Chromium, so centred labels sit half a space left of centre.
        var at=new float2(MathF.Round((lo.X+hi.X-size.X)/2),MathF.Round((lo.Y+hi.Y-size.Y)/2));
        NativeUiPaint.Tracked(draw,font,fontPx,at,ink,text,spacing);
    }
}
