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
        private ScrollPane _scrollPane;
        private ComboBoxValue _cbv;

        public TableDemoDialog1()
        {
            ListModel<String> cbm = new SimpleChangeableListModel<String>("Hallo", "Welt", "Test");
            _cbv = new ComboBoxValue(0, cbm);
            TableModel m = new DemoTableModel(this);
            Table t = new Table(m);
            // register the ComboBoxValue class (see below) and it's cell widget creator
            // this will change the behavior of cells when they contain a data valzue of
            // the type "ComboBoxValue"
            t.RegisterCellRenderer(typeof(ComboBoxValue), new ComboBoxCellWidgetCreator());

            t.SetTheme("/table");
            t.SetVariableRowHeight(true);
            t.SetDefaultSelectionManager();

            _scrollPane = new ScrollPane(t);
            _scrollPane.SetTheme("/tableScrollPane");

            SetTheme("scrollPaneDemoDialog1");
            SetTitle("Table with variable row height");
            Add(_scrollPane);
        }

        public class ComboBoxValue : SimpleIntegerModel
        {
            private ListModel<String> _model;
            public ComboBoxValue(int value, ListModel<String> model) : base(0, model.Entries - 1, value)
            {
                this._model = model;
            }

            public ListModel<String> GetModel() {
                return _model;
            }
        }

        private class DemoTableModel : AbstractTableModel
        {
            TableDemoDialog1 _tableDemoDialog;
            public DemoTableModel(TableDemoDialog1 tableDemoDialog)
            {
                this._tableDemoDialog = tableDemoDialog;
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
                    return _tableDemoDialog._cbv;
                }
                if (row == 6 && column == 1)
                {
                    return "Selected: " + _tableDemoDialog._cbv.Value;
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
            private int _comboBoxHeight;
            private ComboBoxValue _data;

            public void ApplyTheme(ThemeInfo themeInfo)
            {
                _comboBoxHeight = themeInfo.GetParameter("comboBoxHeight", 0);
            }

            public String GetTheme()
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
            public Widget UpdateWidget(Widget existingWidget)
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
                cb.SetData(_data);
                return cb;
            }

            public void PositionWidget(Widget widget, int x, int y, int w, int h)
            {
                // this method will size and position the ComboBox
                // If the widget should be centered (like a check box) then this
                // would be done here
                widget.SetPosition(x, y);
                widget.SetSize(w, h);
            }

            public void SetCellData(int row, int column, Object data)
            {
                // we have to remember the cell data for the next call of updateWidget
                this._data = (ComboBoxValue)data;
            }

            public Widget GetCellRenderWidget(int x, int y, int width, int height, bool isSelected)
            {
                // this cell does not render anything itself
                return null;
            }

            public int GetColumnSpan()
            {
                // no column spanning
                return 1;
            }

            public int GetPreferredHeight()
            {
                // we have to inform the table about the required cell height before
                // we can create the widget - so we need to get the required height
                // from the theme -  see applyTheme/getTheme
                return _comboBoxHeight;
            }

            /**
             * We need a subclass of ComboBox to contain ("be" in this example) the
             * listeners.
             */
            private class MyComboBox : ComboBox<String>
            {
                ComboBoxValue _data;

                public MyComboBox()
                {
                    SetTheme("combobox");   // keep default theme name
                    this.SelectionChanged += MyComboBox_SelectionChanged;
                }

                private void MyComboBox_SelectionChanged(object sender, ComboBoxSelectionChangedEventArgs e)
                {
                    if (_data != null)
                    {
                        _data.Value = GetSelected();
                    }
                }

                public void SetData(ComboBoxValue data)
                {
                    this._data = null;
                    SetModel(data.GetModel());
                    SetSelected(data.Value);
                    this._data = data;
                }
            }
        }

    }
}
