using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Model;
using XNATWL.Utils;

namespace XNATWL
{
    public class ValueAdjusterFloat : ValueAdjuster
    {
        private float value;
        private float minValue;
        private float maxValue = 100f;
        private float dragStartValue;
        private float stepSize = 1f;
        private FloatModel model;
        private String format = "%.2f";
        //private Locale locale = Locale.ENGLISH;

        public ValueAdjusterFloat()
        {
            setTheme("valueadjuster");
            setDisplayText();
        }

        public ValueAdjusterFloat(FloatModel model)
        {
            setTheme("valueadjuster");
            setModel(model);
        }

        public float getMaxValue()
        {
            return maxValue;
        }

        public float getMinValue()
        {
            return minValue;
        }

        public void setMinMaxValue(float minValue, float maxValue)
        {
            if (maxValue < minValue)
            {
                throw new ArgumentOutOfRangeException("maxValue < minValue");
            }
            this.minValue = minValue;
            this.maxValue = maxValue;
            setValue(value);
        }

        public float getValue()
        {
            return value;
        }

        public void setValue(float value)
        {
            if (value > maxValue)
            {
                value = maxValue;
            }
            else if (value < minValue)
            {
                value = minValue;
            }
            if (this.value != value)
            {
                this.value = value;
                if (model != null)
                {
                    model.Value = value;
                }
                setDisplayText();
            }
        }

        public float getStepSize()
        {
            return stepSize;
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
        public void setStepSize(float stepSize)
        {
            // NaN always compares as false
            if (!(stepSize > 0))
            {
                throw new ArgumentOutOfRangeException("stepSize");
            }
            this.stepSize = stepSize;
        }

        public FloatModel getModel()
        {
            return model;
        }

        public void setModel(FloatModel model)
        {
            if (this.model != model)
            {
                removeModelCallback();
                this.model = model;
                if (model != null)
                {
                    this.minValue = model.MinValue;
                    this.maxValue = model.MaxValue;
                    addModelCallback();
                }
            }
        }

        public String getFormat()
        {
            return format;
        }

        public void setFormat(String format)
        {
            // test format
            //String.Format(locale, format, 42f);
            this.format = format;
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

        protected override String onEditStart()
        {
            return formatText();
        }

        protected override bool onEditEnd(String text)
        {
            try
            {
                setValue(parseText(text));
                return true;
            }
            catch (ParseException ex)
            {
                return false;
            }
        }

        protected override String validateEdit(String text)
        {
            try
            {
                parseText(text);
                return null;
            }
            catch (ParseException ex)
            {
                return ex.ToString();
            }
        }

        protected override void onEditCanceled()
        {
        }

        protected override bool shouldStartEdit(char ch)
        {
            return (ch >= '0' && ch <= '9') || (ch == '-') || (ch == '.');
        }

        protected override void onDragStart()
        {
            dragStartValue = value;
        }

        protected override void onDragUpdate(int dragDelta)
        {
            float range = Math.Max(1e-4f, Math.Abs(getMaxValue() - getMinValue()));
            setValue(dragStartValue + dragDelta / Math.Max(3, getWidth() / range));
        }

        protected override void onDragCancelled()
        {
            setValue(dragStartValue);
        }

        protected override void doDecrement()
        {
            setValue(value - getStepSize());
        }

        protected override void doIncrement()
        {
            setValue(value + getStepSize());
        }

        protected override String formatText()
        {
            return value.ToString(); // String.format(locale, format, value);
        }

        protected float parseText(String value)
        {
            return float.Parse(value);
        }

        protected override void syncWithModel()
        {
            cancelEdit();
            this.minValue = model.MinValue;
            this.maxValue = model.MaxValue;
            this.value = model.Value;
            setDisplayText();
        }

        protected override void afterAddToGUI(GUI gui)
        {
            base.afterAddToGUI(gui);
            addModelCallback();
        }

        protected override void beforeRemoveFromGUI(GUI gui)
        {
            removeModelCallback();
            base.beforeRemoveFromGUI(gui);
        }

        protected void removeModelCallback()
        {
            if (model != null)
            {
                model.Changed -= Model_Changed;
            }
        }

        protected void addModelCallback()
        {
            if (model != null && getGUI() != null)
            {
                model.Changed += Model_Changed;
                syncWithModel();
            }
        }

        private void Model_Changed(object sender, FloatChangedEventArgs e)
        {
            this.syncWithModel();
        }
    }
}
