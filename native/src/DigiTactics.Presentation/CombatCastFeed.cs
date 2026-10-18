using DigiTactics.Rules;

namespace DigiTactics.Presentation;

public sealed record CastFeedUnit(string Line,int Stage,int Team,string Name,string Ability,string Element);
public sealed record CastFeedRow(int Unit,string Line,int Stage,int Team,string Name,string Ability,string Color,double Started,double Life);

/// <summary>The source cast feed uses wall-clock expiry, captures speed when a cast arrives,
/// and keeps the last five casts in event order. A new watched battle skips old events.</summary>
public sealed class CombatCastFeed
{
    private readonly IReadOnlyList<CombatEvent> events;
    private readonly Func<int,CastFeedUnit?> unit;
    private readonly bool mirrored;
    private readonly List<CastFeedRow> rows=[];
    private int cursor;
    public double Time { get; private set; }
    public IReadOnlyList<CastFeedRow> Rows=>rows;
    public CombatCastFeed(IReadOnlyList<CombatEvent> events,Func<int,CastFeedUnit?> unit,bool mirrored=false,bool skipHistory=true)
    {this.events=events;this.unit=unit;this.mirrored=mirrored;cursor=skipHistory?events.Count:0;}
    public CombatCastFeed(CombatSimulation combat,string variant,AbilityElements elements,bool mirrored=false)
        :this(combat.Events,id=>id<0||id>=combat.Units.Count?null:Describe(combat.Units[id],variant,elements),mirrored){}
    private static CastFeedUnit Describe(CombatUnit unit,string variant,AbilityElements elements)=>
        new(unit.Line.Id,unit.Stage,unit.Team,unit.Line.Forms[unit.Stage],unit.Line.Ability.Names[unit.Stage],elements.Get(variant,unit.Line.Id,unit.Stage));
    public void Advance(double wallDelta)
    {
        if(!double.IsFinite(wallDelta)||wallDelta<0)throw new ArgumentOutOfRangeException(nameof(wallDelta));
        Time+=wallDelta;rows.RemoveAll(row=>Time-row.Started>=row.Life+.4);
    }
    public void Observe(int speed)
    {
        if(speed<=0)throw new ArgumentOutOfRangeException(nameof(speed));
        while(cursor<events.Count)
        {
            var cue=events[cursor++];if(cue.Type!="cast"||unit(cue.Unit) is not {} caster)continue;
            int team=mirrored?1-caster.Team:caster.Team;
            rows.Add(new(cue.Unit,caster.Line,caster.Stage,team,(team==0?"":"Enemy ")+caster.Name,caster.Ability,ElementColor(caster.Element),Time,Math.Max(.9,2.6/speed)));
            if(rows.Count>5)rows.RemoveAt(0);
        }
    }
    public void Clear(){rows.Clear();cursor=events.Count;}
    public static string ElementColor(string element)=>element switch
    {
        "fire"=>"#ff7a3a","ice"=>"#8fe3ff","thunder"=>"#ffe94a","water"=>"#4fb0ff","plant"=>"#6cff7a",
        "wind"=>"#b8fff0","holy"=>"#fff0a0","dark"=>"#b46bff","metal"=>"#ffb04a",_=>"#ffd6a8"
    };
}
