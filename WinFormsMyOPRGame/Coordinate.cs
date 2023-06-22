using System;

public struct Coordinate
{
    public int X;
    public int Y;


    public Coordinate(int x = -1, int y = -1)
    {
        X = x;
        Y = y;
    }

    public int Distance(Coordinate pointB)
    {
        return (int)Math.Sqrt((X-pointB.X)*(X-pointB.X) + (Y-pointB.Y)*(Y-pointB.Y));
    }
}
