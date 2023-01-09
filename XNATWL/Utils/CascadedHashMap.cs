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

using System.Collections.Generic;
using System.Linq;

namespace XNATWL.Utils
{
    public class CascadedHashMap<K, Object> : Dictionary<K, object>
    {
        public CascadedHashMap<K, Object> fallback = null;

        public CascadedHashMap()
        {
        }

        public void CollapseAndSetFallback(CascadedHashMap<K, Object> map)
        {
            if (fallback != null)
            {
                this.CollapsePutAll(map);
                fallback = null;
            }

            fallback = map;
        }

        public void CollapsePutAll(CascadedHashMap<K, Object> map)
        {
            do
            {
                K[] mapKeys = map.Keys.ToArray();
                for (int i = 0, n = mapKeys.Count(); i < n; i++)
                {
                    object mapEntry = map[mapKeys[i]];
                    if (mapEntry != null)
                    {
                        if (!this.ContainsKey(mapKeys[i]))
                        {
                            this[mapKeys[i]] = mapEntry;
                        }
                    }
                }
                map = map.fallback;
            } while (map != null);
        }

        public object CascadingEntry(K key)
        {
            return getEntry(this, key);
        }

        public object PutCascadingEntry(K key, object value)
        {
            if (this.ContainsKey(key))
            {
                object oldValue = this[key];
                this[key] = value;
                return oldValue;
            }
            else
            {
                object cascadedEntry = CascadingEntry(key);
                this[key] = value;
                return cascadedEntry;
            }
        }

        protected static object getEntry<K>(CascadedHashMap<K, Object> map, K key)
        {
            do
            {
                if (map.ContainsKey(key) && map[key] != null)
                {
                    return map[key];
                }
                map = map.fallback;
            } while (map != null);

            return null;
        }
    }
}
