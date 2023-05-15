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

        private List<Mapping> _mappings;
        private int _numMappings;

        public ActionMap()
        {
            _mappings = new List<Mapping>();
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
        public bool Invoke(String action, Event evt)
        {
            foreach (Mapping mapping in _mappings)
            {
                if (mapping._key == action)
                {
                    mapping.Call(evt);
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
        public bool InvokeDirect(String action)
        {
            foreach (Mapping mapping in _mappings)
            {
                if (mapping._key == action)
                {
                    mapping.Call();
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
        public void AddMapping(String action, Object target, String methodName, Object[] parameters, int flags)
        {
            if (action == null)
            {
                throw new NullReferenceException("action");
            }

            foreach (MethodInfo m in target.GetType().GetMethods())
            {
                if (m.Name.Equals(methodName) && !m.IsStatic)
                {
                    if (ClassUtils.IsParamsCompatible(m.GetParameters(), parameters))
                    {
                        AddMappingImpl(action, target, m, parameters, flags);
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
        public void AddMapping(String action, Type targetClass, String methodName, Object[] parameters, int flags)
        {
            if (action == null)
            {
                throw new NullReferenceException("action");
            }
            foreach (MethodInfo m in targetClass.GetMethods())
            {
                if (m.Name.Equals(methodName) && m.IsStatic)
                {
                    if (ClassUtils.IsParamsCompatible(m.GetParameters(), parameters))
                    {
                        AddMappingImpl(action, null, m, parameters, flags);
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
        public void AddMapping(String action, Object target, MethodInfo method, Object[] parameters, int flags)
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
            if (!ClassUtils.IsParamsCompatible(method.GetParameters(), parameters))
            {
                throw new ArgumentOutOfRangeException("Paramters don't match method");
            }
            AddMappingImpl(action, target, method, parameters, flags);
        }

        /**
         * Add action mapping for all public methods of the specified class which
         * are annotated with the {@code Action} annotation.
         *
         * @param target the target class
         * @see Action
         */
        public void AddMapping(Object target)
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
                    AddMappingImpl(name, target, m, null, flags);
                }
            }
        }

        protected void AddMappingImpl(String action, Object target, MethodInfo method, Object[] parameters, int flags)
        {
            _mappings.Add(new Mapping(action, target, method, parameters, flags));
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

        public class Mapping
        {
            Object _target;
            MethodInfo _method;
            Object[] _parameters;
            int _flags;
            internal string _key;

            internal Mapping(String key, Object target, MethodInfo method, Object[] parameters, int flags)
            {
                this._key = key;
                this._target = target;
                this._method = method;
                this._parameters = parameters;
                this._flags = flags;
            }

            internal void Call(Event e)
            {
                EventType type = e.GetEventType();
                if ((type == EventType.KEY_RELEASED && ((_flags & FLAG_ON_RELEASE) != 0)) ||
                        (type == EventType.KEY_PRESSED && ((_flags & FLAG_ON_PRESSED) != 0) &&
                        (!e.IsKeyRepeated() || ((_flags & FLAG_ON_REPEAT) != 0))))
                {
                    Call();
                }
            }

            internal void Call()
            {
                try
                {
                    _method.Invoke(_target, _parameters);
                }
                catch (Exception ex)
                {
                    Logger.GetLogger(typeof(ActionMap)).Log(Level.SEVERE,
                            "Exception while invoking action handler", ex);
                }
            }
        }
    }
}
