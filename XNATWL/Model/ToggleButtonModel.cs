/*
 * Copyright (c) 2008-2012, Matthias Mann
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 *     * Redistributions of source code must retain the above copyright notice,
 *       this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of Matthias Mann nor the names of its contributors may
 *       be used to endorse or promote products derived from this software
 *       without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

namespace XNATWL.Model
{
    public class ToggleButtonModel : SimpleButtonModel
    {
        protected static int STATE_MASK_SELECTED = 256;

        private BooleanModel model;
        private bool invertModelState;
        private bool isConnected = true;

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
                //isConnected = true;
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
