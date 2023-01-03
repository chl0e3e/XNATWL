using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Model;

namespace XNATWL
{
    public class ToggleButton : Button
    {
        public ToggleButton() : base(new ToggleButtonModel())
        {
        }

        public ToggleButton(BooleanModel model) : base(new ToggleButtonModel(model))
        {
        }

        public ToggleButton(String text) : this()
        {
            setText(text);
        }

        public void setModel(BooleanModel model)
        {
            ((ToggleButtonModel)getModel()).setModel(model);
        }

        public bool isActive()
        {
            return getModel().Selected;
        }

        public void setActive(bool active)
        {
            getModel().Selected = active;
        }
    }

}
