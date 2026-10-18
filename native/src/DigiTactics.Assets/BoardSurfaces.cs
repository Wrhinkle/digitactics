using System.Text.Json;

namespace DigiTactics.Assets;

public sealed record BoardSurfaceGeometry(int Version, BoardGeometry Tile, BoardGeometry Slot, BoardGeometry Ped)
{
    public static BoardSurfaceGeometry Load(string path)
    {
        var geometry=JsonSerializer.Deserialize<BoardSurfaceGeometry>(File.ReadAllText(path),new JsonSerializerOptions { PropertyNameCaseInsensitive=true }) ?? throw new InvalidDataException("Missing board surface geometry.");
        if(geometry.Version!=1) throw new InvalidDataException("Unsupported board surface geometry.");
        return geometry;
    }
}

public sealed record BoardSurfaces(BoardMaterial Mine, BoardMaterial Enemy, BoardMaterial EnemyPlanning, BoardMaterial Slot, BoardMaterial Ped)
{
    public static BoardSurfaces From(BoardScene scene)
    {
        static string Color(JsonElement e,string key,string fallback)=>e.TryGetProperty(key,out var value)?value.GetString()!:fallback;
        static float Number(JsonElement e,string key,float fallback)=>e.TryGetProperty(key,out var value)?value.GetSingle():fallback;
        BoardMaterial Material(string color,string glow,float rough,float metal,string surface)
        {
            var c=BoardLighting.Linear(color); var emission=BoardLighting.Linear(glow);
            return new([c.X,c.Y,c.Z,1],scene.Surface.TryGetValue(surface,out int texture)?texture:null,false,false,true,0,true,false,0,
                [emission.X,emission.Y,emission.Z],1,rough,metal,false,false,1,1,true);
        }
        var t=scene.Tiles; var look=scene.Look;
        float rough=Number(t,"rough",.6f),metal=Number(t,"metal",.1f);
        return new(
            Material(Color(t,"mine","#30598c"),Color(t,"glowMine","#000000"),rough,metal,"tile"),
            Material(Color(t,"enemy","#7a3442"),Color(t,"glowEnemy","#000000"),rough,metal,"tile"),
            Material(Color(t,"enemyPlan","#4a2a33"),Color(t,"glowEnemy","#000000"),rough,metal,"tile"),
            Material(scene.Slot,"#000000",Number(look,"slotRough",.7f),Number(look,"slotMetal",0),"slot"),
            Material(scene.Ped,"#000000",Number(look,"pedRough",.6f),Number(look,"pedMetal",0),"ped"));
    }
}
