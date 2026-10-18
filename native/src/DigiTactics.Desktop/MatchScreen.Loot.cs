using System.Text.Json;
using System.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private readonly LootOrbView lootOrbs = new();
    private MatchPlanningSession? lootSession;
    private readonly HashSet<int> pendingOrbs = [];
    internal int LootBurstDrawCalls => lootOrbs.BurstDrawCalls;
    internal int LootOrbDrawCalls => lootOrbs.DrawCalls;
    internal int LootOrbShadowDrawCalls => lootOrbs.ShadowDrawCalls;
    internal Brutal.Numerics.float2 LootGroundScreen(LootOrb orb)
    {
        var p=LootOrbView.Position(orb,0);
        var clip=Vector4.Transform(new Vector4(p.X,0,p.Z,1),arenaEnvironment.Camera.Matrix(Width,Height,battle?.Combat.Columns??7));
        return new(Left+(clip.X/clip.W+1)*Width/2,Top+(1-clip.Y/clip.W)*Height/2);
    }
    private void UpdateLootCollection()
    {
        if(!ReferenceEquals(lootSession,Session)) { lootSession=Session; pendingOrbs.Clear(); lootOrbs.Reset(); }
        if(Session is null || IsSpectating || !Human.Alive || Session.State.Over || arenaCompanion is null) return;
        pendingOrbs.RemoveWhere(id=>!Human.Orbs.Any(o=>o.Id==id));
        foreach(var orb in Human.Orbs.ToArray())
        {
            if(pendingOrbs.Contains(orb.Id) || !LootOrbView.InRange(orb,arenaCompanion.Travel.X,arenaCompanion.Travel.Z)) continue;
            if(networkRoom is null) Act("orb",new {id=orb.Id});
            else if(SendNetworkAction("orb",JsonSerializer.SerializeToElement(new {id=orb.Id}))) pendingOrbs.Add(orb.Id);
        }
    }
}
