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
using XNATWL.Renderer;
using XNATWL.Utils;

namespace XNATWL
{
    public class WheelWidget<T> : Widget
    {
        public interface ItemRenderer
        {
            Widget GetRenderWidget(Object data);
        }

        private TypeMapping _itemRenderer;
        private R _renderer;

        protected int _itemHeight;
        protected int _numVisibleItems;
        protected Image _selectedOverlay;

        private static int TIMER_INTERVAL = 30;
        private static int MIN_SPEED = 3;
        private static int MAX_SPEED = 100;

        protected Timer _timer;
        protected int _dragStartY;
        protected long _lastDragTime;
        protected long _lastDragDelta;
        protected int _lastDragDist;
        protected bool _hasDragStart;
        protected bool _dragActive;
        protected int _scrollOffset;
        protected int _scrollAmount;

        protected ListModel<T> _model;
        protected IntegerModel _selectedModel;
        protected int _selected;
        protected bool _cyclic;

        public WheelWidget()
        {
            this._itemRenderer = new TypeMapping();
            this._renderer = new R(this);

            _itemRenderer.SetByType(typeof(String), new StringItemRenderer());

            base.InsertChild(_renderer, 0);
            SetCanAcceptKeyboardFocus(true);
        }

        public WheelWidget(ListModel<T> model) : this()
        {
            this._model = model;
        }

        public ListModel<T> GetModel()
        {
            return _model;
        }

        public void SetModel(ListModel<T> model)
        {
            RemoveListener();
            this._model = model;
            AddListener();
            InvalidateLayout();
        }

        public IntegerModel GetSelectedModel()
        {
            return _selectedModel;
        }

        public void SetSelectedModel(IntegerModel selectedModel)
        {
            RemoveSelectedListener();
            this._selectedModel = selectedModel;
            AddSelectedListener();
        }

        public int GetSelected()
        {
            return _selected;
        }

        public void SetSelected(int selected)
        {
            int oldSelected = this._selected;
            if (oldSelected != selected)
            {
                this._selected = selected;
                if (_selectedModel != null)
                {
                    _selectedModel.Value = selected;
                }
                FirePropertyChange("selected", oldSelected, selected);
            }
        }

        public bool IsCyclic()
        {
            return _cyclic;
        }

        public void SetCyclic(bool cyclic)
        {
            this._cyclic = cyclic;
        }

        public int GetItemHeight()
        {
            return _itemHeight;
        }

        public int GetNumVisibleItems()
        {
            return _numVisibleItems;
        }

        public bool RemoveItemRenderer(Type clazz)
        {
            if (_itemRenderer.RemoveByType(clazz))
            {
                base.RemoveAllChildren();
                InvalidateLayout();
                return true;
            }
            return false;
        }

        public void RegisterItemRenderer(Type clazz, ItemRenderer value)
        {
            _itemRenderer.SetByType(clazz, value);
            InvalidateLayout();
        }

        public void Scroll(int amount)
        {
            ScrollInt(amount);
            _scrollAmount = 0;
        }

        protected void ScrollInt(int amount)
        {
            int pos = _selected;
            int half = _itemHeight / 2;

            _scrollOffset += amount;
            while (_scrollOffset >= half)
            {
                _scrollOffset -= _itemHeight;
                pos++;
            }
            while (_scrollOffset <= -half)
            {
                _scrollOffset += _itemHeight;
                pos--;
            }

            if (!_cyclic)
            {
                int n = GetNumEntries();
                if (n > 0)
                {
                    while (pos >= n)
                    {
                        pos--;
                        _scrollOffset += _itemHeight;
                    }
                }
                while (pos < 0)
                {
                    pos++;
                    _scrollOffset -= _itemHeight;
                }
                _scrollOffset = Math.Max(-_itemHeight, Math.Min(_itemHeight, _scrollOffset));
            }

            SetSelected(pos);

            if (_scrollOffset == 0 && _scrollAmount == 0)
            {
                StopTimer();
            }
            else
            {
                StartTimer();
            }
        }

        public void AutoScroll(int dir)
        {
            if (dir != 0)
            {
                if (_scrollAmount != 0 && Math.Sign(_scrollAmount) != Math.Sign(dir))
                {
                    _scrollAmount = dir;
                }
                else
                {
                    _scrollAmount += dir;
                }
                StartTimer();
            }
        }

        public override int GetPreferredInnerHeight()
        {
            return _numVisibleItems * _itemHeight;
        }

        public override int GetPreferredInnerWidth()
        {
            int width = 0;
            for (int i = 0, n = GetNumEntries(); i < n; i++)
            {
                Widget w = GetItemRenderer(i);
                if (w != null)
                {
                    width = Math.Max(width, w.GetPreferredWidth());
                }
            }
            return width;
        }

        protected override void PaintOverlay(GUI gui)
        {
            base.PaintOverlay(gui);

            if (_selectedOverlay != null)
            {
                int y = GetInnerY() + _itemHeight * (_numVisibleItems / 2);
                if ((_numVisibleItems & 1) == 0)
                {
                    y -= _itemHeight / 2;
                }
                _selectedOverlay.Draw(GetAnimationState(), GetX(), y, GetWidth(), _itemHeight);
            }
        }

