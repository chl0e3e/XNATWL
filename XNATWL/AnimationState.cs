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
        private AnimationState _parent;

        private State[] _stateTable;
        private GUI _gui;

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
            this._parent = parent;
            this._stateTable = new State[size];
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

        public void SetGUI(GUI gui)
        {
            this._gui = gui;

            long curTime = GetCurrentTime();
            foreach (State s in _stateTable)
            {
                if (s != null)
                {
                    s.LastChangedTime = curTime;
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
            State state = GetState(stateKey);
            if (state != null)
            {
                long a = GetCurrentTime();
                long b = state.LastChangedTime;
                return (int)Math.Min(Int32.MaxValue, a - b);
            }
            if (_parent != null)
            {
                return _parent.GetAnimationTime(stateKey);
            }

            return (int)GetCurrentTime() & 2147483647;
        }

        /**
         * Checks if the given state is active.
         *
         * @param stateKey the state key.
         * @return true if the state is set
         */
        public bool GetAnimationState(StateKey stateKey)
        {
            State state = GetState(stateKey);
            if (state != null)
            {
                return state.Active;
            }

            if (_parent != null)
            {
                return _parent.GetAnimationState(stateKey);
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
            State state = GetState(stateKey);
            if (state != null)
            {
                return state.ShouldAnimate;
            }

            if (_parent != null)
            {
                return _parent.ShouldAnimateState(stateKey);
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
        public void SetAnimationState(String stateName, bool active)
        {
            SetAnimationState(StateKey.Get(stateName), active);
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
        public void SetAnimationState(StateKey stateKey, bool active)
        {
            State state = GetOrCreate(stateKey);
            if (state.Active != active)
            {
                state.Active = active;
                state.LastChangedTime = GetCurrentTime();
                state.ShouldAnimate = true;
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
        public void ResetAnimationTime(String stateName)
        {
            ResetAnimationTime(StateKey.Get(stateName));
        }

        /**
         * Resets the animation time of the specified animation state.
         * Resetting the animation time also enables the {@code shouldAnimate} flag.
         *
         * @param stateKey the state key.
         * @see #getAnimationTime(de.matthiasmann.twl.renderer.AnimationState.StateKey)
         * @see #getShouldAnimateState(de.matthiasmann.twl.renderer.AnimationState.StateKey) 
         */
        public void ResetAnimationTime(StateKey stateKey)
        {
            State state = GetOrCreate(stateKey);
            state.LastChangedTime = GetCurrentTime();
            state.ShouldAnimate = true;
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
        public void DontAnimate(String stateName)
        {
            DontAnimate(StateKey.Get(stateName));
        }

        /**
         * Clears the {@code shouldAnimate} flag of the specified animation state.
         *
         * @param stateKey the state key.
         * @see #getShouldAnimateState(de.matthiasmann.twl.renderer.AnimationState.StateKey)
         */
        public void DontAnimate(StateKey stateKey)
        {
            State state = GetState(stateKey);
            if (state != null)
            {
                state.ShouldAnimate = false;
            }
        }

        private State GetState(StateKey stateKey)
        {
            int id = stateKey.ID;
            if (id < _stateTable.Length)
            {
                return _stateTable[id];
            }
            return null;
        }

        private State GetOrCreate(StateKey stateKey)
        {
            int id = stateKey.ID;
            if (id < _stateTable.Length)
            {
                State state = _stateTable[id];
                if (state != null)
                {
                    return state;
                }
            }
            return CreateState(id);
        }

        private State CreateState(int id)
        {
            if (id >= _stateTable.Length)
            {
                State[] newTable = new State[id + 1];
                Array.Copy(_stateTable, 0, newTable, 0, _stateTable.Length);
                _stateTable = newTable;
            }
            State state = new State();
            state.LastChangedTime = GetCurrentTime();
            _stateTable[id] = state;
            return state;
        }

        private long GetCurrentTime()
        {
            return (_gui != null) ? _gui._curTime : 0;
        }

        public class State
        {
            public long LastChangedTime;
            public bool Active;
            public bool ShouldAnimate;
        }
    }
}
