using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DopeGrid.Standard;

public struct ValueGridShape<T> : IEquatable<ValueGridShape<T>>, IDisposable where T : IEquatable<T>
{
    public int Width { get; }
    public int Height { get; }
    public int Size => Width * Height;
    public bool IsEmpty => Width == 0 || Height == 0;
    public bool IsCreated => _values != null;
    private T[]? _values;

    internal Span<T> Values => _values!.AsSpan(0, Size);

    public ValueGridShape(int width, int height)
    {
        Width = width;
        Height = height;
        _values = ArrayPool<T>.Shared.Rent(width * height);
        Array.Clear(_values, 0, width * height);
    }

    public ValueGridShape(int width, int height, T defaultValue)
    {
        Width = width;
        Height = height;
        _values = ArrayPool<T>.Shared.Rent(width * height);
        Array.Fill(_values, defaultValue, 0, width * height);
    }

    public T GetValue(GridPosition pos) => GetValue(pos.X, pos.Y);
    public T GetValue(int x, int y) => Values[GetIndex(x, y)];

    public void SetValue(GridPosition pos, T value) => SetValue(pos.X, pos.Y, value);
    public void SetValue(int x, int y, T value) => Values[GetIndex(x, y)] = value;

    public T this[int x, int y]
    {
        get => GetValue(x, y);
        set => SetValue(x, y, value);
    }

    public T this[GridPosition pos]
    {
        get => GetValue(pos);
        set => SetValue(pos, value);
    }

    public T this[int index]
    {
        get => Values[index];
        set => Values[index] = value;
    }

    public void Fill(T value)
    {
        Values.Fill(value);
    }

    public void FillRect(int x, int y, int width, int height, T value)
    {
        for (int dy = 0; dy < height; dy++)
        {
            for (int dx = 0; dx < width; dx++)
            {
                var px = x + dx;
                var py = y + dy;
                if (px >= 0 && px < Width && py >= 0 && py < Height)
                {
                    SetValue(px, py, value);
                }
            }
        }
    }

    public void FillRect(GridPosition pos, (int width, int height) size, T value)
    {
        FillRect(pos.X, pos.Y, size.width, size.height, value);
    }

    public void Clear()
    {
        Fill(default!);
    }

    public bool Contains(GridPosition pos) => Contains(pos.X, pos.Y);
    public bool Contains(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    public ValueGridShape<T> Clone() => AsReadOnly().Clone();
    public void CopyTo(ValueGridShape<T> other) => AsReadOnly().CopyTo(other);

    public GridShape ToGridShape(T trueValue) => AsReadOnly().ToGridShape(trueValue);
    public GridShape ToGridShape(Func<T, bool> predicate) => AsReadOnly().ToGridShape(predicate);

    public void FromGridShape(GridShape shape, T trueValue, T falseValue)
    {
        if (Width != shape.Width || Height != shape.Height)
            throw new ArgumentException($"Cannot copy from GridShape with different dimensions. Source: {shape.Width}x{shape.Height}, Target: {Width}x{Height}");

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                SetValue(x, y, shape.GetCell(x, y) ? trueValue : falseValue);
            }
        }
    }

    public void Dispose()
    {
        if (_values != null)
        {
            ArrayPool<T>.Shared.Return(_values);
            _values = null;
        }
    }

    public bool Equals(ValueGridShape<T> other)
    {
        if (Width != other.Width || Height != other.Height)
            return false;

        return Values.SequenceEqual(other.Values);
    }

    public override int GetHashCode() => throw new NotSupportedException("GetHashCode() on ValueGridShape is not supported.");
    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
    public override bool Equals(object? obj) => throw new NotSupportedException("Equals(object) on ValueGridShape is not supported.");

    public static bool operator ==(ValueGridShape<T> left, ValueGridShape<T> right) => left.Equals(right);
    public static bool operator !=(ValueGridShape<T> left, ValueGridShape<T> right) => !(left == right);

    public static implicit operator ReadOnly(ValueGridShape<T> value) => value.AsReadOnly();
    public ReadOnly AsReadOnly() => new(Width, Height, Values);

    [SuppressMessage("Design", "CA1716:Identifiers should not match keywords")]
    public readonly ref struct ReadOnly
    {
        public int Width { get; }
        public int Height { get; }
        public int Size => Width * Height;
        public bool IsEmpty => Width == 0 || Height == 0;
        private readonly ReadOnlySpan<T> _values;

        internal ReadOnly(int width, int height, ReadOnlySpan<T> values)
        {
            Width = width;
            Height = height;
            _values = values;
        }

        public int GetIndex(GridPosition pos) => GetIndex(pos.X, pos.Y);
        public int GetIndex(int x, int y) => y * Width + x;

        public T GetValue(GridPosition pos) => GetValue(pos.X, pos.Y);
        public T GetValue(int x, int y) => _values[GetIndex(x, y)];

        public T this[int x, int y] => GetValue(x, y);
        public T this[GridPosition pos] => GetValue(pos);
        public T this[int index] => _values[index];

        public ValueGridShape<T> Clone()
        {
            var clone = new ValueGridShape<T>(Width, Height);
            CopyTo(clone);
            return clone;
        }

        public void CopyTo(ValueGridShape<T> other)
        {
            if (Width != other.Width || Height != other.Height)
                throw new ArgumentException($"Cannot copy to ValueGridShape<{typeof(T).Name}> with different dimensions. Source: {Width}x{Height}, Target: {other.Width}x{other.Height}");
            _values.CopyTo(other.Values);
        }

        public GridShape ToGridShape(T trueValue)
        {
            return ToGridShape((value, @true) => EqualityComparer<T>.Default.Equals(value, @true), trueValue);
        }

        public GridShape ToGridShape(Func<T, bool> predicate)
        {
            return ToGridShape((value, p) => p(value), predicate);
        }

        public GridShape ToGridShape<TCaptureData>(Func<T, TCaptureData, bool> predicate, TCaptureData data)
        {
            var shape = new GridShape(Width, Height);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    shape.SetCell(x, y, predicate(GetValue(x, y), data));
                }
            }
            return shape;
        }

        public bool Equals(ReadOnly other)
        {
            if (Width != other.Width || Height != other.Height)
                return false;

            return _values.SequenceEqual(other._values);
        }

        public override int GetHashCode() => throw new NotSupportedException("GetHashCode() on ValueGridShape.ReadOnly is not supported.");
        [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
        public override bool Equals(object? obj) => throw new NotSupportedException("Equals(object) on ValueGridShape.ReadOnly is not supported.");

        public static bool operator ==(ReadOnly left, ReadOnly right) => left.Equals(right);
        public static bool operator !=(ReadOnly left, ReadOnly right) => !left.Equals(right);
    }
}