        public override bool HandleEvent(Event evt)
        {
            if (evt.IsMouseDragEnd() && _dragActive)
            {
                int absDist = Math.Abs(_lastDragDist);
                if (absDist > 3 && _lastDragDelta > 0)
                {
                    int amount = (int)Math.Min(1000, absDist * 100 / _lastDragDelta);
                    AutoScroll(amount * Math.Sign(_lastDragDist));
                }

                _hasDragStart = false;
                _dragActive = false;
                return true;
            }

            if (evt.IsMouseDragEvent())
            {
                if (_hasDragStart)
                {
                    long time = GetTime();
                    _dragActive = true;
                    _lastDragDist = _dragStartY - evt.GetMouseY();
                    _lastDragDelta = Math.Max(1, time - _lastDragTime);
                    Scroll(_lastDragDist);
                    _dragStartY = evt.GetMouseY();
                    _lastDragTime = time;
                }
                return true;
            }

            if (base.HandleEvent(evt))
            {
                return true;
            }

            if (evt.GetEventType() == EventType.MOUSE_WHEEL)
            {
                AutoScroll(_itemHeight * evt.GetMouseWheelDelta());
                return true;
            }
            else if (evt.GetEventType() == EventType.MOUSE_BTNDOWN)
            {
                if (evt.GetMouseButton() == Event.MOUSE_LBUTTON)
                {
                    _dragStartY = evt.GetMouseY();
                    _lastDragTime = GetTime();
                    _hasDragStart = true;
                }
                return true;
            }
            else if (evt.GetEventType() == EventType.MOUSE_BTNDOWN)
            {
                switch (evt.GetKeyCode())
                {
                    case Event.KEY_UP:
                        AutoScroll(-_itemHeight);
                        return true;
                    case Event.KEY_DOWN:
                        AutoScroll(+_itemHeight);
                        return true;
                }
                return false;
            }

            return evt.IsMouseEvent();
        }

        protected long GetTime()
        {
            GUI gui = GetGUI();
            return (gui != null) ? gui.GetCurrentTime() : 0;
        }

        protected int GetNumEntries()
        {
            return (_model == null) ? 0 : _model.Entries;
        }

        protected Widget GetItemRenderer(int i)
        {
            T item = _model.EntryAt(i);
            if (item != null)
            {
                ItemRenderer ir = (ItemRenderer)_itemRenderer.GetByType(item.GetType());
                if (ir != null)
                {
                    Widget w = ir.GetRenderWidget(item);
                    if (w != null)
                    {
                        if (w.GetParent() != _renderer)
                        {
                            w.SetVisible(false);
                            _renderer.Add(w);
                        }
                        return w;
                    }
                }
            }
            return null;
        }

        protected void StartTimer()
        {
            if (_timer != null && !_timer.IsRunning())
            {
                _timer.Start();
            }
        }

