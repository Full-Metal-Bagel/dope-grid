using DopeGrid.Map;
using NUnit.Framework;

namespace DopeGrid.Tests;

[TestFixture]
public class DualGridMapTests
{
    private static byte ExpectedMaskForVertex(int vx, int vy, int width, int height, int[,] world, int defaultValue, int target)
    {
        // Bit layout around a vertex (vx, vy):
        // bit 0: (vx-1, vy-1) top-left
        // bit 1: (vx,   vy-1) top-right
        // bit 2: (vx-1, vy  ) bottom-left
        // bit 3: (vx,   vy  ) bottom-right
        byte mask = 0;

        bool IsEqual(int cx, int cy)
        {
            if ((uint)cx >= (uint)width || (uint)cy >= (uint)height)
                return defaultValue == target;
            return world[cx, cy] == target;
        }

        if (IsEqual(vx - 1, vy - 1)) mask |= 1 << 0;
        if (IsEqual(vx,     vy - 1)) mask |= 1 << 1;
        if (IsEqual(vx - 1, vy    )) mask |= 1 << 2;
        if (IsEqual(vx,     vy    )) mask |= 1 << 3;

        return mask;
    }

    [Test]
    public void Constructor_InitializesDefaultLayerToAllOnesNibble()
    {
        const int w = 3, h = 2, def = 0;
        var grid = new DualGridMap<int>(w, h, def);

        var defaultLayer = grid.GetLayerGrid(def);
        Assert.That(defaultLayer.Width, Is.EqualTo(w + 1));
        Assert.That(defaultLayer.Height, Is.EqualTo(h + 1));

        for (int y = 0; y <= h; y++)
        for (int x = 0; x <= w; x++)
        {
            Assert.That(defaultLayer[x, y], Is.EqualTo(0x0F), $"Vertex ({x},{y}) should be 0x0F");
        }
    }

    [Test]
    public void NonDefaultLayer_Initially_AllZeros()
    {
        const int w = 4, h = 3, def = 0, other = 7;
        var grid = new DualGridMap<int>(w, h, def);

        var otherLayer = grid.GetLayerGrid(other);
        for (int y = 0; y <= h; y++)
        for (int x = 0; x <= w; x++)
        {
            Assert.That(otherLayer[x, y], Is.EqualTo(0));
        }
    }

    [Test]
    public void SetWorldValue_InteriorCell_UpdatesFourAdjacentVertices()
    {
        const int w = 5, h = 5, def = 0, val = 2;
        var grid = new DualGridMap<int>(w, h, def);
        int[,] world = new int[w, h];

        int cx = 2, cy = 2;
        grid.SetWorldValue(cx, cy, val);
        world[cx, cy] = val;

        var defLayer = grid.GetLayerGrid(def);
        var valLayer = grid.GetLayerGrid(val);

        for (int vy = cy; vy <= cy + 1; vy++)
        for (int vx = cx; vx <= cx + 1; vx++)
        {
            var expectedDef = ExpectedMaskForVertex(vx, vy, w, h, world, def, def);
            var expectedVal = ExpectedMaskForVertex(vx, vy, w, h, world, def, val);
            Assert.That(defLayer[vx, vy], Is.EqualTo(expectedDef), $"Default layer mismatch at vertex ({vx},{vy})");
            Assert.That(valLayer[vx, vy], Is.EqualTo(expectedVal), $"Value layer mismatch at vertex ({vx},{vy})");
        }

        // Unaffected vertex remains at 0x0F for default, 0 for value
        Assert.That(defLayer[0, 0], Is.EqualTo(0x0F));
        Assert.That(valLayer[0, 0], Is.EqualTo(0));
    }

    [Test]
    public void SetWorldValue_CornerCell_HandlesOutOfBoundsAsDefault()
    {
        const int w = 3, h = 3, def = 0, val = 1;
        var grid = new DualGridMap<int>(w, h, def);
        int[,] world = new int[w, h];

        int cx = 0, cy = 0;
        grid.SetWorldValue(cx, cy, val);
        world[cx, cy] = val;

        var defLayer = grid.GetLayerGrid(def);
        var valLayer = grid.GetLayerGrid(val);

        for (int vy = cy; vy <= cy + 1; vy++)
        for (int vx = cx; vx <= cx + 1; vx++)
        {
            var expectedDef = ExpectedMaskForVertex(vx, vy, w, h, world, def, def);
            var expectedVal = ExpectedMaskForVertex(vx, vy, w, h, world, def, val);
            Assert.That(defLayer[vx, vy], Is.EqualTo(expectedDef), $"Default layer mismatch at vertex ({vx},{vy})");
            Assert.That(valLayer[vx, vy], Is.EqualTo(expectedVal), $"Value layer mismatch at vertex ({vx},{vy})");
        }
    }

