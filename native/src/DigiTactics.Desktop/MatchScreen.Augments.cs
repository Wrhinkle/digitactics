using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal IReadOnlyList<string> DisplayedAugments { get; private set; } = [];
    internal int AugmentPortraits { get; private set; }

    // #augs: .ph "Augments n/3", then .aug rows (30px art, 12.5px/700 tier-coloured name, 11px muted text).
    private void AugmentSidebar(MatchPlayer player)
    {
        DisplayedAugments = player.Augments.ToArray();
        AugmentPortraits = 0;
        if (DisplayedAugments.Count == 0) return;
        var draw=ImGui.GetWindowDrawList();var header=ImGui.GetCursorScreenPos();float railWidth=ImGui.GetContentRegionAvail().X;
        ImGui.Dummy(new(railWidth,HudSectionHeader(draw,header.X,header.Y,railWidth,"Augments",$"{DisplayedAugments.Count}/3")));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(0,0));
        foreach (string id in DisplayedAugments)
        {
            var description = text.Get(Rules.Id, id);
            var definition = Rules.Offers!.Augments.Concat(Rules.Offers.DuoAugments).Single(augment => augment.Id == id);
            var color = definition.Tier switch { 1 => HudHex("c9d1dc"), 2 => CssGold, _ => HudHex("e58bff") };
            var position=ImGui.GetCursorScreenPos();float width=ImGui.GetContentRegionAvail().X;
            bool art=ItemArtwork.Has(Rules.Id,"aug:"+id);
            float textX=1+7+(art?30+8:0),bodyWidth=width-textX-8;
            var title=WrapHud(HudWeight(700),12.5f,description.Name,bodyWidth);
            var body=WrapHud(HudWeight(400),11,description.Description,bodyWidth);
            float textHeight=title.Length*12.5f*1.35f+body.Length*11*1.25f;
            float height=Math.Max(30,textHeight)+10+2;
            ImGui.Dummy(new(width,height));
            draw=ImGui.GetWindowDrawList();
            draw.AddRectFilled(position,position+new float2(width,height),CssPanel,7);
            draw.AddRect(position,position+new float2(width,height),HudOver(color,CssPanel,.33f),7);
            if(art)
            {
                itemArtwork.Draw(Rules.Id,"aug:"+id,position+new float2(1+7,(height-30)/2),30);
                AugmentPortraits++;
            }
            float y=position.Y+(height-textHeight)/2;
            foreach(var line in title){HudRun(draw,HudWeight(700),12.5f,position.X+textX,y,12.5f*1.35f,line,color);y+=12.5f*1.35f;}
            foreach(var line in body){HudRun(draw,HudWeight(400),11,position.X+textX,y,11*1.25f,line,CssMuted);y+=11*1.25f;}
            ImGui.Dummy(new(width,4));
        }
        ImGui.PopStyleVar();
        ImGui.Dummy(new(0,8));
    }
}
