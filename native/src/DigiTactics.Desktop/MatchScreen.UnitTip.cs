using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>
/// The original #tooltip: 290px, theme.js navy gradient with a gold-4 border, Inter 13px, with
/// ui.js tooltipFor / traitTip / playerTip content blocks, placed where placeTip puts it.
/// </summary>
internal sealed partial class MatchScreen
{
    private enum TipSide { Cursor, Top, Right, Left }
    private readonly record struct TipRun(string Text,bool Bold,Rgb Color);
    private abstract record TipPart;
    // .th: a 52px portrait (drawn by the callback) beside an h3 and a .line.
    private sealed record TipHead(Action<ImDrawListPtr,float2>? Portrait,string Title,Rgb TitleColor,TipRun[] Line,int Stars=-1,int Stage=0) : TipPart;
    private sealed record TipTitle(string Title,Rgb Color) : TipPart;
    private sealed record TipLine(TipRun[] Runs) : TipPart;
    private sealed record TipTags((string Name,Rgb Color)[] Tags) : TipPart;
    private sealed record TipStats((string Label,string Value)[] Stats) : TipPart;
    private sealed record TipAbility(TipRun[] Runs) : TipPart;
    private sealed record TipBreakpoint(TipRun[] Runs,bool On) : TipPart;
    private sealed record TipFoot(TipRun[] Runs,Rgb? Bar=null) : TipPart;
    private sealed record TipItem(string Id) : TipPart;
    private const float TipWidth=290,TipPadX=12,TipPadY=10;
    private static float TipInner=>TipWidth-2*TipPadX-2;
    private static TipRun[] Runs(string text,Rgb color,bool bold=false)=>[new(text,bold,color)];

    private List<TipPart> UnitTipParts(UnitLine line,int stage,string[] traits,(string,string)[] stats,DisplayEntry ability,string[] notes,IReadOnlyList<string> items)
    {
        var codex=codexCatalog.Value.Variant(Rules.Id);var entry=codex.Lines.FirstOrDefault(value=>value.Id==line.Id);
        string stageName=entry?.Forms.ElementAtOrDefault(stage)?.StageName??"";
        var parts=new List<TipPart>
        {
            new TipHead((draw,at)=>
            {
                draw.AddRectFilled(at,at+new float2(52,52),tierColors[Math.Clamp(line.Tier,0,5)],8);
                HudPortraitTile(draw,at+new float2(2,2),48,FamilyColor(line.Id),6);
                portraits.Draw(Rules.Id,line.Id,stage,at+new float2(2,2),48);
            },line.Forms[stage],CssText,Runs($"{stageName} · {line.Tier}-cost · ",CssMuted),stage+1,stage),
        };
        var evolution=new List<TipRun>();
        for(int i=0;i<line.Forms.Length;i++){if(i>0)evolution.Add(new(" → ",false,CssMuted));evolution.Add(new(line.Forms[i],i==stage,i==stage?CssText:CssMuted));}
        parts.Add(new TipLine(evolution.ToArray()));
        parts.Add(new TipTags(traits.Select(id=>codex.Traits.FirstOrDefault(t=>t.Id==id) is {} t?(t.Name,HudCss(t.Color)):(Name(id),CssMuted))
            .Append((line.Attribute,AttributeColor(line.Attribute))).ToArray()));
        parts.Add(new TipStats(stats));
        parts.Add(new TipAbility([new(ability.Name,true,HudHex("3fd6ff")),new(": "+ability.Description,false,CssText)]));
        DetailedUnitNotes=notes;UnitNoteRenderSerial++;
        foreach(string note in notes)parts.Add(new TipFoot(Runs(note,HudHex("e9d6ff")),HudHex("b45cff")));
        foreach(string item in items)parts.Add(new TipItem(item));
        return parts;
    }
    private List<TipPart> PlanningTip(string lineId,int stage,IReadOnlyList<string> items,Dictionary<string,int>? bonusTraits,System.Text.Json.JsonElement? mods)
    {
        var line=Rules.Lines.Concat(Rules.ExtraLines).Single(l=>l.Id==lineId);
        var stats=UnitValues.Stats(line,stage);
        var traits=TraitRules.UnitTraits(new(lineId,stage,0,0,Items:items.ToArray()),Rules)
            .Concat((bonusTraits??[]).Keys.Where(id=>Rules.Traits.Any(trait=>trait.Id==id))).Distinct().ToArray();
        return UnitTipParts(line,stage,traits,[("HP",$"{stats.MaxHealth:0}"),("Mana",$"{stats.StartMana:0}/{stats.MaxMana:0}"),("AD",$"{stats.AttackDamage:0}"),("AS",$"{stats.AttackSpeed:0.00}"),
            ("Armor",$"{stats.Armor:0}"),("MR",$"{stats.MagicResist:0}"),("Range",$"{stats.Range:0}"),("","")],text.Ability(Rules.Id,lineId,stage),unitNotes.Describe(Rules.Id,lineId,mods),items);
    }
    private List<TipPart> CombatTip(CombatUnit unit)
        =>UnitTipParts(unit.Line,unit.Stage,unit.Traits.ToArray(),[("HP",$"{unit.Health:0}/{unit.MaxHealth:0}"),("Mana",$"{Math.Floor(unit.Mana):0}/{unit.MaxMana:0}"),("AD",$"{unit.AttackDamage:0}"),("AS",$"{unit.EffectiveAttackSpeed:0.00}"),
            ("Armor",$"{unit.Armor:0}"),("MR",$"{unit.MagicResist:0}"),("Range",$"{unit.Range:0}"),("AP",$"{unit.AbilityPower:0}")],
            text.Ability(Rules.Id,unit.Line.Id,unit.Stage,unit.AbilityPower),unitNotes.Describe(Rules.Id,unit.Entry),unit.Items);

