using DopeGrid;
using DopeGrid.Native;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

public class GridBoardExtensionTests
{
    [Test]
    public void FindFirstFit_FindsFirstAvailablePosition()
    {
        var inventory = new GridShape(5, 5, Allocator.Temp);
        inventory.SetCell(new int2(0, 0), true);
        inventory.SetCell(new int2(1, 0), true);

        var item = new GridShape(2, 2, Allocator.Temp);
        item.SetCell(new int2(0, 0), true);
        item.SetCell(new int2(1, 0), true);
        item.SetCell(new int2(0, 1), true);
        item.SetCell(new int2(1, 1), true);

        var position = inventory.FindFirstFit(item);

        Assert.AreEqual(2, position.X);
        Assert.AreEqual(0, position.Y);

        inventory.Dispose();
        item.Dispose();
    }

    [Test]
    public void FindFirstFit_ReturnsNegativeWhenNoSpace()
    {
        var inventory = new GridShape(2, 2, Allocator.Temp);
        for (var y = 0; y < 2; y++)
        for (var x = 0; x < 2; x++)
            inventory.SetCell(new int2(x, y), true);

        var item = new GridShape(1, 1, Allocator.Temp);
        item.SetCell(new int2(0, 0), true);

        var position = inventory.FindFirstFit(item);

        Assert.AreEqual(-1, position.X);
        Assert.AreEqual(-1, position.Y);

        inventory.Dispose();
        item.Dispose();
    }

    [Test]
    public void CanPlaceItem_ChecksCollisionCorrectly()
    {
        var inventory = new GridShape(5, 5, Allocator.Temp);
        inventory.SetCell(new int2(2, 2), true);

        var item = new GridShape(2, 2, Allocator.Temp);
        item.SetCell(new int2(0, 0), true);
        item.SetCell(new int2(1, 0), true);
        item.SetCell(new int2(0, 1), true);
        item.SetCell(new int2(1, 1), true);

        Assert.IsTrue(inventory.CanPlaceItem(item, new int2(0, 0)));
        Assert.IsFalse(inventory.CanPlaceItem(item, new int2(2, 2)));
        Assert.IsFalse(inventory.CanPlaceItem(item, new int2(1, 1)));
        Assert.IsTrue(inventory.CanPlaceItem(item, new int2(3, 3)));

        inventory.Dispose();
        item.Dispose();
    }

    [Test]
    public void PlaceItem_SetsCorrectCells()
    {
        var inventory = new GridShape(5, 5, Allocator.Temp);

        var item = new GridShape(2, 3, Allocator.Temp);
        item.SetCell(new int2(0, 0), true);
        item.SetCell(new int2(0, 1), true);
        item.SetCell(new int2(1, 1), true);
        item.SetCell(new int2(1, 2), true);

        inventory.PlaceItem(item, new int2(1, 1));

        Assert.IsTrue(inventory.GetCell(new int2(1, 1)));
        Assert.IsTrue(inventory.GetCell(new int2(1, 2)));
        Assert.IsTrue(inventory.GetCell(new int2(2, 2)));
        Assert.IsTrue(inventory.GetCell(new int2(2, 3)));
        Assert.IsFalse(inventory.GetCell(new int2(2, 1)));
        Assert.IsFalse(inventory.GetCell(new int2(1, 3)));

        inventory.Dispose();
        item.Dispose();
    }

    [Test]
    public void RemoveItem_ClearsCorrectCells()
    {
        var inventory = new GridShape(5, 5, Allocator.Temp);

        var item = new GridShape(2, 2, Allocator.Temp);
        item.SetCell(new int2(0, 0), true);
        item.SetCell(new int2(1, 0), true);
        item.SetCell(new int2(0, 1), true);
        item.SetCell(new int2(1, 1), true);

        inventory.PlaceItem(item, new int2(1, 1));
        inventory.RemoveItem(item, new int2(1, 1));

        for (var y = 0; y < 5; y++)
        for (var x = 0; x < 5; x++)
            Assert.IsFalse(inventory.GetCell(new int2(x, y)));

        inventory.Dispose();
        item.Dispose();
    }

    [Test]
    public void PlaceMultipleShapes_PlacesAllPossible()
    {
        var inventory = new GridShape(10, 10, Allocator.Temp);

        var items = new NativeArray<GridShape>(3, Allocator.Temp);
        var positions = new NativeArray<GridPosition>(3, Allocator.Temp);

        for (var i = 0; i < 3; i++)
        {
            items[i] = new GridShape(3, 3, Allocator.Temp);
            for (var y = 0; y < 3; y++)
            for (var x = 0; x < 3; x++)
                items[i].SetCell(new int2(x, y), true);
        }

        var placed = inventory.PlaceMultipleShapes(items, positions);

        Assert.AreEqual(3, placed);

        for (var i = 0; i < 3; i++)
            items[i].Dispose();
        items.Dispose();
        positions.Dispose();
        inventory.Dispose();
    }
}
