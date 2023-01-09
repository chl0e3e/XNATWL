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
        private BoxLayout pathBox;
        private EditField editField;
        private String separator = "/";
        private TreeTableNode currentNode;
        private bool allowEdit;
        public event EventHandler<TreePathElementClickedEventArgs> PathElementClicked;

        public TreePathDisplay()
        {
            pathBox = new PathBox(this);
            pathBox.setScroll(true);
            pathBox.setClip(true);

            editField = new PathEditField(this);
            editField.setVisible(false);

            add(pathBox);
            add(editField);
        }

        public abstract bool resolvePath(String path);

        public TreeTableNode getCurrentNode()
        {
            return currentNode;
        }

        public void setCurrentNode(TreeTableNode currentNode)
        {
            this.currentNode = currentNode;
            rebuildPathBox();
        }

        public String getSeparator()
        {
            return separator;
        }

        public void setSeparator(String separator)
        {
            this.separator = separator;
            rebuildPathBox();
        }

        public bool isAllowEdit()
        {
            return allowEdit;
        }

        public void setAllowEdit(bool allowEdit)
        {
            this.allowEdit = allowEdit;
            rebuildPathBox();
        }

        public void setEditErrorMessage(String msg)
        {
            editField.setErrorMessage(msg);
        }

        public EditField getEditField()
        {
            return editField;
        }

        protected String getTextFromNode(TreeTableNode node)
        {
            Object data = node.DataAtColumn(0);
            String text = (data != null) ? data.ToString() : "";
            if (text.EndsWith(separator))
            {
                // strip of separator
                text = text.Substring(0, text.Length - 1);
            }
            return text;
        }

        private void rebuildPathBox()
        {
            pathBox.removeAllChildren();
            if (currentNode != null)
            {
                recursiveAddNode(currentNode, null);
            }
        }

        private void recursiveAddNode(TreeTableNode node, TreeTableNode child)
        {
            if (node.Parent != null)
            {
                recursiveAddNode(node.Parent, node);

                Button btn = new Button(getTextFromNode(node));
                btn.setTheme("node");
                /*btn.addCallback(new Runnable() {
                    public void run() {
                        firePathElementClicked(node, child);
                    }
                });*/
                pathBox.add(btn);

                Label l = new Label(separator);
                l.setTheme("separator");
                if (allowEdit)
                {
                    l.Clicked += (sender, e) =>
                    {
                        if (e.ClickType == Label.ClickType.DOUBLE_CLICK)
                        {
                            editPath(node);
                        }
                    };
                }
                pathBox.add(l);
            }
        }

        void endEdit()
        {
            editField.setVisible(false);
            requestKeyboardFocus();
        }

        void editPath(TreeTableNode cursorAfterNode)
        {
            StringBuilder sb = new StringBuilder();
            int cursorPos = 0;
            if (currentNode != null)
            {
                cursorPos = recursiveAddPath(sb, currentNode, cursorAfterNode);
            }
            editField.setErrorMessage(null);
            editField.setText(sb.ToString());
            editField.setCursorPos(cursorPos, false);
            editField.setVisible(true); 
            editField.requestKeyboardFocus();
        }

        private int recursiveAddPath(StringBuilder sb, TreeTableNode node, TreeTableNode cursorAfterNode)
        {
            int cursorPos = 0;
            if (node.Parent != null)
            {
                cursorPos = recursiveAddPath(sb, node.Parent, cursorAfterNode);
                sb.Append(getTextFromNode(node)).Append(separator);
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

        public override int getPreferredInnerWidth()
        {
            return pathBox.getPreferredWidth();
        }

        public override int getPreferredInnerHeight()
        {
            return Math.Max(
                    pathBox.getPreferredHeight(),
                    editField.getPreferredHeight());
        }

        public override int getMinHeight()
        {
            int minInnerHeight = Math.Max(pathBox.getMinHeight(), editField.getMinHeight());
            return Math.Max(base.getMinHeight(), minInnerHeight + getBorderVertical());
        }

        protected override void layout()
        {
            layoutChildFullInnerArea(pathBox);
            layoutChildFullInnerArea(editField);
        }

        private class PathBox : BoxLayout
        {
            private TreePathDisplay _treePathDisplay;
            public PathBox(TreePathDisplay treePathDisplay) : base(BoxLayout.Direction.HORIZONTAL)
            {
                this._treePathDisplay = treePathDisplay;
            }

            public override bool handleEvent(Event evt)
            {
                if (evt.isMouseEvent())
                {
                    if (evt.getEventType() == Event.EventType.MOUSE_CLICKED && evt.getMouseClickCount() == 2)
                    {
                        this._treePathDisplay.editPath(this._treePathDisplay.getCurrentNode());
                        return true;
                    }
                    return evt.getEventType() != Event.EventType.MOUSE_WHEEL;
                }
                return base.handleEvent(evt);
            }
        }

        private class PathEditField : EditField
        {
            private TreePathDisplay _treePathDisplay;
            public PathEditField(TreePathDisplay treePathDisplay)
            {
                this._treePathDisplay = treePathDisplay;
            }

            protected override void keyboardFocusLost()
            {
                if (!hasOpenPopups())
                {
                    setVisible(false);
                }
            }

            protected override void doCallback(int key)
            {
                // for auto completion
                base.doCallback(key);

                switch (key)
                {
                    case Event.KEY_RETURN:
                        if (this._treePathDisplay.resolvePath(getText()))
                        {
                            this._treePathDisplay.endEdit();
                        }
                        break;
                    case Event.KEY_ESCAPE:
                        this._treePathDisplay.endEdit();
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
