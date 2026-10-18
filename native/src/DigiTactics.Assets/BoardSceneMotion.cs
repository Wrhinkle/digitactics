namespace DigiTactics.Assets;

public static class BoardSceneMotion
{
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
}
