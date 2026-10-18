using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

internal sealed partial class CosmeticScenePreview
{
    private readonly PedestalInteraction pointer;
    private ImGuiMouseButton pointerButton;
    private bool pointerCaptured;
    public double PointerYaw=>pointer.YawOffset;
    public bool PointerDragging=>pointerCaptured&&pointer.Dragging;
    public int PointerReactionCount {get;private set;}
    public string? LastPointerReaction {get;private set;}

    /// <summary>Owns the image's mouse capture, including movement and release outside its bounds.</summary>
    public string? PointerItem(string id,float2 at,float2 extent)
    {
        var cursor=ImGui.GetCursorScreenPos();
        ImGui.SetCursorScreenPos(at);ImGui.InvisibleButton(id,extent,ImGuiButtonFlags.MouseButtonLeft|ImGuiButtonFlags.MouseButtonRight|ImGuiButtonFlags.MouseButtonMiddle);
        bool activated=ImGui.IsItemActivated();var mouse=ImGui.GetMousePos();
        ImGui.SetCursorScreenPos(cursor);
        if(activated)
        {
            pointerButton=ImGui.IsMouseClicked(ImGuiMouseButton.Right)?ImGuiMouseButton.Right:ImGui.IsMouseClicked(ImGuiMouseButton.Middle)?ImGuiMouseButton.Middle:ImGuiMouseButton.Left;
            pointer.Down(mouse.X);pointerCaptured=true;
        }
        // Home remains drawn behind Collection. Shared yaw must not let that
        // other widget move or cancel the active widget's mouse capture.
        if(!pointerCaptured)return null;
        // Dear ImGui invalidates mouse coordinates after focus/capture loss.
        if(!float.IsFinite(mouse.X)||mouse.X<-1e20f){CancelPointer();return null;}
        pointer.Move(mouse.X);
        if(ImGui.IsMouseDown(pointerButton))return null;
        if(!ImGui.IsMouseReleased(pointerButton)){CancelPointer();return null;}
        string? role=pointer.Up(animator is not null);pointerCaptured=false;
        if(role is not null){animator!.Play(role);LastPointerReaction=role;PointerReactionCount++;}
        return role;
    }
    private void CancelPointer(){if(pointerCaptured)pointer.Cancel();pointerCaptured=false;}
}
