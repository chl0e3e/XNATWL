using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Model;
using XNATWL.Property;

namespace XNATWL
{
    public class MenuCheckbox : MenuElement
    {
        private BooleanModel model;

        public MenuCheckbox()
        {
        }

        public MenuCheckbox(BooleanModel model)
        {
            this.model = model;
        }

        public MenuCheckbox(String name, BooleanModel model) : base(name)
        {
            this.model = model;
        }

        public BooleanModel getModel()
        {
            return model;
        }

        public void setModel(BooleanModel model)
        {
            BooleanModel oldModel = this.model;
            this.model = model;
            firePropertyChange("model", oldModel, model);
        }

        class MenuCheckBoxBtn : MenuBtn
        {
            private MenuCheckbox _menuCheckBox;

            public MenuCheckBoxBtn(MenuCheckbox menuCheckBox, MenuElement menuElement) : base(menuElement)
            {
                this._menuCheckBox = menuCheckBox;
            }

            public void propertyChange(PropertyChangeEvent evt)
            {
                base.propertyChange(evt);
                ((ToggleButtonModel)getModel()).setModel(this._menuCheckBox.getModel());
            }
        }

        //@Override
        protected internal override Widget createMenuWidget(MenuManager mm, int level)
        {
            MenuBtn btn = new MenuCheckBoxBtn(this, this);
            btn.setModel(new ToggleButtonModel(getModel()));
            setWidgetTheme(btn, "checkbox");
            btn.Action += (sender, e) => {
                mm.closePopup();
            };

            return btn;
        }
    }
}
