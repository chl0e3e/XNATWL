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
            DateTime Parse(String text, DateTime time, bool update);
        }

        public event EventHandler<DatePickerCalendarChangedEventArgs> CalendarChanged;

        public static StateKey STATE_PREV_MONTH = StateKey.Get("prevMonth");
        public static StateKey STATE_NEXT_MONTH = StateKey.Get("nextMonth");

        private List<ToggleButton> _dayButtons;
        private MonthAdjuster _monthAdjuster;

        String[] _monthNamesLong;
        String[] _monthNamesShort;
        protected internal DateTime _calendar;
        private DateTimeFormatInfo _dateFormat;
        private CultureInfo _locale;
        private ParseHook _parseHook;

        private DateModel _model;
        private bool _cbAdded;

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
            this._locale = cultureInfo;
            this._dayButtons = new List<ToggleButton>();
            this._monthAdjuster = new MonthAdjuster(this);
            this._calendar = DateTime.Now;

            SetDateFormat(cultureInfo, info);
        }

        public DateModel GetModel()
        {
            return _model;
        }

        public CultureInfo GetLocale()
        {
            return this._locale;
        }

        public void SetModel(DateModel model)
        {
            if (this._model != model)
            {
                if (_cbAdded && this._model != null)
                {
                    this._model.Changed -= Model_Changed;
                }
                this._model = model; ;
                this._model.Changed += Model_Changed;
                ModelChanged();
            }
        }

        private void Model_Changed(object sender, DateChangedEventArgs e)
        {
            ModelChanged();
        }

        public DateTimeFormatInfo GetDateFormatInfo()
        {
            return _dateFormat;
        }

        public void SetDateFormat(CultureInfo info, DateTimeFormatInfo dateFormat)
        {
            if (dateFormat == null)
            {
                throw new ArgumentNullException("dateFormat");
            }
            if (_locale == null)
            {
                throw new ArgumentNullException("locale");
            }
            if (this._dateFormat != dateFormat)
            {
                long time = DateTime.Now.Ticks;
                this._locale = info;
                this._dateFormat = dateFormat;
                this._monthNamesLong = dateFormat.MonthNames;
                this._monthNamesShort = dateFormat.AbbreviatedMonthNames;
                this._calendar = DateTime.Now;
                //calendar.setTimeInMillis(time);
                Create();
                ModelChanged();
            }
        }

        public ParseHook GetParseHook()
        {
            return _parseHook;
        }

        public void SetParseHook(ParseHook parseHook)
        {
            this._parseHook = parseHook;
        }

        public String FormatDate()
        {
            return _calendar.ToString(this._dateFormat.LongDatePattern);
        }

        public void ParseDate(String date)
        {
            ParseDateImpl(date, true);
        }

        protected void ParseDateImpl(String text, bool update)
        {
            if (_parseHook != null)
            {
                DateTime parsedDt = _parseHook.Parse(text, _calendar, update);
                if (parsedDt != DateTime.MinValue)
                {
                    if (update)
                    {
                        this._calendar = parsedDt;
                    }
                    return;
                }
            }

            DateTime parsed = DateTime.Parse(text);
            if (update)
            {
                this._calendar = parsed;
                FireCalendarChanged();
            }

            String lowerText = text.Trim().ToLower();
            String[][] monthNamesStyles = new String[][] { _monthNamesLong, _monthNamesShort };

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
                    year = FixupSmallYear(year);
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
                    if (_calendar.Month > month)
                    {
                        _calendar = _calendar.AddMonths(_calendar.Month - month);
                    }
                    else if (_calendar.Month < month)
                    {
                        _calendar = _calendar.AddMonths(month - _calendar.Month);
                    }
                }
                if (hasYear)
                {
                    if (_calendar.Year > month)
                    {
                        _calendar = _calendar.AddYears(_calendar.Year - year);
                    }
                    else if (_calendar.Year < month)
                    {
                        _calendar.AddYears(year - _calendar.Year);
                    }
                }
                FireCalendarChanged();
            }
        }

        private int FixupSmallYear(int year)
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

        protected override void AfterAddToGUI(GUI gui)
        {
            base.AfterAddToGUI(gui);
            if (!_cbAdded && this._model != null)
            {
                this._model.Changed += Model_Changed;
            }
            _cbAdded = true;
        }

        protected override void BeforeRemoveFromGUI(GUI gui)
        {
            if (_cbAdded && this._model != null)
            {
                this._model.Changed -= Model_Changed;
            }
            _cbAdded = false;
            base.BeforeRemoveFromGUI(gui);
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

        private void Create()
        {
            int[] minMaxDaysOfWeek = DisplayWeekDatesFromGivenMonthYear(this._calendar.Month, this._calendar.Year);
            var firstDayOfMonth = new DateTime(this._calendar.Year, this._calendar.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddSeconds(-1);
            int minDay = firstDayOfMonth.Day;
            int maxDay = lastDayOfMonth.Day;
            int minDayOfWeek = minMaxDaysOfWeek[0];
            int maxDayOfWeek = minMaxDaysOfWeek[1];
            int daysPerWeek = maxDayOfWeek - minDayOfWeek + 1;
            int numWeeks = (maxDay - minDay + daysPerWeek * 2 - 1) / daysPerWeek;

            SetHorizontalGroup(null);
            SetVerticalGroup(null);
            RemoveAllChildren();
            _dayButtons.Clear(); 

            String[] weekDays = this._dateFormat.AbbreviatedDayNames;

            Group daysHorz = CreateSequentialGroup();
            Group daysVert = CreateSequentialGroup();
            Group[] daysOfWeekHorz = new Group[daysPerWeek];
            Group daysRow = CreateParallelGroup();
            daysVert.AddGroup(daysRow);

            for (int i = 0; i < daysPerWeek; i++)
            {
                daysOfWeekHorz[i] = CreateParallelGroup();
                daysHorz.AddGroup(daysOfWeekHorz[i]);

                Label l = new Label(weekDays[i + minDay]);
                daysOfWeekHorz[i].AddWidget(l);
                daysRow.AddWidget(l);
            }

            for (int week = 0; week < numWeeks; week++)
            {
                daysRow = CreateParallelGroup();
                daysVert.AddGroup(daysRow);

                for (int day = 0; day < daysPerWeek; day++)
                {
                    ToggleButton tb = new ToggleButton();
                    tb.SetTheme("daybutton");
                    _dayButtons.Add(tb);

                    daysOfWeekHorz[day].AddWidget(tb);
                    daysRow.AddWidget(tb);
                }
            }

            SetHorizontalGroup(CreateParallelGroup()
                    .AddWidget(_monthAdjuster)
                    .AddGroup(daysHorz));
            SetVerticalGroup(CreateSequentialGroup()
                    .AddWidget(_monthAdjuster)
                    .AddGroup(daysVert));
        }

        void ModelChanged()
        {
            if (_model != null)
            {
                _calendar = new DateTime(_model.Value);
            }
            UpdateDisplay();
        }

        void FireCalendarChanged()
        {
            if (_model != null)
            {
                _model.Value = _calendar.Ticks;
            }
            UpdateDisplay();
        }

        void UpdateDisplay()
        {
            _monthAdjuster.Sync();
            DateTime cal = (DateTime)new DateTime(_calendar.Ticks);

            int[] minMaxDaysOfWeek = DisplayWeekDatesFromGivenMonthYear(this._calendar.Month, this._calendar.Year);
            var firstDayOfMonth = new DateTime(this._calendar.Year, this._calendar.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddSeconds(-1);
            int minDay = firstDayOfMonth.Day;
            int maxDay = lastDayOfMonth.Day;
            int minDayOfWeek = minMaxDaysOfWeek[0];
            int maxDayOfWeek = minMaxDaysOfWeek[1];
            int daysPerWeek = maxDayOfWeek - minDayOfWeek + 1;
            int numWeeks = (maxDay - minDay + daysPerWeek * 2 - 1) / daysPerWeek;

            int day = _calendar.Day;
            int weekDay = (int) _calendar.Date.DayOfWeek;

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

            foreach (ToggleButton tb in _dayButtons)
            {
                DayModel dayModel = new DayModel(day);
                tb.SetText(cal.Day.ToString());
                tb.SetModel(dayModel);
                AnimationState animState = tb.GetAnimationState();
                animState.SetAnimationState(STATE_PREV_MONTH, day < minDay);
                animState.SetAnimationState(STATE_NEXT_MONTH, day > maxDay);
                dayModel.Update();
                cal = cal.AddDays(1);
                ++day;
            }

            if (this.CalendarChanged != null)
            {
                this.CalendarChanged.Invoke(this, new DatePickerCalendarChangedEventArgs(_calendar));
            }
        }

        class DayModel : BooleanModel
        {
            int _day;
            bool _active;
            DatePicker _datePicker;

            public DayModel(DatePicker datePicker)
            {
                this._datePicker = datePicker;
            }

            public bool Value {
                get
                {
                    return _active;
                }
                set
                {
                    if (value && !_active)
                    {
                        this._datePicker._calendar = new DateTime(this._day, this._datePicker._calendar.Month, this._datePicker._calendar.Year);
                        this._datePicker.FireCalendarChanged();
                    }
                }
            }

            public event EventHandler<BooleanChangedEventArgs> Changed;

            protected internal DayModel(int day)
            {
                this._day = day;
            }

            public bool GetValue()
            {
                return _active;
            }

            protected internal void Update()
            {
                bool newActive = this._datePicker._calendar.Day == _day;
                if (this._active != newActive)
                {
                    var oldActive = newActive;
                    this._active = newActive;
                    this.Changed.Invoke(this, new BooleanChangedEventArgs(oldActive, newActive));
                }
            }

            public void SetValue(bool value)
            {
                if (value && !_active)
                {
                    this._datePicker._calendar = new DateTime(this._day, this._datePicker._calendar.Month, this._datePicker._calendar.Year);
                    this._datePicker.FireCalendarChanged();
                }
            }
        }

        class MonthAdjuster : ValueAdjuster
        {
            private long _dragStartDate;
            DatePicker _datePicker;

            public MonthAdjuster(DatePicker datePicker)
            {
                this._datePicker = datePicker;
            }

            protected override void DoDecrement()
            {
                this._datePicker._calendar = this._datePicker._calendar.AddMonths(-1);
                this._datePicker.FireCalendarChanged();
            }

            protected override void DoIncrement()
            {
                this._datePicker._calendar = this._datePicker._calendar.AddMonths(1);
                this._datePicker.FireCalendarChanged();
            }

            protected override String FormatText()
            {
                return this._datePicker._monthNamesLong[this._datePicker._calendar.Month] + " " + this._datePicker._calendar.Year;
            }

            protected override void OnDragCancelled()
            {
                this._datePicker._calendar = new DateTime(_dragStartDate);
                this._datePicker.FireCalendarChanged();
            }

            protected override void OnDragStart()
            {
                _dragStartDate = this._datePicker._calendar.Ticks;
            }

            protected override void OnDragUpdate(int dragDelta)
            {
                dragDelta /= 5;
                this._datePicker._calendar = new DateTime(_dragStartDate);
                this._datePicker._calendar = this._datePicker._calendar.AddMonths(dragDelta);
                this._datePicker.FireCalendarChanged();
            }

            protected override void OnEditCanceled()
            {
            }

            protected override bool OnEditEnd(String text)
            {
                try
                {
                    this._datePicker.ParseDateImpl(text, true);
                    return true;
                }
                catch (ParseException ex)
                {
                    return false;
                }
            }

            protected override String OnEditStart()
            {
                return FormatText();
            }

            protected override bool ShouldStartEdit(char ch)
            {
                return false;
            }

            protected override void SyncWithModel()
            {
                SetDisplayText();
            }

            public void Sync()
            {
                this.SyncWithModel();
            }

            protected override String ValidateEdit(String text)
            {
                try
                {
                    this._datePicker.ParseDateImpl(text, false);
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
