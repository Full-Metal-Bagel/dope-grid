using System;
using System.Collections.Generic;

namespace DopeGrid.Map;

// Visual layer bit mapping (per vertex) — compact 4-bit storage
// -------------------------------------------------------------
// WorldGrid is W x H (cell-based). Each visual layer is a (W+1) x (H+1) vertex grid.
// Each vertex stores a 4-bit nibble packed via ValueGridShape4Bits, using the same
// nibble semantics as DualGridMap:
//
// Bit layout for a vertex at (vx, vy):
//   bit 0 (1): BR => world cell (vx,   vy  )
//   bit 1 (2): BL => world cell (vx-1, vy  )
//   bit 2 (4): TR => world cell (vx,   vy-1)
//   bit 3 (8): TL => world cell (vx-1, vy-1)
//
// Visual (vertex-centred). The star (*) is the vertex (vx, vy),
// and each quadrant shows the bit index within the nibble:
//
//     x = vx-1     x = vx
//   +-----------+-----------+
// y |   bit 3   |   bit 2   | y = vy-1
// = |    TL     |    TR     |
// v +-----*-----+-----*-----+
// y |   bit 1   |   bit 0   | y = vy
//   |    BL     |    BR     |
//   +-----------+-----------+
//
// Update rule (sequential mapping): when world cell (x, y) changes from old->new,
// we recompute the 4 vertices around it in this order and XOR the corresponding bit:
//   (x, y)     -> bit 0 (BR)
//   (x+1, y)   -> bit 1 (BL)
//   (x, y+1)   -> bit 2 (TR)
//   (x+1, y+1) -> bit 3 (TL)
//
// Visual (cell-centred). For a changed world cell (x, y), the four affected
// vertices and their bit indices (in this order) are:
//
//   (x, y) [0]       (x+1, y) [1]
//        *-----------*
//        |    cell   |
//        |   (x,y)   |
//        *-----------*
//   (x, y+1) [2]   (x+1, y+1) [3]
//
// Initialization:
// - For the default world value's layer, all vertices start at 0x0F (all four bits set).
//   Since storage is 4-bit packed, we fill bytes with 0xFF to set both nibbles to 0xF.
// - Non-default layers start at 0x0 (all bits cleared).
public readonly struct CompactDualGridMap<T> : IDualGridMap<T, ValueGridShape4Bits, ValueGridShape4Bits.ReadOnly>, IEquatable<CompactDualGridMap<T>>, IDisposable
    where T : unmanaged, IEquatable<T>
{
    public ValueGridShape<T> WorldGrid { get; }
    private readonly List<ValueGridShape4Bits> _visualLayers;
    private readonly Dictionary<T, int> _worldValueVisualLayersMap;

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
        _visualLayers = ListPool<ValueGridShape4Bits>.Rent();
        _worldValueVisualLayersMap = DictionaryPool<T, int>.Rent();
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

    private ValueGridShape4Bits GetOrCreateVisualLayer(T value)
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

    public void Dispose()
    {
        WorldGrid.Dispose();
        foreach (var layer in _visualLayers) layer.Dispose();
        _visualLayers.Clear();
        _worldValueVisualLayersMap.Clear();
        ListPool<ValueGridShape4Bits>.Return(_visualLayers);
        DictionaryPool<T, int>.Return(_worldValueVisualLayersMap);
    }
}
