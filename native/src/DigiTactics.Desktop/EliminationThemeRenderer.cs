using System.Numerics;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

/// <summary>BRUTAL ImGui backend for the source game's procedural Canvas themes.</summary>
internal sealed class EliminationThemeRenderer : EliminationCanvas
{
    private sealed record State(EliminationPaint Fill,EliminationPaint Stroke,double Alpha,double Width,bool Round,Matrix3x2 Matrix);
    private State state=new("#000","#000",1,1,false,Matrix3x2.Identity);
    private readonly Stack<State> saved=[];
    private readonly List<float2> path=[];
    private ImDrawListPtr draw;
    private float2 origin;
    private double opacity;
    private bool closed;
    public int Operations {get;private set;}
    public static ImColor8 ThemeColor(string? variant,bool pve=false)=>Convert(EliminationThemes.Rim(variant,pve),1);
    public static ImColor8 TextColor(string? variant,bool pve=false)=>Convert(EliminationThemes.Text(variant,pve),1);
    public static bool Silhouette(string? variant)=>EliminationThemes.Silhouette(variant);
    public void Draw(string? variant,bool isPve,double time,float2 origin,float2 size,float opacity=1)
    {
        if(size.X<=0||size.Y<=0||opacity<=0)return;
        draw=ImGui.GetWindowDrawList();this.origin=origin;this.opacity=Math.Clamp(opacity,0,1);Operations=0;
        state=new("#000","#000",1,1,false,Matrix3x2.Identity);saved.Clear();path.Clear();closed=false;
        draw.PushClipRect(origin,origin+size,true);
        try {EliminationThemes.Draw(this,variant,isPve,size.X,size.Y,Math.Max(0,time));}
        finally {draw.PopClipRect();}
    }
    protected override void Emit(string op,double[] v,EliminationPaint? paint=null)
    {
        Operations++;
        switch(op)
        {
            case "fillStyle":state=state with {Fill=paint!};break;
            case "strokeStyle":state=state with {Stroke=paint!};break;
            case "globalAlpha":state=state with {Alpha=v[0]};break;
            case "lineWidth":state=state with {Width=v[0]};break;
            case "lineCap":state=state with {Round=v[0]==1};break;
            case "save":saved.Push(state);break;
            case "restore":state=saved.Pop();break;
            case "translate":state=state with {Matrix=Matrix3x2.CreateTranslation((float)v[0],(float)v[1])*state.Matrix};break;
            case "rotate":state=state with {Matrix=Matrix3x2.CreateRotation((float)v[0])*state.Matrix};break;
            case "beginPath":path.Clear();closed=false;break;
            case "closePath":closed=true;break;
            case "moveTo":case "lineTo":path.Add(Point(v[0],v[1]));break;
            case "arc":Curve(v[0],v[1],v[2],v[2],0,v[3],v[4]);break;
            case "ellipse":Curve(v[0],v[1],v[2],v[3],v[4],v[5],v[6]);break;
            case "fill":Fill(path,state.Fill);break;
            case "stroke":Stroke(path,closed);break;
            case "fillRect":
                if(paint is not null)throw new InvalidOperationException();
                if(v[2]==0||v[3]==0)break;
                if(state.Fill.Kind==2)
                {
                    draw.PushClipRect(Point(v[0],v[1]),Point(v[0]+v[2],v[1]+v[3]),true);
                    try{Radial(state.Fill);}finally{draw.PopClipRect();}
                }
                else if(state.Fill.Kind==1)LinearRectangle(v);
                else Fill([Point(v[0],v[1]),Point(v[0]+v[2],v[1]),Point(v[0]+v[2],v[1]+v[3]),Point(v[0],v[1]+v[3])],state.Fill);
                break;
            case "strokeRect":Stroke([Point(v[0],v[1]),Point(v[0]+v[2],v[1]),Point(v[0]+v[2],v[1]+v[3]),Point(v[0],v[1]+v[3])],true);break;
            default:throw new InvalidOperationException("Unknown theme canvas operation: "+op);
        }
    }
    private float2 Point(double x,double y)
    {var p=Vector2.Transform(new((float)x,(float)y),state.Matrix);return new(p.X+origin.X,p.Y+origin.Y);}
    private static ImColor8 Convert(EliminationColor c,double alpha)=>new((byte)Math.Clamp(Math.Round(c.R),0,255),(byte)Math.Clamp(Math.Round(c.G),0,255),(byte)Math.Clamp(Math.Round(c.B),0,255),(byte)Math.Clamp(Math.Round(c.A*alpha*255),0,255));
    private static uint Packed(EliminationColor c,double alpha)
    {return (uint)Math.Clamp(Math.Round(c.R),0,255) | (uint)Math.Clamp(Math.Round(c.G),0,255)<<8 | (uint)Math.Clamp(Math.Round(c.B),0,255)<<16 | (uint)Math.Clamp(Math.Round(c.A*alpha*255),0,255)<<24;}
    private void Curve(double x,double y,double rx,double ry,double rotation,double start,double end)
    {
        double sweep=Math.Min(Math.PI*2,Math.Max(0,end-start));
        int segments=Math.Max(2,(int)Math.Ceiling(sweep/Math.Max(.01,2*Math.Acos(Math.Max(-1,1-.15/Math.Max(1,Math.Max(rx,ry)))))));
        double cs=Math.Cos(rotation),sn=Math.Sin(rotation);
        for(int i=0;i<=segments;i++){double a=start+sweep*i/segments,px=rx*Math.Cos(a),py=ry*Math.Sin(a);path.Add(Point(x+px*cs-py*sn,y+px*sn+py*cs));}
    }
    private unsafe void Fill(IReadOnlyList<float2> points,EliminationPaint paint)
    {
        if(points.Count<3)return;
        int first=draw.VtxBuffer.Count;
        foreach(var point in points)draw.PathLineTo(point);
        draw.PathFillConcave(paint.Kind==0?Convert(new(paint.Values[0],paint.Values[1],paint.Values[2],paint.Values[3]),state.Alpha*opacity):new ImColor8(255,255,255));
        if(paint.Kind==0)return;
        Matrix3x2.Invert(state.Matrix,out var inverse);
        for(int i=first;i<draw.VtxBuffer.Count;i++)
        {
            ref var vertex=ref draw.VtxBuffer.Data[i];
            var p=Vector2.Transform(new(vertex.pos.X-origin.X,vertex.pos.Y-origin.Y),inverse);
            double dx=paint.Values[2]-paint.Values[0],dy=paint.Values[3]-paint.Values[1];
            double at=(dx*dx+dy*dy)==0?0:((p.X-paint.Values[0])*dx+(p.Y-paint.Values[1])*dy)/(dx*dx+dy*dy);
            double aa=(vertex.col>>24)/255d;
            vertex.col=Packed(At(paint,at),state.Alpha*opacity*aa);
        }
    }
    private void Stroke(IReadOnlyList<float2> points,bool close)
    {
        if(points.Count<2)return;
        var p=state.Stroke;var color=Convert(new(p.Values[0],p.Values[1],p.Values[2],p.Values[3]),state.Alpha*opacity);
        foreach(var point in points)draw.PathLineTo(point);
        draw.PathStroke(color,close?ImDrawFlags.Closed:ImDrawFlags.None,(float)state.Width);
        if(state.Round&&!close){draw.AddCircleFilled(points[0],(float)state.Width/2,color);draw.AddCircleFilled(points[^1],(float)state.Width/2,color);}
    }
    private static EliminationColor At(EliminationPaint paint,double value)
    {
        var stops=paint.Stops;value=Math.Clamp(value,0,1);
        if(value<=stops[0].At)return stops[0].Color;if(value>=stops[^1].At)return stops[^1].Color;
        for(int i=1;i<stops.Count;i++)if(value<=stops[i].At)
        {
            var a=stops[i-1];var b=stops[i];double t=(value-a.At)/(b.At-a.At),alpha=a.Color.A*(1-t)+b.Color.A*t;
            if(alpha<=0)return b.Color;
            return new(a.Color.R*(1-t)+b.Color.R*t,a.Color.G*(1-t)+b.Color.G*t,a.Color.B*(1-t)+b.Color.B*t,alpha);
        }
        return stops[^1].Color;
    }
    private void LinearRectangle(double[] v)
    {
        // Add vertices at every gradient stop so a three-stop light pillar keeps its bright center.
        var p=state.Fill;bool vertical=p.Values[0]==p.Values[2];
        double from=vertical?v[1]:v[0],to=from+(vertical?v[3]:v[2]),g0=p.Values[vertical?1:0],g1=p.Values[vertical?3:2];
        var boundaries=p.Stops.Select(s=>g0+(g1-g0)*s.At).Where(x=>x>from&&x<to).Prepend(from).Append(to).Distinct().Order().ToArray();
        for(int i=1;i<boundaries.Length;i++)
        {
            double a=boundaries[i-1],b=boundaries[i];
            if(vertical)Fill([Point(v[0],a),Point(v[0]+v[2],a),Point(v[0]+v[2],b),Point(v[0],b)],p);
            else Fill([Point(a,v[1]),Point(b,v[1]),Point(b,v[1]+v[3]),Point(a,v[1]+v[3])],p);
        }
    }
    private void Radial(EliminationPaint paint)
    {
        // All original glows are concentric, two-stop radial gradients. A triangle
        // fan interpolates the original straight RGBA stops (including transparent black).
        double x=paint.Values[3],y=paint.Values[4],radius=paint.Values[5];
        var center=paint.Stops[0].Color;var transparent=paint.Stops[^1].Color;
        int count=Math.Max(48,(int)Math.Ceiling(Math.PI*2/Math.Max(.01,2*Math.Acos(Math.Max(-1,1-.15/Math.Max(1,radius))))));
        var uv=ImGui.GetFontTexUvWhitePixel();
        draw.PrimReserve(count*3,count*3);
        for(int i=0;i<count;i++)
        {
            double a=i*Math.PI*2/count,b=(i+1)*Math.PI*2/count;
            draw.PrimVtx(Point(x,y),uv,Packed(center,state.Alpha*opacity));
            draw.PrimVtx(Point(x+Math.Cos(a)*radius,y+Math.Sin(a)*radius),uv,Packed(transparent,1));
            draw.PrimVtx(Point(x+Math.Cos(b)*radius,y+Math.Sin(b)*radius),uv,Packed(transparent,1));
        }
    }
}
