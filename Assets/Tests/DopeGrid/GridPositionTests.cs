using DopeGrid;

namespace DopeGrid.Tests;

[TestFixture]
public class GridPositionTests
{
    [Test]
    public void Constructor_CreatesPositionWithCorrectValues()
    {
        var pos = new GridPosition(3, 5);

        Assert.That(pos.X, Is.EqualTo(3));
        Assert.That(pos.Y, Is.EqualTo(5));
    }

    [Test]
    public void Zero_HasZeroCoordinates()
    {
        Assert.That(GridPosition.Zero.X, Is.EqualTo(0));
        Assert.That(GridPosition.Zero.Y, Is.EqualTo(0));
    }

    [Test]
    public void Invalid_HasNegativeCoordinates()
    {
        Assert.That(GridPosition.Invalid.X, Is.EqualTo(-1));
        Assert.That(GridPosition.Invalid.Y, Is.EqualTo(-1));
    }

    [Test]
    public void IsValid_WithValidPosition_ReturnsTrue()
    {
        var pos = new GridPosition(0, 0);
        Assert.That(pos.IsValid, Is.True);

        pos = new GridPosition(5, 10);
        Assert.That(pos.IsValid, Is.True);
    }

    [Test]
    public void IsValid_WithInvalidPosition_ReturnsFalse()
    {
        Assert.That(GridPosition.Invalid.IsValid, Is.False);
    }

    [Test]
    public void IsInvalid_WithInvalidPosition_ReturnsTrue()
    {
        Assert.That(GridPosition.Invalid.IsInvalid, Is.True);
    }

    [Test]
    public void IsInvalid_WithValidPosition_ReturnsFalse()
    {
        var pos = new GridPosition(0, 0);
        Assert.That(pos.IsInvalid, Is.False);
    }

    [Test]
    public void ImplicitConversion_FromTuple_Works()
    {
        GridPosition pos = (3, 5);

        Assert.That(pos.X, Is.EqualTo(3));
        Assert.That(pos.Y, Is.EqualTo(5));
    }

    [Test]
    public void ImplicitConversion_ToTuple_Works()
    {
        var pos = new GridPosition(3, 5);
        (int x, int y) = pos;

        Assert.That(x, Is.EqualTo(3));
        Assert.That(y, Is.EqualTo(5));
    }

    [Test]
    public void Deconstruct_ReturnsCorrectValues()
    {
        var pos = new GridPosition(7, 9);
        var (x, y) = pos;

        Assert.That(x, Is.EqualTo(7));
        Assert.That(y, Is.EqualTo(9));
    }

    [Test]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        var pos1 = new GridPosition(3, 5);
        var pos2 = new GridPosition(3, 5);

        Assert.That(pos1.Equals(pos2), Is.True);
        Assert.That(pos1 == pos2, Is.True);
    }

    [Test]
    public void Equals_WithDifferentValues_ReturnsFalse()
    {
        var pos1 = new GridPosition(3, 5);
        var pos2 = new GridPosition(3, 6);

        Assert.That(pos1.Equals(pos2), Is.False);
        Assert.That(pos1 != pos2, Is.True);
    }

    [Test]
    public void ToString_ReturnsFormattedString()
    {
        var pos = new GridPosition(3, 5);
        Assert.That(pos.ToString(), Is.EqualTo("(3, 5)"));
    }

    [Test]
    public void GetHashCode_SamePositions_ReturnSameHash()
    {
        var pos1 = new GridPosition(3, 5);
        var pos2 = new GridPosition(3, 5);

        Assert.That(pos1.GetHashCode(), Is.EqualTo(pos2.GetHashCode()));
    }

    [Test]
    public void RecordEquality_Works()
    {
        var pos1 = new GridPosition(3, 5);
        var pos2 = new GridPosition(3, 5);
        var pos3 = new GridPosition(4, 5);

        Assert.That(pos1, Is.EqualTo(pos2));
        Assert.That(pos1, Is.Not.EqualTo(pos3));
    }

    [Test]
    public void NegativeCoordinates_AreAllowed()
    {
        var pos = new GridPosition(-5, -10);

        Assert.That(pos.X, Is.EqualTo(-5));
        Assert.That(pos.Y, Is.EqualTo(-10));
        Assert.That(pos.IsValid, Is.True);
    }
}
