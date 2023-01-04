using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Utils
{
    public class TypeMapping
    {
        private Dictionary<Type, object> Types;

        public TypeMapping()
        {
            this.Types = new Dictionary<Type, object>();
        }

        public object GetByType(Type type)
        {
            foreach (Type storedType in this.Types.Keys)
            {
                if (storedType == type)
                {
                    return this.Types[type];
                }

                foreach (Type interfaceType in storedType.GetInterfaces())
                {
                    if (interfaceType == type)
                    {
                        return this.Types[type];
                    }
                }

                if (storedType.BaseType == type)
                {
                    return this.Types[type];
                }
            }

            return null;
        }

        public HashSet<Object> getUniqueValues()
        {
            HashSet<Object> result = new HashSet<Object>();
            foreach (object e in this.Types.Values)
            {
                if (!result.Contains(e))
                {
                    result.Add(e);
                }
            }
            return result;
        }

        public void SetByType(Type type, object value)
        {
            this.Types.Add(type, value);
        }

        public bool RemoveByType(Type type)
        {
            if (GetByType(type) != null)
            {
                this.Types.Remove(type);
                return true;
            }

            return false;
        }
    }
}
