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
    internal (float2 Min,float2 Max) ArenaBounds=>(new(Left,Top),new(Left+Width,Top+Height));
    // #castFeed: 230px column at the arena's top left; .cf rows 30px portrait with a 2px element ring,
    // 4px team stripe, 11px muted name and 13px/800 ability (14px at stage 3) in the element colour.
    private void DrawCastFeed()
    {
        CastFeedDraws=0;
        if(castFeed is null||castFeed.Rows.Count==0)return;
        const float rowHeight=38,gap=5;
        int count=Math.Min(5,Math.Max(0,(int)((Height-20+gap)/(rowHeight+gap))));
        var rows=castFeed.Rows.TakeLast(count).ToArray();if(rows.Length==0)return;
        float width=Math.Min(230,Width-20),height=rows.Length*rowHeight+(rows.Length-1)*gap;
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
            var element=HudCss(row.Color);var stripe=row.Team==0?HudHex("4fb3ff"):HudHex("ff6070");
            var end=pos+new float2(width,rowHeight);
            draw.AddRectFilled(pos,end,new ImColor8(12,16,26,(byte)(219*alpha)),8);
            draw.AddRect(pos,end,HudAlpha(CssLine2,alpha),8);
            draw.AddRectFilled(pos,new(pos.X+4,end.Y),HudAlpha(stripe,alpha),8,ImDrawFlags.RoundCornersLeft);
            var portraitAt=pos+new float2(4+3,4);
            draw.AddRectFilled(portraitAt-new float2(2,2),portraitAt+new float2(32,32),HudAlpha(element,alpha),7);
            HudPortraitTile(draw,portraitAt,30,HudOver(FamilyColor(row.Line),HudHex("0c101a"),alpha),6);
            portraits.Draw(Rules.Id,row.Line,row.Stage,portraitAt,30,(byte)(255*alpha));
            float textX=portraitAt.X+30+8,textWidth=end.X-10-textX;
            float abilitySize=row.Stage>=2?14:13,top=pos.Y+(rowHeight-11*1.1f-abilitySize*1.15f)/2;
            HudRun(draw,HudWeight(400),11,textX,top,11*1.1f,row.Name,HudAlpha(CssMuted,alpha),textWidth);
            HudShadowRun(draw,HudWeight(800),abilitySize,textX,top+11*1.1f,abilitySize*1.15f,row.Ability,HudAlpha(element,alpha),textWidth,0,alpha);
            CastFeedDraws++;
        }
        draw.PopClipRect();
        ImGui.End();
    }
}
