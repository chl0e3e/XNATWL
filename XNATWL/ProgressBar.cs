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

		private Image progressImage;
		private float value;

		public ProgressBar()
		{
			getAnimationState().resetAnimationTime(STATE_VALUE_CHANGED);
		}

		/**
		 * Returns the current value or VALUE_INDETERMINATE
		 * @return the current value or VALUE_INDETERMINATE
		 */
		public float getValue()
		{
			return value;
		}

		/**
		 * Sets the progress bar to an indeterminate state.
		 * @see #STATE_INDETERMINATE
		 */
		public void setIndeterminate()
		{
			if (value >= 0)
			{
				value = VALUE_INDETERMINATE;
				AnimationState animationState = getAnimationState();
				animationState.setAnimationState(STATE_INDETERMINATE, true);
				animationState.resetAnimationTime(STATE_VALUE_CHANGED);
			}
		}

		/**
		 * Sets the progress value to the specified value between 0.0f and 1.0f.
		 * This will also clear the {@link #STATE_INDETERMINATE} state.
		 *
		 * @param value the progress value between 0.0f and 1.0f.
		 */
		public void setValue(float value)
		{
			if (!(value > 0))
			{  // protect against NaN
				value = 0;
			}
			else if (value > 1)
			{
				value = 1;
			}
			if (this.value != value)
			{
				this.value = value;
				AnimationState animationState = getAnimationState();
				animationState.setAnimationState(STATE_INDETERMINATE, false);
				animationState.resetAnimationTime(STATE_VALUE_CHANGED);
			}
		}

		public String getText()
		{
			return (String)getCharSequence();
		}

		/**
		 * Sets the text which is displayed on top of the progress bar image.
		 * @param text the text
		 */
		public void setText(String text)
		{
			setCharSequence(text);
		}

		public Image getProgressImage()
		{
			return progressImage;
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
		public void setProgressImage(Image progressImage)
		{
			this.progressImage = progressImage;
		}

		protected void applyThemeProgressBar(ThemeInfo themeInfo)
		{
			setProgressImage(themeInfo.GetImage("progressImage"));
		}

		protected override void applyTheme(ThemeInfo themeInfo)
		{
			base.applyTheme(themeInfo);
			applyThemeProgressBar(themeInfo);
		}

		protected override void paintWidget(GUI gui)
		{
			int width = getInnerWidth();
			int height = getInnerHeight();

			if (progressImage != null && value >= 0)
			{
				int imageWidth = progressImage.Width;

				int progressWidth = width - imageWidth;

				int scaledWidth = (int)(progressWidth * value);
				if (scaledWidth < 0)
				{
					scaledWidth = 0;
				}
				else if (scaledWidth > progressWidth)
				{
					scaledWidth = progressWidth;
				}

				progressImage.Draw(getAnimationState(), getInnerX(), getInnerY(), imageWidth + scaledWidth, height);
			}

			base.paintWidget(gui);
		}

		public override int getMinWidth()
		{
			int minWidth = base.getMinWidth();
			Image bg = getBackground();
			if (bg != null)
			{
				minWidth = Math.Max(minWidth, bg.Width + getBorderHorizontal());
			}
			return minWidth;
		}

		public override int getMinHeight()
		{
			int minHeight = base.getMinHeight();
			Image bg = getBackground();
			if (bg != null)
			{
				minHeight = Math.Max(minHeight, bg.Height + getBorderVertical());
			}
			return minHeight;
		}

		public override int getPreferredInnerWidth()
		{
			int prefWidth = base.getPreferredInnerWidth();
			if (progressImage != null)
			{
				prefWidth = Math.Max(prefWidth, progressImage.Width);
			}
			return prefWidth;
		}

		public override int getPreferredInnerHeight()
		{
			int prefHeight = base.getPreferredInnerHeight();
			if (progressImage != null)
			{
				prefHeight = Math.Max(prefHeight, progressImage.Height);
			}
			return prefHeight;
		}

	}

}
