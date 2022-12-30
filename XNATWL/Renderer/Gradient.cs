using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Renderer
{
    public enum GradientType
    {
        HORIZONTAL,
        VERTICAL
    }

    public enum GradientWrap
    {
        SCALE,
        CLAMP,
        REPEAT,
        MIRROR
    }

    public class Gradient
    {
        private GradientType _gradientType;
        private GradientWrap _gradientWrap;
        private List<GradientStop> _stops;

        public Gradient(GradientType type)
        {
            this._gradientType = type;
            this._gradientWrap = GradientWrap.SCALE;
            this._stops = new List<GradientStop>();
        }

        public GradientType Type
        {
            get
            {
                return this._gradientType;
            }
        }

        public GradientWrap Wrap
        {
            get
            {
                return this._gradientWrap;
            }

            set
            {
                this._gradientWrap = value;
            }
        }

        int Stops
        {
            get
            {
                return this._stops.Count;
            }
        }

        public GradientStop StopAt(int index)
        {
            return this._stops[index];
        }

        public GradientStop[] StopsAsArray
        {
            get
            {
                return this._stops.ToArray();
            }
        }

        public void AddStop(float pos, Color color)
        {
            int numStops = this.Stops;

            if (numStops == 0)
            {
                if (!(pos >= 0))
                {
                    throw new ArgumentOutOfRangeException("first stop must be >= 0.0f");
                }

                if (pos > 0)
                {
                    this._stops.Add(new GradientStop(0.0f, color));
                }
            }

            if (numStops > 0 && !(pos > this._stops[numStops - 1]._pos))
            {
                throw new ArgumentOutOfRangeException("pos must be monotone increasing");
            }

            this._stops.Add(new GradientStop(pos, color));
        }

    }

    public class GradientStop
    {
        internal float _pos;
        internal Color _color;

        public GradientStop(float pos, Color color)
        {
            this._pos = pos;
            this._color = color;
        }

        public float Pos
        {
            get
            {
                return _pos;
            }
        }

        public Color Color
        {
            get
            {
                return _color;
            }
        }
    }
}
