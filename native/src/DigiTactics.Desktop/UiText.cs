using Brutal.ImGuiApi;

namespace DigiTactics.Desktop;

internal static class UiText
{
    /// <summary>Wrap literal content without treating percent signs as native printf arguments.</summary>
    public static void Wrapped(ImString value)
    {
        // BRUTAL's Text is its unformatted binding. TextWrapped forwards to printf.
        ImGui.PushTextWrapPos(0);
        try { ImGui.Text(value); }
        finally { ImGui.PopTextWrapPos(); }
    }
}
