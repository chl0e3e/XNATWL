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

using static XNATWL.Utils.Logger;
using System.Threading.Tasks;
using System;
using XNATWL.Model;
using XNATWL.Utils;

namespace XNATWL
{
    public class EditFieldAutoCompletionWindow : InfoWindow
    {
        private ResultListModel _listModel;
        private ListBox<String> _listBox;

        private bool _captureKeys;
        private bool _useInvokeAsync;
        private AutoCompletionDataSource _dataSource;
        //private ExecutorService executorService;
        //private Task<AutoCompletionResult> future;

        /**
         * Creates an EditFieldAutoCompletionWindow associated with the specified
         * EditField.
         *
         * Auto completion will start to work once a data source is set
         *
         * @param editField the EditField to which auto completion should be applied
         */
        public EditFieldAutoCompletionWindow(EditField editField) : base(editField)
        {

            this._listModel = new ResultListModel();
            this._listBox = new ListBox<String>(_listModel);

            Add(_listBox);

            _listBox.Callback += ListBox_Callback;
        }

        private void ListBox_Callback(object sender, ListBoxEventArgs e)
        {
            switch (e.Reason)
            {
                case ListBoxCallbackReason.MouseDoubleClick:
                    AcceptAutoCompletion();
                    break;
            }
        }

        /**
         * Creates an EditFieldAutoCompletionWindow associated with the specified
         * EditField.
         *
         * Auto completion is operational with the given data source (when it's not null)
         *
         * @param editField the EditField to which auto completion should be applied
         * @param dataSource the data source used for auto completion - can be null
         */
        public EditFieldAutoCompletionWindow(EditField editField, AutoCompletionDataSource dataSource) : this(editField)
        {
            this._dataSource = dataSource;
        }

        /**
         * Creates an EditFieldAutoCompletionWindow associated with the specified
         * EditField.
         *
         * Auto completion is operational with the given data source (when it's not null)
         *
         * @see #setExecutorService(java.util.concurrent.ExecutorService) 
         *
         * @param editField the EditField to which auto completion should be applied
         * @param dataSource the data source used for auto completion - can be null
         * @param executorService the executorService used to execute the data source queries
         */
        /*public EditFieldAutoCompletionWindow(EditField editField,
                AutoCompletionDataSource dataSource,
                ExecutorService executorService)
        {
            this(editField);
            this.dataSource = dataSource;
            this.executorService = executorService;
        }*/

        /**
         * Returns the EditField to which this EditFieldAutoCompletionWindow is attached
         * @return the EditField
         */
        public EditField GetEditField()
        {
            return (EditField)GetOwner();
        }

        /**
         * Returns the current ExecutorService
         * @return the current ExecutorService
         */
        //public ExecutorService getExecutorService() {
        //    return executorService;
        //}

        /**
         * Returns true if {@link GUI#invokeAsync} is used
         * @return true if {@code GUI.invokeAsync} is used
         */
        public bool IsUseInvokeAsync()
        {
            return _useInvokeAsync;
        }

        /**
         * Sets the ExecutorService which is used to perform async queries on the
         * AutoCompletionDataSource.
         *
         * This will disable the use of {@link GUI#invokeAsync}
         *
         * If it is null then all queries are done synchronously from the EditField
         * callback. This is good as long as data source is very fast (eg small in
         * memory tables).
         *
         * When the data source quries take too long they will impact the UI
         * responsiveness. To prevent that the queries can be executed in another
         * thread. This requires the data source and results to be thread save.
         *
         * @param executorService the ExecutorService or null
         * @see #setUseInvokeAsync(bool)
         */
        /*public void setExecutorService(ExecutorService executorService)
        {
            this.executorService = executorService;
            this.useInvokeAsync = false;
            cancelFuture();
        }*/

