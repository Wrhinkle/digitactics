using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private sealed record RuleRun(string Text,ImColor8 Color,bool Bold=false);
    private sealed record RulePiece(string Text,float2 At,ImColor8 Color,bool Bold);
    private sealed record RuleTextLayout(RulePiece[] Pieces,float Width,float Height);
    private static readonly Dictionary<string,XElement> ruleMarkup=[];
    internal int CodexRuleRoundChips {get;private set;}
    internal bool CodexRuleTableTextFits {get;private set;}
    internal string? CodexRuleTableLayoutError {get;private set;}
    internal Dictionary<string,(float2 Min,float2 Max)> CodexRuleTableBounds {get;}=[];
    internal HashSet<string> CodexRuleVisibleSections {get;}=[];
    internal HashSet<string> CodexRuleVisibleRoundChips {get;}=[];
    internal HashSet<string> CodexRuleVisibleTableCells {get;}=[];

    private void CodexRules(bool compact)
    {
        CodexRuleRoundChips=0;CodexRuleTableTextFits=true;CodexRuleTableLayoutError=null;CodexRuleTableBounds.Clear();
        CodexRuleVisibleSections.Clear();CodexRuleVisibleRoundChips.Clear();CodexRuleVisibleTableCells.Clear();
        var entries=CodexData.Rules.Where(rule=>compact||CodexMatch(rule.SearchText)).ToArray();
        if(compact)
        {
            foreach(var rule in entries)
            {
                CodexRows++;ImGui.SetNextItemOpen(rule==entries[0],ImGuiCond.Once);
                if(ImGui.CollapsingHeader(rule.Title+"##codexRule/"+rule.Id))CodexRuleBody(rule);
            }
        }
        else CodexGrid("Codex rules",entries,360,rule=>
        {
            CodexRows++;
            if(CodexBeginCard("Rule/"+rule.Id))
            {
                CodexHeading(rule.Title.ToUpperInvariant(),16,new ImColor8(255,207,74));
                if(RuleFullyVisible(ImGui.GetItemRectMin(),ImGui.GetItemRectMax()))CodexRuleVisibleSections.Add(rule.Id);
                CodexRuleBody(rule);
            }
            CodexEndCard();
        });
    }

    private void CodexRuleBody(CodexRule rule)
    {
        // The exported HTML is the authoritative source for emphasis, tier
        // colors and round chips. Blocks remain unchanged for data/search checks.
        if(!ruleMarkup.TryGetValue(rule.Html,out var markup))
        {
            string html=Regex.Replace(rule.Html,@"&(?!#\d+;|#x[\da-fA-F]+;|amp;|lt;|gt;|quot;|apos;)","&amp;");
            markup=XElement.Parse("<rule>"+html+"</rule>",LoadOptions.PreserveWhitespace);ruleMarkup.Add(rule.Html,markup);
        }
        int table=0;
        void Draw(XNode node)
        {
            if(node is XText text)
            {
                if(!string.IsNullOrWhiteSpace(text.Value))RuleParagraph([new(text.Value,CodexBody)]);
                return;
            }
            if(node is not XElement element)return;
            if(element.Name.LocalName=="table"){RuleTable(rule.Id+"/"+table++,element);return;}
            if(RuleClass(element,"cx-rounds")){RuleRounds(rule.Id,element);return;}
            if(element.Name.LocalName is "p" or "li")
            {
                var runs=RuleRuns(element,RuleClass(element,"muted")?CodexMuted:CodexBody).ToArray();
                if(element.Name.LocalName=="li")runs=[new("• ",CodexMuted),..runs];
                RuleParagraph(runs);return;
            }
            foreach(var child in element.Nodes())Draw(child);
        }
        foreach(var node in markup.Nodes())Draw(node);
    }

    private static bool RuleClass(XElement element,string name)=>((string?)element.Attribute("class")??"").Split(' ').Contains(name);
    private static IEnumerable<RuleRun> RuleRuns(XNode node,ImColor8 color,bool bold=false)
    {
        if(node is XText text){yield return new(WebUtility.HtmlDecode(text.Value),color,bold);yield break;}
        if(node is not XElement element)yield break;
        bool emphasized=bold||element.Name.LocalName is "b" or "th";
        if(element.Name.LocalName=="b"&&!bold)color=CodexInk;
        var match=Regex.Match((string?)element.Attribute("style")??"",@"(?:^|;)\s*color\s*:\s*(#[0-9a-fA-F]{6})");
        if(match.Success)color=CodexColor(match.Groups[1].Value);
        foreach(var child in element.Nodes())foreach(var run in RuleRuns(child,color,emphasized))yield return run;
    }
    private static ImFontPtr RuleFont(bool bold)=>bold?NativeUiTheme.BodyBoldFont:NativeUiTheme.BodyFont;
    private static float RuleTextWidth(string value,float font,bool bold=false)=>RuleFont(bold).CalcTextSizeA(font,float.MaxValue,0,value).X;
    private static bool RuleNumericToken(string value)=>Regex.IsMatch(value,@"^[+-]?\d+(?:[.,]\d+)*(?:%|g|\+|x)?$");
    private static RuleTextLayout RuleLayout(IEnumerable<RuleRun> runs,float width,float font=14,float lineHeight=20,bool breakWords=true)
    {
        var pieces=new List<RulePiece>();float x=0,y=0,max=0;bool space=false;
        width=Math.Max(1,width);
        foreach(var run in runs)foreach(Match token in Regex.Matches(run.Text,@"\s+|\S+"))
        {
            if(char.IsWhiteSpace(token.Value[0])){space=true;continue;}
            string word=token.Value;float gap=space&&x>0?RuleTextWidth(" ",font,run.Bold):0;space=false;
            if(x>0&&x+gap+RuleTextWidth(word,font,run.Bold)>width+.1f){max=Math.Max(max,x);x=0;y+=lineHeight;gap=0;}
            x+=gap;
            while(word.Length>0)
            {
                int length=word.Length;
                if(breakWords&&!RuleNumericToken(word))
                    while(length>1&&RuleTextWidth(word[..length],font,run.Bold)>width-x+.1f)length--;
                string part=word[..length];pieces.Add(new(part,new(x,y),run.Color,run.Bold));x+=RuleTextWidth(part,font,run.Bold);max=Math.Max(max,x);
                word=word[length..];if(word.Length>0){x=0;y+=lineHeight;}
            }
        }
        return new(pieces.ToArray(),max,pieces.Count==0?0:y+lineHeight);
    }
    private static void RuleDrawText(ImDrawListPtr draw,float2 at,RuleTextLayout layout,float font)
    {
        foreach(var piece in layout.Pieces)
        {
            draw.AddText(RuleFont(piece.Bold),font,at+piece.At,piece.Color,piece.Text);
        }
    }
    private static void RuleParagraph(RuleRun[] runs)
    {
        float width=ImGui.GetContentRegionAvail().X;var at=ImGui.GetCursorScreenPos();var layout=RuleLayout(runs,width);
        RuleDrawText(ImGui.GetWindowDrawList(),at,layout,14);ImGui.Dummy(new(width,layout.Height));
    }

    private static bool RuleFullyVisible(float2 min,float2 max)
    {
        var draw=ImGui.GetWindowDrawList();var clipMin=draw.GetClipRectMin();var clipMax=draw.GetClipRectMax();
        return min.X>=clipMin.X-.1f&&max.X<=clipMax.X+.1f&&min.Y>=clipMin.Y-.1f&&max.Y<=clipMax.Y+.1f;
    }
    private void RuleRounds(string ruleId,XElement rounds)
    {
        var at=ImGui.GetCursorScreenPos();float available=ImGui.GetContentRegionAvail().X,y=0;
        var draw=ImGui.GetWindowDrawList();
        foreach(var row in rounds.Elements())
        {
            var stage=row.Elements().FirstOrDefault(element=>RuleClass(element,"cx-rl"));
            float x=66,lineHeight=0,rowTop=y;
            foreach(var chip in row.Elements().Where(element=>RuleClass(element,"cx-rd")))
            {
                string number=chip.Element("b")?.Value??"",kind=string.Concat(chip.Nodes().OfType<XText>().Select(text=>text.Value)).Trim(),tags=chip.Element("i")?.Value??"";
                float width=MathF.Ceiling(Math.Max(RuleTextWidth(number,13,true),Math.Max(RuleTextWidth(kind,13),RuleTextWidth(tags,12))))+12;
                float height=tags.Length>0?55:38;
                if(x>66&&x+width>available){y+=lineHeight+4;x=66;lineHeight=0;}
                if(x+width>available)x=0;
                var min=at+new float2(x,y);var max=min+new float2(width,height);
                var border=RuleClass(chip,"carousel")?new ImColor8(122,100,32):RuleClass(chip,"pve")?new ImColor8(47,107,69):new ImColor8(122,52,66);
                draw.AddRectFilled(min,max,new ImColor8(21,30,49),5);draw.AddRect(min,max,border,5);
                RuleDrawText(draw,min+new float2(6,3),RuleLayout([new(number,CodexInk,true)],width-12,13,16),13);
                draw.AddText(RuleFont(false),13,min+new float2(6,19),CodexBody,kind);
                if(tags.Length>0)draw.AddText(RuleFont(false),12,min+new float2(6,36),new ImColor8(255,207,74),tags);
                if(RuleFullyVisible(min,max))CodexRuleVisibleRoundChips.Add(ruleId+"/"+number);
                x+=width+4;lineHeight=Math.Max(lineHeight,height);CodexRuleRoundChips++;
            }
            if(stage is not null)draw.AddText(RuleFont(false),12,at+new float2(0,rowTop+12),CodexMuted,stage.Value.ToUpperInvariant());
            y+=lineHeight+4;
        }
        ImGui.Dummy(new(available,y));
    }

    private void RuleTable(string id,XElement table)
    {
        var rows=table.Descendants("tr").Select(row=>row.Elements().Where(cell=>cell.Name.LocalName is "th" or "td").ToArray()).ToArray();
        if(rows.Length==0)return;
        int columns=rows.Max(row=>row.Length);if(columns==0)return;
        const float font=13,lineHeight=18,padding=5;
        float available=ImGui.GetContentRegionAvail().X;
        RuleRun[][][] cells=rows.Select((row,r)=>Enumerable.Range(0,columns).Select(c=>c<row.Length?RuleRuns(row[c],r==0||c==0?CodexMuted:CodexBody,r==0||c>0).ToArray():[]).ToArray()).ToArray();
        var cellIds=rows.Select((row,r)=>Enumerable.Range(0,columns).Select(c=>id+"/"+r+"/"+c).ToArray()).ToArray();
        // Whole pixels plus a one-pixel allowance avoid subtracting cell padding
        // into a fractionally narrower width than the same measured token.
        float[] widths=Enumerable.Range(0,columns).Select(c=>MathF.Ceiling(cells.Max(row=>RuleLayout(row[c],float.MaxValue,font,lineHeight).Width))+2*padding+1).ToArray();
        // Numeric stage tables can be read vertically in a narrow in-match panel.
        // Every source cell is retained, and the row names stay whole.
        if(widths.Sum()>available&&columns>rows.Length&&rows[0][0].Value=="Stage")
        {
            cells=Enumerable.Range(0,columns).Select(c=>Enumerable.Range(0,rows.Length).Select(r=>cells[r][c]).ToArray()).ToArray();
            cellIds=Enumerable.Range(0,columns).Select(c=>Enumerable.Range(0,rows.Length).Select(r=>cellIds[r][c]).ToArray()).ToArray();
            columns=rows.Length;
            widths=Enumerable.Range(0,columns).Select(c=>MathF.Ceiling(cells.Max(row=>RuleLayout(row[c],float.MaxValue,font,lineHeight).Width))+2*padding+1).ToArray();
        }
        if(widths.Sum()>available)
        {
            float[] minimum=Enumerable.Range(0,columns).Select(c=>Math.Min(widths[c],Math.Max(34,Math.Min(120,MathF.Ceiling(cells.Max(row=>row[c].SelectMany(run=>Regex.Matches(run.Text,@"\S+").Select(word=>RuleTextWidth(word.Value,font,run.Bold))).DefaultIfEmpty(0).Max()))+2*padding+1)))).ToArray();
            float remaining=Math.Max(0,available-minimum.Sum()),weights=widths.Select((width,c)=>Math.Max(0,width-minimum[c])).Sum();
            if(minimum.Sum()>available)widths=minimum.Select(width=>width*available/minimum.Sum()).ToArray();
            else widths=widths.Select((width,c)=>minimum[c]+(weights>0?remaining*(width-minimum[c])/weights:0)).ToArray();
        }
        var layouts=cells.Select(row=>row.Select((cell,c)=>RuleLayout(cell,Math.Max(1,widths[c]-2*padding),font,lineHeight,breakWords:false)).ToArray()).ToArray();
        var heights=layouts.Select(row=>row.Max(cell=>cell.Height)+6).ToArray();
        var at=ImGui.GetCursorScreenPos();var extent=new float2(widths.Sum(),heights.Sum());var draw=ImGui.GetWindowDrawList();
        CodexRuleTableBounds[id]=(at,at+extent);float y=0;
        for(int r=0;r<cells.Length;r++)
        {
            float x=0;
            for(int c=0;c<columns;c++)
            {
                var min=at+new float2(x,y);var max=min+new float2(widths[c],heights[r]);
                if(r==0)draw.AddRectFilled(min,max,new ImColor8(21,30,49));
                draw.AddRect(min,max,new ImColor8(34,48,73));
                float offset=c==0?padding:Math.Max(padding,(widths[c]-layouts[r][c].Width)/2);
                RuleDrawText(draw,min+new float2(offset,3),layouts[r][c],font);
                string value=string.Concat(cells[r][c].Select(run=>run.Text)).Trim();
                bool fits=layouts[r][c].Width<=widths[c]-2*padding+.1f&&(!RuleNumericToken(value)||layouts[r][c].Height<=lineHeight+.1f);
                CodexRuleTableTextFits&=fits;
                if(!fits)CodexRuleTableLayoutError??=$"{id} row {r}, column {c}: '{value}', content width {widths[c]-2*padding}, text {layouts[r][c].Width}x{layouts[r][c].Height}.";
                if(fits&&RuleFullyVisible(min,max))CodexRuleVisibleTableCells.Add(cellIds[r][c]);
                x+=widths[c];
            }
            y+=heights[r];
        }
        ImGui.Dummy(extent);
    }
}
