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
        float width = 10f;
        float height = 20f;

        var (offsetX0, offsetY0) = RotationDegree.None.GetRotationOffset(width, height);
        Assert.That(offsetX0, Is.EqualTo(0f));
        Assert.That(offsetY0, Is.EqualTo(0f));

        var (offsetX90, offsetY90) = RotationDegree.Clockwise90.GetRotationOffset(width, height);
        Assert.That(offsetX90, Is.EqualTo(20f));
        Assert.That(offsetY90, Is.EqualTo(0f));

        var (offsetX180, offsetY180) = RotationDegree.Clockwise180.GetRotationOffset(width, height);
        Assert.That(offsetX180, Is.EqualTo(10f));
        Assert.That(offsetY180, Is.EqualTo(-20f));

        var (offsetX270, offsetY270) = RotationDegree.Clockwise270.GetRotationOffset(width, height);
        Assert.That(offsetX270, Is.EqualTo(0f));
        Assert.That(offsetY270, Is.EqualTo(-10f));
    }

    [Test]
    public void CalculateRotatedSize_Floats_ReturnsCorrectSize()
    {
        float width = 10f;
        float height = 20f;

        var (rotatedWidth0, rotatedHeight0) = RotationDegree.None.CalculateRotatedSize(width, height);
        Assert.That(rotatedWidth0, Is.EqualTo(10f));
        Assert.That(rotatedHeight0, Is.EqualTo(20f));

        var (rotatedWidth90, rotatedHeight90) = RotationDegree.Clockwise90.CalculateRotatedSize(width, height);
        Assert.That(rotatedWidth90, Is.EqualTo(20f));
        Assert.That(rotatedHeight90, Is.EqualTo(10f));

        var (rotatedWidth180, rotatedHeight180) = RotationDegree.Clockwise180.CalculateRotatedSize(width, height);
        Assert.That(rotatedWidth180, Is.EqualTo(10f));
        Assert.That(rotatedHeight180, Is.EqualTo(20f));

        var (rotatedWidth270, rotatedHeight270) = RotationDegree.Clockwise270.CalculateRotatedSize(width, height);
        Assert.That(rotatedWidth270, Is.EqualTo(20f));
        Assert.That(rotatedHeight270, Is.EqualTo(10f));
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
        float squareSize = 10f;

        var (width0, height0) = RotationDegree.None.CalculateRotatedSize(squareSize, squareSize);
        Assert.That(width0, Is.EqualTo(squareSize));
        Assert.That(height0, Is.EqualTo(squareSize));

        var (width90, height90) = RotationDegree.Clockwise90.CalculateRotatedSize(squareSize, squareSize);
        Assert.That(width90, Is.EqualTo(squareSize));
        Assert.That(height90, Is.EqualTo(squareSize));

        var (width180, height180) = RotationDegree.Clockwise180.CalculateRotatedSize(squareSize, squareSize);
        Assert.That(width180, Is.EqualTo(squareSize));
        Assert.That(height180, Is.EqualTo(squareSize));

        var (width270, height270) = RotationDegree.Clockwise270.CalculateRotatedSize(squareSize, squareSize);
        Assert.That(width270, Is.EqualTo(squareSize));
        Assert.That(height270, Is.EqualTo(squareSize));
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
