using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using DigiTactics.Assets;

/// <summary>Exhaustive shipped-catalog inventory. Numerical validity is never a visual approval.</summary>
internal static class ModelInventoryAudit
{
    private static readonly JsonSerializerOptions JsonOptions=new(){PropertyNamingPolicy=JsonNamingPolicy.CamelCase,WriteIndented=true};
    private sealed record Finding(string Code,string Detail);
    private sealed record Alias(string Variant,string Line,int Stage);
    private sealed record ImageEvidence(int Index,int Width,int Height,string PixelSha256,string? OriginalPixelSha256,string Status);
    private sealed record TextureEvidence(int Index,int Image,TextureSampler Sampler);
    private sealed record MaterialEvidence(int Index,GltfMaterial Material,int[] TextureDependencies);
    private sealed record ClipEvidence(int Index,string Name,string Origin,double Duration,int Channels,int Keys,int[] Nodes,int[] VisibleJointAncestors,int ChangingChannels,string MotionStatus);
    private sealed record CompanionBinding(string Context,double Height,bool Unit,JsonElement Clips,Dictionary<string,string> Procedural);
    private sealed record RoleEvidence(string Role,string? Clip,string Status,string Detail);
    private sealed class AssetEvidence
    {
        public required string Asset {get;init;}
        public required string Payload {get;init;}
        public required string ExpectedSha256 {get;init;}
        public string Status {get;set;}="UNVERIFIED";
        public string Integrity {get;set;}="UNVERIFIED";
        public int Nodes {get;set;}
        public int Meshes {get;set;}
        public int Skins {get;set;}
        public int Vertices {get;set;}
        public int Triangles {get;set;}
        public int RepairedKeys {get;set;}
        public int RepairedBindMatrices {get;set;}
        public int ZeroScaleNodes {get;set;}
        public List<ImageEvidence> Images {get;}=[];
        public List<TextureEvidence> Textures {get;}=[];
        public List<MaterialEvidence> Materials {get;}=[];
        public List<Finding> Failures {get;}=[];
        public List<Finding> Review {get;}=[];
    }
    private sealed class SourceEvidence
    {
        public required string SourceKey {get;init;}
        public required string Asset {get;init;}
        public string? MotionFile {get;init;}
        public string? MotionSha256 {get;init;}
        public required JsonElement Metadata {get;init;}
        public required Alias[] Aliases {get;init;}
        public required string[] CompanionContexts {get;init;}
        public CompanionBinding[] CompanionBindings {get;init;}=[];
        public string Status {get;set;}="UNVERIFIED";
        public string StructureStatus {get;set;}="UNVERIFIED";
        public string RenderedStatus {get;set;}="UNVERIFIED";
        public string RuntimeStatus {get;set;}="UNVERIFIED";
        public int PreparedParentChanges {get;set;}
        public int PreparedZeroScaleNodes {get;set;}
        public List<ClipEvidence> Clips {get;}=[];
        public List<RoleEvidence> Roles {get;}=[];
        public List<Finding> Failures {get;}=[];
        public List<Finding> Review {get;}=[];
    }
    public static int Run(string root)
    {
        string content=Path.Combine(root,"artifacts/model-content"),output=Path.Combine(root,"artifacts/model-audit-inventory.json");
        var codeIdentity=new Dictionary<string,string>();
        foreach(string file in new[]{"src/DigiTactics.Assets/GltfAsset.cs","src/DigiTactics.Assets/GltfRig.cs","src/DigiTactics.Assets/GltfMesh.cs","src/DigiTactics.Assets/GltfMaterial.cs","tests/DigiTactics.Tests/ModelInventoryAudit.cs"})
            codeIdentity[file]=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(root,file)))).ToLowerInvariant();
        string loadedAssetsAssemblySha256=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(GltfRig).Assembly.Location))).ToLowerInvariant();
        var catalog=ModelCatalog.Load(Path.Combine(root,"content/models.json"));
        var companions=CompanionCatalog.Load(Path.Combine(root,"content/companion-models.json"));
        using var textureFixture=JsonDocument.Parse(File.ReadAllBytes(Path.Combine(root,"tests/fixtures/textures.json")));
        var references=textureFixture.RootElement.EnumerateArray().ToDictionary(item=>item.GetProperty("asset").GetString()!,item=>item.GetProperty("images"));
        var aliases=new Dictionary<string,List<Alias>>();var missingAliases=new List<object>();
        foreach(var (variant,lines) in catalog.Variants)foreach(var (line,stages) in lines)for(int stage=0;stage<stages.Length;stage++)
        {
            string? key=stages[stage];
            if(key is null||!catalog.Sources.ContainsKey(key)){missingAliases.Add(new{variant,line,stage,sourceKey=key,status="FAIL",detail="Unresolved shipped variant model."});continue;}
            if(!aliases.TryGetValue(key,out var list))aliases[key]=list=[];
            list.Add(new(variant,line,stage));
        }
        var contexts=new Dictionary<string,List<string>>();var companionBindings=new Dictionary<string,List<CompanionBinding>>();
        foreach(var (kind,visuals) in new[]{("base",companions.Bases),("form",companions.Forms)})foreach(var (id,visual) in visuals)
        {
            if(!contexts.TryGetValue(visual.Source,out var list))contexts[visual.Source]=list=[];
            list.Add(kind+":"+id);
            if(!companionBindings.TryGetValue(visual.Source,out var bindings))companionBindings[visual.Source]=bindings=[];
            bindings.Add(new(kind+":"+id,visual.Height,visual.Unit,visual.Clips,visual.Procedural));
        }
        var assets=new List<AssetEvidence>();var sources=new List<SourceEvidence>();int done=0;
        foreach(var (assetKey,entry) in catalog.Assets.OrderBy(pair=>pair.Key,StringComparer.Ordinal))
        {
            var evidence=new AssetEvidence{Asset=assetKey,Payload=entry.File,ExpectedSha256=entry.Sha256};assets.Add(evidence);
            GltfRig? original=null;GltfMesh? mesh=null;
            try
            {
                using var asset=catalog.Open(content,assetKey);evidence.Integrity="VERIFIED";
                original=GltfRig.Load(asset);mesh=GltfMesh.Load(asset);
                evidence.Nodes=original.Nodes.Length;evidence.Meshes=mesh.Meshes.Length;evidence.Skins=original.Skins.Length;
                evidence.Vertices=mesh.Meshes.Sum(group=>group.Sum(primitive=>primitive.VertexCount));evidence.Triangles=mesh.Meshes.Sum(group=>group.Sum(primitive=>primitive.Indices.Length/3));
                evidence.RepairedKeys=original.RepairedKeys;evidence.RepairedBindMatrices=original.RepairedBindMatrices;
                evidence.ZeroScaleNodes=original.Nodes.Count(node=>ZeroScale(node.Rest));
                if(evidence.Nodes!=entry.Nodes||evidence.Meshes!=entry.Meshes||evidence.Skins!=entry.Skins)throw new InvalidDataException("Payload structure differs from the catalog counts.");
                if(evidence.Vertices==0||mesh.Instances.Length==0)throw new InvalidDataException("Model has no rendered mesh vertices or scene instances.");
                ValidateRig(original);
                int materials=asset.Root.TryGetProperty("materials",out var materialJson)?materialJson.GetArrayLength():0;
                int textures=asset.Root.TryGetProperty("textures",out var textureJson)?textureJson.GetArrayLength():0;
                int images=asset.Root.TryGetProperty("images",out var imageJson)?imageJson.GetArrayLength():0;
                for(int i=0;i<images;i++)
                {
                    var image=asset.Image(i);string hash=Convert.ToHexString(SHA256.HashData(image.Rgba)).ToLowerInvariant();
                    string? expected=null;string status="UNVERIFIED";
                    if(references.TryGetValue(assetKey,out var expectedImages)&&i<expectedImages.GetArrayLength())
                    {
                        var expectedImage=expectedImages[i];expected=expectedImage.GetProperty("sha256").GetString();
                        bool matches=hash==expected&&image.Width==expectedImage.GetProperty("width").GetInt32()&&image.Height==expectedImage.GetProperty("height").GetInt32();
                        status=matches?"VERIFIED":"FAIL";
                        if(!matches)evidence.Failures.Add(new("texture-pixels","Decoded image "+i+" differs from original-browser pixel fixture."));
                    }
                    else evidence.Review.Add(new("texture-reference-missing","Image "+i+" decoded but has no original-browser pixel reference."));
                    evidence.Images.Add(new(i,image.Width,image.Height,hash,expected,status));
                }
                for(int i=0;i<textures;i++)
                {
                    var texture=GltfTexture.Load(asset,i);if(texture.Image<0||texture.Image>=images)throw new InvalidDataException("Texture references an unavailable image: "+i);
                    evidence.Textures.Add(new(i,texture.Image,texture.Sampler));
                }
                for(int i=0;i<materials;i++)
                {
                    var material=GltfMaterial.Load(asset,i);var bindings=material.Textures().ToArray();
                    if(bindings.Any(binding=>binding.Texture<0||binding.Texture>=textures||binding.TexCoord!=0))throw new InvalidDataException("Material texture/UV dependency is unsupported: "+i);
                    double[] values=[material.Color.R,material.Color.G,material.Color.B,material.Color.A,material.Roughness,material.AlphaCutoff,material.NormalScale,material.SpecularIntensity,material.Emissive.R,material.Emissive.G,material.Emissive.B,material.EmissiveIntensity];
                    if(values.Any(value=>!double.IsFinite(value)))throw new InvalidDataException("Material contains a nonfinite scalar: "+i);
                    evidence.Materials.Add(new(i,material,bindings.Select(binding=>binding.Texture).Distinct().ToArray()));
                }
                foreach(var instance in mesh.Instances)
                {
                    if(instance.Node<0||instance.Node>=original.Nodes.Length||instance.Mesh<0||instance.Mesh>=mesh.Meshes.Length||instance.Skin>=original.Skins.Length)throw new InvalidDataException("Scene references an invalid mesh, node or skin.");
                    foreach(var primitive in mesh.Meshes[instance.Mesh])
                    {
                        if(primitive.Material>=materials||primitive.Material< -1)throw new InvalidDataException("Primitive material index is invalid.");
                        if(primitive.HasSkin&&instance.Skin<0)evidence.Review.Add(new("unbound-skin-attributes","Mesh "+instance.Mesh+" has joint attributes on an unskinned instance."));
                        if(instance.Skin<0||!primitive.HasSkin)continue;
                        int joints=original.Skins[instance.Skin].Joints.Length;
                        for(int vertex=0;vertex<primitive.VertexCount;vertex++)for(int lane=0;lane<4;lane++)
                        {
                            float joint=primitive.Vertices[vertex*GltfPrimitive.Stride+8+lane],weight=primitive.Vertices[vertex*GltfPrimitive.Stride+12+lane];
                            if(joint<0||joint>=joints||joint!=MathF.Truncate(joint)||!float.IsFinite(weight)||weight<0)throw new InvalidDataException($"Invalid weighted bone index at mesh {instance.Mesh}, vertex {vertex}, lane {lane}.");
                        }
                    }
                }
                if(original.RepairedKeys>0||original.RepairedBindMatrices>0)evidence.Review.Add(new("repaired-original-data",$"Loader repairs {original.RepairedKeys} invalid keys and {original.RepairedBindMatrices} inverse bind matrices; repair fidelity needs rendered review."));
                if(evidence.ZeroScaleNodes>0)evidence.Review.Add(new("zero-scale-nodes",evidence.ZeroScaleNodes+" rest nodes have a zero scale axis; may be intentional hidden attachments."));
                evidence.Status=evidence.Failures.Count>0?"FAIL":"VERIFIED";
            }
            catch(Exception error)when(error is not OutOfMemoryException){evidence.Status="FAIL";evidence.Failures.Add(new("asset-load-validation",error.GetType().Name+": "+error.Message));}
            foreach(var (key,source) in catalog.Sources.Where(pair=>pair.Value.Asset==assetKey).OrderBy(pair=>pair.Key,StringComparer.Ordinal))
            {
                var row=new SourceEvidence{SourceKey=key,Asset=assetKey,Metadata=source.Metadata,MotionFile=source.Motion?.File,MotionSha256=source.Motion?.Sha256,Aliases=aliases.GetValueOrDefault(key)?.ToArray()??[],CompanionContexts=contexts.GetValueOrDefault(key)?.ToArray()??[],CompanionBindings=companionBindings.GetValueOrDefault(key)?.ToArray()??[]};sources.Add(row);
                if(evidence.Status=="FAIL"||original is null||mesh is null){row.Status=row.StructureStatus="FAIL";row.Failures.Add(new("asset-failed","See asset evidence."));continue;}
                try
                {
                    var rig=original.Prepare(source,content);ValidateRig(rig);
                    using var compressed=File.OpenRead(Path.Combine(content,source.Motion!.File));using var gzip=new GZipStream(compressed,CompressionMode.Decompress);using var prepared=JsonDocument.Parse(gzip);
                    row.PreparedParentChanges=rig.Nodes.Where((node,i)=>node.Parent!=original.Nodes[i].Parent).Count();row.PreparedZeroScaleNodes=rig.Nodes.Count(node=>ZeroScale(node.Rest));
                    var visibleNodes=VisibleAncestors(rig,mesh);
                    for(int i=0;i<rig.Animations.Length;i++)
                    {
                        var clip=rig.Animations[i];var definition=prepared.RootElement.GetProperty("clips")[i];bool authored=definition.TryGetProperty("authored",out var authoredIndex);
                        int changing=clip.Channels.Count(Changes);int[] targets=clip.Channels.Select(channel=>channel.Node).Distinct().ToArray();
                        row.Clips.Add(new(i,clip.Name,authored?"authored:"+authoredIndex.GetInt32():"generated-fallback",clip.Duration,clip.Channels.Length,clip.Channels.Sum(channel=>channel.Times.Length),targets,targets.Where(visibleNodes.Contains).ToArray(),changing,changing==0?"STATIC_KEYS":"CHANGING_KEYS"));
                        if(changing==0)row.Review.Add(new("static-clip",clip.Name+" has no varying key values; full-cycle pose/render evidence required."));
                        else if(!clip.Channels.Any(channel=>Changes(channel)&&visibleNodes.Contains(channel.Node)))row.Review.Add(new("unbound-animation",clip.Name+" varies only nodes outside active meshes and their bone ancestors."));
                    }
                    var idle=ModelAnimator.FindIdle(rig);
                    row.Roles.Add(new("idle",idle?.Name,"UNVERIFIED",idle is null?"No skeletal clip available for native idle selection; procedural transform or intentionally static actor requires separate context review.":"ModelAnimator.FindIdle resolves this clip; motion and appearance require runtime/rendered evidence."));
                    foreach(string role in new[]{"attack","special","win"})
                    {
                        var clip=rig.Animations.FirstOrDefault(animation=>animation.Name==role);
                        row.Roles.Add(new(role,clip?.Name,"UNVERIFIED",clip is null?"No exact native action clip; runtime may use source procedural/root-motion fallback.":"Exact native action name resolves; playback/fidelity requires runtime/rendered evidence."));
                    }
                    var walk=rig.Animations.FirstOrDefault(animation=>System.Text.RegularExpressions.Regex.IsMatch(animation.Name,"walk|run|move",System.Text.RegularExpressions.RegexOptions.IgnoreCase));
                    row.Roles.Add(new("walk",walk?.Name,"UNVERIFIED",walk is null?"No authored walk/run clip. Native combat movement uses presentation transform; companion movement requires its separate role mapping.":"Named movement clip exists; its production use is not assumed."));
                    if(idle is null)row.Review.Add(new("clipless-model",row.Aliases.Length>0?"Active battle model has no skeletal clips; inspect intended static or procedural behavior.":"No skeletal clips; inspect companion/procedural role mappings before judging motion."));
                    string metadata=source.Metadata.GetRawText();if(metadata.Contains("placeholder",StringComparison.OrdinalIgnoreCase)||metadata.Contains("fallback",StringComparison.OrdinalIgnoreCase)||metadata.Contains("missing",StringComparison.OrdinalIgnoreCase))row.Review.Add(new("placeholder-metadata","Metadata explicitly mentions fallback, placeholder or missing model; retained in audit."));
                    if(row.Aliases.Length==0&&row.CompanionContexts.Length==0)row.Review.Add(new("unmapped-shipped-source","Shipped source has no battle variant or companion context; retained in inventory."));
                    row.StructureStatus=row.Failures.Count>0?"FAIL":"VERIFIED";row.Status=row.Failures.Count>0?"FAIL":"UNVERIFIED";
                }
                catch(Exception error)when(error is not OutOfMemoryException){row.Status=row.StructureStatus="FAIL";row.Failures.Add(new("prepared-rig-validation",error.GetType().Name+": "+error.Message));}
            }
            if(++done%25==0)Console.WriteLine($"Model inventory: {done}/{catalog.Assets.Count} assets, {sources.Count}/{catalog.Sources.Count} mappings.");
        }
        foreach(var (key,source) in catalog.Sources.Where(pair=>!catalog.Assets.ContainsKey(pair.Value.Asset)))
            sources.Add(new(){SourceKey=key,Asset=source.Asset,Metadata=source.Metadata,MotionFile=source.Motion?.File,MotionSha256=source.Motion?.Sha256,Aliases=aliases.GetValueOrDefault(key)?.ToArray()??[],CompanionContexts=contexts.GetValueOrDefault(key)?.ToArray()??[],Status="FAIL",StructureStatus="FAIL",Failures={new("missing-asset","Source references an asset absent from the shipped catalog.")}});
        int failures=assets.Count(asset=>asset.Status=="FAIL")+sources.Count(source=>source.Status=="FAIL")+missingAliases.Count;
        var report=new{schemaVersion=1,generatedUtc=DateTimeOffset.UtcNow,codeIdentity,loadedAssetsAssemblySha256,catalogSha256=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(root,"content/models.json")))).ToLowerInvariant(),scope="All shipped model assets and source mappings; no exclusions. VERIFIED means only the named numerical/dependency check passed. Overall appearance remains UNVERIFIED until a rendered comparison is joined.",summary=new{assets=assets.Count,sources=sources.Count,variants=catalog.Variants.Count,failedAssets=assets.Count(asset=>asset.Status=="FAIL"),failedSources=sources.Count(source=>source.Status=="FAIL"),unresolvedAliases=missingAliases.Count,renderedVerified=0,runtimeVerified=0},missingAliases,assets,sources};
        File.WriteAllText(output,JsonSerializer.Serialize(report,JsonOptions));
        File.WriteAllLines(Path.ChangeExtension(output,"csv"),new[]{"sourceKey,asset,status,structureStatus,renderedStatus,runtimeStatus,clips,review"}.Concat(sources.Select(source=>string.Join(",",new[]{source.SourceKey,source.Asset,source.Status,source.StructureStatus,source.RenderedStatus,source.RuntimeStatus,string.Join(" | ",source.Clips.Select(clip=>clip.Name+":"+clip.MotionStatus)),string.Join(" | ",source.Review.Select(review=>review.Code))}.Select(value=>"\""+value.Replace("\"","\"\"")+"\"")))));
        Console.WriteLine($"Model inventory written: {assets.Count} assets / {sources.Count} mappings / {failures} structural failures; all rendered and runtime statuses remain UNVERIFIED. {output}");
        return failures;
    }
    private static bool ZeroScale(NodeTransform node)=>Math.Abs(node.Scale.X)<1e-12||Math.Abs(node.Scale.Y)<1e-12||Math.Abs(node.Scale.Z)<1e-12;
    private static bool Changes(RigChannel channel)=>channel.Values.Skip(1).Any(value=>(value-channel.Values[0]).LengthSquared()>1e-12f);
    private static HashSet<int> VisibleAncestors(GltfRig rig,GltfMesh mesh)
    {
        var nodes=new HashSet<int>();
        void Add(int node){while(node>=0&&nodes.Add(node))node=rig.Nodes[node].Parent;}
        foreach(var instance in mesh.Instances){Add(instance.Node);if(instance.Skin>=0)foreach(int joint in rig.Skins[instance.Skin].Joints)Add(joint);}
        return nodes;
    }
    private static void ValidateRig(GltfRig rig)
    {
        var pose=new RigPose(rig);
        for(int i=0;i<rig.Nodes.Length;i++)
        {
            var node=rig.Nodes[i];var q=node.Rest.Rotation;
            if(!node.Rest.Matrix.IsFinite||!pose.World[i].IsFinite||q.X*q.X+q.Y*q.Y+q.Z*q.Z+q.W*q.W<1e-12)throw new InvalidDataException("Invalid rest transform: "+i+" "+node.Name);
        }
        foreach(var skin in rig.Skins)
        {
            if(skin.Joints.Length!=skin.InverseBindMatrices.Length||skin.Joints.Any(joint=>joint<0||joint>=rig.Nodes.Length)||skin.InverseBindMatrices.Any(matrix=>!matrix.IsFinite))throw new InvalidDataException("Invalid skin joint or inverse-bind mapping.");
        }
        foreach(var clip in rig.Animations)
        {
            if(!double.IsFinite(clip.Duration)||clip.Duration<0)throw new InvalidDataException("Invalid clip duration: "+clip.Name);
            foreach(var channel in clip.Channels)
            {
                if(channel.Node<0||channel.Node>=rig.Nodes.Length||channel.Times.Length==0||channel.Times.Length!=channel.Values.Length)throw new InvalidDataException("Invalid animation bone/key mapping: "+clip.Name);
                for(int i=0;i<channel.Times.Length;i++)
                {
                    double time=channel.Times[i];var value=channel.Values[i];
                    if(!double.IsFinite(time)||time<0||i>0&&time<channel.Times[i-1]||!float.IsFinite(value.X)||!float.IsFinite(value.Y)||!float.IsFinite(value.Z)||!float.IsFinite(value.W)||channel.Path==AnimationPath.Rotation&&value.LengthSquared()<1e-12f)throw new InvalidDataException("Invalid animation key: "+clip.Name+" / "+i);
                }
            }
        }
    }
}
