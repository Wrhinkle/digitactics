using Brutal.GlfwApi;
using Brutal.ImGuiApi;
using Brutal.ImGuiApi.Backends;
using Brutal.Numerics;
using Brutal.OpenGlApi;

namespace DigiTactics.Desktop;

internal static class EliminationThemeSmoke
{
    public static readonly string[] Themes=["basic","gate","courage","glitch","pillar","ocean","firewall","stream","mega","confetti","sting",..Enumerable.Range(1,20).Select(i=>"mystery_daily_killscreen_"+i),..Enumerable.Range(1,20).Select(i=>"mystery_vault_killscreen_"+i)];
    public static unsafe void Run(string? capture)
    {
        if(!Glfw.Init())throw new InvalidOperationException("BRUTAL GLFW initialization failed.");
        try
        {
            Glfw.WindowHint(GlfwWindowHint.ContextVersionMajor,4);Glfw.WindowHint(GlfwWindowHint.ContextVersionMinor,5);Glfw.WindowHint(GlfwWindowHint.Visible,0);
            const int width=1360,height=760;
            using var window=Glfw.CreateWindow(new GlfwWindow.CreateInfo{Title="Elimination theme verification",Size=new(width,height)});window.MakeContextCurrent();
            if(!Glad.Initialize(name=>Glfw.PInvoke.GetProcAddress(name)))throw new InvalidOperationException("OpenGL initialization failed.");
            var context=ImGui.CreateContext();
            try
            {
                ImGui.GetIO().IniFilename=default;
                using var platform=new GlfwBackend(window,GlfwBackend.ClientApi.OpenGL);using var backend=new OpenGlBackend();
                var themes=new EliminationThemeRenderer();
                for(int frame=0;frame<6;frame++)
                {
                    UiFrameLifetime.Begin(); Glfw.PollEvents();backend.NewFrame();platform.NewFrame();ImGui.NewFrame();
                    ImGui.SetNextWindowPos(new(0,0));ImGui.SetNextWindowSize(new(width,height));ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(0,0));
                    ImGui.Begin("Theme raster checks"u8,ImGuiWindowFlags.NoDecoration|ImGuiWindowFlags.NoMove|ImGuiWindowFlags.NoSavedSettings|ImGuiWindowFlags.NoBackground|ImGuiWindowFlags.NoScrollbar);
                    int page=Math.Max(0,frame-2);
                    for(int i=page*16;i<Math.Min(Themes.Length,(page+1)*16);i++)
                    {
                        themes.Draw(Themes[i],false,.7,new(i%4*340,i%16/4*190),new(340,190));
                        if(themes.Operations<6)throw new InvalidOperationException("Theme rendered no procedural geometry: "+Themes[i]);
                    }
                    ImGui.End();ImGui.PopStyleVar();ImGui.Render();
                    var size=window.FramebufferSize;OpenGl.Viewport(0,0,size.X,size.Y);OpenGl.ClearColor(new float4(0,0,0,1));OpenGl.Clear(GlClearBit.Color);backend.RenderDrawData(ImGui.GetDrawData());
                    if(frame>=2)
                    {
                        if(size.X!=width||size.Y!=height)throw new InvalidOperationException($"Theme comparison requires a 1:1 framebuffer: {size.X}x{size.Y}.");
                        if(capture is not null)FrameCapture.SaveBitmap(page==0?capture:Path.ChangeExtension(capture,"page"+page+".bmp"),width,height);
                        if(OpenGl.GetError()!=OpenGl.GL_NO_ERROR)throw new InvalidOperationException("Theme rendering generated a GL error.");
                    }
                    window.SwapBuffers();
                }
            }
            finally{ImGui.DestroyContext(context);}
        }
        finally{Glfw.Terminate();}
        Console.WriteLine("Elimination themes: all 51 backgrounds rendered with native BRUTAL graphics at the source fixture time.");
    }
}
