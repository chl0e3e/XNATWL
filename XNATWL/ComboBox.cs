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
using XNATWL.Model;
using XNATWL.Renderer;

namespace XNATWL
{
    public class ComboBox<T> : ComboBoxBase
    {
        public static StateKey STATE_ERROR = StateKey.Get("error");

        private static int INVALID_WIDTH = -1;

        private ComboboxLabel _label;
        private ListBox<T> _listbox;

        String _displayTextNoSelection = "";
        bool _noSelectionIsError;
        bool _computeWidthFromModel;
        int _modelWidth = INVALID_WIDTH;
        int _selectionOnPopupOpen = ListBox<T>.NO_SELECTION;

        public event EventHandler<ComboBoxSelectionChangedEventArgs> SelectionChanged;

        public ComboBox(ListSelectionModel<T> model) : this()
        {
            SetModel(model);
        }

        public ComboBox(ListModel<T> model, IntegerModel selectionModel) : this()
        {
            SetModel(model);
            SetSelectionModel(selectionModel);
        }

        public ComboBox(ListModel<T> model) : this()
        {
            SetModel(model);
        }

        public ComboBox()
        {
            this._label = new ComboboxLabel(this, GetAnimationState());
            this._listbox = new ComboboxListbox<T>();

            this._button.GetModel().State += ComboBox_State;
            this._listbox.Callback += Listbox_Callback;

            _popup.SetTheme("comboboxPopup");
            _popup.Add(_listbox);
            Add(_label);
        }

        private void Listbox_Callback(object sender, ListBoxEventArgs e)
        {
            switch (e.Reason)
            {
                case ListBoxCallbackReason.KeyboardReturn:
                case ListBoxCallbackReason.MouseClick:
                case ListBoxCallbackReason.MouseDoubleClick:
                    ListBoxSelectionChanged(true);
                    break;
                default:
                    ListBoxSelectionChanged(false);
                    break;
            }
        }

        private void ComboBox_State(object sender, ButtonStateChangedEventArgs e)
        {
            this.UpdateHover();
        }

        public void SetModel(ListModel<T> model)
        {
            UnregisterModelChangeListener();
            _listbox.SetModel(model);
            if (_computeWidthFromModel)
            {
                RegisterModelChangeListener();
            }
        }

        public ListModel<T> GetModel()
        {
            return _listbox.getModel();
        }

        public void SetSelectionModel(IntegerModel selectionModel)
        {
            _listbox.SetSelectionModel(selectionModel);
        }

        public IntegerModel GetSelectionModel()
        {
            return _listbox.GetSelectionModel();
        }

        public void SetModel(ListSelectionModel<T> model)
        {
            _listbox.SetModel(model);
        }

        public void SetSelected(int selected)
        {
            _listbox.SetSelected(selected);
            UpdateLabel();
        }

        public int GetSelected()
        {
            return _listbox.GetSelected();
        }

        public bool IsComputeWidthFromModel()
        {
            return _computeWidthFromModel;
        }

        public void SetComputeWidthFromModel(bool computeWidthFromModel)
        {
            if (this._computeWidthFromModel != computeWidthFromModel)
            {
                this._computeWidthFromModel = computeWidthFromModel;
                if (computeWidthFromModel)
                {
                    RegisterModelChangeListener();
                }
                else
                {
                    UnregisterModelChangeListener();
                }
            }
        }

        public String GetDisplayTextNoSelection()
        {
            return _displayTextNoSelection;
        }

        /**
         * Sets the text to display when nothing is selected.
         * Default is {@code ""}
         *
         * @param displayTextNoSelection the text to display
         * @throws NullPointerException when displayTextNoSelection is null
         */
        public void SetDisplayTextNoSelection(String displayTextNoSelection)
        {
            if (displayTextNoSelection == null)
            {
                throw new ArgumentNullException("displayTextNoSelection");
            }
            this._displayTextNoSelection = displayTextNoSelection;
            UpdateLabel();
        }

        public bool IsNoSelectionIsError()
        {
            return _noSelectionIsError;
        }

