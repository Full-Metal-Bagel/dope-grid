using System.Diagnostics.CodeAnalysis;
using DopeGrid.Native;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace DopeGrid.Inventory;

public struct Inventory : INativeDisposable
{
    private ValueGridShape<int> _grid;
    public ValueGridShape<int> Grid => _grid;

    private NativeList<InventoryItem> _items;
    public NativeList<InventoryItem> Items => _items;

    private NativeHashMap<ulong/*instance id*/, int/*item index*/> _itemMap;

    public readonly int Width => _grid.Width;
    public readonly int Height => _grid.Height;
    public readonly int ItemCount => _items.IsCreated ? _items.Length : 0;
    public readonly bool IsEmpty => ItemCount == 0;
    public readonly bool IsCreated => _items.IsCreated;
    public readonly int FreeSpaceCount => ((ReadOnly)this).FreeSpaceCount;
    public readonly int OccupiedSpaceCount => ((ReadOnly)this).OccupiedSpaceCount;

    public Inventory(int width, int height, Allocator allocator)
    {
        _grid = new ValueGridShape<int>(width, height, -1, allocator); // -1 means empty cell
        _items = new NativeList<InventoryItem>(width * height, allocator);
        _itemMap = new NativeHashMap<ulong, int>(width * height, allocator);
    }

    public InventoryItem this[int index] => _items[index];
    [MustDisposeResource]
    public NativeArray<InventoryItem>.Enumerator Enumerator() => _items.GetEnumerator();

    public readonly bool IsSame(in Inventory other) => ((ReadOnly)this).IsSame(other);
    public readonly InventoryItem GetItemAt(GridPosition position) => ((ReadOnly)this).GetItemAt(position);
    public readonly InventoryItem GetItemByInstanceId(InventoryItemInstanceId id) => ((ReadOnly)this).GetItemByInstanceId(id);
    public readonly bool IsPositionOccupied(GridPosition position) => ((ReadOnly)this).IsPositionOccupied(position);
    public readonly bool CanPlaceShapeAt(ImmutableGridShape shape, GridPosition position) => ((ReadOnly)this).CanPlaceItemAt(shape, position);
    public readonly bool ContainsItem(InventoryItemInstanceId id) => ((ReadOnly)this).ContainsItem(id);
    public readonly int GetItemIndex(InventoryItemInstanceId id) => ((ReadOnly)this).GetItemIndex(id);
    public readonly GridPosition FindFirstFitPosition(ImmutableGridShape shape) => ((ReadOnly)this).FindFirstFitPosition(shape);
    public readonly bool CanMoveItem(InventoryItemInstanceId id, ImmutableGridShape shape, GridPosition newPosition) => ((ReadOnly)this).CanMoveItem(id, shape, newPosition);

    public void Clear()
    {
        _grid.FillAll(-1);
        _items.Clear();
        _itemMap.Clear();
    }

    public InventoryItem TryAutoPlaceItem(InventoryItemInstanceId id, ItemDefinition itemDefinition)
    {
        for (var rotation = RotationDegree.None; rotation <= RotationDegree.Clockwise270; rotation++)
        {
            var shape = itemDefinition.Shape.GetRotatedShape(rotation);
            var position = ((ReadOnly)this).FindFirstFitPosition(shape);
            if (position.IsValid)
            {
                var placedItem = new InventoryItem(id, itemDefinition, rotation, position);
                PlaceItem(placedItem);
                return placedItem;
            }
        }
        return InventoryItem.Invalid;
    }

    public bool TryPlaceItem(in InventoryItem inventoryItem)
    {
        var shape = inventoryItem.Shape;
        if (!CanPlaceShapeAt(shape, inventoryItem.Position)) return false;
        PlaceItem(inventoryItem);
        return true;
    }

    private void PlaceItem(in InventoryItem inventoryItem)
    {
        var itemIndex = _items.Length;
        _items.Add(inventoryItem);
        _itemMap.Add(inventoryItem.InstanceId, itemIndex);
        PlaceItemOnGrid(inventoryItem.Shape, inventoryItem.Position, itemIndex);
    }

