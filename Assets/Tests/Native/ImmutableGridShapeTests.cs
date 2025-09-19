using System.Collections.Generic;
using DopeGrid.Native;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

public class ImmutableGridShapeTests
{
    [Test]
    public void Empty_ReturnsEmptyShape()
    {
        var empty = ImmutableGridShape.Empty;

        Assert.AreEqual(0, empty.Id);
        Assert.AreEqual(int2.zero, empty.Bound);
        Assert.AreEqual(0, empty.Width);
        Assert.AreEqual(0, empty.Height);
    }

    [Test]
    public void GetOrCreateImmutable_SingleCell()
    {
        var shape = new GridShape(1, 1, Allocator.Temp);
        shape.SetCell(new int2(0, 0), true);

        var immutable = shape.AsReadOnly().GetOrCreateImmutable();

        Assert.AreNotEqual(0, immutable.Id);
        Assert.AreEqual(1, immutable.Width);
        Assert.AreEqual(1, immutable.Height);

        shape.Dispose();
    }

    [Test]
    public void GetOrCreateImmutable_Line()
    {
        var shape = Shapes.Line(3, Allocator.Temp);

        var immutable = shape.AsReadOnly().GetOrCreateImmutable();

        Assert.AreNotEqual(0, immutable.Id);
        Assert.AreEqual(3, immutable.Width);
        Assert.AreEqual(1, immutable.Height);

        shape.Dispose();
    }

    [Test]
    public void GetOrCreateImmutable_LShape()
    {
        var shape = Shapes.LShape(Allocator.Temp);

        var immutable = shape.AsReadOnly().GetOrCreateImmutable();

        Assert.AreNotEqual(0, immutable.Id);
        Assert.AreEqual(2, immutable.Width);
        Assert.AreEqual(2, immutable.Height);

        shape.Dispose();
    }

    [Test]
    public void GetOrCreateImmutable_SameShapeReturnsSameId()
    {
        var shape1 = Shapes.TShape(Allocator.Temp);
        var shape2 = Shapes.TShape(Allocator.Temp);

        var immutable1 = shape1.AsReadOnly().GetOrCreateImmutable();
        var immutable2 = shape2.AsReadOnly().GetOrCreateImmutable();

        Assert.AreEqual(immutable1.Id, immutable2.Id);

        shape1.Dispose();
        shape2.Dispose();
    }

    [Test]
    public void Rotate90_LineShape()
    {
        // Create horizontal line
        var shape = Shapes.Line(3, Allocator.Temp);
        var immutable = shape.AsReadOnly().GetOrCreateImmutable();

        // Rotate 90 degrees
        var rotated = immutable.Rotate90();

        Assert.AreNotEqual(immutable.Id, rotated.Id);
        Assert.AreEqual(1, rotated.Width);
        Assert.AreEqual(3, rotated.Height);

        // Rotate again should give 180 rotation
        var rotated180 = rotated.Rotate90();
        Assert.AreEqual(3, rotated180.Width);
        Assert.AreEqual(1, rotated180.Height);

        // Rotate again should give 270 rotation
        var rotated270 = rotated180.Rotate90();
        Assert.AreEqual(1, rotated270.Width);
        Assert.AreEqual(3, rotated270.Height);

        // Rotate again should return to original
        var rotated360 = rotated270.Rotate90();
        Assert.AreEqual(immutable.Id, rotated360.Id);

        shape.Dispose();
    }

    [Test]
    public void Rotate90_SquareShape()
    {
        // Create 2x2 square
        var shape = Shapes.Square(2, Allocator.Temp);
        var immutable = shape.AsReadOnly().GetOrCreateImmutable();

        // Rotating a square should return the same shape
        var rotated = immutable.Rotate90();

        Assert.AreEqual(immutable.Id, rotated.Id);
        Assert.AreEqual(2, rotated.Width);
        Assert.AreEqual(2, rotated.Height);

        shape.Dispose();
    }

