using System.Reflection;
using System.Text.Json;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

/// <summary>Durable real-round rewards, then actual hover, click and keyboard notice interactions.</summary>
internal sealed class LootNoticeUiSmoke
{
    private static readonly BindingFlags Private=BindingFlags.NonPublic|BindingFlags.Instance;
    private readonly ContentCatalog catalog;
    private readonly NativeProfileStore profile;
    private int step,frames;
    private double heldElapsed;
    private MatchPlanningSession? session;
    public string Pending=>$"loot notice step {step}, frame {frames}";
    public LootNoticeUiSmoke(ContentCatalog catalog,NativeProfileStore profile)
    {
        if(profile.Exists)throw new InvalidOperationException("Loot notice smoke requires a new isolated profile.");
        this.catalog=catalog;this.profile=profile;
        var yesterday=DateOnly.FromDateTime(DateTime.Now).AddDays(-1).ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture);
        if(!profile.UpdateStorage(new Dictionary<string,string>{{MysteryLoot.StorageKey,JsonSerializer.Serialize(new{day=yesterday,dayProgress=1})}}))throw new InvalidOperationException(profile.Error);
    }
    private static void Invoke(MatchScreen screen,string method)=>typeof(MatchScreen).GetMethod(method,Private)!.Invoke(screen,null);
    private void Finish(MatchScreen screen)
    {
        while(session!.Rounds.RoundType=="carousel"){session.StartCombat();session.FinishCombat();session.Advance();}
        session!.StartCombat();session.FinishCombat();Invoke(screen,"AwardMysteryRound");
    }
    public void Input(MatchScreen screen)
    {
        frames++;
        switch(step)
        {
            case 0:
                session=new(catalog.Variants.Single(r=>r.Id=="fates/normal"),143,Enumerable.Range(0,8).Select(i=>new MatchSeat("Loot Tamer "+i,true)).ToArray());
                while(session.Rounds.Position.Stage<2){session.StartCombat();session.FinishCombat();session.Advance();}
                typeof(MatchScreen).GetProperty(nameof(MatchScreen.Session))!.SetValue(screen,session);
                typeof(MatchScreen).GetField("localPaused",Private)!.SetValue(screen,true);
                screen.MysteryRandom=()=>0;Finish(screen);Invoke(screen,"AwardMysteryRound");
                ImGui.GetIO().AddMousePosEvent(0,0);step=1;frames=0;break;
            case 2:
                var b=screen.LootAwardBounds;ImGui.GetIO().AddMousePosEvent(b.Min.X+20,b.Min.Y+20);
                if(frames==3)heldElapsed=screen.LootAwardElapsed;
                if(frames>340){if(screen.CurrentLootAward is null||Math.Abs(screen.LootAwardElapsed-heldElapsed)>.04)throw new InvalidOperationException("Hover did not pause award notice beyond its normal lifetime.");step=3;frames=0;}
                break;
            case 3:
                session!.Advance();Finish(screen);session.Advance();Finish(screen);
                if(screen.QueuedLootAwards!=4)throw new InvalidOperationException("Two pending rounds lost grouped box/key counts.");step=4;frames=0;break;
            case 5:
                var at=screen.Buttons["dismissLootAward"];ImGui.GetIO().AddMousePosEvent(at.X,at.Y);ImGui.GetIO().AddMouseButtonEvent(0,frames==1);
                if(frames==3){step=6;frames=0;}break;
            case 7:
                ImGui.GetIO().AddKeyEvent(ImGuiKey.Escape,frames==1);
                if(frames==3){step=8;frames=0;}break;
            case 9:
                if(!new ClientPreferencesStore(profile).Save(new(false,true,true)))throw new InvalidOperationException("Reduced-motion fixture could not persist.");
                session!.Advance();Finish(screen);ImGui.GetIO().AddMousePosEvent(0,0);step=10;frames=0;break;
            case 11:
                var point=screen.Buttons["dismissLootAward"];ImGui.GetIO().AddMousePosEvent(point.X,point.Y);ImGui.GetIO().AddMouseButtonEvent(0,frames==1);
                if(frames==3){step=12;frames=0;}break;
        }
    }
    public bool Verify(MatchScreen screen,int2 size,string? capture)
    {
        if(step is 1 or 4 or 6 or 10 && frames>=18)
        {
            if(!screen.LootAwardVisible||screen.CurrentLootAward is not {} award)throw new InvalidOperationException($"Earned loot notice did not render: step {step}, visible {screen.LootAwardVisible}, current {screen.CurrentLootAward}, elapsed {screen.LootAwardElapsed}.");
            var bounds=screen.LootAwardBounds;
            if(bounds.Min.X<0||bounds.Min.Y<size.Y/2f||bounds.Max.X>size.X||bounds.Max.Y>size.Y-20)throw new InvalidOperationException($"Award notice escaped readable lower-third bounds: {bounds}, size {size}, elapsed {screen.LootAwardElapsed}, step {step}.");
            if(step==1 && (!award.Items.Select(i=>i.Name).SequenceEqual(new[]{"Daybreak Box","Cipher Box","Cipher Key"})||award.Items.Any(i=>i.Count!=1)||screen.MysteryInventory is not{Daily:1,Vault:1,Keys:1}))throw new InvalidOperationException("Notice differs from the actual committed round inventory.");
            if(step==6 && (award.Items.Count!=2||award.Items.Any(i=>i.Count!=2||!i.Source.Contains("2 rounds",StringComparison.Ordinal))))throw new InvalidOperationException("Grouped notice did not conserve pending item counts and source rounds.");
            if(step==10 && (screen.InterfacePreferences.Motion||Math.Abs(bounds.Max.Y-(size.Y-24))>.1))throw new InvalidOperationException("Reduced motion award did not settle immediately.");
            if(capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,step switch{1=>"earned.bmp",4=>"queued.bmp",6=>"grouped.bmp",_=>"reduced.bmp"}),size.X,size.Y);
            step++;frames=0;
        }
        if(step==8&&frames>=3)
        {
            if(screen.CurrentLootAward is not null||screen.LootAwardVisible||screen.Buttons.ContainsKey("requestLeave"))throw new InvalidOperationException("Focused Escape did not dismiss only the reward notice.");
            step=9;frames=0;
        }
        if(step!=12||frames<3)return false;
        if(screen.CurrentLootAward is not null||screen.MysteryInventory is not{Daily:1,Vault:4,Keys:4})throw new InvalidOperationException("Dismiss altered earned inventory or left stale notice.");
        Console.WriteLine("Loot notice UI passed: real committed rounds, three earned names/icons/counts/sources, duplicate settlement suppression, lower-third bounds, hover lifetime pause, grouped pending counts, actual Dismiss/focused Escape, reduced motion, and unchanged saved inventory.");return true;
    }
}
