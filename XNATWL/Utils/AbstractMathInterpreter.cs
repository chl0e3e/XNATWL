/*
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
using System.Collections.Generic;
using System.Reflection;

namespace XNATWL.Utils
{
    public abstract class AbstractMathInterpreter : SimpleMathParser.Interpreter
    {
        public interface Function
        {
            Object Execute(params Object[] args);
        }

        private List<Object> _stack;
        private Dictionary<String, Function> _functions;

        public AbstractMathInterpreter()
        {
            this._stack = new List<Object>();
            this._functions = new Dictionary<String, Function>();

            RegisterFunction("min", new FunctionMin());
            RegisterFunction("max", new FunctionMax());
        }

        public abstract void AccessVariable(string name);

        public void RegisterFunction(String name, Function function)
        {
            if (function == null)
            {
                throw new NullReferenceException("function");
            }

            _functions.Add(name, function);
        }

        public Number Execute(String str)
        {
            _stack.Clear();
            SimpleMathParser.Interpret(str, this);
            if (_stack.Count != 1)
            {
                throw new InvalidOperationException("Expected one return value on the stack");
            }
            return PopNumber();
        }

        public int[] ExecuteIntArray(String str)
        {
            _stack.Clear();
            int count = SimpleMathParser.InterpretArray(str, this);
            if (_stack.Count != count)
            {
                throw new InvalidOperationException("Expected " + count + " return values on the stack");
            }
            int[] result = new int[count];
            for (int i = count; i-- > 0;)
            {
                result[i] = PopNumber().IntValue();
            }
            return result;
        }

        public T ExecuteCreateObject<T>(String str, Type type)
        {
            _stack.Clear();
            int count = SimpleMathParser.InterpretArray(str, this);
            if (_stack.Count != count)
            {
                throw new InvalidOperationException("Expected " + count + " return values on the stack");
            }

            if (count == 1 && type.IsInstanceOfType(_stack[0]))
            {
                return (T)_stack[0];
            }

            foreach (ConstructorInfo c in type.GetConstructors())
            {
                ParameterInfo[] parameters = c.GetParameters();
                if (parameters.Length == count)
                {
                    bool match = true;
                    for (int i = 0; i < count; i++)
                    {
                        if (!ClassUtils.IsParamCompatible(parameters[i], _stack[i]))
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                    {
                        try
                        {
                            return (T) c.Invoke(_stack.ToArray());
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("AbstractMathInterpreter can't instantiate object", ex);
                        }
                    }
                }
            }

            throw new ArgumentOutOfRangeException("Can't construct a " + type + " from expression: \"" + str + "\"");
        }

        protected void Push(Object obj)
        {
            if (obj.GetType() == typeof(Single))
            {
                System.Diagnostics.Debug.WriteLine("objPush: " + obj.GetType().FullName);
            }

            _stack.Add(obj);
        }

        protected Object Pop()
        {
            int size = _stack.Count;
            if (size == 0)
            {
                throw new InvalidOperationException("stack underflow");
            }

            object item = _stack[size - 1];
            _stack.RemoveAt(size - 1);
            return item;
        }

        protected Number PopNumber()
        {
            Object obj = Pop();

            if (obj is Number)
            {
                return (Number)obj;
            }

            System.Diagnostics.Debug.WriteLine(obj);
            throw new InvalidOperationException("expected number on stack - found: " +
                    ((obj != null) ? obj.GetType().Name : "null"));
        }

        public void LoadConst(Number n)
        {
            Push(n);
        }

        public void Add()
        {
            Number b = PopNumber();
            Number a = PopNumber();
            bool oIsFloat = IsFloat(a) || IsFloat(b);
            Push(a + b);
        }

        public void Sub()
        {
            Number b = PopNumber();
            Number a = PopNumber();
            bool oIsFloat = IsFloat(a) || IsFloat(b);
            Push(a - b);
        }

        public void Mul()
        {
            Number b = PopNumber();
            Number a = PopNumber();
            bool oIsFloat = IsFloat(a) || IsFloat(b);
            Push(a * b);
        }

        public void Div()
        {
            Number b = PopNumber();
            Number a = PopNumber();
            bool oIsFloat = IsFloat(a) || IsFloat(b);
            Push(a / b);
        }

        public void Negate()
        {
            Number a = PopNumber();
            Push(-a);
        }

        public void AccessArray()
        {
            Number idx = PopNumber();
            Object obj = Pop();

            if (obj == null)
            {
                throw new InvalidOperationException("null pointer");
            }

            if (!obj.GetType().IsArray)
            {
                throw new InvalidOperationException("array expected");
            }

            try
            {
                Push(((Array)obj).GetValue(idx.IntValue()));
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new InvalidOperationException("array index out of bounds", ex);
            }
        }

        public virtual void AccessField(String field)
        {
            Object obj = Pop();
            if (obj == null)
            {
                throw new InvalidOperationException("null pointer");
            }
            Object result = AccessField(obj, field);
            Push(result);
        }

        protected virtual Object AccessField(Object obj, String field)
        {
            Type clazz = obj.GetType();
            try
            {
                if (clazz.IsArray)
                {
                    if ("length".Equals(field))
                    {
                        return ((Array)obj).Length;
                    }
                }
                else
                {
                    MethodInfo m = FindGetter(clazz, field);
                    if (m == null)
                    {
                        foreach (Type i in clazz.GetInterfaces())
                        {
                            m = FindGetter(i, field);
                            if (m != null)
                            {
                                break;
                            }
                        }
                    }
                    if (m != null)
                    {
                        if (m.ReturnType == typeof(Int32))
                        {
                            return new Number((Int32)m.Invoke(obj, new object[0]));
                        }
                        else if (m.ReturnType == typeof(float))
                        {
                            return new Number((float)m.Invoke(obj, new object[0]));
                        }
                        return m.Invoke(obj, new object[0]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("error accessing field '" + field +
                            "' of class '" + clazz + "'", ex);
            }
            throw new InvalidOperationException("unknown field '" + field +
            "' of class '" + clazz + "'");
        }

        private static MethodInfo FindGetter(Type clazz, String field)
        {
            foreach (MethodInfo m in clazz.GetMethods())
            {
                if (!m.IsStatic &&
                        m.ReturnType != typeof(void) &&
                        m.IsPublic &&
                        m.GetParameters().Length == 0 &&
                        (CmpName(m, field, "get") || CmpName(m, field, "get_") || CmpName(m, field, "is")))
                {
                    return m;
                }
            }
            return null;
        }

        private static bool CmpName(MethodInfo m, String fieldName, String prefix)
        {
            return (prefix + fieldName).ToLower() == m.Name.ToLower();
        }

        public void CallFunction(String name, int args)
        {
            Object[] values = new Object[args];
            for (int i = args; i-- > 0;)
            {
                values[i] = Pop();
            }
            Function function = _functions[name];
            if (function == null)
            {
                throw new ArgumentOutOfRangeException("Unknown function");
            }
            Push(function.Execute(values));
        }

        protected static bool IsFloat(Number n)
        {
            return !n.IsRational();
        }

        public abstract class NumberFunction : Function
        {
            protected abstract Object Execute(params int[] values);
            protected abstract Object Execute(params float[] values);

            public Object Execute(params Object[] args)
            {
                foreach (Object o in args)
                {
                    if (!(o is Int32)) {
                        float[] fvalues = new float[args.Length];
                        for (int i = 0; i < fvalues.Length; i++)
                        {
                            fvalues[i] = ((Number)args[i]).FloatValue();
                        }
                        return Execute(fvalues);
                    }
                }
                int[] values = new int[args.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = ((Number)args[i]).IntValue();
                }
                return Execute(values);
            }
        }

        public class FunctionMin : NumberFunction
        {
            protected override Object Execute(params int[] values)
            {
                int result = values[0];
                for (int i = 1; i < values.Length; i++)
                {
                    result = Math.Min(result, values[i]);
                }
                return result;
            }

            protected override Object Execute(params float[] values)
            {
                float result = values[0];
                for (int i = 1; i < values.Length; i++)
                {
                    result = Math.Min(result, values[i]);
                }
                return result;
            }
        }

        public class FunctionMax : NumberFunction
        {
            protected override Object Execute(params int[] values)
            {
                int result = values[0];
                for (int i = 1; i < values.Length; i++)
                {
                    result = Math.Max(result, values[i]);
                }
                return result;
            }

            protected override Object Execute(params float[] values)
            {
                float result = values[0];
                for (int i = 1; i < values.Length; i++)
                {
                    result = Math.Max(result, values[i]);
                }
                return result;
            }
        }
    }
}
