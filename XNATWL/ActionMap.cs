using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static XNATWL.Utils.Logger;
using XNATWL.Utils;

namespace XNATWL
{
    public class ActionMap
    {
        /**
         * Invocation flag
         *
         * Invoke the method on the first key pressed event.
         *
         * @see #addMapping(java.lang.String, java.lang.Object, java.lang.reflect.Method, java.lang.Object[], int)
         * @see #FLAG_ON_REPEAT
         */
        public static int FLAG_ON_PRESSED = 1;

        /**
         * Invocation flag
         *
         * Invoke the method on a key release event.
         *
         * @see #addMapping(java.lang.String, java.lang.Object, java.lang.reflect.Method, java.lang.Object[], int)
         */
        public static int FLAG_ON_RELEASE = 2;

        /**
         * Invocation flag
         *
         * Invoke the method also on a repeated key pressed event.
         *
         * @see #addMapping(java.lang.String, java.lang.Object, java.lang.reflect.Method, java.lang.Object[], int)
         * @see #FLAG_ON_PRESSED
         */
        public static int FLAG_ON_REPEAT = 4;

        private List<Mapping> mappings;
        private int numMappings;

        public ActionMap()
        {
            mappings = new List<Mapping>();
        }

        /**
         * Invoke the mapping for the given action if one is defined and it's flags
         * match the passed event.
         * 
         * @param action the action name
         * @param event the event which caused the invocation
         * @return true if a mapping was found, false if no mapping was found.
         * @see #addMapping(java.lang.String, java.lang.Object, java.lang.reflect.Method, java.lang.Object[], int)
         * @throws NullReferenceException when either action or event is null
         */
        public bool invoke(String action, Event evt)
        {
            foreach (Mapping mapping in mappings)
            {
                if (mapping.key == action)
                {
                    mapping.call(evt);
                    return true;
                }
            }

            return false;
        }

        /**
         * Invoke the mapping for the given action if one is defined without
         * checking any flags.
         * 
         * @param action the action name
         * @return true if a mapping was found, false if no mapping was found.
         * @see #addMapping(java.lang.String, java.lang.Object, java.lang.reflect.Method, java.lang.Object[], int)
         * @throws NullReferenceException when action is null
         */
        public bool invokeDirect(String action)
        {
            foreach (Mapping mapping in mappings)
            {
                if (mapping.key == action)
                {
                    mapping.call();
                    return true;
                }
            }

            return false;
        }

        /**
         * Add an action mapping for the specified action to the given public instance method.
         *
         * Parameters can be passed to the method to differentiate between different
         * actions using the same handler method.
         *
         * NOTE: if multiple methods are compatible to the given parameters then it's
         * undefined which method will be selected. No overload resolution is performed
         * besides a simple parameter compatibility check.
         *
         * @param action the action name
         * @param target the target object
         * @param methodName the method name
         * @param params parameters passed to the method
         * @param flags flags to control on which events the method should be invoked
         * @throws ArgumentOutOfRangeException if no matching method was found
         * @throws NullReferenceException when {@code action}, {@code target} or {@code params} is null
         * @see ClassUtils#isParamsCompatible(java.lang.Class<?>[], java.lang.Object[])
         * @see #FLAG_ON_PRESSED
         * @see #FLAG_ON_RELEASE
         * @see #FLAG_ON_REPEAT
         */
        public void addMapping(String action, Object target, String methodName, Object[] parameters, int flags)
        {
            if (action == null)
            {
                throw new NullReferenceException("action");
            }

            foreach (MethodInfo m in target.GetType().GetMethods())
            {
                if (m.Name.Equals(methodName) && !m.IsStatic)
                {
                    if (ClassUtils.isParamsCompatible(m.GetParameters(), parameters))
                    {
                        addMappingImpl(action, target, m, parameters, flags);
                        return;
                    }
                }
            }

            throw new ArgumentOutOfRangeException("Can't find matching method: " + methodName);
        }

        /**
         * Add an action mapping for the specified action to the given public static method.
         *
         * Parameters can be passed to the method to differentiate between different
         * actions using the same handler method.
         *
         * NOTE: if multiple methods are compatible to the given parameters then it's
         * undefined which method will be selected. No overload resolution is performed
         * besides a simple parameter compatibility check.
         *
         * @param action the action name
         * @param targetClass the target class
         * @param methodName the method name
         * @param params parameters passed to the method
         * @param flags flags to control on which events the method should be invoked
         * @throws NullReferenceException when {@code action}, {@code targetClass} or {@code params} is null
         * @throws ArgumentOutOfRangeException if no matching method was found
         * @see ClassUtils#isParamsCompatible(java.lang.Class<?>[], java.lang.Object[])
         * @see #FLAG_ON_PRESSED
         * @see #FLAG_ON_RELEASE
         * @see #FLAG_ON_REPEAT
         */
        public void addMapping(String action, Type targetClass, String methodName, Object[] parameters, int flags)
        {
            if (action == null)
            {
                throw new NullReferenceException("action");
            }
            foreach (MethodInfo m in targetClass.GetMethods())
            {
                if (m.Name.Equals(methodName) && m.IsStatic)
                {
                    if (ClassUtils.isParamsCompatible(m.GetParameters(), parameters))
                    {
                        addMappingImpl(action, null, m, parameters, flags);
                        return;
                    }
                }
            }
            throw new ArgumentOutOfRangeException("Can't find matching method: " + methodName);
        }

