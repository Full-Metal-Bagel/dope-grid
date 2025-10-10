using NUnit.Framework;

namespace DopeGrid.Map.Tests;

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
        Assert.That(map.Bound.MinX, Is.EqualTo(-2));
        Assert.That(map.Bound.MinY, Is.EqualTo(-2));
        Assert.That(map.Bound.Width, Is.EqualTo(12));
        Assert.That(map.Bound.Height, Is.EqualTo(12));
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

    [Test]
    public void Enumerator_EnumeratesAllCells()
    {
        using var map = new ExpandableMap<int>(3, 2, defaultValue: 0);
        map[0, 0] = 1;
        map[1, 0] = 2;
        map[2, 0] = 3;
        map[0, 1] = 4;
        map[1, 1] = 5;
        map[2, 1] = 6;

        var count = 0;
        foreach (var (value, x, y) in map)
        {
            Assert.That(value, Is.EqualTo(map[x, y]));
            count++;
        }

        Assert.That(count, Is.EqualTo(6)); // 3x2 = 6 cells
    }

    [Test]
    public void Enumerator_WithNegativeBounds_UsesWorldCoordinates()
    {
        var bound = new MapBound(MinX: -1, MinY: -1, MaxX: 2, MaxY: 2);
        using var map = new ExpandableMap<int>(bound, defaultValue: 0);
        map[-1, -1] = 10;
        map[0, 0] = 20;
        map[1, 1] = 30;

        var values = new System.Collections.Generic.List<(int value, int x, int y)>();
        foreach (var item in map)
        {
            values.Add(item);
        }

        Assert.That(values.Count, Is.EqualTo(9)); // 3x3 = 9 cells
        Assert.That(values[0], Is.EqualTo((10, -1, -1))); // First cell
        Assert.That(values.Exists(v => v == (20, 0, 0)), Is.True);
        Assert.That(values.Exists(v => v == (30, 1, 1)), Is.True);
    }

    [Test]
    public void Enumerator_EmptyMap_EnumeratesDefaultValues()
    {
        using var map = new ExpandableMap<int>(2, 2, defaultValue: 99);

        var count = 0;
        foreach (var (value, x, y) in map)
        {
            Assert.That(value, Is.EqualTo(99));
            count++;
        }

        Assert.That(count, Is.EqualTo(4)); // 2x2 = 4 cells
    }

    [Test]
    public void Enumerator_OrderIsRowMajor()
    {
        using var map = new ExpandableMap<int>(2, 2, defaultValue: 0);
        map[0, 0] = 1;
        map[1, 0] = 2;
        map[0, 1] = 3;
        map[1, 1] = 4;

        var values = new System.Collections.Generic.List<int>();
        foreach (var (value, _, _) in map)
        {
            values.Add(value);
        }

        // Row-major: (0,0), (1,0), (0,1), (1,1)
        Assert.That(values, Is.EqualTo(new[] { 1, 2, 3, 4 }));
    }

    [Test]
    public void Enumerator_ZeroWidth_ShouldNotIterate()
    {
        // Bug: The current implementation will incorrectly iterate when width is 0
        // For a 0xN map, it should enumerate 0 items, but due to the bug in MoveNext:
        // - Initial: _x = -1, _y = 0
        // - MoveNext: _x++  → _x = 0
        //   - if (_x >= Width) where Width = 0 → true
        //   - _x = 0, _y++ → _y = 1
        //   - return _y < Height → 1 < 5 = true (incorrectly yields item)
        // This causes it to iterate Height-1 times instead of 0 times
        using var map = new ExpandableMap<int>(0, 5, defaultValue: 42);

        var count = 0;
        foreach (var (value, x, y) in map)
        {
            count++;
        }

        // Correct assertion: A map with zero width should not iterate at all
        // This test WILL FAIL with current implementation (actual count = 4)
        Assert.That(count, Is.EqualTo(0), "Map with zero width should not iterate");
    }

    [Test]
    public void Enumerator_ZeroHeight_ShouldNotIterate()
    {
        using var map = new ExpandableMap<int>(5, 0, defaultValue: 42);

        var count = 0;
        foreach (var (value, x, y) in map)
        {
            count++;
        }

        // This correctly enumerates 0 items (passes)
        Assert.That(count, Is.EqualTo(0), "Map with zero height should not iterate");
    }

    [Test]
    public void Enumerator_ZeroWidthAndHeight_ShouldNotIterate()
    {
        using var map = new ExpandableMap<int>(0, 0, defaultValue: 42);

        var count = 0;
        foreach (var (value, x, y) in map)
        {
            count++;
        }

        // This correctly enumerates 0 items (passes)
        Assert.That(count, Is.EqualTo(0), "Empty map should not iterate");
    }
}
