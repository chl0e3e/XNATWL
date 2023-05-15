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
        private ComboboxLabel _label;
        private DatePicker _datePicker;

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
            _label = new ComboboxLabel(this, GetAnimationState());
            _label.SetTheme("display");
            _label.Clicked += (sender, e) =>
            {
                OpenPopup();
            };

            _datePicker = new DatePicker(cultureInfo, info);
            _datePicker.CalendarChanged += (sender, e) =>
            {
                UpdateLabel();
            };

            _popup.Add(_datePicker);
            _popup.SetTheme("datepickercomboboxPopup");

            _button.GetModel().State += (sender, e) =>
            {
                UpdateHover();
            };

            Add(_label);
        }

        public void SetModel(DateModel model)
        {
            _datePicker.SetModel(model);
        }

        public DateModel GetModel()
        {
            return _datePicker.GetModel();
        }

        public void SetDateFormat(CultureInfo cultureInfo, DateTimeFormatInfo info)
        {
            _datePicker.SetDateFormat(cultureInfo, info);
        }

        public DateTimeFormatInfo GetDateFormat()
        {
            return _datePicker.GetDateFormatInfo();
        }

        public CultureInfo GetLocale()
        {
            return _datePicker.GetLocale();
        }

        protected override Widget GetLabel()
        {
            return _label;
        }

        protected DatePicker GetDatePicker()
        {
            return _datePicker;
        }

        protected override void SetPopupSize()
        {
            int minWidth = _popup.GetMinWidth();
            int minHeight = _popup.GetMinHeight();
            int popupWidth = ComputeSize(minWidth,
                    _popup.GetPreferredWidth(),
                    _popup.GetMaxWidth());
            int popupHeight = ComputeSize(minHeight,
                    _popup.GetPreferredHeight(),
                    _popup.GetMaxHeight());
            Widget container = _popup.GetParent();
            int popupMaxRight = container.GetInnerRight();
            int popupMaxBottom = container.GetInnerBottom();
            int x = GetX();
            int y = GetBottom();
            if (x + popupWidth > popupMaxRight)
            {
                if (GetRight() - popupWidth >= container.GetInnerX())
                {
                    x = GetRight() - popupWidth;
                }
                else
                {
                    x = popupMaxRight - minWidth;
                }
            }
            if (y + popupHeight > popupMaxBottom)
            {
                if (GetY() - popupHeight >= container.GetInnerY())
                {
                    y = GetY() - popupHeight;
                }
                else
                {
                    y = popupMaxBottom - minHeight;
                }
            }
            popupWidth = Math.Min(popupWidth, popupMaxRight - x);
            popupHeight = Math.Min(popupHeight, popupMaxBottom - y);
            _popup.SetPosition(x, y);
            _popup.SetSize(popupWidth, popupHeight);
        }

        protected void UpdateLabel()
        {
            _label.SetText(_datePicker.FormatDate());
        }

        void UpdateHover()
        {
            GetAnimationState().SetAnimationState(Label.STATE_HOVER,
                    _label._hover || _button.GetModel().Hover);
        }

        protected class ComboboxLabel : Label
        {
            protected internal bool _hover;

            private DatePickerComboBox _datePickerComboBox;

            public ComboboxLabel(DatePickerComboBox datePickerComboBox, AnimationState animState) : base(animState)
            {
                this._datePickerComboBox = datePickerComboBox;
                SetAutoSize(false);
                SetClip(true);
                SetTheme("display");
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

            protected override void HandleMouseHover(Event evt)
            {
                if (evt.IsMouseEvent())
                {
                    bool newHover = evt.GetEventType() != EventType.MOUSE_EXITED;
                    if (newHover != _hover)
                    {
                        _hover = newHover;
                        this._datePickerComboBox.UpdateHover();
                    }
                }
            }
        }
    }
}
