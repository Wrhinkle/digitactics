namespace DigiTactics.Presentation;

/// <summary>Little Legend pedestal drag and click behavior from the client Stage.</summary>
public sealed class PedestalInteraction(Func<double>? random = null)
{
    private static readonly string[] roles = ["happy", "taunt", "dance", "wave"];
    private readonly Func<double> nextRandom = random ?? Random.Shared.NextDouble;
    private double downX, startingYaw;
    public double YawOffset { get; private set; }
    public bool Dragging { get; private set; }
    public bool HasMoved { get; private set; }

    public void Down(double x)
    {
        downX = x;
        startingYaw = YawOffset;
        Dragging = true;
        HasMoved = false;
    }

    public void Move(double x)
    {
        if (!Dragging) return;
        double delta = x - downX;
        if (Math.Abs(delta) > 3) HasMoved = true;
        YawOffset = startingYaw + delta * 0.012;
    }

    /// <summary>Returns the click reaction to play, or null after a drag or absent view.</summary>
    public string? Up(bool hasView)
    {
        string? role = Dragging && !HasMoved && hasView ? roles[(int)(nextRandom() * roles.Length)] : null;
        Cancel();
        return role;
    }

    /// <summary>Releases native pointer capture without triggering a click reaction.</summary>
    public void Cancel() { Dragging = false; HasMoved = false; }

    public double Yaw(double time, bool motion) => ClientMotion.PedestalYaw(time, motion) + YawOffset;
}
