using DopeGrid.Native;
using NUnit.Framework;
using Unity.Collections;

[TestFixture]
public class NativeBitArrayExtensionsTests
{
    [Test]
    public void Equals_IdenticalArrays_ReturnsTrue()
    {
        using var array1 = new NativeBitArray(64, Allocator.Temp);
        using var array2 = new NativeBitArray(64, Allocator.Temp);

        array1.Set(0, true);
        array1.Set(15, true);
        array1.Set(31, true);
        array1.Set(63, true);

        array2.Set(0, true);
        array2.Set(15, true);
        array2.Set(31, true);
        array2.Set(63, true);

        Assert.IsTrue(array1.SequenceEquals(array2));
        Assert.IsTrue(array2.SequenceEquals(array1));
    }

    [Test]
    public void Equals_DifferentBits_ReturnsFalse()
    {
        using var array1 = new NativeBitArray(32, Allocator.Temp);
        using var array2 = new NativeBitArray(32, Allocator.Temp);

        array1.Set(0, true);
        array1.Set(10, true);

        array2.Set(0, true);
        array2.Set(11, true);

        Assert.IsFalse(array1.SequenceEquals(array2));
        Assert.IsFalse(array2.SequenceEquals(array1));
    }

    [Test]
    public void Equals_DifferentLengths_ReturnsFalse()
    {
        using var array1 = new NativeBitArray(32, Allocator.Temp);
        using var array2 = new NativeBitArray(64, Allocator.Temp);

        array1.Set(0, true);
        array2.Set(0, true);

        Assert.IsFalse(array1.SequenceEquals(array2));
        Assert.IsFalse(array2.SequenceEquals(array1));
    }

    [Test]
    public void Equals_EmptyArrays_ReturnsTrue()
    {
        using var array1 = new NativeBitArray(128, Allocator.Temp);
        using var array2 = new NativeBitArray(128, Allocator.Temp);

        Assert.IsTrue(array1.SequenceEquals(array2));
    }

    [Test]
    public void Equals_OneEmptyOneNot_ReturnsFalse()
    {
        using var array1 = new NativeBitArray(16, Allocator.Temp);
        using var array2 = new NativeBitArray(16, Allocator.Temp);

        array1.Set(5, true);

        Assert.IsFalse(array1.SequenceEquals(array2));
        Assert.IsFalse(array2.SequenceEquals(array1));
    }

    [Test]
    public void Equals_NotCreatedArrays_ReturnsTrue()
    {
        var array1 = new NativeBitArray();
        var array2 = new NativeBitArray();

        Assert.IsTrue(array1.SequenceEquals(array2));
    }

    [Test]
    public void Equals_OneCreatedOneNot_ReturnsFalse()
    {
        using var array1 = new NativeBitArray(32, Allocator.Temp);
        var array2 = new NativeBitArray();

        Assert.IsFalse(array1.SequenceEquals(array2));
        Assert.IsFalse(array2.SequenceEquals(array1));
    }

    [Test]
    public void Equals_AllBitsSet_ReturnsTrue()
    {
        using var array1 = new NativeBitArray(16, Allocator.Temp);
        using var array2 = new NativeBitArray(16, Allocator.Temp);

        for (var i = 0; i < 16; i++)
        {
            array1.Set(i, true);
            array2.Set(i, true);
        }

        Assert.IsTrue(array1.SequenceEquals(array2));
    }

    [Test]
    public void Equals_AlternatingPattern_ReturnsTrue()
    {
        using var array1 = new NativeBitArray(32, Allocator.Temp);
        using var array2 = new NativeBitArray(32, Allocator.Temp);

        for (var i = 0; i < 32; i++)
        {
            var value = i % 2 == 0;
            array1.Set(i, value);
            array2.Set(i, value);
        }

        Assert.IsTrue(array1.SequenceEquals(array2));
    }

