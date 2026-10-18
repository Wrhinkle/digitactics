using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Presentation;
using DigiTactics.Storage;

internal static class LootNoticeChecks
{
    public static void Run(string root)
    {
        var queue=new LootAwardNotices();int checks=0;
        void Check(bool ok,string why){if(!ok)throw new InvalidOperationException("Loot notification: "+why);checks++;}
        using var fixtures=JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"tests/fixtures/loot-notices.json")));
        var options=new JsonSerializerOptions{PropertyNamingPolicy=JsonNamingPolicy.CamelCase};
        foreach(var test in fixtures.RootElement.EnumerateArray())
        {
            var action=test.GetProperty("action");bool? result=null;
            switch(action.GetProperty("op").GetString())
            {
                case "earn":result=queue.Earned(action.GetProperty("id").GetString()!,action.GetProperty("round").GetString()!,action.GetProperty("drops").EnumerateArray().Select(d=>d.GetString()!));break;
                case "advance":queue.Advance(action.GetProperty("seconds").GetDouble(),action.TryGetProperty("paused",out var paused)&&paused.GetBoolean());break;
                case "dismiss":queue.Dismiss();break;
                case "clear":queue.Clear();break;
                case "reload":queue=new();break;
            }
            Check(JsonNode.DeepEquals(JsonSerializer.SerializeToNode(result),JsonNode.Parse(test.GetProperty("result").GetRawText())),"source acceptance/dedup result");
            var current=JsonSerializer.SerializeToNode(new{current=queue.Current,elapsed=queue.Elapsed,pendingCount=queue.PendingCount},options);
            Check(JsonNode.DeepEquals(current,JsonNode.Parse(test.GetProperty("expected").GetRawText())),"source queue/count/source/lifetime state");
        }
        string path=Path.Combine(Path.GetTempPath(),"digitactics-loot-notice-"+Guid.NewGuid().ToString("N"));
        try
        {
            var profile=new NativeProfileStore(path);var day=new DateOnly(2026,10,18);
            Check(profile.UpdateStorage(new Dictionary<string,string>{{MysteryLoot.StorageKey,"{\"day\":\"2026-10-17\",\"dayProgress\":1}"}}),"isolated profile seed");
            var progress=new MysteryLootProgress(profile);var round=new MysteryRound("fates",false,143,0,2,1,true);
            var drops=progress.RecordRound(round,day,()=>0);
            Check(drops.SequenceEqual(new[]{"Daybreak Box","Cipher Box","Cipher Key"}),"actual committed activity/box/key award");
            queue=new();Check(queue.Earned("143:201","2-1",drops),"committed award notification");
            Check(progress.RecordRound(round,day,()=>0).Length==0,"duplicate settlement cannot produce new notice");
            var reloaded=new NativeProfileStore(path);var recovered=new MysteryLootProgress(reloaded);
            Check(recovered.Recover()&&recovered.RecordRound(round,day,()=>0).Length==0&&new LootAwardNotices().Current is null,"reload/recovery does not reannounce inventory");
            using(var held=new FileStream(Path.Combine(path,"profile.json.lock"),FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None))
                Check(progress.RecordRound(round with{Round=2},day,()=>0).Length==0,"failed durable award creates no notice");
            Check(MysteryLoot.Inventory(reloaded.Storage.GetValueOrDefault(MysteryLoot.StorageKey)) is {Daily:1,Vault:1,Keys:1},"notification never spends or alters inventory");
        }
        finally{if(Directory.Exists(path))Directory.Delete(path,true);}
        Console.WriteLine($"Loot award notification passed {checks} checks: source/native queue parity, grouped counts/reasons, expiry/pause, secret-name filter, successful-write gating and reload/settlement idempotence.");
    }
}
