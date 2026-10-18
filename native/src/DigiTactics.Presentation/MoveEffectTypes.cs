using System.Text.Json;

namespace DigiTactics.Presentation;

public sealed class MoveVector(double x = 0, double y = 0, double z = 0)
{
    public double X = x, Y = y, Z = z;
    public void Set(double x, double y, double z) { X=x; Y=y; Z=z; }
    public void SetScalar(double value) => Set(value,value,value);
    public MoveVector Copy() => new(X,Y,Z);
}

/// <summary>Linear RGB, matching Three.Color's conversion of source sRGB hex colors.</summary>
public readonly record struct MoveColor(double R, double G, double B)
{
    public static MoveColor Hex(string hex)
    {
        int value=Convert.ToInt32(hex.TrimStart('#'),16);
        static double Linear(int n) { double s=n/255.0; return s<.04045?s/12.92:Math.Pow((s+.055)/1.055,2.4); }
        return new(Linear((value>>16)&255),Linear((value>>8)&255),Linear(value&255));
    }
    public static readonly MoveColor White=new(1,1,1);
}

public sealed class MoveOptions
{
    private readonly Dictionary<string,object?> values;
    public MoveOptions(params (string Key,object? Value)[] pairs) => values=pairs.ToDictionary(p=>p.Key,p=>p.Value,StringComparer.Ordinal);
    public MoveOptions(JsonElement json)
    {
        values=new(StringComparer.Ordinal);
        if(json.ValueKind==JsonValueKind.Object) foreach(var property in json.EnumerateObject()) values.Add(property.Name,property.Value.Clone());
    }
    public object? Raw(string name) => values.GetValueOrDefault(name);
    public bool Has(string name) => Raw(name) is { } value && value is not JsonElement { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined };
    public string? String(string name,string? fallback=null) => Raw(name) switch { string s when s.Length>0=>s, JsonElement { ValueKind:JsonValueKind.String } j=>j.GetString() is {Length:>0} s?s:fallback,_=>fallback };
    public double Value(string name,double fallback=0) => Raw(name) switch { JsonElement {ValueKind:JsonValueKind.Number} j=>j.GetDouble(),JsonElement {ValueKind:JsonValueKind.True}=>1,JsonElement {ValueKind:JsonValueKind.False}=>0,bool b=>b?1:0,double n=>n,float n=>n,int n=>n,long n=>n,_=>fallback };
    public double Number(string name,double fallback=0) { double n=Value(name,fallback); return n==0?fallback:n; }
    public bool IsFalse(string name) => Raw(name) is false or JsonElement {ValueKind:JsonValueKind.False};
    public bool Flag(string name) => Raw(name) switch { null=>false,bool b=>b,JsonElement j=>j.ValueKind switch {JsonValueKind.False or JsonValueKind.Null or JsonValueKind.Undefined=>false,JsonValueKind.Number=>j.GetDouble()!=0,JsonValueKind.String=>j.GetString()?.Length>0,_=>true},double n=>n!=0,int n=>n!=0,string s=>s.Length>0,_=>true };
}

public sealed record MoveSpec(string Element,string Charge,double Release,string Impact,double Shake,double[] Scale,string[] Colors,MoveStep[] Steps);
public sealed record MoveStep(string Primitive,MoveOptions Options);
public sealed class MoveContext
{
    public required MoveVector A,P,C;
    public required MoveVector[] T;
    public MoveVector? From;
    public required MoveSpec Spec;
    public double H,R,S,Yaw,StunTime=1;
    public int Stage;
    public MoveColor Ca,Cb;
    public string? Type;
    public bool Shook;
}

public sealed record MovePreset(double Gravity,double Drag,double Grow,double Spin)
{
    public static readonly MovePreset None=new(0,0,1,0),Drift=new(0,1.5,1.3,0),Flame=new(-2.2,1.8,.35,.5),FireTrail=new(-1.2,2.5,.3,1),
        Smoke=new(-.6,1.5,2.4,.8),Spark=new(6,1.2,.6,0),Debris=new(12,.4,1,4),Float=new(-1.4,1,1,1.5),Flutter=new(1.2,2.5,1,3),
        Pop=new(0,4,2.2,0),Shrink=new(0,0,.2,0),Swirl=new(-1,.5,.8,6),Breath=new(-.3,.4,2.3,1);
}

public sealed class MoveMesh
{
    public required string Kind;
    public readonly MoveVector Position=new(),Rotation=new(),Scale=new(1,1,1);
    public string RotationOrder="XYZ";
    public MoveColor Color=MoveColor.White,Emissive;
    public double Opacity=1;
    public bool Billboard,Visible;
    public MoveVector? LookTarget;
    public void LookAt(double x,double y,double z) { (LookTarget??=new()).Set(x,y,z); }
    public void RotateZ(double angle) => Rotation.Z+=angle;
    internal void Reset() { Position.Set(0,0,0);Rotation.Set(0,0,0);Scale.SetScalar(1);RotationOrder="XYZ";Color=MoveColor.White;Emissive=default;Opacity=1;Billboard=false;LookTarget=null;Visible=true; }
}

public sealed class MoveRibbon
{
    public readonly MoveVector[] Points=Enumerable.Range(0,24).Select(_=>new MoveVector()).ToArray();
    public int Count;
    public double Width=.1,Taper=1,Brightness=1;
    public MoveColor Color=MoveColor.White;
    public bool Visible;
    public void Build(double brightness=1) => Brightness=brightness;
}

public readonly record struct MovePiece(string Kind,double X,double Y,double Z,double Sx,double Sy,double Sz,MoveColor Color,double Alpha,double Rotation,double? Face,double Rx=0,double Ry=0,double Rz=0);
