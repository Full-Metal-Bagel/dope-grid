using System;
using Unity.Collections;

namespace DopeGrid.Native;

public struct GridBoard : IDisposable
{
    private GridShape _grid;
    public GridShape.ReadOnly CurrentGrid => _grid;

    private GridShape _initializedGrid;
    public GridShape.ReadOnly InitializedGrid => _initializedGrid;

    private NativeList<ItemSlot> _items; // Stable array - indices never change
    private NativeList<int> _freeIndices; // Stack of freed item indices
    private int _itemCount;

    public int Width => _initializedGrid.Width;
    public int Height => _initializedGrid.Height;

    public int ItemCount => _itemCount;
    public int FreeSpace => _grid.FreeSpaceCount;

    private readonly record struct ItemSlot(ImmutableGridShape Shape, GridPosition Position)
    {
        public static readonly ItemSlot Invalid = new(ImmutableGridShape.Empty, GridPosition.Invalid);
        public ImmutableGridShape Shape { get; } = Shape;
        public GridPosition Position { get; } = Position;
        public bool IsValid => Position.IsValid || !Shape.IsEmpty;
    }

    public GridBoard(int width, int height, Allocator allocator = Allocator.Persistent)
    {
        _grid = new GridShape(width, height, allocator);
        _initializedGrid = _grid.Clone(allocator);
        _items = new NativeList<ItemSlot>(allocator);
        _freeIndices = new NativeList<int>(allocator);
        _itemCount = 0;
    }

    public GridBoard(GridShape containerShape, Allocator allocator = Allocator.Persistent)
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
        if (index < 0 || index >= _items.Length || !_items[index].IsValid)
            return ImmutableGridShape.Empty;
        return _items[index].Shape;
    }

    public GridPosition GetItemPosition(int index)
    {
        if (index < 0 || index >= _items.Length || !_items[index].IsValid)
            return GridPosition.Invalid;
        return _items[index].Position;
    }

    public bool IsCellOccupied(GridPosition pos)
    {
        return _grid.GetCell(pos);
    }

    public int TryAddItem(ImmutableGridShape item)
    {
        var pos = _grid.FindFirstFit(item);
        if (pos.X >= 0)
        {
            return AddItemAt(item, pos);
        }

        return -1;
    }

    public int TryAddItemAt(ImmutableGridShape shape, GridPosition pos)
    {
        if (_grid.CanPlaceItem(shape, pos))
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

        _grid.PlaceItem(shape, pos);
        _items[itemIndex] = new ItemSlot(shape, pos);
        _itemCount++;

        return itemIndex;
    }

    public void RemoveItem(int index)
    {
        if (index >= 0 && index < _items.Length && _items[index].IsValid)
        {
            var slot = _items[index];
            _grid.RemoveItem(slot.Shape, slot.Position);

            // Mark slot as free and add to free list
            _items[index] = ItemSlot.Invalid;
            _freeIndices.Add(index);
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
        _items.Dispose();
        _freeIndices.Dispose();
    }

    public GridBoard Clone(Allocator allocator)
    {
        var clone = new GridBoard
        {
            _initializedGrid = InitializedGrid.Clone(allocator),
            _grid = _grid.Clone(allocator),
            _items = new NativeList<ItemSlot>(_items.Capacity, allocator),
            _freeIndices = new NativeList<int>(_freeIndices.Capacity, allocator),
            _itemCount = _itemCount
        };
        clone._items.CopyFrom(_items);
        clone._freeIndices.CopyFrom(_freeIndices);
        return clone;
    }
}