        /**
         * Perform async queries on the AutoCompletionDataSource using {@link GUI#invokeAsync}
         *
         * This will set executorService to null.
         *
         * If it is false then all queries are done synchronously from the EditField
         * callback. This is good as long as data source is very fast (eg small in
         * memory tables).
         *
         * When the data source quries take too long they will impact the UI
         * responsiveness. To prevent that the queries can be executed in another
         * thread. This requires the data source and results to be thread save.
         *
         * @param useInvokeAsync true if invokeAsync should be used
         * @see #setExecutorService(java.util.concurrent.ExecutorService)
         */
        /*public void setUseInvokeAsync(bool useInvokeAsync)
        {
            //this.executorService = null;
            this.useInvokeAsync = useInvokeAsync;
            cancelFuture();
        }*/

        /**
         * Returns the current data source
         * @return the current data source
         */
        public AutoCompletionDataSource GetDataSource()
        {
            return _dataSource;
        }

        /**
         * Sets a new data source.
         *
         * If the info window is currently open, then the displayed auto completion
         * will be refreshed. If you also need to change the ExecutorService then
         * it's adviced to do that first.
         *
         * @param dataSource the new AutoCompletionDataSource - can be null
         */
        public void SetDataSource(AutoCompletionDataSource dataSource)
        {
            this._dataSource = dataSource;
            //cancelFuture();
            if (IsOpen())
            {
                UpdateAutoCompletion();
            }
        }

        /**
         * This will update the auto completion and open the info window when results
         * are available
         */
        public void UpdateAutoCompletion()
        {
            //cancelFuture();
            AutoCompletionResult result = null;
            if (_dataSource != null)
            {
                EditField ef = GetEditField();
                int cursorPos = ef.GetCursorPos();
                if (cursorPos > 0)
                {
                    String text = ef.GetText();
                    GUI gui = ef.GetGUI();
                    if (_listModel._result != null)
                    {
                        result = _listModel._result.Refine(text, cursorPos);
                    }
                    if (result == null)
                    {
                        //if (gui != null && (useInvokeAsync || executorService != null))
                        {
                            //future = (useInvokeAsync ? gui.executorService : executorService).submit(
                            //        (Callable<AutoCompletionResult>)new AsyncQuery(gui, dataSource, text, cursorPos, listModel.result));
                        }
                        //else
                        {
                            try
                            {
                                result = _dataSource.CollectSuggestions(text, cursorPos, _listModel._result);
                            }
                            catch (Exception ex)
                            {
                                ReportQueryException(ex);
                            }
                        }
                    }
                }
            }
            UpdateAutoCompletion(result);
        }

        /**
         * Stops the auto completion.
         * 
         * Closes the infow window and discards the collected results.
         */
        public void StopAutoCompletion()
        {
            _listModel.SetResult(null);
            InstallAutoCompletion();
        }

        protected internal override void InfoWindowClosed()
        {
            StopAutoCompletion();
        }

        protected void UpdateAutoCompletion(AutoCompletionResult results)
        {
            _listModel.SetResult(results);
            _captureKeys = false;
            InstallAutoCompletion();
        }

        /*void checkFuture()
        {
            if (future != null)
            {
                if (future.isDone())
                {
                    AutoCompletionResult result = null;
                    try
                    {
                        result = future.get();
                    }
                    catch (InterruptedException ex)
                    {
                        // set the interrupted state again
                        Thread.currentThread().interrupt();
                    }
                    catch (ExecutionException ex)
                    {
                        reportQueryException(ex.getCause());
                    }
                    future = null;
                    updateAutoCompletion(result);
                }
            }
        }

        void cancelFuture()
        {
            if (future != null)
            {
                future.cancel(true);
                future = null;
            }
        }*/

        protected void ReportQueryException(Exception ex)
        {
            Logger.GetLogger(typeof(EditFieldAutoCompletionWindow)).Log(
                    Level.SEVERE, "Exception while collecting auto completion results", ex);
        }

