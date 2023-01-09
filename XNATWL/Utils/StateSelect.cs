/*
 * Copyright (c) 2008-2012, Matthias Mann
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 *     * Redistributions of source code must retain the above copyright notice,
 *       this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of Matthias Mann nor the names of its contributors may
 *       be used to endorse or promote products derived from this software
 *       without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System.Collections.Generic;
using System.Linq;
using XNATWL.Renderer;

namespace XNATWL.Utils
{
    public class StateSelect
    {

        private static bool useOptimizer = false;

        private StateExpression[] expressions;
        private StateKey[] programKeys;
        private short[] programCodes;

        public static StateSelect EMPTY = new StateSelect();

        public StateSelect(ICollection<StateExpression> expressions) : this(expressions.ToArray())
        {
            
        }

        public StateSelect(params StateExpression[] expressions)
        {
            this.expressions = expressions;

            StateSelectOptimizer sso = useOptimizer
                    ? StateSelectOptimizer.Optimize(expressions)
                    : null;

            if (sso != null)
            {
                programKeys = sso.programKeys;
                programCodes = sso.programCodes;
            }
            else
            {
                programKeys = null;
                programCodes = null;
            }
        }

        public static bool IsUseOptimizer()
        {
            return useOptimizer;
        }

        /**
         * Controls the use of the StateSelectOptimizer.
         * 
         * @param useOptimizer true if the StateSelectOptimizer should be used
         */
        public static void SetUseOptimizer(bool useOptimizer)
        {
            StateSelect.useOptimizer = useOptimizer;
        }

        /**
         * Returns the number of expressions.
         * <p>This is also the return value of {@link #evaluate(de.matthiasmann.twl.renderer.AnimationState) }
         * when no expression matched.</p>
         * @return the number of expressions
         */
        public int Expressions()
        {
            return expressions.Length;
        }

        /**
         * Retrives the specified expression
         * @param idx the expression index
         * @return the expression
         * @see #getNumExpressions() 
         */
        public StateExpression ExpressionAt(int idx)
        {
            return expressions[idx];
        }

        /**
         * Evaluates the expression list.
         * 
         * @param as the animation stateor null
         * @return the index of the first matching expression or
         *         {@link #getNumExpressions()} when no expression matches
         */
        public int Evaluate(Renderer.AnimationState animationState)
        {
            if (programKeys != null)
            {
                return EvaluateProgram(animationState);
            }
            return EvaluateExpr(animationState);
        }

        private int EvaluateExpr(Renderer.AnimationState animationState)
        {
            int i = 0;
            for (int n = expressions.Length; i < n; i++)
            {
                if (expressions[i].Evaluate(animationState))
                {
                    break;
                }
            }
            return i;
        }

        private int EvaluateProgram(Renderer.AnimationState animationState)
        {
            int pos = 0;
            do
            {
                if (animationState == null || !animationState.GetAnimationState(programKeys[pos >> 1])) {
                    pos++;
                }
                pos = programCodes[pos];
            } while (pos >= 0);
            return pos & CODE_MASK;
        }

        public static int CODE_RESULT = 0x8000;
        public static int CODE_MASK = 0x7FFF;
    }
}
