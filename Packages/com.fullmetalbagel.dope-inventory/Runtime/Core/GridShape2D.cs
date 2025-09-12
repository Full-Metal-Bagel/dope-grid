using System;
using Unity.Collections;
using Unity.Mathematics;

namespace DopeInventory;

public struct GridShape2D : IDisposable
{
    public int Width { get; }
    public int Height { get; }
    public NativeBitArray Bits;

    public int Size => Bits.Length;
    public int OccupiedSpaceCount => Bits.CountBits(0, Size);
    public int FreeSpaceCount => Size - OccupiedSpaceCount;
    public bool IsCreated => Bits.IsCreated;

    public GridShape2D(int width, int height, Allocator allocator)
    {
        Width = width;
        Height = height;
        Bits = new NativeBitArray(Width * Height, allocator);
    }

    public int GetIndex(int2 pos)
    {
        return pos.y * Width + pos.x;
    }

    public void SetCell(int2 pos, bool value)
    {
        Bits.Set(GetIndex(pos), value);
    }

    public bool GetCell(int2 pos)
    {
        return Bits.IsSet(GetIndex(pos));
    }

    public void Clear()
    {
        Bits.Clear();
    }

    public void Dispose()
    {
        if (Bits.IsCreated)
            Bits.Dispose();
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
}