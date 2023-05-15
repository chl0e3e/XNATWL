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
        protected ThemeExceptionSource _source;

        public ThemeException(string msg, FileSystemObject fso, int lineNumber, int columnNumber, Exception cause) : base(msg, cause)
        {
            this._source = new ThemeExceptionSource(fso, lineNumber, columnNumber);
        }

        internal void AddIncludedBy(FileSystemObject fso, int lineNumber, int columnNumber)
        {
            ThemeExceptionSource head = _source;
            while (head._includedBy != null)
            {
                head = head._includedBy;
            }
            head._includedBy = new ThemeExceptionSource(fso, lineNumber, columnNumber);
        }

        public String GetMessage()
        {
            StringBuilder sb = new StringBuilder(base.Message);
            String prefix = "\n           in ";
            for (ThemeExceptionSource src = _source; src != null; src = src._includedBy)
            {
                sb.Append(prefix).Append(src._fileSystemObject)
                        .Append(" @").Append(src._lineNumber)
                        .Append(':').Append(src._columnNumber);
                prefix = "\n  included by ";
            }
            return sb.ToString();
        }

        /**
         * Returns the source URL of the XML file and the line/column number
         * where the exception originated.
         * @return the source
         */
        public ThemeExceptionSource GetSource()
        {
            return _source;
        }

        /**
         * Describes a position in an XML file
         */
        public class ThemeExceptionSource
        {
            internal FileSystemObject _fileSystemObject;
            internal int _lineNumber;
            internal int _columnNumber;
            internal ThemeExceptionSource _includedBy;

            internal ThemeExceptionSource(FileSystemObject fso, int lineNumber, int columnNumber)
            {
                this._fileSystemObject = fso;
                this._lineNumber = lineNumber;
                this._columnNumber = columnNumber;
            }

            public FileSystemObject GetFso()
            {
                return _fileSystemObject;
            }

            public int GetLineNumber()
            {
                return _lineNumber;
            }

            public int GetColumnNumber()
            {
                return _columnNumber;
            }

            public ThemeExceptionSource GetIncludedBy()
            {
                return _includedBy;
            }
        }
    }
}
