using System.Text.Json;
using DigiTactics.Assets;

internal static class ModelRuntimeProbe
{
    public static void Run(string root,string key,string role,double time)
    {
        var catalog=ModelCatalog.Load(Path.Combine(root,"content/models.json"));var source=catalog.Sources[key];string assets=Path.Combine(root,"artifacts/model-content");
        using var asset=catalog.Open(assets,source.Asset);var rig=GltfRig.Load(asset).Prepare(source,assets);var mesh=GltfMesh.Load(asset);
        var idle=ModelAnimator.FindIdle(rig);var baseline=new RigPose(rig);baseline.Sample(idle,0);var bounds=mesh.Bounds(rig,baseline,false);double h=bounds.Max.Y-bounds.Min.Y;
        var clip=rig.Animations.Single(a=>a.Name==role);var pose=new RigPose(rig);pose.Sample(clip,time);
        double[] V(Vector3D v)=>[v.X/h,v.Y/h,v.Z/h];
        var instances=mesh.Instances.Select(instance=>
        {
            var single=new GltfMesh(mesh.Meshes,[instance]);var before=single.Bounds(rig,baseline);var after=single.Bounds(rig,pose);
            return new{node=instance.Node,name=rig.Nodes[instance.Node].Name,instance.Skin,instance.Mesh,materials=mesh.Meshes[instance.Mesh].Select(p=>p.Material),beforeMin=V(before.Min),beforeMax=V(before.Max),afterMin=V(after.Min),afterMax=V(after.Max)};
        });
        var changed=clip.Channels.Select(channel=>channel.Node).Distinct().Select(node=>new{node,name=rig.Nodes[node].Name,parent=rig.Nodes[node].Parent,before=baseline.Local[node],after=pose.Local[node],beforeWorld=V(new(baseline.World[node].M41,baseline.World[node].M42,baseline.World[node].M43)),afterWorld=V(new(pose.World[node].M41,pose.World[node].M42,pose.World[node].M43))});
        var result=new{key,source.Asset,role,time,idle=idle?.Name,baseHeight=h,instances,changed};
        string file=Path.Combine(root,"artifacts/model-probe-"+string.Concat(key.Select(c=>char.IsAsciiLetterOrDigit(c)?c:'_'))+".json");
        File.WriteAllText(file,JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine(file);
    }
}
