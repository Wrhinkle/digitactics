using DigiTactics.Assets;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Store kill-screen previews use the same full dancing stage as the match, with the source sample team.</summary>
internal sealed class CollectionKillPreview : IDisposable
{
    private static readonly Lazy<ModelCatalog> Models=new(()=>ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory,"content/models.json")));
    private readonly EliminationPlayback playback;
    private readonly EliminationStage stage;
    private double? previousTime;
    public int DrawCalls=>stage.DrawCalls;
    public int Members=>stage.Members;
    public uint Texture=>stage.Texture;
    public CollectionKillPreview(Ruleset rules,string legend,string dance,string theme,EliminationMember[]? currentTeam=null)
    {
        var team=currentTeam is {Length:>0}?currentTeam:SampleTeam(rules);
        var player=new EliminationPlayer(0,"Preview",true,0,null,team);
        var entry=new EliminationEvent(player,player,false,[player],[player],null,team);
        playback=new(entry,-1);stage=new(rules,playback,legend,dance,theme);
    }
    private static EliminationMember[] SampleTeam(Ruleset rules)
    {
        var modeled=rules.Lines.Where(line=>Models.Value.Resolve(rules.Id,line.Id,0) is not null).ToArray();
        var pool=(modeled.Length>=9?modeled:rules.Lines).OrderByDescending(line=>line.Tier).ThenBy(line=>line.Id,StringComparer.Ordinal).ToArray();
        int[] stages=[2,2,1,1,1,1,0,0,0];var result=new List<EliminationMember>();
        for(int i=0;i<9&&pool.Length>0;i++)
        {
            var line=pool[i*pool.Length/9];if(result.Any(x=>x.Line==line.Id))continue;
            result.Add(new(line.Id,Math.Min(stages[i],line.Forms.Length-1)));
        }
        return result.ToArray();
    }
    public void Draw(int width,int height,double time)
    {
        playback.Advance(previousTime is {} before?Math.Clamp(time-before,0,.1):0);previousTime=time;stage.Draw(width,height);
    }
    public void Dispose()=>stage.Dispose();
}
