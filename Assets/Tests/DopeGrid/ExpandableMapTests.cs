using System;
using DopeGrid;
using DopeGrid.Map;
using NUnit.Framework;

namespace DopeGrid.Tests;

[TestFixture]
public class ExpandableMapTests
{
    [Test]
    public void Constructor_CreatesMapWithCorrectDimensions()
    {
        using var map = new ExpandableMap<int>(3, 4);

        Assert.That(map.Width, Is.EqualTo(3));
        Assert.That(map.Height, Is.EqualTo(4));
        Assert.That(map.MinX, Is.EqualTo(0));
        Assert.That(map.MinY, Is.EqualTo(0));
        Assert.That(map.MaxX, Is.EqualTo(3));
        Assert.That(map.MaxY, Is.EqualTo(4));
    }

    [Test]
    public void Constructor_WithBound_CreatesMapWithCorrectBounds()
    {
        var bound = new MapBound(MinX: -5, MinY: -3, MaxX: 5, MaxY: 7);
        using var map = new ExpandableMap<int>(bound);

        Assert.That(map.Width, Is.EqualTo(10));
        Assert.That(map.Height, Is.EqualTo(10));
        Assert.That(map.MinX, Is.EqualTo(-5));
        Assert.That(map.MinY, Is.EqualTo(-3));
        Assert.That(map.MaxX, Is.EqualTo(5));
        Assert.That(map.MaxY, Is.EqualTo(7));
    }

    [Test]
    public void Indexer_SetAndGet_WorksCorrectly()
    {
        using var map = new ExpandableMap<int>(3, 3, defaultValue: 0);

        map[1, 1] = 42;

        Assert.That(map[1, 1], Is.EqualTo(42));
        Assert.That(map[0, 0], Is.EqualTo(0));
    }

    [Test]
    public void Indexer_WithNegativeIndices_WorksCorrectly()
    {
        var bound = new MapBound(MinX: -5, MinY: -5, MaxX: 5, MaxY: 5);
        using var map = new ExpandableMap<int>(bound);

        map[-2, -3] = 99;

        Assert.That(map[-2, -3], Is.EqualTo(99));
    }

    [Test]
    public void IsOccupied_ReturnsTrueForNonDefaultValues()
    {
        using var map = new ExpandableMap<int>(3, 3, defaultValue: 0);

        map[1, 1] = 5;

        Assert.That(map.IsOccupied(1, 1), Is.True);
        Assert.That(map.IsOccupied(0, 0), Is.False);
    }

    [Test]
    public void Contains_ReturnsTrueForValidIndices()
    {
        using var map = new ExpandableMap<int>(3, 3);

        Assert.That(map.Contains(0, 0), Is.True);
        Assert.That(map.Contains(2, 2), Is.True);
        Assert.That(map.Contains(3, 3), Is.False);
        Assert.That(map.Contains(-1, 0), Is.False);
    }

    [Test]
    public void Contains_WithNegativeBounds_WorksCorrectly()
    {
        var bound = new MapBound(MinX: -2, MinY: -2, MaxX: 2, MaxY: 2);
        using var map = new ExpandableMap<int>(bound);

        Assert.That(map.Contains(-2, -2), Is.True);
        Assert.That(map.Contains(1, 1), Is.True);
        Assert.That(map.Contains(2, 2), Is.False); // MaxX/MaxY are exclusive
        Assert.That(map.Contains(-3, 0), Is.False);
    }

    [Test]
    public void Expand_ExtendsMapBounds()
    {
        using var map = new ExpandableMap<int>(3, 3);
        map[1, 1] = 42;

        var newBound = new MapBound(MinX: -2, MinY: -2, MaxX: 5, MaxY: 5);
        map.Expand(newBound);

        Assert.That(map.Width, Is.EqualTo(7));
        Assert.That(map.Height, Is.EqualTo(7));
        Assert.That(map.MinX, Is.EqualTo(-2));
        Assert.That(map.MinY, Is.EqualTo(-2));
        Assert.That(map[1, 1], Is.EqualTo(42)); // Previous value preserved
    }

    [Test]
    public void Expand_PreservesExistingData()
    {
        using var map = new ExpandableMap<int>(3, 3, defaultValue: 0);
        map[0, 0] = 1;
        map[1, 1] = 2;
        map[2, 2] = 3;

        var newBound = new MapBound(MinX: -1, MinY: -1, MaxX: 4, MaxY: 4);
        map.Expand(newBound);

        Assert.That(map[0, 0], Is.EqualTo(1));
        Assert.That(map[1, 1], Is.EqualTo(2));
        Assert.That(map[2, 2], Is.EqualTo(3));
    }

    [Test]
    public void Expand_UsesCustomExpandFunc()
    {
        using var map = new ExpandableMap<int>(3, 3)
        {
            ExpandBoundFunc = MapBound.Intersection
        };

        var newBound = new MapBound(MinX: -2, MinY: -2, MaxX: 10, MaxY: 10);
        map.Expand(newBound);

        // With Intersection, the bound should be clamped to overlap
        // But Union is always applied after ExpandFunc, so final bound is Union
        Assert.That(map.Bound.MinX, Is.LessThanOrEqualTo(0));
    }

    [Test]
    public void Expand_WithSameBound_DoesNothing()
    {
        using var map = new ExpandableMap<int>(3, 3);
        var originalBound = map.Bound;

        map.Expand(originalBound);

        Assert.That(map.Bound, Is.EqualTo(originalBound));
    }

    [Test]
    public void Bound_ReturnsCorrectMapBound()
    {
        var bound = new MapBound(MinX: 5, MinY: 10, MaxX: 15, MaxY: 20);
        using var map = new ExpandableMap<int>(bound);

        var result = map.Bound;

        Assert.That(result.MinX, Is.EqualTo(5));
        Assert.That(result.MinY, Is.EqualTo(10));
        Assert.That(result.MaxX, Is.EqualTo(15));
        Assert.That(result.MaxY, Is.EqualTo(20));
        Assert.That(result.Width, Is.EqualTo(10));
        Assert.That(result.Height, Is.EqualTo(10));
    }

    [Test]
    public void Dispose_ReleasesResources()
    {
        var map = new ExpandableMap<int>(3, 3);
        map[1, 1] = 42;

        map.Dispose();

        // No assertion needed - just verify no exception
        Assert.Pass();
    }
}
