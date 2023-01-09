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

namespace XNATWL
{
    public class Event
    {


        public class EventType
        {
            /**
             * The mouse has entered the widget.
             * You need to return true from {@link Widget#handleEvent(de.matthiasmann.twl.Event) } in order to receive further mouse events.
             */
            public static EventType MOUSE_ENTERED = new EventType(true, false);
            /**
             * The mouse has moved over the widget - no mouse buttons are pressed.
             * You need to return true from {@link Widget#handleEvent(de.matthiasmann.twl.Event) } in order to receive further mouse events.
             */
            public static EventType MOUSE_MOVED = new EventType(true, false);
            /**
             * A mouse button has been pressed. The pressed button is available via {@link Event#getMouseButton() }
             */
            public static EventType MOUSE_BTNDOWN = new EventType(true, false);
            /**
             * A mouse button has been released. The released button is available via {@link Event#getMouseButton() }
             */
            public static EventType MOUSE_BTNUP = new EventType(true, false);
            /**
             * A click event with the left mouse button. A click is defined by a MOUSE_BTNDOWN event followed
             * by a MOUSE_BTNUP without moving the mouse outside the click distance. The MOUSE_BTNUP event is
             * sent before the MOUSE_CLICKED.
             */
            public static EventType MOUSE_CLICKED = new EventType(true, false);
            /**
             * The mouse has moved while at least one mouse button was pressed. The widget automatically
             * captures the mouse when a drag is started, which means that the widgets will receive mouse
             * events from this drag also outside of it's bounds. The drag ends when the last mouse button
             * is released.
             * 
             * @see Event#isMouseDragEvent()
             * @see Event#isMouseDragEnd()
             */
            public static EventType MOUSE_DRAGGED = new EventType(true, false);
            /**
             * The mouse has left the widget.
             */
            public static EventType MOUSE_EXITED = new EventType(true, false);
            /**
             * The mouse wheel has been turned. The amount is available via {@link Event#getMouseWheelDelta() }
             */
            public static EventType MOUSE_WHEEL = new EventType(true, false);
            /**
             * A key has been pressed. Not all keys generate characters.
             * @see #isKeyEvent()
             * @see #isKeyPressedEvent() 
             * @see #isKeyRepeated() 
             * @see #hasKeyChar() 
             * @see #hasKeyCharNoModifiers() 
             */
            public static EventType KEY_PRESSED = new EventType(false, true);
            /**
             * A key has been released. No character data is available.
             * @see #isKeyEvent() 
             */
            public static EventType KEY_RELEASED = new EventType(false, true);
            /**
             * A popup has been opened. Input event delivery will stop until the popup is closed.
             */
            public static EventType POPUP_OPENED = new EventType(false, false);
            /**
             * A popup has closed. Input events delivery will resume if no other popups are open.
             */
            public static EventType POPUP_CLOSED = new EventType(false, false);
            /**
             * Send when {@link GUI#clearKeyboardState() } is called.
             * Widgets which remeber {@link #KEY_PRESSED} events should clear their state.
             */
            public static EventType CLEAR_KEYBOARD_STATE = new EventType(false, false);

            public bool isMouseEvent;
            public bool isKeyEvent;
            public EventType(bool isMouseEvent, bool isKeyEvent)
            {
                this.isMouseEvent = isMouseEvent;
                this.isKeyEvent = isKeyEvent;
            }
        };

        public const int MODIFIER_LSHIFT = 1;
        public const int MODIFIER_LMETA = 2;
        public const int MODIFIER_LCTRL = 4;
        public const int MODIFIER_RSHIFT = 8;
        public const int MODIFIER_RMETA = 16;
        public const int MODIFIER_RCTRL = 32;
        public const int MODIFIER_LBUTTON = 64;
        public const int MODIFIER_RBUTTON = 128;
        public const int MODIFIER_MBUTTON = 256;
        public const int MODIFIER_LALT = 512;
        public const int MODIFIER_RALT = 1024;

        /**
         * One of the shift keys is pressed
         * @see #getModifiers()
         */
        public const int MODIFIER_SHIFT = MODIFIER_LSHIFT | MODIFIER_RSHIFT;

