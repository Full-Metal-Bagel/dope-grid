using System;
using System.Collections.Generic;

namespace DopeGrid.Standard;

public struct GridBoard : IDisposable
{
    private GridShape _grid;
    public GridShape.ReadOnly CurrentGrid => _grid;

    private GridShape _initializedGrid;
    public GridShape.ReadOnly InitializedGrid => _initializedGrid;

    private readonly List<ItemSlot> _items; // Stable array - indices never change
    private readonly Stack<int> _freeIndices; // Stack of freed item indices
    private int _itemCount;

    public int Width => _initializedGrid.Width;
    public int Height => _initializedGrid.Height;

    public int ItemCount => _itemCount;
    public int FreeSpace => _grid.FreeSpaceCount;
    public bool IsCreated => _grid.IsCreated;

    private struct ItemSlot
    {
        public ImmutableGridShape Shape;
        public GridPosition Position;
        public bool IsValid;

        public ItemSlot(ImmutableGridShape shape, GridPosition position)
        {
            Shape = shape;
            Position = position;
            IsValid = true;
        }

        public static readonly ItemSlot Invalid = new() { IsValid = false };
    }

    public GridBoard(int width, int height)
    {
        _grid = new GridShape(width, height);
        _initializedGrid = _grid.Clone();
        _items = new();
        _freeIndices = new();
        _itemCount = 0;
    }

    public GridBoard(GridShape containerShape)
    {
        if (containerShape.IsEmpty) throw new ArgumentException(nameof(containerShape));
        _grid = containerShape.Clone();
        _initializedGrid = _grid.Clone();
        _items = new();
        _freeIndices = new();
        _itemCount = 0;
    }

    public ImmutableGridShape GetItemShape(int index)
    {
        if (index < 0 || index >= _items.Count || !_items[index].IsValid)
            return ImmutableGridShape.Empty;
        return _items[index].Shape;
    }

    public GridPosition GetItemPosition(int index)
    {
        if (index < 0 || index >= _items.Count || !_items[index].IsValid)
            return GridPosition.Invalid;
        return _items[index].Position;
    }

    public bool IsCellOccupied(GridPosition pos)
    {
        return _grid.GetCellValue(pos);
    }

    public (int index, RotationDegree rotation) TryAddItem(ImmutableGridShape item)
    {
        var (pos, rotation) = _grid.FindFirstFitWithFreeRotation(item, freeValue: false);
        if (pos.IsValid)
        {
            var index = AddItemAt(item.GetRotatedShape(rotation), pos);
            return (index, rotation);
        }

        return (-1, RotationDegree.None);
    }

    public int TryAddItemAt(ImmutableGridShape shape, GridPosition pos)
    {
        if (_grid.CanPlaceItem(shape, pos, freeValue: false))
        {
            return AddItemAt(shape, pos);
        }

        return -1;
    }

    private int AddItemAt(ImmutableGridShape shape, GridPosition pos)
    {
        // Allocate stable index
        int itemIndex;
        if (_freeIndices.Count > 0)
        {
            itemIndex = _freeIndices.Pop();
        }
        else
        {
            itemIndex = _items.Count;
            _items.Add(ItemSlot.Invalid); // Placeholder
        }

        _grid.PlaceItem(shape, pos, value: true);
        _items[itemIndex] = new ItemSlot(shape, pos);
        _itemCount++;

        return itemIndex;
    }

    public void RemoveItem(int index)
    {
        if (index >= 0 && index < _items.Count && _items[index].IsValid)
        {
            var slot = _items[index];
            _grid.RemoveItem(slot.Shape, slot.Position, freeValue: false);

            // Mark slot as free and add to free list
            _items[index] = ItemSlot.Invalid;
            _freeIndices.Push(index);
            _itemCount--;
        }
    }

    public void Clear()
    {
        InitializedGrid.CopyTo(_grid);
        _items.Clear();
        _freeIndices.Clear();
        _itemCount = 0;
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
        foreach (var freeIndex in _freeIndices)
        {
            clone._freeIndices.Push(freeIndex);
        }
        clone._itemCount = _itemCount;
        return clone;
    }
}
