using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed class ItemDragSmoke
{
    private int step, slot, holder, sends, partnerItems;
    private string[] inventory = [];
    private string item = "";
    private UnitSpot destination;
    private bool tooltipHovered, tooltipChecked;
    private string tooltipItem = "";
    public string? CaptureName => tooltipHovered && !tooltipChecked ? "item-tooltip" : step == 8 ? "item-equip" : step == 12 ? "item-partner" : null;
    public bool Input(MatchScreen screen)
    {
        var session = screen.Session!; var player = session.State.Players[0]; var io = ImGui.GetIO();
        void Move(float2 point) => io.AddMousePosEvent(point.X, point.Y);
        void Start(int index) { Move(screen.Buttons[(step <= 3 ? "shelfItem/" : "item/") + index]); io.AddMouseButtonEvent(0, true); }
        void Up() => io.AddMouseButtonEvent(0, false);
        if (!tooltipChecked)
        {
            if (!tooltipHovered)
            {
                int component = player.Items.FindIndex(id => session.State.Rules.Items.Single(item => item.Id == id).Component);
                if (component < 0) throw new InvalidOperationException("Tooltip smoke requires a component.");
                tooltipItem = player.Items[component]; Move(screen.Buttons["item/" + component]); tooltipHovered = true; return false;
            }
            int expected = session.State.Rules.Recipes!.Count(recipe => recipe.Key.Split('+').Contains(tooltipItem));
            if (expected == 0 || screen.ItemTooltipRecipes != expected) throw new InvalidOperationException("Component tooltip omitted combination recipes.");
            tooltipChecked = true;
        }
        switch (step++)
        {
            case 0:
                inventory = player.Items.ToArray();
                if (inventory.Length < 2) throw new InvalidOperationException("Item drag smoke requires two earned items.");
                if (screen.InventoryArtworkDrawCalls != inventory.Length) throw new InvalidOperationException("Inventory controls omitted original item artwork.");
                if (screen.ItemShelfDrawCalls != Math.Min(10, inventory.Length)) throw new InvalidOperationException("Planning shelf omitted item tokens.");
                Start(0); break;
            case 1: Move(screen.Buttons["shelfItem/1"]); break;
            case 2: Up(); break;
            case 3:
                (inventory[0], inventory[1]) = (inventory[1], inventory[0]);
                if (!inventory.SequenceEqual(player.Items) || screen.ItemDragging) throw new InvalidOperationException("Item drag did not swap inventory slots exactly once.");
                Start(0); break;
            case 4: Move(new(0, 0)); break;
            case 5: Up(); break;
            case 6:
                if (!inventory.SequenceEqual(player.Items) || screen.ItemDragging) throw new InvalidOperationException("Outside item drop changed inventory or retained capture.");
                slot = player.Items.FindIndex(id => session.State.Rules.Items.Single(i => i.Id == id).Component);
                if (slot < 0) throw new InvalidOperationException("Item drag smoke has no component to equip.");
                var entry = player.Roster.Entries.First(e => e.Unit.Items.Length == 0);
                holder = entry.Unit.Id; destination = entry.Spot; item = player.Items[slot]; Start(slot); break;
            case 7:
                Move(screen.Buttons[destination.IsBoard ? $"hex/{destination.Column}/{destination.Row}" : $"arenaBench/{destination.Index}"]); break;
            case 8: Up(); break;
            case 9:
                if (player.Items.Count != inventory.Length - 1 || !player.Roster.Entries.Single(e => e.Unit.Id == holder).Unit.Items.Contains(item) || screen.ItemDragging)
                    throw new InvalidOperationException("Item drop did not equip the target unit.");
                if (!session.State.Duos) return true;
                inventory = player.Items.ToArray(); item = inventory[0]; sends = player.PortalSends;
                partnerItems = session.Units.Portal.Partner(player)!.Items.Count; Start(0); break;
            case 10:
                var point = screen.Buttons["item/0"]; Move(new(point.X + 12, point.Y)); break;
            case 11: Move(screen.Buttons["dropPartner"]); break;
            case 12: Up(); break;
            case 13:
                var partner = session.Units.Portal.Partner(player)!;
                if (player.Items.Count != inventory.Length - 1 || player.PortalSends != sends + 1 || partner.Items.Count != partnerItems + 1 || partner.Items[^1] != item || screen.ItemDragging)
                    throw new InvalidOperationException("Item partner drop did not transfer exactly once.");
                return true;
        }
        return false;
    }
}
