using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.TextArea;

namespace XNATWL.Input.XNA
{
    public class XNAInput : Input
    {
        private bool wasActive;

        public bool PollInput(GUI gui)
        {
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
