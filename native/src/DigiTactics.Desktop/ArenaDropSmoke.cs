using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Real mouse input verifies sale rewards and partner transfer after the match smoke's battles.</summary>
internal sealed class ArenaDropSmoke
{
    private int step, id, gold, reward, sends, bench;
    private float2 source;
    public string? CaptureName => step == 3 ? "sell-drop" : step == 14 ? "partner-drop" : null;
    public bool Input(MatchScreen screen)
    {
        var session = screen.Session!;
        var player = session.State.Players[0];
        var io = ImGui.GetIO();
        void Move(float2 point) => io.AddMousePosEvent(point.X, point.Y);
        void Down() => io.AddMouseButtonEvent(0, true);
        void Up() => io.AddMouseButtonEvent(0, false);
        void StartBoard()
        {
            var entry = player.Roster.Board.First(); id = entry.Unit.Id;
            source = screen.Buttons[$"hex/{entry.Spot.Column}/{entry.Spot.Row}"];
            Move(source); Down();
        }
        switch (step++)
        {
            case 0:
                StartBoard(); gold = player.Economy.Gold;
                var unit = player.Roster.Entries.Single(e => e.Unit.Id == id).Unit;
                reward = UnitValues.SellValue(session.State.Rules.Lines.Concat(session.State.Rules.ExtraLines).Single(l => l.Id == unit.Line), unit.Stage);
                break;
            case 1: Move(new(source.X + 12, source.Y)); break;
            case 2: Move(screen.Buttons["dropSell"]); break;
            case 3: Up(); break;
            case 4:
                if (player.Roster.Entries.Any(e => e.Unit.Id == id) || player.Economy.Gold != gold + reward || screen.ArenaDragging)
                    throw new InvalidOperationException("Dragging to the shop did not sell once for the expected gold.");
                if (!session.State.Duos) return true;
                StartBoard(); sends = player.PortalSends; bench = player.Roster.FreeBenchSlot;
                break;
            case 5: Move(new(source.X + 12, source.Y)); break;
            case 6: Move(screen.Buttons["dropPartner"]); break;
            case 7: Up(); break;
            case 8:
                if (!player.Roster.Entries.Any(e => e.Unit.Id == id) || player.PortalSends != sends)
                    throw new InvalidOperationException("Partner drop accepted a deployed unit.");
                Move(source); Down(); break;
            case 9: Move(screen.Buttons[$"arenaBench/{bench}"]); break;
            case 10: Up(); break;
            case 11:
                if (player.Roster.At(UnitSpot.Bench(bench))?.Id != id) throw new InvalidOperationException("Transfer setup failed to bench its unit.");
                Move(screen.Buttons[$"arenaBench/{bench}"]); Down(); break;
            case 12:
                var point = screen.Buttons[$"arenaBench/{bench}"]; Move(new(point.X + 12, point.Y)); break;
            case 13: Move(screen.Buttons["dropPartner"]); break;
            case 14: Up(); break;
            case 15:
                var partner = session.Units.Portal.Partner(player)!;
                if (player.Roster.Entries.Any(e => e.Unit.Id == id) || player.PortalSends != sends + 1 || !partner.Incoming.Any(e => e.Uid == id) || screen.ArenaDragging)
                    throw new InvalidOperationException("Partner drop failed to transfer the benched unit exactly once.");
                return true;
        }
        return false;
    }
}
