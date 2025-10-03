using System;
using DopeGrid;
using DopeGrid.Native;
using NUnit.Framework;
using Unity.Collections;

public class ShapesFlipTests
{
    [Test]
    public void FlipHorizontal_SingleCell_MovesToOpposite()
    {
        var shape = new GridShape(3, 3, Allocator.Temp);
        shape.SetCellValue(0, 1, true); // Left middle

        var flipped = shape.Flip(FlipAxis.Horizontal, Allocator.Temp);

        Assert.IsFalse(flipped.GetCellValue(0, 1));
        Assert.IsTrue(flipped.GetCellValue(2, 1)); // Right middle

        shape.Dispose();
        flipped.Dispose();
    }

    [Test]
    public void FlipVertical_SingleCell_MovesToOpposite()
    {
        var shape = new GridShape(3, 3, Allocator.Temp);
        shape.SetCellValue(1, 0, true); // Top middle

        var flipped = shape.Flip(FlipAxis.Vertical, Allocator.Temp);

        Assert.IsFalse(flipped.GetCellValue(1, 0));
        Assert.IsTrue(flipped.GetCellValue(1, 2)); // Bottom middle

        shape.Dispose();
        flipped.Dispose();
    }

    [Test]
    public void FlipHorizontal_VerticalLine_StaysInPlace()
    {
        var shape = new GridShape(3, 3, Allocator.Temp);
        // Create vertical line in middle
        shape.SetCellValue(1, 0, true);
        shape.SetCellValue(1, 1, true);
        shape.SetCellValue(1, 2, true);

        var flipped = shape.Flip(FlipAxis.Horizontal, Allocator.Temp);

        // Middle column should stay the same
        Assert.IsTrue(flipped.GetCellValue(1, 0));
        Assert.IsTrue(flipped.GetCellValue(1, 1));
        Assert.IsTrue(flipped.GetCellValue(1, 2));

        shape.Dispose();
        flipped.Dispose();
    }

    [Test]
    public void FlipVertical_HorizontalLine_StaysInPlace()
    {
        var shape = new GridShape(3, 3, Allocator.Temp);
        // Create horizontal line in middle
        shape.SetCellValue(0, 1, true);
        shape.SetCellValue(1, 1, true);
        shape.SetCellValue(2, 1, true);

        var flipped = shape.Flip(FlipAxis.Vertical, Allocator.Temp);

        // Middle row should stay the same
        Assert.IsTrue(flipped.GetCellValue(0, 1));
        Assert.IsTrue(flipped.GetCellValue(1, 1));
        Assert.IsTrue(flipped.GetCellValue(2, 1));

        shape.Dispose();
        flipped.Dispose();
    }

    [Test]
    public void FlipHorizontal_LShape_Mirrors()
    {
        var shape = new GridShape(3, 3, Allocator.Temp);
        // Create L shape
        shape.SetCellValue(0, 0, true);
        shape.SetCellValue(0, 1, true);
        shape.SetCellValue(0, 2, true);
        shape.SetCellValue(1, 2, true);
        shape.SetCellValue(2, 2, true);

        var flipped = shape.Flip(FlipAxis.Horizontal, Allocator.Temp);

        // Should create mirrored L
        Assert.IsTrue(flipped.GetCellValue(2, 0));
        Assert.IsTrue(flipped.GetCellValue(2, 1));
        Assert.IsTrue(flipped.GetCellValue(2, 2));
        Assert.IsTrue(flipped.GetCellValue(1, 2));
        Assert.IsTrue(flipped.GetCellValue(0, 2));

        // Check empty cells
        Assert.IsFalse(flipped.GetCellValue(0, 0));
        Assert.IsFalse(flipped.GetCellValue(0, 1));
        Assert.IsFalse(flipped.GetCellValue(1, 0));
        Assert.IsFalse(flipped.GetCellValue(1, 1));

        shape.Dispose();
        flipped.Dispose();
    }

