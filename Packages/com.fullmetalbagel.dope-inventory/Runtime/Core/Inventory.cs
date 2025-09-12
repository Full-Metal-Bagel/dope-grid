using System;
using Unity.Collections;
using Unity.Mathematics;

namespace DopeInventory;

public struct Inventory : IDisposable
{
    public GridShape2D InventoryGrid;
    public NativeList<GridShape2D> ItemShapes;
    public NativeList<int2> ItemPositions; // top-left position

    public Inventory(int width, int height, Allocator allocator = Allocator.Persistent)
    {
        InventoryGrid = new GridShape2D(width, height, allocator);
        ItemShapes = new NativeList<GridShape2D>(allocator);
        ItemPositions = new NativeList<int2>(allocator);
    }

    public int ItemCount => ItemShapes.Length;
    public int FreeSpace => GetFreeCellCount();

    public bool IsCellOccupied(int2 pos)
    {
        return InventoryGrid.GetCell(pos);
    }

    public int GetFreeCellCount()
    {
        return InventoryGrid.FreeSpaceCount;
    }

    public bool TryAddItem(GridShape2D shape)
    {
        var pos = InventoryGrid.FindFirstFit(shape);
        if (pos.x >= 0)
        {
            AddItemAt(shape, pos);
            return true;
        }

        return false;
    }

    public bool TryAddItemAt(GridShape2D shape, int2 pos)
    {
        if (InventoryGrid.CanPlaceItem(shape, pos))
        {
            AddItemAt(shape, pos);
            return true;
        }

        return false;
    }

    private void AddItemAt(GridShape2D shape, int2 pos)
    {
        InventoryGrid.PlaceItem(shape, pos);
        ItemShapes.Add(shape);
        ItemPositions.Add(pos);
    }

    public void RemoveItem(int index)
    {
        if (index >= 0 && index < ItemShapes.Length)
        {
            var shape = ItemShapes[index];
            var pos = ItemPositions[index];
            InventoryGrid.RemoveItem(shape, pos);

            ItemShapes.RemoveAtSwapBack(index);
            ItemPositions.RemoveAtSwapBack(index);
        }
    }

    public void Clear()
    {
        InventoryGrid.Clear();
        ItemShapes.Clear();
        ItemPositions.Clear();
    }

    public void Dispose()
    {
        InventoryGrid.Dispose();
        foreach (var shape in ItemShapes)
            shape.Dispose();
        ItemShapes.Dispose();
        ItemPositions.Dispose();
    }

    public Inventory Clone(Allocator allocator)
    {
        var clone = new Inventory
        {
            InventoryGrid = InventoryGrid.Clone(allocator),
            ItemShapes = new NativeList<GridShape2D>(ItemShapes.Length, allocator),
            ItemPositions = new NativeList<int2>(ItemPositions.Length, allocator)
        };

        for (var i = 0; i < ItemShapes.Length; i++)
        {
            clone.ItemShapes.Add(ItemShapes[i].Clone(allocator));
            clone.ItemPositions.Add(ItemPositions[i]);
        }

        return clone;
    }
}