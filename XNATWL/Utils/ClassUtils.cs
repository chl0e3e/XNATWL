using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Utils
{
    public class ClassUtils
    {
        public static bool isParamCompatible(ParameterInfo type, Object obj) {
            if(obj == null && !type.ParameterType.IsPrimitive) {
                return true;
            }

            return type.ParameterType.IsInstanceOfType(obj);
        }
    }
}
