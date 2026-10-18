namespace DigiTactics.Assets;

public static class BoardDrawOrder
{
    public static double Depth(BoardObject obj,Matrix4D world,Matrix4D camera)
    {
        var c=obj.SortCenter;double x=c?[0]??0,y=c?[1]??0,z=c?[2]??0;
        var m=world*camera;
        // Three sorts on clip-space Z before the perspective divide.
        return x*m.M13+y*m.M23+z*m.M33+m.M43;
    }
    public static int Compare(BoardObject a,double depthA,BoardObject b,double depthB)
    {
        int order=a.RenderOrder.CompareTo(b.RenderOrder);
        if(order!=0)return order;
        int depth=depthB.CompareTo(depthA);
        return depth!=0?depth:a.SortId.CompareTo(b.SortId);
    }
}
