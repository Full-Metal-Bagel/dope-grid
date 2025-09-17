using DopeGrid;
using NUnit.Framework;
using Unity.Collections;

public class ShapesTrimTests
{
    [Test]
    public void IsTrimmed_EmptyShape_ReturnsTrue()
    {
        var shape = new GridShape2D(0, 0, Allocator.Temp);
        Assert.IsTrue(shape.IsTrimmed());
        shape.Dispose();

        var shape2 = new GridShape2D(5, 0, Allocator.Temp);
        Assert.IsTrue(shape2.IsTrimmed());
        shape2.Dispose();

        var shape3 = new GridShape2D(0, 5, Allocator.Temp);
        Assert.IsTrue(shape3.IsTrimmed());
        shape3.Dispose();
    }

    [Test]
    public void IsTrimmed_SingleCellInCorner_ReturnsTrue()
    {
        var shape = new GridShape2D(1, 1, Allocator.Temp);
        shape.SetCell(0, 0, true);
        Assert.IsTrue(shape.IsTrimmed());
        shape.Dispose();
    }

    [Test]
    public void IsTrimmed_FullyOccupiedShape_ReturnsTrue()
    {
        var shape = new GridShape2D(3, 3, Allocator.Temp);
        for (var y = 0; y < 3; y++)
        for (var x = 0; x < 3; x++)
            shape.SetCell(x, y, true);

        Assert.IsTrue(shape.IsTrimmed());
        shape.Dispose();
    }

    [Test]
    public void IsTrimmed_ShapeWithEmptyTopRow_ReturnsFalse()
    {
        var shape = new GridShape2D(3, 3, Allocator.Temp);
        // Set cells in bottom two rows only
        for (var y = 1; y < 3; y++)
        for (var x = 0; x < 3; x++)
            shape.SetCell(x, y, true);

        Assert.IsFalse(shape.IsTrimmed());
        shape.Dispose();
    }

    [Test]
    public void IsTrimmed_ShapeWithEmptyBottomRow_ReturnsFalse()
    {
        var shape = new GridShape2D(3, 3, Allocator.Temp);
        // Set cells in top two rows only
        for (var y = 0; y < 2; y++)
        for (var x = 0; x < 3; x++)
            shape.SetCell(x, y, true);

        Assert.IsFalse(shape.IsTrimmed());
        shape.Dispose();
    }

    [Test]
    public void IsTrimmed_ShapeWithEmptyLeftColumn_ReturnsFalse()
    {
        var shape = new GridShape2D(3, 3, Allocator.Temp);
        // Set cells in right two columns only
        for (var y = 0; y < 3; y++)
        for (var x = 1; x < 3; x++)
            shape.SetCell(x, y, true);

        Assert.IsFalse(shape.IsTrimmed());
        shape.Dispose();
    }

    [Test]
    public void IsTrimmed_ShapeWithEmptyRightColumn_ReturnsFalse()
    {
        var shape = new GridShape2D(3, 3, Allocator.Temp);
        // Set cells in left two columns only
        for (var y = 0; y < 3; y++)
        for (var x = 0; x < 2; x++)
            shape.SetCell(x, y, true);

        Assert.IsFalse(shape.IsTrimmed());
        shape.Dispose();
    }

    [Test]
    public void IsTrimmed_CrossShape_ReturnsTrue()
    {
        var shape = new GridShape2D(3, 3, Allocator.Temp);
        // Create a cross shape
        shape.SetCell(1, 0, true); // Top
        shape.SetCell(0, 1, true); // Left
        shape.SetCell(1, 1, true); // Center
        shape.SetCell(2, 1, true); // Right
        shape.SetCell(1, 2, true); // Bottom

        Assert.IsTrue(shape.IsTrimmed());
        shape.Dispose();
    }

    [Test]
    public void IsTrimmed_DiagonalLine_ReturnsTrue()
    {
        var shape = new GridShape2D(3, 3, Allocator.Temp);
        shape.SetCell(0, 0, true);
        shape.SetCell(1, 1, true);
        shape.SetCell(2, 2, true);

        Assert.IsTrue(shape.IsTrimmed());
        shape.Dispose();
    }

    [Test]
    public void IsTrimmed_ReadOnlyShape_WorksCorrectly()
    {
        var shape = new GridShape2D(3, 3, Allocator.Temp);
        shape.SetCell(0, 0, true);
        shape.SetCell(2, 2, true);

        var readOnly = shape.ToReadOnly();
        Assert.IsTrue(readOnly.IsTrimmed());

        shape.Dispose();
    }

    [Test]
    public void Trim_EmptyShape_ReturnsEmptyShape()
    {
        var shape = new GridShape2D(5, 5, Allocator.Temp);
        var trimmed = shape.ToReadOnly().Trim(Allocator.Temp);

        Assert.AreEqual(0, trimmed.Width);
        Assert.AreEqual(0, trimmed.Height);

        shape.Dispose();
        trimmed.Dispose();
    }

    [Test]
    public void Trim_AlreadyTrimmedShape_ReturnsIdenticalCopy()
    {
        var shape = new GridShape2D(3, 3, Allocator.Temp);
        shape.SetCell(0, 0, true);
        shape.SetCell(2, 0, true);
        shape.SetCell(1, 1, true);
        shape.SetCell(0, 2, true);
        shape.SetCell(2, 2, true);

        var trimmed = shape.ToReadOnly().Trim(Allocator.Temp);

        Assert.AreEqual(shape.Width, trimmed.Width);
        Assert.AreEqual(shape.Height, trimmed.Height);

        // Verify all cells match
        for (var y = 0; y < 3; y++)
        for (var x = 0; x < 3; x++)
            Assert.AreEqual(shape.GetCell(x, y), trimmed.GetCell(x, y));

        shape.Dispose();
        trimmed.Dispose();
    }

