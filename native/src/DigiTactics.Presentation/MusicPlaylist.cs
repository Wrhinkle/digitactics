namespace DigiTactics.Presentation;

public sealed record PlaylistTrack(string Id,string Use);
public sealed record MusicTransition(string? TrackId,double FadeSeconds);

/// <summary>Independent phase shuffle bags matching the original custom music player; never uses match RNG.</summary>
public sealed class MusicPlaylist(Func<double>? random = null)
{
    private readonly Func<double> random = random ?? Random.Shared.NextDouble;
    private readonly Dictionary<string,List<string>> bags = [];
    private IReadOnlyList<PlaylistTrack> tracks = [];
    public PlaylistTrack? Current { get; private set; }
    public bool Playing { get; private set; }
    public bool Failed { get; private set; }
    public int Failures { get; private set; }
    public string Phase { get; private set; } = "planning";
    public void SetTracks(IEnumerable<PlaylistTrack> value)
    {
        var next=value.ToArray();
        if(next.Any(t=>string.IsNullOrEmpty(t.Id) || t.Use is not ("planning" or "combat" or "both")) || next.Select(t=>t.Id).Distinct(StringComparer.Ordinal).Count()!=next.Length)
            throw new ArgumentException("Music tracks require unique IDs and valid phases.",nameof(value));
        tracks=next;
        if(Current is { } current) Current=tracks.FirstOrDefault(t=>t.Id==current.Id)??current;
    }
    private static bool Fits(PlaylistTrack track,string phase) => track.Use=="both" || track.Use==phase;
    private PlaylistTrack? Pick()
    {
        var pool=tracks.Where(t=>Fits(t,Phase)).ToArray();
        var from=pool.Length>0?pool:tracks;
        if(from.Count==0) return null;
        var ids=from.Select(t=>t.Id).ToHashSet(StringComparer.Ordinal);
        var bag=bags.GetValueOrDefault(Phase,[]).Where(ids.Contains).ToList();
        if(bag.Count==0)
        {
            bag=from.Select(t=>t.Id).ToList();
            for(int i=bag.Count-1;i>0;i--)
            {
                double sample=random();
                if(!double.IsFinite(sample) || sample<0 || sample>=1) throw new InvalidOperationException("Playlist random values must be in [0,1).");
                int j=(int)Math.Floor(sample*(i+1)); (bag[i],bag[j])=(bag[j],bag[i]);
            }
            if(bag.Count>1 && bag[0]==Current?.Id) { bag.Add(bag[0]); bag.RemoveAt(0); }
        }
        string id=bag[0]; bag.RemoveAt(0); bags[Phase]=bag;
        return from.Single(t=>t.Id==id);
    }
    private MusicTransition? Play(double fade)
    {
        var next=Pick(); if(next is null) return null;
        Current=next; return new(next.Id,fade);
    }
    public void SetPhaseContext(string phase) { ValidatePhase(phase); Phase=phase; }
    public MusicTransition? Start(string phase)
    {
        ValidatePhase(phase); Phase=phase;
        if(Playing || tracks.Count==0) return null;
        Playing=true; Failed=false; return Play(1);
    }
    public MusicTransition? Next(double fade=.8) => Playing?Play(fade):null;
    public MusicTransition? ChangePhase(string phase)
    {
        ValidatePhase(phase); Phase=phase;
        if(!Playing || Current is null || Fits(Current,phase) || !tracks.Any(t=>Fits(t,phase))) return null;
        return Play(1.5);
    }
    public MusicTransition? Stop(double fade=1)
    {
        if(!Playing) return null;
        Playing=false; Current=null; return new(null,fade);
    }
    public void PlaybackStarted() => Failures=0;
    public MusicTransition? PlaybackFailed()
    {
        if(++Failures<Math.Max(3,tracks.Count)) return Next(.1);
        var transition=Stop(.1); Failed=true; return transition;
    }
    private static void ValidatePhase(string phase)
    {
        if(phase is not ("planning" or "combat")) throw new ArgumentException("Unknown music phase.",nameof(phase));
    }
}
