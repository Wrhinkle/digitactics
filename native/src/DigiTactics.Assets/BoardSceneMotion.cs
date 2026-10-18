namespace DigiTactics.Assets;

public static class BoardSceneMotion
{
    public static Matrix4D RotatingParent(BoardMotion motion,double time)
    {
        double halfAngle=time*motion.Speed/2;
        var rotation=Matrix4D.Compose(new(0,0,0),new(0,Math.Sin(halfAngle),0,Math.Cos(halfAngle)),new(1,1,1));
        return Matrix4D.From(motion.Matrix!)*rotation*Matrix4D.From(motion.Parent!);
    }
    public static float? Opacity(string? kind,double time)=>kind switch
    {
        "mountain-stars"=>(float)(.75+.25*Math.Sin(time*1.7)),
        "grid-rings"=>(float)(.7+.2*Math.Sin(time*2.2)),
        "desert-haze"=>Haze(time).Opacity,
        _=>null
    };
    public static (double U,double V,float Opacity) Haze(double time)=>(time*.01,Math.Sin(time*2.3)*.04,(float)(.28+.08*Math.Sin(time*3.1)));
    public static void ScrollingUv(float[] vertices,float[] initial,double time,string kind)
    {
        var offset=kind switch
        {
            "desert-haze"=>(Haze(time).U,Haze(time).V),
            "sewer-pour"=>(0d,(time*1.5)%1),
            "grid-floor"=>(0d,(time*.05)%1),
            _=>throw new ArgumentException("Unknown scrolling board texture: "+kind,nameof(kind))
        };
        for(int at=0;at<vertices.Length;at+=GltfPrimitive.Stride)
        {
            vertices[at+6]=(float)(initial[at+6]+offset.Item1);
            vertices[at+7]=(float)(initial[at+7]+offset.Item2);
        }
    }
    public static Matrix4D Prop(string kind,BoardProp p,double time)
    {
        double x=p.X,y=p.Y,z=p.Z,rx=0,ry=p.Ry,rz=0,sx=p.S,sy=p.S,sz=p.S;
        switch(kind)
        {
            case "freeze-men":return BoardFreezeMotion.Snowman(p,time);
            case "village-eggs": rz=Math.Sin(time*2+p.Phase)*.18; break;
            case "village-blocks":
                y=p.Base+Math.Sin(time*1.2+p.Phase)*.3; ry=time*.4+p.Phase; rx=Math.Sin(time*.5+p.Phase)*.3; break;
            case "toy-tops":
                ry=time*9+p.Phase; rx=Math.Sin(time*1.5+p.Phase)*.12;
                x=p.Cx+Math.Sin(time*.6+p.Phase)*.25; z=p.Cz+Math.Cos(time*.6+p.Phase)*.25; break;
            case "toy-train":
                double angle=time*.12-p.K*.085; x=Math.Cos(angle)*12.05; z=Math.Sin(angle)*12.05; ry=-angle-Math.PI/2; break;
            case "coliseum-banners": y=.65; rx=-.08+Math.Sin(time*1.6+p.Phase)*.08; break;
            case "savanna-gears": rx=p.Rx; ry=p.Rx!=0?0:p.Ry; rz=time*p.Spin; break;
            case "grid-polys": y=p.Base+Math.Sin(time+p.Phase)*.25; ry=time*.5+p.Phase; rx=time*.3; break;
            case "coliseum-flames":
                sy=1+.18*Math.Sin(time*13+p.Phase)+.1*Math.Sin(time*7.3+p.Phase*2); sx=sz=1/Math.Sqrt(sy); break;
            default: throw new ArgumentException("Unknown board prop motion: "+kind,nameof(kind));
        }
        return new CompanionTransform(new(x,y,z),new(rx,ry,rz),new(sx,sy,sz)).Matrix;
    }
    public static float SeaHeight(double x,double z,double time,double amplitude,double speed)
    {
        double s=time*speed;
        return (float)(amplitude*(Math.Sin(x*.33+s*1.1)+.8*Math.Sin(z*.27-s*.9)+.35*Math.Sin((x+z)*.7+s*1.9)));
    }
    public static void Sea(float[] interleaved,double time,BoardMotion motion)
    {
        for(int at=0;at<interleaved.Length;at+=GltfPrimitive.Stride)
            interleaved[at+1]=SeaHeight(interleaved[at],interleaved[at+2],time,motion.Amp,motion.Speed);
    }
    public static Matrix4D Crystal(BoardCrystal item,double time)
    {
        double yaw=time*.6+item.Phase,y=1.1+Math.Sin(time*1.5+item.Phase)*.15;
        return Matrix4D.Compose(new(item.X,y,item.Z),new(0,Math.Sin(yaw/2),0,Math.Cos(yaw/2)),new(item.Sx,item.Sy,item.Sz));
    }
    public static void Particles(float[] vertices,double delta,double time,BoardMotion motion)
    {
        var box=motion.Box!; var velocity=motion.Velocity!; var speeds=motion.Speeds!;
        static float Wrap(double value,double min,double max)=>(float)(value<min?value+(max-min):value>max?value-(max-min):value);
        for(int i=0;i<speeds.Length;i++)
        {
            int at=i*GltfPrimitive.Stride; double speed=speeds[i];
            vertices[at]=Wrap(vertices[at]+(velocity[0]*speed+Math.Sin(time*1.3+i)*motion.Wobble)*delta,box[0],box[1]);
            vertices[at+1]=Wrap(vertices[at+1]+velocity[1]*speed*delta,box[2],box[3]);
            vertices[at+2]=Wrap(vertices[at+2]+(velocity[2]*speed+Math.Cos(time*1.1+i*.7)*motion.Wobble)*delta,box[4],box[5]);
        }
    }
    public static Matrix4D Palm(BoardPalm palm,double time)=>new CompanionTransform(new(palm.X,0,palm.Z),
        new(Math.Cos(time*.7+palm.Phase)*.025,palm.Ry,Math.Sin(time*.9+palm.Phase)*.035),new(palm.S,palm.S,palm.S)).Matrix;
    public static (Matrix4D Matrix,float Opacity) Surf(BoardMotion motion,double time)
    {
        double k=.5+.5*Math.Sin(time*.9),scale=.97+k*.05;
        return (Matrix4D.Compose(new(0,0,0),new(0,0,0,1),new(scale,scale,scale))*Matrix4D.From(motion.Matrix!),(float)(.2+.35*(1-k)));
    }
}
