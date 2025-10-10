using System;

namespace DopeGrid.Map;

public sealed class DualGridMap<T> : IDisposable
    where T : unmanaged, IEquatable<T>
{
    private ExpandableMap<T> _map = null!;
    public ExpandableMap<T>.ExpandFunc ExpandBoundFunc { get; set; } = MapBound.Union;

    public MapBound VertexBound => new(MinX: MinX, MinY: MinY, MaxX: MaxX + 1, MaxY: MaxY + 1);
    public MapBound WorldBound => new(MinX: MinX, MinY: MinY, MaxX: MaxX, MaxY: MaxY);

    public int Width => MaxX - MinX;
    public int Height => MaxY - MinY;
    public int MinX => _map.MinX + 1;
    public int MinY => _map.MinY + 1;
    public int MaxX => _map.MaxX - 1;
    public int MaxY => _map.MaxY - 1;

    public DualGridMap(int width, int height, int minX = 0, int minY = 0, T defaultValue = default)
        : this(new MapBound(MinX: minX, MinY: minY, MaxX: minX + width, MaxY: minY + height), defaultValue)
    {
    }

    public DualGridMap(MapBound bound, T defaultValue = default)
    {
        _map = new ExpandableMap<T>(GetExpandedBound(bound), defaultValue);
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
        _map.Expand(GetExpandedBound(newBound));
    }

    public (T BL, T BR, T TL, T TR) GetVertexNeighbors(int x, int y)
    {
        return (this[x - 1, y - 1], this[x, y - 1], this[x - 1, y], this[x, y]);
    }

    public Enumerator GetEnumerator() => new(this);

    private static MapBound GetExpandedBound(MapBound bound)
    {
        return new MapBound(
            MinX: bound.MinX - 1,
            MinY: bound.MinY - 1,
            MaxX: bound.MaxX + 1,
            MaxY: bound.MaxY + 1
        );
    }

    public void Dispose()
    {
        _map?.Dispose();
        _map = null!;
    }

    public ref struct Enumerator
    {
        private readonly DualGridMap<T> _map;
        private int _x;
        private int _y;

        internal Enumerator(DualGridMap<T> map)
        {
            _map = map;
            _x = -1;
            _y = 0;
        }

        public (T value, int x, int y) Current
        {
            get
            {
                var worldX = _map.MinX + _x;
                var worldY = _map.MinY + _y;
                return (_map[worldX, worldY], worldX, worldY);
            }
        }

        public bool MoveNext()
        {
            _x++;
            if (_x >= _map.Width)
            {
                _x = 0;
                _y++;
            }
            return _y < _map.Height;
        }
    }
}
