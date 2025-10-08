using System;
using System.Collections.Generic;

namespace DopeGrid.Map;

public readonly struct DualGridMap<T> : IDualGridMap<T, ValueGridShape<byte>>, IEquatable<DualGridMap<T>>, IDisposable
    where T : unmanaged, IEquatable<T>
{
    public ValueGridShape<T> WorldGrid { get; }
    private readonly List<ValueGridShape<byte>> _visualLayers = new();
    private readonly Dictionary<T, int> _worldValueVisualLayersMap = new();

    public int Width => WorldGrid.Width;
    public int Height => WorldGrid.Height;

    public bool IsOccupied(int x, int y) => WorldGrid.IsOccupied(x, y);

    public T this[int x, int y]
    {
        get => WorldGrid[x, y];
        set => this.SetWorldValue(x, y, value, default(ValueGridShape<byte>));
    }

    public DualGridMap(int width, int height, T defaultValue = default)
    {
        WorldGrid = new ValueGridShape<T>(width, height, defaultValue);
        var visualLayer = GetOrCreateVisualLayer(defaultValue);
        visualLayer.Values.Fill(0x0F);
    }

    public ValueGridShape<byte>.ReadOnly GetVisualLayer(T value)
    {
        return GetOrCreateVisualLayer(value);
    }

    private ValueGridShape<byte> GetOrCreateVisualLayer(T value)
    {
        if (!_worldValueVisualLayersMap.TryGetValue(value, out var layerIndex))
        {
            var grid = new ValueGridShape<byte>(WorldGrid.Width + 1, WorldGrid.Height + 1);
            layerIndex = _visualLayers.Count;
            _visualLayers.Add(grid);
            _worldValueVisualLayersMap.Add(value, layerIndex);
        }
        return _visualLayers[layerIndex];
    }

    ValueGridShape<byte> IDualGridMap<T, ValueGridShape<byte>>.GetOrCreateVisualLayer(T value)
    {
        return GetOrCreateVisualLayer(value);
    }

    public bool Equals(DualGridMap<T> other) => throw new NotSupportedException();
    public override bool Equals(object? obj) => throw new NotSupportedException();
    public override int GetHashCode() => throw new NotSupportedException();
    public static bool operator ==(DualGridMap<T> left, DualGridMap<T> right) => left.Equals(right);
    public static bool operator !=(DualGridMap<T> left, DualGridMap<T> right) => !(left == right);

    public void Dispose()
    {
        WorldGrid.Dispose();
        foreach (var layer in _visualLayers) layer.Dispose();
        _visualLayers.Clear();
        _worldValueVisualLayersMap.Clear();
    }
}
