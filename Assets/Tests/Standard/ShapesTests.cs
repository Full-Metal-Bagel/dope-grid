using DopeGrid;
using DopeGrid.Standard;
using NUnit.Framework;

[TestFixture]
public class StandardShapesTests
{
    [Test]
    public void Single_CreatesCorrectShape()
    {
        using var shape = Shapes.Single();

        Assert.AreEqual(1, shape.Width);
        Assert.AreEqual(1, shape.Height);
        Assert.IsTrue(shape.GetCell((0, 0)));
        Assert.AreEqual(1, shape.OccupiedSpaceCount);
    }

    [Test]
    public void Line_CreatesCorrectShape()
    {
        using var shape = Shapes.Line(4);

        Assert.AreEqual(4, shape.Width);
        Assert.AreEqual(1, shape.Height);
        Assert.AreEqual(4, shape.OccupiedSpaceCount);

        for (int x = 0; x < 4; x++)
            Assert.IsTrue(shape.GetCell((x, 0)));
    }

    [Test]
    public void Square_CreatesCorrectShape()
    {
        using var shape = Shapes.Square(3);

        Assert.AreEqual(3, shape.Width);
        Assert.AreEqual(3, shape.Height);
        Assert.AreEqual(9, shape.OccupiedSpaceCount);

        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 3; x++)
            Assert.IsTrue(shape.GetCell((x, y)));
    }

    [Test]
    public void LShape_CreatesCorrectShape()
    {
        using var shape = Shapes.LShape();

        Assert.AreEqual(2, shape.Width);
        Assert.AreEqual(2, shape.Height);
        Assert.AreEqual(3, shape.OccupiedSpaceCount);

        Assert.IsTrue(shape.GetCell((0, 0)));
        Assert.IsTrue(shape.GetCell((0, 1)));
        Assert.IsTrue(shape.GetCell((1, 1)));
        Assert.IsFalse(shape.GetCell((1, 0)));
    }

    [Test]
    public void TShape_CreatesCorrectShape()
    {
        using var shape = Shapes.TShape();

        Assert.AreEqual(3, shape.Width);
        Assert.AreEqual(2, shape.Height);
        Assert.AreEqual(4, shape.OccupiedSpaceCount);

        Assert.IsTrue(shape.GetCell((0, 0)));
        Assert.IsTrue(shape.GetCell((1, 0)));
        Assert.IsTrue(shape.GetCell((2, 0)));
        Assert.IsTrue(shape.GetCell((1, 1)));
    }

    [Test]
    public void Cross_CreatesCorrectShape()
    {
        using var shape = Shapes.Cross();

        Assert.AreEqual(3, shape.Width);
        Assert.AreEqual(3, shape.Height);
        Assert.AreEqual(5, shape.OccupiedSpaceCount);

        Assert.IsTrue(shape.GetCell((1, 0)));
        Assert.IsTrue(shape.GetCell((0, 1)));
        Assert.IsTrue(shape.GetCell((1, 1)));
        Assert.IsTrue(shape.GetCell((2, 1)));
        Assert.IsTrue(shape.GetCell((1, 2)));
    }

    [Test]
    public void Rotate_Line_90Degrees()
    {
        using var shape = Shapes.Line(3);
        using var rotated = shape.Rotate(RotationDegree.Clockwise90);

        Assert.AreEqual(1, rotated.Width);
        Assert.AreEqual(3, rotated.Height);
        Assert.IsTrue(rotated.GetCell((0, 0)));
        Assert.IsTrue(rotated.GetCell((0, 1)));
        Assert.IsTrue(rotated.GetCell((0, 2)));
        Assert.AreEqual(shape.OccupiedSpaceCount, rotated.OccupiedSpaceCount);
    }

    [Test]
    public void Rotate_LShape_90Degrees()
    {
        using var shape = Shapes.LShape();
        using var rotated = shape.Rotate(RotationDegree.Clockwise90);

        Assert.AreEqual(2, rotated.Width);
        Assert.AreEqual(2, rotated.Height);
        Assert.AreEqual(3, rotated.OccupiedSpaceCount);

        // L rotated 90 clockwise becomes different orientation
        Assert.IsTrue(rotated.GetCell((0, 0)));
        Assert.IsTrue(rotated.GetCell((1, 0)));
        Assert.IsTrue(rotated.GetCell((0, 1)));
    }

    [Test]
    public void Rotate_180Degrees_FlipsCompletely()
    {
        using var shape = Shapes.Line(3);
        using var rotated = shape.Rotate(RotationDegree.Clockwise180);

        Assert.AreEqual(3, rotated.Width);
        Assert.AreEqual(1, rotated.Height);
        Assert.AreEqual(shape.OccupiedSpaceCount, rotated.OccupiedSpaceCount);
    }

    [Test]
    public void Rotate_270Degrees()
    {
        using var shape = Shapes.Line(3);
        using var rotated = shape.Rotate(RotationDegree.Clockwise270);

        Assert.AreEqual(1, rotated.Width);
        Assert.AreEqual(3, rotated.Height);
        Assert.AreEqual(shape.OccupiedSpaceCount, rotated.OccupiedSpaceCount);
    }

    [Test]
    public void Flip_Horizontal()
    {
        using var shape = Shapes.LShape();
        using var flipped = shape.Flip(FlipAxis.Horizontal);

        Assert.AreEqual(shape.Width, flipped.Width);
        Assert.AreEqual(shape.Height, flipped.Height);
        Assert.AreEqual(shape.OccupiedSpaceCount, flipped.OccupiedSpaceCount);

        // Original L: (0,0), (0,1), (1,1)
        // Flipped horizontally: (1,0), (1,1), (0,1)
        Assert.IsTrue(flipped.GetCell((1, 0)));
        Assert.IsTrue(flipped.GetCell((1, 1)));
        Assert.IsTrue(flipped.GetCell((0, 1)));
    }

    [Test]
    public void Flip_Vertical()
    {
        using var shape = Shapes.LShape();
        using var flipped = shape.Flip(FlipAxis.Vertical);

        Assert.AreEqual(shape.Width, flipped.Width);
        Assert.AreEqual(shape.Height, flipped.Height);
        Assert.AreEqual(shape.OccupiedSpaceCount, flipped.OccupiedSpaceCount);
    }

    [Test]
    public void Trim_RemovesEmptyBorders()
    {
        using var shape = new GridShape(5, 5);
        shape.SetCell((2, 2), true);
        shape.SetCell((2, 3), true);
        shape.SetCell((3, 2), true);

        using var trimmed = shape.AsReadOnly().Trim();

        Assert.AreEqual(2, trimmed.Width);
        Assert.AreEqual(2, trimmed.Height);
        Assert.AreEqual(3, trimmed.OccupiedSpaceCount);
    }

    [Test]
    public void Trim_EmptyShape_ReturnsEmpty()
    {
        using var shape = new GridShape(5, 5);
        using var trimmed = shape.AsReadOnly().Trim();

        Assert.AreEqual(0, trimmed.Width);
        Assert.AreEqual(0, trimmed.Height);
    }

    [Test]
    public void Trim_AlreadyTrimmed_ReturnsClone()
    {
        using var shape = Shapes.Square(3);
        using var trimmed = shape.AsReadOnly().Trim();

        Assert.AreEqual(shape.Width, trimmed.Width);
        Assert.AreEqual(shape.Height, trimmed.Height);
    }

    [Test]
    public void IsTrimmed_DetectsCorrectly()
    {
        using var untrimmed = new GridShape(5, 5);
        untrimmed.SetCell((2, 2), true);
        Assert.IsFalse(untrimmed.IsTrimmed());

        using var trimmed = Shapes.Square(3);
        Assert.IsTrue(trimmed.IsTrimmed());
    }

    [Test]
    public void GetRotatedDimensions_ReturnsCorrectDimensions()
    {
        using var shape = new GridShape(3, 5);
        var ro = shape.AsReadOnly();

        var dim90 = ro.GetRotatedDimensions(RotationDegree.Clockwise90);
        Assert.AreEqual((5, 3), dim90);

        var dim180 = ro.GetRotatedDimensions(RotationDegree.Clockwise180);
        Assert.AreEqual((3, 5), dim180);

        var dim270 = ro.GetRotatedDimensions(RotationDegree.Clockwise270);
        Assert.AreEqual((5, 3), dim270);
    }
}