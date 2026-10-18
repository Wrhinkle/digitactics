using System.Text.Json;
using DigiTactics.Rules;

namespace DigiTactics.Presentation;

public sealed class AbilityElements
{
    private readonly Dictionary<string, Dictionary<string, string[]>> variants;
    public AbilityElements(string path) => variants = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string[]>>>(File.ReadAllText(path))!;
    public string Get(string variant, string line, int stage) => variants[variant][line][stage];
}

/// <summary>Audio presentation for one viewed battle. Cues retain simulation ordering; viewing an
/// existing battle starts at its present cursor so past sounds are never replayed when scouting.</summary>
public sealed class CombatAudioCues(CombatSimulation combat, string variant, AbilityElements elements, bool skipHistory = true)
{
    private int cursor = skipHistory ? combat.Events.Count : 0;
    private double? castTime;
    private string? castKind;
    public void Update(Action<string> play)
    {
        while (cursor < combat.Events.Count)
        {
            var cue = combat.Events[cursor++];
            var unit = cue.Unit >= 0 && cue.Unit < combat.Units.Count ? combat.Units[cue.Unit] : null;
            string? sound = cue.Type switch
            {
                "hit" when unit is not null && cue.Target is not null => cue.Big ? "hit_big" : "hit_normal",
                "shot" when unit is not null && cue.Target is not null => "shot",
                "shield" or "death" => cue.Type,
                "heal" when castKind == "heal" && castTime is { } time && Math.Abs(time - cue.Time) < 1e-6 => "heal",
                _ => null
            };
            if (cue.Type == "cast" && unit is not null)
            {
                castTime = cue.Time; castKind = unit.Line.Ability.Kind;
                sound = $"cast_{elements.Get(variant, unit.Line.Id, unit.Stage)}_{unit.Stage}";
            }
            if (sound is not null) play(sound);
        }
    }
}
