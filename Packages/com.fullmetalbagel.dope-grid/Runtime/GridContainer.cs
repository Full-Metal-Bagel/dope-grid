using System;
using Unity.Collections;
using Unity.Mathematics;

namespace DopeGrid;

public struct GridContainer : IDisposable
{
    private GridShape2D _grid;
    public GridShape2D.ReadOnly CurrentGrid => _grid.AsReadOnly();

    private GridShape2D _initializedGrid;
    public GridShape2D.ReadOnly InitializedGrid => _initializedGrid.AsReadOnly();

    private NativeList<GridShape2D> _itemShapes;
    public NativeArray<GridShape2D>.ReadOnly ItemShapes => _itemShapes.AsReadOnly();

    private NativeList<int2> _itemPositions; // top-left position
    public NativeArray<int2>.ReadOnly ItemPositions => _itemPositions.AsReadOnly();

    public int Width => _initializedGrid.Width;
    public int Height => _initializedGrid.Height;

    public GridContainer(int width, int height, Allocator allocator = Allocator.Persistent)
    {
        _grid = new GridShape2D(width, height, allocator);
        _initializedGrid = _grid.Clone(allocator);
        _itemShapes = new NativeList<GridShape2D>(allocator);
        _itemPositions = new NativeList<int2>(allocator);
    }

    public GridContainer(GridShape2D containerShape, Allocator allocator = Allocator.Persistent)
    {
        _grid = containerShape.Clone(allocator);
        _initializedGrid = _grid.Clone(allocator);
        _itemShapes = new NativeList<GridShape2D>(allocator);
        _itemPositions = new NativeList<int2>(allocator);
    }

    public int ItemCount => _itemShapes.Length;
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
        _itemShapes.Add(shape);
        _itemPositions.Add(pos);
    }

    public void RemoveItem(int index)
    {
        if (index >= 0 && index < _itemShapes.Length)
        {
            var shape = _itemShapes[index];
            var pos = _itemPositions[index];
            _grid.RemoveItem(shape, pos);

            _itemShapes.RemoveAtSwapBack(index);
            _itemPositions.RemoveAtSwapBack(index);
        }
    }

    public void Clear()
    {
        _initializedGrid.CopyTo(_grid);
        _itemShapes.Clear();
        _itemPositions.Clear();
    }

    public void Dispose()
    {
        _initializedGrid.Dispose();
        _grid.Dispose();
        foreach (var shape in _itemShapes)
            shape.Dispose();
        _itemShapes.Dispose();
        _itemPositions.Dispose();
    }

    public GridContainer Clone(Allocator allocator)
    {
        var clone = new GridContainer
        {
            _initializedGrid = _initializedGrid.Clone(allocator),
            _grid = _grid.Clone(allocator),
            _itemShapes = new NativeList<GridShape2D>(_itemShapes.Capacity, allocator),
            _itemPositions = new NativeList<int2>(_itemPositions.Capacity, allocator)
        };

        for (var i = 0; i < _itemShapes.Length; i++)
        {
            clone._itemShapes.Add(_itemShapes[i].Clone(allocator));
            clone._itemPositions.Add(_itemPositions[i]);
        }

        return clone;
    }
}
