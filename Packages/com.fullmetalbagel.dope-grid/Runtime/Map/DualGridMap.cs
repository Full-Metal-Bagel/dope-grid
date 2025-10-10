using System;

namespace DopeGrid.Map;

public sealed class DualGridMap<T> : IDisposable
    where T : unmanaged, IEquatable<T>
{
    private ExpandableMap<T> _map = null!;
    public ExpandableMap<T>.ExpandFunc ExpandBoundFunc { get; set; } = MapBound.Union;

    public MapBound Bound => _map.Bound;
    public int Width => _map.Width;
    public int Height => _map.Height;
    public int MinX => _map.MinX;
    public int MinY => _map.MinY;
    public int MaxX => _map.MaxX;
    public int MaxY => _map.MaxY;

    public DualGridMap(int width, int height, int minX = 0, int minY = 0, T defaultValue = default)
        :this(new MapBound(MinX: minX - 1, MinY: minY - 1, MaxX: minX + width + 1, MaxY: minY + height + 1), defaultValue)
    {
    }

    public DualGridMap(MapBound bound, T defaultValue = default)
    {
        _map = new ExpandableMap<T>(bound, defaultValue);
    }

    public bool IsOccupied(int x, int y) => _map.IsOccupied(x, y);
    public bool Contains(int x, int y) => _map.Contains(x, y);

    public T this[int x, int y]
    {
        get => _map[x, y];
        set => _map[x, y] = value;
    }

    public void Expand(MapBound newBound)
    {
        _map.Expand(newBound);
    }

    public (T BL, T BR, T TL, T TR) GetVertexNeighbors(int x, int y)
    {
        return (this[x - 1, y - 1], this[x, y - 1], this[x - 1, y], this[x, y]);
    }

    public ExpandableMap<T>.Enumerator GetEnumerator() => _map.GetEnumerator();

    public void Dispose()
    {
        _map?.Dispose();
        _map = null!;
    }
}
