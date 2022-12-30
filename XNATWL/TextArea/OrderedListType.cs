using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Utils;

namespace XNATWL.TextArea
{
    public class OrderedListType
    {
        public static OrderedListType DECIMAL = new OrderedListType();

        protected string characterList;

        public OrderedListType()
        {
            this.characterList = null;
        }

        public OrderedListType(String characterList)
        {
            this.characterList = characterList;
        }

        public string Format(int nr)
        {
            if (nr >= 1 && characterList != null)
            {
                return TextUtil.ToCharListNumber(nr, characterList);
            }
            else
            {
                return nr.ToString();
            }
        }
    }
}
