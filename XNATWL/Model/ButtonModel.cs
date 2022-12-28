using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public interface ButtonModel
    {
        bool Selected
        {
            get;
            set;
        }

        bool Pressed
        {
            get;
            set;
        }

        bool Armed
        {
            get;
            set;
        }

        bool Hover
        {
            get;
            set;
        }

        bool Enabled
        {
            get;
            set;
        }

        event EventHandler<ActionEventArgs> Action;
        event EventHandler<ButtonStateChangedEventArgs> State;
    }

    public class ButtonStateChangedEventArgs : EventArgs
    {
        public ButtonStateChangedEventArgs()
        {

        }
    }
}
