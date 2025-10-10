using NUnit.Framework;

namespace DopeGrid.Map.Tests;

[TestFixture]
public class DualGridMapTests
{
    [Test]
    public void Constructor_CreatesMapWithCorrectDimensions()
    {
        using var map = new DualGridMap<int>(3, 4);

        Assert.That(map.Width, Is.EqualTo(3)); // WorldBound width
        Assert.That(map.Height, Is.EqualTo(4)); // WorldBound height
        Assert.That(map.MinX, Is.EqualTo(0));
        Assert.That(map.MinY, Is.EqualTo(0));
        Assert.That(map.MaxX, Is.EqualTo(3));
        Assert.That(map.MaxY, Is.EqualTo(4));
    }

    [Test]
    public void Constructor_WithNegativeMinXY_CreatesMapWithCorrectBounds()
    {
        using var map = new DualGridMap<int>(3, 4, minX: -5, minY: -3);

        // Properties represent WorldBound
        Assert.That(map.MinX, Is.EqualTo(-5));
        Assert.That(map.MinY, Is.EqualTo(-3));
        Assert.That(map.MaxX, Is.EqualTo(-2)); // -5 + 3
        Assert.That(map.MaxY, Is.EqualTo(1));  // -3 + 4
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

        var bound = map.WorldBound;

        Assert.That(bound.MinX, Is.EqualTo(5));
        Assert.That(bound.MinY, Is.EqualTo(10));
        Assert.That(bound.MaxX, Is.EqualTo(8));
        Assert.That(bound.MaxY, Is.EqualTo(13));
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

    [Test]
    public void Contains_ReturnsTrueForValidIndices()
    {
        using var map = new DualGridMap<int>(3, 3);

        Assert.That(map.Contains(0, 0), Is.True);
        Assert.That(map.Contains(2, 2), Is.True);
        Assert.That(map.Contains(-1, -1), Is.True); // Padding
        Assert.That(map.Contains(3, 3), Is.True);   // Padding
        Assert.That(map.Contains(4, 4), Is.False);  // Out of bounds
        Assert.That(map.Contains(-2, 0), Is.False); // Out of bounds
    }

    [Test]
    public void Contains_WithNegativeBounds_WorksCorrectly()
    {
        using var map = new DualGridMap<int>(3, 3, minX: -5, minY: -5);

        Assert.That(map.Contains(-6, -6), Is.True);  // Padding at min corner
        Assert.That(map.Contains(-5, -5), Is.True);  // Start of actual grid
        Assert.That(map.Contains(-3, -3), Is.True);  // End of actual grid
        Assert.That(map.Contains(-2, -2), Is.True);  // Padding at max corner
        Assert.That(map.Contains(-1, -1), Is.False); // Out of bounds
        Assert.That(map.Contains(-7, -5), Is.False); // Out of bounds
    }

    // TODO: Re-enable when DualGridMap implements GetEnumerator
    // [Test]
    // public void Enumerator_EnumeratesAllCells()
    // {
    //     using var map = new DualGridMap<int>(2, 2, defaultValue: 0);
    //     map[0, 0] = 1;
    //     map[1, 1] = 5;
    //
    //     var count = 0;
    //     foreach (var (value, x, y) in map)
    //     {
    //         Assert.That(value, Is.EqualTo(map[x, y]));
    //         count++;
    //     }
    //
    //     // 2x2 grid + 1 padding on each side = 4x4 = 16 cells
    //     Assert.That(count, Is.EqualTo(16));
    // }
    //
    // [Test]
    // public void Enumerator_WithPadding_IncludesPaddedCells()
    // {
    //     using var map = new DualGridMap<int>(2, 2, defaultValue: 0);
    //     map[-1, -1] = 99; // Padding cell
    //
    //     var paddingCellFound = false;
    //     foreach (var (value, x, y) in map)
    //     {
    //         if (x == -1 && y == -1 && value == 99)
    //         {
    //             paddingCellFound = true;
    //         }
    //     }
    //
    //     Assert.That(paddingCellFound, Is.True);
    // }
    //
    // [Test]
    // public void Enumerator_OrderIsRowMajor()
    // {
    //     using var map = new DualGridMap<int>(2, 2, defaultValue: 0);
    //     map[-1, -1] = 1;
    //     map[0, -1] = 2;
    //     map[1, -1] = 3;
    //     map[2, -1] = 4;
    //
    //     var firstRowValues = new System.Collections.Generic.List<int>();
    //     var y = -1;
    //     foreach (var (value, x, currentY) in map)
    //     {
    //         if (currentY == y)
    //         {
    //             firstRowValues.Add(value);
    //         }
    //         if (currentY > y) break;
    //     }
    //
    //     // Row-major: first row should be (-1,-1), (0,-1), (1,-1), (2,-1)
    //     Assert.That(firstRowValues, Is.EqualTo(new[] { 1, 2, 3, 4 }));
    // }

    [Test]
    public void Expand_ExpandsMapBounds()
    {
        using var map = new DualGridMap<int>(3, 3, defaultValue: 0);
        map[0, 0] = 42;

        // Initial WorldBound: (0, 0, 3, 3), VertexBound: (0, 0, 4, 4)
        var newBound = new MapBound(MinX: 0, MinY: 0, MaxX: 10, MaxY: 10);
        map.Expand(newBound);

        // After expansion with Union, VertexBound should union current (0,0,4,4) with new expanded (0,0,11,11)
        Assert.That(map.VertexBound.MinX, Is.EqualTo(0));
        Assert.That(map.VertexBound.MinY, Is.EqualTo(0));
        Assert.That(map.VertexBound.MaxX, Is.EqualTo(11));
        Assert.That(map.VertexBound.MaxY, Is.EqualTo(11));
        Assert.That(map[0, 0], Is.EqualTo(42)); // Previous value preserved
    }

    [Test]
    public void Expand_PreservesExistingData()
    {
        using var map = new DualGridMap<int>(3, 3, defaultValue: 0);
        map[0, 0] = 1;
        map[1, 1] = 2;
        map[2, 2] = 3;

        var newBound = new MapBound(MinX: -5, MinY: -5, MaxX: 10, MaxY: 10);
        map.Expand(newBound);

        Assert.That(map[0, 0], Is.EqualTo(1));
        Assert.That(map[1, 1], Is.EqualTo(2));
        Assert.That(map[2, 2], Is.EqualTo(3));
    }

    [Test]
    public void Expand_AddsPaddingToNewBound()
    {
        using var map = new DualGridMap<int>(2, 2, defaultValue: 0);

        // Initial WorldBound: (0, 0, 2, 2), VertexBound: (0, 0, 3, 3)
        var worldBound = new MapBound(MinX: 5, MinY: 5, MaxX: 15, MaxY: 15);
        map.Expand(worldBound);

        // With Union (default), expands to include both old and new bounds
        // Internal bound after GetExpandedBound: (4, 4, 16, 16)
        // Union with current (0, 0, 3, 3) = (0, 0, 16, 16)
        // VertexBound = (0, 0, 17, 17)
        Assert.That(map.VertexBound.MinX, Is.EqualTo(0));
        Assert.That(map.VertexBound.MinY, Is.EqualTo(0));
        Assert.That(map.VertexBound.MaxX, Is.EqualTo(16));
        Assert.That(map.VertexBound.MaxY, Is.EqualTo(16));
    }

    [Test]
    public void Expand_WithNegativeBounds_WorksCorrectly()
    {
        using var map = new DualGridMap<int>(3, 3, defaultValue: 0);
        map[0, 0] = 99;

        // Initial WorldBound: (0, 0, 3, 3), VertexBound: (0, 0, 4, 4)
        var newBound = new MapBound(MinX: -10, MinY: -10, MaxX: 10, MaxY: 10);
        map.Expand(newBound);

        // GetExpandedBound: (-11, -11, 11, 11)
        // _map bounds: (-11, -11, 11, 11)
        // WorldBound (properties): MinX = _map.MinX + 1 = -10
        // VertexBound: (MinX, MinY, MaxX+1, MaxY+1) = (-10, -10, 11, 11)
        Assert.That(map.VertexBound.MinX, Is.EqualTo(-10));
        Assert.That(map.VertexBound.MinY, Is.EqualTo(-10));
        Assert.That(map.VertexBound.MaxX, Is.EqualTo(11));
        Assert.That(map.VertexBound.MaxY, Is.EqualTo(11));
        Assert.That(map[0, 0], Is.EqualTo(99));
    }

    [Test]
    public void Expand_OnlyInOneDirection_ExpandsCorrectly()
    {
        using var map = new DualGridMap<int>(3, 3, defaultValue: 0);

        // Only expand to the right
        var originalBound = map.VertexBound;
        var newBound = new MapBound(
            MinX: originalBound.MinX + 1,
            MinY: originalBound.MinY + 1,
            MaxX: originalBound.MaxX + 10,
            MaxY: originalBound.MaxY + 1
        );
        map.Expand(newBound);

        // Due to Union being the default ExpandFunc, it should expand
        Assert.That(map.VertexBound.MaxX, Is.GreaterThan(originalBound.MaxX));
    }

    [Test]
    public void Expand_MultipleTimesAccumulatesExpansion()
    {
        using var map = new DualGridMap<int>(2, 2, defaultValue: 0);
        map[0, 0] = 10;

        map.Expand(new MapBound(MinX: -5, MinY: -5, MaxX: 5, MaxY: 5));
        var bound1 = map.VertexBound;

        map.Expand(new MapBound(MinX: -10, MinY: -10, MaxX: 10, MaxY: 10));
        var bound2 = map.VertexBound;

        Assert.That(bound2.MinX, Is.LessThanOrEqualTo(bound1.MinX));
        Assert.That(bound2.MinY, Is.LessThanOrEqualTo(bound1.MinY));
        Assert.That(bound2.MaxX, Is.GreaterThanOrEqualTo(bound1.MaxX));
        Assert.That(bound2.MaxY, Is.GreaterThanOrEqualTo(bound1.MaxY));
        Assert.That(map[0, 0], Is.EqualTo(10)); // Data still preserved
    }

    [Test]
    public void Expand_WithCustomExpandFunc_UsesCustomLogic()
    {
        using var map = new DualGridMap<int>(3, 3, defaultValue: 0);
        map.ExpandBoundFunc = MapBound.Intersection;

        var originalBound = map.VertexBound;
        var newBound = new MapBound(MinX: -10, MinY: -10, MaxX: 10, MaxY: 10);
        map.Expand(newBound);

        // With Intersection as ExpandFunc, the logic may differ
        // The actual behavior depends on ExpandableMap implementation
        Assert.That(map.VertexBound, Is.Not.EqualTo(originalBound));
    }

    [Test]
    public void Expand_SmallerBound_BehaviorDependsOnExpandFunc()
    {
        using var map = new DualGridMap<int>(10, 10, defaultValue: 0);

        var originalBound = map.VertexBound;
        var smallerBound = new MapBound(MinX: 2, MinY: 2, MaxX: 5, MaxY: 5);
        map.Expand(smallerBound);

        // With Union (default), bound should not shrink
        Assert.That(map.VertexBound.Width, Is.GreaterThanOrEqualTo(originalBound.Width));
        Assert.That(map.VertexBound.Height, Is.GreaterThanOrEqualTo(originalBound.Height));
    }

    [Test]
    public void VertexBound_ReturnsInternalMapBound()
    {
        using var map = new DualGridMap<int>(3, 3, minX: 5, minY: 10);

        var vertexBound = map.VertexBound;

        Assert.That(vertexBound.MinX, Is.EqualTo(5));   // WorldBound.MinX
        Assert.That(vertexBound.MinY, Is.EqualTo(10));  // WorldBound.MinY
        Assert.That(vertexBound.MaxX, Is.EqualTo(9));   // WorldBound.MaxX + 1 = 8 + 1
        Assert.That(vertexBound.MaxY, Is.EqualTo(14));  // WorldBound.MaxY + 1 = 13 + 1
    }

    [Test]
    public void WorldBound_ReturnsUnpaddedBound()
    {
        using var map = new DualGridMap<int>(3, 3, minX: 5, minY: 10);

        var worldBound = map.WorldBound;

        Assert.That(worldBound.MinX, Is.EqualTo(5));    // VertexBound.MinX + 1
        Assert.That(worldBound.MinY, Is.EqualTo(10));   // VertexBound.MinY + 1
        Assert.That(worldBound.MaxX, Is.EqualTo(8));    // VertexBound.MaxX - 1
        Assert.That(worldBound.MaxY, Is.EqualTo(13));   // VertexBound.MaxY - 1
    }
}
