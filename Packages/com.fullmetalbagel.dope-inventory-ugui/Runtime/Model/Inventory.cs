using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DopeGrid.Native;
using JetBrains.Annotations;
using Unity.Mathematics;

namespace DopeGrid.Inventory;

public struct Inventory
{
    private ValueGridShape<int> _grid;
    public ValueGridShape<int> Grid => _grid;

    private List<InventoryItem> _items;
    public IReadOnlyList<InventoryItem> Items => _items;

    private Dictionary<ulong/*instance id*/, int/*item index*/> _itemMap;

    public int Width => _grid.Width;
    public int Height => _grid.Height;
    public int ItemCount => _items.Count;
    public bool IsEmpty => ItemCount == 0;

    public Inventory(int width, int height)
    {
        _grid = new ValueGridShape<int>(width, height, -1); // -1 means empty cell
        _items = new List<InventoryItem>(width * height);
        _itemMap = new Dictionary<ulong, int>(width * height);
    }

    public InventoryItem this[int index] => _items[index];

    public readonly bool IsSame(in Inventory other) => ((ReadOnly)this).IsSame(other);
    public readonly InventoryItem GetItemAt(int2 position) => ((ReadOnly)this).GetItemAt(position);
    public readonly InventoryItem GetItemByInstanceId(InventoryItemInstanceId id) => ((ReadOnly)this).GetItemByInstanceId(id);
    public readonly bool IsPositionOccupied(int2 position) => ((ReadOnly)this).IsPositionOccupied(position);
    public readonly bool CanPlaceItemAt(in InventoryItem inventoryItem, int2 position) => ((ReadOnly)this).CanPlaceItemAt(inventoryItem, position);
    public readonly bool CanPlaceItemAt(ImmutableGridShape shape, int2 position) => ((ReadOnly)this).CanPlaceItemAt(shape, position);
    public readonly int GetFreeSpaceCount() => ((ReadOnly)this).GetFreeSpaceCount();
    public readonly int GetOccupiedSpaceCount() => ((ReadOnly)this).GetOccupiedSpaceCount();
    public readonly bool ContainsItem(InventoryItemInstanceId id) => ((ReadOnly)this).ContainsItem(id);
    public readonly int GetItemIndex(InventoryItemInstanceId id) => ((ReadOnly)this).GetItemIndex(id);
    public readonly bool TryFindFirstFitPosition(ImmutableGridShape shape, out int2 position) => ((ReadOnly)this).TryFindFirstFitPosition(shape, out position);
    public readonly bool CanMoveItem(InventoryItemInstanceId id, ImmutableGridShape shape, int2 newPosition) => ((ReadOnly)this).CanMoveItem(id, shape, newPosition);

    public void Clear()
    {
        _grid.Clear();
        _grid.Fill(-1);
        _items.Clear();
        _itemMap.Clear();
    }

    public bool TryAutoPlaceItem(InventoryItemInstanceId id, ItemDefinition itemDefinition, out int2 position)
    {
        position = new(-1, -1);
        for (var rotation = RotationDegree.None; rotation <= RotationDegree.Clockwise270; rotation++)
        {
            var shape = itemDefinition.Shape.GetRotatedShape(rotation);
            if (((ReadOnly)this).TryFindFirstFitPosition(shape, out position))
            {
                var placedItem = new InventoryItem(id, itemDefinition, rotation, position);
                PlaceItem(placedItem);
                return true;
            }
        }
        return false;
    }

    public bool TryPlaceItem(in InventoryItem inventoryItem)
    {
        var shape = inventoryItem.Shape;
        if (!CanPlaceItemAt(shape, inventoryItem.Position)) return false;
        PlaceItem(inventoryItem);
        return true;
    }

    private void PlaceItem(in InventoryItem inventoryItem)
    {
        var itemIndex = _items.Count;
        _items.Add(inventoryItem);
        _itemMap[inventoryItem.InstanceId] = itemIndex;
        PlaceItemOnGrid(inventoryItem.Shape, inventoryItem.Position, itemIndex);
    }

    public bool RemoveItem(InventoryItemInstanceId id)
    {
        var itemIndex = GetItemIndex(id);
        if (itemIndex < 0 || itemIndex >= _items.Count)
            return false;

        var item = _items[itemIndex];
        var shape = item.Shape;

        // Clear the item from the grid
        RemoveItemFromGrid(shape, item.Position);

        // If not the last item, we need to update the grid for the swapped item
        if (itemIndex < _items.Count - 1)
        {
            var lastItem = _items[^1];
            _grid.FillShape(lastItem.Shape, lastItem.Position, itemIndex);
            _itemMap[lastItem.InstanceId] = itemIndex;
        }

        // Remove from items list using swap and remove
        var lastIdx = _items.Count - 1;
        if (itemIndex != lastIdx)
        {
            _items[itemIndex] = _items[lastIdx];
        }
        _items.RemoveAt(lastIdx);
        _itemMap.Remove(item.InstanceId);
        return true;
    }

