using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private CombatCastFeed? castFeed;
    internal IReadOnlyList<CastFeedRow> CastFeedRows=>castFeed?.Rows??[];
    internal int CastFeedDraws {get;private set;}
    internal (float2 Min,float2 Max) CastFeedBounds {get;private set;}
    private void DrawCastFeed()
    {
        CastFeedDraws=0;
        if(castFeed is null||castFeed.Rows.Count==0)return;
        const float rowHeight=42,gap=4;
        int count=Math.Min(5,Math.Max(0,(int)((Height-20+gap)/(rowHeight+gap))));
        var rows=castFeed.Rows.TakeLast(count).ToArray();if(rows.Length==0)return;
        float width=Math.Min(320,Width-20),height=rows.Length*rowHeight+(rows.Length-1)*gap;
        float2 at=new(Left+10,Top+10);
        CastFeedBounds=(at,at+new float2(width,height));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(0,0));ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize,0);
        Window("Ability cast feed",at.X,at.Y,width,height,ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoBackground|ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoInputs|ImGuiWindowFlags.NoFocusOnAppearing|ImGuiWindowFlags.NoBringToFrontOnFocus);
        ImGui.PopStyleVar(2);
        var draw=ImGui.GetWindowDrawList();
        draw.PushClipRect(new(Left,Top),new(Left+Width,Top+Height),false);
        for(int i=0;i<rows.Length;i++)
        {
            var row=rows[i];double age=castFeed.Time-row.Started;
            float entry=ClientMotion?(float)Math.Clamp(age/.18,0,1):1;
            float fade=ClientMotion?(float)Math.Clamp((age-row.Life)/.4,0,1):age>=row.Life?1:0,alpha=entry*(1-fade);
            float2 pos=at+new float2(ClientMotion?-24*(1-entry)-20*fade:0,i*(rowHeight+gap));
            ImColor8 Ink(byte r,byte g,byte b,byte a=255)=>new(r,g,b,(byte)(a*alpha));
            var color=Convert.ToUInt32(row.Color[1..],16);
            var element=Ink((byte)(color>>16),(byte)(color>>8),(byte)color);
            draw.AddRectFilled(pos,pos+new float2(width,rowHeight),Ink(12,16,26,220),8);
            draw.AddRect(pos,pos+new float2(width,rowHeight),Ink(55,68,91),8);
            draw.AddRectFilled(pos,pos+new float2(4,rowHeight),row.Team==0?Ink(79,179,255):Ink(255,96,112),3);
            var portraitAt=pos+new float2(9,5);
            draw.AddRect(portraitAt-new float2(1,1),portraitAt+new float2(33,33),element,3,0,2);
            portraits.Draw(Rules.Id,row.Line,row.Stage,portraitAt,32,(byte)(255*alpha));
            HudClipText(draw,pos+new float2(49,3),width-57,row.Name,Ink(160,173,194),16);
            HudClipText(draw,pos+new float2(49,21),width-57,row.Ability,element,row.Stage>=2?18:17);
            CastFeedDraws++;
        }
        draw.PopClipRect();
        ImGui.End();
    }
}
