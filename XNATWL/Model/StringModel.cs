using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public interface StringModel
    {
        event EventHandler<StringChangedEventArgs> Changed;

        string Value
        {
            get;
            set;
        }
    }

    public class StringChangedEventArgs : EventArgs
    {
        public string New;
        public string Old;

        public StringChangedEventArgs(string _old, string _new)
        {
            this.Old = _old;
            this.New = _new;
        }
    }
}
