﻿/*
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
    public interface Renderer
    {
        /**
         * Returns the elapsed time in milliseconds.
         * @return the elapsed time in milliseconds.
         */
        long TimeMillis
        {
            get;
        }

        /**
         * Returns the polled input object for this renderer target.
         * 
         * <p>When using the "push" method for input event generation
         * return null.</p>
         * 
         * @return the Input object or null if none is available/used.
         */
        Input.Input Input
        {
            get;
        }

        /**
         * Setup rendering for TWL.
         * <p>Must be called before any Font or Image objects is drawn.</p>
         * <p>When this method returned {@code true} then {@link #endRendering()}
         * must be called.</p>
         * @return true if rendering was started, false otherwise
         */
        bool StartRendering();

        /**
         * Clean up after rendering TWL.
         * Only call this method when {@link #startRendering()} returned {@code true}
         */
        void EndRendering();

        /**
         * Returns the width of the renderable surface
         * @return the width of the renderable surface
         */
        int Width
        {
            get;
        }

        /**
         * Returns the height of the renderable surface
         * @return the height of the renderable surface
         */
        int Height
        {
            get;
        }

        /**
         * Creates a new cache context.
         * Call setActiveCacheContext to activate it.
         * 
         * @return a new CacheContext
         * @see #setActiveCacheContext(de.matthiasmann.twl.renderer.CacheContext) 
         */
        CacheContext CreateNewCacheContext();

        /**
         * Sets the active cache context. It will be used for all future load operations.
         *
         * @param cc The CacheContext object
         * @throws NullPointerException when cc is null
         * @throws IllegalStateException when the CacheContext object is invalid
         */
        void SetActiveCacheContext(CacheContext cc);

        /**
         * Returns the active cache context.
         * If no valid cache context is active then a new one is created and activated.
         * 
         * @return the active CacheContext object
         */
        CacheContext GetActiveCacheContext();

        /**
         * Loads a font.
         * 
         * @param baseUrl the base URL that can be used to load font data
         * @param select the StateSelect object
         * @param parameterList the font parameters - must be exactly 1 more then
         *                      the number of expressions in the select object
         * @return a Font object
         * @throws java.io.IOException if the font could not be loaded
         * @throws NullPointerException when one of the parameters is null
         * @throws IllegalArgumentException when the number of font parameters doesn't match the number of state expressions
         */
        Font LoadFont(FileSystemObject baseFile, StateSelect select, params FontParameter[] parameterList);

        /**
         * Loads a texture. Textures are used to create images.
         * 
         * @param url the URL of the texture file
         * @param format a format description - depends on the implementation
         * @param filter how the texture should be filtered - should support "nearest" and linear"
         * @return a Texture object
         * @throws java.io.IOException if the texture could not be loaded
         */
        Texture LoadTexture(FileSystemObject file, String format, String filter);

        /**
         * Returns the line renderer. If line rendering is not supported then this method returns null.
         *
         * This is an optional operation.
         *
         * @return the line renderer or null if not supported.
         */
        LineRenderer LineRenderer
        {
            get;
        }

        /**
         * Returns the offscreen renderer. If offscreen rendering is not supported then this method returns null.
         * 
         * This is an optional operation.
         *
         * @return the offscreen renderer or null if not supported.
         */
        OffscreenRenderer OffscreenRenderer
        {
            get;
        }

        /**
         * Returns the font mapper object if one is available.
         * 
         * This is an optional operation.
         *
         * @return the font mapper or null if not supported.
         */
        FontMapper FontMapper
        {
            get;
        }

        /**
         * Creates a dynamic image with undefined content.
         * 
         * This is an optional operation.
         * 
         * @param width the width of the image
         * @param height the height of the image
         * @return a new dynamic image or null if the image could not be created
         */
        DynamicImage CreateDynamicImage(int width, int height);

        Image CreateGradient(Gradient gradient);

        /**
         * Enters a clip region.
         * 
         * The new clip region is the intersection of the current clip region with
         * the specified coordinates.
         * 
         * @param x the left edge
         * @param y the top edge
         * @param w the width
         * @param h the height
         */
        void ClipEnter(int x, int y, int w, int h);

        /**
         * Enters a clip region.
         * 
         * The new clip region is the intersection of the current clip region with
         * the specified coordinates.
         * 
         * @param rect the coordinates
         */
        void ClipEnter(Rect rect);

        /**
         * Checks if the active clip region is empty (nothing will render).
         * @return true if the active clip region is empty
         */
        bool ClipIsEmpty();

        /**
         * Leaves a clip region creeated by {@code #clipEnter}
         * @see #clipEnter(int, int, int, int) 
         * @see #clipEnter(de.matthiasmann.twl.Rect) 
         */
        void ClipLeave();

        void SetCursor(MouseCursor cursor);

        /**
         * Sets the mouse position for SW mouse cursor rendering
         * 
         * @param mouseX X mouse position
         * @param mouseY Y mouse position
         */
        void SetMousePosition(int mouseX, int mouseY);

        /**
         * Sets the mouse button state for SW mouse cursor rendering
         * 
         * @param button the mouse button
         * @param state true if the mouse button is pressed
         * @see Event#MOUSE_LBUTTON
         * @see Event#MOUSE_MBUTTON
         * @see Event#MOUSE_RBUTTON
         */
        void SetMouseButton(int button, bool state);

        /**
         * Pushes a new tint color on the tint stack. The current tint color is
         * multiplied by the new tint color.
         *
         * For every call of {@code pushGlobalTintColor} a call to {@code popGlobalTintColor}
         * must be made.
         * 
         * @param r red, must be 0.0f &lt;= r &lt;= 1.0f
         * @param g green, must be 0.0f &lt;= g &lt;= 1.0f
         * @param b blue, must be 0.0f &lt;= b &lt;= 1.0f
         * @param a alpha, must be 0.0f &lt;= a &lt;= 1.0f
         */
        void PushGlobalTintColor(float r, float g, float b, float a);

        void PopGlobalTintColor();
    }
}
