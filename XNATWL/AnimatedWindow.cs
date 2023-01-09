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

using System;
using XNATWL.Model;

namespace XNATWL
{
    public class AnimatedWindow : Widget
    {
        private int numAnimSteps = 10;
        private int currentStep;
        private int animSpeed;

        private BooleanModel model;
        public event EventHandler<AnimatedWindowOpenCloseEventArgs> OpenClose;

        public AnimatedWindow()
        {
            setVisible(false); // we start closed
        }

        public int getNumAnimSteps()
        {
            return numAnimSteps;
        }

        public void setNumAnimSteps(int numAnimSteps)
        {
            if (numAnimSteps < 1)
            {
                throw new ArgumentOutOfRangeException("numAnimSteps");
            }

            this.numAnimSteps = numAnimSteps;
        }

        public void setState(bool open)
        {
            if (open && !isOpen())
            {
                animSpeed = 1;
                setVisible(true);
                this.OpenClose.Invoke(this, new AnimatedWindowOpenCloseEventArgs());
            }
            else if (!open && !isClosed())
            {
                animSpeed = -1;
                this.OpenClose.Invoke(this, new AnimatedWindowOpenCloseEventArgs());
            }

            if (model != null)
            {
                model.Value = open;
            }
        }

        public BooleanModel getModel()
        {
            return model;
        }

        public void setModel(BooleanModel model)
        {
            if (this.model != model)
            {
                if (this.model != null)
                {
                    this.model.Changed -= Model_Changed;
                }
                this.model = model;
                if (model != null)
                {
                    this.model.Changed += Model_Changed;
                    syncWithModel();
                }
            }
        }

        private void Model_Changed(object sender, BooleanChangedEventArgs e)
        {
            syncWithModel();
        }

        public bool isOpen()
        {
            return currentStep == numAnimSteps && animSpeed >= 0;
        }

        public bool isOpening()
        {
            return animSpeed > 0;
        }

        public bool isClosed()
        {
            return currentStep == 0 && animSpeed <= 0;
        }

        public bool isClosing()
        {
            return animSpeed < 0;
        }

        public bool isAnimating()
        {
            return animSpeed != 0;
        }

        //@Override
        public override bool handleEvent(Event evt)
        {
            if (isOpen())
            {
                if (base.handleEvent(evt))
                {
                    return true;
                }

                if (evt.isKeyPressedEvent())
                {
                    switch (evt.getKeyCode())
                    {
                        case Event.KEY_ESCAPE:
                            setState(false);
                            return true;
                        default:
                            break;
                    }
                }

                return false;
            }

            if (isClosed())
            {
                return false;
            }

            // eat every event when we animate
            int mouseX = evt.getMouseX() - getX();
            int mouseY = evt.getMouseY() - getY();
            return mouseX >= 0 && mouseX < getAnimatedWidth() &&
                    mouseY >= 0 && mouseY < getAnimatedHeight();
        }

        //@Override
        public override int getMinWidth()
        {
            int minWidth = 0;
            for (int i = 0, n = getNumChildren(); i < n; i++)
            {
                Widget child = getChild(i);
                minWidth = Math.Max(minWidth, child.getMinWidth());
            }
            return Math.Max(base.getMinWidth(), minWidth + getBorderHorizontal());
        }

        public override int getMinHeight()
        {
            int minHeight = 0;
            for (int i = 0, n = getNumChildren(); i < n; i++)
            {
                Widget child = getChild(i);
                minHeight = Math.Max(minHeight, child.getMinHeight());
            }
            return Math.Max(base.getMinHeight(), minHeight + getBorderVertical());
        }

        public override int getPreferredInnerWidth()
        {
            return BoxLayout.computePreferredWidthVertical(this);
        }

        public override int getPreferredInnerHeight()
        {
            return BoxLayout.computePreferredHeightHorizontal(this);
        }

        protected override void layout()
        {
            layoutChildrenFullInnerArea();
        }

        protected override void paint(GUI gui)
        {
            if (animSpeed != 0)
            {
                animate();
            }

            if (isOpen())
            {
                base.paint(gui);
            }
            else if (!isClosed() && getBackground() != null)
            {
                getBackground().Draw(getAnimationState(),
                        getX(), getY(), getAnimatedWidth(), getAnimatedHeight());
            }
        }

        private void animate()
        {
            currentStep += animSpeed;
            if (currentStep == 0 || currentStep == numAnimSteps)
            {
                setVisible(currentStep > 0);
                animSpeed = 0;
                this.OpenClose.Invoke(this, new AnimatedWindowOpenCloseEventArgs());
            }
        }

        private int getAnimatedWidth()
        {
            return getWidth() * currentStep / numAnimSteps;
        }

        private int getAnimatedHeight()
        {
            return getHeight() * currentStep / numAnimSteps;
        }

        void syncWithModel()
        {
            setState(model.Value);
        }
    }

    public class AnimatedWindowOpenCloseEventArgs : EventArgs
    {
    }
}
