using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.TextAreaModel;

namespace XNATWL.Input.XNA
{
    public class XNAInput : Input
    {
        private bool wasActive;

        public bool PollInput(GUI gui)
        {
            KeyboardState ks = Keyboard.GetState();
            MouseState ms = Mouse.GetState();

            gui.handleMouse(ms.X, ms.Y, (ms.LeftButton == ButtonState.Pressed || ms.LeftButton == ButtonState.Released) ? Event.MOUSE_LBUTTON : 0, ms.LeftButton == ButtonState.Pressed ? true : false);
            foreach (Keys key in ks.GetPressedKeys())
            {
                switch(key)
                {
                    case Keys.A:
                        gui.handleKey(Event.KEY_A, 'A', true);
                        break;
                }
            }
            /*if (Keyboard.GetState)
            {
                while (Keyboard.next())
                {
                    gui.handleKey(
                            Keyboard.getEventKey(),
                            Keyboard.getEventCharacter(),
                            Keyboard.getEventKeyState());
                }
            }
            if (Mouse.isCreated())
            {
                while (Mouse.next())
                {
                    gui.handleMouse(
                            Mouse.getEventX(), gui.getHeight() - Mouse.getEventY() - 1,
                            Mouse.getEventButton(), Mouse.getEventButtonState());

                    int wheelDelta = Mouse.getEventDWheel();
                    if (wheelDelta != 0)
                    {
                        gui.handleMouseWheel(wheelDelta / 120);
                    }
                }
            }*/
            return true;
        }
    }
}
