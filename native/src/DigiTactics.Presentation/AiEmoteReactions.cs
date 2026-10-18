using System.Text.Json;
using DigiTactics.Storage;

namespace DigiTactics.Presentation;

public sealed record AiEmotePlayer(int Seat, string Name, bool Human, bool Alive);
public sealed record AiEmoteResult(int A, int? B, bool Pve, bool Ghost, int Winner);
public sealed record AiEmoteReaction(int Seat, string Id, double Delay);

/// <summary>Local cosmetic reactions only; never consumes the match's deterministic random stream.</summary>
public sealed class AiEmoteReactions(IEnumerable<int> alive)
{
    private HashSet<int> aliveIds = alive.ToHashSet();
    public AiEmoteReaction? Choose(IReadOnlyList<AiEmotePlayer> players, IEnumerable<AiEmoteResult> results, int localSeat,
        Func<double>? random = null, Func<AiEmotePlayer, string?[]>? loadout = null)
    {
        random ??= Random.Shared.NextDouble;
        loadout ??= p => CosmeticLoadouts.Ai(p.Seat,p.Name)["emote"]!.AsArray().Select(x=>x?.GetValue<string>()).ToArray();
        bool IsAi(AiEmotePlayer? p) => p is not null && !p.Human && p.Seat != localSeat;
        string? Pick(AiEmotePlayer p, bool win)
        {
            var ids = loadout(p).Where(id =>
            {
                var item = CosmeticCollection.Item("emote",id);
                string? mood = item?.Data.ValueKind == JsonValueKind.Object && item.Data.TryGetProperty("mood",out var value) ? value.GetString() : null;
                return mood == "any" || mood == (win ? "good" : "bad");
            }).ToArray();
            return ids.Length == 0 ? null : ids[(int)(random()*ids.Length)];
        }
        var candidates = new List<(AiEmotePlayer Player, int Outcome)>();
        foreach(var result in results)
        {
            if(result.Pve) continue;
            var a=players.FirstOrDefault(p=>p.Seat==result.A);var b=players.FirstOrDefault(p=>p.Seat==result.B);
            if(IsAi(a))candidates.Add((a!,result.Winner==0?1:result.Winner==1?-1:0));
            if(!result.Ghost && IsAi(b))candidates.Add((b!,result.Winner==1?1:result.Winner==0?-1:0));
        }
        var eliminated=players.Where(p=>aliveIds.Contains(p.Seat)&&!p.Alive&&IsAi(p)).ToArray();
        aliveIds=players.Where(p=>p.Alive).Select(p=>p.Seat).ToHashSet();
        AiEmotePlayer? chosen=null;string? emote=null;
        foreach(var p in eliminated)if(random()<.4){chosen=p;emote=Pick(p,false);break;}
        if(chosen is null)
        {
            RandomSort(candidates,random);
            foreach(var (p,outcome) in candidates)
                if(outcome!=0 && random()<(outcome==1?.05:.035)){chosen=p;emote=Pick(p,outcome==1);break;}
        }
        return chosen is not null && emote is not null ? new(chosen.Seat,emote,500+random()*1500) : null;
    }
    // The original uses Array.sort with a random comparator. For the <=8 round
    // candidates, V8 detects the initial run and finishes with stable binary insertion.
    private static void RandomSort<T>(List<T> values,Func<double> random)
    {
        if(values.Count<2)return;
        bool descending=random()<.5;int run=2;
        while(run<values.Count){bool less=random()<.5;if(less!=descending)break;run++;}
        if(descending)values.Reverse(0,run);
        for(int i=run;i<values.Count;i++)
        {
            var pivot=values[i];int left=0,right=i;
            while(left<right){int middle=left+(right-left)/2;if(random()<.5)right=middle;else left=middle+1;}
            values.RemoveAt(i);values.Insert(left,pivot);
        }
    }
}
