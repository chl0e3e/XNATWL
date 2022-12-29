using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public class SimpleAutoCompletionResult : AutoCompletionResult
    {
        public override int Results
        {
            get
            {
                return this._results.Count();
            }
        }

        private string[] _results;

        public SimpleAutoCompletionResult(string text, int prefixLength, ICollection<string> results) : base(text, prefixLength)
        {
            this._results = results.ToArray();
        }

        public SimpleAutoCompletionResult(string text, int prefixLength, params string[] results) : base(text, prefixLength)
        {
            this._results = (string[]) results.Clone();
        }

        public override string ResultAt(int index)
        {
            return this._results[index];
        }
    }
}
