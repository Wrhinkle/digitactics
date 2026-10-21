using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal bool RefreshEnabled { get; private set; }
    internal bool BuyXpEnabled { get; private set; }
    private bool showMatchInventory;
    internal (float2 Min,float2 Max) MatchInventoryBounds {get;private set;}

    private float ShopPadding=>HudNarrow?6:10;
    private float ShopGap=>HudNarrow?6:10;
    private float EconWidth=>HudNarrow?140:HudCompact?178:200;
    private float ShopButtonsWidth=>HudNarrow?84:HudCompact?98:112;

    // #shopbar: padding 8px 10px, the #econ grid, the #shopBtns column and one row of offers.
    private void Roster()
    {
        ShopPortraitDrawCalls=0;
        float top=size.Y-ShopHeight;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(0,0));
        ImGui.PushStyleColor(ImGuiCol.WindowBg,CssPanel);ImGui.PushStyleColor(ImGuiCol.Border,new ImColor8(0,0,0,0));
        Window("Shop and bench",0,top,size.X,ShopHeight,ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse);
        ImGui.PopStyleColor(2);ImGui.PopStyleVar();
        var draw=ImGui.GetWindowDrawList();
        draw.PushClipRect(new(0,top),new(size.X,size.Y));
        draw.AddRectFilledMultiColor(new(0,top),new(size.X,size.Y),(ImColor8)CssPanel,(ImColor8)CssPanel,(ImColor8)HudHex("10151f"),(ImColor8)HudHex("10151f"));
        draw.AddLine(new(0,top+.5f),new(size.X,top+.5f),CssLine);
        draw.PopClipRect();
        float contentTop=top+1+(HudNarrow?6:8);
        ImGui.SetCursorScreenPos(new(ShopPadding,contentTop));
        if(IsSpectating){HudRun(draw,HudWeight(400),14,ShopPadding,contentTop,18.9f,"Spectating. Take an available seat to control a team.",CssMuted);ImGui.End();return;}
        if(SellDropZone()){ImGui.End();return;}
        ImGui.BeginDisabled(Session!.State.Over||!Human.Alive);
        ShopControls(contentTop);
        float shopX=ShopPadding+EconWidth+ShopGap+ShopButtonsWidth+ShopGap,shopWidth=size.X-ShopPadding-shopX;
        ShopCardBounds.Clear();ShopViewportBounds=(new(shopX,contentTop),new(size.X-ShopPadding,contentTop+HudCardHeight));
        // grid-template-columns: repeat(5, 1fr) 0.85fr; the last column is the Data Wisp slot.
        int slots=Human.Shop.Slots.Count;float gap=HudNarrow?4:8;
        float unit=(shopWidth-gap*slots)/(slots+.85f);
        for(int i=0;i<slots;i++)ShopCard(i,new(shopX+i*(unit+gap),contentTop),unit);
        WispShopCard(new(shopX+slots*(unit+gap),contentTop),unit*.85f,Rules.Wisps is {Length:>0});
        ImGui.EndDisabled();ImGui.End();
        DrawMatchInventory();
    }

    private void ShopControls(float contentTop)
    {
        var economy=Human.Economy;var draw=ImGui.GetWindowDrawList();
        float x=ShopPadding,width=EconWidth,cardHeight=HudCardHeight;
        int capacity=new MatchPlacement(Session!.State).Capacity(Human,Session.Rounds.Position.Stage);
        var xp=Rules.Economy!.Xp;
        var income=Session.Rounds.PreviewIncome(Human);
        bool econLine=Rules.Economy.Interest is not null||Rules.Economy.Streak.Length>0;
        // #econ rows: level/board (+ XP bar), gold, interest/streak, bench and items.
        float row1=17*1.15f+11*1.15f+(xp is null?0:7),row2=26,row3=econLine?11*1.2f:0,row4=11*1.35f;
        float total=row1+4+row2+4+(econLine?row3+4:0)+row4;
        float y=contentTop+(cardHeight-total)/2;
        HudRun(draw,HudWeight(800),17,x,y,17*1.15f,$"Level {economy.Level}",CssText);
        string levelNote;
        if(xp is not null)levelNote=economy.Level>=xp.MaxLevel?"Max level":$"{economy.Xp}/{xp.Table[economy.Level]} XP";
        else levelNote=NextLevelNote();
        HudRun(draw,HudWeight(400),11,x,y+17*1.15f,11*1.15f,levelNote,CssMuted);
        if(xp is not null)
        {
            bool maxed=economy.Level>=xp.MaxLevel;int need=maxed?0:xp.Table[economy.Level];
            float fraction=maxed?1:Math.Clamp((float)economy.Xp/Math.Max(1,need),0,1),barWidth=(width-10)/2;
            var bar=new float2(x,y+17*1.15f+11*1.15f+3);
            draw.AddRectFilled(bar,bar+new float2(barWidth,4),new ImColor8(255,255,255,31),2);
            if(fraction>0)draw.AddRectFilledMultiColor(bar,bar+new float2(barWidth*fraction,4),(ImColor8)HudHex("3fd6ff"),(ImColor8)HudHex("7fb8ff"),(ImColor8)HudHex("7fb8ff"),(ImColor8)HudHex("3fd6ff"));
        }
        bool under=Human.Roster.BoardCount<capacity&&Human.Roster.Bench.Any(unit=>unit is not null)&&Session.Rounds.Phase==MatchPhase.Planning;
        var boardInk=under?CssGold:CssText;
        HudRunRight(draw,HudWeight(700),17,x+width,y,17*1.15f,$"{Human.Roster.BoardCount}/{capacity}",boardInk);
        HudRunRight(draw,HudWeight(400),11,x+width,y+17*1.15f,11*1.15f,"on board",under?CssGold:CssMuted);
        y+=row1+4;
        HudCoin(draw,new(x+9,y+13),18);
        float goldWidth=HudRun(draw,HudWeight(900),26,x+24,y,26,economy.Gold.ToString(),CssGold);
        ImGui.SetCursorScreenPos(new(x+24+goldWidth+6,y+6));IncomeDisplay(x+24+goldWidth+6,y);
        y+=row2+4;
        if(econLine)
        {
            float ex=x;
            if(Rules.Economy.Interest is {} interest)
            {
                string next=income.Interest<interest.Max?$" ({interest.Per*(income.Interest+1)-Math.Max(0,economy.Gold)} to +{income.Interest+1})":" (max)";
                ex+=HudRun(draw,HudWeight(400),11,ex,y,row3,"Interest ",CssMuted);
                ex+=HudRun(draw,HudWeight(700),11,ex,y,row3,$"+{income.Interest}",CssGold);
                ex+=HudRun(draw,HudWeight(400),11,ex,y,row3,next,CssMuted)+8;
            }
            if(Rules.Economy.Streak.Length>0)
            {
                int streak=economy.Streak;var ink=streak>1?HudHex("6fe08a"):streak<-1?HudHex("ff8a7a"):CssMuted;
                ex+=HudRun(draw,HudWeight(400),11,ex,y,row3,(Math.Abs(streak)>1?$"{(streak>0?"Win":"Loss")} streak {Math.Abs(streak)}":"No streak")+" ",ink);
                HudRun(draw,HudWeight(700),11,ex,y,row3,$"+{income.Streak}",streak>1||streak<-1?ink:CssGold);
            }
            y+=row3+4;
        }
        // #capLabel. The native item drawer opens from it; returned overflow items live there.
        int bench=Human.Roster.Bench.Count(unit=>unit is not null);
        float cx=x;
        cx+=HudRun(draw,HudWeight(400),11,cx,y,row4,"Bench ",CssMuted);
        cx+=HudRun(draw,HudWeight(700),11,cx,y,row4,$"{bench}/9",bench>=9?CssBad:CssText);
        cx+=HudRun(draw,HudWeight(400),11,cx,y,row4," · Items ",CssMuted);
        cx+=HudRun(draw,HudWeight(700),11,cx,y,row4,$"{Human.Items.Count}/{Rules.Loot!.ItemCap}",Human.Items.Count>=Rules.Loot.ItemCap?CssBad:CssText);
        ImGui.SetCursorScreenPos(new(x,y));
        if(ImGui.InvisibleButton("matchInventory"u8,new(Math.Max(1,cx-x),row4)))showMatchInventory=!showMatchInventory;
        Buttons["matchInventory"]=new(x+(cx-x)/2,y+row4/2);
        if(ImGui.IsItemHovered())ResultTooltip("Show your item inventory");

        // #shopBtns: optional Buy XP, Reroll (two lines) and Lock shop, centered in the card height.
        float bx=x+width+ShopGap,bw=ShopButtonsWidth;
        float xpHeight=xp is null?0:33,rollHeight=MathF.Round(15*1.1f+1+12*1.1f+20+2),lockHeight=MathF.Round(12*1.35f+10+2);
        float stack=(xp is null?0:xpHeight+6)+rollHeight+6+lockHeight;
        float by=contentTop+(cardHeight-stack)/2;
        BuyXpEnabled=false;
        if(xp is not null)
        {
            BuyXpEnabled=economy.Gold>=xp.BuyCost&&economy.Level<xp.MaxLevel;
            if(ShopButton("xp",new(bx,by),new(bw,xpHeight),BuyXpEnabled,false,(draw,min,max,ink)=>
            {
                float labelWidth=HudWidth(HudWeight(400),14,"Buy XP ")+11+4+HudWidth(HudWeight(400),12,xp.BuyCost.ToString());
                float lx=(min.X+max.X-labelWidth)/2,ly=min.Y+(xpHeight-18.9f)/2;
                lx+=HudRun(draw,HudWeight(400),14,lx,ly,18.9f,"Buy XP ",ink);
                HudCoin(draw,new(lx+5.5f,ly+10),11);
                HudRun(draw,HudWeight(400),12,lx+15,ly+1,18.9f,xp.BuyCost.ToString(),CssGold);
            }))Act("buyXp");
            if(ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))ResultTooltip(economy.Level>=xp.MaxLevel?"Max level":$"Buy {xp.BuyXp} XP for {xp.BuyCost} gold. +{xp.PerRound} XP each round.");
            by+=xpHeight+6;
        }
        RefreshEnabled=Human.Shop.FreeRolls>0||economy.Gold>=Rules.RerollCost;
        if(ShopButton("reroll",new(bx,by),new(bw,rollHeight),RefreshEnabled,false,(draw,min,max,ink)=>
        {
            float cxm=(min.X+max.X)/2,ly=min.Y+11;
            HudRunCentered(draw,HudWeight(800),15,cxm,ly,15*1.1f,"Reroll",ink);
            ly+=15*1.1f+1;
            if(Human.Shop.FreeRolls>0)HudRunCentered(draw,HudWeight(700),12,cxm,ly,12*1.1f,"free x"+Human.Shop.FreeRolls,CssGold);
            else
            {
                string cost=Rules.RerollCost.ToString();float costWidth=11+4+HudWidth(HudWeight(700),12,cost),cx0=cxm-costWidth/2;
                HudCoin(draw,new(cx0+5.5f,ly+12*1.1f/2+.5f),11);
                HudRun(draw,HudWeight(700),12,cx0+15,ly,12*1.1f,cost,CssGold);
            }
        }))Act("reroll");
        by+=rollHeight+6;
        bool locked=Human.Shop.Locked;
        if(ShopButton("lock",new(bx,by),new(bw,lockHeight),true,locked,(draw,min,max,ink)=>
            HudRunCentered(draw,HudWeight(400),12,(min.X+max.X)/2,min.Y+6,12*1.35f,locked?"Shop locked":"Lock shop",locked?CssGold:ink)))Act("lock");
    }
    // game.nextLevelAt(): Hyper Roll levels by stage.
    private string NextLevelNote()
    {
        int stage=Session!.Rounds.Position.Stage;var levels=Rules.Economy!.LevelByStage;
        return levels is not null&&stage+1<levels.Length?$"Lv {levels[stage+1]} at stage {stage+1}":"Max level";
    }
    /// <summary>A style.css button (panel2 fill, line2 border, 6px radius) drawn at a fixed rectangle.</summary>
    private bool ShopButton(string key,float2 at,float2 extent,bool enabled,bool on,Action<ImDrawListPtr,float2,float2,Rgb> content)
    {
        var draw=ImGui.GetWindowDrawList();var max=at+extent;
        ImGui.SetCursorScreenPos(at);
        ImGui.BeginDisabled(!enabled);
        bool pressed=ImGui.InvisibleButton(key,extent);
        bool hovered=enabled&&ImGui.IsItemHovered();
        ImGui.EndDisabled();
        Buttons[key]=(at+max)*.5f;
        if(ImGui.IsItemFocused())KeyboardFocusedButton=key;
        var fill=on?HudHex("2a2516"):hovered?CssPanel3:CssPanel2;var border=on?CssGold:hovered?HudHex("3fd6ff"):CssLine2;var ink=CssText;
        if(!enabled){fill=HudOver(fill,CssPanel,.45f);border=HudOver(border,CssPanel,.45f);ink=HudOver(ink,CssPanel,.45f);}
        draw.AddRectFilled(at,max,fill,6);draw.AddRect(at,max,border,6);
        content(draw,at,max,ink);
        return pressed&&enabled;
    }

    private void DrawMatchInventory()
    {
        // Returned merge items can exceed the normal shelf capacity. Surface the
        // drawer automatically so none of those items become inaccessible.
        if(Human.Items.Count>Rules.Loot!.ItemCap)showMatchInventory=true;
        if(!showMatchInventory)return;
        float width=Math.Min(720,size.X-32),height=Math.Min(240,Height*.42f);
        ImGui.SetNextWindowFocus();
        Window("Inventory",(size.X-width)/2,Top+8,width,height,ImGuiWindowFlags.NoTitleBar);
        MatchInventoryBounds=(ImGui.GetWindowDrawList().GetClipRectMin(),ImGui.GetWindowDrawList().GetClipRectMax());
        HudText("ITEM INVENTORY",18,HudGold);ImGui.SameLine();
        if(HudButton("closeMatchInventory","Close",70,28)){showMatchInventory=false;ImGui.End();return;}
        HudText("Drag an item onto a Digimon, or select an item and its holder.",14,HudMuted);
        ImGui.BeginChild("Inventory items"u8,new(0,0));
        MatchInventoryBounds=(ImGui.GetWindowDrawList().GetClipRectMin(),ImGui.GetWindowDrawList().GetClipRectMax());
        float itemWidth=(ImGui.GetContentRegionAvail().X-16)/3;
        ImGui.BeginDisabled(Session!.State.Over||!Human.Alive);
        for(int i=0;i<Human.Items.Count;i++)
        {
            if(i%3!=0)ImGui.SameLine();
            if(ItemButton(i,itemWidth)&&!itemDragMoved)
            {
                if(selectedItem is {} old&&old!=i){Act("swapItems",new{a=old,b=i});selectedItem=null;}
                else{selectedItem=selectedItem==i?null:i;selectedUnit=null;}
            }
            ItemDragSource(i);if(!itemDragMoved)Tooltip(Human.Items[i]);
        }
        if(selectedItem is {} item&&Session.State.Duos&&HudButton("sendItem","Send selected item",190,34)){Act("portal",new{item});selectedItem=null;}
        ImGui.EndDisabled();ImGui.EndChild();ImGui.End();
    }
}
