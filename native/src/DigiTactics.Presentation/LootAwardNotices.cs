namespace DigiTactics.Presentation;

public sealed record LootAwardItem(string Id,string Name,int Count,string Source);
public sealed record LootAwardNotice(long Serial,IReadOnlyList<LootAwardItem> Items);

/// <summary>Ephemeral presentation of already committed awards. Inventory is never scanned to
/// create notices, and unopened cosmetic contents never enter this queue.</summary>
public sealed class LootAwardNotices
{
    public const double Duration=10;
    private sealed class Item(string id,string name){public readonly string Id=id,Name=name;public int Count,Rounds;public string LastRound="";}
    private sealed class Batch(long serial){public readonly long Serial=serial;public readonly Dictionary<string,Item> Items=[];}
    private readonly HashSet<string> seen=[];
    private readonly Queue<string> recent=[];
    private Batch? current,pending;
    private long serial;
    public double Elapsed {get;private set;}
    public int PendingCount=>pending?.Items.Values.Sum(i=>i.Count)??0;
    public LootAwardNotice? Current=>current is null?null:new(current.Serial,current.Items.Values.OrderBy(i=>i.Id=="daily"?0:i.Id=="vault"?1:2).Select(i=>new LootAwardItem(i.Id,i.Name,i.Count,
        i.Id=="daily"?"Activity reward · 2 active days completed":i.Rounds==1?$"Round drop · {i.LastRound} complete":$"Round drops · {i.Rounds} rounds, latest {i.LastRound}")).ToArray());
    public bool Earned(string transaction,string round,IEnumerable<string> drops)
    {
        var known=drops.Where(name=>name is "Daybreak Box" or "Cipher Box" or "Cipher Key").GroupBy(name=>name).ToArray();
        if(known.Length==0||!seen.Add(transaction))return false;
        recent.Enqueue(transaction);while(recent.Count>1000)seen.Remove(recent.Dequeue());
        Batch target;
        if(current is null){current=new(++serial);target=current;Elapsed=0;}
        else {pending??=new(++serial);target=pending;}
        foreach(var group in known)
        {
            string id=group.Key=="Daybreak Box"?"daily":group.Key=="Cipher Box"?"vault":"key";
            if(!target.Items.TryGetValue(id,out var item))target.Items[id]=item=new(id,group.Key);
            item.Count=checked(item.Count+group.Count());item.Rounds++;item.LastRound=round;
        }
        return true;
    }
    public void Advance(double seconds,bool paused=false)
    {
        if(!double.IsFinite(seconds)||seconds<0)throw new ArgumentOutOfRangeException(nameof(seconds));
        if(current is null||paused)return;
        Elapsed+=seconds;if(Elapsed>=Duration)Dismiss();
    }
    public void Dismiss(){current=pending;pending=null;Elapsed=0;}
    public void Clear(){current=pending=null;Elapsed=0;seen.Clear();recent.Clear();}
}
