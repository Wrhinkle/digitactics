using System.Numerics;

namespace DigiTactics.Presentation;

/// <summary>Visual-only source move timeline. Pools survive casts, and simulation state is never modified.</summary>
public sealed class MoveEffects
{
    private sealed class Task
    {
        public double Start,Duration;
        public Action<double,double>? Frame;
        public Action? End;
    }
    private readonly List<Task> tasks=[];
    private readonly Stack<Task> freeTasks=[];
    private readonly Dictionary<string,Stack<MoveMesh>> freeMeshes=new(StringComparer.Ordinal);
    private readonly List<MoveMesh> meshes=[];
    private readonly List<MoveRibbon> ribbons=[];
    private readonly List<MovePiece> pieces=new(2048);
    private readonly Dictionary<string,int> pieceCounts=new(StringComparer.Ordinal);
    private uint randomState;
    public double Now {get;private set;}
    public MoveVector CameraPosition {get;}=new(0,8,12);
    public Action<double,double>? Shake {get;set;}
    public MoveParticles Add {get;}
    public MoveParticles Alpha {get;}
    public IReadOnlyList<MoveMesh> Meshes=>meshes;
    public IReadOnlyList<MoveRibbon> Ribbons=>ribbons;
    public IReadOnlyList<MovePiece> Pieces=>pieces;
    public int TaskCount=>tasks.Count;
    public MoveEffects(uint seed=84219) {randomState=seed;Add=new(this,9000);Alpha=new(this,2500);}
    public double Random()
    {
        unchecked { uint t=randomState+=0x6D2B79F5;t=(t^(t>>15))*(t|1);t^=t+(t^(t>>7))*(t|61);return (t^(t>>14))/4294967296.0; }
    }
    public static int Round(double n)=>(int)Math.Floor(n+.5);
    public void Run(double delay,double duration,Action<double,double>? frame,Action? end=null)
    {
        var task=freeTasks.TryPop(out var free)?free:new();task.Start=Now+Math.Max(0,delay);task.Duration=Math.Max(0,duration);task.Frame=frame;task.End=end;tasks.Add(task);
    }
    public void Run(double delay,double duration,Action<double> frame,Action? end=null)=>Run(delay,duration,(k,_)=>frame(k),end);
    public void Later(double delay,Action callback)=>Run(delay,0,(Action<double,double>?)null,callback);
    public void Update(double dt)
    {
        if(!double.IsFinite(dt)||dt<0)throw new ArgumentOutOfRangeException(nameof(dt));
        Now+=dt;pieces.Clear();pieceCounts.Clear();int write=0;
        // Newly scheduled immediate work is processed in this same frame, as in MoveFx.step.
        for(int read=0;read<tasks.Count;read++)
        {
            var task=tasks[read];if(Now<task.Start){tasks[write++]=task;continue;}
            double k=task.Duration>0?Math.Min(1,(Now-task.Start)/task.Duration):1;
            task.Frame?.Invoke(k,dt);
            if(k>=1){task.End?.Invoke();task.Frame=null;task.End=null;freeTasks.Push(task);}else tasks[write++]=task;
        }
        if(write<tasks.Count)tasks.RemoveRange(write,tasks.Count-write);
        Add.Update(dt);Alpha.Update(dt);
    }
    public void Clear()
    {
        // Release retained objects without running scheduled impacts or creating new work.
        foreach(var task in tasks){task.Frame=null;task.End=null;freeTasks.Push(task);}tasks.Clear();
        foreach(var mesh in meshes)if(mesh.Visible)Free(mesh);foreach(var ribbon in ribbons)ribbon.Visible=false;
        Add.Clear();Alpha.Clear();pieces.Clear();pieceCounts.Clear();
    }
    public MoveMesh Mesh(string kind)
    {
        if(!freeMeshes.TryGetValue(kind,out var pool))freeMeshes.Add(kind,pool=[]);
        if(!pool.TryPop(out var mesh)){mesh=new(){Kind=kind};meshes.Add(mesh);}mesh.Reset();return mesh;
    }
    public void Free(MoveMesh? mesh){if(mesh is null||!mesh.Visible)return;mesh.Visible=false;freeMeshes[mesh.Kind].Push(mesh);}
    public MoveRibbon Ribbon(){var ribbon=ribbons.FirstOrDefault(r=>!r.Visible);if(ribbon is null){ribbon=new();ribbons.Add(ribbon);}ribbon.Visible=true;ribbon.Count=0;ribbon.Taper=1;return ribbon;}
    public void FreeRibbon(MoveRibbon? ribbon){if(ribbon is not null)ribbon.Visible=false;}
    private void Piece(MovePiece piece)
    {
        int count=pieceCounts.GetValueOrDefault(piece.Kind),max=piece.Kind switch{"glow"=>768,"claw" or "pillar"=>128,_=>256};
        if(count>=max)return;pieceCounts[piece.Kind]=count+1;pieces.Add(piece);
    }
    public void Quad(string kind,double x,double y,double z,double sx,double sy,MoveColor color,double alpha,double rot=0,double? face=0)
    {if(kind=="glow")alpha*=.75;if(alpha>.002)Piece(new(kind,x,y,z,sx,sy,1,color,alpha,rot,face));}
    public void Ball(double x,double y,double z,double r,MoveColor color,double alpha=1){if(alpha*.8>.002)Piece(new("ball",x,y,z,r,r,r,color,alpha*.8,0,null));}
    public void Spike(string kind,double x,double z,double r,double h,double rx,double ry,double rz)
    {if(h>.01)Piece(new("spike_"+kind,x,0,z,r,h,r,MoveColor.White,1,0,null,rx,ry,rz));}
    public void Burst(MoveParticles sys,double x,double y,double z,double n,double speed,MoveColor ca,MoveColor cb,double life,double size,string cell,MovePreset? pre=null,double up=0,double alpha=1)
    {
        for(int i=0;i<n;i++){double th=Random()*6.283,ph=Math.Acos(Rnd(-1,1)),s=speed*Rnd(.35,1);sys.Add(x,y,z,Math.Sin(ph)*Math.Cos(th)*s,Math.Abs(Math.Cos(ph))*s*.7+up,Math.Sin(ph)*Math.Sin(th)*s,Random()<.5?ca:cb,life*Rnd(.6,1.2),size*Rnd(.7,1.3),cell,pre??MovePreset.Drift,alpha);}
    }
    public void RingBurst(MoveParticles sys,double x,double y,double z,double n,double speed,MoveColor color,double life,double size,string cell,MovePreset? pre=null)
    {for(int i=0;i<n;i++){double th=i/n*6.283+Rnd(-.1,.1);sys.Add(x,y,z,Math.Cos(th)*speed,Rnd(0,.4),Math.Sin(th)*speed,color,life,size,cell,pre??MovePreset.Drift);}}
    public void Flash(double delay,double x,double y,double z,MoveColor color,double size,double dur=.25)=>Run(delay,dur,k=>{double s=size*(.5+k*.8);Quad("glow",x,y,z,s,s,color,.8*(1-k));});
    public void GroundRing(double delay,double x,double z,MoveColor color,double r,double dur=.5,string kind="shock",double y=.16)
    {string b=kind=="ring"?"ring":"shock";Run(delay,dur,k=>{double e=1-(1-k)*(1-k),s=.15+e*r;Quad(b,x,y,z,s,s,color,(b=="ring"?.6:.55)*(1-k),0,1);});}
    public void Pillar(double delay,double x,double z,MoveColor color,double r,double h,double dur=.6)=>Run(delay,dur,k=>{double w=r*(k<.15?k/.15:1-(k-.15)*.7);Piece(new("pillar",x,0,z,w,h*(k<.15?.5+k/.3:1),w,color,(1-k)*.5,0,null));});
    public void Bolt(double delay,double ax,double ay,double az,double bx,double by,double bz,MoveColor color,double dur=.25,double width=.08,double jag=.35,int segs=10)
    {
        MoveRibbon? ribbon=null;double next=0;
        Run(delay,dur,k=>{if(ribbon is null){ribbon=Ribbon();ribbon.Color=color;ribbon.Taper=.4;}
            if(k>=next){next=k+.3;ribbon.Count=Math.Min(24,segs+1);for(int i=0;i<ribbon.Count;i++){double t=(double)i/(ribbon.Count-1),j=i==0||i==ribbon.Count-1?0:jag;ribbon.Points[i].Set(ax+(bx-ax)*t+Rnd(-j,j),ay+(by-ay)*t+Rnd(-j,j)*.6,az+(bz-az)*t+Rnd(-j,j));}}
            ribbon.Width=width*(k<.2?1.6:1);ribbon.Build(k<.15?1.6:1-k*.8);},()=>FreeRibbon(ribbon));
    }
    public void Fly(double delay,double dur,double ax,double ay,double az,double bx,double by,double bz,double arc,Action<double,double,double,double,double> drawHead,Action? onHit=null,double side=0)
    {double dx=bx-ax,dz=bz-az,len=Math.Sqrt(dx*dx+dz*dz);if(len==0)len=1;double px=-dz/len,pz=dx/len;Run(delay,dur,(k,dt)=>{double s=Math.Sin(Math.PI*k);drawHead(ax+dx*k+px*side*s,ay+(by-ay)*k+arc*s,az+dz*k+pz*side*s,k,dt);},onHit);}
    private double Rnd(double a,double b)=>a+Random()*(b-a);
}

