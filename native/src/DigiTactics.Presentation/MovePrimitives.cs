namespace DigiTactics.Presentation;

/// <summary>Source delivery composition. All random choices belong to the visual engine.</summary>
public static partial class MovePrimitives
{
    public const double Tau=Math.PI*2;
    internal static readonly MoveColor White=MoveColor.White,Grey=HexColor("#8a8f99"),Rock=HexColor("#8a7458"),Rock2=HexColor("#5e5040"),Smoke=HexColor("#6a6a72"),DrainA=HexColor("#ff3a6a"),DrainB=HexColor("#ffb3c6"),Stun=HexColor("#ffe94a");
    public static readonly string[] Names=["orb","breath","beam","missiles","bolt","slash","dash","erupt","meteor","sphere","wave","arrows","cross","vortex","portal","spikes","whip","swarm","shock","dome","heal","tornado","rings","volley","streak","wheel","net","pillar","rain","glyph","drain"];
    public static void Play(string name,MoveEffects fx,MoveContext c,MoveOptions o,double delay)
    {
        switch(name){case "orb":Orb(fx,c,o,delay);break;case "breath":Breath(fx,c,o,delay);break;case "beam":Beam(fx,c,o,delay);break;case "missiles":Missiles(fx,c,o,delay);break;case "bolt":Bolt(fx,c,o,delay);break;case "slash":Slash(fx,c,o,delay);break;case "dash":Dash(fx,c,o,delay);break;case "erupt":Erupt(fx,c,o,delay);break;case "meteor":Meteor(fx,c,o,delay);break;case "sphere":Sphere(fx,c,o,delay);break;case "wave":Wave(fx,c,o,delay);break;case "arrows":Arrows(fx,c,o,delay);break;case "cross":Cross(fx,c,o,delay);break;case "vortex":Vortex(fx,c,o,delay);break;case "portal":Portal(fx,c,o,delay);break;case "spikes":Spikes(fx,c,o,delay);break;case "whip":Whip(fx,c,o,delay);break;case "swarm":Swarm(fx,c,o,delay);break;case "shock":Shock(fx,c,o,delay);break;case "dome":Dome(fx,c,o,delay);break;case "heal":Heal(fx,c,o,delay);break;case "tornado":Tornado(fx,c,o,delay);break;case "rings":Rings(fx,c,o,delay);break;case "volley":Volley(fx,c,o,delay);break;case "streak":Streak(fx,c,o,delay);break;case "wheel":Wheel(fx,c,o,delay);break;case "net":Net(fx,c,o,delay);break;case "pillar":Pillar(fx,c,o,delay);break;case "rain":Rain(fx,c,o,delay);break;case "glyph":Glyph(fx,c,o,delay);break;case "drain":Drain(fx,c,o,delay);break;default:throw new InvalidDataException("Unknown move primitive: "+name);}
    }
    internal static double Rnd(MoveEffects fx,double a,double b)=>a+fx.Random()*(b-a);
    internal static T Pick<T>(MoveEffects fx,T a,T b)=>fx.Random()<.5?a:b;
    internal static double Dist(MoveVector a,MoveVector b)=>Math.Sqrt((b.X-a.X)*(b.X-a.X)+(b.Z-a.Z)*(b.Z-a.Z));
    internal static MoveColor HexColor(string hex)=>MoveColor.Hex(hex);
    internal static MoveColor Col(MoveContext c,string? value,MoveColor? fallback=null)=>value switch {null=>fallback??c.Ca,"a"=>c.Ca,"b"=>c.Cb,"w"=>White,_=>HexColor(value)};
    internal static readonly HashSet<string> Cells=["glow","spark","flame","smoke","bubble","feather","note","card","leaf","petal","shard","streak","cross","gear","bat","fish","square","needle","star","ring","seed","blade","bolt","paw","drop","snow","skull","heart","hex","dot","ember","wisp"];
    internal static string CellOf(string? name,string fallback="glow")=>name is not null&&Cells.Contains(name)?name:fallback;
    internal static MoveParticles SysFor(MoveEffects fx,string cell)=>cell is "card" or "leaf" or "smoke"?fx.Alpha:fx.Add;
    internal static MoveVector[] Spots(MoveContext c,MoveOptions o)
    {string at=o.String("to","targets")!;if(at=="self")return [new(c.P.X,.8,c.P.Z)];return at=="center"||c.T.Length==0?[new(c.C.X,.8,c.C.Z)]:c.T;}
    internal static MoveVector Launch(MoveEffects fx,MoveContext c,string? from,int i=0)
    {var a=c.A;double side=i%2!=0?1:-1,px=Math.Cos(c.Yaw),pz=-Math.Sin(c.Yaw);return from switch{
        "chest"=>new(a.X+px*side*.25*c.S,a.Y*.75,a.Z+pz*side*.25*c.S),
        "back"=>new(a.X+px*side*.4*c.S-Math.Sin(c.Yaw)*.3,a.Y+.5*c.S,a.Z+pz*side*.4*c.S-Math.Cos(c.Yaw)*.3),
        "hands"=>new(a.X+px*side*.35*c.S,a.Y*.85,a.Z+pz*side*.35*c.S),
        "sky"=>new(a.X+Rnd(fx,-.8,.8),a.Y+1.6+Rnd(fx,0,.8),a.Z+Rnd(fx,-.8,.8)),_=>a};}
    internal static void Impact(MoveEffects fx,MoveContext c,string? kind,double x,double z,double d,double s=1,double y=.8)
    {fx.Later(d,()=>{ImpactNow(fx,kind??c.Spec.Impact,x,y,z,s*c.S,c.Ca,c.Cb);if(!c.Shook&&c.Spec.Shake!=0){c.Shook=true;fx.Shake?.Invoke(c.Spec.Shake*(.7+c.Stage*.3),.3+c.Spec.Shake);}if(c.Type=="stun")Dizzy(fx,x,z,c.StunTime);if(c.Type=="drain")Flow(fx,x,z,c.P.X,c.P.Z);});}
    public static void ImpactNow(MoveEffects fx,string kind,double x,double y,double z,double s,MoveColor ca,MoveColor cb)
    {
        switch(kind)
        {
            case "none":return;
            case "blast":
                fx.Flash(0,x,y,z,cb,1.2*s,.22);fx.Flash(0,x,y,z,ca,2.4*s,.4);
                fx.Burst(fx.Add,x,y,z,26*s,3.2*s,ca,cb,.55,.5*s,"flame",MovePreset.Flame,1.2);
                fx.Burst(fx.Add,x,y,z,14*s,6*s,cb,White,.4,.14,"ember",MovePreset.Spark,2);
                fx.Burst(fx.Alpha,x,y,z,6*s,1.2,Smoke,Smoke,1,.7*s,"smoke",MovePreset.Smoke,.8,.45);fx.GroundRing(0,x,z,ca,1.6*s,.45);break;
            case "frost":
                fx.Flash(0,x,y,z,ca,2*s,.3);fx.Burst(fx.Add,x,y,z,18*s,4.5*s,ca,cb,.5,.3*s,"shard",MovePreset.Spark,1);fx.Burst(fx.Add,x,y,z,14*s,2*s,White,cb,.9,.22,"snow",MovePreset.Flutter,.8);
                for(int i=0;i<3;i++)SpikeAt(fx,.02*i,x+Rnd(fx,-.35,.35),z+Rnd(fx,-.35,.35),.16*s,(.6+fx.Random()*.5)*s,"ice",.7,Rnd(fx,-.4,.4));fx.GroundRing(0,x,z,cb,1.4*s,.45);break;
            case "zap":
                fx.Flash(0,x,y,z,cb,2*s,.2);fx.Burst(fx.Add,x,y,z,22*s,6*s,ca,cb,.35,.28,"spark",MovePreset.Spark,1.5);
                for(int i=0;i<2;i++){double a=fx.Random()*Tau,r=.9*s;fx.Bolt(i*.04,x,y,z,x+Math.Cos(a)*r,.15,z+Math.Sin(a)*r,ca,.18,.05,.18,5);}fx.GroundRing(0,x,z,ca,1.2*s,.35,"ring");break;
            case "splash":
                fx.Flash(0,x,y,z,cb,1.6*s,.22);fx.Burst(fx.Add,x,y,z,22*s,4*s,ca,cb,.6,.22*s,"drop",MovePreset.Debris,2.5);fx.Burst(fx.Add,x,y,z,10*s,1.5,cb,White,.8,.25*s,"bubble",MovePreset.Float,.5);fx.GroundRing(0,x,z,ca,1.5*s,.5);break;
            case "holy":
                fx.Flash(0,x,y,z,White,2.2*s,.3);fx.Pillar(0,x,z,ca,.45*s,5,.5);fx.Burst(fx.Add,x,.3,z,20*s,1.6,ca,White,1,.3,"star",MovePreset.Float,.6);fx.Burst(fx.Add,x,y,z,14,4*s,cb,White,.4,.3,"spark",MovePreset.Drift);fx.GroundRing(0,x,z,ca,1.5*s,.5,"ring");break;
            case "dark":
                for(int i=0;i<18*s;i++){double a=fx.Random()*Tau,r=Rnd(fx,.8,1.4)*s;fx.Add.Add(x+Math.Cos(a)*r,y+Rnd(fx,-.4,.5),z+Math.Sin(a)*r,-Math.Cos(a)*r*5,0,-Math.Sin(a)*r*5,Pick(fx,ca,cb),.2,.35,"wisp",MovePreset.None);}
                fx.Later(.16,()=>{fx.Flash(0,x,y,z,ca,2.4*s,.35);fx.Burst(fx.Add,x,y,z,22*s,3.5*s,ca,cb,.6,.45*s,"wisp",MovePreset.Smoke,1);fx.GroundRing(0,x,z,cb,1.6*s,.5);});break;
            case "poison":
                fx.Flash(0,x,y,z,ca,1.5*s,.25);fx.Burst(fx.Add,x,y,z,14*s,1.5,ca,cb,.9,.28*s,"bubble",MovePreset.Float,.8);fx.Burst(fx.Alpha,x,y,z,6*s,1,ca,ca,.9,.6*s,"smoke",MovePreset.Smoke,.4,.4);break;
            case "leaf":
                fx.Flash(0,x,y,z,cb,1.4*s,.22);fx.Burst(fx.Alpha,x,y,z,14*s,3.5*s,ca,cb,.9,.3*s,"leaf",MovePreset.Flutter,1.2);fx.Burst(fx.Add,x,y,z,8,2.5,cb,White,.5,.25,"petal",MovePreset.Flutter,1);break;
            case "metal":
                fx.Flash(0,x,y,z,cb,1.5*s,.18);fx.Burst(fx.Add,x,y,z,16*s,7*s,ca,White,.35,.3,"streak",MovePreset.Spark,2);fx.Burst(fx.Alpha,x,y,z,4,1,Smoke,Smoke,.7,.5*s,"smoke",MovePreset.Smoke,.5,.4);break;
            case "wind":
                fx.Flash(0,x,y,z,cb,1.3*s,.2);for(int i=0;i<16*s;i++){double a=fx.Random()*Tau;fx.Add.Add(x+Math.Cos(a)*.4,y+Rnd(fx,-.5,.6),z+Math.Sin(a)*.4,-Math.Sin(a)*5,Rnd(fx,.5,2),Math.Cos(a)*5,Pick(fx,ca,White),.35,.4,"streak",MovePreset.Drift);}fx.GroundRing(0,x,z,ca,1.3*s,.4,"ring");break;
            default:
                fx.Flash(0,x,y,z,White,1.4*s,.16);fx.Burst(fx.Add,x,y,z,12*s,6*s,ca,cb,.3,.32,"streak",MovePreset.Drift,.5);fx.Add.Add(x,y,z,0,0,0,ca,.22,1.1*s,"ring",MovePreset.Pop);break;
        }
    }
    private static void Dizzy(MoveEffects fx,double x,double z,double dur)=>fx.Run(0,dur,k=>{double t=fx.Now*7;for(int i=0;i<3;i++){double a=t+i/3.0*Tau;fx.Add.Add(x+Math.Cos(a)*.4,2,z+Math.Sin(a)*.4,0,0,0,Stun,.05,.22,"star",MovePreset.None,1,0);}});
    internal static void Flow(MoveEffects fx,double x,double z,double tx,double tz)
    {double dx=tx-x,dz=tz-z,d=Math.Sqrt(dx*dx+dz*dz);if(d==0)d=1;for(int i=0;i<16;i++){double s=Rnd(fx,3,4.5);fx.Add.Add(x+Rnd(fx,-.2,.2),Rnd(fx,.6,1.1),z+Rnd(fx,-.2,.2),dx/d*s,Rnd(fx,0,.6),dz/d*s,Pick(fx,DrainA,DrainB),Math.Min(.9,d/s),.3,"heart",MovePreset.None);}}
    internal static void SpikeAt(MoveEffects fx,double d,double x,double z,double r,double h,string kind="ice",double hold=.6,double tilt=0)
    {double duration=.14+hold+.25,ry=fx.Random()*Tau;fx.Run(d,duration,k=>{double t=k*duration,g=t<.14?t/.14:t<.14+hold?1:1-(t-.14-hold)/.25;fx.Spike(kind,x,z,r*(.6+.4*g),h*g,tilt,ry,tilt*.5);});}
    internal sealed record MoveHead(Action<double,double,double> Draw,Action Done);
    internal static MoveHead MakeHead(MoveEffects fx,MoveOptions o,double s,MoveColor ca,MoveColor cb,MoveContext c)
    {string? cell=o.String("head");return new((x,y,z)=>{double r=o.Number("size",.25)*s;if(cell is not null){var sys=SysFor(fx,CellOf(cell));sys.Add(x,y,z,0,0,0,o.Has("headColor")?Col(c,o.String("headColor")):sys==fx.Alpha?White:cb,.05,r*2.2,CellOf(cell),MovePreset.None,1,o.Flag("spin")?fx.Now*o.Value("spin"):0);if(!o.IsFalse("halo"))fx.Quad("glow",x,y,z,r*3,r*3,ca,.6);}else{fx.Ball(x,y,z,r*.55,cb,.9);fx.Quad("glow",x,y,z,r*4,r*4,ca,.85,fx.Now*3);}},()=>{});}
    internal static void EmitTrail(MoveEffects fx,string kind,double x,double y,double z,double dt,double s,MoveColor ca=default,MoveColor cb=default)
    {
        int n=Math.Max(1,MoveEffects.Round(dt*60));
        switch(kind)
        {
            case "flame":for(int i=0;i<n*2;i++)fx.Add.Add(x+Rnd(fx,-.08,.08)*s,y+Rnd(fx,-.08,.08)*s,z+Rnd(fx,-.08,.08)*s,Rnd(fx,-.4,.4),Rnd(fx,0,.6),Rnd(fx,-.4,.4),fx.Random()<.75?ca:cb,Rnd(fx,.25,.4),.38*s,"flame",MovePreset.FireTrail,.55);break;
            case "smoke":for(int i=0;i<n;i++)fx.Alpha.Add(x,y,z,Rnd(fx,-.3,.3),Rnd(fx,0,.4),Rnd(fx,-.3,.3),Smoke,Rnd(fx,.5,.8),.32*s,"smoke",MovePreset.Smoke,.5);break;
            case "spark":for(int i=0;i<n;i++)fx.Add.Add(x,y,z,Rnd(fx,-1.5,1.5),Rnd(fx,-1.5,1.5),Rnd(fx,-1.5,1.5),Pick(fx,ca,cb),.22,.2*s,"spark",MovePreset.Drift);break;
            case "bubble":for(int i=0;i<n;i++)fx.Add.Add(x+Rnd(fx,-.1,.1),y,z+Rnd(fx,-.1,.1),Rnd(fx,-.3,.3),Rnd(fx,.2,.8),Rnd(fx,-.3,.3),Pick(fx,ca,cb),.5,.14*s,"bubble",MovePreset.Float);break;
            case "leaf":for(int i=0;i<n;i++)fx.Alpha.Add(x,y,z,Rnd(fx,-.8,.8),Rnd(fx,-.2,.8),Rnd(fx,-.8,.8),Pick(fx,ca,cb),.6,.22*s,"leaf",MovePreset.Flutter);break;
            case "petal":for(int i=0;i<n;i++)fx.Add.Add(x,y,z,Rnd(fx,-.8,.8),Rnd(fx,-.2,.8),Rnd(fx,-.8,.8),Pick(fx,ca,cb),.6,.22*s,"petal",MovePreset.Flutter);break;
            case "dark":for(int i=0;i<n*2;i++)fx.Add.Add(x,y,z,Rnd(fx,-.5,.5),Rnd(fx,-.2,.7),Rnd(fx,-.5,.5),Pick(fx,ca,cb),Rnd(fx,.3,.5),.4*s,"wisp",MovePreset.Smoke);break;
            case "ice":for(int i=0;i<n;i++)fx.Add.Add(x,y,z,Rnd(fx,-.6,.6),Rnd(fx,-.6,.6),Rnd(fx,-.6,.6),Pick(fx,ca,White),.4,.18*s,Pick(fx,"snow","shard"),MovePreset.Drift);break;
            case "holy":for(int i=0;i<n;i++)fx.Add.Add(x,y,z,Rnd(fx,-.6,.6),Rnd(fx,-.3,.6),Rnd(fx,-.6,.6),Pick(fx,cb,White),.45,.2*s,"star",MovePreset.Drift);break;
            case "wind":for(int i=0;i<n;i++)fx.Add.Add(x,y,z,Rnd(fx,-.4,.4),Rnd(fx,-.4,.4),Rnd(fx,-.4,.4),Pick(fx,ca,White),.2,.3*s,"glow",MovePreset.Pop,.5);break;
            case "data":for(int i=0;i<n;i++)fx.Add.Add(x+Rnd(fx,-.12,.12),y+Rnd(fx,-.12,.12),z+Rnd(fx,-.12,.12),0,Rnd(fx,0,.4),0,Pick(fx,ca,cb),.35,.16*s,"square",MovePreset.Shrink,1,0);break;
            case "zap":for(int i=0;i<n;i++)fx.Add.Add(x,y,z,Rnd(fx,-2,2),Rnd(fx,-2,2),Rnd(fx,-2,2),Pick(fx,ca,White),.12,.25*s,"bolt",MovePreset.Drift);break;
            case "note":for(int i=0;i<n;i++)fx.Add.Add(x,y,z,Rnd(fx,-.5,.5),Rnd(fx,.2,.8),Rnd(fx,-.5,.5),Pick(fx,ca,cb),.4,.2*s,"note",MovePreset.Drift,1,0);break;
        }
    }
    internal static void Glyph(MoveEffects fx,string kind,MoveVector a,double yaw,MoveColor color,double size,double d,double dur,bool flat=false)
    {
        MoveMesh? mesh=null;string meshKind=kind=="tri"?"tri":kind=="circle"?"torus":flat?"crossFlat":"cross";
        fx.Run(d,dur,k=>{if(mesh is null){mesh=fx.Mesh(meshKind);mesh.Color=color;}mesh.Position.Set(a.X+Math.Sin(yaw)*.35,a.Y,a.Z+Math.Cos(yaw)*.35);
            if(flat&&meshKind=="crossFlat")mesh.Rotation.Set(0,yaw+fx.Now,0);else if(flat){mesh.Position.Set(a.X,.2,a.Z);mesh.Rotation.Set(Math.PI/2,0,0);}else{mesh.RotationOrder="YXZ";mesh.Rotation.Set(0,yaw,kind=="x"?Math.PI/4:kind=="tri"?fx.Now*3:0);}
            double g=k<.2?k/.2:k>.75?(1-k)/.25:1;mesh.Scale.SetScalar(size*(.4+.6*g)*.5);mesh.Opacity=(flat?.45:.8)*g;},()=>fx.Free(mesh));
    }
    internal static void ChargeGlow(MoveEffects fx,MoveVector a,MoveColor ca,MoveColor cb,double size,double d,double dur)
    {fx.Run(d,dur,(k,dt)=>{fx.Quad("glow",a.X,a.Y,a.Z,size*(.2+k),size*(.2+k),ca,.85,fx.Now*5);int n=MoveEffects.Round(dt*80);for(int i=0;i<n;i++){double angle=fx.Random()*Tau,b=Rnd(fx,-1,1),r=size*.8,x=Math.Cos(angle)*Math.Sqrt(1-b*b)*r,y=b*r,z=Math.Sin(angle)*Math.Sqrt(1-b*b)*r;fx.Add.Add(a.X+x,a.Y+y,a.Z+z,-x/.2,-y/.2,-z/.2,Pick(fx,ca,cb),.2,.2,"spark",MovePreset.None);}});}
    internal static void SlashMesh(MoveEffects fx,double d,double x,double y,double z,MoveColor color,double size,double ang,string kind,double dur)
    {fx.Run(d,dur,k=>{double sc=size*(.55+.6*Math.Min(1,k*2.5));fx.Quad(kind,x,y,z,sc,sc,color,k<.6?1:(1-k)/.4,ang+(k-.5)*.7);});}
    public static void Charge(MoveEffects fx,MoveContext c,double dur)
    {
        switch(c.Spec.Charge)
        {
            case "none":break;
            case "mouth":ChargeGlow(fx,c.A,c.Ca,c.Cb,.35*c.S,0,Math.Max(.12,dur));break;
            case "hands":for(int i=0;i<2;i++)ChargeGlow(fx,Launch(fx,c,"hands",i),c.Ca,c.Cb,.28*c.S,0,Math.Max(.12,dur));break;
            case "back":for(int i=0;i<2;i++)ChargeGlow(fx,Launch(fx,c,"back",i),c.Ca,c.Cb,.35*c.S,0,Math.Max(.15,dur));break;
            case "sky":ChargeGlow(fx,new(c.P.X,c.H+.8,c.P.Z),c.Ca,c.Cb,.4*c.S,0,Math.Max(.15,dur));fx.Pillar(0,c.P.X,c.P.Z,c.Ca,.35,4,Math.Max(.3,dur+.15));break;
            case "ground":fx.GroundRing(0,c.P.X,c.P.Z,c.Ca,1.8*c.S,Math.Max(.3,dur+.1));for(int i=0;i<16;i++){double a=fx.Random()*Tau;fx.Alpha.Add(c.P.X+Math.Cos(a)*.8,.1,c.P.Z+Math.Sin(a)*.8,Math.Cos(a),Rnd(fx,.5,1.5),Math.Sin(a),Smoke,.6,.5,"smoke",MovePreset.Smoke,.5);}break;
            default:
                for(int i=0;i<20+c.Stage*10;i++){double th=fx.Random()*Tau,r=Rnd(fx,.9,1.5);fx.Add.Add(c.P.X+Math.Cos(th)*r,Rnd(fx,.2,1.5),c.P.Z+Math.Sin(th)*r,-Math.Cos(th)*r/Math.Max(.2,dur),Rnd(fx,0,1),-Math.Sin(th)*r/Math.Max(.2,dur),Pick(fx,c.Ca,c.Cb),Math.Max(.2,dur),.3,"spark",MovePreset.None);}fx.GroundRing(0,c.P.X,c.P.Z,c.Cb,1.3,Math.Max(.3,dur+.1),"ring");break;
        }
    }
    public static void Basic(MoveEffects fx,MoveOptions b,MoveVector a,MoveVector target,double h,int stage,bool big,MoveColor ca,MoveColor cb)
    {
        double s=1+stage*.2,y=.85,dx=target.X-a.X,dz=target.Z-a.Z,length=Dist(a,target);if(length==0)length=1;
        string? kind=b.String("kind");
        switch(kind)
        {
            case "shot":
                string cell=CellOf(b.String("head"));var sys=SysFor(fx,cell);double ay=h*.6,dur=Math.Min(.3,Math.Max(.12,length/b.Number("speed",16))),size=b.Number("size",.3)*s*(big?1.4:1);
                fx.Fly(0,dur,a.X,ay,a.Z,target.X,y,target.Z,b.Value("arc",.25),(x,yy,z,k,dt)=>{sys.Add(x,yy,z,0,0,0,sys==fx.Alpha?White:cb,.05,size*2,cell,MovePreset.None,1,b.Flag("spin")?fx.Now*b.Value("spin"):0);fx.Add.Add(x,yy,z,0,0,0,ca,.05,size*2.6,"glow",MovePreset.None,.5,0);if(b.String("trail") is {} trail&&fx.Random()<.6)EmitTrail(fx,trail,x,yy,z,dt*.5,size*2.5,ca,cb);},()=>{fx.Burst(fx.Add,target.X,y,target.Z,6,2.5,ca,cb,.25,.2,"spark",MovePreset.Drift,.5);fx.Add.Add(target.X,y,target.Z,0,0,0,ca,.16,size*3,"glow",MovePreset.Pop,.8);});return;
            case "bite":SlashMesh(fx,0,target.X,y+.15,target.Z,ca,.7*s,Math.PI,"crescent",.16);SlashMesh(fx,.02,target.X,y-.15,target.Z,ca,.7*s,0,"crescent",.16);break;
            case "punch":fx.Add.Add(target.X,y,target.Z,0,0,0,ca,.18,1.1*s,"ring",MovePreset.Pop);fx.Add.Add(target.X,y,target.Z,0,0,0,cb,.1,.9*s,"glow",MovePreset.Pop);break;
            case "horn":case "peck":case "sting":for(int i=0;i<(kind=="peck"?2:1);i++)fx.Add.Add(target.X-dx/length*.6,y+i*.15,target.Z-dz/length*.6,dx/length*6,0,dz/length*6,ca,.12,.9*s,"streak",MovePreset.None);break;
            case "kick":SlashMesh(fx,0,target.X,y,target.Z,ca,.9*s,Math.PI/2+.3,"crescent",.18);break;
            case "tail":SlashMesh(fx,0,target.X,y-.1,target.Z,ca,1.1*s,.1,"crescent",.2);break;
            case "claw":SlashMesh(fx,0,target.X,y,target.Z,ca,.8*s,.6+Rnd(fx,-.4,.4),"claw",.18);break;
            default:SlashMesh(fx,0,target.X,y,target.Z,ca,.9*s,.5+Rnd(fx,-.6,.6),"crescent",.18);break;
        }
        fx.Burst(fx.Add,target.X,y,target.Z,big?10:5,3.5,ca,cb,.22,.2,"spark",MovePreset.Drift,.5);if(big)fx.Flash(0,target.X,y,target.Z,White,1.1,.15);
    }
}
