using System;
using System.Collections.Generic;

namespace DopeGrid.Map;

public readonly struct DualGridMap<T> : IEquatable<DualGridMap<T>>, IDisposable
    where T : unmanaged, IEquatable<T>
{
    private readonly ValueGridShape<T> _world;
    private readonly List<ValueGridShape<byte>> _visualLayers = new();
    private readonly Dictionary<T, int> _worldValueVisualLayersMap = new();

    public DualGridMap(int width, int height, T defaultValue = default)
    {
        _world = new ValueGridShape<T>(width, height, defaultValue);
        var visualLayer = GetOrCreateVisualLayer(defaultValue);
        visualLayer.Values.Fill(0x0F);
    }

    public ValueGridShape<byte>.ReadOnly GetLayerGrid(T value)
    {
        return GetOrCreateVisualLayer(value);
    }

    public T GetWorldValue(int x, int y) => _world[x, y];

    public void SetWorldValue(int x, int y, T value)
    {
        var old = _world[x, y];
        if (old.Equals(value)) return;

        _world[x, y] = value;

        // Recompute the four vertices around the changed cell for both old and new values
        // Vertices: (x, y), (x+1, y), (x, y+1), (x+1, y+1)
        int bitIndex = 0;
        for (var dy = 0; dy <= 1; dy++)
        for (var dx = 0; dx <= 1; dx++)
        {
            var vx = x + dx;
            var vy = y + dy;

            var layerValueIndex = vy * (_world.Width + 1) + vx;
            FlipLayerValueBit(old, layerValueIndex, bitIndex);
            FlipLayerValueBit(value, layerValueIndex, bitIndex);

            bitIndex++;
        }
    }

    private void FlipLayerValueBit(T value, int index, int bitIndex)
    {
        var layer = GetOrCreateVisualLayer(value);
        ref byte layerValue = ref layer.Values[index];
        layerValue ^= (byte)(1 << bitIndex);
    }

    private ValueGridShape<byte> GetOrCreateVisualLayer(T value)
    {
        if (!_worldValueVisualLayersMap.TryGetValue(value, out var layerIndex))
        {
            var grid = new ValueGridShape<byte>(_world.Width + 1, _world.Height + 1);
            layerIndex = _visualLayers.Count;
            _visualLayers.Add(grid);
            _worldValueVisualLayersMap.Add(value, layerIndex);
        }
        return _visualLayers[layerIndex];
    }

    public bool Equals(DualGridMap<T> other) => throw new NotSupportedException();
    public override bool Equals(object? obj) => throw new NotSupportedException();
    public override int GetHashCode() => throw new NotSupportedException();
    public static bool operator ==(DualGridMap<T> left, DualGridMap<T> right) => left.Equals(right);
    public static bool operator !=(DualGridMap<T> left, DualGridMap<T> right) => !(left == right);

    public void Dispose()
    {
        _world.Dispose();
        foreach (var layer in _visualLayers) layer.Dispose();
        _visualLayers.Clear();
        _worldValueVisualLayersMap.Clear();
    }
}
