using System.Text.Json;

namespace DigiTactics.Presentation;

public sealed record HomeHighlight(string Title,string Description);
public sealed record HomeEvents(string Title,string Countdown,string Missions,string Store,string StoreDescription,string Farm,string FarmDescription);
public sealed record HomeContent(HomeHighlight[] Patch,HomeEvents Events,HomeHeroCatalog Hero)
{
    public static HomeContent Load(string path)=>JsonSerializer.Deserialize<HomeContent>(File.ReadAllText(path),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})??throw new InvalidDataException("Home content is missing.");
}

public static class HomeRotation
{
    public static TimeSpan UntilMidnight(DateTimeOffset now,TimeZoneInfo zone)
    {
        var local=TimeZoneInfo.ConvertTime(now,zone);
        var midnight=DateTime.SpecifyKind(local.Date.AddDays(1),DateTimeKind.Unspecified);
        // A timezone can advance its clock at midnight; match Date.setHours by
        // advancing through a skipped wall time instead of assuming 24 hours.
        while(zone.IsInvalidTime(midnight))midnight=midnight.AddMinutes(1);
        return TimeZoneInfo.ConvertTimeToUtc(midnight,zone)-now.UtcDateTime;
    }
    public static string Label(TimeSpan remaining)=>$"{Math.Max(0,(int)remaining.TotalHours)}h {Math.Max(0,remaining.Minutes)}m";
}
