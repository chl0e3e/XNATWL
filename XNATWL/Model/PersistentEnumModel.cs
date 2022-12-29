using System;
using XNATWL.IO;

namespace XNATWL.Model
{
    public class PersistentEnumModel<T> : AbstractEnumModel<T> where T : struct, IConvertible
    {
        public override T Value
        {
            get
            {
                return (T)this._preferences.Get(this._preferenceKey, this._defaultValue);
            }
            set
            {
                var old = this.Value;
                if (!old.Equals(value))
                {
                    this._preferences.Set(this._preferenceKey, value);
                    this.Changed.Invoke(this, new EnumChangedEventArgs<T>(old, value));
                }
            }
        }

        private Preferences _preferences;
        private string _preferenceKey;
        private T _defaultValue;

        public Exception InitialError;

        public PersistentEnumModel(Preferences preferences, string preferenceKey, T defaultValue) : this(preferences, preferenceKey, defaultValue.GetType(), defaultValue)
        {
        }

        public PersistentEnumModel(Preferences preferences, string preferenceKey, Type type, T defaultValue) : base(type)
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

        public override event EventHandler<EnumChangedEventArgs<T>> Changed;
    }
}
