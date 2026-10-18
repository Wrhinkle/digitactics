using System.Collections.Concurrent;
using System.Text.Json;
using System.IO.Compression;
using System.Security.Cryptography;
using DigiTactics.Assets;

/// <summary>Diagnostic evidence, not a visual approval: exact deformed vertex bounds
/// across prepared cycles and production mixer transitions for every source.</summary>
internal static class ModelRuntimeAudit
{
    private sealed record Sample(string Phase,double Time,double[] Min,double[] Max,double Height,double Width,double Depth,double CenterOffset);
    private sealed record Phase(string Name,string? Clip,double Duration,bool RuntimeSupported,Sample[] Samples,string[] ReviewFlags);
    private sealed record Evidence(string SourceKey,string Asset,int Vertices,string? Idle,double BaseHeight,double BaseFootprint,Phase[] Phases,string[] ReviewFlags,string? Error);
    public static void Run(string root,string? filter=null)
    {
        string Hash(string path)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
        var codeFiles=new[]{"../js/model-motion.js","../js/model-anatomy.js","../js/model-rigid-parts.js","src/DigiTactics.Assets/GltfRig.cs","src/DigiTactics.Assets/ModelAnimator.cs","src/DigiTactics.Assets/CombatModelAnimator.cs","src/DigiTactics.Assets/CompanionLocomotion.cs","src/DigiTactics.Assets/GltfMesh.cs","src/DigiTactics.Desktop/CombatModelAnimations.cs","src/DigiTactics.Desktop/BattleView.cs","src/DigiTactics.Desktop/GpuModel.cs","src/DigiTactics.Presentation/CombatChoreography.cs","tests/DigiTactics.Tests/ModelRuntimeAudit.cs"};
        string catalogHash=Hash(Path.Combine(root,"content/models.json"));
        var codeHashes=codeFiles.ToDictionary(path=>path,path=>Hash(Path.Combine(root,path)));
        var assemblyHashes=new[]{typeof(ModelRuntimeAudit).Assembly,typeof(ModelAnimator).Assembly}.ToDictionary(assembly=>Path.GetFileName(assembly.Location),assembly=>Hash(assembly.Location));
        var catalog=ModelCatalog.Load(Path.Combine(root,"content/models.json"));
        string assetRoot=Path.Combine(root,"artifacts/model-content"),suffix=filter is null?"":"-filtered",output=Path.Combine(root,"artifacts/model-runtime-audit"+suffix+".json.gz");
        var result=new ConcurrentDictionary<string,Evidence>();int completed=0;
        var sources=catalog.Sources.Where(pair=>filter is null||pair.Key.Contains(filter,StringComparison.OrdinalIgnoreCase)).ToArray();
        Parallel.ForEach(sources,new ParallelOptions{MaxDegreeOfParallelism=Math.Min(6,Environment.ProcessorCount)},pair=>
        {
            try{result[pair.Key]=Inspect(pair.Key,pair.Value,catalog,assetRoot);}
            catch(Exception error){result[pair.Key]=new(pair.Key,pair.Value.Asset,0,null,0,0,[],["load-or-sampling-failure"],error.ToString());}
            int count=Interlocked.Increment(ref completed);if(count%25==0||count==sources.Length)Console.WriteLine($"Runtime animation audit: {count}/{sources.Length} sources sampled.");
        });
        if(catalogHash!=Hash(Path.Combine(root,"content/models.json"))||codeHashes.Any(pair=>pair.Value!=Hash(Path.Combine(root,pair.Key))))
            throw new InvalidOperationException("Model audit inputs changed during sampling. Preserve the previous coherent report and rerun after source/content export is frozen.");
        var report=new{schemaVersion=1,generatedUtc=DateTimeOffset.UtcNow,scope="CPU exact skinned vertex bounds; visual T-pose, texture appearance and clipping require rendered audit",sampling="33 uniformly spaced full-cycle samples including both endpoints; runtime fades at20ms and action endpoints; two win loops; movement start/two gait cycles/arrival",sourceCount=sources.Length,perSource=result.OrderBy(pair=>pair.Key,StringComparer.Ordinal).ToDictionary(pair=>pair.Key,pair=>pair.Value),errors=result.Values.Count(value=>value.Error is not null),review=result.Values.Count(value=>value.ReviewFlags.Length>0)};
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);
        using(var file=File.Create(output))using(var gzip=new GZipStream(file,CompressionLevel.Optimal))JsonSerializer.Serialize(gzip,report);
        string evidenceHash;using(var file=File.OpenRead(output))using(var gzip=new GZipStream(file,CompressionMode.Decompress))evidenceHash=Convert.ToHexString(SHA256.HashData(gzip)).ToLowerInvariant();
        var summary=new{schemaVersion=1,catalogSha256=catalogHash,codeHashes,assemblyHashes,evidenceSha256=evidenceHash,compressedSha256=Hash(output),evidenceFile=output,report.scope,report.sampling,
            sources=result.Values.OrderBy(value=>value.SourceKey,StringComparer.Ordinal).Select(value=>new{sourceKey=value.SourceKey,asset=value.Asset,status=value.Error is null?"UNVERIFIED":"FAIL",runtimeSampling=value.Error is null?"FINITE":"FAIL",review=value.ReviewFlags,failures=value.Error is null?Array.Empty<string>():[value.Error],
                phases=value.Phases.Select(phase=>new{name=phase.Name,clip=phase.Clip,duration=phase.Duration,runtimeSupported=phase.RuntimeSupported,sampleCount=phase.Samples.Length,review=phase.ReviewFlags,maxHeight=phase.Samples.MaxBy(sample=>sample.Height),minHeight=phase.Samples.MinBy(sample=>sample.Height),maxOffset=phase.Samples.MaxBy(sample=>sample.CenterOffset),maxFloor=phase.Samples.MaxBy(sample=>sample.Min[1]),minCeiling=phase.Samples.MinBy(sample=>sample.Max[1])})})};
        File.WriteAllText(Path.Combine(root,"artifacts/model-runtime-summary"+suffix+".json"),JsonSerializer.Serialize(summary,new JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine($"Runtime animation audit wrote {output}: {report.sourceCount} sources, {report.errors} sampling errors, {report.review} sources needing review. Bounds alone do not approve visual quality.");
        if(report.errors>0)throw new InvalidDataException("Animation audit contains sampling errors; see per-source evidence.");
    }
    private static Evidence Inspect(string key,ModelSource source,ModelCatalog catalog,string assetRoot)
    {
        using var asset=catalog.Open(assetRoot,source.Asset);
        var rig=GltfRig.Load(asset).Prepare(source,assetRoot);var mesh=GltfMesh.Load(asset);
        var idle=ModelAnimator.FindIdle(rig);var pose=new RigPose(rig);pose.Sample(idle,0);
        var baseline=mesh.Bounds(rig,pose,precise:false);
        double height=Math.Max(baseline.Max.Y-baseline.Min.Y,.001),cx=(baseline.Min.X+baseline.Max.X)/2,cz=(baseline.Min.Z+baseline.Max.Z)/2;
        double footprint=Math.Max(baseline.Max.X-baseline.Min.X,baseline.Max.Z-baseline.Min.Z)/height;
        var phases=new List<Phase>();var flags=new List<string>();
        Sample Measure(string phase,double time,RigPose current)
        {
            if(current.World.Any(matrix=>!matrix.IsFinite))throw new InvalidDataException($"Nonfinite world matrix {key}/{phase}/{time}");
            var bounds=mesh.Bounds(rig,current);
            double[] min=[(bounds.Min.X-cx)/height,(bounds.Min.Y-baseline.Min.Y)/height,(bounds.Min.Z-cz)/height];
            double[] max=[(bounds.Max.X-cx)/height,(bounds.Max.Y-baseline.Min.Y)/height,(bounds.Max.Z-cz)/height];
            if(min.Concat(max).Any(value=>!double.IsFinite(value)))throw new InvalidDataException($"Nonfinite skinned bounds {key}/{phase}/{time}");
            double ox=(min[0]+max[0])/2,oz=(min[2]+max[2])/2;
            return new(phase,time,min,max,max[1]-min[1],max[0]-min[0],max[2]-min[2],Math.Sqrt(ox*ox+oz*oz));
        }
        void Add(string name,RigAnimation? clip,bool supported,List<Sample> samples)
        {
            var review=new List<string>();
            if(clip is null&&supported)review.Add("missing-role-clip");
            if(samples.Count>0)
            {
                double initial=Math.Max(samples[0].Height,.001);
                if(samples.Max(sample=>sample.Height)>Math.Max(2.5,initial*2.5))review.Add("height-exceeds-2.5x-idle");
                if(samples.Min(sample=>sample.Height)<.2)review.Add("height-below-0.2x-idle");
                if(samples.Max(sample=>sample.CenterOffset)>Math.Max(1.5,footprint))review.Add("horizontal-center-leaves-body-footprint");
                if(samples.Max(sample=>sample.Width)>Math.Max(3,footprint*3)||samples.Max(sample=>sample.Depth)>Math.Max(3,footprint*3))review.Add("footprint-exceeds-3x-idle");
                if(samples.Min(sample=>sample.Min[1])<-.5)review.Add("vertices-below-half-idle-height");
                if(samples.Max(sample=>sample.Min[1])>1.5)review.Add("entire-model-above-1.5x-idle-height");
                if(samples.Min(sample=>sample.Max[1])<0)review.Add("entire-model-below-idle-floor");
                if(samples.Skip(1).All(sample=>sample.Min.Zip(samples[0].Min).All(values=>Math.Abs(values.First-values.Second)<1e-7)&&sample.Max.Zip(samples[0].Max).All(values=>Math.Abs(values.First-values.Second)<1e-7)))review.Add("constant-bounds-review-motion");
            }
            phases.Add(new(name,clip?.Name,clip?.Duration??0,supported,samples.ToArray(),review.ToArray()));
            flags.AddRange(review.Select(flag=>name+":"+flag));
        }
        var roles=new[]{("idle",idle),("attack",rig.Animations.FirstOrDefault(clip=>clip.Name=="attack")),("special",rig.Animations.FirstOrDefault(clip=>clip.Name=="special")),("win",rig.Animations.FirstOrDefault(clip=>clip.Name=="win")),("walk",rig.Animations.FirstOrDefault(clip=>clip.Name is "walk" or "run" or "move"))};
        foreach(var (role,clip) in roles)
        {
            var samples=new List<Sample>();
            if(clip is not null)for(int i=0;i<=32;i++){double time=clip.Duration*i/32;pose.Sample(clip,time);samples.Add(Measure(role+"-cycle",time,pose));}
            // Raw clips are separate from the production movement blend below.
            // Missing authored travel is valid: classified procedural gait takes over.
            Add(role+"-cycle",clip,role!="walk"||clip is not null,samples);
            if(clip is null||role is "idle" or "walk")continue;
            var animator=new ModelAnimator(rig,(idle?.Duration??0)*.17);animator.Update(0);
            if(!animator.Play(role))throw new InvalidDataException($"Prepared runtime rejected {key}/{role}");
            double speed=role=="attack"?Math.Max(1,clip.Duration/.55):role=="special"?Math.Max(1,clip.Duration/1.1):1;
            double end=clip.Duration/speed;
            var times=new SortedSet<double>{0,.02,.04,.06,.08,.1,.15};
            for(int i=0;i<=32;i++)times.Add(end*i/32);
            foreach(double t in new[]{end-.001,end,end+.001,end+.02,end+.08,end+.15,end+.3})if(t>=0)times.Add(t);
            if(role=="win")for(int i=0;i<=32;i++)times.Add(end+end*i/32);
            samples=[];double previous=0;
            foreach(double time in times){animator.Update(time-previous);samples.Add(Measure(role+"-runtime",time,animator.Pose));previous=time;}
            Add(role+"-runtime",clip,true,samples);
        }
        var attack=roles[1].Item2;var special=roles[2].Item2;var win=roles[3].Item2;
        var moving=new CombatModelAnimator(rig,key);double gaitDuration=roles[4].Item2?.Duration??Math.PI*2/8.5;
        double travelDuration=.25+gaitDuration*2,previousMovementTime=0;
        var travelSamples=new List<Sample>();
        for(int frame=0;frame<=64;frame++)
        {
            double time=travelDuration*frame/64;moving.Update(time-previousMovementTime,true);previousMovementTime=time;
            if(!double.IsFinite(moving.GlideOffset)||Math.Abs(moving.GlideOffset)>.0250001)throw new InvalidDataException($"Unbounded movement offset: {key}");
            travelSamples.Add(Measure("movement-"+moving.LocomotionKind,time,moving.Pose));
        }
        Add("movement-"+moving.LocomotionKind,roles[4].Item2??idle,idle is not null,travelSamples);
        var arrivalSamples=new List<Sample>();
        for(int frame=0;frame<=32;frame++){moving.Update(frame==0?0:.025,false);arrivalSamples.Add(Measure("movement-arrival",frame*.025,moving.Pose));}
        Add("movement-arrival",idle,idle is not null,arrivalSamples);
        if(attack is not null&&special is not null)
        {
            var animator=new ModelAnimator(rig);animator.Update(.13);animator.Play("attack");animator.Update(.1);animator.Play("special");
            var samples=new List<Sample>{Measure("attack-to-special",0,animator.Pose)};
            for(int i=1;i<=12;i++){animator.Update(.02);samples.Add(Measure("attack-to-special",i*.02,animator.Pose));}
            Add("attack-to-special",special,true,samples);
            if(win is not null)
            {
                animator.Play("win");samples=[Measure("special-to-win",0,animator.Pose)];
                for(int i=1;i<=12;i++){animator.Update(.02);samples.Add(Measure("special-to-win",i*.02,animator.Pose));}
                Add("special-to-win",win,true,samples);
            }
        }
        return new(key,source.Asset,mesh.Meshes.Sum(group=>group.Sum(primitive=>primitive.VertexCount)),idle?.Name,height,footprint,phases.ToArray(),flags.ToArray(),null);
    }
}
