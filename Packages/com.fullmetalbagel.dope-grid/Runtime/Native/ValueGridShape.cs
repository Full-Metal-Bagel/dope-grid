using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.Collections;
using Unity.Jobs;

namespace DopeGrid.Native;

public struct ValueGridShape<T> : IEquatable<ValueGridShape<T>>, INativeDisposable where T : unmanaged, IEquatable<T>
{
    public int Width { get; }
    public int Height { get; }

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
        FillAll(defaultValue);
    }

    public T this[int x, int y]
    {
        get => this.GetCellValue(x, y);
        set => SetValue(x, y, value);
    }

    public T this[GridPosition pos]
    {
        get => this.GetCellValue(pos);
        set => SetValue(pos, value);
    }

    public T this[int index]
    {
        readonly get => _values[index];
        set => _values[index] = value;
    }

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
                SetValue(x, y, shape.GetCellValue(x, y) ? trueValue : falseValue);
            }
        }
    }

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
        public readonly NativeArray<T>.ReadOnly Values;

        internal ReadOnly(int width, int height, NativeArray<T>.ReadOnly values)
        {
            Width = width;
            Height = height;
            Values = values;
        }

        public T this[int x, int y] => this[this.GetIndex(x, y)];
        public T this[GridPosition pos] => this[pos.X, pos.Y];
        public T this[int index] => Values[index];

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
                    shape.SetCell(x, y, predicate(this[x, y], data));
                }
            }
            return shape;
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
