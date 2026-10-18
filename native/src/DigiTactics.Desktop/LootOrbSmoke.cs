using Brutal.ImGuiApi;
using DigiTactics.Rules;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

internal sealed class LootOrbSmoke
{
    private int step, gold, items, waits;
    public bool Done { get; private set; }
    public string Pending=>$"Loot/{step}/wait {waits}";
    public bool Capture => step == 1;
    public void Input(MatchScreen screen)
    {
        var session=screen.Session!; var player=session.State.Players[0]; var io=ImGui.GetIO();
        bool Click(string key,int button=0)
        {
            var p=screen.Buttons[key];
            if(key.StartsWith("view/",StringComparison.Ordinal))
            {
                var bounds=screen.ScoutingBounds;
                if(p.Y<bounds.Min.Y+20 || p.Y>bounds.Max.Y-20)
                {
                    io.AddMousePosEvent((bounds.Min.X+bounds.Max.X)/2,(bounds.Min.Y+bounds.Max.Y)/2);
                    io.AddMouseWheelEvent(0,p.Y<bounds.Min.Y+20?1:-1);return false;
                }
            }
            io.AddMousePosEvent(p.X,p.Y);io.AddMouseButtonEvent(button,true);return true;
        }
        switch(step)
        {
            case 0:
                var loot=new MatchLoot(session.State); loot.CollectAll(player);
                gold=player.Economy.Gold; items=player.Items.Count;
                loot.Drop(player,[new("gold",Gold:7),new(Items:[session.State.Rules.Loot!.StatComponents[0]])]);
                for(int i=0;i<2;i++)
                {
                    var cell=CombatLayout.Cell(new(3+i*2,6),7,false);
                    player.Orbs[i]=player.Orbs[i] with {X=(cell.X+5.2)/10.4,Z=(cell.Z-.9)/4.2};
                }
                step++; break;
            case 1:
                if(screen.LootOrbDrawCalls!=4 || screen.LootOrbShadowDrawCalls!=2) throw new InvalidOperationException("Orb core/glow/shadow passes missing.");
                if(Click("view/1"))step++; break;
            case 2: io.AddMouseButtonEvent(0,false); step++; break;
            case 3:
                if(screen.ViewedPlayer!=1 || screen.LootOrbDrawCalls!=0 || player.Orbs.Count!=2) throw new InvalidOperationException("Scouting did not select the other player or exposed/collected another board's loot.");
                if(Click("view/0"))step++; break;
            case 4: io.AddMouseButtonEvent(0,false); step++; break;
            case 5:
                if(screen.ViewedPlayer!=0)throw new InvalidOperationException("Loot collection did not return to the local player's board.");
                Click("hex/3/6",1); step++; break;
            case 6: io.AddMouseButtonEvent(1,false); step++; break;
            case 7:
                if(player.Orbs.Count==2) { if(++waits>90) throw new InvalidOperationException("Companion failed to reach the gold orb."); return; }
                if(screen.LootBurstDrawCalls==0) throw new InvalidOperationException("Collected orb did not render its burst.");
                if(player.Orbs.Count!=1 || player.Economy.Gold!=gold+7) throw new InvalidOperationException("Gold orb did not award exactly once.");
                Click("hex/5/6",1); step++; waits=0; break;
            case 8: io.AddMouseButtonEvent(1,false); step++; break;
            case 9:
                if(player.Orbs.Count!=0) { if(++waits>90) throw new InvalidOperationException("Companion failed to reach the item orb."); return; }
                if(screen.LootBurstDrawCalls==0) throw new InvalidOperationException("Item pickup burst was missing.");
                if(player.Items.Count!=items+1 || player.Economy.Gold!=gold+7) throw new InvalidOperationException("Item orb reward or duplicate pickup failed.");
                Done=true; Console.WriteLine("Loot arena UI: original core/glow/shadows, scouting visibility, right-click travel and exact gold/item collection passed."); break;
        }
    }
}
