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

namespace XNATWL.Utils
{
    public class SparseGrid
    {
        public interface GridFunction
        {
            void Apply(int row, int column, Entry e);
        }

        Node _root;
        int _numLevels;

        public SparseGrid(int pageSize)
        {
            _root = new Node(pageSize);
            _numLevels = 1;
        }

        public Entry Get(int row, int column)
        {
            if (_root._size > 0)
            {
                int levels = _numLevels;
                Entry e = _root;

                do
                {
                    Node node = (Node)e;
                    int pos = node.FindPos(row, column, node._size);
                    if (pos == node._size)
                    {
                        return null;
                    }
                    e = node._children[pos];
                } while (--levels > 0);

                System.Diagnostics.Debug.Assert(e != null);
                if (e.Compare(row, column) == 0)
                {
                    return e;
                }
            }
            return null;
        }

        public void Set(int row, int column, Entry entry)
        {
            entry._row = row;
            entry._column = column;

            if (_root._size == 0)
            {
                _root.InsertAt(0, entry);
                _root.UpdateRowColumn();
            }
            else if (!_root.Insert(entry, _numLevels))
            {
                SplitRoot();
                _root.Insert(entry, _numLevels);
            }
        }

        public Entry Remove(int row, int column)
        {
            if (_root._size == 0)
            {
                return null;
            }
            Entry e = _root.Remove(row, column, _numLevels);
            if (e != null)
            {
                MaybeRemoveRoot();
            }
            return e;
        }

        public void InsertRows(int row, int count)
        {
            if (count > 0 && _root._size > 0)
            {
                _root.InsertRows(row, count, _numLevels);
            }
        }

        public void InsertColumns(int column, int count)
        {
            if (count > 0 && _root._size > 0)
            {
                _root.InsertColumns(column, count, _numLevels);
            }
        }

        public void RemoveRows(int row, int count)
        {
            if (count > 0)
            {
                _root.RemoveRows(row, count, _numLevels);
                MaybeRemoveRoot();
            }
        }

        public void RemoveColumns(int column, int count)
        {
            if (count > 0)
            {
                _root.RemoveColumns(column, count, _numLevels);
                MaybeRemoveRoot();
            }
        }

        public void Iterate(int startRow, int startColumn,
                int endRow, int endColumn, GridFunction func)
        {
            if (_root._size > 0)
            {
                int levels = _numLevels;
                Entry e = _root;
                Node node;
                int pos;

                do
                {
                    node = (Node)e;
                    pos = node.FindPos(startRow, startColumn, node._size - 1);
                    e = node._children[pos];
                } while (--levels > 0);

                System.Diagnostics.Debug.Assert(e != null);
                if (e.Compare(startRow, startColumn) < 0)
                {
                    return;
                }

                do
                {
                    for (int size = node._size; pos < size; pos++)
                    {
                        e = node._children[pos];
                        if (e._row > endRow)
                        {
                            return;
                        }
                        if (e._column >= startColumn && e._column <= endColumn)
                        {
                            func.Apply(e._row, e._column, e);
                        }
                    }
                    pos = 0;
                    node = node._next;
                } while (node != null);
            }
        }

        public bool IsEmpty()
        {
            return _root._size == 0;
        }

        public void Clear()
        {
            for (int i = 0; i < _root._children.Length; i++)
            {
                _root._children[i] = null;
            }
            _root._size = 0;
            _numLevels = 1;
        }

        private void MaybeRemoveRoot()
        {
            while (_numLevels > 1 && _root._size == 1)
            {
                _root = (Node)_root._children[0];
                _root._prev = null;
                _root._next = null;
                _numLevels--;
            }
            if (_root._size == 0)
            {
                _numLevels = 1;
            }
        }

        private void SplitRoot()
        {
            Node newNode = _root.Split();
            Node newRoot = new Node(_root._children.Length);
            newRoot._children[0] = _root;
            newRoot._children[1] = newNode;
            newRoot._size = 2;
            _root = newRoot;
            _numLevels++;
        }

        public class Node : Entry
        {
            protected internal Entry[] _children;
            protected internal int _size;
            protected internal Node _next;
            protected internal Node _prev;

