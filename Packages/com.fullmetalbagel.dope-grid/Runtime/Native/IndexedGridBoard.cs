using System;
using Unity.Collections;

namespace DopeGrid.Native;

public struct IndexedGridBoard : IDisposable
{
    private ValueGridShape<int> _grid; // Stores item index at each cell (-1 for empty)
    public readonly ValueGridShape<int>.ReadOnly Grid => _grid;

    private ValueGridShape<int> _initializedGrid;
    public readonly ValueGridShape<int>.ReadOnly InitializedGrid => _initializedGrid;

    private NativeList<ItemSlot> _items; // Stable array - indices never change
    private NativeList<int> _freeIndices; // Stack of freed item indices
    private int _itemCount;

    public readonly int Width => _initializedGrid.Width;
    public readonly int Height => _initializedGrid.Height;

    public readonly int ItemCount => _itemCount;
    public readonly int FreeSpace => _grid.CountValue(-1);

    public IndexedGridBoard(int width, int height, Allocator allocator = Allocator.Persistent)
    {
        _grid = new ValueGridShape<int>(width, height, -1, allocator);
        _initializedGrid = _grid.Clone(allocator);
        _items = new NativeList<ItemSlot>(allocator);
        _freeIndices = new NativeList<int>(allocator);
        _itemCount = 0;
    }

    public IndexedGridBoard(ValueGridShape<int> containerShape, Allocator allocator = Allocator.Persistent)
    {
        if (containerShape.IsEmpty) throw new ArgumentException(nameof(containerShape));
        _grid = containerShape.Clone(allocator);
        _initializedGrid = _grid.Clone(allocator);
        _items = new NativeList<ItemSlot>(allocator);
        _freeIndices = new NativeList<int>(allocator);
        _itemCount = 0;
    }

    public ImmutableGridShape GetItemShape(int index)
    {
        if (index < 0 || index >= _items.Length)
            return ImmutableGridShape.Empty;
        return _items[index].Shape;
    }

    public GridPosition GetItemPosition(int index)
    {
        if (index < 0 || index >= _items.Length)
            return GridPosition.Invalid;
        return _items[index].Position;
    }

    public int GetItemIndexAt(GridPosition pos)
    {
        if (!_grid.Contains(pos)) return -1;
        return _grid[pos];
    }

    public bool IsCellOccupied(GridPosition pos)
    {
        return GetItemIndexAt(pos) >= 0;
    }

    public (int index, RotationDegree rotation) TryAddItem(ImmutableGridShape item)
    {
        // Try all rotations to find first fit
        var rotateCount = 0;
        var rotatedItem = item;
        GridPosition position;
        do
        {
            position = _grid.FindFirstFitWithFixedRotation(rotatedItem);
            if (position.IsValid)
            {
                var index = AddItemAt(rotatedItem, position);
                var rotation = rotateCount switch
                {
                    0 => RotationDegree.None,
                    1 => RotationDegree.Clockwise90,
                    2 => RotationDegree.Clockwise180,
                    3 => RotationDegree.Clockwise270,
                    _ => throw new NotImplementedException()
                };
                return (index, rotation);
            }
            rotatedItem = rotatedItem.Rotate90();
            rotateCount++;
        }
        while (rotatedItem != item);

        return (-1, RotationDegree.None);
    }

    public int TryAddItemAt(ImmutableGridShape shape, GridPosition pos)
    {
        if (CanPlaceItem(shape, pos))
        {
            return AddItemAt(shape, pos);
        }

        return -1;
    }

    private int AddItemAt(ImmutableGridShape shape, GridPosition pos)
    {
        // Allocate stable index
        int itemIndex;
        if (_freeIndices.Length > 0)
        {
            itemIndex = _freeIndices[^1];
            _freeIndices.Length--;
        }
        else
        {
            itemIndex = _items.Length;
            _items.Add(ItemSlot.Invalid); // Placeholder
        }

        // Place item on grid with its index
        _grid.FillShape(shape, pos, itemIndex);
        _items[itemIndex] = new ItemSlot(shape, pos);
        _itemCount++;

        return itemIndex;
    }

    public void RemoveItem(int index)
    {
        if (index >= 0 && index < _items.Length && _items[index].IsValid)
        {
            var slot = _items[index];

            // Clear item from grid (set to -1)
            _grid.FillShape(slot.Shape, slot.Position, -1);

            // Mark slot as free and add to free list
            _items[index] = ItemSlot.Invalid;
            _freeIndices.Add(index);
            _itemCount--;
        }
    }

    public void Clear()
    {
        _initializedGrid.CopyTo(_grid);
        _items.Clear();
        _freeIndices.Clear();
        _itemCount = 0;
    }

    public void Dispose()
    {
        _initializedGrid.Dispose();
        _grid.Dispose();
        _items.Dispose();
        _freeIndices.Dispose();
    }

    public IndexedGridBoard Clone(Allocator allocator)
    {
        var clone = new IndexedGridBoard
        {
            _initializedGrid = _initializedGrid.Clone(allocator),
            _grid = _grid.Clone(allocator),
            _items = new NativeList<ItemSlot>(_items.Capacity, allocator),
            _freeIndices = new NativeList<int>(_freeIndices.Capacity, allocator),
            _itemCount = _itemCount
        };
        clone._items.CopyFrom(_items);
        clone._freeIndices.CopyFrom(_freeIndices);
        return clone;
    }

    private bool CanPlaceItem(ImmutableGridShape item, GridPosition pos)
    {
        var grid = _grid.AsReadOnly();
        if (!grid.IsWithinBounds(item, pos))
            return false;

        return grid.CheckShapeCells(item, pos, (_, value) => value == -1);
    }

    public static implicit operator ReadOnly(IndexedGridBoard board) => board.AsReadOnly();
    public ReadOnly AsReadOnly() => new(_grid.AsReadOnly(), _items.AsReadOnly(), _itemCount);

    public readonly ref struct ReadOnly
    {
        private readonly ValueGridShape<int>.ReadOnly _grid;
        private readonly NativeArray<ItemSlot>.ReadOnly _items;
        private readonly int _itemCount;

        public ValueGridShape<int>.ReadOnly Grid => _grid;
        public int Width => _grid.Width;
        public int Height => _grid.Height;
        public int ItemCount => _itemCount;
        public int FreeSpace => _grid.CountValue(-1);

        internal ReadOnly(ValueGridShape<int>.ReadOnly grid, NativeArray<ItemSlot>.ReadOnly items, int itemCount)
        {
            _grid = grid;
            _items = items;
            _itemCount = itemCount;
        }

        public ImmutableGridShape GetItemShape(int index)
        {
            if (index < 0 || index >= _items.Length)
                return ImmutableGridShape.Empty;
            return _items[index].Shape;
        }

        public GridPosition GetItemPosition(int index)
        {
            if (index < 0 || index >= _items.Length)
                return GridPosition.Invalid;
            return _items[index].Position;
        }

        public int GetItemIndexAt(GridPosition pos)
        {
            if (!_grid.Contains(pos)) return -1;
            return _grid[pos];
        }

        public bool IsCellOccupied(GridPosition pos)
        {
            return GetItemIndexAt(pos) >= 0;
        }
    }
}
