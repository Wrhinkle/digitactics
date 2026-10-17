using System.Numerics;

namespace DigiTactics.Assets;

/// <summary>Double-precision row-vector transform. Keep hierarchy/skin calculations in double
/// precision: imported rigs can use tiny scales and large inverse binds that cancel at draw time.</summary>
public readonly record struct Matrix4D(
    double M11, double M12, double M13, double M14, double M21, double M22, double M23, double M24,
    double M31, double M32, double M33, double M34, double M41, double M42, double M43, double M44)
{
    public static Matrix4D Identity => new(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1);
    public static Matrix4D From(ReadOnlySpan<double> a) => new(a[0],a[1],a[2],a[3],a[4],a[5],a[6],a[7],a[8],a[9],a[10],a[11],a[12],a[13],a[14],a[15]);
    public void CopyTo(Span<double> a) {
        a[0]=M11;a[1]=M12;a[2]=M13;a[3]=M14;a[4]=M21;a[5]=M22;a[6]=M23;a[7]=M24;
        a[8]=M31;a[9]=M32;a[10]=M33;a[11]=M34;a[12]=M41;a[13]=M42;a[14]=M43;a[15]=M44;
    }
    public bool IsFinite { get { Span<double> a = stackalloc double[16]; CopyTo(a); foreach (double x in a) if (!double.IsFinite(x)) return false; return true; } }
    public static Matrix4D Compose(Vector3D t, QuaternionD q, Vector3D s) {
        double x=q.X,y=q.Y,z=q.Z,w=q.W,x2=x+x,y2=y+y,z2=z+z;
        double xx=x*x2,xy=x*y2,xz=x*z2,yy=y*y2,yz=y*z2,zz=z*z2,wx=w*x2,wy=w*y2,wz=w*z2;
        return new((1-(yy+zz))*s.X,(xy+wz)*s.X,(xz-wy)*s.X,0,
            (xy-wz)*s.Y,(1-(xx+zz))*s.Y,(yz+wx)*s.Y,0,
            (xz+wy)*s.Z,(yz-wx)*s.Z,(1-(xx+yy))*s.Z,0,t.X,t.Y,t.Z,1);
    }
    public static Matrix4D operator *(Matrix4D left, Matrix4D right) {
        Span<double> a=stackalloc double[16],b=stackalloc double[16],c=stackalloc double[16];left.CopyTo(a);right.CopyTo(b);
        for(int r=0;r<4;r++)for(int col=0;col<4;col++)c[r*4+col]=a[r*4]*b[col]+a[r*4+1]*b[col+4]+a[r*4+2]*b[col+8]+a[r*4+3]*b[col+12];
        return From(c);
    }
    public static bool Invert(Matrix4D value, out Matrix4D inverse) {
        Span<double> a=stackalloc double[16],m=stackalloc double[32];value.CopyTo(a);m.Clear();
        for(int r=0;r<4;r++){for(int c=0;c<4;c++)m[r*8+c]=a[r*4+c];m[r*8+r+4]=1;}
        for(int c=0;c<4;c++){
            int pivot=c;for(int r=c+1;r<4;r++)if(Math.Abs(m[r*8+c])>Math.Abs(m[pivot*8+c]))pivot=r;
            if(m[pivot*8+c]==0){inverse=default;return false;}
            if(pivot!=c)for(int k=0;k<8;k++)(m[c*8+k],m[pivot*8+k])=(m[pivot*8+k],m[c*8+k]);
            double divisor=m[c*8+c];for(int k=0;k<8;k++)m[c*8+k]/=divisor;
            for(int r=0;r<4;r++)if(r!=c){double factor=m[r*8+c];for(int k=0;k<8;k++)m[r*8+k]-=factor*m[c*8+k];}
        }
        for(int r=0;r<4;r++)for(int c=0;c<4;c++)a[r*4+c]=m[r*8+c+4];
        inverse=From(a);return inverse.IsFinite;
    }
}