        /**
         * One of the meta keys (ALT on Windows) is pressed
         * @see #getModifiers()
         */
        public const int MODIFIER_META = MODIFIER_LMETA | MODIFIER_RMETA;

        /**
         * One of the control keys is pressed
         * @see #getModifiers()
         */
        public const int MODIFIER_CTRL = MODIFIER_LCTRL | MODIFIER_RCTRL;

        /**
         * One of the mouse buttons is pressed
         * @see #getModifiers()
         */
        public const int MODIFIER_BUTTON = MODIFIER_LBUTTON | MODIFIER_MBUTTON | MODIFIER_RBUTTON;

        /**
         * One of the alt/menu keys is pressed
         * @see #getModifiers()
         */
        public const int MODIFIER_ALT = MODIFIER_LALT | MODIFIER_RALT;

        /**
         * Left mouse button - this is the primary mouse button
         * @see #getMouseButton()
         */
        public const int MOUSE_LBUTTON = 0;

        /**
         * Right mouse button - this is for context menus
         * @see #getMouseButton()
         */
        public const int MOUSE_RBUTTON = 1;

        /**
         * Middle mouse button
         * @see #getMouseButton()
         */
        public const int MOUSE_MBUTTON = 2;

        /**
         * The special character meaning that no character was translated for the event.
         */
        public const char CHAR_NONE = '\0';

        /**
         * The special keycode meaning that only the translated character is valid.
         */
        public const int KEY_NONE = 0x00;

