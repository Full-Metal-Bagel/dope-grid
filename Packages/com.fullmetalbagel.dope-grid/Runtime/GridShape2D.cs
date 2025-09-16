using System;
using System.Diagnostics.CodeAnalysis;
using Unity.Collections;
using Unity.Mathematics;

namespace DopeGrid;

public struct GridShape2D : IDisposable, IEquatable<GridShape2D>
{
    public int Width { get; }
    public int Height { get; }
    public NativeBitArray.ReadOnly Bits { get; }
    private NativeBitArray _bits;

    public readonly int Size => Bits.Length;
    public readonly int OccupiedSpaceCount => Bits.CountBits(0, Size);
    public readonly int FreeSpaceCount => Size - OccupiedSpaceCount;
    public readonly bool IsCreated => _bits.IsCreated;

    public GridShape2D(int width, int height, Allocator allocator)
    {
        Width = width;
        Height = height;
        _bits = new NativeBitArray(Width * Height, allocator);
        Bits = _bits.AsReadOnly();
    }

    public readonly int GetIndex(int2 pos)
    {
        return pos.y * Width + pos.x;
    }

    public void SetCell(int2 pos, bool value)
    {
        _bits.Set(GetIndex(pos), value);
    }

    public readonly bool GetCell(int2 pos)
    {
        return Bits.IsSet(GetIndex(pos));
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

    public readonly GridShape2D Clone(Allocator allocator)
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

    public void CopyTo(GridShape2D other)
    {
        if (!_bits.IsCreated)
            throw new InvalidOperationException("Cannot copy from a non-created GridShape2D");

        if (!other._bits.IsCreated)
            throw new InvalidOperationException("Cannot copy to a non-created GridShape2D");

        if (Width != other.Width || Height != other.Height)
            throw new ArgumentException($"Cannot copy to GridShape2D with different dimensions. Source: {Width}x{Height}, Target: {other.Width}x{other.Height}");

        other._bits.Copy(0, ref _bits, 0, Size);
    }

    public static implicit operator ReadOnly(GridShape2D shape) => shape.AsReadOnly();
    public ReadOnly AsReadOnly() => new(Width, Height, Bits);

    public bool Equals(GridShape2D other) => Width == other.Width && Height == other.Height && _bits.SequenceEquals(other._bits);
    public override bool Equals(object? obj) => obj is GridShape2D other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(_bits.IsCreated ? _bits.CalculateHashCode() : 0, Width, Height);

    public static bool operator ==(GridShape2D left, GridShape2D right) => left.Equals(right);
    public static bool operator !=(GridShape2D left, GridShape2D right) => !left.Equals(right);

    [SuppressMessage("Design", "CA1716:Identifiers should not match keywords")]
    public readonly ref struct ReadOnly
    {
        public int Width { get; }
        public int Height { get; }
        private readonly NativeBitArray.ReadOnly _bits;

        public int Size => Width * Height;
        public int OccupiedSpaceCount => _bits.CountBits(0, Size);
        public int FreeSpaceCount => Size - OccupiedSpaceCount;

        internal ReadOnly(int width, int height, NativeBitArray.ReadOnly bits)
        {
            Width = width;
            Height = height;
            _bits = bits;
        }

        public int GetIndex(int2 pos) => pos.y * Width + pos.x;
        public bool GetCell(int2 pos) => _bits.IsSet(GetIndex(pos));

        public bool Equals(ReadOnly other) => Width == other.Width && Height == other.Height && _bits.SequenceEquals(other._bits);
        [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
        public override bool Equals(object? obj) => throw new NotSupportedException();
        public override int GetHashCode() => HashCode.Combine(_bits.CalculateHashCode(), Width, Height);
        public static bool operator ==(ReadOnly left, ReadOnly right) => left.Equals(right);
        public static bool operator !=(ReadOnly left, ReadOnly right) => !left.Equals(right);
    }
}
