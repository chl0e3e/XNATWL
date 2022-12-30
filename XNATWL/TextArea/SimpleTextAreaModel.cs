using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.TextArea
{
    public class SimpleTextAreaModel : TextAreaModel
    {
        public event EventHandler<TextAreaChangedEventArgs> Changed;

        private Style style;
        private Element element;

        public SimpleTextAreaModel()
        {
            style = new Style();
        }

        public Style Style
        {
            get
            {
                return style;
            }

            set
            {
                style = value;
            }
        }

        public void SetText(string text)
        {
            this.SetText(text, true);
        }

        public void SetText(string text, bool preformatted)
        {
            Style textstyle = style.With(StyleAttribute.PREFORMATTED, preformatted);
            this.element = new TextElement(textstyle, text);
            this.Changed.Invoke(this, new TextAreaChangedEventArgs());
        }

        public IEnumerator<Element> GetEnumerator()
        {
            return new List<Element> { element }.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new List<Element> { element }.GetEnumerator();
        }
    }
}