    [Test]
    public void FlipVertical_TShape_InvertsVertically()
    {
        var shape = new GridShape(3, 3, Allocator.Temp);
        // Create T shape
        shape.SetCellValue(0, 0, true);
        shape.SetCellValue(1, 0, true);
        shape.SetCellValue(2, 0, true);
        shape.SetCellValue(1, 1, true);
        shape.SetCellValue(1, 2, true);

        var flipped = shape.Flip(FlipAxis.Vertical, Allocator.Temp);

        // Should create inverted T
        Assert.IsTrue(flipped.GetCellValue(0, 2));
        Assert.IsTrue(flipped.GetCellValue(1, 2));
        Assert.IsTrue(flipped.GetCellValue(2, 2));
        Assert.IsTrue(flipped.GetCellValue(1, 1));
        Assert.IsTrue(flipped.GetCellValue(1, 0));

        // Check empty cells
        Assert.IsFalse(flipped.GetCellValue(0, 0));
        Assert.IsFalse(flipped.GetCellValue(2, 0));
        Assert.IsFalse(flipped.GetCellValue(0, 1));
        Assert.IsFalse(flipped.GetCellValue(2, 1));

        shape.Dispose();
        flipped.Dispose();
    }

    [Test]
    public void FlipHorizontal_Diagonal_ReversesDirection()
    {
        var shape = new GridShape(3, 3, Allocator.Temp);
        // Create diagonal from top-left to bottom-right
        shape.SetCellValue(0, 0, true);
        shape.SetCellValue(1, 1, true);
        shape.SetCellValue(2, 2, true);

        var flipped = shape.Flip(FlipAxis.Horizontal, Allocator.Temp);

        // Should create diagonal from top-right to bottom-left
        Assert.IsTrue(flipped.GetCellValue(2, 0));
        Assert.IsTrue(flipped.GetCellValue(1, 1));
        Assert.IsTrue(flipped.GetCellValue(0, 2));

        shape.Dispose();
        flipped.Dispose();
    }

    [Test]
    public void DoubleFlip_SameAxis_RestoresOriginal()
    {
        var shape = new GridShape(4, 4, Allocator.Temp);
        // Create an asymmetric pattern
        shape.SetCellValue(0, 0, true);
        shape.SetCellValue(1, 0, true);
        shape.SetCellValue(0, 1, true);
        shape.SetCellValue(2, 2, true);
        shape.SetCellValue(3, 3, true);

        var flippedOnce = shape.Flip(FlipAxis.Horizontal, Allocator.Temp);
        var flippedTwice = flippedOnce.Flip(FlipAxis.Horizontal, Allocator.Temp);

        // Should be identical to original
        for (var y = 0; y < 4; y++)
        for (var x = 0; x < 4; x++)
            Assert.AreEqual(shape.GetCellValue(x, y), flippedTwice.GetCellValue(x, y),
                $"Cell ({x},{y}) doesn't match");

        shape.Dispose();
        flippedOnce.Dispose();
        flippedTwice.Dispose();
    }

    [Test]
    public void FlipBits_DirectMethod_WorksCorrectly()
    {
        var shape = new GridShape(3, 3, Allocator.Temp);
        try
        {
            shape.SetCellValue(0, 0, true);
            shape.SetCellValue(1, 1, true);

            var output = new SpanBitArray(new byte[2].AsSpan(), 9);
            output.SetAll(false); // Ensure the buffer is clean

            shape.AsReadOnly().FlipBits(FlipAxis.Horizontal, output);

            // Flipped cells
            Assert.IsTrue(output.Get(2)); // (2, 0)
            Assert.IsTrue(output.Get(4)); // (1, 1)

            // Ensure other cells are false
            Assert.IsFalse(output.Get(0));
            Assert.IsFalse(output.Get(1));
            Assert.IsFalse(output.Get(3));
            Assert.IsFalse(output.Get(5));
            Assert.IsFalse(output.Get(6));
            Assert.IsFalse(output.Get(7));
            Assert.IsFalse(output.Get(8));
        }
        finally
        {
            shape.Dispose();
        }
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
            shape.SetCellValue(x, y, true);

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
        shape.SetCellValue(0, 0, true);
        shape.SetCellValue(4, 2, true);

        var flipped = shape.Flip(FlipAxis.Horizontal, Allocator.Temp);

        Assert.AreEqual(5, flipped.Width);
        Assert.AreEqual(3, flipped.Height);
        Assert.IsTrue(flipped.GetCellValue(4, 0));
        Assert.IsTrue(flipped.GetCellValue(0, 2));

        shape.Dispose();
        flipped.Dispose();
    }
}
