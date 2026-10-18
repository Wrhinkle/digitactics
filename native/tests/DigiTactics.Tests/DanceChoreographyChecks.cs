using System.Text.Json;
using DigiTactics.Assets;

internal static class DanceChoreographyChecks
{
    public static int Run(string root)
    {
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/dance-choreography.json.gz"));
        using var gzip = new System.IO.Compression.GZipStream(file, System.IO.Compression.CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip); int checks = 0;
        foreach (var row in fixture.RootElement.EnumerateArray())
        {
            string id = row.GetProperty("id").GetString()!; double seconds = row.GetProperty("seconds").GetDouble();
            void Check(bool ok, string why) { checks++; if (!ok) throw new InvalidOperationException($"Choreography {id}/{seconds:R}: {why}"); }
            void Near(double a, double e, string key) => Check(Math.Abs(a - e) < 1e-5, $"{key}: {a:R} vs {e:R}");
            Check(DanceChoreography.Bpm(id) == row.GetProperty("bpm").GetInt32(), "tempo");
            var pose = DanceChoreography.Evaluate(id, seconds); int vectors = 0, scalars = 0, roots = 0;
            foreach (var p in row.GetProperty("pose").EnumerateObject())
            {
                if (p.Value.ValueKind == JsonValueKind.Array)
                {
                    vectors++; Check(pose.Vectors.ContainsKey(p.Name), "missing " + p.Name); var v = pose.Vectors[p.Name];
                    Near(v.X, p.Value[0].GetDouble(), p.Name + ".x"); Near(v.Y, p.Value[1].GetDouble(), p.Name + ".y"); Near(v.Z, p.Value[2].GetDouble(), p.Name + ".z");
                }
                else if (p.Value.ValueKind == JsonValueKind.Number)
                { scalars++; Check(pose.Scalars.ContainsKey(p.Name), "missing " + p.Name); Near(pose.Scalars[p.Name], p.Value.GetDouble(), p.Name); }
                else if (p.Name == "rt") foreach (var r in p.Value.EnumerateObject())
                { roots++; Check(pose.Root.ContainsKey(r.Name), "missing root " + r.Name); Near(pose.Root[r.Name], r.Value.GetDouble(), r.Name); }
            }
            Check(vectors == pose.Vectors.Count && scalars == pose.Scalars.Count && roots == pose.Root.Count, "unexpected channels");
        }
        foreach (string? id in new string?[] { null, "unknown", "mystery_daily_dance_0", "mystery_vault_dance_21" })
        {
            checks++; if (DanceChoreography.Bpm(id) != 120 || DanceChoreography.Duration(id) != 4) throw new InvalidOperationException("Default dance fallback failed.");
        }
        checks+=SecretQuality(root);
        Console.WriteLine($"Dance choreography: {checks:N0} continuous pose/channel/tempo and authored-secret-quality checks passed."); return checks;
    }
    private static int SecretQuality(string root)
    {
        using var asset=GltfAsset.LoadCompressed(File.ReadAllBytes(Path.Combine(root,"tests/fixtures/dance-skeleton.glb.gz")));
        using var data=JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"content/mystery-dances.json")));
        var rig=GltfRig.Load(asset);var target=new RigPose(rig);int checks=0;
        void Check(bool ok,string why){checks++;if(!ok)throw new InvalidOperationException("Secret dance quality: "+why);}
        var body=new DanceSkeleton(new([],true,true));
        var tail=new DanceSkeleton(new([new(1,"tail",-1,new(0,0,1),System.Numerics.Quaternion.Identity,System.Numerics.Quaternion.Identity)],false,false));
        var quad=new DanceSkeleton(new([new(0,"up",-1,new(0,-1,0),System.Numerics.Quaternion.Identity,System.Numerics.Quaternion.Identity,1,Limit:55*MathF.PI/180)],false,true,Quadruped:true));
        for(int pool=0;pool<2;pool++)for(int i=1;i<=20;i++)
        {
            string id=$"mystery_{(pool==0?"daily":"vault")}_dance_{i}";double seconds=60d/DanceChoreography.Bpm(id);
            var phrase=data.RootElement.GetProperty("phrases")[pool*20+i-1];
            var cosmetic=DigiTactics.Storage.CosmeticCollection.Item("dance",id)!;
            Check(cosmetic.Name.EndsWith(" "+phrase.GetProperty("title").GetString(),StringComparison.Ordinal),"native title matches authored dance "+id);
            Check(cosmetic.Desc==phrase.GetProperty("story").GetString()+$" {DanceChoreography.Bpm(id)} BPM.","native description matches authored dance "+id);
            Check(cosmetic.Secret&&cosmetic.LootPool==(pool==0?"daily":"vault")&&cosmetic.Egg==false,"native unlock remains loot-exclusive "+id);
            Check(cosmetic.Data.GetProperty("bpm").GetInt32()==DanceChoreography.Bpm(id),"native cosmetic tempo matches choreography "+id);
            double hold=phrase.GetProperty("hold").GetDouble();
            for(int beat=0;beat<8;beat++)
            {
                target.Sample(null,0);tail.Apply(DanceChoreography.Evaluate(id,(beat+hold-1e-7)*seconds),target);var before=target.Local[1].Rotation;
                target.Sample(null,0);tail.Apply(DanceChoreography.Evaluate(id,(beat+hold+1e-7)*seconds),target);var after=target.Local[1].Rotation;
                Check(Math.Abs(before.X-after.X)+Math.Abs(before.Y-after.Y)+Math.Abs(before.Z-after.Z)+Math.Abs(before.W-after.W)<.0001,"tail starts smoothly after held pose "+id);
            }
            for(int frame=0;frame<=128;frame++)
            {
                double beat=frame/16d;var p=DanceChoreography.Evaluate(id,beat*seconds);var repeat=DanceChoreography.Evaluate(id,(beat+8)*seconds);
                foreach(var v in p.Vectors)Check(System.Numerics.Vector3.Distance(v.Value,repeat.Vectors[v.Key])<.0001,"phrase loop closes "+id);
                foreach(float weight in new[]{0f,.2f,1f})
                {
                    var placement=body.Apply(p,null,weight);
                    Check(placement.Squash==1,"limbless size stays rigid "+id);
                    Check(placement.Position.Length()<.18f,"body stays within its assigned spot "+id);
                }
                target.Sample(null,0);quad.Apply(p,target);
                var q=target.Local[0].Rotation;double angle=2*Math.Acos(Math.Min(1,Math.Abs(q.W)));
                Check(angle<.3,"quadruped foreleg remains a small step, not an overhead arm raise "+id);
                foreach(var t in target.Local)Check(t.Scale.X==1&&t.Scale.Y==1&&t.Scale.Z==1,"retargeting preserves bone scales "+id);
            }
        }
        return checks;
    }
}
