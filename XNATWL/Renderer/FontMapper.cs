using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using XNATWL.IO;
using XNATWL.Utils;

namespace XNATWL.Renderer
{
    public interface FontMapper
    {
        /**
         * Retrive the cloest font for the given parameters
         * 
         * @param fontFamilies a list of family names with decreasing piority
         * @param fontSize the desired font size in pixels
         * @param style a combination of the STYLE_* flags
         * @param select the StateSelect object
         * @param fontParams the font parameters - must be exactly 1 more then
         *                   the number of expressions in the select object
         * @return the Font object or {@code null} if the font could not be found
         * @throws NullPointerException when one of the parameters is null
         * @throws IllegalArgumentException when the number of font parameters doesn't match the number of state expressions
         */
        Font GetFont(List<string> fontFamilies, int fontSize, int style, StateSelect select, params FontParameter[] fontParams);

        /**
         * Registers a font file
         * 
         * @param fontFamily the font family for which to register the font
         * @param style a combination of the STYLE_* and REGISTER_* flags
         * @param url the URL for the font file
         * @return true if the specified font could be registered
         */
        bool RegisterFont(String fontFamily, int style, FileSystemObject file);

        /**
         * Registers a font file and determines the style from the font itself.
         * 
         * @param fontFamily the font family for which to register the font
         * @param url the URL for the font file
         * @return true if the specified font could be registered
         * @throws IOException when the font could not be parsed 
         */
        bool RegisterFont(String fontFamily, FileSystemObject file);
    }
}
