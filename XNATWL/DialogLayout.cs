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
using System.Text;
using static XNATWL.Utils.Logger;
using XNATWL.Renderer;
using XNATWL.Utils;

namespace XNATWL
{

    /**
     * A layout manager similar to Swing's GroupLayout
     *
     * This layout manager uses two independant layout groups:
     *   one for the horizontal axis
     *   one for the vertical axis.
     * Every widget must be added to both the horizontal and the vertical group.
     *
     * When a widget is added to a group it will also be added as a child widget
     * if it was not already added. You can add widgets to DialogLayout before
     * adding them to a group to set the focus order.
     *
     * There are two kinds of groups:
     *   a sequential group which which behaves similar to BoxLayout
     *   a parallel group which alignes the start and size of each child
     *
     * Groups can be cascaded as a tree without restrictions.
     *
     * It is also possible to add widgets to DialogLayout without adding them
     * to the layout groups. These widgets are then not touched by DialogLayout's
     * layout system.
     *
     * When a widget is only added to either the horizontal or vertical groups
     * and not both, then an IllegalStateException exception is created on layout.
     *
     * To help debugging the group construction you can set the system property
     * "debugLayoutGroups" to "true" which will collect additional stack traces
     * to help locate the source of the error.
     *
     * @author Matthias Mann
     * @see #createParallelGroup() 
     * @see #createSequentialGroup()
     */
    public class DialogLayout : Widget
    {

        /**
         * Symbolic constant to refer to "small gap".
         * @see #getSmallGap()
         * @see Group#addGap(int)
         * @see Group#addGap(int, int, int)
         */
        public static int SMALL_GAP = -1;

        /**
         * Symbolic constant to refer to "medium gap".
         * @see #getMediumGap()
         * @see Group#addGap(int)
         * @see Group#addGap(int, int, int)
         */
        public static int MEDIUM_GAP = -2;

        /**
         * Symbolic constant to refer to "large gap".
         * @see #getLargeGap()
         * @see Group#addGap(int)
         * @see Group#addGap(int, int, int)
         */
        public static int LARGE_GAP = -3;


        public static int AXIS_X = 0;
        public static int AXIS_Y = 1;


        /**
         * Symbolic constant to refer to "default gap".
         * The default gap is added (when enabled) between widgets.
         *
         * @see #getDefaultGap()
         * @see #setAddDefaultGaps(bool)
         * @see #isAddDefaultGaps()
         * @see Group#addGap(int)
         * @see Group#addGap(int, int, int)
         */
        public static int DEFAULT_GAP = -4;

        protected Dimension _smallGap;
        protected Dimension _mediumGap;
        protected Dimension _largeGap;
        protected Dimension _defaultGap;
        protected ParameterMap _namedGaps;

        protected bool _bAddDefaultGaps = true;
        protected bool _bIncludeInvisibleWidgets = true;
        protected bool _redoDefaultGaps;
        protected bool _isPrepared;
        protected bool _blockInvalidateLayoutTree;
        protected bool _bWarnOnIncomplete;

        private Group _horzGroup;
        private Group _vertGroup;

        /**
         * Debugging aid. Captures the stack trace where one of the group was last assigned.
         */
        Exception _debugStackTrace;

        Dictionary<Widget, WidgetSpring> _widgetSprings;

        /**
         * Creates a new DialogLayout widget.
         *
         * Initially both the horizontal and the vertical group are null.
         * 
         * @see #setHorizontalGroup(de.matthiasmann.twl.DialogLayout.Group)
         * @see #setVerticalGroup(de.matthiasmann.twl.DialogLayout.Group)
         */
        public DialogLayout()
        {
            _widgetSprings = new Dictionary<Widget, WidgetSpring>();
            CollectDebugStack();
        }

        public Group GetHorizontalGroup()
        {
            return _horzGroup;
        }

        /**
         * The horizontal group controls the position and size of all child
         * widgets along the X axis.
         *
         * Every widget must be part of both horizontal and vertical group.
         * Otherwise a IllegalStateException is thrown at layout time.
         *
         * If you want to change both horizontal and vertical group then
         * it's recommended to set the other group first to null:
         * <pre>
         * setVerticalGroup(null);
         * setHorizontalGroup(newHorzGroup);
         * setVerticalGroup(newVertGroup);
         * </pre>
         *
         * @param g the group used for the X axis
         * @see #setVerticalGroup(de.matthiasmann.twl.DialogLayout.Group)
         */
        public void SetHorizontalGroup(Group g)
        {
            if (g != null)
            {
                g.CheckGroup(this);
            }
            this._horzGroup = g;
            CollectDebugStack();
            LayoutGroupsChanged();
        }

        public Group GetVerticalGroup()
        {
            return _vertGroup;
        }

        /**
         * The vertical group controls the position and size of all child
         * widgets along the Y axis.
         *
         * Every widget must be part of both horizontal and vertical group.
         * Otherwise a IllegalStateException is thrown at layout time.
         *
         * @param g the group used for the Y axis
         * @see #setHorizontalGroup(de.matthiasmann.twl.DialogLayout.Group) 
         */
        public void SetVerticalGroup(Group g)
        {
            if (g != null)
            {
                g.CheckGroup(this);
            }
            this._vertGroup = g;
            CollectDebugStack();
            LayoutGroupsChanged();
        }

        public Dimension GetSmallGap()
        {
            return _smallGap;
        }

