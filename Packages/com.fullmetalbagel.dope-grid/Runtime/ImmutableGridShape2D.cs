using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace DopeGrid;

public readonly record struct ImmutableGridShape2D(int Id)
{
    private ImmutableGridShape2DList.Shapes Shapes => ImmutableGridShape2DList.ImmutableShapes.Value;
    public static ImmutableGridShape2D Empty => new(0);

    public int Id { get; } = Id;
    public int2 Bound => Shapes.Bounds[Id];
    public int Width => Bound.x;
    public int Height => Bound.y;
    public UnsafeBitArray.ReadOnly Pattern => Shapes.GetPattern(Id);

    public ImmutableGridShape2D Rotate90() => new(Shapes.Rotate90Indices[Id]);
    public ImmutableGridShape2D Flip() => new(Shapes.FlipIndices[Id]);

    public static implicit operator GridShape2D.ReadOnly(ImmutableGridShape2D shape) => shape.ToReadOnlyGridShape();
    public GridShape2D.ReadOnly ToReadOnlyGridShape() => new(Width, Height, Pattern);
}

internal static class ImmutableGridShape2DList
{
    public static readonly Lazy<Shapes> ImmutableShapes = new(() => new Shapes(Allocator.Persistent));

    public unsafe class Shapes
    {
        public NativeList<ulong> Patterns;
        public NativeList<int> PatternBegins;
        public NativeList<int2> Bounds;
        public NativeList<int> Rotate90Indices; // the shape index after rotate 90 in clockwise
        public NativeList<int> FlipIndices;

        public Shapes(Allocator allocator)
        {
            Patterns = new(1024, allocator);
            PatternBegins = new(128, allocator);
            Bounds = new(128, allocator);
            Rotate90Indices = new(128, allocator); // the shape index after rotate 90 in clockwise
            FlipIndices = new(128, allocator);

            PatternBegins[0] = 0;
            Bounds[0] = int2.zero;
            Rotate90Indices[0] = 0;
            FlipIndices[0] = 0;
        }

        public UnsafeBitArray.ReadOnly GetPattern(int id)
        {
            var begin = PatternBegins[id];
            var size = Bounds[id];
            var ptr = (ulong*)UnsafeUtility.AddressOf(ref Patterns.ElementAt(begin));
            return NativeCollectionExtension.CreateReadOnlyUnsafeBitArray(ptr, size.x * size.y);
        }
    }

    public static ImmutableGridShape2D GetOrCreateImmutable(GridShape2D.ReadOnly shape)
    {
        // Step 1: Trim the shape
        var trimmedShape = shape.Trim(Allocator.Temp);
        if (trimmedShape.Width == 0 || trimmedShape.Height == 0)
        {
            // Empty shape, return a default ID (0)
            return new ImmutableGridShape2D(0);
        }

        var shapes = ImmutableShapes.Value;

        // Step 2: Find if the shape already exists
        var existingId = FindExistingShape(shapes, trimmedShape);
        if (existingId >= 0)
        {
            return new ImmutableGridShape2D(existingId);
        }

        // Step 3: Create new shapes with rotations and flips
        return CreateNewShapeVariants(trimmedShape, shapes);
    }

    public static int FindExistingShape(this Shapes shapes, GridShape2D shape)
    {
        return shapes.FindExistingShape(shape.ToReadOnly());
    }

    public static int FindExistingShape(this Shapes shapes, in GridShape2D.ReadOnly shape)
    {
        for (var i = 0; i < shapes.Bounds.Length; i++)
        {
            var bound = shapes.Bounds[i];
            if (bound.x != shape.Width || bound.y != shape.Height)
                continue;

            var pattern = shapes.GetPattern(i);
            var isMatch = true;
            for (var j = 0; j < shape.Size && isMatch; j++)
            {
                if (pattern.IsSet(j) != shape.Bits.IsSet(j))
                    isMatch = false;
            }

            if (isMatch)
                return i;
        }
        return -1;
    }

    private static int AddShapeToList(GridShape2D shape, Shapes shapes)
    {
        var id = shapes.Bounds.Length;

        // Store pattern
        var patternBegin = shapes.Patterns.Length;
        shapes.PatternBegins.Add(patternBegin);

        // Calculate number of ulong elements needed
        var bitCount = shape.Size;
        var ulongCount = (bitCount + 63) / 64;

        // Add pattern data
        for (var i = 0; i < ulongCount; i++)
        {
            var startBit = i * 64;
            var endBit = math.min(startBit + 64, bitCount);
            ulong value = 0;

            for (var bit = startBit; bit < endBit; bit++)
            {
                if (shape.Bits.IsSet(bit))
                {
                    value |= 1UL << (bit - startBit);
                }
            }

            shapes.Patterns.Add(value);
        }

        shapes.Bounds.Add(new int2(shape.Width, shape.Height));

        return id;
    }