    public bool RemoveItem(InventoryItemInstanceId id)
    {
        var itemIndex = GetItemIndex(id);
        if (itemIndex < 0 || itemIndex >= _items.Length)
            return false;

        var item = _items[itemIndex];
        var shape = item.Shape;

        // Clear the item from the grid
        RemoveItemFromGrid(shape, item.Position);

        // If not the last item, we need to update the grid for the swapped item
        if (itemIndex < _items.Length - 1)
        {
            var lastItem = _items[^1];
            _grid.FillShape(lastItem.Shape, lastItem.Position, itemIndex);
            _itemMap[lastItem.InstanceId] = itemIndex;
        }

        // Remove from items list using swap and remove
        _items.RemoveAtSwapBack(itemIndex);
        _itemMap.Remove(item.InstanceId);
        return true;
    }

    public bool TryMoveItem(InventoryItemInstanceId id, GridPosition newPosition)
    {
        var itemIndex = GetItemIndex(id);
        if (itemIndex < 0 || itemIndex >= _items.Length) return false;

        var item = _items[itemIndex];
        return TryMoveItemInternal(itemIndex, item, newPosition, item.Rotation);
    }

    public bool TryMoveItem(InventoryItemInstanceId id, GridPosition newPosition, RotationDegree newRotation)
    {
        var itemIndex = GetItemIndex(id);
        if (itemIndex < 0 || itemIndex >= _items.Length) return false;

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
            _itemMap.Dispose();
        }
    }

    public JobHandle Dispose(JobHandle inputDeps)
    {
        return _items.IsCreated ?
            JobHandle.CombineDependencies(_grid.Dispose(inputDeps), _items.Dispose(inputDeps), _itemMap.Dispose(inputDeps)) :
            inputDeps;
    }

    public static implicit operator ReadOnly(Inventory inventory) => inventory.AsReadOnly();
    public ReadOnly AsReadOnly() => new(_grid.AsReadOnly(), _items.AsReadOnly(), _itemMap.AsReadOnly());

    [SuppressMessage("Design", "CA1716:Identifiers should not match keywords")]
    public readonly ref struct ReadOnly
    {
        private readonly ValueGridShape<int>.ReadOnly _grid;
        private readonly NativeArray<InventoryItem>.ReadOnly _items;
        private readonly NativeHashMap<ulong/*instance id*/, int/*item index*/>.ReadOnly _itemMap;

        public ValueGridShape<int>.ReadOnly Grid => _grid;
        public NativeArray<InventoryItem>.ReadOnly Items => _items;

        public int Width => _grid.Width;
        public int Height => _grid.Height;
        public int ItemCount => _items.Length;
        public bool IsEmpty => _items.Length == 0;

        public int FreeSpaceCount => _grid.CountValue(-1);
        public int OccupiedSpaceCount => _grid.Size() - FreeSpaceCount;

        internal ReadOnly(ValueGridShape<int>.ReadOnly grid, NativeArray<InventoryItem>.ReadOnly items, NativeHashMap<ulong, int>.ReadOnly itemMap)
        {
            _grid = grid;
            _items = items;
            _itemMap = itemMap;
        }

        public InventoryItem this[int index] => _items[index];
        [MustDisposeResource]
        public NativeArray<InventoryItem>.ReadOnly.Enumerator Enumerator() => _items.GetEnumerator();

        public unsafe bool IsSame(in ReadOnly other)
        {
            return _items.GetUnsafeReadOnlyPtr() == other._items.GetUnsafeReadOnlyPtr();
        }

        public InventoryItem GetItemAt(GridPosition position)
        {
            if (!_grid.Contains(position))
                return InventoryItem.Invalid;
            var itemIndex = _grid[position];
            return itemIndex < 0 ? InventoryItem.Invalid : _items[itemIndex];
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
            var itemIndex = GetItemIndex(id);
            return _grid.CheckShapeCells(shape, newPosition, (_, value, index) => value == -1 || value == index, itemIndex);
        }

        public bool ContainsItem(InventoryItemInstanceId id)
        {
            return _itemMap.ContainsKey(id);
        }

        public int GetItemIndex(InventoryItemInstanceId id)
        {
            return _itemMap.TryGetValue(id, out var itemIndex) ? itemIndex : -1;
        }

        public InventoryItem GetItemByInstanceId(InventoryItemInstanceId id)
        {
            var index = GetItemIndex(id);
            return index >= 0 ? _items[index] : InventoryItem.Invalid;
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
