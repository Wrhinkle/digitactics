using DigiTactics.Presentation;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class GameAudio
{
    private sealed record BoardDuck(long Start,long End);
    private readonly List<BoardDuck> scheduledBoardDucks=[];
    public void PlayEliminationTheme(string? variant,bool big,bool pve=false)
    {
        string theme=EliminationThemes.Resolve(variant,pve);
        // Mystery visual themes deliberately share the default sting in the source game.
        if(theme.StartsWith("mystery_",StringComparison.Ordinal))theme="basic";
        Play("killscreen_"+theme+(big?"_big":"_normal"));
    }
    public bool PlayEliminationSound(string? id,bool mine,bool preview=false,double delay=.3)
    {
        if(!mine&&!preview&&MuteOthers)return false;
        var item=CosmeticCollection.Item("elimsound",id)??CosmeticCollection.Item("elimsound","es_chime")!;
        string clip=item.Data.GetProperty("clip"u8).GetString()!;
        return ScheduleBoard(clip,delay);
    }
    private bool ScheduleBoard(string id,double delay)
    {
        string key="clip_"+id;
        if(!Available||Muted||!manifest.Clips.TryGetValue(key,out var clip)||voices.Count(v=>!v.Music)>=40)return false;
        if(!double.IsFinite(delay))throw new ArgumentOutOfRangeException(nameof(delay));
        long start=frame+(long)(Math.Clamp(delay,0,60)*manifest.SampleRate);
        scheduledBoardDucks.Add(new(start,start+(long)((clip.Duration??1)*manifest.SampleRate)));
        scheduledBoardDucks.Sort((a,b)=>a.Start.CompareTo(b.Start));
        voices.Add(new(Load(key),1,board:true){StartFrame=start});ApplyScheduledBoardDucks(frame);Played++;return true;
    }
    private static float EvaluateDuck(long at,long start,long end,float from)
    {
        if(at<start+2205)return from+(.45f-from)*Math.Clamp((at-start)/2205f,0,1);
        if(at<=end)return .45f;
        return .45f+.55f*Math.Clamp((at-end)/17640f,0,1);
    }
    private float ScheduledMusicDuck(long at)
    {
        long start=duckStart,end=duckEnd;float from=duckFrom;
        foreach(var next in scheduledBoardDucks)
        {
            if(next.Start>at)break;
            from=EvaluateDuck(next.Start,start,end,from);start=next.Start;end=next.End;
        }
        return EvaluateDuck(at,start,end,from);
    }
    private void ApplyScheduledBoardDucks(long at)
    {
        while(scheduledBoardDucks.Count>0&&scheduledBoardDucks[0].Start<=at)
        {
            var next=scheduledBoardDucks[0];scheduledBoardDucks.RemoveAt(0);
            duckFrom=MusicDuckCurrent(next.Start);duckStart=next.Start;duckEnd=next.End;
        }
    }
}
