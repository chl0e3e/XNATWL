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

namespace XNATWL
{
    public class ValueAdjusterInt : ValueAdjuster
    {
        private int _value;
        private int _minValue;
        private int _maxValue = 100;
        private int _dragStartValue;
        private IntegerModel _model;

        public ValueAdjusterInt()
        {
            SetTheme("valueadjuster");
            SetDisplayText();
        }

        public ValueAdjusterInt(IntegerModel model)
        {
            SetTheme("valueadjuster");
            SetModel(model);
        }

        public int GetMaxValue()
        {
            if (_model != null)
            {
                _maxValue = _model.MaxValue;
            }
            return _maxValue;
        }

        public int GetMinValue()
        {
            if (_model != null)
            {
                _minValue = _model.MinValue;
            }
            return _minValue;
        }

        public void SetMinMaxValue(int minValue, int maxValue)
        {
            if (maxValue < minValue)
            {
                throw new ArgumentOutOfRangeException("maxValue < minValue");
            }
            this._minValue = minValue;
            this._maxValue = maxValue;
            SetValue(_value);
        }

        public int GetValue()
        {
            return _value;
        }

        public void SetValue(int value)
        {
            value = Math.Max(GetMinValue(), Math.Min(GetMaxValue(), value));
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

        public IntegerModel GetModel()
        {
            return _model;
        }

        public void SetModel(IntegerModel model)
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

        protected override String OnEditStart()
        {
            return FormatText();
        }

        protected override bool OnEditEnd(String text)
        {
            try
            {
                SetValue(int.Parse(text));
                return true;
            }
            catch (FormatException ex)
            {
                return false;
            }
        }

        protected override String ValidateEdit(String text)
        {
            try
            {
                int.Parse(text);
                return null;
            }
            catch (FormatException ex)
            {
                return ex.ToString();
            }
        }

        protected override void OnEditCanceled()
        {
        }

        protected override bool ShouldStartEdit(char ch)
        {
            return (ch >= '0' && ch <= '9') || (ch == '-');
        }

        protected override void OnDragStart()
        {
            _dragStartValue = _value;
        }

        protected override void OnDragUpdate(int dragDelta)
        {
            int range = Math.Max(1, Math.Abs(GetMaxValue() - GetMinValue()));
            SetValue(_dragStartValue + dragDelta / Math.Max(3, GetWidth() / range));
        }

        protected override void OnDragCancelled()
        {
            SetValue(_dragStartValue);
        }

        protected override void DoDecrement()
        {
            SetValue(_value - 1);
        }

        protected override void DoIncrement()
        {
            SetValue(_value + 1);
        }

        protected override String FormatText()
        {
            return _value.ToString();
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

        private void Model_Changed(object sender, IntegerChangedEventArgs e)
        {
            SyncWithModel();
        }
    }
}
