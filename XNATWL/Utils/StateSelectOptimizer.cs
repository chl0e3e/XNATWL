using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Renderer;
using XNATWL.Util;

namespace XNATWL.Utils
{
    public class StateSelectOptimizer
    {
        private StateKey[] keys;
        private byte[] matrix;

        internal StateKey[] programKeys;
        internal short[] programCodes;
        int programIdx;

        public static StateSelectOptimizer optimize(params StateExpression[] expressions)
        {
            int numExpr = expressions.Length;
            if (numExpr == 0 || numExpr >= 255)
            {
                return null;
            }

            BitSet bs = new BitSet();
            foreach (StateExpression e in expressions)
            {
                e.getUsedStateKeys(bs);
            }

            int numKeys = bs.Cardinality();
            if (numKeys == 0 || numKeys > 16)
            {
                return null;
            }

            StateKey[] keys = new StateKey[numKeys];
            for (int keyIdx = 0, keyID = -1; (keyID = bs.NextSetBit(keyID + 1)) >= 0; keyIdx++)
            {
                keys[keyIdx] = StateKey.Get(keyID);
            }

            int matrixSize = 1 << numKeys;
            byte[] matrix = new byte[matrixSize];
            AnimationState animationState = new AnimationState(null, keys[numKeys - 1].ID + 1);

            for (int matrixIdx = 0; matrixIdx < matrixSize; matrixIdx++)
            {
                for (int keyIdx = 0; keyIdx < numKeys; keyIdx++)
                {
                    animationState.setAnimationState(keys[keyIdx], (matrixIdx & (1 << keyIdx)) != 0);
                }
                int exprIdx = 0;
                for (; exprIdx < numExpr; exprIdx++)
                {
                    if (expressions[exprIdx].evaluate(animationState))
                    {
                        break;
                    }
                }
                matrix[matrixIdx] = (byte)exprIdx;
            }

            StateSelectOptimizer sso = new StateSelectOptimizer(keys, matrix);
            sso.compute(0, 0);
            return sso;
        }

        private StateSelectOptimizer(StateKey[] keys, byte[] matrix)
        {
            this.keys = keys;
            this.matrix = matrix;

            programKeys = new StateKey[matrix.Length - 1];
            programCodes = new short[matrix.Length * 2 - 2];
        }

        private int compute(int bits, int mask)
        {
            if (mask == matrix.Length - 1)
            {
                return (matrix[bits] & 255) | StateSelect.CODE_RESULT;
            }

            int best = -1;
            int bestScore = -1;
            int bestSet0 = 0;
            int bestSet1 = 0;


            int matrixIdxInc = (bits == 0) ? 1 : BitOperations.LowestOneBit(bits);

            for (int keyIdx = 0; keyIdx < keys.Length; keyIdx++)
            {
                int test = 1 << keyIdx;

                if ((mask & test) == 0)
                {
                    int set0 = 0;
                    int set1 = 0;

                    for (int matrixIdx = bits; matrixIdx < matrix.Length; matrixIdx += matrixIdxInc)
                    {
                        if ((matrixIdx & mask) == bits)
                        {
                            int resultMask = 1 << (matrix[matrixIdx] & 255);
                            if ((matrixIdx & test) == 0)
                            {
                                set0 |= resultMask;
                            }
                            else
                            {
                                set1 |= resultMask;
                            }
                        }
                    }

                    int score = BitOperations.BitCount((uint)(set0 ^ set1));
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestSet0 = set0;
                        bestSet1 = set1;
                        best = keyIdx;
                    }
                }
            }

            if (best < 0)
            {
                throw new Exception("Assertion failed");
            }

            if (bestSet0 == bestSet1 && (bestSet0 & (bestSet0 - 1)) == 0)
            {
                int result = BitOperations.NumberOfTrailingZeros(bestSet0);
                return result | StateSelect.CODE_RESULT;
            }

            int bestMask = 1 << best;
            mask |= bestMask;

            int idx = programIdx;
            programIdx += 2;
            programKeys[idx >> 1] = keys[best];
            programCodes[idx + 0] = (short)compute(bits | bestMask, mask);
            programCodes[idx + 1] = (short)compute(bits, mask);

            return idx;
        }
    }
}