    [Test]
    public void Equals_LargeArrays_WorksCorrectly()
    {
        using var array1 = new NativeBitArray(1024, Allocator.Temp);
        using var array2 = new NativeBitArray(1024, Allocator.Temp);

        // Set some bits in a pattern
        for (var i = 0; i < 1024; i += 7)
        {
            array1.Set(i, true);
            array2.Set(i, true);
        }

        Assert.IsTrue(array1.SequenceEquals(array2));

        // Change one bit
        array1.Set(500, !array1.IsSet(500));

        Assert.IsFalse(array1.SequenceEquals(array2));
    }

    [Test]
    public void SequenceEquals_TailBitsHandling_NonAligned()
    {
        // Test various non-64-bit-aligned sizes to ensure tail bits are handled correctly
        TestSequenceEqualsWithSize(1);   // 1 bit (63 bits tail)
        TestSequenceEqualsWithSize(7);   // 7 bits (57 bits tail)
        TestSequenceEqualsWithSize(31);  // 31 bits (33 bits tail)
        TestSequenceEqualsWithSize(63);  // 63 bits (1 bit tail)
        TestSequenceEqualsWithSize(65);  // 65 bits (1 complete ulong + 1 bit tail)
        TestSequenceEqualsWithSize(127); // 127 bits (1 complete ulong + 63 bits tail)
        TestSequenceEqualsWithSize(129); // 129 bits (2 complete ulongs + 1 bit tail)
        TestSequenceEqualsWithSize(200); // 200 bits (3 complete ulongs + 8 bits tail)
    }

    private void TestSequenceEqualsWithSize(int size)
    {
        using var array1 = new NativeBitArray(size, Allocator.Temp);
        using var array2 = new NativeBitArray(size, Allocator.Temp);

        // Test 1: Both empty should be equal
        Assert.IsTrue(array1.SequenceEquals(array2), $"Empty arrays of size {size} should be equal");

        // Test 2: Set identical patterns
        for (var i = 0; i < size; i += 3)
        {
            array1.Set(i, true);
            array2.Set(i, true);
        }
        Assert.IsTrue(array1.SequenceEquals(array2), $"Arrays with identical pattern (size {size}) should be equal");

        // Test 3: Modify last bit to test tail handling
        if (size > 0)
        {
            var lastBitIndex = size - 1;
            array1.Set(lastBitIndex, !array1.IsSet(lastBitIndex));
            Assert.IsFalse(array1.SequenceEquals(array2), $"Arrays should differ when last bit is different (size {size})");

            // Restore for next test
            array1.Set(lastBitIndex, array2.IsSet(lastBitIndex));
        }

        // Test 4: Modify first bit of last ulong block
        if (size > 64)
        {
            var firstBitOfLastBlock = (size / 64) * 64;
            // Only modify if it's within bounds (for cases where size is exact multiple of 64)
            if (firstBitOfLastBlock < size)
            {
                array1.Set(firstBitOfLastBlock, !array1.IsSet(firstBitOfLastBlock));
                Assert.IsFalse(array1.SequenceEquals(array2), $"Arrays should differ when first bit of last block differs (size {size})");
            }
            else if (size > 64)
            {
                // For exact multiples, test the first bit of the actual last block
                var actualLastBlockStart = ((size - 1) / 64) * 64;
                array1.Set(actualLastBlockStart, !array1.IsSet(actualLastBlockStart));
                Assert.IsFalse(array1.SequenceEquals(array2), $"Arrays should differ when first bit of last block differs (size {size})");
            }
        }
    }

    [Test]
    public void SequenceEquals_EdgeCases_64BitBoundaries()
    {
        // Test exactly at 64-bit boundaries
        TestSequenceEqualsWithSize(64);   // Exactly 1 ulong
        TestSequenceEqualsWithSize(128);  // Exactly 2 ulongs
        TestSequenceEqualsWithSize(192);  // Exactly 3 ulongs
        TestSequenceEqualsWithSize(256);  // Exactly 4 ulongs
        TestSequenceEqualsWithSize(512);  // Exactly 8 ulongs
        TestSequenceEqualsWithSize(1024); // Exactly 16 ulongs
    }

