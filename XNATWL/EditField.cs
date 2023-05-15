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
using XNATWL.Utils;

namespace XNATWL
{
    public class EditField : Widget
    {
        public static StateKey STATE_ERROR = StateKey.Get("error");
        public static StateKey STATE_READONLY = StateKey.Get("readonly");
        public static StateKey STATE_HOVER = StateKey.Get("hover");
        public static StateKey STATE_CURSOR_MOVED = StateKey.Get("cursorMoved");

        EditFieldModel _editBuffer;
        private TextRenderer _textRenderer;
        private PasswordMasker _passwordMasking;
        private StringModel _model;
        private bool _readOnly;
        StringAttributes _attributes;

        private int _cursorPos;
        int _scrollPos;
        int _selectionStart;
        int _selectionEnd;
        int _numberOfLines;
        bool _multiLine;
        bool _pendingScrollToCursor;
        bool _pendingScrollToCursorForce;
        private int _maxTextLength = short.MaxValue;

        private int _columns = 5;
        private Image _cursorImage;
        Image _selectionImage;
        private char _passwordChar;
        private Object _errorMsg;
        private bool _errorMsgFromModel;
        private Menu _popupMenu;
        private bool _textLongerThenWidget;
        private bool _forwardUnhandledKeysToCallback;
        private bool _autoCompletionOnSetText = true;
        bool _scrollToCursorOnSizeChange = true;

        private EditFieldAutoCompletionWindow _autoCompletionWindow;
        private int _autoCompletionHeight = 100;

        private InfoWindow _errorInfoWindow;
        private Label _errorInfoLabel;

        public event EventHandler<EditFieldCallbackEventArgs> Callback;

        /**
         * Creates a new EditField with an optional parent animation state.
         *
         * Unlike other widgets which use the passed animation state directly,
         * the EditField always creates it's animation state with the passed
         * one as parent.
         *
         * @param parentAnimationState
         * @param editFieldModel the edit field model to use
         * @see AnimationState#AnimationState(de.matthiasmann.twl.AnimationState) 
         */
        public EditField(AnimationState parentAnimationState, EditFieldModel editFieldModel) : base(parentAnimationState, true)
        {
            if (editFieldModel == null)
            {
                throw new NullReferenceException("editFieldModel");
            }

            this._editBuffer = editFieldModel;
            this._textRenderer = new TextRenderer(this, GetAnimationState());
            this._passwordChar = '*';

            _textRenderer.SetTheme("renderer");
            _textRenderer.SetClip(true);

            Add(_textRenderer);
            SetCanAcceptKeyboardFocus(true);
            SetDepthFocusTraversal(false);

            AddActionMapping("cut", "CutToClipboard");
            AddActionMapping("copy", "CopyToClipboard");
            AddActionMapping("paste", "PasteFromClipboard");
            AddActionMapping("selectAll", "SelectAll");
            AddActionMapping("duplicateLineDown", "DuplicateLineDown");
        }

        /**
         * Creates a new EditField with an optional parent animation state.
         *
         * Unlike other widgets which use the passed animation state directly,
         * the EditField always creates it's animation state with the passed
         * one as parent.
         *
         * @param parentAnimationState
         * @see AnimationState#AnimationState(de.matthiasmann.twl.AnimationState)
         */
        public EditField(AnimationState parentAnimationState) : this(parentAnimationState, new DefaultEditFieldModel())
        {

        }

        public EditField() :
            this(null)
        {
        }


        public bool IsForwardUnhandledKeysToCallback()
        {
            return _forwardUnhandledKeysToCallback;
        }

        /**
         * Controls if unhandled key presses are forwarded to the callback or not.
         * Default is false. If set to true then the EditField will consume all key
         * presses.
         *
         * @param forwardUnhandledKeysToCallback true if unhandled keys should be forwarded to the callbacks
         */
        public void SetForwardUnhandledKeysToCallback(bool forwardUnhandledKeysToCallback)
        {
            this._forwardUnhandledKeysToCallback = forwardUnhandledKeysToCallback;
        }

        public bool IsAutoCompletionOnSetText()
        {
            return _autoCompletionOnSetText;
        }

        /**
         * Controls if a call to setText() should trigger auto completion or not.
         * Default is true.
         *
         * @param autoCompletionOnSetText true if setText() should trigger auto completion
         * @see #setText(java.lang.String)
         */
        public void SetAutoCompletionOnSetText(bool autoCompletionOnSetText)
        {
            this._autoCompletionOnSetText = autoCompletionOnSetText;
        }

        public bool IsScrollToCursorOnSizeChange()
        {
            return _scrollToCursorOnSizeChange;
        }

        public void SetScrollToCursorOnSizeChange(bool scrollToCursorOnSizeChange)
        {
            this._scrollToCursorOnSizeChange = scrollToCursorOnSizeChange;
        }

        protected virtual void DoCallback(int key)
        {
            if (this.Callback != null)
            {
                this.Callback.Invoke(this, new EditFieldCallbackEventArgs(key));
            }
        }

        public bool IsPasswordMasking()
        {
            return _passwordMasking != null;
        }

        public void SetPasswordMasking(bool passwordMasking)
        {
            if (passwordMasking != IsPasswordMasking())
            {
                if (passwordMasking)
                {
                    this._passwordMasking = new PasswordMasker(_editBuffer, _passwordChar);
                }
                else
                {
                    this._passwordMasking = null;
                }
                UpdateTextDisplay();
            }
        }

        public char GetPasswordChar()
        {
            return _passwordChar;
        }

