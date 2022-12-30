using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Utils
{
    internal class ParseException : Exception
    {
        public ParseException(string message, int position) : base(message + "\nAt pos: " + position.ToString())
        {

        }

        public ParseException(string message, int position, Exception other) : base(message + "\nAt pos: " + position.ToString(), other)
        {
        }
    }
}
