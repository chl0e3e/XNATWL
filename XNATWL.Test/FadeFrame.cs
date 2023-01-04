using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Utils;

namespace XNATWL.Test
{
    public class FadeFrame : ResizableFrame
    {
        public FadeFrame()
        {
        }

        public void show()
        {
            setVisible(true);
            requestKeyboardFocus();
        }

        public void hide()
        {
            if (isVisible() && getFadeDurationHide() > 0)
            {
                //MinimizeEffect minimizeEffect = new MinimizeEffect(this);
                //minimizeEffect.setAnimationDuration(getFadeDurationHide());
                //setRenderOffscreen(minimizeEffect);
            }
            setVisible(false);
        }

        public void center(float relX, float relY)
        {
            Widget p = getParent();
            setPosition(
                    p.getInnerX() + (int)((p.getInnerWidth() - getWidth()) * relX),
                    p.getInnerY() + (int)((p.getInnerHeight() - getHeight()) * relY));
        }

        public void addCloseCallback()
        {
            base.Closed += (sender, e) =>
            {
                hide();
            };
        }
    }

}