        public void SetSmallGap(Dimension smallGap)
        {
            this._smallGap = smallGap;
            MaybeInvalidateLayoutTree();
        }

        public Dimension GetMediumGap()
        {
            return _mediumGap;
        }

        public void SetMediumGap(Dimension mediumGap)
        {
            this._mediumGap = mediumGap;
            MaybeInvalidateLayoutTree();
        }

        public Dimension GetLargeGap()
        {
            return _largeGap;
        }

        public void SetLargeGap(Dimension largeGap)
        {
            this._largeGap = largeGap;
            MaybeInvalidateLayoutTree();
        }

        public Dimension GetDefaultGap()
        {
            return _defaultGap;
        }

        public void SetDefaultGap(Dimension defaultGap)
        {
            this._defaultGap = defaultGap;
            MaybeInvalidateLayoutTree();
        }

        public bool IsAddDefaultGaps()
        {
            return _bAddDefaultGaps;
        }

        /**
         * Determine whether default gaps should be added from the theme or not.
         * 
         * @param addDefaultGaps if true then default gaps are added.
         */
        public void SetAddDefaultGaps(bool bAddDefaultGaps)
        {
            this._bAddDefaultGaps = bAddDefaultGaps;
        }

        /**
         * removes all default gaps from all groups.
         */
        public void RemoveDefaultGaps()
        {
            if (_horzGroup != null && _vertGroup != null)
            {
                _horzGroup.RemoveDefaultGaps();
                _vertGroup.RemoveDefaultGaps();
                MaybeInvalidateLayoutTree();
            }
        }

        /**
         * Adds theme dependant default gaps to all groups.
         */
        public void AddDefaultGaps()
        {
            if (_horzGroup != null && _vertGroup != null)
            {
                _horzGroup.AddDefaultGap();
                _vertGroup.AddDefaultGap();
                MaybeInvalidateLayoutTree();
            }
        }

        public bool IsIncludeInvisibleWidgets()
        {
            return _bIncludeInvisibleWidgets;
        }

        /**
         * Controls whether invisible widgets should be included in the layout or
         * not. If they are not included then the layout is recomputed when the
         * visibility of a child widget changes.
         *
         * The default is true
         *
         * @param includeInvisibleWidgets If true then invisible widgets are included,
         *      if false they don't contribute to the layout.
         */
        public void SetIncludeInvisibleWidgets(bool includeInvisibleWidgets)
        {
            if (this._bIncludeInvisibleWidgets != includeInvisibleWidgets)
            {
                this._bIncludeInvisibleWidgets = includeInvisibleWidgets;
                LayoutGroupsChanged();
            }
        }

        private void CollectDebugStack()
        {
            _bWarnOnIncomplete = true;
            if (DEBUG_LAYOUT_GROUPS)
            {
                _debugStackTrace = new Exception("DialogLayout created/used here");
            }
        }

        private void WarnOnIncomplete()
        {
            _bWarnOnIncomplete = false;
            GetLogger().Log(Level.WARNING, "Dialog layout has incomplete state", _debugStackTrace);
        }

        static Logger GetLogger()
        {
            return Logger.GetLogger(typeof(DialogLayout));
        }

        protected void ApplyThemeDialogLayout(ThemeInfo themeInfo)
        {
            try
            {
                _blockInvalidateLayoutTree = true;
                SetSmallGap(themeInfo.GetParameterValue("smallGap", true, typeof(Dimension), Dimension.ZERO));
                SetMediumGap(themeInfo.GetParameterValue("mediumGap", true, typeof(Dimension), Dimension.ZERO));
                SetLargeGap(themeInfo.GetParameterValue("largeGap", true, typeof(Dimension), Dimension.ZERO));
                SetDefaultGap(themeInfo.GetParameterValue("defaultGap", true, typeof(Dimension), Dimension.ZERO));
                _namedGaps = themeInfo.GetParameterMap("namedGaps");
            }
            finally
            {
                _blockInvalidateLayoutTree = false;
            }
            InvalidateLayout();
        }

        protected override void ApplyTheme(ThemeInfo themeInfo)
        {
            base.ApplyTheme(themeInfo);
            ApplyThemeDialogLayout(themeInfo);
        }

        public override int GetMinWidth()
        {
            if (_horzGroup != null)
            {
                Prepare();
                return _horzGroup.GetMinSize(AXIS_X) + GetBorderHorizontal();
            }
            return base.GetMinWidth();
        }

        public override int GetMinHeight()
        {
            if (_vertGroup != null)
            {
                Prepare();
                return _vertGroup.GetMinSize(AXIS_Y) + GetBorderVertical();
            }
            return base.GetMinHeight();
        }

        public override int GetPreferredInnerWidth()
        {
            if (_horzGroup != null)
            {
                Prepare();
                return _horzGroup.GetPrefSize(DialogLayout.AXIS_X);
            }
            return base.GetPreferredInnerWidth();
        }

        public override int GetPreferredInnerHeight()
        {
            if (_vertGroup != null)
            {
                Prepare();
                return _vertGroup.GetPrefSize(AXIS_Y);
            }
            return base.GetPreferredInnerHeight();
        }

