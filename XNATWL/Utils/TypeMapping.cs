﻿using System;
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

        public void SetByType(Type type, object value)
        {
            this.Types.Add(type, value);
        }

        public void RemoveByType(Type type)
        {
            this.Types.Remove(type);
        }
    }
}
