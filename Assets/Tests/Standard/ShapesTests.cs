using DopeGrid;
using DopeGrid.Standard;
using NUnit.Framework;

[TestFixture]
public class StandardShapesTests
{
    [Test]
    public void Single_CreatesCorrectShape()
    {
        var shape = Shapes.Single();

        Assert.AreEqual(1, shape.Width);
        Assert.AreEqual(1, shape.Height);
        Assert.IsTrue(shape.GetCellValue(0, 0));
        Assert.AreEqual(1, shape.OccupiedSpaceCount);
        shape.Dispose();
    }

    [Test]
    public void Line_CreatesCorrectShape()
    {
        var shape = Shapes.Line(4);

        Assert.AreEqual(4, shape.Width);
        Assert.AreEqual(1, shape.Height);
        Assert.AreEqual(4, shape.OccupiedSpaceCount);

        for (int x = 0; x < 4; x++)
            Assert.IsTrue(shape.GetCellValue(x, 0));
        shape.Dispose();
    }

    [Test]
    public void Square_CreatesCorrectShape()
    {
        var shape = Shapes.Square(3);

        Assert.AreEqual(3, shape.Width);
        Assert.AreEqual(3, shape.Height);
        Assert.AreEqual(9, shape.OccupiedSpaceCount);

        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 3; x++)
            Assert.IsTrue(shape.GetCellValue(x, y));
        shape.Dispose();
    }

    [Test]
    public void LShape_CreatesCorrectShape()
    {
        var shape = Shapes.LShape();

        Assert.AreEqual(2, shape.Width);
        Assert.AreEqual(2, shape.Height);
        Assert.AreEqual(3, shape.OccupiedSpaceCount);

        Assert.IsTrue(shape.GetCellValue(0, 0));
        Assert.IsTrue(shape.GetCellValue(0, 1));
        Assert.IsTrue(shape.GetCellValue(1, 1));
        Assert.IsFalse(shape.GetCellValue(1, 0));
        shape.Dispose();
    }

    [Test]
    public void TShape_CreatesCorrectShape()
    {
        var shape = Shapes.TShape();

        Assert.AreEqual(3, shape.Width);
        Assert.AreEqual(2, shape.Height);
        Assert.AreEqual(4, shape.OccupiedSpaceCount);

        Assert.IsTrue(shape.GetCellValue(0, 0));
        Assert.IsTrue(shape.GetCellValue(1, 0));
        Assert.IsTrue(shape.GetCellValue(2, 0));
        Assert.IsTrue(shape.GetCellValue(1, 1));
        shape.Dispose();
    }

    [Test]
    public void Cross_CreatesCorrectShape()
    {
        var shape = Shapes.Cross();

        Assert.AreEqual(3, shape.Width);
        Assert.AreEqual(3, shape.Height);
        Assert.AreEqual(5, shape.OccupiedSpaceCount);

        Assert.IsTrue(shape.GetCellValue(1, 0));
        Assert.IsTrue(shape.GetCellValue(0, 1));
        Assert.IsTrue(shape.GetCellValue(1, 1));
        Assert.IsTrue(shape.GetCellValue(2, 1));
        Assert.IsTrue(shape.GetCellValue(1, 2));
        shape.Dispose();
    }

    [Test]
    public void Rotate_Line_90Degrees()
    {
        var shape = Shapes.Line(3);
        var rotated = shape.Rotate(RotationDegree.Clockwise90);

        Assert.AreEqual(1, rotated.Width);
        Assert.AreEqual(3, rotated.Height);
        Assert.IsTrue(rotated.GetCellValue(0, 0));
        Assert.IsTrue(rotated.GetCellValue(0, 1));
        Assert.IsTrue(rotated.GetCellValue(0, 2));
        Assert.AreEqual(shape.OccupiedSpaceCount, rotated.OccupiedSpaceCount);
        rotated.Dispose();
        shape.Dispose();
    }

    [Test]
    public void Rotate_LShape_90Degrees()
    {
        var shape = Shapes.LShape();
        var rotated = shape.Rotate(RotationDegree.Clockwise90);

        Assert.AreEqual(2, rotated.Width);
        Assert.AreEqual(2, rotated.Height);
        Assert.AreEqual(3, rotated.OccupiedSpaceCount);

        // L rotated 90 clockwise becomes different orientation
        Assert.IsTrue(rotated.GetCellValue(0, 0));
        Assert.IsTrue(rotated.GetCellValue(1, 0));
        Assert.IsTrue(rotated.GetCellValue(0, 1));
        rotated.Dispose();
        shape.Dispose();
    }

    [Test]
    public void Rotate_180Degrees_FlipsCompletely()
    {
        var shape = Shapes.Line(3);
        var rotated = shape.Rotate(RotationDegree.Clockwise180);

        Assert.AreEqual(3, rotated.Width);
        Assert.AreEqual(1, rotated.Height);
        Assert.AreEqual(shape.OccupiedSpaceCount, rotated.OccupiedSpaceCount);
        rotated.Dispose();
        shape.Dispose();
    }

    [Test]
    public void Rotate_270Degrees()
    {
        var shape = Shapes.Line(3);
        var rotated = shape.Rotate(RotationDegree.Clockwise270);

        Assert.AreEqual(1, rotated.Width);
        Assert.AreEqual(3, rotated.Height);
        Assert.AreEqual(shape.OccupiedSpaceCount, rotated.OccupiedSpaceCount);
        rotated.Dispose();
        shape.Dispose();
    }

    [Test]
    public void Flip_Horizontal()
    {
        var shape = Shapes.LShape();
        var flipped = shape.Flip(FlipAxis.Horizontal);

        Assert.AreEqual(shape.Width, flipped.Width);
        Assert.AreEqual(shape.Height, flipped.Height);
        Assert.AreEqual(shape.OccupiedSpaceCount, flipped.OccupiedSpaceCount);

        // Original L: (0,0), (0,1), (1,1)
        // Flipped horizontally: (1,0), (1,1), (0,1)
        Assert.IsTrue(flipped.GetCellValue(1, 0));
        Assert.IsTrue(flipped.GetCellValue(1, 1));
        Assert.IsTrue(flipped.GetCellValue(0, 1));
        flipped.Dispose();
        shape.Dispose();
    }

    [Test]
    public void Flip_Vertical()
    {
        var shape = Shapes.LShape();
        var flipped = shape.Flip(FlipAxis.Vertical);

        Assert.AreEqual(shape.Width, flipped.Width);
        Assert.AreEqual(shape.Height, flipped.Height);
        Assert.AreEqual(shape.OccupiedSpaceCount, flipped.OccupiedSpaceCount);
        flipped.Dispose();
        shape.Dispose();
    }

    [Test]
    public void Trim_RemovesEmptyBorders()
    {
        var shape = new GridShape(5, 5);
        shape[2, 2] = true;
        shape[2, 3] = true;
        shape[3, 2] = true;

        var trimmed = shape.AsReadOnly().Trim();

        Assert.AreEqual(2, trimmed.Width);
        Assert.AreEqual(2, trimmed.Height);
        Assert.AreEqual(3, trimmed.OccupiedSpaceCount);
        trimmed.Dispose();
        shape.Dispose();
    }

    [Test]
    public void Trim_EmptyShape_ReturnsEmpty()
    {
        var shape = new GridShape(5, 5);
        var trimmed = shape.AsReadOnly().Trim();

        Assert.AreEqual(0, trimmed.Width);
        Assert.AreEqual(0, trimmed.Height);
        trimmed.Dispose();
        shape.Dispose();
    }

    [Test]
    public void Trim_AlreadyTrimmed_ReturnsClone()
    {
        var shape = Shapes.Square(3);
        var trimmed = shape.AsReadOnly().Trim();

        Assert.AreEqual(shape.Width, trimmed.Width);
        Assert.AreEqual(shape.Height, trimmed.Height);
        trimmed.Dispose();
        shape.Dispose();
    }

    [Test]
    public void IsTrimmed_DetectsCorrectly()
    {
        var untrimmed = new GridShape(5, 5);
        untrimmed[2, 2] = true;
        Assert.IsFalse(untrimmed.IsTrimmed(freeValue: false));
        var trimmed = Shapes.Square(3);
        Assert.IsTrue(trimmed.IsTrimmed(freeValue: false));
        trimmed.Dispose();
        untrimmed.Dispose();
    }

    [Test]
    public void GetRotatedDimensions_ReturnsCorrectDimensions()
    {
        var shape = new GridShape(3, 5);
        var ro = shape.AsReadOnly();

        var dim90 = ro.GetRotatedDimensions(RotationDegree.Clockwise90);
        Assert.AreEqual((5, 3), dim90);

        var dim180 = ro.GetRotatedDimensions(RotationDegree.Clockwise180);
        Assert.AreEqual((3, 5), dim180);

        var dim270 = ro.GetRotatedDimensions(RotationDegree.Clockwise270);
        Assert.AreEqual((5, 3), dim270);
        shape.Dispose();
    }
}
