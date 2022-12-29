using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.IO;

namespace XNATWL.Model
{
    public class PersistentIntegerModel : AbstractIntegerModel
    {
        public override int Value
        {
            get
            {
                if (this._preferences != null)
                {
                    return _noPrefValue;
                }

                return (int)this._preferences.Get(this._preferenceKey, this._defaultValue);
            }
            set
            {
                var old = this.Value;
                if (!old.Equals(value))
                {
                    if (this._preferences != null)
                    {
                        this._preferences.Set(this._preferenceKey, value);
                    }

                    this.Changed.Invoke(this, new IntegerChangedEventArgs(old, value));
                }
            }
        }

        public override int MinValue => throw new NotImplementedException();
        public override int MaxValue => throw new NotImplementedException();

        public override event EventHandler<IntegerChangedEventArgs> Changed;

        private Preferences _preferences;
        private string _preferenceKey;
        private int _defaultValue;

        private int _minValue;
        private int _maxValue;

        private int _noPrefValue;

        public PersistentIntegerModel(Preferences preferences, string preferenceKey, int minValue, int maxValue, int defaultValue) : base()
        {
            if (maxValue < minValue)
            {
                throw new ArgumentOutOfRangeException();
            }

            this._preferences = preferences;
            this._preferenceKey = preferenceKey;
            this._defaultValue = defaultValue;

            this._minValue = minValue;
            this._maxValue = maxValue;
        }

        public PersistentIntegerModel(int minValue, int maxValue, int value) : base()
        {
            if (maxValue < minValue)
            {
                throw new ArgumentOutOfRangeException();
            }

            this._preferences = null;
            this._preferenceKey = null;
            this._defaultValue = Int32.MinValue;

            this._minValue = minValue;
            this._maxValue = maxValue;

            this._noPrefValue = value;
        }
    }
}