        /**
         * Add an action mapping for the specified action to the given method.
         *
         * Parameters can be passed to the method to differentiate between different
         * actions using the same handler method.
         *
         * @param action the action name
         * @param target the target object. Can be null when the method is static
         * @param method the method to invoke
         * @param params the parameters to pass to the method
         * @param flags flags to control on which events the method should be invoked
         * @throws NullReferenceException when {@code action}, {@code method} or {@code params} is null
         * @throws ArgumentOutOfRangeException <ul>
         *   <li>when the method is not public</li>
         *   <li>when the method does not belong to the target object</li>
         *   <li>when the parameters do not match the arguments</li>
         * </ul>
         * @see ClassUtils#isParamsCompatible(java.lang.Class<?>[], java.lang.Object[])
         * @see #FLAG_ON_PRESSED
         * @see #FLAG_ON_RELEASE
         * @see #FLAG_ON_REPEAT
         */
        public void addMapping(String action, Object target, MethodInfo method, Object[] parameters, int flags)
        {
            if (action == null)
            {
                throw new NullReferenceException("action");
            }
            if (!method.IsPublic)
            {
                throw new ArgumentOutOfRangeException("Method is not public");
            }
            if (target == null && !method.IsStatic)
            {
                throw new ArgumentOutOfRangeException("Method is not static but target is null");
            }
            if (target != null && method.DeclaringType.IsInstanceOfType(target))
            {
                throw new ArgumentOutOfRangeException("method does not belong to target");
            }
            if (!ClassUtils.isParamsCompatible(method.GetParameters(), parameters))
            {
                throw new ArgumentOutOfRangeException("Paramters don't match method");
            }
            addMappingImpl(action, target, method, parameters, flags);
        }

        /**
         * Add action mapping for all public methods of the specified class which
         * are annotated with the {@code Action} annotation.
         *
         * @param target the target class
         * @see Action
         */
        public void addMapping(Object target)
        {
            foreach (MethodInfo m in target.GetType().GetMethods())
            {
                Action action = (Action) m.GetCustomAttribute(typeof(Action));
                if (action != null)
                {
                    if (m.GetParameters().Length > 0)
                    {
                        throw new InvalidOperationException("automatic binding of actions not supported for methods with parameters");
                    }
                    String name = m.Name;
                    if (action.Name.Length > 0)
                    {
                        name = action.Name;
                    }
                    int flags =
                            (action.OnPressed ? FLAG_ON_PRESSED : 0) |
                            (action.OnRelease ? FLAG_ON_RELEASE : 0) |
                            (action.OnRepeat ? FLAG_ON_REPEAT : 0);
                    addMappingImpl(name, target, m, null, flags);
                }
            }
        }

        protected void addMappingImpl(String action, Object target, MethodInfo method, Object[] parameters, int flags)
        {
            mappings.Add(new Mapping(action, target, method, parameters, flags));
        }

        public class Action : System.Attribute
        {
            public string Name;
            public bool OnPressed;
            public bool OnRelease;
            public bool OnRepeat;

            public Action(string name, bool onPressed, bool onRelease, bool onRepeat)
            {
                this.Name = name;
                this.OnPressed = onPressed;
                this.OnRelease = onRelease;
                this.OnRepeat = onRepeat;
            }

            public Action(bool onPressed, bool onRelease, bool onRepeat)
            {
                this.Name = "";
                this.OnPressed = onPressed;
                this.OnRelease = onRelease;
                this.OnRepeat = onRepeat;
            }

            public Action()
            {
                this.Name = "";
                this.OnPressed = true;
                this.OnRelease = false;
                this.OnRepeat = true;
            }
        }


        /**
         * Annotation used for automatic handler registration
         *
         * @see #addMapping(java.lang.Object)
         */
        //@Documented
        //@Retention(RetentionPolicy.RUNTIME)
        //@Target(ElementType.METHOD)
        //public @interface Action {
        /**
         * Optional action name. If not specified then the method name is used
         * as action
         * @return the action name
         */
        //String name() default "";
        /**
         * Invoke the method on first key press events
         * @return default true
         */
        //bool onPressed() default true;
        /**
         * Invoke the method on key release events
         * @return default false
         */
        //bool onRelease() default false;
        /**
         * Invoke the method also on repeated key press events
         * @return default false
         */
        //bool onRepeat() default true;
        //}

        public class Mapping
        {
            Object target;
            MethodInfo method;
            Object[] parameters;
            int flags;
            internal string key;

            internal Mapping(String key, Object target, MethodInfo method, Object[] parameters, int flags)
            {
                this.key = key;
                this.target = target;
                this.method = method;
                this.parameters = parameters;
                this.flags = flags;
            }

            internal void call(Event e)
            {
                Event.EventType type = e.getEventType();
                if ((type == Event.EventType.KEY_RELEASED && ((flags & FLAG_ON_RELEASE) != 0)) ||
                        (type == Event.EventType.KEY_PRESSED && ((flags & FLAG_ON_PRESSED) != 0) &&
                        (!e.isKeyRepeated() || ((flags & FLAG_ON_REPEAT) != 0))))
                {
                    call();
                }
            }

            internal void call()
            {
                try
                {
                    method.Invoke(target, parameters);
                }
                catch (Exception ex)
                {
                    Logger.GetLogger(typeof(ActionMap)).log(Level.SEVERE,
                            "Exception while invoking action handler", ex);
                }
            }
        }
    }
}
