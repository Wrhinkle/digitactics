namespace DigiTactics.Presentation;

public sealed record ScoutSeat(int Id,bool Alive,double Health,int Placement,int? Team);
public sealed record MatchKeyContext(ScoutSeat[] Players,int LocalSeat,int Viewed,bool Duos,bool Over,bool Planning,
    int? Hovered=null,bool TextInput=false,bool Modified=false,bool Blocked=false,bool Repeated=false);
public sealed record MatchKeyAction(string Kind,int? Seat=null,int? Unit=null);

/// <summary>Local input projection from ui.js/scout.js; never mutates game state.</summary>
public static class MatchKeyboard
{
    public static ScoutSeat[] Order(IEnumerable<ScoutSeat> players,bool duos)=>players.OrderByDescending(p=>p.Alive)
        .ThenBy(p=>p.Alive?-p.Health:p.Placement).ThenBy(p=>duos?p.Team??0:0).ThenBy(p=>p.Id).ToArray();
    public static MatchKeyAction? Resolve(string key,MatchKeyContext context)
    {
        if(context.TextInput||context.Modified||context.Blocked)return null;
        key=key.ToLowerInvariant();
        if(context.LocalSeat>=0&&context.Viewed!=context.LocalSeat&&key is " " or "escape")
            return context.Repeated&&key==" "?null:new("scout",context.LocalSeat);
        if(key=="escape")return context.Repeated?null:new("menu");
        if(key=="r")return new("reset");
        if(key is "=" or "+")return new("zoomIn");
        if(key is "-" or "_")return new("zoomOut");
        if(context.Over||context.Repeated)return null;
        var order=Order(context.Players,context.Duos);
        if(key.Length==1&&key[0] is >= '1' and <= '8')
            return order.ElementAtOrDefault(key[0]-'1') is {} seat&&seat.Id!=context.Viewed?new("scout",seat.Id):null;
        if(key=="q"||key=="e"&&context.Hovered is null)
        {
            var living=order.Where(p=>p.Alive||p.Id==context.LocalSeat).ToArray();
            if(living.Length<2)return null;
            int current=Array.FindIndex(living,p=>p.Id==context.Viewed),step=key=="q"?-1:1;
            int next=current<0?(step>0?0:living.Length-1):(current+step+living.Length)%living.Length;
            return new("scout",living[next].Id);
        }
        if(!context.Players.Any(p=>p.Id==context.LocalSeat&&p.Alive))return null;
        if(key=="d")return new("reroll");
        if(key=="f")return new("buyXp");
        if(key=="e"&&context.Viewed==context.LocalSeat&&context.Hovered is {} unit)return new("sell",Unit:unit);
        return key==" "&&context.Planning?new("fight"):null;
    }
    public static bool RequiresOwnBoard(string action)=>action is "buy" or "wisp" or "sell" or "equip" or "move";
}
