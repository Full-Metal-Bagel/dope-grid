using System;
using DopeGrid;
using DopeGrid.Native;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public class ShapesFlipTests
{
    [Test]
    public void FlipHorizontal_SingleCell_MovesToOpposite()
    {
        var shape = new GridShape(3, 3, Allocator.Temp);
        shape.SetCell(0, 1, true); // Left middle

        var flipped = shape.Flip(FlipAxis.Horizontal, Allocator.Temp);

        Assert.IsFalse(flipped.GetCell(0, 1));
        Assert.IsTrue(flipped.GetCell(2, 1)); // Right middle

        shape.Dispose();
        flipped.Dispose();
    }

    [Test]
    public void FlipVertical_SingleCell_MovesToOpposite()
    {
        var shape = new GridShape(3, 3, Allocator.Temp);
        shape.SetCell(1, 0, true); // Top middle

        var flipped = shape.Flip(FlipAxis.Vertical, Allocator.Temp);

        Assert.IsFalse(flipped.GetCell(1, 0));
        Assert.IsTrue(flipped.GetCell(1, 2)); // Bottom middle

        shape.Dispose();
        flipped.Dispose();
    }

    [Test]
    public void FlipHorizontal_VerticalLine_StaysInPlace()
    {
        var shape = new GridShape(3, 3, Allocator.Temp);
        // Create vertical line in middle
        shape.SetCell(1, 0, true);
        shape.SetCell(1, 1, true);
        shape.SetCell(1, 2, true);

        var flipped = shape.Flip(FlipAxis.Horizontal, Allocator.Temp);

        // Middle column should stay the same
        Assert.IsTrue(flipped.GetCell(1, 0));
        Assert.IsTrue(flipped.GetCell(1, 1));
        Assert.IsTrue(flipped.GetCell(1, 2));

        shape.Dispose();
        flipped.Dispose();
    }

    [Test]
    public void FlipVertical_HorizontalLine_StaysInPlace()
    {
        var shape = new GridShape(3, 3, Allocator.Temp);
        // Create horizontal line in middle
        shape.SetCell(0, 1, true);
        shape.SetCell(1, 1, true);
        shape.SetCell(2, 1, true);

        var flipped = shape.Flip(FlipAxis.Vertical, Allocator.Temp);

        // Middle row should stay the same
        Assert.IsTrue(flipped.GetCell(0, 1));
        Assert.IsTrue(flipped.GetCell(1, 1));
        Assert.IsTrue(flipped.GetCell(2, 1));

        shape.Dispose();
        flipped.Dispose();
    }

    [Test]
    public void FlipHorizontal_LShape_Mirrors()
    {
        var shape = new GridShape(3, 3, Allocator.Temp);
        // Create L shape
        shape.SetCell(0, 0, true);
        shape.SetCell(0, 1, true);
        shape.SetCell(0, 2, true);
        shape.SetCell(1, 2, true);
        shape.SetCell(2, 2, true);

        var flipped = shape.Flip(FlipAxis.Horizontal, Allocator.Temp);

        // Should create mirrored L
        Assert.IsTrue(flipped.GetCell(2, 0));
        Assert.IsTrue(flipped.GetCell(2, 1));
        Assert.IsTrue(flipped.GetCell(2, 2));
        Assert.IsTrue(flipped.GetCell(1, 2));
        Assert.IsTrue(flipped.GetCell(0, 2));

        // Check empty cells
        Assert.IsFalse(flipped.GetCell(0, 0));
        Assert.IsFalse(flipped.GetCell(0, 1));
        Assert.IsFalse(flipped.GetCell(1, 0));
        Assert.IsFalse(flipped.GetCell(1, 1));

        shape.Dispose();
        flipped.Dispose();
    }

    [Test]
    public void FlipVertical_TShape_InvertsVertically()
    {
        var shape = new GridShape(3, 3, Allocator.Temp);
        // Create T shape
        shape.SetCell(0, 0, true);
        shape.SetCell(1, 0, true);
        shape.SetCell(2, 0, true);
        shape.SetCell(1, 1, true);
        shape.SetCell(1, 2, true);

        var flipped = shape.Flip(FlipAxis.Vertical, Allocator.Temp);

        // Should create inverted T
        Assert.IsTrue(flipped.GetCell(0, 2));
        Assert.IsTrue(flipped.GetCell(1, 2));
        Assert.IsTrue(flipped.GetCell(2, 2));
        Assert.IsTrue(flipped.GetCell(1, 1));
        Assert.IsTrue(flipped.GetCell(1, 0));

        // Check empty cells
        Assert.IsFalse(flipped.GetCell(0, 0));
        Assert.IsFalse(flipped.GetCell(2, 0));
        Assert.IsFalse(flipped.GetCell(0, 1));
        Assert.IsFalse(flipped.GetCell(2, 1));

        shape.Dispose();
        flipped.Dispose();
    }

    [Test]
    public void FlipHorizontal_Diagonal_ReversesDirection()
    {
        var shape = new GridShape(3, 3, Allocator.Temp);
        // Create diagonal from top-left to bottom-right
        shape.SetCell(0, 0, true);
        shape.SetCell(1, 1, true);
        shape.SetCell(2, 2, true);

        var flipped = shape.Flip(FlipAxis.Horizontal, Allocator.Temp);

        // Should create diagonal from top-right to bottom-left
        Assert.IsTrue(flipped.GetCell(2, 0));
        Assert.IsTrue(flipped.GetCell(1, 1));
        Assert.IsTrue(flipped.GetCell(0, 2));

        shape.Dispose();
        flipped.Dispose();
    }

    [Test]
    public void DoubleFlip_SameAxis_RestoresOriginal()
    {
        var shape = new GridShape(4, 4, Allocator.Temp);
        // Create an asymmetric pattern
        shape.SetCell(0, 0, true);
        shape.SetCell(1, 0, true);
        shape.SetCell(0, 1, true);
        shape.SetCell(2, 2, true);
        shape.SetCell(3, 3, true);

        var flippedOnce = shape.Flip(FlipAxis.Horizontal, Allocator.Temp);
        var flippedTwice = flippedOnce.Flip(FlipAxis.Horizontal, Allocator.Temp);

        // Should be identical to original
        for (var y = 0; y < 4; y++)
        for (var x = 0; x < 4; x++)
            Assert.AreEqual(shape.GetCell(x, y), flippedTwice.GetCell(x, y),
                $"Cell ({x},{y}) doesn't match");

        shape.Dispose();
        flippedOnce.Dispose();
        flippedTwice.Dispose();
    }

    [Test]
    public void FlipBits_DirectMethod_WorksCorrectly()
    {
        var shape = new GridShape(3, 3, Allocator.Temp);
        shape.SetCell(0, 0, true);
        shape.SetCell(1, 1, true);

        var output = new SpanBitArray(new byte[2].AsSpan(), 9);
        var result = shape.AsReadOnly().FlipBits(FlipAxis.Horizontal, output);

        Assert.IsTrue(output.Get(2)); // (2, 0)
        Assert.IsTrue(output.Get(4)); // (1, 1)

        shape.Dispose();
    }

    [Test]
    public void Flip_EmptyShape_ReturnsEmpty()
    {
        var shape = new GridShape(3, 3, Allocator.Temp);

        var flippedH = shape.Flip(FlipAxis.Horizontal, Allocator.Temp);
        var flippedV = shape.Flip(FlipAxis.Vertical, Allocator.Temp);

        Assert.AreEqual(0, flippedH.OccupiedSpaceCount);
        Assert.AreEqual(0, flippedV.OccupiedSpaceCount);

        shape.Dispose();
        flippedH.Dispose();
        flippedV.Dispose();
    }

    [Test]
    public void Flip_FullShape_RemainsFull()
    {
        var shape = new GridShape(3, 3, Allocator.Temp);
        for (var y = 0; y < 3; y++)
        for (var x = 0; x < 3; x++)
            shape.SetCell(x, y, true);

        var flippedH = shape.Flip(FlipAxis.Horizontal, Allocator.Temp);
        var flippedV = shape.Flip(FlipAxis.Vertical, Allocator.Temp);

        Assert.AreEqual(9, flippedH.OccupiedSpaceCount);
        Assert.AreEqual(9, flippedV.OccupiedSpaceCount);

        shape.Dispose();
        flippedH.Dispose();
        flippedV.Dispose();
    }

    [Test]
    public void Flip_RectangularShape_MaintainsDimensions()
    {
        var shape = new GridShape(5, 3, Allocator.Temp);
        shape.SetCell(0, 0, true);
        shape.SetCell(4, 2, true);

        var flipped = shape.Flip(FlipAxis.Horizontal, Allocator.Temp);

        Assert.AreEqual(5, flipped.Width);
        Assert.AreEqual(3, flipped.Height);
        Assert.IsTrue(flipped.GetCell(4, 0));
        Assert.IsTrue(flipped.GetCell(0, 2));

        shape.Dispose();
        flipped.Dispose();
    }
}
