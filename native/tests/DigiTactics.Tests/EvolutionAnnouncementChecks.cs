using System.Text.Json;
using DigiTactics.Presentation;

internal static class EvolutionAnnouncementChecks
{
    public static int Run(string root)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "tests/fixtures/evolution-announcement.json")));
        var source = document.RootElement;
        if (EvolutionAnnouncement.CompletionSeconds != source.GetProperty("completionSeconds").GetDouble() || EvolutionAnnouncement.ActionSeconds != source.GetProperty("actionSeconds").GetDouble())
            throw new InvalidOperationException("Evolution announcement lifetime differs from source.");
        var style = source.GetProperty("style");
        foreach (var expected in new Dictionary<string, string> { ["fontSize"] = "15px", ["fontWeight"] = "800", ["color"] = "rgb(255, 255, 255)", ["bottom"] = "10px", ["padding"] = "0px 8px", ["textAlign"] = "center", ["lineHeight"] = "20.25px", ["transitionDuration"] = "0.3s, 0.3s", ["textShadow"] = "rgb(0, 0, 0) 0px 2px 6px, rgb(255, 211, 106) 0px 0px 16px" })
            if (style.GetProperty(expected.Key).GetString() != expected.Value) throw new InvalidOperationException("Evolution announcement style changed: " + expected.Key);
        int count = 2;
        foreach (var row in source.GetProperty("samples").EnumerateArray())
        {
            var announcement = new EvolutionAnnouncement(); announcement.Show("Digivolved!", 10);
            double now = 10 + row.GetProperty("time").GetDouble() + (row.GetProperty("showing").GetBoolean() ? 0 : EvolutionAnnouncement.CompletionSeconds);
            var actual = announcement.Sample(now);
            if (Math.Abs(actual.Opacity - row.GetProperty("opacity").GetDouble()) > .00002 || Math.Abs(actual.Slide - row.GetProperty("slide").GetDouble()) > .00002)
                throw new InvalidOperationException("Evolution announcement differs from the browser CSS transition.");
            count++;
        }
        foreach (var row in source.GetProperty("reversal").EnumerateArray())
        {
            var announcement = new EvolutionAnnouncement(); announcement.Show("Digivolved!", 0); announcement.Hide(.1);
            var actual = announcement.Sample(.1 + row.GetProperty("time").GetDouble());
            if (Math.Abs(actual.Opacity - row.GetProperty("opacity").GetDouble()) > .00002 || Math.Abs(actual.Slide - row.GetProperty("slide").GetDouble()) > .00002)
                throw new InvalidOperationException("Reversing an evolution announcement differs from the browser CSS transition.");
            count++;
        }
        var notice = new EvolutionAnnouncement();
        notice.Show("Completed", 0);
        if (notice.Sample(.3).Opacity != 1 || notice.Sample(3.199).Opacity != 1 || notice.Sample(3.35).Opacity is <= 0 or >= 1 || notice.Sample(3.5).Active || notice.Sample(3.5).Opacity != 0)
            throw new InvalidOperationException("Evolution announcement must hold, then fade out instead of disappearing.");
        notice.Show("Level 3!", 10, EvolutionAnnouncement.ActionSeconds);
        notice.Show("Back to Bukamon.", 11, EvolutionAnnouncement.ActionSeconds);
        if (notice.Sample(11).Opacity != 1 || notice.Sample(11).Text != "Back to Bukamon." || !notice.Sample(12.9).Active || notice.Sample(13.2).Active)
            throw new InvalidOperationException("Replacing an announcement must preserve opacity and restart its expiry.");
        notice.Show("New form", 20); notice.Hide(20.1);
        double fading = notice.Sample(20.1).Opacity;
        if (fading is <= 0 or >= 1 || notice.Sample(20.15).Opacity >= fading || notice.Sample(20.4).Active)
            throw new InvalidOperationException("Starting another digivolution must smoothly hide the old message.");
        notice.Clear();
        if (notice.Sample(21).Active || notice.Sample(21).Text is not null || notice.Sample(21).Opacity != 0)
            throw new InvalidOperationException("Closing care must clear its announcement.");
        var reduced=new EvolutionAnnouncement{MotionEnabled=false};reduced.Show("Reduced",0);
        if(reduced.Sample(.001).Opacity!=1 || reduced.Sample(.001).Slide!=0 || reduced.Sample(3.199).Opacity!=1 || reduced.Sample(3.202).Active)
            throw new InvalidOperationException("Reduced motion must shorten the CSS transition, preserving the announcement lifetime.");
        reduced.Show("Level",10,EvolutionAnnouncement.ActionSeconds);
        if(reduced.Sample(11.799).Opacity!=1 || reduced.Sample(11.802).Active)
            throw new InvalidOperationException("Reduced motion changed the action announcement lifetime.");
        if(source.GetProperty("reduced").GetProperty("duration").GetString()!="0.001s")throw new InvalidOperationException("Source reduced-motion duration changed.");
        foreach(var row in source.GetProperty("reduced").GetProperty("samples").EnumerateArray())
        {
            var announcement=new EvolutionAnnouncement{MotionEnabled=false};announcement.Show("Reduced",0);
            var actual=announcement.Sample(row.GetProperty("time").GetDouble()+(row.GetProperty("showing").GetBoolean()?0:EvolutionAnnouncement.CompletionSeconds));
            if(Math.Abs(actual.Opacity-row.GetProperty("opacity").GetDouble())>.00002 || Math.Abs(actual.Slide-row.GetProperty("slide").GetDouble())>.00002)
                throw new InvalidOperationException("Reduced-motion announcement differs from original browser CSS.");
            count++;
        }
        Console.WriteLine($"Evolution announcement: {count - 2} browser samples, source styling, completion/action expiry, replacement and cancellation passed.");
        return count + 6;
    }
}
