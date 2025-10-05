using System.Numerics;
using DopeGrid;

namespace DopeGrid.Tests;

[TestFixture]
public class RotationDegreeTests
{
    [Test]
    public void GetNextClockwiseRotation_ReturnsCorrectRotation()
    {
        Assert.That(RotationDegree.None.GetNextClockwiseRotation(), Is.EqualTo(RotationDegree.Clockwise90));
        Assert.That(RotationDegree.Clockwise90.GetNextClockwiseRotation(), Is.EqualTo(RotationDegree.Clockwise180));
        Assert.That(RotationDegree.Clockwise180.GetNextClockwiseRotation(), Is.EqualTo(RotationDegree.Clockwise270));
        Assert.That(RotationDegree.Clockwise270.GetNextClockwiseRotation(), Is.EqualTo(RotationDegree.None));
    }

    [Test]
    public void GetPreviousClockwiseRotation_ReturnsCorrectRotation()
    {
        Assert.That(RotationDegree.None.GetPreviousClockwiseRotation(), Is.EqualTo(RotationDegree.Clockwise270));
        Assert.That(RotationDegree.Clockwise90.GetPreviousClockwiseRotation(), Is.EqualTo(RotationDegree.None));
        Assert.That(RotationDegree.Clockwise180.GetPreviousClockwiseRotation(), Is.EqualTo(RotationDegree.Clockwise90));
        Assert.That(RotationDegree.Clockwise270.GetPreviousClockwiseRotation(), Is.EqualTo(RotationDegree.Clockwise180));
    }

    [Test]
    public void GetZRotation_ReturnsCorrectAngle()
    {
        Assert.That(RotationDegree.None.GetZRotation(), Is.EqualTo(0f));
        Assert.That(RotationDegree.Clockwise90.GetZRotation(), Is.EqualTo(-90f));
        Assert.That(RotationDegree.Clockwise180.GetZRotation(), Is.EqualTo(-180f));
        Assert.That(RotationDegree.Clockwise270.GetZRotation(), Is.EqualTo(-270f));
    }

    [Test]
    public void GetRotationOffset_ReturnsCorrectOffset()
    {
        var size = new Vector2(10f, 20f);

        var offset0 = RotationDegree.None.GetRotationOffset(size);
        Assert.That(offset0.X, Is.EqualTo(0f));
        Assert.That(offset0.Y, Is.EqualTo(0f));

        var offset90 = RotationDegree.Clockwise90.GetRotationOffset(size);
        Assert.That(offset90.X, Is.EqualTo(20f));
        Assert.That(offset90.Y, Is.EqualTo(0f));

        var offset180 = RotationDegree.Clockwise180.GetRotationOffset(size);
        Assert.That(offset180.X, Is.EqualTo(10f));
        Assert.That(offset180.Y, Is.EqualTo(-20f));

        var offset270 = RotationDegree.Clockwise270.GetRotationOffset(size);
        Assert.That(offset270.X, Is.EqualTo(0f));
        Assert.That(offset270.Y, Is.EqualTo(-10f));
    }

    [Test]
    public void CalculateRotatedSize_Vector2_ReturnsCorrectSize()
    {
        var size = new Vector2(10f, 20f);

        var rotated0 = RotationDegree.None.CalculateRotatedSize(size);
        Assert.That(rotated0.X, Is.EqualTo(10f));
        Assert.That(rotated0.Y, Is.EqualTo(20f));

        var rotated90 = RotationDegree.Clockwise90.CalculateRotatedSize(size);
        Assert.That(rotated90.X, Is.EqualTo(20f));
        Assert.That(rotated90.Y, Is.EqualTo(10f));

        var rotated180 = RotationDegree.Clockwise180.CalculateRotatedSize(size);
        Assert.That(rotated180.X, Is.EqualTo(10f));
        Assert.That(rotated180.Y, Is.EqualTo(20f));

        var rotated270 = RotationDegree.Clockwise270.CalculateRotatedSize(size);
        Assert.That(rotated270.X, Is.EqualTo(20f));
        Assert.That(rotated270.Y, Is.EqualTo(10f));
    }

