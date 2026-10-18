using System.Text.Json;
using static System.Math;

namespace DigiTactics.Assets;

public static partial class DanceChoreography
{
    private sealed record SecretPhrase(string[] Keys,string Route,string Accent,double Hold);
    private sealed record SecretLibrary(Dictionary<string,DancePose> Gestures,SecretPhrase[] Phrases);
    private static readonly Lazy<SecretLibrary> SecretDances=new(LoadSecretDances);
    private static SecretLibrary LoadSecretDances()
    {
        using var stream=typeof(DanceChoreography).Assembly.GetManifestResourceStream("DigiTactics.MysteryDances.json")
            ?? throw new InvalidDataException("Missing authored mystery dance phrases.");
        using var document=JsonDocument.Parse(stream);var root=document.RootElement;
        if(root.GetProperty("version").GetInt32()!=1||root.GetProperty("beats").GetInt32()!=8)throw new InvalidDataException("Unsupported mystery dance data.");
        var gestures=new Dictionary<string,DancePose>();
        foreach(var entry in root.GetProperty("gestures").EnumerateObject())
        {
            var pose=new DancePose([],[],[]);
            foreach(var p in entry.Value.EnumerateObject())
                if(p.Value.ValueKind==JsonValueKind.Array)pose.Vectors[p.Name]=V(p.Value[0].GetDouble(),p.Value[1].GetDouble(),p.Value[2].GetDouble());
                else pose.Scalars[p.Name]=p.Value.GetSingle();
            gestures.Add(entry.Name,pose);
        }
        var phrases=root.GetProperty("phrases").EnumerateArray().Select(p=>new SecretPhrase(
            p.GetProperty("keys").EnumerateArray().Select(k=>k.GetString()!).ToArray(),p.GetProperty("route").GetString()!,p.GetProperty("accent").GetString()!,p.GetProperty("hold").GetDouble())).ToArray();
        if(phrases.Length!=40||phrases.Any(p=>p.Keys.Length!=8||p.Keys.Any(k=>!gestures.ContainsKey(k))||p.Hold<0||p.Hold>=1))throw new InvalidDataException("Invalid mystery dance phrase.");
        return new(gestures,phrases);
    }
    private static double SecretEase(double t)=>t*t*t*(t*(t*6-15)+10);
    private static DancePose MysteryPhrase(double b,int index)
    {
        var library=SecretDances.Value;var phrase=library.Phrases[index];double cycle=((b%8)+8)%8;int key=(int)Floor(cycle);
        double t=SecretEase(Clamp((cycle-key-phrase.Hold)/(1-phrase.Hold),0,1));
        var a=library.Gestures[phrase.Keys[key]];var z=library.Gestures[phrase.Keys[(key+1)%8]];var p=new PoseBuilder();
        foreach(var entry in a.Vectors)p.Set(entry.Key,Mix(entry.Value,z.Vectors[entry.Key],t));
        foreach(var entry in a.Scalars)p.Scalar(entry.Key,entry.Value+(z.Scalars[entry.Key]-entry.Value)*t);
        double step=Sin(PI*b),lift=Pow(Sin(PI*b),2),turn=Sin(PI*b/4),circle=PI*b/4;
        p.Set("lL",.12,-1,.18*step);p.Set("lR",-.12,-1,-.18*step);
        p.Set("kL",.05,-1,-.12*Max(0,step));p.Set("kR",-.05,-1,-.12*Max(0,-step));
        double x=0,zp=0,yaw=0;
        switch(phrase.Route)
        {
            case "side":x=.055*turn;yaw=.16*turn;break;
            case "sway":x=.035*turn;yaw=.12*Sin(circle*2);break;
            case "step":x=.045*Sin(PI*b/2);zp=.018*turn;break;
            case "zigzag":x=.04*Sin(PI*b/2);yaw=.22*turn;break;
            case "forward":zp=.045*(1-Cos(circle));yaw=.1*turn;break;
            case "arc":x=.05*turn;zp=.025*(1-Cos(circle));yaw=.3*turn;break;
            case "orbit":x=.035*turn;zp=.03*(1-Cos(circle));yaw=.5*turn;break;
        }
        double bob=phrase.Accent switch{"spark"=>Pow(lift,3)*.035,"pulse"=>Pow(lift,2)*.028,"impact"=>lift*.045,"rise"=>.035*(1-Cos(circle)),_=>.018*lift};
        p.Root("x",x);p.Root("y",bob);p.Root("z",zp);p.Root("ry",yaw);p.Scalar("preserveScale",1);p.Scalar("familyAdapt",1);return p.Pose;
    }
}
