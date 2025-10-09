using System;
using System.Diagnostics.CodeAnalysis;

namespace DopeGrid;

public readonly struct ValueGridShape<T> : IReadOnlyGridShape<T>, IGridShape<T>, IEquatable<ValueGridShape<T>>, IDisposable
    where T : unmanaged, IEquatable<T>
{
    public int Width { get; }
    public int Height { get; }

    private readonly T[] _values = Array.Empty<T>();
    public T EmptyValue { get; } = default;
    public Span<T> Values => _values.AsSpan();

    public int Size => Width * Height;

    public ValueGridShape(int width, int height, T emptyValue = default)
    {
        Width = width;
        Height = height;
        EmptyValue = emptyValue;
        if (Size > 0)
        {
            _values = ArrayPool<T>.Rent(Size);
            Array.Fill(_values, emptyValue, 0, Size);
        }
    }

    public T this[int x, int y]
    {
        get => _values[this.GetIndex(x, y)];
        set => _values[this.GetIndex(x, y)] = value;
    }

    public bool IsOccupied(int x, int y) => !this[x, y].Equals(EmptyValue);

    public void Clear()
    {
        Array.Fill(_values, EmptyValue, 0, Size);
    }

    public void CopyTo(ValueGridShape<T> other) => _values.AsSpan(0, Size).CopyTo(other._values);
    public ValueGridShape<T> Clone()
    {
        var clone = new ValueGridShape<T>(Width, Height, EmptyValue);
        CopyTo(clone);
        return clone;
    }

    public void Dispose()
    {
        if (_values.Length == 0) return;
        ArrayPool<T>.Return(_values);
    }

    public bool IsSame(ValueGridShape<T> other) => ReferenceEquals(_values, other._values);

    public override int GetHashCode() => throw new NotSupportedException("GetHashCode() on GridShape and GridShape.ReadOnly is not supported.");
    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
    public override bool Equals(object? obj) => throw new NotSupportedException("Equals(object) on GridShape and GridShape.ReadOnly is not supported.");

    public bool Equals(ValueGridShape<T> other) => AsReadOnly().Equals(other);
    public static bool operator ==(ValueGridShape<T> left, ValueGridShape<T> right) => left.Equals(right);
    public static bool operator !=(ValueGridShape<T> left, ValueGridShape<T> right) => !left.Equals(right);

    public static implicit operator ReadOnly(ValueGridShape<T> value) => value.AsReadOnly();
    public ReadOnly AsReadOnly() => new(this);

    // TODO: better be `ref struct`, but can't have interface until C# 13
    public readonly struct ReadOnly : IReadOnlyGridShape<T>, IEquatable<ReadOnly>
    {
        private readonly ValueGridShape<T> _shape;
        public int Width => _shape.Width;
        public int Height => _shape.Height;
        public ReadOnlySpan<T> Values => _shape.Values;

        internal ReadOnly(ValueGridShape<T> shape) => _shape = shape;
        public T this[int x, int y] => _shape[x, y];
        public bool IsOccupied(int x, int y) => _shape.IsOccupied(x, y);
        public bool IsSame(ReadOnly other) => _shape.IsSame(other._shape);
        public void CopyTo(ValueGridShape<T> other) => _shape.CopyTo(other);
        public ValueGridShape<T> Clone() => _shape.Clone();

        public bool Equals(ReadOnly other) => this.IsValuesEquals(other, default(T));
        public static bool operator ==(ReadOnly left, ReadOnly right) => left.Equals(right);
        public static bool operator !=(ReadOnly left, ReadOnly right) => !left.Equals(right);

        public override int GetHashCode() => throw new NotSupportedException("GetHashCode() on ValueGridShape<T> is not supported.");
        [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
        public override bool Equals(object? obj) => throw new NotSupportedException("Equals(object) on ValueGridShape<T> is not supported.");
    }
}
