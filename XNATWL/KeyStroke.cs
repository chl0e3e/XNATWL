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
using System.Text;
using XNATWL.Utils;

namespace XNATWL
{
    public class KeyStroke
    {
        private const int SHIFT = 1;
        private const int CTRL = 2;
        private const int META = 4;
        private const int ALT = 8;
        private const int CMD = 20;  // special: CMD is LMETA so META is also set ...

        private int _modifier;
        private int _keyCode;
        private char _keyChar;
        private String _action;

        private KeyStroke(int modifier, int keyCode, char keyChar, String action)
        {
            this._modifier = modifier;
            this._keyCode = keyCode;
            this._keyChar = keyChar;
            this._action = action;
        }

        /**
         * Returns the action name for this key stroke
         * @return the action name
         */
        public String GetAction()
        {
            return _action;
        }

        /**
         * Returns the key stroke in parsable form
         * @return the key stroke
         * @see #parse(java.lang.String, java.lang.String)
         */
        public String GetStroke()
        {
            StringBuilder sb = new StringBuilder();
            if ((_modifier & SHIFT) == SHIFT)
            {
                sb.Append("shift ");
            }
            if ((_modifier & CTRL) == CTRL)
            {
                sb.Append("ctrl ");
            }
            if ((_modifier & ALT) == ALT)
            {
                sb.Append("alt ");
            }
            if ((_modifier & CMD) == CMD)
            {
                sb.Append("cmd ");
            }
            else if ((_modifier & META) == META)
            {
                sb.Append("meta ");
            }
            if (_keyCode != Event.KEY_NONE)
            {
                sb.Append(Event.GetKeyNameForCode(_keyCode));
            }
            else
            {
                sb.Append("typed ").Append(_keyChar);
            }
            return sb.ToString();
        }

        /**
         * Two KeyStroke objects are equal if the have the same key stroke, it does not compare the action.
         *
         * @param obj the other object to compare against
         * @return true if the other object is a KeyStroke and responds to the same input event
         * @see #getStroke()
         */
        public override bool Equals(Object obj)
        {
            if (obj is KeyStroke)
            {
                KeyStroke other = (KeyStroke)obj;
                return (this._modifier == other._modifier) &&
                        (this._keyCode == other._keyCode) &&
                        (this._keyChar == other._keyChar);
            }
            return false;
        }

        /**
         * Computes the hash code for this key stroke without the action.
         * @return the hash code
         */
        public override int GetHashCode()
        {
            int hash = 5;
            hash = 83 * hash + this._modifier;
            hash = 83 * hash + this._keyCode;
            hash = 83 * hash + this._keyChar;
            return hash;
        }

        /**
         * Parses a key stroke from string representation.<p>
         * The following syntax is supported:<ul>
         * <li>{@code <modifiers>* <keyName>}</li>
         * <li>{@code <modifiers>* typed <character>}</li>
         * </ul>
         * Thw folloiwng modifiers are supported;<ul>
         * <li>{@code ctrl}</li>
         * <li>{@code shift}</li>
         * <li>{@code meta}</li>
         * <li>{@code alt}</li>
         * <li>{@code cmd}</li>
         * </ul>
         * All matching is case insensitive.
         * 
         * @param stroke the key stroke
         * @param action the action to associate
         * @return the parsed KeyStroke
         * @throws ArgumentOutOfRangeException if the key stroke can't be parsed
         * @see Keyboard#getKeyIndex(java.lang.String)
         */
        public static KeyStroke Parse(String stroke, String action)
        {
            if (stroke == null)
            {
                throw new ArgumentNullException("stroke");
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            int idx = TextUtil.SkipSpaces(stroke, 0);
            int modifers = 0;
            char keyChar = Event.CHAR_NONE;
            int keyCode = Event.KEY_NONE;
            bool typed = false;
            bool end = false;

            foreach(string strokePart in stroke.Split(' '))
            {
                if (typed)
                {
                    if (strokePart.Length != 1)
                    {
                        throw new ArgumentOutOfRangeException("Expected single character after 'typed'");
                    }
                    keyChar = strokePart[0];
                    if (keyChar == Event.CHAR_NONE)
                    {
                        throw new ArgumentOutOfRangeException("Unknown character: " + strokePart);
                    }
                    end = true;
                }
                else if ("ctrl".Equals(strokePart.ToLower()) || "control".Equals(strokePart.ToLower()))
                {
                    modifers |= CTRL;
                }
                else if ("shift".Equals(strokePart.ToLower()))
                {
                    modifers |= SHIFT;
                }
                else if ("meta".Equals(strokePart.ToLower()))
                {
                    modifers |= META;
                }
                else if ("cmd".Equals(strokePart.ToLower()))
                {
                    modifers |= CMD;
                }
                else if ("alt".Equals(strokePart.ToLower()))
                {
                    modifers |= ALT;
                }
                else if ("typed".Equals(strokePart.ToLower()))
                {
                    typed = true;
                }
                else
                {
                    keyCode = Event.GetKeyCodeForName(strokePart.ToUpper());
                    if (keyCode == Event.KEY_NONE)
                    {
                        throw new ArgumentOutOfRangeException("Unknown key: " + strokePart);
                    }
                    end = true;
                }
            }

            if (!end)
            {
                throw new ArgumentOutOfRangeException("Unexpected end of string");
            }

            return new KeyStroke(modifers, keyCode, keyChar, action);
        }

        /**
         * Creates a KeyStroke from the KEY_PRESSED event.
         *
         * @param event the input event
         * @param action the action to associate
         * @return the KeyStroke for this event and action
         * @throws ArgumentOutOfRangeException if the event is not a Type.KEY_PRESSED
         */
        public static KeyStroke FromEvent(Event @event, String action)
        {
            if (@event == null)
            {
                throw new ArgumentNullException("event");
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            if (@event.GetEventType() != EventType.KEY_PRESSED)
            {
                throw new ArgumentOutOfRangeException("Event is not a Type.KEY_PRESSED");
            }
            int modifiers = ConvertModifier(@event);
            return new KeyStroke(modifiers, @event.GetKeyCode(), Event.CHAR_NONE, action);
        }

        public bool Match(Event e, int mappedEventModifiers)
        {
            if (mappedEventModifiers != _modifier)
            {
                return false;
            }
            if (_keyCode != Event.KEY_NONE && _keyCode != e.GetKeyCode())
            {
                return false;
            }
            if (_keyChar != Event.CHAR_NONE && (!e.HasKeyChar() || _keyChar != e.GetKeyChar()))
            {
                return false;
            }
            return true;
        }

        public static int ConvertModifier(Event @event)
        {
            int eventModifiers = @event.GetModifiers();
            int modifiers = 0;
            if ((eventModifiers & Event.MODIFIER_SHIFT) != 0)
            {
                modifiers |= SHIFT;
            }
            if ((eventModifiers & Event.MODIFIER_CTRL) != 0)
            {
                modifiers |= CTRL;
            }
            if ((eventModifiers & Event.MODIFIER_META) != 0)
            {
                modifiers |= META;
            }
            if ((eventModifiers & Event.MODIFIER_LMETA) != 0)
            {
                modifiers |= CMD;
            }
            if ((eventModifiers & Event.MODIFIER_ALT) != 0)
            {
                modifiers |= ALT;
            }
            return modifiers;
        }
    }
}
