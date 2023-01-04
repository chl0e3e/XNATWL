using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static XNATWL.TextArea;
using static XNATWL.Utils.SparseGrid;
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
