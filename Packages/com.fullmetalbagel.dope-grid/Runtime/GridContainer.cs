using System;
using Unity.Collections;
using Unity.Mathematics;

namespace DopeGrid;

public struct GridContainer : IDisposable
{
    private GridShape2D _grid;
    public GridShape2D.ReadOnly CurrentGrid => _grid;

    private GridShape2D _initializedGrid;
    public GridShape2D.ReadOnly InitializedGrid => _initializedGrid;

    private NativeList<ImmutableGridShape2D> _items;
    public NativeArray<ImmutableGridShape2D>.ReadOnly Items => _items.AsReadOnly();

    private NativeList<int2> _itemPositions; // top-left position
    public NativeArray<int2>.ReadOnly ItemPositions => _itemPositions.AsReadOnly();

    public int Width => _initializedGrid.Width;
    public int Height => _initializedGrid.Height;

    public GridContainer(int width, int height, Allocator allocator = Allocator.Persistent)
    {
        _grid = new GridShape2D(width, height, allocator);
        _initializedGrid = _grid.Clone(allocator);
        _items = new NativeList<ImmutableGridShape2D>(allocator);
        _itemPositions = new NativeList<int2>(allocator);
    }

    public GridContainer(GridShape2D containerShape, Allocator allocator = Allocator.Persistent)
    {
        if (containerShape.IsEmpty) throw new ArgumentException(nameof(containerShape));
        _grid = containerShape.Clone(allocator);
        _initializedGrid = _grid.Clone(allocator);
        _items = new NativeList<ImmutableGridShape2D>(allocator);
        _itemPositions = new NativeList<int2>(allocator);
    }

    public int ItemCount => _items.Length;
    public int FreeSpace => _grid.FreeSpaceCount;

    public bool IsCellOccupied(int2 pos)
    {
        return _grid.GetCell(pos);
    }

    public bool TryAddItem(ImmutableGridShape2D item)
    {
        var pos = _grid.FindFirstFit(item);
        if (pos.x >= 0)
        {
            AddItemAt(item, pos);
            return true;
        }

        return false;
    }

    public bool TryAddItemAt(ImmutableGridShape2D shape, int2 pos)
    {
        if (_grid.CanPlaceItem(shape, pos))
        {
            AddItemAt(shape, pos);
            return true;
        }

        return false;
    }

    private void AddItemAt(ImmutableGridShape2D shape, int2 pos)
    {
        _grid.PlaceItem(shape, pos);
        _items.Add(shape);
        _itemPositions.Add(pos);
    }

    public void RemoveItem(int index)
    {
        if (index >= 0 && index < _items.Length)
        {
            var shape = _items[index];
            var pos = _itemPositions[index];
            _grid.RemoveItem(shape, pos);

            _items.RemoveAtSwapBack(index);
            _itemPositions.RemoveAtSwapBack(index);
        }
    }

    public void Clear()
    {
        InitializedGrid.CopyTo(_grid);
        _items.Clear();
        _itemPositions.Clear();
    }

    public void Dispose()
    {
        _grid.Dispose();
        _items.Dispose();
        _itemPositions.Dispose();
    }

    public GridContainer Clone(Allocator allocator)
    {
        var clone = new GridContainer
        {
            _initializedGrid = InitializedGrid.Clone(allocator),
            _grid = _grid.Clone(allocator),
            _items = new NativeList<ImmutableGridShape2D>(_items.Capacity, allocator),
            _itemPositions = new NativeList<int2>(_itemPositions.Capacity, allocator)
        };
        clone._items.CopyFrom(_items);
        clone._itemPositions.CopyFrom(_itemPositions);
        return clone;
    }
}
