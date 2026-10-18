using DigiTactics.Rules;

namespace DigiTactics.Presentation;

/// <summary>Source cast collection, isolated from simulation and model animation policy.</summary>
public sealed class CombatMovePlayback(CombatSimulation combat,MovePlayback playback,Func<int,double?> specialDuration)
{
    private int cursor;
    private (int Unit,double Time,string Type)? cast;
    private (int Unit,WorldPosition Point)? leap;
    private readonly LegacyCombatEffects legacy=new(playback.Effects);
    public int ConsumedEvents=>cursor;
    public int Casts {get;private set;}
    public int Basics {get;private set;}
    public int EquipmentCues {get;private set;}
    public void Consume(bool mirror)
    {
        while(cursor<combat.Events.Count)
        {
            var e=combat.Events[cursor++];CombatUnit? from=e.Unit>=0&&e.Unit<combat.Units.Count?combat.Units[e.Unit]:null;
            CombatUnit? to=e.Target is {} id&&id>=0&&id<combat.Units.Count?combat.Units[id]:null;
            bool Ultimate(int? owner=null)=>cast is {} current&&Math.Abs(current.Time-e.Time)<1e-6&&(owner is null||owner==current.Unit);
            WorldPosition Point(CombatUnit unit)=>CombatLayout.Unit(unit,combat.Columns,mirror);
            switch(e.Type)
            {
                case "cast" when from is not null:
                    cast=(from.Id,e.Time,from.Line.Ability.Kind);playback.Begin(from.Line,from.Stage,Point(from),from.Target is {Alive:true} target?Point(target):null,height:new[]{1.3,1.65,2.0}[from.Stage]+(from.Line.Tier==5?.15:0),specialDuration:specialDuration(from.Id));Casts++;break;
                case "heal" when from is not null&&Ultimate()&&cast?.Type=="heal":playback.Target(Point(from));break;
                case "leap" when Ultimate()&&e.Cell is {} origin:leap=(e.Unit,CombatLayout.Cell(origin,combat.Columns,mirror));break;
                case "hit" or "shot" or "beam" when from is not null&&to is not null:
                    if(Ultimate(from.Id)){playback.Target(Point(to),e.Type=="hit"?(leap is {} previous&&previous.Unit==from.Id?previous.Point:Point(from)):null);if(e.Type=="hit")leap=null;}
                    else if(e.Type!="beam"){playback.Basic(from.Line,from.Stage,Point(from),Point(to),new[]{1.3,1.65,2.0}[from.Stage]+(from.Line.Tier==5?.15:0),e.Big,e.Type=="shot");Basics++;}
                    else {legacy.Beam(Point(from),Point(to),LegacyCombatEffects.AttributeColor(from.Line.Attribute),e.Time);EquipmentCues++;}
                    break;
                case "burst" when e.Cell is {} center:
                    if(Ultimate())playback.Area(CombatLayout.Cell(center,combat.Columns,mirror),e.Radius);
                    else {legacy.Burst(CombatLayout.Cell(center,combat.Columns,mirror),e.Radius,mirror?1-(e.Team??0):e.Team??0,e.Time);EquipmentCues++;}
                    break;
                case "shield" when from is not null&&!Ultimate(from.Id):legacy.Shield(Point(from),e.Time);EquipmentCues++;break;
            }
        }
        playback.Flush();
        legacy.Update(combat.Time);
    }
    public void Clear(){legacy.Clear();playback.Clear();cast=null;leap=null;cursor=combat.Events.Count;}
}
