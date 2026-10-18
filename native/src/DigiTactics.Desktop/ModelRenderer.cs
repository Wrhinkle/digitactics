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
    public FarmLighting? FarmLight { get; set; }
    public BoardLighting? BoardLight { get; set; }
    public Vector3 FarmEye { get; set; }
    public bool ShadowPass { get; set; }
    public uint ShadowTexture { get; set; }
    public Matrix4x4 ShadowCamera { get; set; } = SceneShadowMap.FarmCamera;
    public int ShadowMapSize { get; set; } = 1024;
    public float ShadowBias { get; set; } = -.0015f;
    public bool ReceiveShadow { get; set; }
    private void SetFarmLight(Vector3 emissive = default, float roughness = -1, float metalness = 0, bool fog = true)
    {
        program.Uniform(15, BoardLight is not null ? 2 : FarmLight is null ? 0 : 1);
        program.Uniform(16, new Brutal.Numerics.float4((float)(FarmLight?.Hemisphere ?? 0), (float)(FarmLight?.Sun ?? 0), (float)(FarmLight?.Lamp ?? 0), 0));
        void V(int location, Vector3 value, float w = 0) => program.Uniform(location, new Brutal.Numerics.float4(value.X,value.Y,value.Z,w));
        if (BoardLight is { } board)
        {
            V(33,board.Sky,board.HemisphereIntensity); V(34,board.Ground); V(35,board.Sun,board.SunIntensity); V(36,board.Direction);
            V(37,BoardLighting.Srgb(board.FogColor)); V(38,new(board.FogNear,board.FogFar,fog ? 1 : 0));
        }
        else V(38,Vector3.Zero);
        program.Uniform(17, new Brutal.Numerics.float4(emissive.X, emissive.Y, emissive.Z, 0));
        program.Uniform(18, new Brutal.Numerics.float4(FarmEye.X, FarmEye.Y, FarmEye.Z, 0));
        program.Uniform(19, new Brutal.Numerics.float4(roughness, metalness, 0, 0));
        program.Uniform(20, new Brutal.Numerics.float4(0, 0, 1, 0));
        OpenGl.BindTextureUnit(1, 0); OpenGl.BindTextureUnit(2, 0);
        OpenGl.BindTextureUnit(3, 0); program.Uniform(21, 0);
        OpenGl.BindTextureUnit(4, 0); program.Uniform(22, new Brutal.Numerics.float4(0, 1, 0, 0));
        program.Uniform(23, ShadowPass ? 1 : 0);
        program.Uniform(29, 0);
        program.Uniform(30, new Brutal.Numerics.float4(0,0,0,0));
        program.Uniform(24, !ShadowPass && ReceiveShadow && ShadowTexture != 0 ? 1 : 0);
        Matrix(25, From(ShadowCamera));
        program.Uniform(39, new Brutal.Numerics.float4(ShadowMapSize, ShadowBias, 0, 0));
        OpenGl.BindTextureUnit(5, ShadowPass ? 0 : ShadowTexture);
    }
    public ModelRenderer()
    {
        using var vertex = OpenGl.CreateShader(GlShaderType.Vertex).SetSource(VertexShader).Compile();
        using var fragment = OpenGl.CreateShader(GlShaderType.Fragment).SetSource(FragmentShader).Compile();
        program = OpenGl.CreateProgram();
        try { program.Attach(vertex).Attach(fragment).Link(); }
        catch { program.Dispose(); throw; }
        Span<GlBuffer> buffers = stackalloc GlBuffer[1]; OpenGl.CreateBuffers(buffers); bones = buffers[0];
    }


    public static unsafe void Begin(int width, int height, bool clearDepth = true)
    {
        OpenGl.Viewport(0, 0, width, height);
        OpenGl.Native.glEnable(GlCapability.DepthTest);
        OpenGl.Native.glDisable(GlCapability.ScissorTest);
        OpenGl.Native.glDisable((GlCapability)OpenGl.GL_FRAMEBUFFER_SRGB);
        OpenGl.Native.glDepthMask(true); if (clearDepth) OpenGl.Clear(GlClearBit.Depth);
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
        SetFarmLight();
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
                SetFarmLight(new((float)(material.Emissive.R * material.EmissiveIntensity), (float)(material.Emissive.G * material.EmissiveIntensity), (float)(material.Emissive.B * material.EmissiveIntensity)), roughness: (float)material.Roughness);
                program.Uniform(21, material.EmissiveMap is null ? 0 : 1);
                OpenGl.BindTextureUnit(3, material.EmissiveMap is { } emissiveMap ? model.Textures[emissiveMap.Texture].Id : 0);
                program.Uniform(22, new Brutal.Numerics.float4(material.NormalMap is null ? 0 : 1, (float)material.NormalScale, geometry.Tangents.Length > 0 ? 1 : 0, 0));
                OpenGl.BindTextureUnit(4, material.NormalMap is { } normalMap ? model.SurfaceTextures[normalMap.Texture].Id : 0);
                program.Uniform(20, new Brutal.Numerics.float4(material.RoughnessMap is null ? 0 : 1, material.SpecularMap is null ? 0 : 1, (float)material.SpecularIntensity, 0));
                OpenGl.BindTextureUnit(1, material.RoughnessMap is { } roughMap ? model.SurfaceTextures[roughMap.Texture].Id : 0);
                OpenGl.BindTextureUnit(2, material.SpecularMap is { } specMap ? model.SurfaceTextures[specMap.Texture].Id : 0);
                program.Uniform(8, instance.Skin >= 0 && geometry.HasSkin ? 1 : 0);
                program.Uniform(9, geometry.HasNormals ? 1 : 0);
                program.Uniform(10, material.Unlit ? 1 : 0);
                program.Uniform(11, (float)material.AlphaCutoff);
                program.Uniform(12, new Brutal.Numerics.float4((float)material.Color.R, (float)material.Color.G, (float)material.Color.B, (float)material.Color.A));
                program.Uniform(13, material.ColorMap is null ? 0 : 1);
                OpenGl.BindTextureUnit(0, material.ColorMap is { } map ? textures[map.Texture].Id : 0);
                SetSides(material.DoubleSided ? 2 : 0);
                if (transparent) { OpenGl.Native.glEnable((GlCapability)OpenGl.GL_BLEND); OpenGl.Native.glBlendFunc(OpenGl.GL_SRC_ALPHA, OpenGl.GL_ONE_MINUS_SRC_ALPHA); }
                else OpenGl.Native.glDisable((GlCapability)OpenGl.GL_BLEND);
                OpenGl.Native.glDepthMask(ShadowPass || material.DepthWrite);
                primitives[offset + i].Draw(); DrawCalls++;
            }
        }

    }
    public static unsafe void End()
    {
        OpenGl.Native.glDepthMask(true); OpenGl.Native.glFrontFace(GlFrontFace.CounterClockwise);
        OpenGl.CullFace(GlFace.Back);
        OpenGl.Native.glDisable(GlCapability.DepthTest); OpenGl.Native.glDisable(GlCapability.CullFace); OpenGl.Native.glDisable(GlCapability.Blend);
        OpenGl.Native.glBindVertexArray(default); OpenGl.Native.glUseProgram(default);
        if (OpenGl.GetError() != OpenGl.GL_NO_ERROR) throw new InvalidOperationException("Native model rendering failed.");
    }
    public unsafe void DrawStatic(GpuPrimitive geometry, Matrix4D world, Matrix4x4 camera, Brutal.Numerics.float4 color)
    {
        program.Use(); Matrix(0, world); Matrix(4, From(camera));
        SetFarmLight();
        program.Uniform(8, 0); program.Uniform(9, 1); program.Uniform(10, 1); program.Uniform(11, 0f);
        program.Uniform(12, color); program.Uniform(13, 0); program.Uniform(14, 0f);
        OpenGl.Native.glDisable(GlCapability.CullFace); OpenGl.Native.glDisable(GlCapability.Blend); OpenGl.Native.glDepthMask(true);
        geometry.Draw();
    }
    public void Dispose() { OpenGl.DeleteBuffers(new[] { bones }); program.Dispose(); }
    public unsafe void DrawRoom(GpuPrimitive geometry, Matrix4D world, Matrix4x4 camera, Brutal.Numerics.float4 color, GpuTexture? texture, bool unlit, bool transparent, bool depthWrite, Vector3 emissive = default, float roughness = -1, float metalness = 0, int side = 2, int? shadowSide = null, bool additive = false, bool vertexColors = false, bool flatShading = false, float alphaTest = 0, bool wireframe = false, PointSprite? points = null, bool fog = true)
    {
        program.Use(); Matrix(0, world); Matrix(4, From(camera));
        SetFarmLight(emissive, roughness, metalness, fog);
        program.Uniform(8, 0); program.Uniform(9, flatShading ? 0 : 1); program.Uniform(10, unlit ? 1 : 0); program.Uniform(11, alphaTest);
        program.Uniform(29, vertexColors && geometry.HasColors ? 1 : 0);
        program.Uniform(12, color); program.Uniform(13, texture is null ? 0 : 1); program.Uniform(14, 0f);
        OpenGl.BindTextureUnit(0, texture?.Id ?? 0); SetSides(side, shadowSide);
        double determinant = world.M11 * (world.M22 * world.M33 - world.M23 * world.M32)
            - world.M12 * (world.M21 * world.M33 - world.M23 * world.M31) + world.M13 * (world.M21 * world.M32 - world.M22 * world.M31);
        OpenGl.Native.glFrontFace(determinant < 0 ? GlFrontFace.Clockwise : GlFrontFace.CounterClockwise);
        if (transparent) { OpenGl.Native.glEnable(GlCapability.Blend); OpenGl.Native.glBlendFunc(OpenGl.GL_SRC_ALPHA, additive ? OpenGl.GL_ONE : OpenGl.GL_ONE_MINUS_SRC_ALPHA); }
        else OpenGl.Native.glDisable(GlCapability.Blend);
        if (points is not null)
        {
            program.Uniform(30, new Brutal.Numerics.float4(points.Size, points.ViewportHeight * .5f, points.Attenuate ? 1 : 0, 1));
            program.Uniform(31, new Brutal.Numerics.float4(points.U.X, points.U.Y, points.U.Z, 0));
            program.Uniform(32, new Brutal.Numerics.float4(points.V.X, points.V.Y, points.V.Z, 0));
            OpenGl.Native.glEnable((GlCapability)OpenGl.GL_PROGRAM_POINT_SIZE);
        }
        OpenGl.Native.glDepthMask(ShadowPass || depthWrite); geometry.Draw(wireframe);
        if (points is not null) OpenGl.Native.glDisable((GlCapability)OpenGl.GL_PROGRAM_POINT_SIZE);
    }
    private unsafe void SetSides(int side, int? shadowSide = null)
    {
        if (ShadowPass) side = shadowSide ?? (side == 2 ? 2 : 1 - side);
        if (side == 2) OpenGl.Native.glDisable(GlCapability.CullFace);
        else
        {
            OpenGl.Native.glEnable(GlCapability.CullFace);
            OpenGl.CullFace(side == 0 ? GlFace.Back : GlFace.Front);
        }
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
        layout(location=5) in vec4 tangent;
        layout(location=6) in vec3 vertexColor;
        layout(std430,binding=0) readonly buffer Bones { mat4 bones[]; };
        layout(location=0) uniform mat4 world;
        layout(location=4) uniform mat4 camera;
        layout(location=8) uniform int skinned;
        layout(location=30) uniform vec4 pointSettings;
        out vec2 texCoord; out vec3 worldPosition; out vec3 worldNormal; out vec4 worldTangent;
        out vec3 meshColor; out float eyeDepth;
        void main() {
            mat4 skin = mat4(1);
            if(skinned!=0) skin=bones[int(joints.x)]*weights.x+bones[int(joints.y)]*weights.y+bones[int(joints.z)]*weights.z+bones[int(joints.w)]*weights.w;
            vec4 p=world*skin*vec4(position,1);
            worldPosition=p.xyz; worldNormal=transpose(inverse(mat3(world)))*mat3(skin)*normal;
            worldTangent=vec4(mat3(world)*mat3(skin)*tangent.xyz,tangent.w);
            texCoord=uv; meshColor=vertexColor; gl_Position=camera*p; eyeDepth=gl_Position.w;
            gl_PointSize=pointSettings.w!=0 ? pointSettings.x*(pointSettings.z!=0 ? pointSettings.y/gl_Position.w : 1) : 1;
            gl_Position.z=2*gl_Position.z-gl_Position.w;
        }
        """;
    private const string FragmentShader = """
        #version 450 core
        in vec2 texCoord; in vec3 worldPosition; in vec3 worldNormal; in vec4 worldTangent;
        in vec3 meshColor; in float eyeDepth;
        layout(binding=0) uniform sampler2D colorTexture;
        layout(binding=1) uniform sampler2D roughnessTexture;
        layout(binding=2) uniform sampler2D specularTexture;
        layout(binding=3) uniform sampler2D emissiveTexture;
        layout(binding=4) uniform sampler2D normalTexture;
        layout(binding=5) uniform sampler2D shadowTexture;
        layout(location=9) uniform int hasNormals;
        layout(location=10) uniform int unlit;
        layout(location=11) uniform float cutoff;
        layout(location=12) uniform vec4 color;
        layout(location=13) uniform int textured;
        layout(location=14) uniform float emission;
        layout(location=15) uniform int farmLighting;
        layout(location=16) uniform vec4 farmIntensity;
        layout(location=17) uniform vec4 materialEmission;
        layout(location=18) uniform vec4 farmEye;
        layout(location=19) uniform vec4 surface;
        layout(location=20) uniform vec4 surfaceMaps;
        layout(location=21) uniform int hasEmissiveMap;
        layout(location=22) uniform vec4 normalSettings;
        layout(location=23) uniform int shadowPass;
        layout(location=24) uniform int receiveShadow;
        layout(location=25) uniform mat4 shadowCamera;
        layout(location=29) uniform int hasVertexColors;
        layout(location=30) uniform vec4 pointSettings;
        layout(location=31) uniform vec4 pointU;
        layout(location=32) uniform vec4 pointV;
                layout(location=33) uniform vec4 boardSky;
        layout(location=34) uniform vec4 boardGround;
        layout(location=35) uniform vec4 boardSun;
        layout(location=36) uniform vec4 boardDirection;
        layout(location=37) uniform vec4 boardFogColor;
        layout(location=38) uniform vec4 boardFog;
        layout(location=39) uniform vec4 shadowSettings;
        out vec4 outputColor;
        vec3 toLinear(vec3 color) {
            return mix(color/12.92,pow((color+.055)/1.055,vec3(2.4)),step(vec3(.04045),color));
        }
        float shadowCompare(vec2 uv, float depth) { return step(depth,texture(shadowTexture,uv).r); }
        float sunVisibility() {
            if(receiveShadow==0) return 1;
            vec4 p=shadowCamera*vec4(worldPosition,1);
            vec3 uv=p.xyz/p.w; uv.xy=uv.xy*.5+.5; uv.z+=shadowSettings.y;
            if(any(lessThan(uv,vec3(0))) || any(greaterThan(uv,vec3(1)))) return 1;
            vec2 f=fract(uv.xy*shadowSettings.x+.5); vec2 base=uv.xy-f/shadowSettings.x;
            float d=1.0/shadowSettings.x, z=uv.z;
            return (shadowCompare(base,z)+shadowCompare(base+vec2(d,0),z)+shadowCompare(base+vec2(0,d),z)+shadowCompare(base+vec2(d,d),z)
                +mix(shadowCompare(base+vec2(-d,0),z),shadowCompare(base+vec2(2*d,0),z),f.x)
                +mix(shadowCompare(base+vec2(-d,d),z),shadowCompare(base+vec2(2*d,d),z),f.x)
                +mix(shadowCompare(base+vec2(0,-d),z),shadowCompare(base+vec2(0,2*d),z),f.y)
                +mix(shadowCompare(base+vec2(d,-d),z),shadowCompare(base+vec2(d,2*d),z),f.y)
                +mix(mix(shadowCompare(base+vec2(-d,-d),z),shadowCompare(base+vec2(2*d,-d),z),f.x),mix(shadowCompare(base+vec2(-d,2*d),z),shadowCompare(base+vec2(2*d,2*d),z),f.x),f.y))/9;
        }
        // Isotropic MeshStandardMaterial BRDF from the bundled Three shader chunks.
        vec3 specularGGX(vec3 l, vec3 v, vec3 n, vec3 f0, float f90, float roughness) {
            vec3 h=normalize(l+v);
            float nl=clamp(dot(n,l),0,1), nv=clamp(dot(n,v),0,1);
            float nh=clamp(dot(n,h),0,1), vh=clamp(dot(v,h),0,1);
            float alpha=roughness*roughness, a2=alpha*alpha;
            float gv=nl*sqrt(a2+(1-a2)*nv*nv), gl=nv*sqrt(a2+(1-a2)*nl*nl);
            float visibility=.5/max(gv+gl,1e-6);
            float denom=nh*nh*(a2-1)+1;
            float distribution=a2/(3.141592653589793*denom*denom);
            float fresnel=exp2((-5.55473*vh-6.98316)*vh);
            return (f0*(1-fresnel)+vec3(f90*fresnel))*visibility*distribution;
        }
        void main() {
            vec3 pointUv=vec3(gl_PointCoord.x,1-gl_PointCoord.y,1);
            vec2 mapUv=pointSettings.w!=0 ? vec2(dot(pointU.xyz,pointUv),dot(pointV.xyz,pointUv)) : texCoord;
            vec4 c=color*(textured!=0?texture(colorTexture,mapUv):vec4(1));
            if(hasVertexColors!=0) c.rgb*=meshColor;
            if(c.a<cutoff) discard;
            if(shadowPass!=0) { outputColor=vec4(1); return; }
            vec3 n=normalize(hasNormals!=0?worldNormal:cross(dFdx(worldPosition),dFdy(worldPosition)));
            if(!gl_FrontFacing) n=-n;
            vec3 geometryNormal=n;
            if(normalSettings.x!=0 && unlit==0) {
                vec3 t,b;
                if(normalSettings.z!=0) {
                    t=normalize(worldTangent.xyz);
                    b=normalize(cross(normalize(worldNormal),t)*worldTangent.w);
                } else {
                    vec3 q0=dFdx(worldPosition),q1=dFdy(worldPosition);
                    vec2 st0=dFdx(texCoord),st1=dFdy(texCoord);
                    vec3 q1perp=cross(q1,n),q0perp=cross(n,q0);
                    t=q1perp*st0.x+q0perp*st1.x; b=q1perp*st0.y+q0perp*st1.y;
                    float det=max(dot(t,t),dot(b,b)); float scale=det==0?0:inversesqrt(det);
                    t*=scale;b*=scale;
                }
                if(!gl_FrontFacing) {t=-t;b=-b;}
                vec3 mapN=texture(normalTexture,texCoord).xyz*2-1;
                mapN.xy*=normalSettings.y;
                n=normalize(mat3(t,b,n)*mapN);
            }
            vec3 light=unlit!=0?vec3(1):vec3(.35)+vec3(.65)*max(dot(n,normalize(vec3(2,4,3))),0);
            vec3 specular=vec3(0);
            if(farmLighting!=0 && unlit==0) {
                vec3 sunDirection=farmLighting==2 ? boardDirection.xyz : normalize(vec3(4,9,5));
                vec3 hemi=mix(toLinear(vec3(74,90,58)/255.0),vec3(1),n.y*.5+.5)*farmIntensity.x;
                if(farmLighting==2) hemi=mix(boardGround.rgb,boardSky.rgb,n.y*.5+.5)*boardSky.w;
                vec3 sunColor=farmLighting==2 ? boardSun.rgb*boardSun.w : toLinear(vec3(255,243,220)/255.0)*farmIntensity.y;
                vec3 sun=sunColor*max(dot(n,sunDirection),0)*sunVisibility();
                vec3 offset=vec3(1.6,2.1,-3.7)-worldPosition;
                float distance=length(offset);
                float attenuation=1.0/max(pow(distance,1.5),.01);
                attenuation*=pow(clamp(1.0-pow(distance/9.0,4.0),0.0,1.0),2.0);
                vec3 lamp=toLinear(vec3(255,200,120)/255.0)*farmIntensity.z*attenuation*max(dot(n,normalize(offset)),0);
                if(farmLighting==2) lamp=vec3(0);
                light=(hemi+sun+lamp)/3.141592653589793;
                if(surface.x>=0) {
                    vec3 derivatives=max(abs(dFdx(geometryNormal)),abs(dFdy(geometryNormal)));
                    float roughnessFactor=surface.x*(surfaceMaps.x!=0?texture(roughnessTexture,texCoord).g:1);
                    float roughness=min(1,max(roughnessFactor,.0525)+max(derivatives.x,max(derivatives.y,derivatives.z)));
                    vec3 v=normalize(farmEye.xyz-worldPosition);
                    float specularIntensity=surfaceMaps.z*(surfaceMaps.y!=0?texture(specularTexture,texCoord).a:1);
                    vec3 f0=mix(vec3(.04*specularIntensity),c.rgb,surface.y);
                    float f90=mix(specularIntensity,1,surface.y);
                    specular=sun*specularGGX(sunDirection,v,n,f0,f90,roughness)
                        +lamp*specularGGX(normalize(offset),v,n,f0,f90,roughness);
                    light*=1-surface.y;
                }
            }
            vec3 linear=max(c.rgb*light+vec3(unlit!=0?0:emission),vec3(0));
            if(unlit==0) linear+=materialEmission.rgb*(hasEmissiveMap!=0?texture(emissiveTexture,texCoord).rgb:vec3(1));
            if(farmLighting!=0 && unlit==0) linear+=specular;
            vec3 srgb=mix(12.92*linear,1.055*pow(linear,vec3(1.0/2.4))-.055,step(vec3(.0031308),linear));
            if(boardFog.z!=0) srgb=mix(srgb,boardFogColor.rgb,smoothstep(boardFog.x,boardFog.y,eyeDepth));
            outputColor=vec4(srgb,c.a);
        }
        """;
}
