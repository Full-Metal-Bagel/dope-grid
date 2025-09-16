using System.Collections.Generic;
using DopeGrid;
using NUnit.Framework;
using Unity.Collections;

[TestFixture]
public class NativeBitArrayHashCodeTests
{
    [Test]
    public void CalculateHashCode_EmptyArrays_ReturnsSameHash()
    {
        using var array1 = new NativeBitArray(64, Allocator.Temp);
        using var array2 = new NativeBitArray(64, Allocator.Temp);

        var hash1 = array1.AsReadOnly().CalculateHashCode();
        var hash2 = array2.AsReadOnly().CalculateHashCode();

        Assert.AreEqual(hash1, hash2, "Empty arrays of same size should have same hash");
    }

    [Test]
    public void CalculateHashCode_IdenticalContent_ReturnsSameHash()
    {
        using var array1 = new NativeBitArray(100, Allocator.Temp);
        using var array2 = new NativeBitArray(100, Allocator.Temp);

        // Set same bits
        array1.Set(0, true);
        array1.Set(10, true);
        array1.Set(50, true);
        array1.Set(99, true);

        array2.Set(0, true);
        array2.Set(10, true);
        array2.Set(50, true);
        array2.Set(99, true);

        var hash1 = array1.AsReadOnly().CalculateHashCode();
        var hash2 = array2.AsReadOnly().CalculateHashCode();

        Assert.AreEqual(hash1, hash2, "Arrays with identical content should have same hash");
    }

    [Test]
    public void CalculateHashCode_DifferentContent_UsuallyDifferentHash()
    {
        using var array1 = new NativeBitArray(64, Allocator.Temp);
        using var array2 = new NativeBitArray(64, Allocator.Temp);

        array1.Set(0, true);
        array2.Set(1, true);

        var hash1 = array1.AsReadOnly().CalculateHashCode();
        var hash2 = array2.AsReadOnly().CalculateHashCode();

        Assert.AreNotEqual(hash1, hash2, "Arrays with different content should usually have different hash");
    }

    [Test]
    public void CalculateHashCode_DifferentLength_DifferentHash()
    {
        using var array1 = new NativeBitArray(64, Allocator.Temp);
        using var array2 = new NativeBitArray(65, Allocator.Temp);

        // Even with same bit pattern, different lengths should give different hashes
        var hash1 = array1.AsReadOnly().CalculateHashCode();
        var hash2 = array2.AsReadOnly().CalculateHashCode();

        Assert.AreNotEqual(hash1, hash2, "Arrays with different lengths should have different hash");
    }

    [Test]
    public void CalculateHashCode_ConsistentAcrossMultipleCalls()
    {
        using var array = new NativeBitArray(100, Allocator.Temp);
        array.Set(5, true);
        array.Set(50, true);
        array.Set(95, true);

        var readOnly = array.AsReadOnly();
        var hash1 = readOnly.CalculateHashCode();
        var hash2 = readOnly.CalculateHashCode();
        var hash3 = readOnly.CalculateHashCode();

        Assert.AreEqual(hash1, hash2, "Hash should be consistent across calls");
        Assert.AreEqual(hash2, hash3, "Hash should be consistent across calls");
    }

    [Test]
    public void CalculateHashCode_TailBitsHandled()
    {
        // Test with various non-64-bit-aligned sizes
        using var array1 = new NativeBitArray(65, Allocator.Temp);
        using var array2 = new NativeBitArray(65, Allocator.Temp);

        // Set bit in the tail portion
        array1.Set(64, true);
        array2.Set(64, true);

        var hash1 = array1.AsReadOnly().CalculateHashCode();
        var hash2 = array2.AsReadOnly().CalculateHashCode();

        Assert.AreEqual(hash1, hash2, "Arrays with same tail bits should have same hash");

        // Modify tail bit
        array2.Set(64, false);
        var hash3 = array2.AsReadOnly().CalculateHashCode();

        Assert.AreNotEqual(hash1, hash3, "Different tail bits should produce different hash");
    }

    [Test]
    public void CalculateHashCode_NullOrEmpty_ReturnsZero()
    {
        var notCreated = new NativeBitArray();
        Assert.AreEqual(0, notCreated.CalculateHashCode(), "Non-created array should return hash 0");

        using var empty = new NativeBitArray(0, Allocator.Temp);
        Assert.AreEqual(0, empty.AsReadOnly().CalculateHashCode(), "Zero-length array should return hash 0");
    }

    [Test]
    public void CalculateHashCode_GoodDistribution()
    {
        var hashes = new HashSet<int>();
        var collisions = 0;

        // Create many arrays with slightly different patterns
        for (int i = 0; i < 100; i++)
        {
            using var array = new NativeBitArray(128, Allocator.Temp);

            // Set bits based on index to create unique patterns
            array.Set(i % 128, true);
            if (i > 0) array.Set((i * 7) % 128, true);
            if (i > 50) array.Set((i * 13) % 128, true);

            var hash = array.AsReadOnly().CalculateHashCode();

            if (!hashes.Add(hash))
                collisions++;
        }

        // Allow some collisions but not too many (good hash function should minimize collisions)
        Assert.Less(collisions, 10, $"Too many hash collisions: {collisions}/100");
        UnityEngine.Debug.Log($"Hash distribution test: {collisions} collisions out of 100 unique patterns");
    }

    [Test]
    public void CalculateHashCode_AllBitsSet_DifferentFromEmpty()
    {
        using var empty = new NativeBitArray(64, Allocator.Temp);
        using var full = new NativeBitArray(64, Allocator.Temp);

        for (int i = 0; i < 64; i++)
        {
            full.Set(i, true);
        }

        var emptyHash = empty.AsReadOnly().CalculateHashCode();
        var fullHash = full.AsReadOnly().CalculateHashCode();

        Assert.AreNotEqual(emptyHash, fullHash, "All bits set should have different hash from empty");
    }

    [Test]
    public void CalculateHashCode_ComplexPatterns()
    {
        using var array1 = new NativeBitArray(256, Allocator.Temp);
        using var array2 = new NativeBitArray(256, Allocator.Temp);

        // Create a complex pattern
        for (int i = 0; i < 256; i++)
        {
            if (i % 3 == 0 || i % 5 == 0 || i % 7 == 0)
            {
                array1.Set(i, true);
                array2.Set(i, true);
            }
        }

        Assert.AreEqual(
            array1.AsReadOnly().CalculateHashCode(),
            array2.AsReadOnly().CalculateHashCode(),
            "Complex identical patterns should have same hash"
        );

        // Change one bit
        array2.Set(100, !array2.IsSet(100));

        Assert.AreNotEqual(
            array1.AsReadOnly().CalculateHashCode(),
            array2.AsReadOnly().CalculateHashCode(),
            "Single bit difference in complex pattern should change hash"
        );
    }
}
