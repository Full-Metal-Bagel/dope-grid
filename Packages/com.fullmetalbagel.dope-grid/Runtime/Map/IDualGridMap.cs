using System;

namespace DopeGrid.Map;

public interface IDualGridMap<T, out TVisualLayerGrid> : IGridShape<T>, IReadOnlyGridShape<T>
    where T : unmanaged, IEquatable<T>
    where TVisualLayerGrid : IGridShape<byte>
{
    ValueGridShape<T> WorldGrid { get; }
    TVisualLayerGrid GetOrCreateVisualLayer(T value);
}

public static class DualGridMapExtension
{
    public static void SetWorldValue<T, TValue, TVisualLayerGrid>(this T map, int x, int y, TValue value, TVisualLayerGrid _ = default!)
        where T : IDualGridMap<TValue, TVisualLayerGrid>
        where TValue : unmanaged, IEquatable<TValue>
        where TVisualLayerGrid : IGridShape<byte>
    {
        var world = map.WorldGrid;
        var old = world[x, y];
        if (old.Equals(value)) return;

        world[x, y] = value;

        // Recompute the four vertices around the changed cell for both old and new values
        // Vertices: (x, y), (x+1, y), (x, y+1), (x+1, y+1)
        int bitIndex = 0;
        for (var dy = 0; dy <= 1; dy++)
        for (var dx = 0; dx <= 1; dx++)
        {
            var vx = x + dx;
            var vy = y + dy;
            FlipLayerValueBit(map, old, vx, vy, bitIndex);
            FlipLayerValueBit(map, value, vx, vy, bitIndex);
            bitIndex++;
        }
        return;

        static void FlipLayerValueBit(T map, TValue value, int x, int y, int bitIndex)
        {
            var layer = map.GetOrCreateVisualLayer(value);
            byte layerValue = layer[x, y];
            layer[x, y] = (byte)(layerValue ^ (byte)(1 << bitIndex));
        }
    }
}
