using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>
/// style.css .ulabel (cast badge, name, HP/shield/mana bars with 300 HP ticks, stage stars, items)
/// and .ftext floating combat numbers, anchored where render3d.js projects them.
/// </summary>
internal sealed partial class MatchScreen
{
    internal int UnitLabelsDrawn {get;private set;}
    internal int FloatingTextsDrawn {get;private set;}
    private readonly record struct CombatBars(bool Enemy,double Health,double Shield,double MaxHealth,double Mana,double MaxMana,bool Stunned);
    private sealed record FloatingText(long Started,int Unit,double X,double Y,double Z,string Text,Rgb Color,float Size);
    private readonly List<FloatingText> floatingTexts=[];
    private CombatSimulation? floatingCombat;
    private int floatingCursor;

    // fx.js elemColor.
    private static Rgb ElementColor(string element)=>element switch
    {
        "fire"=>HudHex("ff7a3a"),"ice"=>HudHex("8fe3ff"),"thunder"=>HudHex("ffe94a"),"water"=>HudHex("4fb0ff"),"plant"=>HudHex("6cff7a"),
        "wind"=>HudHex("b8fff0"),"holy"=>HudHex("fff0a0"),"dark"=>HudHex("b46bff"),"metal"=>HudHex("ffb04a"),_=>HudHex("ffd6a8"),
    };

    private void DrawUnitLabel(ImDrawListPtr draw,float2 head,int stage,IReadOnlyList<string> items,string? name,CombatBars? bars,(string Name,Rgb Color,bool Enemy)? cast,float alpha=1)
    {
        float x=head.X,y=head.Y;
        ImColor8 A(Rgb color,float a=1)=>HudAlpha(color,a*alpha);
        // .items: 23px completed items, 18px components, 2px apart, 1px under the stars.
        if(items.Count>0)
        {
            var defs=items.Select(id=>Rules.Items.FirstOrDefault(item=>item.Id==id)).ToArray();
            float rowHeight=defs.Any(d=>d is {Component:false})?23:18;
            float total=defs.Sum(d=>d is {Component:true}?18:23)+(items.Count-1)*2,ix=x-total/2;
            for(int i=0;i<items.Count;i++)
            {
                float sizeOf=defs[i] is {Component:true}?18:23;
                itemArtwork.Draw(Rules.Id,items[i],new(ix,y-rowHeight+(rowHeight-sizeOf)/2),sizeOf);ix+=sizeOf+2;
            }
            y-=rowHeight+1+1;
        }
        // .stars: 13px (14px at three stars), -1px tracking, bronze/silver/gold.
        float starSize=stage>=2?14:13,step=starSize*.78f-1;int count=Math.Clamp(stage,0,2)+1;
        float sx=x-(count*step)/2+step/2,sy=y-starSize/2-.5f;
        for(int i=0;i<count;i++)
        {
            HudStar(draw,new(sx+i*step,sy+1),starSize*.4f,new ImColor8(0,0,0,(byte)(200*alpha)));
            HudStar(draw,new(sx+i*step,sy),starSize*.38f,A(stageColors[Math.Clamp(stage,0,2)]));
        }
        y-=starSize+1;
        if(bars is {} b)
        {
            // .bars: 62px; .hp 8px with a 1px black frame, .mp 4px without a top border.
            var hpMin=new float2(x-31,y-12);var hpMax=hpMin+new float2(62,8);
            if(b.MaxMana>0&&b.Mana>=b.MaxMana)HudRadial(draw,new(x,y-6),40,HudHex("7fd0ff"),.35f*alpha,8);
            draw.AddRectFilled(hpMin,hpMax,new ImColor8(0,0,0,(byte)(242*alpha)),2);
            var inner=hpMin+new float2(1,1);float innerWidth=60;
            double total=Math.Max(b.MaxHealth,b.Health+b.Shield);
            float hp=(float)Math.Clamp(b.Health/Math.Max(1,total),0,1),shield=(float)Math.Clamp(b.Shield/Math.Max(1,total),0,1-hp);
            if(hp>0)draw.AddRectFilled(inner,inner+new float2(innerWidth*hp,6),A(b.Enemy?HudHex("ff5a6a"):HudHex("4fdc7f")));
            if(shield>0)draw.AddRectFilled(inner+new float2(innerWidth*hp,0),inner+new float2(innerWidth*(hp+shield),6),A(HudHex("eef3fb")));
            // a tick every 300 HP
            float tick=(float)(innerWidth*300/Math.Max(300,total));
            for(float t=tick;t<innerWidth-.5f;t+=tick)draw.AddRectFilled(inner+new float2(t-1,0),inner+new float2(t,6),new ImColor8(0,0,0,(byte)(191*alpha)));
            var mpMin=new float2(hpMin.X,hpMax.Y);var mpMax=mpMin+new float2(62,4);
            bool full=b.MaxMana>0&&b.Mana>=b.MaxMana;
            draw.AddRectFilled(mpMin,mpMax,full?A(HudHex("ffffff")):new ImColor8(0,0,0,(byte)(242*alpha)),2,ImDrawFlags.RoundCornersBottom);
            float mana=b.MaxMana>0?(float)Math.Clamp(b.Mana/b.MaxMana,0,1):0;
            if(mana>0&&!full)draw.AddRectFilledMultiColor(mpMin+new float2(1,0),mpMin+new float2(1+innerWidth*mana,3),(ImColor8)A(HudHex("2f7bff")),(ImColor8)A(HudHex("6fc3ff")),(ImColor8)A(HudHex("6fc3ff")),(ImColor8)A(HudHex("2f7bff")));
            y-=12+1;
        }
        if(name is not null)
        {
            float line=12*1.35f;y-=line;
            HudShadowRun(draw,HudWeight(700),12,x-HudWidth(HudWeight(700),12,name)/2,y,line,name,A(HudHex("ffffff")),0,0,alpha);
            y-=1;
        }
        if(cast is {} c)
        {
            // .ucast: 12px/800 pill in the element colour with a 2px white (enemy: pink) frame.
            float width=HudWidth(HudWeight(800),12,c.Name)+14+4,height=12*1.35f+2+4;
            y-=2;var min=new float2(x-width/2,y-height);var max=new float2(x+width/2,y);
            HudRadial(draw,(min+max)*.5f,width*.62f,c.Color,.45f*alpha,8);
            draw.AddRectFilled(min,max,A(c.Enemy?HudHex("ff9aa4"):HudHex("ffffff")),10);
            draw.AddRectFilled(min+new float2(2,2),max-new float2(2,2),A(c.Color),8);
            HudRun(draw,HudWeight(800),12,min.X+9,min.Y+3,12*1.35f,c.Name,A(CssInk));
        }
        UnitLabelsDrawn++;
    }

