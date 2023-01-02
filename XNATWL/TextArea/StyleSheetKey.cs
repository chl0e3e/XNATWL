using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.TextArea
{
    public class StyleSheetKey
    {
        private string _element;
        private string _className;
        private string _id;

        public StyleSheetKey(string element, string className, string id)
        {
            _element = element;
            _className = className;
            _id = id;
        }

        public string ClassName
        {
            get
            {
                return this._className;
            }
        }

        public string Element
        {
            get
            {
                return this._element;
            }
        }
        
        public string ID
        {
            get
            {
                return this._id;
            }
        }

        public override int GetHashCode()
        {
            int hash = 7;
            hash = 53 * hash + (this.Element != null ? this.Element.GetHashCode() : 0);
            hash = 53 * hash + (this.ClassName != null ? this.ClassName.GetHashCode() : 0);
            hash = 53 * hash + (this.ID != null ? this.ID.GetHashCode() : 0);
            return hash;
        }

        public bool Matches(StyleSheetKey what)
        {
            if (this._element != null && !this._element.Equals(what.Element))
            {
                return false;
            }
            if (this.ClassName != null && !this.ClassName.Equals(what.ClassName))
            {
                return false;
            }
            if (this.ID != null && !this.ID.Equals(what.ID))
            {
                return false;
            }
            return true;
        }

        public override bool Equals(object other)
        {
            if (other is StyleSheetKey)
            {
                StyleSheetKey otherKey = (StyleSheetKey)other;

                return ((this.Element == null) ? (otherKey.Element == null) : this.Element.Equals(otherKey.Element)) &&
                        ((this.ClassName == null) ? (otherKey.ClassName == null) : this.ClassName.Equals(otherKey.ClassName)) &&
                        ((this.ID == null) ? (otherKey.ID == null) : this.ID.Equals(otherKey.ID));
            }

            return false;
        }
    }
}
