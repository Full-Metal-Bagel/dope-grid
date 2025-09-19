using System;
using DopeGrid;
using NUnit.Framework;

[TestFixture]
public class SpanBitArrayTests
{
    private static int ByteCount(int bitLength) => (bitLength + 7) / 8;

    [Test]
    public void Constructor_FromSpan_SetsProperties()
    {
        const int bitLength = 128;
        Span<byte> buffer = stackalloc byte[ByteCount(bitLength)];
        var bits = new SpanBitArray(buffer, bitLength);

        Assert.AreEqual(bitLength, bits.BitLength);
        Assert.AreEqual(ByteCount(bitLength), bits.Bytes.Length);
        Assert.IsFalse(bits.IsEmpty);
    }

    [Test]
    public void Constructor_FromSpan_ThrowsWhenLengthTooLarge()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            Span<byte> buffer = stackalloc byte[16];
            _ = new SpanBitArray(buffer, 129);
        });
    }

    [Test]
    public void SetAndGet_BoundaryBits_Works()
    {
        const int bitLength = 128;
        Span<byte> buffer = stackalloc byte[ByteCount(bitLength)];
        buffer.Clear();
        var bits = new SpanBitArray(buffer, bitLength);

        bits.Set(0, true);
        bits.Set(bitLength - 1, true);

        Assert.IsTrue(bits.Get(0));
        Assert.IsTrue(bits.Get(bitLength - 1));
        Assert.IsFalse(bits.Get(1));
    }

    [Test]
    public void SetRange_CrossesMultipleBytes()
    {
        const int bitLength = 160;
        Span<byte> buffer = stackalloc byte[ByteCount(bitLength)];
        buffer.Clear();
        var bits = new SpanBitArray(buffer, bitLength);

        bits.SetRange(30, 100, true);
        var readOnly = bits.AsReadOnly();

        Assert.IsTrue(readOnly.TestAll(30, 100));
        Assert.AreEqual(100, readOnly.CountBits(0, bitLength));

        bits.SetRange(60, 20, false);
        readOnly = bits.AsReadOnly();
        Assert.AreEqual(80, readOnly.CountBits(0, bitLength));
        Assert.IsFalse(readOnly.TestAll(30, 100));
    }

    [Test]
    public void SetAllAndClear_UpdateAllBits()
    {
        const int bitLength = 128;
        Span<byte> buffer = stackalloc byte[ByteCount(bitLength)];
        buffer.Clear();
        var bits = new SpanBitArray(buffer, bitLength);

        bits.SetAll(true);
        Assert.IsTrue(bits.AsReadOnly().TestAll(0, bitLength));

        bits.Clear();
        Assert.IsFalse(bits.AsReadOnly().TestAny(0, bitLength));
    }

    [Test]
    public void SetBits_HandlesFullRange()
    {
        const int bitLength = 64;
        Span<byte> buffer = stackalloc byte[ByteCount(bitLength)];
        buffer.Clear();
        var bits = new SpanBitArray(buffer, bitLength);

        const ulong pattern = 0xDEADBEEFCAFEBABE;
        bits.SetBits(0, pattern, 64);

        Assert.AreEqual(pattern, bits.GetBits(0, 64));
    }

    [Test]
    public void SetBits_ThrowsWhenBitCountOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            Span<byte> buffer = stackalloc byte[ByteCount(64)];
            var bits = new SpanBitArray(buffer, 64);

            bits.SetBits(0, 0, 0);
        });
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            Span<byte> buffer = stackalloc byte[ByteCount(64)];
            var bits = new SpanBitArray(buffer, 64);

            bits.SetBits(0, 0, 65);
        });
    }

    [Test]
    public void SequenceEqual_IgnoresTrailingBitsBeyondLength()
    {
        const int bitLength = 70;
        Span<byte> buffer1 = stackalloc byte[ByteCount(bitLength)];
        Span<byte> buffer2 = stackalloc byte[ByteCount(bitLength)];
        buffer1.Clear();
        buffer2.Clear();

        var bits1 = new SpanBitArray(buffer1, bitLength);
        var bits2 = new SpanBitArray(buffer2, bitLength);
        bits1.Set(5, true);
        bits2.Set(5, true);

        buffer1[^1] &= 0b0000_0011;
        Assert.IsTrue(bits1.SequenceEqual(bits2));

        bits2.Set(6, true);
        Assert.IsFalse(bits1.SequenceEqual(bits2));
    }

    [Test]
    public void SequenceEqual_DifferentLengths_ReturnsFalse()
    {
        Span<byte> buffer1 = stackalloc byte[ByteCount(32)];
        Span<byte> buffer2 = stackalloc byte[ByteCount(16)];
        var bits1 = new SpanBitArray(buffer1, 32);
        var bits2 = new SpanBitArray(buffer2, 16);

        Assert.IsFalse(bits1.SequenceEqual(bits2));
    }

    [Test]
    public void Inverse_SingleBit_FlipsBit()
    {
        const int bitLength = 64;
        Span<byte> buffer = stackalloc byte[ByteCount(bitLength)];
        buffer.Clear();
        var bits = new SpanBitArray(buffer, bitLength);

        bits.Inverse(0);
        Assert.IsTrue(bits.Get(0));

        bits.Inverse(0);
        Assert.IsFalse(bits.Get(0));

        bits.Inverse(63);
        Assert.IsTrue(bits.Get(63));

        bits.Inverse(63);
        Assert.IsFalse(bits.Get(63));
    }

    [Test]
    public void Inverse_MultipleBits_FlipsAllBits()
    {
        const int bitLength = 64;
        Span<byte> buffer = stackalloc byte[ByteCount(bitLength)];
        buffer.Clear();
        var bits = new SpanBitArray(buffer, bitLength);

        bits.SetRange(10, 10, true);
        Assert.AreEqual(10, bits.AsReadOnly().CountBits(0, bitLength));

        bits.Inverse(5, 20);

        Assert.IsTrue(bits.Get(5));
        Assert.IsTrue(bits.Get(6));
        Assert.IsTrue(bits.Get(7));
        Assert.IsTrue(bits.Get(8));
        Assert.IsTrue(bits.Get(9));
        Assert.IsFalse(bits.Get(10));
        Assert.IsFalse(bits.Get(11));
        Assert.IsFalse(bits.Get(19));
        Assert.IsTrue(bits.Get(20));
        Assert.IsTrue(bits.Get(24));
        Assert.IsFalse(bits.Get(25));
    }

    [Test]
    public void Inverse_CrossesByteBoundary()
    {
        const int bitLength = 128;
        Span<byte> buffer = stackalloc byte[ByteCount(bitLength)];
        buffer.Clear();
        var bits = new SpanBitArray(buffer, bitLength);

        bits.Inverse(6, 12);

        // Check bits before the inverted range (should be false)
        for (int i = 0; i < 6; i++)
        {
            Assert.IsFalse(bits.Get(i), $"Bit {i} should be false");
        }

        // Check bits in the inverted range (should be true)
        for (int i = 6; i < 18; i++)
        {
            Assert.IsTrue(bits.Get(i), $"Bit {i} should be true");
        }

        // Check bits after the inverted range (should be false)
        for (int i = 18; i < 24; i++)
        {
            Assert.IsFalse(bits.Get(i), $"Bit {i} should be false");
        }
    }

    [Test]
    public void Inverse_EntireRange_FlipsAllBits()
    {
        const int bitLength = 32;
        Span<byte> buffer = stackalloc byte[ByteCount(bitLength)];
        buffer.Clear();
        var bits = new SpanBitArray(buffer, bitLength);

        bits.SetBits(0, 0x55555555, 32);

        bits.Inverse(0, 32);

        Assert.AreEqual(0xAAAAAAAA, bits.GetBits(0, 32));
    }

    [Test]
    public void Inverse_ZeroBitCount_DoesNothing()
    {
        const int bitLength = 64;
        Span<byte> buffer = stackalloc byte[ByteCount(bitLength)];
        buffer.Fill(0x55);
        var bits = new SpanBitArray(buffer, bitLength);

        var originalValue = bits.GetBits(0, 32);
        bits.Inverse(10, 0);

        Assert.AreEqual(originalValue, bits.GetBits(0, 32));
    }

    [Test]
    public void Inverse_LargeBitRange_HandlesCorrectly()
    {
        const int bitLength = 256;
        Span<byte> buffer = stackalloc byte[ByteCount(bitLength)];
        buffer.Clear();
        var bits = new SpanBitArray(buffer, bitLength);

        bits.Inverse(50, 150);

        Assert.AreEqual(0, bits.AsReadOnly().CountBits(0, 50));
        Assert.AreEqual(150, bits.AsReadOnly().CountBits(50, 150));
        Assert.AreEqual(0, bits.AsReadOnly().CountBits(200, 56));

        bits.Inverse(50, 150);
        Assert.AreEqual(0, bits.AsReadOnly().CountBits(0, bitLength));
    }

    [Test]
    public void Inverse_NonAlignedBits_WorksCorrectly()
    {
        const int bitLength = 128;
        Span<byte> buffer = stackalloc byte[ByteCount(bitLength)];
        buffer.Clear();
        var bits = new SpanBitArray(buffer, bitLength);

        bits.SetRange(0, bitLength, true);

        bits.Inverse(13, 47);

        Assert.IsTrue(bits.AsReadOnly().TestAll(0, 13));
        Assert.IsFalse(bits.AsReadOnly().TestAny(13, 47));
        Assert.IsTrue(bits.AsReadOnly().TestAll(60, bitLength - 60));
    }

    [Test]
    public void Inverse_NoParameters_InvertsAllBits()
    {
        const int bitLength = 64;
        Span<byte> buffer = stackalloc byte[ByteCount(bitLength)];
        buffer.Clear();
        var bits = new SpanBitArray(buffer, bitLength);

        bits.SetBits(0, 0x0F0F0F0F0F0F0F0F, 64);

        bits.Inverse();

        Assert.AreEqual(0xF0F0F0F0F0F0F0F0, bits.GetBits(0, 64));
    }
}

