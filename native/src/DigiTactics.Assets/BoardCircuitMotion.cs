namespace DigiTactics.Assets;

public sealed class BoardCircuitMotion(bool initialWon=false)
{
    private double speed=1,phase,flash;
    private bool previousWon=initialWon;
    public double LightFlash { get; private set; }
    public void Advance(double delta,bool combat,bool won)
    {
        if(won && !previousWon) {flash=1;LightFlash=1.2;}
        previousWon=won;
        speed+=((combat?2.6:1)-speed)*Math.Min(1,2*delta);phase+=delta*speed;
        flash=Math.Max(0,flash-delta*.8);LightFlash=Math.Max(0,LightFlash-delta*5);
    }
    public double Offset(string kind,double time)=>kind=="circuit-screen"?(time*.12*speed)%1:(-time*.03)%1;
    public float Opacity(string kind)=>(float)(kind=="circuit-screen"?.5+.5*flash:.16+.4*flash);
    public void Scroll(float[] vertices,float[] initial,string kind,double time)
    {
        double offset=Offset(kind,time);
        for(int at=0;at<vertices.Length;at+=GltfPrimitive.Stride) vertices[at+7]=(float)(initial[at+7]+offset);
    }
    public void Pulses(float[] vertices,BoardCircuitPulse[] pulses)
    {
        double edge=14*Math.Sqrt(.5)+.6;
        for(int i=0;i<pulses.Length;i++)
        {
            var p=pulses[i];double k=(p.Off+phase*p.Sp)%1*60-30;int at=i*GltfPrimitive.Stride;
            vertices[at]=(float)(p.Along?k:p.Lane);vertices[at+2]=(float)(p.Along?p.Lane-6:k);
            vertices[at+1]=(float)(Math.Abs(vertices[at])<edge && Math.Abs(vertices[at+2])<edge?-60:-1.3);
        }
    }
}
