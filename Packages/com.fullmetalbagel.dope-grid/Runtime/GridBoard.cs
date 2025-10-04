using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DopeGrid;

public readonly struct GridBoard : IReadOnlyGridShape<bool>, IDisposable, IEquatable<GridBoard>
{
    private readonly GridShape _grid;
    public GridShape.ReadOnly CurrentGrid => _grid;

    private readonly GridShape _initializedGrid;
    public GridShape.ReadOnly InitializedGrid => _initializedGrid;

    private readonly List<ImmutableGridShape> _items;
    public IReadOnlyList<ImmutableGridShape> Items => _items;

    private readonly List<GridPosition> _itemPositions; // top-left position
    public IReadOnlyList<GridPosition> ItemPositions => _itemPositions;

    public int Width => _initializedGrid.Width;
    public int Height => _initializedGrid.Height;

    public bool this[int x, int y] => _grid[x, y];
    public bool IsOccupied(int x, int y) => _grid.IsOccupied(x, y);

    public GridBoard(int width, int height)
    {
        _grid = new GridShape(width, height);
        _initializedGrid = _grid.Clone();
        _items = new List<ImmutableGridShape>(capacity: 8);
        _itemPositions = new List<GridPosition>(capacity: 8);
    }

    public GridBoard(GridBoard other)
    {
        _grid = other._grid.Clone();
        _initializedGrid = _grid.Clone();
        _items = new List<ImmutableGridShape>(other._items.Capacity);
        _items.AddRange(other._items);
        _itemPositions = new List<GridPosition>(other._itemPositions.Capacity);
        _itemPositions.AddRange(other.ItemPositions);
    }

    public GridBoard(GridShape containerShape)
    {
        if (containerShape.IsEmpty())
            throw new ArgumentException("container shape is empty", nameof(containerShape));
        _grid = containerShape.Clone();
        _initializedGrid = _grid.Clone();
        _items = new List<ImmutableGridShape>(capacity: 8);
        _itemPositions = new List<GridPosition>(capacity: 8);
    }

    public int ItemCount => _items.Count;

    public bool TryAddItem(ImmutableGridShape item)
    {
        var pos = _grid.FindFirstFitWithFixedRotation(item);
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
        _grid.FillShapeWithValue(shape, pos, true);
        _items.Add(shape);
        _itemPositions.Add(pos);
    }

    public void RemoveItem(int index)
    {
        if (index >= 0 && index < _items.Count)
        {
            var shape = _items[index];
            var pos = _itemPositions[index];
            _grid.FillShapeWithValue(shape, pos, false);

            // swap-back remove
            var lastIdx = _items.Count - 1;
            if (index != lastIdx)
            {
                _items[index] = _items[lastIdx];
                _itemPositions[index] = _itemPositions[lastIdx];
            }
            _items.RemoveAt(lastIdx);
            _itemPositions.RemoveAt(lastIdx);
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
        // Lists are managed; nothing to dispose.
    }

    public GridBoard Clone()
    {
        return new GridBoard(this);
    }

    public override int GetHashCode() => throw new NotSupportedException("GetHashCode() on GridShape and GridShape.ReadOnly is not supported.");
    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
    public override bool Equals(object? obj) => throw new NotSupportedException("Equals(object) on GridShape and GridShape.ReadOnly is not supported.");
    public bool Equals(GridBoard other) => throw new NotSupportedException("Equals(object) on GridShape and GridShape.ReadOnly is not supported.");
    public static bool operator ==(GridBoard left, GridBoard right) => left.Equals(right);
    public static bool operator !=(GridBoard left, GridBoard right) => !left.Equals(right);
}
