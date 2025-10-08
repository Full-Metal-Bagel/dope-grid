using System;
using System.Collections.Generic;

namespace DopeGrid.Map;

// Visual layer bit mapping (per vertex)
// ------------------------------------
// WorldGrid is W x H (cell-based). Each visual layer is a (W+1) x (H+1) vertex grid.
// Each vertex stores a 4-bit nibble describing which of the four adjacent world cells
// match that layer's world value. Bits are updated via XOR when a world cell changes.
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
// - Non-default layers start at 0x00.
//
// Note: DualGridMap uses ValueGridShape<byte>. Only the lower nibble is meaningful;
// the upper nibble is ignored.
public readonly struct DualGridMap<T> : IDualGridMap<T, ValueGridShape<byte>, ValueGridShape<byte>.ReadOnly>, IEquatable<DualGridMap<T>>, IDisposable
    where T : unmanaged, IEquatable<T>
{
    public ValueGridShape<T> WorldGrid { get; }
    private readonly List<ValueGridShape<byte>> _visualLayers;
    private readonly Dictionary<T, int> _worldValueVisualLayersMap;

    public int Width => WorldGrid.Width;
    public int Height => WorldGrid.Height;

    public bool IsOccupied(int x, int y) => WorldGrid.IsOccupied(x, y);

    public T this[int x, int y]
    {
        get => WorldGrid[x, y];
        set => this.SetWorldValue<DualGridMap<T>, T, ValueGridShape<byte>, ValueGridShape<byte>.ReadOnly>(x, y, value);
    }

    public DualGridMap(int width, int height, T defaultValue = default)
    {
        WorldGrid = new ValueGridShape<T>(width, height, defaultValue);
        _visualLayers = ListPool<ValueGridShape<byte>>.Rent();
        _worldValueVisualLayersMap = DictionaryPool<T, int>.Rent();
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

    ValueGridShape<byte> IDualGridMap<T, ValueGridShape<byte>, ValueGridShape<byte>.ReadOnly>.GetOrCreateVisualLayer(T value)
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
        ListPool<ValueGridShape<byte>>.Return(_visualLayers);
        DictionaryPool<T, int>.Return(_worldValueVisualLayersMap);
    }
}
