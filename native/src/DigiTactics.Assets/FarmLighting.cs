namespace DigiTactics.Assets;

public sealed class FarmLighting
{
    public double Hemisphere { get; private set; } = 1.7;
    public double Sun { get; private set; } = 2.3;
    public double Lamp { get; private set; }
    public bool Night { get; set; }
    public double BulbEmission => Night ? 1.6 : .2;
    public void Update(double delta, string theme)
    {
        double k = Math.Min(1, delta * 3);
        Hemisphere += ((Night ? .45 : theme is "grid" or "sewer" ? 1.3 : 1.7) - Hemisphere) * k;
        Sun += ((Night ? .12 : 2.3) - Sun) * k;
        Lamp += ((Night ? 7 : 0) - Lamp) * k;
    }
}