    public bool TryMoveItem(InventoryItemInstanceId id, int2 newPosition)
    {
        var itemIndex = GetItemIndex(id);
        if (itemIndex < 0 || itemIndex >= _items.Count) return false;

        var item = _items[itemIndex];
        return TryMoveItemInternal(itemIndex, item, newPosition, item.Rotation);
    }

    public bool TryMoveItem(InventoryItemInstanceId id, int2 newPosition, RotationDegree newRotation)
    {
        var itemIndex = GetItemIndex(id);
        if (itemIndex < 0 || itemIndex >= _items.Length) return false;

        var item = _items[itemIndex];
        return TryMoveItemInternal(itemIndex, item, newPosition, newRotation);
    }

    private bool TryMoveItemInternal(int itemIndex, in InventoryItem item, int2 newPosition, RotationDegree newRotation)
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

    private void PlaceItemOnGrid(ImmutableGridShape shape, int2 position, int itemIndex)
    {
        _grid.FillShape(shape, position, itemIndex);
    }

    private void RemoveItemFromGrid(ImmutableGridShape shape, int2 position)
    {
        _grid.FillShape(shape, position, -1);
    }

    public void Dispose() { _grid.Dispose(); }

    public static implicit operator ReadOnly(Inventory inventory) => inventory.AsReadOnly();
    public ReadOnly AsReadOnly() => new(_grid.AsReadOnly(), _items, _itemMap);

    [SuppressMessage("Design", "CA1716:Identifiers should not match keywords")]
    public readonly ref struct ReadOnly
    {
        private readonly ValueGridShape<int>.ReadOnly _grid;
        private readonly IReadOnlyList<InventoryItem> _items;
        private readonly Dictionary<ulong, int> _itemMap;

        public ValueGridShape<int>.ReadOnly Grid => _grid;
        public IReadOnlyList<InventoryItem> Items => _items;

        public int Width => _grid.Width;
        public int Height => _grid.Height;
        public int ItemCount => _items.Count;
        public bool IsEmpty => _items.Count == 0;

        internal ReadOnly(ValueGridShape<int>.ReadOnly grid, List<InventoryItem> items, Dictionary<ulong, int> itemMap)
        {
            _grid = grid;
            _items = items;
            _itemMap = itemMap;
        }

        public InventoryItem this[int index] => _items[index];

        public bool IsSame(in ReadOnly other) => _itemMap == other._itemMap;

        public InventoryItem GetItemAt(int2 position)
        {
            if (!_grid.Contains(position))
                return InventoryItem.Invalid;
            var itemIndex = _grid[position];
            return itemIndex < 0 ? InventoryItem.Invalid : _items[itemIndex];
        }

        public bool IsPositionOccupied(int2 position) => GetItemAt(position).IsValid;

        public bool CanPlaceItemAt(in InventoryItem inventoryItem, int2 position)
        {
            var itemAtPosition = new InventoryItem(inventoryItem.InstanceId, inventoryItem.Definition, inventoryItem.Rotation, position);
            return CanPlaceAt(itemAtPosition.Shape, position);
        }

        public bool CanPlaceItemAt(ImmutableGridShape shape, int2 position) => CanPlaceAt(shape, position);

        public bool CanMoveItem(InventoryItemInstanceId id, ImmutableGridShape shape, int2 newPosition)
        {
            if (!_grid.IsWithinBounds(shape, newPosition))
                return false;

            // Check that cells are either empty or occupied by the item we're moving
            var itemIndex = GetItemIndex(id);
            return _grid.CheckShapeCells(shape, newPosition, (_, value, index) => value == -1 || value == index, itemIndex);
        }

        public int GetFreeSpaceCount() => _grid.CountValue(-1);
        public int GetOccupiedSpaceCount() => _grid.Size - GetFreeSpaceCount();

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

        public bool TryFindFirstFitPosition(ImmutableGridShape shape, out int2 position)
        {
            for (int y = 0; y <= _grid.Height - shape.Height; y++)
            {
                for (int x = 0; x <= _grid.Width - shape.Width; x++)
                {
                    var pos = new int2(x, y);
                    if (CanPlaceAt(shape, pos))
                    {
                        position = pos;
                        return true;
                    }
                }
            }

            position = new int2(-1, -1);
            return false;
        }

        private bool CanPlaceAt(ImmutableGridShape shape, int2 position)
        {
            if (!_grid.IsWithinBounds(shape, position))
                return false;
            return _grid.CheckShapeCells(shape, position, (_, value) => value == -1);
        }
    }
}
