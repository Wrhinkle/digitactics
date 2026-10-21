using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private static readonly Dictionary<string, string> TrainingTitles = new() { ["strength"] = "Power Swing", ["speed"] = "Beat Dash", ["wisdom"] = "Data Memory", ["stamina"] = "Endurance Run" };
    private TrainingGame? training;
    private string trainingName = "", trainingPet = "";
    private double trainingTime;
    private double? trainingScore;
    private long trainingTimestamp;
    private bool trainingSaved;
    internal TrainingGame? ActiveTraining => training;
    internal bool TrainingStarted => trainingTime >= 0;
    private void StartTraining(string name)
    {
        if (farmPet is null || !farm!.CanTrain(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), farmPet)) return;
        training = TrainingGame.Create(name); trainingName = name; trainingPet = farmPet;
        trainingTime = -1.5; trainingScore = null; trainingSaved = false;
        farmView?.StartTraining(trainingPet);
    }
    private void UpdateTraining(double delta, double wallDelta)
    {
        if (trainingScore is not null) return;
        double step = Math.Min(.05, delta);
        if (trainingTime < 0) { trainingTime += step; return; }
        trainingTime += step; training!.Update(step, trainingTime, wallDelta);
        foreach (string sound in training.DrainSounds()) audio.Play(sound);
        if (training.Score is { } result) FinishTraining(result);
    }
    private void FinishTraining(double score)
    {
        trainingScore = score; trainingTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        SaveTraining(); audio.Play(score >= .6 ? "win" : "loss");
    }
    private void SaveTraining()
    {
        farm!.Act(trainingTimestamp, FarmCareAction.Train, trainingPet, trainingName, score: trainingScore!.Value);
        trainingSaved = farm.Error is null;
        if (trainingSaved) farmView?.FinishTraining(trainingPet, trainingScore.Value);
    }
    private void TrainingInput(int value)
    {
        if (trainingTime < 0 || trainingScore is not null) return;
        training!.Input(value);
        foreach (string sound in training.DrainSounds()) audio.Play(sound);
    }
    private string TrainingStatName => trainingName switch { "strength" => "Strength", "speed" => "Speed", "wisdom" => "Wisdom", _ => "Stamina" };
    private string TrainingHelpText => trainingName switch
    {
        "strength" => "Press Space / Enter or click when the marker is in the gold zone. 5 swings.",
        "speed" => "Hit F / Up or J / Down as notes reach the ring. Click a lane works too.",
        "wisdom" => "Watch the pads light up, then repeat them. Click or press 1-4.",
        _ => "Tap Space / Enter or click to keep the meter inside the green band."
    };
    private float TrainingStageHeight => size.Y <= 800 ? 240 : 280;
    private static float TrainingTextHeight(string value,float width,float font=14)
    {
        ImGui.PushFont(NativeUiTheme.BodyFont,font);
        float height=ImGui.CalcTextSize(value,false,width).Y;
        ImGui.PopFont();return height;
    }
    private (string Text,bool Stat,bool Error)[] TrainingRewardChips()
    {
        var chips=new List<(string Text,bool Stat,bool Error)>();
        if(farm!.LastResult is not {} receipt)return [];
        if(receipt["ok"]?.GetValue<bool>()==true)
        {
            if(receipt["gains"] is System.Text.Json.Nodes.JsonObject gains)
                foreach(var gain in gains)chips.Add(((gain.Key switch{"str"=>"Strength","spd"=>"Speed","wis"=>"Wisdom",_=>"Stamina"})+" +"+gain.Value,true,false));
            if(receipt["reward"] is {} reward)
            {
                if(int.TryParse(reward["bits"]?.ToString(),out int bits)&&bits>0)chips.Add(($"+{bits} Bits",false,false));
                if(int.TryParse(reward["data"]?.ToString(),out int data)&&data>0)chips.Add(($"+{data} Digi-Data",false,false));
            }
        }
        else if(receipt["msg"]?.GetValue<string>() is {} message)chips.Add((message,false,true));
        return chips.ToArray();
    }
    private static List<(string Text,bool Stat,bool Error,float Width)>[] TrainingChipRows((string Text,bool Stat,bool Error)[] chips,float width)
    {
        var rows=new List<List<(string Text,bool Stat,bool Error,float Width)>>();
        float used=0;ImGui.PushFont(NativeUiTheme.BodyFont,14);
        foreach(var chip in chips)
        {
            // Leave a small rasterization allowance beyond the measured advance,
            // so a single-line badge does not wrap at a rounded glyph boundary.
            float extent=Math.Min(width,MathF.Ceiling(ImGui.CalcTextSize(chip.Text).X)+20);
            if(rows.Count==0||used+8+extent>width){rows.Add([]);used=0;}
            rows[^1].Add((chip.Text,chip.Stat,chip.Error,extent));used+=(used>0?8:0)+extent;
        }
        ImGui.PopFont();return rows.ToArray();
    }
    private float TrainingDialogHeight(float width)
    {
        float contentWidth=width-32;
        // Source .dg has a compact inline heading, help, separate time bar,
        // 280/240px stage and one score row, with 10px gaps and 14px padding.
        if(trainingScore is null)return 28+24+Math.Max(18,TrainingTextHeight(TrainingHelpText,contentWidth))+6+TrainingStageHeight+18+40;
        if(!trainingSaved)return 28+10+18+28+12+TrainingTextHeight(farm!.Error??"Training result could not be saved.",contentWidth)+10+38;
        var rows=TrainingChipRows(TrainingRewardChips(),contentWidth);
        float chipsHeight=rows.Sum(row=>Math.Max(24,row.Max(chip=>TrainingTextHeight(chip.Text,chip.Width-16)+8)))+Math.Max(0,rows.Length-1)*6;
        return 28+10+18+28+8+chipsHeight+8+Math.Max(18,TrainingTextHeight("Training used energy and hunger and trimmed a little weight.",contentWidth))+10+34;
    }
    private void TrainingPanel()
    {
        float contentWidth = ImGui.GetContentRegionAvail().X;
        if (trainingScore is { } score)
        {
            DrawTrainingResult(score,contentWidth);return;
        }
        var contentAt=ImGui.GetCursorScreenPos();var headerDraw=ImGui.GetWindowDrawList();
        ImGui.PushFont(NativeUiTheme.DisplayBoldFont,20);
        float titleWidth=ImGui.CalcTextSize(TrainingTitles[trainingName]).X;
        headerDraw.AddText(ImGui.GetFont(),20,contentAt,NativeUiTheme.GoldBright,TrainingTitles[trainingName]);ImGui.PopFont();
        headerDraw.AddText(NativeUiTheme.BodyFont,14,contentAt+new float2(titleWidth+10,4),NativeUiTheme.Muted,TrainingStatName+" training");
        ImGui.SetCursorScreenPos(contentAt+new float2(contentWidth-60,-4));
        // Cancelling does not consume care or award training rewards.
        if (TrainingActionButton("trainingQuit", "Quit", 60, 28) || ImGui.IsKeyPressed(ImGuiKey.Escape, false)) { training = null; return; }
        float helpHeight=Math.Max(18,TrainingTextHeight(TrainingHelpText,contentWidth));
        headerDraw.AddText(NativeUiTheme.BodyFont,14,contentAt+new float2(0,34),NativeUiTheme.Muted,TrainingHelpText,contentWidth);
        var game=training!;
        var timeAt=contentAt+new float2(0,34+helpHeight+10);
        headerDraw.AddRectFilled(timeAt,timeAt+new float2(contentWidth,6),new ImColor8(3,10,20),3);
        headerDraw.AddRectFilledMultiColor(timeAt,timeAt+new float2(contentWidth*(float)game.TimeLeft,6),
            new ImColor8(143,240,255),new ImColor8(47,155,255),new ImColor8(47,155,255),new ImColor8(143,240,255));
        var origin=timeAt+new float2(0,16);float width=contentWidth,height=TrainingStageHeight;
        ImGui.SetCursorScreenPos(origin);
        var draw = ImGui.GetWindowDrawList();
        void Rect(float x, float y, float w, float h, ImColor8 color, float rounding = 0) => draw.AddRectFilled(new(origin.X + x, origin.Y + y), new(origin.X + x + w, origin.Y + y + h), color, rounding);
        void Label(float x, float y, string label, ImColor8 color, float fontSize = 18) => draw.AddText(ImGui.GetFont(), fontSize, new(origin.X + x, origin.Y + y), color, label);
        var white = new ImColor8(235, 242, 250); var green = new ImColor8(72, 159, 99); var gold = new ImColor8(255, 207, 74);
        Rect(0, 0, width, height, new(12, 18, 32), 12);
        draw.AddRect(new(origin.X,origin.Y),new(origin.X+width,origin.Y+height),new ImColor8(58,71,98),12);
        string feedback = trainingTime < 0 ? trainingTime < -1 ? "Ready" : "Go!" : game.Feedback;
        bool compactMemory=game is MemoryTraining&&height<=240;
        float feedbackFont=compactMemory?22:28,feedbackY=compactMemory?4:18;
        var feedbackSize = ImGui.CalcTextSize(feedback) * (feedbackFont / ImGui.GetFontSize());
        Label((width - feedbackSize.X) / 2+1, feedbackY+1, feedback, new(0,0,0,190), feedbackFont);
        Label((width - feedbackSize.X) / 2, feedbackY, feedback, white, feedbackFont);
        if (game is SwingTraining swing)
        {
            float trackX = width * .08f, trackWidth = width * .84f;
            Rect(width / 2 - 30, 64, 60, 70, new(133, 87, 54), 8);
            for (int line = 0; line < 5; line++) Rect(width / 2 - 28, 74 + line * 13, 56, 3, new(90, 58, 36));
            float trackY=height<=240?130:150;
            Rect(trackX, trackY, trackWidth, 34, new(11, 18, 32), 17);
            draw.AddRect(new(origin.X+trackX,origin.Y+trackY),new(origin.X+trackX+trackWidth,origin.Y+trackY+34),new ImColor8(58,71,98),17);
            float zoneX=origin.X+trackX+trackWidth*(float)(swing.Center-swing.Zone/2),zoneWidth=trackWidth*(float)swing.Zone;
            draw.AddRectFilledMultiColor(new(zoneX,origin.Y+trackY),new(zoneX+zoneWidth/2,origin.Y+trackY+34),new ImColor8(255,179,71),new ImColor8(255,231,168),new ImColor8(255,231,168),new ImColor8(255,179,71));
            draw.AddRectFilledMultiColor(new(zoneX+zoneWidth/2,origin.Y+trackY),new(zoneX+zoneWidth,origin.Y+trackY+34),new ImColor8(255,231,168),new ImColor8(255,179,71),new ImColor8(255,179,71),new ImColor8(255,231,168));
            Rect(trackX + trackWidth * (float)swing.Position - 3, trackY, 6, 34, white,3);
        }
        else if (game is BeatTraining beats)
        {
            for (int lane = 0; lane < 2; lane++)
            {
                float y = 94 + lane * 110;
                Rect(90, y, width - 110, 2, new(48, 65, 86));
                draw.AddCircle(new(origin.X + 90, origin.Y + y), 27, gold);
                Label(12, y - 47, lane == 0 ? "F / UP" : "J / DOWN", white);
                foreach (var note in beats.Notes.Where(n => n.Lane == lane && !n.Done && n.At - beats.Time <= 1.6))
                {
                    float x = 90 + (width - 110) * (float)((note.At - beats.Time) / 1.6);
                    draw.AddCircleFilled(new(origin.X + x, origin.Y + y), 20, lane == 0 ? gold : new ImColor8(79, 179, 255));
                }
            }
        }
        else if (game is MemoryTraining memory)
        {
            ImColor8[] colors = [gold, new(255, 111, 154), new(79, 179, 255), new(159, 224, 122)];
            for (int pad = 0; pad < 4; pad++)
            {
                float x = width / 2 - 102 + pad % 2 * 108, y = height/2-204*.42f + pad / 2 * 108;
                Rect(x, y, 96, 96, memory.LitPad == pad ? colors[pad] : new(40, 53, 72), 10);
                draw.AddRect(new(origin.X + x, origin.Y + y), new(origin.X + x + 96, origin.Y + y + 96), colors[pad], 10);
                Label(x + 37, y + 28, (pad + 1).ToString(), memory.LitPad == pad ? new(15, 25, 39) : colors[pad], 38);
            }
        }
        else if (game is EnduranceTraining endurance)
        {
            float meterX = width / 2 - 28;
            float meterHeight=height-70;
            Rect(meterX, 40, 56, meterHeight, new(48, 65, 86), 6);
            Rect(meterX, 40 + meterHeight * .18f, 56, meterHeight * .44f, green);
            Rect(meterX + 10, 40 + meterHeight * (1 - (float)endurance.Meter), 36, meterHeight * (float)endurance.Meter, gold, 3);
            Label(24, 120, "KEEP IT", white, 20); Label(24, 150, "IN THE GREEN", white, 20);
            Label(width / 2 + 70, 134, "SPACE / ENTER", gold, 20);
        }
        ImGui.InvisibleButton("trainingStage"u8, new(width, height));
        Buttons["trainingStage"] = new(origin.X + width / 2, origin.Y + height / 2);
        for (int pad = 0; pad < 4; pad++) Buttons["trainingPad/" + pad] = new(origin.X + width / 2 - 54 + pad % 2 * 108, origin.Y + height/2-204*.42f + 48 + pad / 2 * 108);
        for (int lane = 0; lane < 2; lane++) Buttons["trainingLane/" + lane] = new(origin.X + width / 2, origin.Y + 94 + lane * 110);
        if (ImGui.IsItemActivated())
        {
            var mouse = ImGui.GetMousePos(); float x = mouse.X - origin.X, y = mouse.Y - origin.Y;
            if (game is MemoryTraining)
            {
                float gridX = x - (width / 2 - 102), gridY = y - (height/2-204*.42f);
                if (gridX is >= 0 and < 204 && gridY is >= 0 and < 204 && gridX % 108 < 96 && gridY % 108 < 96)
                    TrainingInput((int)(gridY / 108) * 2 + (int)(gridX / 108));
            }
            else TrainingInput(game is BeatTraining ? y < 149 ? 0 : 1 : 0);
        }
        foreach (var (key, value) in new (ImGuiKey, string)[] { (ImGuiKey.Space, " "), (ImGuiKey.Enter, "Enter"), (ImGuiKey.F, "f"), (ImGuiKey.UpArrow, "ArrowUp"), (ImGuiKey.W, "w"), (ImGuiKey.J, "j"), (ImGuiKey.DownArrow, "ArrowDown"), (ImGuiKey.S, "s"), (ImGuiKey._1, "1"), (ImGuiKey._2, "2"), (ImGuiKey._3, "3"), (ImGuiKey._4, "4") })
            if (ImGui.IsKeyPressed(key, false) && game.Key(value) is { } input) TrainingInput(input);
        var scoreAt=origin+new float2(0,height+10);
        ImGui.PushFont(NativeUiTheme.BodyFont,16);
        draw.AddText(ImGui.GetFont(),16,scoreAt,NativeUiTheme.GoldBright,game.Left);
        draw.AddText(ImGui.GetFont(),16,new(scoreAt.X+contentWidth-ImGui.CalcTextSize(game.Right).X,scoreAt.Y),NativeUiTheme.GoldBright,game.Right);
        ImGui.PopFont();ImGui.SetCursorScreenPos(scoreAt);ImGui.Dummy(new(contentWidth,18));
    }
    private bool TrainingActionButton(string key,string label,float width,float height,bool primary=false)
    {
        ImGui.PushFont(NativeUiTheme.BodyFont,16);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding,0);ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,1);
        ImGui.PushStyleColor(ImGuiCol.Button,new ImColor8(27,31,33));ImGui.PushStyleColor(ImGuiCol.ButtonHovered,new ImColor8(44,46,40));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive,new ImColor8(55,51,37));ImGui.PushStyleColor(ImGuiCol.Border,NativeUiTheme.GoldBorder);
        ImGui.PushStyleColor(ImGuiCol.Text,primary?new ImColor8(0,0,0,0):NativeUiTheme.Gold);
        bool clicked=Button(key,label,width,height);
        if(primary)
        {
            byte alpha=(byte)Math.Clamp(Math.Round(255*ImGui.GetStyle().Alpha),0,255);
            var at=ImGui.GetItemRectMin();var end=ImGui.GetItemRectMax();var draw=ImGui.GetWindowDrawList();
            var top=new ImColor8(255,218,122,alpha);var bottom=new ImColor8(255,143,56,alpha);
            draw.AddRectFilledMultiColor(at,end,top,top,bottom,bottom);draw.AddRect(at,end,new ImColor8(255,224,160,alpha));
            draw.AddText(at+(end-at-ImGui.CalcTextSize(label))*.5f,new ImColor8(28,20,9,alpha),label);
            if(ImGui.IsItemFocused())draw.AddRect(at+new float2(3,3),end-new float2(3,3),new ImColor8(255,249,219,alpha));
        }
        ImGui.PopStyleColor(5);ImGui.PopStyleVar(2);ImGui.PopFont();return clicked;
    }
    private void DrawTrainingResult(double score,float width)
    {
        // Original .dg-result: title, rating with a muted inline score, separate
        // gain/reward badges, then the two centered actions. Height follows wraps.
        var contentAt=ImGui.GetCursorScreenPos();var draw=ImGui.GetWindowDrawList();
        void Center(string value,float font,ImColor8 ink,float y)
        {
            ImGui.PushFont(NativeUiTheme.BodyFont,font);
            float textWidth=ImGui.CalcTextSize(value).X;
            draw.AddText(ImGui.GetFont(),font,new(contentAt.X+Math.Max(0,(width-textWidth)/2),contentAt.Y+y),ink,value,width);ImGui.PopFont();
        }
        string subtitle=TrainingTitles[trainingName]+" \u00b7 "+CosmeticCollection.Item("legend",trainingPet)!.Name;
        Center(subtitle,14,NativeUiTheme.Muted,10);
        string rating=score>=.9?"Excellent!":score>=.7?"Great!":score>=.4?"Good":"Keep practicing";
        string scoreText=$"score {Math.Floor(score*100+.5)}";
        ImGui.PushFont(NativeUiTheme.DisplayBoldFont,24);float ratingWidth=ImGui.CalcTextSize(rating).X;ImGui.PopFont();
        ImGui.PushFont(NativeUiTheme.BodyFont,14);float scoreWidth=ImGui.CalcTextSize(scoreText).X;ImGui.PopFont();
        float ratingX=contentAt.X+Math.Max(0,(width-ratingWidth-scoreWidth-7)/2);
        draw.AddText(NativeUiTheme.DisplayBoldFont,24,new(ratingX,contentAt.Y+28),NativeUiTheme.GoldBright,rating);
        draw.AddText(NativeUiTheme.BodyFont,14,new(ratingX+ratingWidth+7,contentAt.Y+36),NativeUiTheme.Muted,scoreText);
        if(!trainingSaved)
        {
            string error=farm!.Error??"Training result could not be saved.";
            float errorHeight=TrainingTextHeight(error,width);
            draw.AddText(NativeUiTheme.BodyFont,14,contentAt+new float2(0,68),new ImColor8(255,153,128),error,width);
            ImGui.SetCursorScreenPos(contentAt+new float2(Math.Max(0,(width-240)/2),78+errorHeight));
            if(TrainingActionButton("retryTraining","Retry saving result",240,38))SaveTraining();
            return;
        }
        ImGui.SetCursorScreenPos(contentAt+new float2(width-60,-4));
        if(TrainingActionButton("trainingDoneTop","Done",60,28)){training=null;return;}
        float y=64;
        var chipRows=TrainingChipRows(TrainingRewardChips(),width);
        foreach(var row in chipRows)
        {
            float rowWidth=row.Sum(chip=>chip.Width)+Math.Max(0,row.Count-1)*8;
            float rowHeight=Math.Max(24,row.Max(chip=>TrainingTextHeight(chip.Text,chip.Width-16)+8));
            float x=contentAt.X+Math.Max(0,(width-rowWidth)/2);
            foreach(var chip in row)
            {
                var at=new float2(x,contentAt.Y+y);
                var ink=chip.Error?new ImColor8(255,153,128):chip.Stat?new ImColor8(159,224,122):new ImColor8(222,230,241);
                var border=chip.Error?new ImColor8(124,68,55):chip.Stat?new ImColor8(79,111,56):new ImColor8(48,65,89);
                draw.AddRectFilled(at,at+new float2(chip.Width,rowHeight),new ImColor8(9,17,29),10);draw.AddRect(at,at+new float2(chip.Width,rowHeight),border,10);
                draw.AddText(NativeUiTheme.BodyFont,14,at+new float2(8,4),ink,chip.Text,chip.Width-16);
                x+=chip.Width+8;
            }
            y+=rowHeight+6;
        }
        y+=chipRows.Length==0?8:2;
        const string note="Training used energy and hunger and trimmed a little weight.";
        Center(note,14,NativeUiTheme.Muted,y);
        y+=Math.Max(18,TrainingTextHeight(note,width))+10;
        ImGui.SetCursorScreenPos(contentAt+new float2(Math.Max(0,(width-136)/2),y));
        bool canTrain=farm!.CanTrain(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),trainingPet);
        ImGui.BeginDisabled(!canTrain);
        if(TrainingActionButton("trainingAgain","Again",66,34,true))StartTraining(trainingName);
        ImGui.EndDisabled();
        ImGui.SameLine(0,8);if(TrainingActionButton("trainingDone","Done",62,34))training=null;
    }

}
