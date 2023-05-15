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
using System.Collections.Generic;
using System.Globalization;
using XNATWL.Model;
using XNATWL.Renderer;
using XNATWL.Utils;

namespace XNATWL
{

    public class DatePicker : DialogLayout
    {

        public interface ParseHook
        {
            /**
             * Try to parse the text as date
             * 
             * @param text the text to parse
             * @param calendar the calendar object - don't modify unless update is true
             * @param update true if the calendar should be upadted on success
             * @return true if the parsing was sucessful, false if the default parsing should be executed
             * @throws ParseException if the text could not be parsed and the default parsing should be skipped
             */
            DateTime parse(String text, DateTime time, bool update);
        }

        public event EventHandler<DatePickerCalendarChangedEventArgs> CalendarChanged;

        public static StateKey STATE_PREV_MONTH = StateKey.Get("prevMonth");
        public static StateKey STATE_NEXT_MONTH = StateKey.Get("nextMonth");

        private List<ToggleButton> dayButtons;
        private MonthAdjuster monthAdjuster;
        private Runnable modelChangedCB;

        String[] monthNamesLong;
        String[] monthNamesShort;
        protected internal DateTime calendar;
        private DateTimeFormatInfo dateFormat;
        private CultureInfo locale;
        private ParseHook parseHook;

        private DateModel model;
        private bool cbAdded;

        public DatePicker() : this(CultureInfo.CurrentCulture, DateTimeFormatInfo.CurrentInfo)
        {
            
        }

        /**
         * Constructs a date picker panel using the specified locale and date format style
         * @param locale the locale
         * @param style the date style
         * @see DateFormat#getDateInstance(int, java.util.Locale) 
         */

        public DatePicker(CultureInfo cultureInfo, DateTimeFormatInfo info)
        {
            this.locale = cultureInfo;
            this.dayButtons = new List<ToggleButton>();
            this.monthAdjuster = new MonthAdjuster(this);
            this.calendar = DateTime.Now;

            setDateFormat(cultureInfo, info);
        }

        public DateModel getModel()
        {
            return model;
        }

        public CultureInfo getLocale()
        {
            return this.locale;
        }

        public void setModel(DateModel model)
        {
            if (this.model != model)
            {
                if (cbAdded && this.model != null)
                {
                    this.model.Changed -= Model_Changed;
                }
                this.model = model; ;
                this.model.Changed += Model_Changed;
                modelChanged();
            }
        }

        private void Model_Changed(object sender, DateChangedEventArgs e)
        {
            modelChanged();
        }

        public DateTimeFormatInfo getDateFormatInfo()
        {
            return dateFormat;
        }

        public void setDateFormat(CultureInfo info, DateTimeFormatInfo dateFormat)
        {
            if (dateFormat == null)
            {
                throw new ArgumentNullException("dateFormat");
            }
            if (locale == null)
            {
                throw new ArgumentNullException("locale");
            }
            if (this.dateFormat != dateFormat)
            {
                long time = DateTime.Now.Ticks;
                this.locale = info;
                this.dateFormat = dateFormat;
                this.monthNamesLong = dateFormat.MonthNames;
                this.monthNamesShort = dateFormat.AbbreviatedMonthNames;
                this.calendar = DateTime.Now;
                //calendar.setTimeInMillis(time);
                create();
                modelChanged();
            }
        }

        public ParseHook getParseHook()
        {
            return parseHook;
        }

        public void setParseHook(ParseHook parseHook)
        {
            this.parseHook = parseHook;
        }

        public String formatDate()
        {
            return calendar.ToString(this.dateFormat.LongDatePattern);
        }

        public void parseDate(String date)
        {
            parseDateImpl(date, true);
        }

