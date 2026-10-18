using System.Text.Json;
using System.Text.Json.Nodes;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Networking;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Two room controllers, one real rendered guest screen in seat one.</summary>
internal sealed class NetworkMatchSmoke : IDisposable
{
    private readonly NetworkRoom host, guest;
    private readonly MatchReplica replica;
    private readonly MatchHostController controller;
    private readonly Queue<(string Sender, RoomPacket Packet)> packets = [];
    private long now = 10000;
    private int stage, cooldown = 3;
    private bool down, combatRendered, planningRendered;
    private bool companionRendered;
    private bool arenaEmoteRendered;
    private bool danceRendered;
    private int networkMenuStep;
    private double menuCountdown;
    private int lootStep;
    private int? lootId;
    private bool lootVerified;
    private int travelStep;
    private double travelX, travelZ;
    private readonly bool spectator;
    private bool spectatorVerified;
    private int soundEffects;
    private int emoteEffects;
    private int socialStep;
    private int socialCheck;
    private bool carouselDown,hostCarouselSubmitted;
    private readonly string[] socialCommands = ["players", "playerMute/123", "playerMute/123", "closePlayers", "emotes", "emote/4"];
    public string Pending => $"network UI stage {stage}, social {socialStep}, menu {networkMenuStep}, travel {travelStep}, loot {lootStep}";
    public NetworkMatchSmoke(ContentCatalog catalog, MatchScreen screen, bool duos = false, bool spectator = false)
    {
        this.spectator = spectator; stage = spectator ? -2 : -5;
        var lobby = JsonNode.Parse("{\"host\":\"123\",\"name\":\"Test room\",\"hostName\":\"Host\",\"status\":\"playing\",\"slots\":{\"1\":{\"uid\":\"234\",\"name\":\"Guest\"}}}"u8)!.AsObject();
        if (spectator) { lobby["slots"]!.AsObject().Remove("1"); lobby["slots"]!["0"] = new JsonObject { ["uid"] = "123", ["name"] = "Host" }; }
        else lobby["slots"]!["0"] = new JsonObject { ["uid"] = "123", ["name"] = "Host" };
        host = new("999", "123", lobby); guest = new("999", "234", lobby);
        host.Members(["123", "234"], "123", now); guest.Members(["123", "234"], "123", now);
        host.Send += p => packets.Enqueue(("123", p)); guest.Send += p => packets.Enqueue(("234", p));
        host.Effect += (_, effect) => { if (effect.GetProperty("kind"u8).GetString() == "sound" && effect.GetProperty("s"u8).GetDouble() == 1) soundEffects++; };
        host.Effect += (_, effect) => { if (effect.GetProperty("kind"u8).GetString() == "emote" && effect.GetProperty("s"u8).GetDouble() == 1) emoteEffects++; };
        host.PublishInit(JsonSerializer.SerializeToElement(new { seed = 143, at = now, planTime = 30, setId = "faction-wars", pace = "hyper", format = duos ? "duos" : "solo", slots = Enumerable.Range(0, 8).Select(i => new { name = i == 1 ? "Guest" : "Tamer " + i, human = i == (spectator ? 0 : 1) }).ToArray() }));
        PumpPackets(); replica = new(host, catalog); controller = new(host, replica, () => now); replica.Pump(now);
        guest.SetPresence(new JsonObject { ["name"] = "Guest" }, false, now); PumpPackets();
        var hostLook = DigiTactics.Storage.CosmeticLoadouts.Ai(0, "Host");
        host.SetPresence(new JsonObject { ["name"] = "Host", ["look"] = hostLook }, false, now); PumpPackets();
        if (!JsonNode.DeepEquals(guest.Authority.Presence["123"]?["look"], hostLook))
            throw new InvalidOperationException("Peer cosmetic loadout was not delivered intact.");
        screen.AttachNetworkMatch(guest, () => now);
    }
    private void PumpPackets()
    {
        while (packets.TryDequeue(out var p)) (p.Packet.Recipient == "123" ? host : guest).Receive(p.Sender, p.Packet.Text, now);
    }
    public void Input(MatchScreen screen)
    {
        now += !spectator && stage == 3 && screen.CompletedRounds > 0 ? 16 : 200; PumpPackets(); controller.Pump(now); PumpPackets();
        if(carouselDown){ImGui.GetIO().AddMouseButtonEvent(0,false);carouselDown=false;cooldown=3;return;}
        if (cooldown-- > 0) return;
        // Complete the real opening carousel before clicking controls behind it.
        // The second peer uses its normal authorized intent; the rendered guest
        // chooses through its actual card and waits for the shared next entry.
        if(screen.Session is {} opening && opening.Units.Carousel.Active)
        {
            var carousel=opening.Units.Carousel;
            if(carousel.Turn?.Id!=0)hostCarouselSubmitted=false;
            if(carousel.Turn?.Id==0&&!hostCarouselSubmitted)
            {
                int choice=carousel.Options.Select((option,index)=>(option,index)).First(pair=>pair.option.PickedBy is null).index;
                hostCarouselSubmitted=host.SubmitIntent(0,"carousel",JsonSerializer.SerializeToElement(new{i=choice}),opening.Rounds.Position.Stage*100+opening.Rounds.Position.Round);
            }
            else if(!screen.IsSpectating&&carousel.Turn?.Id==1)
            {
                int choice=carousel.Options.Select((option,index)=>(option,index)).First(pair=>pair.option.PickedBy is null).index;
                if(screen.Buttons.TryGetValue("carousel/"+choice,out var carouselPoint))
                {
                    var bounds=screen.CarouselChoicesBounds;
                    if(carouselPoint.Y<bounds.Min.Y+10||carouselPoint.Y>bounds.Max.Y-10)
                    {ImGui.GetIO().AddMousePosEvent((bounds.Min.X+bounds.Max.X)*.5f,(bounds.Min.Y+bounds.Max.Y)*.5f);ImGui.GetIO().AddMouseWheelEvent(0,carouselPoint.Y<bounds.Min.Y?1:-1);}
                    else{ImGui.GetIO().AddMousePosEvent(carouselPoint.X,carouselPoint.Y);ImGui.GetIO().AddMouseButtonEvent(0,true);carouselDown=true;}
                }
            }
            return;
        }
        if (!spectator && socialStep < socialCommands.Length)
        {
            if (down)
            {
                ImGui.GetIO().AddMouseButtonEvent(0, false); down = false; socialStep++; cooldown = 3;
                if (socialStep is 2 or 3) socialCheck = socialStep;
                return;
            }
            if (screen.Buttons.Keys.Any(k => k.StartsWith("playerRemove/", StringComparison.Ordinal))) throw new InvalidOperationException("Guest sees host removal controls.");
            if (screen.Buttons.TryGetValue(socialCommands[socialStep], out var socialPoint))
            { ImGui.GetIO().AddMousePosEvent(socialPoint.X, socialPoint.Y); ImGui.GetIO().AddMouseButtonEvent(0, true); down = true; }
            return;
        }
        if (!spectator && stage == 0 && screen.Session?.Rounds.RoundType != "carousel" && networkMenuStep < 4)
        {
            var io = ImGui.GetIO();
            if (networkMenuStep == 0)
            {
                menuCountdown = screen.NetworkPlanningRemaining ?? throw new InvalidOperationException("Missing online countdown before opening menu.");
                var menuPoint = screen.Buttons["gameMenu"]; io.AddMousePosEvent(menuPoint.X,menuPoint.Y); io.AddMouseButtonEvent(0,true);
            }
            else if (networkMenuStep == 2)
            {
                if (screen.IsPaused || screen.Buttons.ContainsKey("menuPause") || screen.NetworkPlanningRemaining is not { } left || left >= menuCountdown)
                    throw new InvalidOperationException("Online menu stopped the countdown or exposed pause.");
                var resumePoint = screen.Buttons["resume"]; io.AddMousePosEvent(resumePoint.X,resumePoint.Y); io.AddMouseButtonEvent(0,true);
            }
            else io.AddMouseButtonEvent(0,false);
            networkMenuStep++; cooldown = 3; return;
        }
        if (!spectator && stage == 0 && screen.Session?.Rounds.RoundType != "carousel" && travelStep < 3)
        {
            if (travelStep == 0 && screen.Buttons.TryGetValue("hex/0/7", out var destination) && screen.CompanionTravel is { } initial)
            {
                travelX = initial.X; travelZ = initial.Z;
                ImGui.GetIO().AddMousePosEvent(destination.X, destination.Y); ImGui.GetIO().AddMouseButtonEvent(1, true); travelStep = 1;
            }
            else if (travelStep == 1) { ImGui.GetIO().AddMouseButtonEvent(1, false); travelStep = 2; }
            else if (travelStep == 2 && screen.CompanionTravel is { TargetX: null, Moving: false } arrived)
            {
                if (Math.Abs(arrived.X - travelX) + Math.Abs(arrived.Z - travelZ) < .1) throw new InvalidOperationException("Right-click did not move the arena companion.");
                travelStep = 3;
            }
            return;
        }
        if (!spectator && stage == 3 && screen.CompletedRounds > 0 && screen.Session?.Rounds.Phase == MatchPhase.Planning && !lootVerified)
        {
            if (lootStep == 0)
            {
                var orb=screen.Session.State.Players[1].Orbs.FirstOrDefault() ?? throw new InvalidOperationException("Network loot fixture has no PvE reward.");
                lootId=orb.Id; var lootPoint=screen.LootGroundScreen(orb);
                ImGui.GetIO().AddMousePosEvent(lootPoint.X,lootPoint.Y); ImGui.GetIO().AddMouseButtonEvent(1,true); lootStep++;
            }
            else if (lootStep == 1) { ImGui.GetIO().AddMouseButtonEvent(1,false); lootStep++; }
            return;
        }
        if (down) { ImGui.GetIO().AddMouseButtonEvent(0, false); down = false; stage++; cooldown = 3; return; }
        string? key = stage switch { -5 => "soundboard", -4 => "soundboard/0", -3 => "closeSoundboard", -2 => "joinSeat", 0 when screen.Session?.Rounds.RoundType != "carousel" => "lock", 2 => "fight", 4 => "spectate", 20 => "players", 21 => "playerRemove/234", 22 => "closePlayers", _ => null };
        if (key is not null && screen.Buttons.TryGetValue(key, out var point)) { ImGui.GetIO().AddMousePosEvent(point.X, point.Y); ImGui.GetIO().AddMouseButtonEvent(0, true); down = true; }
    }
    public bool Verify(MatchScreen screen, int2 size, string? capture)
    {
        foreach(string control in new[]{"fight","players","replaySpeed","joinSeat","spectate","leaveOnline"})
            if(screen.Buttons.TryGetValue(control,out var target)&&(target.X<20||target.X>size.X-20||target.Y<20||target.Y>124))
                throw new InvalidOperationException("Online header control is outside the visible header: "+control);
        if (screen.Session is { } clockSession && clockSession.Rounds.Phase == MatchPhase.Planning && clockSession.Rounds.RoundType != "carousel" && !clockSession.State.Over && clockSession.State.Players[screen.ViewedPlayer].Alive)
        {
            var match = replica.Match!;
            double expected = Math.Max(0,(match.PhaseAt + match.PlanMs - host.ServerTime(now)) / 1000);
            if (screen.NetworkPlanningRemaining is not { } displayed || Math.Abs(displayed - expected) > .001 || screen.Buttons.ContainsKey("planningTimer") || screen.Buttons.ContainsKey("pause"))
                throw new InvalidOperationException("Network countdown differed from host time or exposed local clock controls.");
            int ready=clockSession.State.Players.Count(p=>p.Alive&&p.IsHuman&&match.Votes.Contains(p.Id));
            int humans=clockSession.State.Players.Count(p=>p.Alive&&p.IsHuman);
            int local=spectator?screen.ViewedPlayer:stage>=20?0:1;
            if(!screen.IsSpectating&&screen.MatchReadyLabel!=(match.Votes.Contains(local)?$"Ready {ready}/{humans}":$"Ready ({ready}/{humans})"))
                throw new InvalidOperationException("Ready button lost the actual living-player vote count or local readiness.");
            if(capture is not null&&!spectator&&stage<20&&(networkMenuStep==0&&stage==0||match.Votes.Contains(1)))
                FrameCapture.SaveBitmap(Path.ChangeExtension(capture,match.Votes.Contains(1)?"ready.bmp":"planning.bmp"),size.X,size.Y);
        }
        planningRendered |= screen.PlanningDrawCalls > 0;
        companionRendered |= screen.CompanionDrawCalls > 0;
        danceRendered |= screen.CompanionDancing && screen.CompanionDrawCalls > 0;
        if (socialCheck != 0)
        {
            if (!host.SendEffect(JsonSerializer.SerializeToElement(new { s = 0, kind = "sound", id = "digivice" }))) throw new InvalidOperationException("Host effect was not sent.");
            PumpPackets();
            if (screen.ReceivedSoundboardEffects != (socialCheck == 2 ? 0 : 1)) throw new InvalidOperationException("Per-player mute/unmute did not filter incoming soundboard.");
            socialCheck = 0;
        }
        if (screen.ArenaEmoteDraws > 0)
        {
            if (capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "arena-emote.bmp"), size.X, size.Y);
            arenaEmoteRendered = true;
        }
        if (!spectator && socialStep == 2 && capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "players.bmp"), size.X, size.Y);
        if (!spectator && socialStep == 5 && capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "emotes.bmp"), size.X, size.Y);
        if (!spectator && stage == -3 && capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "soundboard.bmp"), size.X, size.Y);
        if (!spectator && stage == -2) { if (soundEffects != 1 || emoteEffects != 1 || !arenaEmoteRendered || !danceRendered) throw new InvalidOperationException("Equipped social effects did not reach host exactly once or render the sticker and dance in the arena."); stage = 0; }
        if (stage == -2 && screen.IsSpectating)
        {
            if (screen.Buttons.ContainsKey("lock") || screen.Buttons.ContainsKey("fight") || screen.Buttons.ContainsKey("buy/0")) throw new InvalidOperationException("Spectator sees player controls.");
            spectatorVerified = true;
        }
        if (stage == -1 && !screen.IsSpectating) stage = 0;
        if (stage == 1 && replica.Match!.Session.State.Players[1].Shop.Locked)
        {
            if (!screen.Session!.State.Players[1].Shop.Locked) return false;
            if (host.Log.Entries.Last(e => e.GetProperty("k"u8).GetString() == "a").GetProperty("s"u8).GetDouble() != 1) throw new InvalidOperationException("Online action used the wrong seat.");
            stage = 2;
        }
        if (screen.Session?.Rounds.Phase == MatchPhase.Combat && screen.BattleDrawCalls > 0)
        {
            if (!combatRendered && capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "combat.bmp"), size.X, size.Y);
            combatRendered = true;
        }
        if (stage < 3 || screen.CompletedRounds < 1 || screen.Session?.Rounds.Phase != MatchPhase.Planning) return false;
        if (!spectator && !lootVerified)
        {
            if (lootId is not { } id || screen.Session.State.Players[1].Orbs.Any(o=>o.Id==id) || replica.Match!.Session.State.Players[1].Orbs.Any(o=>o.Id==id)) return false;
            int actions=host.Log.Entries.Count(e=>e.GetProperty("k"u8).GetString()=="a" && e.GetProperty("a"u8).GetString()=="orb" && e.GetProperty("s"u8).GetDouble()==1);
            if(actions!=1) throw new InvalidOperationException("Proximity pickup submitted duplicate or incorrectly seated network actions.");
            lootVerified=true; Console.WriteLine("Network loot pickup: actual PvE orb collected through guest travel and exactly one host-confirmed action.");
        }
        if (spectator && stage == 3) { stage = 4; return false; }
        if (spectator && (stage < 5 || !screen.IsSpectating)) return false;
        if (spectator && (!spectatorVerified || screen.Buttons.ContainsKey("lock") || screen.Buttons.ContainsKey("fight"))) throw new InvalidOperationException("Returning to spectating did not remove player controls.");
        if (!planningRendered || !combatRendered || !companionRendered || screen.Session.State.Seed != replica.Match!.Session.State.Seed) throw new InvalidOperationException("Online match did not render planning, combat and companion.");
        if (screen.NetworkStateHash != replica.Match.Check()) throw new InvalidOperationException("Rendered guest state differs from host after settlement.");
        if (!spectator && stage == 3) { screen.AttachNetworkMatch(host, () => now); stage = 20; return false; }
        if (!spectator && stage < 23)
        {
            if(screen.StandingRecent(1).Count!=screen.CompletedRounds)
                throw new InvalidOperationException("Reattached host lost or duplicated the completed rounds in standings history.");
            if (screen.Buttons.ContainsKey("playerRemove/123")) throw new InvalidOperationException("Host can remove itself.");
            if (stage == 21 && capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "host-players.bmp"), size.X, size.Y);
            return false;
        }
        if (!spectator && (!guest.Authority.Banned("234") || host.Authority.Lobby!["slots"]!["1"] is not null)) throw new InvalidOperationException("Host removal did not reach peer and clear its seat.");
        if(!spectator&&stage==23&&cooldown>0)return false; // Capture after the closing Players window's last rendered frame.
        if (capture is not null) FrameCapture.SaveBitmap(capture, size.X, size.Y);
        Console.WriteLine(spectator ? "Spectator UI smoke passed: read-only view, take seat, replicated controls/combat, return to spectating and matching host state."
            : "Network match UI smoke passed: guest seat-one input, replicated shop lock, ready vote, rendered 3D planning/combat, host settlement and next round."); return true;
    }
    public void Dispose() { controller.Dispose(); replica.Dispose(); }
}
