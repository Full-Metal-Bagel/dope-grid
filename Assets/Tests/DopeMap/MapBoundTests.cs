using NUnit.Framework;

namespace DopeGrid.Map.Tests;

[TestFixture]
public class MapBoundTests
{
    [Test]
    public void Constructor_CreatesMapBoundWithCorrectValues()
    {
        var bound = new MapBound(MinX: 1, MinY: 2, MaxX: 5, MaxY: 7);

        Assert.That(bound.MinX, Is.EqualTo(1));
        Assert.That(bound.MinY, Is.EqualTo(2));
        Assert.That(bound.MaxX, Is.EqualTo(5));
        Assert.That(bound.MaxY, Is.EqualTo(7));
        Assert.That(bound.Width, Is.EqualTo(4));
        Assert.That(bound.Height, Is.EqualTo(5));
    }

    [Test]
    public void Create_CreatesMapBoundFromWidthHeight()
    {
        var bound = MapBound.Create(minX: 2, minY: 3, width: 10, height: 15);

        Assert.That(bound.MinX, Is.EqualTo(2));
        Assert.That(bound.MinY, Is.EqualTo(3));
        Assert.That(bound.MaxX, Is.EqualTo(12));
        Assert.That(bound.MaxY, Is.EqualTo(18));
        Assert.That(bound.Width, Is.EqualTo(10));
        Assert.That(bound.Height, Is.EqualTo(15));
    }

    [Test]
    public void Contains_ReturnsTrueForPointInsideBound()
    {
        var bound = new MapBound(MinX: 0, MinY: 0, MaxX: 10, MaxY: 10);

        Assert.That(bound.Contains(0, 0), Is.True);
        Assert.That(bound.Contains(5, 5), Is.True);
        Assert.That(bound.Contains(9, 9), Is.True);
    }

    [Test]
    public void Contains_ReturnsFalseForPointOutsideBound()
    {
        var bound = new MapBound(MinX: 0, MinY: 0, MaxX: 10, MaxY: 10);

        Assert.That(bound.Contains(10, 10), Is.False); // MaxX/MaxY are exclusive
        Assert.That(bound.Contains(-1, 5), Is.False);
        Assert.That(bound.Contains(5, -1), Is.False);
        Assert.That(bound.Contains(15, 15), Is.False);
    }

    [Test]
    public void Contains_WithNegativeBounds_WorksCorrectly()
    {
        var bound = new MapBound(MinX: -5, MinY: -10, MaxX: 5, MaxY: 10);

        Assert.That(bound.Contains(-5, -10), Is.True);
        Assert.That(bound.Contains(0, 0), Is.True);
        Assert.That(bound.Contains(4, 9), Is.True);
        Assert.That(bound.Contains(5, 10), Is.False);
        Assert.That(bound.Contains(-6, 0), Is.False);
    }

    [Test]
    public void Union_CombinesTwoBounds()
    {
        var bound1 = new MapBound(MinX: 0, MinY: 0, MaxX: 5, MaxY: 5);
        var bound2 = new MapBound(MinX: 3, MinY: 3, MaxX: 10, MaxY: 10);

        var result = MapBound.Union(bound1, bound2);

        Assert.That(result.MinX, Is.EqualTo(0));
        Assert.That(result.MinY, Is.EqualTo(0));
        Assert.That(result.MaxX, Is.EqualTo(10));
        Assert.That(result.MaxY, Is.EqualTo(10));
    }

    [Test]
    public void Union_WithNonOverlappingBounds_CreatesLargerBound()
    {
        var bound1 = new MapBound(MinX: 0, MinY: 0, MaxX: 5, MaxY: 5);
        var bound2 = new MapBound(MinX: 10, MinY: 10, MaxX: 15, MaxY: 15);

        var result = MapBound.Union(bound1, bound2);

        Assert.That(result.MinX, Is.EqualTo(0));
        Assert.That(result.MinY, Is.EqualTo(0));
        Assert.That(result.MaxX, Is.EqualTo(15));
        Assert.That(result.MaxY, Is.EqualTo(15));
    }

    [Test]
    public void Intersection_FindsOverlapBetweenBounds()
    {
        var bound1 = new MapBound(MinX: 0, MinY: 0, MaxX: 10, MaxY: 10);
        var bound2 = new MapBound(MinX: 5, MinY: 5, MaxX: 15, MaxY: 15);

        var result = MapBound.Intersection(bound1, bound2);

        Assert.That(result.MinX, Is.EqualTo(5));
        Assert.That(result.MinY, Is.EqualTo(5));
        Assert.That(result.MaxX, Is.EqualTo(10));
        Assert.That(result.MaxY, Is.EqualTo(10));
    }

    [Test]
    public void Intersection_WithNoOverlap_CreatesInvalidBound()
    {
        var bound1 = new MapBound(MinX: 0, MinY: 0, MaxX: 5, MaxY: 5);
        var bound2 = new MapBound(MinX: 10, MinY: 10, MaxX: 15, MaxY: 15);

        var result = MapBound.Intersection(bound1, bound2);

        // Result will have MinX > MaxX and MinY > MaxY (invalid bound)
        Assert.That(result.MinX, Is.EqualTo(10));
        Assert.That(result.MinY, Is.EqualTo(10));
        Assert.That(result.MaxX, Is.EqualTo(5));
        Assert.That(result.MaxY, Is.EqualTo(5));
        Assert.That(result.Width, Is.LessThan(0));
        Assert.That(result.Height, Is.LessThan(0));
    }

