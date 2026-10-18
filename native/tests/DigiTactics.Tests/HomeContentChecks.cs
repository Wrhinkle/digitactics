using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Presentation;

internal static class HomeContentChecks
{
    internal static int Run(string root)
    {
        using var fixture=JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"tests/fixtures/home-content.json")));
        var content=HomeContent.Load(Path.Combine(root,"content/home/content.json"));int checks=0;
        void Check(bool value,string message){if(!value)throw new InvalidOperationException(message);checks++;}
        var jsonOptions=new JsonSerializerOptions{PropertyNameCaseInsensitive=true};
        var sourceContent=fixture.RootElement.GetProperty("content").Deserialize<HomeContent>(jsonOptions)!;
        Check(JsonNode.DeepEquals(JsonSerializer.SerializeToNode(content),JsonSerializer.SerializeToNode(sourceContent)),"Native Home copy differs from original client rendering.");
        Check(content.Patch.Length==7&&content.Patch[1].Description.Contains("Chaos plays a random set every game",StringComparison.Ordinal),"Home patch notes lost dynamic set rotation copy.");
        foreach(var row in fixture.RootElement.GetProperty("rotation").EnumerateArray())
        {
            var now=row.GetProperty("now").GetDateTimeOffset();var zone=TimeZoneInfo.FindSystemTimeZoneById(row.GetProperty("zone").GetString()!);
            var remaining=HomeRotation.UntilMidnight(now,zone);
            Check(Math.Abs(remaining.TotalMilliseconds-row.GetProperty("milliseconds").GetDouble())<.01,"Daily reset differs from JavaScript across a local midnight/DST transition.");
            Check(HomeRotation.Label(remaining)==row.GetProperty("text").GetString(),"Rotation hours/minutes differ from original Home Events.");
        }
        foreach(var row in fixture.RootElement.GetProperty("heroCases").EnumerateArray())
        {
            var actual=content.Hero.Slides(content.Hero.Sets.Take(row.GetProperty("count").GetInt32()),row.GetProperty("featured").GetBoolean());
            var expected=row.GetProperty("slides").Deserialize<HomeHeroSlide[]>(jsonOptions)!;
            Check(actual.SequenceEqual(expected),"Carousel order, set copy, Store insertion or Farm/Digivolution fallback differs from heroSlides().");
        }
        foreach(var scenario in fixture.RootElement.GetProperty("timers").EnumerateArray())
        {
            var carousel=new HomeCarousel(content.Hero.Slides().Length);carousel.Render(0);
            foreach(var row in scenario.EnumerateArray())
            {
                double now=row.GetProperty("now").GetDouble();
                if(row.GetProperty("selected").ValueKind!=JsonValueKind.Null)carousel.Render(now,row.GetProperty("selected").GetInt32());
                else carousel.Tick(now,row.GetProperty("home").GetBoolean(),row.GetProperty("client").GetBoolean(),row.GetProperty("module").GetBoolean(),row.GetProperty("result").GetBoolean(),row.GetProperty("overview").GetBoolean(),row.GetProperty("hovered").GetBoolean());
                Check(carousel.Index==row.GetProperty("index").GetInt32(),"Carousel timeout advanced differently from the original client.");
                Check(carousel.Revision==row.GetProperty("revision").GetInt32(),"Carousel retry/render count differs from the original client.");
                double? due=row.GetProperty("due").ValueKind==JsonValueKind.Null?null:row.GetProperty("due").GetDouble();
                Check(carousel.Due==due||carousel.Due is {} actualDue&&due is {} expectedDue&&Math.Abs(actualDue-expectedDue)<1e-9,$"Carousel's 8-second retry deadline differs from the original client: {carousel.Due}/{due} at {now}.");
            }
        }
        var hover=new HomeCarousel(14);hover.Render(0);hover.Tick(4,true,true,false,false,true,false);hover.Tick(8,true,true,false,false,true,true);
        Check(hover.Index==0&&hover.Deferred&&hover.Progress(true)==.5,"Hover must pause the thumbnail progress and defer the carousel.");
        hover.Tick(16,true,true,false,false,true,false);Check(hover.Index==0&&!hover.Deferred,"First retry should refresh the same slide.");
        hover.Tick(24,true,true,false,false,true,false);Check(hover.Index==1,"The next 8-second deadline should advance after a blocked retry.");
        Check(HomeCarousel.Opacity(.002,false)==1&&HomeCarousel.Scale(.002,false)==1&&HomeCarousel.TextProgress(.202,4,false)>.999,"Reduced motion must shorten CSS durations to 1ms while retaining the stagger.");
        Check(HomeCarousel.Opacity(.1,true)<1&&HomeCarousel.Scale(4.5,true)>1&&HomeCarousel.Scale(9,true)==1,"Source hero fade/9-second zoom duration changed.");
        Console.WriteLine($"Home content: {checks} source-copy, local-midnight/DST and hero composition/timeout/motion checks passed.");return checks;
    }
}