    [Test]
    public void SequenceEquals_BitsAtBoundaries()
    {
        using var array1 = new NativeBitArray(130, Allocator.Temp);
        using var array2 = new NativeBitArray(130, Allocator.Temp);

        // Set bits at critical boundaries
        array1.Set(0, true);     // First bit of first block
        array1.Set(63, true);    // Last bit of first block
        array1.Set(64, true);    // First bit of second block
        array1.Set(127, true);   // Last bit of second block
        array1.Set(128, true);   // First bit of third (partial) block
        array1.Set(129, true);   // Last bit (in partial block)

        array2.Set(0, true);
        array2.Set(63, true);
        array2.Set(64, true);
        array2.Set(127, true);
        array2.Set(128, true);
        array2.Set(129, true);

        Assert.IsTrue(array1.SequenceEquals(array2), "Arrays with bits at boundaries should be equal");

        // Modify bit at boundary between blocks
        array2.Set(64, false);
        Assert.IsFalse(array1.SequenceEquals(array2), "Arrays should differ when boundary bit differs");

        // Restore and modify last bit in partial block
        array2.Set(64, true);
        array2.Set(129, false);
        Assert.IsFalse(array1.SequenceEquals(array2), "Arrays should differ when last bit in partial block differs");
    }

    [Test]
    public void SequenceEquals_ReadOnly_Variants()
    {
        using var array1 = new NativeBitArray(100, Allocator.Temp);
        using var array2 = new NativeBitArray(100, Allocator.Temp);

        // Set same pattern
        for (var i = 0; i < 100; i += 5)
        {
            array1.Set(i, true);
            array2.Set(i, true);
        }

        // Test NativeBitArray.ReadOnly
        var readOnly1 = array1.AsReadOnly();
        var readOnly2 = array2.AsReadOnly();
        Assert.IsTrue(readOnly1.SequenceEquals(readOnly2), "ReadOnly variants should be equal");

        // Test UnsafeBitArray.ReadOnly
        var unsafe1 = array1.GetReadOnlyUnsafeBitArray();
        var unsafe2 = array2.GetReadOnlyUnsafeBitArray();
        Assert.IsTrue(unsafe1.SequenceEquals(unsafe2), "UnsafeBitArray.ReadOnly variants should be equal");
    }

    [Test]
    public void SequenceEquals_SamePointer_ReturnsTrue()
    {
        using var array = new NativeBitArray(100, Allocator.Temp);
        array.Set(10, true);
        array.Set(50, true);

        var unsafe1 = array.GetReadOnlyUnsafeBitArray();
        var unsafe2 = array.GetReadOnlyUnsafeBitArray();

        // Same pointer should return true immediately
        Assert.IsTrue(unsafe1.SequenceEquals(unsafe2), "Same pointer should return true");
    }

    [Test]
    public void SequenceEquals_NullPointers()
    {
        var array1 = new NativeBitArray(); // Not created
        var array2 = new NativeBitArray(); // Not created

        Assert.IsTrue(array1.SequenceEquals(array2), "Two non-created arrays should be equal");

        using var array3 = new NativeBitArray(10, Allocator.Temp);
        Assert.IsFalse(array1.SequenceEquals(array3), "Non-created and created arrays should not be equal");
        Assert.IsFalse(array3.SequenceEquals(array1), "Created and non-created arrays should not be equal");
    }

    [Test]
    public void SequenceEquals_AllPossibleTailMasks()
    {
        // Test all possible tail sizes (1-63 bits) with various bit patterns
        for (var tailSize = 1; tailSize < 64; tailSize++)
        {
            var totalSize = 64 + tailSize; // One complete ulong + tail
            using var array1 = new NativeBitArray(totalSize, Allocator.Temp);
            using var array2 = new NativeBitArray(totalSize, Allocator.Temp);

            // Set all bits in the tail portion
            for (var i = 64; i < totalSize; i++)
            {
                array1.Set(i, true);
                array2.Set(i, true);
            }

            Assert.IsTrue(array1.SequenceEquals(array2), $"Arrays with all tail bits set (tail size {tailSize}) should be equal");

            // Toggle one bit in the tail
            array1.Set(64 + tailSize / 2, false);
            Assert.IsFalse(array1.SequenceEquals(array2), $"Arrays should differ when tail bit differs (tail size {tailSize})");
        }
    }

