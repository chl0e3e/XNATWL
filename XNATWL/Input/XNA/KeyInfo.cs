using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Input.XNA
{
    /// <summary>
    /// A struct representing how a XNA `Keys` behaves in text inputs and TWL events
    /// </summary>
    public struct KeyInfo
    {
        /// <summary>
        /// The TWL event code for this key
        /// </summary>
        public int TWL;
        /// <summary>
        /// The character this KeyInfo represents for text input
        /// </summary>
        public char Char;
        /// <summary>
        /// The capitalised or shifted KeyInfo Char this struct represents for text input
        /// </summary>
        public char ShiftChar;
    }
}
