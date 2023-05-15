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

        public void Show()
        {
            SetVisible(true);
            RequestKeyboardFocus();
        }

        public void Hide()
        {
            if (IsVisible() && GetFadeDurationHide() > 0)
            {
                //MinimizeEffect minimizeEffect = new MinimizeEffect(this);
                //minimizeEffect.setAnimationDuration(getFadeDurationHide());
                //setRenderOffscreen(minimizeEffect);
            }
            SetVisible(false);
        }

        public void Center(float relX, float relY)
        {
            Widget p = GetParent();
            SetPosition(
                    p.GetInnerX() + (int)((p.GetInnerWidth() - GetWidth()) * relX),
                    p.GetInnerY() + (int)((p.GetInnerHeight() - GetHeight()) * relY));
        }

        public void AddCloseCallback()
        {
            base.Closed += (sender, e) =>
            {
                Hide();
            };
        }
    }

}