    [Test]
    public void Rotate90_LShape()
    {
        var shape = Shapes.LShape(Allocator.Temp);
        var immutable = shape.AsReadOnly().GetOrCreateImmutable();

        // L-shape should have 4 distinct rotations
        var rotated90 = immutable.Rotate90();
        var rotated180 = rotated90.Rotate90();
        var rotated270 = rotated180.Rotate90();
        var rotated360 = rotated270.Rotate90();

        // All rotations should be different except 360 which returns to original
        Assert.AreNotEqual(immutable.Id, rotated90.Id);
        Assert.AreNotEqual(immutable.Id, rotated180.Id);
        Assert.AreNotEqual(immutable.Id, rotated270.Id);
        Assert.AreNotEqual(rotated90.Id, rotated180.Id);
        Assert.AreNotEqual(rotated90.Id, rotated270.Id);
        Assert.AreNotEqual(rotated180.Id, rotated270.Id);
        Assert.AreEqual(immutable.Id, rotated360.Id);

        shape.Dispose();
    }

    [Test]
    public void Flip_LineShape()
    {
        // Horizontal line
        var shape = Shapes.Line(3, Allocator.Temp);
        var immutable = shape.AsReadOnly().GetOrCreateImmutable();

        // Flipping horizontal line horizontally should return same shape
        var flipped = immutable.Flip();

        Assert.AreEqual(immutable.Id, flipped.Id);

        shape.Dispose();
    }

    [Test]
    public void Flip_LShape()
    {
        var shape = Shapes.LShape(Allocator.Temp);
        var immutable = shape.AsReadOnly().GetOrCreateImmutable();

        // Flipping L-shape should create a different shape
        var flipped = immutable.Flip();

        Assert.AreNotEqual(immutable.Id, flipped.Id);
        Assert.AreEqual(2, flipped.Width);
        Assert.AreEqual(2, flipped.Height);

        // Flipping again should return to original
        var doubleFlipped = flipped.Flip();
        Assert.AreEqual(immutable.Id, doubleFlipped.Id);

        shape.Dispose();
    }

    [Test]
    public void Flip_TShape()
    {
        var shape = Shapes.TShape(Allocator.Temp);
        var immutable = shape.AsReadOnly().GetOrCreateImmutable();

        var flipped = immutable.Flip();

        // T-shape flipped horizontally should be same (symmetric)
        Assert.AreEqual(immutable.Id, flipped.Id);

        shape.Dispose();
    }

    [Test]
    public void ToReadOnlyGridShape_ConversionWorks()
    {
        var shape = Shapes.Cross(Allocator.Temp);
        var immutable = shape.AsReadOnly().GetOrCreateImmutable();

        // Implicit conversion
        GridShape.ReadOnly readOnly = immutable;

        Assert.AreEqual(immutable.Width, readOnly.Width);
        Assert.AreEqual(immutable.Height, readOnly.Height);

        // Explicit method call
        var explicit_readonly = immutable.ToReadOnlyGridShape();
        Assert.AreEqual(immutable.Width, explicit_readonly.Width);
        Assert.AreEqual(immutable.Height, explicit_readonly.Height);

        shape.Dispose();
    }

    [Test]
    public void ComplexTransformations_LShape()
    {
        var shape = Shapes.LShape(Allocator.Temp);
        var immutable = shape.AsReadOnly().GetOrCreateImmutable();

        // Rotate then flip
        var rotatedThenFlipped = immutable.Rotate90().Flip();

        // Flip then rotate
        var flippedThenRotated = immutable.Flip().Rotate90();

        // These should generally produce different results
        // (unless the shape has special symmetries)
        Assert.AreNotEqual(immutable.Id, rotatedThenFlipped.Id);
        Assert.AreEqual(immutable.Id, flippedThenRotated.Id);

        shape.Dispose();
    }

    [Test]
    public void GetOrCreateImmutable_RequiresTrimmedShape()
    {
        // Create a shape with empty border
        var shape = new GridShape(5, 5, Allocator.Temp);
        shape.SetCell(new int2(2, 2), true); // Single cell in center

        // This should throw because shape is not trimmed
        Assert.Throws<System.ArgumentException>(() =>
        {
            shape.AsReadOnly().GetOrCreateImmutable();
        });

        // Trim and try again
        var trimmed = shape.AsReadOnly().Trim(Allocator.Temp);
        var immutable = trimmed.AsReadOnly().GetOrCreateImmutable();

        Assert.AreEqual(1, immutable.Width);
        Assert.AreEqual(1, immutable.Height);

        shape.Dispose();
        trimmed.Dispose();
    }

