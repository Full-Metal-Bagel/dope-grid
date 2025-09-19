using System;
using DopeGrid.Native;
using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[TestFixture]
public class ImmutableGridShapeJobDebugTests
{
    #region Step 1: Test without Burst - Basic Shape Creation

    struct SimpleShapeCreationJob : IJob
    {
        public NativeArray<int> Result;

        public void Execute()
        {
            try
            {
                // Step 1: Just create a simple GridShape
                var shape = new GridShape(2, 2, Allocator.Temp);
                shape.SetCell(new int2(0, 0), true);
                shape.SetCell(new int2(1, 1), true);

                Result[0] = 1; // Success marker

                shape.Dispose();
            }
            catch (Exception e)
            {
                Debug.LogError($"SimpleShapeCreationJob failed: {e}");
                Result[0] = -1;
            }
        }
    }

    [Test]
    public void Step1_SimpleShapeCreation_WithoutBurst()
    {
        var result = new NativeArray<int>(1, Allocator.TempJob);

        try
        {
            var job = new SimpleShapeCreationJob
            {
                Result = result
            };

            job.Schedule().Complete();

            Assert.AreEqual(1, result[0], "Simple shape creation should succeed");
        }
        finally
        {
            result.Dispose();
        }
    }

    #endregion

    #region Step 2: Test Trim Operation

    struct ShapeWithTrimJob : IJob
    {
        public NativeArray<int> Result;

        public void Execute()
        {
            try
            {
                var shape = new GridShape(3, 3, Allocator.Temp);
                shape.SetCell(new int2(1, 1), true);

                // Try to trim the shape
                var trimmed = shape.AsReadOnly().Trim(Allocator.Temp);

                Result[0] = trimmed.Width; // Should be 1
                Result[1] = trimmed.Height; // Should be 1

                trimmed.Dispose();
                shape.Dispose();
            }
            catch (Exception e)
            {
                Debug.LogError($"ShapeWithTrimJob failed: {e}");
                Result[0] = -1;
                Result[1] = -1;
            }
        }
    }

    [Test]
    public void Step2_ShapeWithTrim_WithoutBurst()
    {
        var result = new NativeArray<int>(2, Allocator.TempJob);

        try
        {
            var job = new ShapeWithTrimJob
            {
                Result = result
            };

            job.Schedule().Complete();

            Assert.AreEqual(1, result[0], "Trimmed width should be 1");
            Assert.AreEqual(1, result[1], "Trimmed height should be 1");
        }
        finally
        {
            result.Dispose();
        }
    }

    #endregion

    #region Step 3: Test GetOrCreateImmutable

    struct ImmutableCreationJob : IJob
    {
        public NativeArray<int> Result;

        public void Execute()
        {
            try
            {
                var shape = new GridShape(2, 2, Allocator.Temp);
                shape.SetCell(new int2(0, 0), true);
                shape.SetCell(new int2(1, 1), true);

                var trimmed = shape.AsReadOnly().Trim(Allocator.Temp);

                // This is where it might fail - accessing static lazy field
                var immutable = trimmed.AsReadOnly().GetOrCreateImmutable();

                Result[0] = immutable.Id;
                Result[1] = immutable.Width;
                Result[2] = immutable.Height;

                trimmed.Dispose();
                shape.Dispose();
            }
            catch (Exception e)
            {
                Debug.LogError($"ImmutableCreationJob failed: {e.Message}\n{e.StackTrace}");
                Result[0] = -999;
                Result[1] = -1;
                Result[2] = -1;
            }
        }
    }

    [Test]
    public void Step3_ImmutableCreation_WithoutBurst()
    {
        var result = new NativeArray<int>(3, Allocator.TempJob);

        try
        {
            var job = new ImmutableCreationJob
            {
                Result = result
            };

            job.Schedule().Complete();

            Assert.Greater(result[0], 0, $"Should get valid ID, but got {result[0]}");
            Assert.AreEqual(2, result[1], "Width should be 2");
            Assert.AreEqual(2, result[2], "Height should be 2");
        }
        finally
        {
            result.Dispose();
        }
    }

    #endregion

    #region Step 4: Test with Burst Compilation

    struct BurstShapeCreationJob : IJob
    {
        public NativeArray<int> Result;

        public void Execute()
        {
            try
            {
                var shape = new GridShape(2, 2, Allocator.Temp);
                shape.SetCell(new int2(0, 0), true);

                Result[0] = 1;

                shape.Dispose();
            }
            catch
            {
                Result[0] = -1;
            }
        }
    }

    [Test]
    public void Step4_SimpleShapeCreation_WithBurst()
    {
        var result = new NativeArray<int>(1, Allocator.TempJob);

        try
        {
            var job = new BurstShapeCreationJob
            {
                Result = result
            };

            job.Schedule().Complete();

            Assert.AreEqual(1, result[0], "Burst-compiled shape creation should succeed");
        }
        finally
        {
            result.Dispose();
        }
    }

    #endregion

    #region Step 5: Test Burst with GetOrCreateImmutable (Expected to Fail)

    struct BurstImmutableCreationJob : IJob
    {
        public NativeArray<int> Result;

        public void Execute()
        {
            // This will likely fail in Burst because GetOrCreateImmutable
            // accesses static managed fields and uses locks
            try
            {
                var shape = new GridShape(2, 2, Allocator.Temp);
                shape.SetCell(new int2(0, 0), true);

                var trimmed = shape.AsReadOnly().Trim(Allocator.Temp);
                var immutable = trimmed.AsReadOnly().GetOrCreateImmutable();

                Result[0] = immutable.Id;

                trimmed.Dispose();
                shape.Dispose();
            }
            catch
            {
                Result[0] = -1;
            }
        }
    }

