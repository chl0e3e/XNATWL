using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using XNATWL.IO;

namespace XNATWL.Input.XNA
{
    /// <summary>
    /// Class which uses XNA to poll for input
    /// </summary>
    public class XNAInput : Input
    {
        // declared in constructor
        private KeyboardLayout _keyboardLayout;
        private ConsumingMouseState _consumingMouseState;
        private List<Keys> _pressedKeys;

        /// <summary>
        /// Creates a new instance of XNAInput which polls new inputs similarly to the LWJGL code for TWL
        /// </summary>
        public XNAInput()
        {
            int keyboardLayoutId = CultureInfo.CurrentUICulture.KeyboardLayoutId;
            string keyboardLayoutPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "KeyboardLayouts", keyboardLayoutId + ".xml");
            if (!System.IO.File.Exists(keyboardLayoutPath))
            {
                keyboardLayoutPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "KeyboardLayouts", "1033.xml");
            }
            FileSystemObject kbdLayoutFso = new FileSystemObject(FileSystemObject.FileSystemObjectType.File, keyboardLayoutPath);

            this._keyboardLayout = new KeyboardLayout(kbdLayoutFso);
            this._pressedKeys = new List<Keys>();
            this._consumingMouseState = new ConsumingMouseState();
        }

        /// <summary>
        /// Poll both the keyboard and the mouse using Microsoft.Xna.Framework.Input
        /// </summary>
        /// <param name="gui">TWL GUI to send key/mouse events</param>
        /// <returns><b>true</b> if successful</returns>
        public bool PollInput(GUI gui)
        {
            return PollKeyboard(gui) && PollMouse(gui);
        }

        /// <summary>
        /// Poll the keyboard using Microsoft.Xna.Framework.Input
        /// </summary>
        /// <param name="gui">TWL GUI to send key events</param>
        /// <returns><b>true</b> if successful</returns>
        public bool PollKeyboard(GUI gui)
        {
            KeyboardState ks = Keyboard.GetState();

            // build up a list of keys that have newly been pressed
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
                    // mark it as pressed to stop key repeating
                    _pressedKeys.Add(key);

                    // fire handle key once when it is detected as pressed
                    KeyInfo keyInfo = this._keyboardLayout.KeyInfoFor(key);
                    gui.HandleKey(keyInfo.TWL, shiftPressed ? keyInfo.ShiftChar : keyInfo.Char, true);
                }
            }

            // if its not pressed now, add to a queue to remove it later when handling the key
            List<Keys> keyToRemove = new List<Keys>();
            foreach (Keys key in _pressedKeys)
            {
                if (!newPressedKeys.Contains(key))
                {
                    keyToRemove.Add(key);
                }
            }

            // when removing a key, fire the event for the key marked as not pressed
            for (int i = 0; i < keyToRemove.Count; i++)
            {
                KeyInfo keyInfo = this._keyboardLayout.KeyInfoFor(keyToRemove[i]);
                gui.HandleKey(keyInfo.TWL, shiftPressed ? keyInfo.ShiftChar : keyInfo.Char, false);
                _pressedKeys.Remove(keyToRemove[i]);
            }

            return true;
        }

        /// <summary>
        /// Poll the mouse using Microsoft.Xna.Framework.Input
        /// </summary>
        /// <param name="gui">TWL GUI to send mouse events</param>
        /// <returns><b>true</b> if successful</returns>
        public bool PollMouse(GUI gui)
        {
            // poll the latest mouse state from XNA
            MouseState ms = Mouse.GetState();
            // consume the mouse state accumulating the latest poll with the previous one
            CMSAction[] consumedActions = this._consumingMouseState.Consume(ms);

            bool buzzed = false;

            // if anything changed about the left mouse button, and if it was pressed while changing position
            if (consumedActions.Contains(CMSAction.LeftChanged) || consumedActions.Contains(CMSAction.XChangedLeft) || consumedActions.Contains(CMSAction.YChangedLeft))
            {
                gui.HandleMouse(this._consumingMouseState.X, this._consumingMouseState.Y, Event.MOUSE_LBUTTON, this._consumingMouseState.Left == ButtonState.Pressed);
                buzzed = true;
            }

            // if anything changed about the right mouse button, and if it was pressed while changing position
            if (consumedActions.Contains(CMSAction.RightChanged) || consumedActions.Contains(CMSAction.XChangedRight) || consumedActions.Contains(CMSAction.YChangedRight))
            {
                gui.HandleMouse(this._consumingMouseState.X, this._consumingMouseState.Y, Event.MOUSE_RBUTTON, this._consumingMouseState.Right == ButtonState.Pressed);
                buzzed = true;
            }

            // if anything changed about the middle mouse button, and if it was pressed while changing position
            if (consumedActions.Contains(CMSAction.MiddleChanged) || consumedActions.Contains(CMSAction.XChangedMiddle) || consumedActions.Contains(CMSAction.YChangedMiddle))
            {
                gui.HandleMouse(this._consumingMouseState.X, this._consumingMouseState.Y, Event.MOUSE_MBUTTON, this._consumingMouseState.Middle == ButtonState.Pressed);
                buzzed = true;
            }

            // if the scroll delta was changed
            if (consumedActions.Contains(CMSAction.Scroll))
            {
                gui.HandleMouseWheel(this._consumingMouseState.ScrollDelta);
            }

            // if the X or Y of the mouse changed but no other HandleMouse event was fired
            if (!buzzed && (consumedActions.Contains(CMSAction.XChanged) || consumedActions.Contains(CMSAction.YChanged)))
            {
                gui.HandleMouse(this._consumingMouseState.X, this._consumingMouseState.Y, -1, false);
            }

            return true;
        }
    }
}
