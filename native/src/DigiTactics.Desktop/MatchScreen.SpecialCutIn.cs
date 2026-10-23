using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Special cut-in panel (js/cutin.js drawPanel): a tilted band with speed lines, the caster's live head
/// view in a slanted window, and the Digimon and special names. The board dims behind it.</summary>
internal sealed partial class MatchScreen
{
    /// <summary>Set by the application loop while the window is minimised; cinematics wait for a visible window.</summary>
    internal bool WindowMinimized {get;set;}
    internal int CutInPanelsDrawn {get;private set;}
    internal BattleView? ShownBattle=>battle;
    private bool cutInFaceLeft=true;
    private System.Numerics.Vector3 cutInRim=System.Numerics.Vector3.One;
    private CutInMode CutInSetting=>SpecialCinematic.Mode(clientPreferences.CutIns);

    /// <summary>Settings and gates for the shown fight, before it advances. "Mine only" means units the local player
    /// owns in their own fight; the Animations setting (reduced motion) turns cinematics off.</summary>
    private void SyncCutIns()
    {
        if(battle is not {} shown||Session is null)return;
        shown.CutIns=CutInSetting;
        shown.CutInsAllowed=ClientMotion&&!WindowMinimized&&Session.Rounds.Phase==MatchPhase.Combat&&!EliminationCinematicActive;
        int seat=localSeat;
        shown.IsMine=unit=>viewed==seat&&shown.IsAlly(unit)&&(unit.Entry.Owner is not {} owner||owner==seat);
    }

