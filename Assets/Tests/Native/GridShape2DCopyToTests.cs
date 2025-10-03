using System;
using DopeGrid.Native;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

[TestFixture]
public class GridShapeCopyToTests
{
    [Test]
    public void CopyTo_ValidTarget_CopiesAllData()
    {
        var source = new GridShape(5, 5, Allocator.Temp);
        var target = new GridShape(5, 5, Allocator.Temp);
        try
        {
            // Set some cells in source
            source.SetCellValue(0, 0, true);
            source.SetCellValue(2, 2, true);
            source.SetCellValue(4, 4, true);
            source.SetCellValue(1, 3, true);

            // Copy to target
            source.CopyTo(target);

            // Verify target has same data
            Assert.IsTrue(target.Equals(source), "Target should have same data as source");
            Assert.AreEqual(source.OccupiedSpaceCount, target.OccupiedSpaceCount);

            // Verify specific cells
            Assert.IsTrue(target.GetCellValue(0, 0));
            Assert.IsTrue(target.GetCellValue(2, 2));
            Assert.IsTrue(target.GetCellValue(4, 4));
            Assert.IsTrue(target.GetCellValue(1, 3));
            Assert.IsFalse(target.GetCellValue(1, 1));
        }
        finally
        {
            source.Dispose();
            target.Dispose();
        }
    }

    [Test]
    public void CopyTo_OverwritesExistingData()
    {
        var source = new GridShape(3, 3, Allocator.Temp);
        var target = new GridShape(3, 3, Allocator.Temp);
        try
        {
            // Set different patterns in source and target
            source.SetCellValue(0, 0, true);
            source.SetCellValue(1, 1, true);

            target.SetCellValue(2, 2, true);
            target.SetCellValue(0, 1, true);

            // Copy should overwrite target
            source.CopyTo(target);

            // Target should now match source exactly
            Assert.IsTrue(target.Equals(source));
            Assert.IsTrue(target.GetCellValue(0, 0));
            Assert.IsTrue(target.GetCellValue(1, 1));
            Assert.IsFalse(target.GetCellValue(2, 2)); // Should be cleared
            Assert.IsFalse(target.GetCellValue(0, 1)); // Should be cleared
        }
        finally
        {
            source.Dispose();
            target.Dispose();
        }
    }

    [Test]
    public void CopyTo_EmptySource_ClearsTarget()
    {
        var source = new GridShape(4, 4, Allocator.Temp);
        var target = new GridShape(4, 4, Allocator.Temp);
        try
        {
            // Fill target with data
            for (var y = 0; y < 4; y++)
            for (var x = 0; x < 4; x++)
                target.SetCellValue(x, y, true);

            Assert.AreEqual(16, target.OccupiedSpaceCount);

            // Copy empty source to target
            source.CopyTo(target);

            // Target should now be empty
            Assert.AreEqual(0, target.OccupiedSpaceCount);
            Assert.IsTrue(target.Equals(source));
        }
        finally
        {
            source.Dispose();
            target.Dispose();
        }
    }

    [Test]
    public void CopyTo_DifferentDimensions_ThrowsException()
    {
        var source = new GridShape(3, 3, Allocator.Temp);
        var target1 = new GridShape(4, 3, Allocator.Temp); // Different width
        var target2 = new GridShape(3, 4, Allocator.Temp); // Different height
        var target3 = new GridShape(2, 2, Allocator.Temp); // Both different
        try
        {
            Assert.Throws<ArgumentException>(() => source.CopyTo(target1),
                "Should throw when width is different");
            Assert.Throws<ArgumentException>(() => source.CopyTo(target2),
                "Should throw when height is different");
            Assert.Throws<ArgumentException>(() => source.CopyTo(target3),
                "Should throw when both dimensions are different");
        }
        finally
        {
            source.Dispose();
            target1.Dispose();
            target2.Dispose();
            target3.Dispose();
        }
    }

    [Test]
    public void CopyTo_ComplexShapes()
    {
        var cross = Shapes.Cross(Allocator.Temp);
        var target = new GridShape(3, 3, Allocator.Temp);
        try
        {
            // Fill target with different pattern
            target.SetCellValue(0, 0, true);
            target.SetCellValue(2, 2, true);

            // Copy cross to target
            cross.CopyTo(target);

            // Target should now be a cross
            Assert.IsTrue(target.Equals(cross));
            Assert.AreEqual(5, target.OccupiedSpaceCount);

            // Verify cross pattern
            Assert.IsFalse(target.GetCellValue(0, 0));
            Assert.IsTrue(target.GetCellValue(1, 0));
            Assert.IsFalse(target.GetCellValue(2, 0));

            Assert.IsTrue(target.GetCellValue(0, 1));
            Assert.IsTrue(target.GetCellValue(1, 1));
            Assert.IsTrue(target.GetCellValue(2, 1));

            Assert.IsFalse(target.GetCellValue(0, 2));
            Assert.IsTrue(target.GetCellValue(1, 2));
            Assert.IsFalse(target.GetCellValue(2, 2));
        }
        finally
        {
            cross.Dispose();
            target.Dispose();
        }
    }

    [Test]
    public void CopyTo_SelfCopy_WorksCorrectly()
    {
        var shape = new GridShape(3, 3, Allocator.Temp);
        try
        {
            shape.SetCellValue(0, 0, true);
            shape.SetCellValue(1, 1, true);
            shape.SetCellValue(2, 2, true);

            var originalCount = shape.OccupiedSpaceCount;

            // Copy to self should work without issues
            shape.CopyTo(shape);

            // Should remain unchanged
            Assert.AreEqual(originalCount, shape.OccupiedSpaceCount);
            Assert.IsTrue(shape.GetCellValue(0, 0));
            Assert.IsTrue(shape.GetCellValue(1, 1));
            Assert.IsTrue(shape.GetCellValue(2, 2));
        }
        finally
        {
            shape.Dispose();
        }
    }

    [Test]
    public void CopyTo_LargeShape_Performance()
    {
        const int size = 100;
        var source = new GridShape(size, size, Allocator.Temp);
        var target = new GridShape(size, size, Allocator.Temp);
        try
        {
            // Create a pattern
            for (var i = 0; i < size * size; i += 7)
            {
                var x = i % size;
                var y = i / size;
                source.SetCellValue(x, y, true);
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            const int iterations = 1000;

            for (var i = 0; i < iterations; i++)
            {
                source.CopyTo(target);
            }

            stopwatch.Stop();
            UnityEngine.Debug.Log($"CopyTo performance: {iterations} copies of {size}x{size} grid in {stopwatch.ElapsedMilliseconds}ms");

            // Verify copy worked
            Assert.IsTrue(target.Equals(source));
        }
        finally
        {
            source.Dispose();
            target.Dispose();
        }
    }
}
