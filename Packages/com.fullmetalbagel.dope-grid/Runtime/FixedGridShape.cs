using System;

namespace DopeGrid;

public readonly record struct FixedGridShape<TSize> : IGridShape<bool>, IReadOnlyBitsGridShape
    where TSize : unmanaged, ISize
{
    public int Width { get; }
    public int Height { get; }
    public int Size => Width * Height;
    private readonly TSize _buffer = default;
    private unsafe Span<byte> GetSpan() => new(_buffer.HeadPtr.ToPointer(), sizeof(TSize));

    public ReadOnlySpanBitArray ReadOnlyBits => new(GetSpan(), Size);
    public SpanBitArray Bits => new(GetSpan(), Size);

    public bool this[int x, int y]
    {
        get => ReadOnlyBits.Get(this.GetIndex(x, y));
        set => Bits.Set(this.GetIndex(x, y), value);
    }

    public bool IsOccupied(int x, int y) => this[x, y];

    public FixedGridShape(int width, int height)
    {
        var byteCount = SpanBitArrayUtility.ByteCount(width * height);
        if (_buffer.Size < byteCount)
        {
            throw new ArgumentException($"Provided buffer is too small, actual={_buffer.Size} expect={byteCount}", nameof(width));
        }

        Width = width;
        Height = height;
    }

    public static implicit operator ReadOnly(FixedGridShape<TSize> shape) => shape.AsReadOnly();
    public ReadOnly AsReadOnly() => new(this);
    public UnsafeBitsGridShape AsUnsafe() => new(Width, Height, _buffer.HeadPtr);

    public readonly record struct ReadOnly : IReadOnlyBitsGridShape
    {
        private readonly FixedGridShape<TSize> _shape = default;
        public int Width => _shape.Width;
        public int Height => _shape.Height;
        public int Size => _shape.Size;
        public bool IsOccupied(int x, int y) => _shape.IsOccupied(x, y);
        public ReadOnlySpanBitArray ReadOnlyBits => _shape.ReadOnlyBits;
        public bool this[int x, int y] => _shape[x, y];

        internal ReadOnly(FixedGridShape<TSize> shape) => _shape = shape;
        public ReadOnly(int width, int height)
            : this(new FixedGridShape<TSize>(width, height))
        {
        }
    }
}
