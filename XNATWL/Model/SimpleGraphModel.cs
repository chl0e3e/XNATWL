using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Model
{
    public class SimpleGraphModel : GraphModel
    {
        private List<GraphLineModel> lines;
        private bool scaleLinesIndependent;

        public SimpleGraphModel()
        {
            lines = new List<GraphLineModel>();
        }

        public SimpleGraphModel(GraphLineModel[] lines) : this(lines.ToList())
        {
            
        }

        public SimpleGraphModel(ICollection<GraphLineModel> lines)
        {
            this.lines = new List<GraphLineModel>(lines);
        }

        public GraphLineModel LineAt(int idx)
        {
            return this.lines[idx];
        }

        public int Lines
        {
            get
            {
                return this.lines.Count;
            }
        }

        public bool ScaleLinesIndependent()
        {
            return this.scaleLinesIndependent;
        }

        public void SetScaleLinesIndependent(bool val)
        {
            this.scaleLinesIndependent = val;
        }

        /**
         * Adds a new line at the end of the list
         * @param line the new line
         */
        public void AddLine(GraphLineModel line)
        {
            InsertLine(this.lines.Count, line);
        }

        /**
         * Inserts a new line before the specified index in the list
         * @param idx the index before which the new line will be inserted
         * @param line the new line
         * @throws NullPointerException if line is null
         * @throws IllegalArgumentException if the line is already part of this model
         */
        public void InsertLine(int idx, GraphLineModel line)
        {
            if (IndexOfLine(line) >= 0)
            {
                throw new ArgumentOutOfRangeException("line already added");
            }

            this.lines.Insert(idx, line);
        }

        /**
         * Returns the index of the specified line in this list or -1 if not found.
         * @param line the line to locate
         * @return the index or -1 if not found
         */
        public int IndexOfLine(GraphLineModel line)
        {
            return this.lines.IndexOf(line);
        }

        /**
         * Removes the line at the specified index
         * @param idx the index of the line to remove
         * @return the line that was removed
         */
        public GraphLineModel RemoveLine(int idx)
        {
            GraphLineModel lineModel = this.lines[idx];
            this.lines.RemoveAt(idx);
            return lineModel;
        }
    }
}
