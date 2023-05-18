/*
 * Copyright (c) 2008-2012, Matthias Mann
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 *     * Redistributions of source code must retain the above copyright notice,
 *       this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of Matthias Mann nor the names of its contributors may
 *       be used to endorse or promote products derived from this software
 *       without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using XNATWL.IO;
using XNATWL.Utils;

namespace XNATWL.Renderer
{
    /// <summary>
    /// A font mapper which tries to retrieve the closest font for the specified parameters
    /// </summary>
    public interface FontMapper
    {
        /// <summary>
        /// Retrieve the cloest font for the given parameters
        /// </summary>
        /// <param name="fontFamilies">a list of family names with decreasing priority</param>
        /// <param name="fontSize">the desired font size in pixels</param>
        /// <param name="style">a combination of the STYLE_* flags</param>
        /// <param name="select">the StateSelect object</param>
        /// <param name="fontParams">the font parameters - must be exactly 1 more than the number of expressions in the select object</param>
        /// <returns>the Font object or <em>null</em> if the font could not be found</returns>
        Font GetFont(List<string> fontFamilies, int fontSize, int style, StateSelect select, params FontParameter[] fontParams);

        /// <summary>
        /// Registers a font file
        /// </summary>
        /// <param name="fontFamily">the font family for which to register the font</param>
        /// <param name="style">a combination of the STYLE_* and REGISTER_* flags</param>
        /// <param name="file">the FSO for the font file</param>
        /// <returns><b>true</b> if the specified font could be registered</returns>
        bool RegisterFont(String fontFamily, int style, FileSystemObject file);

        /// <summary>
        /// Registers a font file and determines the style from the font itself.
        /// </summary>
        /// <param name="fontFamily">the font family for which to register the font</param>
        /// <param name="file">the FSO for the font file</param>
        /// <returns><b>true</b> if the specified font could be registered</returns>
        bool RegisterFont(String fontFamily, FileSystemObject file);
    }


    public class FontMapperStatics
    {
        public static int STYLE_NORMAL = 0;
        public static int STYLE_BOLD = 1;
        public static int STYLE_ITALIC = 2;
    }
}