            public Node(int size)
            {
                this._children = new Entry[size];
            }

            protected internal bool Insert(Entry e, int levels)
            {
                if (--levels == 0)
                {
                    return InsertLeaf(e);
                }

                for (; ; )
                {
                    int position = FindPos(e._row, e._column, _size - 1);
                    System.Diagnostics.Debug.Assert(position < _size);
                    Node node = (Node)_children[position];
                    if (!node.Insert(e, levels))
                    {
                        if (IsFull())
                        {
                            return false;
                        }
                        Node node2 = node.Split();
                        InsertAt(position + 1, node2);
                        continue;
                    }
                    UpdateRowColumn();
                    return true;
                }
            }

            protected internal bool InsertLeaf(Entry e)
            {
                int pos = FindPos(e._row, e._column, _size);
                if (pos < _size)
                {
                    Entry c = _children[pos];
                    System.Diagnostics.Debug.Assert(c.GetType() != typeof(Node));
                    int cmp = c.Compare(e._row, e._column);
                    if (cmp == 0)
                    {
                        _children[pos] = e;
                        return true;
                    }
                    System.Diagnostics.Debug.Assert(cmp > 0);
                }

                if (IsFull())
                {
                    return false;
                }
                InsertAt(pos, e);
                return true;
            }

            protected internal Entry Remove(int row, int column, int levels)
            {
                if (--levels == 0)
                {
                    return RemoveLeaf(row, column);
                }

                int pos = FindPos(row, column, _size - 1);
                System.Diagnostics.Debug.Assert(pos < _size);
                Node node = (Node)_children[pos];
                Entry e = node.Remove(row, column, levels);
                if (e != null)
                {
                    if (node._size == 0)
                    {
                        RemoveNodeAt(pos);
                    }
                    else if (node.IsBelowHalf())
                    {
                        TryMerge(pos);
                    }
                    UpdateRowColumn();
                }
                return e;
            }

            protected internal Entry RemoveLeaf(int row, int column)
            {
                int pos = FindPos(row, column, _size);
                if (pos == _size)
                {
                    return null;
                }

                Entry c = _children[pos];
                System.Diagnostics.Debug.Assert(c.GetType() != typeof(Node));
                int cmp = c.Compare(row, column);
                if (cmp == 0)
                {
                    RemoveAt(pos);
                    if (pos == _size && _size > 0)
                    {
                        UpdateRowColumn();
                    }
                    return c;
                }
                return null;
            }

            protected internal int FindPos(int row, int column, int high)
            {
                int low = 0;
                while (low < high)
                {
                    int mid = BitOperations.RightMove((low + high), 1);
                    Entry e = _children[mid];
                    int cmp = e.Compare(row, column);
                    if (cmp > 0)
                    {
                        high = mid;
                    }
                    else if (cmp < 0)
                    {
                        low = mid + 1;
                    }
                    else
                    {
                        return mid;
                    }
                }
                return low;
            }

            protected internal void InsertRows(int row, int count, int levels)
            {
                if (--levels > 0)
                {
                    for (int i = _size; i-- > 0;)
                    {
                        Node n = (Node)_children[i];
                        if (n._row < row)
                        {
                            break;
                        }
                        n.InsertRows(row, count, levels);
                    }
                }
                else
                {
                    for (int i = _size; i-- > 0;)
                    {
                        Entry e = _children[i];
                        if (e._row < row)
                        {
                            break;
                        }
                        e._row += count;
                    }
                }
                UpdateRowColumn();
            }

            protected internal void InsertColumns(int column, int count, int levels)
            {
                if (--levels > 0)
                {
                    for (int i = 0; i < _size; i++)
                    {
                        Node n = (Node)_children[i];
                        n.InsertColumns(column, count, levels);
                    }
                }
                else
                {
                    for (int i = 0; i < _size; i++)
                    {
                        Entry e = _children[i];
                        if (e._column >= column)
                        {
                            e._column += count;
                        }
                    }
                }
                UpdateRowColumn();
            }

