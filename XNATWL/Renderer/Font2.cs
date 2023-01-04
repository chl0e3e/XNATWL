using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Renderer
{
    public interface Font2 : Font
    {
        int DrawText(Color color, int x, int y, AttributedString attributedString);

        int DrawText(Color color, int x, int y, AttributedString attributedString, int start, int end);

        void DrawMultiLineText(Color color, int x, int y, AttributedString attributedString);

        void DrawMultiLineText(Color color, int x, int y, AttributedString attributedString, int start, int end);

        AttributedStringFontCache CacheText(Color color, AttributedStringFontCache prevCache, AttributedString attributedString);

        AttributedStringFontCache CacheText(Color color, AttributedStringFontCache prevCache, AttributedString attributedString, int start, int end);

        AttributedStringFontCache CacheMultiLineText(Color color, AttributedStringFontCache prevCache, AttributedString attributedString);

        AttributedStringFontCache CacheMultiLineText(Color color, AttributedStringFontCache prevCache, AttributedString attributedString, int start, int end);
    }
}
