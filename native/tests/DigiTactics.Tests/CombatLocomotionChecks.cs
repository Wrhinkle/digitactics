using DigiTactics.Assets;

internal static class CombatLocomotionChecks
{
    public static int Run(string root)
    {
        var catalog=ModelCatalog.Load(Path.Combine(root,"content/models.json"));
        string assets=Path.Combine(root,"artifacts/model-content");int count=0;
        void Check(bool condition,string message){count++;if(!condition)throw new InvalidOperationException("Combat locomotion: "+message);}
        GltfRig Load(string key){var source=catalog.Sources[key];using var asset=catalog.Open(assets,source.Asset);return GltfRig.Load(asset).Prepare(source,assets);}
        foreach(var (key,leftName,rightName) in new[]{("agumon:2","J_kakato_L","J_kakato_R"),("palmon:0","J_kakato_L","J_kakato_R")})
        {
            var rig=Load(key);var still=new ModelAnimator(rig);var moving=new CombatModelAnimator(rig,key);
            int left=Array.FindIndex(rig.Nodes,node=>node.Name==leftName),right=Array.FindIndex(rig.Nodes,node=>node.Name==rightName);
            Check(left>=0&&right>=0,key+" has paired feet");
            static double Distance(Matrix4D a,Matrix4D b)=>Math.Sqrt(Math.Pow(a.M41-b.M41,2)+Math.Pow(a.M42-b.M42,2)+Math.Pow(a.M43-b.M43,2));
            for(int frame=0;frame<17;frame++){still.Update(1d/60);moving.Update(1d/60,true);}
            double width=Distance(still.Pose.World[left],still.Pose.World[right]);
            Check(Distance(still.Pose.World[left],moving.Pose.World[left])>width*.02&&Distance(still.Pose.World[right],moving.Pose.World[right])>width*.02,key+" moving articulates both feet instead of sliding the idle pose");
            var paused=moving.Pose.Local.ToArray();moving.Update(0,true);Check(paused.SequenceEqual(moving.Pose.Local),key+" zero delta freezes the movement pose");
            for(int frame=0;frame<30;frame++){still.Update(1d/60);moving.Update(1d/60,false);}
            Check(still.Pose.Local.SequenceEqual(moving.Pose.Local),key+" arrival restores the exact current idle pose");
            for(int frame=0;frame<17;frame++){still.Update(1d/60);moving.Update(1d/60,true);}
            foreach(string role in new[]{"attack","special","win"})
            {
                Check(still.Play(role)&&moving.Play(role),key+" accepts "+role+" during travel");
                for(int frame=0;frame<25;frame++){still.Update(1d/60);moving.Update(1d/60,true);Check(still.Pose.Local.SequenceEqual(moving.Pose.Local),key+" authored "+role+" has priority over gait");}
            }
        }
        foreach(string key in new[]{"hagurumon:0","ac:Flymon"})
        {
            var rig=Load(key);var still=new ModelAnimator(rig);var moving=new CombatModelAnimator(rig,key);
            for(int frame=0;frame<17;frame++){still.Update(1d/60);moving.Update(1d/60,true);}
            Check(moving.LocomotionKind==CompanionLocomotionKind.Glide&&!moving.UsesTravelHop,key+" uses its explicit airborne classification");
            Check(still.Pose.Local.SequenceEqual(moving.Pose.Local),key+" does not receive a grounded gait");
            Check(Math.Abs(moving.GlideOffset) is >.005 and <=.025,key+" has bounded visible flight motion");
            for(int frame=0;frame<30;frame++)moving.Update(1d/60,false);
            Check(moving.GlideOffset==0,key+" arrival clears flight offset");
        }
        int authoredRuns=0;
        foreach(string key in catalog.Sources.Keys.Where(key=>key.StartsWith("legend:",StringComparison.Ordinal)))
        {
            var rig=Load(key);var run=new[]{"run","walk","move"}.Select(name=>rig.Animations.FirstOrDefault(clip=>clip.Name==name)).FirstOrDefault(clip=>clip is not null);
            if(run is null)continue;authoredRuns++;
            var moving=new CombatModelAnimator(rig,key);var raw=new RigPose(rig);var idle=new ModelAnimator(rig);double time=0;
            for(int frame=0;frame<24;frame++){time+=1d/60;moving.Update(1d/60,true);idle.Update(1d/60);}
            raw.Sample(run,time,loop:true);
            Check(moving.LocomotionKind==CompanionLocomotionKind.Authored&&moving.Pose.Local.SequenceEqual(raw.Local),key+" preserves the exact authored run pose after fade-in");
            for(int frame=0;frame<30;frame++){moving.Update(1d/60,false);idle.Update(1d/60);}
            Check(moving.Pose.Local.SequenceEqual(idle.Pose.Local),key+" authored run fades back to the current idle pose");
        }
        Check(authoredRuns>=31,"all31 authored run fixtures were exercised");
        Console.WriteLine($"Combat locomotion: {count} movement, arrival, pause and action-priority checks passed.");
        return count + GaitAnatomyChecks.Run(root);
    }
}
