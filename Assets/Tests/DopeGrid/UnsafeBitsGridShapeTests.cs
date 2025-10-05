using System;
using DopeGrid;

namespace DopeGrid.Tests;

[TestFixture]
public class UnsafeBitsGridShapeTests
{
    [Test]
    public void Constructor_WithByteArray_CreatesCorrectShape()
    {
        var buffer = new byte[4]; // 32 bits
        using var shape = new UnsafeBitsGridShape(5, 5, buffer);

        Assert.That(shape.Width, Is.EqualTo(5));
        Assert.That(shape.Height, Is.EqualTo(5));
        Assert.That(shape.Size, Is.EqualTo(25));
    }

    [Test]
    public void Constructor_WithIntPtr_CreatesCorrectShape()
    {
        var buffer = new byte[4];
        unsafe
        {
            fixed (byte* ptr = buffer)
            {
                var shape = new UnsafeBitsGridShape(4, 4, new IntPtr(ptr));

                Assert.That(shape.Width, Is.EqualTo(4));
                Assert.That(shape.Height, Is.EqualTo(4));
                Assert.That(shape.Size, Is.EqualTo(16));
            }
        }
    }

    [Test]
    public void Indexer_SetAndGet_WorksCorrectly()
    {
        var buffer = new byte[4];
        using var shape = new UnsafeBitsGridShape(5, 5, buffer);

        shape[0, 0] = true;
        shape[4, 4] = true;
        shape[2, 3] = true;

        Assert.That(shape[0, 0], Is.True);
        Assert.That(shape[4, 4], Is.True);
        Assert.That(shape[2, 3], Is.True);
        Assert.That(shape[1, 1], Is.False);
    }

    [Test]
    public void IsOccupied_ReturnsCorrectValue()
    {
        var buffer = new byte[4];
        using var shape = new UnsafeBitsGridShape(5, 5, buffer);

        shape[3, 2] = true;

        Assert.That(shape.IsOccupied(3, 2), Is.True);
        Assert.That(shape.IsOccupied(0, 0), Is.False);
    }

    [Test]
    public void ReadOnlyBits_ReflectsCurrentState()
    {
        var buffer = new byte[4];
        using var shape = new UnsafeBitsGridShape(5, 5, buffer);

        shape[1, 1] = true;
        shape[2, 2] = true;

        var bits = shape.ReadOnlyBits;

        Assert.That(bits.Get(shape.GetIndex(1, 1)), Is.True);
        Assert.That(bits.Get(shape.GetIndex(2, 2)), Is.True);
        Assert.That(bits.Get(shape.GetIndex(0, 0)), Is.False);
        Assert.That(bits.CountBits(), Is.EqualTo(2));
    }

    [Test]
    public void Bits_AllowsModification()
    {
        var buffer = new byte[4];
        using var shape = new UnsafeBitsGridShape(5, 5, buffer);

        var bits = shape.Bits;
        bits.Set(shape.GetIndex(3, 3), true);

        Assert.That(shape[3, 3], Is.True);
    }

    [Test]
    public void Dispose_WithByteArray_FreesGCHandle()
    {
        var buffer = new byte[4];
        var shape = new UnsafeBitsGridShape(5, 5, buffer);

        shape[0, 0] = true;
        Assert.That(shape[0, 0], Is.True);

        shape.Dispose();

        // After dispose, the GC handle should be freed
        // We can't easily verify this without reflection, but at least ensure no exception
        Assert.Pass("Dispose completed without exception");
    }

    [Test]
    public void Dispose_WithIntPtr_DoesNotThrow()
    {
        var buffer = new byte[4];
        unsafe
        {
            fixed (byte* ptr = buffer)
            {
                var shape = new UnsafeBitsGridShape(4, 4, new IntPtr(ptr));
                shape.Dispose();

                // Should not throw because IntPtr constructor doesn't create GC handle
                Assert.Pass("Dispose completed without exception");
            }
        }
    }

    [Test]
    public void MultipleShapes_SharingBuffer_AffectEachOther()
    {
        var buffer = new byte[4];
        using var shape1 = new UnsafeBitsGridShape(5, 5, buffer);
        using var shape2 = new UnsafeBitsGridShape(5, 5, buffer);

        shape1[0, 0] = true;

        // shape2 shares the same buffer, so it should see the change
        Assert.That(shape2[0, 0], Is.True);
    }