        public void SetPasswordChar(char passwordChar)
        {
            this._passwordChar = passwordChar;
            if (_passwordMasking != null && _passwordMasking._maskingChar != passwordChar)
            {
                _passwordMasking = new PasswordMasker(_editBuffer, passwordChar);
                UpdateTextDisplay();
            }
        }

        public int GetColumns()
        {
            return _columns;
        }

        /**
         * This is used to determine the desired width of the EditField based on
         * it's font and the character 'X'
         * 
         * @param columns number of characters
         * @throws IllegalArgumentException if columns < 0
         */
        public void SetColumns(int columns)
        {
            if (columns < 0)
            {
                throw new ArgumentOutOfRangeException("columns");
            }
            this._columns = columns;
        }

        public bool IsMultiLine()
        {
            return _multiLine;
        }

        /**
         * Controls multi line editing.
         *
         * Default is false (single line editing).
         *
         * Disabling multi line editing when multi line text is present
         * will clear the text.
         *
         * @param multiLine true for multi line editing.
         */
        public void SetMultiLine(bool multiLine)
        {
            this._multiLine = multiLine;
            if (!multiLine && _numberOfLines > 1)
            {
                SetText("");
            }
        }

        public StringModel GetModel()
        {
            return _model;
        }

        public void SetModel(StringModel model)
        {
            RemoveModelChangeListener();
            if (this._model != null)
            {
                this._model.Changed -= Model_Changed;
            }
            this._model = model;
            if (GetGUI() != null)
            {
                AddModelChangeListener();
            }
        }

        private void Model_Changed(object sender, StringChangedEventArgs e)
        {
            ModelChanged();
        }

        /**
         * Set a new text for this EditField.
         * If the new text is longer then {@link #getMaxTextLength()} then it is truncated.
         * The selection is cleared.
         * The cursor is positioned at the end of the new text (single line) or at
         * the start of the text (multi line).
         * If a model is set, then the model is also updated.
         *
         * @param text the new text
         * @throws NullReferenceException if text is null
         * @see #setMultiLine(bool)
         */
        public void SetText(String text)
        {
            SetText(text, false);
        }

        void SetText(String text, bool fromModel)
        {
            text = TextUtil.LimitStringLength(text, _maxTextLength);
            _editBuffer.Replace(0, _editBuffer.Length, text);
            _cursorPos = _multiLine ? 0 : _editBuffer.Length;
            _selectionStart = 0;
            _selectionEnd = 0;
            UpdateSelection();
            UpdateText(_autoCompletionOnSetText, fromModel, Event.KEY_NONE);
            ScrollToCursor(true);
        }

        public String GetText()
        {
            return _editBuffer.ToString();
        }

        public StringAttributes GetStringAttributes()
        {
            if (_attributes == null)
            {
                _textRenderer.SetCache(false);
                _attributes = new StringAttributes(_editBuffer, GetAnimationState());
            }
            return _attributes;
        }

        public void DisableStringAttributes()
        {
            if (_attributes != null)
            {
                _attributes = null;
            }
        }

        public String GetSelectedText()
        {
            return _editBuffer.Substring(_selectionStart, _selectionEnd);
        }

        public bool HasSelection()
        {
            return _selectionStart != _selectionEnd;
        }

        public int GetCursorPos()
        {
            return _cursorPos;
        }

        public int GetTextLength()
        {
            return _editBuffer.Length;
        }

        public bool IsReadOnly()
        {
            return _readOnly;
        }

        public void SetReadOnly(bool readOnly)
        {
            if (this._readOnly != readOnly)
            {
                this._readOnly = readOnly;
                this._popupMenu = null;  // popup menu depends on read only state
                GetAnimationState().SetAnimationState(STATE_READONLY, readOnly);
                FirePropertyChange("readonly", !readOnly, readOnly);
            }
        }

        public virtual void InsertText(String str)
        {
            if (!_readOnly)
            {
                bool update = false;
                if (HasSelection())
                {
                    DeleteSelection();
                    update = true;
                }
                int insertLength = Math.Min(str.Length, _maxTextLength - _editBuffer.Length);
                if (insertLength > 0)
                {
                    int inserted = _editBuffer.Replace(_cursorPos, 0, str.Substring(0, insertLength));
                    if (inserted > 0)
                    {
                        _cursorPos += inserted;
                        update = true;
                    }
                }
                if (update)
                {
                    UpdateText(true, false, Event.KEY_NONE);
                }
            }
        }

        public void PasteFromClipboard()
        {
            String cbText = Clipboard.GetClipboard();
            if (cbText != null)
            {
                if (!_multiLine)
                {
                    cbText = TextUtil.StripNewLines(cbText);
                }
                InsertText(cbText);
            }
        }

        public void CopyToClipboard()
        {
            String text;
            if (HasSelection())
            {
                text = GetSelectedText();
            }
            else
            {
                text = GetText();
            }
            if (IsPasswordMasking())
            {
                text = TextUtil.CreateString(_passwordChar, text.Length);
            }
            Clipboard.SetClipboard(text);
        }

        public void CutToClipboard()
        {
            String text;
            if (!HasSelection())
            {
                SelectAll();
            }
            text = GetSelectedText();
            if (!_readOnly)
            {
                DeleteSelection();
                UpdateText(true, false, Event.KEY_DELETE);
            }
            if (IsPasswordMasking())
            {
                text = TextUtil.CreateString(_passwordChar, text.Length);
            }
            Clipboard.SetClipboard(text);
        }

