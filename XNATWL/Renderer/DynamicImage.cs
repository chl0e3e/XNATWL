using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Renderer
{
    public interface DynamicImage : Image, Resource
    {
        void Update(Microsoft.Xna.Framework.Color[] data);
        //void Update(Microsoft.Xna.Framework.Color[] data, DynamicImageFormat format);
        //void Update(Microsoft.Xna.Framework.Color[] data, int stride, DynamicImageFormat format);
        //void Update(int xoffset, int yoffset, int width, int height, byte[] data, DynamicImageFormat format);
        //void Update(int xoffset, int yoffset, int width, int height, byte[] data, int stride, DynamicImageFormat format);
    }

    public enum DynamicImageFormat
    {
        RGBA,
        BGRA
    }
}
