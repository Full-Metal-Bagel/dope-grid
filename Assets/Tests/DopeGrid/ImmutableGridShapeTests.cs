using DopeGrid;
using NUnit.Framework;

namespace DopeGrid.Tests;

[TestFixture]
public class ImmutableGridShapeTests
{
    [Test]
    public void Empty_HasZeroDimensions()
    {
        var shape = ImmutableGridShape.Empty;

        Assert.That(shape.Width, Is.EqualTo(0));
        Assert.That(shape.Height, Is.EqualTo(0));
        Assert.That(shape.Size, Is.EqualTo(0));
    }

    [Test]
    public void GetOrCreateImmutable_CreatesSameIdForIdenticalShapes()
    {
        var shape1 = Shapes.Single();
        var shape2 = Shapes.Single();

        var immutable1 = shape1.GetOrCreateImmutable();
        var immutable2 = shape2.GetOrCreateImmutable();

        Assert.That(immutable1.Id, Is.EqualTo(immutable2.Id));
    }

    [Test]
    public void GetOrCreateImmutable_CreatesDifferentIdForDifferentShapes()
    {
        var shape1 = Shapes.Single();
        var shape2 = Shapes.Line(2);

        var immutable1 = shape1.GetOrCreateImmutable();
        var immutable2 = shape2.GetOrCreateImmutable();

        Assert.That(immutable1.Id, Is.Not.EqualTo(immutable2.Id));
    }

    [Test]
    public void GetOrCreateImmutable_RequiresTrimmedShape()
    {
        using var shape = new GridShape(3, 3);
        shape[1, 1] = true;

        Assert.Throws<System.ArgumentException>(() => shape.GetOrCreateImmutable());
    }

    [Test]
    public void Indexer_ReturnsCorrectValues()
    {
        var immutable = Shapes.ImmutableLShape();

        Assert.That(immutable[0, 0], Is.True);
        Assert.That(immutable[0, 1], Is.True);
        Assert.That(immutable[1, 1], Is.True);
        Assert.That(immutable[1, 0], Is.False);
    }

    [Test]
    public void IsOccupied_ReturnsSameAsValue()
    {
        var immutable = Shapes.ImmutableSingle();

        Assert.That(immutable.IsOccupied(0, 0), Is.True);
    }

    [Test]
    public void Bound_ReturnsCorrectDimensions()
    {
        var immutable = Shapes.ImmutableLine(5);

        var (width, height) = immutable.Bound;

        Assert.That(width, Is.EqualTo(5));
        Assert.That(height, Is.EqualTo(1));
    }

    [Test]
    public void Width_ReturnsCorrectValue()
    {
        var immutable = Shapes.ImmutableLine(3);

        Assert.That(immutable.Width, Is.EqualTo(3));
    }

    [Test]
    public void Height_ReturnsCorrectValue()
    {
        var immutable = Shapes.ImmutableLine(3).Rotate90();

        Assert.That(immutable.Height, Is.EqualTo(3));
    }

    [Test]
    public void OccupiedSpaceCount_ReturnsCorrectCount()
    {
        var immutable = Shapes.ImmutableLShape();

        Assert.That(immutable.OccupiedSpaceCount(), Is.EqualTo(3));
    }

    [Test]
    public void FreeSpaceCount_ReturnsCorrectCount()
    {
        var immutable = Shapes.ImmutableLShape();

        Assert.That(immutable.FreeSpaceCount(), Is.EqualTo(1));
    }

    [Test]
    public void Rotate90_CreatesRotatedShape()
    {
        var immutable = Shapes.ImmutableLine(3);

        var rotated = immutable.Rotate90();

        Assert.That(rotated.Width, Is.EqualTo(1));
        Assert.That(rotated.Height, Is.EqualTo(3));
    }

    [Test]
    public void Rotate90_FourTimes_ReturnsSameShape()
    {
        var immutable = Shapes.ImmutableLShape();

        var rotated = immutable.Rotate90().Rotate90().Rotate90().Rotate90();

        Assert.That(rotated.Id, Is.EqualTo(immutable.Id));
    }

    [Test]
    public void Rotate90_CachesResult()
    {
        var immutable = Shapes.ImmutableLShape();

        var rotated1 = immutable.Rotate90();
        var rotated2 = immutable.Rotate90();

        Assert.That(rotated1.Id, Is.EqualTo(rotated2.Id));
    }

