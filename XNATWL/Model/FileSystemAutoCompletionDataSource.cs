using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public class FileSystemAutoCompletionDataSource : AutoCompletionDataSource
    {
        internal FileSystemModel _fileSystemModel;
        FileFilter _fileFilter;

        public FileSystemAutoCompletionDataSource(FileSystemModel fileSystemModel, FileFilter fileFilter)
        {
            if (fileSystemModel == null)
            {
                throw new NullReferenceException("fsm");
            }

            this._fileSystemModel = fileSystemModel;
            this._fileFilter = fileFilter;
        }

        public AutoCompletionResult CollectSuggestions(string text, int cursorPos, AutoCompletionResult prev)
        {
            text = text.Substring(0, cursorPos);
            int prefixLength = ComputePrefixLength(text);
            String prefix = text.Substring(0, prefixLength);
            Object parent;

            if ((prev is Result) &&
                    prev.PrefixLength == prefixLength &&
                    prev.Text.StartsWith(prefix)) {
                parent = ((Result)prev)._parent;
            }
            else
            {
                parent = this._fileSystemModel.FileByPath(prefix);
            }

            if (parent == null)
            {
                return null;
            }

            Result result = new Result(this, text, prefixLength, parent);
            this._fileSystemModel.ListFolder(parent, result);

            if (result.Results == 0)
            {
                return null;
            }

            return result;
        }

        int ComputePrefixLength(string text)
        {
            string separator = this._fileSystemModel.Separator;
            int prefixLength = text.LastIndexOf(separator) + separator.Length;

            if (prefixLength < 0)
            {
                prefixLength = 0;
            }

            return prefixLength;
        }

        class Result : AutoCompletionResult, FileFilter
        {
            internal object _parent;
            string _nameFilter;
            FileSystemAutoCompletionDataSource _source;

            List<string> results1 = new List<string>();
            List<string> results2 = new List<string>();

            public Result(FileSystemAutoCompletionDataSource source, string text, int prefixLength, object parent) : base(text, prefixLength)
            {
                this._source = source;
                this._parent = parent;
                this._nameFilter = text.Substring(prefixLength).ToUpper();
            }

            public bool Accept(Object file)
            {
                FileFilter ff = this._source._fileFilter;
                if (ff == null || ff.Accept(file))
                {
                    int idx = MatchIndex(this._source._fileSystemModel.NameOf(file));
                    if (idx >= 0)
                    {
                        AddName(this._source._fileSystemModel.PathOf(file), idx);
                    }
                }

                return false;
            }

            private int MatchIndex(String partName)
            {
                return partName.ToUpper().IndexOf(_nameFilter);
            }

            private void AddName(String fullName, int matchIdx)
            {
                if (matchIdx == 0)
                {
                    results1.Add(fullName);
                }
                else if (matchIdx > 0)
                {
                    results2.Add(fullName);
                }
            }

            private void AddFilteredNames(List<String> results)
            {
                for (int i = 0, n = results.Count; i < n; i++)
                {
                    String fullName = results[i];
                    int idx = MatchIndex(fullName.Substring(base.PrefixLength));
                    AddName(fullName, idx);
                }
            }

            public override int Results
            {
                get
                {
                    return results1.Count + results2.Count;
                }
            }

            public override string ResultAt(int idx)
            {
                int size1 = results1.Count;
                if (idx >= size1)
                {
                    return results2[idx - size1];
                }
                else
                {
                    return results1[idx];
                }
            }

            bool CanRefine(string text)
            {
                return base.PrefixLength == this._source.ComputePrefixLength(text) && text.StartsWith(this.Text);
            }

            public AutoCompletionResult Refine(String text, int cursorPos)
            {
                text = text.Substring(0, cursorPos);
                if (CanRefine(text))
                {
                    Result result = new Result(this._source, text, base.PrefixLength, _parent);
                    result.AddFilteredNames(results1);
                    result.AddFilteredNames(results2);
                    return result;
                }
                return null;
            }
        }
    }
}
