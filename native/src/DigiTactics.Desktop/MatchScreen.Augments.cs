using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal IReadOnlyList<string> DisplayedAugments { get; private set; } = [];
    internal int AugmentPortraits { get; private set; }

    private void AugmentSidebar(MatchPlayer player)
    {
        DisplayedAugments = player.Augments.ToArray();
        AugmentPortraits = 0;
        if (DisplayedAugments.Count == 0) return;
        foreach (string id in DisplayedAugments)
        {
            var description = text.Get(Rules.Id, id);
            var definition = Rules.Offers!.Augments.Concat(Rules.Offers.DuoAugments).Single(augment => augment.Id == id);
            var color = definition.Tier switch
            {
                1 => new ImColor8(201,209,220),
                2 => new ImColor8(255,207,74),
                _ => new ImColor8(229,139,255)
            };
            var position=ImGui.GetCursorScreenPos();float width=ImGui.GetContentRegionAvail().X,bodyWidth=width-54;
            float titleHeight=ChoiceLines(description.Name,16,bodyWidth).Length*20;
            float bodyHeight=ChoiceLines(description.Description,14,bodyWidth).Length*18;
            float height=Math.Max(46,titleHeight+bodyHeight+16);
            ImGui.Dummy(new(width,height));
            var draw=ImGui.GetWindowDrawList();
            draw.AddRectFilled(position,position+new float2(width,height),new ImColor8(15,22,37),7);
            draw.AddRect(position,position+new float2(width,height),new ImColor8(34,48,73),7);
            if(ItemArtwork.Has(Rules.Id,"aug:"+id))
            {
                itemArtwork.Draw(Rules.Id,"aug:"+id,position+new float2(8,(height-30)/2),30);
                AugmentPortraits++;
            }
            ChoiceText(draw,position+new float2(46,8),bodyWidth,description.Name,16,20,color,false);
            ChoiceText(draw,position+new float2(46,8+titleHeight),bodyWidth,description.Description,14,18,HudMuted,false);
        }
        ImGui.Dummy(new(0,8));
    }
}
