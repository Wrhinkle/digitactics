using System.Text.Json.Nodes;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed class LegendNavigationSmoke
{
    private sealed record Step(string Name,Func<MatchScreen,bool> Input,Action<MatchScreen>? Check=null,string? Capture=null);
    private readonly List<Step> steps=[];
    private readonly NativeProfileStore profile;
    private readonly HashSet<int> captures=[];
    private int index,checkedIndex=-1,cooldown=5;
    private bool down,mouse;
    private ImGuiKey key;
    public string Pending=>steps.ElementAtOrDefault(index)?.Name??"done";
    public LegendNavigationSmoke(NativeProfileStore profile)
    {
        this.profile=profile;Require(!profile.Exists,"Legend navigation requires an empty isolated profile.");
        var wallet=new CosmeticCollection("{\"bits\":30000,\"owned\":{},\"equipped\":{}}");wallet.Grant("legend","bukamon");wallet.Equip("legend","tokomon");
        Save(new(){{"dt.name","Legend Tamer"},{"dt.cosmetics.v1",wallet.Json}});
        steps.Add(new("Home partner actions",s=>true,s=>
        {
            var b=s.HomePartnerBounds;
            foreach(string id in new[]{"homeLegendChange","homeLegendEvolution","homeLegendFarm","homeRuleset","homePace","homeFormat","start"})
                Require(s.Buttons.TryGetValue(id,out var p)&&p.X>b.Min.X+8&&p.X<b.Max.X-8&&p.Y>b.Min.Y+8&&p.Y<b.Max.Y-8,"Home partner control is clipped: "+id);
            Require(s.Buttons["homeLegendFarm"].Y+36<s.Buttons["homeRuleset"].Y,"Partner actions overlap the launch selectors.");
        },"home"));
        Click("homeLegendChange",s=>Require(s.CollectionVisible&&s.CollectionFilterKind=="legend"&&s.CollectionSelectedId=="tokomon","Home Change did not select the current legend in Collection."));
        Key("Change returns Home",ImGuiKey.Escape,s=>Require(!s.CollectionVisible&&s.Buttons.ContainsKey("homeLegendChange"),"Change Escape lost Home."));
        Click("homeLegendFarm",s=>Require(s.FarmSelectedResident=="tokomon"&&s.Buttons.ContainsKey("closeFarm"),"Home Farm did not select the equipped resident."));Click("closeFarm");
        Click("homeLegendEvolution",s=>Require(s.EvolutionSelectedLegend=="tokomon"&&s.EvolutionSelectableLegends.Length==4&&s.EvolutionSelectableLegends.All(id=>Wallet().Owns("legend",id)),"Home Digivolve or owned-only picker differs from the collection."),"evolution");
        Click("evolutionLegend/bukamon",s=>Require(s.EvolutionSelectedLegend=="bukamon"&&s.DisplayedEvolutionForm=="bukamon","Owned portrait did not switch the displayed evolution."));
        Click("equipEvolutionLegend",s=>Require(Wallet().Equipped("legend")?.GetValue<string>()=="bukamon","Evolution Equip did not update the equipped legend."));
        Click("closeEvolution",s=>Require(!s.CollectionVisible&&s.Buttons.ContainsKey("homeLegendFarm"),"Home Digivolve back did not return Home."));
        Click("homeLegendFarm",s=>Require(s.FarmSelectedResident=="bukamon","Home Farm did not follow the newly equipped legend."));Click("closeFarm");
        Click("homeLegendEvolution");Click("evolutionLegend/tokomon");Click("levelLegend");Click("levelLegend");
        Click("digivolve",s=>Require(s.EvolutionBusy&&s.EvolutionCareScroll<1,"Evolution did not show its real transformation at the top of the care panel."),"busy");
        Click("evolutionLegend/bukamon",s=>Require(s.EvolutionBusy&&s.EvolutionSelectedLegend=="tokomon","Busy transformation allowed switching its selected legend."));
        Click("equipEvolutionLegend",s=>Require(s.EvolutionBusy&&Wallet().Equipped("legend")?.GetValue<string>()=="bukamon","Busy transformation allowed an Equip mutation."));
        Key("Busy Escape",ImGuiKey.Escape,s=>Require(s.EvolutionBusy&&s.CollectionVisible,"Busy Escape left the transformation."));
        Click("closeEvolution",s=>Require(s.EvolutionBusy&&s.CollectionVisible,"Busy Back left the transformation."));
        steps.Add(new("Wait for evolution",s=>!s.EvolutionBusy));Click("closeEvolution");
        string last=CosmeticCollection.Items("legend").Last().Id;
        var longest=CosmeticCollection.Items("legend").SelectMany(item=>LegendEvolution.Tree(item.Id).Edges.Select(edge=>(Legend:item.Id,Parent:edge[0],Child:edge[1])))
            .OrderByDescending(edge=>LegendEvolution.Form(edge.Child).Name.Length).First();
        steps.Add(new("Owned selector overflow and long odds fixture",s=>
        {
            var all=Wallet();foreach(var item in CosmeticCollection.Items("legend"))all.Grant("legend",item.Id);
            var result=LegendRewards.Duplicate(profile.Storage.GetValueOrDefault("dt.legendevo.v1"),longest.Legend);
            var save=JsonNode.Parse(result.Save!)!;var legend=save["legends"]![longest.Legend]!;
            legend["form"]=longest.Parent;legend["xp"]=1800;legend["dex"]=new JsonArray(longest.Legend,longest.Parent);legend["path"]=new JsonArray(longest.Legend);
            Save(new(){{"dt.cosmetics.v1",all.Json},{"dt.legendevo.v1",save.ToJsonString()}});return true;
        }));
        Click("homeLegendEvolution");Click("evolutionLegend/"+last,s=>Require(s.EvolutionSelectedLegend==last,"Horizontal owned-selector overflow cannot reach its final legend."),"selector-scrolled");
        Click("evolutionLegend/"+longest.Legend,s=>Require(s.DisplayedEvolutionForm==longest.Parent,"Long odds fixture selected a different form."));
        steps.Add(new("Long branch name and bar",s=>
        {
            if(!s.EvolutionOddsRows.TryGetValue(longest.Child,out var row))return false;
            var b=s.EvolutionOddsBounds;
            if(row.Min.Y<b.Min.Y+6||row.Max.Y>b.Max.Y-6)
            {
                ImGui.GetIO().AddMousePosEvent(b.Max.X-20,(b.Min.Y+b.Max.Y)/2);
                ImGui.GetIO().AddMouseWheelEvent(0,row.Min.Y<b.Min.Y+6?1:-1);cooldown=3;return false;
            }
            ImGui.GetIO().AddMousePosEvent(2,2);return true;
        },s=>Require(s.EvolutionTreePortraitSize>=25&&s.EvolutionTreeNodes==s.EvolutionTreePortraits,"Branch odds squeezed the inline evolution tree below readable portraits."),"long-odds"));
        Key("Final Home return",ImGuiKey.Escape,s=>Require(!s.CollectionVisible&&s.Buttons.ContainsKey("homeLegendEvolution"),"Final evolution Escape did not return Home."));
    }
    private CosmeticCollection Wallet()=>new(profile.Storage.GetValueOrDefault("dt.cosmetics.v1"));
    private void Save(Dictionary<string,string> values)=>Require(profile.UpdateStorage(values),profile.Error??"Legend navigation fixture could not save.");
    private static void Require(bool okay,string error){if(!okay)throw new InvalidOperationException(error);}
    private void Click(string id,Action<MatchScreen>? check=null,string? capture=null)=>steps.Add(new(id,s=>Pointer(s,id),check,capture));
    private void Key(string name,ImGuiKey next,Action<MatchScreen>? check=null)=>steps.Add(new(name,_=>{ImGui.GetIO().AddMousePosEvent(2,2);ImGui.GetIO().AddKeyEvent(next,true);key=next;down=true;return true;},check));
    private bool Pointer(MatchScreen s,string id)
    {
        if(!s.Buttons.TryGetValue(id,out var at))return false;
        var io=ImGui.GetIO();
        if(id.StartsWith("evolutionLegend/",StringComparison.Ordinal))
        {
            var b=s.EvolutionSelectorBounds;
            if(at.X-28<b.Min.X||at.X+28>b.Max.X){io.AddMousePosEvent((b.Min.X+b.Max.X)/2,(b.Min.Y+b.Max.Y)/2);io.AddMouseWheelEvent(at.X<b.Min.X+29?4:-4,0);cooldown=3;return false;}
        }
        if(id is "equipEvolutionLegend" or "levelLegend" or "digivolve")
        {
            var b=s.EvolutionCareBounds;
            if(at.Y-18<b.Min.Y+3||at.Y+18>b.Max.Y-3){io.AddMousePosEvent(b.Max.X-3,(b.Min.Y+b.Max.Y)/2);io.AddMouseWheelEvent(0,at.Y<b.Min.Y+21?3:-3);cooldown=3;return false;}
        }
        io.AddMousePosEvent(at.X,at.Y);io.AddMouseButtonEvent(0,true);down=mouse=true;return true;
    }
    public void Input(MatchScreen screen)
    {
        if(cooldown-->0)return;
        if(down){if(mouse)ImGui.GetIO().AddMouseButtonEvent(0,false);else ImGui.GetIO().AddKeyEvent(key,false);down=mouse=false;cooldown=3;return;}
        if(index>=steps.Count)return;
        if(index>0&&checkedIndex<index-1){steps[index-1].Check?.Invoke(screen);checkedIndex=index-1;}
        if(steps[index].Input(screen)){index++;cooldown=3;}
    }
    public bool Verify(MatchScreen screen,int2 size,string? capture)
    {
        if(!down&&cooldown==0&&index>0&&steps[index-1].Capture is {} name&&captures.Add(index-1)&&capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,name+".bmp"),size.X,size.Y);
        if(index<steps.Count||down||cooldown>0)return false;steps[^1].Check?.Invoke(screen);
        Console.WriteLine($"Legend navigation UI passed {steps.Count} steps: Home Change/Digivolve/Farm origins, owned selector/overflow, Equip, busy guards and long-name odds.");return true;
    }
}
