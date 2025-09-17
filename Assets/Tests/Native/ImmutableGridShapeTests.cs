using DopeGrid.Native;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

public class ImmutableGridShapeTests
{
    [Test]
    public void Empty_ReturnsEmptyShape()
    {
        var empty = ImmutableGridShape.Empty;

        Assert.AreEqual(0, empty.Id);
        Assert.AreEqual(int2.zero, empty.Bound);
        Assert.AreEqual(0, empty.Width);
        Assert.AreEqual(0, empty.Height);
    }

    [Test]
    public void GetOrCreateImmutable_SingleCell()
    {
        var shape = new GridShape(1, 1, Allocator.Temp);
        shape.SetCell(new int2(0, 0), true);

        var immutable = shape.ToReadOnly().GetOrCreateImmutable();

        Assert.AreNotEqual(0, immutable.Id);
        Assert.AreEqual(1, immutable.Width);
        Assert.AreEqual(1, immutable.Height);

        shape.Dispose();
    }

    [Test]
    public void GetOrCreateImmutable_Line()
    {
        var shape = Shapes.Line(3, Allocator.Temp);

        var immutable = shape.ToReadOnly().GetOrCreateImmutable();

        Assert.AreNotEqual(0, immutable.Id);
        Assert.AreEqual(3, immutable.Width);
        Assert.AreEqual(1, immutable.Height);

        shape.Dispose();
    }

    [Test]
    public void GetOrCreateImmutable_LShape()
    {
        var shape = Shapes.LShape(Allocator.Temp);

        var immutable = shape.ToReadOnly().GetOrCreateImmutable();

        Assert.AreNotEqual(0, immutable.Id);
        Assert.AreEqual(2, immutable.Width);
        Assert.AreEqual(2, immutable.Height);

        shape.Dispose();
    }

    [Test]
    public void GetOrCreateImmutable_SameShapeReturnsSameId()
    {
        var shape1 = Shapes.TShape(Allocator.Temp);
        var shape2 = Shapes.TShape(Allocator.Temp);

        var immutable1 = shape1.ToReadOnly().GetOrCreateImmutable();
        var immutable2 = shape2.ToReadOnly().GetOrCreateImmutable();

        Assert.AreEqual(immutable1.Id, immutable2.Id);

        shape1.Dispose();
        shape2.Dispose();
    }

    [Test]
    public void Rotate90_LineShape()
    {
        // Create horizontal line
        var shape = Shapes.Line(3, Allocator.Temp);
        var immutable = shape.ToReadOnly().GetOrCreateImmutable();

        // Rotate 90 degrees
        var rotated = immutable.Rotate90();

        Assert.AreNotEqual(immutable.Id, rotated.Id);
        Assert.AreEqual(1, rotated.Width);
        Assert.AreEqual(3, rotated.Height);

        // Rotate again should give 180 rotation
        var rotated180 = rotated.Rotate90();
        Assert.AreEqual(3, rotated180.Width);
        Assert.AreEqual(1, rotated180.Height);

        // Rotate again should give 270 rotation
        var rotated270 = rotated180.Rotate90();
        Assert.AreEqual(1, rotated270.Width);
        Assert.AreEqual(3, rotated270.Height);

        // Rotate again should return to original
        var rotated360 = rotated270.Rotate90();
        Assert.AreEqual(immutable.Id, rotated360.Id);

        shape.Dispose();
    }

    [Test]
    public void Rotate90_SquareShape()
    {
        // Create 2x2 square
        var shape = Shapes.Square(2, Allocator.Temp);
        var immutable = shape.ToReadOnly().GetOrCreateImmutable();

        // Rotating a square should return the same shape
        var rotated = immutable.Rotate90();

        Assert.AreEqual(immutable.Id, rotated.Id);
        Assert.AreEqual(2, rotated.Width);
        Assert.AreEqual(2, rotated.Height);

        shape.Dispose();
    }

    [Test]
    public void Rotate90_LShape()
    {
        var shape = Shapes.LShape(Allocator.Temp);
        var immutable = shape.ToReadOnly().GetOrCreateImmutable();

        // L-shape should have 4 distinct rotations
        var rotated90 = immutable.Rotate90();
        var rotated180 = rotated90.Rotate90();
        var rotated270 = rotated180.Rotate90();
        var rotated360 = rotated270.Rotate90();

        // All rotations should be different except 360 which returns to original
        Assert.AreNotEqual(immutable.Id, rotated90.Id);
        Assert.AreNotEqual(immutable.Id, rotated180.Id);
        Assert.AreNotEqual(immutable.Id, rotated270.Id);
        Assert.AreNotEqual(rotated90.Id, rotated180.Id);
        Assert.AreNotEqual(rotated90.Id, rotated270.Id);
        Assert.AreNotEqual(rotated180.Id, rotated270.Id);
        Assert.AreEqual(immutable.Id, rotated360.Id);

        shape.Dispose();
    }

    [Test]
    public void Flip_LineShape()
    {
        // Horizontal line
        var shape = Shapes.Line(3, Allocator.Temp);
        var immutable = shape.ToReadOnly().GetOrCreateImmutable();

        // Flipping horizontal line horizontally should return same shape
        var flipped = immutable.Flip();

        Assert.AreEqual(immutable.Id, flipped.Id);

        shape.Dispose();
    }