    [Test]
    public void SetWorldValue_ToggleBackToDefault_RevertsVertices()
    {
        const int w = 4, h = 4, def = 0, val = 3;
        var grid = new DualGridMap<int>(w, h, def);
        int[,] world = new int[w, h];

        grid.SetWorldValue(1, 1, val);
        world[1, 1] = val;
        grid.SetWorldValue(1, 1, def);
        world[1, 1] = def;

        var defLayer = grid.GetLayerGrid(def);
        var valLayer = grid.GetLayerGrid(val);

        for (int y = 0; y <= h; y++)
        for (int x = 0; x <= w; x++)
        {
            Assert.That(defLayer[x, y], Is.EqualTo(0x0F), $"Default layer should revert to 0x0F at ({x},{y})");
            Assert.That(valLayer[x, y], Is.EqualTo(0), $"Value layer should revert to 0 at ({x},{y})");
        }
    }

    [Test]
    public void MultipleAdjacentCells_ComposesNibbleBits()
    {
        const int w = 4, h = 3, def = 0, val = 9;
        var grid = new DualGridMap<int>(w, h, def);
        int[,] world = new int[w, h];

        grid.SetWorldValue(1, 1, val); world[1, 1] = val;
        grid.SetWorldValue(2, 1, val); world[2, 1] = val;

        var defLayer = grid.GetLayerGrid(def);
        var valLayer = grid.GetLayerGrid(val);

        // Check around shared vertex (2,1)
        int vx = 2, vy = 1;
        var expectedDef = ExpectedMaskForVertex(vx, vy, w, h, world, def, def);
        var expectedVal = ExpectedMaskForVertex(vx, vy, w, h, world, def, val);
        Assert.That(defLayer[vx, vy], Is.EqualTo(expectedDef));
        Assert.That(valLayer[vx, vy], Is.EqualTo(expectedVal));
    }

    [Test]
    public void SwitchingBetweenNonDefaultValues_MovesBitsBetweenLayers()
    {
        const int w = 3, h = 3, def = 0, a = 2, b = 5;
        var grid = new DualGridMap<int>(w, h, def);
        int[,] world = new int[w, h];

        grid.SetWorldValue(1, 1, a); world[1, 1] = a;
        grid.SetWorldValue(1, 1, b); world[1, 1] = b;

        var aLayer = grid.GetLayerGrid(a);
        var bLayer = grid.GetLayerGrid(b);

        for (int vy = 1; vy <= 2; vy++)
        for (int vx = 1; vx <= 2; vx++)
        {
            // a's bits should be cleared; b's bits should be set
            var expectedA = ExpectedMaskForVertex(vx, vy, w, h, world, def, a);
            var expectedB = ExpectedMaskForVertex(vx, vy, w, h, world, def, b);
            Assert.That(aLayer[vx, vy], Is.EqualTo(expectedA));
            Assert.That(bLayer[vx, vy], Is.EqualTo(expectedB));
        }
    }

    [Test]
    public void ZeroSizeWorld_HasSingleVertex()
    {
        const int w = 0, h = 0, def = 0;
        var grid = new DualGridMap<int>(w, h, def);
        var defLayer = grid.GetLayerGrid(def);

        Assert.That(defLayer.Width, Is.EqualTo(1));
        Assert.That(defLayer.Height, Is.EqualTo(1));
        Assert.That(defLayer[0, 0], Is.EqualTo(0x0F));
    }

    [Test]
    public void GetWorldValue_ReturnsLatest()
    {
        var grid = new DualGridMap<int>(2, 2, 0);
        Assert.That(grid.GetWorldValue(1, 1), Is.EqualTo(0));
        grid.SetWorldValue(1, 1, 7);
        Assert.That(grid.GetWorldValue(1, 1), Is.EqualTo(7));
    }
}

