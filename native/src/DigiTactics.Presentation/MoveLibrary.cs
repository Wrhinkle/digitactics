using System.Text.Json;
using DigiTactics.Rules;

namespace DigiTactics.Presentation;

public sealed record MoveLine(string Type,double Radius,MoveSpec[] Moves,MoveOptions[] Basic);
/// <summary>Exact declarative move resolver output for every playable rules variant.</summary>
public sealed class MoveLibrary
{
    private readonly Dictionary<string,Dictionary<string,MoveLine>> variants=new(StringComparer.Ordinal);
    private static readonly Dictionary<string,string[]> palette=new(StringComparer.Ordinal)
    {
        ["fire"]=["#ff5a1a","#ffd36a"],["ice"]=["#7fdcff","#ffffff"],["thunder"]=["#ffe94a","#a8dcff"],
        ["water"]=["#2f9bff","#bff0ff"],["plant"]=["#5cff6a","#ffb3e6"],["wind"]=["#b8fff0","#ffffff"],
        ["holy"]=["#fff0a0","#ffffff"],["dark"]=["#a04aff","#ff3a7a"],["metal"]=["#ffb04a","#d8e4f4"],["beast"]=["#ffffff","#ff9a5a"]
    };
    public MoveLibrary(string path)
    {
        using var doc=JsonDocument.Parse(File.ReadAllBytes(path));var root=doc.RootElement;
        if(root.GetProperty("schema").GetInt32()!=1)throw new InvalidDataException("Unsupported native move library.");
        foreach(var variant in root.GetProperty("variants").EnumerateObject())
        {
            var lines=new Dictionary<string,MoveLine>(StringComparer.Ordinal);variants.Add(variant.Name,lines);
            foreach(var line in variant.Value.EnumerateObject())
            {var data=line.Value;lines.Add(line.Name,new(data.GetProperty("type").GetString()!,data.GetProperty("radius").GetDouble(),data.GetProperty("moves").EnumerateArray().Select(ReadSpec).ToArray(),data.GetProperty("basic").EnumerateArray().Select(j=>new MoveOptions(j)).ToArray()));}
        }
    }
    public MoveLine Line(string variant,string line)=>variants.TryGetValue(variant,out var lines)&&lines.TryGetValue(line,out var result)?result:throw new InvalidDataException($"Missing native moves for {variant}/{line}.");
    public static string[] Palette(string? element)=>palette.GetValueOrDefault(element??"beast")??palette["beast"];
    private static MoveSpec ReadSpec(JsonElement data)
    {
        var o=new MoveOptions(data);string element=o.String("elem","beast")!;
        string[] colors=data.TryGetProperty("colors",out var c)?c.EnumerateArray().Select(x=>x.GetString()!).ToArray():Palette(element);
        double[] scale=data.TryGetProperty("scale",out var s)?s.EnumerateArray().Select(x=>x.GetDouble()).ToArray():[1,1.35,1.75];
        var steps=data.GetProperty("fx").EnumerateArray().Select(step=>{var parts=step.EnumerateArray().ToArray();string name=parts[0].GetString()!;if(!MovePrimitives.Names.Contains(name))throw new InvalidDataException("Unsupported move: "+name);return new MoveStep(name,parts.Length>1?new(parts[1]):new());}).ToArray();
        return new(element,o.String("charge","aura")!,o.Value("release",.3),o.String("impact","hit")!,o.Value("shake"),scale,colors,steps);
    }
}

