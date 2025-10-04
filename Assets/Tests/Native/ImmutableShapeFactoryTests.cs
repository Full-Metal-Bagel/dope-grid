using DopeGrid;
using DopeGrid.Native;
using NUnit.Framework;
using Unity.Collections;

[TestFixture]
public class ImmutableShapeFactoryTests
{
    [Test]
    public void ImmutableSingle_CreatesSingleCellShape()
    {
        var immutable = Shapes.ImmutableSingle();

        Assert.AreEqual(1, immutable.Width);
        Assert.AreEqual(1, immutable.Height);
        Assert.AreNotEqual(0, immutable.Id);

        // Verify pattern
        var pattern = immutable.Pattern;
        Assert.IsTrue(pattern.Get(0));
    }

    [Test]
    public void ImmutableLine_CreatesHorizontalLine()
    {
        var immutable = Shapes.ImmutableLine(4);

        Assert.AreEqual(4, immutable.Width);
        Assert.AreEqual(1, immutable.Height);
        Assert.AreNotEqual(0, immutable.Id);

        // Verify pattern
        var pattern = immutable.Pattern;
        for (var i = 0; i < 4; i++)
        {
            Assert.IsTrue(pattern.Get(i));
        }
    }

    [Test]
    public void ImmutableSquare_CreatesFilledSquare()
    {
        var immutable = Shapes.ImmutableSquare(3);

        Assert.AreEqual(3, immutable.Width);
        Assert.AreEqual(3, immutable.Height);
        Assert.AreNotEqual(0, immutable.Id);

        // Verify all cells are filled
        var pattern = immutable.Pattern;
        for (var i = 0; i < 9; i++)
        {
            Assert.IsTrue(pattern.Get(i));
        }
    }

    [Test]
    public void ImmutableLShape_CreatesLShape()
    {
        var immutable = Shapes.ImmutableLShape();

        Assert.AreEqual(2, immutable.Width);
        Assert.AreEqual(2, immutable.Height);
        Assert.AreNotEqual(0, immutable.Id);

        // Verify L pattern (0,0), (0,1), (1,1)
        var pattern = immutable.Pattern;
        Assert.IsTrue(pattern.Get(0));  // (0,0)
        Assert.IsTrue(pattern.Get(2));  // (0,1)
        Assert.IsTrue(pattern.Get(3));  // (1,1)
        Assert.IsFalse(pattern.Get(1)); // (1,0)
    }

    [Test]
    public void ImmutableTShape_CreatesTShape()
    {
        var immutable = Shapes.ImmutableTShape();

        Assert.AreEqual(3, immutable.Width);
        Assert.AreEqual(2, immutable.Height);
        Assert.AreNotEqual(0, immutable.Id);

        // Verify T pattern - top row fully filled, bottom row middle only
        var pattern = immutable.Pattern;
        Assert.IsTrue(pattern.Get(0));  // (0,0)
        Assert.IsTrue(pattern.Get(1));  // (1,0)
        Assert.IsTrue(pattern.Get(2));  // (2,0)
        Assert.IsFalse(pattern.Get(3)); // (0,1)
        Assert.IsTrue(pattern.Get(4));  // (1,1)
        Assert.IsFalse(pattern.Get(5)); // (2,1)
    }

    [Test]
    public void ImmutableCross_CreatesCrossShape()
    {
        var immutable = Shapes.ImmutableCross();

        Assert.AreEqual(3, immutable.Width);
        Assert.AreEqual(3, immutable.Height);
        Assert.AreNotEqual(0, immutable.Id);

        // Verify cross pattern
        var pattern = immutable.Pattern;
        Assert.IsFalse(pattern.Get(0)); // (0,0)
        Assert.IsTrue(pattern.Get(1));  // (1,0)
        Assert.IsFalse(pattern.Get(2)); // (2,0)
        Assert.IsTrue(pattern.Get(3));  // (0,1)
        Assert.IsTrue(pattern.Get(4));  // (1,1)
        Assert.IsTrue(pattern.Get(5));  // (2,1)
        Assert.IsFalse(pattern.Get(6)); // (0,2)
        Assert.IsTrue(pattern.Get(7));  // (1,2)
        Assert.IsFalse(pattern.Get(8)); // (2,2)
    }

    [Test]
    public void ImmutableShapes_CachedCorrectly()
    {
        // Same shapes should return same IDs
        var lShape1 = Shapes.ImmutableLShape();
        var lShape2 = Shapes.ImmutableLShape();
        Assert.AreEqual(lShape1.Id, lShape2.Id);

        var line1 = Shapes.ImmutableLine(3);
        var line2 = Shapes.ImmutableLine(3);
        Assert.AreEqual(line1.Id, line2.Id);

        // Different shapes should have different IDs
        var line3 = Shapes.ImmutableLine(4);
        Assert.AreNotEqual(line1.Id, line3.Id);
    }

    [Test]
    public void ImmutableShapes_CanConvertToGridShape()
    {
        var immutableCross = Shapes.ImmutableCross();

        // Convert to mutable GridShape
        var mutableShape = new GridShape(3, 3, Allocator.Temp);
        immutableCross.CopyTo(mutableShape);

        // Verify the pattern was copied correctly
        Assert.IsFalse(mutableShape.GetCellValue(0, 0));
        Assert.IsTrue(mutableShape.GetCellValue(1, 0));
        Assert.IsFalse(mutableShape.GetCellValue(2, 0));
        Assert.IsTrue(mutableShape.GetCellValue(0, 1));
        Assert.IsTrue(mutableShape.GetCellValue(1, 1));
        Assert.IsTrue(mutableShape.GetCellValue(2, 1));
        Assert.IsFalse(mutableShape.GetCellValue(0, 2));
        Assert.IsTrue(mutableShape.GetCellValue(1, 2));
        Assert.IsFalse(mutableShape.GetCellValue(2, 2));
        mutableShape.Dispose();
    }
}
