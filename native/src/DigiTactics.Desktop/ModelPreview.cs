using System.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

/// <summary>Native model workbench. Shared geometry stays on the GPU; poses update only bone palettes.</summary>
internal sealed class ModelPreview : IDisposable
{
    private readonly ModelCatalog catalog = ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/models.json"));
    private readonly GlProgram program;
    private GlBuffer bones;
    private readonly List<Primitive> primitives = [];
    private readonly Dictionary<int, GpuTexture> textures = [];
    private GltfRig? rig;
    private RigPose? pose;
    private GltfMesh? mesh;
    private GltfMaterial[] materials = [];
    private RigAnimation? idle;
    private ModelSource? source;
    private Matrix4D normalization;
    private Matrix4D[] palette = [];
    private float[] paletteFloats = [];
    public int DrawCalls { get; private set; }

    public ModelPreview()
    {
        using var vertex = OpenGl.CreateShader(GlShaderType.Vertex).SetSource(VertexShader).Compile();
        using var fragment = OpenGl.CreateShader(GlShaderType.Fragment).SetSource(FragmentShader).Compile();
        program = OpenGl.CreateProgram();
        try { program.Attach(vertex).Attach(fragment).Link(); }
        catch { program.Dispose(); throw; }
        Span<GlBuffer> buffers = stackalloc GlBuffer[1]; OpenGl.CreateBuffers(buffers); bones = buffers[0];
    }

