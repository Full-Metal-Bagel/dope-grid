using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DopeGrid.Standard;

public class ValueGridShape<T> : IEquatable<ValueGridShape<T>>, IDisposable where T : IEquatable<T>
{
    public int Width { get; }
    public int Height { get; }
    public int Size => Width * Height;
    public bool IsEmpty => Width == 0 || Height == 0;
    private T[] _values;
    public T[] Values => _values;

    public ValueGridShape(int width, int height)
    {
        Width = width;
        Height = height;
        _values = new T[width * height];
    }

    public ValueGridShape(int width, int height, T defaultValue)
    {
        Width = width;
        Height = height;
        _values = new T[width * height];
        Fill(defaultValue);
    }

    public int GetIndex((int x, int y) pos) => GetIndex(pos.x, pos.y);
    public int GetIndex(int x, int y) => y * Width + x;

    public T GetValue((int x, int y) pos) => GetValue(pos.x, pos.y);
    public T GetValue(int x, int y) => _values[GetIndex(x, y)];

    public void SetValue((int x, int y) pos, T value) => SetValue(pos.x, pos.y, value);
    public void SetValue(int x, int y, T value) => _values[GetIndex(x, y)] = value;

    public T this[int x, int y]
    {
        get => GetValue(x, y);
        set => SetValue(x, y, value);
    }

    public T this[(int x, int y) pos]
    {
        get => GetValue(pos);
        set => SetValue(pos, value);
    }

    public void Fill(T value)
    {
        Array.Fill(_values, value);
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

    public void FillRect((int x, int y) pos, (int width, int height) size, T value)
    {
        FillRect(pos.x, pos.y, size.width, size.height, value);
    }

    public void Clear()
    {
        Fill(default!);
    }

    public bool Contains((int x, int y) pos) => Contains(pos.x, pos.y);
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

    public int CountValue(T value) => AsReadOnly().CountValue(value);
    public int CountWhere(Func<T, bool> predicate) => AsReadOnly().CountWhere(predicate);
    public bool Any(Func<T, bool> predicate) => AsReadOnly().Any(predicate);
    public bool All(Func<T, bool> predicate) => AsReadOnly().All(predicate);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public bool Equals(ValueGridShape<T>? other)
    {
        if (other == null || Width != other.Width || Height != other.Height)
            return false;

        return _values.AsSpan().SequenceEqual(other._values.AsSpan());
    }

    public override int GetHashCode() => throw new NotSupportedException("GetHashCode() on ValueGridShape is not supported.");
    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
    public override bool Equals(object? obj) => throw new NotSupportedException("Equals(object) on ValueGridShape is not supported.");

    public static bool operator ==(ValueGridShape<T>? left, ValueGridShape<T>? right) => left?.Equals(right) ?? right is null;
    public static bool operator !=(ValueGridShape<T>? left, ValueGridShape<T>? right) => !(left == right);

    public static implicit operator ReadOnly(ValueGridShape<T> value) => value.AsReadOnly();
    public ReadOnly AsReadOnly() => new(Width, Height, _values);

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

        public int GetIndex((int x, int y) pos) => GetIndex(pos.x, pos.y);
        public int GetIndex(int x, int y) => y * Width + x;

        public T GetValue((int x, int y) pos) => GetValue(pos.x, pos.y);
        public T GetValue(int x, int y) => _values[GetIndex(x, y)];

        public T this[int x, int y] => GetValue(x, y);
        public T this[(int x, int y) pos] => GetValue(pos);

        public bool Contains((int x, int y) pos) => Contains(pos.x, pos.y);
        public bool Contains(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

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
            _values.CopyTo(other._values.AsSpan());
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

        public int CountValue(T target)
        {
            return Count((v, t) => EqualityComparer<T>.Default.Equals(v, t), target);
        }

        public int CountWhere(Func<T, bool> predicate)
        {
            return Count((value, p) => p(value), predicate);
        }

        public int Count<TCaptureData>(Func<T, TCaptureData, bool> predicate, TCaptureData data)
        {
            int count = 0;
            for (int i = 0; i < _values.Length; i++)
            {
                if (predicate(_values[i], data))
                    count++;
            }
            return count;
        }

        public bool Any(Func<T, bool> predicate)
        {
            return Any((v, p) => p(v), predicate);
        }

        public bool Any<TCaptureData>(Func<T, TCaptureData, bool> predicate, TCaptureData data)
        {
            foreach (var t in _values)
            {
                if (predicate(t, data))
                    return true;
            }
            return false;
        }

        public bool All(Func<T, bool> predicate)
        {
            return All((v, p) => p(v), predicate);
        }

        public bool All<TCaptureData>(Func<T, TCaptureData, bool> predicate, TCaptureData data)
        {
            foreach (var t in _values)
            {
                if (!predicate(t, data))
                    return false;
            }
            return true;
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