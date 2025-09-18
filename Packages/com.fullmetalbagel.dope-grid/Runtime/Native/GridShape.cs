using System;
using System.Diagnostics.CodeAnalysis;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace DopeGrid.Native;

public struct GridShape : IEquatable<GridShape>, INativeDisposable
{
    public int Width { get; }
    public int Height { get; }
    public readonly int Size => Width * Height;

    public readonly ReadOnlySpanBitArray ReadOnlyBits => new(_bits.AsReadOnlySpan(), Size);
    internal SpanBitArray Bits => new(_bits.AsSpan(), Size);
    private NativeArray<byte> _bits;

    public readonly int OccupiedSpaceCount => ReadOnlyBits.CountBits(0, Size);
    public readonly int FreeSpaceCount => Size - OccupiedSpaceCount;
    public readonly bool IsCreated => _bits.IsCreated;
    public readonly bool IsEmpty => Width == 0 || Height == 0;

    public GridShape(int width, int height, Allocator allocator)
    {
        Width = width;
        Height = height;
        var bitLength = width * height;
        _bits = new NativeArray<byte>(SpanBitArrayUtility.ByteCount(bitLength), allocator);
    }

    public readonly int GetIndex(int2 pos) => GetIndex(pos.x, pos.y);
    public readonly int GetIndex(int x, int y) => y * Width + x;

    public readonly bool GetCell(int2 pos) => GetCell(pos.x, pos.y);
    public readonly bool GetCell(int x, int y) => ReadOnlyBits.Get(GetIndex(x, y));

    public void SetCell(int2 pos, bool value) => SetCell(pos.x, pos.y, value);
    public void SetCell(int x, int y, bool value) => Bits.Set(GetIndex(x, y), value);

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
        return ToReadOnly().Clone(allocator);
    }

    public void CopyTo(GridShape other)
    {
        ToReadOnly().CopyTo(other);
    }

    public static implicit operator ReadOnly(GridShape shape) => shape.ToReadOnly();
    public readonly ReadOnly ToReadOnly() => new(Width, Height, Bits);

    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
    public override bool Equals(object? obj) => throw new NotSupportedException();
    public bool Equals(GridShape other) => Width == other.Width && Height == other.Height && _bits.SequenceEquals(other._bits);
    public override int GetHashCode() => HashCode.Combine(_bits.IsCreated ? _bits.CalculateHashCode() : 0, Width, Height);

    public static bool operator ==(GridShape left, GridShape right) => left.Equals(right);
    public static bool operator !=(GridShape left, GridShape right) => !left.Equals(right);

    [SuppressMessage("Design", "CA1716:Identifiers should not match keywords")]
    public readonly ref struct ReadOnly
    {
        public int Width { get; }
        public int Height { get; }
        public int2 Bound => new(Width, Height);
        public ReadOnlySpanBitArray Bits { get; }

        public int Size => Width * Height;
        public int OccupiedSpaceCount => Bits.CountBits(0, Size);
        public int FreeSpaceCount => Size - OccupiedSpaceCount;

        internal ReadOnly(int width, int height, ReadOnlySpanBitArray bits)
        {
            Width = width;
            Height = height;
            Bits = bits;
        }

        internal ReadOnly(int2 bound, ReadOnlySpanBitArray bits)
            : this(bound.x, bound.y, bits)
        {
        }

        public int GetIndex(int2 pos) => GetIndex(pos.x, pos.y);
        public int GetIndex(int x, int y) => y * Width + x;

        public bool GetCell(int2 pos) => GetCell(pos.x, pos.y);
        public bool GetCell(int x, int y) => Bits.Get(GetIndex(x, y));

        public GridShape Clone(Allocator allocator)
        {
            var clone = new GridShape(Width, Height, allocator);
            CopyTo(clone);
            return clone;
        }

        public unsafe void CopyTo(GridShape other)
        {
            if (!other.IsCreated)
                throw new InvalidOperationException("Cannot copy to a non-created GridShape2D");

            if (Width != other.Width || Height != other.Height)
                throw new ArgumentException($"Cannot copy to GridShape2D with different dimensions. Source: {Width}x{Height}, Target: {other.Width}x{other.Height}");

            if (Bits.BitLength == 0)
            {
                other.Clear();
                return;
            }

            var destBits = other.Bits;
            var sizeInBytes = (Bits.Length + 7) / 8;
            UnsafeUtility.MemCpy(destBits.Ptr, Bits.Ptr, sizeInBytes);
        }

        [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
        public override bool Equals(object? obj) => throw new NotSupportedException();
        public bool Equals(ReadOnly other) => Width == other.Width && Height == other.Height && Bits.SequenceEquals(other.Bits);
        public override int GetHashCode() => HashCode.Combine(Bits.CalculateHashCode(), Width, Height);
        public static bool operator ==(ReadOnly left, ReadOnly right) => left.Equals(right);
        public static bool operator !=(ReadOnly left, ReadOnly right) => !left.Equals(right);
    }
}