    [Test]
    public void Pattern_AccessibleAndCorrect()
    {
        var shape = new GridShape(2, 2, Allocator.Temp);
        shape.SetCell(new int2(0, 0), true);
        shape.SetCell(new int2(1, 1), true);

        var trimmedShape = shape.AsReadOnly().Trim(Allocator.Temp);
        var immutable = trimmedShape.AsReadOnly().GetOrCreateImmutable();

        var pattern = immutable.Pattern;

        Assert.IsTrue(pattern.Get(0)); // (0,0)
        Assert.IsFalse(pattern.Get(1)); // (1,0)
        Assert.IsFalse(pattern.Get(2)); // (0,1)
        Assert.IsTrue(pattern.Get(3)); // (1,1)

        shape.Dispose();
        trimmedShape.Dispose();
    }

    [Test]
    public void Rotate90_MultipleShapes_ShareRotations()
    {
        // Create two identical L-shapes
        var shape1 = Shapes.LShape(Allocator.Temp);
        var shape2 = Shapes.LShape(Allocator.Temp);

        var immutable1 = shape1.AsReadOnly().GetOrCreateImmutable();
        var immutable2 = shape2.AsReadOnly().GetOrCreateImmutable();

        // They should share the same ID
        Assert.AreEqual(immutable1.Id, immutable2.Id);

        // Their rotations should also share IDs
        var rotated1 = immutable1.Rotate90();
        var rotated2 = immutable2.Rotate90();

        Assert.AreEqual(rotated1.Id, rotated2.Id);

        shape1.Dispose();
        shape2.Dispose();
    }

    [Test]
    public void SingleCellShape_SymmetricUnderAllTransformations()
    {
        var shape = Shapes.Single(Allocator.Temp);
        var immutable = shape.AsReadOnly().GetOrCreateImmutable();

        // All transformations should return the same shape
        var rotated90 = immutable.Rotate90();
        var rotated180 = rotated90.Rotate90();
        var rotated270 = rotated180.Rotate90();
        var flipped = immutable.Flip();

        Assert.AreEqual(immutable.Id, rotated90.Id);
        Assert.AreEqual(immutable.Id, rotated180.Id);
        Assert.AreEqual(immutable.Id, rotated270.Id);
        Assert.AreEqual(immutable.Id, flipped.Id);

        shape.Dispose();
    }

    #region Stress Tests (Non-Concurrent)

    [Test]
    public void StressTest_LargeNumberOfUniqueShapes()
    {
        const int shapeCount = 1000;
        var createdIds = new HashSet<int>();
        var random = new Unity.Mathematics.Random(42);

        for (int i = 0; i < shapeCount; i++)
        {
            // Create shapes with increasing complexity
            var size = 1 + (i % 10);
            var shape = new GridShape(size, size, Allocator.Temp);

            // Create unique pattern based on index
            for (int bit = 0; bit < size * size; bit++)
            {
                if ((i & (1 << (bit % 32))) != 0 || bit == 0)
                {
                    shape.SetCell(new int2(bit % size, bit / size), true);
                }
            }

            var trimmed = shape.AsReadOnly().Trim(Allocator.Temp);
            var immutable = trimmed.AsReadOnly().GetOrCreateImmutable();

            Assert.Greater(immutable.Id, 0, $"Shape {i} has invalid ID");
            createdIds.Add(immutable.Id);

            trimmed.Dispose();
            shape.Dispose();
        }

        // Log statistics
        UnityEngine.Debug.Log($"Created {createdIds.Count} unique shapes from {shapeCount} attempts");
        Assert.Greater(createdIds.Count, shapeCount * 0.5,
            "Should create a significant number of unique shapes");
    }

    [Test]
    public void StressTest_DeepTransformationChains()
    {
        var shape = new GridShape(3, 3, Allocator.Temp);
        // Create an asymmetric pattern
        shape.SetCell(new int2(0, 0), true);
        shape.SetCell(new int2(1, 0), true);
        shape.SetCell(new int2(0, 1), true);
        shape.SetCell(new int2(2, 2), true);

        var trimmed = shape.AsReadOnly().Trim(Allocator.Temp);
        var immutable = trimmed.AsReadOnly().GetOrCreateImmutable();
        shape.Dispose();
        trimmed.Dispose();

        var seenIds = new HashSet<int> { immutable.Id };
        var current = immutable;

        // Perform a long chain of transformations
        const int transformationCount = 1000;
        var random = new Unity.Mathematics.Random(123);

        for (int i = 0; i < transformationCount; i++)
        {
            if (random.NextBool())
            {
                current = current.Rotate90();
            }
            else
            {
                current = current.Flip();
            }

            Assert.Greater(current.Id, 0, $"Invalid ID at transformation {i}");
            seenIds.Add(current.Id);
        }

        // Should cycle through a limited set of transformations (max 8 for most shapes)
        Assert.LessOrEqual(seenIds.Count, 8,
            "Shape transformations should cycle through at most 8 unique states");
        UnityEngine.Debug.Log($"Transformation chain visited {seenIds.Count} unique states");
    }

