using System;
using DopeGrid;
using DopeGrid.Native;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

[TestFixture]
public class GridContainerShapeConstructorTests
{
    [Test]
    public void Constructor_WithEmptyShape_CreatesEmptyContainer()
    {
        var shape = new GridShape(5, 5, Allocator.Temp);
        var container = new GridBoard(shape, Allocator.Temp);
        try
        {
            Assert.AreEqual(5, container.Width);
            Assert.AreEqual(5, container.Height);
            Assert.AreEqual(0, container.ItemCount);
            Assert.AreEqual(25, container.FreeSpace);

            // Verify all cells are unoccupied
            for (var y = 0; y < 5; y++)
            for (var x = 0; x < 5; x++)
            {
                Assert.IsFalse(container.IsCellOccupied(new GridPosition(x, y)));
            }
        }
        finally
        {
            shape.Dispose();
            container.Dispose();
        }
    }

    [Test]
    public void Constructor_WithPartiallyFilledShape_CreatesCorrectContainer()
    {
        var shape = new GridShape(4, 4, Allocator.Temp);
        var container = new GridBoard(shape, Allocator.Temp);
        try
        {
            // Fill some cells to create a pattern
            shape.SetCellValue(new GridPosition(0, 0), true);
            shape.SetCellValue(new GridPosition(1, 1), true);
            shape.SetCellValue(new GridPosition(2, 2), true);
            shape.SetCellValue(new GridPosition(3, 3), true);

            Assert.AreEqual(4, container.Width);
            Assert.AreEqual(4, container.Height);
            Assert.AreEqual(0, container.ItemCount); // No items added yet
            Assert.AreEqual(12, container.FreeSpace); // 16 - 4 occupied cells

            // Verify the pattern was copied
            Assert.IsTrue(container.IsCellOccupied(new GridPosition(0, 0)));
            Assert.IsTrue(container.IsCellOccupied(new GridPosition(1, 1)));
            Assert.IsTrue(container.IsCellOccupied(new GridPosition(2, 2)));
            Assert.IsTrue(container.IsCellOccupied(new GridPosition(3, 3)));

            // Verify other cells are free
            Assert.IsFalse(container.IsCellOccupied(new GridPosition(0, 1)));
            Assert.IsFalse(container.IsCellOccupied(new GridPosition(1, 0)));
        }
        finally
        {
            shape.Dispose();
            container.Dispose();
        }
    }

    [Test]
    public void Constructor_WithComplexShape_PreservesInitialState()
    {
        var cross = Shapes.Cross(Allocator.Temp);
        var container = new GridBoard(cross, Allocator.Temp);
        try
        {
            Assert.AreEqual(3, container.Width);
            Assert.AreEqual(3, container.Height);
            Assert.AreEqual(4, container.FreeSpace); // Cross has 5 occupied cells out of 9

            // Verify cross pattern
            Assert.IsFalse(container.IsCellOccupied(new GridPosition(0, 0)));
            Assert.IsTrue(container.IsCellOccupied(new GridPosition(1, 0)));
            Assert.IsFalse(container.IsCellOccupied(new GridPosition(2, 0)));

            Assert.IsTrue(container.IsCellOccupied(new GridPosition(0, 1)));
            Assert.IsTrue(container.IsCellOccupied(new GridPosition(1, 1)));
            Assert.IsTrue(container.IsCellOccupied(new GridPosition(2, 1)));

            Assert.IsFalse(container.IsCellOccupied(new GridPosition(0, 2)));
            Assert.IsTrue(container.IsCellOccupied(new GridPosition(1, 2)));
            Assert.IsFalse(container.IsCellOccupied(new GridPosition(2, 2)));
        }
        finally
        {
            cross.Dispose();
            container.Dispose();
        }
    }

    [Test]
    public void Constructor_ClonesShapeIndependently()
    {
        var originalShape = new GridShape(3, 3, Allocator.Temp);
        originalShape.SetCellValue(new GridPosition(1, 1), true);

        var container = new GridBoard(originalShape, Allocator.Temp);
        try
        {
            // Modify original shape after creating container
            originalShape.SetCellValue(new GridPosition(0, 0), true);
            originalShape.SetCellValue(new GridPosition(2, 2), true);

            // Container should not be affected
            Assert.IsFalse(container.IsCellOccupied(new GridPosition(0, 0)));
            Assert.IsTrue(container.IsCellOccupied(new GridPosition(1, 1)));
            Assert.IsFalse(container.IsCellOccupied(new GridPosition(2, 2)));
            Assert.AreEqual(8, container.FreeSpace); // Only center cell occupied
        }
        finally
        {
            originalShape.Dispose();
            container.Dispose();
        }
    }

