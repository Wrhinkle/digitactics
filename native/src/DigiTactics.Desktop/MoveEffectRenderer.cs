using System.Numerics;
using System.Text.Json;
using Brutal.OpenGlApi;
using DigiTactics.Assets;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Reusable source signature moves for combat and the elimination camera.</summary>
internal sealed class NativeMoveEffects : IDisposable
{
    private static readonly Lazy<MoveLibrary> library=new(()=>new(Path.Combine(AppContext.BaseDirectory,"content/move-effects/specs.json")));
    public MovePlayback Playback {get;}
    public MoveEffects Effects=>Playback.Effects;
    private readonly MoveEffectRenderer renderer=new();
    public NativeMoveEffects(string variantId){Playback=new(library.Value,variantId);}
    public void Play(UnitLine line,int stage,WorldPosition from,IReadOnlyList<WorldPosition> targets,double? specialDuration=null,double? height=null)=>Playback.Demo(line,stage,from,targets,height,specialDuration);
    public void Advance(double delta)=>Playback.Update(delta);
    public int Draw(ModelRenderer models,Matrix4x4 camera,int width,int height,bool includeLit=true)=>renderer.Draw(models,camera,Effects,width,height,includeLit);
    public int DrawLit(ModelRenderer models,Matrix4x4 camera,bool transparent)=>renderer.DrawLit(models,camera,Effects,transparent);
    public void Clear()=>Playback.Clear();
    public void Dispose(){Clear();renderer.Dispose();}
}

