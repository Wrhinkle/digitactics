using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal bool MatchMusicNextEnabled {get;private set;}
    internal string MusicStatusText {get;private set;}="";
    internal (float2 Min,float2 Max) MusicStatusBounds {get;private set;}
    internal (float2 Min,float2 Max) MatchSoundScrollBounds {get;private set;}
    // The source popover enables Next only while custom music is actually playing.
    // A selected track ID also exists while its decoder is still opening.
    private bool CanAdvanceMusic(CustomMusicController controller)=>audio.MusicSource=="own"&&controller.Playing&&!audio.Muted&&!audio.MusicOff&&audio.MusicVolume>.001f&&controller.Library?.Tracks.Count>1;
    private void MusicStatus(CustomMusicController controller)
    {
        string? current=controller.Playing?controller.Library?.Tracks.FirstOrDefault(t=>t.Id==controller.CurrentTrackId)?.Name:null;
        if(audio.Muted||audio.MusicOff)MusicStatusText="Music is off.";
        else if(current is not null)MusicStatusText="Now playing: "+current;
        else if(audio.BuiltinMusicPhase is {} phase)
            MusicStatusText=$"Now playing: {audio.BuiltinTrackName(phase)} ({(phase=="combat"?"combat":"planning")} theme)";
        else if(audio.MusicSource=="own"&&controller.Library?.Tracks.Count>0)MusicStatusText="Your files play during a game.";
        else MusicStatusText=$"Built-in: {audio.BuiltinTrackName("planning")} for planning, {audio.BuiltinTrackName("combat")} for combat.";
        UiText.Wrapped(MusicStatusText);
        MusicStatusBounds=(ImGui.GetItemRectMin(),ImGui.GetItemRectMax());
    }
    private void AudioControls()
    {
        MatchSoundScrollBounds=(ImGui.GetWindowDrawList().GetClipRectMin(),ImGui.GetWindowDrawList().GetClipRectMax());
        // Original soundui.js: compact, right-anchored popover with volume rows
        // followed by the music source and file list, all in the same surface.
        ImGui.PushFont(default,16);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,new float2(4,2));
        HomeLabel("Sound",18,ClientInk);
        ImGui.SameLine(Math.Max(180,ImGui.GetContentRegionAvail().X-108));
        bool muted=audio.Muted;
        if(ImGui.Checkbox("Mute all##matchMute"u8,ref muted))audio.Muted=muted;
        Buttons["mute"]=(ImGui.GetItemRectMin()+ImGui.GetItemRectMax())*.5f;
        float effects=audio.EffectsVolume,music=audio.MusicVolume,board=audio.BoardVolume;
        if(MatchSoundSlider("matchMusicVolume","Music",ref music,true))audio.MusicVolume=music;
        if(MatchSoundSlider("matchEffectsVolume","Effects",ref effects))audio.EffectsVolume=effects;
        if(MatchSoundSlider("matchBoardVolume","Soundboard",ref board))audio.BoardVolume=board;
        bool muteOthers=audio.MuteOthers;
        if(ImGui.Checkbox("Mute other players' soundboard"u8,ref muteOthers))audio.MuteOthers=muteOthers;
        if(audio.Error is {} error)UiText.Wrapped(error);
        MatchMusicNextEnabled=false;
        if(audio.MusicController is {} controller)
        {
            ImGui.Spacing();HomeLabel("MUSIC",14,ClientMuted);
            ImGui.BeginDisabled(controller.LibraryBusy||musicPicker is not null||controller.Library is null||closeAfterFileOperations);
            bool own=audio.MusicSource=="own"&&controller.Library?.Tracks.Count>0;
            if(ImGui.RadioButton("Built-in soundtrack"u8,!own))audio.MusicSource="builtin";
            Buttons["matchMusicBuiltin"]=(ImGui.GetItemRectMin()+ImGui.GetItemRectMax())*.5f;
            ImGui.SameLine();
            if(ImGui.RadioButton("My music files"u8,own))
            {
                if(controller.Library?.Tracks.Count is not >0)BeginMusicSelection();
                else audio.MusicSource="own";
            }
            Buttons["matchMusicOwn"]=(ImGui.GetItemRectMin()+ImGui.GetItemRectMax())*.5f;
            ImGui.EndDisabled();
            ImGui.PushStyleColor(ImGuiCol.Text,ClientMuted);MusicStatus(controller);
            ImGui.Spacing();
            UiText.Wrapped("Use your own music: add audio files you own. They stay on this computer and are never uploaded. They replace the built-in tracks and play in shuffled order."u8);
            ImGui.PopStyleColor();ImGui.Spacing();
            ImGui.BeginDisabled(controller.LibraryBusy||musicPicker is not null||controller.Library is null||closeAfterFileOperations);
            if(HomeButton("addMusic","Add files...",108,32))BeginMusicSelection();
            ImGui.SameLine();MatchMusicNextEnabled=CanAdvanceMusic(controller);
            ImGui.BeginDisabled(!MatchMusicNextEnabled);
            if(HomeButton("nextMusic","Next track",102,32))controller.Next();
            ImGui.EndDisabled();ImGui.SameLine();
            ImGui.BeginDisabled(controller.Library?.Tracks.Count is not >0);
            if(HomeButton("clearMusic","Remove all",106,32)){clearMusic=true;removeMusicId=null;}
            ImGui.EndDisabled();ImGui.EndDisabled();
            MusicRemovalConfirmation(controller);
            MusicTrackRows(controller,true);
            if(controller.Loading||musicPicker is not null)ImGui.Text("Loading music..."u8);
            if(musicPickerError is {} pickerError)UiText.Wrapped(pickerError);
            if(controller.EditError is {} editError)UiText.Wrapped(editError);
            if(controller.Error is {} musicError)UiText.Wrapped(musicError);
        }
        if(audio.SettingsError is {} settingsError)UiText.Wrapped(settingsError);
        ImGui.PopStyleVar();ImGui.PopFont();
    }
    private bool MatchSoundSlider(string id,string label,ref float value,bool musicToggle=false)
    {
        float2 at=ImGui.GetCursorScreenPos();float width=ImGui.GetContentRegionAvail().X;
        bool musicOn=!audio.MusicOff;
        if(musicToggle)
        {
            if(ImGui.Checkbox("Music##matchMusicOn"u8,ref musicOn))audio.MusicOff=!musicOn;
            Buttons["matchMusicOn"]=(ImGui.GetItemRectMin()+ImGui.GetItemRectMax())*.5f;
        }
        else {ImGui.AlignTextToFramePadding();ImGui.Text(label);}
        ImGui.SameLine();ImGui.SetCursorScreenPos(at+new float2(116,0));
        ImGui.SetNextItemWidth(Math.Max(96,width-158));
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,new float2(4,3));
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,0);
        foreach(var color in new[]{ImGuiCol.FrameBg,ImGuiCol.FrameBgHovered,ImGuiCol.FrameBgActive,ImGuiCol.SliderGrab,ImGuiCol.SliderGrabActive})ImGui.PushStyleColor(color,new ImColor8(0,0,0,0));
        float percent=value*100;
        bool changed=ImGui.SliderFloat("##"+id,ref percent,0,100,""u8);
        ImGui.PopStyleColor(5);ImGui.PopStyleVar(2);
        Buttons[id]=(ImGui.GetItemRectMin()+ImGui.GetItemRectMax())*.5f;
        if(changed)value=MathF.Round(percent)/100;
        // Previews must observe the slider as the last submitted item.
        if(ImGui.IsItemDeactivatedAfterEdit())
        {
            if(id=="matchEffectsVolume")audio.Play("buy");
            else if(id=="matchBoardVolume")audio.PlayBoard("digivice");
        }
        var min=ImGui.GetItemRectMin();var max=ImGui.GetItemRectMax();
        float center=(min.Y+max.Y)*.5f,left=min.X+8,right=max.X-8;
        var draw=ImGui.GetWindowDrawList();var cyan=new ImColor8(63,214,255);
        draw.AddRectFilled(new(left,center-3),new(right,center+3),new ImColor8(91,94,98),3);
        draw.AddRectFilled(new(left,center-3),new(left+(right-left)*value,center+3),cyan,3);
        draw.AddCircleFilled(new(left+(right-left)*value,center),7,cyan);
        string percentage=MathF.Round(percent).ToString(System.Globalization.CultureInfo.InvariantCulture);
        ImGui.GetWindowDrawList().AddText(ImGui.GetFont(),16,at+new float2(width-ImGui.CalcTextSize(percentage).X,3),ClientMuted,percentage);
        return changed;
    }
}
