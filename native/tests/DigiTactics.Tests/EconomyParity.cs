using System.Text.Json;
using DigiTactics.Rules;

internal static class EconomyParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var fixture = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "tests/fixtures/economy.json")));
        int checks = 0;
        void Check(bool condition, string context)
        {
            checks++;
            if (!condition) throw new InvalidOperationException($"Economy parity: {context}");
        }
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            string id = trace.GetProperty("variant").GetString()!;
            var rules = catalog.Variants.Single(v => v.Id == id);
            var player = new PlayerEconomy(rules.Economy!);
            var position = RoundPosition.First;
            foreach (var round in trace.GetProperty("rounds").EnumerateArray())
            {
                Check(position.Stage == round.GetProperty("stage").GetInt32() &&
                    position.Round == round.GetProperty("round").GetInt32(), id + " round progression");
                Check(rules.Schedule!.TypeOf(position) == round.GetProperty("type").GetString(), id + " round type");
                position = rules.Schedule.Next(position);
            }
            void State(JsonElement expected)
            {
                Check(player.Gold == expected.GetProperty("gold").GetInt32(), id + " gold");
                Check(player.Level == expected.GetProperty("level").GetInt32(), id + " level");
                Check(player.Xp == expected.GetProperty("xp").GetInt32(), id + " XP");
                Check(player.Streak == expected.GetProperty("streak").GetInt32(), id + " streak");
            }
            State(trace.GetProperty("initial"));
            foreach (var step in trace.GetProperty("steps").EnumerateArray())
            {
                var income = player.BeginRound(step.GetProperty("stage").GetInt32(), step.GetProperty("round").GetInt32());
                var expected = step.GetProperty("income");
                Check(income.Base == expected.GetProperty("base").GetInt32(), id + " base income");
                Check(income.Interest == expected.GetProperty("interest").GetInt32(), id + " interest");
                Check(income.Streak == expected.GetProperty("streak").GetInt32(), id + " streak income");
                Check(income.Total == expected.GetProperty("total").GetInt32(), id + " total income");
                if (step.GetProperty("bought").ValueKind != JsonValueKind.Null)
                    Check(player.BuyXp() == step.GetProperty("bought").GetBoolean(), id + " XP purchase");
                player.RecordOutcome(step.GetProperty("won").GetBoolean(), step.GetProperty("pve").GetBoolean());
                player.Credit(-step.GetProperty("debit").GetInt32());
                State(step.GetProperty("expected"));
            }
            int before = player.Gold;
            Check(!player.Spend(Math.Max(0, before) + 1) && player.Gold == before, id + " rejected spend mutated balance");
            var baseline = player.Income(1000, 1);
            var hooked = player.BeginRound(1000, 1, income => income with { Augment = 3, Bonus = 7 });
            Check(hooked.Total == baseline.Total + 10 && player.Gold == before + hooked.Total, id + " income hook");
        }
        Console.WriteLine($"Economy parity: {checks:N0} checks across all 22 mode/pace variants.");
        return checks;
    }
}