    [Test]
    public void ExpandToInclude_PointInsideBound_ReturnsOriginalBound()
    {
        var bound = new MapBound(MinX: 0, MinY: 0, MaxX: 10, MaxY: 10);

        var result = MapBound.ExpandToInclude(bound, x: 5, y: 5);

        Assert.That(result, Is.EqualTo(bound));
    }

    [Test]
    public void ExpandToInclude_PointAtMinCorner_ReturnsOriginalBound()
    {
        var bound = new MapBound(MinX: 2, MinY: 3, MaxX: 10, MaxY: 12);

        var result = MapBound.ExpandToInclude(bound, x: 2, y: 3);

        Assert.That(result, Is.EqualTo(bound));
    }

    [Test]
    public void ExpandToInclude_PointAtMaxCornerMinusOne_ReturnsOriginalBound()
    {
        var bound = new MapBound(MinX: 0, MinY: 0, MaxX: 10, MaxY: 10);

        var result = MapBound.ExpandToInclude(bound, x: 9, y: 9);

        Assert.That(result, Is.EqualTo(bound));
    }

    [Test]
    public void ExpandToInclude_PointBelowMinX_ExpandsLeft()
    {
        var bound = new MapBound(MinX: 5, MinY: 5, MaxX: 10, MaxY: 10);

        var result = MapBound.ExpandToInclude(bound, x: 2, y: 7);

        Assert.That(result.MinX, Is.EqualTo(2));
        Assert.That(result.MinY, Is.EqualTo(5));
        Assert.That(result.MaxX, Is.EqualTo(10));
        Assert.That(result.MaxY, Is.EqualTo(10));
    }

    [Test]
    public void ExpandToInclude_PointBelowMinY_ExpandsDown()
    {
        var bound = new MapBound(MinX: 5, MinY: 5, MaxX: 10, MaxY: 10);

        var result = MapBound.ExpandToInclude(bound, x: 7, y: 2);

        Assert.That(result.MinX, Is.EqualTo(5));
        Assert.That(result.MinY, Is.EqualTo(2));
        Assert.That(result.MaxX, Is.EqualTo(10));
        Assert.That(result.MaxY, Is.EqualTo(10));
    }

    [Test]
    public void ExpandToInclude_PointAboveMaxX_ExpandsRight()
    {
        var bound = new MapBound(MinX: 0, MinY: 0, MaxX: 10, MaxY: 10);

        var result = MapBound.ExpandToInclude(bound, x: 15, y: 5);

        Assert.That(result.MinX, Is.EqualTo(0));
        Assert.That(result.MinY, Is.EqualTo(0));
        Assert.That(result.MaxX, Is.EqualTo(16)); // 15 + 1
        Assert.That(result.MaxY, Is.EqualTo(10));
    }

    [Test]
    public void ExpandToInclude_PointAboveMaxY_ExpandsUp()
    {
        var bound = new MapBound(MinX: 0, MinY: 0, MaxX: 10, MaxY: 10);

        var result = MapBound.ExpandToInclude(bound, x: 5, y: 15);

        Assert.That(result.MinX, Is.EqualTo(0));
        Assert.That(result.MinY, Is.EqualTo(0));
        Assert.That(result.MaxX, Is.EqualTo(10));
        Assert.That(result.MaxY, Is.EqualTo(16)); // 15 + 1
    }

    [Test]
    public void ExpandToInclude_NegativePoint_ExpandsNegatively()
    {
        var bound = new MapBound(MinX: 0, MinY: 0, MaxX: 10, MaxY: 10);

        var result = MapBound.ExpandToInclude(bound, x: -5, y: -3);

        Assert.That(result.MinX, Is.EqualTo(-5));
        Assert.That(result.MinY, Is.EqualTo(-3));
        Assert.That(result.MaxX, Is.EqualTo(10));
        Assert.That(result.MaxY, Is.EqualTo(10));
    }

    [Test]
    public void ExpandToInclude_PointFarOutside_ExpandsInAllDirections()
    {
        var bound = new MapBound(MinX: 5, MinY: 5, MaxX: 10, MaxY: 10);

        var result = MapBound.ExpandToInclude(bound, x: 20, y: -10);

        Assert.That(result.MinX, Is.EqualTo(5));
        Assert.That(result.MinY, Is.EqualTo(-10));
        Assert.That(result.MaxX, Is.EqualTo(21)); // 20 + 1
        Assert.That(result.MaxY, Is.EqualTo(10));
    }

    [Test]
    public void ExpandToInclude_WithNegativeBounds_WorksCorrectly()
    {
        var bound = new MapBound(MinX: -10, MinY: -10, MaxX: -5, MaxY: -5);

        var result = MapBound.ExpandToInclude(bound, x: -15, y: -2);

        Assert.That(result.MinX, Is.EqualTo(-15));
        Assert.That(result.MinY, Is.EqualTo(-10));
        Assert.That(result.MaxX, Is.EqualTo(-5));
        Assert.That(result.MaxY, Is.EqualTo(-1)); // -2 + 1
    }

    [Test]
    public void ExpandToInclude_SequentialExpansion_WorksCorrectly()
    {
        var bound = new MapBound(MinX: 0, MinY: 0, MaxX: 5, MaxY: 5);

        var result1 = MapBound.ExpandToInclude(bound, x: -2, y: 3);
        var result2 = MapBound.ExpandToInclude(result1, x: 3, y: 10);
        var result3 = MapBound.ExpandToInclude(result2, x: 7, y: -1);

        Assert.That(result3.MinX, Is.EqualTo(-2));
        Assert.That(result3.MinY, Is.EqualTo(-1));
        Assert.That(result3.MaxX, Is.EqualTo(8)); // 7 + 1
        Assert.That(result3.MaxY, Is.EqualTo(11)); // 10 + 1
    }
}
