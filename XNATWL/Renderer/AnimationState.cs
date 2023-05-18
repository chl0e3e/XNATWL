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
using System.Collections.Generic;

namespace XNATWL.Renderer
{
    /// <summary>
    /// Time source for animations.
    /// </summary>
    public interface AnimationState
    {
        /// <summary>
        /// <para>Returns the time since the specified state has changed in ms.</para>
        /// <para>If the specified state was never changed then a free running time is returned.</para>
        /// </summary>
        /// <param name="state">the state key</param>
        /// <returns>time since last state change is ms</returns>
        int GetAnimationTime(StateKey state);

        /// <summary>
        /// Checks if the given state is active.
        /// </summary>
        /// <param name="state">the state key</param>
        /// <returns><strong>true</strong> if the state is set</returns>
        bool GetAnimationState(StateKey state);

        /// <summary>
        /// <para>Checks if this state was changed based on user interaction or not.</para>
        /// <para>If this method returns false then the animation time should not be used for single shot animations.</para>
        /// </summary>
        /// <param name="state">the state key.</param>
        /// <returns><strong>true</strong> if single shot animations should run or not</returns>
        bool ShouldAnimateState(StateKey state);
    }

    /// <summary>
    /// An animation state key which maps each animation state name to
    /// an unique ID. This allows to implement faster lookups based on
    /// the unique ID instead of performing a string lookup.
    /// </summary>
    public class StateKey
    {
        private String _name;
        private int _id;

        private static Dictionary<String, StateKey> KEYS = new Dictionary<String, StateKey>();
        private static List<StateKey> KEYS_BY_ID = new List<StateKey>();

        private StateKey(String name, int id)
        {
            this._name = name;
            this._id = id;
        }

        /// <summary>
        /// The name of this animation state key
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// The unique ID of this StateKey.
        /// The first StateKey has ID 0.
        /// </summary>
        public int ID
        {
            get
            {
                return _id;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is StateKey) {
                StateKey other = (StateKey)obj;
                return this.ID == other.ID;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return _id;
        }

        /// <summary>
        /// Returns the StateKey for the specified name.
        /// The StateKey is created if it didn't exist.
        /// </summary>
        /// <param name="name">the name to look up</param>
        /// <returns>the StateKey - never null.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The name is an empty string</exception>
        public static StateKey Get(String name)
        {
            if (name.Length == 0)
            {
                throw new ArgumentOutOfRangeException("name");
            }

            if (!KEYS.ContainsKey(name))
            {
                StateKey key = new StateKey(name, KEYS.Count);
                KEYS.Add(name, key);
                KEYS_BY_ID.Add(key);
            }
            return KEYS[name];
        }

        /// <summary>
        /// Returns the StateKey for the specified id.
        /// </summary>
        /// <param name="id">the ID to lookup</param>
        /// <returns>the StateKey</returns>
        public static StateKey Get(int id)
        {
            return KEYS_BY_ID[id];
        }

        public static int StateKeys
        {
            get
            {
                return KEYS.Count;
            }
        }
    }
}
