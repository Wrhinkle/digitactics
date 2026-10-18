namespace DigiTactics.Assets;

public sealed class BoardCityBuilding
{
    private readonly BoardCityBuildingData data;
    private double scale,velocity;
    public bool Visible=>Math.Max(.001,scale)>.01;
    public BoardCityBuilding(BoardCityBuildingData data,int initialStage=1)
    {
        this.data=data;scale=initialStage>=data.Stage?1:0;
    }
    public void Advance(double delta,int stage)
    {
        double target=stage>=data.Stage?1:0;
        velocity+=((target-scale)*60-velocity*9)*delta;
        scale=Math.Max(0,scale+velocity*delta);
    }
    public Matrix4D Matrix
    {
        get {double s=Math.Max(.001,scale);return new CompanionTransform(new(data.X,0,data.Z),new(0,data.Ry,0),new(s,s,s)).Matrix;}
    }
}

public sealed record BoardCityBuildingData(double X,double Z,double Ry,int Stage);
