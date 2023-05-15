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
using XNATWL.Renderer;
using XNATWL.Utils;

namespace XNATWL
{
    public class TextWidget : Widget
    {
        public static StateKey STATE_HOVER = StateKey.Get("hover");
        public static StateKey STATE_TEXT_CHANGED = StateKey.Get("textChanged");
        public static StateKey STATE_TEXT_SELECTION = StateKey.Get("textSelection");

        private static int NOT_CACHED = -1;

        private Font font;
        private FontCache cache;
        private string text;
        private int cachedTextWidth = NOT_CACHED;
        private int numTextLines;
        private bool useCache = true;
        private bool cacheDirty;
        private Alignment alignment = Alignment.TOPLEFT;

        public TextWidget() : this(null, false)
        {
            
        }

        /**
         * Creates a TextWidget with a shared animation state
         *
         * @param animState the animation state to share, can be null
         */
        public TextWidget(AnimationState animState) : this(animState, false)
        {
            
        }

        /**
         * Creates a TextWidget with a shared or inherited animation state
         *
         * @param animState the animation state to share or inherit, can be null
         * @param inherit true if the animation state should be inherited false for sharing
         */
        public TextWidget(AnimationState animState, bool inherit) : base(animState, inherit)
        {
            this.text = "";
        }

        public Font getFont()
        {
            return font;
        }

        public virtual void setFont(Font font)
        {
            if (cache != null)
            {
                cache.Dispose();
                cache = null;
            }
            this.font = font;
            this.cachedTextWidth = NOT_CACHED;
            if (useCache)
            {
                this.cacheDirty = true;
            }
        }

        /**
         * Sets a new text to be displayed.
         * If CharSequence changes it's content this function must be called
         * again or correct rendering can't be guranteed.
         *
         * @param text The CharSequence to display
         */
        public void setCharSequence(string text)
        {
            if (text == null)
            {
                throw new NullReferenceException("text");
            }
            this.text = text;
            this.cachedTextWidth = NOT_CACHED;
            this.numTextLines = TextUtil.CountNumLines(text);
            this.cacheDirty = true;
            getAnimationState().resetAnimationTime(STATE_TEXT_CHANGED);
        }

        protected string getCharSequence()
        {
            return text;
        }

        public bool hasText()
        {
            return numTextLines > 0;
        }

        public bool isMultilineText()
        {
            return numTextLines > 1;
        }

        public int getNumTextLines()
        {
            return numTextLines;
        }

        public Alignment getAlignment()
        {
            return alignment;
        }

        public void setAlignment(Alignment alignment)
        {
            if (alignment == null)
            {
                throw new NullReferenceException("alignment");
            }
            if (this.alignment != alignment)
            {
                this.alignment = alignment;
                this.cacheDirty = true;
            }
        }

        public bool isCache()
        {
            return useCache;
        }

        public void setCache(bool cache)
        {
            if (this.useCache != cache)
            {
                this.useCache = cache;
                this.cacheDirty = true;
            }
        }

        protected void applyThemeTextWidget(ThemeInfo themeInfo)
        {
            setFont(themeInfo.GetFont("font"));
            setAlignment(themeInfo.GetParameterValue<Alignment>("textAlignment", false, typeof(Alignment), Alignment.TOPLEFT));
        }

        //@Override
        protected override void applyTheme(ThemeInfo themeInfo)
        {
            base.applyTheme(themeInfo);
            applyThemeTextWidget(themeInfo);
        }

        //@Override
        public override void destroy()
        {
            if (cache != null)
            {
                cache.Dispose();
                cache = null;
            }
            base.destroy();
        }

        protected virtual int computeTextX()
        {
            int x = getInnerX();
            int pos = alignment.getHPosition();
            if (pos > 0)
            {
                return x + (getInnerWidth() - computeTextWidth()) * pos / 2;
            }
            return x;
        }

        protected virtual internal int computeTextY()
        {
            int y = getInnerY();
            int pos = alignment.getVPosition();
            if (pos > 0)
            {
                return y + (getInnerHeight() - computeTextHeight()) * pos / 2;
            }
            return y;
        }