    [Test]
    public void Flip_LShape()
    {
        var shape = Shapes.LShape(Allocator.Temp);
        var immutable = shape.ToReadOnly().GetOrCreateImmutable();

        // Flipping L-shape should create a different shape
        var flipped = immutable.Flip();

        Assert.AreNotEqual(immutable.Id, flipped.Id);
        Assert.AreEqual(2, flipped.Width);
        Assert.AreEqual(2, flipped.Height);

        // Flipping again should return to original
        var doubleFlipped = flipped.Flip();
        Assert.AreEqual(immutable.Id, doubleFlipped.Id);

        shape.Dispose();
    }

    [Test]
    public void Flip_TShape()
    {
        var shape = Shapes.TShape(Allocator.Temp);
        var immutable = shape.ToReadOnly().GetOrCreateImmutable();

        var flipped = immutable.Flip();

        // T-shape flipped horizontally should be same (symmetric)
        Assert.AreEqual(immutable.Id, flipped.Id);

        shape.Dispose();
    }

    [Test]
    public void ToReadOnlyGridShape_ConversionWorks()
    {
        var shape = Shapes.Cross(Allocator.Temp);
        var immutable = shape.ToReadOnly().GetOrCreateImmutable();

        // Implicit conversion
        GridShape.ReadOnly readOnly = immutable;

        Assert.AreEqual(immutable.Width, readOnly.Width);
        Assert.AreEqual(immutable.Height, readOnly.Height);

        // Explicit method call
        var explicit_readonly = immutable.ToReadOnlyGridShape();
        Assert.AreEqual(immutable.Width, explicit_readonly.Width);
        Assert.AreEqual(immutable.Height, explicit_readonly.Height);

        shape.Dispose();
    }

    [Test]
    public void ComplexTransformations_LShape()
    {
        var shape = Shapes.LShape(Allocator.Temp);
        var immutable = shape.ToReadOnly().GetOrCreateImmutable();

        // Rotate then flip
        var rotatedThenFlipped = immutable.Rotate90().Flip();

        // Flip then rotate
        var flippedThenRotated = immutable.Flip().Rotate90();

        // These should generally produce different results
        // (unless the shape has special symmetries)
        Assert.AreNotEqual(immutable.Id, rotatedThenFlipped.Id);
        Assert.AreEqual(immutable.Id, flippedThenRotated.Id);

        shape.Dispose();
    }

    [Test]
    public void GetOrCreateImmutable_RequiresTrimmedShape()
    {
        // Create a shape with empty border
        var shape = new GridShape(5, 5, Allocator.Temp);
        shape.SetCell(new int2(2, 2), true); // Single cell in center

        // This should throw because shape is not trimmed
        Assert.Throws<System.ArgumentException>(() =>
        {
            shape.ToReadOnly().GetOrCreateImmutable();
        });

        // Trim and try again
        var trimmed = shape.ToReadOnly().Trim(Allocator.Temp);
        var immutable = trimmed.ToReadOnly().GetOrCreateImmutable();

        Assert.AreEqual(1, immutable.Width);
        Assert.AreEqual(1, immutable.Height);

        shape.Dispose();
        trimmed.Dispose();
    }

    [Test]
    public void Pattern_AccessibleAndCorrect()
    {
        var shape = new GridShape(2, 2, Allocator.Temp);
        shape.SetCell(new int2(0, 0), true);
        shape.SetCell(new int2(1, 1), true);

        var trimmedShape = shape.ToReadOnly().Trim(Allocator.Temp);
        var immutable = trimmedShape.ToReadOnly().GetOrCreateImmutable();

        var pattern = immutable.Pattern;

        Assert.IsTrue(pattern.IsSet(0)); // (0,0)
        Assert.IsFalse(pattern.IsSet(1)); // (1,0)
        Assert.IsFalse(pattern.IsSet(2)); // (0,1)
        Assert.IsTrue(pattern.IsSet(3)); // (1,1)

        shape.Dispose();
        trimmedShape.Dispose();
    }

    [Test]
    public void Rotate90_MultipleShapes_ShareRotations()
    {
        // Create two identical L-shapes
        var shape1 = Shapes.LShape(Allocator.Temp);
        var shape2 = Shapes.LShape(Allocator.Temp);

        var immutable1 = shape1.ToReadOnly().GetOrCreateImmutable();
        var immutable2 = shape2.ToReadOnly().GetOrCreateImmutable();

        // They should share the same ID
        Assert.AreEqual(immutable1.Id, immutable2.Id);

        // Their rotations should also share IDs
        var rotated1 = immutable1.Rotate90();
        var rotated2 = immutable2.Rotate90();

        Assert.AreEqual(rotated1.Id, rotated2.Id);

        shape1.Dispose();
        shape2.Dispose();
    }

    [Test]
    public void SingleCellShape_SymmetricUnderAllTransformations()
    {
        var shape = Shapes.Single(Allocator.Temp);
        var immutable = shape.ToReadOnly().GetOrCreateImmutable();

        // All transformations should return the same shape
        var rotated90 = immutable.Rotate90();
        var rotated180 = rotated90.Rotate90();
        var rotated270 = rotated180.Rotate90();
        var flipped = immutable.Flip();

        Assert.AreEqual(immutable.Id, rotated90.Id);
        Assert.AreEqual(immutable.Id, rotated180.Id);
        Assert.AreEqual(immutable.Id, rotated270.Id);
        Assert.AreEqual(immutable.Id, flipped.Id);

        shape.Dispose();
    }
}
