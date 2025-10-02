using System.Diagnostics.CodeAnalysis;
using DopeGrid.Native;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace DopeGrid.Inventory;

public struct Inventory : INativeDisposable
{
    private ValueGridShape<int> _grid;
    public ValueGridShape<int> Grid => _grid;

    private NativeList<InventoryItem> _items; // Stable array - indices never change
    private NativeList<int> _freeIndices; // Stack of freed item indices
    private int _itemCount;

    public readonly int Width => _grid.Width;
    public readonly int Height => _grid.Height;
    public readonly int ItemCount => _itemCount;
    public readonly bool IsEmpty => _itemCount == 0;
    public readonly bool IsCreated => _items.IsCreated;
    public readonly int FreeSpaceCount => ((ReadOnly)this).FreeSpaceCount;
    public readonly int OccupiedSpaceCount => ((ReadOnly)this).OccupiedSpaceCount;

    public Inventory(int width, int height, Allocator allocator)
    {
        _grid = new ValueGridShape<int>(width, height, -1, allocator); // -1 means empty cell
        _items = new NativeList<InventoryItem>(width * height, allocator);
        _freeIndices = new NativeList<int>(width * height, allocator);
        _itemCount = 0;
    }

    public readonly InventoryItem this[int index]
    {
        get
        {
            if (index < 0 || index >= _items.Length || !_items[index].IsValid)
                return InventoryItem.Invalid;
            return _items[index];
        }
    }

    public readonly bool IsSame(in Inventory other) => ((ReadOnly)this).IsSame(other);
    public readonly InventoryItem GetItemAt(GridPosition position) => ((ReadOnly)this).GetItemAt(position);
    public readonly bool IsPositionOccupied(GridPosition position) => ((ReadOnly)this).IsPositionOccupied(position);
    public readonly bool CanPlaceShapeAt(ImmutableGridShape shape, GridPosition position) => ((ReadOnly)this).CanPlaceItemAt(shape, position);
    public readonly bool ContainsItem(InventoryItemInstanceId id) => id.Id < (ulong)_items.Length && _items[(int)id.Id].IsValid;
    public readonly GridPosition FindFirstFitPosition(ImmutableGridShape shape) => ((ReadOnly)this).FindFirstFitPosition(shape);
    public readonly bool CanMoveItem(InventoryItemInstanceId id, ImmutableGridShape shape, GridPosition newPosition) => ((ReadOnly)this).CanMoveItem(id, shape, newPosition);

    public void Clear()
    {
        _grid.Clear();
        _grid.Fill(-1);
        _items.Clear();
        _freeIndices.Clear();
        _itemCount = 0;
    }

    public InventoryItem TryAutoPlaceItem(ItemDefinition itemDefinition)
    {
        for (var rotation = RotationDegree.None; rotation <= RotationDegree.Clockwise270; rotation++)
        {
            var shape = itemDefinition.Shape.GetRotatedShape(rotation);
            var position = ((ReadOnly)this).FindFirstFitPosition(shape);
            if (position.IsValid)
            {
                return PlaceItem(itemDefinition, rotation, position);
            }
        }
        return InventoryItem.Invalid;
    }

    public InventoryItem TryPlaceItem(ItemDefinition itemDefinition, RotationDegree rotation, GridPosition position)
    {
        var shape = itemDefinition.Shape.GetRotatedShape(rotation);
        if (!CanPlaceShapeAt(shape, position)) return InventoryItem.Invalid;
        return PlaceItem(itemDefinition, rotation, position);
    }

    private InventoryItem PlaceItem(ItemDefinition itemDefinition, RotationDegree rotation, GridPosition position)
    {
        // Allocate stable index
        int itemIndex;
        if (_freeIndices.Length > 0)
        {
            itemIndex = _freeIndices[_freeIndices.Length - 1];
            _freeIndices.Length--;
        }
        else
        {
            itemIndex = _items.Length;
            _items.Add(InventoryItem.Invalid); // Placeholder
        }

        var instanceId = new InventoryItemInstanceId((ulong)itemIndex);
        var item = new InventoryItem(instanceId, itemDefinition, rotation, position);

        _items[itemIndex] = item;
        _itemCount++;
        PlaceItemOnGrid(item.Shape, position, itemIndex);

        return item;
    }

    public bool RemoveItem(InventoryItemInstanceId id)
    {
        var itemIndex = (int)id.Id;
        if (itemIndex < 0 || itemIndex >= _items.Length || !_items[itemIndex].IsValid)
            return false;

        var item = _items[itemIndex];

        // Clear the item from the grid
        RemoveItemFromGrid(item.Shape, item.Position);

        // Mark slot as free and add to free list
        _items[itemIndex] = InventoryItem.Invalid;
        _freeIndices.Add(itemIndex);
        _itemCount--;

        return true;
    }

    public bool TryMoveItem(InventoryItemInstanceId id, GridPosition newPosition)
    {
        var itemIndex = (int)id.Id;
        if (itemIndex < 0 || itemIndex >= _items.Length || !_items[itemIndex].IsValid)
            return false;

        var item = _items[itemIndex];
        return TryMoveItemInternal(itemIndex, item, newPosition, item.Rotation);
    }

