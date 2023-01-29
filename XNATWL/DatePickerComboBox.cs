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
using System.Globalization;
using XNATWL.Model;

namespace XNATWL
{
    public class DatePickerComboBox : ComboBoxBase
    {
        private ComboboxLabel label;
        private DatePicker datePicker;

        public DatePickerComboBox() : this(CultureInfo.CurrentCulture, DateTimeFormatInfo.CurrentInfo)
        {
        }

        /**
         * Constructs a date picker combo box using the specified locale and date format style
         * @param locale the locale
         * @param style the date style
         * @see DateFormat#getDateInstance(int, java.util.Locale) 
         */

        public DatePickerComboBox(CultureInfo cultureInfo, DateTimeFormatInfo info)
        {
            label = new ComboboxLabel(this, getAnimationState());
            label.setTheme("display");
            label.Clicked += (sender, e) =>
            {
                openPopup();
            };

            datePicker = new DatePicker(cultureInfo, info);
            datePicker.CalendarChanged += (sender, e) =>
            {
                updateLabel();
            };

            popup.add(datePicker);
            popup.setTheme("datepickercomboboxPopup");

            button.getModel().State += (sender, e) =>
            {
                updateHover();
            };

            add(label);
        }

        public void setModel(DateModel model)
        {
            datePicker.setModel(model);
        }

        public DateModel getModel()
        {
            return datePicker.getModel();
        }

        public void setDateFormat(CultureInfo cultureInfo, DateTimeFormatInfo info)
        {
            datePicker.setDateFormat(cultureInfo, info);
        }

        public DateTimeFormatInfo getDateFormat()
        {
            return datePicker.getDateFormatInfo();
        }

        public CultureInfo getLocale()
        {
            return datePicker.getLocale();
        }

        protected override Widget getLabel()
        {
            return label;
        }

        protected DatePicker getDatePicker()
        {
            return datePicker;
        }

        protected override void setPopupSize()
        {
            int minWidth = popup.getMinWidth();
            int minHeight = popup.getMinHeight();
            int popupWidth = computeSize(minWidth,
                    popup.getPreferredWidth(),
                    popup.getMaxWidth());
            int popupHeight = computeSize(minHeight,
                    popup.getPreferredHeight(),
                    popup.getMaxHeight());
            Widget container = popup.getParent();
            int popupMaxRight = container.getInnerRight();
            int popupMaxBottom = container.getInnerBottom();
            int x = getX();
            int y = getBottom();
            if (x + popupWidth > popupMaxRight)
            {
                if (getRight() - popupWidth >= container.getInnerX())
                {
                    x = getRight() - popupWidth;
                }
                else
                {
                    x = popupMaxRight - minWidth;
                }
            }
            if (y + popupHeight > popupMaxBottom)
            {
                if (getY() - popupHeight >= container.getInnerY())
                {
                    y = getY() - popupHeight;
                }
                else
                {
                    y = popupMaxBottom - minHeight;
                }
            }
            popupWidth = Math.Min(popupWidth, popupMaxRight - x);
            popupHeight = Math.Min(popupHeight, popupMaxBottom - y);
            popup.setPosition(x, y);
            popup.setSize(popupWidth, popupHeight);
        }

        protected void updateLabel()
        {
            label.setText(datePicker.formatDate());
        }

        void updateHover()
        {
            getAnimationState().setAnimationState(Label.STATE_HOVER,
                    label.hover || button.getModel().Hover);
        }

        protected class ComboboxLabel : Label
        {
            protected internal bool hover;

            private DatePickerComboBox datePickerComboBox;

            public ComboboxLabel(DatePickerComboBox datePickerComboBox, AnimationState animState) : base(animState)
            {
                this.datePickerComboBox = datePickerComboBox;
                setAutoSize(false);
                setClip(true);
                setTheme("display");
            }

            public override int getPreferredInnerHeight()
            {
                int prefHeight = base.getPreferredInnerHeight();
                if (getFont() != null)
                {
                    prefHeight = Math.Max(prefHeight, getFont().LineHeight);
                }
                return prefHeight;
            }

            protected override void handleMouseHover(Event evt)
            {
                if (evt.isMouseEvent())
                {
                    bool newHover = evt.getEventType() != EventType.MOUSE_EXITED;
                    if (newHover != hover)
                    {
                        hover = newHover;
                        this.datePickerComboBox.updateHover();
                    }
                }
            }
        }
    }

}
