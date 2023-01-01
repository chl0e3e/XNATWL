using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Renderer
{
    public class TintStack
    {
        private static float ONE_OVER_255 = 1f;// / 255f;

        TintStack prev;
        TintStack next;
        float r, g, b, a;

        public TintStack()
        {
            this.prev = this;
            this.r = ONE_OVER_255;
            this.g = ONE_OVER_255;
            this.b = ONE_OVER_255;
            this.a = ONE_OVER_255;
        }

        private TintStack(TintStack prev)
        {
            this.prev = prev;
        }

        public TintStack pushReset()
        {
            if (next == null)
            {
                next = new TintStack(this);
            }
            next.r = ONE_OVER_255;
            next.g = ONE_OVER_255;
            next.b = ONE_OVER_255;
            next.a = ONE_OVER_255;
            return next;
        }

        public TintStack push(float r, float g, float b, float a)
        {
            if (next == null)
            {
                next = new TintStack(this);
            }
            next.r = this.r * r;
            next.g = this.g * g;
            next.b = this.b * b;
            next.a = this.a * a;
            return next;
        }

        public TintStack push(Color color)
        {
            return push(
                    color.RedF,
                    color.GreenF,
                    color.BlueF,
                    color.AlphaF) ;
        }

        public Microsoft.Xna.Framework.Color XNAColor
        {
            get
            {
                return new Microsoft.Xna.Framework.Color(this.r, this.g, this.b, this.a);
            }
        }

        public TintStack pop()
        {
            return prev;
        }

        public float getR()
        {
            return r;
        }

        public float getG()
        {
            return g;
        }

        public float getB()
        {
            return b;
        }

        public float getA()
        {
            return a;
        }
    }
}