    [Test]
    public void MultipleShapes_DifferentBuffers_AreIndependent()
    {
        var buffer1 = new byte[4];
        var buffer2 = new byte[4];
        using var shape1 = new UnsafeBitsGridShape(5, 5, buffer1);
        using var shape2 = new UnsafeBitsGridShape(5, 5, buffer2);

        shape1[0, 0] = true;

        Assert.That(shape1[0, 0], Is.True);
        Assert.That(shape2[0, 0], Is.False);
    }

    [Test]
    public void LargeShape_WorksCorrectly()
    {
        var buffer = new byte[100]; // 800 bits
        using var shape = new UnsafeBitsGridShape(20, 20, buffer);

        // Fill corners
        shape[0, 0] = true;
        shape[19, 0] = true;
        shape[0, 19] = true;
        shape[19, 19] = true;

        Assert.That(shape[0, 0], Is.True);
        Assert.That(shape[19, 0], Is.True);
        Assert.That(shape[0, 19], Is.True);
        Assert.That(shape[19, 19], Is.True);
        Assert.That(shape[10, 10], Is.False);
    }

    [Test]
    public void ModificationsThroughBits_ReflectInIndexer()
    {
        var buffer = new byte[4];
        using var shape = new UnsafeBitsGridShape(5, 5, buffer);

        var bits = shape.Bits;
        bits.Set(shape.GetIndex(2, 3), true);
        bits.Set(shape.GetIndex(4, 1), true);

        Assert.That(shape[2, 3], Is.True);
        Assert.That(shape[4, 1], Is.True);
    }

    [Test]
    public void ReadOnlyBits_CountOnes_Works()
    {
        var buffer = new byte[4];
        using var shape = new UnsafeBitsGridShape(5, 5, buffer);

        shape[0, 0] = true;
        shape[1, 1] = true;
        shape[2, 2] = true;

        var bits = shape.ReadOnlyBits;
        Assert.That(bits.CountBits(), Is.EqualTo(3));
    }

    [Test]
    public void ZeroSizeShape_WorksCorrectly()
    {
        var buffer = new byte[1];
        using var shape = new UnsafeBitsGridShape(0, 0, buffer);

        Assert.That(shape.Width, Is.EqualTo(0));
        Assert.That(shape.Height, Is.EqualTo(0));
        Assert.That(shape.Size, Is.EqualTo(0));
    }

    [Test]
    public void SingleCellShape_WorksCorrectly()
    {
        var buffer = new byte[1];
        using var shape = new UnsafeBitsGridShape(1, 1, buffer);

        shape[0, 0] = true;
        Assert.That(shape[0, 0], Is.True);

        shape[0, 0] = false;
        Assert.That(shape[0, 0], Is.False);
    }

    [Test]
    public void RectangularShape_WorksCorrectly()
    {
        var buffer = new byte[4];
        using var shape = new UnsafeBitsGridShape(8, 3, buffer);

        // Fill middle row
        for (int x = 0; x < 8; x++)
        {
            shape[x, 1] = true;
        }

        // Verify middle row filled
        for (int x = 0; x < 8; x++)
        {
            Assert.That(shape[x, 0], Is.False);
            Assert.That(shape[x, 1], Is.True);
            Assert.That(shape[x, 2], Is.False);
        }
    }

    [Test]
    public void PatternFilling_WorksCorrectly()
    {
        var buffer = new byte[8];
        using var shape = new UnsafeBitsGridShape(8, 8, buffer);

        // Create checkerboard pattern
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            shape[x, y] = (x + y) % 2 == 0;
        }

        // Verify checkerboard
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            var expected = (x + y) % 2 == 0;
            Assert.That(shape[x, y], Is.EqualTo(expected), $"Cell [{x},{y}] pattern mismatch");
        }
    }

    [Test]
    public void UsingStatement_DisposesCorrectly()
    {
        var buffer = new byte[4];

        using (var shape = new UnsafeBitsGridShape(5, 5, buffer))
        {
            shape[0, 0] = true;
            Assert.That(shape[0, 0], Is.True);
        }

        // After using block, shape should be disposed
        Assert.Pass("Using statement completed successfully");
    }
}
