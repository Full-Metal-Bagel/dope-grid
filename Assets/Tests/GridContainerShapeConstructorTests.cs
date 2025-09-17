using System;
using DopeGrid;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

[TestFixture]
public class GridContainerShapeConstructorTests
{
    [Test]
    public void Constructor_WithEmptyShape_CreatesEmptyContainer()
    {
        using var shape = new GridShape2D(5, 5, Allocator.Temp);
        using var container = new GridContainer(shape, Allocator.Temp);

        Assert.AreEqual(5, container.Width);
        Assert.AreEqual(5, container.Height);
        Assert.AreEqual(0, container.ItemCount);
        Assert.AreEqual(25, container.FreeSpace);

        // Verify all cells are unoccupied
        for (var y = 0; y < 5; y++)
        for (var x = 0; x < 5; x++)
        {
            Assert.IsFalse(container.IsCellOccupied(new int2(x, y)));
        }
    }

    [Test]
    public void Constructor_WithPartiallyFilledShape_CreatesCorrectContainer()
    {
        using var shape = new GridShape2D(4, 4, Allocator.Temp);

        // Fill some cells to create a pattern
        shape.SetCell(new int2(0, 0), true);
        shape.SetCell(new int2(1, 1), true);
        shape.SetCell(new int2(2, 2), true);
        shape.SetCell(new int2(3, 3), true);

        using var container = new GridContainer(shape, Allocator.Temp);

        Assert.AreEqual(4, container.Width);
        Assert.AreEqual(4, container.Height);
        Assert.AreEqual(0, container.ItemCount); // No items added yet
        Assert.AreEqual(12, container.FreeSpace); // 16 - 4 occupied cells

        // Verify the pattern was copied
        Assert.IsTrue(container.IsCellOccupied(new int2(0, 0)));
        Assert.IsTrue(container.IsCellOccupied(new int2(1, 1)));
        Assert.IsTrue(container.IsCellOccupied(new int2(2, 2)));
        Assert.IsTrue(container.IsCellOccupied(new int2(3, 3)));

        // Verify other cells are free
        Assert.IsFalse(container.IsCellOccupied(new int2(0, 1)));
        Assert.IsFalse(container.IsCellOccupied(new int2(1, 0)));
    }

    [Test]
    public void Constructor_WithComplexShape_PreservesInitialState()
    {
        using var cross = Shapes.Cross(Allocator.Temp);
        using var container = new GridContainer(cross, Allocator.Temp);

        Assert.AreEqual(3, container.Width);
        Assert.AreEqual(3, container.Height);
        Assert.AreEqual(4, container.FreeSpace); // Cross has 5 occupied cells out of 9

        // Verify cross pattern
        Assert.IsFalse(container.IsCellOccupied(new int2(0, 0)));
        Assert.IsTrue(container.IsCellOccupied(new int2(1, 0)));
        Assert.IsFalse(container.IsCellOccupied(new int2(2, 0)));

        Assert.IsTrue(container.IsCellOccupied(new int2(0, 1)));
        Assert.IsTrue(container.IsCellOccupied(new int2(1, 1)));
        Assert.IsTrue(container.IsCellOccupied(new int2(2, 1)));

        Assert.IsFalse(container.IsCellOccupied(new int2(0, 2)));
        Assert.IsTrue(container.IsCellOccupied(new int2(1, 2)));
        Assert.IsFalse(container.IsCellOccupied(new int2(2, 2)));
    }

    [Test]
    public void Constructor_ClonesShapeIndependently()
    {
        using var originalShape = new GridShape2D(3, 3, Allocator.Temp);
        originalShape.SetCell(new int2(1, 1), true);

        using var container = new GridContainer(originalShape, Allocator.Temp);

        // Modify original shape after creating container
        originalShape.SetCell(new int2(0, 0), true);
        originalShape.SetCell(new int2(2, 2), true);

        // Container should not be affected
        Assert.IsFalse(container.IsCellOccupied(new int2(0, 0)));
        Assert.IsTrue(container.IsCellOccupied(new int2(1, 1)));
        Assert.IsFalse(container.IsCellOccupied(new int2(2, 2)));
        Assert.AreEqual(8, container.FreeSpace); // Only center cell occupied
    }

    [Test]
    public void Constructor_InitializedGridMatchesInput()
    {
        using var shape = new GridShape2D(3, 3, Allocator.Temp);
        shape.SetCell(new int2(0, 0), true);
        shape.SetCell(new int2(2, 0), true);
        shape.SetCell(new int2(1, 1), true);

        using var container = new GridContainer(shape, Allocator.Temp);

        var initializedGrid = container.InitializedGrid;

        // InitializedGrid should match the input shape
        Assert.AreEqual(3, initializedGrid.Width);
        Assert.AreEqual(3, initializedGrid.Height);
        Assert.AreEqual(3, initializedGrid.OccupiedSpaceCount);

        Assert.IsTrue(initializedGrid.GetCell(new int2(0, 0)));
        Assert.IsTrue(initializedGrid.GetCell(new int2(2, 0)));
        Assert.IsTrue(initializedGrid.GetCell(new int2(1, 1)));
    }

    [Test]
    public void Constructor_CurrentGridMatchesInitialGrid()
    {
        using var shape = new GridShape2D(4, 3, Allocator.Temp);
        shape.SetCell(new int2(1, 0), true);
        shape.SetCell(new int2(2, 1), true);

        using var container = new GridContainer(shape, Allocator.Temp);

        var currentGrid = container.CurrentGrid;
        var initializedGrid = container.InitializedGrid;

        // At creation, CurrentGrid should match InitializedGrid
        Assert.IsTrue(currentGrid.Equals(initializedGrid));
    }

