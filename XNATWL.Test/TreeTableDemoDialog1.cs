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
        private ScrollPane scrollPane;
        private Timer timer;
        private MyNode dynamicNode;

        public TreeTableDemoDialog1(Preferences preferences)
        {
            MyModel m = new MyModel();
            PersistentStringModel psm = new PersistentStringModel(preferences, "demoEditField", "you can edit this");

            MyNode a = m.insert("A", "1");
            a.insert("Aa", "2");
            a.insert("Ab", "3");
            MyNode ac = a.insert("Ac", "4");
            ac.insert("Ac1", "Hello");
            ac.insert("Ac2", "World");
            ac.insert("EditField", psm);
            a.insert("Ad", "5");
            MyNode b = m.insert("B", "6");
            b.insert("Ba", "7");
            b.insert("Bb", "8");
            b.insert("Bc", "9");
            dynamicNode = b.insert("Dynamic", "stuff");
            m.insert(new SpanString("This is a very long string which will span into the next column.", 2), "Not visible");
            m.insert("This is a very long string which will be clipped.", "This is visible");

            TreeTable t = new TreeTable(m);
            t.setTheme("/table");
            t.registerCellRenderer(typeof(SpanString), new SpanRenderer());
            t.registerCellRenderer(typeof(PersistentStringModel), new EditFieldCellRenderer());
            t.setDefaultSelectionManager();

            scrollPane = new ScrollPane(t);
            scrollPane.setTheme("/tableScrollPane");

            setTheme("scrollPaneDemoDialog1");
            setTitle("Dynamic TreeTable");
            add(scrollPane);
        }

        protected override void afterAddToGUI(GUI gui)
        {
            base.afterAddToGUI(gui);
            timer = gui.createTimer();
            timer.Tick += Timer_Tick;
            timer.setDelay(1500);
            timer.setContinuous(true);
            timer.start();
        }

        private void Timer_Tick(object sender, TimerTickEventArgs e)
        {
            //System.out.println("state="+state);
            switch (state++)
            {
                case 0:
                    dynamicNode.insert("Counting", "3...");
                    break;
                case 1:
                    dynamicNode.insert("Counting", "2...");
                    break;
                case 2:
                    dynamicNode.insert("Counting", "1...");
                    break;
                case 3:
                    subNode = dynamicNode.insert("this is a", "folder");
                    break;
                case 4:
                    subNode.insert("first", "entry");
                    break;
                case 5:
                    subNode.insert("now starting to remove", "counter");
                    break;
                case 6:
                case 7:
                case 8:
                    dynamicNode.remove(0);
                    break;
                case 9:
                    subNode.insert("last", "entry");
                    break;
                case 10:
                    dynamicNode.insert("now removing", "folder");
                    break;
                case 11:
                    dynamicNode.remove(0);
                    break;
                case 12:
                    dynamicNode.insert("starting", "again");
                    break;
                case 13:
                    dynamicNode.removeAll();
                    state = 0;
                    break;
            }
        }

        int state;
        MyNode subNode;

        public void centerScrollPane()
        {
            scrollPane.updateScrollbarSizes();
            scrollPane.setScrollPositionX(scrollPane.getMaxScrollPosX() / 2);
            scrollPane.setScrollPositionY(scrollPane.getMaxScrollPosY() / 2);
        }

        class MyNode : AbstractTreeTableNode
        {
            private Object str0;
            private Object str1;

            public MyNode(TreeTableNode parent, Object str0, Object str1) : base(parent)
            {
                this.str0 = str0;
                this.str1 = str1;
                IsLeaf = true;
            }

            public override Object DataAtColumn(int column)
            {
                return (column == 0) ? str0 : str1;
            }

            public MyNode insert(Object str0, Object str1)
            {
                MyNode n = new MyNode(this, str0, str1);
                InsertChild(n, this.Children);
                IsLeaf = false;
                return n;
            }

            public void remove(int idx)
            {
                RemoveChild(idx);
            }

            public void removeAll()
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

            public MyNode insert(Object str0, String str1)
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
            private String str;
            private int span;

            public int Span
            {
                get
                {
                    return span;
                }
            }

            public SpanString(String str, int span)
            {
                this.str = str;
                this.span = span;
            }

            public override String ToString()
            {
                return str;
            }
        }

        class SpanRenderer : TreeTable.StringCellRenderer
        {
            int span;

            public override void setCellData(int row, int column, Object data)
            {
                base.setCellData(row, column, data);
                span = ((SpanString)data).Span;
            }

            public override int getColumnSpan()
            {
                return span;
            }
        }

        class EditFieldCellRenderer : TreeTable.CellWidgetCreator
        {
            private StringModel model;
            private int editFieldHeight;

            public Widget updateWidget(Widget existingWidget)
            {
                EditField ef = (EditField)existingWidget;
                if (ef == null)
                {
                    ef = new EditField();
                }
                ef.setModel(model);
                return ef;
            }

            public void positionWidget(Widget widget, int x, int y, int w, int h)
            {
                widget.setPosition(x, y);
                widget.setSize(w, h);
            }

            public void applyTheme(ThemeInfo themeInfo)
            {
                editFieldHeight = themeInfo.GetParameter("editFieldHeight", 10);
            }

            public String getTheme()
            {
                return "EditFieldCellRenderer";
            }

            public void setCellData(int row, int column, Object data)
            {
                this.model = (StringModel)data;
            }

            public int getColumnSpan()
            {
                return 1;
            }

            public int getPreferredHeight()
            {
                return editFieldHeight;
            }

            public Widget getCellRenderWidget(int x, int y, int width, int height, bool isSelected)
            {
                return null;
            }
        }
    }

}
