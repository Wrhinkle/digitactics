using System.Text.Json.Nodes;
using Brutal.ImGuiApi;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal (Brutal.Numerics.float2 Min, Brutal.Numerics.float2 Max) FarmManagementBounds { get; private set; }
    private void FarmCaretakerControls(JsonNode state)
    {
        long now = FarmCaretaker.Time(state, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        var current = FarmCaretaker.Current(state, now);
        CollectionText($"{profile!.Progress.Bits:N0} Bits · Current care: {current?.Name ?? "Manual care"}");
        if (current is not null)
        {
            var remaining = TimeSpan.FromMilliseconds(FarmCaretaker.Until(state) - now);
            CollectionText($"{(int)remaining.TotalHours}h {remaining.Minutes}m remaining · Expires {DateTimeOffset.FromUnixTimeMilliseconds(FarmCaretaker.Until(state)).LocalDateTime:g}");
        }
        else if (FarmCaretaker.Until(state) > 0) CollectionText("Coverage expired. No Bits were charged automatically.");
        CollectionText("Prepaid care covers the whole farm. Supplies are included. Renew explicitly: no automatic charges or debt.");
        CollectionText("Without staff, each absence applies at most two hours of needs decay. Food and bandage supplies still refill in real time.");
        foreach (var plan in FarmCaretaker.Plans)
        {
            ImGui.Separator(); FarmHeading(plan.Name, 24);
            CollectionText(plan.Id == "basic"
                ? "Keeps hunger, energy and happiness at least 40/100, cleans the room and treats sickness. Preserves current discipline."
                : "Keeps hunger, energy, happiness and discipline at 100/100, cleans and treats sickness, and maintains ideal weight.");
            var quote = FarmCaretaker.Quote(state, plan, now);
            CollectionText($"{plan.Price:N0} Bits / 24 hours" + (quote.Upgrade ? " · Upgrade preserves your paid expiry" : ""));
            ImGui.BeginDisabled(quote.Blocked || profile.Progress.Bits < quote.Price);
            string action = quote.Blocked ? "Available after Full care expires" : quote.Upgrade ? $"Upgrade remaining time · {quote.Price:N0} Bits" : $"Add 24 hours · {quote.Price:N0} Bits";
            if (Button("farmHire/" + plan.Id, action, ImGui.GetContentRegionAvail().X, 40))
                FarmAct(FarmCareAction.HireCaretaker, plan.Id);
            ImGui.EndDisabled();
        }
        ImGui.Separator();
        CollectionText("Caretakers do not earn Bits, XP, training stats or evolution care credits. Existing care history is preserved.");
        if (farm!.Error is { } error) CollectionText(error);
        else if (farm.Message is { } message) CollectionText(message);
    }
}
