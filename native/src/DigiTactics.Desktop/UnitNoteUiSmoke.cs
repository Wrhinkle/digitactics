using System.Reflection;
using System.Text.Json;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Hover real shop, bench and combat targets with original-source modifier fixtures.</summary>
internal sealed class UnitNoteUiSmoke
{
    private sealed record Sample(string Variant,string Line,int Stage,JsonElement? Mods,string[] Notes,string[] CombatNotes);
    private readonly ContentCatalog catalog;
    private readonly Sample[] samples;
    private int scenario,step,unitId,settle,traitScrollAttempts;
    private long beforeFrameSerial;
    private static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    public string Pending=>$"unit notes scenario {scenario}, step {step}";
    public UnitNoteUiSmoke(ContentCatalog catalog)
    {
        this.catalog=catalog;
        samples=JsonSerializer.Deserialize<Sample[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"content/unit-note-ui-cases.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
    }
    private static void Invoke(MatchScreen screen,string method)=>typeof(MatchScreen).GetMethod(method,Private)!.Invoke(screen,null);
    public void Input(MatchScreen screen)
    {
        beforeFrameSerial=screen.UnitNoteRenderSerial;
        var sample=samples[scenario];
        if(step==0)
        {
            traitScrollAttempts=0;
            Invoke(screen,"LeaveMatch");
            var rules=catalog.Variants.Single(r=>r.Id==sample.Variant);
            var session=new MatchPlanningSession(rules,461u,Enumerable.Range(0,8).Select(i=>new MatchSeat("Note Tamer "+i,i==0)).ToArray());
            if(session.Rounds.RoundType=="carousel"){session.StartCombat();session.FinishCombat();session.Advance();}
            var player=session.State.Players[0];
            foreach(var old in player.Roster.Entries.ToArray())player.Roster.Remove(old.Unit.Id,false);
            var trait=rules.Lines.Single(l=>l.Id==sample.Line).Traits[0];
            var unit=session.State.CreateUnit(sample.Line,sample.Stage) with{Mods=sample.Mods,BonusTraits=new(){{trait,rules.Traits.Single(t=>t.Id==trait).Breakpoints[0]}}};unitId=unit.Id;player.Roster.AddToBench(unit);
            if(player.Offer is not null)session.Offers.Choose(player,0,false);
            player.Shop.ReplaceOffer(0,sample.Line);player.Shop.SetMetadata(0,new(Unit:new(sample.Stage,Mods:sample.Mods)));
            typeof(MatchScreen).GetProperty(nameof(MatchScreen.Session))!.SetValue(screen,session);
            typeof(MatchScreen).GetField("localPaused",Private)!.SetValue(screen,true);
            ImGui.GetIO().AddMousePosEvent(0,0);
        }
        if(step is 2 or 3)Hover(screen,"buy/0");
        if(step is 5 or 6)Hover(screen,"arenaBench/0");
        if(step==8)
        {
            var session=screen.Session!;var player=session.State.Players[0];
            player.Roster.Move(UnitSpot.Bench(0),UnitSpot.Board(3,4),false,()=>9);
        }
        if(step is 9 or 10)
        {
            var target=screen.Buttons.Where(pair=>pair.Key.StartsWith("traitUnit/",StringComparison.Ordinal)&&pair.Key.EndsWith("/"+unitId,StringComparison.Ordinal)).OrderBy(pair=>pair.Value.Y).First();
            var bounds=screen.TraitCardsBounds;
            if(target.Value.Y<bounds.Min.Y+4||target.Value.Y>bounds.Max.Y-4)
            {
                if(++traitScrollAttempts>24)throw new InvalidOperationException($"Could not scroll to unit-note contributor {target.Key} at {target.Value} within {bounds}.");
                var at=(bounds.Min+bounds.Max)*.5f;
                ImGui.GetIO().AddMousePosEvent(at.X,at.Y);
                ImGui.GetIO().AddMouseWheelEvent(0,target.Value.Y<bounds.Min.Y+4?1:-1);
                settle=0;return;
            }
            Hover(screen,target.Key);
        }
        if(step==12){screen.Session!.StartCombat();Invoke(screen,"CombatStarted");}
        if(step is 14 or 15)
        {
            var target=screen.Session!.Battles.First(b=>b.A==0).Combat.Units.First(u=>u.Entry.Uid==unitId&&u.Team==0);
            Hover(screen,"combatUnit/"+target.Id);
        }
        // Tooltip windows need their real auto-size/fade frames before capture.
        if(step is 3 or 6 or 10 or 15&&settle++<3)return;
        settle=0;step++;
    }
    private static void Hover(MatchScreen screen,string key)
    {
        if(!screen.Buttons.TryGetValue(key,out var at))throw new InvalidOperationException("Missing unit note hover target: "+key);
        ImGui.GetIO().AddMousePosEvent(at.X,at.Y);
    }
    public bool Verify(MatchScreen screen,int2 size,string? capture)
    {
        string? route=step switch{4=>"shop",7=>"bench",11=>"trait",16=>"combat",_=>null};
        if(route is null)return false;
        var expected=route=="combat"?samples[scenario].CombatNotes:samples[scenario].Notes;
        if(screen.UnitNoteRenderSerial<=beforeFrameSerial||!screen.DetailedUnitNotes.SequenceEqual(expected)||route=="bench"&&screen.DetailedUnit!=unitId||route=="combat"&&screen.DetailedCombatUnit?.Entry.Uid!=unitId)
        {
            if(capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,$"{scenario}.{route}.failed.bmp"),size.X,size.Y);
            throw new InvalidOperationException($"Unit note tooltip differs from original on {route}: {Pending}; expected [{string.Join(" | ",expected)}], actual [{string.Join(" | ",screen.DetailedUnitNotes)}].");
        }
        if(capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,$"{scenario}.{route}.bmp"),size.X,size.Y);
        if(route!="combat")return false;
        scenario++;step=0;
        if(scenario<samples.Length)return false;
        Console.WriteLine("Unit note UI passed: actual shop/bench/trait-contributor/combat hover routes preserve Partner, Anomaly, ordered Digi-Core/form notes and suppress unknown modifiers.");return true;
    }
}