        public const int KEY_ESCAPE = 0x01;
        public const int KEY_1 = 0x02;
        public const int KEY_2 = 0x03;
        public const int KEY_3 = 0x04;
        public const int KEY_4 = 0x05;
        public const int KEY_5 = 0x06;
        public const int KEY_6 = 0x07;
        public const int KEY_7 = 0x08;
        public const int KEY_8 = 0x09;
        public const int KEY_9 = 0x0A;
        public const int KEY_0 = 0x0B;
        public const int KEY_MINUS = 0x0C; /* - on main keyboard */
        public const int KEY_EQUALS = 0x0D;
        public const int KEY_BACK = 0x0E; /* backspace */
        public const int KEY_TAB = 0x0F;
        public const int KEY_Q = 0x10;
        public const int KEY_W = 0x11;
        public const int KEY_E = 0x12;
        public const int KEY_R = 0x13;
        public const int KEY_T = 0x14;
        public const int KEY_Y = 0x15;
        public const int KEY_U = 0x16;
        public const int KEY_I = 0x17;
        public const int KEY_O = 0x18;
        public const int KEY_P = 0x19;
        public const int KEY_LBRACKET = 0x1A;
        public const int KEY_RBRACKET = 0x1B;
        public const int KEY_RETURN = 0x1C; /* Enter on main keyboard */
        public const int KEY_LCONTROL = 0x1D;
        public const int KEY_A = 0x1E;
        public const int KEY_S = 0x1F;
        public const int KEY_D = 0x20;
        public const int KEY_F = 0x21;
        public const int KEY_G = 0x22;
        public const int KEY_H = 0x23;
        public const int KEY_J = 0x24;
        public const int KEY_K = 0x25;
        public const int KEY_L = 0x26;
        public const int KEY_SEMICOLON = 0x27;
        public const int KEY_APOSTROPHE = 0x28;
        public const int KEY_GRAVE = 0x29; /* accent grave */
        public const int KEY_LSHIFT = 0x2A;
        public const int KEY_BACKSLASH = 0x2B;
        public const int KEY_Z = 0x2C;
        public const int KEY_X = 0x2D;
        public const int KEY_C = 0x2E;
        public const int KEY_V = 0x2F;
        public const int KEY_B = 0x30;
        public const int KEY_N = 0x31;
        public const int KEY_M = 0x32;
        public const int KEY_COMMA = 0x33;
        public const int KEY_PERIOD = 0x34; /* . on main keyboard */
        public const int KEY_SLASH = 0x35; /* / on main keyboard */
        public const int KEY_RSHIFT = 0x36;
        public const int KEY_MULTIPLY = 0x37; /* * on numeric keypad */
        public const int KEY_LMENU = 0x38; /* left Alt */
        public const int KEY_SPACE = 0x39;
        public const int KEY_CAPITAL = 0x3A;
        public const int KEY_F1 = 0x3B;
        public const int KEY_F2 = 0x3C;
        public const int KEY_F3 = 0x3D;
        public const int KEY_F4 = 0x3E;
        public const int KEY_F5 = 0x3F;
        public const int KEY_F6 = 0x40;
        public const int KEY_F7 = 0x41;
        public const int KEY_F8 = 0x42;
        public const int KEY_F9 = 0x43;
        public const int KEY_F10 = 0x44;
        public const int KEY_NUMLOCK = 0x45;
        public const int KEY_SCROLL = 0x46; /* Scroll Lock */
        public const int KEY_NUMPAD7 = 0x47;
        public const int KEY_NUMPAD8 = 0x48;
        public const int KEY_NUMPAD9 = 0x49;
        public const int KEY_SUBTRACT = 0x4A; /* - on numeric keypad */
        public const int KEY_NUMPAD4 = 0x4B;
        public const int KEY_NUMPAD5 = 0x4C;
        public const int KEY_NUMPAD6 = 0x4D;
        public const int KEY_ADD = 0x4E; /* + on numeric keypad */
        public const int KEY_NUMPAD1 = 0x4F;
        public const int KEY_NUMPAD2 = 0x50;
        public const int KEY_NUMPAD3 = 0x51;
        public const int KEY_NUMPAD0 = 0x52;
        public const int KEY_DECIMAL = 0x53; /* . on numeric keypad */
        public const int KEY_F11 = 0x57;
        public const int KEY_F12 = 0x58;
        public const int KEY_F13 = 0x64; /*                     (NEC PC98) */
        public const int KEY_F14 = 0x65; /*                     (NEC PC98) */
        public const int KEY_F15 = 0x66; /*                     (NEC PC98) */
        public const int KEY_KANA = 0x70; /* (Japanese keyboard)            */
        public const int KEY_CONVERT = 0x79; /* (Japanese keyboard)            */
        public const int KEY_NOCONVERT = 0x7B; /* (Japanese keyboard)            */
        public const int KEY_YEN = 0x7D; /* (Japanese keyboard)            */
        public const int KEY_NUMPADEQUALS = 0x8D; /* = on numeric keypad (NEC PC98) */
        public const int KEY_CIRCUMFLEX = 0x90; /* (Japanese keyboard)            */
        public const int KEY_AT = 0x91; /*                     (NEC PC98) */
        public const int KEY_COLON = 0x92; /*                     (NEC PC98) */
        public const int KEY_UNDERLINE = 0x93; /*                     (NEC PC98) */
        public const int KEY_KANJI = 0x94; /* (Japanese keyboard)            */
        public const int KEY_STOP = 0x95; /*                     (NEC PC98) */
        public const int KEY_AX = 0x96; /*                     (Japan AX) */
        public const int KEY_UNLABELED = 0x97; /*                        (J3100) */
        public const int KEY_NUMPADENTER = 0x9C; /* Enter on numeric keypad */
        public const int KEY_RCONTROL = 0x9D;
        public const int KEY_NUMPADCOMMA = 0xB3; /* , on numeric keypad (NEC PC98) */
        public const int KEY_DIVIDE = 0xB5; /* / on numeric keypad */
        public const int KEY_SYSRQ = 0xB7;
        public const int KEY_RMENU = 0xB8; /* right Alt */
        public const int KEY_PAUSE = 0xC5; /* Pause */
        public const int KEY_HOME = 0xC7; /* Home on arrow keypad */
        public const int KEY_UP = 0xC8; /* UpArrow on arrow keypad */
        public const int KEY_PRIOR = 0xC9; /* PgUp on arrow keypad */
        public const int KEY_LEFT = 0xCB; /* LeftArrow on arrow keypad */
        public const int KEY_RIGHT = 0xCD; /* RightArrow on arrow keypad */
        public const int KEY_END = 0xCF; /* End on arrow keypad */
        public const int KEY_DOWN = 0xD0; /* DownArrow on arrow keypad */
        public const int KEY_NEXT = 0xD1; /* PgDn on arrow keypad */
        public const int KEY_INSERT = 0xD2; /* Insert on arrow keypad */
        public const int KEY_DELETE = 0xD3; /* Delete on arrow keypad */
        public const int KEY_LMETA = 0xDB; /* Left Windows/Option key */
        public const int KEY_RMETA = 0xDC; /* Right Windows/Option key */
        public const int KEY_APPS = 0xDD; /* AppMenu key */
        public const int KEY_POWER = 0xDE;
        public const int KEY_SLEEP = 0xDF;

