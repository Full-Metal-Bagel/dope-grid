using System;

namespace DopeGrid.Map;

public sealed class ExpandableMap<T> : IReadOnlyGridShape<T>, IDisposable
    where T : unmanaged, IEquatable<T>
{
    private ValueGridShape<T> _world;
    public delegate MapBound ExpandFunc(MapBound currentBound, MapBound newBound);

    public ExpandFunc ExpandBoundFunc { get; set; } = MapBound.Union;

    public int MinX { get; private set; }
    public int MinY { get; private set; }
    public int MaxX => MinX + Width;
    public int MaxY => MinY + Height;
    public int Width => _world.Width;
    public int Height => _world.Height;
    public MapBound Bound => new(MinX: MinX, MinY: MinY, MaxX: MinX + Width, MaxY: MinY + Height);

    private readonly int _threadId;

    public ExpandableMap(int width, int height, T defaultValue = default)
        : this(new MapBound(MinX: 0, MinY: 0, MaxX: width, MaxY: height), defaultValue)
    {
    }

    public ExpandableMap(MapBound bound, T defaultValue = default)
    {
        _threadId = Environment.CurrentManagedThreadId;
        _world = new ValueGridShape<T>(bound.Width, bound.Height, defaultValue);
        MinX = bound.MinX;
        MinY = bound.MinY;
    }

    public bool IsOccupied(int x, int y) => _world.IsOccupied(x - MinX, y - MinY);
    public bool Contains(int x, int y) => _world.Contains(x - MinX, y - MinY);

    public T this[int x, int y]
    {
        get => _world[x - MinX, y - MinY];
        set => _world[x - MinX, y - MinY] = value;
    }

    public void Expand(MapBound newBound)
    {
        var currentBound = Bound;
        newBound = ExpandBoundFunc(currentBound, newBound);
        newBound = MapBound.Union(currentBound, newBound);
        if (newBound == currentBound) return;

        if (_threadId != Environment.CurrentManagedThreadId)
            throw new NotSupportedException("multiple threads are not supported");

        var offsetX = currentBound.MinX - newBound.MinX;
        var offsetY = currentBound.MinY - newBound.MinY;
        var world = new ValueGridShape<T>(newBound.Width, newBound.Height, _world.EmptyValue);

        // Copy existing data to the new world at the offset position
        for (int y = 0; y < _world.Height; y++)
        for (int x = 0; x < _world.Width; x++)
        {
            world[offsetX + x, offsetY + y] = _world[x, y];
        }

        _world.Dispose();
        _world = world;
        MinX = newBound.MinX;
        MinY = newBound.MinY;
    }

    public void Dispose()
    {
        _world.Dispose();
    }
}
