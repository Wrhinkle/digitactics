using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private readonly ItemArtwork itemArtwork = new();
    internal int InventoryArtworkDrawCalls { get; private set; }

    private bool ItemButton(int index, float width)
    {
        bool pressed = HudButton("item/" + index, "", width, 38, selectedItem == index);
        var min = ImGui.GetItemRectMin(); var max = ImGui.GetItemRectMax();
        var draw = ImGui.GetWindowDrawList();
        draw.PushClipRect(min, max, true);
        InventoryArtworkDrawCalls++;
        itemArtwork.Draw(Rules.Id, Human.Items[index], new(min.X + 4, min.Y + 3), 32);
        HudClipText(draw,new(min.X+43,min.Y+11),width-49,Name(Human.Items[index]),HudInk);
        draw.PopClipRect();
        return pressed;
    }

    private sealed record ItemDrag(int Index, string Id, string[] Inventory, float2 Start, MatchPhase Phase);
    private ItemDrag? itemDrag;
    private bool itemDragMoved;
    private int itemDropSlot = -1;
    internal bool ItemDragging => itemDragMoved;
    internal string? ItemDropFeedback { get; private set; }

    internal string ItemDropPreview(string id, PlanningUnit unit)
    {
        var item = Rules.Items.Single(item => item.Id == id);
        if (item.Consumable) return Name(id) + ": " + text.ConsumablePreview(Rules, id, unit.Items.Length, Human.Items.Count);
        if (item.Emblem is { } emblem && TraitRules.UnitTraits(new(unit.Line, unit.Stage, 0, 0, Items: unit.Items), Rules).Contains(emblem))
            return "Already has this trait";
        if (item.Component)
            for (int index = 0; index < unit.Items.Length; index++)
                if (Rules.Items.Single(item => item.Id == unit.Items[index]).Component && Session!.Units.Items.Combine(unit.Items[index], id) is { } result)
                    return Session.Units.Items.CanFit(unit, result, index) ? "Combines into " + Name(result) : "Needs all 3 item slots free";
        if (!Session!.Units.Items.HoldsItems(unit)) return "Can't hold items";
        if (!Session.Units.Items.CanFit(unit, id) && (ItemCapacity.Slots(item) == 3 || unit.Items.Any(held => ItemCapacity.Slots(Rules.Items.Single(i => i.Id == held)) == 3)))
            return "Gloves occupy all 3 item slots";
        return !Session.Units.Items.CanHold(Human, unit, id) ? "Item slots full" : "Equip " + Name(id);
    }

    private void ItemDragSource(int index)
    {
        bool hover = ImGui.IsWindowHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem)
            && ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());
        if (hover) itemDropSlot = index;
        if (hover && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !Session!.State.Over && Human.Alive && viewed == localSeat)
        {
            itemDrag = new(index, Human.Items[index], Human.Items.ToArray(), ImGui.GetMousePos(), Session.Rounds.Phase);
            itemDragMoved = false;
        }
    }

    private void ItemDragPointer(bool editable, UnitSpot? spot)
    {
        if (itemDrag is not { } drag) return;
        if (!editable || drag.Phase != Session!.Rounds.Phase || !drag.Inventory.SequenceEqual(Human.Items))
        { itemDrag = null; itemDragMoved = false; return; }
        var mouse = ImGui.GetMousePos();
        itemDragMoved |= Math.Sqrt(Math.Pow(mouse.X - drag.Start.X, 2) + Math.Pow(mouse.Y - drag.Start.Y, 2)) > 4;
        if (ImGui.IsMouseClicked(ImGuiMouseButton.Right)) { itemDrag = null; itemDragMoved = false; return; }
        if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            if (itemDragMoved)
            {
                if (partnerDropHovered) Act("portal", new { item = drag.Index });
                else if (itemDropSlot >= 0 && itemDropSlot != drag.Index) Act("swapItems", new { a = drag.Index, b = itemDropSlot });
                else if (spot is { } destination && Human.Roster.At(destination) is { } unit) Act("equip", new { idx = drag.Index, uid = unit.Id });
                selectedItem = null;
            }
            itemDrag = null; itemDragMoved = false; return;
        }
        if (!itemDragMoved) return;
        selectedUnit = selectedItem = null;
        string label = Name(drag.Id);
        if (spot is { } target && Human.Roster.At(target) is { } holder)
        {
            ItemDropFeedback = ItemDropPreview(drag.Id, holder);
            label = UnitName(holder) + ": " + ItemDropFeedback;
        }
        var draw = ImGui.GetForegroundDrawList();
        float width = Math.Min(Math.Min(420, size.X - 24), ImGui.CalcTextSize(label).X + 12);
        float height = ImGui.CalcTextSize(label, false, width - 12).Y + 12;
        float x = Math.Clamp(mouse.X + 10, 12, size.X - width - 12);
        float y = Math.Max(12, Math.Min(mouse.Y + 14, size.Y - height - 12));
        draw.AddRectFilled(new(x, y), new(x + width, y + height), new ImColor8(15, 28, 40, 235));
        draw.PushClipRect(new(x,y),new(x+width,y+height),true);
        draw.AddText(ImGui.GetFont(), ImGui.GetFontSize(), new(x + 6, y + 6), new ImColor8(240, 220, 140), label, width - 12);
        draw.PopClipRect();
    }
}
