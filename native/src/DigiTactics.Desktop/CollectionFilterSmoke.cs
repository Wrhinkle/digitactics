using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

/// <summary>Exercises real collection controls; source fixtures independently validate the resulting query model.</summary>
internal sealed class CollectionFilterSmoke
{
    private int step,cooldown=2;
    private bool down;
    private ImGuiKey key;
    private string? originalSave,selection;
    public bool Done {get;private set;}
    public void Input(MatchScreen screen,NativeProfileStore profile)
    {
        if(Done||cooldown-->0)return;
        var io=ImGui.GetIO();
        if(down){if(key==ImGuiKey.None)io.AddMouseButtonEvent(0,false);else io.AddKeyEvent(key,false);down=false;key=ImGuiKey.None;cooldown=3;return;}
        originalSave??=profile.Storage.GetValueOrDefault("dt.cosmetics.v1");selection??=screen.CollectionSelectedId;
        bool advance=true;
        switch(step)
        {
            case 0:advance=Click("collectionOwnership/owned");break;
            case 1:advance=Click("collectionOwnership/locked");break;
            case 2:advance=Click("collectionOwnership/all");break;
            case 3:advance=Click("collectionRarity");break;
            case 4:advance=Click("collectionRarity/epic");break;
            case 5:advance=Click("collectionSort");break;
            case 6:advance=Click("collectionSort/rarity-asc");break;
            case 7:advance=Click("collectionSort");break;
            case 8:advance=Click("collectionSort/name");break;
            case 9:advance=Click("collectionSort");break;
            case 10:advance=Click("collectionSort/owned");break;
            case 11:advance=Click("collectionRarity");break;
            case 12:advance=Click("collectionRarity/all");break;
            case 13:advance=Click("collectionSearch");break;
            case 14:io.AddInputCharactersUTF8("  IN-TRAINING  "u8);cooldown=4;break;
            case 15:advance=Click("clearCollectionSearch");break;
            case 16:advance=Click("collectionSearch");break;
            case 17:io.AddInputCharactersUTF8("no-such-cosmetic-xyz"u8);cooldown=4;break;
            case 18:key=ImGuiKey.Escape;io.AddKeyEvent(key,true);down=true;break;
            case 19:advance=Click("collectionCategory");break;
            case 20:advance=Click("collectionCategory/1");break;
            case 21:advance=Click("collectionRetro");break;
            case 22:advance=Click("collectionCategory");break;
            case 23:advance=Click("collectionCategory/0");break;
            case 24:advance=Click("collectionSort");break;
            case 25:advance=Click("collectionSort/rarity");break;
            default:
                if(profile.Storage.GetValueOrDefault("dt.cosmetics.v1")!=originalSave)throw new InvalidOperationException("Collection browsing changed the saved wallet, ownership or loadout.");
                Done=true;Console.WriteLine("Collection filters UI passed: all/owned/not-owned, rarity, four sorts, trimmed case-insensitive search, empty state, Escape clear, retro tags and category reset without changing selection or save.");return;
        }
        if(advance)step++;
        bool Click(string name)
        {
            if(!screen.Buttons.TryGetValue(name,out var point))return false;
            if(point.X<0||point.Y<0||point.X>io.DisplaySize.X||point.Y>io.DisplaySize.Y)throw new InvalidOperationException("Collection filter control clipped: "+name);
            io.AddMousePosEvent(point.X,point.Y);io.AddMouseButtonEvent(0,true);down=true;return true;
        }
    }
    public void Verify(MatchScreen screen,NativeProfileStore profile,int2 size,string? capture)
    {
        if(Done||cooldown!=1)return;
        string kind=step is >=21 and <=23?"emote":"legend";
        var collection=new CosmeticCollection(profile.Storage.GetValueOrDefault("dt.cosmetics.v1"));
        if(!screen.CollectionVisible)throw new InvalidOperationException("Collection search Escape closed the panel.");
        var expected=CosmeticBrowser.Items(collection,kind,screen.CollectionFilterQuery,screen.CollectionFilterOwnership,screen.CollectionFilterRarity,screen.CollectionFilterSort).Select(item=>item.Id);
        if(!expected.SequenceEqual(screen.CollectionFilterIds))throw new InvalidOperationException("Collection controls did not bind the filtered item list.");
        string? ownership=step switch{1=>"owned",2=>"locked",3=>"all",_=>null};
        if(ownership is not null&&screen.CollectionFilterOwnership!=ownership)throw new InvalidOperationException("Collection ownership control failed.");
        if(step==5&&screen.CollectionFilterRarity!="epic")throw new InvalidOperationException("Collection rarity control failed.");
        string? sort=step switch{7=>"rarity-asc",9=>"name",11=>"owned",26=>"rarity",_=>null};
        if(sort is not null&&screen.CollectionFilterSort!=sort)throw new InvalidOperationException("Collection sort control failed.");
        if(step==15&&(screen.CollectionFilterQuery!="  IN-TRAINING  "||screen.CollectionFilterIds.Length==0))throw new InvalidOperationException("Collection search did not accept mixed-case description text.");
        if(step==18&&(screen.CollectionFilterIds.Length!=0||screen.CollectionSelectedId!=selection))throw new InvalidOperationException("Collection empty search changed its selected preview.");
        if(step==19&&screen.CollectionFilterQuery.Length!=0)throw new InvalidOperationException("Collection Escape did not clear the focused query.");
        if(step==22&&(screen.CollectionFilterQuery!="Digimon World 1"||screen.CollectionFilterIds.Length==0||screen.CollectionSelectedId!="retro_hug"))throw new InvalidOperationException("Collection retro-emote quick filter failed.");
        if(step==24&&(screen.CollectionFilterQuery.Length!=0||screen.CollectionSelectedId!=selection))throw new InvalidOperationException("Category switch did not reset search and restore its selected cosmetic.");
        if(capture is not null&&step is 5 or 15 or 18 or 22)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"collection-filter-"+step+".bmp"),size.X,size.Y);
    }
}
