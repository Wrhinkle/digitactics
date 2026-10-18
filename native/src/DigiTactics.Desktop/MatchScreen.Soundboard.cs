using System.Text.Json;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Rules;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool showSoundboard;
    private bool soundboardToggleHovered;
    private int soundboardHitSlot=-1;
    private long soundboardHitAt;
    internal (float2 Min,float2 Max)[] SoundboardPadBounds {get;private set;}=[];
    internal int SoundboardColumns {get;private set;}
    internal int SoundboardLastPlayedSlot=>soundboardHitSlot;
    internal long SoundboardCooldown=>soundboard.Cooldown(Environment.TickCount64);
    private MatchPlanningSession? soundboardSession;
    private MatchSoundboard soundboard = new();
    internal int ReceivedSoundboardEffects { get; private set; }
    private void EnsureSoundboard()
    {
        if (ReferenceEquals(soundboardSession, Session)) return;
        soundboardSession = Session; soundboard = new(); soundboard.Play += audio.PlayBoard;
    }
    private void NetworkEffect(string sender, JsonElement effect)
    {
        if (Session is null || PlayerMuted(sender)) return;
        if (effect.GetProperty("kind"u8).GetString() == "emote")
        {
            EnsureEmotes(); emotes.Receive((int)effect.GetProperty("s"u8).GetDouble(), effect.GetProperty("id"u8).GetString()!, localSeat, muteEmotes, Environment.TickCount64); return;
        }
        if (effect.GetProperty("kind"u8).GetString() != "sound") return;
        EnsureSoundboard();
        if (soundboard.Receive((int)effect.GetProperty("s"u8).GetDouble(), effect.GetProperty("id"u8).GetString()!, localSeat, audio.MuteOthers, Environment.TickCount64)) ReceivedSoundboardEffects++;
    }
    private void Soundboard()
    {
        EnsureSoundboard(); long now = Environment.TickCount64;
        bool keyboard = SocialKeyboardAllowed;
        if (keyboard && ImGui.IsKeyPressed(ImGuiKey.G, false)) { showSoundboard = !showSoundboard; if (showSoundboard) showEmotes = false;overlayShortcutConsumed=true; }
        if (!showSoundboard) return;
        if(keyboard&&Enumerable.Range(0,6).Any(SlotKeyPressed))overlayShortcutConsumed=true;
        // Match the compact green pad deck anchored to the bottom of the arena.
        // Narrow boards use two rows, keeping every name and shortcut readable.
        int columns=Width>=760?6:3;const float padWidth=112,padHeight=90,gap=8;
        float width=columns*padWidth+(columns-1)*gap+28,height=columns==6?200:298;
        float2 origin=new(Left+(Width-width)*.5f,Math.Max(Top+12,Top+Height-height-12));
        if(PressedOutsideSocialPanel(origin,origin+new float2(width,height),soundboardToggleHovered)){showSoundboard=false;return;}
        SoundboardColumns=columns;SoundboardPadBounds=new (float2,float2)[6];
        ImGui.PushStyleColor(ImGuiCol.WindowBg,new ImColor8(10,25,20,250));ImGui.PushStyleColor(ImGuiCol.Border,new ImColor8(47,143,106));
        Window("Soundboard",origin.X,origin.Y,width,height,ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse);ImGui.PopStyleColor(2);
        HomeLabel("SOUNDBOARD",20,new ImColor8(127,232,184));
        HomeLabel("1-6 to play / G to close",16,new ImColor8(181,214,197));
        var draw=ImGui.GetWindowDrawList();float2 pads=origin+new float2(14,62);
        try
        {
            var collection = new CosmeticCollection(profile?.Storage.GetValueOrDefault("dt.cosmetics.v1"));
            var slots = collection.Equipped("sound")!.AsArray();
            for (int i = 0; i < 6; i++)
            {
                string? id = slots.ElementAtOrDefault(i)?.GetValue<string>();
                bool disabled=IsSpectating||id is null||soundboard.Cooldown(now)>0;
                var at=pads+new float2(i%columns*(padWidth+gap),i/columns*(padHeight+gap));
                SoundboardPadBounds[i]=(at,at+new float2(padWidth,padHeight));ImGui.SetCursorScreenPos(at);
                ImGui.PushStyleColor(ImGuiCol.Button,new ImColor8(19,54,43));ImGui.PushStyleColor(ImGuiCol.ButtonHovered,new ImColor8(27,74,58));ImGui.PushStyleColor(ImGuiCol.ButtonActive,new ImColor8(37,101,73));
                ImGui.BeginDisabled(disabled);
                bool pressed=Button("soundboard/"+i,"",padWidth,padHeight);
                bool hover=ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled),focused=ImGui.IsItemFocused();
                var ink=disabled?new ImColor8(112,151,132):new ImColor8(223,247,236);
                var accent=disabled?new ImColor8(68,113,91):new ImColor8(127,232,184);
                draw.AddRect(at,at+new float2(padWidth,padHeight),focused||hover?new ImColor8(90,223,168):new ImColor8(44,110,85),7,thickness:focused?2:1);
                if(soundboardHitSlot==i&&now-soundboardHitAt<350)draw.AddRect(at-new float2(2,2),at+new float2(padWidth+2,padHeight+2),new ImColor8(90,223,168),8,thickness:3);
                draw.AddText(ImGui.GetFont(),16,at+new float2(7,4),accent,(i+1).ToString());
                var icon=at+new float2(padWidth*.5f,27);
                if(id is null){draw.AddLine(icon+new float2(-8,0),icon+new float2(8,0),accent,2);draw.AddLine(icon+new float2(0,-8),icon+new float2(0,8),accent,2);}
                else if(CollectionArt.Value.TryGetValue("sound/"+id,out string? artwork))collectionArtwork.Draw(artwork,at+new float2((padWidth-80)/2,-3),new(80,60),disabled?(byte)140:(byte)255);
                else {draw.AddCircleFilled(icon+new float2(-7,8),5,accent);draw.AddLine(icon+new float2(-3,7),icon+new float2(-3,-11),accent,3);draw.AddLine(icon+new float2(-3,-11),icon+new float2(10,-7),accent,3);}
                string name=CosmeticCollection.Item("sound",id)?.Name??"Empty slot";
                draw.AddText(ImGui.GetFont(),16,at+new float2(8,48),ink,name,padWidth-16,new float4(at.X+6,at.Y+44,at.X+padWidth-6,at.Y+padHeight-5));
                if(hover)ResultTooltip(id is null?"Choose a clip for this slot in Collection.":name+" ("+(i+1)+")");
                if (pressed ||
                    keyboard && !IsSpectating && SlotKeyPressed(i))
                    if(soundboard.Send(collection, i, localSeat, now, networkRoom is null ? null : clip => networkRoom.SendEffect(JsonSerializer.SerializeToElement(new { s = localSeat, kind = "sound", id = clip })))){soundboardHitSlot=i;soundboardHitAt=now;}
                ImGui.EndDisabled();ImGui.PopStyleColor(3);
            }
            float footer=columns==6?158:256;long cooldown=soundboard.Cooldown(now);
            draw.AddRectFilled(origin+new float2(14,footer-5),origin+new float2(width-14,footer-1),new ImColor8(11,31,24),2);
            if(cooldown>0)draw.AddRectFilled(origin+new float2(14,footer-5),origin+new float2(14+(width-28)*cooldown/5000f,footer-1),new ImColor8(90,223,168),2);
            ImGui.SetCursorScreenPos(origin+new float2(14,footer+6));
            HomeLabel(IsSpectating?"Spectating":cooldown>0?$"Ready in {cooldown/1000d:F1}s":"Two clips per five seconds",14,new ImColor8(181,214,197));
            ImGui.SetCursorScreenPos(origin+new float2(width-99,footer));if(HomeButton("closeSoundboard","Close",85,36))showSoundboard=false;
        }
        catch (Exception error) when (error is JsonException or InvalidDataException or InvalidOperationException or FormatException)
        { UiText.Wrapped("Soundboard could not be loaded: " + error.Message); }
        ImGui.End();
    }
}
