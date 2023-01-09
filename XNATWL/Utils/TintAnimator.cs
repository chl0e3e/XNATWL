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

namespace XNATWL.Utils
{
    public class TintAnimator
    {
        /**
         * A time source for the fade animation
         */
        public interface TimeSource
        {
            /**
             * Restarts the time from 0 for a new fade animation
             */
            void resetTime();
            /**
             * Returns the current time (since last reset) in milliseconds.
             * @return current time in ms
             */
            int getTime();
        }

        private static float ZERO_EPSILON = 1e-3f;
        private static float ONE_EPSILON = 1f - ZERO_EPSILON;

        private TimeSource timeSource;
        private float[] currentTint;
        private int fadeDuration;
        private bool fadeActive;
        private bool bHasTint;
        public event EventHandler<FadeDoneEventArgs> FadeDone;
        //private Runnable[] fadeDoneCallbacks;

        /**
         * Creates a new TintAnimator which starts in the specified color.
         *
         * @param timeSource the time source for the fade animation
         * @param color the starting color
         */
        public TintAnimator(TimeSource timeSource, Color color)
        {
            if (timeSource == null)
            {
                throw new NullReferenceException("timeSource");
            }
            if (color == null)
            {
                throw new NullReferenceException("color");
            }
            this.timeSource = timeSource;
            this.currentTint = new float[12];
            setColor(color);
        }

        /**
         * Creates a new TintAnimator which starts in the specified color
         * and uses the specified GUI as time source.
         * 
         * @param gui the GUI instance - must not be null
         * @param color the starting color
         */
        public TintAnimator(GUI gui, Color color) : this(new GUITimeSource(gui), color)
        {

        }

        /**
         * Creates a new TintAnimator which starts in the specified color
         * and uses the specified Widget as time source.
         * 
         * @param owner the Widget instance - must not be null
         * @param color the starting color
         */
        public TintAnimator(Widget owner, Color color) : this(new GUITimeSource(owner), color)
        {

        }

        /**
         * Creates a new TintAnimator which starts with Color.WHITE
         *
         * @param timeSource the time source for the fade animation
         */
        public TintAnimator(TimeSource timeSource) : this(timeSource, Color.WHITE)
        {
            
        }

        /**
         * Creates a new TintAnimator which starts with Color.WHITE
         * and uses the specified GUI as time source.
         * 
         * @param gui the GUI instance - must not be null
         * @see GUITimeSource#GUITimeSource(de.matthiasmann.twl.GUI) 
         */
        public TintAnimator(GUI gui) : this(new GUITimeSource(gui))
        {
            
        }

        /**
         * Creates a new TintAnimator which starts with Color.WHITE
         * and uses the specified Widget as time source.
         * 
         * @param owner the Widget instance - must not be null
         * @see GUITimeSource#GUITimeSource(de.matthiasmann.twl.GUI) 
         */
        public TintAnimator(Widget owner) : this(new GUITimeSource(owner))
        {

        }

        /**
         * Sets the current color without a fade. Any active fade is stopped.
         * The time source is also reset even so no animation is started.
         * 
         * @param color the new color
         */
        public void setColor(Color color)
        {
            color.WriteToFloatArray(currentTint, 0);
            color.WriteToFloatArray(currentTint, 4);
            bHasTint = !Color.WHITE.Equals(color);
            fadeActive = false;
            fadeDuration = 0;
            timeSource.resetTime();
        }

        /**
         * Fade the current color to the specified color.
         * 
         * <p>Any active fade is stopped.</p>
         * 
         * <p>A zero or negative fadeDuration will set the new color
         * directly and does not start a fade. So no callbacks are fired as a
         * result of this.</p>
         *
         * @param color the destination color of the fade
         * @param fadeDuration the fade time in miliseconds
         * @see #addFadeDoneCallback(java.lang.Runnable) 
         */
        public void fadeTo(Color color, int fadeDuration)
        {
            if (fadeDuration <= 0)
            {
                setColor(color);
            }
            else
            {
                color.WriteToFloatArray(currentTint, 8);
                Array.Copy(currentTint, 0, currentTint, 4, 4);
                this.fadeActive = true;
                this.fadeDuration = fadeDuration;
                this.bHasTint = true;
                timeSource.resetTime();
            }
        }

        /**
         * Fade the current color to alpha 0.0f. Any active fade is stopped.
         *
         * <p>This method uses the current color (which may be a mix if a fade was
         * active) as a base to fade the alpha value. Because of that the only
         * defined part of the target color is the alpha channel. This is
         * the reason why no fadeToShow method exists. Use fadeTo with the
         * desired color to make the widget visible again.</p>
         * 
         * <p>A zero or negative fadeDuration will set the alpha value
         * directly and does not start a fade. So no callbacks are fired as a
         * result of this.</p>
         *
         * @param fadeDuration the fade time in miliseconds
         * @see #addFadeDoneCallback(java.lang.Runnable) 
         */
        public void fadeToHide(int fadeDuration)
        {
            if (fadeDuration <= 0)
            {
                currentTint[3] = 0.0f;
                this.fadeActive = false;
                this.fadeDuration = 0;
                this.bHasTint = true;
            }
            else
            {
                Array.Copy(currentTint, 0, currentTint, 4, 8);
                currentTint[11] = 0.0f;
                this.fadeActive = !isZeroAlpha();
                this.fadeDuration = fadeDuration;
                this.bHasTint = true;
                timeSource.resetTime();
            }
        }

