using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.IO;

namespace XNATWL.Model
{
    public class PersistentStringModel : StringModel
    {
        public string Value
        {
            get
            {
                return (string) this._preferences.Get(this._preferenceKey, this._defaultValue);
            }
            set
            {
                var old = this.Value;
                if (!old.Equals(value))
                {
                    this._preferences.Set(this._preferenceKey, value);
                    this.Changed.Invoke(this, new StringChangedEventArgs(old, value));
                }
            }
        }

        public event EventHandler<StringChangedEventArgs> Changed;

        private Preferences _preferences;
        private string _preferenceKey;
        private string _defaultValue;

        public PersistentStringModel(Preferences preferences, string preferenceKey, string defaultValue)
        {
            this._preferences = preferences;
            this._preferenceKey = preferenceKey;
            this._defaultValue = defaultValue;
        }
    }
}