internal sealed class MoveEffectRenderer : IDisposable
{
    private static readonly string folder=Path.Combine(AppContext.BaseDirectory,"content/move-effects");
    private readonly Dictionary<string,GpuPrimitive> geometry=[];
    private readonly Dictionary<string,GpuTexture> textures=[];
    private readonly Dictionary<string,MoveGeometryBatch> batches=[];
    private readonly Dictionary<MoveRibbon,(GpuPrimitive Geometry,float[] Vertices)> ribbonGeometry=[];
    private readonly MoveSpriteRenderer sprites;
    private static readonly string[] batchNames=["glow","shock","ring","crescent","claw","ball","pillar"];
    private static readonly Dictionary<string,(string Color,string Emission)> spikeLooks=new(){["ice"]=("#cdf3ff","#2a7fb0"),["rock"]=("#8a7458","#1a1208"),["thorn"]=("#4f9a3a","#0d2a08"),["dark"]=("#4a2a6a","#3a0a5a"),["metal"]=("#b8c0cc","#202833")};
    public MoveEffectRenderer()
    {
        using var doc=JsonDocument.Parse(File.ReadAllBytes(Path.Combine(folder,"geometry.json")));
        var data=new Dictionary<string,GltfPrimitive>();
        foreach(var entry in doc.RootElement.EnumerateObject())
        {var g=new GltfPrimitive(entry.Value.GetProperty("vertices"u8).EnumerateArray().Select(x=>x.GetSingle()).ToArray(),entry.Value.GetProperty("indices"u8).EnumerateArray().Select(x=>x.GetUInt32()).ToArray(),-1,true,false);data.Add(entry.Name,g);geometry.Add(entry.Name,new(g));}
        {
            using var legacy=JsonDocument.Parse(File.ReadAllBytes(Path.Combine(folder,"legacy-geometry.json")));
            foreach(var entry in legacy.RootElement.EnumerateObject())geometry.Add(entry.Name,new(new(entry.Value.GetProperty("vertices"u8).EnumerateArray().Select(x=>x.GetSingle()).ToArray(),entry.Value.GetProperty("indices"u8).EnumerateArray().Select(x=>x.GetUInt32()).ToArray(),-1,true,false)));
        }
        foreach(string path in Directory.GetFiles(folder,"*.png")){string name=Path.GetFileNameWithoutExtension(path);textures.Add(name,new(PngImage.Decode(File.ReadAllBytes(path)),new(MinFilter:name=="atlas"?9729:9987,WrapS:name=="beam"?10497:33071,WrapT:33071),true));}
        foreach(string name in batchNames)batches.Add(name,new(data[name switch{"glow"=>"glow","ball"=>"sphere","pillar"=>"pillar",_=>"plane"}],name=="glow"?768:name is "claw" or "pillar"?128:256));
        sprites=new(textures["atlas"]);
    }
    public unsafe int Draw(ModelRenderer renderer,Matrix4x4 camera,MoveEffects effects,int width,int height,bool includeLit=true)
    {
        int calls=0;OpenGl.Native.glEnable(GlCapability.DepthTest);OpenGl.Native.glDisable(GlCapability.CullFace);OpenGl.Native.glDisable(GlCapability.ScissorTest);
        var facing=Facing(camera);var cameraEye=new Vector3((float)effects.CameraPosition.X,(float)effects.CameraPosition.Y,(float)effects.CameraPosition.Z);
        try
        {
            // Source render orders: lit pieces 0, alpha sprites 6, additive meshes 7, additive sprites 8.
            if(includeLit){calls+=DrawLit(renderer,camera,effects,false);renderer.BeginTransparency();try{calls+=DrawLit(renderer,camera,effects,true);renderer.FlushTransparency();}finally{renderer.ClearTransparency();}}
            calls+=sprites.Draw(effects.Alpha,false,camera,width,height);
            foreach(var batch in batches.Values)batch.Clear();
            foreach(var piece in effects.Pieces)if(batches.TryGetValue(piece.Kind,out var batch))
            {
                Quaternion q=piece.Face switch{0=>facing,1=>Quaternion.CreateFromAxisAngle(Vector3.UnitX,-MathF.PI/2),null=>Quaternion.Identity,_=>Quaternion.CreateFromAxisAngle(Vector3.UnitY,(float)piece.Face.Value)};
                if(piece.Rotation!=0)q=Quaternion.Multiply(q,Quaternion.CreateFromAxisAngle(Vector3.UnitZ,(float)piece.Rotation));
                batch.Add(World(piece.X,piece.Y,piece.Z,q,piece.Sx,piece.Sy,piece.Sz),piece.Color,piece.Alpha);
            }
            foreach(string name in batchNames)calls+=batches[name].Draw(camera,name=="ball"?null:textures.GetValueOrDefault(name),renderer.EliminationStage||renderer.EvolutionStage||renderer.CosmeticStage?null:renderer.BoardLight);
            foreach(var mesh in effects.Meshes)if(mesh.Visible&&!Early(mesh.Kind))calls+=DrawMesh(renderer,camera,mesh,facing);
            foreach(var ribbon in effects.Ribbons)if(ribbon.Visible&&ribbon.Count>1){DrawRibbon(ribbon);calls++;}
            calls+=sprites.Draw(effects.Add,true,camera,width,height);
        }
        finally {ModelRenderer.End();}
        return calls;

        void DrawRibbon(MoveRibbon ribbon)
        {
            if(!ribbonGeometry.TryGetValue(ribbon,out var buffer))
            {
                float[] vertices=new float[48*GltfPrimitive.Stride];uint[] indices=new uint[23*6];for(int i=0;i<23;i++){uint a=(uint)i*2;int j=i*6;indices[j]=a;indices[j+1]=a+1;indices[j+2]=a+2;indices[j+3]=a+1;indices[j+4]=a+3;indices[j+5]=a+2;}
                buffer=(new(new(vertices,indices,-1,false,false)),vertices);ribbonGeometry.Add(ribbon,buffer);
            }
            for(int i=0;i<24;i++)
            {
                int j=Math.Min(i,ribbon.Count-1);var p=ribbon.Points[j];var a=ribbon.Points[Math.Max(0,j-1)];var b=ribbon.Points[Math.Min(ribbon.Count-1,j+1)];
                var point=new Vector3((float)p.X,(float)p.Y,(float)p.Z);var tangent=new Vector3((float)(b.X-a.X),(float)(b.Y-a.Y),(float)(b.Z-a.Z));var side=Vector3.Cross(tangent,cameraEye-point);float length=side.Length();if(length==0)length=1;
                double k=(double)j/(ribbon.Count-1),w=ribbon.Width*(1-ribbon.Taper*.85*Math.Pow(Math.Abs(k*2-1),3))*.5;side*=((float)w/length);
                Put(i*2,point+side);Put(i*2+1,point-side);
            }
            buffer.Geometry.UpdateVertices(buffer.Vertices);var c=ribbon.Color;
            renderer.DrawRoom(buffer.Geometry,Matrix4D.Identity,camera,new((float)(c.R*ribbon.Brightness),(float)(c.G*ribbon.Brightness),(float)(c.B*ribbon.Brightness),1),null,true,true,false,additive:true);
            void Put(int index,Vector3 p){int offset=index*GltfPrimitive.Stride;buffer.Vertices[offset]=p.X;buffer.Vertices[offset+1]=p.Y;buffer.Vertices[offset+2]=p.Z;}
        }
    }
    public int DrawLit(ModelRenderer renderer,Matrix4x4 camera,MoveEffects effects,bool transparent)
    {
        int calls=0;
        foreach(var piece in effects.Pieces)if(piece.Kind.StartsWith("spike_",StringComparison.Ordinal)&&(piece.Kind=="spike_ice")==transparent)
        {
            string look=piece.Kind[6..];if(!spikeLooks.TryGetValue(look,out var material))material=spikeLooks["ice"];
            var color=MoveColor.Hex(material.Color);var emission=MoveColor.Hex(material.Emission);var q=Euler(piece.Rx,piece.Ry,piece.Rz,"XYZ");
            renderer.DrawRoom(geometry["spike"],World(piece.X,piece.Y,piece.Z,q,piece.Sx,piece.Sy,piece.Sz),camera,Color(color,transparent?.88:1),null,false,transparent,true,Vector(emission),.25f,.1f,side:0,flatShading:true,receiveShadow:false);calls++;
        }
        if(transparent){var facing=Facing(camera);foreach(var mesh in effects.Meshes)if(mesh.Visible&&Early(mesh.Kind))calls+=DrawMesh(renderer,camera,mesh,facing);}
        return calls;
    }
    private int DrawMesh(ModelRenderer renderer,Matrix4x4 camera,MoveMesh mesh,Quaternion facing)
    {
        string geo=mesh.Kind switch{"sphere" or "body" or "beam" or "cone" or "ring" or "torus" or "tri" or "pillar" or "spike" or "legacyBeam" or "legacyBurst" or "legacyShield"=>mesh.Kind,"shock" or "swirl" or "crossFlat"=>"flat","glow"=>"glow",_=>"plane"};
        string tex=mesh.Kind switch{"swirlUp"=>"swirl","crossFlat"=>"cross",_=>mesh.Kind};
        Quaternion q=mesh.Billboard||mesh.Kind=="glow"?facing:mesh.LookTarget is {} target?Look(mesh.Position,target):Euler(mesh.Rotation.X,mesh.Rotation.Y,mesh.Rotation.Z,mesh.RotationOrder);bool lit=mesh.Kind is "body" or "spike";
        // Three renders transparent double-sided cylinders back then front. The
        // legacy beam writes depth, so one unordered two-sided pass changes its blend.
        bool beam=mesh.Kind=="legacyBeam";
        if(beam)Draw(1);
        Draw(beam||lit||mesh.Kind=="legacyShield"?0:2);
        return beam?2:1;
        void Draw(int side)=>renderer.DrawRoom(geometry[geo],World(mesh.Position.X,mesh.Position.Y,mesh.Position.Z,q,mesh.Scale.X,mesh.Scale.Y,mesh.Scale.Z),camera,Color(mesh.Color,mesh.Opacity),textures.GetValueOrDefault(tex),!lit,true,lit||mesh.Kind is "legacyBeam" or "legacyBurst",Vector(mesh.Emissive),mesh.Kind=="spike"?.25f:lit?.5f:-1,mesh.Kind=="spike"?.1f:lit?.3f:0,side:side,additive:!Early(mesh.Kind),flatShading:mesh.Kind=="spike",receiveShadow:false);
    }
    private static bool Early(string kind)=>kind is "body" or "spike" or "legacyBeam" or "legacyBurst" or "legacyShield";
    private static Vector3 Vector(MoveColor c)=>new((float)c.R,(float)c.G,(float)c.B);
    private static Brutal.Numerics.float4 Color(MoveColor c,double a)=>new((float)c.R,(float)c.G,(float)c.B,(float)a);
    private static Quaternion Facing(Matrix4x4 camera)
    {var right=Vector3.Normalize(new(camera.M11,camera.M21,camera.M31));var up=Vector3.Normalize(new(camera.M12,camera.M22,camera.M32));var back=Vector3.Cross(right,up);return Quaternion.CreateFromRotationMatrix(new(right.X,right.Y,right.Z,0,up.X,up.Y,up.Z,0,back.X,back.Y,back.Z,0,0,0,0,1));}
    private static Quaternion Look(MoveVector from,MoveVector target)
    {var z=new Vector3((float)(target.X-from.X),(float)(target.Y-from.Y),(float)(target.Z-from.Z));if(z.LengthSquared()<1e-20f)z=Vector3.UnitZ;z=Vector3.Normalize(z);var x=Vector3.Cross(Vector3.UnitY,z);if(x.LengthSquared()<1e-12f){z.X+=.0001f;z=Vector3.Normalize(z);x=Vector3.Cross(Vector3.UnitY,z);}x=Vector3.Normalize(x);var y=Vector3.Cross(z,x);return Quaternion.CreateFromRotationMatrix(new(x.X,x.Y,x.Z,0,y.X,y.Y,y.Z,0,z.X,z.Y,z.Z,0,0,0,0,1));}
    private static Quaternion Euler(double x,double y,double z,string order)
    {var qx=Quaternion.CreateFromAxisAngle(Vector3.UnitX,(float)x);var qy=Quaternion.CreateFromAxisAngle(Vector3.UnitY,(float)y);var qz=Quaternion.CreateFromAxisAngle(Vector3.UnitZ,(float)z);return order=="YXZ"?qy*qx*qz:qx*qy*qz;}
    private static Matrix4D World(double x,double y,double z,Quaternion q,double sx,double sy,double sz)=>Matrix4D.Compose(new(x,y,z),new(q.X,q.Y,q.Z,q.W),new(sx,sy,sz));
    public void Dispose(){sprites.Dispose();foreach(var batch in batches.Values)batch.Dispose();foreach(var geometry in geometry.Values)geometry.Dispose();foreach(var texture in textures.Values)texture.Dispose();foreach(var ribbon in ribbonGeometry.Values)ribbon.Geometry.Dispose();}
}

