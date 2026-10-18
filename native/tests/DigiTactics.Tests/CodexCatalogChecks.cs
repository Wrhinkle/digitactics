using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Presentation;
using DigiTactics.Rules;

internal static class CodexCatalogChecks
{
    public static int Run(string root)
    {
        var catalog=new CodexCatalog(Path.Combine(root,"content/codex.json"));
        var gameplay=ContentCatalog.Load(Path.Combine(root,"content/catalog.json"));
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/codex.json.gz"));
        using var gzip=new GZipStream(file,CompressionMode.Decompress);
        using var fixture=JsonDocument.Parse(gzip);
        int checks=0;
        void Check(bool okay,string message){checks++;if(!okay)throw new InvalidOperationException("Codex: "+message);}
        static string Esc(string text)=>text.Replace("&","&amp;").Replace("<","&lt;").Replace(">","&gt;").Replace("\"","&quot;");
        void Text(string html,string text,string message)=>Check(html.Contains(Esc(text),StringComparison.Ordinal),message);
        void EqualIds(IEnumerable<string> actual,IEnumerable<string> expected,string message)=>Check(actual.Order(StringComparer.Ordinal).SequenceEqual(expected.Order(StringComparer.Ordinal)),message);
        Check(catalog.Variants.Count==22,"all 22 rules variants");
        EqualIds(catalog.Variants.Select(v=>v.Id),gameplay.Variants.Select(v=>v.Id),"gameplay variant coverage");
        foreach(var source in fixture.RootElement.EnumerateArray())
        {
            string id=source.GetProperty("id").GetString()!;var v=catalog.Variant(id);var rules=gameplay.Variants.Single(r=>r.Id==id);
            var nativeLines=rules.Lines.Concat(rules.ExtraLines).ToDictionary(l=>l.Id,StringComparer.Ordinal);
            EqualIds(v.Lines.Select(l=>l.Id),nativeLines.Keys,id+" unit coverage");
            EqualIds(v.Traits.Select(t=>t.Id),rules.Traits.Select(t=>t.Id),id+" trait coverage");
            EqualIds(v.Items.Select(i=>i.Id),rules.Items.Select(i=>i.Id),id+" item coverage");
            EqualIds(v.Wisps.Select(w=>w.Id),rules.Wisps!.Select(w=>w.Id),id+" Wisp coverage");
            Check(v.Tabs.Contains("wisps")== (v.Wisps.Length>0),id+" conditional Wisp tab");
            Check(v.Tabs.Contains("augments")== (v.Augments.Length>0),id+" conditional augment tab");
            foreach(var trait in v.Traits)
            {
                string html=source.GetProperty("traitCards").GetProperty(trait.Id).GetString()!;
                Text(html,trait.Name,id+" trait name "+trait.Id);Text(html,trait.Description,id+" trait description "+trait.Id);
                foreach(string note in trait.Notes)Text(html,note,id+" trait note "+trait.Id);
                var definition=rules.Traits.Single(t=>t.Id==trait.Id);
                Check(trait.Breakpoints.Select(b=>b.Count).SequenceEqual(definition.Breakpoints),id+" trait thresholds "+trait.Id);
                foreach(var bp in trait.Breakpoints)Text(html,bp.Text,id+" breakpoint explanation "+trait.Id);
                Check(trait.LineIds.All(line=>v.Lines.Any(l=>l.Id==line&&l.Traits.Contains(trait.Id))),id+" trait line links "+trait.Id);
                Check(trait.CrestIds.All(item=>v.Items.Any(i=>i.Id==item&&i.Emblem==trait.Id)),id+" trait crest links "+trait.Id);
            }
            foreach(var line in v.Lines)
            {
                var definition=nativeLines[line.Id];string html=source.GetProperty("unitCards").GetProperty(line.Id).GetString()!;
                Check(line.Forms.Length==3,id+" all three forms "+line.Id);
                Text(html,line.AttributeText,id+" attribute interaction "+line.Id);
                EqualIds(line.Traits,definition.Traits,id+" line traits "+line.Id);
                foreach(var form in line.Forms)
                {
                    string context=id+"/"+line.Id+"/"+form.Stage;
                    var actual=form.Stats;var expected=UnitValues.Stats(definition,form.Stage);
                    Check(actual.MaxHp==expected.MaxHealth&&actual.Ad==expected.AttackDamage&&actual.As==expected.AttackSpeed
                        &&actual.Range==expected.Range&&actual.Armor==expected.Armor&&actual.Mr==expected.MagicResist
                        &&actual.Mana==expected.StartMana&&actual.MaxMana==expected.MaxMana,context+" display stats match gameplay");
                    Check(form.Name==definition.Forms[form.Stage],context+" form name");
                    Text(html,form.StageName,context+" stage name");Text(html,form.AbilityName,context+" ability name");Text(html,form.AbilityText,context+" full ability text");
                    if(line.Shop)Check(form.SellValue==UnitValues.SellValue(definition,form.Stage),context+" sell value");
                }
            }
            foreach(var item in v.Items)
            {
                string html=source.GetProperty("itemCards").GetProperty(item.Id).GetString()!;
                Text(html,item.Name,id+" item name "+item.Id);Text(html,item.Description,id+" item description "+item.Id);
                if(item.Kind!="component")Text(html,item.StatsText,id+" item stats text "+item.Id);
                Check(item.Kind is "component" or "core" or "crest" or "special",id+" item classification "+item.Id);
                Check(item.Recipes.All(recipe=>recipe.Length==2&&recipe.All(component=>v.ComponentIds.Contains(component))),id+" complete item recipes "+item.Id);
                foreach(var recipe in item.Recipes)
                {
                    int a=Array.IndexOf(v.ComponentIds,recipe[0]),b=Array.IndexOf(v.ComponentIds,recipe[1]);
                    Check(v.RecipeGrid[a][b]==item.Id&&v.RecipeGrid[b][a]==item.Id,id+" recipe grid result "+item.Id);
                }
            }
            for(int a=0;a<v.ComponentIds.Length;a++)for(int b=0;b<v.ComponentIds.Length;b++)
            {
                string result=v.RecipeGrid[a][b];Check(result.Length==0||v.Items.Any(i=>i.Id==result),id+" recipe grid item link");
                Check(result==v.RecipeGrid[b][a],id+" symmetric recipe grid");
            }
            foreach(var wisp in v.Wisps)
            {
                var native=rules.Wisps!.Single(w=>w.Id==wisp.Id);
                Check(wisp.BasePrice==native.Base&&wisp.MinStage==native.MinStage&&wisp.ExampleStage==Math.Max(2,native.MinStage)
                    &&wisp.ExampleCost==native.Base+wisp.ExampleStage,id+" Wisp pricing "+wisp.Id);
            }
            foreach(var query in source.GetProperty("searches").EnumerateArray())
            {
                string text=query.GetProperty("query").GetString()!;
                void Search(string category,IEnumerable<(string Id,string SearchText)> entries)=>Check(
                    entries.Where(e=>CodexCatalog.Matches(text,e.SearchText)).Select(e=>e.Id).SequenceEqual(query.GetProperty(category).EnumerateArray().Select(e=>e.GetString()!)),id+" original "+category+" search: "+text);
                Search("traits",v.Traits.Select(t=>(t.Id,t.SearchText)));Search("digimon",v.Lines.Select(t=>(t.Id,t.SearchText)));
                Search("items",v.Items.Select(t=>(t.Id,t.SearchText)));Search("augments",v.Augments.Select(t=>(t.Id,t.SearchText)));
                Search("wisps",v.Wisps.Select(t=>(t.Id,t.SearchText)));Search("rules",v.Rules.Select(t=>(t.Id,t.SearchText)));
            }
            foreach(var table in source.GetProperty("tables").EnumerateArray())
            {
                string section=table.GetProperty("section").GetString()!;int index=table.GetProperty("index").GetInt32();
                var block=v.Rules.Single(r=>r.Id==section).Blocks.Where(b=>b.Kind=="table").ElementAt(index);
                var expected=table.GetProperty("rows");Check(block.Rows.Length==expected.GetArrayLength(),id+" rules table row count "+section);
                for(int row=0;row<block.Rows.Length;row++)Check(block.Rows[row].SequenceEqual(expected[row].EnumerateArray().Select(c=>c.GetString()!)),id+" exact source table cells "+section+"/"+row);
            }
            foreach(var rule in v.Rules)
            {
                Check(rule.Blocks.Length>0,id+" readable rule "+rule.Id);
                Check(rule.Blocks.Where(b=>b.Kind=="paragraph").All(b=>!b.Text.Contains('<')&&!b.Text.Contains('>')),id+" decoded rule paragraphs "+rule.Id);
                string text=string.Join(" ",rule.Blocks.Select(block=>block.Kind=="table"?string.Join(" ",block.Rows.SelectMany(row=>row)):block.Text.StartsWith("• ",StringComparison.Ordinal)?block.Text[2..]:block.Text));
                Check(string.Concat(text.Where(c=>!char.IsWhiteSpace(c)))==source.GetProperty("ruleText").GetProperty(rule.Id).GetString(),id+" no lost source paragraph, list, schedule, or table text "+rule.Id);
            }
        }
        Check(CodexCatalog.Matches("GOLD  damage","gold: damage and armor"),"case-insensitive multiple-word search");
        Check(!CodexCatalog.Matches("gold unknown","gold damage"),"all words required");
        Console.WriteLine($"Codex: {checks:N0} original text, search, table, recipe, coverage, and gameplay-stat checks passed across all 22 variants.");
        return checks;
    }
}
