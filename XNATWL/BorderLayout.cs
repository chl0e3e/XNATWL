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
using System.Collections.Generic;

namespace XNATWL
{

    public class BorderLayout : Widget
    {
        private Dictionary<Location, Widget> _widgets;
        private int _hGap, _vGap;

        /**
         * The location of a widget in the BorderLayout.
         */
        public enum Location
        {
            EAST, WEST, NORTH, SOUTH, CENTER
        }

        public BorderLayout()
        {
            _widgets = new Dictionary<BorderLayout.Location, Widget>();
        }

        /**
         * Adds the specific
         * <code>widget</code> to a
         * <code>location</code> in the BorderLayout.
         *
         * @param widget the widget to add
         * @param location the location to set the widget to
         */
        public void Add(Widget widget, Location location)
        {
            if (widget == null)
            {
                throw new ArgumentNullException("widget is null");
            }

            if (_widgets.ContainsKey(location))
            {
                throw new ArgumentOutOfRangeException("a widget was already added to that location: " + location);
            }

            _widgets.Add(location, widget);
            try
            {
                base.InsertChild(widget, GetNumChildren());
            }
            catch (Exception e)
            {
                RemoveChild(location);
            }
        }

        /**
         * @param location the location to look retrieve
         * @return the child at the specific
         * <code>location</code> or null if there is no child.
         */
        public Widget GetChild(Location location)
        {
            return _widgets[location];
        }

        /**
         * Remove the child at the specific
         * <code>location</code>.
         *
         * @param location the location to remove
         * @return the removed widget or null if there is no child.
         */
        public Widget RemoveChild(Location location)
        {
            Widget w = _widgets[location];
            _widgets.Remove(location);
            if (w != null)
            {
                RemoveChild(w);
            }

            return w;
        }

        /**
         * Adds the widget to the center location of the layout.
         */
        public override void Add(Widget child)
        {
            Add(child, Location.CENTER);
        }

        /**
         * This is not supproted in the BorderLayout.
         *
         * @throws UnsupportedOperationException
         */
        public override void InsertChild(Widget child, int index)
        {
            throw new InvalidOperationException("insert child is not supported by the BorderLayout");
        }

        protected override void ChildRemoved(Widget exChild)
        {
            foreach (Location loc in _widgets.Keys)
            {
                if (_widgets[loc] == exChild)
                {
                    _widgets.Remove(loc);
                    break;
                }
            }
            base.ChildRemoved(exChild);
        }

        protected override void AllChildrenRemoved()
        {
            _widgets.Clear();
            base.AllChildrenRemoved();
        }

        protected override void ApplyTheme(ThemeInfo themeInfo)
        {
            _hGap = themeInfo.GetParameter("hgap", 0);
            _vGap = themeInfo.GetParameter("vgap", 0);

            base.ApplyTheme(themeInfo);
        }

        protected override void Layout()
        {
            int top = GetInnerY();
            int bottom = GetInnerBottom();
            int left = GetInnerX();
            int right = GetInnerRight();
            Widget w;

            if ((w = _widgets[Location.NORTH]) != null)
            {
                w.SetPosition(left, top);
                w.SetSize(Math.Max(right - left, 0), Math.Max(w.GetPreferredHeight(), 0));
                top += w.GetPreferredHeight() + _vGap;
            }
            if ((w = _widgets[Location.SOUTH]) != null)
            {
                w.SetPosition(left, bottom - w.GetPreferredHeight());
                w.SetSize(Math.Max(right - left, 0), Math.Max(w.GetPreferredHeight(), 0));
                bottom -= w.GetPreferredHeight() + _vGap;
            }
            if ((w = _widgets[Location.EAST]) != null)
            {
                w.SetPosition(right - w.GetPreferredWidth(), top);
                w.SetSize(Math.Max(w.GetPreferredWidth(), 0), Math.Max(bottom - top, 0));
                right -= w.GetPreferredWidth() + _hGap;
            }
            if ((w = _widgets[Location.WEST]) != null)
            {
                w.SetPosition(left, top);
                w.SetSize(Math.Max(w.GetPreferredWidth(), 0), Math.Max(bottom - top, 0));
                left += w.GetPreferredWidth() + _hGap;
            }
            if ((w = _widgets[Location.CENTER]) != null)
            {
                w.SetPosition(left, top);
                w.SetSize(Math.Max(right - left, 0), Math.Max(bottom - top, 0));
            }
        }