    private static ImmutableGridShape2D CreateNewShapeVariants(GridShape2D shape, Shapes shapes)
    {
        var allocator = Allocator.Temp;

        // Add base shape
        var baseId = AddShapeToList(shape, shapes);

        // Create rotations
        using var rotated90 = shape.Rotate(RotationDegree.Rotate90, allocator);
        using var rotated180 = shape.Rotate(RotationDegree.Rotate180, allocator);
        using var rotated270 = shape.Rotate(RotationDegree.Rotate270, allocator);

        // Check if rotations are unique and add them
        var rotate90Id = shape.Equals(rotated90) ? baseId : shapes.FindExistingShape(rotated90);
        if (rotate90Id < 0) rotate90Id = AddShapeToList(rotated90, shapes);

        var rotate180Id = shape.Equals(rotated180) ? baseId :
                          rotated90.Equals(rotated180) ? rotate90Id : shapes.FindExistingShape(rotated180);
        if (rotate180Id < 0) rotate180Id = AddShapeToList(rotated180, shapes);

        var rotate270Id = shape.Equals(rotated270) ? baseId :
                          rotated90.Equals(rotated270) ? rotate90Id :
                          rotated180.Equals(rotated270) ? rotate180Id : shapes.FindExistingShape(rotated270);
        if (rotate270Id < 0) rotate270Id = AddShapeToList(rotated270, shapes);

        // Create flipped version
        using var flipped = shape.Flip(FlipAxis.Horizontal, Allocator.Temp);
        var flippedId = shape.Equals(flipped) ? baseId : shapes.FindExistingShape(flipped);
        if (flippedId < 0) flippedId = AddShapeToList(flipped, shapes);

        // Create rotations of flipped shape
        using var flippedRotated90 = flipped.Rotate(RotationDegree.Rotate90, allocator);
        using var flippedRotated180 = flipped.Rotate(RotationDegree.Rotate180, allocator);
        using var flippedRotated270 = flipped.Rotate(RotationDegree.Rotate270, allocator);

        var flippedRotate90Id = FindOrAddShape(flippedRotated90, shapes, baseId, rotate90Id, rotate180Id, rotate270Id, flippedId);
        var flippedRotate180Id = FindOrAddShape(flippedRotated180, shapes, baseId, rotate90Id, rotate180Id, rotate270Id, flippedId, flippedRotate90Id);
        var flippedRotate270Id = FindOrAddShape(flippedRotated270, shapes, baseId, rotate90Id, rotate180Id, rotate270Id, flippedId, flippedRotate90Id, flippedRotate180Id);

        // Set up rotation indices for base shape
        shapes.Rotate90Indices.Add(rotate90Id);
        shapes.FlipIndices.Add(flippedId);

        // Set up rotation indices for rotated shapes
        if (rotate90Id != baseId)
        {
            shapes.Rotate90Indices.Add(rotate180Id);
            shapes.FlipIndices.Add(flippedRotate90Id);
        }

        if (rotate180Id != baseId && rotate180Id != rotate90Id)
        {
            shapes.Rotate90Indices.Add(rotate270Id);
            shapes.FlipIndices.Add(flippedRotate180Id);
        }

        if (rotate270Id != baseId && rotate270Id != rotate90Id && rotate270Id != rotate180Id)
        {
            shapes.Rotate90Indices.Add(baseId);
            shapes.FlipIndices.Add(flippedRotate270Id);
        }

        // Set up rotation indices for flipped shapes
        if (flippedId != baseId)
        {
            shapes.Rotate90Indices.Add(flippedRotate90Id);
            shapes.FlipIndices.Add(baseId);
        }

        if (flippedRotate90Id != baseId && flippedRotate90Id != flippedId)
        {
            shapes.Rotate90Indices.Add(flippedRotate180Id);
            shapes.FlipIndices.Add(rotate90Id);
        }

        if (flippedRotate180Id != baseId && flippedRotate180Id != flippedId && flippedRotate180Id != flippedRotate90Id)
        {
            shapes.Rotate90Indices.Add(flippedRotate270Id);
            shapes.FlipIndices.Add(rotate180Id);
        }

        if (flippedRotate270Id != baseId && flippedRotate270Id != flippedId && flippedRotate270Id != flippedRotate90Id && flippedRotate270Id != flippedRotate180Id)
        {
            shapes.Rotate90Indices.Add(flippedId);
            shapes.FlipIndices.Add(rotate270Id);
        }

        shape.Dispose();
        return new ImmutableGridShape2D(baseId);
    }

    private static int FindOrAddShape(GridShape2D shape, Shapes shapes, params int[] existingIds)
    {
        // Check against existing IDs first
        foreach (var id in existingIds)
        {
            if (id < 0 || id >= shapes.Bounds.Length) continue;

            if (shapes.Bounds[id].x != shape.Width || shapes.Bounds[id].y != shape.Height)
                continue;

            var pattern = shapes.GetPattern(id);
            var isMatch = true;
            for (var j = 0; j < shape.Size && isMatch; j++)
            {
                if (pattern.IsSet(j) != shape.Bits.IsSet(j))
                    isMatch = false;
            }

            if (isMatch)
                return id;
        }

        // Check rest of the list
        var foundId = shapes.FindExistingShape(shape);
        return foundId >= 0 ? foundId : AddShapeToList(shape, shapes);
    }
}