        public override void AdjustSize()
        {
            if (_horzGroup != null && _vertGroup != null)
            {
                Prepare();
                int minWidth = _horzGroup.GetMinSize(AXIS_X);
                int minHeight = _vertGroup.GetMinSize(AXIS_Y);
                int prefWidth = _horzGroup.GetPrefSize(AXIS_X);
                int prefHeight = _vertGroup.GetPrefSize(AXIS_Y);
                int maxWidth = GetMaxWidth();
                int maxHeight = GetMaxHeight();
                SetInnerSize(
                        ComputeSize(minWidth, prefWidth, maxWidth),
                        ComputeSize(minHeight, prefHeight, maxHeight));
                DoLayout();
            }
        }

        protected override void Layout()
        {
            if (_horzGroup != null && _vertGroup != null)
            {
                Prepare();
                DoLayout();
            }
            else if (_bWarnOnIncomplete)
            {
                WarnOnIncomplete();
            }
        }

        protected void Prepare()
        {
            if (_redoDefaultGaps)
            {
                if (_bAddDefaultGaps)
                {
                    try
                    {
                        _blockInvalidateLayoutTree = true;
                        RemoveDefaultGaps();
                        AddDefaultGaps();
                    }
                    finally
                    {
                        _blockInvalidateLayoutTree = false;
                    }
                }
                _redoDefaultGaps = false;
                _isPrepared = false;
            }
            if (!_isPrepared)
            {
                foreach (WidgetSpring s in _widgetSprings.Values)
                {
                    if (_bIncludeInvisibleWidgets || s._w.IsVisible())
                    {
                        s.prepare();
                    }
                }
                _isPrepared = true;
            }
        }

