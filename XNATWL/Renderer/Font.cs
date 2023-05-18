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

namespace XNATWL.Renderer
{
    /// <summary>
    /// A font rendering interface
    /// </summary>
    public interface Font : Resource
    {
        /// <summary>
        /// <strong>true</strong> if the font is proportional or <strong>false</strong> if it's fixed width.
        /// </summary>
        bool Proportional
        {
            get;
        }

        /// <summary>
        /// The base line of the font measured in pixels from the top of the text bounding box
        /// </summary>
        int BaseLine
        {
            get;
        }

        /// <summary>
        /// The line height in pixels for this font
        /// </summary>
        int LineHeight
        {
            get;
        }

        /// <summary>
        /// Returns the width of a ' '
        /// </summary>
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

        /// <summary>
        /// Computes the width in pixels of the longest text line. Lines are splitted at '\n'
        /// </summary>
        /// <param name="str">the text to evaluate</param>
        /// <returns>the width in pixels of the longest line</returns>
        int ComputeMultiLineTextWidth(string str);

        /// <summary>
        /// Computes the width in pixels of a text
        /// </summary>
        /// <param name="str">the text to evaluate</param>
        /// <returns>the width in pixels</returns>
        int ComputeTextWidth(string str);

        /// <summary>
        /// Computes the width in pixels of a text
        /// </summary>
        /// <param name="str">the text to evaluate</param>
        /// <param name="start">index of first character in str</param>
        /// <param name="end">index after last character in str</param>
        /// <returns>the width in pixels</returns>
        int ComputeTextWidth(string str, int start, int end);

        /// <summary>
        /// Computes how many glyphs of the supplied CharSequence can be display completely in the given amount of pixels.
        /// </summary>
        /// <param name="str">the CharSequence</param>
        /// <param name="start">the start index in the CharSequence</param>
        /// <param name="end">the end index (exclusive) in the CharSequence</param>
        /// <param name="width">the number of available pixels.</param>
        /// <returns>the number (relative to start) of fitting glyphs</returns>
        int ComputeVisibleGlyphs(string str, int start, int end, int width);

        /// <summary>
        /// Draws multi line text - lines are splitted at '\n'
        /// </summary>
        /// <param name="animState">A time source for animation - may be null</param>
        /// <param name="x">left coordinate of the text block </param>
        /// <param name="y">top coordinate of the text block</param>
        /// <param name="str">the text to draw</param>
        /// <param name="width">the width of the text block</param>
        /// <param name="alignment">horizontal alignment for shorter lines</param>
        /// <returns>the height in pixels of the multi line text</returns>
        int DrawMultiLineText(AnimationState animState, int x, int y, string str, int width, HAlignment alignment);

        /// <summary>
        /// Draws a single line text
        /// </summary>
        /// <param name="animState">A time source for animation - may be null</param>
        /// <param name="x">left coordinate of the text block</param>
        /// <param name="y">top coordinate of the text block</param>
        /// <param name="str">the text to draw</param>
        /// <returns>the width in pixels of the text</returns>
        int DrawText(AnimationState animState, int x, int y, string str);

        /// <summary>
        /// Draws a single line text
        /// </summary>
        /// <param name="animState">A time source for animation - may be null</param>
        /// <param name="x">left coordinate of the text block</param>
        /// <param name="y">top coordinate of the text block</param>
        /// <param name="str">the text to draw</param>
        /// <param name="start">index of first character to draw in str</param>
        /// <param name="end">index after last character to draw in str</param>
        /// <returns>the width in pixels of the text</returns>
        int DrawText(AnimationState animState, int x, int y, string str, int start, int end);

        /// <summary>
        /// Caches a text for faster drawing
        /// </summary>
        /// <param name="prevCache">the previous cached text or null</param>
        /// <param name="str">the text to cache</param>
        /// <param name="width">the width of the text block</param>
        /// <param name="alignment">horizontal alignment for shorter lines</param>
        /// <returns>A cache object or null if caching was not possible</returns>
        FontCache CacheMultiLineText(FontCache prevCache, string str, int width, HAlignment alignment);

        /// <summary>
        /// Caches a text for faster drawing
        /// </summary>
        /// <param name="prevCache">the previous cached text or null</param>
        /// <param name="str">the text to cache</param>
        /// <returns>A cache object or null if caching was not possible</returns>
        FontCache CacheText(FontCache prevCache, string str);

        /// <summary>
        /// Caches a text for faster drawing
        /// </summary>
        /// <param name="prevCache">the previous cached text or null</param>
        /// <param name="str">the text to cache</param>
        /// <param name="start">index of first character to draw in str</param>
        /// <param name="end">index after last character to draw in str</param>
        /// <returns>A cache object or null if caching was not possible</returns>
        FontCache CacheText(FontCache prevCache, string str, int start, int end);
    }
}
