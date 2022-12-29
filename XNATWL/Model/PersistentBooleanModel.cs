using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.IO;

namespace XNATWL.Model
{
    public class PersistentBooleanModel : BooleanModel
    {
        public bool Value
        {
            get
            {
                return (bool) this._preferences.Get(this._preferenceKey, this._defaultValue);
            }
            set
            {
                var old = this.Value;
                if (!old.Equals(value))
                {
                    this._preferences.Set(this._preferenceKey, value);
                    this.Changed.Invoke(this, new BooleanChangedEventArgs(old, value));
                }
            }
        }

        public event EventHandler<BooleanChangedEventArgs> Changed;

        private Preferences _preferences;
        private string _preferenceKey;
        private bool _defaultValue;

        public PersistentBooleanModel(Preferences preferences, string preferenceKey, bool defaultValue)
        {
            this._preferences = preferences;
            this._preferenceKey = preferenceKey;
            this._defaultValue = defaultValue;
        }
    }
}
