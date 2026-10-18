using System.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal static class ArenaPicking
{
    public static WorldPosition? Ground(float x, float y, int width, int height, int columns, ArenaCamera? camera = null)
    {
        if (width <= 0 || height <= 0 || !Matrix4x4.Invert(camera?.Matrix(width, height, columns) ?? BattleView.Camera(width, height, columns), out var inverse)) return null;
        float nx = x / width * 2 - 1, ny = 1 - y / height * 2;
        var near = Vector4.Transform(new Vector4(nx, ny, 0, 1), inverse); near /= near.W;
        var far = Vector4.Transform(new Vector4(nx, ny, 1, 1), inverse); far /= far.W;
        var direction = far - near;
        if (Math.Abs(direction.Y) < 1e-6) return null;
        float t = -near.Y / direction.Y;
        if (t < 0) return null;
        var point = near + direction * t;
        return new(point.X, 0, point.Z);
    }

    public static UnitSpot? Spot(WorldPosition point, bool wide, bool board)
    {
        double bx = wide ? (point.X + 6.1) / .72 : point.X;
        int index = (int)Math.Floor(bx / 1.45 + 4.5);
        if (index is >= 0 and < 9 && Math.Abs(point.Z - 7.55) <= .8 && Math.Abs(CombatLayout.Bench(index, wide).X - point.X) < .72)
            return UnitSpot.Bench(index);
        if (!board) return null;
        UnitSpot? best = null; double distance = .95 * .95;
        for (int row = 0; row < 8; row++) for (int column = 0; column < 7; column++)
        {
            var cell = CombatLayout.Cell(new(column, row), 7, false);
            double d = Math.Pow(cell.X - point.X, 2) + Math.Pow(cell.Z - point.Z, 2);
            if (d < distance) { distance = d; best = row >= 4 ? UnitSpot.Board(column, row) : null; }
        }
        return best;
    }
}
