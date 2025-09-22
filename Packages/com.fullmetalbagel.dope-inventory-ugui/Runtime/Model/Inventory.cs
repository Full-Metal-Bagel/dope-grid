using DopeGrid.Native;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace DopeGrid.Inventory;

public struct Inventory : INativeDisposable
{
    private ValueGridShape<int> _grid;
    public ValueGridShape<int> Grid => _grid;

    private NativeList<InventoryItem> _items;
    public NativeList<InventoryItem> Items => _items;

    public int Width => _grid.Width;
    public int Height => _grid.Height;
    public int ItemCount => _items.Length;
    public bool IsEmpty => _items.Length == 0;
    public bool IsCreated => _items.IsCreated;

    public Inventory(int width, int height, Allocator allocator)
    {
        _grid = new ValueGridShape<int>(width, height, -1, allocator); // -1 means empty cell
        _items = new NativeList<InventoryItem>(allocator);
    }

    public bool TryPlaceItem(in InventoryItem inventoryItem)
    {
        var shape = inventoryItem.Shape;
        if (!CanPlaceItem(shape, inventoryItem.Position)) return false;

        var itemIndex = _items.Length;
        _items.Add(inventoryItem);
        PlaceItemOnGrid(shape, inventoryItem.Position, itemIndex);
        return true;
    }

    public bool RemoveItem(int itemIndex)
    {
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
        }

        // Remove from items list using swap and remove
        _items.RemoveAtSwapBack(itemIndex);
        return true;
    }

    public bool TryMoveItem(int itemIndex, int2 newPosition)
    {
        if (itemIndex < 0 || itemIndex >= _items.Length)
            return false;

        var item = _items[itemIndex];
        var shape = item.Shape;

        // First check if new position is valid (excluding current item's cells)
        if (!CanMoveItem(itemIndex, shape, newPosition))
            return false;

        // Remove from current position
        RemoveItemFromGrid(shape, item.Position);

        // Place at new position
        PlaceItemOnGrid(shape, newPosition, itemIndex);

        // Update item's position
        var updatedItem = new InventoryItem(item.InstanceId, item.Definition, item.Rotation, newPosition);
        _items[itemIndex] = updatedItem;

        return true;
    }

    public int GetItemAt(int2 position)
    {
        if (!_grid.Contains(position))
            return -1;

        return _grid[position];
    }

    public bool IsPositionOccupied(int2 position)
    {
        return GetItemAt(position) != -1;
    }

    public bool CanPlaceItemAt(in InventoryItem inventoryItem, int2 position)
    {
        var itemAtPosition = new InventoryItem(inventoryItem.InstanceId, inventoryItem.Definition, inventoryItem.Rotation, position);
        return CanPlaceItem(itemAtPosition.Shape, position);
    }

    public bool CanPlaceItemAt(ImmutableGridShape shape, int2 position)
    {
        return CanPlaceItem(shape, position);
    }

    public void Clear()
    {
        _grid.Clear();
        _grid.Fill(-1);
        _items.Clear();
    }

    public int GetFreeSpaceCount()
    {
        return _grid.CountValue(-1);
    }

    public int GetOccupiedSpaceCount()
    {
        return _grid.Size - GetFreeSpaceCount();
    }

    public bool ContainsItem(int instanceId)
    {
        for (int i = 0; i < _items.Length; i++)
        {
            if (_items[i].InstanceId == instanceId)
                return true;
        }
        return false;
    }

    public int FindItemIndex(int instanceId)
    {
        for (int i = 0; i < _items.Length; i++)
        {
            if (_items[i].InstanceId == instanceId)
                return i;
        }
        return -1;
    }

    public bool TryGetItem(int itemIndex, out InventoryItem inventoryItem)
    {
        if (itemIndex >= 0 && itemIndex < _items.Length)
        {
            inventoryItem = _items[itemIndex];
            return true;
        }

        inventoryItem = default;
        return false;
    }

    public bool TryFindFirstFitPosition(ImmutableGridShape shape, out int2 position)
    {
        for (int y = 0; y <= _grid.Height - shape.Height; y++)
        {
            for (int x = 0; x <= _grid.Width - shape.Width; x++)
            {
                var pos = new int2(x, y);
                if (CanPlaceItem(shape, pos))
                {
                    position = pos;
                    return true;
                }
            }
        }

        position = new int2(-1, -1);
        return false;
    }

    public bool TryAutoPlaceItem(in InventoryItem inventoryItem, out int2 position)
    {
        if (TryFindFirstFitPosition(inventoryItem.Shape, out position))
        {
            var placedItem = new InventoryItem(inventoryItem.InstanceId, inventoryItem.Definition, inventoryItem.Rotation, position);
            return TryPlaceItem(placedItem);
        }

        return false;
    }

    private bool CanPlaceItem(ImmutableGridShape shape, int2 position)
    {
        if (!_grid.IsWithinBounds(shape, position))
            return false;

        // Check that all cells are empty (-1)
        return _grid.CheckShapeCells(shape, position, (_, value) => value == -1);
    }

    private bool CanMoveItem(int itemIndex, ImmutableGridShape shape, int2 newPosition)
    {
        if (!_grid.IsWithinBounds(shape, newPosition))
            return false;

        // Check that cells are either empty or occupied by the item we're moving
        return _grid.CheckShapeCells(shape, newPosition, (_, value, index) => value == -1 || value == index, itemIndex);
    }

    private void PlaceItemOnGrid(ImmutableGridShape shape, int2 position, int itemIndex)
    {
        _grid.FillShape(shape, position, itemIndex);
    }

    private void RemoveItemFromGrid(ImmutableGridShape shape, int2 position)
    {
        _grid.FillShape(shape, position, -1);
    }

    public void Dispose()
    {
        if (_items.IsCreated)
        {
            _grid.Dispose();
            _items.Dispose();
        }
    }

    public JobHandle Dispose(JobHandle inputDeps)
    {
        if (_items.IsCreated)
        {
            inputDeps = _grid.Dispose(inputDeps);
            _items.Dispose(inputDeps);
        }
        return inputDeps;
    }
}
