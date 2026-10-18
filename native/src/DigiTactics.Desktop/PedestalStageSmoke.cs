using Brutal.GlfwApi;
using Brutal.ImGuiApi;
using Brutal.ImGuiApi.Backends;
using Brutal.OpenGlApi;

namespace DigiTactics.Desktop;

/// <summary>A fixed production Stage render for comparison with render-pedestal-reference.cjs.</summary>
internal static class PedestalStageSmoke
{
    public static unsafe void Run(string? capture)
    {
        if(CosmeticScenePreview.SelectSamples([8,4,2],[8,2])!=2||CosmeticScenePreview.SelectSamples([4,2],[4,2])!=4||CosmeticScenePreview.SelectSamples([4],[])!=1)
            throw new InvalidOperationException("Preview antialiasing did not respect color/depth sample support.");
        if(!Glfw.Init())throw new InvalidOperationException("BRUTAL GLFW initialization failed.");
        try
        {
            Glfw.WindowHint(GlfwWindowHint.ContextVersionMajor,4);Glfw.WindowHint(GlfwWindowHint.ContextVersionMinor,5);Glfw.WindowHint(GlfwWindowHint.Visible,0);
            const int width=CosmeticScenePreview.Width,height=CosmeticScenePreview.Height;
            using var window=Glfw.CreateWindow(new GlfwWindow.CreateInfo{Title="Pedestal Stage source verification",Size=new(width,height)});window.MakeContextCurrent();
            if(!Glad.Initialize(name=>Glfw.PInvoke.GetProcAddress(name)))throw new InvalidOperationException("OpenGL initialization failed.");
            var context=ImGui.CreateContext();
            try
            {
                ImGui.GetIO().IniFilename=default;
                using var platform=new GlfwBackend(window,GlfwBackend.ClientApi.OpenGL);using var backend=new OpenGlBackend();
                using var preview=new CosmeticScenePreview();
                for(int frame=0;frame<3;frame++)
                {
                    UiFrameLifetime.Begin(); Glfw.PollEvents();backend.NewFrame();platform.NewFrame();ImGui.NewFrame();
                    preview.Draw("legend","tokomon","tokomon",0,motion:false);
                    ImGui.GetBackgroundDrawList().AddImage(new ImTextureRef{_TexID=new((nint)preview.Texture)},new(0,0),new(width,height),new(0,1),new(1,0));
                    ImGui.Render();OpenGl.Viewport(0,0,width,height);OpenGl.ClearColor(new(0,0,0,1));OpenGl.Clear(GlClearBit.Color);backend.RenderDrawData(ImGui.GetDrawData());
                    if(frame==2)
                    {
                        if(window.FramebufferSize.X!=width||window.FramebufferSize.Y!=height)throw new InvalidOperationException("Pedestal comparison requires a 1:1 framebuffer.");
                        if(preview.PedestalDrawCalls!=6||preview.DrawCalls<=6)throw new InvalidOperationException("Pedestal render omitted the six-piece Stage or original companion.");
                        var rgba=new byte[width*height*4];fixed(byte* data=rgba)OpenGl.GetTextureImage(preview.Texture,0,OpenGl.GL_RGBA,OpenGl.GL_UNSIGNED_BYTE,rgba.Length,data);
                        int visible=0,edges=0;for(int i=3;i<rgba.Length;i+=4){if(rgba[i]>0)visible++;if(rgba[i]>0&&rgba[i]<255)edges++;}
                        if(visible<10000||visible>width*height*.6)throw new InvalidOperationException("Pedestal source framing or transparent background diverged.");
                        if(preview.Samples>1&&edges==0)throw new InvalidOperationException("Multisample preview lost its transparent edge coverage.");
                        if(capture is not null)FrameCapture.SaveBitmap(capture,width,height);
                        Console.WriteLine($"Pedestal Stage source render passed: {visible} visible pixels, {preview.DrawCalls} draws, all 6 source pieces, {preview.Samples}x sampling/{edges} alpha edges, time 0, 384x240.");
                    }
                    window.SwapBuffers();
                }
            }
            finally{ImGui.DestroyContext(context);}
        }
        finally{Glfw.Terminate();}
    }
}
