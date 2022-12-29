using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.IO;

namespace XNATWL.Model
{
    public class PersistentColorModel : ColorModel
    {
        public Color Value
        {
            get
            {
                return (Color) this._preferences.Get(this._preferenceKey, this._defaultValue);
            }
            set
            {
                var old = this.Value;
                if (!old.Equals(value))
                {
                    this._preferences.Set(this._preferenceKey, value);
                    this.Changed.Invoke(this, new ColorChangedEventArgs(old, value));
                }
            }
        }

        public event EventHandler<ColorChangedEventArgs> Changed;

        private Preferences _preferences;
        private string _preferenceKey;
        private Color _defaultValue;

        public Exception InitialError;

        public PersistentColorModel(Preferences preferences, string preferenceKey, Color defaultValue)
        {
            this._preferences = preferences;
            this._preferenceKey = preferenceKey;
            this._defaultValue = defaultValue;

            try
            {
                this.Value = this.Value;
            }
            catch (Exception e)
            {
                this.InitialError = e;
            }
        }
    }
}
