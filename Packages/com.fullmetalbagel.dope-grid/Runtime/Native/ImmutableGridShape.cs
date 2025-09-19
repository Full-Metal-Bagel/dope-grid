using System;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DopeGrid.Native;

public readonly record struct ImmutableGridShape(int Id)
{
    private ImmutableGridShape2DList.Shapes Shapes => ImmutableGridShape2DList.s_shapes.Value;
    public static ImmutableGridShape Empty => new(0);

    public int Id { get; } = Id;
    public int2 Bound => Shapes.Bounds[Id];
    public int Width => Bound.x;
    public int Height => Bound.y;
    public int Size => Width * Height;
    public ReadOnlySpanBitArray Pattern => Shapes.GetPattern(Id);
    public int OccupiedSpaceCount => Pattern.CountBits(0, Size);
    public int FreeSpaceCount => Size - OccupiedSpaceCount;
    public bool IsEmpty => Width == 0 || Height == 0;

    public ImmutableGridShape Rotate90() => new(Shapes.GetOrCreateRotated90(Id));
    public ImmutableGridShape Flip() => new(Shapes.GetOrCreateFlipped(Id));

    public int GetIndex(int2 pos) => GetIndex(pos.x, pos.y);
    public int GetIndex(int x, int y) => y * Width + x;

    public bool GetCell(int2 pos) => GetCell(pos.x, pos.y);
    public bool GetCell(int x, int y) => Pattern.Get(GetIndex(x, y));

    public static implicit operator GridShape.ReadOnly(ImmutableGridShape shape) => shape.ToReadOnlyGridShape();
    public GridShape.ReadOnly ToReadOnlyGridShape() => new(Width, Height, Pattern);

    public void CopyTo(GridShape other)
    {
        ToReadOnlyGridShape().CopyTo(other);
    }
}

public static class ImmutableGridShape2DList
{
    internal static readonly Lazy<Shapes> s_shapes = new(() => new Shapes(Allocator.Persistent));

    static ImmutableGridShape2DList()
    {
        // Register cleanup for runtime
        Application.quitting -= Cleanup;
        Application.quitting += Cleanup;

#if UNITY_EDITOR
        // Register cleanup for editor
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        AssemblyReloadEvents.beforeAssemblyReload -= Cleanup;
        AssemblyReloadEvents.beforeAssemblyReload += Cleanup;
#endif
    }

#if UNITY_EDITOR
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.ExitingEditMode)
        {
            Cleanup();
        }
    }
#endif

    private static void Cleanup()
    {
        if (s_shapes.IsValueCreated)
        {
            s_shapes.Value.Dispose();
        }
    }

    public static ImmutableGridShape GetOrCreateImmutable(this GridShape shape)
    {
        return shape.AsReadOnly().GetOrCreateImmutable();
    }

    public static ImmutableGridShape GetOrCreateImmutable(this in GridShape.ReadOnly shape)
    {
        if (!shape.IsTrimmed()) throw new ArgumentException("shape is not trimmed", nameof(shape));
        return new ImmutableGridShape(s_shapes.Value.GetOrCreateShape(shape));
    }

    internal unsafe class Shapes : IDisposable
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

            PatternBegins.Add(0);
            Bounds.Add(int2.zero);
            Rotate90Indices.Add(0);
            FlipIndices.Add(0);
        }

        [Pure, MustUseReturnValue]
        public GridShape.ReadOnly GetReadOnlyShape(int id)
        {
            var bound = Bounds[id];
            var pattern = GetPattern(id);
            return new GridShape.ReadOnly(bound, pattern);
        }

        [Pure, MustUseReturnValue]
        public SpanBitArray GetPattern(int id)
        {
            var begin = PatternBegins[id];
            var size = Bounds[id];
            var ptr = UnsafeUtility.AddressOf(ref Patterns.ElementAt(begin));
            var bitLength = size.x * size.y;
            var span = new Span<byte>(ptr, SpanBitArrayUtility.ByteCount(bitLength));
            return new SpanBitArray(span, bitLength);
        }

        [Pure, MustUseReturnValue]
        public int FindExistingShape(in GridShape.ReadOnly shape)
        {
            for (var i = 0; i < Bounds.Length; i++)
            {
                var bound = Bounds[i];
                if (bound.x != shape.Width || bound.y != shape.Height)
                    continue;

                var pattern = GetPattern(i);
                if (pattern.SequenceEqual(shape.Bits))
                    return i;
            }
            return -1;
        }

        public int GetOrCreateShape(in GridShape.ReadOnly shape)
        {
            var id = FindExistingShape(shape);
            if (id < 0) id = AddShape(shape);
            return id;
        }

        public int GetOrCreateRotated90(int id)
        {
            var rotatedId = Rotate90Indices[id];
            if (rotatedId < 0)
            {
                var shape = GetReadOnlyShape(id);
                using var rotatedShape = shape.Rotate(RotationDegree.Rotate90, Allocator.Temp);
                rotatedId = FindExistingShape(rotatedShape);
                if (rotatedId < 0) rotatedId = AddShape(rotatedShape);
                Rotate90Indices[id] = rotatedId;
            }
            return rotatedId;
        }

        public int GetOrCreateFlipped(int id)
        {
            var flippedId = FlipIndices[id];
            if (flippedId < 0)
            {
                var shape = GetReadOnlyShape(id);
                using var flippedShape = shape.Flip(FlipAxis.Horizontal, Allocator.Temp);
                flippedId = FindExistingShape(flippedShape);
                if (flippedId < 0) flippedId = AddShape(flippedShape);
                FlipIndices[id] = flippedId;
                FlipIndices[flippedId] = id;
            }
            return flippedId;
        }

        private int AddShape(in GridShape.ReadOnly shape)
        {
            var id = Bounds.Length;

            // Add bounds first so GetPattern can access it
            Bounds.Add(new int2(shape.Width, shape.Height));

            // Store pattern
            var patternBegin = Patterns.Length;
            PatternBegins.Add(patternBegin);

            // Calculate the actual size in bytes of the bit array
            var sizeInBytes = (shape.Size + 7) / 8; // Convert bits to bytes
            var ulongCount = (sizeInBytes + sizeof(ulong) - 1) / sizeof(ulong); // Round up to ulong boundary

            // Resize patterns array to accommodate new data
            Patterns.Resize(Patterns.Length + ulongCount, NativeArrayOptions.ClearMemory);

            // Direct memory copy from shape's bit array to patterns
            var pattern = GetPattern(id);
            shape.Bits.CopyTo(pattern);

            Rotate90Indices.Add(-1);
            FlipIndices.Add(-1);
            return id;
        }

        public void Dispose()
        {
            Patterns.Dispose();
            PatternBegins.Dispose();
            Bounds.Dispose();
            Rotate90Indices.Dispose();
            FlipIndices.Dispose();
        }
    }
}