    [Test]
    public void Flip_CreatesFlippedShape()
    {
        var immutable = Shapes.ImmutableLShape();

        var flipped = immutable.Flip();

        Assert.That(flipped.Width, Is.EqualTo(immutable.Width));
        Assert.That(flipped.Height, Is.EqualTo(immutable.Height));
        Assert.That(flipped.Id, Is.Not.EqualTo(immutable.Id));
    }

    [Test]
    public void Flip_TwiceReturnsSameShape()
    {
        var immutable = Shapes.ImmutableLShape();

        var flipped = immutable.Flip().Flip();

        Assert.That(flipped.Id, Is.EqualTo(immutable.Id));
    }

    [Test]
    public void Flip_CachesResult()
    {
        var immutable = Shapes.ImmutableLShape();

        var flipped1 = immutable.Flip();
        var flipped2 = immutable.Flip();

        Assert.That(flipped1.Id, Is.EqualTo(flipped2.Id));
    }

    [Test]
    public void Pattern_ReturnsCorrectBitArray()
    {
        var immutable = Shapes.ImmutableSingle();

        var pattern = immutable.Pattern;

        Assert.That(pattern.BitLength, Is.EqualTo(1));
        Assert.That(pattern.Get(0), Is.True);
    }

    [Test]
    public void ImmutableShapes_FromFactory_Work()
    {
        var single = Shapes.ImmutableSingle();
        Assert.That(single.Width, Is.EqualTo(1));
        Assert.That(single.Height, Is.EqualTo(1));

        var line = Shapes.ImmutableLine(3);
        Assert.That(line.Width, Is.EqualTo(3));
        Assert.That(line.Height, Is.EqualTo(1));

        var square = Shapes.ImmutableSquare(2);
        Assert.That(square.Width, Is.EqualTo(2));
        Assert.That(square.Height, Is.EqualTo(2));

        var lShape = Shapes.ImmutableLShape();
        Assert.That(lShape.OccupiedSpaceCount(), Is.EqualTo(3));

        var tShape = Shapes.ImmutableTShape();
        Assert.That(tShape.Width, Is.EqualTo(3));
        Assert.That(tShape.Height, Is.EqualTo(2));

        var cross = Shapes.ImmutableCross();
        Assert.That(cross.OccupiedSpaceCount(), Is.EqualTo(5));
    }

    [Test]
    public void GetRotatedShape_ReturnsCorrectRotation()
    {
        var immutable = Shapes.ImmutableLine(3);

        var rotated0 = immutable.GetRotatedShape(RotationDegree.None);
        Assert.That(rotated0.Id, Is.EqualTo(immutable.Id));

        var rotated90 = immutable.GetRotatedShape(RotationDegree.Clockwise90);
        Assert.That(rotated90.Width, Is.EqualTo(1));
        Assert.That(rotated90.Height, Is.EqualTo(3));

        var rotated180 = immutable.GetRotatedShape(RotationDegree.Clockwise180);
        Assert.That(rotated180.Width, Is.EqualTo(3));
        Assert.That(rotated180.Height, Is.EqualTo(1));

        var rotated270 = immutable.GetRotatedShape(RotationDegree.Clockwise270);
        Assert.That(rotated270.Width, Is.EqualTo(1));
        Assert.That(rotated270.Height, Is.EqualTo(3));
    }

    [Test]
    public void RecordEquality_WorksCorrectly()
    {
        var immutable1 = Shapes.ImmutableSingle();
        var immutable2 = Shapes.ImmutableSingle();

        Assert.That(immutable1, Is.EqualTo(immutable2));
        Assert.That(immutable1.GetHashCode(), Is.EqualTo(immutable2.GetHashCode()));
    }

    [Test]
    public void ComplexShape_RotationAndFlip_Work()
    {
        var immutable = Shapes.ImmutableLShape();

        var rotated = immutable.Rotate90();
        var flipped = immutable.Flip();
        var rotatedFlipped = immutable.Rotate90().Flip();

        Assert.That(rotated.Id, Is.Not.EqualTo(immutable.Id), "Rotated should differ");
        Assert.That(flipped.Id, Is.Not.EqualTo(immutable.Id), "Flipped should differ");
        Assert.That(rotatedFlipped.Id, Is.Not.EqualTo(immutable.Id), "Rotated+Flipped should differ");
    }
}
