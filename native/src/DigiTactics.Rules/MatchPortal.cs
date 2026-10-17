namespace DigiTactics.Rules;

public sealed record PortalArrival(string Kind, int From, int? Uid = null, string? Line = null, int? Stage = null, string? Item = null, bool Gift = false);
public readonly record struct PortalRequest(int? Uid = null, int? ItemIndex = null);
public sealed record PortalEvent(MatchPlayer From, MatchPlayer To, PlanningUnit? Unit, string? Item);
public sealed record PortalResult(string Status, IReadOnlyList<PlanningUnit> Evolved);
public interface IMatchPortalHooks
{
    void Combined(MatchPlayer player, PlanningUnit unit, IReadOnlyList<PlanningUnit> sources);
    void PartnerCombined(MatchPlayer partner, MatchPlayer player, PlanningUnit unit);
    void Sent(MatchPlayer player, PortalEvent transfer);
}

/// <summary>Duos transfers preserve shared pool ownership and dispatch both partners' hooks in order.</summary>
public sealed class MatchPortal(MatchState state, IMatchPortalHooks hooks)
{
    private readonly Dictionary<string, UnitLine> lines = state.Rules.Lines.Concat(state.Rules.ExtraLines).ToDictionary(l => l.Id);
    public MatchPlayer? Partner(MatchPlayer player) => !state.Duos || player.Team is not { } team ? null :
        state.Teams[team].Members.FirstOrDefault(p => p != player);
    public int Limit(MatchPlayer player) => state.Duos ? state.Rules.Duo!.Sends + (player.Augments.Contains("duo_wide_portal") ? 1 : 0) : 0;
    public void Reset(MatchPlayer player) { player.PortalSends = 0; player.Incoming.Clear(); }
    public int PartnerCopies(MatchPlayer player, string line, int stage) => Partner(player) is { Alive: true } partner && player.PortalSends < Limit(player)
        ? partner.Roster.Entries.Count(e => e.Unit.Line == line && e.Unit.Stage == stage) : 0;

    public string CanSend(MatchPlayer player, PortalRequest request, MatchPhase phase, string roundType)
    {
        if (!state.Duos) return "solo";
        if (Partner(player) is not { Alive: true } partner || !player.Alive) return "partner";
        if (phase != MatchPhase.Planning || roundType == "carousel") return "phase";
        if (player.PortalSends >= Limit(player)) return "limit";
        if (request.Uid is { } uid)
        {
            var entry = player.Roster.Entries.FirstOrDefault(e => e.Unit.Id == uid);
            if (entry is null || entry.Spot.IsBoard) return "bench";
            var unit = entry.Unit;
            if (unit.NoSell || lines[unit.Line].NoSell) return "noSend";
            if (partner.Roster.FreeBenchSlot >= 0) return "ok";
            int same = partner.Roster.Entries.Count(e => e.Unit.Line == unit.Line && e.Unit.Stage == unit.Stage);
            return unit.Stage < 2 && same >= lines[unit.Line].Copies - 1 ? "ok" : "full";
        }
        if (request.ItemIndex is { } index)
        {
            if ((uint)index >= player.Items.Count) return "none";
            return partner.Items.Count < state.Rules.Loot!.ItemCap ? "ok" : "itemsFull";
        }
        return "none";
    }

    public PortalResult Send(MatchPlayer player, PortalRequest request, MatchPhase phase, string roundType)
    {
        string status = CanSend(player, request, phase, roundType);
        if (status != "ok") return new(status, []);
        var partner = Partner(player)!; player.PortalSends++;
        PlanningUnit? unit = null; string? item = null; IReadOnlyList<PlanningUnit> evolved = [];
        if (request.Uid is { } uid)
        {
            unit = player.Roster.Remove(uid, false)!;
            partner.Incoming.Add(new("unit", player.Id, unit.Id, unit.Line, unit.Stage));
            evolved = partner.Roster.Combine(false, partner.GiveItem, unit, onCombine: (up, sources) =>
            {
                hooks.Combined(partner, up, sources);
                if (Partner(partner) is { Alive: true } other)
                    hooks.PartnerCombined(other, partner, partner.Roster.Entries.FirstOrDefault(e => e.Unit.Id == up.Id)?.Unit ?? up);
            });
            partner.Shop.ClearMaxed();
        }
        else
        {
            int index = request.ItemIndex!.Value; item = player.Items[index]; player.Items.RemoveAt(index);
            partner.Items.Add(item); partner.Incoming.Add(new("item", player.Id, Item: item));
        }
        var transfer = new PortalEvent(player, partner, unit, item);
        hooks.Sent(player, transfer); hooks.Sent(partner, transfer);
        return new(unit is null ? "item" : "unit", evolved);
    }

    public void AiTurn(MatchPlayer player, MatchPhase phase, string roundType)
    {
        if (Partner(player) is not { Alive: true } partner) return;
        int Count(MatchPlayer p, PlanningUnit u) => p.Roster.Entries.Count(e => e.Unit.Line == u.Line && e.Unit.Stage == u.Stage);
        for (int guard = 0; guard < 4 && player.PortalSends < Limit(player); guard++)
        {
            PlanningUnit? best = null; double bestScore = 0;
            foreach (var unit in player.Roster.Bench.OfType<PlanningUnit>())
            {
                if (CanSend(player, new(Uid: unit.Id), phase, roundType) != "ok") continue;
                int theirs = Count(partner, unit), own = Count(player, unit); var line = lines[unit.Line];
                double score = theirs >= line.Copies - 1 ? 10 + unit.Stage * 5 + line.Tier : theirs > own ? 3 + theirs + line.Tier * 0.1 : 0;
                if (score > bestScore) { bestScore = score; best = unit; }
            }
            if (best is not null) { Send(player, new(Uid: best.Id), phase, roundType); continue; }
            if (player.Items.Count >= 7 && partner.Items.Count <= 4)
            {
                int index = player.Items.FindIndex(id => state.Rules.Items.Any(i => i.Id == id && i.Component));
                if (index >= 0 && CanSend(player, new(ItemIndex: index), phase, roundType) == "ok")
                { Send(player, new(ItemIndex: index), phase, roundType); continue; }
            }
            break;
        }
    }
}
