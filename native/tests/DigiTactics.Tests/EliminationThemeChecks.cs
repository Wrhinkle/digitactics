using System.Globalization;
using System.Text.Json;
using DigiTactics.Presentation;

internal static class EliminationThemeChecks
{
    private sealed record Operation(string OperationName,double[] Values,EliminationPaint? Paint);
    private sealed class Trace : EliminationCanvas
    {
        public List<Operation> Operations {get;}=[];
        protected override void Emit(string operation,double[] values,EliminationPaint? paint=null)=>Operations.Add(new(operation,values,paint));
    }
    public static int Run(string root)
    {
        int checks=0;
        void Check(bool okay,string message){checks++;if(!okay)throw new InvalidOperationException("Elimination theme: "+message);}
        void Number(double actual,double expected,string context)=>Check(double.IsFinite(actual)&&Math.Abs(actual-expected)<1e-7,context+$" ({actual} != {expected})");
        void Color(EliminationColor color,JsonElement expected,string context)
        {
            double[] values=[color.R,color.G,color.B,color.A];
            if(expected.ValueKind==JsonValueKind.Array)for(int i=0;i<4;i++)Number(values[i],expected[i].GetDouble(),context);
            else {Number(color.R,expected.GetProperty("r").GetDouble(),context);Number(color.G,expected.GetProperty("g").GetDouble(),context);Number(color.B,expected.GetProperty("b").GetDouble(),context);Number(color.A,expected.GetProperty("a").GetDouble(),context);}
        }
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/elimination-themes.json.gz"));
        using var gzip=new System.IO.Compression.GZipStream(file,System.IO.Compression.CompressionMode.Decompress);
        using var fixture=JsonDocument.Parse(gzip);
        var themes=new HashSet<string>();
        CultureInfo previous=CultureInfo.CurrentCulture;
        try
        {
            // Localized computers must produce the same procedural color channels.
            CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo("fr-FR");
            foreach(var row in fixture.RootElement.EnumerateArray())
            {
                string id=row.GetProperty("id").GetString()!;themes.Add(id);double time=row.GetProperty("t").GetDouble();
                string context=id+" at "+time;var trace=new Trace();
                EliminationThemes.Draw(trace,id,false,row.GetProperty("w").GetDouble(),row.GetProperty("h").GetDouble(),time);
                Color(EliminationThemes.Rim(id),row.GetProperty("rim"),context+" rim");Color(EliminationThemes.Text(id),row.GetProperty("text"),context+" text");
                Check(EliminationThemes.Silhouette(id)==row.GetProperty("silhouette").GetBoolean(),context+" silhouette");
                var operations=row.GetProperty("operations");Check(trace.Operations.Count==operations.GetArrayLength(),context+" operation count");
                for(int i=0;i<operations.GetArrayLength();i++)
                {
                    var actual=trace.Operations[i];var expected=operations[i];string label=context+" operation "+i+" "+actual.OperationName;
                    Check(actual.OperationName==expected.GetProperty("operation").GetString(),label);
                    var values=expected.GetProperty("values");Check(actual.Values.Length==values.GetArrayLength(),label+" parameter count");
                    for(int n=0;n<actual.Values.Length;n++)Number(actual.Values[n],values[n].GetDouble(),label+" value "+n);
                    bool hasPaint=expected.TryGetProperty("paint",out var paint);Check((actual.Paint is not null)==hasPaint,label+" paint");
                    if(actual.Paint is not {} p)continue;
                    Check(p.Kind==paint.GetProperty("kind").GetInt32(),label+" paint kind");
                    var pv=paint.GetProperty("values");Check(p.Values.Length==pv.GetArrayLength(),label+" paint values");
                    for(int n=0;n<p.Values.Length;n++)Number(p.Values[n],pv[n].GetDouble(),label+" paint value "+n);
                    var stops=paint.GetProperty("stops");Check(p.Stops.Count==stops.GetArrayLength(),label+" stops");
                    for(int n=0;n<p.Stops.Count;n++){Number(p.Stops[n].At,stops[n].GetProperty("at").GetDouble(),label+" stop");Color(p.Stops[n].Color,stops[n].GetProperty("color"),label+" stop color");}
                }
            }
        }
        finally {CultureInfo.CurrentCulture=previous;}
        Check(themes.Count==51,"every original and mystery background represented");
        Check(EliminationThemes.Resolve("ks_gate")=="gate","cosmetic ID mapping");Check(EliminationThemes.Resolve("unknown")=="basic","unknown theme fallback");
        Check(EliminationThemes.Resolve("ks_gate",true)=="sting","PvE neutral theme");Check(EliminationThemes.Resolve("eliminated")=="sting","PvE audio ID");
        Check(EliminationThemes.Resolve("mystery_daily_killscreen_00")=="basic","invalid mystery ID fallback");
        Console.WriteLine($"Elimination themes: {checks:N0} original drawing-operation, geometry, color and timing checks passed.");return checks;
    }
}
