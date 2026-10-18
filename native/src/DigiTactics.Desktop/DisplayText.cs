using System.Text.Json;

namespace DigiTactics.Desktop;

internal sealed record DisplayEntry(string Name, string Description);
internal sealed class DisplayText
{
    private readonly Dictionary<string, Dictionary<string, DisplayEntry>> variants = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, DisplayEntry>>>(
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "content/display.json")), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    public DisplayEntry Get(string variant, string id) => variants[variant].GetValueOrDefault(id) ?? new(id, "");
}
