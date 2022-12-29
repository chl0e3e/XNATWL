using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public int Evaluate(AnimationState animationState)
        {
            if (programKeys != null)
            {
                return EvaluateProgram(animationState);
            }
            return EvaluateExpr(animationState);
        }

        private int EvaluateExpr(AnimationState animationState)
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

        private int EvaluateProgram(AnimationState animationState)
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