        internal EventType type;
        internal int mouseX;
        internal int mouseY;
        internal int mouseWheelDelta;
        internal int mouseButton;
        internal int mouseClickCount;
        internal bool dragEvent;
        internal bool keyRepeated;
        internal char keyChar;
        internal int keyCode;
        internal int modifier;
        private Event subEvent;

        internal Event()
        {
        }

        /**
         * Returns the type of the event.
         * @return the type of the event.
         */
        public EventType getEventType()
        {
            return type;
        }

        /**
         * Returns true for all MOUSE_* event types.
         * @return true if this is a mouse event.
         */
        public bool isMouseEvent()
        {
            return type.isMouseEvent;
        }

        /**
         * Returns true for all MOUSE_* event types except MOUSE_WHEEL.
         * @return true if this is a mouse event but not a mouse wheel event.
         */
        public bool isMouseEventNoWheel()
        {
            return type.isMouseEvent && type != EventType.MOUSE_WHEEL;
        }

        /**
         * Returns true for all KEY_* event types.
         * @return true if this is a key event.
         */
        public bool isKeyEvent()
        {
            return type.isKeyEvent;
        }

        /**
         * Returns true for the KEY_PRESSED event type.
         * @return true if this is key pressed event.
         */
        public bool isKeyPressedEvent()
        {
            return type == EventType.KEY_PRESSED;
        }

        /**
         * Returns true if this event is part of a drag operation
         * @return true if this event is part of a drag operation
         */
        public bool isMouseDragEvent()
        {
            return dragEvent;
        }

        /**
         * Returns true if this event ends a drag operation
         * @return true if this event ends a drag operation
         */
        public bool isMouseDragEnd()
        {
            return (modifier & MODIFIER_BUTTON) == 0;
        }

        /**
         * Returns the current absolute mouse X coordinate
         * @return the current absolute mouse X coordinate
         */
        public int getMouseX()
        {
            return mouseX;
        }

        /**
         * Returns the current absolute mouse Y coordinate
         * @return the current absolute mouse Y coordinate
         */
        public int getMouseY()
        {
            return mouseY;
        }

        /**
         * The mouse button. Only valid for MOUSE_BTNDOWN or MOUSE_BTNUP events
         * @return the mouse button
         * @see EventType#MOUSE_BTNDOWN
         * @see EventType#MOUSE_BTNUP
         * @see #MOUSE_LBUTTON
         * @see #MOUSE_RBUTTON
         * @see #MOUSE_MBUTTON
         */
        public int getMouseButton()
        {
            return mouseButton;
        }

        /**
         * The mouse wheel delta. Only valid for MOUSE_WHEEL events
         * @return the mouse wheel delta
         * @see EventType#MOUSE_WHEEL
         */
        public int getMouseWheelDelta()
        {
            return mouseWheelDelta;
        }

        /**
         * The mouse click count. Only valid for MOUSE_CLICKED events
         * @return the mouse click count
         * @see EventType#MOUSE_CLICKED
         */
        public int getMouseClickCount()
        {
            return mouseClickCount;
        }

        /**
         * Returns the key code. Only valid for KEY_PRESSED or KEY_RELEASED events
         * @return the key code (one of the KEY_* constants)
         */
        public int getKeyCode()
        {
            return keyCode;
        }

        /**
         * Returns the key character. Only valid if hasKeyChar() returns true.
         * @see #hasKeyChar()
         * @return the key character
         */
        public char getKeyChar()
        {
            return keyChar;
        }