    [Test]
    public void StressTest_MemoryEfficiency_DuplicateShapes()
    {
        const int duplicateCount = 500;
        var ids = new int[duplicateCount];

        // Create the same complex shape many times
        for (int i = 0; i < duplicateCount; i++)
        {
            var shape = new GridShape(5, 5, Allocator.Temp);
            // Complex pattern
            shape.SetCell(new int2(1, 1), true);
            shape.SetCell(new int2(2, 1), true);
            shape.SetCell(new int2(3, 1), true);
            shape.SetCell(new int2(2, 2), true);
            shape.SetCell(new int2(2, 3), true);

            var trimmed = shape.AsReadOnly().Trim(Allocator.Temp);
            ids[i] = trimmed.AsReadOnly().GetOrCreateImmutable().Id;

            trimmed.Dispose();
            shape.Dispose();
        }

        // All should return the same ID (memory efficient)
        var firstId = ids[0];
        for (int i = 1; i < duplicateCount; i++)
        {
            Assert.AreEqual(firstId, ids[i],
                $"Duplicate shape {i} got different ID: expected {firstId}, got {ids[i]}");
        }
    }

    [Test]
    public void StressTest_AllPossibleSmallShapes()
    {
        // Test all possible 2x2 shapes (2^4 = 16 combinations)
        var uniqueIds = new HashSet<int>();

        for (int pattern = 0; pattern < 16; pattern++)
        {
            var shape = new GridShape(2, 2, Allocator.Temp);

            // Set cells based on bit pattern
            for (int bit = 0; bit < 4; bit++)
            {
                if ((pattern & (1 << bit)) != 0)
                {
                    shape.SetCell(new int2(bit % 2, bit / 2), true);
                }
            }

            // Skip empty shape
            if (pattern == 0)
            {
                shape.Dispose();
                continue;
            }

            var trimmed = shape.AsReadOnly().Trim(Allocator.Temp);
            var immutable = trimmed.AsReadOnly().GetOrCreateImmutable();

            uniqueIds.Add(immutable.Id);

            // Test all transformations
            var rot90 = immutable.Rotate90();
            var rot180 = rot90.Rotate90();
            var rot270 = rot180.Rotate90();
            var flipped = immutable.Flip();

            Assert.Greater(rot90.Id, 0);
            Assert.Greater(rot180.Id, 0);
            Assert.Greater(rot270.Id, 0);
            Assert.Greater(flipped.Id, 0);

            trimmed.Dispose();
            shape.Dispose();
        }

        UnityEngine.Debug.Log($"Created {uniqueIds.Count} unique IDs from 15 possible 2x2 patterns");
        // Due to rotations and flips being equivalent, we expect fewer unique IDs than patterns
        Assert.Greater(uniqueIds.Count, 0);
        Assert.LessOrEqual(uniqueIds.Count, 15);
    }

    [Test]
    public void StressTest_RapidGetOrCreate_SameShapes()
    {
        // Rapidly create and retrieve the same shapes
        const int iterations = 10000;
        var random = new Unity.Mathematics.Random(999);

        // Pre-create a set of template shapes
        var templates = new (GridShape shape, int expectedId)[5];

        templates[0].shape = Shapes.Line(3, Allocator.Temp);
        templates[0].expectedId = templates[0].shape.AsReadOnly().GetOrCreateImmutable().Id;

        templates[1].shape = Shapes.Square(2, Allocator.Temp);
        templates[1].expectedId = templates[1].shape.AsReadOnly().GetOrCreateImmutable().Id;

        templates[2].shape = Shapes.LShape(Allocator.Temp);
        templates[2].expectedId = templates[2].shape.AsReadOnly().GetOrCreateImmutable().Id;

        templates[3].shape = Shapes.TShape(Allocator.Temp);
        templates[3].expectedId = templates[3].shape.AsReadOnly().GetOrCreateImmutable().Id;

        templates[4].shape = Shapes.Cross(Allocator.Temp);
        templates[4].expectedId = templates[4].shape.AsReadOnly().GetOrCreateImmutable().Id;

        // Rapidly create copies and verify they get the same ID
        for (int i = 0; i < iterations; i++)
        {
            var templateIndex = random.NextInt(0, 5);
            var template = templates[templateIndex];

            // Create a copy of the template
            var shapeCopy = new GridShape(template.shape.Width, template.shape.Height, Allocator.Temp);
            template.shape.AsReadOnly().CopyTo(shapeCopy);

            var immutable = shapeCopy.AsReadOnly().GetOrCreateImmutable();
            Assert.AreEqual(template.expectedId, immutable.Id,
                $"Iteration {i}: Copy of template {templateIndex} got wrong ID");

            shapeCopy.Dispose();
        }

        // Clean up templates
        foreach (var template in templates)
        {
            template.shape.Dispose();
        }
    }

