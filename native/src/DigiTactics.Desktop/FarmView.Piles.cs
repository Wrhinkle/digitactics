using System.Numerics;
using System.Text.Json.Nodes;

namespace DigiTactics.Desktop;

internal sealed partial class FarmView
{
    private sealed class Pile(string id, int key)
    {
        public string Id { get; } = id;
        public int Key { get; } = key;
        public double X, Y, Z, Grow, Death, Yaw;
        public double Scale = .01;
        public bool Neglected;
    }
    private readonly Dictionary<(string,int),Pile> piles = [];
    private readonly List<Pile> dyingPiles = [];
    public int PileCount => piles.Count;
    public int RemovingPileCount => dyingPiles.Count;
    public IEnumerable<(string Id, int Key, float X, float Y)> PileAnchors(int width, int height)
    {
        var camera = Camera(width,height).Camera;
        foreach(var pile in piles.Values)
        {
            var p = Vector4.Transform(new Vector4((float)pile.X,.25f,(float)pile.Z,1),camera);
            yield return (pile.Id,pile.Key,(p.X/p.W*.5f+.5f)*width,(1-(p.Y/p.W*.5f+.5f))*height);
        }
    }
    private void SyncPiles(JsonNode farm)
    {
        var keep = new HashSet<(string,int)>();
        foreach(var resident in farm["residents"]!.AsArray())
        {
            string id = resident!.GetValue<string>();
            foreach(var value in farm["pets"]![id]!["poop"]!.AsArray())
            {
                int key = value!["k"]!.GetValue<int>(); keep.Add((id,key));
                if(!piles.TryGetValue((id,key),out var pile)) { LoadProp("pile"); piles[(id,key)] = pile = new(id,key); }
                pile.X = value["x"]!.GetValue<double>(); pile.Z = value["z"]!.GetValue<double>(); pile.Y = 0;
                pile.Neglected = value["n"]?.GetValue<bool>() == true;
            }
        }
        foreach(var key in piles.Keys.Where(key => !keep.Contains(key)).ToArray())
        {
            dyingPiles.Add(piles[key]); piles.Remove(key);
        }
    }
    private void UpdatePiles(double delta)
    {
        foreach(var pile in piles.Values)
        {
            if(pile.Grow < 1)
            {
                pile.Grow = Math.Min(1,pile.Grow+delta*3);
                pile.Scale = .01+pile.Grow*(1+Math.Sin(pile.Grow*Math.PI)*.3);
            }
            pile.Yaw = pile.Neglected ? Math.Sin(choreography.Time*3)*.05 : 0;
        }
        foreach(var pile in dyingPiles)
        {
            pile.Death += delta*4; pile.Scale = Math.Max(.01,1-pile.Death); pile.Y += delta;
        }
        dyingPiles.RemoveAll(pile => pile.Death >= 1);
    }
    private void DrawPiles(Matrix4x4 camera, bool shadow)
    {
        if(!propModels.TryGetValue("pile",out var model)) return;
        foreach(var pile in piles.Values.Concat(dyingPiles))
            DrawProp(model,GpuModel.Transform(pile.X,pile.Y,pile.Z,pile.Yaw,pile.Scale),camera,shadow);
    }
}
