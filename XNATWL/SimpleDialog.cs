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
using static XNATWL.Utils.Logger;
using XNATWL.Utils;

namespace XNATWL
{
    public class SimpleDialog
    {
        private String _theme = "simpledialog";
        private String _title;
        private Object _msg;
        private Runnable _cbOk;
        private Runnable _cbCancel;
        private bool _focusCancelButton;

        public event EventHandler<SimpleDialogOKEventArgs> OK;
        public event EventHandler<SimpleDialogCancelEventArgs> Cancel;

        public SimpleDialog()
        {
        }

        public void SetTheme(String theme)
        {
            if (theme == null)
            {
                throw new NullReferenceException();
            }
            this._theme = theme;
        }

        public String GetTitle()
        {
            return _title;
        }

        /**
         * Sets the title for this dialog, can be null
         *
         * Default is null
         * 
         * @param title the title
         */
        public void SetTitle(String title)
        {
            this._title = title;
        }

        public Object GetMessage()
        {
            return _msg;
        }

        /**
         * Sets a message object which is displayed below the title.
         * Can be a String or a Widget.
         *
         * Default is null
         *
         * @param msg the message object, can be null
         */
        public void SetMessage(Object msg)
        {
            this._msg = msg;
        }

        public Runnable GetOkCallback()
        {
            return _cbOk;
        }

        /**
         * Sets the callback to call when "Ok" was clicked.
         * The dialog is closed before the callback is fired.
         *
         * @param cbOk the callback or null
         */
        public void SetOkCallback(Runnable cbOk)
        {
            this._cbOk = cbOk;
        }

        public Runnable GetCancelCallback()
        {
            return _cbCancel;
        }

        /**
         * Sets the callback to call when "Cancel" was clicked.
         * The dialog is closed before the callback is fired.
         *
         * @param cbCancel the callback or null
         */
        public void SetCancelCallback(Runnable cbCancel)
        {
            this._cbCancel = cbCancel;
        }

        public bool IsFocusCancelButton()
        {
            return _focusCancelButton;
        }

        /**
         * Should the cancel button be focused when the dialog is created?
         * Default is false (eg focus the message or the OK button).
         * 
         * @param focusCancelButton true to focus the cancel button
         */
        public void SetFocusCancelButton(bool focusCancelButton)
        {
            this._focusCancelButton = focusCancelButton;
        }

        /**
         * Shows the dialog centered
         *
         * @param owner The owner of the dialog
         * @return the PopupWindow object to close the dialog ealier
         */
        public PopupWindow ShowDialog(Widget owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            Widget msgWidget = null;

            if (_msg is Widget) {
                msgWidget = (Widget)_msg;

                // remove message widget from previous owner if it's in a closed dialog
                if (msgWidget.GetParent() is DialogLayout) {
                    if (msgWidget.GetParent().GetParent() is PopupWindow) {
                        PopupWindow prevPopup = (PopupWindow)msgWidget.GetParent().GetParent();
                        if (!prevPopup.IsOpen())
                        {
                            msgWidget.GetParent().RemoveChild(msgWidget);
                        }
                    }
                }

                if (msgWidget.GetParent() != null)
                {
                    throw new ArgumentException("message widget alreay in use");
                }
            } else if (_msg is String) {
                msgWidget = new Label((String)_msg);
            } else if (_msg != null)
            {
                Logger.GetLogger(typeof(SimpleDialog)).Log(Level.WARNING, "Unsupported message type: " + _msg.GetType().FullName);
            }

            PopupWindow popupWindow = new PopupWindow(owner);

            Button btnOk = new Button("Ok");
            btnOk.SetTheme("btnOk");
            btnOk.Action += BtnOk_Action;

            ButtonCB btnCancelCallback = new ButtonCB(popupWindow, _cbCancel);
            popupWindow.SetRequestCloseCallback(btnCancelCallback);

            Button btnCancel = new Button("Cancel");
            btnCancel.SetTheme("btnCancel");
            btnCancel.Action += BtnCancel_Action;

            DialogLayout layout = new DialogLayout();
            layout.SetTheme("content");
            layout.SetHorizontalGroup(layout.CreateParallelGroup());
            layout.SetVerticalGroup(layout.CreateSequentialGroup());

            String vertPrevWidget = "top";

            if (_title != null)
            {
                Label labelTitle = new Label(_title);
                labelTitle.SetTheme("title");
                labelTitle.SetLabelFor(msgWidget);

                layout.GetHorizontalGroup().AddWidget(labelTitle);
                layout.GetVerticalGroup().AddWidget(labelTitle);
                vertPrevWidget = "title";
            }

            if (msgWidget != null)
            {
                layout.GetHorizontalGroup().AddGroup(layout.CreateSequentialGroup()
                    .AddGap("left-msg")
                    .AddWidget(msgWidget)
                    .AddGap("msg-right"));
                layout.GetVerticalGroup().AddGap(vertPrevWidget + "-msg").AddWidget(msgWidget).AddGap("msg-buttons");
            }
            else
            {
                layout.GetVerticalGroup().AddGap(vertPrevWidget + "-buttons");
            }

            layout.GetHorizontalGroup().AddGroup(layout.CreateSequentialGroup()
                    .AddGap("left-btnOk")
                    .AddWidget(btnOk)
                    .AddGap("btnOk-btnCancel")
                    .AddWidget(btnCancel)
                    .AddGap("btnCancel-right"));
            layout.GetVerticalGroup().AddGroup(layout.CreateParallelGroup(btnOk, btnCancel));

            popupWindow.SetTheme(_theme);
            popupWindow.Add(layout);
            popupWindow.OpenPopupCentered();

            if (_focusCancelButton)
            {
                btnCancel.RequestKeyboardFocus();
            }
            else if (msgWidget != null && msgWidget.CanAcceptKeyboardFocus())
            {
                msgWidget.RequestKeyboardFocus();
            }

            return popupWindow;
        }

        private void BtnCancel_Action(object sender, Model.ButtonActionEventArgs e)
        {
            this.Cancel.Invoke(sender, new SimpleDialogCancelEventArgs());
        }

        private void BtnOk_Action(object sender, Model.ButtonActionEventArgs e)
        {
            this.OK.Invoke(sender, new SimpleDialogOKEventArgs());
        }

        public class ButtonCB : Runnable
        {
            private PopupWindow _popupWindow;
            private Runnable _cb;

            public ButtonCB(PopupWindow popupWindow, Runnable cb)
            {
                this._popupWindow = popupWindow;
                this._cb = cb;
            }

            public override void Run()
            {
                _popupWindow.ClosePopup();
                if (_cb != null)
                {
                    _cb.Run();
                }
            }
        }
    }

    public class SimpleDialogCancelEventArgs
    {
    }

    public class SimpleDialogOKEventArgs
    {
    }
}
