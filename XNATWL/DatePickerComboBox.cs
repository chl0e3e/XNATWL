using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    bool newHover = evt.getEventType() != Event.EventType.MOUSE_EXITED;
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
