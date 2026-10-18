using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private (int Id, UnitSpot From, float2 Start, MatchPhase Phase)? arenaDrag;
    private bool arenaDragMoved;
    private WorldPosition? arenaDragPosition;
    private bool sellDropHovered, partnerDropHovered;
    internal bool ArenaDragging => arenaDragMoved;
    internal int ArenaDropBench => arenaEnvironment.Feedback.HoverBench;
    private void CancelArenaDrag()
    {
        itemDrag = null; itemDragMoved = false;
        arenaDrag = null; arenaDragMoved = false; arenaDragPosition = null;
        planning.DraggedUnit = null;
        if (battle is not null) battle.DraggedUnit = null;
        arenaEnvironment.Feedback.UnitDragMoved = false;
        arenaEnvironment.Feedback.HoverCell = null; arenaEnvironment.Feedback.HoverBench = -1;
    }

    private void ArenaPointer()
    {
        var feedback = arenaEnvironment.Feedback;
        feedback.UnitDragMoved = false; feedback.HoverCell = null; feedback.HoverBench = -1;
        bool editable = Session is not null && !IsSpectating && viewed == localSeat && !Session.State.Over && Human.Alive;
        if (!editable || arenaDrag is { } old && (old.Phase != Session!.Rounds.Phase || Human.Roster.At(old.From)?.Id != old.Id))
        { arenaDrag = null; arenaDragMoved = false; arenaDragPosition = null; }
        var mouse = ImGui.GetMousePos();
        bool inside = ImGui.IsWindowHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem) && mouse.X >= Left && mouse.X < Left + Width && mouse.Y >= Top && mouse.Y < Top + Height;
        var ground = ArenaPicking.Ground(mouse.X - Left, mouse.Y - Top, Width, Height, battle?.Combat.Columns ?? 7, arenaEnvironment.Camera);
        var spot = inside && ground is { } p ? ArenaPicking.Spot(p, battle?.Combat.Columns > 7, battle is null) : null;
        if (editable && spot is { } ownedSpot) hoveredOwnedUnit = Human.Roster.At(ownedSpot)?.Id;
        int shelfHover = -1; double nearest = 30 * 30;
        if (battle?.Combat.Columns is null or 7)
            for (int i = 0; i < Math.Min(10, Session!.State.Players[viewed].Items.Count); i++)
            {
                var point = ItemShelfView.Screen(i, Width, Height, arenaEnvironment.Camera);
                Buttons[$"shelfItem/{i}"] = new(Left + point.X, Top + point.Y);
                double distance = Math.Pow(mouse.X - Left - point.X, 2) + Math.Pow(mouse.Y - Top - point.Y, 2);
                if (inside && distance < nearest) { nearest = distance; shelfHover = i; }
            }
        feedback.HoverItem = shelfHover;
        if (CameraNavigation(inside, spot, shelfHover)) { CancelArenaDrag(); return; }
        if (editable && shelfHover >= 0)
        {
            itemDropSlot = shelfHover; spot = null;
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                itemDrag = new(shelfHover, Human.Items[shelfHover], Human.Items.ToArray(), mouse, Session!.Rounds.Phase);
                itemDragMoved = false; selectedItem = shelfHover; selectedUnit = null;
            }
        }
        ItemDragPointer(editable, spot);
        feedback.ItemDragging = itemDragMoved;
        feedback.ItemDragOver = itemDragMoved && itemDropSlot != itemDrag?.Index ? itemDropSlot : -1;
        arenaEnvironment.Items.DragIndex = itemDragMoved ? itemDrag?.Index ?? -1 : -1;
        arenaEnvironment.Items.DragPosition = ground;
        if (editable && inside && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && spot is { } start)
        {
            if (selectedItem is null && Human.Roster.At(start) is { } unit)
                arenaDrag = (unit.Id, start, mouse, Session!.Rounds.Phase);
            else Select(start);
        }
        if (arenaDrag is { } drag)
        {
            arenaDragMoved |= Math.Sqrt(Math.Pow(mouse.X - drag.Start.X, 2) + Math.Pow(mouse.Y - drag.Start.Y, 2)) > 4;
            if (arenaDragMoved && ground is { } target) arenaDragPosition = target with { Y = .6 };
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right)) { arenaDrag = null; arenaDragMoved = false; arenaDragPosition = null; }
            else if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
            {
                if (arenaDragMoved && partnerDropHovered) { Act("portal", new { uid = drag.Id }); selectedUnit = null; }
                else if (arenaDragMoved && sellDropHovered) { Act("sell", new { uid = drag.Id }); selectedUnit = null; }
                else if (inside && spot is { } destination)
                {
                    if (arenaDragMoved) { if (destination != drag.From) Act("move", new { from = Spot(drag.From), to = Spot(destination) }); selectedUnit = null; }
                    else Select(drag.From);
                }
                arenaDrag = null; arenaDragMoved = false; arenaDragPosition = null;
            }
        }
        if (arenaDragMoved)
        {
            feedback.UnitDragMoved = true;
            if (spot is { IsBoard: true } cell) feedback.HoverCell = (cell.Column, cell.Row);
            else if (spot is { IsBoard: false } bench) feedback.HoverBench = bench.Index;
        }
        else if (!itemDragMoved && shelfHover >= 0)
        {
            ItemTooltip(Session!.State.Players[viewed].Items[shelfHover]);
        }
        else if (!itemDragMoved && inside && spot is { } hover && Session!.State.Players[viewed].Roster.At(hover) is { } unit)
        {
            ImGui.SetNextWindowSize(new(340, 0)); ImGui.BeginTooltip(); UnitDetails(unit); ImGui.EndTooltip();
        }
        planning.DraggedUnit = battle is null && arenaDragMoved ? arenaDrag?.Id : null;
        planning.DragPosition = arenaDragPosition;
        if (battle is not null) { battle.DraggedUnit = arenaDragMoved ? arenaDrag?.Id : null; battle.DragPosition = arenaDragPosition; }
    }

    private bool SellDropZone()
    {
        if (!arenaDragMoved || arenaDrag is not { } drag || Human.Roster.At(drag.From) is not { } unit) return false;
        var line = Rules.Lines.Concat(Rules.ExtraLines).Single(l => l.Id == unit.Line);
        bool canSell = !unit.NoSell && !line.NoSell && (Session!.Rounds.Phase != MatchPhase.Combat || !drag.From.IsBoard);
        sellDropHovered = ImGui.IsWindowHovered();
        Button("dropSell", canSell ? $"Drop to sell {UnitName(unit)} for {UnitValues.SellValue(line, unit.Stage)} gold" : "This unit cannot be sold", size.X - 48, 165, sellDropHovered && canSell);
        return true;
    }

    private void PartnerDropZone()
    {
        if (!Session!.State.Duos) return;
        PortalRequest request;
        if (itemDragMoved && itemDrag is { } item) request = new(ItemIndex: item.Index);
        else if (arenaDragMoved && arenaDrag is { } drag) request = new(Uid: drag.Id);
        else return;
        string eligibility = Session.Units.Portal.CanSend(Human, request, Session.Rounds.Phase, Session.Rounds.RoundType);
        string label = eligibility switch
        {
            "ok" => "Drop to send to partner",
            "bench" => "Move to bench before sending",
            "phase" => "Send during planning",
            "limit" => "No sends remaining this round",
            "full" => "Partner's bench is full",
            "partner" => "Partner unavailable",
            "itemsFull" => "Partner's inventory is full",
            _ => "Cannot send this to partner"
        };
        Button("dropPartner", label, 190, 64, eligibility == "ok");
        partnerDropHovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem)
            && ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());
    }
}