        //@Override
        protected override void paintWidget(GUI gui)
        {
            paintLabelText(getAnimationState());
        }

        protected void paintLabelText(AnimationState animState)
        {
            if (cacheDirty)
            {
                updateCache();
            }
            if (hasText() && font != null)
            {
                int x = computeTextX();
                int y = computeTextY();

                paintTextAt(animState, x, y);
            }
        }

        protected void paintTextAt(AnimationState animState, int x, int y)
        {
            if (cache != null)
            {
                cache.Draw(animState, x, y);
            }
            else if (numTextLines > 1)
            {
                font.DrawMultiLineText(animState, x, y, text, computeTextWidth(), alignment.getFontHAlignment());
            }
            else
            {
                font.DrawText((Renderer.AnimationState)animState, x, y, text);
            }
        }

        protected void paintWithSelection(AnimationState animState, int start, int end)
        {
            paintWithSelection(animState, start, end, 0, text.Length, computeTextY());
        }

        protected void paintWithSelection(AnimationState animState, int start, int end, int lineStart, int lineEnd, int y)
        {
            if (cacheDirty)
            {
                updateCache();
            }
            if (hasText() && font != null)
            {
                int x = computeTextX();

                start = limit(start, lineStart, lineEnd);
                end = limit(end, lineStart, lineEnd);

                if (start > lineStart)
                {
                    x += font.DrawText(animState, x, y, text, lineStart, start);
                }
                if (end > start)
                {
                    animState.setAnimationState(STATE_TEXT_SELECTION, true);
                    x += font.DrawText(animState, x, y, text, start, end);
                    animState.setAnimationState(STATE_TEXT_SELECTION, false);
                }
                if (end < lineEnd)
                {
                    font.DrawText(animState, x, y, text, end, lineEnd);
                }
            }
        }

        private static int limit(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }
            if (value > max)
            {
                return max;
            }
            return value;
        }

        //@Override
        public override int getPreferredInnerWidth()
        {
            int prefWidth = base.getPreferredInnerWidth();
            if (hasText() && font != null)
            {
                prefWidth = Math.Max(prefWidth, computeTextWidth());
            }
            return prefWidth;
        }

        //@Override
        public override int getPreferredInnerHeight()
        {
            int prefHeight = base.getPreferredInnerHeight();
            if (hasText() && font != null)
            {
                prefHeight = Math.Max(prefHeight, computeTextHeight());
            }
            return prefHeight;
        }

        public int computeRelativeCursorPositionX(int charIndex)
        {
            return computeRelativeCursorPositionX(0, charIndex);
        }

        public int computeRelativeCursorPositionX(int startIndex, int charIndex)
        {
            if (font != null && charIndex > startIndex)
            {
                return font.ComputeTextWidth(text, startIndex, charIndex);
            }
            return 0;
        }

        public int computeTextWidth()
        {
            if (font != null)
            {
                if (cachedTextWidth == NOT_CACHED || cacheDirty)
                {
                    if (numTextLines > 1)
                    {
                        cachedTextWidth = font.ComputeMultiLineTextWidth(text);
                    }
                    else
                    {
                        cachedTextWidth = font.ComputeTextWidth(text);
                    }
                }
                return cachedTextWidth;
            }
            return 0;
        }

        public int computeTextHeight()
        {
            if (font != null)
            {
                return Math.Max(1, numTextLines) * font.LineHeight;
            }
            return 0;
        }

        private void updateCache()
        {
            cacheDirty = false;
            if (useCache && hasText() && font != null)
            {
                if (numTextLines > 1)
                {
                    cache = font.CacheMultiLineText(cache, text,
                            font.ComputeMultiLineTextWidth(text),
                            alignment.getFontHAlignment());
                }
                else
                {
                    cache = font.CacheText(cache, text);
                }
                if (cache != null)
                {
                    cachedTextWidth = cache.Width;
                }
            }
            else
            {
                destroy();
            }
        }

        protected virtual void handleMouseHover(Event evt)
        {
            if (evt.isMouseEvent() && !hasSharedAnimationState())
            {
                getAnimationState().setAnimationState(STATE_HOVER, evt.getEventType() != EventType.MOUSE_EXITED);
            }
        }
    }

}
