/*
 * BitSet.cs — an original, independent implementation of a growable vector of bits.
 *
 * This file is NOT derived from the GNU Classpath java.util.BitSet port that previously lived here.
 * It was written from scratch against the public java.util.BitSet API contract (the method names,
 * signatures and documented behaviour that the rest of XNATWL depends on) using only standard,
 * non-creative bit-manipulation algorithms (SWAR population count, binary-search trailing-zero
 * count, word-array storage). No GPL / GNU Classpath source was consulted while writing it.
 *
 * Provided under the same terms as the rest of the XNATWL project; it carries no GPL obligation.
 */

using System;

// NOTE: the original GPL port declared this type in the "XNATWL.Util" namespace (singular, not
// matching the Utils folder). Kept identical so this remains a drop-in replacement for its callers.
namespace XNATWL.Util
{
    /// <summary>
    /// A growable array of bits, addressed by non-negative integer index. Mirrors the behaviour of
    /// Java's <c>java.util.BitSet</c> for the subset of the API used by XNATWL. Bits not explicitly
    /// set are false; the set grows automatically as higher indices are written.
    /// </summary>
    public class BitSet
    {
        // Storage: bit i lives in word (i >> 6) at bit (i & 63). Length is always >= 1.
        private long[] _words;

        /// <summary>Create an empty bit set.</summary>
        public BitSet()
        {
            _words = new long[1];
        }

        /// <summary>Create an empty bit set sized to comfortably hold <paramref name="nbits"/> bits.</summary>
        public BitSet(int nbits)
        {
            if (nbits < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(nbits));
            }
            int words = (nbits == 0) ? 1 : (((nbits - 1) >> 6) + 1);
            _words = new long[words];
        }

        private void EnsureCapacity(int wordsNeeded)
        {
            if (_words.Length < wordsNeeded)
            {
                Array.Resize(ref _words, Math.Max(2 * _words.Length, wordsNeeded));
            }
        }

        private static void CheckRange(int from, int to)
        {
            if (from < 0 || to < 0 || from > to)
            {
                throw new ArgumentOutOfRangeException("from=" + from + ", to=" + to);
            }
        }

        // Standard SWAR 64-bit population count (Hacker's Delight / well-known public algorithm).
        private static int PopCount(ulong x)
        {
            x -= (x >> 1) & 0x5555555555555555UL;
            x = (x & 0x3333333333333333UL) + ((x >> 2) & 0x3333333333333333UL);
            x = (x + (x >> 4)) & 0x0f0f0f0f0f0f0f0fUL;
            return (int)((x * 0x0101010101010101UL) >> 56);
        }

        // Standard binary-search count of trailing zero bits. Returns 64 for zero.
        private static int TrailingZeroCount(ulong x)
        {
            if (x == 0)
            {
                return 64;
            }
            int n = 0;
            if ((x & 0xFFFFFFFFUL) == 0) { n += 32; x >>= 32; }
            if ((x & 0x0000FFFFUL) == 0) { n += 16; x >>= 16; }
            if ((x & 0x000000FFUL) == 0) { n += 8; x >>= 8; }
            if ((x & 0x0000000FUL) == 0) { n += 4; x >>= 4; }
            if ((x & 0x00000003UL) == 0) { n += 2; x >>= 2; }
            if ((x & 0x00000001UL) == 0) { n += 1; }
            return n;
        }