/// <summary>Instanced glow/core batches; one upload and draw per source kind.</summary>
internal sealed class MoveGeometryBatch : IDisposable
{
    private readonly GlProgram program;
    private GlVertexArray array;
    private readonly GlBuffer[] buffers=new GlBuffer[3];
    private readonly float[] instances;
    private readonly int elements;
    private int count;
    public unsafe MoveGeometryBatch(GltfPrimitive geometry,int capacity)
    {
        instances=new float[capacity*20];elements=geometry.Indices.Length;
        using var vertex=OpenGl.CreateShader(GlShaderType.Vertex).SetSource(Vertex).Compile();using var fragment=OpenGl.CreateShader(GlShaderType.Fragment).SetSource(Fragment).Compile();program=OpenGl.CreateProgram();program.Attach(vertex).Attach(fragment).Link();
        Span<GlVertexArray> arrays=stackalloc GlVertexArray[1];OpenGl.CreateVertexArrays(arrays);array=arrays[0];OpenGl.CreateBuffers(buffers);
        fixed(float* data=geometry.Vertices)OpenGl.NamedBufferData(buffers[0],geometry.Vertices.Length*sizeof(float),data,OpenGl.GL_STATIC_DRAW);
        fixed(uint* data=geometry.Indices)OpenGl.NamedBufferData(buffers[1],geometry.Indices.Length*sizeof(uint),data,OpenGl.GL_STATIC_DRAW);
        OpenGl.NamedBufferData(buffers[2],instances.Length*sizeof(float),null,OpenGl.GL_DYNAMIC_DRAW);
        OpenGl.VertexArrayVertexBuffer(array.Id,0,buffers[0],0,GltfPrimitive.Stride*sizeof(float));OpenGl.VertexArrayElementBuffer(array.Id,buffers[1]);
        OpenGl.EnableVertexArrayAttrib(array.Id,0);OpenGl.VertexArrayAttribBinding(array.Id,0,0);OpenGl.VertexArrayAttribFormat(array.Id,0,3,OpenGl.GL_FLOAT,false,0);
        OpenGl.EnableVertexArrayAttrib(array.Id,1);OpenGl.VertexArrayAttribBinding(array.Id,1,0);OpenGl.VertexArrayAttribFormat(array.Id,1,2,OpenGl.GL_FLOAT,false,6*sizeof(float));
    }
    public void Clear()=>count=0;
    public void Add(Matrix4D world,MoveColor color,double alpha)
    {if(count>=instances.Length/20)return;Span<double> matrix=stackalloc double[16];world.CopyTo(matrix);int offset=count++*20;for(int i=0;i<16;i++)instances[offset+i]=(float)matrix[i];instances[offset+16]=(float)(color.R*alpha);instances[offset+17]=(float)(color.G*alpha);instances[offset+18]=(float)(color.B*alpha);instances[offset+19]=1;}
    public unsafe int Draw(Matrix4x4 camera,GpuTexture? texture,BoardLighting? fog)
    {
        if(count==0)return 0;program.Use();MoveGpu.Camera(program,camera);program.Uniform(4,texture is null?0:1);OpenGl.BindTextureUnit(0,texture?.Id??0);
        var color=fog is null?Vector3.Zero:BoardLighting.Srgb(fog.FogColor);program.Uniform(5,new Brutal.Numerics.float4(color.X,color.Y,color.Z,fog is null?0:1));program.Uniform(6,new Brutal.Numerics.float4(fog?.FogNear??0,fog?.FogFar??1,0,0));
        fixed(float* data=instances)OpenGl.NamedBufferSubData(buffers[2],0,count*20*sizeof(float),data);OpenGl.BindBufferBase(OpenGl.GL_SHADER_STORAGE_BUFFER,1,buffers[2]);
        OpenGl.Native.glDepthMask(false);OpenGl.Native.glDisable(GlCapability.CullFace);OpenGl.Native.glEnable(GlCapability.Blend);OpenGl.Native.glBlendFunc(OpenGl.GL_SRC_ALPHA,OpenGl.GL_ONE);array.Bind();OpenGl.Native.glDrawElementsInstanced(OpenGl.GL_TRIANGLES,elements,OpenGl.GL_UNSIGNED_INT,null,count);return 1;
    }
    public void Dispose(){OpenGl.DeleteBuffers(buffers);OpenGl.DeleteVertexArrays(new[]{array});program.Dispose();}
    private const string Vertex="""
        #version 450 core
        layout(location=0) in vec3 position; layout(location=1) in vec2 uv;
        layout(location=0) uniform mat4 camera;
        struct Instance {mat4 world;vec4 color;};layout(std430,binding=1) readonly buffer Instances {Instance items[];};
        out vec2 texCoord;out vec4 tint;out float eyeDepth;
        void main(){Instance i=items[gl_InstanceID];gl_Position=camera*i.world*vec4(position,1);eyeDepth=gl_Position.w;gl_Position.z=2*gl_Position.z-gl_Position.w;texCoord=uv;tint=i.color;}
        """;
    private const string Fragment="""
        #version 450 core
        layout(binding=0) uniform sampler2D image;layout(location=4) uniform int textured;layout(location=5) uniform vec4 fogColor;layout(location=6) uniform vec4 fogRange;in vec2 texCoord;in vec4 tint;in float eyeDepth;out vec4 outputColor;
        void main(){vec4 c=tint*(textured!=0?texture(image,texCoord):vec4(1));vec3 rgb=max(c.rgb,vec3(0));rgb=mix(12.92*rgb,1.055*pow(rgb,vec3(1.0/2.4))-.055,step(vec3(.0031308),rgb));if(fogColor.w!=0)rgb=mix(rgb,fogColor.rgb,smoothstep(fogRange.x,fogRange.y,eyeDepth));outputColor=vec4(rgb,c.a);}
        """;
}

internal static class MoveGpu
{
    public static unsafe void Camera(GlProgram program,Matrix4x4 m)
    {float* data=stackalloc float[16]{m.M11,m.M12,m.M13,m.M14,m.M21,m.M22,m.M23,m.M24,m.M31,m.M32,m.M33,m.M34,m.M41,m.M42,m.M43,m.M44};OpenGl.Native.glProgramUniformMatrix4fv(program,0,1,false,data);}
}