    private void DrawSpecialCutIn(ImDrawListPtr draw)
    {
        if(battle is not {} shown||shown.Cinematic.CutIn is not {} curve||shown.Cinematic.Unit is not {} id)return;
        var unit=shown.Combat.Units[id];bool ally=shown.IsAlly(unit);
        var element=ElementColor(abilityElements.Get(Rules.Id,unit.Line.Id,unit.Stage));var tier=HudHex(UnitRing.TierColor(unit.Line.Tier));
        var deep=HudHex("0a1020");
        float left=Left,top=Top,width=Width,height=Height;
        draw.AddRectFilled(new(left,top),new(left+width,top+height),new ImColor8(0,0,0,(byte)Math.Round(255*curve.Dim)));
        float bh=Math.Clamp(height*.34f,130,330),k=.45f,dir=ally?1:-1,half=width*.5f+bh;
        var axis=new float2(MathF.Cos(-7*MathF.PI/180),MathF.Sin(-7*MathF.PI/180));var across=new float2(-axis.Y,axis.X);
        var center=new float2(left+width*.5f,top+height*.42f);
        float shift=(float)curve.Offset*(half+width*.5f+bh)*dir;
        float2 P(float u,float v)=>center+axis*(u+shift)+across*v;
        // A line across the band at u, slanted like its ends ("/").
        float2 Upper(float u)=>P(u+k*bh/2,-bh/2);float2 Lower(float u)=>P(u-k*bh/2,bh/2);
        void Strip(float u0,float u1,ImColor8 color)=>draw.AddQuadFilled(Upper(u0),Upper(u1),Lower(u1),Lower(u0),color);
        // Body: dark at the text end, the special's element colour toward the caster.
        const int strips=24;
        for(int i=0;i<strips;i++)
        {
            float u0=-half+2*half*i/strips,u1=-half+2*half*(i+1)/strips,toward=ally?(i+.5f)/strips:1-(i+.5f)/strips;
            Strip(u0,u1+.5f,HudAlpha(HudMix(element,deep,.18f+.5f*toward*toward),.94f));
        }
        // Speed lines streaming away from the caster.
        double time=curve.Time;
        for(int i=0;i<30;i++)
        {
            double a=Hash(i*3+1),b=Hash(i*3+2),c=Hash(i*3+3);
            float v=(float)((a-.5)*(bh-14)),length=(float)(width*(.12+.3*b)),thick=(float)(1.2+2.2*c);
            float span=2*half+length,travel=(float)((c*span+time*(1400+900*a))%span);
            float u=ally?half-travel:-half+travel-length;
            var col=new ImColor8(255,255,255,(byte)(40+90*b));
            draw.AddQuadFilled(P(u,v),P(u+length,v),P(u+length,v+thick),P(u,v+thick),col);
        }
        // The caster's window: a brighter panel, radial lines from the head, then the live view.
        float window=width*.46f,mu0=ally?width*.02f:-window-width*.02f,mu1=mu0+window;
        float texture0=mu0-k*bh/2,textureWidth=window+k*bh;
        draw.AddQuadFilled(Upper(mu0),Upper(mu1),Lower(mu1),Lower(mu0),HudAlpha(HudMix(element,HudHex("ffffff"),.72f),.96f));
        var focus=(mu0+mu1)*.5f;
        for(int i=0;i<22;i++)
        {
            float angle=MathF.Tau*i/22+(float)time*.6f,s=MathF.Sin(angle),cs=MathF.Cos(angle);
            float reach=Math.Min(window*.75f,bh*.48f/Math.Max(Math.Abs(s),.05f)),w=reach*.05f;
            var tip=new float2(focus+cs*reach,s*reach);var side=new float2(-s,cs)*w;
            draw.AddTriangleFilled(P(focus+cs*bh*.16f,s*bh*.16f),P(tip.X+side.X,tip.Y+side.Y),P(tip.X-side.X,tip.Y-side.Y),HudAlpha(HudHex("ffffff"),.38f));
        }
        uint texture=shown.CutInTexture((int)MathF.Ceiling(textureWidth),(int)MathF.Ceiling(bh));
        float2 Uv(float u,float v)=>new((u-texture0)/textureWidth,.5f-v/bh);
        draw.AddImageQuad(new ImTextureRef{_TexID=new((nint)texture)},Upper(mu0),Upper(mu1),Lower(mu1),Lower(mu0),
            Uv(mu0+k*bh/2,-bh/2),Uv(mu1+k*bh/2,-bh/2),Uv(mu1-k*bh/2,bh/2),Uv(mu0-k*bh/2,bh/2),new ImColor8(255,255,255));
        draw.AddLine(Upper(mu0),Lower(mu0),new ImColor8(255,255,255,235),4);draw.AddLine(Upper(mu1),Lower(mu1),new ImColor8(255,255,255,235),4);
        cutInFaceLeft=ally;cutInRim=new(Linear(element.R),Linear(element.G),Linear(element.B));
        // Edges: white rules and the rarity stripe.
        draw.AddLine(Upper(-half),Upper(half),new ImColor8(255,255,255,240),3);draw.AddLine(Lower(-half),Lower(half),new ImColor8(255,255,255,240),3);
        draw.AddQuadFilled(P(-half+k*bh/2,-bh/2+6),P(half+k*bh/2,-bh/2+6),P(half+k*bh/2,-bh/2+11),P(-half+k*bh/2,-bh/2+11),HudAlpha(tier,.95f));
        // Names on the text side.
        string name=unit.Line.Forms[unit.Stage].ToUpperInvariant(),special=unit.Line.Ability.Names[unit.Stage];
        float nameSize=Math.Clamp(bh*.22f,22,54),specialSize=nameSize*.56f,textU=ally?-width*.24f:width*.24f;
        void Title(ImFontPtr font,float size,float v,string text,Rgb color,float tracking)
        {
            float textWidth=NativeUiPaint.TrackedWidth(font,size,text,tracking);var at=P(textU,v)-new float2(textWidth/2,NativeUiTheme.LineHeight(font,size)/2);
            NativeUiPaint.Tracked(draw,font,size,at+new float2(3,3),new ImColor8(0,0,0,200),text,tracking);
            NativeUiPaint.Tracked(draw,font,size,at,color,text,tracking);
        }
        Title(NativeUiTheme.DisplayBoldFont,nameSize,-bh*.12f,name,HudHex("ffffff"),.06f);
        Title(NativeUiTheme.DisplayFont,specialSize,bh*.2f,special,HudMix(element,HudHex("fff4d8"),.55f),.04f);
        CutInPanelsDrawn++;
    }
    private static double Hash(int i){double x=Math.Sin(i*12.9898+78.233)*43758.5453;return x-Math.Floor(x);}
    private static float Linear(byte channel)=>MathF.Pow(channel/255f,2.2f);
    /// <summary>After the arena: render the caster's head view into the panel texture drawn this frame.</summary>
    private void RenderSpecialCutIn()=>battle?.DrawCutIn(cutInFaceLeft,cutInRim);
}
