using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private readonly PortraitArtwork portraits = new();
    internal int ShopPortraitDrawCalls {get;private set;}
    internal Dictionary<int,(float2 Min,float2 Max)> ShopCardBounds {get;}=[];
    internal (float2 Min,float2 Max) ShopViewportBounds {get;private set;}
    internal (float2 Min,float2 Max) ScoutingBounds {get;private set;}
    internal (float2 Min,float2 Max) TeamSidebarBounds {get;private set;}
    internal (float2 Min,float2 Max) AudioSettingsBounds {get;private set;}
    internal bool AudioSettingsVisible=>showAudio;
    internal string? ShopTooltipFooter {get;private set;}
    internal long ShopTooltipSerial {get;private set;}
    private static readonly ImColor8[] tierColors = [new(154,164,177),new(154,164,177),new(63,191,106),new(59,140,255),new(180,92,255),new(255,176,46)];
    private void ShopCard(int slot,float width)
    {
        string? id=Human.Shop.Slots[slot];
        bool buy=HudButton("buy/"+slot,"",width,ShopCardHeight);
        var min=ImGui.GetItemRectMin(); var max=ImGui.GetItemRectMax(); bool hovered=ImGui.IsItemHovered();
        ShopCardBounds[slot]=(min,max);
        var draw=ImGui.GetWindowDrawList();
        if(id is null) { HudClipText(draw,new(min.X+12,min.Y+58),width-24,"Empty slot",HudMuted);return; }
        var line=Rules.Lines.Single(l=>l.Id==id);var meta=Human.Shop.Metadata[slot];int stage=meta?.Unit?.Stage??0;
        int price=Human.Shop.Cost(slot); bool affordable=Human.Economy.Gold>=price;
        var ink=affordable?HudInk:HudMuted;
        var tier=tierColors[Math.Clamp(line.Tier,0,5)];
        draw.PushClipRect(min,max,true);
        draw.AddRectFilled(min,max,hovered?new ImColor8(32,44,64):HudSurface,8);
        int beforePortrait=portraits.DrawCalls;
        portraits.Draw(Rules.Id,id,stage,new(max.X-110,min.Y+1),110,affordable?(byte)255:(byte)120);
        ShopPortraitDrawCalls+=portraits.DrawCalls-beforePortrait;
        draw.AddRectFilledMultiColor(min,new(max.X,min.Y+108),new ImColor8(14,18,28,240),new ImColor8(14,18,28,15),new ImColor8(14,18,28,40),new ImColor8(14,18,28,240));
        void Text(float x,float y,float available,string value,ImColor8 color) => HudClipText(draw,new(min.X+x,min.Y+y),available,value,color);
        var bonus=meta?.Unit?.BonusTraits;
        var traits=line.Traits.Concat(bonus?.Keys.AsEnumerable()??[]).Distinct().ToArray();
        for(int i=0;i<traits.Length;i++)
        {
            int count=(line.Traits.Contains(traits[i])?1:0)+(bonus?.GetValueOrDefault(traits[i])??0);
            if(i<3)
            {
                var trait=codexCatalog.Value.Variant(Rules.Id).Traits.FirstOrDefault(value=>value.Id==traits[i]);
                var marker=min+new float2(12,17+i*19);var tint=trait is null?HudMuted:CodexColor(trait.Color);
                draw.AddTriangleFilled(marker+new float2(0,-5),marker+new float2(5,0),marker+new float2(0,5),tint);
                draw.AddTriangleFilled(marker+new float2(0,-5),marker+new float2(-5,0),marker+new float2(0,5),tint);
                HudClipText(draw,min+new float2(22,9+i*19),width-29,Name(traits[i])+(count>1?$" x{count}":""),ink,14);
            }
        }
        int owned=Human.Roster.Entries.Count(e=>e.Unit.Line==id&&e.Unit.Stage==stage);
        string progress=stage<2&&owned>0?(owned>=line.Copies-1?"Digivolve":$"{owned}/{line.Copies}"):"";
        Text(9,85,width-18,meta?.Tag??line.Attribute,meta?.Tag is null?ink:new ImColor8(229,139,255));
        if(progress.Length>0)Text(9,65,width-18,progress,HudGold);
        draw.AddRectFilled(new(min.X,min.Y+108),max,new ImColor8(14,18,27,245));
        draw.AddLine(new(min.X,min.Y+108),new(max.X,min.Y+108),tier);
        Text(9,116,width-51,line.Forms[stage]+(stage>0?" "+new string('*',stage+1):""),ink);
        draw.AddCircleFilled(new(max.X-28,min.Y+124),5,affordable?HudGold:HudMuted);
        Text(width-19,116,18,price.ToString(),affordable?HudGold:HudMuted);
        draw.AddRect(min+new float2(1,1),max-new float2(1,1),progress=="Digivolve"?HudGold:meta?.Tag is not null?new ImColor8(229,139,255):tier,8,thickness:hovered||progress=="Digivolve"?2:1);
        draw.PopClipRect();
        if(hovered)
        {
            BeginInspectionTooltip(290);
            UnitDetails(id,stage,null,meta?.Unit?.BonusTraits,meta?.Unit?.Mods);
            string offer=string.IsNullOrEmpty(meta?.Tag)?"":meta.Tag+" offer"+(string.IsNullOrEmpty(meta.Note)?"":": "+meta.Note);
            string pool=$"{Session!.State.Pool.Remaining(id)} more in the shared pool (of {Rules.PoolSizes[line.Tier]})";
            ImGui.Separator();if(offer.Length>0)HudText(offer,16,new ImColor8(229,139,255));HudText(pool,16,HudMuted);
            ShopTooltipFooter=offer.Length>0?offer+"\n"+pool:pool;ShopTooltipSerial++;
            EndInspectionTooltip();
        }
        if(buy)Act("buy",new {slot});
    }
}
