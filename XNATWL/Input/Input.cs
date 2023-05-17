using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Input
{
    /// <summary>
    /// Interface for implementing input polling
    /// </summary>
    public interface Input
    {
        /// <summary>
        /// Poll any input devices or APIs (mouse, keyboard, etc.)
        /// </summary>
        /// <param name="gui">TWL GUI to send input events</param>
        /// <returns></returns>
        bool PollInput(GUI gui);
    }
}
