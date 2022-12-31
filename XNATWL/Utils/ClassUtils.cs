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
        public static bool isParamCompatible(ParameterInfo paramInfo, Object obj)
        {
            if(obj == null && !paramInfo.ParameterType.IsPrimitive)
            {
                return true;
            }

            return paramInfo.ParameterType.IsInstanceOfType(obj);
        }

        public static bool isParamCompatible(Type type, Object obj)
        {
            if (obj == null && !type.IsPrimitive)
            {
                return true;
            }

            return type.IsInstanceOfType(obj);
        }

        public static bool isParamsCompatible(ParameterInfo[] paramInfos, Object[] parameters)
        {
            if (paramInfos.Length != parameters.Length)
            {
                return false;
            }

            for (int i = 0; i < paramInfos.Length; i++)
            {
                if (!isParamCompatible(paramInfos[i], parameters[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool isParamsCompatible(Type[] types, Object[] parameters)
        {
            if (types.Length != parameters.Length) {
                return false;
            }

            for (int i = 0; i < types.Length; i++)
            {
                if (!isParamCompatible(types[i], parameters[i])) {
                    return false;
                }
            }

            return true;
        }
    }
}
