using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public class EnumListModel<T> : SimpleListModel<T> where T : struct, IConvertible
    {
        private Type _class;
        private T[] _values;

        public override event EventHandler<ListSubsetChangedEventArgs> EntriesInserted;
        public override event EventHandler<ListSubsetChangedEventArgs> EntriesDeleted;
        public override event EventHandler<ListSubsetChangedEventArgs> EntriesChanged;
        public override event EventHandler<ListAllChangedEventArgs> AllChanged;

        public EnumListModel(Type enumType)
        {
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("Type supplied is not an enum");
            }

            this._class = enumType;
            this._values = Enum.GetValues(enumType).Cast<T>().ToArray();
        }

        public override T EntryAt(int index)
        {
            return this._values[index];
        }

        public override int Entries
        {
            get
            {
                return this._values.Length;
            }
        }

        public int FindEntry(T value)
        {
            for (int i = 0, n = this._values.Length; i < n; i++)
            {
                if (this._values[i].Equals(value))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
