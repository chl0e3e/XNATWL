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
using System.Runtime.Serialization;
using XNATWL.IO;
using static XNATWL.Utils.SparseGrid;

namespace XNATWL.Model
{
    /// <summary>
    /// <para>A persistent MRU list model.</para>
    /// <para>Entries are stored compressed (deflate) using serialization and putByteArray except Strings which use<code> put</code></para>
    /// </summary>
    /// <typeparam name="T">the data type stored in this MRU model</typeparam>
    public class PersistentMRUListModel<T> : SimpleMRUListModel<T> where T : ISerializable
    {
        private Preferences _preferences;
        private string _preferenceKey;
        private Type _type;

        public PersistentMRUListModel(int maxEntries, Type type, Preferences preferences, string preferenceKey) : base(maxEntries)
        {
            this._preferences = preferences;
            this._preferenceKey = preferenceKey;
            this._type = type;

            int numEntries = Math.Min((int)this._preferences.Get(KeyForNumEntries(), 0), maxEntries);

            for (int i = 0; i < numEntries; i++)
            {
                object entry = this._preferences.Get(KeyForIndex(i), null);

                if (entry != null)
                {
                    this._entries.Add((T) entry);
                }
            }
        }

        protected override void Save()
        {
            int numEntries = Math.Min((int)this._preferences.Get(KeyForNumEntries(), 0), this._maxEntries);

            for (int i = 0; i < numEntries; i++)
            {
                object entry = this._preferences.Get(KeyForIndex(i), null);

                if (entry != null && !entry.Equals(this._entries[i]))
                {
                    this._preferences.Set(KeyForIndex(i), entry);
                }
            }

            this._preferences.Set(KeyForNumEntries(), this.Entries);
        }

        protected string KeyForIndex(int idx)
        {
            return this._preferenceKey + "_" + idx;
        }

        protected string KeyForNumEntries()
        {
            return this._preferenceKey + "_entries";
        }
    }
}
