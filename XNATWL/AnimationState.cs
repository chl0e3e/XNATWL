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
    public class AnimationState : XNATWL.Renderer.AnimationState
    {
        private AnimationState parent;

        private State[] stateTable;
        private GUI gui;

        /**
         * Create a new animation state with optional parent.
         *
         * When a parent animation state is set, then any request for a state which
         * has not been set (to either true or false) in this instance are forwarded
         * to the parent.
         *
         * @param parent the parent animation state or null
         * @param size the initial size of the state table (indexed by state IDs) 
         */
        public AnimationState(AnimationState parent, int size)
        {
            this.parent = parent;
            this.stateTable = new State[size];
        }

        /**
         * Create a new animation state with optional parent.
         *
         * When a parent animation state is set, then any request for a state which
         * has not been set (to either true or false) in this instance are forwarded
         * to the parent.
         *
         * @param parent the parent animation state or null
         */
        public AnimationState(AnimationState parent) : this(parent, 16)
        {
            
        }

        /**
         * Creates a new animation state without parent
         *
         * @see #AnimationState(de.matthiasmann.twl.AnimationState) 
         */
        public AnimationState() : this(null)
        {
        }

        public void setGUI(GUI gui)
        {
            this.gui = gui;

            long curTime = getCurrentTime();
            foreach (State s in stateTable)
            {
                if (s != null)
                {
                    s.lastChangedTime = curTime;
                }
            }
        }

        /**
         * Returns the time since the specified state has changed in ms.
         * If the specified state was never changed then a free running time is returned.
         *
         * @param stateKey the state key.
         * @return time since last state change is ms.
         */
        public int GetAnimationTime(StateKey stateKey)
        {
            State state = getState(stateKey);
            if (state != null)
            {
                return (int)Math.Min(Int32.MaxValue, getCurrentTime() - state.lastChangedTime);
            }
            if (parent != null)
            {
                return parent.GetAnimationTime(stateKey);
            }
            return (int)getCurrentTime() & 2147483647;
        }

        /**
         * Checks if the given state is active.
         *
         * @param stateKey the state key.
         * @return true if the state is set
         */
        public bool GetAnimationState(StateKey stateKey)
        {
            State state = getState(stateKey);
            if (state != null)
            {
                return state.active;
            }
            if (parent != null)
            {
                return parent.GetAnimationState(stateKey);
            }
            return false;
        }

        /**
         * Checks if this state was changed based on user interaction or not.
         * If this method returns false then the animation time should not be used
         * for single shot animations.
         *
         * @param stateKey the state key.
         * @return true if single shot animations should run or not.
         */
        public bool ShouldAnimateState(StateKey stateKey)
        {
            State state = getState(stateKey);
            if (state != null)
            {
                return state.shouldAnimate;
            }
            if (parent != null)
            {
                return parent.ShouldAnimateState(stateKey);
            }
            return false;
        }

        /**
         * Equivalent to calling {@code setAnimationState(StateKey.get(stateName), active);}
         * 
         * @param stateName the string specifying the state key
         * @param active the new value
         * @deprecated
         * @see #setAnimationState(de.matthiasmann.twl.renderer.AnimationState.StateKey, bool)
         * @see de.matthiasmann.twl.renderer.AnimationState.StateKey#get(java.lang.String)
         */
        public void setAnimationState(String stateName, bool active)
        {
            setAnimationState(StateKey.Get(stateName), active);
        }

        /**
         * Sets the specified animation state to the given value.
         * If the value is changed then the animation time is reset too.
         *
         * @param stateKey the state key
         * @param active the new value
         * @see #getAnimationState(de.matthiasmann.twl.renderer.AnimationState.StateKey)
         * @see #resetAnimationTime(de.matthiasmann.twl.renderer.AnimationState.StateKey)
         */
        public void setAnimationState(StateKey stateKey, bool active)
        {
            State state = getOrCreate(stateKey);
            if (state.active != active)
            {
                state.active = active;
                state.lastChangedTime = getCurrentTime();
                state.shouldAnimate = true;
            }
        }

        /**
         * Equivalent to calling {@code resetAnimationTime(StateKey.get(stateName));}
         *
         * @param stateName the string specifying the state key
         * @deprecated
         * @see #resetAnimationTime(de.matthiasmann.twl.renderer.AnimationState.StateKey) 
         * @see de.matthiasmann.twl.renderer.AnimationState.StateKey#get(java.lang.String)
         */
        //@Deprecated
        public void resetAnimationTime(String stateName)
        {
            resetAnimationTime(StateKey.Get(stateName));
        }

        /**
         * Resets the animation time of the specified animation state.
         * Resetting the animation time also enables the {@code shouldAnimate} flag.
         *
         * @param stateKey the state key.
         * @see #getAnimationTime(de.matthiasmann.twl.renderer.AnimationState.StateKey)
         * @see #getShouldAnimateState(de.matthiasmann.twl.renderer.AnimationState.StateKey) 
         */
        public void resetAnimationTime(StateKey stateKey)
        {
            State state = getOrCreate(stateKey);
            state.lastChangedTime = getCurrentTime();
            state.shouldAnimate = true;
        }

        /**
         * Equivalent to calling {@code dontAnimate(StateKey.get(stateName));}
         * 
         * @param stateName the string specifying the state key
         * @deprecated
         * @see #dontAnimate(de.matthiasmann.twl.renderer.AnimationState.StateKey) 
         * @see de.matthiasmann.twl.renderer.AnimationState.StateKey#get(java.lang.String)
         */
        //@Deprecated
        public void dontAnimate(String stateName)
        {
            dontAnimate(StateKey.Get(stateName));
        }

        /**
         * Clears the {@code shouldAnimate} flag of the specified animation state.
         *
         * @param stateKey the state key.
         * @see #getShouldAnimateState(de.matthiasmann.twl.renderer.AnimationState.StateKey)
         */
        public void dontAnimate(StateKey stateKey)
        {
            State state = getState(stateKey);
            if (state != null)
            {
                state.shouldAnimate = false;
            }
        }

        private State getState(StateKey stateKey)
        {
            int id = stateKey.ID;
            if (id < stateTable.Length)
            {
                return stateTable[id];
            }
            return null;
        }

        private State getOrCreate(StateKey stateKey)
        {
            int id = stateKey.ID;
            if (id < stateTable.Length)
            {
                State state = stateTable[id];
                if (state != null)
                {
                    return state;
                }
            }
            return createState(id);
        }

        private State createState(int id)
        {
            if (id >= stateTable.Length)
            {
                State[] newTable = new State[id + 1];
                Array.Copy(stateTable, 0, newTable, 0, stateTable.Length);
                stateTable = newTable;
            }
            State state = new State();
            state.lastChangedTime = getCurrentTime();
            stateTable[id] = state;
            return state;
        }

        private long getCurrentTime()
        {
            return (gui != null) ? gui.curTime : 0;
        }

        public class State
        {
            public long lastChangedTime;
            public bool active;
            public bool shouldAnimate;
        }
    }
}
