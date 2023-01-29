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
            Widget getRenderWidget(Object data);
        }

        private TypeMapping itemRenderer;
        private R renderer;
        private Runnable timerCB;

        protected int itemHeight;
        protected int numVisibleItems;
        protected Image selectedOverlay;

        private static int TIMER_INTERVAL = 30;
        private static int MIN_SPEED = 3;
        private static int MAX_SPEED = 100;

        protected Timer timer;
        protected int dragStartY;
        protected long lastDragTime;
        protected long lastDragDelta;
        protected int lastDragDist;
        protected bool hasDragStart;
        protected bool dragActive;
        protected int scrollOffset;
        protected int scrollAmount;

        protected ListModel<T> model;
        protected IntegerModel selectedModel;
        protected int selected;
        protected bool cyclic;

        public WheelWidget()
        {
            this.itemRenderer = new TypeMapping();
            this.renderer = new R(this);

            itemRenderer.SetByType(typeof(String), new StringItemRenderer());

            base.insertChild(renderer, 0);
            setCanAcceptKeyboardFocus(true);
        }

        public WheelWidget(ListModel<T> model) : this()
        {
            this.model = model;
        }

        public ListModel<T> getModel()
        {
            return model;
        }

        public void setModel(ListModel<T> model)
        {
            removeListener();
            this.model = model;
            addListener();
            invalidateLayout();
        }

        public IntegerModel getSelectedModel()
        {
            return selectedModel;
        }

        public void setSelectedModel(IntegerModel selectedModel)
        {
            removeSelectedListener();
            this.selectedModel = selectedModel;
            addSelectedListener();
        }

        public int getSelected()
        {
            return selected;
        }

        public void setSelected(int selected)
        {
            int oldSelected = this.selected;
            if (oldSelected != selected)
            {
                this.selected = selected;
                if (selectedModel != null)
                {
                    selectedModel.Value = selected;
                }
                firePropertyChange("selected", oldSelected, selected);
            }
        }

        public bool isCyclic()
        {
            return cyclic;
        }

        public void setCyclic(bool cyclic)
        {
            this.cyclic = cyclic;
        }

        public int getItemHeight()
        {
            return itemHeight;
        }

        public int getNumVisibleItems()
        {
            return numVisibleItems;
        }

        public bool removeItemRenderer(Type clazz)
        {
            if (itemRenderer.RemoveByType(clazz))
            {
                base.removeAllChildren();
                invalidateLayout();
                return true;
            }
            return false;
        }

        public void registerItemRenderer(Type clazz, ItemRenderer value)
        {
            itemRenderer.SetByType(clazz, value);
            invalidateLayout();
        }

        public void scroll(int amount)
        {
            scrollInt(amount);
            scrollAmount = 0;
        }

        protected void scrollInt(int amount)
        {
            int pos = selected;
            int half = itemHeight / 2;

            scrollOffset += amount;
            while (scrollOffset >= half)
            {
                scrollOffset -= itemHeight;
                pos++;
            }
            while (scrollOffset <= -half)
            {
                scrollOffset += itemHeight;
                pos--;
            }

            if (!cyclic)
            {
                int n = getNumEntries();
                if (n > 0)
                {
                    while (pos >= n)
                    {
                        pos--;
                        scrollOffset += itemHeight;
                    }
                }
                while (pos < 0)
                {
                    pos++;
                    scrollOffset -= itemHeight;
                }
                scrollOffset = Math.Max(-itemHeight, Math.Min(itemHeight, scrollOffset));
            }

            setSelected(pos);

            if (scrollOffset == 0 && scrollAmount == 0)
            {
                stopTimer();
            }
            else
            {
                startTimer();
            }
        }

        public void autoScroll(int dir)
        {
            if (dir != 0)
            {
                if (scrollAmount != 0 && Math.Sign(scrollAmount) != Math.Sign(dir))
                {
                    scrollAmount = dir;
                }
                else
                {
                    scrollAmount += dir;
                }
                startTimer();
            }
        }

        public override int getPreferredInnerHeight()
        {
            return numVisibleItems * itemHeight;
        }

        public override int getPreferredInnerWidth()
        {
            int width = 0;
            for (int i = 0, n = getNumEntries(); i < n; i++)
            {
                Widget w = getItemRenderer(i);
                if (w != null)
                {
                    width = Math.Max(width, w.getPreferredWidth());
                }
            }
            return width;
        }

        protected override void paintOverlay(GUI gui)
        {
            base.paintOverlay(gui);

            if (selectedOverlay != null)
            {
                int y = getInnerY() + itemHeight * (numVisibleItems / 2);
                if ((numVisibleItems & 1) == 0)
                {
                    y -= itemHeight / 2;
                }
                selectedOverlay.Draw(getAnimationState(), getX(), y, getWidth(), itemHeight);
            }
        }

        public override bool handleEvent(Event evt)
        {
            if (evt.isMouseDragEnd() && dragActive)
            {
                int absDist = Math.Abs(lastDragDist);
                if (absDist > 3 && lastDragDelta > 0)
                {
                    int amount = (int)Math.Min(1000, absDist * 100 / lastDragDelta);
                    autoScroll(amount * Math.Sign(lastDragDist));
                }

                hasDragStart = false;
                dragActive = false;
                return true;
            }

            if (evt.isMouseDragEvent())
            {
                if (hasDragStart)
                {
                    long time = getTime();
                    dragActive = true;
                    lastDragDist = dragStartY - evt.getMouseY();
                    lastDragDelta = Math.Max(1, time - lastDragTime);
                    scroll(lastDragDist);
                    dragStartY = evt.getMouseY();
                    lastDragTime = time;
                }
                return true;
            }

            if (base.handleEvent(evt))
            {
                return true;
            }

            if (evt.getEventType() == EventType.MOUSE_WHEEL)
            {
                autoScroll(itemHeight * evt.getMouseWheelDelta());
                return true;
            }
            else if (evt.getEventType() == EventType.MOUSE_BTNDOWN)
            {
                if (evt.getMouseButton() == Event.MOUSE_LBUTTON)
                {
                    dragStartY = evt.getMouseY();
                    lastDragTime = getTime();
                    hasDragStart = true;
                }
                return true;
            }
            else if (evt.getEventType() == EventType.MOUSE_BTNDOWN)
            {
                switch (evt.getKeyCode())
                {
                    case Event.KEY_UP:
                        autoScroll(-itemHeight);
                        return true;
                    case Event.KEY_DOWN:
                        autoScroll(+itemHeight);
                        return true;
                }
                return false;
            }

            return evt.isMouseEvent();
        }

        protected long getTime()
        {
            GUI gui = getGUI();
            return (gui != null) ? gui.getCurrentTime() : 0;
        }

        protected int getNumEntries()
        {
            return (model == null) ? 0 : model.Entries;
        }

        protected Widget getItemRenderer(int i)
        {
            T item = model.EntryAt(i);
            if (item != null)
            {
                ItemRenderer ir = (ItemRenderer) itemRenderer.GetByType(item.GetType());
                if (ir != null)
                {
                    Widget w = ir.getRenderWidget(item);
                    if (w != null)
                    {
                        if (w.getParent() != renderer)
                        {
                            w.setVisible(false);
                            renderer.add(w);
                        }
                        return w;
                    }
                }
            }
            return null;
        }

        protected void startTimer()
        {
            if (timer != null && !timer.isRunning())
            {
                timer.start();
            }
        }

        protected void stopTimer()
        {
            if (timer != null)
            {
                timer.stop();
            }
        }

        protected void onTimer()
        {
            int amount = scrollAmount;
            int newAmount = amount;

            if (amount == 0 && !dragActive)
            {
                amount = -scrollOffset;
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

                scrollAmount = newAmount;
                scrollInt(dir);
            }
        }

        protected override void layout()
        {
            layoutChildFullInnerArea(renderer);
        }

        protected override void applyTheme(ThemeInfo themeInfo)
        {
            base.applyTheme(themeInfo);
            applyThemeWheel(themeInfo);
        }

        protected void applyThemeWheel(ThemeInfo themeInfo)
        {
            itemHeight = themeInfo.getParameter("itemHeight", 10);
            numVisibleItems = themeInfo.getParameter("visibleItems", 5);
            selectedOverlay = themeInfo.getImage("selectedOverlay");
            invalidateLayout();
        }

        protected override void afterAddToGUI(GUI gui)
        {
            base.afterAddToGUI(gui);
            addListener();
            addSelectedListener();
            timer = gui.createTimer();
            timer.Tick += (sender, e) =>
            {
                onTimer();
            };
            timer.setDelay(TIMER_INTERVAL);
            timer.setContinuous(true);
        }

        protected override void beforeRemoveFromGUI(GUI gui)
        {
            timer.stop();
            timer = null;
            removeListener();
            removeSelectedListener();
            base.beforeRemoveFromGUI(gui);
        }

        public override void insertChild(Widget child, int index)
        {
            throw new InvalidOperationException();
        }

        public override void removeAllChildren()
        {
            throw new InvalidOperationException();
        }

        public override Widget removeChild(int index)
        {
            throw new InvalidOperationException();
        }

        private void addListener()
        {
            if (model != null)
            {
                this.model.AllChanged += Model_AllChanged;
                this.model.EntriesChanged += Model_EntriesChanged;
                this.model.EntriesDeleted += Model_EntriesDeleted;
                this.model.EntriesInserted += Model_EntriesInserted;
            }
        }

        private void removeListener()
        {
            if (model != null)
            {
                this.model.AllChanged -= Model_AllChanged;
                this.model.EntriesChanged -= Model_EntriesChanged;
                this.model.EntriesDeleted -= Model_EntriesDeleted;
                this.model.EntriesInserted -= Model_EntriesInserted;
            }
        }

        private void Model_EntriesInserted(object sender, ListSubsetChangedEventArgs e)
        {
            this.entriesInserted(e.First, e.Last);
        }

        private void Model_EntriesDeleted(object sender, ListSubsetChangedEventArgs e)
        {
            this.entriesDeleted(e.First, e.Last);
        }

        private void Model_EntriesChanged(object sender, ListSubsetChangedEventArgs e)
        {
            invalidateLayout();
        }

        private void Model_AllChanged(object sender, ListAllChangedEventArgs e)
        {
            invalidateLayout();
        }

        private void addSelectedListener()
        {
            if (selectedModel != null)
            {
                this.selectedModel.Changed += SelectedModel_Changed;
                syncSelected();
            }
        }

        private void SelectedModel_Changed(object sender, IntegerChangedEventArgs e)
        {
            syncSelected();
        }

        private void removeSelectedListener()
        {
            if (selectedModel != null)
            {
                this.selectedModel.Changed -= SelectedModel_Changed;
            }
        }

        void syncSelected()
        {
            setSelected(selectedModel.Value);
        }

        void entriesDeleted(int first, int last)
        {
            if (selected > first)
            {
                if (selected > last)
                {
                    setSelected(selected - (last - first + 1));
                }
                else
                {
                    setSelected(first);
                }
            }
            invalidateLayout();
        }

        void entriesInserted(int first, int last)
        {
            if (selected >= first)
            {
                setSelected(selected + (last - first + 1));
            }
            invalidateLayout();
        }

        class R : Widget
        {
            private WheelWidget<T> wheelWidget;

            public R(WheelWidget<T> wheelWidget)
            {
                this.wheelWidget = wheelWidget;
                setTheme("");
                setClip(true);
            }

            protected override void paintWidget(GUI gui)
            {
                if (this.wheelWidget.model == null)
                {
                    return;
                }

                int width = getInnerWidth();
                int x = getInnerX();
                int y = getInnerY();

                int numItems = this.wheelWidget.model.Entries;
                int numDraw = this.wheelWidget.numVisibleItems;
                int startIdx = this.wheelWidget.selected - this.wheelWidget.numVisibleItems / 2;

                if ((numDraw & 1) == 0)
                {
                    y -= this.wheelWidget.itemHeight / 2;
                    numDraw++;
                }

                if (this.wheelWidget.scrollOffset > 0)
                {
                    y -= this.wheelWidget.scrollOffset;
                    numDraw++;
                }
                if (this.wheelWidget.scrollOffset < 0)
                {
                    y -= this.wheelWidget.itemHeight + this.wheelWidget.scrollOffset;
                    numDraw++;
                    startIdx--;
                }

                for (int i = 0; i < numDraw; i++)
                {
                    int idx = startIdx + i;

                    bool breakAndContinue = false;
                    while (idx < 0)
                    {
                        if (!this.wheelWidget.cyclic)
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
                        if (!this.wheelWidget.cyclic)
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

                    Widget w = this.wheelWidget.getItemRenderer(idx);
                    if (w != null)
                    {
                        w.setSize(width, this.wheelWidget.itemHeight);
                        w.setPosition(x, y + i * this.wheelWidget.itemHeight);
                        w.validateLayout();
                        paintChild(gui, w);
                    }
                }
            }

            public override void invalidateLayout()
            {
            }

            protected override void sizeChanged()
            {
            }
        }

        public class StringItemRenderer : Label, ItemRenderer
        {
            public StringItemRenderer()
            {
                setCache(false);
            }

            public Widget getRenderWidget(Object data)
            {
                setText(data.ToString());
                return this;
            }

            protected override void sizeChanged()
            {
            }
        }
    }
}
