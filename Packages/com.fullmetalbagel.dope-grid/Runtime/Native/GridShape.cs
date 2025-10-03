using System;
using System.Diagnostics.CodeAnalysis;
using Unity.Collections;
using Unity.Jobs;

namespace DopeGrid.Native;

public struct GridShape : IReadOnlyGridShape, IEquatable<GridShape>, INativeDisposable
{
    public int Width { get; }
    public int Height { get; }
    public readonly int Size => Width * Height;

    public readonly ReadOnlySpanBitArray ReadOnlyBits => new(_bits.AsReadOnlySpan(), Size);
    internal SpanBitArray Bits => new(_bits.AsSpan(), Size);
    private NativeArray<byte> _bits;

    public readonly int OccupiedSpaceCount => ((ReadOnly)this).OccupiedSpaceCount;
    public readonly int FreeSpaceCount => ((ReadOnly)this).FreeSpaceCount;
    public bool IsCreated => _bits.IsCreated;

    public GridShape(int width, int height, Allocator allocator)
    {
        Width = width;
        Height = height;
        var bitLength = width * height;
        _bits = new NativeArray<byte>(SpanBitArrayUtility.ByteCount(bitLength), allocator, NativeArrayOptions.ClearMemory);
    }

    public void SetCell(GridPosition pos, bool value) => SetCell(pos.X, pos.Y, value);
    public void SetCell(int x, int y, bool value) => Bits.Set(this.GetIndex(x, y), value);

    public bool this[int x, int y]
    {
        get => this.GetCellValue(x, y);
        set => SetCell(x, y, value);
    }

    public bool this[GridPosition pos]
    {
        get => this.GetCellValue(pos);
        set => SetCell(pos, value);
    }

    public bool this[int index]
    {
        readonly get => ReadOnlyBits.Get(index);
        set => Bits.Set(index, value);
    }

    public GridShape FillAll(bool value)
    {
        Bits.SetAll(value);
        return this;
    }

    public GridShape FillRect(int x, int y, int width, int height, bool value = true)
    {
        for (int dy = 0; dy < height; dy++)
        {
            for (int dx = 0; dx < width; dx++)
            {
                var px = x + dx;
                var py = y + dy;
                if (px >= 0 && px < Width && py >= 0 && py < Height)
                {
                    SetCell(px, py, value);
                }
            }
        }
        return this;
    }

    public GridShape FillRect(GridPosition pos, int width, int height, bool value = true)
    {
        return FillRect(pos.X, pos.Y, width, height, value);
    }

    public void Clear()
    {
        _bits.AsSpan().Clear();
    }

    public void Dispose()
    {
        _bits.Dispose();
    }

    public JobHandle Dispose(JobHandle inputDeps)
    {
        return _bits.Dispose(inputDeps);
    }

    public readonly GridShape Clone(Allocator allocator)
    {
        return AsReadOnly().Clone(allocator);
    }

    public void CopyTo(GridShape other)
    {
        AsReadOnly().CopyTo(other);
    }

    public static implicit operator ReadOnly(GridShape shape) => shape.AsReadOnly();
    public readonly ReadOnly AsReadOnly() => new(Width, Height, ReadOnlyBits);

    public override int GetHashCode() => throw new NotSupportedException("GetHashCode() on GridShape and GridShape.ReadOnly is not supported.");
    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
    public override bool Equals(object? obj) => throw new NotSupportedException("Equals(object) on GridShape and GridShape.ReadOnly is not supported.");
    public readonly bool Equals(GridShape other) => AsReadOnly().Equals(other.AsReadOnly());
    public static bool operator ==(GridShape left, GridShape right) => left.Equals(right);
    public static bool operator !=(GridShape left, GridShape right) => !left.Equals(right);

    [SuppressMessage("Design", "CA1716:Identifiers should not match keywords")]
    public readonly ref struct ReadOnly
    {
        public int Width { get; }
        public int Height { get; }
        public ReadOnlySpanBitArray Bits { get; }

        public int OccupiedSpaceCount => Bits.CountBits(0, Bits.BitLength);
        public int FreeSpaceCount => this.Size() - OccupiedSpaceCount;

        internal ReadOnly(int width, int height, ReadOnlySpanBitArray bits)
        {
            Width = width;
            Height = height;
            Bits = bits;
        }

        public bool this[int x, int y] => this.GetCellValue(x, y);
        public bool this[GridPosition pos] => this.GetCellValue(pos);
        public bool this[int index] => Bits.Get(index);

        public GridShape Clone(Allocator allocator)
        {
            var clone = new GridShape(Width, Height, allocator);
            CopyTo(clone);
            return clone;
        }

        public void CopyTo(GridShape other)
        {
            if (Width != other.Width || Height != other.Height)
                throw new ArgumentException($"Cannot copy to GridShape2D with different dimensions. Source: {Width}x{Height}, Target: {other.Width}x{other.Height}");

            Bits.CopyTo(other.Bits);
        }

        public override int GetHashCode() => throw new NotSupportedException("GetHashCode() on GridShape and GridShape.ReadOnly is not supported.");
        [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
        public override bool Equals(object? obj) => throw new NotSupportedException("Equals(object) on GridShape and GridShape.ReadOnly is not supported.");
        public bool Equals(ReadOnly other) => Width == other.Width && Height == other.Height && Bits.SequenceEqual(other.Bits);
        public static bool operator ==(ReadOnly left, ReadOnly right) => left.Equals(right);
        public static bool operator !=(ReadOnly left, ReadOnly right) => !left.Equals(right);
    }
}
