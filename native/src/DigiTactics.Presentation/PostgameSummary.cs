namespace DigiTactics.Presentation;

public static class PostgameSummary
{
    public static string PlaceLabel(int place,bool duos) => place==1 ? "Champion" : place<=(duos?2:4) ? duos?"Top 2 teams":"Top 4" : $"{place}{(place==3?"rd":"th")} place";
}
