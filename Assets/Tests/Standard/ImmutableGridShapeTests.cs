using DopeGrid;
using DopeGrid.Standard;
using NUnit.Framework;

public class StandardImmutableGridShapeTests
{
    [Test]
    public void GetOrCreateImmutable_CreatesSameIdForIdenticalShapes()
    {
        using var shape1 = Shapes.Square(3);
        using var shape2 = Shapes.Square(3);

        var immutable1 = shape1.GetOrCreateImmutable();
        var immutable2 = shape2.GetOrCreateImmutable();

        Assert.AreEqual(immutable1.Id, immutable2.Id);
    }

    [Test]
    public void GetOrCreateImmutable_CreatesDifferentIdForDifferentShapes()
    {
        using var shape1 = Shapes.Square(3);
        using var shape2 = Shapes.Line(3);

        var immutable1 = shape1.GetOrCreateImmutable();
        var immutable2 = shape2.GetOrCreateImmutable();

        Assert.AreNotEqual(immutable1.Id, immutable2.Id);
    }

    [Test]
    public void GetOrCreateImmutable_RequiresTrimmedShape()
    {
        var untrimmed = new GridShape(5, 5);
        try
        {
            untrimmed[2, 2] = true;

            Assert.Throws<System.ArgumentException>(() => untrimmed.GetOrCreateImmutable());
        }
        finally
        {
            untrimmed.Dispose();
        }
    }

    [Test]
    public void ImmutableShape_PropertiesAccessible()
    {
        var immutable = Shapes.ImmutableSquare(3);

        Assert.AreEqual(3, immutable.Width);
        Assert.AreEqual(3, immutable.Height);
        Assert.AreEqual(9, immutable.Size);
        Assert.AreEqual(9, immutable.OccupiedSpaceCount);
        Assert.AreEqual(0, immutable.FreeSpaceCount);
    }

    [Test]
    public void ImmutableShape_GetCell_WorksCorrectly()
    {
        var immutable = Shapes.ImmutableLShape();

        Assert.IsTrue(immutable.GetCellValue((0, 0)));
        Assert.IsTrue(immutable.GetCellValue((0, 1)));
        Assert.IsTrue(immutable.GetCellValue((1, 1)));
        Assert.IsFalse(immutable.GetCellValue((1, 0)));
    }

    [Test]
    public void ImmutableShape_Rotate90_CreatesRotatedShape()
    {
        var original = Shapes.ImmutableLine(3);
        var rotated = original.Rotate90();

        Assert.AreEqual(1, rotated.Width);
        Assert.AreEqual(3, rotated.Height);
        Assert.AreEqual(original.OccupiedSpaceCount, rotated.OccupiedSpaceCount);
    }

    [Test]
    public void ImmutableShape_Rotate90_IsCached()
    {
        var original = Shapes.ImmutableSquare(2);
        var rotated1 = original.Rotate90();
        var rotated2 = original.Rotate90();

        Assert.AreEqual(rotated1.Id, rotated2.Id);
    }

    [Test]
    public void ImmutableShape_Flip_CreatesFlippedShape()
    {
        var original = Shapes.ImmutableLShape();
        var flipped = original.Flip();

        Assert.AreEqual(original.Width, flipped.Width);
        Assert.AreEqual(original.Height, flipped.Height);
        Assert.AreEqual(original.OccupiedSpaceCount, flipped.OccupiedSpaceCount);
    }

    [Test]
    public void ImmutableShape_Flip_IsCached()
    {
        var original = Shapes.ImmutableLShape();
        var flipped1 = original.Flip();
        var flipped2 = original.Flip();

        Assert.AreEqual(flipped1.Id, flipped2.Id);
    }

    [Test]
    public void ImmutableShape_FlipTwice_ReturnsOriginal()
    {
        var original = Shapes.ImmutableLShape();
        var flipped = original.Flip();
        var flippedBack = flipped.Flip();

        Assert.AreEqual(original.Id, flippedBack.Id);
    }

    [Test]
    public void ImmutableShape_ToReadOnlyGridShape_WorksCorrectly()
    {
        var immutable = Shapes.ImmutableSquare(2);
        var readOnly = immutable.ToReadOnlyGridShape();

        Assert.AreEqual(2, readOnly.Width);
        Assert.AreEqual(2, readOnly.Height);

        for (int y = 0; y < 2; y++)
        for (int x = 0; x < 2; x++)
            Assert.IsTrue(readOnly.GetCellValue((x, y)));
    }

    [Test]
    public void ImmutableShape_CopyTo_CopiesCorrectly()
    {
        var immutable = Shapes.ImmutableLShape();
        using var target = new GridShape(2, 2);

        immutable.CopyTo(target);

        Assert.IsTrue(target[0, 0]);
        Assert.IsTrue(target[0, 1]);
        Assert.IsTrue(target[1, 1]);
        Assert.IsFalse(target[1, 0]);
    }

    [Test]
    public void ImmutableShape_Empty_IsEmpty()
    {
        var empty = ImmutableGridShape.Empty;

        Assert.AreEqual(0, empty.Width);
        Assert.AreEqual(0, empty.Height);
        Assert.IsTrue(empty.IsEmpty);
    }

    [Test]
    public void ImmutableShape_GetIndex_CalculatesCorrectly()
    {
        var shape = Shapes.ImmutableSquare(3);

        Assert.AreEqual(0, shape.GetIndex((0, 0)));
        Assert.AreEqual(1, shape.GetIndex((1, 0)));
        Assert.AreEqual(3, shape.GetIndex((0, 1)));
        Assert.AreEqual(4, shape.GetIndex((1, 1)));
    }

    [Test]
    public void GetRotatedShape_ReturnsCorrectRotation()
    {
        var original = Shapes.ImmutableLine(3);

        var rotated90 = original.GetRotatedShape(RotationDegree.Clockwise90);
        Assert.AreEqual(1, rotated90.Width);
        Assert.AreEqual(3, rotated90.Height);

        var rotated180 = original.GetRotatedShape(RotationDegree.Clockwise180);
        Assert.AreEqual(3, rotated180.Width);
        Assert.AreEqual(1, rotated180.Height);

        var rotated270 = original.GetRotatedShape(RotationDegree.Clockwise270);
        Assert.AreEqual(1, rotated270.Width);
        Assert.AreEqual(3, rotated270.Height);

        var rotatedNone = original.GetRotatedShape(RotationDegree.None);
        Assert.AreEqual(original.Id, rotatedNone.Id);
    }

    [Test]
    public void ImmutableShape_MultipleRotations_MaintainOccupiedCount()
    {
        var original = Shapes.ImmutableLShape();
        var originalCount = original.OccupiedSpaceCount;

        var rotated90 = original.Rotate90();
        var rotated180 = rotated90.Rotate90();
        var rotated270 = rotated180.Rotate90();
        var rotated360 = rotated270.Rotate90();

        Assert.AreEqual(originalCount, rotated90.OccupiedSpaceCount);
        Assert.AreEqual(originalCount, rotated180.OccupiedSpaceCount);
        Assert.AreEqual(originalCount, rotated270.OccupiedSpaceCount);
        Assert.AreEqual(originalCount, rotated360.OccupiedSpaceCount);
    }
}
