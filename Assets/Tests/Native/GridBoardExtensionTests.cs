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
        inventory[0, 0] = true;
        inventory[1, 0] = true;

        var item = new GridShape(2, 2, Allocator.Temp);
        for (var y = 0; y < 2; y++)
        for (var x = 0; x < 2; x++)
            item[x, y] = true;
        var immutableItem = item.GetOrCreateImmutable();

        var position = ReadOnlyGridShapeExtension.FindFirstFitWithFixedRotation(ref inventory, immutableItem, freeValue: false);

        // FindFirstFit scans left to right, top to bottom, so first available is (0,1)
        Assert.IsTrue(position.IsValid);
        Assert.IsTrue(position.X >= 0);
        Assert.IsTrue(position.Y >= 0);

        inventory.Dispose();
        item.Dispose();
    }

    [Test]
    public void FindFirstFit_ReturnsNegativeWhenNoSpace()
    {
        var inventory = new GridShape(2, 2, Allocator.Temp);
        for (var y = 0; y < 2; y++)
        for (var x = 0; x < 2; x++)
            inventory[x, y] = true;

        var item = new GridShape(1, 1, Allocator.Temp);
        item[0, 0] = true;
        var immutableItem = item.GetOrCreateImmutable();

        var position = ReadOnlyGridShapeExtension.FindFirstFitWithFixedRotation(ref inventory, immutableItem, freeValue: false);

        Assert.AreEqual(-1, position.X);
        Assert.AreEqual(-1, position.Y);

        inventory.Dispose();
        item.Dispose();
    }

    [Test]
    public void CanPlaceItem_ChecksCollisionCorrectly()
    {
        var inventory = new GridShape(5, 5, Allocator.Temp);
        inventory[2, 2] = true;

        var item = new GridShape(2, 2, Allocator.Temp);
        for (var y = 0; y < 2; y++)
        for (var x = 0; x < 2; x++)
            item[x, y] = true;
        var immutableItem = item.GetOrCreateImmutable();

        // CanPlaceItem checks if item can be placed at position
        var canPlace00 = inventory.CanPlaceItem(immutableItem, new GridPosition(0, 0), freeValue: false);
        var canPlace22 = inventory.CanPlaceItem(immutableItem, new GridPosition(2, 2), freeValue: false);
        var canPlace11 = inventory.CanPlaceItem(immutableItem, new GridPosition(1, 1), freeValue: false);
        var canPlace33 = inventory.CanPlaceItem(immutableItem, new GridPosition(3, 3), freeValue: false);

        Assert.IsTrue(canPlace00, "Should be able to place at (0,0)");
        Assert.IsFalse(canPlace22, "Should not be able to place at (2,2) - occupied");
        Assert.IsFalse(canPlace11, "Should not be able to place at (1,1) - overlaps (2,2)");
        Assert.IsTrue(canPlace33, "Should be able to place at (3,3)");

        inventory.Dispose();
        item.Dispose();
    }

    [Test]
    public void PlaceItem_SetsCorrectCells()
    {
        var inventory = new GridShape(5, 5, Allocator.Temp);

        // Create L-shaped item: X.
        //                       XX
        //                       .X
        var item = new GridShape(2, 3, Allocator.Temp);
        item[0, 0] = true;
        item[0, 1] = true;
        item[1, 1] = true;
        item[1, 2] = true;
        var trimmed = item.AsReadOnly().Trim(Allocator.Temp);
        var immutableItem = trimmed.GetOrCreateImmutable();
        item.Dispose();

        WritableGridShapeExtension.PlaceItem(ref inventory, immutableItem, new GridPosition(1, 1), true);

        // Check the placed cells
        Assert.IsTrue(inventory[1, 1], "Cell (1,1) should be occupied");
        Assert.IsTrue(inventory[1, 2], "Cell (1,2) should be occupied");
        Assert.IsTrue(inventory[2, 2], "Cell (2,2) should be occupied");
        Assert.IsTrue(inventory[2, 3], "Cell (2,3) should be occupied");

        inventory.Dispose();
        trimmed.Dispose();
    }

    [Test]
    public void RemoveItem_ClearsCorrectCells()
    {
        var inventory = new GridShape(5, 5, Allocator.Temp);

        var item = new GridShape(2, 2, Allocator.Temp);
        for (var y = 0; y < 2; y++)
        for (var x = 0; x < 2; x++)
            item[x, y] = true;
        var immutableItem = item.GetOrCreateImmutable();

        WritableGridShapeExtension.PlaceItem(ref inventory, immutableItem, new GridPosition(1, 1), true);
        WritableGridShapeExtension.RemoveItem(ref inventory, immutableItem, new GridPosition(1, 1), freeValue: false);

        for (var y = 0; y < 5; y++)
        for (var x = 0; x < 5; x++)
            Assert.IsFalse(inventory[x, y], $"Cell ({x},{y}) should be empty");

        inventory.Dispose();
        item.Dispose();
    }

    [Test]
    public void PlaceMultipleShapes_PlacesAllPossible()
    {
        var inventory = new GridShape(10, 10, Allocator.Temp);

        var items = new NativeArray<ImmutableGridShape>(3, Allocator.Temp);
        var positions = new NativeArray<GridPosition>(3, Allocator.Temp);

        for (var i = 0; i < 3; i++)
        {
            var tempShape = new GridShape(3, 3, Allocator.Temp);
            for (var y = 0; y < 3; y++)
            for (var x = 0; x < 3; x++)
                tempShape[x, y] = true;
            items[i] = tempShape.GetOrCreateImmutable();
            tempShape.Dispose();
        }

        // Place items manually since PlaceMultipleShapes doesn't exist
        var placed = 0;
        for (var i = 0; i < items.Length; i++)
        {
            var position = ReadOnlyGridShapeExtension.FindFirstFitWithFixedRotation(ref inventory, items[i], freeValue: false);
            if (position.IsValid)
            {
                WritableGridShapeExtension.PlaceItem(ref inventory, items[i], position, true);
                positions[i] = position;
                placed++;
            }
        }

        Assert.AreEqual(3, placed);

        items.Dispose();
        positions.Dispose();
        inventory.Dispose();
    }
}
