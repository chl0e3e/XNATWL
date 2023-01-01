using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Renderer
{
    public interface Font : Resource
    {
        bool Proportional
        {
            get;
        }

        int BaseLine
        {
            get;
        }

        int LineHeight
        {
            get;
        }

        int SpaceWidth
        {
            get;
        }

        /// <summary>
        ///  the width of the letter 'M'
        /// </summary>
        int MWidth
        {
            get;
        }

        /// <summary>
        ///  the width of the letter 'x'
        /// </summary>
        int xWidth
        {
            get;
        }

        int ComputeMultiLineTextWidth(string str);

        int ComputeTextWidth(string str);

        int ComputeTextWidth(string str, int start, int end);

        int ComputeVisibleGlyphs(string str, int start, int end, int width);

        int DrawMultiLineText(AnimationState animState, int x, int y, string str, int width, HAlignment alignment);

        int DrawText(AnimationState animState, int x, int y, string str);

        int DrawText(AnimationState animState, int x, int y, string str, int start, int end);

        FontCache CacheMultiLineText(FontCache prevCache, string str, int width, HAlignment alignment);

        FontCache CacheText(FontCache prevCache, string str);

        FontCache CacheText(FontCache prevCache, string str, int start, int end);
    }
}