        public void DuplicateLineDown()
        {
            if (_multiLine && !_readOnly)
            {
                int lineStart, lineEnd;
                if (HasSelection())
                {
                    lineStart = _selectionStart;
                    lineEnd = _selectionEnd;
                }
                else
                {
                    lineStart = _cursorPos;
                    lineEnd = _cursorPos;
                }
                lineStart = ComputeLineStart(lineStart);
                lineEnd = ComputeLineEnd(lineEnd);
                String line = _editBuffer.Substring(lineStart, lineEnd);
                line = "\n" + line;
                _editBuffer.Replace(lineEnd, 0, line);
                SetCursorPos(_cursorPos + line.Length);
                UpdateText(true, false, Event.KEY_NONE);
            }
        }

        public int GetMaxTextLength()
        {
            return _maxTextLength;
        }

        public void SetMaxTextLength(int maxTextLength)
        {
            this._maxTextLength = maxTextLength;
        }

        void RemoveModelChangeListener()
        {
            if (_model != null)
            {
                this._model.Changed -= Model_Changed;
            }
        }

        void AddModelChangeListener()
        {
            if (_model != null)
            {
                this._model.Changed += Model_Changed;
                ModelChanged();
            }
        }

        protected override void AfterAddToGUI(GUI gui)
        {
            base.AfterAddToGUI(gui);
            AddModelChangeListener();
        }

        protected override void BeforeRemoveFromGUI(GUI gui)
        {
            RemoveModelChangeListener();
            base.BeforeRemoveFromGUI(gui);
        }

        protected override void ApplyTheme(ThemeInfo themeInfo)
        {
            base.ApplyTheme(themeInfo);
            ApplyThemeEditField(themeInfo);
        }

        protected void ApplyThemeEditField(ThemeInfo themeInfo)
        {
            _cursorImage = themeInfo.GetImage("cursor");
            _selectionImage = themeInfo.GetImage("selection");
            _autoCompletionHeight = themeInfo.GetParameter("autocompletion-height", 100);
            _columns = themeInfo.GetParameter("columns", 5);
            SetPasswordChar((char)themeInfo.GetParameter("passwordChar", '*'));
        }

        protected override void Layout()
        {
            LayoutChildFullInnerArea(_textRenderer);
            CheckTextWidth();
            LayoutInfoWindows();
        }

        //@Override
        protected override void PositionChanged()
        {
            LayoutInfoWindows();
        }

        private void LayoutInfoWindows()
        {
            if (_autoCompletionWindow != null)
            {
                LayoutAutocompletionWindow();
            }
            if (_errorInfoWindow != null)
            {
                LayoutErrorInfoWindow();
            }
        }

        private void LayoutAutocompletionWindow()
        {
            int y = GetBottom();
            GUI gui = GetGUI();
            if (gui != null)
            {
                if (y + _autoCompletionHeight > gui.GetInnerBottom())
                {
                    int ytop = GetY() - _autoCompletionHeight;
                    if (ytop >= gui.GetInnerY())
                    {
                        y = ytop;
                    }
                }
            }
            _autoCompletionWindow.SetPosition(GetX(), y);
            _autoCompletionWindow.SetSize(GetWidth(), _autoCompletionHeight);
        }

        private int ComputeInnerWidth()
        {
            if (_columns > 0)
            {
                Font font = GetFont();
                if (font != null)
                {
                    return font.ComputeTextWidth("X") * _columns;
                }
            }
            return 0;
        }

        private int ComputeInnerHeight()
        {
            int lineHeight = GetLineHeight();
            if (_multiLine)
            {
                return lineHeight * _numberOfLines;
            }
            return lineHeight;
        }

        //@Override
        public override int GetMinWidth()
        {
            int minWidth = base.GetMinWidth();
            minWidth = Math.Max(minWidth, ComputeInnerWidth() + GetBorderHorizontal());
            return minWidth;
        }

        //@Override
        public override int GetMinHeight()
        {
            int minHeight = base.GetMinHeight();
            minHeight = Math.Max(minHeight, ComputeInnerHeight() + GetBorderVertical());
            return minHeight;
        }

        //@Override
        public override int GetPreferredInnerWidth()
        {
            return ComputeInnerWidth();
        }

        //@Override
        public override int GetPreferredInnerHeight()
        {
            return ComputeInnerHeight();
        }

        public void SetErrorMessage(Object errorMsg)
        {
            _errorMsgFromModel = false;
            GetAnimationState().SetAnimationState(STATE_ERROR, errorMsg != null);
            if (this._errorMsg != errorMsg)
            {
                this._errorMsg = errorMsg;
                UpdateTooltip();
            }
            if (errorMsg != null)
            {
                if (HasKeyboardFocus())
                {
                    OpenErrorInfoWindow();
                }
            }
            else if (_errorInfoWindow != null)
            {
                _errorInfoWindow.CloseInfo();
            }
        }

        //@Override
        public override Object GetTooltipContent()
        {
            if (_errorMsg != null)
            {
                return _errorMsg;
            }
            Object tooltip = base.GetTooltipContent();
            if (tooltip == null && !IsPasswordMasking() && _textLongerThenWidget && !HasKeyboardFocus())
            {
                tooltip = GetText();
            }
            return tooltip;
        }

        public void SetAutoCompletionWindow(EditFieldAutoCompletionWindow window)
        {
            if (_autoCompletionWindow != window)
            {
                if (_autoCompletionWindow != null)
                {
                    _autoCompletionWindow.CloseInfo();
                }

                _autoCompletionWindow = window;
            }
        }

