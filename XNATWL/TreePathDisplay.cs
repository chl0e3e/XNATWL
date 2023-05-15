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
using XNATWL.Model;

namespace XNATWL
{
    public abstract class TreePathDisplay : Widget
    {
        private BoxLayout _pathBox;
        private EditField _editField;
        private String _separator = "/";
        private TreeTableNode _currentNode;
        private bool _allowEdit;

        public event EventHandler<TreePathElementClickedEventArgs> PathElementClicked;

        public TreePathDisplay()
        {
            _pathBox = new PathBox(this);
            _pathBox.SetScroll(true);
            _pathBox.SetClip(true);

            _editField = new PathEditField(this);
            _editField.SetVisible(false);

            Add(_pathBox);
            Add(_editField);
        }

        public abstract bool ResolvePath(String path);

        public TreeTableNode GetCurrentNode()
        {
            return _currentNode;
        }

        public void SetCurrentNode(TreeTableNode currentNode)
        {
            this._currentNode = currentNode;
            RebuildPathBox();
        }

        public String GetSeparator()
        {
            return _separator;
        }

        public void SetSeparator(String separator)
        {
            this._separator = separator;
            RebuildPathBox();
        }

        public bool IsAllowEdit()
        {
            return _allowEdit;
        }

        public void SetAllowEdit(bool allowEdit)
        {
            this._allowEdit = allowEdit;
            RebuildPathBox();
        }

        public void SetEditErrorMessage(String msg)
        {
            _editField.SetErrorMessage(msg);
        }

        public EditField GetEditField()
        {
            return _editField;
        }

        protected String GetTextFromNode(TreeTableNode node)
        {
            Object data = node.DataAtColumn(0);
            String text = (data != null) ? data.ToString() : "";
            if (text.EndsWith(_separator))
            {
                // strip of separator
                text = text.Substring(0, text.Length - 1);
            }
            return text;
        }

        private void RebuildPathBox()
        {
            _pathBox.RemoveAllChildren();
            if (_currentNode != null)
            {
                RecursiveAddNode(_currentNode, null);
            }
        }

        private void RecursiveAddNode(TreeTableNode node, TreeTableNode child)
        {
            if (node.Parent != null)
            {
                RecursiveAddNode(node.Parent, node);

                Button btn = new Button(GetTextFromNode(node));
                btn.SetTheme("node");
                /*btn.addCallback(new Runnable() {
                    public void run() {
                        firePathElementClicked(node, child);
                    }
                });*/
                _pathBox.Add(btn);

                Label l = new Label(_separator);
                l.SetTheme("separator");
                if (_allowEdit)
                {
                    l.Clicked += (sender, e) =>
                    {
                        if (e.ClickType == Label.ClickType.DoubleClick)
                        {
                            EditPath(node);
                        }
                    };
                }
                _pathBox.Add(l);
            }
        }

        void EndEdit()
        {
            _editField.SetVisible(false);
            RequestKeyboardFocus();
        }

        void EditPath(TreeTableNode cursorAfterNode)
        {
            StringBuilder sb = new StringBuilder();
            int cursorPos = 0;
            if (_currentNode != null)
            {
                cursorPos = RecursiveAddPath(sb, _currentNode, cursorAfterNode);
            }
            _editField.SetErrorMessage(null);
            _editField.SetText(sb.ToString());
            _editField.SetCursorPos(cursorPos, false);
            _editField.SetVisible(true); 
            _editField.RequestKeyboardFocus();
        }

        private int RecursiveAddPath(StringBuilder sb, TreeTableNode node, TreeTableNode cursorAfterNode)
        {
            int cursorPos = 0;
            if (node.Parent != null)
            {
                cursorPos = RecursiveAddPath(sb, node.Parent, cursorAfterNode);
                sb.Append(GetTextFromNode(node)).Append(_separator);
            }
            if (node == cursorAfterNode)
            {
                return sb.Length;
            }
            else
            {
                return cursorPos;
            }
        }

        public override int GetPreferredInnerWidth()
        {
            return _pathBox.GetPreferredWidth();
        }

        public override int GetPreferredInnerHeight()
        {
            return Math.Max(
                    _pathBox.GetPreferredHeight(),
                    _editField.GetPreferredHeight());
        }

        public override int GetMinHeight()
        {
            int minInnerHeight = Math.Max(_pathBox.GetMinHeight(), _editField.GetMinHeight());
            return Math.Max(base.GetMinHeight(), minInnerHeight + GetBorderVertical());
        }

        protected override void Layout()
        {
            LayoutChildFullInnerArea(_pathBox);
            LayoutChildFullInnerArea(_editField);
        }

        private class PathBox : BoxLayout
        {
            private TreePathDisplay _treePathDisplay;
            public PathBox(TreePathDisplay treePathDisplay) : base(BoxLayout.Direction.Horizontal)
            {
                this._treePathDisplay = treePathDisplay;
            }

            public override bool HandleEvent(Event evt)
            {
                if (evt.IsMouseEvent())
                {
                    if (evt.GetEventType() == EventType.MOUSE_CLICKED && evt.GetMouseClickCount() == 2)
                    {
                        this._treePathDisplay.EditPath(this._treePathDisplay.GetCurrentNode());
                        return true;
                    }
                    return evt.GetEventType() != EventType.MOUSE_WHEEL;
                }
                return base.HandleEvent(evt);
            }
        }

        private class PathEditField : EditField
        {
            private TreePathDisplay _treePathDisplay;
            public PathEditField(TreePathDisplay treePathDisplay)
            {
                this._treePathDisplay = treePathDisplay;
            }

            protected override void KeyboardFocusLost()
            {
                if (!HasOpenPopups())
                {
                    SetVisible(false);
                }
            }

            protected override void DoCallback(int key)
            {
                // for auto completion
                base.DoCallback(key);

                switch (key)
                {
                    case Event.KEY_RETURN:
                        if (this._treePathDisplay.ResolvePath(GetText()))
                        {
                            this._treePathDisplay.EndEdit();
                        }
                        break;
                    case Event.KEY_ESCAPE:
                        this._treePathDisplay.EndEdit();
                        break;
                }
            }
        }

    }

    public class TreePathElementClickedEventArgs
    {
        public readonly TreeTableNode Node;
        public readonly TreeTableNode ChildNode;
        public TreePathElementClickedEventArgs(TreeTableNode node, TreeTableNode child)
        {
            this.Node = node;
            this.ChildNode = child;
        }
    }
}
