using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
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
