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
    internal bool ArenaDragging => arenaDragMoved;
    internal int ArenaDropBench => arenaEnvironment.Feedback.HoverBench;
    private void CancelArenaDrag()
    {
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
        bool inside = ImGui.IsWindowHovered() && mouse.X >= Left && mouse.X < Left + Width && mouse.Y >= Top && mouse.Y < Top + Height;
        var ground = ArenaPicking.Ground(mouse.X - Left, mouse.Y - Top, Width, Height, battle?.Combat.Columns ?? 7);
        var spot = inside && ground is { } p ? ArenaPicking.Spot(p, battle?.Combat.Columns > 7, battle is null) : null;
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
                if (inside && spot is { } destination)
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
        else if (inside && spot is { } hover && Session!.State.Players[viewed].Roster.At(hover) is { } unit)
        {
            ImGui.SetNextWindowSize(new(340, 0)); ImGui.BeginTooltip(); UnitDetails(unit.Line, unit.Stage); ImGui.EndTooltip();
        }
        planning.DraggedUnit = battle is null && arenaDragMoved ? arenaDrag?.Id : null;
        planning.DragPosition = arenaDragPosition;
        if (battle is not null) { battle.DraggedUnit = arenaDragMoved ? arenaDrag?.Id : null; battle.DragPosition = arenaDragPosition; }
    }
}
