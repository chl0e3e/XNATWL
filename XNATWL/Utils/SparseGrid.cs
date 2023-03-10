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
            void apply(int row, int column, Entry e);
        }

        Node root;
        int numLevels;

        public SparseGrid(int pageSize)
        {
            root = new Node(pageSize);
            numLevels = 1;
        }

        public Entry get(int row, int column)
        {
            if (root.size > 0)
            {
                int levels = numLevels;
                Entry e = root;

                do
                {
                    Node node = (Node)e;
                    int pos = node.findPos(row, column, node.size);
                    if (pos == node.size)
                    {
                        return null;
                    }
                    e = node.children[pos];
                } while (--levels > 0);

                System.Diagnostics.Debug.Assert(e != null);
                if (e.compare(row, column) == 0)
                {
                    return e;
                }
            }
            return null;
        }

        public void set(int row, int column, Entry entry)
        {
            entry.row = row;
            entry.column = column;

            if (root.size == 0)
            {
                root.insertAt(0, entry);
                root.updateRowColumn();
            }
            else if (!root.insert(entry, numLevels))
            {
                splitRoot();
                root.insert(entry, numLevels);
            }
        }

        public Entry remove(int row, int column)
        {
            if (root.size == 0)
            {
                return null;
            }
            Entry e = root.remove(row, column, numLevels);
            if (e != null)
            {
                maybeRemoveRoot();
            }
            return e;
        }

        public void insertRows(int row, int count)
        {
            if (count > 0 && root.size > 0)
            {
                root.insertRows(row, count, numLevels);
            }
        }

        public void insertColumns(int column, int count)
        {
            if (count > 0 && root.size > 0)
            {
                root.insertColumns(column, count, numLevels);
            }
        }

        public void removeRows(int row, int count)
        {
            if (count > 0)
            {
                root.removeRows(row, count, numLevels);
                maybeRemoveRoot();
            }
        }

        public void removeColumns(int column, int count)
        {
            if (count > 0)
            {
                root.removeColumns(column, count, numLevels);
                maybeRemoveRoot();
            }
        }

        public void iterate(int startRow, int startColumn,
                int endRow, int endColumn, GridFunction func)
        {
            if (root.size > 0)
            {
                int levels = numLevels;
                Entry e = root;
                Node node;
                int pos;

                do
                {
                    node = (Node)e;
                    pos = node.findPos(startRow, startColumn, node.size - 1);
                    e = node.children[pos];
                } while (--levels > 0);

                System.Diagnostics.Debug.Assert(e != null);
                if (e.compare(startRow, startColumn) < 0)
                {
                    return;
                }

                do
                {
                    for (int size = node.size; pos < size; pos++)
                    {
                        e = node.children[pos];
                        if (e.row > endRow)
                        {
                            return;
                        }
                        if (e.column >= startColumn && e.column <= endColumn)
                        {
                            func.apply(e.row, e.column, e);
                        }
                    }
                    pos = 0;
                    node = node.next;
                } while (node != null);
            }
        }

        public bool isEmpty()
        {
            return root.size == 0;
        }

        public void clear()
        {
            for (int i = 0; i < root.children.Length; i++)
            {
                root.children[i] = null;
            }
            root.size = 0;
            numLevels = 1;
        }

        private void maybeRemoveRoot()
        {
            while (numLevels > 1 && root.size == 1)
            {
                root = (Node)root.children[0];
                root.prev = null;
                root.next = null;
                numLevels--;
            }
            if (root.size == 0)
            {
                numLevels = 1;
            }
        }

        private void splitRoot()
        {
            Node newNode = root.split();
            Node newRoot = new Node(root.children.Length);
            newRoot.children[0] = root;
            newRoot.children[1] = newNode;
            newRoot.size = 2;
            root = newRoot;
            numLevels++;
        }

        public class Node : Entry
        {
            protected internal Entry[] children;
            protected internal int size;
            protected internal Node next;
            protected internal Node prev;

            public Node(int size)
            {
                this.children = new Entry[size];
            }

            protected internal bool insert(Entry e, int levels)
            {
                if (--levels == 0)
                {
                    return insertLeaf(e);
                }

                for (; ; )
                {
                    int position = findPos(e.row, e.column, size - 1);
                    System.Diagnostics.Debug.Assert(position < size);
                    Node node = (Node)children[position];
                    if (!node.insert(e, levels))
                    {
                        if (isFull())
                        {
                            return false;
                        }
                        Node node2 = node.split();
                        insertAt(position + 1, node2);
                        continue;
                    }
                    updateRowColumn();
                    return true;
                }
            }

            protected internal bool insertLeaf(Entry e)
            {
                int pos = findPos(e.row, e.column, size);
                if (pos < size)
                {
                    Entry c = children[pos];
                    System.Diagnostics.Debug.Assert(c.GetType() != typeof(Node));
                    int cmp = c.compare(e.row, e.column);
                    if (cmp == 0)
                    {
                        children[pos] = e;
                        return true;
                    }
                    System.Diagnostics.Debug.Assert(cmp > 0);
                }

                if (isFull())
                {
                    return false;
                }
                insertAt(pos, e);
                return true;
            }

            protected internal Entry remove(int row, int column, int levels)
            {
                if (--levels == 0)
                {
                    return removeLeaf(row, column);
                }

                int pos = findPos(row, column, size - 1);
                System.Diagnostics.Debug.Assert(pos < size);
                Node node = (Node)children[pos];
                Entry e = node.remove(row, column, levels);
                if (e != null)
                {
                    if (node.size == 0)
                    {
                        removeNodeAt(pos);
                    }
                    else if (node.isBelowHalf())
                    {
                        tryMerge(pos);
                    }
                    updateRowColumn();
                }
                return e;
            }

            protected internal Entry removeLeaf(int row, int column)
            {
                int pos = findPos(row, column, size);
                if (pos == size)
                {
                    return null;
                }

                Entry c = children[pos];
                System.Diagnostics.Debug.Assert(c.GetType() != typeof(Node));
                int cmp = c.compare(row, column);
                if (cmp == 0)
                {
                    removeAt(pos);
                    if (pos == size && size > 0)
                    {
                        updateRowColumn();
                    }
                    return c;
                }
                return null;
            }

            protected internal int findPos(int row, int column, int high)
            {
                int low = 0;
                while (low < high)
                {
                    int mid = BitOperations.RightMove((low + high), 1);
                    Entry e = children[mid];
                    int cmp = e.compare(row, column);
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

            protected internal void insertRows(int row, int count, int levels)
            {
                if (--levels > 0)
                {
                    for (int i = size; i-- > 0;)
                    {
                        Node n = (Node)children[i];
                        if (n.row < row)
                        {
                            break;
                        }
                        n.insertRows(row, count, levels);
                    }
                }
                else
                {
                    for (int i = size; i-- > 0;)
                    {
                        Entry e = children[i];
                        if (e.row < row)
                        {
                            break;
                        }
                        e.row += count;
                    }
                }
                updateRowColumn();
            }

            protected internal void insertColumns(int column, int count, int levels)
            {
                if (--levels > 0)
                {
                    for (int i = 0; i < size; i++)
                    {
                        Node n = (Node)children[i];
                        n.insertColumns(column, count, levels);
                    }
                }
                else
                {
                    for (int i = 0; i < size; i++)
                    {
                        Entry e = children[i];
                        if (e.column >= column)
                        {
                            e.column += count;
                        }
                    }
                }
                updateRowColumn();
            }

            protected internal bool removeRows(int row, int count, int levels)
            {
                if (--levels > 0)
                {
                    bool needsMerging = false;
                    for (int i = size; i-- > 0;)
                    {
                        Node n = (Node)children[i];
                        if (n.row < row)
                        {
                            break;
                        }
                        if (n.removeRows(row, count, levels))
                        {
                            removeNodeAt(i);
                        }
                        else
                        {
                            needsMerging |= n.isBelowHalf();
                        }
                    }
                    if (needsMerging && size > 1)
                    {
                        tryMerge();
                    }
                }
                else
                {
                    for (int i = size; i-- > 0;)
                    {
                        Entry e = children[i];
                        if (e.row < row)
                        {
                            break;
                        }
                        e.row -= count;
                        if (e.row < row)
                        {
                            removeAt(i);
                        }
                    }
                }
                if (size == 0)
                {
                    return true;
                }
                updateRowColumn();
                return false;
            }

            protected internal bool removeColumns(int column, int count, int levels)
            {
                if (--levels > 0)
                {
                    bool needsMerging = false;
                    for (int i = size; i-- > 0;)
                    {
                        Node n = (Node)children[i];
                        if (n.removeColumns(column, count, levels))
                        {
                            removeNodeAt(i);
                        }
                        else
                        {
                            needsMerging |= n.isBelowHalf();
                        }
                    }
                    if (needsMerging && size > 1)
                    {
                        tryMerge();
                    }
                }
                else
                {
                    for (int i = size; i-- > 0;)
                    {
                        Entry e = children[i];
                        if (e.column >= column)
                        {
                            e.column -= count;
                            if (e.column < column)
                            {
                                removeAt(i);
                            }
                        }
                    }
                }
                if (size == 0)
                {
                    return true;
                }
                updateRowColumn();
                return false;
            }

            protected internal void insertAt(int idx, Entry what)
            {
                Array.Copy(children, idx, children, idx + 1, size - idx);
                children[idx] = what;
                if (idx == size++)
                {
                    updateRowColumn();
                }
            }

            protected internal void removeAt(int idx)
            {
                size--;
                Array.Copy(children, idx + 1, children, idx, size - idx);
                children[size] = null;
            }

            protected internal void removeNodeAt(int idx)
            {
                Node n = (Node)children[idx];
                if (n.next != null)
                {
                    n.next.prev = n.prev;
                }
                if (n.prev != null)
                {
                    n.prev.next = n.next;
                }
                n.next = null;
                n.prev = null;
                removeAt(idx);
            }

            protected internal void tryMerge()
            {
                if (size == 2)
                {
                    tryMerge2(0);
                }
                else
                {
                    for (int i = size - 1; i-- > 1;)
                    {
                        if (tryMerge3(i))
                        {
                            i--;
                        }
                    }
                }
            }

            protected internal void tryMerge(int pos)
            {
                switch (size)
                {
                    case 0:
                    case 1:
                        // can't merge
                        break;
                    case 2:
                        tryMerge2(0);
                        break;
                    default:
                        if (pos + 1 == size)
                        {
                            tryMerge3(pos - 1);
                        }
                        else if (pos == 0)
                        {
                            tryMerge3(1);
                        }
                        else
                        {
                            tryMerge3(pos);
                        }
                        break;
                }
            }

            private void tryMerge2(int pos)
            {
                Node n1 = (Node)children[pos];
                Node n2 = (Node)children[pos + 1];
                if (n1.isBelowHalf() || n2.isBelowHalf())
                {
                    int sumSize = n1.size + n2.size;
                    if (sumSize < children.Length)
                    {
                        Array.Copy(n2.children, 0, n1.children, n1.size, n2.size);
                        n1.size = sumSize;
                        n1.updateRowColumn();
                        removeNodeAt(pos + 1);
                    }
                    else
                    {
                        Object[] temp = collect2(sumSize, n1, n2);
                        distribute2(temp, n1, n2);
                    }
                }
            }

            private bool tryMerge3(int pos)
            {
                Node n0 = (Node)children[pos - 1];
                Node n1 = (Node)children[pos];
                Node n2 = (Node)children[pos + 1];
                if (n0.isBelowHalf() || n1.isBelowHalf() || n2.isBelowHalf())
                {
                    int sumSize = n0.size + n1.size + n2.size;
                    if (sumSize < children.Length)
                    {
                        Array.Copy(n1.children, 0, n0.children, n0.size, n1.size);
                        Array.Copy(n2.children, 0, n0.children, n0.size + n1.size, n2.size);
                        n0.size = sumSize;
                        n0.updateRowColumn();
                        removeNodeAt(pos + 1);
                        removeNodeAt(pos);
                        return true;
                    }
                    else
                    {
                        Object[] temp = collect3(sumSize, n0, n1, n2);
                        if (sumSize < 2 * children.Length)
                        {
                            distribute2(temp, n0, n1);
                            removeNodeAt(pos + 1);
                        }
                        else
                        {
                            distribute3(temp, n0, n1, n2);
                        }
                    }
                }
                return false;
            }

            private Object[] collect2(int sumSize, Node n0, Node n1)
            {
                Object[] temp = new Object[sumSize];
                Array.Copy(n0.children, 0, temp, 0, n0.size);
                Array.Copy(n1.children, 0, temp, n0.size, n1.size);
                return temp;
            }

            private Object[] collect3(int sumSize, Node n0, Node n1, Node n2)
            {
                Object[] temp = new Object[sumSize];
                Array.Copy(n0.children, 0, temp, 0, n0.size);
                Array.Copy(n1.children, 0, temp, n0.size, n1.size);
                Array.Copy(n2.children, 0, temp, n0.size + n1.size, n2.size);
                return temp;
            }

            private void distribute2(Object[] src, Node n0, Node n1)
            {
                int sumSize = src.Length;

                n0.size = sumSize / 2;
                n1.size = sumSize - n0.size;

                Array.Copy(src, 0, n0.children, 0, n0.size);
                Array.Copy(src, n0.size, n1.children, 0, n1.size);

                n0.updateRowColumn();
                n1.updateRowColumn();
            }

            private void distribute3(Object[] src, Node n0, Node n1, Node n2)
            {
                int sumSize = src.Length;

                n0.size = sumSize / 3;
                n1.size = (sumSize - n0.size) / 2;
                n2.size = sumSize - (n0.size + n1.size);

                Array.Copy(src, 0, n0.children, 0, n0.size);
                Array.Copy(src, n0.size, n1.children, 0, n1.size);
                Array.Copy(src, n0.size + n1.size, n2.children, 0, n2.size);

                n0.updateRowColumn();
                n1.updateRowColumn();
                n2.updateRowColumn();
            }

            protected internal bool isFull()
            {
                return size == children.Length;
            }

            protected internal bool isBelowHalf()
            {
                return size * 2 < children.Length;
            }

            protected internal Node split()
            {
                Node newNode = new Node(children.Length);
                int size1 = size / 2;
                int size2 = size - size1;
                Array.Copy(this.children, size1, newNode.children, 0, size2);
                for (int i = size1; i < this.size; i++)
                {
                    this.children[i] = null;
                }
                newNode.size = size2;
                newNode.updateRowColumn();
                newNode.prev = this;
                newNode.next = this.next;
                this.size = size1;
                this.updateRowColumn();
                this.next = newNode;
                if (newNode.next != null)
                {
                    newNode.next.prev = newNode;
                }
                return newNode;
            }

            protected internal void updateRowColumn()
            {
                Entry e = children[size - 1];
                this.row = e.row;
                this.column = e.column;
            }
        }

        public class Entry
        {
            protected internal int row;
            protected internal int column;

            protected internal int compare(int row, int column)
            {
                int diff = this.row - row;
                if (diff == 0)
                {
                    diff = this.column - column;
                }
                return diff;
            }
        }
    }

}
