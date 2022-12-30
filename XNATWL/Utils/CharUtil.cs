using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Utils
{
    public class CharUtil
    {
        public static CodeDomProvider PROVIDER = CodeDomProvider.CreateProvider("C#");

        public static bool IsCSharpIdentifier(char c)
        {
            return CharUtil.PROVIDER.IsValidIdentifier(c.ToString());
        }

        public static bool IsDigit(char c)
        {
            return "0123456789".Contains(c);
        }

        public static bool IsWhitespace(char c)
        {
            return ' ' == c;
        }
    }
}
