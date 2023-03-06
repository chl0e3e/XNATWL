using Microsoft.Xna.Framework;
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
        private ConsumingMouseState _consumingMouseState;
        private List<Keys> _pressedKeys;
        private bool[] _pressedMouseButtons;
        private int _counter;

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
            this._counter = 0;
            this._consumingMouseState = new ConsumingMouseState();
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

            CMSAction[] consumedActions = this._consumingMouseState.Consume(ms);

            if (consumedActions.Length > 0)
            {
                System.Diagnostics.Debug.WriteLine("=====");
                System.Diagnostics.Debug.WriteLine(this._counter);
                System.Diagnostics.Debug.WriteLine(this._consumingMouseState.Left);
                foreach (CMSAction cmsAction in consumedActions)
                {
                    System.Diagnostics.Debug.WriteLine(cmsAction);
                }
            }

            if (consumedActions.Contains(CMSAction.LeftChanged) || consumedActions.Contains(CMSAction.XChangedLeft) || consumedActions.Contains(CMSAction.YChangedLeft))
            {
                gui.handleMouse(this._consumingMouseState.X, this._consumingMouseState.Y, Event.MOUSE_LBUTTON, this._consumingMouseState.Left == ButtonState.Pressed);
            }

            if (consumedActions.Contains(CMSAction.RightChanged) || consumedActions.Contains(CMSAction.XChangedRight) || consumedActions.Contains(CMSAction.YChangedRight))
            {
                gui.handleMouse(this._consumingMouseState.X, this._consumingMouseState.Y, Event.MOUSE_RBUTTON, this._consumingMouseState.Right == ButtonState.Pressed);
            }

            if (consumedActions.Contains(CMSAction.MiddleChanged) || consumedActions.Contains(CMSAction.XChangedMiddle) || consumedActions.Contains(CMSAction.YChangedMiddle))
            {
                gui.handleMouse(this._consumingMouseState.X, this._consumingMouseState.Y, Event.MOUSE_MBUTTON, this._consumingMouseState.Middle == ButtonState.Pressed);
            }

            if (consumedActions.Contains(CMSAction.Scroll))
            {
                gui.handleMouseWheel(this._consumingMouseState.ScrollDelta);
            }

            this._counter++;
            return true;
        }
    }
}
