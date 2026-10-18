using System.Text.Json.Nodes;
using Brutal.ImGuiApi;
using DigiTactics.Storage;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool showFarm;
    private string? farmPet;
    private bool farmManagement;
    private FarmView? farmView;
    private string? farmViewError;
    private bool farmWasVisible;
    private readonly List<(string Text, double Remaining)> farmNotices = [];
    private readonly FarmPops farmPops = new();
    private GpuTexture? farmMoodTexture;
    private GpuTexture? farmSymbolTexture;
    private double farmMoodTime;
    private int FarmSceneX => 16;
    private int FarmSceneTop => (int)ClientPageTop + 104;
    private int FarmCardWidth => Math.Clamp((int)(size.X * .30f), 350, 400);
    private int FarmSceneWidth => Math.Max(250, size.X - FarmCardWidth - 44);
    private int FarmSceneHeight => Math.Max(200, size.Y - FarmSceneTop - 16);
    public int FarmDrawCalls => farmView?.DrawCalls ?? 0;
    public string? FarmTheme => farmView?.Theme;
    internal string? FarmSelectedResident => farmPet;
    internal (float X, float Y)? FarmPilePoint(string id, int key)
    {
        if (farmView is null) return null;
        foreach (var pile in farmView.PileAnchors(FarmSceneWidth, FarmSceneHeight))
            if (pile.Id == id && pile.Key == key) return (FarmSceneX + pile.X, FarmSceneTop + pile.Y);
        return null;
    }
    private void FarmAct(FarmCareAction action, string? option = null, int pile = 0, string? resident = null)
    {
        string? before = farm!.Save;
        string? target = resident ?? farmPet;
        if (action == FarmCareAction.Play && option is null) option = farmView?.NearestPlaymate(target);
        farm.Act(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), action, target, option, pile);
        if (farm.Error is null && farm.LastResult is { } receipt)
        {
            farmView?.Care(target, action, option, receipt, before);
            farmPops.Add(target, receipt);
            bool asleep = target is not null && before is not null && JsonNode.Parse(before)?["pets"]?[target]?["flags"]?["asleep"]?.GetValue<bool>() == true;
            if (target is not null && FarmFeedback.Symbol(action, option, receipt, asleep) is { } symbol) FarmSymbolPop(target, symbol);
            if (FarmFeedback.Sound(action, receipt) is { } sound) audio.Play(sound);
        }
    }
    private void UpdateFarmView(double delta)
    {
        farmMoodTime += delta;
        for (int i = farmNotices.Count - 1; i >= 0; i--)
        {
            var notice = farmNotices[i];
            if (notice.Remaining <= delta) farmNotices.RemoveAt(i);
            else farmNotices[i] = (notice.Text, notice.Remaining - delta);
        }
        if (!showFarm) { farmWasVisible = false; farmPops.Clear(); }
        if (!showFarm || farm?.Save is null || farm.Error is not null) return;
        try
        {
            farmView ??= new();
            farmView.Sync(farm.Save, profile!.Storage.GetValueOrDefault("dt.legendevo.v1"), profile.Storage.GetValueOrDefault("dt.cosmetics.v1"));
            FarmEvents(farm.DrainEvents(), !farmWasVisible); farmWasVisible = true;
            farmView.Update(delta); farmViewError = null;
            foreach (var feedback in farmView.DrainFeedback()) FarmSymbolPop(feedback.Id, feedback.Symbol);
            var anchors = farmView.Anchors(FarmSceneWidth, FarmSceneHeight, .3).ToDictionary(a => a.Id);
            farmPops.Update(delta, id => anchors.TryGetValue(id, out var a) ? (a.X, a.Y) : null);
        }
        catch (Exception error) when (error is InvalidDataException or IOException or System.Text.Json.JsonException or InvalidOperationException or KeyNotFoundException)
        { farmViewError = "Room view unavailable: " + error.Message; }
    }
    private void FarmEvents(JsonObject[] events, bool away)
    {
        void Notice(string text) { farmNotices.Add((text, 4.2)); if (farmNotices.Count > 5) farmNotices.RemoveAt(0); }
        if (away)
        {
            var parts = new List<string>();
            foreach (var (type, singular, plural) in new[] { ("poop", "poop", "poops"), ("toilet", "toilet trip", "toilet trips"), ("sick", "got sick", "got sick"), ("mistake", "care mistake", "care mistakes") })
            {
                int count = events.Count(e => e["type"]?.GetValue<string>() == type);
                if (count > 0) parts.Add($"{count} {(count == 1 ? singular : plural)}");
            }
            if (parts.Count > 0) Notice("While you were away: " + string.Join(", ", parts) + ".");
            return;
        }
        foreach (var e in events)
        {
            string id = e["id"]!.GetValue<string>();
            string name = CosmeticCollection.Item("legend", id)?.Name ?? id;
            string? why = e["why"]?.GetValue<string>();
            switch (e["type"]?.GetValue<string>())
            {
                case "poop": FarmSymbolPop(id, FarmSymbol.Poop); audio.Play("place"); break;
                case "toilet": farmView?.Toilet(id); break;
                case "sick": Notice($"{name} got sick{(why == "dirty" ? " from the dirty room" : "")}. Use a bandage or medicine."); break;
                case "tantrum": Notice($"{name} is throwing a tantrum! Scold it."); farmView?.React(id, "taunt"); FarmSymbolPop(id, FarmSymbol.Tantrum); break;
                case "mistake":
                    string reason = why switch { "hungry" => "was hungry too long", "sad" => "was sad too long", "lights" => "was sleepy with the lights on", "tantrum" => "had a tantrum nobody handled", "sick" => "was sick too long", _ => "" };
                    Notice($"Care mistake: {name} {reason}."); FarmSymbolPop(id, FarmSymbol.Mistake); break;
            }
        }
    }
    private void DrawFarmPops()
    {
        var draw = ImGui.GetWindowDrawList();
        foreach (var pop in farmPops.Items)
        {
            if (pop.Age < 0 || pop.Position is not { } p) continue;
            if (pop.Symbol is { } symbol)
            {
                farmSymbolTexture ??= new(DigiTactics.Assets.PngImage.Decode(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "content/farm-symbols.png"))), new(MinFilter: 9729, WrapS: 33071, WrapT: 33071), false);
                float x = FarmSceneX + p.X, y = FarmSceneTop + p.Y + pop.Offset;
                draw.AddImage(new ImTextureRef { _TexID = new((nint)farmSymbolTexture.Id) }, new(x - 14, y - 28), new(x + 14, y), new((int)symbol / 18f, 0), new(((int)symbol + 1) / 18f, 1), new ImColor8(255, 255, 255, (byte)Math.Clamp((int)(pop.Opacity * 255), 0, 255)));
                continue;
            }
            var textSize = ImGui.CalcTextSize(pop.Text);
            var position = new Brutal.Numerics.float2(FarmSceneX + p.X - textSize.X / 2, FarmSceneTop + p.Y + pop.Offset - textSize.Y);
            byte alpha = (byte)Math.Clamp((int)(pop.Opacity * 255), 0, 255);
            draw.AddText(ImGui.GetFont(), ImGui.GetFontSize(), position + new Brutal.Numerics.float2(1, 1), new ImColor8(0, 0, 0, alpha), pop.Text);
            draw.AddText(ImGui.GetFont(), ImGui.GetFontSize(), position, new ImColor8(255, 211, 106, alpha), pop.Text);
        }
    }
    private void FarmSymbolPop(string id, FarmSymbol symbol)
    {
        if (farmView is null) return;
        foreach (var anchor in farmView.Anchors(FarmSceneWidth, FarmSceneHeight, .3))
            if (anchor.Id == id) { farmPops.AddSymbol(id, symbol, (anchor.X, anchor.Y)); return; }
    }
    private void DrawFarmMood(FarmMood mood, float x, float y)
    {
        if (mood.Icon == 9) return;
        farmMoodTexture ??= new(DigiTactics.Assets.PngImage.Decode(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "content/farm-moods.png"))), new(MinFilter: 9729, WrapS: 33071, WrapT: 33071), false);
        // Original 1.6-second bubble bob, with the same three-pixel excursion.
        y -= (float)(1.5 * (1 - Math.Cos(farmMoodTime * Math.PI * 2 / 1.6)));
        var draw = ImGui.GetWindowDrawList();
        draw.AddCircleFilled(new(x + 1, y + 2), 14, new ImColor8(0, 0, 0, 70));
        draw.AddCircleFilled(new(x, y), 13, new ImColor8(255, 255, 255, 235));
        draw.AddImage(new ImTextureRef { _TexID = new((nint)farmMoodTexture.Id) }, new(x - 13, y - 13), new(x + 13, y + 13), new(mood.Icon / 10f, 0), new((mood.Icon + 1) / 10f, 1));
    }
}