    [Test]
    public void SequenceEquals_RandomPatterns_StressTest()
    {
        var random = new System.Random(12345);

        for (var test = 0; test < 100; test++)
        {
            var size = random.Next(1, 1000);
            using var array1 = new NativeBitArray(size, Allocator.Temp);
            using var array2 = new NativeBitArray(size, Allocator.Temp);
            using var array3 = new NativeBitArray(size, Allocator.Temp);

            // Create random pattern
            for (var i = 0; i < size; i++)
            {
                var value = random.Next(2) == 1;
                array1.Set(i, value);
                array2.Set(i, value);
                // array3 gets different pattern
                array3.Set(i, random.Next(2) == 1);
            }

            Assert.IsTrue(array1.SequenceEquals(array2), $"Identical random patterns should be equal (test {test}, size {size})");

            // array3 has high probability of being different
            if (size > 10) // For larger sizes, almost certainly different
            {
                var isDifferent = false;
                for (var i = 0; i < size; i++)
                {
                    if (array1.IsSet(i) != array3.IsSet(i))
                    {
                        isDifferent = true;
                        break;
                    }
                }

                if (isDifferent)
                {
                    Assert.IsFalse(array1.SequenceEquals(array3), $"Different random patterns should not be equal (test {test}, size {size})");
                }
            }

            // Test single bit difference
            if (size > 0)
            {
                var bitToFlip = random.Next(size);
                array2.Set(bitToFlip, !array2.IsSet(bitToFlip));
                Assert.IsFalse(array1.SequenceEquals(array2), $"Single bit difference should be detected (test {test}, size {size}, bit {bitToFlip})");
            }
        }
    }

    [Test]
    public void SequenceEquals_MemCmpCorrectness_FullBlocks()
    {
        // Ensure MemCmp comparison works correctly for full blocks
        using var array1 = new NativeBitArray(256, Allocator.Temp); // Exactly 4 ulongs
        using var array2 = new NativeBitArray(256, Allocator.Temp);

        // Set a complex pattern
        for (var i = 0; i < 256; i++)
        {
            // Create a pattern that tests all bytes
            var value = ((i % 7) == 0) || ((i % 11) == 0);
            array1.Set(i, value);
            array2.Set(i, value);
        }

        Assert.IsTrue(array1.SequenceEquals(array2), "Complex pattern should match");

        // Change one bit in the middle
        array1.Set(150, !array1.IsSet(150));
        Assert.IsFalse(array1.SequenceEquals(array2), "MemCmp should detect single bit difference in middle");

        // Restore and change first bit
        array1.Set(150, array2.IsSet(150));
        array1.Set(0, !array1.IsSet(0));
        Assert.IsFalse(array1.SequenceEquals(array2), "MemCmp should detect first bit difference");

        // Restore and change last bit of a complete block
        array1.Set(0, array2.IsSet(0));
        array1.Set(255, !array1.IsSet(255));
        Assert.IsFalse(array1.SequenceEquals(array2), "MemCmp should detect last bit difference");
    }

    [Test]
    public void SequenceEquals_ZeroLengthArrays()
    {
        using var array1 = new NativeBitArray(0, Allocator.Temp);
        using var array2 = new NativeBitArray(0, Allocator.Temp);

        Assert.IsTrue(array1.SequenceEquals(array2), "Zero-length arrays should be equal");

        using var array3 = new NativeBitArray(1, Allocator.Temp);
        Assert.IsFalse(array1.SequenceEquals(array3), "Zero-length and non-zero-length arrays should not be equal");
    }
}