        /**
         * Checks if a character is available for theis KEY_PRESSED event
         * @see #getKeyChar()
         * @return true if a character is available
         */
        public bool hasKeyChar()
        {
            return type == EventType.KEY_PRESSED && keyChar != CHAR_NONE;
        }

        /**
         * Checks if a characters is available and no keyboard modifiers are
         * active (except these needed to generate that character).
         * 
         * @return true if it's a character without additional modifiers
         */
        public bool hasKeyCharNoModifiers()
        {
            int MODIFIER_ALTGR = MODIFIER_LCTRL | MODIFIER_RALT;
            return hasKeyChar() && (
                    ((modifier & ~MODIFIER_SHIFT) == 0) ||
                    ((modifier & ~MODIFIER_ALTGR) == 0));
        }

        /**
         * Returns true if this is a repeated KEY_PRESSED event
         * @return true if this is a repeated KEY_PRESSED event
         */
        public bool isKeyRepeated()
        {
            return type == EventType.KEY_PRESSED && keyRepeated;
        }

        /**
         * Returns the current event modifiers
         * @return the current event modifiers
         */
        public int getModifiers()
        {
            return modifier;
        }

        internal Event createSubEvent(EventType newEventType)
        {
            if (subEvent == null)
            {
                subEvent = new Event();
            }
            subEvent.type = newEventType;
            subEvent.mouseX = mouseX;
            subEvent.mouseY = mouseY;
            subEvent.mouseButton = mouseButton;
            subEvent.mouseWheelDelta = mouseWheelDelta;
            subEvent.mouseClickCount = mouseClickCount;
            subEvent.dragEvent = dragEvent;
            subEvent.keyRepeated = keyRepeated;
            subEvent.keyChar = keyChar;
            subEvent.keyCode = keyCode;
            subEvent.modifier = modifier;
            return subEvent;
        }

        internal Event createSubEvent(int x, int y)
        {
            Event e = createSubEvent(type);
            e.mouseX = x;
            e.mouseY = y;
            return e;
        }

        internal void setModifier(int mask, bool pressed)
        {
            if (pressed)
            {
                modifier |= mask;
            }
            else
            {
                modifier &= ~mask;
            }
        }

        internal void setModifiers(bool pressed)
        {
            int mask;
            switch (keyCode)
            {
                case KEY_LSHIFT: mask = MODIFIER_LSHIFT; break;
                case KEY_LMETA: mask = MODIFIER_LMETA; break;
                case KEY_LCONTROL: mask = MODIFIER_LCTRL; break;
                case KEY_LMENU: mask = MODIFIER_LALT; break;
                case KEY_RSHIFT: mask = MODIFIER_RSHIFT; break;
                case KEY_RMETA: mask = MODIFIER_RMETA; break;
                case KEY_RCONTROL: mask = MODIFIER_RCTRL; break;
                case KEY_RMENU: mask = MODIFIER_RALT; break;
                default: return;
            }
            setModifier(mask, pressed);
        }

        private static String[] KEY_NAMES = new String[256];
        private static Dictionary<String, int> KEY_MAP = new Dictionary<String, int>(256);

        static Event()
        {
            try
            {
                foreach (FieldInfo f in typeof(Event).GetFields())
                {
                    String name = f.Name;
                    if (name.StartsWith("KEY_"))
                    {
                        int code = (int)f.GetValue(null);
                        name = name.Substring(4);
                        KEY_NAMES[code] = name;
                        KEY_MAP.Add(name, code);
                    }
                }
            }
            catch (Exception ignore)
            {
            }
        }

        /**
         * Returns the name for the given key code or null if the key code is not assigned.
         * @param key the key code.
         * @return the name of the key or null.
         */
        public static String getKeyNameForCode(int key)
        {
            if (key >= 0 && key < 256)
            {
                return KEY_NAMES[key];
            }
            return null;
        }

        /**
         * Returns the key code for the given key name.
         * The key name must match one of the KEY_* fields of this class.
         *
         * @param name the key name
         * @return the key code or KEY_NONE
         */
        public static int getKeyCodeForName(String name)
        {
            if (!KEY_MAP.ContainsKey(name))
            {
                return KEY_NONE;
            }

            return KEY_MAP[name];
        }
    }
}
