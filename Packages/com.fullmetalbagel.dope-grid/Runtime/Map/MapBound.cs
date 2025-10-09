namespace DopeGrid.Map;

public readonly record struct MapBound(int MinX, int MinY, int MaxX, int MaxY)
{
    public int Width => MaxX - MinX;
    public int Height => MaxY - MinY;
    public int MinX { get; } = MinX;
    public int MinY { get; } = MinY;
    public int MaxX { get; } = MaxX;
    public int MaxY { get; } = MaxY;

    public bool Contains(int x, int y)
    {
        return x >= MinX && x < MaxX && y >= MinY && y < MaxY;
    }

    public static MapBound Create(int minX, int minY, int width, int height)
    {
        return new MapBound(minX, minY, minX + width, minY + height);
    }

    public static MapBound Union(MapBound bound1, MapBound bound2)
    {
        var minX = bound1.MinX < bound2.MinX ? bound1.MinX : bound2.MinX;
        var minY = bound1.MinY < bound2.MinY ? bound1.MinY : bound2.MinY;
        var maxX = bound1.MaxX > bound2.MaxX ? bound1.MaxX : bound2.MaxX;
        var maxY = bound1.MaxY > bound2.MaxY ? bound1.MaxY : bound2.MaxY;
        return new MapBound(minX, minY, maxX, maxY);
    }

    public static MapBound Intersection(MapBound bound1, MapBound bound2)
    {
        var minX = bound1.MinX < bound2.MinX ? bound2.MinX : bound1.MinX;
        var minY = bound1.MinY < bound2.MinY ? bound2.MinY : bound1.MinY;
        var maxX = bound1.MaxX > bound2.MaxX ? bound2.MaxX : bound1.MaxX;
        var maxY = bound1.MaxY > bound2.MaxY ? bound2.MaxY : bound1.MaxY;
        return new MapBound(minX, minY, maxX, maxY);
    }

    public static MapBound ExpandToInclude(MapBound bound, int x, int y)
    {
        var minX = x < bound.MinX ? x : bound.MinX;
        var minY = y < bound.MinY ? y : bound.MinY;
        var maxX = x > bound.MaxX ? x : bound.MaxX;
        var maxY = y > bound.MaxY ? y : bound.MaxY;
        return new MapBound(minX, minY, maxX, maxY);
    }
}
