using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.IO;

namespace XNATWL.Model
{
    public class PersistentMRUListModel<T> : SimpleMRUModel<T>
    {
        private Preferences _preferences;
        private string _preferenceKey;
        private Type _type;

        public PersistentMRUListModel(int maxEntries, Type type, Preferences preferences, string preferenceKey) : base(maxEntries)
        {
            this._preferences = preferences;
            this._preferenceKey = preferenceKey;
            this._type = type;

            int numEntries = Math.Min((int)this._preferences.Get(keyForNumEntries(), 0), maxEntries);

            for (int i = 0; i < numEntries; i++)
            {
                object entry = this._preferences.Get(keyForIndex(i), null);

                if (entry != null)
                {
                    this._entries.Add((T) entry);
                }
            }
        }

        protected override void Save()
        {
            int numEntries = Math.Min((int)this._preferences.Get(keyForNumEntries(), 0), this._maxEntries);

            for (int i = 0; i < numEntries; i++)
            {
                object entry = this._preferences.Get(keyForIndex(i), null);

                if (entry != null && !entry.Equals(this._entries[i]))
                {
                    this._preferences.Set(keyForIndex(i), entry);
                }
            }

            this._preferences.Set(keyForNumEntries(), this.Entries);
        }

        protected string keyForIndex(int idx)
        {
            return this._preferenceKey + "_" + idx;
        }

        protected string keyForNumEntries()
        {
            return this._preferenceKey + "_entries";
        }
    }
}
