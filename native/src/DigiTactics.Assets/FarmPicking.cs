using System.Numerics;

namespace DigiTactics.Assets;

public static class FarmPicking
{
    /// <summary>Ray against the closed polygonal cylinders used by the original farm hit meshes.</summary>
    public static double? Cylinder(Vector3 origin, Vector3 direction, Matrix4x4 world, double radius, double height, int segments)
    {
        if (!Matrix4x4.Invert(world,out var inverse)) return null;
        var o = Vector3.Transform(origin,inverse); var d = Vector3.TransformNormal(direction,inverse);
        double enter = double.NegativeInfinity, exit = double.PositiveInfinity;
        bool Plane(double nx,double ny,double nz,double offset)
        {
            double position=nx*o.X+ny*o.Y+nz*o.Z-offset, speed=nx*d.X+ny*d.Y+nz*d.Z;
            if(Math.Abs(speed)<1e-12) return position<=0;
            double t=-position/speed;
            if(speed<0) enter=Math.Max(enter,t); else exit=Math.Min(exit,t);
            return enter<=exit;
        }
        if(!Plane(0,-1,0,0)||!Plane(0,1,0,height))return null;
        for(int i=0;i<segments;i++)
        {
            double angle=(i+.5)*Math.PI*2/segments;
            if(!Plane(Math.Sin(angle),0,Math.Cos(angle),radius*Math.Cos(Math.PI/segments)))return null;
        }
        return enter>=0 && enter<=exit ? enter : null;
    }
}