        protected void parseDateImpl(String text, bool update)
        {
            if (parseHook != null)
            {
                DateTime parsedDt = parseHook.parse(text, calendar, update);
                if (parsedDt != DateTime.MinValue)
                {
                    if (update)
                    {
                        this.calendar = parsedDt;
                    }
                    return;
                }
            }

            DateTime parsed = DateTime.Parse(text);
            if (update)
            {
                this.calendar = parsed;
                calendarChanged();
            }

            String lowerText = text.Trim().ToLower();
            String[][] monthNamesStyles = new String[][] { monthNamesLong, monthNamesShort };

            int month = -1;
            int year = -1;
            bool hasYear = false;

            foreach (String[] monthNames in monthNamesStyles)
            {
                bool breakAgain = false;
                for (int i = 0; i < monthNames.Length; i++)
                {
                    String name = monthNames[i].ToLower();
                    if (name.Length > 0 && lowerText.StartsWith(name))
                    {
                        month = i;
                        lowerText = TextUtil.Trim(lowerText, name.Length);
                        breakAgain = true;
                        break;
                    }
                }
                if (breakAgain)
                {
                    break;
                }
            }

            try
            {
                year = int.Parse(lowerText);
                if (year < 100)
                {
                    year = fixupSmallYear(year);
                }
                hasYear = true;
            }
            catch (ArgumentException ignore)
            {
            }

            if (month < 0 && !hasYear)
            {
                throw new ParseException("Unparseable date: \"" + text + "\"", -1);
            }

            if (update)
            {
                if (month >= 0)
                {
                    if (calendar.Month > month)
                    {
                        calendar = calendar.AddMonths(calendar.Month - month);
                    }
                    else if (calendar.Month < month)
                    {
                        calendar = calendar.AddMonths(month - calendar.Month);
                    }
                }
                if (hasYear)
                {
                    if (calendar.Year > month)
                    {
                        calendar = calendar.AddYears(calendar.Year - year);
                    }
                    else if (calendar.Year < month)
                    {
                        calendar.AddYears(year - calendar.Year);
                    }
                }
                calendarChanged();
            }
        }

        private int fixupSmallYear(int year)
        {
            int futureYear = DateTime.Now.AddYears(20).Year;
            int tripPoint = futureYear % 100;
            if (year > tripPoint)
            {
                year -= 100;
            }
            year += futureYear - tripPoint;
            return year;
        }

        protected override void afterAddToGUI(GUI gui)
        {
            base.afterAddToGUI(gui);
            if (!cbAdded && this.model != null)
            {
                this.model.Changed += Model_Changed;
            }
            cbAdded = true;
        }

        protected override void beforeRemoveFromGUI(GUI gui)
        {
            if (cbAdded && this.model != null)
            {
                this.model.Changed -= Model_Changed;
            }
            cbAdded = false;
            base.beforeRemoveFromGUI(gui);
        }

        private const DayOfWeek StartingDayOfWeek = DayOfWeek.Monday;
        private const DayOfWeek EndingDayOfWeek = DayOfWeek.Sunday;

        /// <summary>
        /// https://stackoverflow.com/a/67758842
        /// </summary>
        /// <param name="month"></param>
        /// <param name="year"></param>
        private static int[] DisplayWeekDatesFromGivenMonthYear(int month, int year)
        {
            DateTime tempDT = new DateTime(year, month, 1);
            int curWeek = 1;
            int minDays = 0;
            int maxDays = 0;
            while (tempDT.Month == month)
            {
                // is this day the 1st or any date that is the starting day of the week
                if (tempDT.Date.Day == 1 || tempDT.DayOfWeek == StartingDayOfWeek)
                {
                    minDays = Math.Min(tempDT.Day, minDays);
                    if (tempDT.DayOfWeek == EndingDayOfWeek ||
                        tempDT.Date.Day == DateTime.DaysInMonth(year, month))
                    {
                        Console.WriteLine("  max: " + tempDT.Day);
                        curWeek++;
                        maxDays = Math.Max(tempDT.Day, maxDays);
                    }
                }
                else
                {
                    // is this day at the end of the week
                    if (tempDT.DayOfWeek == EndingDayOfWeek)
                    {
                        curWeek++;
                        maxDays = Math.Max(tempDT.Day, maxDays);
                    }
                    else
                    {
                        // check for last day in month
                        if (tempDT.Date.Day == DateTime.DaysInMonth(year, month))
                        {
                            maxDays = Math.Max(tempDT.Day, maxDays);
                        }
                    }
                }
                tempDT = tempDT.AddDays(1);
            }

            return new int[2] { minDays, maxDays };
        }

