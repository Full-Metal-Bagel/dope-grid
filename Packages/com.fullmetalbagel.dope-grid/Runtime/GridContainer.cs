using System;
using Unity.Collections;
using Unity.Mathematics;

namespace DopeGrid;

public struct GridContainer : IDisposable
{
    public GridShape2D Grid;
    public NativeList<GridShape2D> ItemShapes;
    public NativeList<int2> ItemPositions; // top-left position

    public GridContainer(int width, int height, Allocator allocator = Allocator.Persistent)
    {
        Grid = new GridShape2D(width, height, allocator);
        ItemShapes = new NativeList<GridShape2D>(allocator);
        ItemPositions = new NativeList<int2>(allocator);
    }

    public int ItemCount => ItemShapes.Length;
    public int FreeSpace => Grid.FreeSpaceCount;

    public bool IsCellOccupied(int2 pos)
    {
        return Grid.GetCell(pos);
    }

    public bool TryAddItem(GridShape2D shape)
    {
        var pos = Grid.FindFirstFit(shape);
        if (pos.x >= 0)
        {
            AddItemAt(shape, pos);
            return true;
        }

        return false;
    }

    public bool TryAddItemAt(GridShape2D shape, int2 pos)
    {
        if (Grid.CanPlaceItem(shape, pos))
        {
            AddItemAt(shape, pos);
            return true;
        }

        return false;
    }

    private void AddItemAt(GridShape2D shape, int2 pos)
    {
        Grid.PlaceItem(shape, pos);
        ItemShapes.Add(shape);
        ItemPositions.Add(pos);
    }

    public void RemoveItem(int index)
    {
        if (index >= 0 && index < ItemShapes.Length)
        {
            var shape = ItemShapes[index];
            var pos = ItemPositions[index];
            Grid.RemoveItem(shape, pos);

            ItemShapes.RemoveAtSwapBack(index);
            ItemPositions.RemoveAtSwapBack(index);
        }
    }

    public void Clear()
    {
        Grid.Clear();
        ItemShapes.Clear();
        ItemPositions.Clear();
    }

    public void Dispose()
    {
        Grid.Dispose();
        foreach (var shape in ItemShapes)
            shape.Dispose();
        ItemShapes.Dispose();
        ItemPositions.Dispose();
    }

    public GridContainer Clone(Allocator allocator)
    {
        var clone = new GridContainer
        {
            Grid = Grid.Clone(allocator),
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
