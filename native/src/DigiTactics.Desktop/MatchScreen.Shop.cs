using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private readonly PortraitArtwork portraits = new();
    internal int ShopPortraitDrawCalls => portraits.DrawCalls;
    private static readonly ImColor8[] tierColors = [new(154,164,177),new(154,164,177),new(63,191,106),new(59,140,255),new(180,92,255),new(255,176,46)];
    private void ShopCard(int slot,float width)
    {
        string? id=Human.Shop.Slots[slot];
        bool buy=Button("buy/"+slot,"",width,96);
        var min=ImGui.GetItemRectMin(); var max=ImGui.GetItemRectMax(); bool hovered=ImGui.IsItemHovered();
        var draw=ImGui.GetWindowDrawList();
        if(id is null) { draw.AddText(new(min.X+12,min.Y+38),new ImColor8(130,145,160),"Empty"u8);return; }
        var line=Rules.Lines.Single(l=>l.Id==id);var meta=Human.Shop.Metadata[slot];int stage=meta?.Unit?.Stage??0;
        int price=Human.Shop.Cost(slot); bool affordable=Human.Economy.Gold>=price;
        var ink=affordable?new ImColor8(235,240,248):new ImColor8(155,165,180);
        draw.AddRectFilled(min,new(max.X,min.Y+3),tierColors[Math.Clamp(line.Tier,0,5)]);
        portraits.Draw(Rules.Id,id,stage,new(min.X+3,min.Y+5),64,affordable?(byte)255:(byte)140);
        draw.PushClipRect(min,max,true);
        void Text(float x,float y,string value,ImColor8 color) => draw.AddText(ImGui.GetFont(),12,new(min.X+x,min.Y+y),color,value);
        Text(73,8,line.Attribute+(meta?.Tag is { } badge?" / "+badge:""),ink);
        var bonus=meta?.Unit?.BonusTraits;
        var traits=line.Traits.Concat(bonus?.Keys.AsEnumerable()??[]).Distinct().ToArray();
        for(int i=0;i<traits.Length;i++)
        {
            int count=(line.Traits.Contains(traits[i])?1:0)+(bonus?.GetValueOrDefault(traits[i])??0);
            Text(73,24+i*13,Name(traits[i])+(count>1?$" x{count}":""),ink);
        }
        int owned=Human.Roster.Entries.Count(e=>e.Unit.Line==id&&e.Unit.Stage==stage);
        string progress=stage<2&&owned>0?(owned>=line.Copies-1?"Digivolve":$"{owned}/{line.Copies}"):"";
        draw.PushClipRect(new(min.X,min.Y),new(max.X-62,max.Y),true);
        Text(6,73,line.Forms[stage]+(stage>0?" "+new string('*',stage+1):""),ink);
        draw.PopClipRect();
        Text(width-55,73,$"{price} gold",affordable?new ImColor8(255,218,115):ink);
        if(progress.Length>0) Text(6,55,progress,new ImColor8(100,240,177));
        draw.PopClipRect();
        if(hovered){ImGui.SetNextWindowSize(new(340,0));ImGui.BeginTooltip();UnitDetails(id,stage);ImGui.EndTooltip();}
        if(buy)Act("buy",new {slot});
    }
}
