using System;
using DopeGrid;
using NUnit.Framework;

namespace DopeGrid.Tests;

[TestFixture]
public class ShapesTests
{
    // Factory method tests
    [Test]
    public void Single_CreatesOneByOneShape()
    {
        var shape = Shapes.Single();

        Assert.That(shape.Width, Is.EqualTo(1));
        Assert.That(shape.Height, Is.EqualTo(1));
        Assert.That(shape[0, 0], Is.True);
        Assert.That(shape.OccupiedSpaceCount(), Is.EqualTo(1));
    }

    [Test]
    public void Line_CreatesHorizontalLine()
    {
        var shape = Shapes.Line(5);

        Assert.That(shape.Width, Is.EqualTo(5));
        Assert.That(shape.Height, Is.EqualTo(1));
        for (int i = 0; i < 5; i++)
        {
            Assert.That(shape[i, 0], Is.True);
        }
        Assert.That(shape.OccupiedSpaceCount(), Is.EqualTo(5));
    }

    [Test]
    public void Square_CreatesSquareShape()
    {
        var shape = Shapes.Square(3);

        Assert.That(shape.Width, Is.EqualTo(3));
        Assert.That(shape.Height, Is.EqualTo(3));
        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 3; x++)
        {
            Assert.That(shape[x, y], Is.True);
        }
        Assert.That(shape.OccupiedSpaceCount(), Is.EqualTo(9));
    }

    [Test]
    public void LShape_CreatesLShape()
    {
        var shape = Shapes.LShape();

        Assert.That(shape.Width, Is.EqualTo(2));
        Assert.That(shape.Height, Is.EqualTo(2));
        Assert.That(shape[0, 0], Is.True);
        Assert.That(shape[0, 1], Is.True);
        Assert.That(shape[1, 1], Is.True);
        Assert.That(shape[1, 0], Is.False);
        Assert.That(shape.OccupiedSpaceCount(), Is.EqualTo(3));
    }

    [Test]
    public void TShape_CreatesTShape()
    {
        var shape = Shapes.TShape();

        Assert.That(shape.Width, Is.EqualTo(3));
        Assert.That(shape.Height, Is.EqualTo(2));
        Assert.That(shape[0, 0], Is.True);
        Assert.That(shape[1, 0], Is.True);
        Assert.That(shape[2, 0], Is.True);
        Assert.That(shape[1, 1], Is.True);
        Assert.That(shape[0, 1], Is.False);
        Assert.That(shape[2, 1], Is.False);
        Assert.That(shape.OccupiedSpaceCount(), Is.EqualTo(4));
    }

    [Test]
    public void Cross_CreatesCrossShape()
    {
        var shape = Shapes.Cross();

        Assert.That(shape.Width, Is.EqualTo(3));
        Assert.That(shape.Height, Is.EqualTo(3));
        Assert.That(shape[1, 0], Is.True);
        Assert.That(shape[0, 1], Is.True);
        Assert.That(shape[1, 1], Is.True);
        Assert.That(shape[2, 1], Is.True);
        Assert.That(shape[1, 2], Is.True);
        Assert.That(shape[0, 0], Is.False);
        Assert.That(shape[2, 0], Is.False);
        Assert.That(shape[0, 2], Is.False);
        Assert.That(shape[2, 2], Is.False);
        Assert.That(shape.OccupiedSpaceCount(), Is.EqualTo(5));
    }

    // Immutable factory method tests
    [Test]
    public void ImmutableSingle_CreatesImmutableSingleShape()
    {
        var shape = Shapes.ImmutableSingle();

        Assert.That(shape.Width, Is.EqualTo(1));
        Assert.That(shape.Height, Is.EqualTo(1));
        Assert.That(shape[0, 0], Is.True);
    }

    [Test]
    public void ImmutableLine_CreatesImmutableLineShape()
    {
        var shape = Shapes.ImmutableLine(4);

        Assert.That(shape.Width, Is.EqualTo(4));
        Assert.That(shape.Height, Is.EqualTo(1));
        for (int i = 0; i < 4; i++)
        {
            Assert.That(shape[i, 0], Is.True);
        }
    }

    [Test]
    public void ImmutableSquare_CreatesImmutableSquareShape()
    {
        var shape = Shapes.ImmutableSquare(2);

        Assert.That(shape.Width, Is.EqualTo(2));
        Assert.That(shape.Height, Is.EqualTo(2));
        for (int y = 0; y < 2; y++)
        for (int x = 0; x < 2; x++)
        {
            Assert.That(shape[x, y], Is.True);
        }
    }

    [Test]
    public void ImmutableLShape_CreatesImmutableLShape()
    {
        var shape = Shapes.ImmutableLShape();

        Assert.That(shape.Width, Is.EqualTo(2));
        Assert.That(shape.Height, Is.EqualTo(2));
        Assert.That(shape[0, 0], Is.True);
        Assert.That(shape[0, 1], Is.True);
        Assert.That(shape[1, 1], Is.True);
        Assert.That(shape[1, 0], Is.False);
    }

    [Test]
    public void ImmutableTShape_CreatesImmutableTShape()
    {
        var shape = Shapes.ImmutableTShape();

        Assert.That(shape.Width, Is.EqualTo(3));
        Assert.That(shape.Height, Is.EqualTo(2));
        Assert.That(shape[0, 0], Is.True);
        Assert.That(shape[1, 0], Is.True);
        Assert.That(shape[2, 0], Is.True);
        Assert.That(shape[1, 1], Is.True);
    }

    [Test]
    public void ImmutableCross_CreatesImmutableCrossShape()
    {
        var shape = Shapes.ImmutableCross();

        Assert.That(shape.Width, Is.EqualTo(3));
        Assert.That(shape.Height, Is.EqualTo(3));
        Assert.That(shape[1, 0], Is.True);
        Assert.That(shape[0, 1], Is.True);
        Assert.That(shape[1, 1], Is.True);
        Assert.That(shape[2, 1], Is.True);
        Assert.That(shape[1, 2], Is.True);
    }

    // GetRotatedShape tests
    [Test]
    public void GetRotatedShape_None_ReturnsSameShape()
    {
        var original = Shapes.ImmutableSingle();
        var rotated = original.GetRotatedShape(RotationDegree.None);

        Assert.That(rotated.Id, Is.EqualTo(original.Id));
    }

    [Test]
    public void GetRotatedShape_Clockwise90_RotatesCorrectly()
    {
        var original = Shapes.ImmutableLine(3);
        var rotated = original.GetRotatedShape(RotationDegree.Clockwise90);

        Assert.That(rotated.Width, Is.EqualTo(1));
        Assert.That(rotated.Height, Is.EqualTo(3));
        for (int i = 0; i < 3; i++)
        {
            Assert.That(rotated[0, i], Is.True);
        }
    }

    [Test]
    public void GetRotatedShape_Clockwise180_RotatesCorrectly()
    {
        var original = Shapes.ImmutableLShape();
        var rotated = original.GetRotatedShape(RotationDegree.Clockwise180);

        Assert.That(rotated.Width, Is.EqualTo(2));
        Assert.That(rotated.Height, Is.EqualTo(2));
        // After 180 rotation, L shape should be mirrored
        Assert.That(rotated[1, 0], Is.True);
        Assert.That(rotated[1, 1], Is.True);
        Assert.That(rotated[0, 0], Is.True);
    }

    [Test]
    public void GetRotatedShape_Clockwise270_RotatesCorrectly()
    {
        var original = Shapes.ImmutableLine(3);
        var rotated = original.GetRotatedShape(RotationDegree.Clockwise270);

        Assert.That(rotated.Width, Is.EqualTo(1));
        Assert.That(rotated.Height, Is.EqualTo(3));
        for (int i = 0; i < 3; i++)
        {
            Assert.That(rotated[0, i], Is.True);
        }
    }

    [Test]
    public void GetRotatedShape_InvalidRotation_ThrowsArgumentOutOfRangeException()
    {
        var shape = Shapes.ImmutableSingle();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            shape.GetRotatedShape((RotationDegree)999);
        });
    }

    // UnsafeProcessShape tests

    [Test]
    public void UnsafeProcessShape_SmallShape_UsesBytes8()
    {
        // Test with size that fits in 8 bytes (64 bits)
        var result = Shapes.UnsafeProcessShape(4, 4, 0, (shape, data) =>
        {
            Assert.That(shape.Width, Is.EqualTo(4));
            Assert.That(shape.Height, Is.EqualTo(4));
            Assert.That(shape.Size, Is.EqualTo(16));

            // Verify we can write and read
            shape[0, 0] = true;
            shape[3, 3] = true;

            Assert.That(shape[0, 0], Is.True);
            Assert.That(shape[3, 3], Is.True);
            Assert.That(shape[1, 1], Is.False);

            return 42;
        });

        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    public void UnsafeProcessShape_MediumShape_UsesBytes32()
    {
        // Test with size that fits in 32 bytes (256 bits)
        var result = Shapes.UnsafeProcessShape(10, 10, 0, (shape, data) =>
        {
            Assert.That(shape.Width, Is.EqualTo(10));
            Assert.That(shape.Height, Is.EqualTo(10));
            Assert.That(shape.Size, Is.EqualTo(100));

            // Fill some cells
            for (int i = 0; i < 10; i++)
            {
                shape[i, i] = true;
            }

            // Verify diagonal
            for (int i = 0; i < 10; i++)
            {
                Assert.That(shape[i, i], Is.True);
            }
            Assert.That(shape[0, 1], Is.False);

            return "success";
        });

        Assert.That(result, Is.EqualTo("success"));
    }

    [Test]
    public void UnsafeProcessShape_LargeShape_UsesGridShape()
    {
        // Test with size that exceeds 32 bytes
        var result = Shapes.UnsafeProcessShape(20, 20, 0, (shape, data) =>
        {
            Assert.That(shape.Width, Is.EqualTo(20));
            Assert.That(shape.Height, Is.EqualTo(20));
            Assert.That(shape.Size, Is.EqualTo(400));

            // Set corners
            shape[0, 0] = true;
            shape[19, 0] = true;
            shape[0, 19] = true;
            shape[19, 19] = true;

            // Verify corners
            Assert.That(shape[0, 0], Is.True);
            Assert.That(shape[19, 0], Is.True);
            Assert.That(shape[0, 19], Is.True);
            Assert.That(shape[19, 19], Is.True);
            Assert.That(shape[10, 10], Is.False);

            return true;
        });

        Assert.That(result, Is.True);
    }

    [Test]
    public void UnsafeProcessShape_WithCaptureData_PassesDataCorrectly()
    {
        var capturedData = new { Value = 100, Name = "test" };

        var result = Shapes.UnsafeProcessShape(5, 5, capturedData, (shape, data) =>
        {
            Assert.That(data.Value, Is.EqualTo(100));
            Assert.That(data.Name, Is.EqualTo("test"));

            // Use the shape and data together
            shape[data.Value % 5, data.Value % 5] = true;
            return data.Value * 2;
        });

        Assert.That(result, Is.EqualTo(200));
    }

    [Test]
    public void UnsafeProcessShape_NullProcessor_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            Shapes.UnsafeProcessShape<int, int>(5, 5, 0, null!);
        });
    }

    [Test]
    public void UnsafeProcessShape_ZeroSize_WorksCorrectly()
    {
        var result = Shapes.UnsafeProcessShape(0, 0, 0, (shape, data) =>
        {
            Assert.That(shape.Width, Is.EqualTo(0));
            Assert.That(shape.Height, Is.EqualTo(0));
            Assert.That(shape.Size, Is.EqualTo(0));
            return "empty";
        });

        Assert.That(result, Is.EqualTo("empty"));
    }

    [Test]
    public void UnsafeProcessShape_SingleCell_WorksCorrectly()
    {
        var result = Shapes.UnsafeProcessShape(1, 1, 0, (shape, data) =>
        {
            Assert.That(shape.Width, Is.EqualTo(1));
            Assert.That(shape.Height, Is.EqualTo(1));
            Assert.That(shape.Size, Is.EqualTo(1));

            shape[0, 0] = true;
            Assert.That(shape[0, 0], Is.True);

            return shape.IsOccupied(0, 0);
        });

        Assert.That(result, Is.True);
    }

    [Test]
    public void UnsafeProcessShape_EdgeCaseAt64Bits_WorksCorrectly()
    {
        // Exactly 64 bits (8 bytes) - should use Bytes8
        var result = Shapes.UnsafeProcessShape(8, 8, 0, (shape, data) =>
        {
            Assert.That(shape.Size, Is.EqualTo(64));

            // Fill entire shape
            for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                shape[x, y] = true;
            }

            // Verify all set
            for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                Assert.That(shape[x, y], Is.True, $"Cell [{x},{y}] should be true");
            }

            return shape.Size;
        });

        Assert.That(result, Is.EqualTo(64));
    }

    [Test]
    public void UnsafeProcessShape_EdgeCaseAt256Bits_WorksCorrectly()
    {
        // Exactly 256 bits (32 bytes) - should use Bytes32
        var result = Shapes.UnsafeProcessShape(16, 16, 0, (shape, data) =>
        {
            Assert.That(shape.Size, Is.EqualTo(256));

            // Create checkerboard pattern
            for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
            {
                shape[x, y] = (x + y) % 2 == 0;
            }

            // Verify checkerboard
            for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
            {
                var expected = (x + y) % 2 == 0;
                Assert.That(shape[x, y], Is.EqualTo(expected), $"Cell [{x},{y}] pattern mismatch");
            }

            return shape.Size;
        });

        Assert.That(result, Is.EqualTo(256));
    }

    [Test]
    public void UnsafeProcessShape_ReadOnlyBits_AccessWorks()
    {
        var result = Shapes.UnsafeProcessShape(5, 5, 0, (shape, data) =>
        {
            shape[2, 2] = true;
            shape[3, 3] = true;

            var bits = shape.ReadOnlyBits;
            Assert.That(bits.Get(shape.GetIndex(2, 2)), Is.True);
            Assert.That(bits.Get(shape.GetIndex(3, 3)), Is.True);
            Assert.That(bits.Get(shape.GetIndex(0, 0)), Is.False);

            return bits.CountBits();
        });

        Assert.That(result, Is.EqualTo(2));
    }

    [Test]
    public void UnsafeProcessShape_MultipleInvocations_AreIndependent()
    {
        var result1 = Shapes.UnsafeProcessShape(5, 5, 1, (shape, data) =>
        {
            shape[0, 0] = true;
            return shape[0, 0];
        });

        var result2 = Shapes.UnsafeProcessShape(5, 5, 2, (shape, data) =>
        {
            // Should start fresh, not carry over from previous invocation
            return shape[0, 0];
        });

        Assert.That(result1, Is.True);
        Assert.That(result2, Is.False);
    }

    [Test]
    public void UnsafeProcessShape_RectangularShapes_WorkCorrectly()
    {
        // Wide shape
        var wideResult = Shapes.UnsafeProcessShape(10, 2, 0, (shape, data) =>
        {
            for (int x = 0; x < 10; x++)
            {
                shape[x, 0] = true;
            }
            return shape.OccupiedSpaceCount();
        });

        // Tall shape
        var tallResult = Shapes.UnsafeProcessShape(2, 10, 0, (shape, data) =>
        {
            for (int y = 0; y < 10; y++)
            {
                shape[0, y] = true;
            }
            return shape.OccupiedSpaceCount();
        });

        Assert.That(wideResult, Is.EqualTo(10));
        Assert.That(tallResult, Is.EqualTo(10));
    }

    [Test]
    public void UnsafeProcessShape_EdgeCaseAt512Bytes_UsesStackAlloc()
    {
        // Exactly 512 bytes = 4096 bits
        var result = Shapes.UnsafeProcessShape(64, 64, 0, (shape, data) =>
        {
            Assert.That(shape.Size, Is.EqualTo(4096));

            shape[0, 0] = true;
            shape[63, 63] = true;

            Assert.That(shape[0, 0], Is.True);
            Assert.That(shape[63, 63], Is.True);

            return shape.Size;
        });

        Assert.That(result, Is.EqualTo(4096));
    }

    [Test]
    public void UnsafeProcessShape_LargerThan512Bytes_UsesGridShape()
    {
        // Larger than 512 bytes
        var result = Shapes.UnsafeProcessShape(100, 100, 0, (shape, data) =>
        {
            Assert.That(shape.Size, Is.EqualTo(10000));

            shape[0, 0] = true;
            shape[99, 99] = true;

            Assert.That(shape[0, 0], Is.True);
            Assert.That(shape[99, 99], Is.True);

            return true;
        });

        Assert.That(result, Is.True);
    }

    [Test]
    public void UnsafeProcessShape_Bits_ModificationWorks()
    {
        var result = Shapes.UnsafeProcessShape(5, 5, 0, (shape, data) =>
        {
            var bits = shape.Bits;
            bits.Set(shape.GetIndex(1, 1), true);
            bits.Set(shape.GetIndex(3, 3), true);

            return shape[1, 1] && shape[3, 3];
        });

        Assert.That(result, Is.True);
    }
}
