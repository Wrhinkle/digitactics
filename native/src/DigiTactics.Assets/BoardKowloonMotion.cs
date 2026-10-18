using System.Numerics;

namespace DigiTactics.Assets;

public sealed class BoardKowloonMotion
{
    private double weather;
    private static readonly Vector3 dimSign=BoardLighting.Linear("#222222");
    public void Advance(double delta,bool combat)=>weather+=((combat?1:0)-weather)*Math.Min(1,1.2*delta);
    public float RainOpacity=>(float)(.35+.4*weather);
    public float RainSize=>(float)(.06+.04*weather);
    public float FogNear=>(float)(24+(15-24)*weather);
    public float FogFar=>(float)(58+(44-58)*weather);
    public static Vector3 Sign(BoardNeonSign sign,double time)=>Math.Sin(time*23+sign.Phase)>.93 && Math.Sin(time*1.7+sign.Phase)>.4
        ? dimSign : new(sign.Color[0],sign.Color[1],sign.Color[2]);
    public static (double Y,float Opacity) Scan(string kind,double time)
    {
        double k=time*.18%1;
        return kind switch
        {
            "kowloon-band"=>(-12+k*18,(float)(.75*Math.Sin(k*Math.PI))),
            "kowloon-lattice"=>(-15+k*18,(float)(.28*Math.Sin(k*Math.PI))),
            _=>throw new ArgumentException("Unknown Kowloon scan component.",nameof(kind))
        };
    }
}
