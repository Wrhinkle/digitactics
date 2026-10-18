using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private CosmeticScenePreview? collectionPreview;
    private readonly EggArtwork eggArtwork=new();
    private readonly EliminationThemeRenderer collectionTheme=new();
    private CollectionKillPreview? collectionKillPreview;
    private string? collectionKillSignature,collectionPreviewSignature;
    private double collectionPreviewStarted;
    private string? previewCosmetic;
    private (string Egg,EggAward Award,double Started)? hatching;
    private bool hatchCracked,hatchRevealed;
    private double hatchRevealStarted;
    private (string Kind,string Id)? boughtCosmetic;
    internal bool EggHatchBusy=>hatching is not null && (!hatchRevealed || boardSceneTime-hatchRevealStarted<.4);
    internal double EggHatchAge=>hatching is {} egg?boardSceneTime-egg.Started:-1;
    internal int CollectionPreviewDrawCalls {get;private set;}
    internal double CollectionLegendYaw=>collectionPreview?.PointerYaw??0;
    internal int CollectionPedestalDrawCalls=>collectionPreview?.PedestalDrawCalls??0;
    internal int CollectionLegendReactions=>collectionPreview?.PointerReactionCount??0;
    internal int CollectionPreviewMembers=>collectionKillPreview?.Members??0;
    internal double CollectionPreviewAge=>boardSceneTime-collectionPreviewStarted;
    internal string? CollectionPreviewKind {get;private set;}
    private static ImColor8 RarityColor(string? rarity)=>rarity switch
    {
        "uncommon"=>new(95,211,106),"rare"=>new(79,179,255),"epic"=>new(199,125,255),"legendary"=>new(255,179,71),_=>new(200,210,230)
    };
    private string CurrentPreviewLegend(CosmeticCollection collection,string? legend=null)
    {
        legend??=collection.Equipped("legend")?.GetValue<string>()??"tokomon";
        string form=LegendEvolution.Discovery(profile!.Storage.GetValueOrDefault("dt.legendevo.v1"),legend).Current;
        return LegendEvolution.VisualKey(legend,form);
    }
    private void CosmeticPreview(string kind,CosmeticItem item,CosmeticCollection collection,float width)
    {
        string signature=kind+":"+item.Id;
        if(collectionPreviewSignature!=signature){collectionPreviewSignature=signature;collectionPreviewStarted=boardSceneTime;}
        if(kind!="killscreen" && collectionKillPreview is not null){collectionKillPreview.Dispose();collectionKillPreview=null;collectionKillSignature=null;}
        if(kind is not("legend" or "dance" or "board") && collectionPreview is not null){collectionPreview.Dispose();collectionPreview=null;}
        width=Math.Max(120,width);float height=width*CosmeticScenePreview.Height/CosmeticScenePreview.Width;
        var at=ImGui.GetCursorScreenPos();ImGui.Dummy(new(width,height));var draw=ImGui.GetWindowDrawList();
        draw.AddRectFilled(at,at+new float2(width,height),new ImColor8(12,19,30),8);
        CollectionPreviewDrawCalls=0;CollectionPreviewKind=kind;
        if(kind is "legend" or "dance" or "board")
        {
            collectionPreview??=new(legendPedestal);
            if(kind=="legend")
            {
                Buttons["collectionLegend"]=at+new float2(width/2,height/2);
                Buttons["collectionLegend/outside"]=at+new float2(-12,height/2);
                if(collectionPreview.PointerItem("collectionLegend",at,new(width,height)) is not null)audio.PlayClient("emote",true);
            }
            collectionPreview.Draw(kind,item.Id,CurrentPreviewLegend(collection,kind=="legend"?item.Id:null),boardSceneTime,ClientMotion);
            draw.AddImage(new ImTextureRef{_TexID=new((nint)collectionPreview.Texture)},at,at+new float2(width,height),new(0,1),new(1,0));
            CollectionPreviewDrawCalls=collectionPreview.DrawCalls;
        }
        else if(kind=="emote") {float s=Math.Min(width,height)*.8f;emoteArtwork.Draw(item.Id,(long)(boardSceneTime*1000),at+new float2((width-s)/2,(height-s)/2),s);CollectionPreviewDrawCalls=1;}
        else if(kind=="killscreen")
        {
            collectionTheme.Draw(item.Id,false,(boardSceneTime-collectionPreviewStarted)%3.5,at,new(width,height));
            string legend=CurrentPreviewLegend(collection),dance=collection.Equipped("dance")?.GetValue<string>()??"floss";
            var team=Session?.State.Players[localSeat>=0?localSeat:0].Roster.Entries.Where(x=>x.Spot.IsBoard).Select(x=>new DigiTactics.Presentation.EliminationMember(x.Unit.Line,x.Unit.Stage)).ToArray();
            string teamKey=team is null?"":string.Join(",",team.Select(x=>x.Line+":"+x.Stage));
            string key=Rules.Id+":"+item.Id+":"+legend+":"+dance+":"+teamKey;
            if(collectionKillSignature!=key)
            {
                collectionKillPreview?.Dispose();collectionKillPreview=null;collectionKillSignature=null;
                collectionKillPreview=new(Rules,legend,dance,item.Id,team);collectionKillSignature=key;
            }
            collectionKillPreview!.Draw((int)width,(int)height,boardSceneTime);
            draw.AddImage(new ImTextureRef{_TexID=new((nint)collectionKillPreview.Texture)},at,at+new float2(width,height),new(0,1),new(1,0));
            CollectionPreviewDrawCalls=collectionTheme.Operations+collectionKillPreview.DrawCalls;
        }
        else
        {
            // Original sound previews are explicit play buttons: browsing never starts audio.
            var center=at+new float2(width/2,height/2);draw.AddCircleFilled(center,Math.Min(width,height)*.2f,new ImColor8(34,54,78));
            draw.AddTriangleFilled(center+new float2(-7,-13),center+new float2(-7,13),center+new float2(13,0),RarityColor(item.Rarity));CollectionPreviewDrawCalls=1;
        }
        draw.AddRect(at,at+new float2(width,height),RarityColor(item.Rarity),8,thickness:2);
        if(kind=="killscreen" && Button("previewKill/"+item.Id,"Play with elimination sound"))
        {
            collectionPreviewStarted=boardSceneTime;audio.PlayEliminationTheme(item.Id,true);
            audio.PlayEliminationSound(collection.Equipped("elimsound")?.GetValue<string>(),true,true,.3);
        }
        if(kind is "sound" or "elimsound")
        {
            if(Button("previewSound/"+kind+"/"+item.Id,"Play preview"))
            {string clip=item.Data.TryGetProperty("clip"u8,out var value)?value.GetString()!:item.Id;if(kind=="elimsound")audio.PlayEliminationSound(item.Id,true,true,0);else audio.PlayBoard(clip);}
            if(item.Data.TryGetProperty("dur"u8,out var duration))ImGui.Text($"{duration.GetDouble():0.##} seconds");
        }
    }
    private void SelectedCosmetic(CosmeticCollection collection,string kind)
    {
        var item=CosmeticCollection.Item(kind,previewCosmetic);
        if(item is null || (item.Secret||MatchLockerVisible)&&!collection.Owns(kind,item.Id))
        {
            string? equipped=CosmeticCollection.Multi(kind)?collection.Equipped(kind)?.AsArray().FirstOrDefault(x=>x is not null)?.GetValue<string>():collection.Equipped(kind)?.GetValue<string>();
            item=CosmeticCollection.Item(kind,equipped)??CosmeticCollection.Items(kind).FirstOrDefault(x=>MatchLockerVisible?collection.Owns(kind,x.Id):!x.Secret||collection.Owns(kind,x.Id));
        }
        if(item is null){if(MatchLockerVisible)CollectionText("No owned cosmetics in this category yet.");return;}previewCosmetic=item.Id;
        CosmeticPreview(kind,item,collection,ImGui.GetContentRegionAvail().X);
        ImGui.PushFont(default,23);ImGui.PushStyleColor(ImGuiCol.Text,HomeInk);CollectionText(item.Name);ImGui.PopStyleColor();ImGui.PopFont();
        ImGui.PushStyleColor(ImGuiCol.Text,RarityColor(item.Rarity));ImGui.Text(char.ToUpperInvariant((item.Rarity??"common")[0])+(item.Rarity??"common")[1..]);ImGui.PopStyleColor();
        if(item.Desc is not null)CollectionText(item.Desc);
        if(kind=="dance")CollectionText("Shown on your equipped Little Legend.");
        ImGui.Separator();CollectionActions(collection,kind,item);
    }
    private void EggArt(string id,float height=110,double age=-1,ImColor8? glow=null)
    {
        var at=ImGui.GetCursorScreenPos();float width=Math.Max(120,ImGui.GetContentRegionAvail().X);ImGui.Dummy(new(width,height));
        eggArtwork.Draw(id,at+new float2(width/2,height/2),height*.7f,age,glow,ClientMotion);
        if(age>=0 && ImGui.IsMouseHoveringRect(at,at+new float2(width,height)) && ImGui.IsMouseClicked(ImGuiMouseButton.Left))RevealHatch();
    }
    private void RevealHatch()
    {
        if(hatching is not {} egg || hatchRevealed)return;
        hatchRevealed=true;hatchRevealStarted=boardSceneTime;var item=CosmeticCollection.Item(egg.Award.Kind,egg.Award.Id)!;
        audio.Play(item.Rarity is "legendary" or "epic"?"champion":"digivolve");
    }
    private void HatchPanel(CosmeticCollection collection)
    {
        var hatch=hatching!.Value;var award=hatch.Award;var item=CosmeticCollection.Item(award.Kind,award.Id)!;
        double age=boardSceneTime-hatch.Started;
        if(!hatchRevealed && age>=EggArtwork.CrackAt && !hatchCracked){hatchCracked=true;audio.Play("forge");}
        if(!hatchRevealed && age>=EggArtwork.RevealAt)RevealHatch();
        if(!hatchRevealed)
        {
            EggArt(hatch.Egg,220,age,RarityColor(item.Rarity));
            if(Button("skipEgg","Click to skip"))RevealHatch();
            return;
        }
        var draw=ImGui.GetWindowDrawList();int first=draw.VtxBuffer.Count;var start=ImGui.GetCursorScreenPos();ImGui.BeginGroup();
        try {HatchRewardPanel(collection,hatch.Egg,award,item);}
        finally {ImGui.EndGroup();ApplyHatchPop(first,start,ImGui.GetItemRectMax());}
    }
    private unsafe void ApplyHatchPop(int first,float2 start,float2 end)
    {
        double p=EggArtwork.RevealProgress(boardSceneTime-hatchRevealStarted,ClientMotion);float scale=(float)(.6+.4*p);var center=(start+end)*.5f;
        var draw=ImGui.GetWindowDrawList();
        for(int i=first;i<draw.VtxBuffer.Count;i++)
        {
            ref var vertex=ref draw.VtxBuffer.Data[i];vertex.pos=center+(vertex.pos-center)*scale;
            vertex.col=(vertex.col&0xFFFFFF)|((uint)Math.Round((vertex.col>>24)*p)<<24);
        }
    }
    private void HatchRewardPanel(CosmeticCollection collection,string egg,EggAward award,CosmeticItem item)
    {
        ImGui.Text(item.Name);ImGui.PushStyleColor(ImGuiCol.Text,RarityColor(item.Rarity));ImGui.Text((item.Rarity??"common")+" "+CollectionNames[Array.IndexOf(CosmeticCollection.Kinds,award.Kind)]);ImGui.PopStyleColor();
        CosmeticPreview(award.Kind,item,collection,Math.Min(320,ImGui.GetContentRegionAvail().X));
        CollectionText(award.Evolution is {} evo?$"Duplicate: +{evo.Data} Digi-Data for {item.Name} (level {evo.Level}).":award.Duplicate?$"Duplicate: {award.Refund} Bits refunded.":"New! Added to your collection.");
        if(award.Evolution is not null && Button("eggEvolve","Digivolve")){hatching=null;evolutionLegend=item.Id;return;}
        if(!award.Duplicate && Button("equipEgg",CosmeticCollection.Multi(award.Kind)?"Put on my wheel":"Equip now"))
        {
            if(CosmeticCollection.Multi(award.Kind)){collectionKind=Array.IndexOf(CosmeticCollection.Kinds,award.Kind);collectionSlot=FirstEmptyCollectionSlot(collection,award.Kind);showEggs=false;hatching=null;collectionOwnedOnly=true;previewCosmetic=item.Id;}
            else if(ChangeCollection(c=>c.Equip(award.Kind,award.Id),"Equipped "+item.Name+"."))audio.PlayClient("equip",false);
        }
        var definition=DigiEggs.Definitions.Single(x=>x.Id==egg);ImGui.BeginDisabled(collection.Bits<definition.Price);
        if(Button("anotherEgg","Hatch another")){eggPurchase=(egg,Random.Shared.NextDouble(),Random.Shared.NextDouble(),DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());hatching=null;}
        ImGui.EndDisabled();ImGui.SameLine();if(Button("closeEgg","Keep shopping"))hatching=null;
    }
    private void BoughtPanel(CosmeticCollection collection)
    {
        var purchase=boughtCosmetic!.Value;var item=CosmeticCollection.Item(purchase.Kind,purchase.Id)!;
        ImGui.Text("Bought "+item.Name);CosmeticPreview(purchase.Kind,item,collection,Math.Min(320,ImGui.GetContentRegionAvail().X));
        if(Button("equip/"+purchase.Kind+"/"+item.Id,CosmeticCollection.Multi(purchase.Kind)?"Put on my wheel":"Equip now"))
        {
            if(CosmeticCollection.Multi(purchase.Kind)){collectionKind=Array.IndexOf(CosmeticCollection.Kinds,purchase.Kind);collectionSlot=FirstEmptyCollectionSlot(collection,purchase.Kind);collectionOwnedOnly=true;}
            else if(ChangeCollection(c=>c.Equip(purchase.Kind,item.Id),"Equipped "+item.Name+"."))audio.PlayClient("equip",false);
            boughtCosmetic=null;
        }
        ImGui.SameLine();if(Button("closePurchase","Keep shopping"))boughtCosmetic=null;
    }
    private static int FirstEmptyCollectionSlot(CosmeticCollection collection,string kind)=>Math.Max(0,collection.Equipped(kind)!.AsArray().ToList().FindIndex(x=>x is null));
    private void DisposeCollectionPreview(){collectionPreview?.Dispose();collectionPreview=null;collectionKillPreview?.Dispose();collectionKillPreview=null;collectionKillSignature=null;eggArtwork.Dispose();collectionArtwork.Dispose();}
}
