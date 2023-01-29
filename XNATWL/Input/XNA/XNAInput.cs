using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XNATWL.IO;
using XNATWL.TextAreaModel;

namespace XNATWL.Input.XNA
{
    public class XNAInput : Input
    {
        // declared in constructor
        private KeyboardLayout _keyboardLayout;
        private List<Keys> _pressedKeys;
        private bool[] _pressedMouseButtons;
        private int _lastScrollWheelValue;

        public XNAInput()
        {
            int keyboardLayoutId = CultureInfo.CurrentUICulture.KeyboardLayoutId;
            string keyboardLayoutPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "KeyboardLayouts", keyboardLayoutId + ".xml");
            if (!System.IO.File.Exists(keyboardLayoutPath))
            {
                keyboardLayoutPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "KeyboardLayouts", "1033.xml");
            }
            FileSystemObject kbdLayoutFso = new FileSystemObject(FileSystemObject.FileSystemObjectType.FILE, keyboardLayoutPath);
            this._keyboardLayout = new KeyboardLayout(kbdLayoutFso);
            this._pressedMouseButtons = new bool[3] { false, false, false };
            this._pressedKeys = new List<Keys>();
            this._lastScrollWheelValue = 0;
        }

        public bool PollInput(GUI gui)
        {
            KeyboardState ks = Keyboard.GetState();
            MouseState ms = Mouse.GetState();

            Keys[] newPressedKeys = ks.GetPressedKeys();
            bool shiftPressed = newPressedKeys.Contains(Keys.LeftShift) || newPressedKeys.Contains(Keys.RightShift);
            foreach (Keys key in newPressedKeys)
            {
                if (_pressedKeys.Contains(key))
                {
                    // if its already in there, continue
                    continue;
                }
                else
                {
                    _pressedKeys.Add(key);
                    KeyInfo keyInfo = this._keyboardLayout.KeyInfoFor(key);
                    gui.handleKey(keyInfo.TWL, shiftPressed ? keyInfo.ShiftChar : keyInfo.Char, true);
                }
            }

            // if its not pressed now, remove it
            List<Keys> keyToRemove = new List<Keys>();
            foreach (Keys key in _pressedKeys)
            {
                if (!newPressedKeys.Contains(key))
                {
                    keyToRemove.Add(key);
                }
            }

            for (int i = 0; i < keyToRemove.Count; i++)
            {
                KeyInfo keyInfo = this._keyboardLayout.KeyInfoFor(keyToRemove[i]);
                gui.handleKey(keyInfo.TWL, shiftPressed ? keyInfo.ShiftChar : keyInfo.Char, false);
                _pressedKeys.Remove(keyToRemove[i]);
            }

            gui.handleMouse(ms.X, ms.Y, Event.MOUSE_LBUTTON, ms.LeftButton == ButtonState.Pressed);
            gui.handleMouse(ms.X, ms.Y, Event.MOUSE_RBUTTON, ms.RightButton == ButtonState.Pressed);
            gui.handleMouse(ms.X, ms.Y, Event.MOUSE_MBUTTON, ms.MiddleButton == ButtonState.Pressed);

            int wheelDelta = ms.ScrollWheelValue - this._lastScrollWheelValue;
            if (wheelDelta != 0)
            {
                gui.handleMouseWheel(wheelDelta / 120);
            }
            this._lastScrollWheelValue = ms.ScrollWheelValue;
            return true;
        }
    }
}
