using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public class DefaultEditFieldModel : EditFieldModel
    {
        private StringBuilder stringBuilder;

        public event EventHandler<CharSequenceChangedEventArgs> CharSequenceChanged;

        public DefaultEditFieldModel()
        {
            this.stringBuilder = new StringBuilder();
        }

        public int Length
        {
            get
            {
                return stringBuilder.Length;
            }
        }

        public char CharAt(int index)
        {
            return stringBuilder[index];
        }

        private void CheckRange(int start, int count)
        {
            int len = this.Length;

            if (start < 0 || start > len)
            {
                throw new IndexOutOfRangeException();
            }

            if (count < 0 || count > len - start)
            {
                throw new IndexOutOfRangeException();
            }
        }

        public int Replace(int start, int count, string replacement)
        {
            CheckRange(start, count);

            int replacementLength = replacement.Length;

            if (count > 0 || replacementLength > 0)
            {
                this.stringBuilder.Replace(this.stringBuilder.ToString(start, start + count), replacement, start, 1);
                this.CharSequenceChanged.Invoke(this, new CharSequenceChangedEventArgs(start, count, replacementLength));
            }

            return replacementLength;
        }

        public bool Replace(int start, int count, char replacement)
        {
            CheckRange(start, count);

            if (count == 0)
            {
                this.stringBuilder.Insert(start, replacement);
            }
            else
            {
                this.stringBuilder.Remove(start, count - 1); // TODO: Does the minus one actually mean anything? It is unclear.
                this.stringBuilder.Insert(start, replacement);
            }

            this.CharSequenceChanged.Invoke(this, new CharSequenceChangedEventArgs(start, count, 1));
            return true;
        }

        public string SubSequence(int start, int end)
        {
            return this.stringBuilder.ToString(start, end - start);
        }

        public string Substring(int start, int end)
        {
            return this.stringBuilder.ToString(start, end - start);
        }

        public override string ToString()
        {
            return this.stringBuilder.ToString();
        }
    }
}
