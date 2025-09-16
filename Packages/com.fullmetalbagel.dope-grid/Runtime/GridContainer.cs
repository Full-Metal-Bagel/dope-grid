using System;
using Unity.Collections;
using Unity.Mathematics;

namespace DopeGrid;

public struct GridContainer : IDisposable
{
    private GridShape2D _grid;
    private GridShape2D _initializedGrid;
    public GridShape2D.ReadOnly InitializedGrid => _initializedGrid.AsReadOnly();
    public GridShape2D.ReadOnly CurrentGrid => _grid.AsReadOnly();
    public NativeList<GridShape2D> ItemShapes;
    public NativeList<int2> ItemPositions; // top-left position

    public int Width => _initializedGrid.Width;
    public int Height => _initializedGrid.Height;

    public GridContainer(int width, int height, Allocator allocator = Allocator.Persistent)
    {
        _grid = new GridShape2D(width, height, allocator);
        _initializedGrid = _grid.Clone(allocator);
        ItemShapes = new NativeList<GridShape2D>(allocator);
        ItemPositions = new NativeList<int2>(allocator);
    }

    public GridContainer(GridShape2D containerShape, Allocator allocator = Allocator.Persistent)
    {
        _grid = containerShape.Clone(allocator);
        _initializedGrid = _grid.Clone(allocator);
        ItemShapes = new NativeList<GridShape2D>(allocator);
        ItemPositions = new NativeList<int2>(allocator);
    }

    public int ItemCount => ItemShapes.Length;
    public int FreeSpace => _grid.FreeSpaceCount;

    public bool IsCellOccupied(int2 pos)
    {
        return _grid.GetCell(pos);
    }

    public bool TryAddItem(GridShape2D shape)
    {
        var pos = _grid.FindFirstFit(shape);
        if (pos.x >= 0)
        {
            AddItemAt(shape, pos);
            return true;
        }

        return false;
    }

    public bool TryAddItemAt(GridShape2D shape, int2 pos)
    {
        if (_grid.CanPlaceItem(shape, pos))
        {
            AddItemAt(shape, pos);
            return true;
        }

        return false;
    }

    private void AddItemAt(GridShape2D shape, int2 pos)
    {
        _grid.PlaceItem(shape, pos);
        ItemShapes.Add(shape);
        ItemPositions.Add(pos);
    }

    public void RemoveItem(int index)
    {
        if (index >= 0 && index < ItemShapes.Length)
        {
            var shape = ItemShapes[index];
            var pos = ItemPositions[index];
            _grid.RemoveItem(shape, pos);

            ItemShapes.RemoveAtSwapBack(index);
            ItemPositions.RemoveAtSwapBack(index);
        }
    }

    public void Clear()
    {
        _initializedGrid.CopyTo(_grid);
        ItemShapes.Clear();
        ItemPositions.Clear();
    }

    public void Dispose()
    {
        _initializedGrid.Dispose();
        _grid.Dispose();
        foreach (var shape in ItemShapes)
            shape.Dispose();
        ItemShapes.Dispose();
        ItemPositions.Dispose();
    }

    public GridContainer Clone(Allocator allocator)
    {
        var clone = new GridContainer
        {
            _initializedGrid = _initializedGrid.Clone(allocator),
            _grid = _grid.Clone(allocator),
            ItemShapes = new NativeList<GridShape2D>(ItemShapes.Capacity, allocator),
            ItemPositions = new NativeList<int2>(ItemPositions.Capacity, allocator)
        };

        for (var i = 0; i < ItemShapes.Length; i++)
        {
            clone.ItemShapes.Add(ItemShapes[i].Clone(allocator));
            clone.ItemPositions.Add(ItemPositions[i]);
        }

        return clone;
    }
}
