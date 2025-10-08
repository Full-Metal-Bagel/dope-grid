using NUnit.Framework;
using DopeGrid.Map;

namespace DopeGrid.Tests.Map;

[TestFixture]
public class CompactDualGridMapTests
{
    [Test]
    public void DefaultLayerInitializedWithAllOnes()
    {
        var map = new CompactDualGridMap<int>(width: 2, height: 2, defaultValue: 0);
        var layer = map.GetReadOnlyVisualLayer(0);

        for (int y = 0; y <= map.Height; y++)
        for (int x = 0; x <= map.Width; x++)
        {
            Assert.That(layer[x, y], Is.EqualTo(0x0F), $"vertex ({x},{y})");
        }
    }

    [Test]
    public void SetWorldValue_FlipsBits_OnOldAndNewLayers()
    {
        var map = new CompactDualGridMap<int>(width: 2, height: 2, defaultValue: 0);

        // Act: change a single world cell
        map[0, 0] = 1;

        var oldLayer = map.GetReadOnlyVisualLayer(0);
        var newLayer = map.GetReadOnlyVisualLayer(1);

        // Expect: four vertices flipped with bit masks 1,2,4,8 respectively
        Assert.That(oldLayer[0, 0], Is.EqualTo(0x0F ^ 0x01)); // 0x0E
        Assert.That(oldLayer[1, 0], Is.EqualTo(0x0F ^ 0x02)); // 0x0D
        Assert.That(oldLayer[0, 1], Is.EqualTo(0x0F ^ 0x04)); // 0x0B
        Assert.That(oldLayer[1, 1], Is.EqualTo(0x0F ^ 0x08)); // 0x07

        Assert.That(newLayer[0, 0], Is.EqualTo(0x01));
        Assert.That(newLayer[1, 0], Is.EqualTo(0x02));
        Assert.That(newLayer[0, 1], Is.EqualTo(0x04));
        Assert.That(newLayer[1, 1], Is.EqualTo(0x08));

        // Unaffected vertices remain unchanged
        Assert.That(oldLayer[2, 0], Is.EqualTo(0x0F));
        Assert.That(oldLayer[2, 1], Is.EqualTo(0x0F));
        Assert.That(oldLayer[0, 2], Is.EqualTo(0x0F));
        Assert.That(oldLayer[1, 2], Is.EqualTo(0x0F));
        Assert.That(oldLayer[2, 2], Is.EqualTo(0x0F));

        Assert.That(newLayer[2, 0], Is.EqualTo(0x00));
        Assert.That(newLayer[2, 1], Is.EqualTo(0x00));
        Assert.That(newLayer[0, 2], Is.EqualTo(0x00));
        Assert.That(newLayer[1, 2], Is.EqualTo(0x00));
        Assert.That(newLayer[2, 2], Is.EqualTo(0x00));
    }

    [Test]
    public void SetWorldValue_TogglingBack_RestoresDefaultLayer()
    {
        var map = new CompactDualGridMap<int>(width: 2, height: 2, defaultValue: 0);
        map[0, 0] = 1; // flip once
        map[0, 0] = 0; // flip back

        var defaultLayer = map.GetReadOnlyVisualLayer(0);
        var newLayer = map.GetReadOnlyVisualLayer(1);

        for (int y = 0; y <= map.Height; y++)
        for (int x = 0; x <= map.Width; x++)
        {
            Assert.That(defaultLayer[x, y], Is.EqualTo(0x0F), $"default vertex ({x},{y})");
            Assert.That(newLayer[x, y], Is.EqualTo(0x00), $"new vertex ({x},{y})");
        }
    }
}

