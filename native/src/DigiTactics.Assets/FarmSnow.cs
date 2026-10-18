namespace DigiTactics.Assets;

/// <summary>Original float32 snowfall integration. Time is shared with the room animation clock.</summary>
public sealed class FarmSnow(float[] initial)
{
    public float[] Positions { get; } = (float[])initial.Clone();
    public void Update(double delta, double time)
    {
        for (int i = 0; i < Positions.Length / 3; i++)
        {
            Positions[i * 3 + 1] = (float)(Positions[i * 3 + 1] - delta * (.5 + i % 5 * .12));
            Positions[i * 3] = (float)(Positions[i * 3] + Math.Sin(time + i) * delta * .1);
            if (Positions[i * 3 + 1] < -.4) Positions[i * 3 + 1] = 8;
        }
    }
}
