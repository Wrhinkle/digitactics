using System.Globalization;

namespace DigiTactics.Presentation;

// Small Canvas 2D vocabulary used by the original elimination themes. The native
// backend consumes these procedural operations; no browser is shipped at runtime.
public abstract class EliminationCanvas
{
    public EliminationPaint fillStyle { set => Emit("fillStyle", [], value); }
    public EliminationPaint strokeStyle { set => Emit("strokeStyle", [], value); }
    public double globalAlpha { set => Emit("globalAlpha", [value]); }
    public double lineWidth { set => Emit("lineWidth", [value]); }
    public string lineCap { set => Emit("lineCap", [value == "round" ? 1 : 0]); }
    public void fillRect(double x,double y,double w,double h) => Emit("fillRect",[x,y,w,h]);
    public void strokeRect(double x,double y,double w,double h) => Emit("strokeRect",[x,y,w,h]);
    public void beginPath() => Emit("beginPath",[]);
    public void closePath() => Emit("closePath",[]);
    public void moveTo(double x,double y) => Emit("moveTo",[x,y]);
    public void lineTo(double x,double y) => Emit("lineTo",[x,y]);
    public void arc(double x,double y,double r,double start,double end) => Emit("arc",[x,y,r,start,end]);
    public void ellipse(double x,double y,double rx,double ry,double angle,double start,double end) => Emit("ellipse",[x,y,rx,ry,angle,start,end]);
    public void fill() => Emit("fill",[]);
    public void stroke() => Emit("stroke",[]);
    public void save() => Emit("save",[]);
    public void restore() => Emit("restore",[]);
    public void translate(double x,double y) => Emit("translate",[x,y]);
    public void rotate(double angle) => Emit("rotate",[angle]);
    public EliminationPaint createLinearGradient(double x0,double y0,double x1,double y1) => new(1,[x0,y0,x1,y1]);
    public EliminationPaint createRadialGradient(double x0,double y0,double r0,double x1,double y1,double r1) => new(2,[x0,y0,r0,x1,y1,r1]);
    protected abstract void Emit(string operation,double[] values,EliminationPaint? paint=null);
}

public sealed record EliminationColor(double R,double G,double B,double A=1)
{
    public static EliminationColor Parse(string value)
    {
        if(value[0]=='#')
        {
            string hex=value[1..];if(hex.Length==3)hex=string.Concat(hex.Select(c=>new string(c,2)));
            uint rgb=uint.Parse(hex,NumberStyles.HexNumber,CultureInfo.InvariantCulture);
            return new((rgb>>16)&255,(rgb>>8)&255,rgb&255);
        }
        double[] channels=value[(value.IndexOf('(')+1)..value.IndexOf(')')].Split(',').Select(s=>double.Parse(s,CultureInfo.InvariantCulture)).ToArray();
        return new(channels[0],channels[1],channels[2],channels.Length==4?Math.Clamp(channels[3],0,1):1);
    }
}
public sealed class EliminationPaint(int kind,double[] values)
{
    public int Kind {get;}=kind; // 0 solid, 1 linear, 2 radial
    public double[] Values {get;}=values;
    public List<EliminationStop> Stops {get;}=[];
    public void addColorStop(double at,string color) => Stops.Add(new(at,EliminationColor.Parse(color)));
    public static implicit operator EliminationPaint(string value)
    {
        var color=EliminationColor.Parse(value);return new(0,[color.R,color.G,color.B,color.A]);
    }
}
public sealed record EliminationStop(double At,EliminationColor Color);
