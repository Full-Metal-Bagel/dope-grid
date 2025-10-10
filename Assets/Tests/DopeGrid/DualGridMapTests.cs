using System;
using DopeGrid;
using DopeGrid.Map;
using NUnit.Framework;

namespace DopeGrid.Tests;

[TestFixture]
public class DualGridMapTests
{
    [Test]
    public void Constructor_CreatesMapWithCorrectDimensions()
    {
        using var map = new DualGridMap<int>(3, 4);

        Assert.That(map.Width, Is.EqualTo(5)); // 3 + 1 padding on each side
        Assert.That(map.Height, Is.EqualTo(6)); // 4 + 1 padding on each side
        Assert.That(map.MinX, Is.EqualTo(-1));
        Assert.That(map.MinY, Is.EqualTo(-1));
        Assert.That(map.MaxX, Is.EqualTo(4));
        Assert.That(map.MaxY, Is.EqualTo(5));
    }

    [Test]
    public void Constructor_WithNegativeMinXY_CreatesMapWithCorrectBounds()
    {
        using var map = new DualGridMap<int>(3, 4, minX: -5, minY: -3);

        // minX - 1, minY - 1 to minX + width + 1, minY + height + 1
        Assert.That(map.MinX, Is.EqualTo(-6));
        Assert.That(map.MinY, Is.EqualTo(-4));
        Assert.That(map.MaxX, Is.EqualTo(-1)); // -5 + 3 + 1
        Assert.That(map.MaxY, Is.EqualTo(2));  // -3 + 4 + 1
    }

    [Test]
    public void Constructor_WithBound_CreatesMapWithCorrectBounds()
    {
        var bound = new MapBound(MinX: 0, MinY: 0, MaxX: 10, MaxY: 10);
        using var map = new DualGridMap<int>(bound);

        Assert.That(map.MinX, Is.EqualTo(0));
        Assert.That(map.MinY, Is.EqualTo(0));
        Assert.That(map.MaxX, Is.EqualTo(10));
        Assert.That(map.MaxY, Is.EqualTo(10));
        Assert.That(map.Width, Is.EqualTo(10));
        Assert.That(map.Height, Is.EqualTo(10));
    }

    [Test]
    public void Indexer_SetAndGet_WorksCorrectly()
    {
        using var map = new DualGridMap<int>(3, 3, defaultValue: 0);

        map[0, 0] = 42;

        Assert.That(map[0, 0], Is.EqualTo(42));
        Assert.That(map[1, 1], Is.EqualTo(0));
    }

    [Test]
    public void IsOccupied_ReturnsTrueForNonDefaultValues()
    {
        using var map = new DualGridMap<int>(3, 3, defaultValue: 0);

        map[0, 0] = 5;

        Assert.That(map.IsOccupied(0, 0), Is.True);
        Assert.That(map.IsOccupied(1, 1), Is.False);
    }

    [Test]
    public void GetVertexNeighbors_ReturnsCorrectValues()
    {
        using var map = new DualGridMap<int>(5, 5, defaultValue: 0);

        // Set up a 2x2 grid pattern around vertex (1, 1)
        map[0, 0] = 1; // Bottom-left
        map[1, 0] = 2; // Bottom-right
        map[0, 1] = 3; // Top-left
        map[1, 1] = 4; // Top-right

        var (bl, br, tl, tr) = map.GetVertexNeighbors(1, 1);

        Assert.That(bl, Is.EqualTo(1)); // [x-1, y-1] = [0, 0]
        Assert.That(br, Is.EqualTo(2)); // [x, y-1] = [1, 0]
        Assert.That(tl, Is.EqualTo(3)); // [x-1, y] = [0, 1]
        Assert.That(tr, Is.EqualTo(4)); // [x, y] = [1, 1]
    }

    [Test]
    public void GetVertexNeighbors_AtOrigin_WorksCorrectly()
    {
        using var map = new DualGridMap<int>(3, 3, defaultValue: 0);

        map[-1, -1] = 10;
        map[0, -1] = 20;
        map[-1, 0] = 30;
        map[0, 0] = 40;

        var (bl, br, tl, tr) = map.GetVertexNeighbors(0, 0);

        Assert.That(bl, Is.EqualTo(10));
        Assert.That(br, Is.EqualTo(20));
        Assert.That(tl, Is.EqualTo(30));
        Assert.That(tr, Is.EqualTo(40));
    }

    [Test]
    public void GetVertexNeighbors_AllDefaultValues_ReturnsDefaults()
    {
        using var map = new DualGridMap<int>(3, 3, defaultValue: 0);

        var (bl, br, tl, tr) = map.GetVertexNeighbors(1, 1);

        Assert.That(bl, Is.EqualTo(0));
        Assert.That(br, Is.EqualTo(0));
        Assert.That(tl, Is.EqualTo(0));
        Assert.That(tr, Is.EqualTo(0));
    }

    [Test]
    public void Bound_ReturnsCorrectMapBound()
    {
        using var map = new DualGridMap<int>(3, 3, minX: 5, minY: 10);

        var bound = map.Bound;

        Assert.That(bound.MinX, Is.EqualTo(4));   // 5 - 1
        Assert.That(bound.MinY, Is.EqualTo(9));   // 10 - 1
        Assert.That(bound.MaxX, Is.EqualTo(9));   // 5 + 3 + 1
        Assert.That(bound.MaxY, Is.EqualTo(14));  // 10 + 3 + 1
    }

    [Test]
    public void ExpandBoundFunc_CanBeSet()
    {
        using var map = new DualGridMap<int>(3, 3);

        var customFunc = new ExpandableMap<int>.ExpandFunc(MapBound.Intersection);
        map.ExpandBoundFunc = customFunc;

        // Verify the custom expand function is set
        Assert.That(map.ExpandBoundFunc, Is.Not.Null);
    }

    [Test]
    public void Dispose_ReleasesResources()
    {
        var map = new DualGridMap<int>(3, 3);
        map[0, 0] = 42;

        map.Dispose();

        // No assertion needed - just verify no exception
        Assert.Pass();
    }

    [Test]
    public void Dispose_MultipleTimesDoesNotThrow()
    {
        var map = new DualGridMap<int>(3, 3);

        map.Dispose();
        map.Dispose();

        Assert.Pass();
    }
}