        protected void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
            }
        }

        protected void OnTimer()
        {
            int amount = _scrollAmount;
            int newAmount = amount;

            if (amount == 0 && !_dragActive)
            {
                amount = -_scrollOffset;
            }

            if (amount != 0)
            {
                int absAmount = Math.Abs(amount);
                int speed = absAmount * TIMER_INTERVAL / 200;
                int dir = Math.Sign(amount) * Math.Min(absAmount,
                        Math.Max(MIN_SPEED, Math.Min(MAX_SPEED, speed)));

                if (newAmount != 0)
                {
                    newAmount -= dir;
                }

                _scrollAmount = newAmount;
                ScrollInt(dir);
            }
        }

        protected override void Layout()
        {
            LayoutChildFullInnerArea(_renderer);
        }

        protected override void ApplyTheme(ThemeInfo themeInfo)
        {
            base.ApplyTheme(themeInfo);
            ApplyThemeWheel(themeInfo);
        }

        protected void ApplyThemeWheel(ThemeInfo themeInfo)
        {
            _itemHeight = themeInfo.GetParameter("itemHeight", 10);
            _numVisibleItems = themeInfo.GetParameter("visibleItems", 5);
            _selectedOverlay = themeInfo.GetImage("selectedOverlay");
            InvalidateLayout();
        }

        protected override void AfterAddToGUI(GUI gui)
        {
            base.AfterAddToGUI(gui);
            AddListener();
            AddSelectedListener();
            _timer = gui.CreateTimer();
            _timer.Tick += (sender, e) =>
            {
                OnTimer();
            };
            _timer.SetDelay(TIMER_INTERVAL);
            _timer.SetContinuous(true);
        }

        protected override void BeforeRemoveFromGUI(GUI gui)
        {
            _timer.Stop();
            _timer = null;
            RemoveListener();
            RemoveSelectedListener();
            base.BeforeRemoveFromGUI(gui);
        }

        public override void InsertChild(Widget child, int index)
        {
            throw new InvalidOperationException();
        }

        public override void RemoveAllChildren()
        {
            throw new InvalidOperationException();
        }

        public override Widget RemoveChild(int index)
        {
            throw new InvalidOperationException();
        }

        private void AddListener()
        {
            if (_model != null)
            {
                this._model.AllChanged += Model_AllChanged;
                this._model.EntriesChanged += Model_EntriesChanged;
                this._model.EntriesDeleted += Model_EntriesDeleted;
                this._model.EntriesInserted += Model_EntriesInserted;
            }
        }

        private void RemoveListener()
        {
            if (_model != null)
            {
                this._model.AllChanged -= Model_AllChanged;
                this._model.EntriesChanged -= Model_EntriesChanged;
                this._model.EntriesDeleted -= Model_EntriesDeleted;
                this._model.EntriesInserted -= Model_EntriesInserted;
            }
        }

        private void Model_EntriesInserted(object sender, ListSubsetChangedEventArgs e)
        {
            this.EntriesInserted(e.First, e.Last);
        }

        private void Model_EntriesDeleted(object sender, ListSubsetChangedEventArgs e)
        {
            this.EntriesDeleted(e.First, e.Last);
        }

        private void Model_EntriesChanged(object sender, ListSubsetChangedEventArgs e)
        {
            InvalidateLayout();
        }

        private void Model_AllChanged(object sender, ListAllChangedEventArgs e)
        {
            InvalidateLayout();
        }

        private void AddSelectedListener()
        {
            if (_selectedModel != null)
            {
                this._selectedModel.Changed += SelectedModel_Changed;
                SyncSelected();
            }
        }

        private void SelectedModel_Changed(object sender, IntegerChangedEventArgs e)
        {
            SyncSelected();
        }

        private void RemoveSelectedListener()
        {
            if (_selectedModel != null)
            {
                this._selectedModel.Changed -= SelectedModel_Changed;
            }
        }

        void SyncSelected()
        {
            SetSelected(_selectedModel.Value);
        }

        void EntriesDeleted(int first, int last)
        {
            if (_selected > first)
            {
                if (_selected > last)
                {
                    SetSelected(_selected - (last - first + 1));
                }
                else
                {
                    SetSelected(first);
                }
            }
            InvalidateLayout();
        }

        void EntriesInserted(int first, int last)
        {
            if (_selected >= first)
            {
                SetSelected(_selected + (last - first + 1));
            }
            InvalidateLayout();
        }

        class R : Widget
        {
            private WheelWidget<T> _wheelWidget;

            public R(WheelWidget<T> wheelWidget)
            {
                this._wheelWidget = wheelWidget;
                SetTheme("");
                SetClip(true);
            }

            protected override void PaintWidget(GUI gui)
            {
                if (this._wheelWidget._model == null)
                {
                    return;
                }

                int width = GetInnerWidth();
                int x = GetInnerX();
                int y = GetInnerY();

                int numItems = this._wheelWidget._model.Entries;
                int numDraw = this._wheelWidget._numVisibleItems;
                int startIdx = this._wheelWidget._selected - this._wheelWidget._numVisibleItems / 2;

                if ((numDraw & 1) == 0)
                {
                    y -= this._wheelWidget._itemHeight / 2;
                    numDraw++;
                }

                if (this._wheelWidget._scrollOffset > 0)
                {
                    y -= this._wheelWidget._scrollOffset;
                    numDraw++;
                }
                if (this._wheelWidget._scrollOffset < 0)
                {
                    y -= this._wheelWidget._itemHeight + this._wheelWidget._scrollOffset;
                    numDraw++;
                    startIdx--;
                }

                for (int i = 0; i < numDraw; i++)
                {
                    int idx = startIdx + i;

                    bool breakAndContinue = false;
                    while (idx < 0)
                    {
                        if (!this._wheelWidget._cyclic)
                        {
                            breakAndContinue = true;
                            break;
                        }
                        idx += numItems;
                    }

                    if (breakAndContinue)
                    {
                        continue;
                    }

                    while (idx >= numItems)
                    {
                        if (!this._wheelWidget._cyclic)
                        {
                            breakAndContinue = true;
                            break;
                        }
                        idx -= numItems;
                    }

                    if (breakAndContinue)
                    {
                        continue;
                    }

                    Widget w = this._wheelWidget.GetItemRenderer(idx);
                    if (w != null)
                    {
                        w.SetSize(width, this._wheelWidget._itemHeight);
                        w.SetPosition(x, y + i * this._wheelWidget._itemHeight);
                        w.ValidateLayout();
                        PaintChild(gui, w);
                    }
                }
            }

            public override void InvalidateLayout()
            {
            }

            protected override void SizeChanged()
            {
            }
        }

        public class StringItemRenderer : Label, ItemRenderer
        {
            public StringItemRenderer()
            {
                SetCache(false);
            }

            public Widget GetRenderWidget(Object data)
            {
                SetText(data.ToString());
                return this;
            }

            protected override void SizeChanged()
            {
            }
        }
    }
}
