using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static XNATWL.AnimationState;
using XNATWL.Utils;
using static XNATWL.TextAreaModel.StyleSheet;

namespace XNATWL.Model
{
    public class ToggleButtonModel : SimpleButtonModel
    {
        protected static int STATE_MASK_SELECTED = 256;

        private BooleanModel model;
        private Runnable modelCallback;
        private bool invertModelState;
        private bool isConnected;

        public ToggleButtonModel()
        {
        }

        public ToggleButtonModel(BooleanModel model) : this(model, false)
        {
        }

        public ToggleButtonModel(BooleanModel model, bool invertModelState)
        {
            setModel(model, invertModelState);
        }

        public override bool Selected
        {
            get
            {
                return (this._state & STATE_MASK_SELECTED) != 0;
            }

            set
            {
                if (model != null)
                {
                    model.Value = value ^ invertModelState;
                }
                else
                {
                    setSelectedState(value);
                }
            }
        }

        protected override void FireAction()
        {
            this.Selected = !this.Selected;
            base.FireAction();
        }

        public BooleanModel getModel()
        {
            return model;
        }

        public void setModel(BooleanModel model)
        {
            setModel(model, false);
        }

        public void setModel(BooleanModel model, bool invertModelState)
        {
            this.invertModelState = invertModelState;
            if (this.model != model)
            {
                removeModelCallback();
                this.model = model;
                addModelCallback();
            }
            if (model != null)
            {
                syncWithModel();
            }
        }

        public bool isInvertModelState()
        {
            return invertModelState;
        }

        void syncWithModel()
        {
            setSelectedState(model.Value ^ invertModelState);
        }

        /*public override void connect()
        {
            isConnected = true;
            addModelCallback();
        }

        public override void disconnect()
        {
            isConnected = false;
            removeModelCallback();
        }*/

        private void addModelCallback()
        {
            if (model != null && isConnected)
            {
                model.Changed += Model_Changed;
                syncWithModel();
            }
        }

        private void Model_Changed(object sender, BooleanChangedEventArgs e)
        {
            syncWithModel();
        }

        private void removeModelCallback()
        {
            if (model != null)
            {
                model.Changed -= Model_Changed;
            }
        }

        private void setSelectedState(bool selected)
        {
            if (selected != this.Selected)
            {
                this.SetStateBit(STATE_MASK_SELECTED, selected);
                this.FireState();
            }
        }
    }
}
