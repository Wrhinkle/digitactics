using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

/// <summary>The source shop's CSS egg, including its asymmetric silhouette, spots and hatch keyframes.</summary>
internal sealed class EggArtwork : IDisposable
{
    public const double CrackAt=1.3, BurstAt=2.05, RevealAt=2.43;
    private readonly Dictionary<string,GpuTexture> textures=[];
    internal static double RevealProgress(double age,bool motion=true)=>Ease(Math.Clamp(age/DigiTactics.Presentation.ClientMotion.CssDuration(.4,motion),0,1),0,0,.58,1);
    internal static (double Rotation,double X,double Y,double Scale,double Opacity) Motion(double age,bool motion=true)
    {
        if(age<0)return(0,0,0,1,1);
        if(age<CrackAt)
        {
            if(!motion && age>=.001)return(0,0,0,1,1); // one iteration, no fill
            double phase=motion?age%.42/.42:age/.001;
            double rotation=phase<.25?-12*Ease(phase/.25,.42,0,.58,1):phase<.75?-12+24*Ease((phase-.25)/.5,.42,0,.58,1):12*(1-Ease((phase-.75)/.25,.42,0,.58,1));
            return(rotation*Math.PI/180,0,0,1,1);
        }
        if(age<BurstAt)
        {
            if(!motion && age-CrackAt>=.001)return(0,0,0,1,1); // one iteration, no fill
            double phase=motion?(age-CrackAt)%.12/.12:(age-CrackAt)/.001;
            return phase<.5?(0,-2+8*phase,-2*phase,1,1):(0,2-6*(phase-.5),-1+4*(phase-.5),1,1);
        }
        double p=Ease(Math.Clamp((age-BurstAt)/DigiTactics.Presentation.ClientMotion.CssDuration(.38,motion),0,1),0,0,.58,1);
        return(0,0,0,1+.7*p,1-p);
    }
    private static double Ease(double x,double x1,double y1,double x2,double y2)
    {
        if(x<=0)return 0;if(x>=1)return 1;
        double lo=0,hi=1;
        for(int i=0;i<28;i++){double t=(lo+hi)/2,u=1-t;if(3*u*u*t*x1+3*u*t*t*x2+t*t*t<x)lo=t;else hi=t;}
        double v=(lo+hi)/2,w=1-v;return 3*w*w*v*y1+3*w*v*v*y2+v*v*v;
    }
    public void Draw(string id,float2 center,float height,double age=-1,ImColor8? glow=null,bool motion=true)
    {
        if(!textures.TryGetValue(id,out var texture))textures[id]=texture=new(Create(id),new(MinFilter:9729,WrapS:33071,WrapT:33071),false);
        var m=Motion(age,motion);if(m.Opacity<=0)return;
        float width=height*120/154,scale=(float)m.Scale;center+=new float2((float)m.X,(float)m.Y);
        float2 Point(float x,float y)=>center+new float2((float)(x*Math.Cos(m.Rotation)-y*Math.Sin(m.Rotation))*scale,(float)(x*Math.Sin(m.Rotation)+y*Math.Cos(m.Rotation))*scale);
        var draw=ImGui.GetWindowDrawList();
        if(age>=CrackAt && glow is {} tint)
        {
            var rgb=tint.AsByte4();
            for(int r=34;r>0;r-=2)draw.AddEllipseFilled(center,new(width*.5f+r,height*.5f+r),new ImColor8(rgb.X,rgb.Y,rgb.Z,(byte)(3*m.Opacity)));
        }
        draw.AddImageQuad(new ImTextureRef{_TexID=new((nint)texture.Id)},Point(-width/2,-height/2),Point(width/2,-height/2),Point(width/2,height/2),Point(-width/2,height/2),new(0,0),new(1,0),new(1,1),new(0,1),new ImColor8(255,255,255,(byte)(255*m.Opacity)));
        if(age>=CrackAt && age<BurstAt)
        {
            var crack=new ImColor8(42,29,16);
            void Line(double x,double y,double length,double angle){double a=angle*Math.PI/180;draw.AddLine(Point((float)((x-.5)*width),(float)((y-.5)*height)),Point((float)((x-.5+length*Math.Cos(a))*width),(float)((y-.5)*height+length*width*Math.Sin(a))),crack,3);}
            Line(.18,.44,.34,-24);Line(.46,.36,.30,28);Line(.40,.52,.26,70);
        }
    }
    private static PngImage Create(string id)
    {
        const int w=240,h=308;var rgba=new byte[w*h*4];
        var (light,spot)=id switch
        {
            "mixed"=>(new double[]{246,240,255},new double[]{199,125,255}),
            "legend"=>(new double[]{255,244,214},new double[]{255,179,71}),
            "emote"=>(new double[]{255,249,201},new double[]{255,216,74}),
            "board"=>(new double[]{220,255,233},new double[]{63,191,114}),
            "killscreen"=>(new double[]{255,224,228},new double[]{255,90,106}),
            "sound"=>(new double[]{223,243,255},new double[]{79,179,255}),
            _=>(new double[]{255,244,214},new double[]{240,192,96})
        };
        for(int y=0;y<h;y++)for(int x=0;x<w;x++)
        {
            double px=(x+.5)/w,py=(y+.5)/h,rx=(px-.5)/.5,ry=(py-.6)/(py<.6?.6:.4),distance=Math.Sqrt(rx*rx+ry*ry);
            double alpha=Math.Clamp((1-distance)*w/2,0,1);if(alpha<=0)continue;
            double gradient=Math.Clamp(Math.Sqrt(Math.Pow((px-.5)*w,2)+Math.Pow((py-.4)*h,2))/Math.Sqrt(Math.Pow(w*.5,2)+Math.Pow(h*.6,2))/.9,0,1);
            double Spot(double cx,double cy,double radius)=>Math.Clamp((radius-Math.Sqrt(Math.Pow((px-cx)*w,2)+Math.Pow((py-cy)*h,2))/Math.Sqrt(Math.Pow(Math.Max(cx,1-cx)*w,2)+Math.Pow(Math.Max(cy,1-cy)*h,2)))/.01,0,1);
            double spots=Math.Max(Spot(.62,.62,.13),Spot(.3,.7,.10)),shine=Spot(.35,.3,.09);
            double shade=.25*Math.Clamp((distance-.7)/.3,0,1)*Math.Clamp((px+py)/1.4,0,1);
            int at=(y*w+x)*4;
            for(int c=0;c<3;c++){double baseColor=light[c]*(1-gradient*.4)+85*gradient*.4;double color=baseColor*(1-spots)+spot[c]*spots;color=color*(1-shine)+255*shine;rgba[at+c]=(byte)Math.Clamp(Math.Round(color*(1-shade)),0,255);}
            rgba[at+3]=(byte)Math.Round(alpha*255);
        }
        return new(w,h,rgba);
    }
    public void Dispose(){foreach(var texture in textures.Values)texture.Dispose();textures.Clear();}
}
