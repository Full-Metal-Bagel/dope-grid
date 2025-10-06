using System;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace DopeGrid.Tests;

[StructLayout(LayoutKind.Sequential)]
struct Bytes8
{
    private long _l;
}

[StructLayout(LayoutKind.Sequential)]
struct Bytes32
{
    private long _l0;
    private long _l1;
    private long _l2;
    private long _l3;
}

[TestFixture]
public class FixedGridShapeTests
{
    [Test]
    public void Constructor_WithBytes8_CreatesCorrectShape()
    {
        var shape = new FixedGridShape<Bytes8>(4, 4);

        Assert.That(shape.Width, Is.EqualTo(4));
        Assert.That(shape.Height, Is.EqualTo(4));
        Assert.That(shape.Size, Is.EqualTo(16));
    }

    [Test]
    public void Constructor_WithBytes32_CreatesCorrectShape()
    {
        var shape = new FixedGridShape<Bytes32>(10, 10);

        Assert.That(shape.Width, Is.EqualTo(10));
        Assert.That(shape.Height, Is.EqualTo(10));
        Assert.That(shape.Size, Is.EqualTo(100));
    }

    [Test]
    public void Constructor_BufferTooSmall_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            // Trying to create 20x20 (400 bits = 50 bytes) with Bytes8 (8 bytes)
            var shape = new FixedGridShape<Bytes8>(20, 20);
        });
    }

    [Test]
    public void Indexer_SetAndGet_WorksCorrectly()
    {
        var shape = new FixedGridShape<Bytes8>(4, 4);

        shape[0, 0] = true;
        shape[3, 3] = true;
        shape[1, 2] = true;

        Assert.That(shape[0, 0], Is.True);
        Assert.That(shape[3, 3], Is.True);
        Assert.That(shape[1, 2], Is.True);
        Assert.That(shape[2, 2], Is.False);
    }

    [Test]
    public void IsOccupied_ReturnsCorrectValue()
    {
        var shape = new FixedGridShape<Bytes8>(5, 5);

        shape[2, 3] = true;

        Assert.That(shape.IsOccupied(2, 3), Is.True);
        Assert.That(shape.IsOccupied(0, 0), Is.False);
    }

    [Test]
    public void ReadOnlyBits_ReflectsCurrentState()
    {
        var shape = new FixedGridShape<Bytes8>(4, 4);

        shape[1, 1] = true;
        shape[2, 2] = true;

        var bits = shape.ReadOnlyBits;

        Assert.That(bits.Get(shape.GetIndex(1, 1)), Is.True);
        Assert.That(bits.Get(shape.GetIndex(2, 2)), Is.True);
        Assert.That(bits.Get(shape.GetIndex(0, 0)), Is.False);
    }

    [Test]
    public void Bits_AllowsModification()
    {
        var shape = new FixedGridShape<Bytes8>(4, 4);

        var bits = shape.Bits;
        bits.Set(shape.GetIndex(2, 2), true);

        Assert.That(shape[2, 2], Is.True);
    }

    [Test]
    public void AsReadOnly_CreatesReadOnlyWrapper()
    {
        var shape = new FixedGridShape<Bytes8>(4, 4);
        shape[1, 1] = true;

        var readOnly = shape.AsReadOnly();

        Assert.That(readOnly.Width, Is.EqualTo(4));
        Assert.That(readOnly.Height, Is.EqualTo(4));
        Assert.That(readOnly[1, 1], Is.True);
        Assert.That(readOnly.IsOccupied(1, 1), Is.True);
    }

    [Test]
    public void AsReadOnly_ImplicitConversion_Works()
    {
        var shape = new FixedGridShape<Bytes8>(4, 4);
        shape[2, 2] = true;

        FixedGridShape<Bytes8>.ReadOnly readOnly = shape;

        Assert.That(readOnly[2, 2], Is.True);
    }

    [Test]
    public void ReadOnlyWrapper_Size_ReturnsCorrectValue()
    {
        var shape = new FixedGridShape<Bytes8>(4, 3);
        var readOnly = shape.AsReadOnly();

        Assert.That(readOnly.Size, Is.EqualTo(12));
    }

    [Test]
    public void MultipleShapes_AreIndependent()
    {
        var shape1 = new FixedGridShape<Bytes8>(4, 4);
        var shape2 = new FixedGridShape<Bytes8>(4, 4);

        shape1[0, 0] = true;
        shape2[1, 1] = true;

        Assert.That(shape1[0, 0], Is.True);
        Assert.That(shape1[1, 1], Is.False);
        Assert.That(shape2[0, 0], Is.False);
        Assert.That(shape2[1, 1], Is.True);
    }

    [Test]
    public void LargeShape_WithBytes32_WorksCorrectly()
    {
        var shape = new FixedGridShape<Bytes32>(16, 16);

        // Fill diagonal
        for (int i = 0; i < 16; i++)
        {
            shape[i, i] = true;
        }

        // Verify diagonal
        for (int i = 0; i < 16; i++)
        {
            Assert.That(shape[i, i], Is.True, $"Diagonal [{i},{i}] should be true");
        }

        // Verify non-diagonal
        Assert.That(shape[0, 1], Is.False);
        Assert.That(shape[1, 0], Is.False);
    }

    [Test]
    public void ReadOnlyWrapper_ReadOnlyBits_Works()
    {
        var shape = new FixedGridShape<Bytes8>(4, 4);
        shape[2, 2] = true;

        var readOnly = shape.AsReadOnly();
        var bits = readOnly.ReadOnlyBits;

        Assert.That(bits.Get(shape.GetIndex(2, 2)), Is.True);
        Assert.That(bits.CountBits(), Is.EqualTo(1));
    }

    [Test]
    public void ReadOnlyWrapper_Constructor_CreatesNewShape()
    {
        var readOnly = new FixedGridShape<Bytes8>.ReadOnly(5, 5);

        Assert.That(readOnly.Width, Is.EqualTo(5));
        Assert.That(readOnly.Height, Is.EqualTo(5));
        Assert.That(readOnly.Size, Is.EqualTo(25));
    }

    [Test]
    public void ZeroSizeShape_WorksCorrectly()
    {
        var shape = new FixedGridShape<Bytes8>(0, 0);

        Assert.That(shape.Width, Is.EqualTo(0));
        Assert.That(shape.Height, Is.EqualTo(0));
        Assert.That(shape.Size, Is.EqualTo(0));
    }

    [Test]
    public void RectangularShape_WorksCorrectly()
    {
        var shape = new FixedGridShape<Bytes8>(8, 2);

        // Fill top row
        for (int x = 0; x < 8; x++)
        {
            shape[x, 0] = true;
        }

        // Verify top row filled, bottom row empty
        for (int x = 0; x < 8; x++)
        {
            Assert.That(shape[x, 0], Is.True);
            Assert.That(shape[x, 1], Is.False);
        }
    }
}
