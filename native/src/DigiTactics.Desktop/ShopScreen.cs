using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed class ShopScreen
{
    private readonly ContentCatalog catalog;
    private ShopSession session;
    private string status = "Choose a ruleset to explore the native shop and roster.";
    internal int OwnedCount => session.Roster.Units.Count;
    internal Ruleset Rules => session.Rules;
    internal bool BattleRequested { get; private set; }
    internal CombatEntry[] BattleRoster() => session.Roster.Units.Select((unit, i) => new CombatEntry(unit.Line, unit.Stage, i % 7, 4 + i / 7, Items: [])).ToArray();
    internal string? RequestedAction { get; private set; }
    internal (string Variant, string Line, int Stage)? Preview
    {
        get
        {
            var owned = session.Roster.Units.LastOrDefault();
            string? line = owned?.Line ?? session.Slots.FirstOrDefault(id => id is not null);
            return line is null ? null : (session.Rules.Id, line, owned?.Stage ?? 0);
        }
    }

    public ShopScreen(ContentCatalog catalog)
    {
        this.catalog = catalog;
        session = new(catalog.Variants[0], 143);
    }

    public void Draw(int2 size)
    {
        RequestedAction = null;
        BattleRequested = false;
        ImGui.SetNextWindowPos(new float2(20, 20));
        ImGui.SetNextWindowSize(new float2(size.X - 40, Math.Max(300, size.Y * .53f)));
        ImGui.Begin("Digi-Tactics - native migration"u8, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse);
        ImGui.Text("DIGI-TACTICS / BRUTAL"u8);
        ImGui.Text("Shop and model workbench. Full matches, production rendering and online play are still being ported."u8);
        ImGui.Separator();
        if (ImGui.BeginCombo("Ruleset / pace"u8, $"{session.Rules.Name} / {session.Rules.Pace}"))
        {
            foreach (var variant in catalog.Variants)
                if (ImGui.Selectable($"{variant.Name} / {variant.Pace}", variant.Id == session.Rules.Id))
                    session = new(variant, 143);
            ImGui.EndCombo();
        }
        ImGui.Text($"{session.Gold} gold    Level {session.Level}    {session.Roster.Units.Count}/9 bench slots");
        if (ImGui.Button($"Refresh shop ({session.Rules.RerollCost} gold)"))
            status = session.Reroll() ? "Shop refreshed." : "Not enough gold.";
        ImGui.SameLine();
        if (ImGui.Button("Reset sandbox"u8)) { session = new(session.Rules, 143); status = "Sandbox reset."; }
        ImGui.SameLine(); if (ImGui.Button("Test battle"u8)) BattleRequested = true;
        ImGui.SeparatorText("SHOP"u8);
        for (int slot = 0; slot < session.Slots.Count; slot++)
        {
            if (slot > 0) ImGui.SameLine();
            string? id = session.Slots[slot];
            string label = id is null ? $"Empty##shop{slot}" : $"{session.Line(id).Forms[0]}\n{session.Line(id).Price} gold##shop{slot}";
            if (ImGui.Button(label, new float2(190, 72)) && id is not null)
                status = session.Buy(slot) ? "Unit purchased." : "Cannot buy: check gold or bench space.";
        }
        ImGui.SeparatorText("ROSTER"u8);
        foreach (var unit in session.Roster.Units.ToArray())
        {
            var line = session.Line(unit.Line);
            ImGui.Text($"{line.Forms[unit.Stage]}   {new string('*', unit.Stage + 1)}   {string.Join(" / ", line.Traits)}");
            ImGui.SameLine();
            if (ImGui.SmallButton($"Sell##{unit.Id}")) { session.Sell(unit.Id); status = "Unit returned to the shared pool."; }
        }
        ImGui.Separator();
        ImGui.Text(status);
        if (ImGui.SmallButton("Preview attack"u8)) RequestedAction = "attack";
        ImGui.SameLine(); if (ImGui.SmallButton("Special"u8)) RequestedAction = "special";
        ImGui.SameLine(); if (ImGui.SmallButton("Victory"u8)) RequestedAction = "win";
        ImGui.SameLine(); if (ImGui.SmallButton("Idle"u8)) RequestedAction = "idle";
        ImGui.Text("Owned three-star lines are excluded from this player's shop. Other players keep their own eligibility."u8);
        ImGui.Separator();
        ImGui.Text("BRUTAL Copyright (c) 2024 Ahwoo Limited. Digi-Tactics is an unofficial, non-commercial fan project."u8);
        ImGui.End();
    }
}
