using System;

namespace DopeGrid.Map;

public interface IDualGridMap<T, out TVisualLayerGrid, out TReadOnlyVisualLayerGrid> : IGridShape<T>, IReadOnlyGridShape<T>
    where T : unmanaged, IEquatable<T>
    where TVisualLayerGrid : IGridShape<byte>
    where TReadOnlyVisualLayerGrid : IReadOnlyGridShape<byte>
{
    ValueGridShape<T> WorldGrid { get; }
    TVisualLayerGrid GetOrCreateVisualLayer(T value);
    TReadOnlyVisualLayerGrid GetVisualLayer(T value);
}

public static class DualGridMapExtension
{
    public static void SetWorldValue<T, TValue, TVisualLayerGrid, TReadOnlyVisualLayerGrid>(this T map, int x, int y, TValue value, TVisualLayerGrid _ = default!)
        where T : IDualGridMap<TValue, TVisualLayerGrid, TReadOnlyVisualLayerGrid>
        where TValue : unmanaged, IEquatable<TValue>
        where TVisualLayerGrid : IGridShape<byte>
        where TReadOnlyVisualLayerGrid : IReadOnlyGridShape<byte>
    {
        var world = map.WorldGrid;
        var old = world[x, y];
        if (old.Equals(value)) return;

        world[x, y] = value;

        // Recompute the four vertices around the changed cell for both old and new values
        // Vertices: (x, y), (x+1, y), (x, y+1), (x+1, y+1)
        for (var dy = 0; dy <= 1; dy++)
        for (var dx = 0; dx <= 1; dx++)
        {
            var vx = x + dx;
            var vy = y + dy;
            // Bit layout around vertex (vx, vy):
            // 0: top-left (vx-1, vy-1)
            // 1: top-right (vx,   vy-1)
            // 2: bottom-left (vx-1, vy  )
            // 3: bottom-right (vx,   vy  )
            // For the changed cell at (x, y), relative to vertex (x+dx, y+dy),
            // the corresponding bit index is:
            int bitIndex = (1 - dy) * 2 + (1 - dx);
            FlipLayerValueBit(map, old, vx, vy, bitIndex);
            FlipLayerValueBit(map, value, vx, vy, bitIndex);
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
