using NUnit.Framework;

namespace DopeGrid.Map.Tests;

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

    [Test]
    public void Enumerator_EnumeratesAllCells()
    {
        using var map = new DualGridMap<int>(2, 2, defaultValue: 0);
        map[0, 0] = 1;
        map[1, 1] = 5;

        var count = 0;
        foreach (var (value, x, y) in map)
        {
            Assert.That(value, Is.EqualTo(map[x, y]));
            count++;
        }

        // 2x2 grid + 1 padding on each side = 4x4 = 16 cells
        Assert.That(count, Is.EqualTo(16));
    }

    [Test]
    public void Enumerator_WithPadding_IncludesPaddedCells()
    {
        using var map = new DualGridMap<int>(2, 2, defaultValue: 0);
        map[-1, -1] = 99; // Padding cell

        var paddingCellFound = false;
        foreach (var (value, x, y) in map)
        {
            if (x == -1 && y == -1 && value == 99)
            {
                paddingCellFound = true;
            }
        }

        Assert.That(paddingCellFound, Is.True);
    }

    [Test]
    public void Enumerator_OrderIsRowMajor()
    {
        using var map = new DualGridMap<int>(2, 2, defaultValue: 0);
        map[-1, -1] = 1;
        map[0, -1] = 2;
        map[1, -1] = 3;
        map[2, -1] = 4;

        var firstRowValues = new System.Collections.Generic.List<int>();
        var y = -1;
        foreach (var (value, x, currentY) in map)
        {
            if (currentY == y)
            {
                firstRowValues.Add(value);
            }
            if (currentY > y) break;
        }

        // Row-major: first row should be (-1,-1), (0,-1), (1,-1), (2,-1)
        Assert.That(firstRowValues, Is.EqualTo(new[] { 1, 2, 3, 4 }));
    }
}
