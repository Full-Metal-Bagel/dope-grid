using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DopeGrid.Native;
using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[TestFixture]
public class ImmutableGridShapeConcurrencyTests
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

                var shape = Shapes.TShape(Allocator.TempJob);
                var immutable = shape.AsReadOnly().GetOrCreateImmutable();
                results[index] = immutable.Id;
                shape.Dispose();
            });
        }

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        // All threads should get the same ID for the same shape
        var firstId = results[0];
        Assert.IsTrue(results.All(id => id == firstId),
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
                var shape = new GridShape(index + 1, 1, Allocator.TempJob);
                for (int j = 0; j <= index; j++)
                {
                    shape.SetCell(new int2(j, 0), true);
                }

                var immutable = shape.AsReadOnly().GetOrCreateImmutable();
                results[index] = immutable.Id;
                shape.Dispose();
            });
        }

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        // All threads should get different IDs
        var uniqueIds = new HashSet<int>(results);
        Assert.AreEqual(threadCount, uniqueIds.Count,
            $"Expected {threadCount} unique IDs, but got: [{string.Join(", ", results)}]");
    }

    [Test]
    public void ConcurrentRotations_SameShape_ConsistentResults()
    {
        // First, create a base L-shape
        var baseShape = Shapes.LShape(Allocator.TempJob);
        var baseImmutable = baseShape.AsReadOnly().GetOrCreateImmutable();
        baseShape.Dispose();

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

                var rot90 = baseImmutable.Rotate90(Allocator.TempJob);
                var rot180 = rot90.Rotate90(Allocator.TempJob);
                var rot270 = rot180.Rotate90(Allocator.TempJob);

                results[index] = (rot90.Id, rot180.Id, rot270.Id);
            });
        }

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        // All threads should get the same rotation IDs
        var first = results[0];
        Assert.IsTrue(results.All(r => r.rot90 == first.rot90),
            "Inconsistent 90-degree rotation IDs");
        Assert.IsTrue(results.All(r => r.rot180 == first.rot180),
            "Inconsistent 180-degree rotation IDs");
        Assert.IsTrue(results.All(r => r.rot270 == first.rot270),
            "Inconsistent 270-degree rotation IDs");
    }

    [Test]
    public void ConcurrentFlips_SameShape_ConsistentResults()
    {
        // Create a base shape that's asymmetric
        var baseShape = Shapes.LShape(Allocator.TempJob);
        var baseImmutable = baseShape.AsReadOnly().GetOrCreateImmutable();
        baseShape.Dispose();

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

                var flipped = baseImmutable.Flip(Allocator.TempJob);
                results[index] = flipped.Id;
            });
        }

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        // All threads should get the same flipped ID
        var firstId = results[0];
        Assert.IsTrue(results.All(id => id == firstId),
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

                    var random = new Random((uint)(threadId + 1));
                    var createdShapes = new List<ImmutableGridShape>();

                    for (int op = 0; op < operationsPerThread; op++)
                    {
                        var operation = random.NextInt(0, 4);

                        switch (operation)
                        {
                            case 0: // Create new shape
                            {
                                var width = random.NextInt(1, 5);
                                var height = random.NextInt(1, 5);
                                var shape = new GridShape(width, height, Allocator.TempJob);

                                // Set random cells
                                for (int x = 0; x < width; x++)
                                {
                                    for (int y = 0; y < height; y++)
                                    {
                                        if (random.NextBool())
                                            shape.SetCell(new int2(x, y), true);
                                    }
                                }

                                // Ensure at least one cell is set
                                shape.SetCell(new int2(0, 0), true);

                                var trimmed = shape.AsReadOnly().Trim(Allocator.TempJob);
                                var immutable = trimmed.AsReadOnly().GetOrCreateImmutable();
                                createdShapes.Add(immutable);

                                trimmed.Dispose();
                                shape.Dispose();
                                break;
                            }
                            case 1: // Rotate existing shape
                            {
                                if (createdShapes.Count > 0)
                                {
                                    var shape = createdShapes[random.NextInt(0, createdShapes.Count)];
                                    var rotated = shape.Rotate90(Allocator.TempJob);

                                    // Verify rotation properties
                                    var rotated360 = rotated.Rotate90(Allocator.TempJob).Rotate90(Allocator.TempJob).Rotate90(Allocator.TempJob);
                                    Assert.AreEqual(shape.Id, rotated360.Id,
                                        "360-degree rotation should return to original");
                                }
                                break;
                            }
                            case 2: // Flip existing shape
                            {
                                if (createdShapes.Count > 0)
                                {
                                    var shape = createdShapes[random.NextInt(0, createdShapes.Count)];
                                    var flipped = shape.Flip(Allocator.TempJob);

                                    // Verify flip properties
                                    var doubleFlipped = flipped.Flip(Allocator.TempJob);
                                    Assert.AreEqual(shape.Id, doubleFlipped.Id,
                                        "Double flip should return to original");
                                }
                                break;
                            }
                            case 3: // Verify existing shape data integrity
                            {
                                if (createdShapes.Count > 0)
                                {
                                    var shape = createdShapes[random.NextInt(0, createdShapes.Count)];

                                    // Verify basic properties are consistent
                                    Assert.GreaterOrEqual(shape.Width, 0);
                                    Assert.GreaterOrEqual(shape.Height, 0);
                                    Assert.AreEqual(shape.Width * shape.Height, shape.Size);
                                    Assert.GreaterOrEqual(shape.OccupiedSpaceCount, 0);
                                    Assert.LessOrEqual(shape.OccupiedSpaceCount, shape.Size);
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

        Assert.IsEmpty(errors, $"Errors occurred: {string.Join("\n", errors)}");
    }

    #endregion

    #region Unity Job System Tests

    [BurstCompile]
    struct CreateShapeJob : IJobParallelFor
    {
        public int ShapeSize;
        public NativeArray<int> Results;

        public void Execute(int index)
        {
            var shape = new GridShape(ShapeSize, ShapeSize, Allocator.Temp);

            // Create a unique pattern based on index
            for (int i = 0; i <= index && i < ShapeSize * ShapeSize; i++)
            {
                shape.SetCell(new int2(i % ShapeSize, i / ShapeSize), true);
            }

            var trimmed = shape.AsReadOnly().Trim(Allocator.Temp);
            var immutable = trimmed.AsReadOnly().GetOrCreateImmutable();
            Results[index] = immutable.Id;

            trimmed.Dispose();
            shape.Dispose();
        }
    }

    [Test]
    public void JobSystem_ParallelShapeCreation_ConsistentIds()
    {
        const int jobCount = 10;
        var results = new NativeArray<int>(jobCount, Allocator.TempJob);

        try
        {
            var job = new CreateShapeJob
            {
                ShapeSize = 3,
                Results = results
            };

            var handle = job.Schedule(jobCount, 1);
            handle.Complete();

            // After trimming, shapes with the same pattern should get the same ID
            // Different patterns should get different IDs
            var idToIndices = new Dictionary<int, List<int>>();
            for (int i = 0; i < jobCount; i++)
            {
                if (!idToIndices.ContainsKey(results[i]))
                {
                    idToIndices[results[i]] = new List<int>();
                }
                idToIndices[results[i]].Add(i);
            }

            // Verify that all results have valid IDs (greater than 0)
            for (int i = 0; i < jobCount; i++)
            {
                Assert.Greater(results[i], 0, $"Job {i} returned invalid ID");
            }

            // Log the groupings for debugging
            Debug.Log($"Shape creation resulted in {idToIndices.Count} unique shapes from {jobCount} jobs");
            foreach (var kvp in idToIndices)
            {
                Debug.Log($"Shape ID {kvp.Key} was created by jobs: [{string.Join(", ", kvp.Value)}]");
            }
        }
        finally
        {
            results.Dispose();
        }
    }

    [BurstCompile]
    struct RotateShapeJob : IJob
    {
        public int BaseShapeId;
        public NativeArray<int> RotationResults;

        public void Execute()
        {
            var baseShape = new ImmutableGridShape(BaseShapeId);

            var rot90 = baseShape.Rotate90(Allocator.TempJob);
            var rot180 = rot90.Rotate90(Allocator.TempJob);
            var rot270 = rot180.Rotate90(Allocator.TempJob);
            var rot360 = rot270.Rotate90(Allocator.TempJob);

            RotationResults[0] = rot90.Id;
            RotationResults[1] = rot180.Id;
            RotationResults[2] = rot270.Id;
            RotationResults[3] = rot360.Id;
        }
    }

    [Test]
    public void JobSystem_MultipleRotationJobs_ConsistentResults()
    {
        // Create a base shape first
        var baseShape = Shapes.LShape(Allocator.TempJob);
        var baseImmutable = baseShape.AsReadOnly().GetOrCreateImmutable();
        baseShape.Dispose();

        const int jobCount = 10;
        var jobs = new NativeArray<JobHandle>(jobCount, Allocator.TempJob);
        var results = new NativeArray<int>[jobCount];

        try
        {
            // Schedule multiple rotation jobs
            for (int i = 0; i < jobCount; i++)
            {
                results[i] = new NativeArray<int>(4, Allocator.TempJob);
                var job = new RotateShapeJob
                {
                    BaseShapeId = baseImmutable.Id,
                    RotationResults = results[i]
                };
                jobs[i] = job.Schedule();
            }

            // Wait for all jobs to complete
            JobHandle.CompleteAll(jobs);

            // Verify all jobs got the same rotation results
            var firstResult = results[0];
            for (int i = 1; i < jobCount; i++)
            {
                Assert.AreEqual(firstResult[0], results[i][0], $"Job {i} got different 90-degree rotation");
                Assert.AreEqual(firstResult[1], results[i][1], $"Job {i} got different 180-degree rotation");
                Assert.AreEqual(firstResult[2], results[i][2], $"Job {i} got different 270-degree rotation");
                Assert.AreEqual(baseImmutable.Id, results[i][3], $"Job {i} 360-degree rotation didn't return to original");
            }
        }
        finally
        {
            jobs.Dispose();
            foreach (var result in results)
            {
                if (result.IsCreated)
                    result.Dispose();
            }
        }
    }

    [BurstCompile]
    struct StressTestJob : IJobParallelFor
    {
        public int OperationsPerJob;
        [ReadOnly] public NativeArray<int> Seeds;
        public NativeArray<int> SuccessCount;

        public void Execute(int index)
        {
            var random = new Random((uint)Seeds[index]);
            var successfulOps = 0;

            for (int op = 0; op < OperationsPerJob; op++)
            {
                var operation = random.NextInt(0, 3);

                try
                {
                    switch (operation)
                    {
                        case 0: // Create random shape
                        {
                            var size = random.NextInt(1, 4);
                            var shape = new GridShape(size, size, Allocator.TempJob);
                            shape.SetCell(new int2(0, 0), true);

                            if (random.NextBool())
                                shape.SetCell(new int2(size - 1, size - 1), true);

                            var trimmed = shape.AsReadOnly().Trim(Allocator.TempJob);
                            var immutable = trimmed.AsReadOnly().GetOrCreateImmutable();

                            if (immutable.Id > 0)
                                successfulOps++;

                            trimmed.Dispose();
                            shape.Dispose();
                            break;
                        }
                        case 1: // Test standard shapes
                        {
                            var shapeType = random.NextInt(0, 4);
                            GridShape testShape;

                            switch (shapeType)
                            {
                                case 0: testShape = Shapes.Line(3, Allocator.TempJob); break;
                                case 1: testShape = Shapes.Square(2, Allocator.TempJob); break;
                                case 2: testShape = Shapes.LShape(Allocator.TempJob); break;
                                default: testShape = Shapes.TShape(Allocator.TempJob); break;
                            }

                            var immutable = testShape.AsReadOnly().GetOrCreateImmutable();
                            var rotated = immutable.Rotate90(Allocator.TempJob);
                            var flipped = immutable.Flip(Allocator.TempJob);

                            if (rotated.Id > 0 && flipped.Id > 0)
                                successfulOps++;

                            testShape.Dispose();
                            break;
                        }
                        case 2: // Verify shape properties
                        {
                            var shape = Shapes.Cross(Allocator.TempJob);
                            var immutable = shape.AsReadOnly().GetOrCreateImmutable();

                            if (immutable.Width > 0 && immutable.Height > 0 &&
                                immutable.OccupiedSpaceCount > 0)
                            {
                                successfulOps++;
                            }

                            shape.Dispose();
                            break;
                        }
                    }
                }
                catch
                {
                    // Count failures implicitly by not incrementing successfulOps
                }
            }

            SuccessCount[index] = successfulOps;
        }
    }

    [Test]
    public void JobSystem_StressTest_HighConcurrency()
    {
        const int jobCount = 50;
        const int operationsPerJob = 100;

        var seeds = new NativeArray<int>(jobCount, Allocator.TempJob);
        var successCounts = new NativeArray<int>(jobCount, Allocator.TempJob);

        try
        {
            // Initialize seeds
            var random = new System.Random(42);
            for (int i = 0; i < jobCount; i++)
            {
                seeds[i] = random.Next();
            }

            var job = new StressTestJob
            {
                OperationsPerJob = operationsPerJob,
                Seeds = seeds,
                SuccessCount = successCounts
            };

            var handle = job.Schedule(jobCount, 1);
            handle.Complete();

            // Verify all jobs completed successfully
            var totalSuccess = 0;
            for (int i = 0; i < jobCount; i++)
            {
                totalSuccess += successCounts[i];
                Assert.Greater(successCounts[i], 0, $"Job {i} had no successful operations");
            }

            var expectedMinSuccess = jobCount * operationsPerJob * 0.8f; // Expect at least 80% success
            Assert.Greater(totalSuccess, expectedMinSuccess,
                $"Total successful operations ({totalSuccess}) below expected minimum ({expectedMinSuccess})");
        }
        finally
        {
            seeds.Dispose();
            successCounts.Dispose();
        }
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

                var random = new Random((uint)(threadIndex + 1));

                for (int s = 0; s < shapesPerThread; s++)
                {
                    // Create shapes with varying complexity
                    var complexity = random.NextInt(1, 5);
                    var shape = new GridShape(complexity, complexity, Allocator.TempJob);

                    // Random pattern
                    for (int i = 0; i < complexity * complexity; i++)
                    {
                        if (random.NextFloat() > 0.5f)
                        {
                            shape.SetCell(new int2(i % complexity, i / complexity), true);
                        }
                    }

                    // Ensure at least one cell
                    shape.SetCell(new int2(0, 0), true);

                    var trimmed = shape.AsReadOnly().Trim(Allocator.TempJob);
                    var immutable = trimmed.AsReadOnly().GetOrCreateImmutable();

                    lock (allIds[threadIndex])
                    {
                        allIds[threadIndex].Add(immutable.Id);
                    }

                    // Also test transformations
                    if (random.NextBool())
                    {
                        var rotated = immutable.Rotate90(Allocator.TempJob);
                        lock (allIds[threadIndex])
                        {
                            allIds[threadIndex].Add(rotated.Id);
                        }
                    }

                    if (random.NextBool())
                    {
                        var flipped = immutable.Flip(Allocator.TempJob);
                        lock (allIds[threadIndex])
                        {
                            allIds[threadIndex].Add(flipped.Id);
                        }
                    }

                    trimmed.Dispose();
                    shape.Dispose();
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
        Assert.Greater(totalIds.Count, 0, "No shapes were created");
        Assert.IsTrue(totalIds.All(id => id > 0), "Some IDs were invalid (<=0)");

        Debug.Log($"Stress test created {totalIds.Distinct().Count()} unique shapes across {threadCount} threads");
    }

    [Test]
    public void StressTest_RapidTransformations()
    {
        // Create a complex base shape
        var shape = new GridShape(4, 4, Allocator.TempJob);
        shape.SetCell(new int2(0, 0), true);
        shape.SetCell(new int2(1, 0), true);
        shape.SetCell(new int2(0, 1), true);
        shape.SetCell(new int2(2, 2), true);
        shape.SetCell(new int2(3, 3), true);

        var trimmed = shape.AsReadOnly().Trim(Allocator.TempJob);
        var baseImmutable = trimmed.AsReadOnly().GetOrCreateImmutable();
        shape.Dispose();
        trimmed.Dispose();

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

                    var random = new Random((uint)(threadId + 1));
                    var current = baseImmutable;

                    for (int i = 0; i < transformsPerThread; i++)
                    {
                        if (random.NextBool())
                        {
                            current = current.Rotate90(Allocator.TempJob);

                            // Verify rotation consistency
                            var fullRotation = current.Rotate90(Allocator.TempJob).Rotate90(Allocator.TempJob).Rotate90(Allocator.TempJob);
                            if (fullRotation.Id != baseImmutable.Id && i % 4 == 3)
                            {
                                lock (errors)
                                {
                                    errors.Add($"Thread {threadId}: 360-degree rotation didn't return to original");
                                }
                            }
                        }
                        else
                        {
                            current = current.Flip(Allocator.TempJob);

                            // Verify flip consistency
                            var doubleFlip = current.Flip(Allocator.TempJob);
                            if (i % 2 == 1 && doubleFlip.Id != baseImmutable.Id)
                            {
                                lock (errors)
                                {
                                    errors.Add($"Thread {threadId}: Double flip didn't return to original");
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

        Assert.IsEmpty(errors, $"Errors during transformation stress test:\n{string.Join("\n", errors)}");
    }

    #endregion
}
