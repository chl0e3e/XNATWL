using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public abstract class AbstractColorSpace : ColorSpace
    {
        private string _name;
        private string[] _componentNames;

        public AbstractColorSpace(string name, params string[] componentNames)
        {
            this._name = name;
            this._componentNames = componentNames;
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public int Components
        {
            get
            {
                return this._componentNames.Length;
            }
        }

        public string ComponentNameOf(int component)
        {
            return this._componentNames[component];
        }

        public float ComponentMinValueOf(int component)
        {
            return 0;
        }

        public abstract string ComponentShortNameOf(int component);

        public abstract float ComponentMaxValueOf(int component);

        public abstract float ComponentDefaultValueOf(int component);

        public abstract int RGB(float[] color);

        public abstract float[] FromRGB(int rgb);
    }
}
