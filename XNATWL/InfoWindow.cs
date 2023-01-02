using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL
{
    public class InfoWindow : Container {

        private Widget owner;

        public InfoWindow(Widget owner) {
            if(owner == null) {
                throw new NullReferenceException("owner");
            }
        
            this.owner = owner;
        }

        public Widget getOwner() {
            return owner;
        }

        public bool isOpen() {
            return getParent() != null;
        }
    
        public bool openInfo() {
            if(getParent() != null) {
                return true;
            }
            if(isParentInfoWindow(owner)) {
                return false;
            }
            GUI gui = owner.getGUI();
            if(gui != null) {
                gui.openInfo(this);
                focusFirstChild();
                return true;
            }
            return false;
        }

        public void closeInfo() {
            GUI gui = getGUI();
            if(gui != null) {
                gui.closeInfo(this);
            }
        }

        /**
         * Called after the info window has been closed
         */
        protected internal virtual void infoWindowClosed() {
        }

        private static bool isParentInfoWindow(Widget w) {
            while(w != null) {
                if(w is InfoWindow) {
                    return true;
                }
                w = w.getParent();
            }
            return false;
        }
    }

}