    [Test]
    public void Constructor_InitializedGridMatchesInput()
    {
        var shape = new GridShape(3, 3, Allocator.Temp);
        shape.SetCellValue(new GridPosition(0, 0), true);
        shape.SetCellValue(new GridPosition(2, 0), true);
        shape.SetCellValue(new GridPosition(1, 1), true);

        var container = new GridBoard(shape, Allocator.Temp);
        try
        {
            var initializedGrid = container.InitializedGrid;

            // InitializedGrid should match the input shape
            Assert.AreEqual(3, initializedGrid.Width);
            Assert.AreEqual(3, initializedGrid.Height);
            Assert.AreEqual(3, initializedGrid.OccupiedSpaceCount);

            Assert.IsTrue(initializedGrid.GetCellValue(new GridPosition(0, 0)));
            Assert.IsTrue(initializedGrid.GetCellValue(new GridPosition(2, 0)));
            Assert.IsTrue(initializedGrid.GetCellValue(new GridPosition(1, 1)));
        }
        finally
        {
            shape.Dispose();
            container.Dispose();
        }
    }

    [Test]
    public void Constructor_CurrentGridMatchesInitialGrid()
    {
        var shape = new GridShape(4, 3, Allocator.Temp);
        shape.SetCellValue(new GridPosition(1, 0), true);
        shape.SetCellValue(new GridPosition(2, 1), true);

        var container = new GridBoard(shape, Allocator.Temp);
        try
        {
            var currentGrid = container.CurrentGrid;
            var initializedGrid = container.InitializedGrid;

            // At creation, CurrentGrid should match InitializedGrid
            Assert.IsTrue(currentGrid.Equals(initializedGrid));
        }
        finally
        {
            shape.Dispose();
            container.Dispose();
        }
    }

    [Test]
    public void Constructor_WithLargeShape_WorksCorrectly()
    {
        const int size = 100;
        var largeShape = new GridShape(size, size, Allocator.Temp);
        var container = new GridBoard(largeShape, Allocator.Temp);
        try
        {
            // Create a sparse pattern
            for (var i = 0; i < size * size; i += 7)
            {
                var x = i % size;
                var y = i / size;
                largeShape.SetCellValue(new GridPosition(x, y), true);
            }

            Assert.AreEqual(size, container.Width);
            Assert.AreEqual(size, container.Height);

            var expectedOccupied = (size * size + 6) / 7; // Ceiling division
            var expectedFree = size * size - expectedOccupied;
            Assert.AreEqual(expectedFree, container.FreeSpace);
        }
        finally
        {
            largeShape.Dispose();
            container.Dispose();
        }
    }

    [Test]
    public void Constructor_WithDifferentAllocators()
    {
        var shape = new GridShape(3, 3, Allocator.Temp);
        shape.SetCellValue(new GridPosition(1, 1), true);

        // Test with Temp allocator
        var container1 = new GridBoard(shape, Allocator.Temp);
        try
        {
            Assert.AreEqual(8, container1.FreeSpace);
        }
        finally
        {
            container1.Dispose();
        }

        // Test with TempJob allocator
        var container2 = new GridBoard(shape, Allocator.TempJob);
        try
        {
            Assert.AreEqual(8, container2.FreeSpace);
        }
        finally
        {
            container2.Dispose();
        }

        // Test with Persistent allocator
        var container3 = new GridBoard(shape, Allocator.Persistent);
        try
        {
            Assert.AreEqual(8, container3.FreeSpace);
        }
        finally
        {
            container3.Dispose();
        }

        shape.Dispose();
    }

    [Test]
    public void Constructor_MinimalShape_1x1()
    {
        var shape = new GridShape(1, 1, Allocator.Temp);
        var container = new GridBoard(shape, Allocator.Temp);
        try
        {
            Assert.AreEqual(1, container.Width);
            Assert.AreEqual(1, container.Height);
            Assert.AreEqual(1, container.FreeSpace);
        }
        finally
        {
            container.Dispose();
        }

        shape.SetCellValue(new GridPosition(0, 0), true);
        var fullContainer = new GridBoard(shape, Allocator.Temp);
        try
        {
            Assert.AreEqual(0, fullContainer.FreeSpace);
        }
        finally
        {
            shape.Dispose();
            fullContainer.Dispose();
        }
    }

