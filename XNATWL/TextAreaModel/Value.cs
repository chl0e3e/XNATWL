using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static XNATWL.TextAreaModel.Value;

namespace XNATWL.TextAreaModel
{
    public class Value
    {
        private float _value;
        private Unit _unit;

        public Value(float value, Unit unit)
        {
            if (unit == null)
            {
                throw new ArgumentNullException("unit");
            }

            if (unit == Unit.AUTO && value != 0f)
            {
                throw new ArgumentOutOfRangeException("value must be 0 for Unit.AUTO");
            }

            this._value = value;
            this._unit = unit;
        }

        public float FloatValue
        {
            get
            {
                return this._value;
            }
        }

        public Unit UnitOfValue
        {
            get
            {
                return this._unit;
            }
        }

        public override string ToString()
        {
            if (this._unit == Unit.AUTO)
            {
                return this._unit.Postfix;
            }

            return this._value + this._unit.Postfix;
        }

        public static Value ZERO_PX = new Value(0, Unit.PX);
        public static Value AUTO = new Value(0, Unit.AUTO);

        public class Unit
        {
            public static Unit PX = new Unit(false, "px");
            public static Unit PT = new Unit(false, "pt");
            public static Unit EM = new Unit(true, "em");
            public static Unit EX = new Unit(true, "ex");
            public static Unit PERCENT = new Unit(false, "%");
            public static Unit AUTO = new Unit(false, "auto");

            private bool _fontBased;
            private string _postfix;

            public Unit(bool fontBased, string postfix)
            {
                this._fontBased = fontBased;
                this._postfix = postfix;
            }

            public bool FontBased
            {
                get
                {
                    return _fontBased;
                }
            }

            public string Postfix
            {
                get
                {
                    return _postfix;
                }
            }
        }
    }
}