        /**
         * Controls the value of {@link #STATE_ERROR} on the combobox display when nothing is selected.
         * Default is false.
         * 
         * @param noSelectionIsError
         */
        public void SetNoSelectionIsError(bool noSelectionIsError)
        {
            this._noSelectionIsError = noSelectionIsError;
            UpdateLabel();
        }

        private void RegisterModelChangeListener()
        {
            ListModel<T> model = GetModel();
            if (model != null)
            {
                _modelWidth = INVALID_WIDTH;
                model.AllChanged += Model_AllChanged;
                model.EntriesChanged += Model_EntriesChanged;
                model.EntriesDeleted += Model_EntriesDeleted;
                model.EntriesInserted += Model_EntriesInserted;
            }
        }

        private void Model_EntriesInserted(object sender, ListSubsetChangedEventArgs e)
        {
            UpdateModelWidth(e.First, e.Last);
        }

        private void Model_EntriesDeleted(object sender, ListSubsetChangedEventArgs e)
        {
            InvalidateModelWidth();
        }

        private void Model_EntriesChanged(object sender, ListSubsetChangedEventArgs e)
        {
            InvalidateModelWidth();
        }

        private void Model_AllChanged(object sender, ListAllChangedEventArgs e)
        {
            InvalidateModelWidth();
        }

        private void UnregisterModelChangeListener()
        {
            ListModel<T> model = GetModel();
            if (model != null)
            {
                model.AllChanged -= Model_AllChanged;
                model.EntriesChanged -= Model_EntriesChanged;
                model.EntriesDeleted -= Model_EntriesDeleted;
                model.EntriesInserted -= Model_EntriesInserted;
            }
        }

        protected override bool OpenPopup()
        {
            if (base.OpenPopup())
            {
                _popup.ValidateLayout();
                _selectionOnPopupOpen = GetSelected();
                _listbox.ScrollToSelected();
                return true;
            }
            return false;
        }

        protected override void PopupEscapePressed(Event evt)
        {
            SetSelected(_selectionOnPopupOpen);
            base.PopupEscapePressed(evt);
        }

        /**
         * Called when a right click was made on the ComboboxLabel.
         * The default implementation does nothing
         */
        protected void HandleRightClick()
        {
        }

        protected void ListBoxSelectionChanged(bool close)
        {
            UpdateLabel();
            if (close)
            {
                _popup.ClosePopup();
            }
            this.SelectionChanged.Invoke(this, new ComboBoxSelectionChangedEventArgs());
        }

        protected String GetModelData(int idx)
        {
            return GetModel().EntryAt(idx).ToString();
        }

        protected override Widget GetLabel()
        {
            return _label;
        }

        protected void UpdateLabel()
        {
            int selected = GetSelected();
            if (selected == ListBox<T>.NO_SELECTION)
            {
                _label.SetText(_displayTextNoSelection);
                _label.GetAnimationState().SetAnimationState(STATE_ERROR, _noSelectionIsError);
            }
            else
            {
                _label.SetText(GetModelData(selected));
                _label.GetAnimationState().SetAnimationState(STATE_ERROR, false);
            }
            if (!_computeWidthFromModel)
            {
                InvalidateLayout();
            }
        }

        protected void ApplyTheme(ThemeInfo themeInfo)
        {
            base.ApplyTheme(themeInfo);
            _modelWidth = INVALID_WIDTH;
        }

        public override bool HandleEvent(Event evt)
        {
            if (base.HandleEvent(evt))
            {
                return true;
            }
            if (evt.IsKeyPressedEvent())
            {
                switch (evt.GetKeyCode())
                {
                    case Event.KEY_UP:
                    case Event.KEY_DOWN:
                    case Event.KEY_HOME:
                    case Event.KEY_END:
                        // let the listbox handle this :)
                        _listbox.HandleEvent(evt);
                        return true;
                    case Event.KEY_SPACE:
                    case Event.KEY_RETURN:
                        OpenPopup();
                        return true;
                }
            }
            return false;
        }

        void InvalidateModelWidth()
        {
            if (_computeWidthFromModel)
            {
                _modelWidth = INVALID_WIDTH;
                InvalidateLayout();
            }
        }

