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
}
