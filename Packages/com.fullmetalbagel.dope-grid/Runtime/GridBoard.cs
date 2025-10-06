using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DopeGrid;

[Obsolete("should be replace by a generic type which easier to be extended to other forms like indexed one")]
public readonly struct GridBoard : IDisposable, IEquatable<GridBoard>
{
    private readonly GridShape _grid;
    public GridShape.ReadOnly CurrentGrid => _grid;

    private readonly GridShape _initializedGrid;
    public GridShape.ReadOnly InitializedGrid => _initializedGrid;

    private readonly List<ImmutableGridShape> _items;
    public IReadOnlyList<ImmutableGridShape> Items => _items;

    private readonly List<(int x, int y)> _itemPositions; // top-left position
    public IReadOnlyList<(int x, int y)> ItemPositions => _itemPositions;

    public int Width => _initializedGrid.Width;
    public int Height => _initializedGrid.Height;

    public bool this[int x, int y] => _grid[x, y];
    public bool IsOccupied(int x, int y) => _grid.IsOccupied(x, y);

    public GridBoard(int width, int height)
    {
        _grid = new GridShape(width, height);
        _initializedGrid = _grid.Clone();
        _items = ListPool<ImmutableGridShape>.Rent(8);
        _itemPositions = ListPool<(int x, int y)>.Rent(8);
    }

    public GridBoard(GridBoard other)
    {
        _grid = other._grid.Clone();
        _initializedGrid = _grid.Clone();
        _items = ListPool<ImmutableGridShape>.Rent(other._items.Count);
        _items.AddRange(other._items);
        _itemPositions = ListPool<(int x, int y)>.Rent(other._itemPositions.Count);
        _itemPositions.AddRange(other.ItemPositions);
    }

    public GridBoard(GridShape containerShape)
    {
        if (containerShape.IsZeroSize())
            throw new ArgumentException("container shape is empty", nameof(containerShape));
        _grid = containerShape.Clone();
        _initializedGrid = _grid.Clone();
        _items = ListPool<ImmutableGridShape>.Rent(8);
        _itemPositions = ListPool<(int x, int y)>.Rent(8);
    }

    public int ItemCount => _items.Count;

    public bool TryAddItem(ImmutableGridShape item)
    {
        var (x, y) = _grid.FindFirstFitWithFixedRotation(item);
        if (x >= 0)
        {
            AddItemAt(item, x, y);
            return true;
        }

        return false;
    }

    public bool TryAddItemAt(ImmutableGridShape shape, int x, int y)
    {
        if (_grid.CanPlaceItem(shape, x, y))
        {
            AddItemAt(shape, x, y);
            return true;
        }

        return false;
    }

    private void AddItemAt(ImmutableGridShape shape, int x, int y)
    {
        _grid.FillShapeWithValue(shape, x, y, true);
        _items.Add(shape);
        _itemPositions.Add((x, y));
    }

    public void RemoveItem(int index)
    {
        if (index >= 0 && index < _items.Count)
        {
            var shape = _items[index];
            var (x, y) = _itemPositions[index];
            _grid.FillShapeWithValue(shape, x, y, false);

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
        ListPool<ImmutableGridShape>.Return(_items);
        ListPool<(int x, int y)>.Return(_itemPositions);
    }

    public GridBoard Clone()
    {
        return new GridBoard(this);
    }

    public override int GetHashCode() => throw new NotSupportedException("GetHashCode() on GridBoard is not supported.");
    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
    public override bool Equals(object? obj) => throw new NotSupportedException("Equals(object) on GridBoard is not supported.");
    public bool Equals(GridBoard other) => throw new NotSupportedException("Equals(GridBoard) on GridBoard is not supported.");
    public static bool operator ==(GridBoard left, GridBoard right) => left.Equals(right);
    public static bool operator !=(GridBoard left, GridBoard right) => !left.Equals(right);
}
