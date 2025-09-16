using System;
using Unity.Collections;
using Unity.Mathematics;

namespace DopeGrid;

public struct GridShape2D : IDisposable, IEquatable<GridShape2D>
{
    public int Width { get; }
    public int Height { get; }
    private NativeBitArray _bits;

    public int Size => _bits.Length;
    public int OccupiedSpaceCount => _bits.CountBits(0, Size);
    public int FreeSpaceCount => Size - OccupiedSpaceCount;
    public bool IsCreated => _bits.IsCreated;

    public GridShape2D(int width, int height, Allocator allocator)
    {
        Width = width;
        Height = height;
        _bits = new NativeBitArray(Width * Height, allocator);
    }

    public int GetIndex(int2 pos)
    {
        return pos.y * Width + pos.x;
    }

    public void SetCell(int2 pos, bool value)
    {
        _bits.Set(GetIndex(pos), value);
    }

    public bool GetCell(int2 pos)
    {
        return _bits.IsSet(GetIndex(pos));
    }

    public void Clear()
    {
        _bits.Clear();
    }

    public void Dispose()
    {
        if (_bits.IsCreated)
            _bits.Dispose();
    }

    public GridShape2D Clone(Allocator allocator)
    {
        var clone = new GridShape2D(Width, Height, allocator);
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
        {
            var pos = new int2(x, y);
            clone.SetCell(pos, GetCell(pos));
        }

        return clone;
    }

    public bool Equals(GridShape2D other) => Width == other.Width && Height == other.Height && _bits.SequenceEquals(other._bits);
    public override bool Equals(object? obj) => obj is GridShape2D other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(_bits, Width, Height);
    
    public static bool operator ==(GridShape2D left, GridShape2D right) => left.Equals(right);
    public static bool operator !=(GridShape2D left, GridShape2D right) => !left.Equals(right);
}
