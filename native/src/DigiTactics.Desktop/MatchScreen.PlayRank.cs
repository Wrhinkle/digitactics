using System.Globalization;
using System.Text.Json;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool homePlayRankLoaded;
    private string? homePlayRankSource;
    internal ProfileSummary? HomePlayRank {get;private set;}
    internal string? HomePlayRankError {get;private set;}
    internal bool HomePlayRankVisible=>showHomePlay&&!homePlayParty;
    internal bool HomePlayRankLayoutFits {get;private set;}
    internal (float2 Min,float2 Max) HomePlayRankBounds {get;private set;}

    private float DrawHomePlayRank(float2 headerAt,float headerWidth)
    {
        // The source Play status and Profile rank use the same lifetime totals.
        // Cache the parsed summary until that saved value changes, including
        // updates from a completed match, an import, or a profile reset.
        string? client=profile?.Storage.GetValueOrDefault("dt.client.v1");
        if(!homePlayRankLoaded||homePlayRankSource!=client)
        {
            homePlayRankLoaded=true;homePlayRankSource=client;HomePlayRankError=null;
            try {HomePlayRank=ProfileSummary.Read(client,null);}
            catch(Exception error) when(error is JsonException or InvalidDataException or InvalidOperationException or FormatException or OverflowException or ArgumentOutOfRangeException)
            {HomePlayRank=null;HomePlayRankError="Profile statistics unavailable";}
        }

        // .play-status: 46px emblem, eyebrow label and the .ps-row totals (theme.js), in CSS pixels.
        var draw=ImGui.GetWindowDrawList();const float height=60;
        string label=HomePlayRank is {} known?known.Rank.Label.ToUpperInvariant():"RANK UNAVAILABLE";
        (long Value,string Label)[] stats=HomePlayRank is {} summary?[(summary.Games,"games"),(summary.TopFour,"top finishes"),(summary.Wins,"wins")]:[];
        float rowWidth=stats.Sum(stat=>ClientCss.Width(ClientCss.Face.DisplayBold,16,stat.Value.ToString(CultureInfo.InvariantCulture))+5+ClientCss.Width(ClientCss.Face.Body,11,stat.Label)+6)+Math.Max(0,stats.Length-1)*5;
        if(HomePlayRank is null)rowWidth=ClientCss.Width(ClientCss.Face.Body,11,HomePlayRankError??"");
        float textWidth=Math.Max(ClientCss.Width(ClientCss.Face.DisplayBold,11,label,1.76f),rowWidth);
        float width=Math.Min(headerWidth-200,1+6+46+10+textWidth+14+1);
        var at=headerAt+new float2(headerWidth-width,0);var extent=new float2(width,height);
        HomePlayRankBounds=(at,at+extent);
        HomePlayRankLayoutFits=at.X>=headerAt.X+200&&at.X+width<=headerAt.X+headerWidth+.5f;
        draw.AddRectFilled(at,at+extent,new ImColor8(1,10,19,179));ClientCss.Border(draw,at,at+extent,ClientCss.Gold5);
        draw.PushClipRect(at+new float2(1,1),at+extent-new float2(1,1),true);
        var text=at+new float2(1+6+46+10,1+(height-2-36.4f)/2);
        if(HomePlayRank is {} rank)
        {
            postgameArtwork.Draw("emblem-"+rank.Rank.Id,at+new float2(7,7),new(46,46),grayscale:rank.Rank.Unranked,brightness:rank.Rank.Unranked?(byte)153:(byte)255);
            ClientCss.Text(draw,ClientCss.Face.DisplayBold,11,text,ClientCss.Gold2,label,1.76f,14.85f);
            float x=text.X;
            foreach(var stat in stats)
            {
                x+=ClientCss.Text(draw,ClientCss.Face.DisplayBold,16,new(x,text.Y+14.8f),ClientCss.Gold1,stat.Value.ToString(CultureInfo.InvariantCulture),0,21.6f)+5;
                x+=ClientCss.Text(draw,ClientCss.Face.Body,11,new(x,text.Y+14.8f+5),ClientCss.Text3,stat.Label,0,14.85f)+6+5;
            }
        }
        else
        {
            ClientCss.Text(draw,ClientCss.Face.DisplayBold,11,text,ClientCss.Gold2,label,1.76f,14.85f);
            ClientCss.Text(draw,ClientCss.Face.Body,11,text+new float2(0,14.8f+5),ClientCss.Text3,HomePlayRankError??"",0,14.85f);
        }
        draw.PopClipRect();return height;
    }
}
