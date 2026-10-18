namespace DigiTactics.Presentation;

/// <summary>The original evolution stage announcement's CSS ease, slide and lifetime.</summary>
public sealed class EvolutionAnnouncement
{
    public const double TransitionSeconds = .3;
    public const double CompletionSeconds = 3.2;
    public const double ActionSeconds = 1.8;
    public const double SlidePixels = 6;
    public bool MotionEnabled { get; set; } = true;
    private double changedAt, expiresAt, from, target, duration = TransitionSeconds;
    public string? Text { get; private set; }

    public void Show(string text, double now, double seconds = CompletionSeconds)
    {
        Advance(now);
        if (target != 1) Transition(1, now);
        Text = text;
        expiresAt = now + seconds;
    }

    public void Hide(double now)
    {
        Advance(now);
        if (target != 0) Transition(0, now);
    }

    public void Clear()
    {
        Text = null; changedAt = expiresAt = from = target = 0; duration = TransitionSeconds;
    }

    public EvolutionAnnouncementFrame Sample(double now)
    {
        Advance(now);
        double opacity = Opacity(now);
        return new(Text, opacity, SlidePixels * (1 - opacity), Text is not null && (target != 0 || now < changedAt + duration));
    }

    private void Advance(double now)
    {
        if (Text is not null && target == 1 && now >= expiresAt) Transition(0, expiresAt);
    }

    private void Transition(double next, double now)
    {
        double current = Opacity(now);
        // CSS reversals shorten the transition by the distance already travelled.
        duration = (MotionEnabled ? TransitionSeconds : .001) * Math.Abs(next - current);
        from = current; target = next; changedAt = now;
    }

    private double Opacity(double now)
    {
        double t = duration <= 0 ? 1 : Math.Clamp((now - changedAt) / duration, 0, 1);
        return from + (target - from) * Ease(t);
    }

    private static double Ease(double x)
    {
        // CSS default `ease`: cubic-bezier(.25,.1,.25,1).
        if (x <= 0) return 0;
        if (x >= 1) return 1;
        double low = 0, high = 1;
        for (int i = 0; i < 32; i++)
        {
            double t = (low + high) / 2, inverse = 1 - t;
            double curve = 3 * inverse * inverse * t * .25 + 3 * inverse * t * t * .25 + t * t * t;
            if (curve < x) low = t; else high = t;
        }
        double u = (low + high) / 2;
        return 3 * (1 - u) * (1 - u) * u * .1 + 3 * (1 - u) * u * u + u * u * u;
    }
}

public readonly record struct EvolutionAnnouncementFrame(string? Text, double Opacity, double Slide, bool Active);