            protected internal bool RemoveRows(int row, int count, int levels)
            {
                if (--levels > 0)
                {
                    bool needsMerging = false;
                    for (int i = _size; i-- > 0;)
                    {
                        Node n = (Node)_children[i];
                        if (n._row < row)
                        {
                            break;
                        }
                        if (n.RemoveRows(row, count, levels))
                        {
                            RemoveNodeAt(i);
                        }
                        else
                        {
                            needsMerging |= n.IsBelowHalf();
                        }
                    }
                    if (needsMerging && _size > 1)
                    {
                        TryMerge();
                    }
                }
                else
                {
                    for (int i = _size; i-- > 0;)
                    {
                        Entry e = _children[i];
                        if (e._row < row)
                        {
                            break;
                        }
                        e._row -= count;
                        if (e._row < row)
                        {
                            RemoveAt(i);
                        }
                    }
                }
                if (_size == 0)
                {
                    return true;
                }
                UpdateRowColumn();
                return false;
            }

            protected internal bool RemoveColumns(int column, int count, int levels)
            {
                if (--levels > 0)
                {
                    bool needsMerging = false;
                    for (int i = _size; i-- > 0;)
                    {
                        Node n = (Node)_children[i];
                        if (n.RemoveColumns(column, count, levels))
                        {
                            RemoveNodeAt(i);
                        }
                        else
                        {
                            needsMerging |= n.IsBelowHalf();
                        }
                    }
                    if (needsMerging && _size > 1)
                    {
                        TryMerge();
                    }
                }
                else
                {
                    for (int i = _size; i-- > 0;)
                    {
                        Entry e = _children[i];
                        if (e._column >= column)
                        {
                            e._column -= count;
                            if (e._column < column)
                            {
                                RemoveAt(i);
                            }
                        }
                    }
                }
                if (_size == 0)
                {
                    return true;
                }
                UpdateRowColumn();
                return false;
            }

            protected internal void InsertAt(int idx, Entry what)
            {
                Array.Copy(_children, idx, _children, idx + 1, _size - idx);
                _children[idx] = what;
                if (idx == _size++)
                {
                    UpdateRowColumn();
                }
            }

            protected internal void RemoveAt(int idx)
            {
                _size--;
                Array.Copy(_children, idx + 1, _children, idx, _size - idx);
                _children[_size] = null;
            }

            protected internal void RemoveNodeAt(int idx)
            {
                Node n = (Node)_children[idx];
                if (n._next != null)
                {
                    n._next._prev = n._prev;
                }
                if (n._prev != null)
                {
                    n._prev._next = n._next;
                }
                n._next = null;
                n._prev = null;
                RemoveAt(idx);
            }

            protected internal void TryMerge()
            {
                if (_size == 2)
                {
                    TryMerge2(0);
                }
                else
                {
                    for (int i = _size - 1; i-- > 1;)
                    {
                        if (TryMerge3(i))
                        {
                            i--;
                        }
                    }
                }
            }

            protected internal void TryMerge(int pos)
            {
                switch (_size)
                {
                    case 0:
                    case 1:
                        // can't merge
                        break;
                    case 2:
                        TryMerge2(0);
                        break;
                    default:
                        if (pos + 1 == _size)
                        {
                            TryMerge3(pos - 1);
                        }
                        else if (pos == 0)
                        {
                            TryMerge3(1);
                        }
                        else
                        {
                            TryMerge3(pos);
                        }
                        break;
                }
            }

            private void TryMerge2(int pos)
            {
                Node n1 = (Node)_children[pos];
                Node n2 = (Node)_children[pos + 1];
                if (n1.IsBelowHalf() || n2.IsBelowHalf())
                {
                    int sumSize = n1._size + n2._size;
                    if (sumSize < _children.Length)
                    {
                        Array.Copy(n2._children, 0, n1._children, n1._size, n2._size);
                        n1._size = sumSize;
                        n1.UpdateRowColumn();
                        RemoveNodeAt(pos + 1);
                    }
                    else
                    {
                        Object[] temp = Collect2(sumSize, n1, n2);
                        Distribute2(temp, n1, n2);
                    }
                }
            }

