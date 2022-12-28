using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public interface EditFieldModel : ObservableCharSequence
    {
        int Replace(int start, int count, string replacement);

        bool Replace(int start, int count, char replacement);

        string Substring(int start, int end);
    }
}