        /**
         * Updates the fade animation. Does not need to be called when no fade is active.
         */
        public void update()
        {
            if (fadeActive)
            {
                int time = timeSource.getTime();
                float t = Math.Min(time, fadeDuration) / (float)fadeDuration;
                float tm1 = 1.0f - t;
                float[] tint = currentTint;
                for (int i = 0; i < 4; i++)
                {
                    tint[i] = tm1 * tint[i + 4] + t * tint[i + 8];
                }
                if (time >= fadeDuration)
                {
                    fadeActive = false;
                    // disable tinted rendering if we have full WHITE as tint
                    bHasTint =
                            (currentTint[0] < ONE_EPSILON) ||
                            (currentTint[1] < ONE_EPSILON) ||
                            (currentTint[2] < ONE_EPSILON) ||
                            (currentTint[3] < ONE_EPSILON);
                    // fire callbacks
                    this.FadeDone.Invoke(this, new FadeDoneEventArgs());
                }
            }
        }

        /**
         * Returns true when a fade is active
         * @return true when a fade is active
         */
        public bool isFadeActive()
        {
            return fadeActive;
        }

        /**
         * Returns true when the current tint color is not Color.WHITE
         * @return true when the current tint color is not Color.WHITE
         */
        public bool hasTint()
        {
            return bHasTint;
        }

        /**
         * Returns true is the current alpha value is 0.0f
         * @return true is the current alpha value is 0.0f
         */
        public bool isZeroAlpha()
        {
            return currentTint[3] <= ZERO_EPSILON;
        }

        /**
         * Calls {@code renderer.pushGlobalTintColor} with the current tint color.
         * It is important to call {@code renderer.popGlobalTintColor} after this
         * method.
         *
         * @param renderer The renderer
         *
         * @see Renderer#pushGlobalTintColor(float, float, float, float)
         * @see Renderer#popGlobalTintColor()
         */
        public void paintWithTint(Renderer.Renderer renderer)
        {
            float[] tint = this.currentTint;
            renderer.PushGlobalTintColor(tint[0], tint[1], tint[2], tint[3]);
        }

        /**
         * A time source which uses the GUI object of the specified widget
         * or a directly specified GUI instance.
         * 
         * <p>If using a Widget which is not part of a GUI tree then the time is
         * frozen at 0, and starts ticking as soon as the widget is added to a
         * GUI tree.</p>
         */
        public class GUITimeSource : TimeSource
        {
            private Widget owner;
            private GUI gui;
            private long startTime;
            private bool pendingReset;

            public GUITimeSource(Widget owner)
            {
                if (owner == null)
                {
                    throw new NullReferenceException("owner");
                }
                this.owner = owner;
                this.gui = null;
                resetTime();
            }

            public GUITimeSource(GUI gui)
            {
                if (gui == null)
                {
                    throw new NullReferenceException("gui");
                }
                this.owner = null;
                this.gui = gui;
            }


            public int getTime()
            {
                GUI g = getGUI();
                if (g != null)
                {
                    if (pendingReset)
                    {
                        pendingReset = false;
                        startTime = g.getCurrentTime();
                    }
                    return (int)(g.getCurrentTime() - startTime) & Int32.MaxValue;
                }
                return 0;
            }

            public void resetTime()
            {
                GUI g = getGUI();
                if (g != null)
                {
                    startTime = g.getCurrentTime();
                    pendingReset = false;
                }
                else
                {
                    pendingReset = true;
                }
            }

            private GUI getGUI()
            {
                return (gui != null) ? gui : owner.getGUI();
            }
        }

        /**
         * A time source which uses a specified animation state as time source.
         */
        public class AnimationStateTimeSource : TimeSource
        {
            private AnimationState animState;
            private StateKey animStateKey;

            public AnimationStateTimeSource(AnimationState animState, String animStateName) : this(animState, StateKey.Get(animStateName))
            {
                
            }

            public AnimationStateTimeSource(AnimationState animState, StateKey animStateKey)
            {
                if (animState == null)
                {
                    throw new NullReferenceException("animState");
                }
                if (animStateKey == null)
                {
                    throw new NullReferenceException("animStateKey");
                }
                this.animState = animState;
                this.animStateKey = animStateKey;
            }

            public int getTime()
            {
                return animState.GetAnimationTime(animStateKey);
            }

            /**
             * Calls resetAnimationTime on the animation state
             * @see AnimationState#resetAnimationTime(java.lang.String)
             */
            public void resetTime()
            {
                animState.resetAnimationTime(animStateKey);
            }
        }
    }

    public class FadeDoneEventArgs : EventArgs
    {
    }
}