    [Test]
    public void Constructor_WithLargeShape_WorksCorrectly()
    {
        const int size = 100;
        using var largeShape = new GridShape2D(size, size, Allocator.Temp);

        // Create a sparse pattern
        for (var i = 0; i < size * size; i += 7)
        {
            var x = i % size;
            var y = i / size;
            largeShape.SetCell(new int2(x, y), true);
        }

        using var container = new GridContainer(largeShape, Allocator.Temp);

        Assert.AreEqual(size, container.Width);
        Assert.AreEqual(size, container.Height);

        var expectedOccupied = (size * size + 6) / 7; // Ceiling division
        var expectedFree = size * size - expectedOccupied;
        Assert.AreEqual(expectedFree, container.FreeSpace);
    }

    [Test]
    public void Constructor_WithDifferentAllocators()
    {
        using var shape = new GridShape2D(3, 3, Allocator.Temp);
        shape.SetCell(new int2(1, 1), true);

        // Test with Temp allocator
        using (var container = new GridContainer(shape, Allocator.Temp))
        {
            Assert.AreEqual(8, container.FreeSpace);
        }

        // Test with TempJob allocator
        using (var container = new GridContainer(shape, Allocator.TempJob))
        {
            Assert.AreEqual(8, container.FreeSpace);
        }

        // Test with Persistent allocator
        using (var container = new GridContainer(shape, Allocator.Persistent))
        {
            Assert.AreEqual(8, container.FreeSpace);
        }
    }

    [Test]
    public void Constructor_MinimalShape_1x1()
    {
        using var shape = new GridShape2D(1, 1, Allocator.Temp);
        using var container = new GridContainer(shape, Allocator.Temp);

        Assert.AreEqual(1, container.Width);
        Assert.AreEqual(1, container.Height);
        Assert.AreEqual(1, container.FreeSpace);

        shape.SetCell(new int2(0, 0), true);
        using var fullContainer = new GridContainer(shape, Allocator.Temp);
        Assert.AreEqual(0, fullContainer.FreeSpace);
    }

    [Test]
    public void Constructor_RectangularShapes()
    {
        // Wide rectangle
        using var wideShape = new GridShape2D(10, 2, Allocator.Temp);
        using var wideContainer = new GridContainer(wideShape, Allocator.Temp);

        Assert.AreEqual(10, wideContainer.Width);
        Assert.AreEqual(2, wideContainer.Height);
        Assert.AreEqual(20, wideContainer.FreeSpace);

        // Tall rectangle
        using var tallShape = new GridShape2D(2, 10, Allocator.Temp);
        using var tallContainer = new GridContainer(tallShape, Allocator.Temp);

        Assert.AreEqual(2, tallContainer.Width);
        Assert.AreEqual(10, tallContainer.Height);
        Assert.AreEqual(20, tallContainer.FreeSpace);
    }

    [Test]
    public void Constructor_PreFilledShape_BlocksItemPlacement()
    {
        using var shape = new GridShape2D(3, 3, Allocator.Temp);

        // Block top-left 2x2 area
        shape.SetCell(new int2(0, 0), true);
        shape.SetCell(new int2(1, 0), true);
        shape.SetCell(new int2(0, 1), true);
        shape.SetCell(new int2(1, 1), true);

        using var container = new GridContainer(shape, Allocator.Temp);

        // Try to add a 2x2 item - should fail at blocked position
        using var item = new GridShape2D(2, 2, Allocator.Temp);
        for (var y = 0; y < 2; y++)
        for (var x = 0; x < 2; x++)
            item.SetCell(new int2(x, y), true);

        // Should not be able to place at (0,0) due to pre-filled cells
        Assert.IsFalse(container.TryAddItemAt(item, new int2(0, 0)));

        // Should fail at (1,0) because it would overlap with blocked cells at (1,0) and (1,1)
        Assert.IsFalse(container.TryAddItemAt(item, new int2(1, 0)));

        // Grid state: [X][X][ ]
        //              [X][X][ ]
        //              [ ][ ][ ]
        // There's no valid 2x2 space in this configuration

        // But a 1x2 vertical item should fit in the rightmost column
        using var verticalItem = new GridShape2D(1, 2, Allocator.Temp);
        verticalItem.SetCell(new int2(0, 0), true);
        verticalItem.SetCell(new int2(0, 1), true);
        Assert.IsTrue(container.TryAddItemAt(verticalItem, new int2(2, 0)));
    }

    [Test]
    public void Constructor_NonCreatedShape_ThrowsException()
    {
        var nonCreatedShape = new GridShape2D(); // Default, non-created shape

        // This should ideally throw an exception or handle gracefully
        // The actual behavior depends on implementation
        Assert.Throws<ArgumentException>(() =>
        {
            using var container = new GridContainer(nonCreatedShape, Allocator.Temp);
        });
    }

    [Test]
    public void Constructor_Performance()
    {
        const int size = 200;
        using var shape = new GridShape2D(size, size, Allocator.Temp);

        // Create complex pattern
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            if ((x + y) % 3 == 0)
                shape.SetCell(new int2(x, y), true);
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        const int iterations = 100;

        for (var i = 0; i < iterations; i++)
        {
            using var container = new GridContainer(shape, Allocator.Temp);
            // Use container to prevent optimization
            var _ = container.FreeSpace;
        }

        stopwatch.Stop();
        UnityEngine.Debug.Log($"GridContainer shape constructor performance: {iterations} creations of {size}x{size} grid in {stopwatch.ElapsedMilliseconds}ms");
    }
}
