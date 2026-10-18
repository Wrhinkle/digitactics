using DigiTactics.Rules;

namespace DigiTactics.Presentation;

public sealed record HudRound(string Round, string Outcome, bool Pve, string Opponent, int Damage);
public sealed record HudTimelineRound(string Round, string Kind, string? Offer, string? Outcome, bool Current);

/// <summary>Read-only match HUD data; replay uses the same settlement inputs as live play.</summary>
public sealed class MatchHudProjection
{
    private MatchPlanningSession? session;
    private readonly Dictionary<int,List<HudRound>> rounds=[];
    public IReadOnlyList<HudRound> Recent(int seat) => rounds.TryGetValue(seat,out var entries)?entries:[];
    public void Reset(MatchPlanningSession match){session=match;rounds.Clear();}
    public void Capture(MatchPlanningSession match)
    {
        if(!ReferenceEquals(session,match))Reset(match);
        if(match.Rounds.Phase!=MatchPhase.Result)return;
        foreach(var result in match.Rounds.Results)
        {
            var fight=result.Match;
            void Record(bool sideA)
            {
                int? team=sideA?fight.TeamA:fight.TeamB;
                int seat=(sideA?fight.A:fight.B)!.Value;
                var players=team is {} id?match.State.Teams[id].Members:[match.State.Players[seat]];
                string opponent=fight.Pve?"Wild Digimon":match.State.Players[(sideA?fight.B:fight.A)!.Value].Name;
                string outcome=fight.Winner==(sideA?0:1)?"w":fight.Winner==-1?"d":"l";
                foreach(var player in players)
                {
                    if(!rounds.TryGetValue(player.Id,out var entries))rounds[player.Id]=entries=[];
                    string label=match.Rounds.Position.Label;
                    if(entries.Any(r=>r.Round==label))continue;
                    entries.Add(new(label,outcome,fight.Pve,opponent,sideA?result.DamageA:result.DamageB));
                }
            }
            Record(true);if(!fight.Pve&&!fight.Ghost&&fight.B is not null)Record(false);
        }
    }
    public HudTimelineRound[] Timeline(MatchPlanningSession match,int seat)
    {
        var current=match.Rounds.Position;var schedule=match.State.Rules.Schedule!;
        return Enumerable.Range(1,schedule.RoundsInStage(current.Stage)).Select(round=>
        {
            var position=new RoundPosition(current.Stage,round);
            return new HudTimelineRound(position.Label,schedule.TypeOf(position),match.Offers.At(position)?.Kind,
                ReferenceEquals(session,match)?Recent(seat).LastOrDefault(r=>r.Round==position.Label)?.Outcome:null,round==current.Round);
        }).ToArray();
    }
    public static string ReadyLabel(MatchState state,IReadOnlySet<int> votes,int seat)
    {
        var humans=state.Players.Where(p=>p.Alive&&p.IsHuman).ToArray();int ready=humans.Count(p=>votes.Contains(p.Id));
        return votes.Contains(seat)?$"Ready {ready}/{humans.Length}":$"Ready ({ready}/{humans.Length})";
    }
}
