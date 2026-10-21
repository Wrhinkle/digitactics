using System.Text.Json;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Rules;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool showEmotes, muteEmotes;
    private bool emotesToggleHovered;
    internal (float2 Min,float2 Max)[] EmoteSlotBounds {get;private set;}=[];
    internal float2 EmoteWheelCenter {get;private set;}
    internal bool OtherEmotesMuted=>muteEmotes;
    private MatchPlanningSession? emoteSession;
    private MatchEmotes emotes = new();
    private readonly EmoteArtwork emoteArtwork = new();
    private AiEmoteReactions? aiEmotes;
    internal Func<double>? AiEmoteRandom { get; set; }
    internal Func<long> AiEmoteClock { get; set; } = () => Environment.TickCount64;
    internal int AiEmotesShown { get; private set; }
    private readonly List<(AiEmoteReaction Reaction, double At)> pendingAiEmotes = [];
    private void ScheduleAiEmotes()
    {
        EnsureEmotes(); if(Session is null || aiEmotes is null) return;
        var players=Session.State.Players.Select(p=>new AiEmotePlayer(p.Id,p.Name,p.IsHuman,p.Alive)).ToArray();
        var results=Session.Rounds.Results.Select(r=>new AiEmoteResult(r.Match.A,r.Match.B,r.Match.Pve,r.Match.Ghost,r.Match.Winner));
        if(aiEmotes.Choose(players,results,localSeat,AiEmoteRandom) is { } reaction) pendingAiEmotes.Add((reaction,AiEmoteClock()+reaction.Delay));
    }
    private void UpdateAiEmotes()
    {
        EnsureEmotes();long now=Environment.TickCount64;
        for(int i=pendingAiEmotes.Count-1;i>=0;i--)
        {
            var pending=pendingAiEmotes[i];if(AiEmoteClock()<pending.At)continue;
            if(emotes.Receive(pending.Reaction.Seat,pending.Reaction.Id,localSeat,muteEmotes,now))AiEmotesShown++;
            pendingAiEmotes.RemoveAt(i);
        }
    }
    internal int ArenaEmoteDraws { get; private set; }
    private void ArenaEmotes()
    {
        ArenaEmoteDraws = 0;
        if (showEmotes || showSoundboard || showAudio || showPlayers) return;
        EnsureEmotes(); long now = Environment.TickCount64;
        Window("Arena emote overlay", Left, Top, Width, Height, ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoFocusOnAppearing);
        void Draw(int seat, ArenaCompanionView? view)
        {
            if (view is null || emotes.At(seat, now) is not { } emote) return;
            var p = System.Numerics.Vector4.Transform(new System.Numerics.Vector4(view.Anchor, 1), arenaEnvironment.Camera.Matrix(Width, Height, battle?.Combat.Columns ?? 7));
            if (p.W <= 0) return;
            var anchor = EmoteLayout.Anchor((p.X / p.W + 1) * Width / 2, (1 - p.Y / p.W) * Height / 2, false, Width, Height);
            if (!anchor.Visible) return;
            emoteArtwork.Draw(emote.Id, now - emote.Started, new(Left + (float)anchor.X - 56, Top + (float)anchor.Y - 112), 112, true, true);
            ArenaEmoteDraws++;
        }
        if (!IsSpectating) Draw(localSeat, arenaCompanion);
        Draw(opponentCompanionSeat, opponentCompanion);
        ImGui.End();
    }
    private void EnsureEmotes()
    {
        if (ReferenceEquals(emoteSession, Session)) return;
        emoteSession = Session; emotes = new(); emotes.Played += () => audio.Play("emote");
        pendingAiEmotes.Clear();aiEmotes=Session is null?null:new(Session.State.Players.Where(p=>p.Alive).Select(p=>p.Id));
        muteEmotes = profile?.Storage.GetValueOrDefault("dt.emotes.muteOthers") == "1";
    }
    private void EmoteRow(int seat)
    {
        EnsureEmotes(); long now = Environment.TickCount64;
        if (emotes.At(seat, now) is not { } emote) return;
        emoteArtwork.Draw(emote.Id, now - emote.Started, ImGui.GetCursorScreenPos(), 52, true); ImGui.Dummy(new(52, 52));
    }
    private void EmotePanel()
    {
        EnsureEmotes(); long now = Environment.TickCount64;
        bool keyboard = SocialKeyboardAllowed;
        if (keyboard && ImGui.IsKeyPressed(ImGuiKey.T, false)) { showEmotes = !showEmotes; if (showEmotes) showSoundboard = false;overlayShortcutConsumed=true; }
        if (!showEmotes) return;
        if(keyboard&&Enumerable.Range(0,6).Any(SlotKeyPressed))overlayShortcutConsumed=true;
        // The original six image buttons surround a ring over the arena. Keep
        // the board visible and put the hovered name in the centre of the wheel.
        const float radius=121,diameter=82;
        float2 center=new(Left+Width*.5f,Math.Clamp(Top+Math.Max(174,Height*.45f),190,size.Y-230));
        float2 origin=center-new float2(180,178);
        if(PressedOutsideSocialPanel(origin,origin+new float2(360,390),emotesToggleHovered)){showEmotes=false;return;}
        EmoteWheelCenter=center;EmoteSlotBounds=new (float2,float2)[6];
        Window("Emotes",origin.X,origin.Y,360,390,ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoBackground|ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse);
        var draw=ImGui.GetWindowDrawList();
        draw.AddCircle(center,121,new ImColor8(12,16,26,210),96,54);
        string hoveredName="Emotes";
        try
        {
            var collection = new CosmeticCollection(profile?.Storage.GetValueOrDefault("dt.cosmetics.v1"));
            var slots = collection.Equipped("emote")!.AsArray();
            for (int i = 0; i < 6; i++)
            {
                string? id = slots.ElementAtOrDefault(i)?.GetValue<string>();
                double angle=-Math.PI/2+i*Math.PI/3;
                float2 slotCenter=center+new float2((float)Math.Cos(angle)*radius,(float)Math.Sin(angle)*radius);
                float2 at=slotCenter-new float2(diameter*.5f,diameter*.5f);
                EmoteSlotBounds[i]=(at,at+new float2(diameter,diameter));
                ImGui.SetCursorScreenPos(at);
                ImGui.BeginDisabled(IsSpectating || id is null);
                ImGui.PushStyleColor(ImGuiCol.Button,new ImColor8(0,0,0,0));ImGui.PushStyleColor(ImGuiCol.ButtonHovered,new ImColor8(0,0,0,0));ImGui.PushStyleColor(ImGuiCol.ButtonActive,new ImColor8(0,0,0,0));ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,0);
                bool pressed=Button("emote/"+i,"",diameter,diameter);
                ImGui.PopStyleVar();ImGui.PopStyleColor(3);
                bool focused=ImGui.IsItemFocused();
                var mouse=ImGui.GetMousePos()-slotCenter;
                bool circleHit=mouse.X*mouse.X+mouse.Y*mouse.Y<=diameter*diameter*.25f;
                bool hover=ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)&&circleHit;
                string name=CosmeticCollection.Item("emote",id)?.Name??"Empty slot";
                draw.AddCircleFilled(slotCenter,diameter*.5f,hover?new ImColor8(35,52,68):new ImColor8(18,28,43),64);
                draw.AddCircle(slotCenter,diameter*.5f,hover||focused?new ImColor8(240,218,155):new ImColor8(88,106,132),64,hover||focused?3:2);
                if(id is not null)emoteArtwork.Draw(id,now,at+new float2(6,6),70);
                else {var textAt=slotCenter-new float2(25,10);draw.AddText(ImGui.GetFont(),16,textAt,new ImColor8(165,178,196),"Empty"u8);}
                var badge=at+new float2(7,7);draw.AddCircleFilled(badge,10,new ImColor8(255,209,82),32);
                draw.AddText(ImGui.GetFont(),14,badge-new float2(4,8),new ImColor8(27,20,36),(i+1).ToString());
                if(hover||focused)hoveredName=name;
                if(hover)ResultTooltip(id is null?"Choose an emote for this slot in Collection.":name+" ("+(i+1)+")");
                if ((pressed&&(!ImGui.IsMouseReleased(ImGuiMouseButton.Left)||circleHit)) || keyboard && !IsSpectating && SlotKeyPressed(i))
                {
                    if (id is not null && emotes.Send(collection, id, localSeat, now, networkRoom is null ? null : clip => networkRoom.SendEffect(JsonSerializer.SerializeToElement(new { s = localSeat, kind = "emote", id = clip })))) showEmotes = false;
                    else status = "Too many emotes. Wait a few seconds.";
                }
                ImGui.EndDisabled();
            }
            ImGui.PushFont(default,16);
            ImGui.SetCursorScreenPos(center-ImGui.CalcTextSize(hoveredName,false,140)*.5f);
            ImGui.PushTextWrapPos(ImGui.GetCursorPosX()+140);ImGui.Text(hoveredName);ImGui.PopTextWrapPos();ImGui.PopFont();
            ImGui.SetCursorScreenPos(center+new float2(-74,176));
            if (HomeButton("muteEmotes",muteEmotes?"Others muted":"Mute others",148,32,muteEmotes))
            {
                bool muted=!muteEmotes;
                if (profile is null || profile.UpdateStorage(new Dictionary<string, string> { ["dt.emotes.muteOthers"] = muted ? "1" : "0" })) { muteEmotes = muted; if (muted) emotes.ClearOthers(localSeat); }
                else status = profile.Error ?? "Mute preference could not be saved.";
            }
            ImGui.SetCursorScreenPos(center+new float2(-48,27));ImGui.PushFont(default,13);
            ImGui.PushStyleColor(ImGuiCol.Button,new ImColor8(0,0,0,0));ImGui.PushStyleColor(ImGuiCol.Text,new ImColor8(165,178,196));ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,0);
            if(Button("closeEmotes","T to close",96,24))showEmotes=false;
            ImGui.PopStyleVar();ImGui.PopStyleColor(2);ImGui.PopFont();
        }
        catch (Exception error) when (error is JsonException or InvalidDataException or InvalidOperationException or IOException or FormatException)
        { UiText.Wrapped("Emotes could not be loaded: " + error.Message); }
        ImGui.End();
    }
    private static bool PressedOutsideSocialPanel(float2 min,float2 max,bool toggleHovered)=>
        !toggleHovered&&!ImGui.IsMouseHoveringRect(min,max,false)&&
        (ImGui.IsMouseClicked(ImGuiMouseButton.Left)||ImGui.IsMouseClicked(ImGuiMouseButton.Right)||ImGui.IsMouseClicked(ImGuiMouseButton.Middle));
}