        public EditFieldAutoCompletionWindow GetAutoCompletionWindow()
        {
            return _autoCompletionWindow;
        }

        /**
         * Installs a new auto completion window with the given data source.
         * 
         * @param dataSource the data source used for auto completion - can be null
         * @see EditFieldAutoCompletionWindow#EditFieldAutoCompletionWindow(de.matthiasmann.twl.EditField, de.matthiasmann.twl.model.AutoCompletionDataSource) 
         */
        public void SetAutoCompletion(AutoCompletionDataSource dataSource)
        {
            if (dataSource == null)
            {
                SetAutoCompletionWindow(null);
            }
            else
            {
                SetAutoCompletionWindow(new EditFieldAutoCompletionWindow(this, dataSource));
            }
        }

        /**
         * Installs a new auto completion window with the given data source.
         *
         * @param dataSource the data source used for auto completion - can be null
         * @param executorService the executorService used to execute the data source queries
         * @see EditFieldAutoCompletionWindow#EditFieldAutoCompletionWindow(de.matthiasmann.twl.EditField, de.matthiasmann.twl.model.AutoCompletionDataSource, java.util.concurrent.ExecutorService)
         */
        /*public void setAutoCompletion(AutoCompletionDataSource dataSource, ExecutorService executorService)
        {
            if (dataSource == null)
            {
                setAutoCompletionWindow(null);
            }
            else
            {
                setAutoCompletionWindow(new EditFieldAutoCompletionWindow(this, dataSource, executorService));
            }
        }
        */

