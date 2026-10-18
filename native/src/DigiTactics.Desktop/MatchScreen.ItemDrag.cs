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
        bool pressed = Button("item/" + index, "", width, 24, selectedItem == index);
        var min = ImGui.GetItemRectMin(); var max = ImGui.GetItemRectMax();
        var draw = ImGui.GetWindowDrawList();
        draw.PushClipRect(min, max, true);
        InventoryArtworkDrawCalls++;
        itemArtwork.Draw(Rules.Id, Human.Items[index], new(min.X + 2, min.Y + 1), 22);
        draw.AddText(ImGui.GetFont(), 13, new(min.X + 29, min.Y + 5), new ImColor8(235, 240, 248), Name(Human.Items[index]));
        draw.PopClipRect();
        return pressed;
    }

    private sealed record ItemDrag(int Index, string Id, string[] Inventory, float2 Start, MatchPhase Phase);
    private ItemDrag? itemDrag;
    private bool itemDragMoved;
    private int itemDropSlot = -1;
    internal bool ItemDragging => itemDragMoved;

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
        if (spot is { } target && Human.Roster.At(target) is { } holder) label += " -> " + UnitName(holder);
        var draw = ImGui.GetForegroundDrawList();
        draw.AddRectFilled(new(mouse.X + 10, mouse.Y + 14), new(mouse.X + 290, mouse.Y + 42), new ImColor8(15, 28, 40, 235));
        draw.AddText(ImGui.GetFont(), 14, new(mouse.X + 16, mouse.Y + 20), new ImColor8(240, 220, 140), label);
    }
}
