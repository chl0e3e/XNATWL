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
using XNATWL.Utils;

namespace XNATWL
{
    public class ValueAdjusterFloat : ValueAdjuster
    {
        private float _value;
        private float _minValue;
        private float _maxValue = 100f;
        private float _dragStartValue;
        private float _stepSize = 1f;
        private FloatModel _model;
        private String _format = "%.2f";
        //private Locale locale = Locale.ENGLISH;

        public ValueAdjusterFloat()
        {
            SetTheme("valueadjuster");
            SetDisplayText();
        }

        public ValueAdjusterFloat(FloatModel model)
        {
            SetTheme("valueadjuster");
            SetModel(model);
        }

        public float GetMaxValue()
        {
            return _maxValue;
        }

        public float GetMinValue()
        {
            return _minValue;
        }

        public void SetMinMaxValue(float minValue, float maxValue)
        {
            if (maxValue < minValue)
            {
                throw new ArgumentOutOfRangeException("maxValue < minValue");
            }
            this._minValue = minValue;
            this._maxValue = maxValue;
            SetValue(_value);
        }

        public float GetValue()
        {
            return _value;
        }

        public void SetValue(float value)
        {
            if (value > _maxValue)
            {
                value = _maxValue;
            }
            else if (value < _minValue)
            {
                value = _minValue;
            }
            if (this._value != value)
            {
                this._value = value;
                if (_model != null)
                {
                    _model.Value = value;
                }
                SetDisplayText();
            }
        }

        public float GetStepSize()
        {
            return _stepSize;
        }

        /**
         * Sets the step size for the value adjuster.
         * It must be &gt; 0.
         *
         * Default is 1.0f.
         *
         * @param stepSize the new step size
         * @throws IllegalArgumentException if stepSize is NaN or &lt;= 0.
         */
        public void SetStepSize(float stepSize)
        {
            // NaN always compares as false
            if (!(stepSize > 0))
            {
                throw new ArgumentOutOfRangeException("stepSize");
            }
            this._stepSize = stepSize;
        }

        public FloatModel GetModel()
        {
            return _model;
        }

        public void SetModel(FloatModel model)
        {
            if (this._model != model)
            {
                RemoveModelCallback();
                this._model = model;
                if (model != null)
                {
                    this._minValue = model.MinValue;
                    this._maxValue = model.MaxValue;
                    AddModelCallback();
                }
            }
        }

        public String GetFormat()
        {
            return _format;
        }

        public void SetFormat(String format)
        {
            // test format
            //String.Format(locale, format, 42f);
            this._format = format;
        }

        /*public Locale getLocale()
        {
            return locale;
        }

        public void setLocale(Locale locale)
        {
            if (locale == null)
            {
                throw new NullReferenceException("locale");
            }
            this.locale = locale;
        }*/

        protected override String OnEditStart()
        {
            return FormatText();
        }

        protected override bool OnEditEnd(String text)
        {
            try
            {
                SetValue(ParseText(text));
                return true;
            }
            catch (ParseException ex)
            {
                return false;
            }
        }

        protected override String ValidateEdit(String text)
        {
            try
            {
                ParseText(text);
                return null;
            }
            catch (ParseException ex)
            {
                return ex.ToString();
            }
        }

        protected override void OnEditCanceled()
        {
        }

        protected override bool ShouldStartEdit(char ch)
        {
            return (ch >= '0' && ch <= '9') || (ch == '-') || (ch == '.');
        }

        protected override void OnDragStart()
        {
            _dragStartValue = _value;
        }

        protected override void OnDragUpdate(int dragDelta)
        {
            float range = Math.Max(1e-4f, Math.Abs(GetMaxValue() - GetMinValue()));
            SetValue(_dragStartValue + dragDelta / Math.Max(3, GetWidth() / range));
        }

        protected override void OnDragCancelled()
        {
            SetValue(_dragStartValue);
        }

        protected override void DoDecrement()
        {
            SetValue(_value - GetStepSize());
        }

        protected override void DoIncrement()
        {
            SetValue(_value + GetStepSize());
        }

        protected override String FormatText()
        {
            return _value.ToString(); // String.format(locale, format, value);
        }

        protected float ParseText(String value)
        {
            return float.Parse(value);
        }

        protected override void SyncWithModel()
        {
            CancelEdit();
            this._minValue = _model.MinValue;
            this._maxValue = _model.MaxValue;
            this._value = _model.Value;
            SetDisplayText();
        }

        protected override void AfterAddToGUI(GUI gui)
        {
            base.AfterAddToGUI(gui);
            AddModelCallback();
        }

        protected override void BeforeRemoveFromGUI(GUI gui)
        {
            RemoveModelCallback();
            base.BeforeRemoveFromGUI(gui);
        }

        protected void RemoveModelCallback()
        {
            if (_model != null)
            {
                _model.Changed -= Model_Changed;
            }
        }

        protected void AddModelCallback()
        {
            if (_model != null && GetGUI() != null)
            {
                _model.Changed += Model_Changed;
                SyncWithModel();
            }
        }

        private void Model_Changed(object sender, FloatChangedEventArgs e)
        {
            this.SyncWithModel();
        }
    }
}
