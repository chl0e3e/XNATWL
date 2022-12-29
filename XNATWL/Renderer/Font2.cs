using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Renderer
{
    public interface Font2 : Font
    {
        int DrawText(int x, int y, AttributedString attributedString);

        int DrawText(int x, int y, AttributedString attributedString, int start, int end);

        void DrawMultiLineText(int x, int y, AttributedString attributedString);

        void DrawMultiLineText(int x, int y, AttributedString attributedString, int start, int end);

        AttributedStringFontCache CacheText(AttributedStringFontCache prevCache, AttributedString attributedString);

        AttributedStringFontCache CacheText(AttributedStringFontCache prevCache, AttributedString attributedString, int start, int end);

        AttributedStringFontCache CacheMultiLineText(AttributedStringFontCache prevCache, AttributedString attributedString);

        AttributedStringFontCache CacheMultiLineText(AttributedStringFontCache prevCache, AttributedString attributedString, int start, int end);
    }
}