    /// <summary>Lays out (draw null) or draws the parts at <paramref name="at"/>; returns the tooltip height.</summary>
    private float TipBody(ImDrawListPtr? draw,float2 at,IReadOnlyList<TipPart> parts)
    {
        float x=at.X+1+TipPadX,y=at.Y+1+TipPadY,inner=TipInner;bool first=true;
        var regular=ModalWeight(400);var bold=ModalWeight(700);
        (string,HudFace,Rgb)[] Faces(TipRun[] runs)=>runs.Select(run=>(run.Text,run.Bold?bold:regular,run.Color)).ToArray();
        foreach(var part in parts)
        {
            switch(part)
            {
                case TipHead head:
                {
                    float headText=16*1.35f+1+12*1.35f+4,height=head.Portrait is null?headText:Math.Max(52,headText),tx=head.Portrait is null?x:x+52+10;
                    if(draw is {} d)
                    {
                        head.Portrait?.Invoke(d,new(x,y+(height-52)/2));
                        float ty=y+(height-headText)/2;
                        HudRun(d,ModalDisplay,16,tx,ty,16*1.35f,head.Title,head.TitleColor,x+inner-tx,.32f);
                        ty+=16*1.35f+1;float lx=tx;
                        foreach(var run in head.Line)lx+=HudRun(d,run.Bold?bold:regular,12,lx,ty,12*1.35f,run.Text,run.Color);
                        for(int i=0;i<head.Stars;i++)HudStar(d,new(lx+5+i*10,ty+12*1.35f/2),4.6f,stageColors[Math.Clamp(head.Stage,0,2)]);
                    }
                    y+=height+6;break;
                }
                case TipTitle title:
                    if(draw is {} td)HudRun(td,ModalDisplay,16,x,y,16*1.35f,title.Title,title.Color,inner,.32f);
                    y+=16*1.35f+1;break;
                case TipLine line:y=TipRich(draw,x,y,inner,12,Faces(line.Runs))+4;break;
                case TipTags tags:
                {
                    float tagX=x,tagH=11*1.35f+4;
                    foreach(var tag in tags.Tags)
                    {
                        float w=HudWidth(regular,11,tag.Name)+14;
                        if(tagX>x&&tagX+w>x+inner){tagX=x;y+=tagH+4;}
                        if(draw is {} gd){gd.AddRect(new(tagX,y),new(tagX+w,y+tagH),HudAlpha(tag.Color,.33f),10);HudRun(gd,regular,11,tagX+7,y+2,11*1.35f,tag.Name,tag.Color);}
                        tagX+=w+4;
                    }
                    y+=tagH+6;break;
                }
                case TipStats stats:
                {
                    // table: label left, value right with 10px padding, two pairs per row
                    float pair=MathF.Round(inner*.48f),row=12*1.35f+2;
                    for(int i=0;i<stats.Stats.Length;i+=2)
                    {
                        for(int k=0;k<2&&i+k<stats.Stats.Length;k++)
                        {
                            var (label,value)=stats.Stats[i+k];if(label.Length==0||draw is not {} sd)continue;
                            float px=x+k*(pair+2),pw=k==0?pair:inner-pair-2;
                            HudRun(sd,regular,12,px,y+1,12*1.35f,label,CssMuted);
                            HudRunRight(sd,bold,12,px+pw-10,y+1,12*1.35f,value,CssText);
                        }
                        y+=row;
                    }
                    y+=6;break;
                }
                case TipAbility ability:
                    if(!first){if(draw is {} ad)ad.AddLine(new(x,y+.5f),new(x+inner,y+.5f),CssLine);y+=1+6;}
                    y=TipRich(draw,x,y,inner,13,Faces(ability.Runs));break;
                case TipBreakpoint breakpoint:y+=2;y=TipRich(draw,x,y,inner,12,Faces(breakpoint.Runs));break;
                case TipFoot foot:
                {
                    y+=6;float top=y,indent=foot.Bar is null?0:9;
                    y=TipRich(draw,x+indent,y,inner-indent,12,Faces(foot.Runs));
                    if(foot.Bar is {} bar&&draw is {} fd)fd.AddRectFilled(new(x,top),new(x+3,y),bar);
                    break;
                }
                case TipItem item:
                {
                    y+=6;var definition=Rules.Items.FirstOrDefault(value=>value.Id==item.Id);bool component=definition?.Component==true;
                    var description=text.Get(Rules.Id,item.Id);float top=y,textX=x+28+8;
                    if(draw is not null)itemArtwork.Draw(Rules.Id,item.Id,new(x+(component?2:0),y+(component?2:0)),component?24:28);
                    float end=TipRich(draw,textX,y,inner-36,12,[(description.Name,bold,CssText)]);
                    end=TipRich(draw,textX,end,inner-36,12,[(description.Description,regular,CssMuted)]);
                    y=Math.Max(top+28,end);break;
                }
            }
            first=false;
        }
        return y+TipPadY+1-at.Y;
    }
    /// <summary>Inline runs wrapped as one paragraph (each run keeps its own face and colour).</summary>
    private static float TipRich(ImDrawListPtr? draw,float x,float y,float width,float css,(string Text,HudFace Face,Rgb Color)[] runs)
    {
        float line=css*ModalLine,cx=x;
        foreach(var (value,face,color) in runs)
        {
            var words=value.Split(' ');
            for(int i=0;i<words.Length;i++)
            {
                string word=(i>0?" ":"")+words[i];
                float w=HudWidth(face,css,word);
                if(cx>x&&cx+w>x+width){cx=x;y+=line;word=word.TrimStart();w=HudWidth(face,css,word);}
                if(draw is {} d&&word.Length>0)HudRun(d,face,css,cx,y,line,word,color);
                cx+=w;
            }
        }
        return y+line;
    }
    /// <summary>Shows the parts beside an anchor rectangle or the cursor, kept on screen like placeTip.</summary>
    private (float2 Min,float2 Max) ShowTip(IReadOnlyList<TipPart> parts,TipSide side,float2 min=default,float2 max=default)
    {
        float height=TipBody(null,default,parts);var mouse=ImGui.GetMousePos();
        float x,y;
        switch(side)
        {
            case TipSide.Top:x=(min.X+max.X)/2-TipWidth/2;y=min.Y-height-8;break;
            case TipSide.Right:x=max.X+8;y=min.Y;break;
            case TipSide.Left:x=min.X-TipWidth-8;y=min.Y;break;
            default:x=mouse.X+22;y=mouse.Y+16;if(x+TipWidth>size.X-8)x=mouse.X-TipWidth-22;break;
        }
        x=MathF.Round(Math.Max(8,Math.Min(size.X-TipWidth-8,x)));y=MathF.Round(Math.Max(8,Math.Min(size.Y-height-8,y)));
        ImGui.SetNextWindowPos(new(x,y));ImGui.SetNextWindowSize(new(TipWidth,height));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(0,0));ImGui.PushStyleVar(ImGuiStyleVar.PopupBorderSize,0);ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding,0);
        ImGui.PushStyleColor(ImGuiCol.PopupBg,new ImColor8(0,0,0,0));
        ImGui.BeginTooltip();
        var draw=ImGui.GetWindowDrawList();var at=new float2(x,y);var end=at+new float2(TipWidth,height);
        draw.PushClipRect(at-new float2(30,30),end+new float2(30,40));
        // box-shadow: 0 18px 50px rgba(0,0,0,.6)
        for(int i=1;i<=6;i++)draw.AddRectFilled(at+new float2(-i*2,18-i*2+i),end+new float2(i*2,i*3),new ImColor8(0,0,0,(byte)(24-i*3)));
        HudVerticalGradient(draw,at,end,HudHex("0a1428"),HudHex("010a13"),0,.98f);
        draw.AddRect(at,end,ModalGold4);
        TipBody(draw,at,parts);
        draw.PopClipRect();
        ImGui.Dummy(new(TipWidth,height));
        ImGui.EndTooltip();ImGui.PopStyleColor();ImGui.PopStyleVar(3);
        return (at,end);
    }
}
