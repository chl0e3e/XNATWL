using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public abstract class AutoCompletionResult
    {
        public static int DEFAULT_CURSOR_POS = -1;

        public readonly string Text;
        public readonly int PrefixLength;

        public abstract int Results
        {
            get;
        }

        public AutoCompletionResult(string text, int prefixLength)
        {
            Text = text;
            PrefixLength = prefixLength;
        }

        public abstract string ResultAt(int index);

        public virtual int getCursorPosForResult(int idx)
        {
            return DEFAULT_CURSOR_POS;
        }

        public virtual AutoCompletionResult refine(String text, int cursorPos)
        {
            return null;
        }
    }
}
