using System.Text.Json.Nodes;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Rules;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private static ImFontPtr eliminationAttackFont;
    internal static void LoadEliminationFonts()
    {
        string path=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts),"arialbd.ttf");
        eliminationAttackFont=File.Exists(path)?ImGui.GetIO().Fonts.AddFontFromFileTTF(path,30):default;
    }
    private MatchPlanningSession? eliminationSession;
    private EliminationQueue? eliminationQueue;
    private EliminationPlayback? eliminationCurrent;
    private EliminationStage? eliminationStage;
    private EliminationComposite? eliminationComposite;
    internal void RenderEliminationComposite(Brutal.ImGuiApi.Backends.OpenGlBackend renderer) => eliminationComposite?.Render(renderer);
    private readonly EliminationThemeRenderer eliminationThemes = new();
    private bool eliminationThemePlayed, eliminationCastPlayed;
    private string? eliminationVariant, eliminationSound;
    internal bool EliminationCinematicActive => eliminationQueue?.Current is not null;
    internal int EliminationMembers => eliminationStage?.Members ?? 0;
    internal int EliminationDrawCalls => eliminationStage?.DrawCalls ?? 0;
    internal int EliminationEffectDrawCalls => eliminationStage?.EffectDrawCalls ?? 0;
    internal EliminationPlayback? EliminationPlayback => eliminationQueue?.Current;
    internal double EliminationPoseSignature => eliminationStage?.PoseSignature ?? 0;
    internal bool EliminationSpecial => eliminationStage?.Special ?? false;
    internal bool EliminationFired => eliminationStage?.Fired ?? false;
    private void EnsureEliminationQueue()
    {
        if (ReferenceEquals(eliminationSession, Session)) return;
        ClearEliminationCinematics(); eliminationSession = Session;
        if (Session is not null) eliminationQueue = new(localSeat);
    }
    private void QueueEliminations()
    {
        EnsureEliminationQueue();
        if (Session is not null) eliminationQueue!.Enqueue(EliminationEvents.From(Session));
    }
    private void UpdateEliminationCinematic(double delta)
    {
        EnsureEliminationQueue(); eliminationQueue?.Advance(delta, IsPaused);
        if (eliminationCurrent is not null && !ReferenceEquals(eliminationCurrent, eliminationQueue?.Current))
        { eliminationStage?.Dispose(); eliminationStage = null; eliminationCurrent = null; }
    }
    private void ClearEliminationCinematics()
    {
        eliminationStage?.Dispose(); eliminationStage = null; eliminationCurrent = null;
        eliminationQueue?.Clear(); eliminationQueue = null; eliminationSession = null;
        eliminationComposite?.Dispose();eliminationComposite=null;
    }
    internal void DemoElimination(EliminationEvent entry)
    { EnsureEliminationQueue(); eliminationQueue!.Enqueue([entry]); }
    private JsonObject EliminationLook(EliminationPlayer player)
    {
        if (player.Id == localSeat) return CosmeticLoadouts.Local(profile?.Storage.GetValueOrDefault("dt.cosmetics.v1"),profile?.Storage.GetValueOrDefault("dt.legendevo.v1"));
        if (networkRoom is not null && Session!.State.Players[player.Id].IsHuman)
        {
            string? uid = networkRoom.Authority.Lobby?["slots"]?[player.Id.ToString()]?["uid"]?.GetValue<string>();
            if (uid is not null && networkRoom.Authority.Presence[uid]?["look"] is JsonObject look) return look;
        }
        return CosmeticLoadouts.Ai(player.Id,player.Name);
    }
    private void DrawEliminationCinematic()
    {
        if (eliminationQueue?.Current is not { } playback) return;
        if (!ReferenceEquals(eliminationCurrent,playback))
        {
            eliminationStage?.Dispose();eliminationStage=null;eliminationCurrent=playback;
            eliminationThemePlayed=eliminationCastPlayed=false;eliminationVariant=eliminationSound=null;
            if (playback.Event.Killer is { } killer)
            {
                var look=EliminationLook(killer);eliminationVariant=look["killscreen"]?.GetValue<string>();eliminationSound=look["elimsound"]?.GetValue<string>();
                eliminationStage=new(Rules,playback,CosmeticLoadouts.CompanionKey(look),look["dance"]?.GetValue<string>()??"floss",eliminationVariant);
            }
        }
        if (!playback.Attack && !eliminationThemePlayed)
        {
            // Source killcam handoff plays the basic sting; ordinary lineups use
            // the equipped theme. The equipped elimination clip follows both.
            eliminationThemePlayed=true;audio.PlayEliminationTheme(playback.Cinematic?"ks_basic":eliminationVariant,playback.Big,playback.Event.Killer is null);
            if (playback.Event.Killer is { } killer) audio.PlayEliminationSound(eliminationSound,killer.Id==localSeat);
        }
        if (playback.Attack && playback.Time>.55 && !eliminationCastPlayed && playback.Event.FinalHit is { } hit)
        { eliminationCastPlayed=true;audio.Play($"cast_{abilityElements.Get(Rules.Id,hit.Line,hit.Stage)}_{hit.Stage}"); }
        float width=playback.Big?size.X:Math.Min(320,Width*.6f), height=playback.Big?size.Y:MathF.Round(width*.56f);
        float2 origin=playback.Big?new(0,0):new(Left+12,Top+Height-height-12),extent=new(width,height);
        Window("Elimination cinematic",origin.X,origin.Y,width,height,ImGuiWindowFlags.NoBackground|ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse);
        var draw=ImGui.GetWindowDrawList();draw.PushClipRect(origin,origin+extent,true);
        float opacity=1;
        eliminationThemes.Draw(eliminationVariant,playback.Event.Killer is null,playback.Time,origin,extent,opacity);
        if(playback.Attack)DrawFinalHitBackdrop(playback,origin,extent,opacity);
        eliminationStage?.Draw((int)width,(int)height);
        if(eliminationStage is not null)draw.AddImage(new ImTextureRef{_TexID=new((nint)eliminationStage.Texture)},origin,origin+extent,new(0,1),new(1,0),new ImColor8(255,255,255,(byte)(255*opacity)));
        if(playback.Big)
        {
            float bars=height*.07f*(float)Ease(playback.Time/.35);
            draw.AddRectFilled(origin,new(origin.X+width,origin.Y+bars),new ImColor8(0,0,0,(byte)(255*opacity)));
            draw.AddRectFilled(new(origin.X,origin.Y+height-bars),origin+extent,new ImColor8(0,0,0,(byte)(255*opacity)));
        }
        DrawEliminationCaption(playback,origin,extent,opacity);
        draw.PopClipRect();
        (eliminationComposite??=new()).Capture(draw,origin,extent,(float)playback.Opacity,ImGui.GetForegroundDrawList(),playback.Big?0:10);
        ImGui.SetCursorScreenPos(origin);
        bool clicked=ImGui.InvisibleButton("skipElimination"u8,extent);
        Buttons["skipElimination"]=origin+extent*.5f;
        bool skip=clicked || ImGui.IsKeyPressed(ImGuiKey.Escape,false);
        ImGui.End();
        // The captured draw list still references this frame's stage texture. Its lifetime
        // ends after RenderEliminationComposite; Update disposes the skipped stage next frame.
        if(skip) eliminationQueue.Skip();
    }
    private static double Ease(double fraction) => EliminationPlayback.CssEase(fraction);
    private static string PlacementOrdinal(int n)=>n+(n%100 is >=11 and <=13?"th":(n%10) switch{1=>"st",2=>"nd",3=>"rd",_=>"th"});
    private void DrawEliminationCaption(EliminationPlayback playback,float2 origin,float2 extent,float opacity)
    {
        var entry=playback.Event;string Player(EliminationPlayer p)=>p.Id==localSeat?"You":p.Name;
        string victim=entry.Victims.Length>1?string.Join(" & ",entry.Victims.Select(p=>p.Id==localSeat?"you":p.Name)):entry.Victim.Id==localSeat?"you":entry.Victim.Name;
        string main=entry.Killer is { } killer?$"{Player(killer)} eliminated {victim}":$"{Player(entry.Victim)} {(entry.Victim.Id==localSeat?"were":"was")} eliminated";
        string placed=entry.Victim.Placement!=0?$"{(Session!.State.Duos?$"Team {entry.Victim.Team+1}":Player(entry.Victim))} placed {PlacementOrdinal(entry.Victim.Placement)}":"";
        string extra=entry.Killer is null?"lost to Wild Digimon":entry.Ghost?"by a ghost board":"";
        string sub=string.Join(" \u00b7 ",new[]{placed,extra}.Where(s=>s.Length>0));
        var line=entry.FinalHit is { } h?Rules.Lines.Concat(Rules.ExtraLines).Single(l=>l.Id==h.Line):null;
        if(playback.Attack && entry.FinalHit is { } hit){main=line!.Ability.Names[hit.Stage].ToUpperInvariant()+"!!";sub=line.Forms[hit.Stage].ToUpperInvariant()+" \u00b7 FINAL ATTACK";}
        float mainSize=playback.Attack?Math.Clamp(size.X*.05f,26,76):playback.Big?30:15,subSize=playback.Attack?14:playback.Big?15:11;
        float bottom=extent.Y*(playback.Attack?.15f:playback.Big?.09f:.06f);
        float mainY=origin.Y+extent.Y-bottom-mainSize-subSize-4+18*(1-(float)Ease((playback.Time-.25)/.4));
        float mainAlpha=opacity*(float)Ease((playback.Time-.25)/.4),subAlpha=opacity*(float)Ease((playback.Time-.55)/.4);
        var rim=EliminationThemes.Rim(eliminationVariant,entry.Killer is null);
        string? accented=!playback.Attack&&entry.Killer is { } credited?Player(credited).ToUpperInvariant():null;
        Text(main.ToUpperInvariant(),mainY,mainSize,new(255,255,255,(byte)(255*mainAlpha)),true,mainSize*.06f,accented);
        var color=EliminationThemes.Text(eliminationVariant,entry.Killer is null);
        Text(sub,origin.Y+extent.Y-bottom-subSize,subSize,new((byte)color.R,(byte)color.G,(byte)color.B,(byte)(255*subAlpha)),false,playback.Attack?subSize*.22f:0);
        if(!playback.Attack && entry.FinalHit is { } honor)Text("FINAL HIT \u00b7 "+line!.Forms[honor.Stage].ToUpperInvariant(),origin.Y+extent.Y*.12f,13,new(255,232,162,(byte)(255*opacity)),true,13*.14f);
        if(playback.Big)
        {
            string tag=string.Join(" \u00b7 ",new[]{CosmeticCollection.Item("killscreen",eliminationVariant)?.Name,CosmeticCollection.Item("elimsound",eliminationSound)?.Name}.OfType<string>());
            var textSize=ImGui.CalcTextSize(tag)* (11/ImGui.GetFontSize());
            ImGui.GetWindowDrawList().AddText(ImGui.GetFont(),11,new(origin.X+extent.X-textSize.X-10,origin.Y+8),new ImColor8(255,255,255,(byte)(140*opacity)),tag);
        }
        unsafe void Text(string value,float y,float fontSize,ImColor8 color,bool bold=false,float spacing=0,string? accent=null)
        {
            var font=bold?(playback.Attack?eliminationAttackFont:evolutionAnnouncementFont):ImGui.GetFont();
            if(font.Ptr==null)font=ImGui.GetFont();
            var glyphs=value.EnumerateRunes().Select(r=>r.ToString()).ToArray();
            float Measure()=>glyphs.Sum(g=>font.CalcTextSizeA(fontSize,float.MaxValue,0,g).X)+spacing*Math.Max(0,glyphs.Length-1);
            float measured=Measure();
            if(measured>extent.X-32){float scale=Math.Max(.1f,(extent.X-32)/measured);fontSize*=scale;spacing*=scale;measured=Measure();}
            float2 at=new(origin.X+(extent.X-measured)/2,y);var draw=ImGui.GetWindowDrawList();
            int first=draw.VtxBuffer.Count;
            void Run(float2 offset,ImColor8 ink,bool colors=false)
            {
                float x=at.X;int position=0;
                foreach(string glyph in glyphs)
                {
                    var tint=colors&&accent is not null&&position<accent.Length?new ImColor8((byte)rim.R,(byte)rim.G,(byte)rim.B,color.AsByte4().W):ink;
                    draw.AddText(font,fontSize,new float2(x,at.Y)+offset,tint,glyph);
                    x+=font.CalcTextSizeA(fontSize,float.MaxValue,0,glyph).X+spacing;position+=glyph.Length;
                }
            }
            if(playback.Attack&&bold)
            {
                Run(new(4,3),new(6,42,70,color.AsByte4().W));Run(new(-3,-2),new((byte)rim.R,(byte)rim.G,(byte)rim.B,color.AsByte4().W));
            }
            else Run(new(0,2),new(0,0,0,(byte)(color.AsByte4().W*.8f)));
            Run(new(0,0),color,true);
            if(playback.Attack)
            {
                float center=origin.Y+extent.Y-bottom-(mainSize+subSize+4)/2;
                for(int i=first;i<draw.VtxBuffer.Count;i++){ref var vertex=ref draw.VtxBuffer.Data[i];vertex.pos.X-=MathF.Tan(7*MathF.PI/180)*(vertex.pos.Y-center);}
            }
        }
    }
    private void DrawFinalHitBackdrop(EliminationPlayback playback,float2 origin,float2 extent,float opacity)
    {
        double t=playback.Time;var draw=ImGui.GetWindowDrawList();var rim=EliminationThemes.Rim(eliminationVariant,false);
        var color=new ImColor8((byte)rim.R,(byte)rim.G,(byte)rim.B,(byte)(255*opacity*(.25+.2*Math.Sin(t*9))));
        for(int i=0;i<48;i++)
        {
            double a=i*Math.PI/24,k=.2+((t*.7+i*.17)%1)*.8;
            draw.AddLine(origin+new float2((float)(extent.X/2+Math.Cos(a)*extent.X*k*.4),(float)(extent.Y*.45+Math.Sin(a)*extent.Y*k*.4)),origin+new float2((float)(extent.X/2+Math.Cos(a)*extent.X*k),(float)(extent.Y*.45+Math.Sin(a)*extent.Y*k)),color);
        }
        if(t>2.8)for(int i=0;i<12;i++)
        {
            static double R(int i,int k=0){double x=Math.Sin(i*127.1+k*311.7)*43758.5453;return x-Math.Floor(x);}
            float2 at=origin+new float2((float)(R(i)*extent.X),(float)(R(i,2)*extent.Y));
            draw.AddRect(at,at+new float2((float)(20+R(i,3)*130),(float)(10+R(i,4)*70)),new ImColor8(215,250,255,(byte)(255*opacity*Math.Min(1,(t-2.8)*2))));
        }
        draw.AddRectFilled(origin,origin+extent,new ImColor8(235,251,255,(byte)(255*opacity*playback.Flash)));
    }
}
