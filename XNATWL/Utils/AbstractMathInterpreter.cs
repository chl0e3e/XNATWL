using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Utils
{
    public abstract class AbstractMathInterpreter : SimpleMathParser.Interpreter
    {
        public interface Function
        {
            Object execute(params Object[] args);
        }

        private List<Object> stack;
        private Dictionary<String, Function> functions;

        public AbstractMathInterpreter()
        {
            this.stack = new List<Object>();
            this.functions = new Dictionary<String, Function>();

            registerFunction("min", new FunctionMin());
            registerFunction("max", new FunctionMax());
        }

        public abstract void accessVariable(string name);

        public void registerFunction(String name, Function function)
        {
            if (function == null)
            {
                throw new NullReferenceException("function");
            }
            functions.Add(name, function);
        }

        public Number execute(String str)
        {
            stack.Clear();
            SimpleMathParser.interpret(str, this);
            if (stack.Count != 1)
            {
                throw new InvalidOperationException("Expected one return value on the stack");
            }
            return popNumber();
        }

        public int[] executeIntArray(String str)
        {
            stack.Clear();
            int count = SimpleMathParser.interpretArray(str, this);
            if (stack.Count != count)
            {
                throw new InvalidOperationException("Expected " + count + " return values on the stack");
            }
            int[] result = new int[count];
            for (int i = count; i-- > 0;)
            {
                result[i] = popNumber().intValue();
            }
            return result;
        }

        public T executeCreateObject<T>(String str, Type type)
        {
            stack.Clear();
            int count = SimpleMathParser.interpretArray(str, this);
            if (stack.Count != count)
            {
                throw new InvalidOperationException("Expected " + count + " return values on the stack");
            }

            if (count == 1 && type.IsInstanceOfType(stack[0]))
            {
                return (T)stack[0];
            }

            foreach (ConstructorInfo c in type.GetConstructors())
            {
                ParameterInfo[] parameters = c.GetParameters();
                if (parameters.Length == count)
                {
                    bool match = true;
                    for (int i = 0; i < count; i++)
                    {
                        if (!ClassUtils.isParamCompatible(parameters[i], stack[i]))
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                    {
                        try
                        {
                            return (T) c.Invoke(stack.ToArray());
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

        protected void push(Object obj)
        {
            stack.Add(obj);
        }

        protected Object pop()
        {
            int size = stack.Count;
            if (size == 0)
            {
                throw new InvalidOperationException("stack underflow");
            }

            object item = stack[size - 1];
            stack.RemoveAt(size - 1);
            return item;
        }

        protected Number popNumber()
        {
            Object obj = pop();

            if (obj is Number)
            {
                return (Number)obj;
            }

            throw new InvalidOperationException("expected number on stack - found: " +
                    ((obj != null) ? obj.GetType().Name : "null"));
        }

        public void loadConst(Number n)
        {
            push(n);
        }

        public void add()
        {
            Number b = popNumber();
            Number a = popNumber();
            bool oIsFloat = isFloat(a) || isFloat(b);
            if (oIsFloat)
            {
                push(a.floatValue() + b.floatValue());
            }
            else
            {
                push(a.intValue() + b.intValue());
            }
        }

        public void sub()
        {
            Number b = popNumber();
            Number a = popNumber();
            bool oIsFloat = isFloat(a) || isFloat(b);
            if (oIsFloat)
            {
                push(a.floatValue() - b.floatValue());
            }
            else
            {
                push(a.intValue() - b.intValue());
            }
        }

        public void mul()
        {
            Number b = popNumber();
            Number a = popNumber();
            bool oIsFloat = isFloat(a) || isFloat(b);
            if (oIsFloat)
            {
                push(a.floatValue() * b.floatValue());
            }
            else
            {
                push(a.intValue() * b.intValue());
            }
        }

        public void div()
        {
            Number b = popNumber();
            Number a = popNumber();
            bool oIsFloat = isFloat(a) || isFloat(b);
            if (oIsFloat)
            {
                if (Math.Abs(b.floatValue()) == 0)
                {
                    throw new InvalidOperationException("division by zero");
                }
                push(a.floatValue() / b.floatValue());
            }
            else
            {
                if (b.intValue() == 0)
                {
                    throw new InvalidOperationException("division by zero");
                }
                push(a.intValue() / b.intValue());
            }
        }

        public void negate()
        {
            Number a = popNumber();
            if (isFloat(a))
            {
                push(-a.floatValue());
            }
            else
            {
                push(-a.intValue());
            }
        }

        public void accessArray()
        {
            Number idx = popNumber();
            Object obj = pop();

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
                push(((Array)obj).GetValue(idx.intValue()));
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new InvalidOperationException("array index out of bounds", ex);
            }
        }

        public virtual void accessField(String field)
        {
            Object obj = pop();
            if (obj == null)
            {
                throw new InvalidOperationException("null pointer");
            }
            Object result = accessField(obj, field);
            push(result);
        }

        protected virtual Object accessField(Object obj, String field)
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
                    MethodInfo m = findGetter(clazz, field);
                    if (m == null)
                    {
                        foreach (Type i in clazz.GetInterfaces())
                        {
                            m = findGetter(i, field);
                            if (m != null)
                            {
                                break;
                            }
                        }
                    }
                    if (m != null)
                    {
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

        private static MethodInfo findGetter(Type clazz, String field)
        {
            foreach (MethodInfo m in clazz.GetMethods())
            {
                if (!m.IsStatic &&
                        m.ReturnType != typeof(void) &&
                        m.IsPublic &&
                        m.GetParameters().Length == 0 &&
                        (cmpName(m, field, "get") || cmpName(m, field, "is")))
                {
                    return m;
                }
            }
            return null;
        }

        private static bool cmpName(MethodInfo m, String fieldName, String prefix)
        {
            String methodName = m.Name;
            int prefixLength = prefix.Length;
            int fieldNameLength = fieldName.Length;
            return methodName.Length == (prefixLength + fieldNameLength) &&
                    methodName.StartsWith(prefix) &&
                    methodName[prefixLength] == fieldName.ToUpper()[0] &&
                    methodName.Substring(0, prefixLength + 1) == fieldName.Substring(1, fieldNameLength - 1);
        }

        public void callFunction(String name, int args)
        {
            Object[] values = new Object[args];
            for (int i = args; i-- > 0;)
            {
                values[i] = pop();
            }
            Function function = functions[name];
            if (function == null)
            {
                throw new ArgumentOutOfRangeException("Unknown function");
            }
            push(function.execute(values));
        }

        protected static bool isFloat(Number n)
        {
            return !n.IsRational();
        }

        public abstract class NumberFunction : Function
        {
            protected abstract Object execute(params int[] values);
            protected abstract Object execute(params float[] values);

            public Object execute(params Object[] args)
            {
                foreach (Object o in args)
                {
                    if (!(o is Int32)) {
                        float[] fvalues = new float[args.Length];
                        for (int i = 0; i < fvalues.Length; i++)
                        {
                            fvalues[i] = ((Number)args[i]).floatValue();
                        }
                        return execute(fvalues);
                    }
                }
                int[] values = new int[args.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = ((Number)args[i]).intValue();
                }
                return execute(values);
            }
        }

        public class FunctionMin : NumberFunction
        {
            protected override Object execute(params int[] values)
            {
                int result = values[0];
                for (int i = 1; i < values.Length; i++)
                {
                    result = Math.Min(result, values[i]);
                }
                return result;
            }

            protected override Object execute(params float[] values)
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
            protected override Object execute(params int[] values)
            {
                int result = values[0];
                for (int i = 1; i < values.Length; i++)
                {
                    result = Math.Max(result, values[i]);
                }
                return result;
            }

            protected override Object execute(params float[] values)
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
