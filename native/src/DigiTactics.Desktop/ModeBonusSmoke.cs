using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Render real mode data on two distinct players and scout through the actual UI.</summary>
internal sealed class ModeBonusSmoke(ContentCatalog catalog)
{
    private readonly string[] variants = ["underlords/normal", "autochess/normal"];
    private int scenario, step;
    private readonly WispUiSmoke wisps=new(catalog);
    public void Input(MatchScreen screen)
    {
        if(scenario>=variants.Length){wisps.Input(screen);return;}
        if (step == 0)
        {
            var rules = catalog.Variants.Single(rules => rules.Id == variants[scenario]);
            var session = new MatchPlanningSession(rules, 143, Enumerable.Range(0,8).Select(i => new MatchSeat("Tamer " + i, i == 0)).ToArray(), false);
            var globals = rules.Items.Where(item => item.Category == "global" || item.Relic).Take(2).ToArray();
            if (globals.Length != 2) throw new InvalidOperationException("Mode bonus fixture requires distinct global bonuses.");
            for (int i = 0; i < 2; i++)
            {
                var player = session.State.Players[i];
                player.Globals.Clear(); player.Globals.Add(globals[i].Id);
                player.Power = rules.Offers!.Powers.Length > 0 ? new(rules.Offers.Powers[i], i + 1) : null;
                typeof(MatchPlayer).GetProperty(nameof(MatchPlayer.Offer))!.SetValue(player, null);
            }
            typeof(MatchScreen).GetProperty(nameof(MatchScreen.Session))!.SetValue(screen, session);
            typeof(MatchScreen).GetField("viewed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(screen, 0);
        }
        if (step == 4)
        {
            var point = screen.Buttons["view/1"];
            ImGui.GetIO().AddMousePosEvent(point.X,point.Y); ImGui.GetIO().AddMouseButtonEvent(0,true);
        }
        if (step == 5) ImGui.GetIO().AddMouseButtonEvent(0,false);
        if (step == 9) { ImGui.GetIO().AddMousePosEvent(100,580); ImGui.GetIO().AddMouseWheelEvent(0,-20); }
        step++;
    }
    public bool Verify(MatchScreen screen, int2 size, string? capture)
    {
        if(scenario>=variants.Length)return wisps.Verify(screen,size,capture);
        if (step is not (3 or 8 or 13)) return false;
        int seat = step == 3 ? 0 : 1;
        var player = screen.Session!.State.Players[seat];
        if (screen.ViewedPlayer != seat || screen.DisplayedPower != player.Power || !screen.DisplayedGlobals.SequenceEqual(player.Globals))
            throw new InvalidOperationException("Mode sidebar displayed another player's power or global bonuses.");
        if (capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, variants[scenario].Replace('/', '-') + "-" + seat + (step == 13 ? "-scrolled" : "") + ".bmp"),size.X,size.Y);
        if (step != 13) return false;
        step = 0; scenario++;
        if (scenario != variants.Length) return false;
        Console.WriteLine("Mode bonus UI passed: Underlords powers/ranks/globals and Auto Chess relics, local and scouted players.");
        return false;
    }
}
