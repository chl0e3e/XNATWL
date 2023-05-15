using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static XNATWL.AnimationState;
using XNATWL.Model;
using XNATWL.IO;

namespace XNATWL.Test
{
    public class TreeTableDemoDialog1 : FadeFrame
    {
        private ScrollPane _scrollPane;
        private Timer _timer;
        private MyNode _dynamicNode;

        public TreeTableDemoDialog1(Preferences preferences)
        {
            MyModel m = new MyModel();
            PersistentStringModel psm = new PersistentStringModel(preferences, "demoEditField", "you can edit this");

            MyNode a = m.Insert("A", "1");
            a.Insert("Aa", "2");
            a.Insert("Ab", "3");
            MyNode ac = a.Insert("Ac", "4");
            ac.Insert("Ac1", "Hello");
            ac.Insert("Ac2", "World");
            ac.Insert("EditField", psm);
            a.Insert("Ad", "5");
            MyNode b = m.Insert("B", "6");
            b.Insert("Ba", "7");
            b.Insert("Bb", "8");
            b.Insert("Bc", "9");
            _dynamicNode = b.Insert("Dynamic", "stuff");
            m.Insert(new SpanString("This is a very long string which will span into the next column.", 2), "Not visible");
            m.Insert("This is a very long string which will be clipped.", "This is visible");

            TreeTable t = new TreeTable(m);
            t.SetTheme("/table");
            t.RegisterCellRenderer(typeof(SpanString), new SpanRenderer());
            t.RegisterCellRenderer(typeof(PersistentStringModel), new EditFieldCellRenderer());
            t.SetDefaultSelectionManager();

            _scrollPane = new ScrollPane(t);
            _scrollPane.SetTheme("/tableScrollPane");

            SetTheme("scrollPaneDemoDialog1");
            SetTitle("Dynamic TreeTable");
            Add(_scrollPane);
        }

        protected override void AfterAddToGUI(GUI gui)
        {
            base.AfterAddToGUI(gui);
            _timer = gui.CreateTimer();
            _timer.Tick += Timer_Tick;
            _timer.SetDelay(1500);
            _timer.SetContinuous(true);
            _timer.Start();
        }

        private void Timer_Tick(object sender, TimerTickEventArgs e)
        {
            //System.out.println("state="+state);
            switch (state++)
            {
                case 0:
                    _dynamicNode.Insert("Counting", "3...");
                    break;
                case 1:
                    _dynamicNode.Insert("Counting", "2...");
                    break;
                case 2:
                    _dynamicNode.Insert("Counting", "1...");
                    break;
                case 3:
                    subNode = _dynamicNode.Insert("this is a", "folder");
                    break;
                case 4:
                    subNode.Insert("first", "entry");
                    break;
                case 5:
                    subNode.Insert("now starting to remove", "counter");
                    break;
                case 6:
                case 7:
                case 8:
                    _dynamicNode.Remove(0);
                    break;
                case 9:
                    subNode.Insert("last", "entry");
                    break;
                case 10:
                    _dynamicNode.Insert("now removing", "folder");
                    break;
                case 11:
                    _dynamicNode.Remove(0);
                    break;
                case 12:
                    _dynamicNode.Insert("starting", "again");
                    break;
                case 13:
                    _dynamicNode.RemoveAll();
                    state = 0;
                    break;
            }
        }

        int state;
        MyNode subNode;

        public void CenterScrollPane()
        {
            _scrollPane.UpdateScrollbarSizes();
            _scrollPane.SetScrollPositionX(_scrollPane.GetMaxScrollPosX() / 2);
            _scrollPane.SetScrollPositionY(_scrollPane.GetMaxScrollPosY() / 2);
        }

        class MyNode : AbstractTreeTableNode
        {
            private Object _str0;
            private Object _str1;

            public MyNode(TreeTableNode parent, Object str0, Object str1) : base(parent)
            {
                this._str0 = str0;
                this._str1 = str1;
                IsLeaf = true;
            }

            public override Object DataAtColumn(int column)
            {
                return (column == 0) ? _str0 : _str1;
            }

            public MyNode Insert(Object str0, Object str1)
            {
                MyNode n = new MyNode(this, str0, str1);
                InsertChild(n, this.Children);
                IsLeaf = false;
                return n;
            }

            public void Remove(int idx)
            {
                RemoveChild(idx);
            }

            public void RemoveAll()
            {
                RemoveAllChildren();
            }
        }

        class MyModel : AbstractTreeTableModel
        {
            private static String[] COLUMN_NAMES = { "Left", "Right" };

            public override event EventHandler<ColumnsChangedEventArgs> ColumnInserted;
            public override event EventHandler<ColumnsChangedEventArgs> ColumnDeleted;
            public override event EventHandler<ColumnHeaderChangedEventArgs> ColumnHeaderChanged;

            public override int Columns
            {
                get
                {
                    return 2;
                }
            }

            public MyNode Insert(Object str0, String str1)
            {
                MyNode n = new MyNode(this, str0, str1);
                base.InsertChildAt(n, this.Children);
                return n;
            }

            public override string ColumnHeaderTextFor(int column)
            {
                return COLUMN_NAMES[column];
            }
        }

        class SpanString
        {
            private String _str;
            private int _span;

            public int Span
            {
                get
                {
                    return _span;
                }
            }

            public SpanString(String str, int span)
            {
                this._str = str;
                this._span = span;
            }

            public override String ToString()
            {
                return _str;
            }
        }

        class SpanRenderer : TreeTable.StringCellRenderer
        {
            int _span;

            public override void SetCellData(int row, int column, Object data)
            {
                base.SetCellData(row, column, data);
                _span = ((SpanString)data).Span;
            }

            public override int GetColumnSpan()
            {
                return _span;
            }
        }

        class EditFieldCellRenderer : TreeTable.CellWidgetCreator
        {
            private StringModel _model;
            private int _editFieldHeight;

            public Widget UpdateWidget(Widget existingWidget)
            {
                EditField ef = (EditField)existingWidget;
                if (ef == null)
                {
                    ef = new EditField();
                }
                ef.SetModel(_model);
                return ef;
            }

            public void PositionWidget(Widget widget, int x, int y, int w, int h)
            {
                widget.SetPosition(x, y);
                widget.SetSize(w, h);
            }

            public void ApplyTheme(ThemeInfo themeInfo)
            {
                _editFieldHeight = themeInfo.GetParameter("editFieldHeight", 10);
            }

            public String GetTheme()
            {
                return "EditFieldCellRenderer";
            }

            public void SetCellData(int row, int column, Object data)
            {
                this._model = (StringModel)data;
            }

            public int GetColumnSpan()
            {
                return 1;
            }

            public int GetPreferredHeight()
            {
                return _editFieldHeight;
            }

            public Widget GetCellRenderWidget(int x, int y, int width, int height, bool isSelected)
            {
                return null;
            }
        }
    }
}
