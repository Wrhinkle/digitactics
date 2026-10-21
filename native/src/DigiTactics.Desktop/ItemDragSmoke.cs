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
    private PlacedUnit[] preservedUnits = [];
    private string[] preservedItems = [];
    private int scrollWait;
    private bool inventoryPressed;
    public string? CaptureName => tooltipHovered && !tooltipChecked ? "item-tooltip" : step == 9 ? "unit-equipment" : step == 8 ? "item-equip" : step == 12 ? "item-partner" : step == 16 ? "item-overflow" : null;
    public bool Input(MatchScreen screen)
    {
        var session = screen.Session!; var player = session.State.Players[0]; var io = ImGui.GetIO();
        void Move(float2 point) => io.AddMousePosEvent(point.X, point.Y);
        void Start(int index) { Move(screen.Buttons[(step <= 3 ? "shelfItem/" : "item/") + index]); io.AddMouseButtonEvent(0, true); }
        void Up() => io.AddMouseButtonEvent(0, false);
        if (scrollWait > 0) { scrollWait--; return false; }
        if(inventoryPressed){Up();inventoryPressed=false;scrollWait=2;return false;}
        if(step<20&&!screen.Buttons.ContainsKey("item/0"))
        {
            if(!screen.Buttons.TryGetValue("matchInventory",out var inventoryToggle))throw new InvalidOperationException("Match inventory drawer control is missing.");
            Move(inventoryToggle);io.AddMouseButtonEvent(0,true);inventoryPressed=true;return false;
        }
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
                string componentId = player.Items[slot];
                var recipe = session.State.Rules.Recipes!.First(pair => pair.Key.Split('+').Contains(componentId));
                var ingredients = recipe.Key.Split('+');
                string other = ingredients[0] == componentId ? ingredients[1] : ingredients[0];
                var display = new DisplayText();
                if (screen.ItemDropPreview(componentId, entry.Unit with { Items = [other] }) != "Combines into " + display.Get(session.State.Rules.Id, recipe.Value).Name)
                    throw new InvalidOperationException("Component drop preview did not identify the recipe result.");
                if (screen.ItemDropPreview(componentId, entry.Unit with { NoItems = true }) != "Can't hold items")
                    throw new InvalidOperationException("Drop preview allowed an itemless unit.");
                string completed = session.State.Rules.Items.First(item => !item.Component && !item.Consumable).Id;
                if (screen.ItemDropPreview(componentId, entry.Unit with { Items = [completed, completed, completed] }) != "Item slots full")
                    throw new InvalidOperationException("Drop preview did not identify full item slots.");
                if (entry.Unit.Items.Length != 0 || !inventory.SequenceEqual(player.Items))
                    throw new InvalidOperationException("Drop preview mutated unit or inventory state.");
                holder = entry.Unit.Id; destination = entry.Spot; item = player.Items[slot]; Start(slot); break;
            case 7:
                Move(screen.Buttons[destination.IsBoard ? $"hex/{destination.Column}/{destination.Row}" : $"arenaBench/{destination.Index}"]); break;
            case 8:
                if (screen.ItemDropFeedback != "Equip " + new DisplayText().Get(session.State.Rules.Id, item).Name)
                    throw new InvalidOperationException("Dragged component did not show an equip preview over its holder.");
                Up(); break;
            case 9:
                if (player.Items.Count != inventory.Length - 1 || !player.Roster.Entries.Single(e => e.Unit.Id == holder).Unit.Items.Contains(item) || screen.ItemDragging)
                    throw new InvalidOperationException("Item drop did not equip the target unit.");
                var equipped = player.Roster.Entries.Single(e => e.Unit.Id == holder).Unit;
                if (screen.DetailedUnit != holder || !screen.DetailedItems.SequenceEqual(equipped.Items))
                    throw new InvalidOperationException("Unit hover did not show the equipped items.");
                if (!session.State.Duos) { step = 14; break; }
                inventory = player.Items.ToArray(); item = inventory[0]; sends = player.PortalSends;
                partnerItems = session.Units.Portal.Partner(player)!.Items.Count; Start(0); break;
            case 10:
                var point = screen.Buttons["item/0"]; Move(new(point.X + 12, point.Y)); break;
            case 11:
                var partnerTarget = screen.Buttons["dropPartner"];
                var sidebar = screen.TeamSidebarBounds;
                if (partnerTarget.Y < sidebar.Min.Y + 34 || partnerTarget.Y > sidebar.Max.Y - 34)
                {
                    Move(new(sidebar.Max.X - 3, (sidebar.Min.Y + sidebar.Max.Y) / 2));
                    io.AddMouseWheelEvent(0, partnerTarget.Y < sidebar.Min.Y + 34 ? 1 : -1);
                    step--; scrollWait = 3; break;
                }
                Move(partnerTarget); break;
            case 12: Up(); break;
            case 13:
                var partner = session.Units.Portal.Partner(player)!;
                if (player.Items.Count != inventory.Length - 1 || player.PortalSends != sends + 1 || partner.Items.Count != partnerItems + 1 || partner.Items[^1] != item || screen.ItemDragging)
                    throw new InvalidOperationException("Item partner drop did not transfer exactly once.");
                step = 14; break;
            case 14:
                var rules = session.State.Rules;
                var glove = rules.Items.FirstOrDefault(i => ItemCapacity.Slots(i) == 3);
                if (glove is null) {step=20;break;}
                var line = rules.Lines.First(l => !l.NoItems);
                item = rules.CoreItems.First(id => ItemCapacity.Slots(rules.Items.Single(i => i.Id == id)) == 1);
                preservedUnits = player.Roster.Entries.ToArray(); preservedItems = player.Items.ToArray();
                foreach (var owned in preservedUnits) player.Roster.Remove(owned.Unit.Id, false);
                player.Items.Clear(); player.Items.AddRange(Enumerable.Repeat(item, rules.Loot!.ItemCap));
                for (int i = 0; i < line.Copies; i++)
                    player.Roster.AddToBench(session.State.CreateUnit(line.Id, 0) with { Items = i == 0 ? [glove.Id] : i == 1 ? [item] : [] });
                player.Roster.Combine(false, player.GiveItem);
                if (player.Items.Count != rules.Loot.ItemCap + 1 || player.Items[^1] != item)
                    throw new InvalidOperationException("Glove merge lost an item from a full inventory.");
                var recipient = session.State.CreateUnit(line.Id, 0); holder = recipient.Id; player.Roster.AddToBench(recipient);
                destination = player.Roster.Entries.Single(e => e.Unit.Id == holder).Spot;
                slot = player.Items.Count - 1; break;
            case 15:
                var overflowButton = screen.Buttons["item/" + slot];
                var bounds = screen.MatchInventoryBounds;
                // The item is 38px tall; its final row can sit flush with the
                // child clip edge at maximum scroll. Do not demand extra padding.
                if (overflowButton.Y < bounds.Min.Y + 19 || overflowButton.Y > bounds.Max.Y - 19)
                {
                    Move(new(bounds.Max.X - 10, (bounds.Min.Y + bounds.Max.Y) / 2));
                    io.AddMouseWheelEvent(0, overflowButton.Y < bounds.Min.Y + 19 ? 1 : -1);
                    step--; scrollWait = 3; break;
                }
                Move(overflowButton); break;
            case 16: Start(slot); break;
            case 17: Move(screen.Buttons["arenaBench/" + destination.Index]); break;
            case 18:
                if (screen.ItemDropFeedback != "Equip " + new DisplayText().Get(session.State.Rules.Id, item).Name)
                    throw new InvalidOperationException("Returned item drag did not reach its empty recipient: " + screen.ItemDropFeedback);
                Up(); break;
            case 19:
                if (player.Items.Count != session.State.Rules.Loot!.ItemCap || !player.Roster.Entries.Single(e => e.Unit.Id == holder).Unit.Items.Contains(item))
                    throw new InvalidOperationException("Returned overflow item could not be equipped through the visible inventory control.");
                foreach (var owned in player.Roster.Entries.ToArray()) player.Roster.Remove(owned.Unit.Id, false);
                foreach (var owned in preservedUnits)
                {
                    int bench = player.Roster.FreeBenchSlot; player.Roster.AddToBench(owned.Unit);
                    player.Roster.Move(UnitSpot.Bench(bench), owned.Spot, false, () => 100);
                }
                player.Items.Clear(); player.Items.AddRange(preservedItems);
                Console.WriteLine("Item overflow UI: full-bench glove merge preserved and equipped its returned item.");
                break;
            case 20:
                if(!screen.Buttons.TryGetValue("closeMatchInventory",out var close))return true;
                Move(close);io.AddMouseButtonEvent(0,true);break;
            case 21:Up();break;
            case 22:
                if(screen.Buttons.ContainsKey("closeMatchInventory"))throw new InvalidOperationException("Inventory drawer Close did not dismiss it.");
                return true;
        }
        return false;
    }
}