/// <summary>Collects a cast and its ordered target cues, then schedules one source move composition.</summary>
public sealed class MovePlayback(MoveLibrary library,string variant,uint seed=84219)
{
    private sealed record Pending(MoveLine Line,MoveSpec Spec,int Stage,MoveVector Caster,double Height,double Release,MoveVector? CurrentTarget)
    { public List<MoveVector> Targets=[];public (MoveVector Center,double Radius)? Area;public MoveVector? From; }
    private Pending? pending;
    public MoveEffects Effects {get;}=new(seed);
    public void Begin(UnitLine line,int stage,WorldPosition caster,WorldPosition? currentTarget=null,double? height=null,double? specialDuration=null)
    {
        Flush();var info=library.Line(variant,line.Id);var spec=info.Moves[stage];double release=spec.Release;
        release=specialDuration is {} duration?Math.Min(release,Math.Min(duration,1.1)*.45):release*.7;
        pending=new(info,spec,stage,Vector(caster),height is {} h&&h!=0?h:new[]{1.3,1.65,2.0}[stage],release,currentTarget is {} target?Vector(target):null);
    }
    public void Target(WorldPosition target,WorldPosition? from=null)
    {if(pending is not {} p)return;p.Targets.Add(new(target.X,.8,target.Z));if(from is {} point)p.From=new(point.X,0,point.Z);}
    public void Area(WorldPosition center,double radius){if(pending is {} p)p.Area=(new(center.X,0,center.Z),radius);}
    public void Flush()
    {
        if(pending is not {} p)return;pending=null;var spec=p.Spec;double scale=spec.Scale[Math.Min(p.Stage,spec.Scale.Length-1)];if(scale==0)scale=1;
        MoveVector[] targets=p.Targets.ToArray();if(targets.Length==0&&p.CurrentTarget is {} current&&p.Line.Type!="heal")targets=[new(current.X,.8,current.Z)];
        var center=p.Area?.Center??targets.FirstOrDefault()??p.Caster;var aim=targets.FirstOrDefault()??center;double yaw=Math.Atan2(aim.X-p.Caster.X,aim.Z-p.Caster.Z),forward=.35*scale;
        var context=new MoveContext{A=new(p.Caster.X+Math.Sin(yaw)*forward,p.Height*.62+p.Caster.Y,p.Caster.Z+Math.Cos(yaw)*forward),P=new(p.Caster.X,0,p.Caster.Z),T=targets,C=center,R=p.Area is {} area?(area.Radius+.5)*1.7:1.2,S=scale,Stage=p.Stage,H=p.Height,Ca=MoveColor.Hex(spec.Colors[0]),Cb=MoveColor.Hex(spec.Colors[1]),Yaw=yaw,From=p.From,Spec=spec,Type=p.Line.Type,StunTime=new[]{1.5,1.75,2}[p.Stage]};
        if(p.Area is not null&&targets.Length==0&&context.Type!="heal")context.T=[new(center.X,.8,center.Z)];
        MovePrimitives.Charge(Effects,context,p.Release);foreach(var step in spec.Steps)MovePrimitives.Play(step.Primitive,Effects,context,step.Options,p.Release+step.Options.Value("at"));
    }
    public void Demo(UnitLine line,int stage,WorldPosition from,IReadOnlyList<WorldPosition> targets,double? height=null,double? specialDuration=null)
    {
        Begin(line,stage,from,targets.Count>0?targets[0]:null,height,specialDuration);foreach(var target in targets)Target(target);
        var info=library.Line(variant,line.Id);if(info.Type is "aoe" or "slam"||info.Type=="drain"&&info.Radius!=0)
        {if(info.Type=="slam")Area(from,info.Radius==0?1:info.Radius);else if(targets.Count>0)Area(targets[0],info.Radius==0?1:info.Radius);}
        Flush();
    }
    public void Basic(UnitLine line,int stage,WorldPosition from,WorldPosition to,double height,bool big,bool ranged)
    {
        var b=library.Line(variant,line.Id).Basic[ranged?1:0];var palette=MoveLibrary.Palette(b.String("elem"));
        if(b.Raw("colors") is JsonElement colors){palette=colors.EnumerateArray().Select(j=>j.GetString()!).ToArray();}
        MovePrimitives.Basic(Effects,b,Vector(from),Vector(to),height==0?1.4:height,stage,big,MoveColor.Hex(palette[0]),MoveColor.Hex(palette[1]));
    }
    public void Update(double delta)=>Effects.Update(delta);
    public void Clear(){pending=null;Effects.Clear();}
    private static MoveVector Vector(WorldPosition p)=>new(p.X,p.Y,p.Z);
}
