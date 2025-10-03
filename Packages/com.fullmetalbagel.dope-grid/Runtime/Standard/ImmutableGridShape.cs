using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace DopeGrid.Standard;

public readonly record struct ImmutableGridShape(int Id)
{
    private static ImmutableGridShapeRepository Shapes => ImmutableGridShapeRepository.Instance;
    public static ImmutableGridShape Empty => new(0);

    public int Id { get; } = Id;
    public (int width, int height) Bound => Shapes.GetBound(Id);
    public int Width => Bound.width;
    public int Height => Bound.height;
    public int Size => Width * Height;
    public ReadOnlySpanBitArray Pattern => Shapes.GetPattern(Id);
    public int OccupiedSpaceCount => Pattern.CountBits(0, Size);
    public int FreeSpaceCount => Size - OccupiedSpaceCount;
    public bool IsEmpty => Width == 0 || Height == 0;

    public ImmutableGridShape Rotate90() => new(Shapes.GetOrCreateRotated90(Id));
    public ImmutableGridShape Flip() => new(Shapes.GetOrCreateFlipped(Id));

    public bool this[GridPosition pos] => this.GetCellValue(pos);
    public bool this[int x, int y] => this.GetCellValue(x, y);
    public bool this[int index] => Pattern.Get(index);

    public static implicit operator GridShape.ReadOnly(ImmutableGridShape shape) => shape.ToReadOnlyGridShape();
    public GridShape.ReadOnly ToReadOnlyGridShape() => new(Width, Height, Pattern);

    public void CopyTo(GridShape other)
    {
        ToReadOnlyGridShape().CopyTo(other);
    }
}

public static class ImmutableGridShapeExtensions
{
    public static ImmutableGridShape GetOrCreateImmutable(this GridShape shape)
    {
        return shape.AsReadOnly().GetOrCreateImmutable();
    }

    public static ImmutableGridShape GetOrCreateImmutable(this in GridShape.ReadOnly shape)
    {
        if (!shape.IsTrimmed(freeValue: false)) throw new ArgumentException("shape is not trimmed", nameof(shape));
        return new ImmutableGridShape(ImmutableGridShapeRepository.Instance.GetOrCreateShape(shape));
    }
}

internal class ImmutableGridShapeRepository
{
    private static readonly Lazy<ImmutableGridShapeRepository> _instance = new(() => new ImmutableGridShapeRepository());
    public static ImmutableGridShapeRepository Instance => _instance.Value;

    private readonly List<byte[]> _patterns = new() { Array.Empty<byte>() };
    private readonly List<(int width, int height)> _bounds = new() { (0, 0) };
    private readonly List<int> _rotate90Indices = new() { 0 };
    private readonly List<int> _flipIndices = new() { 0 };
    private readonly object _lock = new();

    private ImmutableGridShapeRepository()
    {
    }

    [Pure, MustUseReturnValue]
    public (int width, int height) GetBound(int id)
    {
        return _bounds[id];
    }

    [Pure, MustUseReturnValue]
    public ReadOnlySpanBitArray GetPattern(int id)
    {
        var pattern = _patterns[id];
        var bound = _bounds[id];
        var bitLength = bound.width * bound.height;
        return new ReadOnlySpanBitArray(pattern.AsSpan(), bitLength);
    }

    [Pure, MustUseReturnValue]
    public GridShape.ReadOnly GetReadOnlyShape(int id)
    {
        var bound = _bounds[id];
        var pattern = GetPattern(id);
        return new GridShape.ReadOnly(bound.width, bound.height, pattern);
    }

    [Pure, MustUseReturnValue]
    private int FindExistingShape(in GridShape.ReadOnly shape)
    {
        for (var i = 0; i < _bounds.Count; i++)
        {
            var bound = _bounds[i];
            if (bound.width != shape.Width || bound.height != shape.Height)
                continue;

            var pattern= GetPattern(i);
            if (pattern.SequenceEqual(shape.Bits))
                return i;
        }
        return -1;
    }

    public int GetOrCreateShape(in GridShape.ReadOnly shape)
    {
        lock (_lock)
        {
            var id = FindExistingShape(shape);
            if (id >= 0) return id;

            id = _bounds.Count;

            // Add bounds
            _bounds.Add((shape.Width, shape.Height));

            // Store pattern
            var sizeInBytes = SpanBitArrayUtility.ByteCount(shape.Size);
            var patternBytes = new byte[sizeInBytes];
            shape.Bits.CopyTo(new SpanBitArray(patternBytes.AsSpan(), shape.Size));
            _patterns.Add(patternBytes);

            _rotate90Indices.Add(-1);
            _flipIndices.Add(-1);
            return id;
        }
    }

    public int GetOrCreateRotated90(int id)
    {
        var rotatedId = _rotate90Indices[id];
        if (rotatedId < 0)
        {
            var shape = GetReadOnlyShape(id);
            using var rotatedShape = shape.Rotate(RotationDegree.Clockwise90);
            rotatedId = GetOrCreateShape(rotatedShape);
            _rotate90Indices[id] = rotatedId;
        }
        return rotatedId;
    }

    public int GetOrCreateFlipped(int id)
    {
        var flippedId = _flipIndices[id];
        if (flippedId < 0)
        {
            var shape = GetReadOnlyShape(id);
            using var flippedShape = shape.Flip(FlipAxis.Horizontal);
            flippedId = GetOrCreateShape(flippedShape);
            _flipIndices[id] = flippedId;
            _flipIndices[flippedId] = id;
        }
        return flippedId;
    }
}
