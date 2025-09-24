using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace DopeGrid.Native;

public struct ValueGridShape<T> : IEquatable<ValueGridShape<T>>, INativeDisposable where T : unmanaged, IEquatable<T>
{
    public int Width { get; }
    public int Height { get; }
    public readonly int Size => Width * Height;
    public readonly bool IsEmpty => Width == 0 || Height == 0;
    public bool IsCreated => _values.IsCreated;
    private NativeArray<T> _values;
    public NativeArray<T> Values => _values;

    public ValueGridShape(int width, int height, Allocator allocator)
    {
        Width = width;
        Height = height;
        _values = new NativeArray<T>(width * height, allocator, NativeArrayOptions.ClearMemory);
    }

    public ValueGridShape(int width, int height, T defaultValue, Allocator allocator)
    {
        Width = width;
        Height = height;
        _values = new NativeArray<T>(width * height, allocator, NativeArrayOptions.UninitializedMemory);
        Fill(defaultValue);
    }

    public readonly int GetIndex(int2 pos) => GetIndex(pos.x, pos.y);
    public readonly int GetIndex(int x, int y) => y * Width + x;

    public readonly T GetValue(int2 pos) => GetValue(pos.x, pos.y);
    public readonly T GetValue(int x, int y) => _values[GetIndex(x, y)];

    public void SetValue(int2 pos, T value) => SetValue(pos.x, pos.y, value);
    public void SetValue(int x, int y, T value) => _values[GetIndex(x, y)] = value;

    public T this[int x, int y]
    {
        readonly get => GetValue(x, y);
        set => SetValue(x, y, value);
    }

    public T this[int2 pos]
    {
        readonly get => GetValue(pos);
        set => SetValue(pos, value);
    }

    public void Fill(T value)
    {
        for (int i = 0; i < _values.Length; i++)
        {
            _values[i] = value;
        }
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

    public void FillRect(int2 pos, int2 size, T value)
    {
        FillRect(pos.x, pos.y, size.x, size.y, value);
    }

    public void Clear()
    {
        Fill(default);
    }

    public readonly bool Contains(int2 pos) => Contains(pos.x, pos.y);
    public readonly bool Contains(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    public readonly ValueGridShape<T> Clone(Allocator allocator) => ((ReadOnly)this).Clone(allocator);
    public readonly void CopyTo(ValueGridShape<T> other) => ((ReadOnly)this).CopyTo(other);

    public readonly GridShape ToGridShape(T trueValue, Allocator allocator) => ((ReadOnly)this).ToGridShape(trueValue, allocator);
    public readonly GridShape ToGridShape(Func<T, bool> predicate, Allocator allocator) => ((ReadOnly)this).ToGridShape(predicate, allocator);

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

    public readonly int CountValue(T value) => ((ReadOnly)this).CountValue(value);
    public readonly int CountWhere(Func<T, bool> predicate) => ((ReadOnly)this).CountWhere(predicate);
    public readonly bool Any(Func<T, bool> predicate) => ((ReadOnly)this).Any(predicate);
    public readonly bool All(Func<T, bool> predicate) => ((ReadOnly)this).All(predicate);

    public void Dispose()
    {
        if (_values.IsCreated)
            _values.Dispose();
    }

    public JobHandle Dispose(JobHandle inputDeps)
    {
        if (_values.IsCreated)
            return _values.Dispose(inputDeps);
        return inputDeps;
    }

    public readonly bool Equals(ValueGridShape<T> other)
    {
        if (Width != other.Width || Height != other.Height)
            return false;

        return _values.ArraysEqual(other._values);
    }

    public override int GetHashCode() => throw new NotSupportedException("GetHashCode() on GridShape and GridShape.ReadOnly is not supported.");
    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
    public override bool Equals(object? obj) => throw new NotSupportedException("Equals(object) on GridShape and GridShape.ReadOnly is not supported.");

    public static bool operator ==(ValueGridShape<T> left, ValueGridShape<T> right) => left.Equals(right);
    public static bool operator !=(ValueGridShape<T> left, ValueGridShape<T> right) => !left.Equals(right);

    public static implicit operator ReadOnly(ValueGridShape<T> value) => value.AsReadOnly();
    public ReadOnly AsReadOnly() => new(Width, Height, _values.AsReadOnly());

    [SuppressMessage("Design", "CA1716:Identifiers should not match keywords")]
    public readonly ref struct ReadOnly
    {
        public int Width { get; }
        public int Height { get; }
        public int Size => Width * Height;
        public bool IsEmpty => Width == 0 || Height == 0;
        public readonly NativeArray<T>.ReadOnly Values;

        internal ReadOnly(int width, int height, NativeArray<T>.ReadOnly values)
        {
            Width = width;
            Height = height;
            Values = values;
        }

        public int GetIndex(int2 pos) => GetIndex(pos.x, pos.y);
        public int GetIndex(int x, int y) => y * Width + x;

        public T GetValue(int2 pos) => GetValue(pos.x, pos.y);
        public T GetValue(int x, int y) => Values[GetIndex(x, y)];

        public T this[int x, int y] => GetValue(x, y);
        public T this[int2 pos] => GetValue(pos);

        public bool Contains(int2 pos) => Contains(pos.x, pos.y);
        public bool Contains(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

        public ValueGridShape<T> Clone(Allocator allocator)
        {
            var clone = new ValueGridShape<T>(Width, Height, allocator);
            CopyTo(clone);
            return clone;
        }

        public void CopyTo(ValueGridShape<T> other)
        {
            if (Width != other.Width || Height != other.Height)
                throw new ArgumentException($"Cannot copy to GridShape<{typeof(T).Name}> with different dimensions. Source: {Width}x{Height}, Target: {other.Width}x{other.Height}");
            Values.CopyTo(other.Values);
        }

        public GridShape ToGridShape(T trueValue, Allocator allocator)
        {
            return ToGridShape((value, @true) => EqualityComparer<T>.Default.Equals(value, @true), allocator, trueValue);
        }

        public GridShape ToGridShape(Func<T, bool> predicate, Allocator allocator)
        {
            return ToGridShape((value, p) => p(value), allocator, predicate);
        }

        public GridShape ToGridShape<TCaptureData>(Func<T, TCaptureData, bool> predicate, Allocator allocator, TCaptureData data)
        {
            var shape = new GridShape(Width, Height, allocator);
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
            for (int i = 0; i < Values.Length; i++)
            {
                if (predicate(Values[i], data))
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
            foreach (var t in Values)
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
            foreach (var t in Values)
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

            return Values.ArraysEqual(other.Values);
        }

        public override int GetHashCode() => throw new NotSupportedException("GetHashCode() on GridShape and GridShape.ReadOnly is not supported.");
        [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
        public override bool Equals(object? obj) => throw new NotSupportedException("Equals(object) on GridShape and GridShape.ReadOnly is not supported.");

        public static bool operator ==(ReadOnly left, ReadOnly right) => left.Equals(right);
        public static bool operator !=(ReadOnly left, ReadOnly right) => !left.Equals(right);
    }
}
