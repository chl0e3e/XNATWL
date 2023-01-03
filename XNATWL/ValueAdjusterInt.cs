using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Model;
using XNATWL.Utils;

namespace XNATWL
{
    public class ValueAdjusterInt : ValueAdjuster
    {
        private int value;
        private int minValue;
        private int maxValue = 100;
        private int dragStartValue;
        private IntegerModel model;

        public ValueAdjusterInt()
        {
            setTheme("valueadjuster");
            setDisplayText();
        }

        public ValueAdjusterInt(IntegerModel model)
        {
            setTheme("valueadjuster");
            setModel(model);
        }

        public int getMaxValue()
        {
            if (model != null)
            {
                maxValue = model.MaxValue;
            }
            return maxValue;
        }

        public int getMinValue()
        {
            if (model != null)
            {
                minValue = model.MinValue;
            }
            return minValue;
        }

        public void setMinMaxValue(int minValue, int maxValue)
        {
            if (maxValue < minValue)
            {
                throw new ArgumentOutOfRangeException("maxValue < minValue");
            }
            this.minValue = minValue;
            this.maxValue = maxValue;
            setValue(value);
        }

        public int getValue()
        {
            return value;
        }

        public void setValue(int value)
        {
            value = Math.Max(getMinValue(), Math.Min(getMaxValue(), value));
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

        public IntegerModel getModel()
        {
            return model;
        }

        public void setModel(IntegerModel model)
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

        protected override String onEditStart()
        {
            return formatText();
        }

        protected override bool onEditEnd(String text)
        {
            try
            {
                setValue(int.Parse(text));
                return true;
            }
            catch (FormatException ex)
            {
                return false;
            }
        }

        protected override String validateEdit(String text)
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

        protected override void onEditCanceled()
        {
        }

        protected override bool shouldStartEdit(char ch)
        {
            return (ch >= '0' && ch <= '9') || (ch == '-');
        }

        protected override void onDragStart()
        {
            dragStartValue = value;
        }

        protected override void onDragUpdate(int dragDelta)
        {
            int range = Math.Max(1, Math.Abs(getMaxValue() - getMinValue()));
            setValue(dragStartValue + dragDelta / Math.Max(3, getWidth() / range));
        }

        protected override void onDragCancelled()
        {
            setValue(dragStartValue);
        }

        protected override void doDecrement()
        {
            setValue(value - 1);
        }

        protected override void doIncrement()
        {
            setValue(value + 1);
        }

        protected override String formatText()
        {
            return value.ToString();
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

        private void Model_Changed(object sender, IntegerChangedEventArgs e)
        {
            syncWithModel();
        }
    }
}
