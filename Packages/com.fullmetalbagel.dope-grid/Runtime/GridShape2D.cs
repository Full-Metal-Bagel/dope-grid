using System;
using System.Diagnostics.CodeAnalysis;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace DopeGrid;

public struct GridShape2D : IEquatable<GridShape2D>, INativeDisposable
{
    public int Width { get; }
    public int Height { get; }
    public readonly int Size => _bits.Length;

    public UnsafeBitArray.ReadOnly Bits { get; }
    internal unsafe UnsafeBitArray WritableBits => *_bits.GetUnsafeBitArrayPtr();
    private NativeBitArray _bits;

    public readonly int OccupiedSpaceCount => Bits.CountBits(0, Size);
    public readonly int FreeSpaceCount => Size - OccupiedSpaceCount;
    public readonly bool IsCreated => _bits.IsCreated;
    public readonly bool IsEmpty => Width == 0 || Height == 0;

    public unsafe GridShape2D(int width, int height, Allocator allocator)
    {
        Width = width;
        Height = height;
        var numBits = width * height;
        _bits = new NativeBitArray(numBits, allocator, NativeArrayOptions.UninitializedMemory);
        UnsafeUtility.MemClear(_bits.GetUnsafeBitArray().Ptr, _bits.Capacity / 8);
        Bits = _bits.GetReadOnlyUnsafeBitArray();
    }

    public readonly int GetIndex(int2 pos) => GetIndex(pos.x, pos.y);
    public readonly int GetIndex(int x, int y) => y * Width + x;

    public readonly bool GetCell(int2 pos) => GetCell(pos.x, pos.y);
    public readonly bool GetCell(int x, int y) => Bits.IsSet(GetIndex(x, y));

    public void SetCell(int2 pos, bool value) => SetCell(pos.x, pos.y, value);
    public void SetCell(int x, int y, bool value) => _bits.Set(GetIndex(x, y), value);

    public void Clear()
    {
        _bits.Clear();
    }

    public void Dispose()
    {
        _bits.Dispose();
    }

    public JobHandle Dispose(JobHandle inputDeps)
    {
        return _bits.Dispose(inputDeps);
    }

    public readonly GridShape2D Clone(Allocator allocator)
    {
        return ToReadOnly().Clone(allocator);
    }

    public void CopyTo(GridShape2D other)
    {
        ToReadOnly().CopyTo(other);
    }

    public static implicit operator ReadOnly(GridShape2D shape) => shape.ToReadOnly();
    public readonly ReadOnly ToReadOnly() => new(Width, Height, Bits);

    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
    public override bool Equals(object? obj) => throw new NotSupportedException();
    public bool Equals(GridShape2D other) => Width == other.Width && Height == other.Height && _bits.SequenceEquals(other._bits);
    public override int GetHashCode() => HashCode.Combine(_bits.IsCreated ? _bits.CalculateHashCode() : 0, Width, Height);

    public static bool operator ==(GridShape2D left, GridShape2D right) => left.Equals(right);
    public static bool operator !=(GridShape2D left, GridShape2D right) => !left.Equals(right);

    [SuppressMessage("Design", "CA1716:Identifiers should not match keywords")]
    public readonly ref struct ReadOnly
    {
        public int Width { get; }
        public int Height { get; }
        public int2 Bound => new(Width, Height);
        public UnsafeBitArray.ReadOnly Bits { get; }

        public int Size => Width * Height;
        public int OccupiedSpaceCount => Bits.CountBits(0, Size);
        public int FreeSpaceCount => Size - OccupiedSpaceCount;

        internal ReadOnly(int width, int height, UnsafeBitArray.ReadOnly bits)
        {
            Width = width;
            Height = height;
            Bits = bits;
        }

        internal ReadOnly(int2 bound, UnsafeBitArray.ReadOnly bits)
            : this(bound.x, bound.y, bits)
        {
        }

        public int GetIndex(int2 pos) => GetIndex(pos.x, pos.y);
        public int GetIndex(int x, int y) => y * Width + x;

        public bool GetCell(int2 pos) => GetCell(pos.x, pos.y);
        public bool GetCell(int x, int y) => Bits.IsSet(GetIndex(x, y));

        public GridShape2D Clone(Allocator allocator)
        {
            var clone = new GridShape2D(Width, Height, allocator);
            CopyTo(clone);
            return clone;
        }

        public unsafe void CopyTo(GridShape2D other)
        {
            if (Bits.Ptr == null)
                throw new InvalidOperationException("Cannot copy from a non-created GridShape2D");

            if (!other.IsCreated)
                throw new InvalidOperationException("Cannot copy to a non-created GridShape2D");

            if (Width != other.Width || Height != other.Height)
                throw new ArgumentException($"Cannot copy to GridShape2D with different dimensions. Source: {Width}x{Height}, Target: {other.Width}x{other.Height}");

            if (Bits.Length == 0)
            {
                other.Clear();
                return;
            }

            var destBits = other.WritableBits;
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
