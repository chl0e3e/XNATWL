using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Renderer
{
    public enum TextureRotation
    {
        NONE,
        CLOCKWISE_90,
        CLOCKWISE_180,
        CLOCKWISE_270
    }

    public interface Texture
    {
        int Width
        {
            get;
        }

        int Height
        {
            get;
        }


        Image GetImage(int x, int y, int width, int height, Color tintColor, bool tiled, TextureRotation rotation);

        MouseCursor CreateCursor(int x, int y, int width, int height, int hotSpotX, int hotSpotY, Image imageRef);

        /**
         * After calling this function getImage() and createCursor() may fail to work
         */
        void ThemeLoadingDone();


    }
}
