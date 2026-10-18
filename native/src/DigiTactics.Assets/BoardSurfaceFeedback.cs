using System.Numerics;

namespace DigiTactics.Assets;

/// <summary>Transient surface feedback, separate from theme materials and match state.</summary>
public sealed class BoardSurfaceFeedback
{
    private readonly Dictionary<(int Column,int Row),(double Until,Vector3 Color)> flashes=[];
    private static readonly Vector3 tileHover=BoardLighting.Linear("#3d7fd6"),benchHover=BoardLighting.Linear("#6b5320"),itemHover=BoardLighting.Linear("#2b4a6b");
    public double Time { get; set; }
    public bool UnitDragMoved { get; set; }
    public (int Column,int Row)? HoverCell { get; set; }
    public int HoverBench { get; set; } = -1;
    public int HoverItem { get; set; } = -1;
    public bool ItemDragging { get; set; }
    public int ItemDragOver { get; set; } = -1;
    public void Flash(int column,int row,double until,Vector3 color)=>flashes[(column,row)]=(until,color);
    public void ClearFlashes()=>flashes.Clear();
    public Vector3 Tile(int column,int row,bool planning,BoardMaterial material)
    {
        if(UnitDragMoved && HoverCell==(column,row)) return tileHover;
        if(!planning && flashes.TryGetValue((column,row),out var flash) && flash.Until>Time) return flash.Color*(float)((flash.Until-Time)*2.2);
        return new(material.Emissive[0],material.Emissive[1],material.Emissive[2]);
    }
    public Vector3 Bench(int index,bool partner=false)=>!partner && UnitDragMoved && HoverBench==index ? benchHover : Vector3.Zero;
    public Vector3 Item(int index)=>(!ItemDragging && HoverItem==index || ItemDragging && ItemDragOver==index) ? itemHover : Vector3.Zero;
}
