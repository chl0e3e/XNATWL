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
        private String theme = "simpledialog";
        private String title;
        private Object msg;
        private Runnable cbOk;
        private Runnable cbCancel;
        private bool focusCancelButton;

        public event EventHandler<SimpleDialogOKEventArgs> OK;
        public event EventHandler<SimpleDialogCancelEventArgs> Cancel;

        public SimpleDialog()
        {
        }

        public void setTheme(String theme)
        {
            if (theme == null)
            {
                throw new NullReferenceException();
            }
            this.theme = theme;
        }

        public String getTitle()
        {
            return title;
        }

        /**
         * Sets the title for this dialog, can be null
         *
         * Default is null
         * 
         * @param title the title
         */
        public void setTitle(String title)
        {
            this.title = title;
        }

        public Object getMessage()
        {
            return msg;
        }

        /**
         * Sets a message object which is displayed below the title.
         * Can be a String or a Widget.
         *
         * Default is null
         *
         * @param msg the message object, can be null
         */
        public void setMessage(Object msg)
        {
            this.msg = msg;
        }

        public Runnable getOkCallback()
        {
            return cbOk;
        }

        /**
         * Sets the callback to call when "Ok" was clicked.
         * The dialog is closed before the callback is fired.
         *
         * @param cbOk the callback or null
         */
        public void setOkCallback(Runnable cbOk)
        {
            this.cbOk = cbOk;
        }

        public Runnable getCancelCallback()
        {
            return cbCancel;
        }

        /**
         * Sets the callback to call when "Cancel" was clicked.
         * The dialog is closed before the callback is fired.
         *
         * @param cbCancel the callback or null
         */
        public void setCancelCallback(Runnable cbCancel)
        {
            this.cbCancel = cbCancel;
        }

        public bool isFocusCancelButton()
        {
            return focusCancelButton;
        }

        /**
         * Should the cancel button be focused when the dialog is created?
         * Default is false (eg focus the message or the OK button).
         * 
         * @param focusCancelButton true to focus the cancel button
         */
        public void setFocusCancelButton(bool focusCancelButton)
        {
            this.focusCancelButton = focusCancelButton;
        }

        /**
         * Shows the dialog centered
         *
         * @param owner The owner of the dialog
         * @return the PopupWindow object to close the dialog ealier
         */
        public PopupWindow showDialog(Widget owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            Widget msgWidget = null;

            if (msg is Widget) {
                msgWidget = (Widget)msg;

                // remove message widget from previous owner if it's in a closed dialog
                if (msgWidget.getParent() is DialogLayout) {
                    if (msgWidget.getParent().getParent() is PopupWindow) {
                        PopupWindow prevPopup = (PopupWindow)msgWidget.getParent().getParent();
                        if (!prevPopup.isOpen())
                        {
                            msgWidget.getParent().removeChild(msgWidget);
                        }
                    }
                }

                if (msgWidget.getParent() != null)
                {
                    throw new ArgumentException("message widget alreay in use");
                }
            } else if (msg is String) {
                msgWidget = new Label((String)msg);
            } else if (msg != null)
            {
                Logger.GetLogger(typeof(SimpleDialog)).log(Level.WARNING, "Unsupported message type: " + msg.GetType().FullName);
            }

            PopupWindow popupWindow = new PopupWindow(owner);

            Button btnOk = new Button("Ok");
            btnOk.setTheme("btnOk");
            btnOk.Action += BtnOk_Action;

            ButtonCB btnCancelCallback = new ButtonCB(popupWindow, cbCancel);
            popupWindow.setRequestCloseCallback(btnCancelCallback);

            Button btnCancel = new Button("Cancel");
            btnCancel.setTheme("btnCancel");
            btnCancel.Action += BtnCancel_Action;

            DialogLayout layout = new DialogLayout();
            layout.setTheme("content");
            layout.setHorizontalGroup(layout.createParallelGroup());
            layout.setVerticalGroup(layout.createSequentialGroup());

            String vertPrevWidget = "top";

            if (title != null)
            {
                Label labelTitle = new Label(title);
                labelTitle.setTheme("title");
                labelTitle.setLabelFor(msgWidget);

                layout.getHorizontalGroup().addWidget(labelTitle);
                layout.getVerticalGroup().addWidget(labelTitle);
                vertPrevWidget = "title";
            }

            if (msgWidget != null)
            {
                layout.getHorizontalGroup().addGroup(layout.createSequentialGroup()
                    .addGap("left-msg")
                    .addWidget(msgWidget)
                    .addGap("msg-right"));
                layout.getVerticalGroup().addGap(vertPrevWidget + "-msg").addWidget(msgWidget).addGap("msg-buttons");
            }
            else
            {
                layout.getVerticalGroup().addGap(vertPrevWidget + "-buttons");
            }

            layout.getHorizontalGroup().addGroup(layout.createSequentialGroup()
                    .addGap("left-btnOk")
                    .addWidget(btnOk)
                    .addGap("btnOk-btnCancel")
                    .addWidget(btnCancel)
                    .addGap("btnCancel-right"));
            layout.getVerticalGroup().addGroup(layout.createParallelGroup(btnOk, btnCancel));

            popupWindow.setTheme(theme);
            popupWindow.add(layout);
            popupWindow.openPopupCentered();

            if (focusCancelButton)
            {
                btnCancel.requestKeyboardFocus();
            }
            else if (msgWidget != null && msgWidget.canAcceptKeyboardFocus())
            {
                msgWidget.requestKeyboardFocus();
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
            private PopupWindow popupWindow;
            private Runnable cb;

            public ButtonCB(PopupWindow popupWindow, Runnable cb)
            {
                this.popupWindow = popupWindow;
                this.cb = cb;
            }

            public void run()
            {
                popupWindow.closePopup();
                if (cb != null)
                {
                    cb.run();
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