        //@Override
        public override bool HandleEvent(Event evt)
        {
            bool selectPressed = (evt.GetModifiers() & Event.MODIFIER_SHIFT) != 0;

            if (evt.IsMouseEvent())
            {
                bool hover = (evt.GetEventType() != EventType.MOUSE_EXITED) && IsMouseInside(evt);
                GetAnimationState().SetAnimationState(STATE_HOVER, hover);
            }

            if (evt.IsMouseDragEvent())
            {
                if (evt.GetEventType() == EventType.MOUSE_DRAGGED &&
                        (evt.GetModifiers() & Event.MODIFIER_LBUTTON) != 0)
                {
                    int newPos = GetCursorPosFromMouse(evt.GetMouseX(), evt.GetMouseY());
                    SetCursorPos(newPos, true);
                }
                return true;
            }

            if (base.HandleEvent(evt))
            {
                return true;
            }

            if (_autoCompletionWindow != null)
            {
                if (_autoCompletionWindow.HandleEvent(evt))
                {
                    return true;
                }
            }

            EventType type = evt.GetEventType();
            if (type == EventType.KEY_PRESSED)
            {
                switch (evt.GetKeyCode())
                {
                    case Event.KEY_BACK:
                        DeletePrev();
                        return true;
                    case Event.KEY_DELETE:
                        DeleteNext();
                        return true;
                    case Event.KEY_NUMPADENTER:
                    case Event.KEY_RETURN:
                        if (_multiLine)
                        {
                            if (evt.HasKeyCharNoModifiers())
                            {
                                InsertChar('\n');
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            DoCallback(Event.KEY_RETURN);
                        }
                        return true;
                    case Event.KEY_ESCAPE:
                        DoCallback(evt.GetKeyCode());
                        return true;
                    case Event.KEY_HOME:
                        SetCursorPos(ComputeLineStart(_cursorPos), selectPressed);
                        return true;
                    case Event.KEY_END:
                        SetCursorPos(ComputeLineEnd(_cursorPos), selectPressed);
                        return true;
                    case Event.KEY_LEFT:
                        MoveCursor(-1, selectPressed);
                        return true;
                    case Event.KEY_RIGHT:
                        MoveCursor(+1, selectPressed);
                        return true;
                    case Event.KEY_UP:
                        if (_multiLine)
                        {
                            MoveCursorY(-1, selectPressed);
                            return true;
                        }
                        break;
                    case Event.KEY_DOWN:
                        if (_multiLine)
                        {
                            MoveCursorY(+1, selectPressed);
                            return true;
                        }
                        break;
                    case Event.KEY_TAB:
                        return false;
                    default:
                        if (evt.HasKeyCharNoModifiers())
                        {
                            InsertChar(evt.GetKeyChar());
                            return true;
                        }
                        break;
                }
                if (_forwardUnhandledKeysToCallback)
                {
                    DoCallback(evt.GetKeyCode());
                    return true;
                }
                return false;
            }
            else if (evt.GetEventType() == EventType.KEY_RELEASED)
            {
                switch (evt.GetKeyCode())
                {
                    case Event.KEY_BACK:
                    case Event.KEY_DELETE:
                    case Event.KEY_NUMPADENTER:
                    case Event.KEY_RETURN:
                    case Event.KEY_ESCAPE:
                    case Event.KEY_HOME:
                    case Event.KEY_END:
                    case Event.KEY_LEFT:
                    case Event.KEY_RIGHT:
                        return true;
                    default:
                        return evt.HasKeyCharNoModifiers() || _forwardUnhandledKeysToCallback;
                }
            }
            else if (evt.GetEventType() == EventType.MOUSE_BTNUP)
            {
                if (evt.GetMouseButton() == Event.MOUSE_RBUTTON && IsMouseInside(evt))
                {
                    ShowPopupMenu(evt);
                    return true;
                }
            }
            else if (evt.GetEventType() == EventType.MOUSE_BTNDOWN)
            {
                if (evt.GetMouseButton() == Event.MOUSE_LBUTTON && IsMouseInside(evt))
                {
                    int newPos = GetCursorPosFromMouse(evt.GetMouseX(), evt.GetMouseY());
                    SetCursorPos(newPos, selectPressed);
                    _scrollPos = _textRenderer._lastScrollPos;
                    return true;
                }
            }
            else if (evt.GetEventType() == EventType.MOUSE_CLICKED)
            {
                if (evt.GetMouseClickCount() == 2)
                {
                    int newPos = GetCursorPosFromMouse(evt.GetMouseX(), evt.GetMouseY());
                    SelectWordFromMouse(newPos);
                    this._cursorPos = _selectionStart;
                    ScrollToCursor(false);
                    this._cursorPos = _selectionEnd;
                    ScrollToCursor(false);
                    return true;
                }
                if (evt.GetMouseClickCount() == 3)
                {
                    SelectAll();
                    return true;
                }
            }
            else if (evt.GetEventType() == EventType.MOUSE_WHEEL)
            {
                return false;
            }

            return evt.IsMouseEvent();
        }

        protected void ShowPopupMenu(Event evt)
        {
            if (_popupMenu == null)
            {
                _popupMenu = CreatePopupMenu();
            }
            if (_popupMenu != null)
            {
                _popupMenu.OpenPopupMenu(this, evt.GetMouseX(), evt.GetMouseY());
            }
        }

        protected Menu CreatePopupMenu()
        {
            Menu menu = new Menu();
            if (!_readOnly)
            {
                menu.Add("cut", new ActionCallback(this, "cut").Run);
            }
            menu.Add("copy", new ActionCallback(this, "copy").Run);
            if (!_readOnly)
            {
                menu.Add("paste", new ActionCallback(this, "paste").Run);
                //menu.add("clear", new Runnable()
                //{
                //public void run()
                //{
                //    if (!isReadOnly())
                //    {
                //        setText("");
                //    }
                // }
                //});
            }
            menu.AddSpacer();
            menu.Add("select all", new ActionCallback(this, "selectAll").Run);
            return menu;
        }

        private void UpdateText(bool bUpdateAutoCompletion, bool fromModel, int key)
        {
            if (_model != null && !fromModel)
            {
                try
                {
                    _model.Value = GetText();
                    if (_errorMsgFromModel)
                    {
                        SetErrorMessage(null);
                    }
                }
                catch (Exception ex)
                {
                    if (_errorMsg == null || _errorMsgFromModel)
                    {
                        SetErrorMessage(ex.Message);
                        _errorMsgFromModel = true;
                    }
                }
            }
            UpdateTextDisplay();
            if (_multiLine)
            {
                int numLines = _textRenderer.GetNumTextLines();
                if (_numberOfLines != numLines)
                {
                    _numberOfLines = numLines;
                    InvalidateLayout();
                }
            }
            DoCallback(key);
            if (_autoCompletionWindow != null && _autoCompletionWindow.IsOpen() || bUpdateAutoCompletion)
            {
                UpdateAutoCompletion();
            }
        }

        private void UpdateTextDisplay()
        {
            _textRenderer.SetCharSequence(_passwordMasking != null ? _passwordMasking.Value : _editBuffer.Value);
            _textRenderer._cacheDirty = true;
            CheckTextWidth();
            ScrollToCursor(false);
        }

        private void CheckTextWidth()
        {
            _textLongerThenWidget = _textRenderer.GetPreferredWidth() > _textRenderer.GetWidth();
        }

        protected void MoveCursor(int dir, bool select)
        {
            SetCursorPos(_cursorPos + dir, select);
        }

        protected void MoveCursorY(int dir, bool select)
        {
            if (_multiLine)
            {
                int x = ComputeRelativeCursorPositionX(_cursorPos);
                int lineStart;
                if (dir < 0)
                {
                    lineStart = ComputeLineStart(_cursorPos);
                    if (lineStart == 0)
                    {
                        SetCursorPos(0, select);
                        return;
                    }
                    lineStart = ComputeLineStart(lineStart - 1);
                }
                else
                {
                    lineStart = Math.Min(ComputeLineEnd(_cursorPos) + 1, _editBuffer.Length);
                }
                SetCursorPos(ComputeCursorPosFromX(x, lineStart), select);
            }
        }

        protected internal void SetCursorPos(int pos, bool select)
        {
            pos = Math.Max(0, Math.Min(_editBuffer.Length, pos));

            if (!select)
            {
                bool hadSelection = HasSelection();
                _selectionStart = pos;
                _selectionEnd = pos;
                if (hadSelection)
                {
                    UpdateSelection();
                }
            }

            if (this._cursorPos != pos)
            {
                if (select)
                {
                    if (HasSelection())
                    {
                        if (_cursorPos == _selectionStart)
                        {
                            _selectionStart = pos;
                        }
                        else
                        {
                            _selectionEnd = pos;
                        }
                    }
                    else
                    {
                        _selectionStart = _cursorPos;
                        _selectionEnd = pos;
                    }
                    if (_selectionStart > _selectionEnd)
                    {
                        int t = _selectionStart;
                        _selectionStart = _selectionEnd;
                        _selectionEnd = t;
                    }
                    UpdateSelection();
                }

                if (this._cursorPos != pos)
                {
                    GetAnimationState().ResetAnimationTime(STATE_CURSOR_MOVED);
                }
                this._cursorPos = pos;
                ScrollToCursor(false);
                UpdateAutoCompletion();
            }
        }

        protected void UpdateSelection()
        {
            if (_attributes != null)
            {
                _attributes.RemoveAnimationState(TextWidget.STATE_TEXT_SELECTION);
                _attributes.SetAnimationState(TextWidget.STATE_TEXT_SELECTION,
                        _selectionStart, _selectionEnd, true);
                _attributes.Optimize();
                _textRenderer._cacheDirty = true;
            }
        }

        public void SetCursorPos(int pos)
        {
            if (pos < 0 || pos > _editBuffer.Length)
            {
                throw new ArgumentOutOfRangeException("pos");
            }
            SetCursorPos(pos, false);
        }

        public void SelectAll()
        {
            _selectionStart = 0;
            _selectionEnd = _editBuffer.Length;
            UpdateSelection();
        }

        public void SetSelection(int start, int end)
        {
            if (start < 0 || start > end || end > _editBuffer.Length)
            {
                throw new ArgumentOutOfRangeException();
            }
            _selectionStart = start;
            _selectionEnd = end;
            UpdateSelection();
        }

        protected void SelectWordFromMouse(int index)
        {
            _selectionStart = index;
            _selectionEnd = index;
            while (_selectionStart > 0 && !Char.IsWhiteSpace(_editBuffer.CharAt(_selectionStart - 1)))
            {
                _selectionStart--;
            }
            while (_selectionEnd < _editBuffer.Length && !Char.IsWhiteSpace(_editBuffer.CharAt(_selectionEnd)))
            {
                _selectionEnd++;
            }
            UpdateSelection();
        }

        protected void ScrollToCursor(bool force)
        {
            int renderWidth = _textRenderer.GetWidth() - 5;
            if (renderWidth <= 0)
            {
                _pendingScrollToCursor = true;
                _pendingScrollToCursorForce = force;
                return;
            }
            _pendingScrollToCursor = false;
            int xpos = ComputeRelativeCursorPositionX(_cursorPos);
            if (xpos < _scrollPos + 5)
            {
                _scrollPos = Math.Max(0, xpos - 5);
            }
            else if (force || xpos - _scrollPos > renderWidth)
            {
                _scrollPos = Math.Max(0, xpos - renderWidth);
            }
            if (_multiLine)
            {
                ScrollPane sp = ScrollPane.GetContainingScrollPane(this);
                if (sp != null)
                {
                    int lineHeight = GetLineHeight();
                    int lineY = ComputeLineNumber(_cursorPos) * lineHeight;
                    sp.ValidateLayout();
                    sp.ScrollToAreaY(lineY, lineHeight, lineHeight / 2);
                }
            }
        }

        protected virtual void InsertChar(char ch)
        {
            // don't add control characters
            if (!_readOnly && (!Char.IsControl(ch) || (_multiLine && ch == '\n')))
            {
                bool update = false;
                if (HasSelection())
                {
                    DeleteSelection();
                    update = true;
                }
                if (_editBuffer.Length < _maxTextLength)
                {
                    if (_editBuffer.Replace(_cursorPos, 0, ch))
                    {
                        _cursorPos++;
                        update = true;
                    }
                }
                if (update)
                {
                    UpdateText(true, false, Event.KEY_NONE);
                }
            }
        }

        protected void DeletePrev()
        {
            if (!_readOnly)
            {
                if (HasSelection())
                {
                    DeleteSelection();
                    UpdateText(true, false, Event.KEY_DELETE);
                }
                else if (_cursorPos > 0)
                {
                    --_cursorPos;
                    DeleteNext();
                }
            }
        }

        protected void DeleteNext()
        {
            if (!_readOnly)
            {
                if (HasSelection())
                {
                    DeleteSelection();
                    UpdateText(true, false, Event.KEY_DELETE);
                }
                else if (_cursorPos < _editBuffer.Length)
                {
                    if (_editBuffer.Replace(_cursorPos, 1, "") >= 0)
                    {
                        UpdateText(true, false, Event.KEY_DELETE);
                    }
                }
            }
        }

        protected void DeleteSelection()
        {
            if (_editBuffer.Replace(_selectionStart, _selectionEnd - _selectionStart, "") >= 0)
            {
                SetCursorPos(_selectionStart, false);
            }
        }

        protected void ModelChanged()
        {
            String modelText = _model.Value;
            if (_editBuffer.Length != modelText.Length || !GetText().Equals(modelText))
            {
                SetText(modelText, true);
            }
        }

        protected bool HasFocusOrPopup()
        {
            return HasKeyboardFocus() || HasOpenPopups();
        }

        protected Font GetFont()
        {
            return _textRenderer.GetFont();
        }

        protected int GetLineHeight()
        {
            Font font = GetFont();
            if (font != null)
            {
                return font.LineHeight;
            }
            return 0;
        }

        protected int ComputeLineNumber(int cursorPos)
        {
            EditFieldModel eb = this._editBuffer;
            int lineNr = 0;
            for (int i = 0; i < cursorPos; i++)
            {
                if (eb.CharAt(i) == '\n')
                {
                    lineNr++;
                }
            }
            return lineNr;
        }

        protected int ComputeLineStart(int cursorPos)
        {
            if (!_multiLine)
            {
                return 0;
            }
            EditFieldModel eb = this._editBuffer;
            while (cursorPos > 0 && eb.CharAt(cursorPos - 1) != '\n')
            {
                cursorPos--;
            }
            return cursorPos;
        }

        protected int ComputeLineEnd(int cursorPos)
        {
            EditFieldModel eb = this._editBuffer;
            int endIndex = eb.Length;
            if (!_multiLine)
            {
                return endIndex;
            }
            while (cursorPos < endIndex && eb.CharAt(cursorPos) != '\n')
            {
                cursorPos++;
            }
            return cursorPos;
        }

        protected int ComputeRelativeCursorPositionX(int cursorPos)
        {
            int lineStart = 0;
            if (_multiLine)
            {
                lineStart = ComputeLineStart(cursorPos);
            }
            return _textRenderer.ComputeRelativeCursorPositionX(lineStart, cursorPos);
        }

        protected int ComputeRelativeCursorPositionY(int cursorPos)
        {
            if (_multiLine)
            {
                return GetLineHeight() * ComputeLineNumber(cursorPos);
            }
            return 0;
        }

        protected int GetCursorPosFromMouse(int x, int y)
        {
            Font font = GetFont();
            if (font != null)
            {
                x -= _textRenderer._lastTextX;
                int lineStart = 0;
                int lineEnd = _editBuffer.Length;
                if (_multiLine)
                {
                    y -= _textRenderer.ComputeTextY();
                    int lineHeight = font.LineHeight;
                    int endIndex = lineEnd;
                    for (; ; )
                    {
                        lineEnd = ComputeLineEnd(lineStart);

                        if (lineStart >= endIndex || y < lineHeight)
                        {
                            break;
                        }

                        lineStart = Math.Min(lineEnd + 1, endIndex);
                        y -= lineHeight;
                    }
                }
                return ComputeCursorPosFromX(x, lineStart, lineEnd);
            }
            else
            {
                return 0;
            }
        }

        protected int ComputeCursorPosFromX(int x, int lineStart)
        {
            return ComputeCursorPosFromX(x, lineStart, ComputeLineEnd(lineStart));
        }

        protected int ComputeCursorPosFromX(int x, int lineStart, int lineEnd)
        {
            Font font = GetFont();
            if (font != null)
            {
                return lineStart + font.ComputeVisibleGlyphs(
                        (_passwordMasking != null) ? _passwordMasking.Value : _editBuffer.Value,
                        lineStart, lineEnd, x + font.SpaceWidth / 2);
            }
            return lineStart;
        }

        //@Override
        protected override void PaintOverlay(GUI gui)
        {
            if (_cursorImage != null && HasFocusOrPopup())
            {
                int xpos = _textRenderer._lastTextX + ComputeRelativeCursorPositionX(_cursorPos);
                int ypos = _textRenderer.ComputeTextY() + ComputeRelativeCursorPositionY(_cursorPos);
                _cursorImage.Draw(GetAnimationState(), xpos, ypos, _cursorImage.Width, GetLineHeight());
            }
            base.PaintOverlay(gui);
        }

        private void OpenErrorInfoWindow()
        {
            if (_autoCompletionWindow == null || !_autoCompletionWindow.IsOpen())
            {
                if (_errorInfoWindow == null)
                {
                    _errorInfoLabel = new Label();
                    _errorInfoLabel.SetClip(true);
                    _errorInfoWindow = new InfoWindow(this);
                    _errorInfoWindow.SetTheme("editfield-errorinfowindow");
                    _errorInfoWindow.Add(_errorInfoLabel);
                }
                _errorInfoLabel.SetText(_errorMsg.ToString());
                _errorInfoWindow.OpenInfo();
                LayoutErrorInfoWindow();
            }
        }

        private void LayoutErrorInfoWindow()
        {
            int x = GetX();
            int width = GetWidth();

            Widget container = _errorInfoWindow.GetParent();
            if (container != null)
            {
                width = Math.Max(width, ComputeSize(
                        _errorInfoWindow.GetMinWidth(),
                        _errorInfoWindow.GetPreferredWidth(),
                        _errorInfoWindow.GetMaxWidth()));
                int popupMaxRight = container.GetInnerRight();
                if (x + width > popupMaxRight)
                {
                    x = popupMaxRight - Math.Min(width, container.GetInnerWidth());
                }
                _errorInfoWindow.SetSize(width, _errorInfoWindow.GetPreferredHeight());
                _errorInfoWindow.SetPosition(x, GetBottom());
            }
        }

        protected override void KeyboardFocusGained()
        {
            if (_errorMsg != null)
            {
                OpenErrorInfoWindow();
            }
            else
            {
                UpdateAutoCompletion();
            }
        }

        protected override void KeyboardFocusLost()
        {
            base.KeyboardFocusLost();
            if (_errorInfoWindow != null)
            {
                _errorInfoWindow.CloseInfo();
            }
            if (_autoCompletionWindow != null)
            {
                _autoCompletionWindow.CloseInfo();
            }
        }

        protected void UpdateAutoCompletion()
        {
            if (_autoCompletionWindow != null)
            {
                _autoCompletionWindow.UpdateAutoCompletion();
            }
        }

        internal class TextRenderer : TextWidget
        {
            internal int _lastTextX;
            internal int _lastScrollPos;
            internal AttributedStringFontCache _cache;
            internal bool _cacheDirty;
            internal EditField _editField;

            protected internal TextRenderer(EditField editField, AnimationState animState) : base(animState)
            {
                this._editField = editField;
            }

            protected override void PaintWidget(GUI gui)
            {
                if (this._editField._pendingScrollToCursor)
                {
                    this._editField.ScrollToCursor(this._editField._pendingScrollToCursorForce);
                }
                _lastScrollPos = this._editField.HasFocusOrPopup() ? this._editField._scrollPos : 0;
                _lastTextX = ComputeTextX();
                Font font = GetFont();
                if (this._editField._attributes != null && font is Font2) {
                    PaintWithAttributes((Font2)font);
                }
                else if (this._editField.HasSelection() && this._editField.HasFocusOrPopup())
                {
                    if (this._editField._multiLine)
                    {
                        PaintMultiLineWithSelection();
                    }
                    else
                    {
                        PaintWithSelection(0, this._editField._editBuffer.Length, ComputeTextY());
                    }
                }
                else
                {
                    PaintLabelText(GetAnimationState());
                }
            }

            protected void PaintWithSelection(int lineStart, int lineEnd, int yoff)
            {
                int selStart = this._editField._selectionStart;
                int selEnd = this._editField._selectionEnd;
                if (this._editField._selectionImage != null && selEnd > lineStart && selStart <= lineEnd)
                {
                    int xpos0 = _lastTextX + ComputeRelativeCursorPositionX(lineStart, selStart);
                    int xpos1 = (lineEnd < selEnd) ? GetInnerRight() :
                            _lastTextX + ComputeRelativeCursorPositionX(lineStart, Math.Min(lineEnd, selEnd));
                    this._editField._selectionImage.Draw(GetAnimationState(), xpos0, yoff,
                            xpos1 - xpos0, GetFont().LineHeight);
                }

                PaintWithSelection(GetAnimationState(), selStart, selEnd, lineStart, lineEnd, yoff);
            }

            protected void PaintMultiLineWithSelection()
            {
                EditFieldModel eb = this._editField._editBuffer;
                int lineStart = 0;
                int endIndex = eb.Length;
                int yoff = ComputeTextY();
                int lineHeight = this._editField.GetLineHeight();
                while (lineStart < endIndex)
                {
                    int lineEnd = this._editField.ComputeLineEnd(lineStart);

                    PaintWithSelection(lineStart, lineEnd, yoff);

                    yoff += lineHeight;
                    lineStart = lineEnd + 1;
                }
            }

            protected void PaintMultiLineSelectionBackground()
            {
                int lineHeight = this._editField.GetLineHeight();
                int lineStart = this._editField.ComputeLineStart(this._editField._selectionStart);
                int lineNumber = this._editField.ComputeLineNumber(lineStart);
                int endIndex = this._editField._selectionEnd;
                int yoff = ComputeTextY() + lineHeight * lineNumber;
                int xstart = _lastTextX + ComputeRelativeCursorPositionX(lineStart, this._editField._selectionStart);
                while (lineStart < endIndex)
                {
                    int lineEnd = this._editField.ComputeLineEnd(lineStart);
                    int xend;

                    if (lineEnd < endIndex)
                    {
                        xend = GetInnerRight();
                    }
                    else
                    {
                        xend = _lastTextX + ComputeRelativeCursorPositionX(lineStart, endIndex);
                    }

                    this._editField._selectionImage.Draw(GetAnimationState(), xstart, yoff, xend - xstart, lineHeight);

                    yoff += lineHeight;
                    lineStart = lineEnd + 1;
                    xstart = GetInnerX();
                }
            }

            protected void PaintWithAttributes(Font2 font)
            {
                if (this._editField._selectionEnd > this._editField._selectionStart && this._editField._selectionImage != null)
                {
                    PaintMultiLineSelectionBackground();
                }
                if (_cache == null || _cacheDirty)
                {
                    _cacheDirty = false;
                    if (this._editField._multiLine)
                    {
                        _cache = font.CacheMultiLineText(_cache, this._editField._attributes);
                    }
                    else
                    {
                        _cache = font.CacheText(_cache, this._editField._attributes);
                    }
                }
                int y = ComputeTextY();
                if (_cache != null)
                {
                    _cache.Draw(_lastTextX, y);
                }
                else if (this._editField._multiLine)
                {
                    font.DrawMultiLineText(_lastTextX, y, this._editField._attributes);
                }
                else
                {
                    font.DrawText(_lastTextX, y, this._editField._attributes);
                }
            }

            protected override void SizeChanged()
            {
                if (this._editField._scrollToCursorOnSizeChange)
                {
                    this._editField.ScrollToCursor(true);
                }
            }

            protected override int ComputeTextX()
            {
                int x = GetInnerX();
                int pos = GetAlignment().GetHPosition();
                if (pos > 0)
                {
                    x += Math.Max(0, GetInnerWidth() - ComputeTextWidth()) * pos / 2;
                }
                return x - _lastScrollPos;
            }

            public override void Destroy()
            {
                base.Destroy();
                if (_cache != null)
                {
                    _cache.Dispose();
                    _cache = null;
                }
            }
        }

        public class PasswordMasker : CharSequence
        {
            CharSequence _baseSeq;
            internal char _maskingChar;

            public string Value
            {
                get
                {
                    string outputValue = "";
                    for (int i = 0; i < this.Length; i++)
                    {
                        outputValue += this.CharAt(i);
                    }
                    return outputValue;
                }
            }

            public int Length
            {
                get
                {
                    return _baseSeq.Length;
                }
            }

            public PasswordMasker(CharSequence baseSeq, char maskingChar)
            {
                this._baseSeq = baseSeq;
                this._maskingChar = maskingChar;
            }

            public CharSequence Subsequence(int start, int end)
            {
                throw new InvalidOperationException("Not supported.");
            }

            public char CharAt(int index)
            {
                return _maskingChar;
            }

            public string SubSequence(int start, int end)
            {
                throw new NotImplementedException();
            }
        }
    }

    public class EditFieldCallbackEventArgs : EventArgs
    {
        public int Key;

        public EditFieldCallbackEventArgs(int key)
        {
            this.Key = key;
        }
    }
}
