using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigiTactics.Storage;

public sealed record ProfileFavorite(string Id,long Games);
public sealed record ProfileRank(string Id,string Label,string Detail,bool Unranked);
public sealed record ProfileBits(long N,string Reason,long At);
public sealed record ProfileSummary(long Games,long Wins,long TopFour,double Average,ProfileRank Rank,
    ProfileFavorite[] Traits,ProfileFavorite[] Units,HistoryMatch[] Matches,int[] Placements,HistoryMatch[] Trend,
    ProfileBits[] BitsHistory,string Banner,string? Avatar)
{
    public static readonly string[] Banners=["profile-1","profile-2","profile-3","profile-4"];
    public static ProfileSummary Read(string? client,string? wallet)
    {
        using var document=JsonDocument.Parse(client??"{}");var root=document.RootElement;
        JsonElement totals=root.TryGetProperty("totals",out var t)?t:default;
        long Value(string name)
        {
            long value=totals.ValueKind==JsonValueKind.Undefined || !totals.TryGetProperty(name,out var v)?0:v.GetInt64();
            return value>=0?value:throw new InvalidDataException("Negative profile statistic.");
        }
        ProfileFavorite[] Favorite(string name,int count)
        {
            if(totals.ValueKind==JsonValueKind.Undefined || !totals.TryGetProperty(name,out var entries))return [];
            return entries.EnumerateObject().Select(p=>new ProfileFavorite(p.Name,p.Value.GetInt64())).OrderByDescending(p=>p.Games).Take(count).ToArray();
        }
        long games=Value("games");double average=games>0?(double)Value("placeSum")/games:0;
        string rank=average<=2.8?"mega":average<=3.8?"ultimate":average<=4.8?"champion":"rookie";
        var rating=games<3?new ProfileRank("rookie","Unranked",$"Play {3-games} more game{(3-games==1?"":"s")} to get a rank",true)
            :new ProfileRank(rank,char.ToUpperInvariant(rank[0])+rank[1..],$"Average place {average.ToString("F2",System.Globalization.CultureInfo.InvariantCulture)} over {games} games",false);
        string banner="profile-1";string? avatar=null;
        if(root.TryGetProperty("profile",out var profile))
        {
            if(profile.TryGetProperty("banner",out var b) && b.GetString() is { } selected && Banners.Contains(selected))banner=selected;
            if(profile.TryGetProperty("avatar",out var a))avatar=a.GetString();
        }
        var matches=TamerProgress.History(client);var placements=new int[8];
        foreach(var match in matches)if(match.Place is >=1 and <=8)placements[match.Place-1]++;
        using var walletDocument=JsonDocument.Parse(wallet??"{}");
        var bits=walletDocument.RootElement.TryGetProperty("history",out var history)?history.EnumerateArray().Take(14).Select(h=>new ProfileBits(h.GetProperty("n").GetInt64(),
            h.TryGetProperty("reason",out var reason) && reason.GetString() is {Length:>0} text?text:"Bits",h.GetProperty("at").GetInt64())).ToArray():[];
        return new(games,Value("wins"),Value("top4"),average,rating,Favorite("traits",3),Favorite("units",5),matches,placements,matches.Take(20).Reverse().ToArray(),bits,banner,avatar);
    }
    public static string CleanName(string value)
    {
        string clean=new(value.Where(c=>char.IsAsciiLetterOrDigit(c)||c is '_' or ' ' or '.' or '\'' or '-').ToArray());
        clean=clean.Trim();return clean[..Math.Min(16,clean.Length)];
    }
    public static string Customize(string? saved,string field,string value,CosmeticCollection collection)
    {
        if(field=="banner"?!Banners.Contains(value):field!="avatar" || !collection.Owns("legend",value))throw new InvalidDataException("Unavailable profile customization.");
        var root=JsonNode.Parse(saved??"{}") as JsonObject??throw new InvalidDataException("Invalid client profile.");
        if(root["profile"] is null)root["profile"]=new JsonObject();
        root["profile"]![field]=value;return root.ToJsonString();
    }
}
