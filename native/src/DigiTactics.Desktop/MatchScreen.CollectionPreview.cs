using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private CosmeticScenePreview? collectionPreview;
    private readonly List<IDisposable> retiredCollectionPreviews=[];
    private void RetireCollectionPreview(IDisposable? preview){if(preview is not null)retiredCollectionPreviews.Add(preview);}
    private void ReleaseRetiredCollectionPreviews(){foreach(var preview in retiredCollectionPreviews)preview.Dispose();retiredCollectionPreviews.Clear();BeginStoreDancePreviewFrame();}
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
    private void CosmeticPreview(string kind,CosmeticItem item,CosmeticCollection collection,float width,float? stageHeight=null,bool compactControls=false,bool bare=false)
    {
        string signature=kind+":"+item.Id;
        if(collectionPreviewSignature!=signature){collectionPreviewSignature=signature;collectionPreviewStarted=boardSceneTime;}
        if(kind!="killscreen" && collectionKillPreview is not null){RetireCollectionPreview(collectionKillPreview);collectionKillPreview=null;collectionKillSignature=null;}
        if(kind is not("legend" or "dance" or "board") && collectionPreview is not null){RetireCollectionPreview(collectionPreview);collectionPreview=null;}
        width=Math.Max(120,width);float naturalHeight=width*CosmeticScenePreview.Height/CosmeticScenePreview.Width,height=stageHeight??naturalHeight;
        var at=ImGui.GetCursorScreenPos();ImGui.Dummy(new(width,height));var draw=ImGui.GetWindowDrawList();
        if(!bare)draw.AddRectFilledMultiColor(at,at+new float2(width,height),new ImColor8(20,37,54),new ImColor8(20,37,54),new ImColor8(1,10,19),new ImColor8(1,10,19));
        CollectionPreviewDrawCalls=0;CollectionPreviewKind=kind;
        if(kind is "legend" or "dance" or "board")
        {
            bool fill=kind=="legend"||bare&&kind=="dance";
            // dances.js frames the dancer at about 44% of the Collection stage height; the companion camera
            // fills ~79%, so render a taller target and show its centre band.
            float zoom=bare&&kind=="dance"?.56f:1,renderTall=fill?Math.Min(1600,height/zoom):naturalHeight;zoom=fill?height/renderTall:1;
            int renderWidth=Math.Clamp((int)Math.Ceiling(width),120,1600),renderHeight=Math.Clamp((int)Math.Ceiling(renderTall),120,1600);
            if(collectionPreview is not null && (collectionPreview.RenderWidth!=renderWidth || collectionPreview.RenderHeight!=renderHeight)){RetireCollectionPreview(collectionPreview);collectionPreview=null;}
            collectionPreview??=new(legendPedestal,renderWidth,renderHeight);
            if(kind=="legend")
            {
                Buttons["collectionLegend"]=at+new float2(width/2,height/2);
                Buttons["collectionLegend/outside"]=at+new float2(-12,height/2);
                if(collectionPreview.PointerItem("collectionLegend",at,new(width,height)) is not null)audio.PlayClient("emote",true);
            }
            collectionPreview.Draw(kind,item.Id,CurrentPreviewLegend(collection,kind=="legend"?item.Id:null),boardSceneTime,ClientMotion,fill?width/renderTall:null);
            float pictureHeight=fill?height:Math.Min(height,naturalHeight);
            var pictureAt=at+new float2(0,Math.Max(0,(height-pictureHeight)*.5f));
            float band=(1-zoom)*.5f;
            draw.AddImage(new ImTextureRef{_TexID=new((nint)collectionPreview.Texture)},pictureAt,pictureAt+new float2(width,pictureHeight),new(0,1-band),new(1,band));
            CollectionPreviewDrawCalls=collectionPreview.DrawCalls;
        }
        else if(bare)
        {
            if(kind=="emote"){float side=Math.Min(width-24,120);emoteArtwork.Draw(item.Id,(long)(boardSceneTime*1000),at+new float2((width-side)/2,(height-side)/2),side);CollectionPreviewDrawCalls=1;}
            else if(kind=="killscreen")
            {
                collectionTheme.Draw(item.Id,false,(boardSceneTime-collectionPreviewStarted)%3.5,at,new(width,height));
                CollectionKillTexture(item,collection,at,new(width,height));
            }
            return;
        }
        else if(kind=="emote") {float s=Math.Min(width,height)*.8f;emoteArtwork.Draw(item.Id,(long)(boardSceneTime*1000),at+new float2((width-s)/2,(height-s)/2),s);CollectionPreviewDrawCalls=1;}
        else if(kind=="killscreen")
        {
            collectionTheme.Draw(item.Id,false,(boardSceneTime-collectionPreviewStarted)%3.5,at,new(width,height));
            CollectionKillTexture(item,collection,at,new(width,height));
        }
        else
        {
            // Original sound previews are explicit play buttons: browsing never starts audio.
            var center=at+new float2(width/2,height/2);draw.AddCircleFilled(center,Math.Min(width,height)*.2f,new ImColor8(34,54,78));
            draw.AddTriangleFilled(center+new float2(-7,-13),center+new float2(-7,13),center+new float2(13,0),RarityColor(item.Rarity));CollectionPreviewDrawCalls=1;
        }
        if(stageHeight is null)draw.AddRect(at,at+new float2(width,height),RarityColor(item.Rarity),thickness:1);
        if(kind=="killscreen" && Button("previewKill/"+item.Id,compactControls?"Play preview":"Play with elimination sound",compactControls?-1:0))
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
    private void CollectionKillTexture(CosmeticItem item,CosmeticCollection collection,float2 at,float2 extent)
    {
        float width=extent.X,height=extent.Y;var draw=ImGui.GetWindowDrawList();
        string legend=CurrentPreviewLegend(collection),dance=collection.Equipped("dance")?.GetValue<string>()??"floss";
        var team=Session?.State.Players[localSeat>=0?localSeat:0].Roster.Entries.Where(x=>x.Spot.IsBoard).Select(x=>new DigiTactics.Presentation.EliminationMember(x.Unit.Line,x.Unit.Stage)).ToArray();
        string teamKey=team is null?"":string.Join(",",team.Select(x=>x.Line+":"+x.Stage));
        string key=Rules.Id+":"+item.Id+":"+legend+":"+dance+":"+teamKey;
        if(collectionKillSignature!=key)
        {
            RetireCollectionPreview(collectionKillPreview);collectionKillPreview=null;collectionKillSignature=null;
            collectionKillPreview=new(Rules,legend,dance,item.Id,team);collectionKillSignature=key;
        }
        collectionKillPreview!.Draw((int)width,(int)height,boardSceneTime);
        draw.AddImage(new ImTextureRef{_TexID=new((nint)collectionKillPreview.Texture)},at,at+new float2(width,height),new(0,1),new(1,0));
        CollectionPreviewDrawCalls=collectionTheme.Operations+collectionKillPreview.DrawCalls;
    }
    private static string CollectionKindName(string kind)=>kind switch
    {
        "legend"=>"Little Legend","emote"=>"Emote","board"=>"Board","killscreen"=>"Kill Screen","dance"=>"Dance","sound"=>"Soundboard","elimsound"=>"Elim Sound",_=>kind
    };
    private static void CollectionBadge(string text,ImColor8 color,bool filled=false)
    {
        ImGui.PushFont(NativeUiTheme.DisplayBoldFont,13);var at=ImGui.GetCursorScreenPos();float width=ImGui.CalcTextSize(text).X+16;
        ImGui.Dummy(new(width,22));var draw=ImGui.GetWindowDrawList();draw.AddRectFilled(at,at+new float2(width,22),filled?color:new ImColor8(12,24,38));draw.AddRect(at,at+new float2(width,22),color);
        draw.AddText(at+new float2(8,3),filled?new ImColor8(1,10,19):color,text);ImGui.PopFont();
    }
    private static void CollectionModalTitle(string title)=>CollectionModalText(title.ToUpperInvariant(),HomeInk,26);
    private static void CollectionModalText(string text,ImColor8 color,float fontSize=16)
    {
        const float left=24;float width=ImGui.GetWindowSize().X-left*2;
        ImGui.PushFont(fontSize>=20?NativeUiTheme.DisplayBoldFont:NativeUiTheme.BodyFont,fontSize);ImGui.PushStyleColor(ImGuiCol.Text,color);
        var lines=new List<string>();string current="";
        foreach(string word in text.Split(' ',StringSplitOptions.RemoveEmptyEntries))
        {
            string next=current.Length==0?word:current+" "+word;
            if(current.Length>0&&ImGui.CalcTextSize(next).X>width){lines.Add(current);current=word;}else current=next;
        }
        if(current.Length>0)lines.Add(current);
        foreach(string line in lines){ImGui.SetCursorPosX(left+Math.Max(0,(width-ImGui.CalcTextSize(line).X)*.5f));ImGui.Text(line);}
        ImGui.SetCursorPosX(left);ImGui.PopStyleColor();ImGui.PopFont();
    }
    private static void CollectionModalButtons(float width)
    {
        const float left=24;ImGui.SetCursorPosX(Math.Max(left,(ImGui.GetWindowSize().X-width)*.5f));
    }
    private void CollectionModalPreview(string kind,CosmeticItem item,CosmeticCollection collection,float desiredWidth)
    {
        const float left=24;float width=Math.Min(desiredWidth,ImGui.GetWindowSize().X-left*2);
        ImGui.SetCursorPosX((ImGui.GetWindowSize().X-width)*.5f);ImGui.BeginGroup();
        try
        {
            CosmeticPreview(kind,item,collection,width);
            // The draggable legend restores its cursor after the image item.
            // Submit that restored position before EndGroup validates bounds.
            ImGui.Dummy(new(0,0));
        }
        finally{ImGui.EndGroup();ImGui.SetCursorPosX(left);}
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
            CollectionModalButtons(160);if(Button("skipEgg","Click to skip",160,36))RevealHatch();
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
        CollectionModalPreview(award.Kind,item,collection,320);CollectionModalTitle(item.Name);
        CollectionModalText((item.Rarity??"common").ToUpperInvariant()+" / "+CollectionKindName(award.Kind),RarityColor(item.Rarity));
        CollectionModalText(award.Evolution is {} evo?$"Duplicate: +{evo.Data} Digi-Data for {item.Name} (level {evo.Level}).":award.Duplicate?$"Duplicate: {award.Refund} Bits refunded.":"New! Added to your collection.",HomeInk);
        bool primary=award.Evolution is not null||!award.Duplicate;CollectionModalButtons(primary?446:274);
        if(award.Evolution is not null && CollectionActionButton("eggEvolve","Digivolve",164,38,true)){hatching=null;evolutionLegend=item.Id;return;}
        if(!award.Duplicate && CollectionActionButton("equipEgg",CosmeticCollection.Multi(award.Kind)?"Put on my wheel":"Equip now",164,38,true))
        {
            if(CosmeticCollection.Multi(award.Kind)){collectionKind=Array.IndexOf(CosmeticCollection.Kinds,award.Kind);collectionSlot=FirstEmptyCollectionSlot(collection,award.Kind);showEggs=false;hatching=null;collectionOwnedOnly=true;previewCosmetic=item.Id;}
            else if(ChangeCollection(c=>c.Equip(award.Kind,award.Id),"Equipped "+item.Name+"."))audio.PlayClient("equip",false);
        }
        if(primary)ImGui.SameLine();
        var definition=DigiEggs.Definitions.Single(x=>x.Id==egg);ImGui.BeginDisabled(collection.Bits<definition.Price);
        if(CollectionActionButton("anotherEgg","Hatch another",156,38)){eggPurchase=(egg,Random.Shared.NextDouble(),Random.Shared.NextDouble(),DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());hatching=null;}
        ImGui.EndDisabled();ImGui.SameLine();if(CollectionActionButton("closeEgg","Close",110,38))hatching=null;
    }
    private void BoughtPanel(CosmeticCollection collection)
    {
        var purchase=boughtCosmetic!.Value;var item=CosmeticCollection.Item(purchase.Kind,purchase.Id)!;
        CollectionModalTitle("Bought "+item.Name);CollectionModalPreview(purchase.Kind,item,collection,320);CollectionModalButtons(342);
        if(CollectionActionButton("equip/"+purchase.Kind+"/"+item.Id,CosmeticCollection.Multi(purchase.Kind)?"Put on my wheel":"Equip now",164,38,true))
        {
            if(CosmeticCollection.Multi(purchase.Kind)){collectionKind=Array.IndexOf(CosmeticCollection.Kinds,purchase.Kind);collectionSlot=FirstEmptyCollectionSlot(collection,purchase.Kind);collectionOwnedOnly=true;}
            else if(ChangeCollection(c=>c.Equip(purchase.Kind,item.Id),"Equipped "+item.Name+"."))audio.PlayClient("equip",false);
            boughtCosmetic=null;
        }
        ImGui.SameLine();if(CollectionActionButton("closePurchase","Keep shopping",170,38))boughtCosmetic=null;
    }
    private static int FirstEmptyCollectionSlot(CosmeticCollection collection,string kind)=>Math.Max(0,collection.Equipped(kind)!.AsArray().ToList().FindIndex(x=>x is null));
    private void DisposeCollectionPreview(){RetireCollectionPreview(collectionPreview);collectionPreview=null;RetireCollectionPreview(collectionKillPreview);collectionKillPreview=null;collectionKillSignature=null;ClearStoreDancePreviews();}
}