        /// <summary>Set the bit at <paramref name="pos"/> to true.</summary>
        public void Set(int pos)
        {
            if (pos < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pos));
            }
            int w = pos >> 6;
            EnsureCapacity(w + 1);
            _words[w] |= (1L << pos);
        }

        /// <summary>Set the bit at <paramref name="index"/> to <paramref name="value"/>.</summary>
        public void Set(int index, bool value)
        {
            if (value)
            {
                Set(index);
            }
            else
            {
                Clear(index);
            }
        }

        /// <summary>Set the bits in the half-open range [from, to) to true.</summary>
        public void Set(int from, int to)
        {
            CheckRange(from, to);
            if (from == to)
            {
                return;
            }
            EnsureCapacity(((to - 1) >> 6) + 1);
            for (int i = from; i < to; i++)
            {
                _words[i >> 6] |= (1L << i);
            }
        }

        /// <summary>Set the bits in the half-open range [from, to) to <paramref name="value"/>.</summary>
        public void Set(int from, int to, bool value)
        {
            if (value)
            {
                Set(from, to);
            }
            else
            {
                Clear(from, to);
            }
        }

        /// <summary>Clear the bit at <paramref name="pos"/> (set it to false).</summary>
        public void Clear(int pos)
        {
            if (pos < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pos));
            }
            int w = pos >> 6;
            if (w < _words.Length)
            {
                _words[w] &= ~(1L << pos);
            }
        }

        /// <summary>Clear the bits in the half-open range [from, to).</summary>
        public void Clear(int from, int to)
        {
            CheckRange(from, to);
            for (int i = from; i < to; i++)
            {
                int w = i >> 6;
                if (w < _words.Length)
                {
                    _words[w] &= ~(1L << i);
                }
            }
        }

        /// <summary>Clear all bits.</summary>
        public void Clear()
        {
            Array.Clear(_words, 0, _words.Length);
        }

        /// <summary>Flip (toggle) the bit at <paramref name="index"/>.</summary>
        public void Flip(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            int w = index >> 6;
            EnsureCapacity(w + 1);
            _words[w] ^= (1L << index);
        }

        /// <summary>Flip (toggle) the bits in the half-open range [from, to).</summary>
        public void Flip(int from, int to)
        {
            CheckRange(from, to);
            if (from == to)
            {
                return;
            }
            EnsureCapacity(((to - 1) >> 6) + 1);
            for (int i = from; i < to; i++)
            {
                _words[i >> 6] ^= (1L << i);
            }
        }

        /// <summary>Return the value of the bit at <paramref name="pos"/>.</summary>
        public Boolean Get(int pos)
        {
            if (pos < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pos));
            }
            int w = pos >> 6;
            return w < _words.Length && ((_words[w] >> pos) & 1L) != 0;
        }

        /// <summary>Return a new bit set holding the bits of the half-open range [from, to), re-based to 0.</summary>
        public BitSet Get(int from, int to)
        {
            CheckRange(from, to);
            BitSet result = new BitSet(Math.Max(1, to - from));
            for (int i = from; i < to; i++)
            {
                if (Get(i))
                {
                    result.Set(i - from);
                }
            }
            return result;
        }

        /// <summary>The number of bits set to true.</summary>
        public int Cardinality()
        {
            int sum = 0;
            for (int i = 0; i < _words.Length; i++)
            {
                sum += PopCount((ulong)_words[i]);
            }
            return sum;
        }

        /// <summary><strong>true</strong> if no bit is set.</summary>
        public bool IsEmpty()
        {
            for (int i = 0; i < _words.Length; i++)
            {
                if (_words[i] != 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>Index of the first set bit at or after <paramref name="from"/>, or -1 if none.</summary>
        public int NextSetBit(int from)
        {
            if (from < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(from));
            }
            int u = from >> 6;
            if (u >= _words.Length)
            {
                return -1;
            }
            // Mask off the bits below 'from' within the starting word.
            ulong word = (ulong)_words[u] & (ulong.MaxValue << from);
            while (true)
            {
                if (word != 0)
                {
                    return (u << 6) + TrailingZeroCount(word);
                }
                if (++u >= _words.Length)
                {
                    return -1;
                }
                word = (ulong)_words[u];
            }
        }

        /// <summary>Index of the first clear bit at or after <paramref name="from"/> (always exists).</summary>
        public int NextClearBit(int from)
        {
            if (from < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(from));
            }
            int u = from >> 6;
            if (u >= _words.Length)
            {
                return from;
            }
            ulong word = ~(ulong)_words[u] & (ulong.MaxValue << from);
            while (true)
            {
                if (word != 0)
                {
                    return (u << 6) + TrailingZeroCount(word);
                }
                if (++u >= _words.Length)
                {
                    return _words.Length << 6;
                }
                word = ~(ulong)_words[u];
            }
        }

        /// <summary>In-place logical AND with <paramref name="bs"/>.</summary>
        public void And(BitSet bs)
        {
            int common = Math.Min(_words.Length, bs._words.Length);
            for (int i = 0; i < common; i++)
            {
                _words[i] &= bs._words[i];
            }
            for (int i = common; i < _words.Length; i++)
            {
                _words[i] = 0;
            }
        }

        /// <summary>In-place clear of every bit that is set in <paramref name="bs"/>.</summary>
        public void AndNot(BitSet bs)
        {
            int common = Math.Min(_words.Length, bs._words.Length);
            for (int i = 0; i < common; i++)
            {
                _words[i] &= ~bs._words[i];
            }
        }

        /// <summary>In-place logical OR with <paramref name="bs"/>.</summary>
        public void Or(BitSet bs)
        {
            EnsureCapacity(bs._words.Length);
            for (int i = 0; i < bs._words.Length; i++)
            {
                _words[i] |= bs._words[i];
            }
        }

        /// <summary>In-place logical XOR with <paramref name="bs"/>.</summary>
        public void XOr(BitSet bs)
        {
            EnsureCapacity(bs._words.Length);
            for (int i = 0; i < bs._words.Length; i++)
            {
                _words[i] ^= bs._words[i];
            }
        }

        /// <summary><strong>true</strong> if this set has any bit in common with <paramref name="set"/>.</summary>
        public bool Intersects(BitSet set)
        {
            int common = Math.Min(_words.Length, set._words.Length);
            for (int i = 0; i < common; i++)
            {
                if ((_words[i] & set._words[i]) != 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary><strong>true</strong> if every bit set in <paramref name="other"/> is also set here.</summary>
        public bool ContainsAll(BitSet other)
        {
            for (int i = 0; i < other._words.Length; i++)
            {
                long word = (i < _words.Length) ? _words[i] : 0L;
                if ((word & other._words[i]) != other._words[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>Return a deep copy of this bit set.</summary>
        public object Clone()
        {
            BitSet copy = new BitSet();
            copy._words = (long[])_words.Clone();
            return copy;
        }
    }
}
