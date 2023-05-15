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

        private Font _font;
        private FontCache _cache;
        private string _text;
        private int _cachedTextWidth = NOT_CACHED;
        private int _numTextLines;
        private bool _useCache = true;
        private bool _cacheDirty;
        private Alignment _alignment = Alignment.TOPLEFT;

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
            this._text = "";
        }

        public Font GetFont()
        {
            return _font;
        }

        public virtual void SetFont(Font font)
        {
            if (_cache != null)
            {
                _cache.Dispose();
                _cache = null;
            }
            this._font = font;
            this._cachedTextWidth = NOT_CACHED;
            if (_useCache)
            {
                this._cacheDirty = true;
            }
        }

        /**
         * Sets a new text to be displayed.
         * If CharSequence changes it's content this function must be called
         * again or correct rendering can't be guranteed.
         *
         * @param text The CharSequence to display
         */
        public void SetCharSequence(string text)
        {
            if (text == null)
            {
                throw new NullReferenceException("text");
            }
            this._text = text;
            this._cachedTextWidth = NOT_CACHED;
            this._numTextLines = TextUtil.CountNumLines(text);
            this._cacheDirty = true;
            GetAnimationState().ResetAnimationTime(STATE_TEXT_CHANGED);
        }

        protected string GetCharSequence()
        {
            return _text;
        }

        public bool HasText()
        {
            return _numTextLines > 0;
        }

        public bool IsMultilineText()
        {
            return _numTextLines > 1;
        }

        public int GetNumTextLines()
        {
            return _numTextLines;
        }

        public Alignment GetAlignment()
        {
            return _alignment;
        }

        public void SetAlignment(Alignment alignment)
        {
            if (alignment == null)
            {
                throw new NullReferenceException("alignment");
            }
            if (this._alignment != alignment)
            {
                this._alignment = alignment;
                this._cacheDirty = true;
            }
        }

        public bool IsCache()
        {
            return _useCache;
        }

        public void SetCache(bool cache)
        {
            if (this._useCache != cache)
            {
                this._useCache = cache;
                this._cacheDirty = true;
            }
        }

        protected void ApplyThemeTextWidget(ThemeInfo themeInfo)
        {
            SetFont(themeInfo.GetFont("font"));
            SetAlignment(themeInfo.GetParameterValue<Alignment>("textAlignment", false, typeof(Alignment), Alignment.TOPLEFT));
        }

        protected override void ApplyTheme(ThemeInfo themeInfo)
        {
            base.ApplyTheme(themeInfo);
            ApplyThemeTextWidget(themeInfo);
        }

        public override void Destroy()
        {
            if (_cache != null)
            {
                _cache.Dispose();
                _cache = null;
            }
            base.Destroy();
        }

        protected virtual int ComputeTextX()
        {
            int x = GetInnerX();
            int pos = _alignment.GetHPosition();
            if (pos > 0)
            {
                return x + (GetInnerWidth() - ComputeTextWidth()) * pos / 2;
            }
            return x;
        }

        protected virtual internal int ComputeTextY()
        {
            int y = GetInnerY();
            int pos = _alignment.GetVPosition();
            if (pos > 0)
            {
                return y + (GetInnerHeight() - ComputeTextHeight()) * pos / 2;
            }
            return y;
        }

        protected override void PaintWidget(GUI gui)
        {
            PaintLabelText(GetAnimationState());
        }

        protected void PaintLabelText(AnimationState animState)
        {
            if (_cacheDirty)
            {
                UpdateCache();
            }
            if (HasText() && _font != null)
            {
                int x = ComputeTextX();
                int y = ComputeTextY();

                PaintTextAt(animState, x, y);
            }
        }

        protected void PaintTextAt(AnimationState animState, int x, int y)
        {
            if (_cache != null)
            {
                _cache.Draw(animState, x, y);
            }
            else if (_numTextLines > 1)
            {
                _font.DrawMultiLineText(animState, x, y, _text, ComputeTextWidth(), _alignment.GetFontHAlignment());
            }
            else
            {
                _font.DrawText((Renderer.AnimationState)animState, x, y, _text);
            }
        }

        protected void PaintWithSelection(AnimationState animState, int start, int end)
        {
            PaintWithSelection(animState, start, end, 0, _text.Length, ComputeTextY());
        }

        protected void PaintWithSelection(AnimationState animState, int start, int end, int lineStart, int lineEnd, int y)
        {
            if (_cacheDirty)
            {
                UpdateCache();
            }
            if (HasText() && _font != null)
            {
                int x = ComputeTextX();

                start = Limit(start, lineStart, lineEnd);
                end = Limit(end, lineStart, lineEnd);

                if (start > lineStart)
                {
                    x += _font.DrawText(animState, x, y, _text, lineStart, start);
                }
                if (end > start)
                {
                    animState.SetAnimationState(STATE_TEXT_SELECTION, true);
                    x += _font.DrawText(animState, x, y, _text, start, end);
                    animState.SetAnimationState(STATE_TEXT_SELECTION, false);
                }
                if (end < lineEnd)
                {
                    _font.DrawText(animState, x, y, _text, end, lineEnd);
                }
            }
        }

        private static int Limit(int value, int min, int max)
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

        public override int GetPreferredInnerWidth()
        {
            int prefWidth = base.GetPreferredInnerWidth();
            if (HasText() && _font != null)
            {
                prefWidth = Math.Max(prefWidth, ComputeTextWidth());
            }
            return prefWidth;
        }

        public override int GetPreferredInnerHeight()
        {
            int prefHeight = base.GetPreferredInnerHeight();
            if (HasText() && _font != null)
            {
                prefHeight = Math.Max(prefHeight, ComputeTextHeight());
            }
            return prefHeight;
        }

        public int ComputeRelativeCursorPositionX(int charIndex)
        {
            return ComputeRelativeCursorPositionX(0, charIndex);
        }

        public int ComputeRelativeCursorPositionX(int startIndex, int charIndex)
        {
            if (_font != null && charIndex > startIndex)
            {
                return _font.ComputeTextWidth(_text, startIndex, charIndex);
            }
            return 0;
        }

        public int ComputeTextWidth()
        {
            if (_font != null)
            {
                if (_cachedTextWidth == NOT_CACHED || _cacheDirty)
                {
                    if (_numTextLines > 1)
                    {
                        _cachedTextWidth = _font.ComputeMultiLineTextWidth(_text);
                    }
                    else
                    {
                        _cachedTextWidth = _font.ComputeTextWidth(_text);
                    }
                }
                return _cachedTextWidth;
            }
            return 0;
        }

        public int ComputeTextHeight()
        {
            if (_font != null)
            {
                return Math.Max(1, _numTextLines) * _font.LineHeight;
            }
            return 0;
        }

        private void UpdateCache()
        {
            _cacheDirty = false;
            if (_useCache && HasText() && _font != null)
            {
                if (_numTextLines > 1)
                {
                    _cache = _font.CacheMultiLineText(_cache, _text,
                            _font.ComputeMultiLineTextWidth(_text),
                            _alignment.GetFontHAlignment());
                }
                else
                {
                    _cache = _font.CacheText(_cache, _text);
                }
                if (_cache != null)
                {
                    _cachedTextWidth = _cache.Width;
                }
            }
            else
            {
                Destroy();
            }
        }

        protected virtual void HandleMouseHover(Event evt)
        {
            if (evt.IsMouseEvent() && !HasSharedAnimationState())
            {
                GetAnimationState().SetAnimationState(STATE_HOVER, evt.GetEventType() != EventType.MOUSE_EXITED);
            }
        }
    }
}
