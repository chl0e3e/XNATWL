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

namespace XNATWL.TextAreaModel
{
    /// <summary>
    /// Represents an inline-style key
    /// </summary>
    public class StyleSheetKey
    {
        private string _element;
        private string _className;
        private string _id;

        /// <summary>
        /// Create a new <see cref="StyleSheetKey"/>
        /// </summary>
        /// <param name="element">Element tag name</param>
        /// <param name="className">Element class name</param>
        /// <param name="id">Element ID</param>
        public StyleSheetKey(string element, string className, string id)
        {
            _element = element;
            _className = className;
            _id = id;
        }

        /// <summary>
        /// CSS class
        /// </summary>
        public string ClassName
        {
            get
            {
                return this._className;
            }
        }

        /// <summary>
        /// XHTML node tag name
        /// </summary>
        public string Element
        {
            get
            {
                return this._element;
            }
        }
        
        /// <summary>
        /// Unique XHTML DOM identifier
        /// </summary>
        public string ID
        {
            get
            {
                return this._id;
            }
        }

        public override int GetHashCode()
        {
            int hash = 7;
            hash = 53 * hash + (this.Element != null ? this.Element.GetHashCode() : 0);
            hash = 53 * hash + (this.ClassName != null ? this.ClassName.GetHashCode() : 0);
            hash = 53 * hash + (this.ID != null ? this.ID.GetHashCode() : 0);
            return hash;
        }

        /// <summary>
        /// Compare this with another <see cref="StyleSheetKey"/>
        /// </summary>
        /// <param name="what">object of comparison</param>
        /// <returns><strong>true</strong> if matched</returns>
        public bool Matches(StyleSheetKey what)
        {
            if (this._element != null && !this._element.Equals(what.Element))
            {
                return false;
            }

            if (this._className != null && !this._className.Equals(what.ClassName))
            {
                return false;
            }

            if (this._id != null && !this._id.Equals(what.ID))
            {
                return false;
            }

            return true;
        }

        public override bool Equals(object other)
        {
            if (other is StyleSheetKey)
            {
                StyleSheetKey otherKey = (StyleSheetKey)other;

                return ((this.Element == null) ? (otherKey.Element == null) : this.Element.Equals(otherKey.Element)) &&
                        ((this.ClassName == null) ? (otherKey.ClassName == null) : this.ClassName.Equals(otherKey.ClassName)) &&
                        ((this.ID == null) ? (otherKey.ID == null) : this.ID.Equals(otherKey.ID));
            }

            return false;
        }
    }
}
