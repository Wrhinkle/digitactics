using System.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

/// <summary>Two bounded source point pools, each uploaded and drawn once.</summary>
internal sealed class MoveSpriteRenderer : IDisposable
{
    private const int Stride=10;
    private readonly GlProgram program;
    private GlVertexArray array;
    private GlBuffer buffer;
    private readonly float[] vertices=new float[9000*Stride];
    private readonly GpuTexture atlas;
    private static readonly Dictionary<string,int> cells=new[]{"glow","spark","flame","smoke","bubble","feather","note","card","leaf","petal","shard","streak","cross","gear","bat","fish","square","needle","star","ring","seed","blade","bolt","paw","drop","snow","skull","heart","hex","dot","ember","wisp"}.Select((name,index)=>(name,index)).ToDictionary(p=>p.name,p=>p.index);
    public unsafe MoveSpriteRenderer(GpuTexture atlas)
    {
        this.atlas=atlas;
        using var vertex=OpenGl.CreateShader(GlShaderType.Vertex).SetSource(Vertex).Compile();using var fragment=OpenGl.CreateShader(GlShaderType.Fragment).SetSource(Fragment).Compile();program=OpenGl.CreateProgram();program.Attach(vertex).Attach(fragment).Link();
        Span<GlVertexArray> arrays=stackalloc GlVertexArray[1];OpenGl.CreateVertexArrays(arrays);array=arrays[0];Span<GlBuffer> buffers=stackalloc GlBuffer[1];OpenGl.CreateBuffers(buffers);buffer=buffers[0];OpenGl.NamedBufferData(buffer,vertices.Length*sizeof(float),null,OpenGl.GL_DYNAMIC_DRAW);
        OpenGl.VertexArrayVertexBuffer(array.Id,0,buffer,0,Stride*sizeof(float));
        int[] sizes=[3,4,1,1,1];uint offset=0;for(uint i=0;i<sizes.Length;i++){OpenGl.EnableVertexArrayAttrib(array.Id,i);OpenGl.VertexArrayAttribBinding(array.Id,i,0);OpenGl.VertexArrayAttribFormat(array.Id,i,sizes[i],OpenGl.GL_FLOAT,false,offset);offset+=(uint)sizes[i]*sizeof(float);}
    }
    public unsafe int Draw(MoveParticles particles,bool additive,Matrix4x4 camera,int width,int height)
    {
        if(particles.Count==0)return 0;particles.Align(camera,(double)width/height);int offset=0;
        foreach(ref readonly var p in particles.Items){vertices[offset++]=p.X;vertices[offset++]=p.Y;vertices[offset++]=p.Z;vertices[offset++]=(float)p.Color.R;vertices[offset++]=(float)p.Color.G;vertices[offset++]=(float)p.Color.B;vertices[offset++]=p.Alpha;vertices[offset++]=p.Size;vertices[offset++]=p.Rotation;vertices[offset++]=cells[p.Cell];}
        fixed(float* data=vertices)OpenGl.NamedBufferSubData(buffer,0,offset*sizeof(float),data);
        program.Use();MoveGpu.Camera(program,camera);float projectionScale=MathF.Sqrt(camera.M12*camera.M12+camera.M22*camera.M22+camera.M32*camera.M32);program.Uniform(4,height*projectionScale/2);OpenGl.BindTextureUnit(0,atlas.Id);
        OpenGl.Native.glDepthMask(false);OpenGl.Native.glEnable(GlCapability.Blend);
        if(additive)OpenGl.Native.glBlendFunc(OpenGl.GL_SRC_ALPHA,OpenGl.GL_ONE);else OpenGl.Native.glBlendFuncSeparate(OpenGl.GL_SRC_ALPHA,OpenGl.GL_ONE_MINUS_SRC_ALPHA,OpenGl.GL_ONE,OpenGl.GL_ONE_MINUS_SRC_ALPHA);
        OpenGl.Native.glEnable((GlCapability)OpenGl.GL_PROGRAM_POINT_SIZE);array.Bind();OpenGl.Native.glDrawArrays(OpenGl.GL_POINTS,0,particles.Count);OpenGl.Native.glDisable((GlCapability)OpenGl.GL_PROGRAM_POINT_SIZE);return 1;
    }
    public void Dispose(){program.Dispose();OpenGl.DeleteBuffers(new[]{buffer});OpenGl.DeleteVertexArrays(new[]{array});}
    private const string Vertex="""
        #version 450 core
        layout(location=0) in vec3 position;layout(location=1) in vec4 color;layout(location=2) in float size;layout(location=3) in float rotation;layout(location=4) in float cell;
        layout(location=0) uniform mat4 camera;layout(location=4) uniform float scale;
        out vec4 vColor;out float vRotation;out float vCell;
        void main(){gl_Position=camera*vec4(position,1);gl_PointSize=min(size*scale/max(.1,gl_Position.w),256);gl_Position.z=2*gl_Position.z-gl_Position.w;vColor=color;vRotation=rotation;vCell=cell;}
        """;
    private const string Fragment="""
        #version 450 core
        layout(binding=0) uniform sampler2D atlas;in vec4 vColor;in float vRotation;in float vCell;out vec4 outputColor;
        void main(){vec2 p=vec2(gl_PointCoord.x-.5,.5-gl_PointCoord.y);float c=cos(vRotation),s=sin(vRotation);vec2 q=vec2(c*p.x+s*p.y,-s*p.x+c*p.y)+.5;if(any(lessThan(q,vec2(0)))||any(greaterThan(q,vec2(1))))discard;
            float cx=mod(vCell,8),cy=floor(vCell/8);vec4 tex=texture(atlas,vec2((cx+q.x)/8,(cy+1-q.y)/4));float alpha=tex.a*vColor.a;if(alpha<.004)discard;outputColor=vec4(vColor.rgb*tex.rgb,alpha);}
        """;
}
