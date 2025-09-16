using System;
using DopeGrid;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

[TestFixture]
public class GridShape2DCopyToTests
{
    [Test]
    public void CopyTo_ValidTarget_CopiesAllData()
    {
        using var source = new GridShape2D(5, 5, Allocator.Temp);
        using var target = new GridShape2D(5, 5, Allocator.Temp);
        
        // Set some cells in source
        source.SetCell(new int2(0, 0), true);
        source.SetCell(new int2(2, 2), true);
        source.SetCell(new int2(4, 4), true);
        source.SetCell(new int2(1, 3), true);
        
        // Copy to target
        source.CopyTo(target);
        
        // Verify target has same data
        Assert.IsTrue(target.Equals(source), "Target should have same data as source");
        Assert.AreEqual(source.OccupiedSpaceCount, target.OccupiedSpaceCount);
        
        // Verify specific cells
        Assert.IsTrue(target.GetCell(new int2(0, 0)));
        Assert.IsTrue(target.GetCell(new int2(2, 2)));
        Assert.IsTrue(target.GetCell(new int2(4, 4)));
        Assert.IsTrue(target.GetCell(new int2(1, 3)));
        Assert.IsFalse(target.GetCell(new int2(1, 1)));
    }
    
    [Test]
    public void CopyTo_OverwritesExistingData()
    {
        using var source = new GridShape2D(3, 3, Allocator.Temp);
        using var target = new GridShape2D(3, 3, Allocator.Temp);
        
        // Set different patterns in source and target
        source.SetCell(new int2(0, 0), true);
        source.SetCell(new int2(1, 1), true);
        
        target.SetCell(new int2(2, 2), true);
        target.SetCell(new int2(0, 1), true);
        
        // Copy should overwrite target
        source.CopyTo(target);
        
        // Target should now match source exactly
        Assert.IsTrue(target.Equals(source));
        Assert.IsTrue(target.GetCell(new int2(0, 0)));
        Assert.IsTrue(target.GetCell(new int2(1, 1)));
        Assert.IsFalse(target.GetCell(new int2(2, 2))); // Should be cleared
        Assert.IsFalse(target.GetCell(new int2(0, 1))); // Should be cleared
    }
    
    [Test]
    public void CopyTo_EmptySource_ClearsTarget()
    {
        using var source = new GridShape2D(4, 4, Allocator.Temp);
        using var target = new GridShape2D(4, 4, Allocator.Temp);
        
        // Fill target with data
        for (var y = 0; y < 4; y++)
        for (var x = 0; x < 4; x++)
            target.SetCell(new int2(x, y), true);
        
        Assert.AreEqual(16, target.OccupiedSpaceCount);
        
        // Copy empty source to target
        source.CopyTo(target);
        
        // Target should now be empty
        Assert.AreEqual(0, target.OccupiedSpaceCount);
        Assert.IsTrue(target.Equals(source));
    }
    
    [Test]
    public void CopyTo_DifferentDimensions_ThrowsException()
    {
        using var source = new GridShape2D(3, 3, Allocator.Temp);
        using var target1 = new GridShape2D(4, 3, Allocator.Temp); // Different width
        using var target2 = new GridShape2D(3, 4, Allocator.Temp); // Different height
        using var target3 = new GridShape2D(2, 2, Allocator.Temp); // Both different
        
        Assert.Throws<ArgumentException>(() => source.CopyTo(target1), 
            "Should throw when width is different");
        Assert.Throws<ArgumentException>(() => source.CopyTo(target2), 
            "Should throw when height is different");
        Assert.Throws<ArgumentException>(() => source.CopyTo(target3), 
            "Should throw when both dimensions are different");
    }
    
    [Test]
    public void CopyTo_NonCreatedSource_ThrowsException()
    {
        var source = new GridShape2D(); // Not created
        using var target = new GridShape2D(3, 3, Allocator.Temp);
        
        Assert.Throws<InvalidOperationException>(() => source.CopyTo(target),
            "Should throw when source is not created");
    }
    
    [Test]
    public void CopyTo_NonCreatedTarget_ThrowsException()
    {
        using var source = new GridShape2D(3, 3, Allocator.Temp);
        var target = new GridShape2D(); // Not created
        
        Assert.Throws<InvalidOperationException>(() => source.CopyTo(target),
            "Should throw when target is not created");
    }
    
    [Test]
    public void CopyTo_ComplexShapes()
    {
        using var cross = Shapes.Cross(Allocator.Temp);
        using var target = new GridShape2D(3, 3, Allocator.Temp);
        
        // Fill target with different pattern
        target.SetCell(new int2(0, 0), true);
        target.SetCell(new int2(2, 2), true);
        
        // Copy cross to target
        cross.CopyTo(target);
        
        // Target should now be a cross
        Assert.IsTrue(target.Equals(cross));
        Assert.AreEqual(5, target.OccupiedSpaceCount);
        
        // Verify cross pattern
        Assert.IsFalse(target.GetCell(new int2(0, 0)));
        Assert.IsTrue(target.GetCell(new int2(1, 0)));
        Assert.IsFalse(target.GetCell(new int2(2, 0)));
        
        Assert.IsTrue(target.GetCell(new int2(0, 1)));
        Assert.IsTrue(target.GetCell(new int2(1, 1)));
        Assert.IsTrue(target.GetCell(new int2(2, 1)));
        
        Assert.IsFalse(target.GetCell(new int2(0, 2)));
        Assert.IsTrue(target.GetCell(new int2(1, 2)));
        Assert.IsFalse(target.GetCell(new int2(2, 2)));
    }
    
    [Test]
    public void CopyTo_SelfCopy_WorksCorrectly()
    {
        using var shape = new GridShape2D(3, 3, Allocator.Temp);
        shape.SetCell(new int2(0, 0), true);
        shape.SetCell(new int2(1, 1), true);
        shape.SetCell(new int2(2, 2), true);
        
        var originalCount = shape.OccupiedSpaceCount;
        
        // Copy to self should work without issues
        shape.CopyTo(shape);
        
        // Should remain unchanged
        Assert.AreEqual(originalCount, shape.OccupiedSpaceCount);
        Assert.IsTrue(shape.GetCell(new int2(0, 0)));
        Assert.IsTrue(shape.GetCell(new int2(1, 1)));
        Assert.IsTrue(shape.GetCell(new int2(2, 2)));
    }
    
    [Test]
    public void CopyTo_LargeShape_Performance()
    {
        const int size = 100;
        using var source = new GridShape2D(size, size, Allocator.Temp);
        using var target = new GridShape2D(size, size, Allocator.Temp);
        
        // Create a pattern
        for (var i = 0; i < size * size; i += 7)
        {
            var x = i % size;
            var y = i / size;
            source.SetCell(new int2(x, y), true);
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
}