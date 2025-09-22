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

    private NativeArray<T> _values;

    public ValueGridShape() : this(0, 0, Allocator.Temp) { }
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
        Fill(default(T));
    }

    public readonly bool Contains(int2 pos) => Contains(pos.x, pos.y);
    public readonly bool Contains(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    public readonly ValueGridShape<T> Clone(Allocator allocator)
    {
        var clone = new ValueGridShape<T>(Width, Height, allocator);
        CopyTo(clone);
        return clone;
    }

    public readonly void CopyTo(ValueGridShape<T> other)
    {
        if (Width != other.Width || Height != other.Height)
            throw new ArgumentException($"Cannot copy to GridShape<{typeof(T).Name}> with different dimensions. Source: {Width}x{Height}, Target: {other.Width}x{other.Height}");

        _values.CopyTo(other._values);
    }

    public readonly GridShape ToGridShape(T trueValue, Allocator allocator)
    {
        var shape = new GridShape(Width, Height, allocator);
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                shape.SetCell(x, y, EqualityComparer<T>.Default.Equals(GetValue(x, y), trueValue));
            }
        }
        return shape;
    }

    public readonly GridShape ToGridShape(Func<T, bool> predicate, Allocator allocator)
    {
        var shape = new GridShape(Width, Height, allocator);
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                shape.SetCell(x, y, predicate(GetValue(x, y)));
            }
        }
        return shape;
    }

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

    public readonly int CountValue(T value)
    {
        int count = 0;
        var comparer = EqualityComparer<T>.Default;
        for (int i = 0; i < _values.Length; i++)
        {
            if (comparer.Equals(_values[i], value))
                count++;
        }
        return count;
    }

    public readonly int CountWhere(Func<T, bool> predicate)
    {
        int count = 0;
        for (int i = 0; i < _values.Length; i++)
        {
            if (predicate(_values[i]))
                count++;
        }
        return count;
    }

    public readonly bool Any(Func<T, bool> predicate)
    {
        for (int i = 0; i < _values.Length; i++)
        {
            if (predicate(_values[i]))
                return true;
        }
        return false;
    }

    public readonly bool All(Func<T, bool> predicate)
    {
        for (int i = 0; i < _values.Length; i++)
        {
            if (!predicate(_values[i]))
                return false;
        }
        return true;
    }

    public void Map(Func<T, T> transform)
    {
        for (int i = 0; i < _values.Length; i++)
        {
            _values[i] = transform(_values[i]);
        }
    }

    public void MapWithPosition(Func<int, int, T, T> transform)
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var index = GetIndex(x, y);
                _values[index] = transform(x, y, _values[index]);
            }
        }
    }

    public readonly ValueGridShape<TResult> Select<TResult>(Func<T, TResult> selector, Allocator allocator) where TResult : unmanaged, IEquatable<TResult>
    {
        var result = new ValueGridShape<TResult>(Width, Height, allocator);
        for (int i = 0; i < _values.Length; i++)
        {
            result._values[i] = selector(_values[i]);
        }
        return result;
    }

    public readonly NativeArray<T> GetValues() => _values;

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
}
