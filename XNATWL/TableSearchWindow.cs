﻿/*
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
        private TableSelectionModel selectionModel;
        private EditField searchTextField;
        private StringBuilder searchTextBuffer;

        private String searchText;
        private String searchTextLowercase;
        private Timer timer;
        private TableModel model;
        private int column;
        private int currentRow;
        private bool searchStartOnly;

        public TableSearchWindow(Table table, TableSelectionModel selectionModel) : base(table)
        {
            this.selectionModel = selectionModel;
            this.searchTextField = new EditField();
            this.searchTextBuffer = new StringBuilder();
            this.searchText = "";

            Label label = new Label("Search");
            label.setLabelFor(searchTextField);

            searchTextField.setReadOnly(true);

            DialogLayout l = new DialogLayout();
            l.setHorizontalGroup(l.createSequentialGroup()
                    .addWidget(label)
                    .addWidget(searchTextField));
            l.setVerticalGroup(l.createParallelGroup()
                    .addWidget(label)
                    .addWidget(searchTextField));

            add(l);
        }

        public Table getTable()
        {
            return (Table)getOwner();
        }

        public TableModel getModel()
        {
            return model;
        }

        public void setModel(TableModel model, int column)
        {
            if (column < 0)
            {
                throw new ArgumentOutOfRangeException("column");
            }
            if (model != null && column >= model.Columns)
            {
                throw new ArgumentOutOfRangeException("column");
            }
            this.model = model;
            this.column = column;
            cancelSearch();

        }

        public bool isActive()
        {
            return isOpen();
        }

        public void updateInfoWindowPosition()
        {
            adjustSize();
            setPosition(getOwner().getX(), getOwner().getBottom());
        }

        public bool handleKeyEvent(Event evt)
        {
            if (model == null)
            {
                return false;
            }

            if (evt.isKeyPressedEvent())
            {
                switch (evt.getKeyCode())
                {
                    case Event.KEY_ESCAPE:
                        if (isOpen())
                        {
                            cancelSearch();
                            return true;
                        }
                        break;
                    case Event.KEY_RETURN:
                        return false;
                    case Event.KEY_BACK:
                        {
                            if (isOpen())
                            {
                                int length = searchTextBuffer.Length;
                                if (length > 0)
                                {
                                    searchTextBuffer.Length = length - 1;
                                    updateText();
                                }
                                restartTimer();
                                return true;
                            }
                            break;
                        }
                    case Event.KEY_UP:
                        if (isOpen())
                        {
                            searchDir(-1);
                            restartTimer();
                            return true;
                        }
                        break;
                    case Event.KEY_DOWN:
                        if (isOpen())
                        {
                            searchDir(+1);
                            restartTimer();
                            return true;
                        }
                        break;
                    default:
                        if (evt.hasKeyCharNoModifiers() && !Char.IsControl(evt.getKeyChar()))
                        {
                            if (searchTextBuffer.Length == 0)
                            {
                                currentRow = Math.Max(0, getTable().getSelectionManager().getLeadRow());
                                searchStartOnly = true;
                            }
                            searchTextBuffer.Append(evt.getKeyChar());
                            updateText();
                            restartTimer();
                            return true;
                        }
                        break;
                }
            }

            return false;
        }

        public void cancelSearch()
        {
            searchTextBuffer.Length = 0;
            updateText();
            closeInfo();
            if (timer != null)
            {
                timer.stop();
            }
        }

        protected override void afterAddToGUI(GUI gui)
        {
            base.afterAddToGUI(gui);
            timer = gui.createTimer();
            timer.setDelay(3000);
            /*timer.setCallback(new Runnable() {
                public void run() {
                    cancelSearch();
                }
            });*/
        }

        protected override void beforeRemoveFromGUI(GUI gui)
        {
            timer.stop();
            timer = null;

            base.beforeRemoveFromGUI(gui);
        }

        private void updateText()
        {
            searchText = searchTextBuffer.ToString();
            searchTextLowercase = null;
            searchTextField.setText(searchText);
            if (searchText.Length >= 0 && model != null)
            {
                if (!isOpen() && openInfo())
                {
                    updateInfoWindowPosition();
                }
                updateSearch();
            }
        }

        private void restartTimer()
        {
            timer.stop();
            timer.start();
        }

        private void updateSearch()
        {
            int numRows = model.Rows;
            if (numRows == 0)
            {
                return;
            }
            for (int row = currentRow; row < numRows; row++)
            {
                if (checkRow(row))
                {
                    setRow(row);
                    return;
                }
            }
            if (searchStartOnly)
            {
                searchStartOnly = false;
            }
            else
            {
                numRows = currentRow;
            }
            for (int row = 0; row < numRows; row++)
            {
                if (checkRow(row))
                {
                    setRow(row);
                    return;
                }
            }
            searchTextField.setErrorMessage("'" + searchText + "' not found");
        }

        private void searchDir(int dir)
        {
            int numRows = model.Rows;
            if (numRows == 0)
            {
                return;
            }

            int startRow = wrap(currentRow, numRows);
            int row = startRow;

            for (; ; )
            {
                do
                {
                    row = wrap(row + dir, numRows);
                    if (checkRow(row))
                    {
                        setRow(row);
                        return;
                    }
                } while (row != startRow);

                if (!searchStartOnly)
                {
                    break;
                }
                searchStartOnly = false;
            }
        }

        private void setRow(int row)
        {
            if (currentRow != row)
            {
                currentRow = row;
                getTable().scrollToRow(row);
                if (selectionModel != null)
                {
                    selectionModel.SetSelection(row, row);
                }
            }
            searchTextField.setErrorMessage(null);
        }

        private bool checkRow(int row)
        {
            Object data = model.CellAt(row, column);
            if (data == null)
            {
                return false;
            }
            String str = data.ToString();
            if (searchStartOnly)
            {
                return str.StartsWith(searchText);
            }
            str = str.ToLower();
            if (searchTextLowercase == null)
            {
                searchTextLowercase = searchText.ToLower();
            }
            return str.Contains(searchTextLowercase);
        }

        private static int wrap(int row, int numRows)
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
