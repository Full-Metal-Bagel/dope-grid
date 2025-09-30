using System;
using System.Collections.Generic;

namespace DopeGrid.Standard;

public struct GridBoard : IDisposable
{
    private GridShape _grid;
    public GridShape.ReadOnly CurrentGrid => _grid;

    private GridShape _initializedGrid;
    public GridShape.ReadOnly InitializedGrid => _initializedGrid;

    private readonly List<ImmutableGridShape> _items;
    public IReadOnlyList<ImmutableGridShape> Items => _items;

    private readonly List<GridPosition> _itemPositions;
    public IReadOnlyList<GridPosition> ItemPositions => _itemPositions;

    public int Width => _initializedGrid.Width;
    public int Height => _initializedGrid.Height;

    public int ItemCount => _items.Count;
    public int FreeSpace => _grid.FreeSpaceCount;
    public bool IsCreated => _grid.IsCreated;

    public GridBoard(int width, int height)
    {
        _grid = new GridShape(width, height);
        _initializedGrid = _grid.Clone();
        _items = new();
        _itemPositions = new();
    }

    public GridBoard(GridShape containerShape)
    {
        if (containerShape.IsEmpty) throw new ArgumentException(nameof(containerShape));
        _grid = containerShape.Clone();
        _initializedGrid = _grid.Clone();
        _items = new();
        _itemPositions = new();
    }

    public bool IsCellOccupied(GridPosition pos)
    {
        return _grid.GetCell(pos);
    }

    public bool TryAddItem(ImmutableGridShape item)
    {
        var pos = _grid.FindFirstFit(item);
        if (pos.X >= 0)
        {
            AddItemAt(item, pos);
            return true;
        }

        return false;
    }

    public bool TryAddItemAt(ImmutableGridShape shape, GridPosition pos)
    {
        if (_grid.CanPlaceItem(shape, pos))
        {
            AddItemAt(shape, pos);
            return true;
        }

        return false;
    }

    private void AddItemAt(ImmutableGridShape shape, GridPosition pos)
    {
        _grid.PlaceItem(shape, pos);
        _items.Add(shape);
        _itemPositions.Add(pos);
    }

    public void RemoveItem(int index)
    {
        if (index >= 0 && index < _items.Count)
        {
            var shape = _items[index];
            var pos = _itemPositions[index];
            _grid.RemoveItem(shape, pos);

            // Swap with last element for O(1) removal
            var lastIndex = _items.Count - 1;
            if (index != lastIndex)
            {
                _items[index] = _items[lastIndex];
                _itemPositions[index] = _itemPositions[lastIndex];
            }
            _items.RemoveAt(lastIndex);
            _itemPositions.RemoveAt(lastIndex);
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
        _initializedGrid.Dispose();
        _grid.Dispose();
    }

    public GridBoard Clone()
    {
        var clone = new GridBoard(Width, Height);
        _grid.CopyTo(clone._grid);
        _initializedGrid.CopyTo(clone._initializedGrid);
        clone._items.AddRange(_items);
        clone._itemPositions.AddRange(_itemPositions);
        return clone;
    }
}
