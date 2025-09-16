using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Unity.Collections.LowLevel.Unsafe;

[SuppressMessage("Design", "CA1716:Identifiers should not match keywords")]
[SuppressMessage("Naming", "CA1720:Identifier contains type name")]
public struct UnsafeBitArray
{
    [SuppressMessage("Design", "CA1716:Identifiers should not match keywords")]
    public struct ReadOnly
    {
        private readonly BitArray? _bits;
        public int Length => _bits?.Length ?? 0;

        public ReadOnly(BitArray? bits)
        {
            _bits = bits;
        }

        public bool SequenceEquals(ReadOnly other)
        {
            if (_bits == null && other._bits == null)
                return true;

            if (_bits == null || other._bits == null)
                return false;

            if (Length != other.Length)
                return false;

            for (int i = 0; i < Length; i++)
            {
                if (_bits[i] != other._bits[i])
                    return false;
            }

            return true;
        }
    }
}
