using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public class SimpleGraphLineModel : GraphLineModel
    {
        private String _visualStyleName;
        private float _minValue = 0;
        private float _maxValue = 100;
        private float[] _data;

        public SimpleGraphLineModel(string style, int size, float minValue, float maxValue)
        {
            this._visualStyleName = style;
            this._data = new float[size];
            this._minValue = minValue;
            this._maxValue = maxValue;
        }

        public string VisualStyleName
        {
            get
            {
                return this._visualStyleName;
            }

            set
            {
                this._visualStyleName = value;
            }
        }

        public int Points
        {
            get
            {
                return this._data.Length;
            }

            set
            {
                float[] newData = new float[value];
                int overlap = Math.Min(this._data.Length, value);
                Array.Copy(this._data, this._data.Length - overlap, newData, value - overlap, overlap);
                this._data = newData;
            }
        }

        public float MinValue
        {
            get
            {
                return this._minValue;
            }
            set
            {
                this._minValue = value;
            }
        }

        public float MaxValue
        {
            get
            {
                return this._maxValue;
            }
            set
            {
                this._maxValue = value;
            }
        }

        public float Point(int index)
        {
            return this._data[index];
        }

        public void AddPoint(float value)
        {
            Array.Copy(this._data, 1, this._data, 0, this._data.Length - 1);
            this._data[this._data.Length - 1] = value;
        }
    }
}
