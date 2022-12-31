using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL
{
    public enum eAlignment
    {
        LEFT, CENTER, RIGHT, TOP, BOTTOM, TOPLEFT, TOPRIGHT, BOTTOMLEFT, BOTTOMRIGHT, FILL
    }

    public class Alignment
    {
        public static Alignment LEFT = new Alignment(HAlignment.LEFT, 0, 1);
        public static Alignment CENTER = new Alignment(HAlignment.CENTER, 1, 1);
        public static Alignment RIGHT = new Alignment(HAlignment.RIGHT, 2, 1);
        public static Alignment TOP = new Alignment(HAlignment.CENTER, 1, 0);
        public static Alignment BOTTOM = new Alignment(HAlignment.CENTER, 1, 2);
        public static Alignment TOPLEFT = new Alignment(HAlignment.LEFT, 0, 0);
        public static Alignment TOPRIGHT = new Alignment(HAlignment.RIGHT, 2, 0);
        public static Alignment BOTTOMLEFT = new Alignment(HAlignment.LEFT, 0, 2);
        public static Alignment BOTTOMRIGHT = new Alignment(HAlignment.RIGHT, 2, 2);
        public static Alignment FILL = new Alignment(HAlignment.CENTER,1,1);

        HAlignment fontHAlignment;
        byte hpos;
        byte vpos;

        private Alignment(HAlignment fontHAlignment, int hpos, int vpos)
        {
            this.fontHAlignment = fontHAlignment;
            this.hpos = (byte)hpos;
            this.vpos = (byte)vpos;
        }

        public HAlignment getFontHAlignment()
        {
            return fontHAlignment;
        }

        /**
         * Returns the horizontal position for this alignment.
         * @return 0 for left, 1 for center and 2 for right
         */
        public int getHPosition()
        {
            return hpos;
        }

        /**
         * Returns the vertical position for this alignment.
         * @return 0 for top, 1 for center and 2 for bottom
         */
        public int getVPosition()
        {
            return vpos;
        }


        public int computePositionX(int containerWidth, int objectWidth)
        {
            return Math.Max(0, containerWidth - objectWidth) * hpos / 2;
        }

        public int computePositionY(int containerHeight, int objectHeight)
        {
            return Math.Max(0, containerHeight - objectHeight) * vpos / 2;
        }
    }
}