    public unsafe void Select(string variant, string line, int stage)
    {
        var next = catalog.Resolve(variant, line, stage);
        if (next == source) return;
        ClearModel(); source = next;
        if (next is null) return;
        try
        {
            using var asset = catalog.Open(Path.Combine(AppContext.BaseDirectory, "assets"), next.Asset);
            rig = GltfRig.Load(asset).Prepare(next, Path.Combine(AppContext.BaseDirectory, "assets"));
            pose = new RigPose(rig); mesh = GltfMesh.Load(asset);
            idle = rig.Animations.FirstOrDefault(a => System.Text.RegularExpressions.Regex.IsMatch(a.Name, "idle|wait|stand", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                ?? rig.Animations.FirstOrDefault(a => a.Name.Contains("_bn01", StringComparison.OrdinalIgnoreCase)) ?? rig.Animations.FirstOrDefault();
            pose.Sample(idle, 0);
            var (min, max) = mesh.Bounds(rig, pose);
            double scale = 2 / Math.Max(.001, Math.Max(max.Y - min.Y, Math.Max(max.X - min.X, max.Z - min.Z)));
            normalization = Matrix4D.Compose(new(-(min.X + max.X) * .5 * scale, -min.Y * scale, -(min.Z + max.Z) * .5 * scale), new(0,0,0,1), new(scale,scale,scale));
            bool frontOnly = next.Metadata.TryGetProperty("frontOnly"u8, out var front) && front.GetBoolean();
            materials = asset.Root.GetProperty("materials"u8).EnumerateArray().Select((_, i) => GltfMaterial.Load(asset, i, frontOnly)).ToArray();
            foreach (var material in materials)
                if (material.ColorMap is { } binding && !textures.ContainsKey(binding.Texture))
                {
                    var texture = GltfTexture.Load(asset, binding.Texture);
                    textures.Add(binding.Texture, new(asset.Image(texture.Image), texture.Sampler, binding.Srgb));
                }
            foreach (var group in mesh.Meshes) foreach (var primitive in group) primitives.Add(new(primitive));
            int size = Math.Max(1, rig.Skins.Select(s => s.Joints.Length).DefaultIfEmpty(0).Max());
            palette = new Matrix4D[size]; paletteFloats = new float[size * 16];
            OpenGl.NamedBufferData(bones, paletteFloats.Length * sizeof(float), null, OpenGl.GL_DYNAMIC_DRAW);
        }
        catch { ClearModel(); source = null; throw; }
    }

    public unsafe void Draw(int width, int height, double time)
    {
        DrawCalls = 0;
        if (rig is null || pose is null || mesh is null || width <= 0 || height <= 0) return;
        pose.Sample(idle, time, loop: true);
        OpenGl.Viewport(0, 0, width, height);
        OpenGl.Native.glEnable((GlCapability)OpenGl.GL_DEPTH_TEST);
        OpenGl.Native.glDisable((GlCapability)OpenGl.GL_SCISSOR_TEST);
        OpenGl.Native.glDisable((GlCapability)OpenGl.GL_FRAMEBUFFER_SRGB);
        OpenGl.Native.glDepthMask(true);
        OpenGl.Clear(GlClearBit.Depth);
        program.Use(); OpenGl.BindBufferBase(OpenGl.GL_SHADER_STORAGE_BUFFER, 0, bones);
        var camera = Matrix4x4.CreateLookAt(new(3, 2, 5), new(0, .9f, 0), Vector3.UnitY)
            * Matrix4x4.CreatePerspectiveFieldOfView(.65f, (float)width / height, .01f, 100);
        Matrix(4, From(camera));
        double rotation = source!.Metadata.TryGetProperty("rotY"u8, out var value) ? value.GetDouble() : 0;
        var turn = Matrix4D.Compose(new(), new(0, Math.Sin(rotation / 2), 0, Math.Cos(rotation / 2)), new(1,1,1));
        // Opaque/cutout surfaces first; preview transparency is not yet production depth-sorted.
        foreach (bool transparent in new[] { false, true })
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
                var world = pose.World[instance.Node] * normalization * turn;
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
        OpenGl.Native.glDepthMask(true);
        OpenGl.Native.glFrontFace(GlFrontFace.CounterClockwise);
        OpenGl.Native.glDisable((GlCapability)OpenGl.GL_DEPTH_TEST); OpenGl.Native.glDisable((GlCapability)OpenGl.GL_CULL_FACE); OpenGl.Native.glDisable((GlCapability)OpenGl.GL_BLEND);
        OpenGl.Native.glBindVertexArray(default); OpenGl.Native.glUseProgram(default);
        if (OpenGl.GetError() != OpenGl.GL_NO_ERROR) throw new InvalidOperationException("Native model draw failed.");
    }

    public static unsafe void VerifyVisible(int width, int height)
    {
        var bytes = new byte[checked(width * height * 4)];
        fixed (byte* pixels = bytes) OpenGl.ReadPixels(0, 0, width, height, OpenGl.GL_RGBA, OpenGl.GL_UNSIGNED_BYTE, pixels);
        int visible = 0;
        for (int i = 0; i < bytes.Length; i += 4)
            if (bytes[i] > 30 || bytes[i + 1] > 30 || bytes[i + 2] > 40) visible++;
        if (visible < 200) throw new InvalidOperationException("Native model produced no visible surface.");
        Console.WriteLine($"Native model smoke passed: {visible} visible pixels.");
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
    private static Matrix4D From(Matrix4x4 m) => new(m.M11,m.M12,m.M13,m.M14,m.M21,m.M22,m.M23,m.M24,m.M31,m.M32,m.M33,m.M34,m.M41,m.M42,m.M43,m.M44);
    private void ClearModel()
    {
        foreach (var primitive in primitives) primitive.Dispose(); primitives.Clear();
        foreach (var texture in textures.Values) texture.Dispose(); textures.Clear();
        rig = null; pose = null; mesh = null;
    }
    public void Dispose() { ClearModel(); OpenGl.DeleteBuffers(new[] { bones }); bones = default; program.Dispose(); }

    private sealed class Primitive : IDisposable
    {
        private GlVertexArray array;
        private readonly GlBuffer[] buffers = new GlBuffer[2];
        private readonly int count;
        public unsafe Primitive(GltfPrimitive data)
        {
            count = data.Indices.Length;
            Span<GlVertexArray> arrays = stackalloc GlVertexArray[1]; OpenGl.CreateVertexArrays(arrays); array = arrays[0];
            try
            {
                OpenGl.CreateBuffers(buffers);
                fixed (float* vertices = data.Vertices) OpenGl.NamedBufferData(buffers[0], data.Vertices.Length * sizeof(float), vertices, OpenGl.GL_STATIC_DRAW);
                fixed (uint* indices = data.Indices) OpenGl.NamedBufferData(buffers[1], data.Indices.Length * sizeof(uint), indices, OpenGl.GL_STATIC_DRAW);
                OpenGl.VertexArrayVertexBuffer(array.Id, 0, buffers[0], 0, GltfPrimitive.Stride * sizeof(float));
                OpenGl.VertexArrayElementBuffer(array.Id, buffers[1]);
                int[] sizes = [3,3,2,4,4]; uint offset = 0;
                for (uint i = 0; i < sizes.Length; i++)
                {
                    OpenGl.EnableVertexArrayAttrib(array.Id, i); OpenGl.VertexArrayAttribBinding(array.Id, i, 0);
                    OpenGl.VertexArrayAttribFormat(array.Id, i, sizes[i], OpenGl.GL_FLOAT, false, offset);
                    offset += (uint)sizes[i] * sizeof(float);
                }
            }
            catch { Dispose(); throw; }
        }
        public unsafe void Draw() { array.Bind(); OpenGl.Native.glDrawElements(OpenGl.GL_TRIANGLES, count, OpenGl.GL_UNSIGNED_INT, null); }
        public void Dispose() { OpenGl.DeleteBuffers(buffers); OpenGl.DeleteVertexArrays(new[] { array }); }
    }

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
        out vec4 outputColor;
        void main() {
            vec4 c=color*(textured!=0?texture(colorTexture,texCoord):vec4(1));
            if(c.a<cutoff) discard;
            vec3 n=normalize(hasNormals!=0?worldNormal:cross(dFdx(worldPosition),dFdy(worldPosition)));
            if(!gl_FrontFacing) n=-n;
            vec3 light=unlit!=0?vec3(1):vec3(.35)+vec3(.65)*max(dot(n,normalize(vec3(2,4,3))),0);
            vec3 linear=max(c.rgb*light,vec3(0));
            vec3 srgb=mix(12.92*linear,1.055*pow(linear,vec3(1.0/2.4))-.055,step(vec3(.0031308),linear));
            outputColor=vec4(srgb,c.a);
        }
        """;
}