            private bool TryMerge3(int pos)
            {
                Node n0 = (Node)_children[pos - 1];
                Node n1 = (Node)_children[pos];
                Node n2 = (Node)_children[pos + 1];
                if (n0.IsBelowHalf() || n1.IsBelowHalf() || n2.IsBelowHalf())
                {
                    int sumSize = n0._size + n1._size + n2._size;
                    if (sumSize < _children.Length)
                    {
                        Array.Copy(n1._children, 0, n0._children, n0._size, n1._size);
                        Array.Copy(n2._children, 0, n0._children, n0._size + n1._size, n2._size);
                        n0._size = sumSize;
                        n0.UpdateRowColumn();
                        RemoveNodeAt(pos + 1);
                        RemoveNodeAt(pos);
                        return true;
                    }
                    else
                    {
                        Object[] temp = Collect3(sumSize, n0, n1, n2);
                        if (sumSize < 2 * _children.Length)
                        {
                            Distribute2(temp, n0, n1);
                            RemoveNodeAt(pos + 1);
                        }
                        else
                        {
                            Distribute3(temp, n0, n1, n2);
                        }
                    }
                }
                return false;
            }

            private Object[] Collect2(int sumSize, Node n0, Node n1)
            {
                Object[] temp = new Object[sumSize];
                Array.Copy(n0._children, 0, temp, 0, n0._size);
                Array.Copy(n1._children, 0, temp, n0._size, n1._size);
                return temp;
            }

            private Object[] Collect3(int sumSize, Node n0, Node n1, Node n2)
            {
                Object[] temp = new Object[sumSize];
                Array.Copy(n0._children, 0, temp, 0, n0._size);
                Array.Copy(n1._children, 0, temp, n0._size, n1._size);
                Array.Copy(n2._children, 0, temp, n0._size + n1._size, n2._size);
                return temp;
            }

            private void Distribute2(Object[] src, Node n0, Node n1)
            {
                int sumSize = src.Length;

                n0._size = sumSize / 2;
                n1._size = sumSize - n0._size;

                Array.Copy(src, 0, n0._children, 0, n0._size);
                Array.Copy(src, n0._size, n1._children, 0, n1._size);

                n0.UpdateRowColumn();
                n1.UpdateRowColumn();
            }

            private void Distribute3(Object[] src, Node n0, Node n1, Node n2)
            {
                int sumSize = src.Length;

                n0._size = sumSize / 3;
                n1._size = (sumSize - n0._size) / 2;
                n2._size = sumSize - (n0._size + n1._size);

                Array.Copy(src, 0, n0._children, 0, n0._size);
                Array.Copy(src, n0._size, n1._children, 0, n1._size);
                Array.Copy(src, n0._size + n1._size, n2._children, 0, n2._size);

                n0.UpdateRowColumn();
                n1.UpdateRowColumn();
                n2.UpdateRowColumn();
            }

            protected internal bool IsFull()
            {
                return _size == _children.Length;
            }

            protected internal bool IsBelowHalf()
            {
                return _size * 2 < _children.Length;
            }

            protected internal Node Split()
            {
                Node newNode = new Node(_children.Length);
                int size1 = _size / 2;
                int size2 = _size - size1;
                Array.Copy(this._children, size1, newNode._children, 0, size2);
                for (int i = size1; i < this._size; i++)
                {
                    this._children[i] = null;
                }
                newNode._size = size2;
                newNode.UpdateRowColumn();
                newNode._prev = this;
                newNode._next = this._next;
                this._size = size1;
                this.UpdateRowColumn();
                this._next = newNode;
                if (newNode._next != null)
                {
                    newNode._next._prev = newNode;
                }
                return newNode;
            }

            protected internal void UpdateRowColumn()
            {
                Entry e = _children[_size - 1];
                this._row = e._row;
                this._column = e._column;
            }
        }

        public class Entry
        {
            protected internal int _row;
            protected internal int _column;

            protected internal int Compare(int row, int column)
            {
                int diff = this._row - row;
                if (diff == 0)
                {
                    diff = this._column - column;
                }
                return diff;
            }
        }
    }

}
