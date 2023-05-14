using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Model;
using static XNATWL.TableBase;

namespace XNATWL.Test
{
    public class TableDemoDialog1 : FadeFrame
    {
        private ScrollPane scrollPane;
        private ComboBoxValue cbv;

        public TableDemoDialog1()
        {
            ListModel<String> cbm = new SimpleChangeableListModel<String>("Hallo", "Welt", "Test");
            cbv = new ComboBoxValue(0, cbm);
            TableModel m = new DemoTableModel(this);
            Table t = new Table(m);
            // register the ComboBoxValue class (see below) and it's cell widget creator
            // this will change the behavior of cells when they contain a data valzue of
            // the type "ComboBoxValue"
            t.registerCellRenderer(typeof(ComboBoxValue), new ComboBoxCellWidgetCreator());

            t.setTheme("/table");
            t.setVaribleRowHeight(true);
            t.setDefaultSelectionManager();

            scrollPane = new ScrollPane(t);
            scrollPane.setTheme("/tableScrollPane");

            setTheme("scrollPaneDemoDialog1");
            setTitle("Table with variable row height");
            add(scrollPane);
        }

        public class ComboBoxValue : SimpleIntegerModel
        {
            private ListModel<String> model;
            public ComboBoxValue(int value, ListModel<String> model) : base(0, model.Entries - 1, value)
            {
                this.model = model;
            }

            public ListModel<String> getModel() {
                return model;
            }
        }

        private class DemoTableModel : AbstractTableModel
        {
            TableDemoDialog1 tableDemoDialog;
            public DemoTableModel(TableDemoDialog1 tableDemoDialog)
            {
                this.tableDemoDialog = tableDemoDialog;
            }

            public override int Columns
            {
                get
                {
                    return 3;
                }
            }

            public override int Rows
            {
                get
                {
                    return 20;
                }
            }

            public override object CellAt(int row, int column)
            {
                if (row == 7 && column == 1)
                {
                    // This cell will contain a ComboBoxValue - via registerCellRenderer
                    // below this will cause a comobox to appear
                    return tableDemoDialog.cbv;
                }
                if (row == 6 && column == 1)
                {
                    return "Selected: " + tableDemoDialog.cbv.Value;
                }
                return "Row " + row + (((row * this.Columns + column) % 17 == 0) ? "\n" : "") + " Column " + column;
            }

            public override string ColumnHeaderTextFor(int column)
            {
                return "Column " + column;
            }

            public override object TooltipAt(int row, int column)
            {
                return "X:" + (column + 1) + " Y:" + (row + 1);
            }
        }

        private class ComboBoxCellWidgetCreator : CellWidgetCreator
        {
            private int comboBoxHeight;
            private ComboBoxValue data;

            public void applyTheme(ThemeInfo themeInfo)
            {
                comboBoxHeight = themeInfo.getParameter("comboBoxHeight", 0);
            }

            public String getTheme()
            {
                return "ComboBoxCellRenderer";
            }

            /**
             * Update or create the ComboBox widget.
             *
             * @param existingWidget null on first call per cell or the previous
             *   widget when an update has been send to that cell.
             * @return the widget to use for this cell
             */
            public Widget updateWidget(Widget existingWidget)
            {
                MyComboBox cb = (MyComboBox)existingWidget;
                if (cb == null)
                {
                    cb = new MyComboBox();
                }
                // in this example there should be no update to cells
                // but the code pattern here can also be used when updates are
                // generated. Care should be taken that the above type cast
                // does not fail.
                cb.setData(data);
                return cb;
            }

            public void positionWidget(Widget widget, int x, int y, int w, int h)
            {
                // this method will size and position the ComboBox
                // If the widget should be centered (like a check box) then this
                // would be done here
                widget.setPosition(x, y);
                widget.setSize(w, h);
            }

            public void setCellData(int row, int column, Object data)
            {
                // we have to remember the cell data for the next call of updateWidget
                this.data = (ComboBoxValue)data;
            }

            public Widget getCellRenderWidget(int x, int y, int width, int height, bool isSelected)
            {
                // this cell does not render anything itself
                return null;
            }

            public int getColumnSpan()
            {
                // no column spanning
                return 1;
            }

            public int getPreferredHeight()
            {
                // we have to inform the table about the required cell height before
                // we can create the widget - so we need to get the required height
                // from the theme -  see applyTheme/getTheme
                return comboBoxHeight;
            }

            /**
             * We need a subclass of ComboBox to contain ("be" in this example) the
             * listeners.
             */
            private class MyComboBox : ComboBox<String>
            {
                ComboBoxValue data;

                public MyComboBox()
                {
                    setTheme("combobox");   // keep default theme name
                    this.SelectionChanged += MyComboBox_SelectionChanged;
                }

                private void MyComboBox_SelectionChanged(object sender, ComboBoxSelectionChangedEventArgs e)
                {
                    if (data != null)
                    {
                        data.Value = getSelected();
                    }
                }

                public void setData(ComboBoxValue data)
                {
                    this.data = null;
                    setModel(data.getModel());
                    setSelected(data.Value);
                    this.data = data;
                }
            }
        }

    }
}
