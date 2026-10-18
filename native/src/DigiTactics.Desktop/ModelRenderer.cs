using System.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Assets;
namespace DigiTactics.Desktop;

internal sealed class ModelRenderer : IDisposable
{
    private readonly GlProgram program;
    private GlBuffer bones;
    private Matrix4D[] palette = [];
    private float[] paletteFloats = [];
    public int DrawCalls { get; private set; }
    public ModelRenderer()
    {
        using var vertex = OpenGl.CreateShader(GlShaderType.Vertex).SetSource(VertexShader).Compile();
        using var fragment = OpenGl.CreateShader(GlShaderType.Fragment).SetSource(FragmentShader).Compile();
        program = OpenGl.CreateProgram();
        try { program.Attach(vertex).Attach(fragment).Link(); }
        catch { program.Dispose(); throw; }
        Span<GlBuffer> buffers = stackalloc GlBuffer[1]; OpenGl.CreateBuffers(buffers); bones = buffers[0];
    }


    public static unsafe void Begin(int width, int height)
    {
        OpenGl.Viewport(0, 0, width, height);
        OpenGl.Native.glEnable(GlCapability.DepthTest);
        OpenGl.Native.glDisable(GlCapability.ScissorTest);
        OpenGl.Native.glDisable((GlCapability)OpenGl.GL_FRAMEBUFFER_SRGB);
        OpenGl.Native.glDepthMask(true); OpenGl.Clear(GlClearBit.Depth);
    }
    public unsafe void Draw(GpuModel model, RigPose pose, Matrix4D placement, Matrix4x4 camera, bool transparent, double emission = 0)
    {
        var rig = model.Rig; var mesh = model.Mesh; var materials = model.Materials; var textures = model.Textures; var primitives = model.Primitives;
        int required = Math.Max(1, rig.Skins.Select(s => s.Joints.Length).DefaultIfEmpty(0).Max());
        if (palette.Length < required)
        {
            palette = new Matrix4D[required]; paletteFloats = new float[required * 16];
            OpenGl.NamedBufferData(bones, paletteFloats.Length * sizeof(float), null, OpenGl.GL_DYNAMIC_DRAW);
        }
        program.Use(); OpenGl.BindBufferBase(OpenGl.GL_SHADER_STORAGE_BUFFER, 0, bones);
        Matrix(4, From(camera)); program.Uniform(14, (float)emission); DrawCalls = 0;
        // The caller controls opaque and transparent passes across all models.
        foreach (var instance in mesh.Instances)
        {
            int offset = 0; for (int i = 0; i < instance.Mesh; i++) offset += mesh.Meshes[i].Length;
            if (instance.Skin >= 0)
            {
                int count = rig.Skins[instance.Skin].Joints.Length;
                pose.SkinMatrices(instance.Skin, instance.Node, palette.AsSpan(0, count));
                for (int i = 0; i < count; i++) Copy(palette[i], paletteFloats.AsSpan(i * 16, 16));
                fixed (float* data = paletteFloats) OpenGl.NamedBufferSubData(bones, 0, count * 16 * sizeof(float), data);
            }
            var world = pose.World[instance.Node] * placement;
            double determinant = world.M11 * (world.M22 * world.M33 - world.M23 * world.M32)
                - world.M12 * (world.M21 * world.M33 - world.M23 * world.M31) + world.M13 * (world.M21 * world.M32 - world.M22 * world.M31);
            OpenGl.Native.glFrontFace(determinant < 0 ? GlFrontFace.Clockwise : GlFrontFace.CounterClockwise);
            Matrix(0, world);
            for (int i = 0; i < mesh.Meshes[instance.Mesh].Length; i++)
            {
                var geometry = mesh.Meshes[instance.Mesh][i];
                var material = materials[geometry.Material];
                if (material.Transparent != transparent) continue;
                program.Uniform(8, instance.Skin >= 0 && geometry.HasSkin ? 1 : 0);
                program.Uniform(9, geometry.HasNormals ? 1 : 0);
                program.Uniform(10, material.Unlit ? 1 : 0);
                program.Uniform(11, (float)material.AlphaCutoff);
                program.Uniform(12, new Brutal.Numerics.float4((float)material.Color.R, (float)material.Color.G, (float)material.Color.B, (float)material.Color.A));
                program.Uniform(13, material.ColorMap is null ? 0 : 1);
                OpenGl.BindTextureUnit(0, material.ColorMap is { } map ? textures[map.Texture].Id : 0);
                if (material.DoubleSided) OpenGl.Native.glDisable((GlCapability)OpenGl.GL_CULL_FACE); else OpenGl.Native.glEnable((GlCapability)OpenGl.GL_CULL_FACE);
                if (transparent) { OpenGl.Native.glEnable((GlCapability)OpenGl.GL_BLEND); OpenGl.Native.glBlendFunc(OpenGl.GL_SRC_ALPHA, OpenGl.GL_ONE_MINUS_SRC_ALPHA); }
                else OpenGl.Native.glDisable((GlCapability)OpenGl.GL_BLEND);
                OpenGl.Native.glDepthMask(material.DepthWrite);
                primitives[offset + i].Draw(); DrawCalls++;
            }
        }

    }
    public static unsafe void End()
    {
        OpenGl.Native.glDepthMask(true); OpenGl.Native.glFrontFace(GlFrontFace.CounterClockwise);
        OpenGl.Native.glDisable(GlCapability.DepthTest); OpenGl.Native.glDisable(GlCapability.CullFace); OpenGl.Native.glDisable(GlCapability.Blend);
        OpenGl.Native.glBindVertexArray(default); OpenGl.Native.glUseProgram(default);
        if (OpenGl.GetError() != OpenGl.GL_NO_ERROR) throw new InvalidOperationException("Native model rendering failed.");
    }
    public unsafe void DrawStatic(GpuPrimitive geometry, Matrix4D world, Matrix4x4 camera, Brutal.Numerics.float4 color)
    {
        program.Use(); Matrix(0, world); Matrix(4, From(camera));
        program.Uniform(8, 0); program.Uniform(9, 1); program.Uniform(10, 1); program.Uniform(11, 0f);
        program.Uniform(12, color); program.Uniform(13, 0); program.Uniform(14, 0f);
        OpenGl.Native.glDisable(GlCapability.CullFace); OpenGl.Native.glDisable(GlCapability.Blend); OpenGl.Native.glDepthMask(true);
        geometry.Draw();
    }
    public void Dispose() { OpenGl.DeleteBuffers(new[] { bones }); program.Dispose(); }
    public unsafe void DrawRoom(GpuPrimitive geometry, Matrix4D world, Matrix4x4 camera, Brutal.Numerics.float4 color, GpuTexture? texture, bool unlit, bool transparent, bool depthWrite)
    {
        program.Use(); Matrix(0, world); Matrix(4, From(camera));
        program.Uniform(8, 0); program.Uniform(9, 1); program.Uniform(10, unlit ? 1 : 0); program.Uniform(11, 0f);
        program.Uniform(12, color); program.Uniform(13, texture is null ? 0 : 1); program.Uniform(14, 0f);
        OpenGl.BindTextureUnit(0, texture?.Id ?? 0); OpenGl.Native.glDisable(GlCapability.CullFace);
        if (transparent) { OpenGl.Native.glEnable(GlCapability.Blend); OpenGl.Native.glBlendFunc(OpenGl.GL_SRC_ALPHA, OpenGl.GL_ONE_MINUS_SRC_ALPHA); }
        else OpenGl.Native.glDisable(GlCapability.Blend);
        OpenGl.Native.glDepthMask(depthWrite); geometry.Draw();
    }
    private unsafe void Matrix(int location, Matrix4D value)
    {
        Span<float> values = stackalloc float[16]; Copy(value, values);
        fixed (float* data = values) OpenGl.Native.glProgramUniformMatrix4fv(program, location, 1, false, data);
    }
    private static void Copy(Matrix4D value, Span<float> result)
    {
        Span<double> values = stackalloc double[16]; value.CopyTo(values);
        for (int i = 0; i < 16; i++) result[i] = (float)values[i];
    }
    private static Matrix4D From(Matrix4x4 m) => new(m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44);
    private const string VertexShader = """
        #version 450 core
        layout(location=0) in vec3 position;
        layout(location=1) in vec3 normal;
        layout(location=2) in vec2 uv;
        layout(location=3) in vec4 joints;
        layout(location=4) in vec4 weights;
        layout(std430,binding=0) readonly buffer Bones { mat4 bones[]; };
        layout(location=0) uniform mat4 world;
        layout(location=4) uniform mat4 camera;
        layout(location=8) uniform int skinned;
        out vec2 texCoord; out vec3 worldPosition; out vec3 worldNormal;
        void main() {
            mat4 skin = mat4(1);
            if(skinned!=0) skin=bones[int(joints.x)]*weights.x+bones[int(joints.y)]*weights.y+bones[int(joints.z)]*weights.z+bones[int(joints.w)]*weights.w;
            vec4 p=world*skin*vec4(position,1);
            worldPosition=p.xyz; worldNormal=transpose(inverse(mat3(world)))*mat3(skin)*normal;
            texCoord=uv; gl_Position=camera*p;
            gl_Position.z=2*gl_Position.z-gl_Position.w;
        }
        """;
    private const string FragmentShader = """
        #version 450 core
        in vec2 texCoord; in vec3 worldPosition; in vec3 worldNormal;
        layout(binding=0) uniform sampler2D colorTexture;
        layout(location=9) uniform int hasNormals;
        layout(location=10) uniform int unlit;
        layout(location=11) uniform float cutoff;
        layout(location=12) uniform vec4 color;
        layout(location=13) uniform int textured;
        layout(location=14) uniform float emission;
        out vec4 outputColor;
        void main() {
            vec4 c=color*(textured!=0?texture(colorTexture,texCoord):vec4(1));
            if(c.a<cutoff) discard;
            vec3 n=normalize(hasNormals!=0?worldNormal:cross(dFdx(worldPosition),dFdy(worldPosition)));
            if(!gl_FrontFacing) n=-n;
            vec3 light=unlit!=0?vec3(1):vec3(.35)+vec3(.65)*max(dot(n,normalize(vec3(2,4,3))),0);
            vec3 linear=max(c.rgb*light+vec3(unlit!=0?0:emission),vec3(0));
            vec3 srgb=mix(12.92*linear,1.055*pow(linear,vec3(1.0/2.4))-.055,step(vec3(.0031308),linear));
            outputColor=vec4(srgb,c.a);
        }
        """;
}
