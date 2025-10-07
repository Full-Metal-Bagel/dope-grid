using System;
using System.Diagnostics.CodeAnalysis;

namespace DopeGrid;

public readonly struct GridShape : IReadOnlyBitsGridShape, IGridShape<bool>, IEquatable<GridShape>, IDisposable
{
    public int Width { get; }
    public int Height { get; }
    private readonly byte[] _bytes = Array.Empty<byte>();

    public ReadOnlySpanBitArray ReadOnlyBits => new(_bytes!, Size);
    public SpanBitArray Bits => new(_bytes!.AsSpan(), Size);

    public int Size => Width * Height;
    public int OccupiedSpaceCount() => ReadOnlyBits.CountBits(0, Size);
    public int FreeSpaceCount() => Size - OccupiedSpaceCount();

    public GridShape(int width, int height)
    {
        Width = width;
        Height = height;
        var bitLength = width * height;
        var byteCount = SpanBitArrayUtility.ByteCount(bitLength);
        if (byteCount > 0)
        {
            _bytes = ArrayPool<byte>.Rent(byteCount);
            Array.Clear(_bytes, 0, byteCount);
        }
    }

    public bool this[int x, int y]
    {
        get => ReadOnlyBits.Get(this.GetIndex(x, y));
        set => Bits.Set(this.GetIndex(x, y), value);
    }

    public bool IsOccupied(int x, int y) => this[x, y];

    public void Clear()
    {
        Array.Clear(_bytes, 0, _bytes.Length);
    }

    public void Dispose()
    {
        if (_bytes.Length == 0) return;
        ArrayPool<byte>.Return(_bytes);
    }

    public GridShape Clone() => AsReadOnly().Clone();
    public void CopyTo(GridShape other) => AsReadOnly().CopyTo(other);

    public static implicit operator ReadOnly(GridShape shape) => shape.AsReadOnly();
    public ReadOnly AsReadOnly() => new(Width, Height, _bytes.AsMemory(0, SpanBitArrayUtility.ByteCount(Size)));
    public UnsafeBitsGridShape AsUnsafe() => new(Width, Height, _bytes);

    public bool Equals(GridShape other) => AsReadOnly().Equals(other.AsReadOnly());
    public static bool operator ==(in GridShape left, in GridShape right) => left.Equals(right);
    public static bool operator !=(in GridShape left, in GridShape right) => !left.Equals(right);

    public override int GetHashCode() => throw new NotSupportedException("GetHashCode() on GridShape and GridShape.ReadOnly is not supported.");
    public override bool Equals(object? obj) => throw new NotSupportedException("Equals(object) on GridShape and GridShape.ReadOnly is not supported.");

    // TODO: better be `ref struct`, but can't have interface until C# 13
    public readonly struct ReadOnly : IReadOnlyBitsGridShape, IEquatable<ReadOnly>
    {
        public int Width { get; }
        public int Height { get; }

        private readonly ReadOnlyMemory<byte> _bytes;
        public ReadOnlySpanBitArray ReadOnlyBits => new(_bytes.Span, Size);

        public int Size => Width * Height;
        public int OccupiedSpaceCount() => ReadOnlyBits.CountBits(0, Size);
        public int FreeSpaceCount() => Size - OccupiedSpaceCount();

        internal ReadOnly(int width, int height, ReadOnlyMemory<byte> bytes)
        {
            Width = width;
            Height = height;
            _bytes = bytes;
        }

        public bool this[int x, int y] => ReadOnlyBits.Get(this.GetIndex(x, y));
        public bool IsOccupied(int x, int y) => this[x, y];

        public void CopyTo(GridShape other)
        {
            if (!this.HasSameSize(other))
                throw new ArgumentException($"Cannot copy to GridShape2D with different dimensions. Source: {Width}x{Height}, Target: {other.Width}x{other.Height}");
            _bytes.CopyTo(other._bytes);
        }

        public GridShape Clone()
        {
            var clone = new GridShape(Width, Height);
            CopyTo(clone);
            return clone;
        }

        public bool Equals(ReadOnly other) => Width == other.Width && Height == other.Height && ReadOnlyBits.SequenceEqual(other.ReadOnlyBits);
        public static bool operator ==(ReadOnly left, ReadOnly right) => left.Equals(right);
        public static bool operator !=(ReadOnly left, ReadOnly right) => !left.Equals(right);

        public override int GetHashCode() => throw new NotSupportedException("GetHashCode() on GridShape and GridShape.ReadOnly is not supported.");
        [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
        public override bool Equals(object? obj) => throw new NotSupportedException("Equals(object) on GridShape and GridShape.ReadOnly is not supported.");

    }
}
