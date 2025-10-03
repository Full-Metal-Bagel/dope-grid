using System;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace DopeGrid.Native;

public readonly record struct ImmutableGridShape(int Id) : IReadOnlyGridShape
{
    private ImmutableGridShape2DList.Shapes Shapes => ImmutableGridShape2DList.s_shapes.Value;
    public static ImmutableGridShape Empty => new(0);

    public int Id { get; } = Id;
    public (int width, int height) Bound => Shapes.Bounds[Id];
    public int Width => Bound.width;
    public int Height => Bound.height;

    public ReadOnlySpanBitArray Pattern => Shapes.GetPattern(Id);
    public int OccupiedSpaceCount => Pattern.CountBits(0, this.Size());
    public int FreeSpaceCount => this.Size() - OccupiedSpaceCount;

    public bool IsFreeCell(GridPosition pos) => !this[pos];

    public bool this[GridPosition pos] => this.GetCellValue(pos);
    public bool this[int x, int y] => this.GetCellValue(x, y);
    public bool this[int index] => Pattern.Get(index);

    public ImmutableGridShape Rotate90(Allocator allocator = Allocator.Temp) => new(Shapes.GetOrCreateRotated90(Id, allocator));
    public ImmutableGridShape Flip(Allocator allocator = Allocator.Temp) => new(Shapes.GetOrCreateFlipped(Id, allocator));

    public static explicit operator GridShape.ReadOnly(ImmutableGridShape shape) => shape.ToReadOnlyGridShape();
    public GridShape.ReadOnly ToReadOnlyGridShape() => new(Width, Height, Pattern);

    public void CopyTo(GridShape other)
    {
        ToReadOnlyGridShape().CopyTo(other);
    }
}

public static class ImmutableGridShape2DList
{
    internal static Lazy<Shapes> s_shapes = new(() => new Shapes(Allocator.Persistent));

    static ImmutableGridShape2DList()
    {
        // Register cleanup for runtime
        Application.quitting -= Cleanup;
        Application.quitting += Cleanup;

#if UNITY_EDITOR
        // Register cleanup for editor
        UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        UnityEditor.AssemblyReloadEvents.beforeAssemblyReload -= Cleanup;
        UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += Cleanup;
#endif
    }

#if UNITY_EDITOR
    private static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
    {
        if (state is UnityEditor.PlayModeStateChange.EnteredEditMode or UnityEditor.PlayModeStateChange.ExitingEditMode)
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
            s_shapes = new Lazy<Shapes>(() => new Shapes(Allocator.Persistent));
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

    internal unsafe struct Shapes : IDisposable
    {
        private NativeList<ulong> _patterns;
        public NativeArray<ulong>.ReadOnly Patterns => _patterns.AsReadOnly();

        private NativeList<int> _patternBegins;
        public NativeArray<int>.ReadOnly PatternBegins => _patternBegins.AsReadOnly();

        private NativeList<(int width, int height)> _bounds;
        public NativeArray<(int width, int height)>.ReadOnly Bounds => _bounds.AsReadOnly();

        private NativeList<int> _rotate90Indices; // the shape index after rotate 90 in clockwise
        public NativeArray<int>.ReadOnly Rotate90Indices => _rotate90Indices.AsReadOnly();

        private NativeList<int> _flipIndices;
        public NativeArray<int>.ReadOnly FlipIndices => _flipIndices.AsReadOnly();

        private readonly object _locker = new();

        public Shapes(Allocator allocator)
        {
            _patterns = new(1024, allocator);
            _patternBegins = new(128, allocator);
            _bounds = new(128, allocator);
            _rotate90Indices = new(128, allocator); // the shape index after rotate 90 in clockwise
            _flipIndices = new(128, allocator);

            _patternBegins.Add(0);
            _bounds.Add((0, 0));
            _rotate90Indices.Add(0);
            _flipIndices.Add(0);
        }

        [Pure, MustUseReturnValue]
        public GridShape.ReadOnly GetReadOnlyShape(int id)
        {
            var bound = _bounds[id];
            var pattern = GetPattern(id);
            return new GridShape.ReadOnly(bound.width, bound.height, pattern);
        }

        [Pure, MustUseReturnValue]
        public SpanBitArray GetPattern(int id)
        {
            var begin = _patternBegins[id];
            var size = _bounds[id];
            var ptr = UnsafeUtility.AddressOf(ref _patterns.ElementAt(begin));
            var bitLength = size.width * size.height;
            var span = new Span<byte>(ptr, SpanBitArrayUtility.ByteCount(bitLength));
            return new SpanBitArray(span, bitLength);
        }

        [Pure, MustUseReturnValue]
        public int FindExistingShape(in GridShape.ReadOnly shape)
        {
            for (var i = 0; i < _bounds.Length; i++)
            {
                var bound = _bounds[i];
                if (bound.width != shape.Width || bound.height != shape.Height)
                    continue;

                var pattern = GetPattern(i);
                if (pattern.SequenceEqual(shape.Bits))
                    return i;
            }
            return -1;
        }

        public int GetOrCreateShape(in GridShape.ReadOnly shape)
        {
            lock (_locker)
            {
                var id = FindExistingShape(shape);
                if (id >= 0) return id;

                id = _bounds.Length;

                // Add bounds first so GetPattern can access it
                _bounds.Add((shape.Width, shape.Height));

                // Store pattern
                var patternBegin = _patterns.Length;
                _patternBegins.Add(patternBegin);

                // Calculate the actual size in bytes of the bit array
                var sizeInBytes = (shape.Size() + 7) / 8; // Convert bits to bytes
                var ulongCount = (sizeInBytes + sizeof(ulong) - 1) / sizeof(ulong); // Round up to ulong boundary

                // Resize patterns array to accommodate new data
                _patterns.Resize(_patterns.Length + ulongCount, NativeArrayOptions.ClearMemory);

                // Direct memory copy from shape's bit array to patterns
                var pattern = GetPattern(id);
                shape.Bits.CopyTo(pattern);

                _rotate90Indices.Add(-1);
                _flipIndices.Add(-1);
                return id;
            }
        }

        public int GetOrCreateRotated90(int id, Allocator allocator = Allocator.Temp)
        {
            var rotatedId = _rotate90Indices[id];
            if (rotatedId < 0)
            {
                var shape = GetReadOnlyShape(id);
                using var rotatedShape = shape.Rotate(RotationDegree.Clockwise90, allocator);
                rotatedId = GetOrCreateShape(rotatedShape);
                _rotate90Indices[id] = rotatedId;
            }
            return rotatedId;
        }

        public int GetOrCreateFlipped(int id, Allocator allocator = Allocator.Temp)
        {
            var flippedId = _flipIndices[id];
            if (flippedId < 0)
            {
                var shape = GetReadOnlyShape(id);
                using var flippedShape = shape.Flip(FlipAxis.Horizontal, allocator);
                flippedId = GetOrCreateShape(flippedShape);
                _flipIndices[id] = flippedId;
                _flipIndices[flippedId] = id;
            }
            return flippedId;
        }

        public void Dispose()
        {
            _patterns.Dispose();
            _patternBegins.Dispose();
            _bounds.Dispose();
            _rotate90Indices.Dispose();
            _flipIndices.Dispose();
        }
    }
}
