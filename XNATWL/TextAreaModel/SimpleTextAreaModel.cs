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
using System.Collections;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;
using static XNATWL.Utils.SparseGrid;

namespace XNATWL.TextAreaModel
{
    /// <summary>
    /// A simple text area model which represents the complete text as a single paragraph.
    ///  
    /// <para>The initial style is an empty style - see <see cref="XNATWL.TextAreaModel.Style()"/>. It can be changed before setting the text.</para>
    /// </summary>
    public class SimpleTextAreaModel : TextAreaModel
    {
        /// <summary>
        /// Change detected in the TextArea
        /// </summary>
        public event EventHandler<TextAreaChangedEventArgs> Changed;

        private Style _style;
        private Element _element;

        /// <summary>
        /// A <see cref="SimpleTextAreaModel"/> without the text element instantiated.
        /// </summary>
        public SimpleTextAreaModel()
        {
            _style = new Style();
        }

        /// <summary>
        /// Constructs a <see cref="SimpleTextAreaModel"/> with pre-formatted text. Use <c>\n</c> to create line breaks.
        /// </summary>
        /// <param name="text">Text to display</param>
        public SimpleTextAreaModel(string text) : this()
        {
            Text = text;
        }

        /// <summary>
        /// Will set the text for this <see cref="SimpleTextAreaModel"/> as pre-formatted text.
        /// </summary>
        public string Text
        {
            get
            {
                return ((TextElement)this._element).Text;
            }

            set
            {
                this.SetText(value, true);
            }
        }

        /// <summary>
        /// Returns the style used for the next call to <see cref="SetText(string, bool)"/>
        /// </summary>
        public Style Style
        {
            get
            {
                return _style;
            }

            set
            {
                _style = value;
            }
        }

        /// <summary>
        /// Sets the text for this SimpleTextAreaModel. Use <c>\n</c> to create line breaks.
        /// <para>The <string>preformatted</string> will set the white space attribute as follows:
        /// <code>false = { white-space: normal }<br/>true  = { white-space: pre }</code></para>
        /// </summary>
        /// <param name="text">Text to display</param>
        /// <param name="preformatted">Preformatted tag</param>
        public void SetText(string text, bool preformatted)
        {
            Style textstyle = _style.With(StyleAttribute.PREFORMATTED, preformatted);
            this._element = new TextElement(textstyle, text);
            if (this.Changed != null)
            {
                this.Changed.Invoke(this, new TextAreaChangedEventArgs());
            }
        }

        /// <summary>
        /// Iterate a list containing just the text element
        /// </summary>
        /// <returns>Iterator on one element</returns>
        public IEnumerator<Element> GetEnumerator()
        {
            return new List<Element> { _element }.GetEnumerator();
        }

        /// <summary>
        /// Iterate a list containing just the text element
        /// </summary>
        /// <returns>Iterator on one element</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new List<Element> { _element }.GetEnumerator();
        }
    }
}