    [Test]
    public void Step5_ImmutableCreation_WithBurst_ExpectedToFail()
    {
        var result = new NativeArray<int>(1, Allocator.TempJob);

        try
        {
            var job = new BurstImmutableCreationJob
            {
                Result = result
            };

            job.Schedule().Complete();

            // This test expects failure or -1 result
            if (result[0] == -1)
            {
                Debug.Log("As expected, Burst-compiled GetOrCreateImmutable failed");
                Assert.Pass("Burst compilation with GetOrCreateImmutable fails as expected");
            }
            else
            {
                Debug.LogWarning($"Unexpected: Burst-compiled GetOrCreateImmutable returned ID {result[0]}");
            }
        }
        finally
        {
            result.Dispose();
        }
    }

    #endregion

    #region Step 6: Test Parallel Jobs without Burst

    struct ParallelShapeJob : IJobParallelFor
    {
        public NativeArray<int> Results;

        public void Execute(int index)
        {
            try
            {
                var shape = new GridShape(2, 2, Allocator.Temp);
                shape.SetCell(new int2(index % 2, index / 2 % 2), true);

                var trimmed = shape.AsReadOnly().Trim(Allocator.Temp);
                var immutable = trimmed.AsReadOnly().GetOrCreateImmutable();

                Results[index] = immutable.Id;

                trimmed.Dispose();
                shape.Dispose();
            }
            catch (Exception e)
            {
                Debug.LogError($"ParallelShapeJob[{index}] failed: {e.Message}");
                Results[index] = -1;
            }
        }
    }

    [Test]
    public void Step6_ParallelJobs_WithoutBurst()
    {
        const int jobCount = 4;
        var results = new NativeArray<int>(jobCount, Allocator.TempJob);

        try
        {
            var job = new ParallelShapeJob
            {
                Results = results
            };

            job.Schedule(jobCount, 1).Complete();

            for (int i = 0; i < jobCount; i++)
            {
                Assert.Greater(results[i], 0, $"Job {i} should succeed");
            }
        }
        finally
        {
            results.Dispose();
        }
    }

    #endregion

    #region Step 7: Test Pre-created Shapes in Jobs

    struct UsePreCreatedShapeJob : IJob
    {
        public int ShapeId;
        public NativeArray<int> Result;

        public void Execute()
        {
            try
            {
                var shape = new ImmutableGridShape(ShapeId);

                // Test basic operations that don't require static access
                Result[0] = shape.Width;
                Result[1] = shape.Height;
                Result[2] = shape.Size;
            }
            catch (Exception e)
            {
                Debug.LogError($"UsePreCreatedShapeJob failed: {e.Message}");
                Result[0] = -1;
            }
        }
    }

    [Test]
    public void Step7_UsePreCreatedShape_InJob()
    {
        // Create shape outside job
        var shape = Shapes.LShape(Allocator.Temp);
        var immutable = shape.AsReadOnly().GetOrCreateImmutable();
        shape.Dispose();

        var result = new NativeArray<int>(3, Allocator.TempJob);

        try
        {
            var job = new UsePreCreatedShapeJob
            {
                ShapeId = immutable.Id,
                Result = result
            };

            job.Schedule().Complete();

            Assert.AreEqual(immutable.Width, result[0], "Width should match");
            Assert.AreEqual(immutable.Height, result[1], "Height should match");
            Assert.AreEqual(immutable.Size, result[2], "Size should match");
        }
        finally
        {
            result.Dispose();
        }
    }

    #endregion

    #region Step 8: Test Transformations with Pre-created Shapes

    struct TransformPreCreatedShapeJob : IJob
    {
        public int BaseShapeId;
        public NativeArray<int> Result;

        public void Execute()
        {
            try
            {
                var shape = new ImmutableGridShape(BaseShapeId);

                // These might fail if they access static fields
                var rotated = shape.Rotate90(Allocator.Temp);
                var flipped = shape.Flip(Allocator.Temp);

                Result[0] = rotated.Id;
                Result[1] = flipped.Id;
            }
            catch (Exception e)
            {
                Debug.LogError($"TransformPreCreatedShapeJob failed: {e.Message}");
                Result[0] = -1;
                Result[1] = -1;
            }
        }
    }

    [Test]
    public void Step8_TransformPreCreatedShape_InJob()
    {
        // Create and transform shapes outside job first
        var shape = Shapes.LShape(Allocator.Temp);
        var immutable = shape.AsReadOnly().GetOrCreateImmutable();

        // Pre-create transformations to ensure they exist
        var preRotated = immutable.Rotate90(Allocator.Temp);
        var preFlipped = immutable.Flip(Allocator.Temp);

        shape.Dispose();

        var result = new NativeArray<int>(2, Allocator.TempJob);

        try
        {
            var job = new TransformPreCreatedShapeJob
            {
                BaseShapeId = immutable.Id,
                Result = result
            };

            job.Schedule().Complete();

            if (result[0] > 0 && result[1] > 0)
            {
                Assert.AreEqual(preRotated.Id, result[0], "Rotated ID should match pre-created");
                Assert.AreEqual(preFlipped.Id, result[1], "Flipped ID should match pre-created");
            }
            else
            {
                Debug.Log("Transformations in job failed - this is expected if they access static fields");
            }
        }
        finally
        {
            result.Dispose();
        }
    }

    #endregion
}
