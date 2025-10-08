using System;
using System.Collections.Generic;

namespace DopeGrid.Map;

public readonly struct DualGrid<T> : IEquatable<DualGrid<T>> where T : unmanaged, IEquatable<T>
{
    private readonly ValueGridShape<T> _world;
    private readonly List<ValueGridShape<byte>> _visualLayers = new();
    private readonly Dictionary<T, int> _worldValueVisualLayersMap = new();

    public DualGrid(int width, int height)
    {
        _world = new ValueGridShape<T>(width, height);
    }

    public ValueGridShape<byte>.ReadOnly GetLayerGrid(T value)
    {
        return GetOrCreateVisualLayer(value);
    }

    public T GetWorldValue(int x, int y) => _world[x, y];

    public void SetWorldValue(int x, int y, T value)
    {
    }

    private ValueGridShape<byte> GetOrCreateVisualLayer(T value)
    {
        if (!_worldValueVisualLayersMap.TryGetValue(value, out var layerIndex))
        {
            var grid = new ValueGridShape<byte>(_world.Width, _world.Height);
            layerIndex = _visualLayers.Count;
            _visualLayers.Add(grid);
            _worldValueVisualLayersMap.Add(value, layerIndex);
        }
        return _visualLayers[layerIndex];
    }

    public bool Equals(DualGrid<T> other) => throw new NotSupportedException();
    public override bool Equals(object? obj) => throw new NotSupportedException();
    public override int GetHashCode() => throw new NotSupportedException();
    public static bool operator ==(DualGrid<T> left, DualGrid<T> right) => left.Equals(right);
    public static bool operator !=(DualGrid<T> left, DualGrid<T> right) => !(left == right);
}
