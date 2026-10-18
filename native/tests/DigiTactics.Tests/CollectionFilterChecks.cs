using System.Globalization;
using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Presentation;
using DigiTactics.Storage;

internal static class CollectionFilterChecks
{
    public static int Run(string root)
    {
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/collection-filters.json.gz"));
        using var gzip=new GZipStream(file,CompressionMode.Decompress);
        using var fixture=JsonDocument.Parse(gzip);
        var data=fixture.RootElement;int checks=0,queries=0;
        void Check(bool okay,string message){checks++;if(!okay)throw new InvalidOperationException("Collection filters: "+message);}
        static string[] Ids(JsonElement items)=>items.EnumerateArray().Select(item=>item.GetString()!).ToArray();
        Check(data.GetProperty("schema").GetInt32()==1,"fixture version");
        Check(CosmeticBrowser.Rarities.SequenceEqual(Ids(data.GetProperty("rarities"))),"original rarity order");
        var kinds=Ids(data.GetProperty("kinds"));
        Check(kinds.Order(StringComparer.Ordinal).SequenceEqual(CosmeticCollection.Kinds.Order(StringComparer.Ordinal)),"every cosmetic kind");
        var registryBefore=JsonSerializer.Serialize(kinds.Select(CosmeticCollection.Items));
        foreach(string kind in kinds)
        {
            var items=CosmeticCollection.Items(kind);var expected=data.GetProperty("metadata").GetProperty(kind);
            Check(items.Count==expected.GetArrayLength(),kind+" registry size");
            for(int i=0;i<items.Count;i++)
            {
                var item=items[i];var source=expected[i];string label=kind+"/"+item.Id;
                Check(item.Id==source.GetProperty("id").GetString()&&item.Name==source.GetProperty("name").GetString(),label+" original identity/order");
                Check(item.Desc==source.GetProperty("description").GetString(),label+" searchable description");
                Check(item.Rarity==source.GetProperty("rarity").GetString(),label+" rarity metadata");
                Check(item.Tag==source.GetProperty("tag").GetString(),label+" searchable original tag");
                Check(item.Secret==source.GetProperty("secret").GetBoolean()&&item.Free==source.GetProperty("free").GetBoolean(),label+" visibility flags");
                var linked=source.GetProperty("linked");Check(linked.ValueKind==JsonValueKind.Null?item.Linked is null:item.Linked is not null&&item.Linked.SequenceEqual(Ids(linked)),label+" linked ownership metadata");
            }
        }
        var exemplar=CosmeticCollection.Items("legend")[0];
        foreach(string? unknown in new string?[]{null,"","future-rarity","RARE"})Check(CosmeticBrowser.EffectiveRarity(exemplar with {Rarity=unknown})=="common","unknown rarity falls back to common");
        foreach(string rarity in CosmeticBrowser.Rarities)Check(CosmeticBrowser.EffectiveRarity(exemplar with {Rarity=rarity})==rarity,"known rarity "+rarity);
        var previousCulture=CultureInfo.CurrentCulture;
        try
        {
            // Name ordering and search must stay source-compatible on a localized PC.
            foreach(string culture in new[]{"en-US","tr-TR"})
            {
                CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture);
                foreach(var snapshot in data.GetProperty("snapshots").EnumerateArray())
                {
                    string label=culture+"/"+snapshot.GetProperty("name").GetString();
                    var collection=new CosmeticCollection(snapshot.GetProperty("save").GetRawText());string before=collection.Json;
                    foreach(string kind in kinds)
                    {
                        Check(CosmeticCollection.Items(kind).Where(item=>collection.Owns(kind,item.Id)).Select(item=>item.Id).SequenceEqual(Ids(snapshot.GetProperty("owned").GetProperty(kind))),label+" ownership "+kind);
                        Check(CosmeticBrowser.Items(collection,kind).Select(item=>item.Id).Order(StringComparer.Ordinal).SequenceEqual(Ids(snapshot.GetProperty("visible").GetProperty(kind)).Order(StringComparer.Ordinal)),label+" secret visibility "+kind);
                    }
                    foreach(var query in snapshot.GetProperty("cases").EnumerateArray())
                    {
                        string kind=query.GetProperty("kind").GetString()!,search=query.GetProperty("query").GetString()!,ownership=query.GetProperty("ownership").GetString()!,rarity=query.GetProperty("rarity").GetString()!,sort=query.GetProperty("sort").GetString()!;
                        var actual=CosmeticBrowser.Items(collection,kind,search,ownership,rarity,sort);var expected=Ids(query.GetProperty("expected"));
                        Check(actual.Select(item=>item.Id).SequenceEqual(expected),$"{label}/{kind} query '{search}' {ownership}/{rarity}/{sort}; got [{string.Join(',',actual.Select(item=>item.Id))}], expected [{string.Join(',',expected)}]");
                        Check(collection.Json==before,label+" query leaves save unchanged");queries++;
                    }
                }
            }
        }
        finally {CultureInfo.CurrentCulture=previousCulture;}
        Check(JsonSerializer.Serialize(kinds.Select(CosmeticCollection.Items))==registryBefore,"sorting and filtering leave shared registry untouched");
        Console.WriteLine($"Collection filters: {checks:N0} checks passed for {queries:N0} original ordered queries across seven kinds, four ownership snapshots and two locales.");
        return checks;
    }
}
