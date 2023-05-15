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
    public class TableSearchWindow : InfoWindow, TableBase.KeyboardSearchHandler
    {
        private TableSelectionModel _selectionModel;
        private EditField _searchTextField;
        private StringBuilder _searchTextBuffer;

        private String _searchText;
        private String _searchTextLowercase;
        private Timer _timer;
        private TableModel _model;
        private int _column;
        private int _currentRow;
        private bool _searchStartOnly;

        public TableSearchWindow(Table table, TableSelectionModel selectionModel) : base(table)
        {
            this._selectionModel = selectionModel;
            this._searchTextField = new EditField();
            this._searchTextBuffer = new StringBuilder();
            this._searchText = "";

            Label label = new Label("Search");
            label.SetLabelFor(_searchTextField);

            _searchTextField.SetReadOnly(true);

            DialogLayout l = new DialogLayout();
            l.SetHorizontalGroup(l.CreateSequentialGroup()
                    .AddWidget(label)
                    .AddWidget(_searchTextField));
            l.SetVerticalGroup(l.CreateParallelGroup()
                    .AddWidget(label)
                    .AddWidget(_searchTextField));

            Add(l);
        }

        public Table GetTable()
        {
            return (Table)GetOwner();
        }

        public TableModel GetModel()
        {
            return _model;
        }

        public void SetModel(TableModel model, int column)
        {
            if (column < 0)
            {
                throw new ArgumentOutOfRangeException("column");
            }
            if (model != null && column >= model.Columns)
            {
                throw new ArgumentOutOfRangeException("column");
            }
            this._model = model;
            this._column = column;
            CancelSearch();
        }

        public bool IsActive()
        {
            return IsOpen();
        }

        public void UpdateInfoWindowPosition()
        {
            AdjustSize();
            SetPosition(GetOwner().GetX(), GetOwner().GetBottom());
        }

        public bool HandleKeyEvent(Event evt)
        {
            if (_model == null)
            {
                return false;
            }

            if (evt.IsKeyPressedEvent())
            {
                switch (evt.GetKeyCode())
                {
                    case Event.KEY_ESCAPE:
                        if (IsOpen())
                        {
                            CancelSearch();
                            return true;
                        }
                        break;
                    case Event.KEY_RETURN:
                        return false;
                    case Event.KEY_BACK:
                        {
                            if (IsOpen())
                            {
                                int length = _searchTextBuffer.Length;
                                if (length > 0)
                                {
                                    _searchTextBuffer.Length = length - 1;
                                    UpdateText();
                                }
                                RestartTimer();
                                return true;
                            }
                            break;
                        }
                    case Event.KEY_UP:
                        if (IsOpen())
                        {
                            SearchDir(-1);
                            RestartTimer();
                            return true;
                        }
                        break;
                    case Event.KEY_DOWN:
                        if (IsOpen())
                        {
                            SearchDir(+1);
                            RestartTimer();
                            return true;
                        }
                        break;
                    default:
                        if (evt.HasKeyCharNoModifiers() && !Char.IsControl(evt.GetKeyChar()))
                        {
                            if (_searchTextBuffer.Length == 0)
                            {
                                _currentRow = Math.Max(0, GetTable().GetSelectionManager().GetLeadRow());
                                _searchStartOnly = true;
                            }
                            _searchTextBuffer.Append(evt.GetKeyChar());
                            UpdateText();
                            RestartTimer();
                            return true;
                        }
                        break;
                }
            }

            return false;
        }

        public void CancelSearch()
        {
            _searchTextBuffer.Length = 0;
            UpdateText();
            CloseInfo();
            if (_timer != null)
            {
                _timer.Stop();
            }
        }

        protected override void AfterAddToGUI(GUI gui)
        {
            base.AfterAddToGUI(gui);
            _timer = gui.CreateTimer();
            _timer.SetDelay(3000);
            /*timer.setCallback(new Runnable() {
                public void run() {
                    cancelSearch();
                }
            });*/
        }

        protected override void BeforeRemoveFromGUI(GUI gui)
        {
            _timer.Stop();
            _timer = null;

            base.BeforeRemoveFromGUI(gui);
        }

        private void UpdateText()
        {
            _searchText = _searchTextBuffer.ToString();
            _searchTextLowercase = null;
            _searchTextField.SetText(_searchText);
            if (_searchText.Length >= 0 && _model != null)
            {
                if (!IsOpen() && OpenInfo())
                {
                    UpdateInfoWindowPosition();
                }
                UpdateSearch();
            }
        }

        private void RestartTimer()
        {
            _timer.Stop();
            _timer.Start();
        }

        private void UpdateSearch()
        {
            int numRows = _model.Rows;
            if (numRows == 0)
            {
                return;
            }
            for (int row = _currentRow; row < numRows; row++)
            {
                if (CheckRow(row))
                {
                    SetRow(row);
                    return;
                }
            }
            if (_searchStartOnly)
            {
                _searchStartOnly = false;
            }
            else
            {
                numRows = _currentRow;
            }
            for (int row = 0; row < numRows; row++)
            {
                if (CheckRow(row))
                {
                    SetRow(row);
                    return;
                }
            }
            _searchTextField.SetErrorMessage("'" + _searchText + "' not found");
        }

        private void SearchDir(int dir)
        {
            int numRows = _model.Rows;
            if (numRows == 0)
            {
                return;
            }

            int startRow = Wrap(_currentRow, numRows);
            int row = startRow;

            for (; ; )
            {
                do
                {
                    row = Wrap(row + dir, numRows);
                    if (CheckRow(row))
                    {
                        SetRow(row);
                        return;
                    }
                } while (row != startRow);

                if (!_searchStartOnly)
                {
                    break;
                }
                _searchStartOnly = false;
            }
        }

        private void SetRow(int row)
        {
            if (_currentRow != row)
            {
                _currentRow = row;
                GetTable().ScrollToRow(row);
                if (_selectionModel != null)
                {
                    _selectionModel.SetSelection(row, row);
                }
            }
            _searchTextField.SetErrorMessage(null);
        }

        private bool CheckRow(int row)
        {
            Object data = _model.CellAt(row, _column);
            if (data == null)
            {
                return false;
            }
            String str = data.ToString();
            if (_searchStartOnly)
            {
                return str.StartsWith(_searchText);
            }
            str = str.ToLower();
            if (_searchTextLowercase == null)
            {
                _searchTextLowercase = _searchText.ToLower();
            }
            return str.Contains(_searchTextLowercase);
        }

        private static int Wrap(int row, int numRows)
        {
            if (row < 0)
            {
                return numRows - 1;
            }
            if (row >= numRows)
            {
                return 0;
            }
            return row;
        }
    }

}
