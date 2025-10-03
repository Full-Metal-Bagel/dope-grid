using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace DopeGrid.Standard;

public struct GridShape : IEquatable<GridShape>, IDisposable
{
    public int Width { get; }
    public int Height { get; }
    public int Size => Width * Height;

    public ReadOnlySpanBitArray ReadOnlyBits => new(_bits!.AsSpan(0, _byteCount), Size);
    internal SpanBitArray Bits => new(_bits!.AsSpan(0, _byteCount), Size);
    private byte[]? _bits;
    private readonly int _byteCount;

    public int OccupiedSpaceCount => ReadOnlyBits.CountBits(0, Size);
    public int FreeSpaceCount => Size - OccupiedSpaceCount;
    public bool IsEmpty => Width == 0 || Height == 0;
    public bool IsCreated => _bits != null;

    public GridShape(int width, int height, Allocator allocator = Allocator.Temp)
    {
        Width = width;
        Height = height;
        var bitLength = width * height;
        _byteCount = SpanBitArrayUtility.ByteCount(bitLength);

        _bits = ArrayPool<byte>.Shared.Rent(_byteCount);
        Array.Clear(_bits, 0, _byteCount);
    }

    public bool this[int x, int y]
    {
        get => this.GetCellValue(x, y);
        set => this.SetCellValue(x, y, value);
    }

    public bool this[GridPosition pos]
    {
        get => this.GetCellValue(pos);
        set => this.SetCellValue(pos, value);
    }

    public bool this[int index]
    {
        get => ReadOnlyBits.Get(index);
        set => Bits.Set(index, value);
    }

    public void FillAll(bool value)
    {
        Bits.SetAll(value);
    }

    public void Clear()
    {
        Array.Clear(_bits, 0, _byteCount);
    }

    public void Dispose()
    {
        if (_bits != null)
        {
            ArrayPool<byte>.Shared.Return(_bits);
            _bits = null;
        }
    }

    public GridShape Clone()
    {
        return AsReadOnly().Clone();
    }

    public void CopyTo(GridShape other)
    {
        AsReadOnly().CopyTo(other);
    }

    public static implicit operator ReadOnly(GridShape shape) => shape.AsReadOnly();
    public ReadOnly AsReadOnly() => new(Width, Height, ReadOnlyBits);

    public override int GetHashCode() => throw new NotSupportedException("GetHashCode() on GridShape and GridShape.ReadOnly is not supported.");
    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
    public override bool Equals(object? obj) => throw new NotSupportedException("Equals(object) on GridShape and GridShape.ReadOnly is not supported.");
    public bool Equals(GridShape other) => AsReadOnly().Equals(other.AsReadOnly());
    public static bool operator ==(GridShape left, GridShape right) => left.Equals(right);
    public static bool operator !=(GridShape left, GridShape right) => !(left == right);

    [SuppressMessage("Design", "CA1716:Identifiers should not match keywords")]
    public readonly ref struct ReadOnly
    {
        public int Width { get; }
        public int Height { get; }
        public (int width, int height) Bound => (Width, Height);
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

        internal ReadOnly((int width, int height) bound, ReadOnlySpanBitArray bits)
            : this(bound.width, bound.height, bits)
        {
        }

        public bool this[int x, int y] => this.GetCellValue(x, y);
        public bool this[GridPosition pos] => this.GetCellValue(pos);
        public bool this[int index] => Bits.Get(index);

        public GridShape Clone(Allocator allocator =  Allocator.Temp)
        {
            var clone = new GridShape(Width, Height);
            CopyTo(clone);
            return clone;
        }

        public void CopyTo(GridShape other)
        {
            if (Width != other.Width || Height != other.Height)
                throw new ArgumentException($"Cannot copy to GridShape with different dimensions. Source: {Width}x{Height}, Target: {other.Width}x{other.Height}");

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
