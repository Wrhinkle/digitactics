using Brutal.OpenGlApi;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

/// <summary>Farm CSS sky, stars and night tint rendered inside the scene viewport.</summary>
internal sealed class FarmAtmosphere : IDisposable
{
    private readonly GlProgram program;
    private GlVertexArray array;
    private bool night;
    private double elapsed = .8, start;
    public double Opacity { get; private set; }
    public FarmAtmosphere()
    {
        using var vertex = OpenGl.CreateShader(GlShaderType.Vertex).SetSource(Vertex).Compile();
        using var fragment = OpenGl.CreateShader(GlShaderType.Fragment).SetSource(Fragment).Compile();
        program = OpenGl.CreateProgram();
        try { program.Attach(vertex).Attach(fragment).Link(); }
        catch { program.Dispose(); throw; }
        Span<GlVertexArray> arrays = stackalloc GlVertexArray[1]; OpenGl.CreateVertexArrays(arrays); array = arrays[0];
    }
    public void Update(double delta, bool target)
    {
        if (target != night) { start = Opacity; elapsed = 0; night = target; }
        elapsed = Math.Min(.8, elapsed + delta);
        // CSS ease: cubic-bezier(.25,.1,.25,1).
        double x = elapsed / .8, lo = 0, hi = 1;
        for (int i = 0; i < 30; i++) { double t = (lo + hi) / 2, u = 1 - t; if (3*u*u*t*.25+3*u*t*t*.25+t*t*t < x) lo = t; else hi = t; }
        double p = (lo + hi) / 2, q = 1 - p, ease = 3*q*q*p*.1+3*q*p*p+p*p*p;
        Opacity = elapsed == .8 ? (night ? 1 : 0) : start + ((night ? 1 : 0) - start) * ease;
    }
    public unsafe void Draw(int left, int bottom, int width, int height, string[] sky, bool overlay)
    {
        float4 Color(string hex) => new(Convert.ToInt32(hex[1..3],16)/255f,Convert.ToInt32(hex[3..5],16)/255f,Convert.ToInt32(hex[5..7],16)/255f,1);
        program.Use(); program.Uniform(0, new float4(left,bottom,width,height));
        program.Uniform(1, Color(sky[0])); program.Uniform(2, Color(sky[1]));
        program.Uniform(3, (float)Opacity); program.Uniform(4, overlay ? 1 : 0);
        OpenGl.Native.glDisable(GlCapability.DepthTest); OpenGl.Native.glDisable(GlCapability.CullFace);
        if (overlay) { OpenGl.Native.glEnable(GlCapability.Blend); OpenGl.Native.glBlendFunc(OpenGl.GL_SRC_ALPHA, OpenGl.GL_ONE_MINUS_SRC_ALPHA); }
        else OpenGl.Native.glDisable(GlCapability.Blend);
        array.Bind(); OpenGl.Native.glDrawArrays(OpenGl.GL_TRIANGLES,0,3);
        OpenGl.Native.glDisable(GlCapability.Blend); OpenGl.Native.glEnable(GlCapability.DepthTest);
    }
    public void Dispose() { OpenGl.DeleteVertexArrays(new[] { array }); program.Dispose(); }
    private const string Vertex = """
        #version 450 core
        void main() { vec2 p=vec2((gl_VertexID<<1)&2,gl_VertexID&2); gl_Position=vec4(p*2-1,0,1); }
        """;
    private const string Fragment = """
        #version 450 core
        layout(location=0) uniform vec4 viewport;
        layout(location=1) uniform vec4 skyTop;
        layout(location=2) uniform vec4 skyBottom;
        layout(location=3) uniform float opacity;
        layout(location=4) uniform int overlay;
        out vec4 color;
        void main() {
            vec2 uv=(gl_FragCoord.xy-viewport.xy)/viewport.zw; uv.y=1-uv.y;
            if(overlay!=0) {
                float t=clamp(length((uv-vec2(.5,.7))/vec2(.70710678,.98994949)),0,1);
                float alpha=mix(.15,.6,t);
                vec3 premult=mix(vec3(20,30,80)/255.0*.15,vec3(5,8,30)/255.0*.6,t);
                color=vec4(premult/alpha,alpha*opacity);
            } else {
                vec3 sky=mix(skyTop.rgb,skyBottom.rgb,uv.y);
                const vec3 stars[8]=vec3[8](vec3(.1,.2,1.5),vec3(.3,.6,1),vec3(.55,.25,1.5),vec3(.7,.5,1),vec3(.85,.15,1.5),vec3(.45,.75,1),vec3(.2,.8,1),vec3(.92,.65,1.5));
                for(int i=0;i<8;i++) {
                    float a=clamp(1-length((uv-stars[i].xy*vec2(1,.4))*viewport.zw)/stars[i].z,0,1)*.9*opacity;
                    sky=mix(sky,vec3(1),a);
                }
                color=vec4(sky,1);
            }
        }
        """;
}
