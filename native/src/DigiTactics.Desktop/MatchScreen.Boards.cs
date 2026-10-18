using DigiTactics.Storage;
using System.Text.Json.Nodes;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private string? boardWallet;
    private string localBoard="file-island";
    private bool boardLoaded;
    private double boardSceneTime;
    public int BoardDrawCalls=>battle?.BoardDrawCalls ?? planning.BoardDrawCalls;
    private void SyncBoardTheme()
    {
        arenaEnvironment.CombatActive=Session?.Rounds.Phase==DigiTactics.Rules.MatchPhase.Combat;
        string? wallet=profile?.Storage.GetValueOrDefault("dt.cosmetics.v1");
        if(!boardLoaded || wallet!=boardWallet)
        {
            try { localBoard=new CosmeticCollection(wallet).Equipped("board")?.GetValue<string>() ?? "file-island"; }
            catch(Exception error) when(error is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or KeyNotFoundException or FormatException or OverflowException)
            { localBoard="file-island"; status="Board appearance could not be read. Saved progress was preserved: "+error.Message; }
            boardWallet=wallet; boardLoaded=true;
        }
        planning.BoardTheme=localBoard;
        planning.SceneTime=boardSceneTime;
        if(battle is null) return;
        battle.SceneTime=boardSceneTime;
        string theme=localBoard;
        if(battle.Mirrored && watching is { Pve:false })
        {
            int seat=watching.A; var player=Session!.State.Players[seat]; JsonObject? look=null;
            if(player.IsHuman && networkRoom is not null)
            {
                string? uid=networkRoom.Authority.Lobby?["slots"]?[seat.ToString()]?["uid"]?.GetValue<string>();
                if(uid is not null) look=networkRoom.Authority.Presence[uid]?["look"] as JsonObject;
            }
            if(look is null)
            {
                if(!aiLooks.TryGetValue((seat,player.Name),out look)) aiLooks[(seat,player.Name)]=look=CosmeticLoadouts.Ai(seat,player.Name);
            }
            theme=look["board"]?.GetValue<string>() ?? "file-island";
        }
        battle.BoardTheme=theme;
    }
}
