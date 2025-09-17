using System;
using DopeGrid;
using NUnit.Framework;

[TestFixture]
public class SpanBitArrayTests
{
    [Test]
    public void Constructor_FromUlongSpan_SetsProperties()
    {
        Span<ulong> buffer = stackalloc ulong[2];
        var bits = new SpanBitArray(buffer, 128);

        Assert.AreEqual(128, bits.Length);
        Assert.AreEqual(2, bits.WordCount);
        Assert.AreEqual(128, bits.Capacity);
        Assert.IsFalse(bits.IsEmpty);
    }

    [Test]
    public void Constructor_FromUlongSpan_ThrowsWhenLengthTooLarge()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            Span<ulong> buffer = stackalloc ulong[1];
            _ = new SpanBitArray(buffer, 65);
        });
    }

    [Test]
    public void Constructor_FromByteSpan_Works()
    {
        Span<byte> bytes = stackalloc byte[16];
        var bits = new SpanBitArray(bytes, 128);
        Assert.AreEqual(128, bits.Length);
    }

    [Test]
    public void Constructor_FromByteSpan_ThrowsWhenMisaligned()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            Span<byte> bytes = stackalloc byte[15];
            _ = new SpanBitArray(bytes, 120);
        });
    }

    [Test]
    public void SetAndGet_BoundaryBits_Works()
    {
        Span<ulong> buffer = stackalloc ulong[2];
        buffer.Clear();
        var bits = new SpanBitArray(buffer, 128);

        bits.Set(0, true);
        bits.Set(bits.Length - 1, true);

        Assert.IsTrue(bits.Get(0));
        Assert.IsTrue(bits.Get(bits.Length - 1));
        Assert.IsFalse(bits.Get(1));
    }

    [Test]
    public void SetRange_CrossesMultipleWords()
    {
        Span<ulong> buffer = stackalloc ulong[3];
        buffer.Clear();
        var bits = new SpanBitArray(buffer, 160);

        bits.SetRange(30, 100, true);
        var readOnly = bits.AsReadOnly();

        Assert.IsTrue(readOnly.TestAll(30, 100));
        Assert.AreEqual(100, readOnly.CountBits(0, bits.Length));

        bits.SetRange(60, 20, false);
        readOnly = bits.AsReadOnly();
        Assert.AreEqual(80, readOnly.CountBits(0, bits.Length));
        Assert.IsFalse(readOnly.TestAll(30, 100));
    }

    [Test]
    public void SetAllAndClear_UpdateAllBits()
    {
        Span<ulong> buffer = stackalloc ulong[2];
        buffer.Clear();
        var bits = new SpanBitArray(buffer, 128);

        bits.SetAll(true);
        Assert.IsTrue(bits.AsReadOnly().TestAll(0, bits.Length));

        bits.Clear();
        Assert.IsFalse(bits.AsReadOnly().TestAny(0, bits.Length));
    }

    [Test]
    public void SetBits_HandlesFullWord()
    {
        Span<ulong> buffer = stackalloc ulong[1];
        buffer.Clear();
        var bits = new SpanBitArray(buffer, 64);

        const ulong pattern = 0xDEADBEEFCAFEBABE;
        bits.SetBits(0, pattern, 64);

        Assert.AreEqual(pattern, bits.GetBits(0, 64));
    }

    [Test]
    public void SetBits_ThrowsWhenBitCountOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            Span<ulong> buffer = stackalloc ulong[1];
            var bits = new SpanBitArray(buffer, 64);
            bits.SetBits(0, 0, 0);
        });
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            Span<ulong> buffer = stackalloc ulong[1];
            var bits = new SpanBitArray(buffer, 64);
            bits.SetBits(0, 0, 65);
        });
    }

    [Test]
    public void CopyFromByteSpan_ThrowsWhenSourceTooSmall()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            Span<ulong> buffer = stackalloc ulong[2];
            var bits = new SpanBitArray(buffer, 128);

            Span<byte> smallSource = stackalloc byte[10];
            bits.CopyFrom(smallSource);
        });
    }

    [Test]
    public void CopyFromReadOnlySpanBitArray_ThrowsWhenLengthMismatch()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            Span<ulong> sourceBuffer = stackalloc ulong[1];
            Span<ulong> destinationBuffer = stackalloc ulong[2];

            var source = new SpanBitArray(sourceBuffer, 64);
            var destination = new SpanBitArray(destinationBuffer, 128);
            destination.CopyFrom(source.AsReadOnly());
        });
    }

    [Test]
    public void CopyToSpanBitArray_CopiesExactly()
    {
        Span<ulong> sourceBuffer = stackalloc ulong[2];
        Span<ulong> destinationBuffer = stackalloc ulong[2];
        sourceBuffer.Clear();
        destinationBuffer.Clear();

        var source = new SpanBitArray(sourceBuffer, 128);
        source.SetRange(10, 5, true);
        source.Set(100, true);

        var destination = new SpanBitArray(destinationBuffer, 128);
        source.CopyTo(destination);

        var readOnly = destination.AsReadOnly();
        Assert.AreEqual(6, readOnly.CountBits(0, destination.Length));
        Assert.IsTrue(readOnly.TestAll(10, 5));
        Assert.IsTrue(readOnly.Get(100));
    }

    [Test]
    public void CopyToByteSpan_WritesTrimmedBytes()
    {
        Span<ulong> buffer = stackalloc ulong[2];
        buffer.Clear();
        var bits = new SpanBitArray(buffer, 70);
        bits.SetBits(0, 0xFFFF, 16);
        bits.Set(69, true);

        Span<byte> destination = stackalloc byte[16];
        destination.Clear();

        bits.CopyTo(destination);

        var expected = bits.AsReadOnly().AsTrimmedBytes();
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i], destination[i]);
        }

        for (var i = expected.Length; i < destination.Length; i++)
        {
            Assert.AreEqual(0, destination[i]);
        }
    }

    [Test]
    public void CopyToByteSpan_ThrowsWhenDestinationTooSmall()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            Span<byte> destination = stackalloc byte[5];
            Span<ulong> buffer = stackalloc ulong[2];
            var bits = new SpanBitArray(buffer, 128);
            bits.CopyTo(destination);
        });
    }
}