    private void CombatLabels()
    {
        UnitLabelsDrawn=0;
        if(battle is null)return;
        var draw=ImGui.GetWindowDrawList();
        int bottom=size.Y-Top-Height;
        draw.PushClipRect(new(Left,Top),new(Left+Width,Top+Height),false);
        // Bench units keep their stars and items during combat.
        var player=Session!.State.Players[viewed];
        foreach(var entry in player.Roster.Entries.Where(e=>!e.Spot.IsBoard))
        {
            var spot=battle.BenchPosition(entry.Spot.Index);var line=Rules.Lines.Concat(Rules.ExtraLines).Single(l=>l.Id==entry.Unit.Line);
            if(battle.ProjectWorld(spot.X,spot.Y+BattleView.LabelHeight(entry.Unit.Stage,line.Tier),spot.Z,Width,Height,size.Y,Left,bottom) is {} head)
                DrawUnitLabel(draw,head,entry.Unit.Stage,entry.Unit.Items,null,null,null);
        }
        var mouse=ImGui.GetMousePos();
        foreach(var anchor in battle.LabelAnchors(Width,Height,size.Y,Left,bottom))
        {
            var unit=anchor.Unit;
            bool hovered=ImGui.IsMouseHoveringRect(anchor.Head+new float2(-34,-6),anchor.Head+new float2(34,70));
            (string,Rgb,bool)? cast=anchor.View.CastVisible&&anchor.View.CastName is {} castName
                ?(castName,ElementColor(abilityElements.Get(Rules.Id,unit.Line.Id,unit.Stage)),!battle.IsAlly(unit)):null;
            DrawUnitLabel(draw,anchor.Head,unit.Stage,unit.Items,hovered?unit.Line.Forms[unit.Stage]:null,
                new CombatBars(!battle.IsAlly(unit),unit.Health,unit.Shield,unit.MaxHealth,unit.Mana,unit.MaxMana,unit.Stun>0),cast);
        }
        FloatingCombatText(draw,bottom);
        draw.PopClipRect();
    }

