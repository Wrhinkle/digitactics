namespace DigiTactics.Rules;

/// <summary>Composes mode and Duos unit effects for every source of purchases, sales and merges.</summary>
public sealed class MatchUnitLifecycle : IMatchShopHooks, IMatchItemHooks, IMatchPortalHooks, IMatchCarouselHooks
{
    private readonly Func<RoundPosition> position;
    private readonly IMatchShopHooks? mode;
    private readonly UnderlordsPlanning? underlords;
    private readonly AutoChessPlanning? autoChess;
    private readonly DuosPlanning duos;

    public MatchUnitLifecycle(MatchState state, Func<RoundPosition> position)
    {
        this.position = position;
        string id = state.Rules.Id.Split('/')[0];
        mode = id switch
        {
            "arcane" => new ArcanePlanning(state),
            "coliseum" => new ColiseumPlanning(state),
            "fates" => new FatesPlanning(state),
            "galaxies" => new GalaxiesPlanning(state),
            "gizmos" => new GizmosPlanning(state),
            _ => null
        };
        if (id is "underlords" or "underlords_ko") underlords = new(state);
        if (id == "autochess") autoChess = new(state);
        Shop = new(this); Items = new(state, this); Portal = new(state, this); Carousel = new(state, this);
        duos = new(state, Portal);
        foreach (var player in state.Players)
            player.Roster.CombineHandler = (unit, sources) => Combined(player, unit, sources);
    }

    public PlanningShopCommands Shop { get; }
    public PlanningItems Items { get; }
    public MatchPortal Portal { get; }
    public MatchCarousel Carousel { get; }

    public IReadOnlyList<PlanningUnit> Combine(MatchPlayer player, MatchPhase phase)
    {
        var result = player.Roster.Combine(phase == MatchPhase.Combat && player.IsHuman, player.GiveItem);
        player.Shop.ClearMaxed();
        return result;
    }

    public PlanningUnit Bought(MatchPlayer player, PlanningUnit unit, int slot, MatchPhase phase) =>
        mode?.Bought(player, unit, slot, phase) ?? unit;

    public void Sold(MatchPlayer player, PlanningUnit unit)
    {
        mode?.Sold(player, unit);
        autoChess?.Sold(player);
    }

    private void ModeCombined(MatchPlayer player, PlanningUnit unit, IReadOnlyList<PlanningUnit> sources)
    {
        mode?.Combined(player, unit, sources);
        underlords?.Combined(player, unit);
    }

    public void Combined(MatchPlayer player, PlanningUnit unit, IReadOnlyList<PlanningUnit> sources)
    {
        ModeCombined(player, unit, sources);
        if (Portal.Partner(player) is { Alive: true } partner) duos.PartnerCombined(partner, position());
    }

    // Transfers and carousel picks already dispatch the partner callback after the owner's.
    void IMatchPortalHooks.Combined(MatchPlayer player, PlanningUnit unit, IReadOnlyList<PlanningUnit> sources) => ModeCombined(player, unit, sources);
    void IMatchCarouselHooks.Combined(MatchPlayer player, PlanningUnit unit, IReadOnlyList<PlanningUnit> sources) => ModeCombined(player, unit, sources);
    public void PartnerCombined(MatchPlayer partner, MatchPlayer player, PlanningUnit unit) => duos.PartnerCombined(partner, position());
    public void Sent(MatchPlayer player, PortalEvent transfer) => duos.Sent(player, transfer);
    public string? EquipVeto(MatchPlayer player, PlanningUnit unit, string item, string? combined) => null;
    // ModeItemConsumables owns all current consumables; unmatched items are a no-op in the source game.
    public string UseConsumable(MatchPlayer player, int itemIndex, PlanningUnit unit) => "none";
}
