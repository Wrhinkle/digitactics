namespace DigiTactics.Rules;

/// <summary>Bit-for-bit implementation of the shipping game's seeded Mulberry32 generator.</summary>
public sealed class DeterministicRandom(uint seed)
{
    public uint State { get; private set; } = seed;

    public double Next()
    {
        unchecked
        {
            State += 0x6d2b79f5;
            uint t = State;
            t = (t ^ (t >> 15)) * (t | 1);
            t ^= t + (t ^ (t >> 7)) * (t | 61);
            return (t ^ (t >> 14)) / 4294967296.0;
        }
    }
}
