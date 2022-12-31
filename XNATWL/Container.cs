using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL
{
    public class Container : Widget
    {
        public override int getMinWidth() {
            return Math.Max(base.getMinWidth(), getBorderHorizontal() +
                    BoxLayout.computeMinWidthVertical(this));
        }

        public override int getMinHeight() {
            return Math.Max(base.getMinHeight(), getBorderVertical() +
                    BoxLayout.computeMinHeightHorizontal(this));
        }

        public override int getPreferredInnerWidth() {
            return BoxLayout.computePreferredWidthVertical(this);
        }

        public override int getPreferredInnerHeight() {
            return BoxLayout.computePreferredHeightHorizontal(this);
        }

        protected override void layout() {
            layoutChildrenFullInnerArea();
        }
    }
}
