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

        EditFieldModel editBuffer;
        private TextRenderer textRenderer;
        private PasswordMasker passwordMasking;
        private StringModel model;
        private bool readOnly;
        StringAttributes attributes;

        private int cursorPos;
        int scrollPos;
        int selectionStart;
        int selectionEnd;
        int numberOfLines;
        bool multiLine;
        bool pendingScrollToCursor;
        bool pendingScrollToCursorForce;
        private int maxTextLength = short.MaxValue;

        private int columns = 5;
        private Image cursorImage;
        Image selectionImage;
        private char passwordChar;
        private Object errorMsg;
        private bool errorMsgFromModel;
        private Menu popupMenu;
        private bool textLongerThenWidget;
        private bool forwardUnhandledKeysToCallback;
        private bool autoCompletionOnSetText = true;
        bool scrollToCursorOnSizeChange = true;

        private EditFieldAutoCompletionWindow autoCompletionWindow;
        private int autoCompletionHeight = 100;

        private InfoWindow errorInfoWindow;
        private Label errorInfoLabel;

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

            this.editBuffer = editFieldModel;
            this.textRenderer = new TextRenderer(this, getAnimationState());
            this.passwordChar = '*';

            textRenderer.setTheme("renderer");
            textRenderer.setClip(true);

            add(textRenderer);
            setCanAcceptKeyboardFocus(true);
            setDepthFocusTraversal(false);

            addActionMapping("cut", "cutToClipboard");
            addActionMapping("copy", "copyToClipboard");
            addActionMapping("paste", "pasteFromClipboard");
            addActionMapping("selectAll", "selectAll");
            addActionMapping("duplicateLineDown", "duplicateLineDown");
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


        public bool isForwardUnhandledKeysToCallback()
        {
            return forwardUnhandledKeysToCallback;
        }

        /**
         * Controls if unhandled key presses are forwarded to the callback or not.
         * Default is false. If set to true then the EditField will consume all key
         * presses.
         *
         * @param forwardUnhandledKeysToCallback true if unhandled keys should be forwarded to the callbacks
         */
        public void setForwardUnhandledKeysToCallback(bool forwardUnhandledKeysToCallback)
        {
            this.forwardUnhandledKeysToCallback = forwardUnhandledKeysToCallback;
        }

        public bool isAutoCompletionOnSetText()
        {
            return autoCompletionOnSetText;
        }

        /**
         * Controls if a call to setText() should trigger auto completion or not.
         * Default is true.
         *
         * @param autoCompletionOnSetText true if setText() should trigger auto completion
         * @see #setText(java.lang.String)
         */
        public void setAutoCompletionOnSetText(bool autoCompletionOnSetText)
        {
            this.autoCompletionOnSetText = autoCompletionOnSetText;
        }

        public bool isScrollToCursorOnSizeChange()
        {
            return scrollToCursorOnSizeChange;
        }

        public void setScrollToCursorOnSizeChange(bool scrollToCursorOnSizeChange)
        {
            this.scrollToCursorOnSizeChange = scrollToCursorOnSizeChange;
        }

        protected virtual void doCallback(int key)
        {
            if (this.Callback != null)
            {
                this.Callback.Invoke(this, new EditFieldCallbackEventArgs(key));
            }
        }

        public bool isPasswordMasking()
        {
            return passwordMasking != null;
        }

        public void setPasswordMasking(bool passwordMasking)
        {
            if (passwordMasking != isPasswordMasking())
            {
                if (passwordMasking)
                {
                    this.passwordMasking = new PasswordMasker(editBuffer, passwordChar);
                }
                else
                {
                    this.passwordMasking = null;
                }
                updateTextDisplay();
            }
        }

        public char getPasswordChar()
        {
            return passwordChar;
        }

        public void setPasswordChar(char passwordChar)
        {
            this.passwordChar = passwordChar;
            if (passwordMasking != null && passwordMasking.maskingChar != passwordChar)
            {
                passwordMasking = new PasswordMasker(editBuffer, passwordChar);
                updateTextDisplay();
            }
        }

        public int getColumns()
        {
            return columns;
        }

        /**
         * This is used to determine the desired width of the EditField based on
         * it's font and the character 'X'
         * 
         * @param columns number of characters
         * @throws IllegalArgumentException if columns < 0
         */
        public void setColumns(int columns)
        {
            if (columns < 0)
            {
                throw new ArgumentOutOfRangeException("columns");
            }
            this.columns = columns;
        }

        public bool isMultiLine()
        {
            return multiLine;
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
        public void setMultiLine(bool multiLine)
        {
            this.multiLine = multiLine;
            if (!multiLine && numberOfLines > 1)
            {
                setText("");
            }
        }

        public StringModel getModel()
        {
            return model;
        }

        public void setModel(StringModel model)
        {
            removeModelChangeListener();
            if (this.model != null)
            {
                this.model.Changed -= Model_Changed;
            }
            this.model = model;
            if (getGUI() != null)
            {
                addModelChangeListener();
            }
        }

        private void Model_Changed(object sender, StringChangedEventArgs e)
        {
            modelChanged();
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
        public void setText(String text)
        {
            setText(text, false);
        }

        void setText(String text, bool fromModel)
        {
            text = TextUtil.limitStringLength(text, maxTextLength);
            editBuffer.Replace(0, editBuffer.Length, text);
            cursorPos = multiLine ? 0 : editBuffer.Length;
            selectionStart = 0;
            selectionEnd = 0;
            updateSelection();
            updateText(autoCompletionOnSetText, fromModel, Event.KEY_NONE);
            scrollToCursor(true);
        }

        public String getText()
        {
            return editBuffer.ToString();
        }

        public StringAttributes getStringAttributes()
        {
            if (attributes == null)
            {
                textRenderer.setCache(false);
                attributes = new StringAttributes(editBuffer, getAnimationState());
            }
            return attributes;
        }

        public void disableStringAttributes()
        {
            if (attributes != null)
            {
                attributes = null;
            }
        }

        public String getSelectedText()
        {
            return editBuffer.Substring(selectionStart, selectionEnd);
        }

        public bool hasSelection()
        {
            return selectionStart != selectionEnd;
        }

        public int getCursorPos()
        {
            return cursorPos;
        }

        public int getTextLength()
        {
            return editBuffer.Length;
        }

        public bool isReadOnly()
        {
            return readOnly;
        }

        public void setReadOnly(bool readOnly)
        {
            if (this.readOnly != readOnly)
            {
                this.readOnly = readOnly;
                this.popupMenu = null;  // popup menu depends on read only state
                getAnimationState().setAnimationState(STATE_READONLY, readOnly);
                firePropertyChange("readonly", !readOnly, readOnly);
            }
        }

        public void insertText(String str)
        {
            if (!readOnly)
            {
                bool update = false;
                if (hasSelection())
                {
                    deleteSelection();
                    update = true;
                }
                int insertLength = Math.Min(str.Length, maxTextLength - editBuffer.Length);
                if (insertLength > 0)
                {
                    int inserted = editBuffer.Replace(cursorPos, 0, str.Substring(0, insertLength));
                    if (inserted > 0)
                    {
                        cursorPos += inserted;
                        update = true;
                    }
                }
                if (update)
                {
                    updateText(true, false, Event.KEY_NONE);
                }
            }
        }

        public void pasteFromClipboard()
        {
            String cbText = Clipboard.getClipboard();
            if (cbText != null)
            {
                if (!multiLine)
                {
                    cbText = TextUtil.stripNewLines(cbText);
                }
                insertText(cbText);
            }
        }

        public void copyToClipboard()
        {
            String text;
            if (hasSelection())
            {
                text = getSelectedText();
            }
            else
            {
                text = getText();
            }
            if (isPasswordMasking())
            {
                text = TextUtil.createString(passwordChar, text.Length);
            }
            Clipboard.setClipboard(text);
        }

        public void cutToClipboard()
        {
            String text;
            if (!hasSelection())
            {
                selectAll();
            }
            text = getSelectedText();
            if (!readOnly)
            {
                deleteSelection();
                updateText(true, false, Event.KEY_DELETE);
            }
            if (isPasswordMasking())
            {
                text = TextUtil.createString(passwordChar, text.Length);
            }
            Clipboard.setClipboard(text);
        }

        public void duplicateLineDown()
        {
            if (multiLine && !readOnly)
            {
                int lineStart, lineEnd;
                if (hasSelection())
                {
                    lineStart = selectionStart;
                    lineEnd = selectionEnd;
                }
                else
                {
                    lineStart = cursorPos;
                    lineEnd = cursorPos;
                }
                lineStart = computeLineStart(lineStart);
                lineEnd = computeLineEnd(lineEnd);
                String line = editBuffer.Substring(lineStart, lineEnd);
                line = "\n" + line;
                editBuffer.Replace(lineEnd, 0, line);
                setCursorPos(cursorPos + line.Length);
                updateText(true, false, Event.KEY_NONE);
            }
        }

        public int getMaxTextLength()
        {
            return maxTextLength;
        }

        public void setMaxTextLength(int maxTextLength)
        {
            this.maxTextLength = maxTextLength;
        }

        void removeModelChangeListener()
        {
            if (model != null)
            {
                this.model.Changed -= Model_Changed;
            }
        }

        void addModelChangeListener()
        {
            if (model != null)
            {
                this.model.Changed += Model_Changed;
                modelChanged();
            }
        }

        //@Override
        protected override void afterAddToGUI(GUI gui)
        {
            base.afterAddToGUI(gui);
            addModelChangeListener();
        }

        //@Override
        protected override void beforeRemoveFromGUI(GUI gui)
        {
            removeModelChangeListener();
            base.beforeRemoveFromGUI(gui);
        }

        //@Override
        protected override void applyTheme(ThemeInfo themeInfo)
        {
            base.applyTheme(themeInfo);
            applyThemeEditField(themeInfo);
        }

        protected void applyThemeEditField(ThemeInfo themeInfo)
        {
            cursorImage = themeInfo.getImage("cursor");
            selectionImage = themeInfo.getImage("selection");
            autoCompletionHeight = themeInfo.getParameter("autocompletion-height", 100);
            columns = themeInfo.getParameter("columns", 5);
            setPasswordChar((char)themeInfo.getParameter("passwordChar", '*'));
        }

        //@Override
        protected override void layout()
        {
            layoutChildFullInnerArea(textRenderer);
            checkTextWidth();
            layoutInfoWindows();
        }

        //@Override
        protected override void positionChanged()
        {
            layoutInfoWindows();
        }

        private void layoutInfoWindows()
        {
            if (autoCompletionWindow != null)
            {
                layoutAutocompletionWindow();
            }
            if (errorInfoWindow != null)
            {
                layoutErrorInfoWindow();
            }
        }

        private void layoutAutocompletionWindow()
        {
            int y = getBottom();
            GUI gui = getGUI();
            if (gui != null)
            {
                if (y + autoCompletionHeight > gui.getInnerBottom())
                {
                    int ytop = getY() - autoCompletionHeight;
                    if (ytop >= gui.getInnerY())
                    {
                        y = ytop;
                    }
                }
            }
            autoCompletionWindow.setPosition(getX(), y);
            autoCompletionWindow.setSize(getWidth(), autoCompletionHeight);
        }

        private int computeInnerWidth()
        {
            if (columns > 0)
            {
                Font font = getFont();
                if (font != null)
                {
                    return font.ComputeTextWidth("X") * columns;
                }
            }
            return 0;
        }

        private int computeInnerHeight()
        {
            int lineHeight = getLineHeight();
            if (multiLine)
            {
                return lineHeight * numberOfLines;
            }
            return lineHeight;
        }

        //@Override
        public override int getMinWidth()
        {
            int minWidth = base.getMinWidth();
            minWidth = Math.Max(minWidth, computeInnerWidth() + getBorderHorizontal());
            return minWidth;
        }

        //@Override
        public override int getMinHeight()
        {
            int minHeight = base.getMinHeight();
            minHeight = Math.Max(minHeight, computeInnerHeight() + getBorderVertical());
            return minHeight;
        }

        //@Override
        public override int getPreferredInnerWidth()
        {
            return computeInnerWidth();
        }

        //@Override
        public override int getPreferredInnerHeight()
        {
            return computeInnerHeight();
        }

        public void setErrorMessage(Object errorMsg)
        {
            errorMsgFromModel = false;
            getAnimationState().setAnimationState(STATE_ERROR, errorMsg != null);
            if (this.errorMsg != errorMsg)
            {
                this.errorMsg = errorMsg;
                updateTooltip();
            }
            if (errorMsg != null)
            {
                if (hasKeyboardFocus())
                {
                    openErrorInfoWindow();
                }
            }
            else if (errorInfoWindow != null)
            {
                errorInfoWindow.closeInfo();
            }
        }

        //@Override
        public override Object getTooltipContent()
        {
            if (errorMsg != null)
            {
                return errorMsg;
            }
            Object tooltip = base.getTooltipContent();
            if (tooltip == null && !isPasswordMasking() && textLongerThenWidget && !hasKeyboardFocus())
            {
                tooltip = getText();
            }
            return tooltip;
        }

        public void setAutoCompletionWindow(EditFieldAutoCompletionWindow window)
        {
            if (autoCompletionWindow != window)
            {
                if (autoCompletionWindow != null)
                {
                    autoCompletionWindow.closeInfo();
                }

                autoCompletionWindow = window;
            }
        }

        public EditFieldAutoCompletionWindow getAutoCompletionWindow()
        {
            return autoCompletionWindow;
        }

        /**
         * Installs a new auto completion window with the given data source.
         * 
         * @param dataSource the data source used for auto completion - can be null
         * @see EditFieldAutoCompletionWindow#EditFieldAutoCompletionWindow(de.matthiasmann.twl.EditField, de.matthiasmann.twl.model.AutoCompletionDataSource) 
         */
        public void setAutoCompletion(AutoCompletionDataSource dataSource)
        {
            if (dataSource == null)
            {
                setAutoCompletionWindow(null);
            }
            else
            {
                setAutoCompletionWindow(new EditFieldAutoCompletionWindow(this, dataSource));
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
        public override bool handleEvent(Event evt)
        {
            bool selectPressed = (evt.getModifiers() & Event.MODIFIER_SHIFT) != 0;

            if (evt.isMouseEvent())
            {
                bool hover = (evt.getEventType() != EventType.MOUSE_EXITED) && isMouseInside(evt);
                getAnimationState().setAnimationState(STATE_HOVER, hover);
            }

            if (evt.isMouseDragEvent())
            {
                if (evt.getEventType() == EventType.MOUSE_DRAGGED &&
                        (evt.getModifiers() & Event.MODIFIER_LBUTTON) != 0)
                {
                    int newPos = getCursorPosFromMouse(evt.getMouseX(), evt.getMouseY());
                    setCursorPos(newPos, true);
                }
                return true;
            }

            if (base.handleEvent(evt))
            {
                return true;
            }

            if (autoCompletionWindow != null)
            {
                if (autoCompletionWindow.handleEvent(evt))
                {
                    return true;
                }
            }

            EventType type = evt.getEventType();
            if (type == EventType.KEY_PRESSED)
            {
                switch (evt.getKeyCode())
                {
                    case Event.KEY_BACK:
                        deletePrev();
                        return true;
                    case Event.KEY_DELETE:
                        deleteNext();
                        return true;
                    case Event.KEY_NUMPADENTER:
                    case Event.KEY_RETURN:
                        if (multiLine)
                        {
                            if (evt.hasKeyCharNoModifiers())
                            {
                                insertChar('\n');
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            doCallback(Event.KEY_RETURN);
                        }
                        return true;
                    case Event.KEY_ESCAPE:
                        doCallback(evt.getKeyCode());
                        return true;
                    case Event.KEY_HOME:
                        setCursorPos(computeLineStart(cursorPos), selectPressed);
                        return true;
                    case Event.KEY_END:
                        setCursorPos(computeLineEnd(cursorPos), selectPressed);
                        return true;
                    case Event.KEY_LEFT:
                        moveCursor(-1, selectPressed);
                        return true;
                    case Event.KEY_RIGHT:
                        moveCursor(+1, selectPressed);
                        return true;
                    case Event.KEY_UP:
                        if (multiLine)
                        {
                            moveCursorY(-1, selectPressed);
                            return true;
                        }
                        break;
                    case Event.KEY_DOWN:
                        if (multiLine)
                        {
                            moveCursorY(+1, selectPressed);
                            return true;
                        }
                        break;
                    case Event.KEY_TAB:
                        return false;
                    default:
                        if (evt.hasKeyCharNoModifiers())
                        {
                            insertChar(evt.getKeyChar());
                            return true;
                        }
                        break;
                }
                if (forwardUnhandledKeysToCallback)
                {
                    doCallback(evt.getKeyCode());
                    return true;
                }
                return false;
            }
            else if (evt.getEventType() == EventType.KEY_RELEASED)
            {
                switch (evt.getKeyCode())
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
                        return evt.hasKeyCharNoModifiers() || forwardUnhandledKeysToCallback;
                }
            }
            else if (evt.getEventType() == EventType.MOUSE_BTNUP)
            {
                if (evt.getMouseButton() == Event.MOUSE_RBUTTON && isMouseInside(evt))
                {
                    showPopupMenu(evt);
                    return true;
                }
            }
            else if (evt.getEventType() == EventType.MOUSE_BTNDOWN)
            {
                if (evt.getMouseButton() == Event.MOUSE_LBUTTON && isMouseInside(evt))
                {
                    int newPos = getCursorPosFromMouse(evt.getMouseX(), evt.getMouseY());
                    setCursorPos(newPos, selectPressed);
                    scrollPos = textRenderer.lastScrollPos;
                    return true;
                }
            }
            else if (evt.getEventType() == EventType.MOUSE_CLICKED)
            {
                if (evt.getMouseClickCount() == 2)
                {
                    int newPos = getCursorPosFromMouse(evt.getMouseX(), evt.getMouseY());
                    selectWordFromMouse(newPos);
                    this.cursorPos = selectionStart;
                    scrollToCursor(false);
                    this.cursorPos = selectionEnd;
                    scrollToCursor(false);
                    return true;
                }
                if (evt.getMouseClickCount() == 3)
                {
                    selectAll();
                    return true;
                }
            }
            else if (evt.getEventType() == EventType.MOUSE_WHEEL)
            {
                return false;
            }

            return evt.isMouseEvent();
        }

        protected void showPopupMenu(Event evt)
        {
            if (popupMenu == null)
            {
                popupMenu = createPopupMenu();
            }
            if (popupMenu != null)
            {
                popupMenu.openPopupMenu(this, evt.getMouseX(), evt.getMouseY());
            }
        }

        protected Menu createPopupMenu()
        {
            Menu menu = new Menu();
            if (!readOnly)
            {
                menu.add("cut", new ActionCallback(this, "cut").run);
            }
            menu.add("copy", new ActionCallback(this, "copy").run);
            if (!readOnly)
            {
                menu.add("paste", new ActionCallback(this, "paste").run);
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
            menu.addSpacer();
            menu.add("select all", new ActionCallback(this, "selectAll").run);
            return menu;
        }

        private void updateText(bool bUpdateAutoCompletion, bool fromModel, int key)
        {
            if (model != null && !fromModel)
            {
                try
                {
                    model.Value = getText();
                    if (errorMsgFromModel)
                    {
                        setErrorMessage(null);
                    }
                }
                catch (Exception ex)
                {
                    if (errorMsg == null || errorMsgFromModel)
                    {
                        setErrorMessage(ex.Message);
                        errorMsgFromModel = true;
                    }
                }
            }
            updateTextDisplay();
            if (multiLine)
            {
                int numLines = textRenderer.getNumTextLines();
                if (numberOfLines != numLines)
                {
                    numberOfLines = numLines;
                    invalidateLayout();
                }
            }
            doCallback(key);
            if (autoCompletionWindow != null && autoCompletionWindow.isOpen() || bUpdateAutoCompletion)
            {
                updateAutoCompletion();
            }
        }

        private void updateTextDisplay()
        {
            textRenderer.setCharSequence(passwordMasking != null ? passwordMasking.Value : editBuffer.Value);
            textRenderer.cacheDirty = true;
            checkTextWidth();
            scrollToCursor(false);
        }

        private void checkTextWidth()
        {
            textLongerThenWidget = textRenderer.getPreferredWidth() > textRenderer.getWidth();
        }

        protected void moveCursor(int dir, bool select)
        {
            setCursorPos(cursorPos + dir, select);
        }

        protected void moveCursorY(int dir, bool select)
        {
            if (multiLine)
            {
                int x = computeRelativeCursorPositionX(cursorPos);
                int lineStart;
                if (dir < 0)
                {
                    lineStart = computeLineStart(cursorPos);
                    if (lineStart == 0)
                    {
                        setCursorPos(0, select);
                        return;
                    }
                    lineStart = computeLineStart(lineStart - 1);
                }
                else
                {
                    lineStart = Math.Min(computeLineEnd(cursorPos) + 1, editBuffer.Length);
                }
                setCursorPos(computeCursorPosFromX(x, lineStart), select);
            }
        }

        protected internal void setCursorPos(int pos, bool select)
        {
            pos = Math.Max(0, Math.Min(editBuffer.Length, pos));
            if (!select)
            {
                bool hadSelection = hasSelection();
                selectionStart = pos;
                selectionEnd = pos;
                if (hadSelection)
                {
                    updateSelection();
                }
            }
            if (this.cursorPos != pos)
            {
                if (select)
                {
                    if (hasSelection())
                    {
                        if (cursorPos == selectionStart)
                        {
                            selectionStart = pos;
                        }
                        else
                        {
                            selectionEnd = pos;
                        }
                    }
                    else
                    {
                        selectionStart = cursorPos;
                        selectionEnd = pos;
                    }
                    if (selectionStart > selectionEnd)
                    {
                        int t = selectionStart;
                        selectionStart = selectionEnd;
                        selectionEnd = t;
                    }
                    updateSelection();
                }

                if (this.cursorPos != pos)
                {
                    getAnimationState().resetAnimationTime(STATE_CURSOR_MOVED);
                }
                this.cursorPos = pos;
                scrollToCursor(false);
                updateAutoCompletion();
            }
        }

        protected void updateSelection()
        {
            if (attributes != null)
            {
                attributes.RemoveAnimationState(TextWidget.STATE_TEXT_SELECTION);
                attributes.SetAnimationState(TextWidget.STATE_TEXT_SELECTION,
                        selectionStart, selectionEnd, true);
                attributes.Optimize();
                textRenderer.cacheDirty = true;
            }
        }

        public void setCursorPos(int pos)
        {
            if (pos < 0 || pos > editBuffer.Length)
            {
                throw new ArgumentOutOfRangeException("pos");
            }
            setCursorPos(pos, false);
        }

        public void selectAll()
        {
            selectionStart = 0;
            selectionEnd = editBuffer.Length;
            updateSelection();
        }

        public void setSelection(int start, int end)
        {
            if (start < 0 || start > end || end > editBuffer.Length)
            {
                throw new ArgumentOutOfRangeException();
            }
            selectionStart = start;
            selectionEnd = end;
            updateSelection();
        }

        protected void selectWordFromMouse(int index)
        {
            selectionStart = index;
            selectionEnd = index;
            while (selectionStart > 0 && !Char.IsWhiteSpace(editBuffer.CharAt(selectionStart - 1)))
            {
                selectionStart--;
            }
            while (selectionEnd < editBuffer.Length && !Char.IsWhiteSpace(editBuffer.CharAt(selectionEnd)))
            {
                selectionEnd++;
            }
            updateSelection();
        }

        protected void scrollToCursor(bool force)
        {
            int renderWidth = textRenderer.getWidth() - 5;
            if (renderWidth <= 0)
            {
                pendingScrollToCursor = true;
                pendingScrollToCursorForce = force;
                return;
            }
            pendingScrollToCursor = false;
            int xpos = computeRelativeCursorPositionX(cursorPos);
            if (xpos < scrollPos + 5)
            {
                scrollPos = Math.Max(0, xpos - 5);
            }
            else if (force || xpos - scrollPos > renderWidth)
            {
                scrollPos = Math.Max(0, xpos - renderWidth);
            }
            if (multiLine)
            {
                ScrollPane sp = ScrollPane.getContainingScrollPane(this);
                if (sp != null)
                {
                    int lineHeight = getLineHeight();
                    int lineY = computeLineNumber(cursorPos) * lineHeight;
                    sp.validateLayout();
                    sp.scrollToAreaY(lineY, lineHeight, lineHeight / 2);
                }
            }
        }

        protected void insertChar(char ch)
        {
            // don't add control characters
            if (!readOnly && (!Char.IsControl(ch) || (multiLine && ch == '\n')))
            {
                bool update = false;
                if (hasSelection())
                {
                    deleteSelection();
                    update = true;
                }
                if (editBuffer.Length < maxTextLength)
                {
                    if (editBuffer.Replace(cursorPos, 0, ch))
                    {
                        cursorPos++;
                        update = true;
                    }
                }
                if (update)
                {
                    updateText(true, false, Event.KEY_NONE);
                }
            }
        }

        protected void deletePrev()
        {
            if (!readOnly)
            {
                if (hasSelection())
                {
                    deleteSelection();
                    updateText(true, false, Event.KEY_DELETE);
                }
                else if (cursorPos > 0)
                {
                    --cursorPos;
                    deleteNext();
                }
            }
        }

        protected void deleteNext()
        {
            if (!readOnly)
            {
                if (hasSelection())
                {
                    deleteSelection();
                    updateText(true, false, Event.KEY_DELETE);
                }
                else if (cursorPos < editBuffer.Length)
                {
                    if (editBuffer.Replace(cursorPos, 1, "") >= 0)
                    {
                        updateText(true, false, Event.KEY_DELETE);
                    }
                }
            }
        }

        protected void deleteSelection()
        {
            if (editBuffer.Replace(selectionStart, selectionEnd - selectionStart, "") >= 0)
            {
                setCursorPos(selectionStart, false);
            }
        }

        protected void modelChanged()
        {
            String modelText = model.Value;
            if (editBuffer.Length != modelText.Length || !getText().Equals(modelText))
            {
                setText(modelText, true);
            }
        }

        protected bool hasFocusOrPopup()
        {
            return hasKeyboardFocus() || hasOpenPopups();
        }

        protected Font getFont()
        {
            return textRenderer.getFont();
        }

        protected int getLineHeight()
        {
            Font font = getFont();
            if (font != null)
            {
                return font.LineHeight;
            }
            return 0;
        }

        protected int computeLineNumber(int cursorPos)
        {
            EditFieldModel eb = this.editBuffer;
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

        protected int computeLineStart(int cursorPos)
        {
            if (!multiLine)
            {
                return 0;
            }
            EditFieldModel eb = this.editBuffer;
            while (cursorPos > 0 && eb.CharAt(cursorPos - 1) != '\n')
            {
                cursorPos--;
            }
            return cursorPos;
        }

        protected int computeLineEnd(int cursorPos)
        {
            EditFieldModel eb = this.editBuffer;
            int endIndex = eb.Length;
            if (!multiLine)
            {
                return endIndex;
            }
            while (cursorPos < endIndex && eb.CharAt(cursorPos) != '\n')
            {
                cursorPos++;
            }
            return cursorPos;
        }

        protected int computeRelativeCursorPositionX(int cursorPos)
        {
            int lineStart = 0;
            if (multiLine)
            {
                lineStart = computeLineStart(cursorPos);
            }
            return textRenderer.computeRelativeCursorPositionX(lineStart, cursorPos);
        }

        protected int computeRelativeCursorPositionY(int cursorPos)
        {
            if (multiLine)
            {
                return getLineHeight() * computeLineNumber(cursorPos);
            }
            return 0;
        }

        protected int getCursorPosFromMouse(int x, int y)
        {
            Font font = getFont();
            if (font != null)
            {
                x -= textRenderer.lastTextX;
                int lineStart = 0;
                int lineEnd = editBuffer.Length;
                if (multiLine)
                {
                    y -= textRenderer.computeTextY();
                    int lineHeight = font.LineHeight;
                    int endIndex = lineEnd;
                    for (; ; )
                    {
                        lineEnd = computeLineEnd(lineStart);

                        if (lineStart >= endIndex || y < lineHeight)
                        {
                            break;
                        }

                        lineStart = Math.Min(lineEnd + 1, endIndex);
                        y -= lineHeight;
                    }
                }
                return computeCursorPosFromX(x, lineStart, lineEnd);
            }
            else
            {
                return 0;
            }
        }

        protected int computeCursorPosFromX(int x, int lineStart)
        {
            return computeCursorPosFromX(x, lineStart, computeLineEnd(lineStart));
        }

        protected int computeCursorPosFromX(int x, int lineStart, int lineEnd)
        {
            Font font = getFont();
            if (font != null)
            {
                return lineStart + font.ComputeVisibleGlyphs(
                        (passwordMasking != null) ? passwordMasking.Value : editBuffer.Value,
                        lineStart, lineEnd, x + font.SpaceWidth / 2);
            }
            return lineStart;
        }

        //@Override
        protected override void paintOverlay(GUI gui)
        {
            if (cursorImage != null && hasFocusOrPopup())
            {
                int xpos = textRenderer.lastTextX + computeRelativeCursorPositionX(cursorPos);
                int ypos = textRenderer.computeTextY() + computeRelativeCursorPositionY(cursorPos);
                cursorImage.Draw(getAnimationState(), xpos, ypos, cursorImage.Width, getLineHeight());
            }
            base.paintOverlay(gui);
        }

        private void openErrorInfoWindow()
        {
            if (autoCompletionWindow == null || !autoCompletionWindow.isOpen())
            {
                if (errorInfoWindow == null)
                {
                    errorInfoLabel = new Label();
                    errorInfoLabel.setClip(true);
                    errorInfoWindow = new InfoWindow(this);
                    errorInfoWindow.setTheme("editfield-errorinfowindow");
                    errorInfoWindow.add(errorInfoLabel);
                }
                errorInfoLabel.setText(errorMsg.ToString());
                errorInfoWindow.openInfo();
                layoutErrorInfoWindow();
            }
        }

        private void layoutErrorInfoWindow()
        {
            int x = getX();
            int width = getWidth();

            Widget container = errorInfoWindow.getParent();
            if (container != null)
            {
                width = Math.Max(width, computeSize(
                        errorInfoWindow.getMinWidth(),
                        errorInfoWindow.getPreferredWidth(),
                        errorInfoWindow.getMaxWidth()));
                int popupMaxRight = container.getInnerRight();
                if (x + width > popupMaxRight)
                {
                    x = popupMaxRight - Math.Min(width, container.getInnerWidth());
                }
                errorInfoWindow.setSize(width, errorInfoWindow.getPreferredHeight());
                errorInfoWindow.setPosition(x, getBottom());
            }
        }

        //@Override
        protected override void keyboardFocusGained()
        {
            if (errorMsg != null)
            {
                openErrorInfoWindow();
            }
            else
            {
                updateAutoCompletion();
            }
        }

        //@Override
        protected override void keyboardFocusLost()
        {
            base.keyboardFocusLost();
            if (errorInfoWindow != null)
            {
                errorInfoWindow.closeInfo();
            }
            if (autoCompletionWindow != null)
            {
                autoCompletionWindow.closeInfo();
            }
        }

        protected void updateAutoCompletion()
        {
            if (autoCompletionWindow != null)
            {
                autoCompletionWindow.updateAutoCompletion();
            }
        }

        internal class TextRenderer : TextWidget
        {
            internal int lastTextX;
            internal int lastScrollPos;
            internal AttributedStringFontCache cache;
            internal bool cacheDirty;
            internal EditField editField;

            protected internal TextRenderer(EditField editField, AnimationState animState) : base(animState)
            {
                this.editField = editField;
            }

            //@Override
            protected override void paintWidget(GUI gui)
            {
                if (this.editField.pendingScrollToCursor)
                {
                    this.editField.scrollToCursor(this.editField.pendingScrollToCursorForce);
                }
                lastScrollPos = this.editField.hasFocusOrPopup() ? this.editField.scrollPos : 0;
                lastTextX = computeTextX();
                Font font = getFont();
                if (this.editField.attributes != null && font is Font2) {
                    paintWithAttributes((Font2)font);
                }
                else if (this.editField.hasSelection() && this.editField.hasFocusOrPopup())
                {
                    if (this.editField.multiLine)
                    {
                        paintMultiLineWithSelection();
                    }
                    else
                    {
                        paintWithSelection(0, this.editField.editBuffer.Length, computeTextY());
                    }
                }
                else
                {
                    paintLabelText(getAnimationState());
                }
            }

            protected void paintWithSelection(int lineStart, int lineEnd, int yoff)
            {
                int selStart = this.editField.selectionStart;
                int selEnd = this.editField.selectionEnd;
                if (this.editField.selectionImage != null && selEnd > lineStart && selStart <= lineEnd)
                {
                    int xpos0 = lastTextX + computeRelativeCursorPositionX(lineStart, selStart);
                    int xpos1 = (lineEnd < selEnd) ? getInnerRight() :
                            lastTextX + computeRelativeCursorPositionX(lineStart, Math.Min(lineEnd, selEnd));
                    this.editField.selectionImage.Draw(getAnimationState(), xpos0, yoff,
                            xpos1 - xpos0, getFont().LineHeight);
                }

                paintWithSelection(getAnimationState(), selStart, selEnd, lineStart, lineEnd, yoff);
            }

            protected void paintMultiLineWithSelection()
            {
                EditFieldModel eb = this.editField.editBuffer;
                int lineStart = 0;
                int endIndex = eb.Length;
                int yoff = computeTextY();
                int lineHeight = this.editField.getLineHeight();
                while (lineStart < endIndex)
                {
                    int lineEnd = this.editField.computeLineEnd(lineStart);

                    paintWithSelection(lineStart, lineEnd, yoff);

                    yoff += lineHeight;
                    lineStart = lineEnd + 1;
                }
            }

            protected void paintMultiLineSelectionBackground()
            {
                int lineHeight = this.editField.getLineHeight();
                int lineStart = this.editField.computeLineStart(this.editField.selectionStart);
                int lineNumber = this.editField.computeLineNumber(lineStart);
                int endIndex = this.editField.selectionEnd;
                int yoff = computeTextY() + lineHeight * lineNumber;
                int xstart = lastTextX + computeRelativeCursorPositionX(lineStart, this.editField.selectionStart);
                while (lineStart < endIndex)
                {
                    int lineEnd = this.editField.computeLineEnd(lineStart);
                    int xend;

                    if (lineEnd < endIndex)
                    {
                        xend = getInnerRight();
                    }
                    else
                    {
                        xend = lastTextX + computeRelativeCursorPositionX(lineStart, endIndex);
                    }

                    this.editField.selectionImage.Draw(getAnimationState(), xstart, yoff, xend - xstart, lineHeight);

                    yoff += lineHeight;
                    lineStart = lineEnd + 1;
                    xstart = getInnerX();
                }
            }

            protected void paintWithAttributes(Font2 font)
            {
                if (this.editField.selectionEnd > this.editField.selectionStart && this.editField.selectionImage != null)
                {
                    paintMultiLineSelectionBackground();
                }
                if (cache == null || cacheDirty)
                {
                    cacheDirty = false;
                    if (this.editField.multiLine)
                    {
                        cache = font.CacheMultiLineText(cache, this.editField.attributes);
                    }
                    else
                    {
                        cache = font.CacheText(cache, this.editField.attributes);
                    }
                }
                int y = computeTextY();
                if (cache != null)
                {
                    cache.Draw(lastTextX, y);
                }
                else if (this.editField.multiLine)
                {
                    font.DrawMultiLineText(lastTextX, y, this.editField.attributes);
                }
                else
                {
                    font.DrawText(lastTextX, y, this.editField.attributes);
                }
            }

            //@Override
            protected override void sizeChanged()
            {
                if (this.editField.scrollToCursorOnSizeChange)
                {
                    this.editField.scrollToCursor(true);
                }
            }

            //@Override
            protected override int computeTextX()
            {
                int x = getInnerX();
                int pos = getAlignment().getHPosition();
                if (pos > 0)
                {
                    x += Math.Max(0, getInnerWidth() - computeTextWidth()) * pos / 2;
                }
                return x - lastScrollPos;
            }

            //@Override
            public override void destroy()
            {
                base.destroy();
                if (cache != null)
                {
                    cache.Dispose();
                    cache = null;
                }
            }
        }

        public class PasswordMasker : CharSequence
        {
            CharSequence baseSeq;
            internal char maskingChar;

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
                    return baseSeq.Length;
                }
            }

            public PasswordMasker(CharSequence baseSeq, char maskingChar)
            {
                this.baseSeq = baseSeq;
                this.maskingChar = maskingChar;
            }

            public CharSequence subSequence(int start, int end)
            {
                throw new InvalidOperationException("Not supported.");
            }

            public char CharAt(int index)
            {
                return maskingChar;
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
