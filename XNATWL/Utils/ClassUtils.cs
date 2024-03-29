﻿/*
 * Copyright (c) 2008-2012, Matthias Mann
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 *     * Redistributions of source code must retain the above copyright notice,
 *       this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of Matthias Mann nor the names of its contributors may
 *       be used to endorse or promote products derived from this software
 *       without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Reflection;

namespace XNATWL.Utils
{
    public class ClassUtils
    {
        public static bool IsParamCompatible(ParameterInfo paramInfo, Object obj)
        {
            if(obj == null && !paramInfo.ParameterType.IsPrimitive)
            {
                return true;
            }

            return paramInfo.ParameterType.IsInstanceOfType(obj);
        }

        public static bool IsParamCompatible(Type type, Object obj)
        {
            if (obj == null && !type.IsPrimitive)
            {
                return true;
            }

            return type.IsInstanceOfType(obj);
        }

        public static bool IsParamsCompatible(ParameterInfo[] paramInfos, Object[] parameters)
        {
            if (paramInfos.Length != parameters.Length)
            {
                return false;
            }

            for (int i = 0; i < paramInfos.Length; i++)
            {
                if (!IsParamCompatible(paramInfos[i], parameters[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsParamsCompatible(Type[] types, Object[] parameters)
        {
            if (types.Length != parameters.Length) {
                return false;
            }

            for (int i = 0; i < types.Length; i++)
            {
                if (!IsParamCompatible(types[i], parameters[i])) {
                    return false;
                }
            }

            return true;
        }
    }
}
