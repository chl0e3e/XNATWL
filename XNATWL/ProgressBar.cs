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

namespace XNATWL
{
	public class ProgressBar : TextWidget
	{
		public static StateKey STATE_VALUE_CHANGED = StateKey.Get("valueChanged");
		public static StateKey STATE_INDETERMINATE = StateKey.Get("indeterminate");
    
		public static float VALUE_INDETERMINATE = -1;

		private Image _progressImage;
		private float _value;

		public ProgressBar()
		{
			GetAnimationState().ResetAnimationTime(STATE_VALUE_CHANGED);
		}

		/**
		 * Returns the current value or VALUE_INDETERMINATE
		 * @return the current value or VALUE_INDETERMINATE
		 */
		public float GetValue()
		{
			return _value;
		}

		/**
		 * Sets the progress bar to an indeterminate state.
		 * @see #STATE_INDETERMINATE
		 */
		public void SetIndeterminate()
		{
			if (_value >= 0)
			{
				_value = VALUE_INDETERMINATE;
				AnimationState animationState = GetAnimationState();
				animationState.SetAnimationState(STATE_INDETERMINATE, true);
				animationState.ResetAnimationTime(STATE_VALUE_CHANGED);
			}
		}

		/**
		 * Sets the progress value to the specified value between 0.0f and 1.0f.
		 * This will also clear the {@link #STATE_INDETERMINATE} state.
		 *
		 * @param value the progress value between 0.0f and 1.0f.
		 */
		public void SetValue(float value)
		{
			if (!(value > 0))
			{  // protect against NaN
				value = 0;
			}
			else if (value > 1)
			{
				value = 1;
			}
			if (this._value != value)
			{
				this._value = value;
				AnimationState animationState = GetAnimationState();
				animationState.SetAnimationState(STATE_INDETERMINATE, false);
				animationState.ResetAnimationTime(STATE_VALUE_CHANGED);
			}
		}

		public String GetText()
		{
			return (String)GetCharSequence();
		}

		/**
		 * Sets the text which is displayed on top of the progress bar image.
		 * @param text the text
		 */
		public void SetText(String text)
		{
			SetCharSequence(text);
		}

		public Image GetProgressImage()
		{
			return _progressImage;
		}

		/**
		 * Sets the progress image.
		 *
		 * <p>This is called from {@link #applyThemeProgressBar(de.matthiasmann.twl.ThemeInfo) }</p>
		 *
		 * <p>When the progress bar is in indeterminate state then the image is not
		 * drawn, otherwise it is drawn with a scaled width based on the current
		 * progress value.</p>
		 *
		 * @param progressImage the progress image, can be null.
		 */
		public void SetProgressImage(Image progressImage)
		{
			this._progressImage = progressImage;
		}

		protected void ApplyThemeProgressBar(ThemeInfo themeInfo)
		{
			SetProgressImage(themeInfo.GetImage("progressImage"));
		}

		protected override void ApplyTheme(ThemeInfo themeInfo)
		{
			base.ApplyTheme(themeInfo);
			ApplyThemeProgressBar(themeInfo);
		}

		protected override void PaintWidget(GUI gui)
		{
			int width = GetInnerWidth();
			int height = GetInnerHeight();

			if (_progressImage != null && _value >= 0)
			{
				int imageWidth = _progressImage.Width;

				int progressWidth = width - imageWidth;

				int scaledWidth = (int)(progressWidth * _value);
				if (scaledWidth < 0)
				{
					scaledWidth = 0;
				}
				else if (scaledWidth > progressWidth)
				{
					scaledWidth = progressWidth;
				}

				_progressImage.Draw(GetAnimationState(), GetInnerX(), GetInnerY(), imageWidth + scaledWidth, height);
			}

			base.PaintWidget(gui);
		}

		public override int GetMinWidth()
		{
			int minWidth = base.GetMinWidth();
			Image bg = GetBackground();
			if (bg != null)
			{
				minWidth = Math.Max(minWidth, bg.Width + GetBorderHorizontal());
			}
			return minWidth;
		}

		public override int GetMinHeight()
		{
			int minHeight = base.GetMinHeight();
			Image bg = GetBackground();
			if (bg != null)
			{
				minHeight = Math.Max(minHeight, bg.Height + GetBorderVertical());
			}
			return minHeight;
		}

		public override int GetPreferredInnerWidth()
		{
			int prefWidth = base.GetPreferredInnerWidth();
			if (_progressImage != null)
			{
				prefWidth = Math.Max(prefWidth, _progressImage.Width);
			}
			return prefWidth;
		}

		public override int GetPreferredInnerHeight()
		{
			int prefHeight = base.GetPreferredInnerHeight();
			if (_progressImage != null)
			{
				prefHeight = Math.Max(prefHeight, _progressImage.Height);
			}
			return prefHeight;
		}
	}
}
