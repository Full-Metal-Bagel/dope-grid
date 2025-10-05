using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DopeGrid;
using NUnit.Framework;

namespace DopeGrid.Tests;

/// <summary>
/// Standard C# concurrency tests for ImmutableGridShape (no Unity dependencies)
/// </summary>
[TestFixture]
public class ImmutableGridShapeConcurrencyStandardTests
{
    [TearDown]
    public void Cleanup()
    {
        // Force cleanup after each test to reset the static state
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    #region Multi-Threading Tests

    [Test]
    public void ConcurrentGetOrCreate_SameShape_ReturnsSameId()
    {
        const int threadCount = 10;
        var results = new int[threadCount];
        var threads = new Thread[threadCount];
        var barrier = new Barrier(threadCount);

        for (int i = 0; i < threadCount; i++)
        {
            var index = i;
            threads[i] = new Thread(() =>
            {
                barrier.SignalAndWait(); // Ensure all threads start simultaneously

                var shape = Shapes.TShape();
                var immutable = shape.GetOrCreateImmutable();
                results[index] = immutable.Id;
            });
        }

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        // All threads should get the same ID for the same shape
        var firstId = results[0];
        Assert.That(results.All(id => id == firstId), Is.True,
            $"Expected all IDs to be {firstId}, but got: [{string.Join(", ", results)}]");
    }

    [Test]
    public void ConcurrentGetOrCreate_DifferentShapes_ReturnsUniqueIds()
    {
        const int threadCount = 10;
        var results = new int[threadCount];
        var threads = new Thread[threadCount];
        var barrier = new Barrier(threadCount);

        for (int i = 0; i < threadCount; i++)
        {
            var index = i;
            threads[i] = new Thread(() =>
            {
                barrier.SignalAndWait(); // Ensure all threads start simultaneously

                // Each thread creates a unique shape
                var shape = Shapes.Line(index + 1);
                var immutable = shape.GetOrCreateImmutable();
                results[index] = immutable.Id;
            });
        }

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        // All threads should get different IDs
        var uniqueIds = new HashSet<int>(results);
        Assert.That(uniqueIds.Count, Is.EqualTo(threadCount),
            $"Expected {threadCount} unique IDs, but got: [{string.Join(", ", results)}]");
    }

    [Test]
    public void ConcurrentRotations_SameShape_ConsistentResults()
    {
        // First, create a base L-shape
        var baseShape = Shapes.LShape();
        var baseImmutable = baseShape.GetOrCreateImmutable();

        const int threadCount = 20;
        var results = new (int rot90, int rot180, int rot270)[threadCount];
        var threads = new Thread[threadCount];
        var barrier = new Barrier(threadCount);

        for (int i = 0; i < threadCount; i++)
        {
            var index = i;
            threads[i] = new Thread(() =>
            {
                barrier.SignalAndWait(); // Ensure all threads start simultaneously

                var rot90 = baseImmutable.Rotate90();
                var rot180 = rot90.Rotate90();
                var rot270 = rot180.Rotate90();

                results[index] = (rot90.Id, rot180.Id, rot270.Id);
            });
        }

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        // All threads should get the same rotation IDs
        var first = results[0];
        Assert.That(results.All(r => r.rot90 == first.rot90), Is.True,
            "Inconsistent 90-degree rotation IDs");
        Assert.That(results.All(r => r.rot180 == first.rot180), Is.True,
            "Inconsistent 180-degree rotation IDs");
        Assert.That(results.All(r => r.rot270 == first.rot270), Is.True,
            "Inconsistent 270-degree rotation IDs");
    }

    [Test]
    public void ConcurrentFlips_SameShape_ConsistentResults()
    {
        // Create a base shape that's asymmetric
        var baseShape = Shapes.LShape();
        var baseImmutable = baseShape.GetOrCreateImmutable();

        const int threadCount = 20;
        var results = new int[threadCount];
        var threads = new Thread[threadCount];
        var barrier = new Barrier(threadCount);

        for (int i = 0; i < threadCount; i++)
        {
            var index = i;
            threads[i] = new Thread(() =>
            {
                barrier.SignalAndWait(); // Ensure all threads start simultaneously

                var flipped = baseImmutable.Flip();
                results[index] = flipped.Id;
            });
        }

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        // All threads should get the same flipped ID
        var firstId = results[0];
        Assert.That(results.All(id => id == firstId), Is.True,
            $"Expected all flipped IDs to be {firstId}, but got: [{string.Join(", ", results)}]");
    }

    [Test]
    public void ConcurrentMixedOperations_NoDataCorruption()
    {
        const int operationsPerThread = 50;
        const int threadCount = 8;
        var errors = new List<Exception>();
        var threads = new Thread[threadCount];
        var barrier = new Barrier(threadCount);

        for (int i = 0; i < threadCount; i++)
        {
            var threadId = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    barrier.SignalAndWait();

                    var random = new Random(threadId + 1);
                    var createdShapes = new List<ImmutableGridShape>();

                    for (int op = 0; op < operationsPerThread; op++)
                    {
                        var operation = random.Next(0, 4);

                        switch (operation)
                        {
                            case 0: // Create new shape
                            {
                                var width = random.Next(1, 5);
                                var height = random.Next(1, 5);
                                using var shape = new GridShape(width, height);

                                // Set random cells
                                for (int x = 0; x < width; x++)
                                {
                                    for (int y = 0; y < height; y++)
                                    {
                                        if (random.Next(0, 2) == 1)
                                            shape[x, y] = true;
                                    }
                                }

                                // Ensure at least one cell is set
                                shape[0, 0] = true;

                                var (_, _, trimWidth, trimHeight) = shape.GetTrimmedBound<GridShape, bool>();
                                using var trimmed = new GridShape(trimWidth, trimHeight);
                                shape.Trim<GridShape, GridShape, bool>(trimmed);
                                var immutable = trimmed.GetOrCreateImmutable();
                                createdShapes.Add(immutable);
                                break;
                            }
                            case 1: // Rotate existing shape
                            {
                                if (createdShapes.Count > 0)
                                {
                                    var shape = createdShapes[random.Next(0, createdShapes.Count)];
                                    var rotated = shape.Rotate90();

                                    // Verify rotation properties
                                    var rotated360 = rotated.Rotate90().Rotate90().Rotate90();
                                    Assert.That(rotated360.Id, Is.EqualTo(shape.Id),
                                        "360-degree rotation should return to original");
                                }
                                break;
                            }
                            case 2: // Flip existing shape
                            {
                                if (createdShapes.Count > 0)
                                {
                                    var shape = createdShapes[random.Next(0, createdShapes.Count)];
                                    var flipped = shape.Flip();

                                    // Verify flip properties
                                    var doubleFlipped = flipped.Flip();
                                    Assert.That(doubleFlipped.Id, Is.EqualTo(shape.Id),
                                        "Double flip should return to original");
                                }
                                break;
                            }
                            case 3: // Verify existing shape data integrity
                            {
                                if (createdShapes.Count > 0)
                                {
                                    var shape = createdShapes[random.Next(0, createdShapes.Count)];

                                    // Verify basic properties are consistent
                                    Assert.That(shape.Width, Is.GreaterThanOrEqualTo(0));
                                    Assert.That(shape.Height, Is.GreaterThanOrEqualTo(0));
                                    Assert.That(shape.Size, Is.EqualTo(shape.Width * shape.Height));
                                    Assert.That(shape.OccupiedSpaceCount(), Is.GreaterThanOrEqualTo(0));
                                    Assert.That(shape.OccupiedSpaceCount(), Is.LessThanOrEqualTo(shape.Size));
                                }
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    lock (errors)
                    {
                        errors.Add(ex);
                    }
                }
            });
        }

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        Assert.That(errors, Is.Empty, $"Errors occurred: {string.Join("\n", errors)}");
    }

    #endregion

    #region Task-based Async Tests

    [Test]
    public async Task AsyncConcurrentCreation_SameShape_ReturnsSameId()
    {
        const int taskCount = 20;
        var tasks = new Task<int>[taskCount];

        for (int i = 0; i < taskCount; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                var shape = Shapes.TShape();
                return shape.GetOrCreateImmutable().Id;
            });
        }

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        // All tasks should get the same ID
        var firstId = results[0];
        Assert.That(results.All(id => id == firstId), Is.True,
            $"Expected all IDs to be {firstId}, but got: [{string.Join(", ", results)}]");
    }

    [Test]
    public async Task AsyncConcurrentTransformations_ConsistentResults()
    {
        var baseShape = Shapes.LShape();
        var baseImmutable = baseShape.GetOrCreateImmutable();

        const int taskCount = 30;
        var tasks = new Task<(int rotId, int flipId)>[taskCount];

        for (int i = 0; i < taskCount; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                var rotated = baseImmutable.Rotate90();
                var flipped = baseImmutable.Flip();
                return (rotated.Id, flipped.Id);
            });
        }

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        // All tasks should get the same transformation IDs
        var first = results[0];
        Assert.That(results.All(r => r.rotId == first.rotId), Is.True,
            "Inconsistent rotation IDs");
        Assert.That(results.All(r => r.flipId == first.flipId), Is.True,
            "Inconsistent flip IDs");
    }

    [Test]
    public async Task AsyncStressTest_ParallelOperations()
    {
        const int taskCount = 50;
        const int operationsPerTask = 100;
        var errors = new List<string>();
        var errorsLock = new object();

        var tasks = Enumerable.Range(0, taskCount).Select(taskId => Task.Run(() =>
        {
            try
            {
                var random = new Random(taskId);
                var successCount = 0;

                for (int op = 0; op < operationsPerTask; op++)
                {
                    var operation = random.Next(0, 3);

                    switch (operation)
                    {
                        case 0: // Create random shape
                        {
                            var size = random.Next(1, 4);
                            using var shape = new GridShape(size, size);
                            shape[0, 0] = true;

                            if (random.Next(0, 2) == 1)
                                shape[size - 1, size - 1] = true;

                            var (_, _, trimWidth, trimHeight) = shape.GetTrimmedBound<GridShape, bool>();
                            using var trimmed = new GridShape(trimWidth, trimHeight);
                            shape.Trim<GridShape, GridShape, bool>(trimmed);
                            var immutable = trimmed.GetOrCreateImmutable();

                            if (immutable.Id > 0)
                                successCount++;
                            break;
                        }
                        case 1: // Test standard shapes
                        {
                            var shapeType = random.Next(0, 4);
                            var testShape = shapeType switch
                            {
                                0 => Shapes.Line(3),
                                1 => Shapes.Square(2),
                                2 => Shapes.LShape(),
                                _ => Shapes.TShape()
                            };

                            var immutable = testShape.GetOrCreateImmutable();
                            var rotated = immutable.Rotate90();
                            var flipped = immutable.Flip();

                            if (rotated.Id > 0 && flipped.Id > 0)
                                successCount++;
                            break;
                        }
                        case 2: // Verify shape properties
                        {
                            var shape = Shapes.Cross();
                            var immutable = shape.GetOrCreateImmutable();

                            if (immutable.Width > 0 && immutable.Height > 0 &&
                                immutable.OccupiedSpaceCount() > 0)
                            {
                                successCount++;
                            }
                            break;
                        }
                    }
                }

                return successCount;
            }
            catch (Exception ex)
            {
                lock (errorsLock)
                {
                    errors.Add($"Task {taskId}: {ex.Message}");
                }
                return 0;
            }
        })).ToArray();

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        Assert.That(errors, Is.Empty, $"Errors occurred:\n{string.Join("\n", errors)}");

        var totalSuccess = results.Sum();
        var expectedMinSuccess = taskCount * operationsPerTask * 0.9; // Expect at least 90% success

        Assert.That(totalSuccess, Is.GreaterThan(expectedMinSuccess),
            $"Total successful operations ({totalSuccess}) below expected minimum ({expectedMinSuccess})");
    }

    #endregion

    #region Stress Tests

    [Test]
    public void StressTest_MassiveConcurrentCreation()
    {
        const int threadCount = 20;
        const int shapesPerThread = 100;
        var allIds = new List<int>[threadCount];
        var threads = new Thread[threadCount];
        var startSignal = new ManualResetEventSlim(false);

        for (int t = 0; t < threadCount; t++)
        {
            var threadIndex = t;
            allIds[threadIndex] = new List<int>();

            threads[t] = new Thread(() =>
            {
                startSignal.Wait(); // Wait for all threads to be ready

                var random = new Random(threadIndex + 1);

                for (int s = 0; s < shapesPerThread; s++)
                {
                    // Create shapes with varying complexity
                    var complexity = random.Next(1, 5);
                    using var shape = new GridShape(complexity, complexity);

                    // Random pattern
                    for (int i = 0; i < complexity * complexity; i++)
                    {
                        if (random.NextDouble() > 0.5)
                        {
                            shape[i % complexity, i / complexity] = true;
                        }
                    }

                    // Ensure at least one cell
                    shape[0, 0] = true;

                    var (_, _, trimWidth, trimHeight) = shape.GetTrimmedBound<GridShape, bool>();
                    using var trimmed = new GridShape(trimWidth, trimHeight);
                    shape.Trim<GridShape, GridShape, bool>(trimmed);
                    var immutable = trimmed.GetOrCreateImmutable();

                    lock (allIds[threadIndex])
                    {
                        allIds[threadIndex].Add(immutable.Id);
                    }

                    // Also test transformations
                    if (random.Next(0, 2) == 1)
                    {
                        var rotated = immutable.Rotate90();
                        lock (allIds[threadIndex])
                        {
                            allIds[threadIndex].Add(rotated.Id);
                        }
                    }

                    if (random.Next(0, 2) == 1)
                    {
                        var flipped = immutable.Flip();
                        lock (allIds[threadIndex])
                        {
                            allIds[threadIndex].Add(flipped.Id);
                        }
                    }
                }
            });
        }

        // Start all threads
        foreach (var thread in threads) thread.Start();
        Thread.Sleep(100); // Give threads time to reach the wait
        startSignal.Set(); // Release all threads simultaneously

        // Wait for completion
        foreach (var thread in threads) thread.Join();

        // Verify results
        var totalIds = allIds.SelectMany(list => list).ToList();
        Assert.That(totalIds.Count, Is.GreaterThan(0), "No shapes were created");
        Assert.That(totalIds.All(id => id > 0), Is.True, "Some IDs were invalid (<=0)");

        TestContext.WriteLine($"Stress test created {totalIds.Distinct().Count()} unique shapes across {threadCount} threads");
    }

    [Test]
    public void StressTest_RapidTransformations()
    {
        // Create a complex base shape
        using var shape = new GridShape(4, 4);
        shape[0, 0] = true;
        shape[1, 0] = true;
        shape[0, 1] = true;
        shape[2, 2] = true;
        shape[3, 3] = true;

        var (_, _, trimWidth, trimHeight) = shape.GetTrimmedBound<GridShape, bool>();
        using var trimmed = new GridShape(trimWidth, trimHeight);
        shape.Trim<GridShape, GridShape, bool>(trimmed);
        var baseImmutable = trimmed.GetOrCreateImmutable();

        const int threadCount = 30;
        const int transformsPerThread = 200;
        var errors = new List<string>();
        var threads = new Thread[threadCount];
        var barrier = new Barrier(threadCount);

        for (int t = 0; t < threadCount; t++)
        {
            var threadId = t;
            threads[t] = new Thread(() =>
            {
                try
                {
                    barrier.SignalAndWait();

                    var random = new Random(threadId + 1);
                    var current = baseImmutable;

                    for (int i = 0; i < transformsPerThread; i++)
                    {
                        if (random.Next(0, 2) == 1)
                        {
                            // Verify that 4 rotations return to the same shape
                            // Do this check occasionally to avoid too much overhead
                            if (i % 10 == 0)
                            {
                                var rot360 = current.Rotate90().Rotate90().Rotate90().Rotate90();
                                if (rot360.Id != current.Id)
                                {
                                    lock (errors)
                                    {
                                        errors.Add($"Thread {threadId}: 4 rotations didn't return to same shape (got {rot360.Id}, expected {current.Id})");
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Verify that double flip returns to the same shape
                            // Do this check occasionally to avoid too much overhead
                            if (i % 10 == 0)
                            {
                                var doubleFlip = current.Flip().Flip();
                                if (doubleFlip.Id != current.Id)
                                {
                                    lock (errors)
                                    {
                                        errors.Add($"Thread {threadId}: Double flip didn't return to same shape (got {doubleFlip.Id}, expected {current.Id})");
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    lock (errors)
                    {
                        errors.Add($"Thread {threadId}: {ex.Message}");
                    }
                }
            });
        }

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        Assert.That(errors, Is.Empty, $"Errors during transformation stress test:\n{string.Join("\n", errors)}");
    }

    [Test]
    public void StressTest_ConcurrentShapeDeduplication()
    {
        // Test that identical shapes created concurrently get deduplicated properly
        const int threadCount = 16;
        const int shapesPerThread = 50;
        var barrier = new Barrier(threadCount);
        var allResults = new int[threadCount][];

        var threads = new Thread[threadCount];
        for (int t = 0; t < threadCount; t++)
        {
            var threadIndex = t;
            allResults[threadIndex] = new int[shapesPerThread];

            threads[t] = new Thread(() =>
            {
                barrier.SignalAndWait();

                // Each thread creates the same sequence of shapes
                for (int i = 0; i < shapesPerThread; i++)
                {
                    using var shape = new GridShape(i % 3 + 1, i % 3 + 1);
                    // Create a deterministic pattern based on index
                    for (int j = 0; j <= i % 5; j++)
                    {
                        int x = j % shape.Width;
                        int y = j / shape.Width;
                        if (y < shape.Height)
                            shape[x, y] = true;
                    }

                    var (_, _, trimWidth, trimHeight) = shape.GetTrimmedBound<GridShape, bool>();
                    using var trimmed = new GridShape(trimWidth, trimHeight);
                    shape.Trim<GridShape, GridShape, bool>(trimmed);
                    var immutable = trimmed.GetOrCreateImmutable();
                    allResults[threadIndex][i] = immutable.Id;
                }
            });
        }

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        // Verify all threads got the same IDs for the same shapes
        for (int i = 0; i < shapesPerThread; i++)
        {
            var expectedId = allResults[0][i];
            for (int t = 1; t < threadCount; t++)
            {
                Assert.That(allResults[t][i], Is.EqualTo(expectedId),
                    $"Thread {t} shape {i} got ID {allResults[t][i]}, expected {expectedId}");
            }
        }

        TestContext.WriteLine($"Deduplication test: {threadCount} threads created {shapesPerThread} shapes each, all properly deduplicated");
    }

    #endregion
}
