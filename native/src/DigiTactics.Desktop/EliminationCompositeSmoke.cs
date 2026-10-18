using Brutal.GlfwApi;
using Brutal.ImGuiApi;
using Brutal.ImGuiApi.Backends;
using Brutal.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

// Isolates CSS group compositing from model/font rasterization: original animated
// backgrounds and overlapping translucent scene/caption shapes sit over a checkerboard.
internal static class EliminationCompositeSmoke
{
    private static readonly string[] themes=["gate","confetti","firewall"];
    public static unsafe void Run(string? capture)
    {
        if(!Glfw.Init())throw new InvalidOperationException("BRUTAL GLFW initialization failed.");
        try
        {
            Glfw.WindowHint(GlfwWindowHint.ContextVersionMajor,4);Glfw.WindowHint(GlfwWindowHint.ContextVersionMinor,5);Glfw.WindowHint(GlfwWindowHint.Visible,0);
            const int width=1360,height=570;
            using var window=Glfw.CreateWindow(new GlfwWindow.CreateInfo{Title="Elimination composite verification",Size=new(width,height)});window.MakeContextCurrent();
            if(!Glad.Initialize(name=>Glfw.PInvoke.GetProcAddress(name)))throw new InvalidOperationException("OpenGL initialization failed.");
            var context=ImGui.CreateContext();
            try
            {
                ImGui.GetIO().IniFilename=default;
                using var platform=new GlfwBackend(window,GlfwBackend.ClientApi.OpenGL);using var backend=new OpenGlBackend();
                var painter=new EliminationThemeRenderer();var composites=Enumerable.Range(0,12).Select(_=>new EliminationComposite()).ToArray();
                try
                {
                    for(int frame=0;frame<3;frame++)
                    {
                        Glfw.PollEvents();backend.NewFrame();platform.NewFrame();ImGui.NewFrame();
                        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(0,0));ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize,0);ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding,0);
                        Begin("Composite checkerboard",new(0,0),new(width,height));
                        var board=ImGui.GetWindowDrawList();
                        for(int y=0;y<height;y+=10)for(int x=0;x<width;x+=10)
                            board.AddRectFilled(new(x,y),new(x+10,y+10),(x/10+y/10)%2==0?new ImColor8(38,52,68):new ImColor8(120,140,160));
                        ImGui.End();
                        for(int i=0;i<12;i++)
                        {
                            int column=i%4;float2 origin=new(column*340,i/4*190),size=new(340,190);
                            double ease=EliminationPlayback.CssEase((column%2+1)/3d),opacity=column<2?ease:1-ease;
                            Begin("Composite cell "+i,origin,size);
                            painter.Draw(themes[i/4],false,.7,origin,size);
                            var draw=ImGui.GetWindowDrawList();
                            draw.AddRectFilled(origin+new float2(95,45),origin+new float2(215,150),new ImColor8(40,150,230,166));
                            draw.AddCircleFilled(origin+new float2(220,90),45,new ImColor8(255,140,60,191),96);
                            draw.AddRectFilled(origin+new float2(65,130),origin+new float2(275,166),new ImColor8(0,0,0,166));
                            for(int letter=0;letter<10;letter++)draw.AddRectFilled(origin+new float2(80+letter*18,141),origin+new float2(91+letter*18,155),new ImColor8(255,255,255,230));
                            composites[i].Capture(draw,origin,size,(float)opacity,ImGui.GetForegroundDrawList());ImGui.End();
                        }
                        ImGui.PopStyleVar(3);ImGui.Render();
                        foreach(var composite in composites)composite.Render(backend);
                        if(frame==2)
                        {
                            var rgba=new byte[340*190*4];
                            for(int i=0;i<composites.Length;i++)
                            {
                                fixed(byte* pixels=rgba)OpenGl.GetTextureImage(composites[i].Texture,0,OpenGl.GL_RGBA,OpenGl.GL_UNSIGNED_BYTE,rgba.Length,pixels);
                                if(rgba[(95*340+170)*4+3]<250)throw new InvalidOperationException($"Composite {i} has no opaque background in its rendered texture (alpha {rgba[(95*340+170)*4+3]}).");
                            }
                        }
                        var sizePixels=window.FramebufferSize;
                        OpenGl.Viewport(0,0,sizePixels.X,sizePixels.Y);OpenGl.ClearColor(new float4(0,0,0,1));OpenGl.Clear(GlClearBit.Color);backend.RenderDrawData(ImGui.GetDrawData());
                        if(frame==2)
                        {
                            if(sizePixels.X!=width||sizePixels.Y!=height)throw new InvalidOperationException("Composite comparison requires a 1:1 framebuffer.");
                            if(capture is not null)FrameCapture.SaveBitmap(capture,width,height);
                            if(OpenGl.GetError()!=OpenGl.GL_NO_ERROR)throw new InvalidOperationException("Composite rendering generated a GL error.");
                        }
                        window.SwapBuffers();
                    }
                }
                finally{foreach(var composite in composites)composite.Dispose();}
            }
            finally{ImGui.DestroyContext(context);}
        }
        finally{Glfw.Terminate();}
        Console.WriteLine("Elimination composite: 12 native group fade surfaces rendered over an opaque checkerboard.");
    }
    private static void Begin(string title,float2 origin,float2 size)
    {
        ImGui.SetNextWindowPos(origin);ImGui.SetNextWindowSize(size);
        ImGui.Begin(title,ImGuiWindowFlags.NoDecoration|ImGuiWindowFlags.NoMove|ImGuiWindowFlags.NoSavedSettings|ImGuiWindowFlags.NoBackground|ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoBringToFrontOnFocus);
    }
}
