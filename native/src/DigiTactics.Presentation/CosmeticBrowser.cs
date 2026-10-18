using System.Globalization;
using DigiTactics.Storage;

namespace DigiTactics.Presentation;

/// <summary>The original collection's visibility, search and stable sorting, without changing ownership or selection.</summary>
public static class CosmeticBrowser
{
    public static readonly string[] Rarities=["common","uncommon","rare","epic","legendary"];
    private static readonly StringComparer Names=StringComparer.Create(CultureInfo.GetCultureInfo("en-US"),false);
    public static string EffectiveRarity(CosmeticItem item)=>Rarities.Contains(item.Rarity)?item.Rarity!:"common";
    public static CosmeticItem[] Items(CosmeticCollection collection,string kind,string query="",string ownership="all",string rarity="all",string sort="rarity")
    {
        string search=query.Trim().ToLowerInvariant();
        var visible=CosmeticCollection.Items(kind).Select(item=>(Item:item,Owned:collection.Owns(kind,item.Id),Rarity:EffectiveRarity(item)))
            .Where(row=>(!row.Item.Secret||row.Owned)&&(ownership!="owned"||row.Owned)&&(ownership!="locked"||!row.Owned)
                &&(rarity=="all"||rarity==row.Rarity)
                &&(search.Length==0||$"{row.Item.Name} {row.Item.Desc??""} {row.Rarity} {row.Item.Tag??""}".ToLowerInvariant().Contains(search,StringComparison.Ordinal)));
        return (sort switch
        {
            "name"=>visible.OrderBy(row=>row.Item.Name,Names),
            "owned"=>visible.OrderByDescending(row=>row.Owned).ThenByDescending(row=>Array.IndexOf(Rarities,row.Rarity)).ThenBy(row=>row.Item.Name,Names),
            "rarity-asc"=>visible.OrderBy(row=>Array.IndexOf(Rarities,row.Rarity)).ThenBy(row=>row.Item.Name,Names),
            _=>visible.OrderByDescending(row=>Array.IndexOf(Rarities,row.Rarity)).ThenBy(row=>row.Item.Name,Names)
        }).Select(row=>row.Item).ToArray();
    }
}