    [Test]
    public void StressTest_ComplexTransformationVerification()
    {
        // Create a complex asymmetric shape
        var shape = new GridShape(4, 3, Allocator.Temp);
        shape.SetCell(new int2(0, 0), true);
        shape.SetCell(new int2(1, 0), true);
        shape.SetCell(new int2(2, 0), true);
        shape.SetCell(new int2(0, 1), true);
        shape.SetCell(new int2(2, 1), true);
        shape.SetCell(new int2(2, 2), true);
        shape.SetCell(new int2(3, 2), true);

        var trimmed = shape.AsReadOnly().Trim(Allocator.Temp);
        var original = trimmed.AsReadOnly().GetOrCreateImmutable();

        // Track all transformation combinations
        var transformationMap = new Dictionary<(int r, int f), int>();

        // Test all combinations of rotations (0-3) and flips (0-1)
        for (int rotations = 0; rotations < 4; rotations++)
        {
            for (int flips = 0; flips < 2; flips++)
            {
                var current = original;

                // Apply rotations
                for (int r = 0; r < rotations; r++)
                {
                    current = current.Rotate90();
                }

                // Apply flip
                if (flips == 1)
                {
                    current = current.Flip();
                }

                transformationMap[(rotations, flips)] = current.Id;

                // Verify inverse operations
                if (flips == 1)
                {
                    var unflipped = current.Flip();
                    for (int r = 0; r < (4 - rotations) % 4; r++)
                    {
                        unflipped = unflipped.Rotate90();
                    }
                    Assert.AreEqual(original.Id, unflipped.Id,
                        $"Failed to return to original from ({rotations}, {flips})");
                }
            }
        }

        // Log unique transformation results
        var uniqueTransforms = new HashSet<int>(transformationMap.Values);
        UnityEngine.Debug.Log($"Complex shape has {uniqueTransforms.Count} unique transformations out of 8 possible");

        shape.Dispose();
        trimmed.Dispose();
    }

    [Test]
    public void StressTest_ExtremeSizes()
    {
        // Test very small shapes
        for (int size = 1; size <= 3; size++)
        {
            var shape = new GridShape(size, 1, Allocator.Temp);
            for (int i = 0; i < size; i++)
            {
                shape.SetCell(new int2(i, 0), true);
            }

            var immutable = shape.AsReadOnly().GetOrCreateImmutable();
            Assert.AreEqual(size, immutable.Width);
            Assert.AreEqual(1, immutable.Height);
            Assert.AreEqual(size, immutable.OccupiedSpaceCount);

            shape.Dispose();
        }

        // Test larger shapes (within reasonable bounds for unit tests)
        var largeShape = new GridShape(16, 16, Allocator.Temp);

        // Create a complex pattern
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 16; y++)
            {
                if ((x + y) % 3 == 0 || (x * y) % 5 == 0)
                {
                    largeShape.SetCell(new int2(x, y), true);
                }
            }
        }

        var trimmedLarge = largeShape.AsReadOnly().Trim(Allocator.Temp);
        var largeImmutable = trimmedLarge.AsReadOnly().GetOrCreateImmutable();

        Assert.Greater(largeImmutable.Id, 0);
        Assert.Greater(largeImmutable.OccupiedSpaceCount, 0);
        Assert.LessOrEqual(largeImmutable.OccupiedSpaceCount, largeImmutable.Size);

        // Test transformations work on large shapes
        var rotatedLarge = largeImmutable.Rotate90();
        var flippedLarge = largeImmutable.Flip();

        Assert.Greater(rotatedLarge.Id, 0);
        Assert.Greater(flippedLarge.Id, 0);

        largeShape.Dispose();
        trimmedLarge.Dispose();
    }

    #endregion
}
