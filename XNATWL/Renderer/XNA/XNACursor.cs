using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Renderer.XNA
{
    public class XNACursor : TextureAreaBase, MouseCursor
    {
        public XNACursor(XNATexture texture, int x, int y, int width, int height, Color tintColor) : base(texture, x, y, width, height)
        {

        }
    }
}