[TestFixture]
public class ReadOnlySpanBitArrayTests
{
    private static int ByteCount(int bitLength) => (bitLength + 7) / 8;

    [Test]
    public void Constructor_FromSpan_ThrowsWhenLengthTooLarge()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            ReadOnlySpan<byte> buffer = stackalloc byte[16];
            _ = new ReadOnlySpanBitArray(buffer, 129);
        });
    }

    [Test]
    public void GetBits_CrossesByteBoundary_ReturnsCombinedValue()
    {
        Span<byte> writableBytes = stackalloc byte[ByteCount(128)];
        writableBytes.Clear();
        var writable = new SpanBitArray(writableBytes, 128);
        writable.SetBits(60, 0b1_0000_0000_0001, 13);

        var readOnly = writable.AsReadOnly();
        var value = readOnly.GetBits(60, 13);
        Assert.AreEqual(0b1_0000_0000_0001UL, value);
    }

    [Test]
    public void TestAny_ReturnsFalseWhenRangeEmpty()
    {
        Span<byte> buffer = stackalloc byte[ByteCount(64)];
        buffer.Clear();
        var readOnly = new SpanBitArray(buffer, 64).AsReadOnly();

        Assert.IsFalse(readOnly.TestAny(0, 64));
        Assert.IsFalse(readOnly.TestAny(10, 0));
    }

    [Test]
    public void TestAll_ReturnsTrueForZeroLength()
    {
        Span<byte> buffer = stackalloc byte[ByteCount(64)];
        buffer.Clear();
        var readOnly = new SpanBitArray(buffer, 64).AsReadOnly();

        Assert.IsTrue(readOnly.TestAll(0, 0));
    }

    [Test]
    public void CountBits_ComputesAcrossBytes()
    {
        Span<byte> buffer = stackalloc byte[ByteCount(128)];
        buffer.Clear();
        var writable = new SpanBitArray(buffer, 128);
        writable.Set(0, true);
        writable.Set(63, true);
        writable.Set(64, true);
        writable.Set(127, true);

        var readOnly = writable.AsReadOnly();
        Assert.AreEqual(4, readOnly.CountBits(0, 128));
        Assert.AreEqual(2, readOnly.CountBits(0, 64));
        Assert.AreEqual(2, readOnly.CountBits(64, 64));
    }

    [Test]
    public void SequenceEqual_IgnoresTrailingBitsBeyondLength()
    {
        const int bitLength = 70;
        Span<byte> buffer1 = stackalloc byte[ByteCount(bitLength)];
        Span<byte> buffer2 = stackalloc byte[ByteCount(bitLength)];
        buffer1.Clear();
        buffer2.Clear();

        var writable1 = new SpanBitArray(buffer1, bitLength);
        var writable2 = new SpanBitArray(buffer2, bitLength);
        writable1.Set(5, true);
        writable2.Set(5, true);

        buffer1[^1] &= 0b0000_0011;

        var readOnly1 = writable1.AsReadOnly();
        var readOnly2 = writable2.AsReadOnly();
        Assert.IsTrue(readOnly1.SequenceEqual(readOnly2));

        writable2.Set(6, true);
        readOnly2 = writable2.AsReadOnly();
        Assert.IsFalse(readOnly1.SequenceEqual(readOnly2));
    }

    [Test]
    public void SequenceEqual_DifferentLengths_ReturnsFalse()
    {
        Span<byte> buffer1 = stackalloc byte[ByteCount(32)];
        Span<byte> buffer2 = stackalloc byte[ByteCount(16)];
        var readOnly1 = new SpanBitArray(buffer1, 32).AsReadOnly();
        var readOnly2 = new SpanBitArray(buffer2, 16).AsReadOnly();

        Assert.IsFalse(readOnly1.SequenceEqual(readOnly2));
    }

    [Test]
    public void CopyToAlignedSpanBitArray_CopiesExactBits()
    {
        Span<byte> sourceBuffer = stackalloc byte[ByteCount(64)];
        sourceBuffer.Clear();
        var source = new SpanBitArray(sourceBuffer, 64);
        source.SetBits(0, 0b1010_1100, 8);
        source.SetBits(16, 0xFFFF, 16);

        Span<byte> destinationBuffer = stackalloc byte[ByteCount(64)];
        destinationBuffer.Fill(0x55);
        var destination = new SpanBitArray(destinationBuffer, 64);

        source.AsReadOnly().CopyTo(destination);

        Assert.AreEqual(0b1010_1100, destination.Bytes[0]);
        Assert.AreEqual(0xFF, destination.Bytes[2]);
        Assert.AreEqual(0xFF, destination.Bytes[3]);
        for (var i = 4; i < destination.Bytes.Length; i++)
        {
            Assert.AreEqual(0x00, destination.Bytes[i]);
        }
    }

    [Test]
    public void CopyToUnalignedDest_ShiftsCorrectly()
    {
        Span<byte> sourceBuffer = stackalloc byte[ByteCount(32)];
        sourceBuffer.Clear();
        var source = new SpanBitArray(sourceBuffer, 32);
        source.SetBits(0, 0b1111_0000, 8);

        // Verify source bits after SetBits(0, 0b1111_0000, 8)
        // Value 240 (0b1111_0000) means bits 4-7 are set, bits 0-3 are clear
        Assert.IsFalse(source.Get(0));
        Assert.IsFalse(source.Get(1));
        Assert.IsFalse(source.Get(2));
        Assert.IsFalse(source.Get(3));
        Assert.IsTrue(source.Get(4));
        Assert.IsTrue(source.Get(5));
        Assert.IsTrue(source.Get(6));
        Assert.IsTrue(source.Get(7));

        Span<byte> destinationBuffer = stackalloc byte[ByteCount(32)];
        destinationBuffer.Clear();
        var destination = new SpanBitArray(destinationBuffer, 32);

        source.AsReadOnly().CopyTo(destination, destIndex: 3, sourceIndex: 0, bitCount: 8);

        // When SetBits(0, 0b1111_0000, 8) is called with value 240:
        // - Source bits 0-3 = 0000 (lower nibble of 240)
        // - Source bits 4-7 = 1111 (upper nibble of 240)
        // After copying to destIndex 3, verify each bit:
        var destReadOnly = destination.AsReadOnly();

        // Destination bits 0-2 should remain 0 (untouched)
        Assert.IsFalse(destReadOnly.Get(0));
        Assert.IsFalse(destReadOnly.Get(1));
        Assert.IsFalse(destReadOnly.Get(2));

        // Destination bits 3-6 should be 0000 (from source bits 0-3)
        Assert.IsFalse(destReadOnly.Get(3));
        Assert.IsFalse(destReadOnly.Get(4));
        Assert.IsFalse(destReadOnly.Get(5));
        Assert.IsFalse(destReadOnly.Get(6));

        // Destination bits 7-10 should be 1111 (from source bits 4-7)
        Assert.IsTrue(destReadOnly.Get(7));
        Assert.IsTrue(destReadOnly.Get(8));
        Assert.IsTrue(destReadOnly.Get(9));
        Assert.IsTrue(destReadOnly.Get(10));

        // Destination bits 11+ should remain 0 (untouched)
        Assert.IsFalse(destReadOnly.Get(11));
    }

    [Test]
    public void CopyToWithOffsets_CopiesPartialRange()
    {
        Span<byte> sourceBuffer = stackalloc byte[ByteCount(64)];
        sourceBuffer.Clear();
        var source = new SpanBitArray(sourceBuffer, 64);
        source.SetBits(8, 0b1010_1011_1100_1101, 16);

        // Verify source bits after SetBits(8, 0b1010_1011_1100_1101, 16)
        // Value 0xABCD = 43981 in binary from LSB to MSB:
        // Bits 8-15:  1011 0011 (0xCD reversed)
        // Bits 16-23: 1101 0101 (0xAB reversed)
        Assert.IsTrue(source.Get(8));   // bit 0 of 0xCD = 1
        Assert.IsFalse(source.Get(9));  // bit 1 of 0xCD = 0
        Assert.IsTrue(source.Get(10));  // bit 2 of 0xCD = 1
        Assert.IsTrue(source.Get(11));  // bit 3 of 0xCD = 1
        Assert.IsFalse(source.Get(12)); // bit 4 of 0xCD = 0
        Assert.IsFalse(source.Get(13)); // bit 5 of 0xCD = 0
        Assert.IsTrue(source.Get(14));  // bit 6 of 0xCD = 1
        Assert.IsTrue(source.Get(15));  // bit 7 of 0xCD = 1
        Assert.IsTrue(source.Get(16));  // bit 0 of 0xAB = 1
        Assert.IsTrue(source.Get(17));  // bit 1 of 0xAB = 1
        Assert.IsFalse(source.Get(18)); // bit 2 of 0xAB = 0
        Assert.IsTrue(source.Get(19));  // bit 3 of 0xAB = 1
        Assert.IsFalse(source.Get(20)); // bit 4 of 0xAB = 0
        Assert.IsTrue(source.Get(21));  // bit 5 of 0xAB = 1
        Assert.IsFalse(source.Get(22)); // bit 6 of 0xAB = 0
        Assert.IsTrue(source.Get(23));  // bit 7 of 0xAB = 1

        Span<byte> destinationBuffer = stackalloc byte[ByteCount(64)];
        destinationBuffer.Fill(0xFF);
        var destination = new SpanBitArray(destinationBuffer, 64);

        source.AsReadOnly().CopyTo(destination, destIndex: 20, sourceIndex: 8, bitCount: 16);

        // Verify destination bits 0-19 remain all 1s (untouched)
        for (int i = 0; i < 20; i++)
        {
            Assert.IsTrue(destination.Get(i), $"Bit {i} should remain 1");
        }

        // Verify destination bits 20-35 match source bits 8-23
        for (int i = 0; i < 16; i++)
        {
            Assert.AreEqual(source.Get(8 + i), destination.Get(20 + i),
                $"Destination bit {20 + i} should match source bit {8 + i}");
        }

        // Verify destination bits 36+ remain all 1s (untouched)
        for (int i = 36; i < 40; i++)
        {
            Assert.IsTrue(destination.Get(i), $"Bit {i} should remain 1");
        }

        var expectedValue = source.AsReadOnly().GetBits(8, 16);
        Assert.AreEqual(expectedValue, destination.AsReadOnly().GetBits(20, 16));
    }
}
