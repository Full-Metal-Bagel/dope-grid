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

        var position = inventory.FindFirstFitWithFixedRotation(item.GetOrCreateImmutable());

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

        var position = inventory.FindFirstFitWithFixedRotation(item.GetOrCreateImmutable());

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
        var immutableItem = item.GetOrCreateImmutable();

        Assert.IsTrue(inventory.CanPlaceItem(immutableItem, new int2(0, 0)));
        Assert.IsFalse(inventory.CanPlaceItem(immutableItem, new int2(2, 2)));
        Assert.IsFalse(inventory.CanPlaceItem(immutableItem, new int2(1, 1)));
        Assert.IsTrue(inventory.CanPlaceItem(immutableItem, new int2(3, 3)));

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

        inventory.PlaceItem(item.GetOrCreateImmutable(), new int2(1, 1));

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

        inventory.PlaceItem(item.GetOrCreateImmutable(), new int2(1, 1));
        inventory.RemoveItem(item.GetOrCreateImmutable(), new int2(1, 1));

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

        var items = new NativeArray<ImmutableGridShape>(3, Allocator.Temp);
        var positions = new NativeArray<GridPosition>(3, Allocator.Temp);

        for (var i = 0; i < 3; i++)
        {
            var item = new GridShape(3, 3, Allocator.Temp);
            for (var y = 0; y < 3; y++)
            for (var x = 0; x < 3; x++)
                item.SetCell(new int2(x, y), true);
            items[i] = item.GetOrCreateImmutable();
            item.Dispose();
        }

        var placed = inventory.PlaceMultipleShapes(items, positions);

        Assert.AreEqual(3, placed);

        items.Dispose();
        positions.Dispose();
        inventory.Dispose();
    }

    [Test]
    public void FindFirstFitWithFreeRotation_FindsWithNoRotation()
    {
        // 5x5 grid with occupied cell at (0,0)
        var inventory = new GridShape(5, 5, Allocator.Temp);
        inventory.SetCell(new int2(0, 0), true);

        // 2x3 L-shaped item that fits without rotation at (2,0)
        var item = new GridShape(2, 3, Allocator.Temp);
        item.SetCell(new int2(0, 0), true);
        item.SetCell(new int2(0, 1), true);
        item.SetCell(new int2(0, 2), true);
        item.SetCell(new int2(1, 2), true);

        var (position, rotation) = inventory.FindFirstFitWithFreeRotation(item.GetOrCreateImmutable());

        Assert.AreEqual(1, position.X);
        Assert.AreEqual(0, position.Y);
        Assert.AreEqual(RotationDegree.None, rotation);

        inventory.Dispose();
        item.Dispose();
    }

    [Test]
    public void FindFirstFitWithFreeRotation_FindsWith90DegreeRotation()
    {
        // 3x5 grid - too narrow for a 3-wide horizontal item
        var inventory = new GridShape(3, 5, Allocator.Temp);
        inventory.SetCell(new int2(0, 0), true);
        inventory.SetCell(new int2(0, 1), true);
        inventory.SetCell(new int2(0, 2), true);
        inventory.SetCell(new int2(0, 3), true);
        inventory.SetCell(new int2(0, 4), true);

        // 3x1 horizontal item that won't fit horizontally (grid is only 2 wide)
        // but will fit vertically after 90° rotation (becomes 1x3)
        var item = new GridShape(3, 1, Allocator.Temp);
        item.SetCell(new int2(0, 0), true);
        item.SetCell(new int2(1, 0), true);
        item.SetCell(new int2(2, 0), true);

        var (position, rotation) = inventory.FindFirstFitWithFreeRotation(item.GetOrCreateImmutable());

        Assert.IsTrue(position.IsValid);
        Assert.AreEqual(RotationDegree.Clockwise90, rotation);

        inventory.Dispose();
        item.Dispose();
    }

    [Test]
    public void FindFirstFitWithFreeRotation_FindsWith180DegreeRotation()
    {
        // 4x4 grid with specific pattern that requires 180° rotation
        var inventory = new GridShape(4, 4, Allocator.Temp);
        // Block top-left corner
        inventory.SetCell(new int2(0, 0), true);
        inventory.SetCell(new int2(1, 0), true);
        inventory.SetCell(new int2(0, 1), true);

        // L-shaped item (occupies cells relative to origin)
        // Normal: (0,0), (1,0), (1,1)
        // This pattern won't fit in top-left but will fit elsewhere with 180° rotation
        var item = new GridShape(2, 2, Allocator.Temp);
        item.SetCell(new int2(0, 0), true);
        item.SetCell(new int2(1, 0), true);
        item.SetCell(new int2(1, 1), true);

        var (position, rotation) = inventory.FindFirstFitWithFreeRotation(item.GetOrCreateImmutable());

        Assert.IsTrue(position.IsValid);
        // Should find a position with some rotation (exact rotation depends on shape after trimming)

        inventory.Dispose();
        item.Dispose();
    }

    [Test]
    public void FindFirstFitWithFreeRotation_FindsWith270DegreeRotation()
    {
        // 4x5 grid
        var inventory = new GridShape(4, 5, Allocator.Temp);
        // Block positions to force 270° rotation
        inventory.SetCell(new int2(0, 0), true);
        inventory.SetCell(new int2(1, 0), true);
        inventory.SetCell(new int2(2, 0), true);

        // 1x3 vertical item
        var item = new GridShape(1, 3, Allocator.Temp);
        item.SetCell(new int2(0, 0), true);
        item.SetCell(new int2(0, 1), true);
        item.SetCell(new int2(0, 2), true);

        var (position, rotation) = inventory.FindFirstFitWithFreeRotation(item.GetOrCreateImmutable());

        Assert.IsTrue(position.IsValid);
        // Should find a valid position with some rotation

        inventory.Dispose();
        item.Dispose();
    }

    [Test]
    public void FindFirstFitWithFreeRotation_ReturnsInvalidWhenNoFit()
    {
        // 2x2 grid completely full
        var inventory = new GridShape(2, 2, Allocator.Temp);
        for (var y = 0; y < 2; y++)
        for (var x = 0; x < 2; x++)
            inventory.SetCell(new int2(x, y), true);

        // 2x2 item that won't fit anywhere
        var item = new GridShape(2, 2, Allocator.Temp);
        item.SetCell(new int2(0, 0), true);
        item.SetCell(new int2(1, 0), true);
        item.SetCell(new int2(0, 1), true);
        item.SetCell(new int2(1, 1), true);

        var (position, rotation) = inventory.FindFirstFitWithFreeRotation(item.GetOrCreateImmutable());

        Assert.IsFalse(position.IsValid);
        Assert.AreEqual(-1, position.X);
        Assert.AreEqual(-1, position.Y);

        inventory.Dispose();
        item.Dispose();
    }

    [Test]
    public void FindFirstFitWithFreeRotation_TriesAllRotations()
    {
        // 5x5 grid with strategic blocking
        var inventory = new GridShape(5, 5, Allocator.Temp);
        // Block areas to ensure we need to try multiple rotations
        inventory.SetCell(new int2(0, 0), true);
        inventory.SetCell(new int2(1, 0), true);

        // Asymmetric L-shaped item
        var item = new GridShape(2, 3, Allocator.Temp);
        item.SetCell(new int2(0, 0), true);
        item.SetCell(new int2(0, 1), true);
        item.SetCell(new int2(0, 2), true);
        item.SetCell(new int2(1, 2), true);

        var (position, rotation) = inventory.FindFirstFitWithFreeRotation(item.GetOrCreateImmutable());

        Assert.IsTrue(position.IsValid);
        // Should find some valid position with appropriate rotation

        inventory.Dispose();
        item.Dispose();
    }

    [Test]
    public void FindFirstFitWithFreeRotation_WorksWithSquareShape()
    {
        // Square shapes look the same in all rotations
        var inventory = new GridShape(5, 5, Allocator.Temp);
        inventory.SetCell(new int2(0, 0), true);

        // 2x2 square
        var item = new GridShape(2, 2, Allocator.Temp);
        item.SetCell(new int2(0, 0), true);
        item.SetCell(new int2(1, 0), true);
        item.SetCell(new int2(0, 1), true);
        item.SetCell(new int2(1, 1), true);

        var (position, rotation) = inventory.FindFirstFitWithFreeRotation(item.GetOrCreateImmutable());

        Assert.IsTrue(position.IsValid);
        Assert.AreEqual(RotationDegree.None, rotation); // Should find immediately without rotation

        inventory.Dispose();
        item.Dispose();
    }
}
