using System;
using System.Diagnostics.CodeAnalysis;

namespace DopeGrid.Map;

public readonly struct ValueGridShape4Bits : IReadOnlyGridShape<byte>, IGridShape<byte>, IEquatable<ValueGridShape4Bits>, IDisposable
{
    public int Width { get; }
    public int Height { get; }

    private readonly byte[] _values = Array.Empty<byte>();
    private readonly byte _emptyValue = default;
    public Span<byte> Values => _values.AsSpan();

    public int Size => Width * Height;
    private int ValueSize => (Size + 1) / 2;

    public ValueGridShape4Bits(int width, int height, byte emptyValue = default)
    {
        Width = width;
        Height = height;
        _emptyValue = emptyValue;
        if (Size > 0)
        {
            _values = ArrayPool<byte>.Rent(ValueSize);
            // Fill both nibbles of each byte with the empty value
            var combined = (byte)((emptyValue & 0x0F) | ((emptyValue & 0x0F) << 4));
            Array.Fill(_values, combined, 0, ValueSize);
        }
    }

    public byte this[int x, int y]
    {
        get
        {
            var index = this.GetIndex(x, y);
            byte value = _values[index / 2];
            return (index & 1) == 1 ? (byte)(value >> 4) : (byte)(value & 0x0F);
        }
        set
        {
            var index = this.GetIndex(x, y);
            var arrayIndex = index / 2;
            byte current = _values[arrayIndex];
            byte v = (byte)(value & 0x0F);

            if ((index & 1) == 1)
            {
                // Set high nibble, keep low nibble
                _values[arrayIndex] = (byte)((current & 0x0F) | (v << 4));
            }
            else
            {
                // Set low nibble, keep high nibble
                _values[arrayIndex] = (byte)((current & 0xF0) | v);
            }
        }
    }

    public bool IsOccupied(int x, int y) => !this[x, y].Equals(_emptyValue);

    public void Clear()
    {
        var combined = (byte)((_emptyValue & 0x0F) | ((_emptyValue & 0x0F) << 4));
        Array.Fill(_values, combined, 0, ValueSize);
    }

    public void CopyTo(ValueGridShape4Bits other) => _values.AsSpan(0, ValueSize).CopyTo(other._values);
    public ValueGridShape4Bits Clone()
    {
        var clone = new ValueGridShape4Bits(Width, Height, _emptyValue);
        CopyTo(clone);
        return clone;
    }

    public void Dispose()
    {
        if (_values.Length == 0) return;
        ArrayPool<byte>.Return(_values);
    }

    public bool IsSame(ValueGridShape4Bits other) => ReferenceEquals(_values, other._values);

    public override int GetHashCode() => throw new NotSupportedException("GetHashCode() on GridShape and GridShape.ReadOnly is not supported.");
    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
    public override bool Equals(object? obj) => throw new NotSupportedException("Equals(object) on GridShape and GridShape.ReadOnly is not supported.");

    public bool Equals(ValueGridShape4Bits other) => AsReadOnly().Equals(other);
    public static bool operator ==(ValueGridShape4Bits left, ValueGridShape4Bits right) => left.Equals(right);
    public static bool operator !=(ValueGridShape4Bits left, ValueGridShape4Bits right) => !left.Equals(right);

    public static implicit operator ReadOnly(ValueGridShape4Bits value) => value.AsReadOnly();
    public ReadOnly AsReadOnly() => new(this);

    // TODO: better be `ref struct`, but can't have interface until C# 13
    public readonly struct ReadOnly : IReadOnlyGridShape<byte>, IEquatable<ReadOnly>
    {
        private readonly ValueGridShape4Bits _shape;
        public int Width => _shape.Width;
        public int Height => _shape.Height;
        public ReadOnlySpan<byte> Values => _shape.Values;

        internal ReadOnly(ValueGridShape4Bits shape) => _shape = shape;
        public byte this[int x, int y] => _shape[x, y];
        public bool IsOccupied(int x, int y) => _shape.IsOccupied(x, y);
        public bool IsSame(ReadOnly other) => _shape.IsSame(other._shape);
        public void CopyTo(ValueGridShape4Bits other) => _shape.CopyTo(other);
        public ValueGridShape4Bits Clone() => _shape.Clone();

        public bool Equals(ReadOnly other) => this.IsValuesEquals(other, default(byte));
        public static bool operator ==(ReadOnly left, ReadOnly right) => left.Equals(right);
        public static bool operator !=(ReadOnly left, ReadOnly right) => !left.Equals(right);

        public override int GetHashCode() => throw new NotSupportedException("GetHashCode() on ValueGridShape<byte> is not supported.");
        [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
        public override bool Equals(object? obj) => throw new NotSupportedException("Equals(object) on ValueGridShape<byte> is not supported.");
    }
}
