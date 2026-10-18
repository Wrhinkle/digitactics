using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;
using DigiTactics.Rules;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigiTactics.Desktop;

internal sealed class ProfileUiSmoke : IDisposable
{
    private readonly NativeProfileStore profile;
    private readonly string avatar;
    private int step,cooldown=5;
    private bool down,overview,scrolled;
    public ProfileUiSmoke(MatchScreen screen,NativeProfileStore profile)
    {
        this.profile=profile;
        if(profile.Exists)throw new InvalidOperationException("Profile smoke requires a new empty isolated profile directory.");
        avatar=CosmeticCollection.Items("legend").First(x=>x.Free).Id;
        string trait=ContentCatalog.Load(Path.Combine(AppContext.BaseDirectory,"content/catalog.json")).Variants[0].Traits[0].Id;
        long now=DateTimeOffset.Now.ToUnixTimeMilliseconds();
        var matches=Enumerable.Range(0,8).Select(i=>new HistoryMatch("profile-smoke-"+i,now-i*3600000,i%2==0?"solo":"mp",i%3==0?"duos":"solo","faction-wars",i%4+1,"3-2",7,
            [new(trait,2,1)],[new("patamon",i%3,[]),new("gabumon",0,[])],JsonSerializer.SerializeToNode(new RewardStats(4,1,3,7,i%4+1),new JsonSerializerOptions{PropertyNamingPolicy=JsonNamingPolicy.CamelCase}),130+i)).ToArray();
        string client=JsonSerializer.Serialize(new {profile=new {avatar=(string?)null,banner="profile-1"},future=new {preserve=true},matches,totals=new {games=12,wins=2,top4=6,placeSum=37,traits=new Dictionary<string,int>{{trait,5}},units=new {patamon=8,gabumon=4}}},new JsonSerializerOptions{PropertyNamingPolicy=JsonNamingPolicy.CamelCase});
        string wallet=JsonSerializer.Serialize(new {bits=2000,owned=new {},equipped=new {},history=Enumerable.Range(0,18).Select(i=>new {n=100+i,reason="Game: placed #"+(i%8+1),at=now-i*3600000})});
        if(!profile.UpdateStorage(new Dictionary<string,string>{{"dt.name",""},{"dt.client.v1",client},{"dt.cosmetics.v1",wallet}}))throw new InvalidOperationException(profile.Error);
    }
    public void Input(MatchScreen screen)
    {
        if(cooldown-->0)return;
        if(down) {ImGui.GetIO().AddMouseButtonEvent(0,false);down=false;step++;cooldown=5;return;}
        if(step==5) {ImGui.GetIO().AddInputCharactersUTF8("Profile% Tamer!"u8);step++;cooldown=5;return;}
        if(step==6) {ImGui.GetIO().AddMousePosEvent(600,65);ImGui.GetIO().AddMouseButtonEvent(0,true);down=true;return;}
        if(step is 7 or 8) {ImGui.GetIO().AddMousePosEvent(step==7?130:650,430);ImGui.GetIO().AddMouseWheelEvent(0,step==7?-35:-100);step++;cooldown=10;return;}
        if(step>=11)return;
        string key=step switch {0=>"profile",1=>"profileBanner/profile-3",2=>"profileAvatar",3=>"profileAvatar/"+avatar,4=>"profileName",9=>"closeProfile",10=>"profile",_=>throw new InvalidOperationException("Unexpected profile smoke step.")};
        if(!screen.Buttons.TryGetValue(key,out var point))return;
        if(point.X<0 || point.Y<0 || point.X>=ImGui.GetIO().DisplaySize.X || point.Y>=ImGui.GetIO().DisplaySize.Y)throw new InvalidOperationException("Profile control outside window: "+key);
        ImGui.GetIO().AddMousePosEvent(point.X,point.Y);ImGui.GetIO().AddMouseButtonEvent(0,true);down=true;
    }
    public bool Verify(MatchScreen screen,int2 size,string? capture)
    {
        if(step==2 && cooldown<3 && !overview)
        {
            if(!screen.ProfileVisible || screen.ProfileHistoryRows!=8 || screen.ProfilePortraits<9 || screen.ProfileMissions!=2 || screen.DisplayedProfile?.Banner!="profile-3")throw new InvalidOperationException($"Profile overview did not render saved content: visible={screen.ProfileVisible}, history={screen.ProfileHistoryRows}, portraits={screen.ProfilePortraits}, missions={screen.ProfileMissions}, banner={screen.DisplayedProfile?.Banner}");
            if(capture is not null)FrameCapture.SaveBitmap(capture,size.X,size.Y);overview=true;
        }
        if(step==9 && cooldown<2 && !scrolled)
        {
            if(screen.ProfileStatsScroll<=0 || screen.ProfileHistoryScroll<=0)throw new InvalidOperationException("Profile statistics/history did not scroll through real mouse input.");
            if(capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"scrolled.bmp"),size.X,size.Y);scrolled=true;
        }
        if(step<11 || cooldown>0)return false;
        var summary=ProfileSummary.Read(profile.Storage["dt.client.v1"],profile.Storage["dt.cosmetics.v1"]);
        if(!overview || !scrolled || !screen.ProfileVisible || summary.Avatar!=avatar || summary.Banner!="profile-3" || profile.Storage["dt.name"]!="Profile Tamer" || summary.Games!=12 || summary.Matches.Length!=8 || JsonNode.Parse(profile.Storage["dt.client.v1"])!["future"]!["preserve"]!.GetValue<bool>()!=true)
            throw new InvalidOperationException("Profile editing did not preserve history, sanitize name or survive panel reopening.");
        Console.WriteLine("Profile UI passed: source stats/history/charts/portraits/missions, banner/avatar/name edits, preserved history and actual two-column scrolling.");return true;
    }
    public void Dispose() { }
}