        private void create()
        {
            int[] minMaxDaysOfWeek = DisplayWeekDatesFromGivenMonthYear(this.calendar.Month, this.calendar.Year);
            var firstDayOfMonth = new DateTime(this.calendar.Year, this.calendar.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddSeconds(-1);
            int minDay = firstDayOfMonth.Day;
            int maxDay = lastDayOfMonth.Day;
            int minDayOfWeek = minMaxDaysOfWeek[0];
            int maxDayOfWeek = minMaxDaysOfWeek[1];
            int daysPerWeek = maxDayOfWeek - minDayOfWeek + 1;
            int numWeeks = (maxDay - minDay + daysPerWeek * 2 - 1) / daysPerWeek;

            setHorizontalGroup(null);
            setVerticalGroup(null);
            removeAllChildren();
            dayButtons.Clear(); 

            String[] weekDays = this.dateFormat.AbbreviatedDayNames;

            Group daysHorz = createSequentialGroup();
            Group daysVert = createSequentialGroup();
            Group[] daysOfWeekHorz = new Group[daysPerWeek];
            Group daysRow = createParallelGroup();
            daysVert.addGroup(daysRow);

            for (int i = 0; i < daysPerWeek; i++)
            {
                daysOfWeekHorz[i] = createParallelGroup();
                daysHorz.addGroup(daysOfWeekHorz[i]);

                Label l = new Label(weekDays[i + minDay]);
                daysOfWeekHorz[i].addWidget(l);
                daysRow.addWidget(l);
            }

            for (int week = 0; week < numWeeks; week++)
            {
                daysRow = createParallelGroup();
                daysVert.addGroup(daysRow);

                for (int day = 0; day < daysPerWeek; day++)
                {
                    ToggleButton tb = new ToggleButton();
                    tb.setTheme("daybutton");
                    dayButtons.Add(tb);

                    daysOfWeekHorz[day].addWidget(tb);
                    daysRow.addWidget(tb);
                }
            }

            setHorizontalGroup(createParallelGroup()
                    .addWidget(monthAdjuster)
                    .addGroup(daysHorz));
            setVerticalGroup(createSequentialGroup()
                    .addWidget(monthAdjuster)
                    .addGroup(daysVert));
        }

        void modelChanged()
        {
            if (model != null)
            {
                calendar = new DateTime(model.Value);
            }
            updateDisplay();
        }

        void calendarChanged()
        {
            if (model != null)
            {
                model.Value = calendar.Ticks;
            }
            updateDisplay();
        }

        void updateDisplay()
        {
            monthAdjuster.Sync();
            DateTime cal = (DateTime)new DateTime(calendar.Ticks);

            int[] minMaxDaysOfWeek = DisplayWeekDatesFromGivenMonthYear(this.calendar.Month, this.calendar.Year);
            var firstDayOfMonth = new DateTime(this.calendar.Year, this.calendar.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddSeconds(-1);
            int minDay = firstDayOfMonth.Day;
            int maxDay = lastDayOfMonth.Day;
            int minDayOfWeek = minMaxDaysOfWeek[0];
            int maxDayOfWeek = minMaxDaysOfWeek[1];
            int daysPerWeek = maxDayOfWeek - minDayOfWeek + 1;
            int numWeeks = (maxDay - minDay + daysPerWeek * 2 - 1) / daysPerWeek;

            int day = calendar.Day;
            int weekDay = (int) calendar.Date.DayOfWeek;

            if (weekDay > minDayOfWeek)
            {
                int adj = minDayOfWeek - weekDay;
                day += adj;
                cal = cal.AddDays(adj);
            }

            while (day > minDay)
            {
                day -= daysPerWeek;
                cal = cal.AddDays(-daysPerWeek);
            }

            foreach (ToggleButton tb in dayButtons)
            {
                DayModel dayModel = new DayModel(day);
                tb.setText(cal.Day.ToString());
                tb.setModel(dayModel);
                AnimationState animState = tb.getAnimationState();
                animState.setAnimationState(STATE_PREV_MONTH, day < minDay);
                animState.setAnimationState(STATE_NEXT_MONTH, day > maxDay);
                dayModel.update();
                cal = cal.AddDays(1);
                ++day;
            }

            if (this.CalendarChanged != null)
            {
                this.CalendarChanged.Invoke(this, new DatePickerCalendarChangedEventArgs(calendar));
            }
        }

        void doCallback()
        {
            if (this.CalendarChanged != null)
            {
                this.CalendarChanged.Invoke(this, new DatePickerCalendarChangedEventArgs(calendar));
            }
        }

        class DayModel : BooleanModel
        {
            int day;
            bool active;
            DatePicker datePicker;

            public DayModel(DatePicker datePicker)
            {
                this.datePicker = datePicker;
            }

            public bool Value {
                get
                {
                    return active;
                }
                set
                {
                    if (value && !active)
                    {
                        this.datePicker.calendar = new DateTime(this.day, this.datePicker.calendar.Month, this.datePicker.calendar.Year);
                        this.datePicker.calendarChanged();
                    }
                }
            }

            public event EventHandler<BooleanChangedEventArgs> Changed;

            protected internal DayModel(int day)
            {
                this.day = day;
            }

            public bool getValue()
            {
                return active;
            }

            protected internal void update()
            {
                bool newActive = this.datePicker.calendar.Day == day;
                if (this.active != newActive)
                {
                    var oldActive = newActive;
                    this.active = newActive;
                    this.Changed.Invoke(this, new BooleanChangedEventArgs(oldActive, newActive));
                }
            }

            public void setValue(bool value)
            {
                if (value && !active)
                {
                    this.datePicker.calendar = new DateTime(this.day, this.datePicker.calendar.Month, this.datePicker.calendar.Year);
                    this.datePicker.calendarChanged();
                }
            }
        }

        class MonthAdjuster : ValueAdjuster
        {
            private long dragStartDate;
            DatePicker datePicker;

            public MonthAdjuster(DatePicker datePicker)
            {
                this.datePicker = datePicker;
            }

            protected override void doDecrement()
            {
                this.datePicker.calendar = this.datePicker.calendar.AddMonths(-1);
                this.datePicker.calendarChanged();
            }

            protected override void doIncrement()
            {
                this.datePicker.calendar = this.datePicker.calendar.AddMonths(1);
                this.datePicker.calendarChanged();
            }

            protected override String formatText()
            {
                return this.datePicker.monthNamesLong[this.datePicker.calendar.Month] + " " + this.datePicker.calendar.Year;
            }

            protected override void onDragCancelled()
            {
                this.datePicker.calendar = new DateTime(dragStartDate);
                this.datePicker.calendarChanged();
            }

            protected override void onDragStart()
            {
                dragStartDate = this.datePicker.calendar.Ticks;
            }

            protected override void onDragUpdate(int dragDelta)
            {
                dragDelta /= 5;
                this.datePicker.calendar = new DateTime(dragStartDate);
                this.datePicker.calendar = this.datePicker.calendar.AddMonths(dragDelta);
                this.datePicker.calendarChanged();
            }

            protected override void onEditCanceled()
            {
            }

            protected override bool onEditEnd(String text)
            {
                try
                {
                    this.datePicker.parseDateImpl(text, true);
                    return true;
                }
                catch (ParseException ex)
                {
                    return false;
                }
            }

            protected override String onEditStart()
            {
                return formatText();
            }

            protected override bool shouldStartEdit(char ch)
            {
                return false;
            }

            protected override void syncWithModel()
            {
                setDisplayText();
            }

            public void Sync()
            {
                this.syncWithModel();
            }

            protected override String validateEdit(String text)
            {
                try
                {
                    this.datePicker.parseDateImpl(text, false);
                    return null;
                }
                catch (ParseException ex)
                {
                    return ex.Message;
                }
            }
        }
    }

    public class DatePickerCalendarChangedEventArgs : EventArgs
    {
        public DateTime Calendar;

        public DatePickerCalendarChangedEventArgs(DateTime calendar)
        {
            this.Calendar = calendar;
        }
    }
}
