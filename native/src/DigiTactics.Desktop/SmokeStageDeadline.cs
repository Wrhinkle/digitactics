namespace DigiTactics.Desktop;

/// <summary>Bounds each sequential smoke phase without charging earlier phases against later ones.</summary>
internal sealed class SmokeStageDeadline
{
    private int previousStage=-1,started,previousFrame=-1;
    public string? Check(int stage,string name,int frame,int budget,bool trace=true)
    {
        if(frame<previousFrame||stage<previousStage)throw new InvalidOperationException("Smoke progress moved backwards.");
        previousFrame=frame;
        if(stage!=previousStage)
        {
            if(trace)Console.WriteLine($"UI smoke stage {name} at frame {frame}; previous phase used {frame-started} frames, budget {budget}.");
            previousStage=stage;started=frame;
        }
        int used=frame-started;
        return used>budget?$"{name} exceeded its {budget}-frame budget ({used} phase frames, {frame} total)":null;
    }
    public static SmokeStageDeadline Verified()
    {
        static void Check(bool value){if(!value)throw new InvalidOperationException("Smoke stage deadline regression.");}
        var deadline=new SmokeStageDeadline();
        Check(deadline.Check(0,"Home",0,900,false) is null);
        Check(deadline.Check(0,"Home",900,900,false) is null);
        Check(deadline.Check(1,"Codex",901,600,false) is null);
        Check(deadline.Check(1,"Codex",1501,600,false) is null); // Healthy later work may exceed the former total cap.
        Check(deadline.Check(1,"Codex",1502,600,false) is not null); // A stalled phase still expires at its own boundary.
        Check(deadline.Check(2,"Gameplay",1503,1200,false) is null);
        Check(deadline.Check(2,"Different diagnostic",2704,1200,false) is not null); // Text changes cannot refresh the deadline.
        bool rejected=false;try{deadline.Check(1,"Codex",2705,600,false);}catch(InvalidOperationException){rejected=true;}Check(rejected);
        rejected=false;try{deadline.Check(2,"Gameplay",2703,1200,false);}catch(InvalidOperationException){rejected=true;}Check(rejected);
        Console.WriteLine("UI smoke deadline checks passed: independent phase budgets, exact expiry, no diagnostic reset and monotonic progress.");
        return new();
    }
}