        void UpdateModelWidth()
        {
            if (_computeWidthFromModel)
            {
                _modelWidth = 0;
                UpdateModelWidth(0, GetModel().Entries - 1);
            }
        }

        void UpdateModelWidth(int first, int last)
        {
            if (_computeWidthFromModel)
            {
                int newModelWidth = _modelWidth;
                for (int idx = first; idx <= last; idx++)
                {
                    newModelWidth = Math.Max(newModelWidth, ComputeEntryWidth(idx));
                }
                if (newModelWidth > _modelWidth)
                {
                    _modelWidth = newModelWidth;
                    InvalidateLayout();
                }
            }
        }

        protected int ComputeEntryWidth(int idx)
        {
            int width = _label.GetBorderHorizontal();
            Font font = _label.GetFont();
            if (font != null)
            {
                width += font.ComputeMultiLineTextWidth(GetModelData(idx));
            }
            return width;
        }

        void UpdateHover()
        {
            GetAnimationState().SetAnimationState(Label.STATE_HOVER,
                    _label._hover || _button.GetModel().Hover);
        }

        class ComboboxLabel : Label
        {
            internal bool _hover;
            private ComboBox<T> _comboBox;

            public ComboboxLabel(ComboBox<T> comboBox, AnimationState animState) : base(animState)
            {
                this._comboBox = comboBox;

                SetAutoSize(false);
                SetClip(true);
                SetTheme("display");
            }

            public override int GetPreferredInnerWidth()
            {
                if (this._comboBox._computeWidthFromModel && this._comboBox.GetModel() != null)
                {
                    if (this._comboBox._modelWidth == INVALID_WIDTH)
                    {
                        this._comboBox.UpdateModelWidth();
                    }
                    return this._comboBox._modelWidth;
                }
                else
                {
                    return base.GetPreferredInnerWidth();
                }
            }

            public override int GetPreferredInnerHeight()
            {
                int prefHeight = base.GetPreferredInnerHeight();
                if (GetFont() != null)
                {
                    prefHeight = Math.Max(prefHeight, GetFont().LineHeight);
                }
                return prefHeight;
            }

            public override bool HandleEvent(Event evt)
            {
                if (evt.IsMouseEvent())
                {
                    bool newHover = evt.GetEventType() != EventType.MOUSE_EXITED;
                    if (newHover != _hover)
                    {
                        _hover = newHover;
                        this._comboBox.UpdateHover();
                    }

                    if (evt.GetEventType() == EventType.MOUSE_CLICKED)
                    {
                        this._comboBox.OpenPopup();
                    }

                    if (evt.GetEventType() == EventType.MOUSE_BTNDOWN &&
                            evt.GetMouseButton() == Event.MOUSE_RBUTTON)
                    {
                        this._comboBox.HandleRightClick();
                    }

                    return evt.GetEventType() != EventType.MOUSE_WHEEL;
                }
                return false;
            }
        }

        /*class ModelChangeListener : ListModel.ChangeListener
        {
            public void entriesInserted(int first, int last)
            {
                updateModelWidth(first, last);
            }
            public void entriesDeleted(int first, int last)
            {
                invalidateModelWidth();
            }
            public void entriesChanged(int first, int last)
            {
                invalidateModelWidth();
            }
            public void allChanged()
            {
                invalidateModelWidth();
            }
        }*/

        class ComboboxListbox<T> : ListBox<T>
        {
            public ComboboxListbox()
            {
                SetTheme("listbox");
            }

            protected override ListBoxDisplay CreateDisplay()
            {
                return new ComboboxListboxLabel();
            }
        }

        class ComboboxListboxLabel : ListBox<T>.ListBoxLabel
        {
            protected override bool HandleListBoxEvent(Event evt)
            {
                if (evt.GetEventType() == EventType.MOUSE_CLICKED)
                {
                    DoListBoxCallback(ListBoxCallbackReason.MouseClick);
                    return true;
                }
                if (evt.GetEventType() == EventType.MOUSE_BTNDOWN)
                {
                    DoListBoxCallback(ListBoxCallbackReason.SetSelected);
                    return true;
                }
                return false;
            }
        }
    }

    public class ComboBoxSelectionChangedEventArgs : EventArgs
    {
    }
}
