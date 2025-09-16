using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace DopeGrid;

public readonly record struct ImmutableGridShape2D
{
    private ImmutableGridShape2DList.Shapes Shapes => ImmutableGridShape2DList.ImmutableShapes.Value;
    public int Id { get; }
    public int2 Bound => Shapes.Bounds[Id];
    public int Width => Bound.x;
    public int Height => Bound.y;
    public UnsafeBitArray.ReadOnly Pattern => Shapes.GetPattern(Id);

    public ImmutableGridShape2D(int id)
    {
        Id = id;
    }

    public ImmutableGridShape2D Rotate90()
    {
        return new ImmutableGridShape2D(Shapes.Rotate90Indices[Id]);
    }

    public ImmutableGridShape2D Flip()
    {
        return new ImmutableGridShape2D(Shapes.FlipIndices[Id]);
    }

    public static implicit operator GridShape2D.ReadOnly(ImmutableGridShape2D shape) => shape.ToReadOnlyGridShape();

    public GridShape2D.ReadOnly ToReadOnlyGridShape()
    {
        return new GridShape2D.ReadOnly(Width, Height, Pattern);
    }
}

internal static class ImmutableGridShape2DList
{
    public static readonly Lazy<Shapes> ImmutableShapes = new(() => new Shapes());

    public unsafe class Shapes
    {
        public NativeList<ulong> Patterns = new(1024, Allocator.Persistent);
        public NativeList<int> PatternBegins = new(128, Allocator.Persistent);
        public NativeList<int2> Bounds = new(128, Allocator.Persistent);
        public NativeList<int> Rotate90Indices = new(128, Allocator.Persistent); // the shape index after rotate 90 in clockwise
        public NativeList<int> FlipIndices = new(128, Allocator.Persistent);

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
    }
}
