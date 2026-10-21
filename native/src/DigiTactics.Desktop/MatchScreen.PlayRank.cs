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

        const float emblem=46,textLeft=62,rightPadding=14,numberSize=18,labelSize=14,rowHeight=20;
        (long Value,string Label)[] stats=HomePlayRank is {} summary
            ?[(summary.Games,"games"),(summary.TopFour,"top finishes"),(summary.Wins,"wins")]:[];
        string[] values=stats.Select(stat=>stat.Value.ToString(CultureInfo.InvariantCulture)).ToArray();
        var numberWidths=new float[stats.Length];var groupWidths=new float[stats.Length];
        for(int i=0;i<stats.Length;i++)
        {
            ImGui.PushFont(NativeUiTheme.DisplayBoldFont,numberSize);numberWidths[i]=ImGui.CalcTextSize(values[i]).X;ImGui.PopFont();
            ImGui.PushFont(NativeUiTheme.BodyFont,labelSize);groupWidths[i]=numberWidths[i]+5+ImGui.CalcTextSize(stats[i].Label).X;ImGui.PopFont();
        }
        float naturalWidth=textLeft+groupWidths.Sum()+Math.Max(0,stats.Length-1)*11+rightPadding;
        float width=Math.Min(Math.Max(270,headerWidth-220),Math.Max(270,naturalWidth));
        float textWidth=width-textLeft-rightPadding,x=0;int row=0;
        var positions=new float2[stats.Length];
        for(int i=0;i<stats.Length;i++)
        {
            if(x>0&&x+groupWidths[i]>textWidth){x=0;row++;}
            positions[i]=new(x,row*rowHeight);x+=groupWidths[i]+11;
        }
        float height=Math.Max(58,35+(row+1)*rowHeight);
        var at=headerAt+new float2(headerWidth-width,0);var extent=new float2(width,height);
        HomePlayRankBounds=(at,at+extent);
        HomePlayRankLayoutFits=at.X>=headerAt.X+200&&at.X+width<=headerAt.X+headerWidth+.5f&&groupWidths.All(value=>value<=textWidth);
        var draw=ImGui.GetWindowDrawList();
        draw.AddRectFilled(at,at+extent,new ImColor8(1,10,19,180));draw.AddRect(at,at+extent,HomeLine);
        draw.PushClipRect(at+new float2(1,1),at+extent-new float2(1,1),true);
        if(HomePlayRank is {} rank)
        {
            postgameArtwork.Draw("emblem-"+rank.Rank.Id,at+new float2(6,(height-emblem)/2),new(emblem,emblem),grayscale:rank.Rank.Unranked,brightness:rank.Rank.Unranked?(byte)153:(byte)255);
            draw.AddText(NativeUiTheme.DisplayBoldFont,15,at+new float2(textLeft,8),HomeGold,rank.Rank.Label.ToUpperInvariant());
            for(int i=0;i<stats.Length;i++)
            {
                var valueAt=at+new float2(textLeft,27)+positions[i];
                draw.AddText(NativeUiTheme.DisplayBoldFont,numberSize,valueAt,HomeInk,values[i]);
                draw.AddText(NativeUiTheme.BodyFont,labelSize,valueAt+new float2(numberWidths[i]+5,3),HomeMute,stats[i].Label);
            }
        }
        else
        {
            draw.AddText(NativeUiTheme.DisplayBoldFont,15,at+new float2(14,10),HomeGold,"RANK UNAVAILABLE"u8);
            draw.AddText(NativeUiTheme.BodyFont,labelSize,at+new float2(14,31),HomeMute,HomePlayRankError!);
        }
        draw.PopClipRect();return height;
    }
}
