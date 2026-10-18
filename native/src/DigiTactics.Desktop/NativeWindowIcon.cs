using Brutal.GlfwApi;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

/// <summary>The same original artwork used by the executable, title bar and taskbar.</summary>
internal static class NativeWindowIcon
{
    public static unsafe void Apply(GlfwWindow window)
    {
        using var source=typeof(NativeWindowIcon).Assembly.GetManifestResourceStream("DigiTactics.ApplicationIcon.png")
            ??throw new InvalidDataException("The application icon is missing.");
        using var encoded=new MemoryStream();source.CopyTo(encoded);
        var image=PngImage.Decode(encoded.ToArray());
        // GLFW copies these top-down, straight-alpha RGBA pixels during SetIcon.
        fixed(byte* pixels=image.Rgba)
            window.SetIcon(1,new GlfwImage{Width=image.Width,Height=image.Height,Pixels=pixels});
    }
}
