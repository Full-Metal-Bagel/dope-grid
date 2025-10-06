using System;
using DopeGrid;
using NUnit.Framework;

namespace DopeGrid.Tests;

[TestFixture]
public class UtilityTests
{
    // SpanBitArrayUtility tests - only testing public methods
    [Test]
    public void SpanBitArrayUtility_ByteCount_CalculatesCorrectly()
    {
        Assert.That(SpanBitArrayUtility.ByteCount(0), Is.EqualTo(0));
        Assert.That(SpanBitArrayUtility.ByteCount(1), Is.EqualTo(1));
        Assert.That(SpanBitArrayUtility.ByteCount(8), Is.EqualTo(1));
        Assert.That(SpanBitArrayUtility.ByteCount(9), Is.EqualTo(2));
        Assert.That(SpanBitArrayUtility.ByteCount(16), Is.EqualTo(2));
        Assert.That(SpanBitArrayUtility.ByteCount(17), Is.EqualTo(3));
    }

    // RotationDegree CalculateRotatedSize tests
    [Test]
    public void RotationDegree_CalculateRotatedSize_WorksCorrectly()
    {
        var (w1, h1) = RotationDegree.None.CalculateRotatedSize(3, 5);
        Assert.That(w1, Is.EqualTo(3));
        Assert.That(h1, Is.EqualTo(5));

        var (w2, h2) = RotationDegree.Clockwise90.CalculateRotatedSize(3, 5);
        Assert.That(w2, Is.EqualTo(5));
        Assert.That(h2, Is.EqualTo(3));

        var (w3, h3) = RotationDegree.Clockwise180.CalculateRotatedSize(3, 5);
        Assert.That(w3, Is.EqualTo(3));
        Assert.That(h3, Is.EqualTo(5));

        var (w4, h4) = RotationDegree.Clockwise270.CalculateRotatedSize(3, 5);
        Assert.That(w4, Is.EqualTo(5));
        Assert.That(h4, Is.EqualTo(3));
    }
}
