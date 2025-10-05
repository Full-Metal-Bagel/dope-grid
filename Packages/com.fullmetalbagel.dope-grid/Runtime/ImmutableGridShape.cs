using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;

namespace DopeGrid;

public readonly record struct ImmutableGridShape(int Id) : IReadOnlyGridShape<bool>
{
    private static ImmutableGridShape2DList.ShapeDataset ShapeDataset => ImmutableGridShape2DList.s_shapes.Value;
    public static ImmutableGridShape Empty => new(0);

    public int Id { get; } = Id;
    public (int width, int height) Bound => ShapeDataset.Bounds[Id];
    public int Width => Bound.width;
    public int Height => Bound.height;

    public bool this[int x, int y] => Pattern.Get(this.GetIndex(x, y));
    public bool IsOccupied(int x, int y) => this[x, y];

    public int Size => Width * Height;
    public ReadOnlySpanBitArray Pattern => ShapeDataset.GetPattern(Id);
    public int OccupiedSpaceCount() => Pattern.CountBits(0, Size);
    public int FreeSpaceCount() => Size - OccupiedSpaceCount();

    [Pure, MustUseReturnValue]
    public ImmutableGridShape Flip() => new(ShapeDataset.GetOrCreateFlipped(Id));

    [Pure, MustUseReturnValue]
    public ImmutableGridShape Rotate90() => new(ShapeDataset.GetOrCreateRotated90(Id));

    [Pure, MustUseReturnValue]
    public ImmutableGridShape GetRotatedShape(RotationDegree rotation)
    {
        return rotation switch
        {
            RotationDegree.None => this,
            RotationDegree.Clockwise90 => Rotate90(),
            RotationDegree.Clockwise180 => Rotate90().Rotate90(),
            RotationDegree.Clockwise270 => Rotate90().Rotate90().Rotate90(),
            _ => throw new ArgumentOutOfRangeException(nameof(rotation), rotation, null)
        };
    }
}

public static class ImmutableGridShape2DList
{
    internal static Lazy<ShapeDataset> s_shapes = new(() => new ShapeDataset(), LazyThreadSafetyMode.ExecutionAndPublication);

#if UNITY_2022_3_OR_NEWER
    static ImmutableGridShape2DList()
    {
        // Register cleanup for runtime
        UnityEngine.Application.quitting -= Cleanup;
        UnityEngine.Application.quitting += Cleanup;

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
            s_shapes = new Lazy<Shapes>(() => new Shapes());
        }
    }
#endif

    public static ImmutableGridShape GetOrCreateImmutable<T>(this T shape) where T : IReadOnlyBitsGridShape
    {
        if (!shape.IsTrimmed()) throw new ArgumentException("shape is not trimmed", nameof(shape));
        return new ImmutableGridShape(s_shapes.Value.GetOrCreateShape(shape));
    }

    internal sealed class ShapeDataset : IDisposable
    {
        private readonly List<byte[]> _patterns;
        private readonly List<(int width, int height)> _bounds;
        public IReadOnlyList<(int width, int height)> Bounds => _bounds;
        private readonly List<int> _rotate90Indices; // shape index after rotate 90
        private readonly List<int> _flipIndices; // shape index after flip

        private readonly object _locker = new();

        public ShapeDataset()
        {
            // id 0 reserved for empty
            _patterns = new List<byte[]>(capacity: 128) { Array.Empty<byte>() };
            _bounds = new List<(int width, int height)>(capacity: 128) { (0, 0) };
            _rotate90Indices = new List<int>(capacity: 128) { 0 };
            _flipIndices = new List<int>(capacity: 128) { 0 };
        }

        [Pure, MustUseReturnValue]
        public SpanBitArray GetPattern(int id)
        {
            var size = _bounds[id];
            var bitLength = size.width * size.height;
            var data = _patterns[id];
            return new SpanBitArray(data.AsSpan(), bitLength);
        }

        [Pure, MustUseReturnValue]
        public int FindExistingShape<T>(T shape) where T : IReadOnlyBitsGridShape
        {
            for (var i = 0; i < _bounds.Count; i++)
            {
                var bound = _bounds[i];
                if (bound.width != shape.Width || bound.height != shape.Height)
                    continue;

                var pattern = GetPattern(i);
                if (pattern.SequenceEqual(shape.ReadOnlyBits))
                    return i;
            }
            return -1;
        }

        public int GetOrCreateShape<T>(T shape) where T : IReadOnlyBitsGridShape
        {
            lock (_locker)
            {
                var id = FindExistingShape(shape);
                if (id >= 0) return id;

                id = _bounds.Count;

                _bounds.Add((shape.Width, shape.Height));

                var bytes = new byte[SpanBitArrayUtility.ByteCount(shape.Size())];
                var dest = new SpanBitArray(bytes.AsSpan(), shape.Size());
                shape.ReadOnlyBits.CopyTo(dest);
                _patterns.Add(bytes);

                _rotate90Indices.Add(-1);
                _flipIndices.Add(-1);
                return id;
            }
        }

        public int GetOrCreateRotated90(int id)
        {
            var rotatedId = _rotate90Indices[id];
            if (rotatedId >= 0) return rotatedId;
            // swap width and height for rotation
            var (height, width) = _bounds[id];
            rotatedId = Shapes.UnsafeProcessShape(width, height, (id, self: this), (temp, t) =>
            {
                var shape = new ImmutableGridShape(t.id);
                shape.RotateShape(RotationDegree.Clockwise90, temp, default(bool));
                return t.self.GetOrCreateShape(temp);
            });
            _rotate90Indices[id] = rotatedId;
            return rotatedId;
        }

        public int GetOrCreateFlipped(int id)
        {
            var flippedId = _flipIndices[id];
            if (flippedId >= 0) return flippedId;
            var (width, height) = _bounds[id];
            flippedId = Shapes.UnsafeProcessShape(width, height, (id, self: this), (temp, t) =>
            {
                var shape = new ImmutableGridShape(t.id);
                shape.FlipShape(FlipAxis.Horizontal, temp, default(bool));
                return t.self.GetOrCreateShape(temp);
            });
            _flipIndices[id] = flippedId;
            _flipIndices[flippedId] = id;
            return flippedId;
        }

        public void Dispose()
        {
            _patterns.Clear();
            _bounds.Clear();
            _rotate90Indices.Clear();
            _flipIndices.Clear();
        }
    }
}