    [Test]
    public void Trim_ShapeWithEmptyBorders_RemovesEmptySpace()
    {
        var shape = new GridShape2D(5, 5, Allocator.Temp);

        // Create a 2x2 shape in the center (offset by 1,1)
        shape.SetCell(1, 1, true);
        shape.SetCell(2, 1, true);
        shape.SetCell(1, 2, true);
        shape.SetCell(2, 2, true);

        var trimmed = shape.ToReadOnly().Trim(Allocator.Temp);

        Assert.AreEqual(2, trimmed.Width);
        Assert.AreEqual(2, trimmed.Height);

        // Verify the trimmed shape has all cells set
        for (var y = 0; y < 2; y++)
        for (var x = 0; x < 2; x++)
            Assert.IsTrue(trimmed.GetCell(x, y));

        shape.Dispose();
        trimmed.Dispose();
    }

    [Test]
    public void Trim_VerticalLine_TrimsHorizontally()
    {
        var shape = new GridShape2D(5, 3, Allocator.Temp);

        // Create a vertical line in column 2
        shape.SetCell(2, 0, true);
        shape.SetCell(2, 1, true);
        shape.SetCell(2, 2, true);

        var trimmed = shape.ToReadOnly().Trim(Allocator.Temp);

        Assert.AreEqual(1, trimmed.Width);
        Assert.AreEqual(3, trimmed.Height);

        for (var y = 0; y < 3; y++)
            Assert.IsTrue(trimmed.GetCell(0, y));

        shape.Dispose();
        trimmed.Dispose();
    }

    [Test]
    public void Trim_HorizontalLine_TrimsVertically()
    {
        var shape = new GridShape2D(3, 5, Allocator.Temp);

        // Create a horizontal line in row 2
        shape.SetCell(0, 2, true);
        shape.SetCell(1, 2, true);
        shape.SetCell(2, 2, true);

        var trimmed = shape.ToReadOnly().Trim(Allocator.Temp);

        Assert.AreEqual(3, trimmed.Width);
        Assert.AreEqual(1, trimmed.Height);

        for (var x = 0; x < 3; x++)
            Assert.IsTrue(trimmed.GetCell(x, 0));

        shape.Dispose();
        trimmed.Dispose();
    }

    [Test]
    public void Trim_SingleCellInCenter_TrimsToSingleCell()
    {
        var shape = new GridShape2D(5, 5, Allocator.Temp);
        shape.SetCell(2, 2, true);

        var trimmed = shape.ToReadOnly().Trim(Allocator.Temp);

        Assert.AreEqual(1, trimmed.Width);
        Assert.AreEqual(1, trimmed.Height);
        Assert.IsTrue(trimmed.GetCell(0, 0));

        shape.Dispose();
        trimmed.Dispose();
    }

    [Test]
    public void Trim_SparseShape_PreservesRelativePositions()
    {
        var shape = new GridShape2D(6, 6, Allocator.Temp);

        // Create a sparse pattern with empty borders
        shape.SetCell(2, 1, true); // Top-left of trimmed area
        shape.SetCell(4, 1, true); // Top-right of trimmed area
        shape.SetCell(3, 2, true); // Middle
        shape.SetCell(2, 3, true); // Bottom-left of trimmed area
        shape.SetCell(4, 3, true); // Bottom-right of trimmed area

        var trimmed = shape.ToReadOnly().Trim(Allocator.Temp);

        Assert.AreEqual(3, trimmed.Width);  // From x=2 to x=4
        Assert.AreEqual(3, trimmed.Height); // From y=1 to y=3

        // Verify the pattern is preserved
        Assert.IsTrue(trimmed.GetCell(0, 0));  // Was (2,1)
        Assert.IsTrue(trimmed.GetCell(2, 0));  // Was (4,1)
        Assert.IsTrue(trimmed.GetCell(1, 1));  // Was (3,2)
        Assert.IsTrue(trimmed.GetCell(0, 2));  // Was (2,3)
        Assert.IsTrue(trimmed.GetCell(2, 2));  // Was (4,3)

        // Verify empty cells
        Assert.IsFalse(trimmed.GetCell(1, 0));
        Assert.IsFalse(trimmed.GetCell(0, 1));
        Assert.IsFalse(trimmed.GetCell(2, 1));
        Assert.IsFalse(trimmed.GetCell(1, 2));

        shape.Dispose();
        trimmed.Dispose();
    }

    [Test]
    public void Trim_LShape_MaintainsShape()
    {
        var shape = new GridShape2D(5, 5, Allocator.Temp);

        // Create an L shape with offset
        shape.SetCell(1, 1, true);
        shape.SetCell(1, 2, true);
        shape.SetCell(1, 3, true);
        shape.SetCell(2, 3, true);
        shape.SetCell(3, 3, true);

        var trimmed = shape.ToReadOnly().Trim(Allocator.Temp);

        Assert.AreEqual(3, trimmed.Width);
        Assert.AreEqual(3, trimmed.Height);

        // Verify L shape is maintained
        Assert.IsTrue(trimmed.GetCell(0, 0));
        Assert.IsTrue(trimmed.GetCell(0, 1));
        Assert.IsTrue(trimmed.GetCell(0, 2));
        Assert.IsTrue(trimmed.GetCell(1, 2));
        Assert.IsTrue(trimmed.GetCell(2, 2));

        // Verify empty cells
        Assert.IsFalse(trimmed.GetCell(1, 0));
        Assert.IsFalse(trimmed.GetCell(2, 0));
        Assert.IsFalse(trimmed.GetCell(1, 1));
        Assert.IsFalse(trimmed.GetCell(2, 1));

        shape.Dispose();
        trimmed.Dispose();
    }
}
