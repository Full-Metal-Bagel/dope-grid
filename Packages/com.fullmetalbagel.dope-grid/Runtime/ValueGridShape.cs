using System;
using System.Diagnostics.CodeAnalysis;

namespace DopeGrid;

public readonly struct ValueGridShape<T> : IReadOnlyGridShape<T>, IGridShape<T>, IEquatable<ValueGridShape<T>>, IDisposable
    where T : unmanaged, IEquatable<T>
{
    public int Width { get; }
    public int Height { get; }

    private readonly T[] _values = Array.Empty<T>();
    private readonly T _emptyValue = default;

    public int Size => Width * Height;

    public ValueGridShape(int width, int height, T emptyValue = default)
    {
        Width = width;
        Height = height;
        _emptyValue = emptyValue;
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

    public bool IsOccupied(int x, int y) => !this[x, y].Equals(_emptyValue);

    public void Clear()
    {
        Array.Fill(_values, _emptyValue, 0, Size);
    }

    public ValueGridShape<T> Clone() => AsReadOnly().Clone();
    public void CopyTo(ValueGridShape<T> other) => AsReadOnly().CopyTo(other);

    public void Dispose()
    {
        if (_values.Length == 0) return;
        ArrayPool<T>.Return(_values);
    }

    public override int GetHashCode() => throw new NotSupportedException("GetHashCode() on GridShape and GridShape.ReadOnly is not supported.");
    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
    public override bool Equals(object? obj) => throw new NotSupportedException("Equals(object) on GridShape and GridShape.ReadOnly is not supported.");

    public bool Equals(ValueGridShape<T> other) => AsReadOnly().Equals(other);
    public static bool operator ==(ValueGridShape<T> left, ValueGridShape<T> right) => left.Equals(right);
    public static bool operator !=(ValueGridShape<T> left, ValueGridShape<T> right) => !left.Equals(right);

    public static implicit operator ReadOnly(ValueGridShape<T> value) => value.AsReadOnly();
    public ReadOnly AsReadOnly() => new(Width, Height, _values!, _emptyValue);
    public ReadOnly AsBoolReadOnly() => new(Width, Height, _values!, _emptyValue);

    // TODO: better be `ref struct`, but can't have interface until C# 13
    public readonly struct ReadOnly : IReadOnlyGridShape<T>, IEquatable<ReadOnly>
    {
        public int Width { get; }
        public int Height { get; }

        private readonly ReadOnlyMemory<T> _values;
        private readonly T _emptyValue;

        internal ReadOnly(int width, int height, ReadOnlyMemory<T> values, T emptyValue)
        {
            Width = width;
            Height = height;
            _values = values;
            _emptyValue = emptyValue;
        }

        public T this[int x, int y] => _values.Span[this.GetIndex(x, y)];
        public bool IsOccupied(int x, int y) => !this[x, y].Equals(_emptyValue);

        public void CopyTo(ValueGridShape<T> other) => _values.CopyTo(other._values);
        public ValueGridShape<T> Clone()
        {
            var clone = new ValueGridShape<T>(Width, Height, _emptyValue);
            CopyTo(clone);
            return clone;
        }

        public bool Equals(ReadOnly other) => this.IsValuesEquals(other, default(T));
        public static bool operator ==(ReadOnly left, ReadOnly right) => left.Equals(right);
        public static bool operator !=(ReadOnly left, ReadOnly right) => !left.Equals(right);

        public override int GetHashCode() => throw new NotSupportedException("GetHashCode() on ValueGridShape<T> is not supported.");
        [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
        public override bool Equals(object? obj) => throw new NotSupportedException("Equals(object) on ValueGridShape<T> is not supported.");
    }
}
