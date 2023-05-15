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
        private int _numAnimSteps = 10;
        private int _currentStep;
        private int _animSpeed;

        private BooleanModel _model;
        public event EventHandler<AnimatedWindowOpenCloseEventArgs> OpenClose;

        public AnimatedWindow()
        {
            SetVisible(false); // we start closed
        }

        public int GetNumAnimSteps()
        {
            return _numAnimSteps;
        }

        public void SetNumAnimSteps(int numAnimSteps)
        {
            if (numAnimSteps < 1)
            {
                throw new ArgumentOutOfRangeException("numAnimSteps");
            }

            this._numAnimSteps = numAnimSteps;
        }

        public void SetState(bool open)
        {
            if (open && !IsOpen())
            {
                _animSpeed = 1;
                SetVisible(true);
                this.OpenClose.Invoke(this, new AnimatedWindowOpenCloseEventArgs());
            }
            else if (!open && !IsClosed())
            {
                _animSpeed = -1;
                this.OpenClose.Invoke(this, new AnimatedWindowOpenCloseEventArgs());
            }

            if (_model != null)
            {
                _model.Value = open;
            }
        }

        public BooleanModel GetModel()
        {
            return _model;
        }

        public void SetModel(BooleanModel model)
        {
            if (this._model != model)
            {
                if (this._model != null)
                {
                    this._model.Changed -= Model_Changed;
                }
                this._model = model;
                if (model != null)
                {
                    this._model.Changed += Model_Changed;
                    SyncWithModel();
                }
            }
        }

        private void Model_Changed(object sender, BooleanChangedEventArgs e)
        {
            SyncWithModel();
        }

        public bool IsOpen()
        {
            return _currentStep == _numAnimSteps && _animSpeed >= 0;
        }

        public bool IsOpening()
        {
            return _animSpeed > 0;
        }

        public bool IsClosed()
        {
            return _currentStep == 0 && _animSpeed <= 0;
        }

        public bool IsClosing()
        {
            return _animSpeed < 0;
        }

        public bool IsAnimating()
        {
            return _animSpeed != 0;
        }

        public override bool HandleEvent(Event evt)
        {
            if (IsOpen())
            {
                if (base.HandleEvent(evt))
                {
                    return true;
                }

                if (evt.IsKeyPressedEvent())
                {
                    switch (evt.GetKeyCode())
                    {
                        case Event.KEY_ESCAPE:
                            SetState(false);
                            return true;
                        default:
                            break;
                    }
                }

                return false;
            }

            if (IsClosed())
            {
                return false;
            }

            // eat every event when we animate
            int mouseX = evt.GetMouseX() - GetX();
            int mouseY = evt.GetMouseY() - GetY();
            return mouseX >= 0 && mouseX < GetAnimatedWidth() &&
                    mouseY >= 0 && mouseY < GetAnimatedHeight();
        }

        public override int GetMinWidth()
        {
            int minWidth = 0;
            for (int i = 0, n = GetNumChildren(); i < n; i++)
            {
                Widget child = GetChild(i);
                minWidth = Math.Max(minWidth, child.GetMinWidth());
            }
            return Math.Max(base.GetMinWidth(), minWidth + GetBorderHorizontal());
        }

        public override int GetMinHeight()
        {
            int minHeight = 0;
            for (int i = 0, n = GetNumChildren(); i < n; i++)
            {
                Widget child = GetChild(i);
                minHeight = Math.Max(minHeight, child.GetMinHeight());
            }
            return Math.Max(base.GetMinHeight(), minHeight + GetBorderVertical());
        }

        public override int GetPreferredInnerWidth()
        {
            return BoxLayout.ComputePreferredWidthVertical(this);
        }

        public override int GetPreferredInnerHeight()
        {
            return BoxLayout.ComputePreferredHeightHorizontal(this);
        }

        protected override void Layout()
        {
            LayoutChildrenFullInnerArea();
        }

        protected override void Paint(GUI gui)
        {
            if (_animSpeed != 0)
            {
                Animate();
            }

            if (IsOpen())
            {
                base.Paint(gui);
            }
            else if (!IsClosed() && GetBackground() != null)
            {
                GetBackground().Draw(GetAnimationState(),
                        GetX(), GetY(), GetAnimatedWidth(), GetAnimatedHeight());
            }
        }

        private void Animate()
        {
            _currentStep += _animSpeed;
            if (_currentStep == 0 || _currentStep == _numAnimSteps)
            {
                SetVisible(_currentStep > 0);
                _animSpeed = 0;
                this.OpenClose.Invoke(this, new AnimatedWindowOpenCloseEventArgs());
            }
        }

        private int GetAnimatedWidth()
        {
            return GetWidth() * _currentStep / _numAnimSteps;
        }

        private int GetAnimatedHeight()
        {
            return GetHeight() * _currentStep / _numAnimSteps;
        }

        void SyncWithModel()
        {
            SetState(_model.Value);
        }
    }

    public class AnimatedWindowOpenCloseEventArgs : EventArgs
    {
    }
}
