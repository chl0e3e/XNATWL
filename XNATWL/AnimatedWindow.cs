using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Model;
using XNATWL.Utils;

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
