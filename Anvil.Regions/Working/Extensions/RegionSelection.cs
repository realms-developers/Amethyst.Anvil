namespace Anvil.Regions.Working.Extensions;

public sealed class RegionSelection
{
    public bool PointsSet => Point1Set && Point2Set;

    public bool Point1Set { get; private set; }
    public int X { get; private set; }
    public int Y { get; private set; }

    public bool Point2Set { get; private set; }
    public int X2 { get; private set; }
    public int Y2 { get; private set; }

    public void SetPoint1(int x, int y)
    {
        X = x;
        Y = y;
        Point1Set = true;
    }

    public void SetPoint2(int x, int y)
    {
        X2 = x;
        Y2 = y;
        Point2Set = true;
    }
}