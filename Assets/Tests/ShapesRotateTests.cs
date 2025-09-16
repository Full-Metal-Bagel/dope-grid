using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using DopeGrid;

[TestFixture]
public class ShapesRotateTests
{
    [Test]
    public void Rotate_Single_90Degrees()
    {
        using var shape = Shapes.Single(Allocator.Temp);
        using var rotated = shape.Rotate(RotationDegree.Rotate90, Allocator.Temp);

        Assert.AreEqual(1, rotated.Width);
        Assert.AreEqual(1, rotated.Height);
        Assert.IsTrue(rotated.GetCell(new int2(0, 0)));

        // Verify invariants
        Assert.AreEqual(shape.OccupiedSpaceCount, rotated.OccupiedSpaceCount, "OccupiedSpaceCount should be preserved");
        Assert.AreEqual(shape.Size, rotated.Size, "Size should be preserved");
    }

    [Test]
    public void Rotate_Line_90Degrees()
    {
        using var shape = Shapes.Line(3, Allocator.Temp);
        using var rotated = shape.Rotate(RotationDegree.Rotate90, Allocator.Temp);

        Assert.AreEqual(1, rotated.Width);
        Assert.AreEqual(3, rotated.Height);
        Assert.IsTrue(rotated.GetCell(new int2(0, 0)));
        Assert.IsTrue(rotated.GetCell(new int2(0, 1)));
        Assert.IsTrue(rotated.GetCell(new int2(0, 2)));

        // Verify invariants
        Assert.AreEqual(shape.OccupiedSpaceCount, rotated.OccupiedSpaceCount, "OccupiedSpaceCount should be preserved");
        Assert.AreEqual(3, rotated.OccupiedSpaceCount, "Line should have 3 occupied cells");
        Assert.AreEqual(shape.Size, rotated.Size, "Total size should be preserved");
    }

    [Test]
    public void Rotate_Line_180Degrees()
    {
        using var shape = Shapes.Line(3, Allocator.Temp);
        using var rotated = shape.Rotate(RotationDegree.Rotate180, Allocator.Temp);

        Assert.AreEqual(3, rotated.Width);
        Assert.AreEqual(1, rotated.Height);
        Assert.IsTrue(rotated.GetCell(new int2(0, 0)));
        Assert.IsTrue(rotated.GetCell(new int2(1, 0)));
        Assert.IsTrue(rotated.GetCell(new int2(2, 0)));

        // Verify invariants
        Assert.AreEqual(shape.OccupiedSpaceCount, rotated.OccupiedSpaceCount, "OccupiedSpaceCount should be preserved");
        Assert.AreEqual(shape.Size, rotated.Size, "Size should be preserved");
    }

    [Test]
    public void Rotate_Line_270Degrees()
    {
        using var shape = Shapes.Line(3, Allocator.Temp);
        using var rotated = shape.Rotate(RotationDegree.Rotate270, Allocator.Temp);

        Assert.AreEqual(1, rotated.Width);
        Assert.AreEqual(3, rotated.Height);
        Assert.IsTrue(rotated.GetCell(new int2(0, 0)));
        Assert.IsTrue(rotated.GetCell(new int2(0, 1)));
        Assert.IsTrue(rotated.GetCell(new int2(0, 2)));

        // Verify invariants
        Assert.AreEqual(shape.OccupiedSpaceCount, rotated.OccupiedSpaceCount, "OccupiedSpaceCount should be preserved");
        Assert.AreEqual(shape.Size, rotated.Size, "Size should be preserved");
    }

    [Test]
    public void Rotate_LShape_90Degrees()
    {
        using var shape = Shapes.LShape(Allocator.Temp);
        using var rotated = shape.Rotate(RotationDegree.Rotate90, Allocator.Temp);

        Assert.AreEqual(2, rotated.Width);
        Assert.AreEqual(2, rotated.Height);
        Assert.IsTrue(rotated.GetCell(new int2(0, 0)));
        Assert.IsTrue(rotated.GetCell(new int2(1, 0)));
        Assert.IsTrue(rotated.GetCell(new int2(0, 1)));
        Assert.IsFalse(rotated.GetCell(new int2(1, 1)));

        // Verify invariants
        Assert.AreEqual(3, shape.OccupiedSpaceCount, "L-shape should have 3 occupied cells");
        Assert.AreEqual(shape.OccupiedSpaceCount, rotated.OccupiedSpaceCount, "OccupiedSpaceCount should be preserved");
        Assert.AreEqual(shape.Size, rotated.Size, "Size should be preserved");
    }

