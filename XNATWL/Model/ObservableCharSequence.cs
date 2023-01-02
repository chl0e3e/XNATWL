using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public interface ObservableCharSequence : XNATWL.Utils.CharSequence
    {
        event EventHandler<CharSequenceChangedEventArgs> CharSequenceChanged;

        string Value
        {
            get;
        }
    }

    public class CharSequenceChangedEventArgs : EventArgs
    {
        public int Start;
        public int OldCount;
        public int NewCount;

        public CharSequenceChangedEventArgs(int start, int oldCount, int newCount)
        {
            this.Start = start;
            this.OldCount = oldCount;
            this.NewCount = newCount;
        }
    }
}
