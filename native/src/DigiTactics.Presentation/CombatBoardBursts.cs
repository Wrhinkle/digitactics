using DigiTactics.Rules;

namespace DigiTactics.Presentation;

public readonly record struct CombatTileFlash(int Column,int Row,double Until,int ViewTeam);

/// <summary>Consumes burst cues once, preserving the original mirrored hex-area selection and overwrite order.</summary>
public sealed class CombatBoardBursts
{
    private readonly int columns;
    private readonly bool mirror;
    private readonly HexCell[] cells;
    private int cursor;
    public int FlashCount { get; private set; }
    public CombatBoardBursts(int columns,bool mirror)
    {
        if(columns is not (7 or 14)) throw new ArgumentOutOfRangeException(nameof(columns));
        this.columns=columns; this.mirror=mirror;
        cells=Enumerable.Range(0,8).SelectMany(r=>Enumerable.Range(0,7).Select(c=>new HexCell(c,r)))
            .Concat(Enumerable.Range(0,8).SelectMany(r=>Enumerable.Range(7,columns-7).Select(c=>new HexCell(c,r)))).ToArray();
    }
    public void Update(IReadOnlyList<CombatEvent> events,Action<CombatTileFlash> flash)
    {
        while(cursor<events.Count)
        {
            var cue=events[cursor++];
            if(cue.Type!="burst" || cue.Cell is not { } cell) continue;
            if(mirror) cell=new(columns-1-cell.Column,7-cell.Row);
            int team=mirror?1-(cue.Team??0):cue.Team??0;
            foreach(var tile in cells)
                if(tile.DistanceTo(cell)<=cue.Radius) { flash(new(tile.Column,tile.Row,cue.Time+.45,team)); FlashCount++; }
        }
    }
}
