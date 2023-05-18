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
using XNATWL.IO;
using XNATWL.Utils;

namespace XNATWL.Renderer
{
    /// <summary>
    /// Interface outlining what an implementation of a TWL renderer requires
    /// </summary>
    public interface Renderer
    {
        /// <summary>
        /// The elapsed time in milliseconds.
        /// </summary>
        long TimeMillis
        {
            get;
        }

        /// <summary>
        /// The polled input object for this renderer target.
        /// </summary>
        Input.Input Input
        {
            get;
        }

        /// <summary>
        /// Setup rendering for TWL.
        /// <para>Must be called before any Font or Image objects is drawn</para>
        /// <para>When this method returned <Strong>true</Strong> then <see cref="EndRendering"/> must be called</para>
        /// </summary>
        /// <returns><strong>true</strong> if rendering was started, <strong>false</strong> otherwise</returns>
        bool StartRendering();

        /// <summary>
        /// Clean up after rendering TWL. Only call this method when <see cref="StartRendering"/> returns <strong>true</strong>
        /// </summary>
        void EndRendering();

        /// <summary>
        /// The width of the renderable surface
        /// </summary>
        int Width
        {
            get;
        }

        /// <summary>
        /// The height of the renderable surface
        /// </summary>
        int Height
        {
            get;
        }

        /// <summary>
        /// Creates a new cache context. Call <see cref="SetActiveCacheContext"/> to activate it.
        /// </summary> 
        /// <returns>a new CacheContext</returns>
        CacheContext CreateNewCacheContext();

        /// <summary>
        /// Sets the active cache context. It will be used for all future load operations.
        /// </summary>
        /// <param name="cc">The CacheContext object</param>
        void SetActiveCacheContext(CacheContext cc);

        /// <summary>
        /// Returns the active cache context. If no valid cache context is active then a new one is created and activated.
        /// </summary>
        /// <returns>the active CacheContext object</returns>
        CacheContext GetActiveCacheContext();

        /// <summary>
        /// Loads a font.
        /// </summary>
        /// <param name="baseFile">the base FSO that can be used to load font data</param>
        /// <param name="select">the StateSelect object</param>
        /// <param name="parameterList"> the font parameters - must be exactly 1 more than the number of expressions in the select object</param>
        /// <returns>a Font object</returns>
        Font LoadFont(FileSystemObject baseFile, StateSelect select, params FontParameter[] parameterList);

        /// <summary>
        /// Loads a texture. Textures are used to create images.
        /// </summary>
        /// <param name="file">the base FSO that can be used to load font data</param>
        /// <param name="format">a format description - depends on the implementation</param>
        /// <param name="filter">how the texture should be filtered - should support "nearest" and linear"</param>
        /// <returns>a Texture object</returns>
        Texture LoadTexture(FileSystemObject file, String format, String filter);

        /// <summary>
        /// The 2D line renderer. If line rendering is not supported then this method returns null. This is an optional operation.
        /// </summary>
        LineRenderer LineRenderer
        {
            get;
        }

        /// <summary>
        /// The offscreen renderer. If offscreen rendering is not supported then this method returns null. This is an optional operation.
        /// </summary>
        OffscreenRenderer OffscreenRenderer
        {
            get;
        }

        /// <summary>
        /// The font mapper object if one is available. This is an optional operation.
        /// </summary>
        FontMapper FontMapper
        {
            get;
        }

        /// <summary>
        /// Creates a <see cref="DynamicImage"/> with undefined content. This is an optional operation.
        /// </summary>
        /// <param name="width">the width of the image</param>
        /// <param name="height">the height of the image</param>
        /// <returns>a new dynamic image or null if the image could not be created</returns>
        DynamicImage CreateDynamicImage(int width, int height);

        /// <summary>
        /// Create an image from a given <see cref="Gradient"/>
        /// </summary>
        /// <param name="gradient"></param>
        /// <returns></returns>
        Image CreateGradient(Gradient gradient);

        /// <summary>
        /// Enters a clip region.
        /// <para>The new clip region is the intersection of the current clip region with the specified coordinates.</para>
        /// </summary>
        /// <param name="x">the left edge</param>
        /// <param name="y">the top edge</param>
        /// <param name="w">the width</param>
        /// <param name="h">the height</param>
        void ClipEnter(int x, int y, int w, int h);

        /// <summary>
        /// Enters a clip region.
        /// <para>The new clip region is the intersection of the current clip region with the specified coordinates</para>
        /// </summary>
        /// <param name="rect">the coordinates</param>
        void ClipEnter(Rect rect);

        /// <summary>
        /// Checks if the active clip region is empty (nothing will render).
        /// </summary>
        /// <returns><b>true</b> if the active clip region is empty</returns>
        bool ClipIsEmpty();

        /// <summary>
        /// Leaves a clip region entered by <see cref="ClipEnter"/>
        /// </summary>
        void ClipLeave();

        /// <summary>
        /// Set the current <see cref="MouseCursor"/>
        /// </summary>
        /// <param name="cursor">Cursor object</param>
        void SetCursor(MouseCursor cursor);

        /// <summary>
        /// Sets the mouse position for SW mouse cursor rendering
        /// </summary>
        /// <param name="mouseX">X mouse position</param>
        /// <param name="mouseY">Y mouse position</param>
        void SetMousePosition(int mouseX, int mouseY);

        /// <summary>
        /// Sets the mouse button state for SW mouse cursor rendering
        /// </summary>
        /// <param name="button">the mouse button</param>
        /// <param name="state"><strong>true</strong> if the mouse button is pressed</param>
        void SetMouseButton(int button, bool state);

        /// <summary>
        /// Pushes a new tint color on the tint stack. The current tint color is
        /// multiplied by the new tint color.
        /// 
        /// <para> For every call of <see cref="PushGlobalTintColor"/> a call to <see cref="PopGlobalTintColor"/> 
        /// must be made.</para>
        /// </summary>
        /// <param name="r">red, must be 0.0f >= r <= 1.0f</param>
        /// <param name="g">green, must be 0.0f >= g <= 1.0f</param>
        /// <param name="b">blue, must be 0.0f >= b <= 1.0f</param>
        /// <param name="a">alpha, must be 0.0f >= a <= 1.0f</param>
        void PushGlobalTintColor(float r, float g, float b, float a);

        /// <summary>
        /// Pops the top element on the tint stack
        /// </summary>
        void PopGlobalTintColor();
    }
}
