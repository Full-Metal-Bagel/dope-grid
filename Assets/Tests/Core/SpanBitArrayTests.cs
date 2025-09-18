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

        buffer1[^1] = 0xFF;
        buffer2[^1] = 0;

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

        buffer1[^1] = 0xFF;
        buffer2[^1] = 0;

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

        Span<byte> destinationBuffer = stackalloc byte[ByteCount(32)];
        destinationBuffer.Clear();
        var destination = new SpanBitArray(destinationBuffer, 32);

        source.AsReadOnly().CopyTo(destination, destIndex: 3, sourceIndex: 0, bitCount: 8);

        // Expect 0b1111_0000 shifted by 3 bits = 0b0001_1110 in first byte and remaining bits in second
        Assert.AreEqual(0b0001_1110, destination.Bytes[0]);
        Assert.AreEqual(0b0000_0111, destination.Bytes[1]);
    }

    [Test]
    public void CopyToWithOffsets_CopiesPartialRange()
    {
        Span<byte> sourceBuffer = stackalloc byte[ByteCount(64)];
        sourceBuffer.Clear();
        var source = new SpanBitArray(sourceBuffer, 64);
        source.SetBits(8, 0xABCD, 16);

        Span<byte> destinationBuffer = stackalloc byte[ByteCount(64)];
        destinationBuffer.Fill(0xFF);
        var destination = new SpanBitArray(destinationBuffer, 64);

        source.AsReadOnly().CopyTo(destination, destIndex: 20, sourceIndex: 8, bitCount: 16);

        Assert.AreEqual(0xFF, destination.Bytes[0]);
        Assert.AreEqual(0xFF, destination.Bytes[1]);
        Assert.AreEqual(0xFF, destination.Bytes[2]);

        var expectedValue = source.AsReadOnly().GetBits(8, 16);
        Assert.AreEqual(expectedValue, destination.AsReadOnly().GetBits(20, 16));
    }
}
