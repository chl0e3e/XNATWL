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
using XNATWL.Model;
using XNATWL.Property;

namespace XNATWL
{
    public class MenuCheckbox : MenuElement
    {
        private BooleanModel model;

        public MenuCheckbox()
        {
        }

        public MenuCheckbox(BooleanModel model)
        {
            this.model = model;
        }

        public MenuCheckbox(String name, BooleanModel model) : base(name)
        {
            this.model = model;
        }

        public BooleanModel getModel()
        {
            return model;
        }

        public void setModel(BooleanModel model)
        {
            BooleanModel oldModel = this.model;
            this.model = model;
            firePropertyChange("model", oldModel, model);
        }

        class MenuCheckBoxBtn : MenuBtn
        {
            private MenuCheckbox _menuCheckBox;

            public MenuCheckBoxBtn(MenuCheckbox menuCheckBox, MenuElement menuElement) : base(menuElement)
            {
                this._menuCheckBox = menuCheckBox;
            }

            public void propertyChange(PropertyChangeEvent evt)
            {
                base.propertyChange(evt);
                ((ToggleButtonModel)getModel()).setModel(this._menuCheckBox.getModel());
            }
        }

        //@Override
        protected internal override Widget createMenuWidget(MenuManager mm, int level)
        {
            MenuBtn btn = new MenuCheckBoxBtn(this, this);
            btn.setModel(new ToggleButtonModel(getModel()));
            setWidgetTheme(btn, "checkbox");
            btn.Action += (sender, e) => {
                mm.closePopup();
            };

            return btn;
        }
    }
}