        public override int GetMinWidth()
        {
            return ComputeMinWidth();
        }

        public override int GetMinHeight()
        {
            return ComputeMinHeight();
        }

        public override int GetPreferredInnerWidth()
        {
            return ComputePrefWidth();
        }

        public override int GetPreferredInnerHeight()
        {
            return ComputePrefHeight();
        }

        private int ComputeMinWidth()
        {
            int size = 0;

            size += GetChildMinWidth(_widgets[Location.EAST], _hGap);
            size += GetChildMinWidth(_widgets[Location.WEST], _hGap);
            size += GetChildMinWidth(_widgets[Location.CENTER], 0);

            size = Math.Max(size, GetChildMinWidth(_widgets[Location.NORTH], 0));
            size = Math.Max(size, GetChildMinWidth(_widgets[Location.SOUTH], 0));

            return size;
        }

        private int ComputeMinHeight()
        {
            int size = 0;

            size = Math.Max(size, GetChildMinHeight(_widgets[Location.EAST], 0));
            size = Math.Max(size, GetChildMinHeight(_widgets[Location.WEST], 0));
            size = Math.Max(size, GetChildMinHeight(_widgets[Location.CENTER], 0));

            size += GetChildMinHeight(_widgets[Location.NORTH], _vGap);
            size += GetChildMinHeight(_widgets[Location.SOUTH], _vGap);

            return size;
        }

        private int ComputePrefWidth()
        {
            int size = 0;

            size += GetChildPrefWidth(_widgets[Location.EAST], _hGap);
            size += GetChildPrefWidth(_widgets[Location.WEST], _hGap);
            size += GetChildPrefWidth(_widgets[Location.CENTER], 0);

            size = Math.Max(size, GetChildPrefWidth(_widgets[Location.NORTH], 0));
            size = Math.Max(size, GetChildPrefWidth(_widgets[Location.SOUTH], 0));

            return size;
        }

        private int ComputePrefHeight()
        {
            int size = 0;

            size = Math.Max(size, GetChildPrefHeight(_widgets[Location.EAST], 0));
            size = Math.Max(size, GetChildPrefHeight(_widgets[Location.WEST], 0));
            size = Math.Max(size, GetChildPrefHeight(_widgets[Location.CENTER], 0));

            size += GetChildPrefHeight(_widgets[Location.NORTH], _vGap);
            size += GetChildPrefHeight(_widgets[Location.SOUTH], _vGap);

            return size;
        }

        // return 0 since a child of the BorderLayout can be null
        private int GetChildMinWidth(Widget w, int gap)
        {
            if (w != null)
            {
                return w.GetMinWidth() + gap;
            }

            return 0;
        }

        private int GetChildMinHeight(Widget w, int gap)
        {
            if (w != null)
            {
                return w.GetMinHeight() + gap;
            }

            return 0;
        }

        private int GetChildPrefWidth(Widget w, int gap)
        {
            if (w != null)
            {
                return ComputeSize(w.GetMinWidth(), w.GetPreferredWidth(), w.GetMaxWidth()) + gap;
            }

            return 0;
        }

        private int GetChildPrefHeight(Widget w, int gap)
        {
            if (w != null)
            {
                return ComputeSize(w.GetMinHeight(), w.GetPreferredHeight(), w.GetMaxHeight()) + gap;
            }

            return 0;
        }
    }
}