    [Test]
    public void Constructor_RectangularShapes()
    {
        // Wide rectangle
        var wideShape = new GridShape(10, 2, Allocator.Temp);
        var wideContainer = new GridBoard(wideShape, Allocator.Temp);
        try
        {
            Assert.AreEqual(10, wideContainer.Width);
            Assert.AreEqual(2, wideContainer.Height);
            Assert.AreEqual(20, wideContainer.FreeSpace);
        }
        finally
        {
            wideShape.Dispose();
            wideContainer.Dispose();
        }

        // Tall rectangle
        var tallShape = new GridShape(2, 10, Allocator.Temp);
        var tallContainer = new GridBoard(tallShape, Allocator.Temp);
        try
        {
            Assert.AreEqual(2, tallContainer.Width);
            Assert.AreEqual(10, tallContainer.Height);
            Assert.AreEqual(20, tallContainer.FreeSpace);
        }
        finally
        {
            tallShape.Dispose();
            tallContainer.Dispose();
        }
    }

    [Test]
    public void Constructor_PreFilledShape_BlocksItemPlacement()
    {
        var shape = new GridShape(3, 3, Allocator.Temp);
        var container = new GridBoard(shape, Allocator.Temp);
        var item = new GridShape(2, 2, Allocator.Temp);
        var verticalItem = new GridShape(1, 2, Allocator.Temp);
        try
        {
            // Block top-left 2x2 area
            shape.SetCellValue(new GridPosition(0, 0), true);
            shape.SetCellValue(new GridPosition(1, 0), true);
            shape.SetCellValue(new GridPosition(0, 1), true);
            shape.SetCellValue(new GridPosition(1, 1), true);

            // Try to add a 2x2 item - should fail at blocked position
            for (var y = 0; y < 2; y++)
            for (var x = 0; x < 2; x++)
                item.SetCellValue(new GridPosition(x, y), true);

            // Should not be able to place at (0,0) due to pre-filled cells
            Assert.That(container.TryAddItemAt(item.GetOrCreateImmutable(), new GridPosition(0, 0)), Is.EqualTo(-1));

            // Should fail at (1,0) because it would overlap with blocked cells at (1,0) and (1,1)
            Assert.That(container.TryAddItemAt(item.GetOrCreateImmutable(), new GridPosition(1, 0)), Is.EqualTo(-1));

            // Grid state: [X][X][ ]
            //              [X][X][ ]
            //              [ ][ ][ ]
            // There's no valid 2x2 space in this configuration

            // But a 1x2 vertical item should fit in the rightmost column
            verticalItem.SetCellValue(new GridPosition(0, 0), true);
            verticalItem.SetCellValue(new GridPosition(0, 1), true);
            Assert.That(container.TryAddItemAt(verticalItem.GetOrCreateImmutable(), new GridPosition(2, 0)), Is.EqualTo(0));
        }
        finally
        {
            shape.Dispose();
            item.Dispose();
            verticalItem.Dispose();
            container.Dispose();
        }
    }

    [Test]
    public void Constructor_NonCreatedShape_ThrowsException()
    {
        var nonCreatedShape = new GridShape(); // Default, non-created shape

        // This should ideally throw an exception or handle gracefully
        // The actual behavior depends on implementation
        Assert.Throws<ArgumentException>(() =>
        {
            var container = new GridBoard(nonCreatedShape, Allocator.Temp);
            container.Dispose();
        });
    }

    [Test]
    public void Constructor_Performance()
    {
        const int size = 200;
        var shape = new GridShape(size, size, Allocator.Temp);
        try
        {
            // Create complex pattern
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                if ((x + y) % 3 == 0)
                    shape.SetCellValue(new GridPosition(x, y), true);
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            const int iterations = 100;

            for (var i = 0; i < iterations; i++)
            {
                var container = new GridBoard(shape, Allocator.Temp);
                try
                {
                    // Use container to prevent optimization
                    var _ = container.FreeSpace;
                }
                finally
                {
                    container.Dispose();
                }
            }

            stopwatch.Stop();
            UnityEngine.Debug.Log($"GridContainer shape constructor performance: {iterations} creations of {size}x{size} grid in {stopwatch.ElapsedMilliseconds}ms");
        }
        finally
        {
            shape.Dispose();
        }
    }
}