    public bool TryMoveItem(InventoryItemInstanceId id, GridPosition newPosition, RotationDegree newRotation)
    {
        var itemIndex = (int)id.Id;
        if (itemIndex < 0 || itemIndex >= _items.Length || !_items[itemIndex].IsValid)
            return false;

        var item = _items[itemIndex];
        return TryMoveItemInternal(itemIndex, item, newPosition, newRotation);
    }

    private bool TryMoveItemInternal(int itemIndex, in InventoryItem item, GridPosition newPosition, RotationDegree newRotation)
    {
        var oldShape = item.Shape;
        var newShape = item.Definition.Shape.GetRotatedShape(newRotation);

        // First check if new position is valid with new rotation (excluding current item's cells)
        if (!CanMoveItem(item.InstanceId, newShape, newPosition))
            return false;

        // Remove from current position
        RemoveItemFromGrid(oldShape, item.Position);

        // Place at new position with new rotation
        PlaceItemOnGrid(newShape, newPosition, itemIndex);

        // Update item with new position and rotation
        var updatedItem = new InventoryItem(item.InstanceId, item.Definition, newRotation, newPosition);
        _items[itemIndex] = updatedItem;

        return true;
    }

    private void PlaceItemOnGrid(ImmutableGridShape shape, GridPosition position, int itemIndex)
    {
        _grid.FillShape(shape, position, itemIndex);
    }

    private void RemoveItemFromGrid(ImmutableGridShape shape, GridPosition position)
    {
        _grid.FillShape(shape, position, -1);
    }

    public void Dispose()
    {
        if (_items.IsCreated)
        {
            _grid.Dispose();
            _items.Dispose();
            _freeIndices.Dispose();
        }
    }

    public JobHandle Dispose(JobHandle inputDeps)
    {
        return _items.IsCreated ?
            JobHandle.CombineDependencies(_grid.Dispose(inputDeps), _items.Dispose(inputDeps), _freeIndices.Dispose(inputDeps)) :
            inputDeps;
    }

    public static implicit operator ReadOnly(Inventory inventory) => inventory.AsReadOnly();
    public ReadOnly AsReadOnly() => new(_grid.AsReadOnly(), _items.AsReadOnly());

    [SuppressMessage("Design", "CA1716:Identifiers should not match keywords")]
    public readonly ref struct ReadOnly
    {
        private readonly ValueGridShape<int>.ReadOnly _grid;
        private readonly NativeArray<InventoryItem>.ReadOnly _items;

        public ValueGridShape<int>.ReadOnly Grid => _grid;

        public int Width => _grid.Width;
        public int Height => _grid.Height;
        public int ItemCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _items.Length; i++)
                {
                    if (_items[i].IsValid) count++;
                }
                return count;
            }
        }
        public bool IsEmpty => ItemCount == 0;

        public int FreeSpaceCount => _grid.CountValue(-1);
        public int OccupiedSpaceCount => _grid.Size - FreeSpaceCount;

        internal ReadOnly(ValueGridShape<int>.ReadOnly grid, NativeArray<InventoryItem>.ReadOnly items)
        {
            _grid = grid;
            _items = items;
        }

        public InventoryItem this[int index]
        {
            get
            {
                if (index < 0 || index >= _items.Length || !_items[index].IsValid)
                    return InventoryItem.Invalid;
                return _items[index];
            }
        }

        public unsafe bool IsSame(in ReadOnly other)
        {
            return _items.GetUnsafeReadOnlyPtr() == other._items.GetUnsafeReadOnlyPtr();
        }

        public InventoryItem GetItemAt(GridPosition position)
        {
            if (!_grid.Contains(position))
                return InventoryItem.Invalid;
            var itemIndex = _grid[position];
            return itemIndex < 0 || itemIndex >= _items.Length ? InventoryItem.Invalid : _items[itemIndex];
        }

        public bool IsPositionOccupied(GridPosition position)
        {
            return GetItemAt(position).IsValid;
        }

        public bool CanPlaceItemAt(ImmutableGridShape shape, GridPosition position)
        {
            return CanPlaceAt(shape, position);
        }

        public bool CanMoveItem(InventoryItemInstanceId id, ImmutableGridShape shape, GridPosition newPosition)
        {
            if (!_grid.IsWithinBounds(shape, newPosition))
                return false;

            // Check that cells are either empty or occupied by the item we're moving
            var itemIndex = (int)id.Id;
            return _grid.CheckShapeCells(shape, newPosition, (_, value, index) => value == -1 || value == index, itemIndex);
        }

        public GridPosition FindFirstFitPosition(ImmutableGridShape shape)
        {
            for (int y = 0; y <= _grid.Height - shape.Height; y++)
            {
                for (int x = 0; x <= _grid.Width - shape.Width; x++)
                {
                    var pos = new GridPosition(x, y);
                    if (CanPlaceAt(shape, pos))
                    {
                        return pos;
                    }
                }
            }

            return GridPosition.Invalid;
        }

        private bool CanPlaceAt(ImmutableGridShape shape, GridPosition position)
        {
            if (!_grid.IsWithinBounds(shape, position))
                return false;
            return _grid.CheckShapeCells(shape, position, (_, value) => value == -1);
        }
    }
}