        public override bool HandleEvent(Event evt)
        {
            if (evt.IsKeyEvent())
            {
                if (_captureKeys)
                {
                    if (evt.IsKeyPressedEvent())
                    {
                        switch (evt.GetKeyCode())
                        {
                            case Event.KEY_RETURN:
                            case Event.KEY_NUMPADENTER:
                                return AcceptAutoCompletion();

                            case Event.KEY_ESCAPE:
                                StopAutoCompletion();
                                break;

                            case Event.KEY_UP:
                            case Event.KEY_DOWN:
                            case Event.KEY_PRIOR:
                            case Event.KEY_NEXT:
                            case Event.KEY_HOME:
                            case Event.KEY_END:
                                _listBox.HandleEvent(evt);
                                break;

                            case Event.KEY_LEFT:
                            case Event.KEY_RIGHT:
                                return false;

                            default:
                                if (evt.HasKeyChar() || evt.GetKeyCode() == Event.KEY_BACK)
                                {
                                    if (!AcceptAutoCompletion())
                                    {
                                        StopAutoCompletion();
                                    }
                                    return false;
                                }
                                break;
                        }
                    }
                    return true;
                }
                else
                {
                    switch (evt.GetKeyCode())
                    {
                        case Event.KEY_UP:
                        case Event.KEY_DOWN:
                        case Event.KEY_NEXT:
                            _listBox.HandleEvent(evt);
                            StartCapture();
                            return _captureKeys;
                        case Event.KEY_ESCAPE:
                            StopAutoCompletion();
                            return false;
                        case Event.KEY_SPACE:
                            if ((evt.GetModifiers() & Event.MODIFIER_CTRL) != 0)
                            {
                                UpdateAutoCompletion();
                                return true;
                            }
                            return false;
                        default:
                            return false;
                    }
                }
            }

            return base.HandleEvent(evt);
        }

        bool AcceptAutoCompletion()
        {
            int selected = _listBox.GetSelected();
            if (selected >= 0)
            {
                EditField editField = GetEditField();
                String text = _listModel.EntryAt(selected);
                int pos = _listModel.GetCursorPosForEntry(selected);

                editField.SetText(text);
                if (pos >= 0 && pos < text.Length)
                {
                    editField.SetCursorPos(pos);
                }

                StopAutoCompletion();
                return true;
            }
            return false;
        }

        private void StartCapture()
        {
            _captureKeys = true;
            InstallAutoCompletion();
        }

        private void InstallAutoCompletion()
        {
            if (_listModel.Entries > 0)
            {
                OpenInfo();
            }
            else
            {
                _captureKeys = false;
                CloseInfo();
            }
        }

        public class ResultListModel : SimpleListModel<String>
        {
            internal AutoCompletionResult _result;

            public override event EventHandler<ListSubsetChangedEventArgs> EntriesInserted;
            public override event EventHandler<ListSubsetChangedEventArgs> EntriesDeleted;
            public override event EventHandler<ListSubsetChangedEventArgs> EntriesChanged;
            public override event EventHandler<ListAllChangedEventArgs> AllChanged;

            public override int Entries
            {
                get
                {
                    return (_result == null) ? 0 : _result.Results;
                }
            }

            public void SetResult(AutoCompletionResult result)
            {
                this._result = result;
                this.AllChanged.Invoke(this, new ListAllChangedEventArgs());
            }

            public int GetCursorPosForEntry(int index)
            {
                return _result.GetCursorPosForResult(index);
            }

            public override string EntryAt(int index)
            {
                return _result.ResultAt(index);
            }
        }

        /*class Callbacks : CallbackWithReason<ListBox.CallbackReason> {
            public void callback(ListBox.CallbackReason reason) {
                switch(reason) {
                    case MOUSE_DOUBLE_CLICK:
                        acceptAutoCompletion();
                        break;
                }
            }
        }*/

        /*class AsyncQuery : Callable<AutoCompletionResult>, Runnable {
            private GUI gui;
            private AutoCompletionDataSource dataSource;
            private String text;
            private int cursorPos;
            private AutoCompletionResult prevResult;

            public AsyncQuery(GUI gui, AutoCompletionDataSource dataSource, String text, int cursorPos, AutoCompletionResult prevResult) {
                this.gui = gui;
                this.dataSource = dataSource;
                this.text = text;
                this.cursorPos = cursorPos;
                this.prevResult = prevResult;
            }

            public AutoCompletionResult call() {
                AutoCompletionResult acr = dataSource.collectSuggestions(text, cursorPos, prevResult);
                gui.invokeLater(this);
                return acr;
            }

            public void run() {
                checkFuture();
            }
        }*/
    }

}