    // render3d.js floatText: dmg/heal/text events rise 40px and fade over .9s from a jittered point 1.6-1.9 high.
    private void FloatingCombatText(ImDrawListPtr draw,int bottom)
    {
        FloatingTextsDrawn=0;
        if(battle is null)return;
        var combat=battle.Combat;long now=Environment.TickCount64;
        if(!ReferenceEquals(combat,floatingCombat)){floatingCombat=combat;floatingTexts.Clear();floatingCursor=0;}
        while(floatingCursor<combat.Events.Count)
        {
            var cue=combat.Events[floatingCursor++];
            if(cue.Type is not ("damage" or "heal" or "text")||cue.Unit<0||cue.Unit>=combat.Units.Count)continue;
            var at=battle.UnitPosition(cue.Unit);var jitter=new Random(cue.Unit*7919+floatingCursor);
            double x=at.X+(jitter.NextDouble()-.5)*.5,y=1.6+jitter.NextDouble()*.3;
            (string text,Rgb color,float sizeOf)=cue.Type switch
            {
                "heal"=>($"+{cue.Amount:0}",HudHex("5adf8a"),15f),
                "text"=>(cue.Text??"",HudHex("dddddd"),13f),
                _=>($"{cue.Amount:0}{(cue.Critical?"!":"")}",cue.Kind switch{DamageKind.Physical=>HudHex("ffb35a"),DamageKind.Magic=>HudHex("6fd3ff"),_=>CssText},cue.Critical?20f:15f),
            };
            // Events resolved faster than real time (Skip, 4x) would pile up; the original only shows recent ones.
            if(combat.Time-cue.Time>1.2)continue;
            floatingTexts.Add(new(now,cue.Unit,x,y,at.Z,text,color,sizeOf));
        }
        floatingTexts.RemoveAll(text=>now-text.Started>1000);
        foreach(var text in floatingTexts)
        {
            float t=Math.Clamp((now-text.Started)/900f,0,1);
            if(t>=1||battle.ProjectWorld(text.X,text.Y,text.Z,Width,Height,size.Y,Left,bottom) is not {} point)continue;
            // animation: rise .9s ease-out
            float eased=1-(1-t)*(1-t)*(1-t*.42f),alpha=1-eased;
            float width=HudWidth(HudWeight(800),text.Size,text.Text),line=text.Size*1.35f;
            var at=new float2(point.X-width/2,point.Y-line/2-40*eased);
            HudRun(draw,HudWeight(800),text.Size,at.X+.5f,at.Y+1.5f,line,text.Text,new ImColor8(0,0,0,(byte)(200*alpha)));
            HudRun(draw,HudWeight(800),text.Size,at.X,at.Y,line,text.Text,HudAlpha(text.Color,alpha));
            FloatingTextsDrawn++;
        }
    }

    // Planning: every owned unit shows its stars and items above its head.
    private void PlanningLabels()
    {
        UnitLabelsDrawn=0;
        var draw=ImGui.GetWindowDrawList();var player=Session!.State.Players[viewed];
        draw.PushClipRect(new(Left,Top),new(Left+Width,Top+Height),false);
        foreach(var entry in player.Roster.Entries)
        {
            var line=Rules.Lines.Concat(Rules.ExtraLines).Single(l=>l.Id==entry.Unit.Line);
            bool dragged=planning.DraggedUnit==entry.Unit.Id&&planning.DragPosition is not null;
            var position=dragged?planning.DragPosition!.Value:entry.Spot.IsBoard?CombatLayout.Cell(new(entry.Spot.Column,entry.Spot.Row),7,false):CombatLayout.Bench(entry.Spot.Index);
            var point=planning.Point(position,position.Y+BattleView.LabelHeight(entry.Unit.Stage,line.Tier),Width,Height);
            bool named=hoveredOwnedUnit==entry.Unit.Id||dragged;
            DrawUnitLabel(draw,new(Left+point.X,Top+point.Y),entry.Unit.Stage,entry.Unit.Items,named?line.Forms[entry.Unit.Stage]:null,null,null);
        }
        draw.PopClipRect();
    }
}
