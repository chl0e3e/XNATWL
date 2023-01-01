using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL
{
    public class DesktopArea : Widget
    {
        public DesktopArea() {
            setFocusKeyEnabled(false);
        }

        protected override void keyboardFocusChildChanged(Widget child) {
            base.keyboardFocusChildChanged(child);
            if(child != null) {
                int fromIdx = getChildIndex(child);
                System.Diagnostics.Debug.Assert(fromIdx >= 0);
                int numChildren = getNumChildren();
                if(fromIdx < numChildren - 1) {
                    moveChild(fromIdx, numChildren - 1);
                }
            }
        }

        protected override void layout() {
            // make sure that all children are still inside
            restrictChildrenToInnerArea();
        }

        protected void restrictChildrenToInnerArea() {
            int top = getInnerY();
            int left = getInnerX();
            int right = getInnerRight();
            int bottom = getInnerBottom();
            int width = Math.Max(0, right-left);
            int height = Math.Max(0, bottom-top);

            for(int i=0,n=getNumChildren() ; i<n ; i++) {
                Widget w = getChild(i);
                w.setSize(
                        Math.Min(Math.Max(width, w.getMinWidth()), w.getWidth()),
                        Math.Min(Math.Max(height, w.getMinHeight()), w.getHeight()));
                w.setPosition(
                        Math.Max(left, Math.Min(right - w.getWidth(), w.getX())),
                        Math.Max(top, Math.Min(bottom - w.getHeight(), w.getY())));
            }
        }

    }
}
