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
using System.Text;
using XNATWL.IO;

namespace XNATWL
{

    public class ThemeException : Exception
    {
        protected Source source;

        public ThemeException(string msg, FileSystemObject fso, int lineNumber, int columnNumber, Exception cause) : base(msg, cause)
        {
            this.source = new Source(fso, lineNumber, columnNumber);
        }

        internal void addIncludedBy(FileSystemObject fso, int lineNumber, int columnNumber)
        {
            Source head = source;
            while (head.includedBy != null)
            {
                head = head.includedBy;
            }
            head.includedBy = new Source(fso, lineNumber, columnNumber);
        }

        public String getMessage()
        {
            StringBuilder sb = new StringBuilder(base.Message);
            String prefix = "\n           in ";
            for (Source src = source; src != null; src = src.includedBy)
            {
                sb.Append(prefix).Append(src.fso)
                        .Append(" @").Append(src.lineNumber)
                        .Append(':').Append(src.columnNumber);
                prefix = "\n  included by ";
            }
            return sb.ToString();
        }

        /**
         * Returns the source URL of the XML file and the line/column number
         * where the exception originated.
         * @return the source
         */
        public Source getSource()
        {
            return source;
        }

        /**
         * Describes a position in an XML file
         */
        public class Source
        {
            internal FileSystemObject fso;
            internal int lineNumber;
            internal int columnNumber;
            internal Source includedBy;

            internal Source(FileSystemObject fso, int lineNumber, int columnNumber)
            {
                this.fso = fso;
                this.lineNumber = lineNumber;
                this.columnNumber = columnNumber;
            }

            public FileSystemObject getFso()
            {
                return fso;
            }

            public int getLineNumber()
            {
                return lineNumber;
            }

            public int getColumnNumber()
            {
                return columnNumber;
            }

            public Source getIncludedBy()
            {
                return includedBy;
            }
        }
    }

}
