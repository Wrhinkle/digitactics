using System.Numerics;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

/// <summary>Theme-specific gameplay surfaces; one shared geometry upload per tile, slot and pedestal.</summary>
internal sealed class BoardSurfaceView : IDisposable
{
    private readonly List<GpuPrimitive> geometry=[];
    private readonly Dictionary<int,GpuTexture> textures=[];
    private readonly BoardSurfaces materials;
    public int DrawCalls { get; private set; }

    public BoardSurfaceView(BoardScene scene,BoardSurfaceGeometry source)
    {
        materials=BoardSurfaces.From(scene);
        try
        {
            var parts=new[] { (source.Tile,materials.Mine),(source.Slot,materials.Slot),(source.Ped,materials.Ped) };
            foreach(var (shape,material) in parts)
            {
                var model=scene with { Geometries=[shape],Materials=[material] };
                var obj=new BoardObject("mesh",0,[0],[],true,false,true,null);
                geometry.Add(new(BoardMesh.Create(model,obj)[0].Primitive));
                if(material.Map is { } map && !textures.ContainsKey(map))
                {
                    var texture=scene.Textures[map];
                    textures.Add(map,new(BoardMesh.Image(texture),new(WrapS:BoardMesh.Wrap(texture.WrapS),WrapT:BoardMesh.Wrap(texture.WrapT)),texture.ColorSpace=="srgb"));
                }
            }
        }
        catch { Dispose(); throw; }
    }

    public void Draw(ModelRenderer renderer,Matrix4x4 camera,int columns,bool planning)
    {
        if(columns is not (7 or 14)) throw new ArgumentOutOfRangeException(nameof(columns));
        DrawCalls=0; renderer.ReceiveShadow=true;
        void Draw(int primitive,BoardMaterial material,double x,double y,double z,double scaleX=1)
        {
            var matrix=Matrix4D.Compose(new(x,y,z),new(0,0,0,1),new(scaleX,1,1));
            renderer.DrawRoom(geometry[primitive],matrix,camera,new(material.Color[0],material.Color[1],material.Color[2],1),
                material.Map is { } map?textures[map]:null,false,false,true,new(material.Emissive[0],material.Emissive[1],material.Emissive[2]),material.Roughness,material.Metalness,side:0);
            DrawCalls++;
        }
        for(int row=0;row<8;row++) for(int column=0;column<columns;column++)
            Draw(0,row>=4?materials.Mine:planning?materials.EnemyPlanning:materials.Enemy,(column+.5*(row&1)-(columns-.5)/2)*Math.Sqrt(3),.07,(row-3.5)*1.5);
        bool wide=columns>7;
        for(int side=0;side<(wide?2:1);side++) for(int i=0;i<9;i++)
        {
            double x=(i-4)*1.45;
            if(wide) x=x*.72+(side==0?-6.1:6.1);
            Draw(1,materials.Slot,x,.05,7.55,wide?.72:1);
        }
        for(int i=0;i<10;i++) Draw(2,materials.Ped,-7.5-i%2*.95,.08,1.2+i/2*1.5);
    }

    public void Dispose()
    {
        foreach(var primitive in geometry) primitive.Dispose(); geometry.Clear();
        foreach(var texture in textures.Values) texture.Dispose(); textures.Clear();
    }
}