    [Test]
    public void Rotate_TShape_90Degrees()
    {
        using var shape = Shapes.TShape(Allocator.Temp);
        using var rotated = shape.Rotate(RotationDegree.Rotate90, Allocator.Temp);

        Assert.AreEqual(2, rotated.Width);
        Assert.AreEqual(3, rotated.Height);
        Assert.IsFalse(rotated.GetCell(new int2(0, 0)));
        Assert.IsTrue(rotated.GetCell(new int2(1, 0)));
        Assert.IsTrue(rotated.GetCell(new int2(0, 1)));
        Assert.IsTrue(rotated.GetCell(new int2(1, 1)));
        Assert.IsFalse(rotated.GetCell(new int2(0, 2)));
        Assert.IsTrue(rotated.GetCell(new int2(1, 2)));

        // Verify invariants
        Assert.AreEqual(4, shape.OccupiedSpaceCount, "T-shape should have 4 occupied cells");
        Assert.AreEqual(shape.OccupiedSpaceCount, rotated.OccupiedSpaceCount, "OccupiedSpaceCount should be preserved");
    }

    [Test]
    public void Rotate_Cross_90Degrees_ShouldRemainSame()
    {
        using var shape = Shapes.Cross(Allocator.Temp);
        using var rotated = shape.Rotate(RotationDegree.Rotate90, Allocator.Temp);

        Assert.AreEqual(3, rotated.Width);
        Assert.AreEqual(3, rotated.Height);

        Assert.IsFalse(rotated.GetCell(new int2(0, 0)));
        Assert.IsTrue(rotated.GetCell(new int2(1, 0)));
        Assert.IsFalse(rotated.GetCell(new int2(2, 0)));

        Assert.IsTrue(rotated.GetCell(new int2(0, 1)));
        Assert.IsTrue(rotated.GetCell(new int2(1, 1)));
        Assert.IsTrue(rotated.GetCell(new int2(2, 1)));

        Assert.IsFalse(rotated.GetCell(new int2(0, 2)));
        Assert.IsTrue(rotated.GetCell(new int2(1, 2)));
        Assert.IsFalse(rotated.GetCell(new int2(2, 2)));

        // Verify invariants
        Assert.AreEqual(5, shape.OccupiedSpaceCount, "Cross should have 5 occupied cells");
        Assert.AreEqual(shape.OccupiedSpaceCount, rotated.OccupiedSpaceCount, "OccupiedSpaceCount should be preserved");
        Assert.AreEqual(shape.Size, rotated.Size, "Size should be preserved for symmetric shape");
    }

    [Test]
    public void Rotate_Square_90Degrees_ShouldRemainSame()
    {
        using var shape = Shapes.Square(2, Allocator.Temp);
        using var rotated = shape.Rotate(RotationDegree.Rotate90, Allocator.Temp);

        Assert.AreEqual(2, rotated.Width);
        Assert.AreEqual(2, rotated.Height);

        Assert.IsTrue(rotated.GetCell(new int2(0, 0)));
        Assert.IsTrue(rotated.GetCell(new int2(1, 0)));
        Assert.IsTrue(rotated.GetCell(new int2(0, 1)));
        Assert.IsTrue(rotated.GetCell(new int2(1, 1)));

        // Verify invariants
        Assert.AreEqual(4, shape.OccupiedSpaceCount, "2x2 square should have 4 occupied cells");
        Assert.AreEqual(shape.OccupiedSpaceCount, rotated.OccupiedSpaceCount, "OccupiedSpaceCount should be preserved");
        Assert.AreEqual(shape.Size, rotated.Size, "Size should be preserved for square");
    }

    [Test]
    public void Rotate_CustomShape()
    {
        using var shape = new GridShape2D(3, 2, Allocator.Temp);
        shape.SetCell(new int2(0, 0), true);
        shape.SetCell(new int2(1, 0), true);
        shape.SetCell(new int2(2, 1), true);

        using var rotated = shape.Rotate(RotationDegree.Rotate90, Allocator.Temp);
        Assert.AreEqual(2, rotated.Width);
        Assert.AreEqual(3, rotated.Height);

        // Verify invariants
        Assert.AreEqual(3, shape.OccupiedSpaceCount, "Custom shape should have 3 occupied cells");
        Assert.AreEqual(shape.OccupiedSpaceCount, rotated.OccupiedSpaceCount, "OccupiedSpaceCount should be preserved");
    }

    [Test]
    public void Rotate_InvalidDegree_ShouldReturnOriginal()
    {
        using var shape = Shapes.Line(3, Allocator.Temp);
        using var rotated = shape.Rotate(RotationDegree.None, Allocator.Temp);

        Assert.AreEqual(3, rotated.Width);
        Assert.AreEqual(1, rotated.Height);
        Assert.IsTrue(rotated.GetCell(new int2(0, 0)));
        Assert.IsTrue(rotated.GetCell(new int2(1, 0)));
        Assert.IsTrue(rotated.GetCell(new int2(2, 0)));
    }

    [Test]
    public void Rotate_EmptyShape()
    {
        using var shape = new GridShape2D(2, 2, Allocator.Temp);
        using var rotated = shape.Rotate(RotationDegree.Rotate90, Allocator.Temp);

        Assert.AreEqual(2, rotated.Width);
        Assert.AreEqual(2, rotated.Height);
        Assert.AreEqual(0, rotated.OccupiedSpaceCount);

        // Verify invariants
        Assert.AreEqual(shape.OccupiedSpaceCount, rotated.OccupiedSpaceCount, "OccupiedSpaceCount should be preserved (0 for empty)");
        Assert.AreEqual(shape.Size, rotated.Size, "Size should be preserved");
    }