[TestFixture]
public class ReadOnlySpanBitArrayTests
{
    [Test]
    public void Constructor_FromUlongSpan_ThrowsWhenLengthTooLarge()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            ReadOnlySpan<ulong> buffer = stackalloc ulong[1];
            _ = new ReadOnlySpanBitArray(buffer, 65);
        });
    }

    [Test]
    public void Constructor_FromByteSpan_ThrowsWhenMisaligned()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            ReadOnlySpan<byte> bytes = stackalloc byte[15];
            _ = new ReadOnlySpanBitArray(bytes, 120);
        });
    }

    [Test]
    public void GetBits_CrossesWordBoundary_ReturnsCombinedValue()
    {
        Span<ulong> buffer = stackalloc ulong[2];
        buffer.Clear();
        var writable = new SpanBitArray(buffer, 128);
        writable.SetBits(60, 0b1_0000_0000_0001, 13);

        var readOnly = writable.AsReadOnly();
        var value = readOnly.GetBits(60, 13);
        Assert.AreEqual(0b1_0000_0000_0001UL, value);
    }

    [Test]
    public void TestAny_ReturnsFalseWhenRangeEmpty()
    {
        Span<ulong> buffer = stackalloc ulong[1];
        buffer.Clear();
        var readOnly = new SpanBitArray(buffer, 64).AsReadOnly();

        Assert.IsFalse(readOnly.TestAny(0, 64));
        Assert.IsFalse(readOnly.TestAny(10, 0));
    }

    [Test]
    public void TestAll_ReturnsTrueForZeroLength()
    {
        Span<ulong> buffer = stackalloc ulong[1];
        buffer.Clear();
        var readOnly = new SpanBitArray(buffer, 64).AsReadOnly();

        Assert.IsTrue(readOnly.TestAll(0, 0));
    }

    [Test]
    public void CountBits_ComputesAcrossWords()
    {
        Span<ulong> buffer = stackalloc ulong[2];
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
    public void CopyToByteSpan_ThrowsWhenDestinationTooSmall()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            Span<ulong> buffer = stackalloc ulong[2];
            var readOnly = new SpanBitArray(buffer, 128).AsReadOnly();

            Span<byte> destination = stackalloc byte[5];
            readOnly.CopyTo(destination);
        });
    }

    [Test]
    public void CopyToSpanBitArray_CopiesBits()
    {
        Span<ulong> sourceBuffer = stackalloc ulong[2];
        Span<ulong> destinationBuffer = stackalloc ulong[2];
        sourceBuffer.Clear();
        destinationBuffer.Clear();

        var writable = new SpanBitArray(sourceBuffer, 128);
        writable.SetBits(40, 0x1234, 16);
        writable.Set(90, true);

        var destination = new SpanBitArray(destinationBuffer, 128);
        writable.AsReadOnly().CopyTo(destination);

        Assert.AreEqual(0x34, destination.AsReadOnly().AsBytes()[5]);
        Assert.IsTrue(destination.Get(90));
    }

    [Test]
    public void CopyToSpanBitArray_ThrowsWhenLengthMismatch()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            Span<ulong> sourceBuffer = stackalloc ulong[2];
            Span<ulong> destinationBuffer = stackalloc ulong[1];

            var writable = new SpanBitArray(sourceBuffer, 128);
            var destination = new SpanBitArray(destinationBuffer, 64);

            writable.AsReadOnly().CopyTo(destination);
        });
    }
}