    [Test]
    public void CalculateRotatedSize_Integers_ReturnsCorrectSize()
    {
        var (rotatedWidth0, rotatedHeight0) = RotationDegree.None.CalculateRotatedSize(10, 20);
        Assert.That(rotatedWidth0, Is.EqualTo(10));
        Assert.That(rotatedHeight0, Is.EqualTo(20));

        var (rotatedWidth90, rotatedHeight90) = RotationDegree.Clockwise90.CalculateRotatedSize(10, 20);
        Assert.That(rotatedWidth90, Is.EqualTo(20));
        Assert.That(rotatedHeight90, Is.EqualTo(10));

        var (rotatedWidth180, rotatedHeight180) = RotationDegree.Clockwise180.CalculateRotatedSize(10, 20);
        Assert.That(rotatedWidth180, Is.EqualTo(10));
        Assert.That(rotatedHeight180, Is.EqualTo(20));

        var (rotatedWidth270, rotatedHeight270) = RotationDegree.Clockwise270.CalculateRotatedSize(10, 20);
        Assert.That(rotatedWidth270, Is.EqualTo(20));
        Assert.That(rotatedHeight270, Is.EqualTo(10));
    }

    [Test]
    public void RotationCycle_CompletesFullCycle()
    {
        var rotation = RotationDegree.None;

        rotation = rotation.GetNextClockwiseRotation();
        Assert.That(rotation, Is.EqualTo(RotationDegree.Clockwise90));

        rotation = rotation.GetNextClockwiseRotation();
        Assert.That(rotation, Is.EqualTo(RotationDegree.Clockwise180));

        rotation = rotation.GetNextClockwiseRotation();
        Assert.That(rotation, Is.EqualTo(RotationDegree.Clockwise270));

        rotation = rotation.GetNextClockwiseRotation();
        Assert.That(rotation, Is.EqualTo(RotationDegree.None));
    }

    [Test]
    public void ReverseCycle_CompletesFullCycle()
    {
        var rotation = RotationDegree.None;

        rotation = rotation.GetPreviousClockwiseRotation();
        Assert.That(rotation, Is.EqualTo(RotationDegree.Clockwise270));

        rotation = rotation.GetPreviousClockwiseRotation();
        Assert.That(rotation, Is.EqualTo(RotationDegree.Clockwise180));

        rotation = rotation.GetPreviousClockwiseRotation();
        Assert.That(rotation, Is.EqualTo(RotationDegree.Clockwise90));

        rotation = rotation.GetPreviousClockwiseRotation();
        Assert.That(rotation, Is.EqualTo(RotationDegree.None));
    }

    [Test]
    public void SquareSize_RemainsUnchangedForAllRotations()
    {
        var size = new Vector2(10f, 10f);

        Assert.That(RotationDegree.None.CalculateRotatedSize(size), Is.EqualTo(size));
        Assert.That(RotationDegree.Clockwise90.CalculateRotatedSize(size), Is.EqualTo(size));
        Assert.That(RotationDegree.Clockwise180.CalculateRotatedSize(size), Is.EqualTo(size));
        Assert.That(RotationDegree.Clockwise270.CalculateRotatedSize(size), Is.EqualTo(size));
    }

    [Test]
    public void EnumValues_HaveCorrectNumericValues()
    {
        Assert.That((int)RotationDegree.None, Is.EqualTo(0));
        Assert.That((int)RotationDegree.Clockwise90, Is.EqualTo(1));
        Assert.That((int)RotationDegree.Clockwise180, Is.EqualTo(2));
        Assert.That((int)RotationDegree.Clockwise270, Is.EqualTo(3));
    }
}