public struct MoveParticle
{
    public float X,Y,Z,Vx,Vy,Vz,Life,MaxLife,Alpha0,Size0,Size,Alpha,Rotation,Spin,Gravity,Drag,Grow;
    public MoveColor Color;
    public string Cell;
    public bool Aligned;
}
public sealed class MoveParticles
{
    private readonly MoveEffects fx;
    private readonly MoveParticle[] data;
    public int Count {get;private set;}
    public ReadOnlySpan<MoveParticle> Items=>data.AsSpan(0,Count);
    public MoveParticles(MoveEffects fx,int capacity){this.fx=fx;data=new MoveParticle[capacity];}
    public void Add(double x,double y,double z,double vx,double vy,double vz,MoveColor color,double life,double size,string cell,MovePreset? pre=null,double alpha=1,double? rot=null)
    {
        // Default parameter evaluation occurs before the source capacity guard.
        double rotation=rot??fx.Random()*6.283;if(Count>=data.Length)return;pre??=MovePreset.None;
        data[Count++]=new(){X=(float)x,Y=(float)y,Z=(float)z,Vx=(float)vx,Vy=(float)vy,Vz=(float)vz,Color=color,Life=(float)life,MaxLife=(float)life,Alpha0=(float)alpha,Size=(float)size,Size0=(float)size,Cell=cell,Grow=(float)pre.Grow,Spin=(float)(pre.Spin*(fx.Random()<.5?-1:1)),Gravity=(float)pre.Gravity,Drag=(float)pre.Drag,Aligned=cell is "streak" or "needle" or "blade" or "feather" or "bolt" or "fish" or "seed",Rotation=(float)rotation};
    }
    public void Update(double dt)
    {
        int write=0;for(int i=0;i<Count;i++){var p=data[i];double life=p.Life-dt;if(life<=0)continue;double drag=Math.Max(0,1-p.Drag*dt),vx=p.Vx*drag,vy=p.Vy*drag-p.Gravity*dt,vz=p.Vz*drag;
            p.X=(float)(p.X+vx*dt);p.Y=(float)Math.Max(.02,p.Y+vy*dt);p.Z=(float)(p.Z+vz*dt);p.Vx=(float)vx;p.Vy=(float)vy;p.Vz=(float)vz;
            double f=life/p.MaxLife,fade=f>.85?(1-f)/.15:f<.5?f/.5:1;p.Alpha=(float)(p.Alpha0*fade);p.Size=(float)(p.Size0*(1+(p.Grow-1)*(1-f)));if(!p.Aligned)p.Rotation+=(float)(p.Spin*dt);p.Life=(float)life;data[write++]=p;
        }Count=write;
    }
    public void Clear()=>Count=0;
    public void Align(Matrix4x4 camera,double aspect)
    {
        for(int i=0;i<Count;i++)
        {
            ref var p=ref data[i];if(!p.Aligned)continue;
            var from=Vector4.Transform(new Vector4(p.X,p.Y,p.Z,1),camera);
            var to=Vector4.Transform(new Vector4(p.X+p.Vx*.05f,p.Y+p.Vy*.05f,p.Z+p.Vz*.05f,1),camera);
            double x=to.X/to.W-from.X/from.W,y=to.Y/to.W-from.Y/from.W;
            if(x*x+y*y>1e-10)p.Rotation=(float)Math.Atan2(y,x*aspect);
        }
    }
}