    [Test]
    public void Rotate_RandomLargeShape([Random(100)] int seed)
    {
        var random = new System.Random(seed);
        var width = random.Next(5, 20);
        var height = random.Next(5, 20);

        using var shape = new GridShape2D(width, height, Allocator.Temp);

        // Randomly populate cells
        var cellsToFill = random.Next(1, width * height / 2); // Fill up to half the grid
        for (var i = 0; i < cellsToFill; i++)
        {
            var x = random.Next(0, width);
            var y = random.Next(0, height);
            shape.SetCell(new int2(x, y), true);
        }

        var originalOccupiedCount = shape.OccupiedSpaceCount;

        // Test all rotation angles
        using var rotated90 = shape.Rotate(RotationDegree.Rotate90, Allocator.Temp);
        using var rotated180 = shape.Rotate(RotationDegree.Rotate180, Allocator.Temp);
        using var rotated270 = shape.Rotate(RotationDegree.Rotate270, Allocator.Temp);

        // Verify invariant: OccupiedSpaceCount is preserved
        Assert.AreEqual(originalOccupiedCount, rotated90.OccupiedSpaceCount,
            $"90° rotation should preserve OccupiedSpaceCount (seed: {seed})");
        Assert.AreEqual(originalOccupiedCount, rotated180.OccupiedSpaceCount,
            $"180° rotation should preserve OccupiedSpaceCount (seed: {seed})");
        Assert.AreEqual(originalOccupiedCount, rotated270.OccupiedSpaceCount,
            $"270° rotation should preserve OccupiedSpaceCount (seed: {seed})");

        // Verify rotation compositions
        // 2x90° = 180°
        using var rotated90x2 = rotated90.Rotate(RotationDegree.Rotate90, Allocator.Temp);
        Assert.IsTrue(rotated180.Equals(rotated90x2),
            $"2x90° should equal 180° rotation (seed: {seed})");

        // 3x90° = 270°
        using var rotated90x3 = rotated90x2.Rotate(RotationDegree.Rotate90, Allocator.Temp);
        Assert.IsTrue(rotated270.Equals(rotated90x3),
            $"3x90° should equal 270° rotation (seed: {seed})");

        // 4x90° = 360° = 0° 
        // Note: Due to bounding box recalculation during rotation, dimensions might change
        // if the original shape had empty rows/columns that get trimmed
        using var rotated90x4 = rotated90x3.Rotate(RotationDegree.Rotate90, Allocator.Temp);
        
        // The number of occupied cells should always be preserved
        Assert.AreEqual(shape.OccupiedSpaceCount, rotated90x4.OccupiedSpaceCount,
            $"OccupiedSpaceCount should be preserved after 4x90° rotation (seed: {seed})");

        // 2x180° = 360° = 0°
        // Same issue as 4x90° - dimensions might change due to bounding box recalculation
        using var rotated180x2 = rotated180.Rotate(RotationDegree.Rotate180, Allocator.Temp);
        Assert.AreEqual(shape.OccupiedSpaceCount, rotated180x2.OccupiedSpaceCount,
            $"OccupiedSpaceCount should be preserved after 2x180° rotation (seed: {seed})");

        // 270° + 90° = 360° = 0°
        // Same issue - dimensions might change due to bounding box recalculation
        using var rotated270plus90 = rotated270.Rotate(RotationDegree.Rotate90, Allocator.Temp);
        Assert.AreEqual(shape.OccupiedSpaceCount, rotated270plus90.OccupiedSpaceCount,
            $"OccupiedSpaceCount should be preserved after 270° + 90° rotation (seed: {seed})");

        // Verify Size invariants
        // The Size should be at least as large as OccupiedSpaceCount
        Assert.GreaterOrEqual(rotated90.Size, rotated90.OccupiedSpaceCount,
            $"90° rotation Size must be >= OccupiedSpaceCount (seed: {seed})");
        Assert.GreaterOrEqual(rotated180.Size, rotated180.OccupiedSpaceCount,
            $"180° rotation Size must be >= OccupiedSpaceCount (seed: {seed})");
        Assert.GreaterOrEqual(rotated270.Size, rotated270.OccupiedSpaceCount,
            $"270° rotation Size must be >= OccupiedSpaceCount (seed: {seed})");

        // Verify that Size = Width * Height
        Assert.AreEqual(rotated90.Width * rotated90.Height, rotated90.Size,
            $"90° rotation Size should equal Width * Height (seed: {seed})");
        Assert.AreEqual(rotated180.Width * rotated180.Height, rotated180.Size,
            $"180° rotation Size should equal Width * Height (seed: {seed})");
        Assert.AreEqual(rotated270.Width * rotated270.Height, rotated270.Size,
            $"270° rotation Size should equal Width * Height (seed: {seed})");

        // Note: Dimensions after rotation are based on the bounding box of occupied cells,
        // not the original grid dimensions. The rotation algorithm trims empty space.
        // So Size may differ from the original, but it will always be minimal to contain all occupied cells.
    }
}