        protected void DoLayout()
        {
            _horzGroup.SetSize(AXIS_X, GetInnerX(), GetInnerWidth());
            _vertGroup.SetSize(AXIS_Y, GetInnerY(), GetInnerHeight());
            try
            {
                foreach (WidgetSpring s in _widgetSprings.Values)
                {
                    if (_bIncludeInvisibleWidgets || s._w.IsVisible())
                    {
                        s.Apply();
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                if (_debugStackTrace != null && ex.InnerException == null)
                {
                    throw new InvalidOperationException(ex.Message, _debugStackTrace);
                }
                throw ex;
            }
        }

        public override void InvalidateLayout()
        {
            _isPrepared = false;
            base.InvalidateLayout();
        }

        protected override void PaintWidget(GUI gui)
        {
            _isPrepared = false;
            // super.paintWidget() is empty
        }

        protected override void SizeChanged()
        {
            _isPrepared = false;
            base.SizeChanged();
        }

        protected override void AfterAddToGUI(GUI gui)
        {
            _isPrepared = false;
            base.AfterAddToGUI(gui);
        }

        /**
         * Creates a new parallel group.
         * All children in a parallel group share the same position and size of it's axis.
         *
         * @return the new parallel Group.
         */
        public Group CreateParallelGroup()
        {
            return new ParallelGroup(this);
        }

        /**
         * Creates a parallel group and adds the specified widgets.
         *
         * @see #createParallelGroup()
         * @param widgets the widgets to add
         * @return a new parallel Group.
         */
        public Group CreateParallelGroup(params Widget[] widgets)
        {
            return CreateParallelGroup().AddWidgets(widgets);
        }

        /**
         * Creates a parallel group and adds the specified groups.
         *
         * @see #createParallelGroup()
         * @param groups the groups to add
         * @return a new parallel Group.
         */
        public Group CreateParallelGroup(params Group[] groups)
        {
            return CreateParallelGroup().AddGroups(groups);
        }

        /**
         * Creates a new sequential group.
         * All children in a sequential group are ordered with increasing coordinates
         * along it's axis in the order they are added to the group. The available
         * size is distributed among the children depending on their min/preferred/max
         * sizes.
         * 
         * @return a new sequential Group.
         */
        public Group CreateSequentialGroup()
        {
            return new SequentialGroup(this);
        }

        /**
         * Creates a sequential group and adds the specified widgets.
         *
         * @see #createSequentialGroup()
         * @param widgets the widgets to add
         * @return a new sequential Group.
         */
        public Group CreateSequentialGroup(params Widget[] widgets)
        {
            return CreateSequentialGroup().AddWidgets(widgets);
        }

        /**
         * Creates a sequential group and adds the specified groups.
         *
         * @see #createSequentialGroup()
         * @param groups the groups to add
         * @return a new sequential Group.
         */
        public Group CreateSequentialGroup(params Group[] groups)
        {
            return CreateSequentialGroup().AddGroups(groups);
        }

        public override void InsertChild(Widget child, int index)
        {
            base.InsertChild(child, index);
            _widgetSprings.Add(child, new WidgetSpring(this, child));
        }

        public override void RemoveAllChildren()
        {
            base.RemoveAllChildren();
            _widgetSprings.Clear();
            RecheckWidgets();
            LayoutGroupsChanged();
        }

        public override Widget RemoveChild(int index)
        {
            Widget widget = base.RemoveChild(index);
            _widgetSprings.Remove(widget);
            RecheckWidgets();
            LayoutGroupsChanged();
            return widget;
        }

        /**
         * Sets the alignment of the specified widget.
         * The widget must have already been added to this container for this method to work.
         *
         * <p>The default alignment of a widget is {@link Alignment#FILL}</p>
         * 
         * @param widget the widget for which the alignment should be set
         * @param alignment the new alignment
         * @return true if the widget's alignment was changed, false otherwise
         */
        public bool SetWidgetAlignment(Widget widget, Alignment alignment)
        {
            if (widget == null)
            {
                throw new NullReferenceException("widget");
            }
            if (alignment == null)
            {
                throw new NullReferenceException("alignment");
            }
            WidgetSpring ws = _widgetSprings[widget];
            if (ws != null)
            {
                System.Diagnostics.Debug.Assert(widget.GetParent() == this);
                ws._alignment = alignment;
                return true;
            }
            return false;
        }

        protected void RecheckWidgets()
        {
            if (_horzGroup != null)
            {
                _horzGroup.RecheckWidgets();
            }
            if (_vertGroup != null)
            {
                _vertGroup.RecheckWidgets();
            }
        }

        protected void LayoutGroupsChanged()
        {
            _redoDefaultGaps = true;
            MaybeInvalidateLayoutTree();
        }

        protected void MaybeInvalidateLayoutTree()
        {
            if (_horzGroup != null && _vertGroup != null && !_blockInvalidateLayoutTree)
            {
                InvalidateLayout();
            }
        }

        //@Override
        protected override void ChildVisibilityChanged(Widget child)
        {
            if (!_bIncludeInvisibleWidgets)
            {
                LayoutGroupsChanged(); // this will also clear isPrepared
            }
        }

        void RemoveChild(WidgetSpring widgetSpring)
        {
            Widget widget = widgetSpring._w;
            int idx = GetChildIndex(widget);
            System.Diagnostics.Debug.Assert(idx >= 0);
            base.RemoveChild(idx);
            _widgetSprings.Remove(widget);
        }

        public class Gap
        {
            public int Min;
            public int Preferred;
            public int Max;

            public Gap() : this(0, 0, 32767)
            {
                
            }

            public Gap(int size) : this(size, size, size)
            {

            }

            public Gap(Utils.Number size) : this(size.IntValue(), size.IntValue(), size.IntValue())
            {

            }

            public Gap(int min, int preferred) : this(min, preferred, 32767)
            {

            }

            public Gap(Utils.Number min, Utils.Number preferred) : this(min.IntValue(), preferred.IntValue(), 32767)
            {

            }

            public Gap(Utils.Number min, Utils.Number preferred, Utils.Number max) : this(min.IntValue(), preferred.IntValue(), max.IntValue())
            {

            }

            public Gap(int min, int preferred, int max)
            {
                if (min < 0)
                {
                    throw new ArgumentOutOfRangeException("min");
                }
                if (preferred < min)
                {
                    throw new ArgumentOutOfRangeException("preferred");
                }
                if (max < 0 || (max > 0 && max < preferred))
                {
                    throw new ArgumentOutOfRangeException("max");
                }
                this.Min = min;
                this.Preferred = preferred;
                this.Max = max;
            }
        }

        public abstract class Spring
        {
            internal abstract int GetMinSize(int axis);
            internal abstract int GetPrefSize(int axis);
            internal abstract int GetMaxSize(int axis);
            internal abstract void SetSize(int axis, int pos, int size);

            //internal DialogLayout _dialogLayout;

            internal Spring()
            {
                //this._dialogLayout = dialogLayout;
            }

            void CollectAllSprings(HashSet<Spring> result)
            {
                result.Add(this);
            }

            internal virtual bool IsVisible()
            {
                return true;
            }
        }

        public class WidgetSpring : Spring
        {
            internal Widget _w;
            internal Alignment _alignment;
            int _x;
            int _y;
            int _width;
            int _height;
            int _minWidth;
            int _minHeight;
            int _maxWidth;
            int _maxHeight;
            int _prefWidth;
            int _prefHeight;
            int _flags;
            private DialogLayout _dialogLayout;

            internal WidgetSpring(DialogLayout dialogLayout, Widget w)
            {
                this._dialogLayout = dialogLayout;
                this._w = w;
                this._alignment = Alignment.FILL;
            }

            internal void prepare()
            {
                this._x = _w.GetX();
                this._y = _w.GetY();
                this._width = _w.GetWidth();
                this._height = _w.GetHeight();
                this._minWidth = _w.GetMinWidth();
                this._minHeight = _w.GetMinHeight();
                this._maxWidth = _w.GetMaxWidth();
                this._maxHeight = _w.GetMaxHeight();
                this._prefWidth = ComputeSize(_minWidth, _w.GetPreferredWidth(), _maxWidth);
                this._prefHeight = ComputeSize(_minHeight, _w.GetPreferredHeight(), _maxHeight);
                this._flags = 0;
            }

            internal override int GetMinSize(int axis)
            {
                if (axis == DialogLayout.AXIS_X)
                {
                    return _minWidth;
                }
                else if (axis == DialogLayout.AXIS_Y)
                {
                    return _minHeight;
                }

                throw new ArgumentOutOfRangeException("axis");
            }

            internal override int GetPrefSize(int axis)
            {
                if (axis == DialogLayout.AXIS_X)
                {
                    return _prefWidth;
                }
                else if (axis == DialogLayout.AXIS_Y)
                {
                    return _prefHeight;
                }

                throw new ArgumentOutOfRangeException("axis");
            }

            internal override int GetMaxSize(int axis)
            {
                if (axis == DialogLayout.AXIS_X)
                {
                    return _maxWidth;
                }
                else if (axis == DialogLayout.AXIS_Y)
                {
                    return _maxHeight;
                }

                throw new ArgumentOutOfRangeException("axis");
            }

            internal override void SetSize(int axis, int pos, int size)
            {
                this._flags |= 1 << axis;

                if (axis == DialogLayout.AXIS_X)
                {
                    this._x = pos;
                    this._width = size;
                    return;
                }
                else if (axis == DialogLayout.AXIS_Y)
                {
                    this._y = pos;
                    this._height = size;
                    return;
                }

                throw new ArgumentOutOfRangeException("axis");
            }

            internal void Apply()
            {
                if (_flags != 3)
                {
                    InvalidState();
                }
                if (_alignment != Alignment.FILL)
                {
                    int newWidth = Math.Min(_width, _prefWidth);
                    int newHeight = Math.Min(_height, _prefHeight);
                    _w.SetPosition(
                            _x + _alignment.ComputePositionX(_width, newWidth),
                            _y + _alignment.ComputePositionY(_height, newHeight));
                    _w.SetSize(newWidth, newHeight);
                }
                else
                {
                    _w.SetPosition(_x, _y);
                    _w.SetSize(_width, _height);
                }
            }

            internal override bool IsVisible()
            {
                return _w.IsVisible();
            }

            void InvalidState()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Widget ").Append(_w)
                        .Append(" with theme ").Append(_w.GetTheme())
                        .Append(" is not part of the following groups:");
                if ((_flags & (1 << AXIS_X)) == 0)
                {
                    sb.Append(" horizontal");
                }
                if ((_flags & (1 << AXIS_Y)) == 0)
                {
                    sb.Append(" vertical");
                }
                throw new InvalidOperationException(sb.ToString());
            }
        }

        private class GapSpring : Spring
        {
            int _min;
            int _pref;
            int _max;
            internal bool _isDefault;
            private DialogLayout _dialogLayout;

            internal GapSpring(DialogLayout dialogLayout, int min, int pref, int max, bool isDefault)
            {
                this._dialogLayout = dialogLayout;
                ConvertConstant(AXIS_X, min);
                ConvertConstant(AXIS_X, pref);
                ConvertConstant(AXIS_X, max);
                this._min = min;
                this._pref = pref;
                this._max = max;
                this._isDefault = isDefault;
            }

            internal override int GetMinSize(int axis)
            {
                return ConvertConstant(axis, _min);
            }

            internal override int GetPrefSize(int axis)
            {
                return ConvertConstant(axis, _pref);
            }

            internal override int GetMaxSize(int axis)
            {
                return ConvertConstant(axis, _max);
            }

            internal override void SetSize(int axis, int pos, int size)
            {
            }

            private int ConvertConstant(int axis, int value)
            {
                if (value >= 0)
                {
                    return value;
                }
                Dimension dim;
                if (value == SMALL_GAP)
                {
                    dim = this._dialogLayout._smallGap;
                }
                else if (value == MEDIUM_GAP)
                {
                    dim = this._dialogLayout._mediumGap;
                }
                else if (value == LARGE_GAP)
                {
                    dim = this._dialogLayout._largeGap;
                }
                else if (value == DEFAULT_GAP)
                {
                    dim = this._dialogLayout._defaultGap;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Invalid gap size: " + value);
                }

                if (dim == null)
                {
                    return 0;
                }
                else if (axis == AXIS_X)
                {
                    return dim.X;
                }
                else
                {
                    return dim.Y;
                }
            }
        }

        static Gap NO_GAP = new Gap(0, 0, 32767);

        private class NamedGapSpring : Spring
        {
            String _name;
            private DialogLayout _dialogLayout;

            public NamedGapSpring(DialogLayout dialogLayout, String name)
            {
                this._dialogLayout = dialogLayout;
                this._name = name;
            }

            internal override int GetMaxSize(int axis)
            {
                return GetGap().Max;
            }

            internal override int GetMinSize(int axis)
            {
                return GetGap().Min;
            }

            internal override int GetPrefSize(int axis)
            {
                return GetGap().Preferred;
            }

            internal override void SetSize(int axis, int pos, int size)
            {
            }

            private Gap GetGap()
            {
                if (this._dialogLayout._namedGaps != null)
                {
                    return this._dialogLayout._namedGaps.GetParameterValue(_name, true, typeof(Gap), NO_GAP);
                }
                return NO_GAP;
            }
        }

        public abstract class Group : Spring
        {
            internal List<Spring> _springs = new List<Spring>();
            bool _alreadyAdded;
            internal DialogLayout _dialogLayout;

            public Group(DialogLayout dialogLayout)
            {
                this._dialogLayout = dialogLayout;
            }

            internal void CheckGroup(DialogLayout owner)
            {
                if (this._dialogLayout != owner) {
                    throw new InvalidOperationException("Can't add group from different layout");
                }
                if (_alreadyAdded)
                {
                    throw new InvalidOperationException("Group already added to another group");
                }
            }

            /**
             * Adds another group. A group can only be added once.
             *
             * WARNING: No check is made to prevent cycles.
             * 
             * @param g the child Group
             * @return this Group
             */
            public Group AddGroup(Group g)
            {
                g.CheckGroup(this._dialogLayout);
                g._alreadyAdded = true;
                AddSpring(g);
                return this;
            }

            /**
             * Adds several groups. A group can only be added once.
             *
             * WARNING: No check is made to prevent cycles.
             *
             * @param groups the groups to add
             * @return this Group
             */
            public Group AddGroups(params Group[] groups)
            {
                foreach (Group g in groups)
                {
                    AddGroup(g);
                }
                return this;
            }

            /**
             * Adds a widget to this group.
             *
             * <p>If the widget is already a child widget of the DialogLayout then it
             * keeps it current settings, otherwise it is added the alignment is set
             * to {@link Alignment#FILL}.</p>
             *
             * @param w the child widget.
             * @return this Group
             * @see Widget#add(de.matthiasmann.twl.Widget)
             */
            public Group AddWidget(Widget w)
            {
                if (w.GetParent() != this._dialogLayout) {
                    this._dialogLayout.Add(w);
                }
                WidgetSpring s = this._dialogLayout._widgetSprings[w];
                if (s == null)
                {
                    throw new InvalidOperationException("WidgetSpring for Widget not found: " + w);
                }
                AddSpring(s);
                return this;
            }

            /**
             * Adds a widget to this group.
             *
             * <p>If the widget is already a child widget of the DialogLayout then it
             * it's alignment is set to the specified value overwriting any current
             * alignment setting, otherwise it is added to the DialogLayout.</p>
             *
             * @param w the child widget.
             * @param alignment the alignment of the child widget.
             * @return this Group
             * @see Widget#add(de.matthiasmann.twl.Widget) 
             * @see #setWidgetAlignment(de.matthiasmann.twl.Widget, de.matthiasmann.twl.Alignment)
             */
            public Group AddWidget(Widget w, Alignment alignment)
            {
                this.AddWidget(w);
                this._dialogLayout.SetWidgetAlignment(w, alignment);
                return this;
            }

            /**
             * Adds several widgets to this group. The widget is automatically added as child widget.
             * 
             * @param widgets The widgets which should be added.
             * @return this Group
             */
            public Group AddWidgets(params Widget[] widgets)
            {
                foreach (Widget w in widgets)
                {
                    AddWidget(w);
                }
                return this;
            }

            /**
             * Adds several widgets to this group, inserting the specified gap in between.
             * Each widget also gets an animation state set depending on it's position.
             *
             * The state gapName+"NotFirst" is set to false for widgets[0] and true for all others
             * The state gapName+"NotLast" is set to false for widgets[n-1] and true for all others
             *
             * @param gapName the name of the gap to insert between widgets
             * @param widgets The widgets which should be added.
             * @return this Group
             */
            public Group AddWidgetsWithGap(String gapName, params Widget[] widgets)
            {
                StateKey stateNotFirst = StateKey.Get(gapName + ("NotFirst"));
                StateKey stateNotLast = StateKey.Get(gapName + ("NotLast"));
                for (int i = 0, n = widgets.Length; i < n; i++)
                {
                    if (i > 0)
                    {
                        AddGap(gapName);
                    }
                    Widget w = widgets[i];
                    AddWidget(w);
                    AnimationState animationState = w.GetAnimationState();
                    animationState.SetAnimationState(stateNotFirst, i > 0);
                    animationState.SetAnimationState(stateNotLast, i < n - 1);
                }
                return this;
            }

            /**
             * Adds a generic gap. Can use symbolic gap names.
             *
             * @param min the minimum size in pixels or a symbolic constant
             * @param pref the preferred size in pixels or a symbolic constant
             * @param max the maximum size in pixels or a symbolic constant
             * @return this Group
             * @see DialogLayout#SMALL_GAP
             * @see DialogLayout#MEDIUM_GAP
             * @see DialogLayout#LARGE_GAP
             * @see DialogLayout#DEFAULT_GAP
             */
            public Group AddGap(int min, int pref, int max)
            {
                AddSpring(new GapSpring(this._dialogLayout, min, pref, max, false));
                return this;
            }

            /**
             * Adds a fixed sized gap. Can use symbolic gap names.
             *
             * @param size the size in pixels or a symbolic constant
             * @return this Group
             * @see DialogLayout#SMALL_GAP
             * @see DialogLayout#MEDIUM_GAP
             * @see DialogLayout#LARGE_GAP
             * @see DialogLayout#DEFAULT_GAP
             */
            public Group AddGap(int size)
            {
                AddSpring(new GapSpring(this._dialogLayout, size, size, size, false));
                return this;
            }

            /**
             * Adds a gap with minimum size. Can use symbolic gap names.
             *
             * @param minSize the minimum size in pixels or a symbolic constant
             * @return this Group
             * @see DialogLayout#SMALL_GAP
             * @see DialogLayout#MEDIUM_GAP
             * @see DialogLayout#LARGE_GAP
             * @see DialogLayout#DEFAULT_GAP
             */
            public Group AddMinGap(int minSize)
            {
                AddSpring(new GapSpring(this._dialogLayout, minSize, minSize, short.MaxValue, false));
                return this;
            }

            /**
             * Adds a flexible gap with no minimum size.
             *
             * <p>This is equivalent to {@code addGap(0, 0, Short.MAX_VALUE) }</p>
             * @return this Group
             */
            public virtual Group AddGap()
            {
                AddSpring(new GapSpring(this._dialogLayout, 0, 0, short.MaxValue, false));
                return this;
            }

            /**
             * Adds a named gap.
             * 
             * <p>Named gaps are configured via the theme parameter "namedGaps" which
             * maps from names to &lt;gap&gt; objects.</p>
             * 
             * <p>They behave equal to {@link #addGap(int, int, int) }.</p>
             * 
             * @param name the name of the gap (vcase sensitive)
             * @return this Group
             */
            public Group AddGap(String name)
            {
                if (name.Length == 0)
                {
                    throw new ArgumentOutOfRangeException("name");
                }
                AddSpring(new NamedGapSpring(this._dialogLayout, name));
                return this;
            }

            /**
             * Remove all default gaps from this and child groups
             */
            public void RemoveDefaultGaps()
            {
                for (int i = _springs.Count; i-- > 0;)
                {
                    Spring s = _springs[i];
                    if (s is GapSpring)
                    {
                        if (((GapSpring)s)._isDefault)
                        {
                            _springs.RemoveAt(i);
                        }
                    }
                    else if (s is Group)
                    {
                        ((Group)s).RemoveDefaultGaps();
                    }
                }
            }

            /**
             * Add a default gap between all children except if the neighbour is already a Gap.
             */
            public virtual void AddDefaultGap()
            {
                for (int i = 0; i < _springs.Count; i++)
                {
                    Spring s = _springs[i];
                    if (s is Group)
                    {
                        ((Group)s).AddDefaultGap();
                    }
                }
            }

            /**
             * Removes the specified group from this group.
             * 
             * @param g the group to remove
             * @param removeWidgets if true all widgets in the specified group
             *      should be removed from the {@code DialogLayout}
             * @return true if it was found and removed, false otherwise
             */
            public bool RemoveGroup(Group g, bool removeWidgets)
            {
                for (int i = 0; i < _springs.Count; i++)
                {
                    if (_springs[i] == g)
                    {
                        _springs.RemoveAt(i);
                        if (removeWidgets)
                        {
                            g.RemoveWidgets();
                            this._dialogLayout.RecheckWidgets();
                        }
                        this._dialogLayout.LayoutGroupsChanged();
                        return true;
                    }
                }
                return false;
            }

            /**
             * Removes all elements from this group
             *
             * @param removeWidgets if true all widgets in this group are removed
             *      from the {@code DialogLayout}
             */
            public void Clear(bool bRemoveWidgets)
            {
                if (bRemoveWidgets)
                {
                    RemoveWidgets();
                }
                _springs.Clear();
                if (bRemoveWidgets)
                {
                    this._dialogLayout.RecheckWidgets();
                }
                this._dialogLayout.LayoutGroupsChanged();
            }

            internal void AddSpring(Spring s)
            {
                _springs.Add(s);
                this._dialogLayout.LayoutGroupsChanged();
            }

            internal void RecheckWidgets()
            {
                for (int i = _springs.Count; i-- > 0;)
                {
                    Spring s = _springs[i];
                    if (s is WidgetSpring)
                    {
                        if (!this._dialogLayout._widgetSprings.ContainsKey(((WidgetSpring)s)._w))
                        {
                            _springs.RemoveAt(i);
                        }
                    }
                    else if (s is Group)
                    {
                        ((Group)s).RecheckWidgets();
                    }
                }
            }

            void RemoveWidgets()
            {
                for (int i = _springs.Count; i-- > 0;)
                {
                    Spring s = _springs[i];
                    if (s is WidgetSpring)
                    {
                        this._dialogLayout.RemoveChild((WidgetSpring)s);
                    }
                    else if (s is Group)
                    {
                        ((Group)s).RemoveWidgets();
                    }
                }
            }
        }

        class SpringDelta : IComparable<SpringDelta> {
            internal int _idx;
            internal int _delta;

            internal SpringDelta(int idx, int delta)
            {
                this._idx = idx;
                this._delta = delta;
            }

            public int CompareTo(SpringDelta o)
            {
                return _delta - o._delta;
            }
        }

        public class SequentialGroup : Group
        {
            public SequentialGroup(DialogLayout dialogLayout) : base(dialogLayout)
            {
            }

            internal override int GetMinSize(int axis)
            {
                int size = 0;
                for (int i = 0, n = _springs.Count; i < n; i++)
                {
                    Spring s = _springs[i];
                    if (this._dialogLayout._bIncludeInvisibleWidgets || s.IsVisible())
                    {
                        size += s.GetMinSize(axis);
                    }
                }
                return size;
            }

            internal override int GetPrefSize(int axis)
            {
                int size = 0;
                for (int i = 0, n = _springs.Count; i < n; i++)
                {
                    Spring s = _springs[i];
                    if (this._dialogLayout._bIncludeInvisibleWidgets || s.IsVisible())
                    {
                        size += s.GetPrefSize(axis);
                    }
                }
                return size;
            }

            internal override int GetMaxSize(int axis)
            {
                int size = 0;
                bool hasMax = false;
                for (int i = 0, n = _springs.Count; i < n; i++)
                {
                    Spring s = _springs[i];
                    if (this._dialogLayout._bIncludeInvisibleWidgets || s.IsVisible())
                    {
                        int max = s.GetMaxSize(axis);
                        if (max > 0)
                        {
                            size += max;
                            hasMax = true;
                        }
                        else
                        {
                            size += s.GetPrefSize(axis);
                        }
                    }
                }
                return hasMax ? size : 0;
            }

            /**
             * Add a default gap between all children except if the neighbour is already a Gap.
             */
            public override void AddDefaultGap()
            {
                if (_springs.Count > 1)
                {
                    bool wasGap = true;
                    for (int i = 0; i < _springs.Count; i++)
                    {
                        Spring s = _springs[i];
                        if (this._dialogLayout._bIncludeInvisibleWidgets || s.IsVisible())
                        {
                            bool isGap = (s is GapSpring) || (s is NamedGapSpring);
                            if (!isGap && !wasGap)
                            {
                                this._springs.Insert(i++, new GapSpring(this._dialogLayout, DEFAULT_GAP, DEFAULT_GAP, DEFAULT_GAP, true));
                            }
                            wasGap = isGap;
                        }
                    }
                }

                base.AddDefaultGap();
            }

            internal override void SetSize(int axis, int pos, int size)
            {
                int prefSize = GetPrefSize(axis);
                if (size == prefSize)
                {
                    foreach (Spring s in _springs)
                    {
                        if (this._dialogLayout._bIncludeInvisibleWidgets || s.IsVisible())
                        {
                            int spref = s.GetPrefSize(axis);
                            s.SetSize(axis, pos, spref);
                            pos += spref;
                        }
                    }
                }
                else if (_springs.Count == 1)
                {
                    // no need to check visibility flag
                    Spring s = _springs[0];
                    s.SetSize(axis, pos, size);
                }
                else if (_springs.Count > 1)
                {
                    SetSizeNonPref(axis, pos, size, prefSize);
                }
            }

            private void SetSizeNonPref(int axis, int pos, int size, int prefSize)
            {
                int delta = size - prefSize;
                bool useMin = delta < 0;
                if (useMin)
                {
                    delta = -delta;
                }

                SpringDelta[] deltas = new SpringDelta[_springs.Count];
                int resizeable = 0;
                for (int i = 0; i < _springs.Count; i++)
                {
                    Spring s = _springs[i];
                    if (this._dialogLayout._bIncludeInvisibleWidgets || s.IsVisible())
                    {
                        int sdelta = useMin
                                ? s.GetPrefSize(axis) - s.GetMinSize(axis)
                                : s.GetMaxSize(axis) - s.GetPrefSize(axis);
                        if (sdelta > 0)
                        {
                            deltas[resizeable++] = new SpringDelta(i, sdelta);
                        }
                    }
                }
                if (resizeable > 0)
                {
                    if (resizeable > 1)
                    {
                        Array.Sort(deltas, 0, resizeable);
                    }

                    int[] sizes = new int[_springs.Count];

                    int remaining = resizeable;
                    for (int i = 0; i < resizeable; i++)
                    {
                        SpringDelta d = deltas[i];

                        int sdelta = delta / remaining;
                        int ddelta = Math.Min(d._delta, sdelta);
                        delta -= ddelta;
                        remaining--;

                        if (useMin)
                        {
                            ddelta = -ddelta;
                        }
                        sizes[d._idx] = ddelta;
                    }

                    for (int i = 0; i < _springs.Count; i++)
                    {
                        Spring s = _springs[i];
                        if (this._dialogLayout._bIncludeInvisibleWidgets || s.IsVisible())
                        {
                            int ssize = s.GetPrefSize(axis) + sizes[i];
                            s.SetSize(axis, pos, ssize);
                            pos += ssize;
                        }
                    }
                }
                else
                {
                    foreach (Spring s in _springs)
                    {
                        if (this._dialogLayout._bIncludeInvisibleWidgets || s.IsVisible())
                        {
                            int ssize;
                            if (useMin)
                            {
                                ssize = s.GetMinSize(axis);
                            }
                            else
                            {
                                ssize = s.GetMaxSize(axis);
                                if (ssize == 0)
                                {
                                    ssize = s.GetPrefSize(axis);
                                }
                            }
                            s.SetSize(axis, pos, ssize);
                            pos += ssize;
                        }
                    }
                }
            }
        }

        public class ParallelGroup : Group
        {
            public ParallelGroup(DialogLayout dialogLayout) : base(dialogLayout)
            {
            }

            override internal int GetMinSize(int axis)
            {
                int size = 0;
                for (int i = 0, n = _springs.Count; i < n; i++)
                {
                    Spring s = _springs[i];
                    if (this._dialogLayout._bIncludeInvisibleWidgets || s.IsVisible())
                    {
                        size = Math.Max(size, s.GetMinSize(axis));
                    }
                }
                return size;
            }

            override internal int GetPrefSize(int axis)
            {
                int size = 0;
                for (int i = 0, n = _springs.Count; i < n; i++)
                {
                    Spring s = _springs[i];
                    if (this._dialogLayout._bIncludeInvisibleWidgets || s.IsVisible())
                    {
                        size = Math.Max(size, s.GetPrefSize(axis));
                    }
                }
                return size;
            }

            override internal int GetMaxSize(int axis)
            {
                int size = 0;
                for (int i = 0, n = _springs.Count; i < n; i++)
                {
                    Spring s = _springs[i];
                    if (this._dialogLayout._bIncludeInvisibleWidgets || s.IsVisible())
                    {
                        size = Math.Max(size, s.GetMaxSize(axis));
                    }
                }
                return size;
            }
            
            override internal void SetSize(int axis, int pos, int size)
            {
                for (int i = 0, n = _springs.Count; i < n; i++)
                {
                    Spring s = _springs[i];
                    if (this._dialogLayout._bIncludeInvisibleWidgets || s.IsVisible())
                    {
                        s.SetSize(axis, pos, size);
                    }
                }
            }

            public override Group AddGap()
            {
                GetLogger().Log(Level.WARNING, "Useless call to addGap() on ParallelGroup", new Exception());
                return this;
            }
        }
    }
}
