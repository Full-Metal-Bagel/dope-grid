using System;
using System.Collections.Generic;

namespace DopeGrid.Map;

public readonly struct CompactDualGridMap<T> : IDualGridMap<T, ValueGridShape4Bits, ValueGridShape4Bits.ReadOnly>, IEquatable<CompactDualGridMap<T>>
    where T : unmanaged, IEquatable<T>
{
    public ValueGridShape<T> WorldGrid { get; }
    private readonly List<ValueGridShape4Bits> _visualLayers = new();
    private readonly Dictionary<T, int> _worldValueVisualLayersMap = new();

    public int Width => WorldGrid.Width;
    public int Height => WorldGrid.Height;

    public bool IsOccupied(int x, int y) => WorldGrid.IsOccupied(x, y);

    public T this[int x, int y]
    {
        get => WorldGrid[x, y];
        set => this.SetWorldValue<CompactDualGridMap<T>, T, ValueGridShape4Bits, ValueGridShape4Bits.ReadOnly>(x, y, value);
    }

    public CompactDualGridMap(int width, int height, T defaultValue = default)
    {
        WorldGrid = new ValueGridShape<T>(width, height, defaultValue);
        var visualLayer = GetOrCreateVisualLayer(defaultValue);
        // 4-bit storage packs two cells per byte; fill both nibbles with 0x0F
        visualLayer.Values.Fill(0xFF);
    }

    public ValueGridShape4Bits.ReadOnly GetVisualLayer(T value)
    {
        return GetOrCreateVisualLayer(value);
    }

    ValueGridShape4Bits IDualGridMap<T, ValueGridShape4Bits, ValueGridShape4Bits.ReadOnly>.GetOrCreateVisualLayer(T value)
    {
        return GetOrCreateVisualLayer(value);
    }

    public ValueGridShape4Bits GetOrCreateVisualLayer(T value)
    {
        if (!_worldValueVisualLayersMap.TryGetValue(value, out var layerIndex))
        {
            var grid = new ValueGridShape4Bits(WorldGrid.Width + 1, WorldGrid.Height + 1);
            layerIndex = _visualLayers.Count;
            _visualLayers.Add(grid);
            _worldValueVisualLayersMap.Add(value, layerIndex);
        }
        return _visualLayers[layerIndex];
    }

    public bool Equals(CompactDualGridMap<T> other) => throw new NotSupportedException();
    public override bool Equals(object? obj) => throw new NotSupportedException();
    public override int GetHashCode() => throw new NotSupportedException();
    public static bool operator ==(CompactDualGridMap<T> left, CompactDualGridMap<T> right) => left.Equals(right);
    public static bool operator !=(CompactDualGridMap<T> left, CompactDualGridMap<T> right) => !(left == right);
}
