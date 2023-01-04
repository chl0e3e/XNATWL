using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
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

        private static bool DEBUG_LAYOUT_GROUPS = Widget.DEBUG_LAYOUT_GROUPS;

        protected Dimension smallGap;
        protected Dimension mediumGap;
        protected Dimension largeGap;
        protected Dimension defaultGap;
        protected ParameterMap namedGaps;

        protected bool bAddDefaultGaps = true;
        protected bool bIncludeInvisibleWidgets = true;
        protected bool redoDefaultGaps;
        protected bool isPrepared;
        protected bool blockInvalidateLayoutTree;
        protected bool bWarnOnIncomplete;

        private Group horz;
        private Group vert;

        /**
         * Debugging aid. Captures the stack trace where one of the group was last assigned.
         */
        Exception debugStackTrace;

        Dictionary<Widget, WidgetSpring> widgetSprings;

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
            widgetSprings = new Dictionary<Widget, WidgetSpring>();
            collectDebugStack();
        }

        public Group getHorizontalGroup()
        {
            return horz;
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
        public void setHorizontalGroup(Group g)
        {
            if (g != null)
            {
                g.checkGroup(this);
            }
            this.horz = g;
            collectDebugStack();
            layoutGroupsChanged();
        }

        public Group getVerticalGroup()
        {
            return vert;
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
        public void setVerticalGroup(Group g)
        {
            if (g != null)
            {
                g.checkGroup(this);
            }
            this.vert = g;
            collectDebugStack();
            layoutGroupsChanged();
        }

        public Dimension getSmallGap()
        {
            return smallGap;
        }

        public void setSmallGap(Dimension smallGap)
        {
            this.smallGap = smallGap;
            maybeInvalidateLayoutTree();
        }

        public Dimension getMediumGap()
        {
            return mediumGap;
        }

        public void setMediumGap(Dimension mediumGap)
        {
            this.mediumGap = mediumGap;
            maybeInvalidateLayoutTree();
        }

        public Dimension getLargeGap()
        {
            return largeGap;
        }

        public void setLargeGap(Dimension largeGap)
        {
            this.largeGap = largeGap;
            maybeInvalidateLayoutTree();
        }

        public Dimension getDefaultGap()
        {
            return defaultGap;
        }

        public void setDefaultGap(Dimension defaultGap)
        {
            this.defaultGap = defaultGap;
            maybeInvalidateLayoutTree();
        }

        public bool isAddDefaultGaps()
        {
            return bAddDefaultGaps;
        }

        /**
         * Determine whether default gaps should be added from the theme or not.
         * 
         * @param addDefaultGaps if true then default gaps are added.
         */
        public void setAddDefaultGaps(bool bAddDefaultGaps)
        {
            this.bAddDefaultGaps = bAddDefaultGaps;
        }

        /**
         * removes all default gaps from all groups.
         */
        public void removeDefaultGaps()
        {
            if (horz != null && vert != null)
            {
                horz.removeDefaultGaps();
                vert.removeDefaultGaps();
                maybeInvalidateLayoutTree();
            }
        }

        /**
         * Adds theme dependant default gaps to all groups.
         */
        public void addDefaultGaps()
        {
            if (horz != null && vert != null)
            {
                horz.addDefaultGap();
                vert.addDefaultGap();
                maybeInvalidateLayoutTree();
            }
        }

        public bool isIncludeInvisibleWidgets()
        {
            return bIncludeInvisibleWidgets;
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
        public void setIncludeInvisibleWidgets(bool includeInvisibleWidgets)
        {
            if (this.bIncludeInvisibleWidgets != includeInvisibleWidgets)
            {
                this.bIncludeInvisibleWidgets = includeInvisibleWidgets;
                layoutGroupsChanged();
            }
        }

        private void collectDebugStack()
        {
            bWarnOnIncomplete = true;
            if (DEBUG_LAYOUT_GROUPS)
            {
                debugStackTrace = new Exception("DialogLayout created/used here");
            }
        }

        private void warnOnIncomplete()
        {
            bWarnOnIncomplete = false;
            getLogger().log(Level.WARNING, "Dialog layout has incomplete state", debugStackTrace);
        }

        static Logger getLogger()
        {
            return Logger.GetLogger(typeof(DialogLayout));
        }

        protected void applyThemeDialogLayout(ThemeInfo themeInfo)
        {
            try
            {
                blockInvalidateLayoutTree = true;
                setSmallGap(themeInfo.getParameterValue("smallGap", true, typeof(Dimension), Dimension.ZERO));
                setMediumGap(themeInfo.getParameterValue("mediumGap", true, typeof(Dimension), Dimension.ZERO));
                setLargeGap(themeInfo.getParameterValue("largeGap", true, typeof(Dimension), Dimension.ZERO));
                setDefaultGap(themeInfo.getParameterValue("defaultGap", true, typeof(Dimension), Dimension.ZERO));
                namedGaps = themeInfo.getParameterMap("namedGaps");
            }
            finally
            {
                blockInvalidateLayoutTree = false;
            }
            invalidateLayout();
        }

        //@Override
        protected override void applyTheme(ThemeInfo themeInfo)
        {
            base.applyTheme(themeInfo);
            applyThemeDialogLayout(themeInfo);
        }

        //@Override
        public override int getMinWidth()
        {
            if (horz != null)
            {
                prepare();
                return horz.getMinSize(AXIS_X) + getBorderHorizontal();
            }
            return base.getMinWidth();
        }

        //@Override
        public override int getMinHeight()
        {
            if (vert != null)
            {
                prepare();
                return vert.getMinSize(AXIS_Y) + getBorderVertical();
            }
            return base.getMinHeight();
        }

        //@Override
        public override int getPreferredInnerWidth()
        {
            if (horz != null)
            {
                prepare();
                return horz.getPrefSize(DialogLayout.AXIS_X);
            }
            return base.getPreferredInnerWidth();
        }

        //@Override
        public override int getPreferredInnerHeight()
        {
            if (vert != null)
            {
                prepare();
                return vert.getPrefSize(AXIS_Y);
            }
            return base.getPreferredInnerHeight();
        }

        //@Override
        public override void adjustSize()
        {
            if (horz != null && vert != null)
            {
                prepare();
                int minWidth = horz.getMinSize(AXIS_X);
                int minHeight = vert.getMinSize(AXIS_Y);
                int prefWidth = horz.getPrefSize(AXIS_X);
                int prefHeight = vert.getPrefSize(AXIS_Y);
                int maxWidth = getMaxWidth();
                int maxHeight = getMaxHeight();
                setInnerSize(
                        computeSize(minWidth, prefWidth, maxWidth),
                        computeSize(minHeight, prefHeight, maxHeight));
                doLayout();
            }
        }

        //@Override
        protected override void layout()
        {
            if (horz != null && vert != null)
            {
                prepare();
                doLayout();
            }
            else if (bWarnOnIncomplete)
            {
                warnOnIncomplete();
            }
        }

        protected void prepare()
        {
            if (redoDefaultGaps)
            {
                if (bAddDefaultGaps)
                {
                    try
                    {
                        blockInvalidateLayoutTree = true;
                        removeDefaultGaps();
                        addDefaultGaps();
                    }
                    finally
                    {
                        blockInvalidateLayoutTree = false;
                    }
                }
                redoDefaultGaps = false;
                isPrepared = false;
            }
            if (!isPrepared)
            {
                foreach (WidgetSpring s in widgetSprings.Values)
                {
                    if (bIncludeInvisibleWidgets || s.w.isVisible())
                    {
                        s.prepare();
                    }
                }
                isPrepared = true;
            }
        }

        protected void doLayout()
        {
            horz.setSize(AXIS_X, getInnerX(), getInnerWidth());
            vert.setSize(AXIS_Y, getInnerY(), getInnerHeight());
            try
            {
                foreach (WidgetSpring s in widgetSprings.Values)
                {
                    if (bIncludeInvisibleWidgets || s.w.isVisible())
                    {
                        s.apply();
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                if (debugStackTrace != null && ex.InnerException == null)
                {
                    throw new InvalidOperationException(ex.Message, debugStackTrace);
                }
                throw ex;
            }
        }

        //@Override
        public override void invalidateLayout()
        {
            isPrepared = false;
            base.invalidateLayout();
        }

        //@Override
        protected override void paintWidget(GUI gui)
        {
            isPrepared = false;
            // super.paintWidget() is empty
        }

        //@Override
        protected override void sizeChanged()
        {
            isPrepared = false;
            base.sizeChanged();
        }

        //@Override
        protected override void afterAddToGUI(GUI gui)
        {
            isPrepared = false;
            base.afterAddToGUI(gui);
        }

        /**
         * Creates a new parallel group.
         * All children in a parallel group share the same position and size of it's axis.
         *
         * @return the new parallel Group.
         */
        public Group createParallelGroup()
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
        public Group createParallelGroup(params Widget[] widgets)
        {
            return createParallelGroup().addWidgets(widgets);
        }

        /**
         * Creates a parallel group and adds the specified groups.
         *
         * @see #createParallelGroup()
         * @param groups the groups to add
         * @return a new parallel Group.
         */
        public Group createParallelGroup(params Group[] groups)
        {
            return createParallelGroup().addGroups(groups);
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
        public Group createSequentialGroup()
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
        public Group createSequentialGroup(params Widget[] widgets)
        {
            return createSequentialGroup().addWidgets(widgets);
        }

        /**
         * Creates a sequential group and adds the specified groups.
         *
         * @see #createSequentialGroup()
         * @param groups the groups to add
         * @return a new sequential Group.
         */
        public Group createSequentialGroup(params Group[] groups)
        {
            return createSequentialGroup().addGroups(groups);
        }

        //@Override
        public override void insertChild(Widget child, int index)
        {
            base.insertChild(child, index);
            widgetSprings.Add(child, new WidgetSpring(this, child));
        }

        //@Override
        public override void removeAllChildren()
        {
            base.removeAllChildren();
            widgetSprings.Clear();
            recheckWidgets();
            layoutGroupsChanged();
        }

        //@Override
        public override Widget removeChild(int index)
        {
            Widget widget = base.removeChild(index);
            widgetSprings.Remove(widget);
            recheckWidgets();
            layoutGroupsChanged();
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
        public bool setWidgetAlignment(Widget widget, Alignment alignment)
        {
            if (widget == null)
            {
                throw new NullReferenceException("widget");
            }
            if (alignment == null)
            {
                throw new NullReferenceException("alignment");
            }
            WidgetSpring ws = widgetSprings[widget];
            if (ws != null)
            {
                System.Diagnostics.Debug.Assert(widget.getParent() == this);
                ws.alignment = alignment;
                return true;
            }
            return false;
        }

        protected void recheckWidgets()
        {
            if (horz != null)
            {
                horz.recheckWidgets();
            }
            if (vert != null)
            {
                vert.recheckWidgets();
            }
        }

        protected void layoutGroupsChanged()
        {
            redoDefaultGaps = true;
            maybeInvalidateLayoutTree();
        }

        protected void maybeInvalidateLayoutTree()
        {
            if (horz != null && vert != null && !blockInvalidateLayoutTree)
            {
                invalidateLayout();
            }
        }

        //@Override
        protected void childVisibilityChanged(Widget child)
        {
            if (!bIncludeInvisibleWidgets)
            {
                layoutGroupsChanged(); // this will also clear isPrepared
            }
        }

        void removeChild(WidgetSpring widgetSpring)
        {
            Widget widget = widgetSpring.w;
            int idx = getChildIndex(widget);
            System.Diagnostics.Debug.Assert(idx >= 0);
            base.removeChild(idx);
            widgetSprings.Remove(widget);
        }

        public class Gap
        {
            public int min;
            public int preferred;
            public int max;

            public Gap() : this(0, 0, 32767)
            {
                
            }
            public Gap(int size) : this(size, size, size)
            {
                
            }
            public Gap(int min, int preferred) : this(min, preferred, 32767)
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
                this.min = min;
                this.preferred = preferred;
                this.max = max;
            }
        }

        public abstract class Spring
        {
            internal abstract int getMinSize(int axis);
            internal abstract int getPrefSize(int axis);
            internal abstract int getMaxSize(int axis);
            internal abstract void setSize(int axis, int pos, int size);

            //internal DialogLayout _dialogLayout;

            internal Spring()
            {
                //this._dialogLayout = dialogLayout;
            }

            void collectAllSprings(HashSet<Spring> result)
            {
                result.Add(this);
            }

            internal virtual bool isVisible()
            {
                return true;
            }
        }

        public class WidgetSpring : Spring
        {
            internal Widget w;
            internal Alignment alignment;
            int x;
            int y;
            int width;
            int height;
            int minWidth;
            int minHeight;
            int maxWidth;
            int maxHeight;
            int prefWidth;
            int prefHeight;
            int flags;
            private DialogLayout _dialogLayout;

            internal WidgetSpring(DialogLayout dialogLayout, Widget w)
            {
                this._dialogLayout = dialogLayout;
                this.w = w;
                this.alignment = Alignment.FILL;
            }

            internal void prepare()
            {
                this.x = w.getX();
                this.y = w.getY();
                this.width = w.getWidth();
                this.height = w.getHeight();
                this.minWidth = w.getMinWidth();
                this.minHeight = w.getMinHeight();
                this.maxWidth = w.getMaxWidth();
                this.maxHeight = w.getMaxHeight();
                this.prefWidth = computeSize(minWidth, w.getPreferredWidth(), maxWidth);
                this.prefHeight = computeSize(minHeight, w.getPreferredHeight(), maxHeight);
                this.flags = 0;
            }

            //@Override
            internal override int getMinSize(int axis)
            {
                if (axis == DialogLayout.AXIS_X)
                {
                    return minWidth;
                }
                else if (axis == DialogLayout.AXIS_Y)
                {
                    return minHeight;
                }

                throw new ArgumentOutOfRangeException("axis");
            }

            //@Override
            internal override int getPrefSize(int axis)
            {
                if (axis == DialogLayout.AXIS_X)
                {
                    return prefWidth;
                }
                else if (axis == DialogLayout.AXIS_Y)
                {
                    return prefHeight;
                }

                throw new ArgumentOutOfRangeException("axis");
            }

            //@Override
            internal override int getMaxSize(int axis)
            {
                if (axis == DialogLayout.AXIS_X)
                {
                    return maxWidth;
                }
                else if (axis == DialogLayout.AXIS_Y)
                {
                    return maxHeight;
                }

                throw new ArgumentOutOfRangeException("axis");
            }

            //@Override
            internal override void setSize(int axis, int pos, int size)
            {
                this.flags |= 1 << axis;

                if (axis == DialogLayout.AXIS_X)
                {
                    this.x = pos;
                    this.width = size;
                    return;
                }
                else if (axis == DialogLayout.AXIS_Y)
                {
                    this.y = pos;
                    this.height = size;
                    return;
                }

                throw new ArgumentOutOfRangeException("axis");
            }

            internal void apply()
            {
                if (flags != 3)
                {
                    invalidState();
                }
                if (alignment != Alignment.FILL)
                {
                    int newWidth = Math.Min(width, prefWidth);
                    int newHeight = Math.Min(height, prefHeight);
                    w.setPosition(
                            x + alignment.computePositionX(width, newWidth),
                            y + alignment.computePositionY(height, newHeight));
                    w.setSize(newWidth, newHeight);
                }
                else
                {
                    w.setPosition(x, y);
                    w.setSize(width, height);
                }
            }

            //@Override
            internal override bool isVisible()
            {
                return w.isVisible();
            }

            void invalidState()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Widget ").Append(w)
                        .Append(" with theme ").Append(w.getTheme())
                        .Append(" is not part of the following groups:");
                if ((flags & (1 << AXIS_X)) == 0)
                {
                    sb.Append(" horizontal");
                }
                if ((flags & (1 << AXIS_Y)) == 0)
                {
                    sb.Append(" vertical");
                }
                throw new InvalidOperationException(sb.ToString());
            }
        }

        private class GapSpring : Spring
        {
            int min;
            int pref;
            int max;
            internal bool isDefault;
            private DialogLayout _dialogLayout;

            internal GapSpring(DialogLayout dialogLayout, int min, int pref, int max, bool isDefault)
            {
                this._dialogLayout = dialogLayout;
                convertConstant(AXIS_X, min);
                convertConstant(AXIS_X, pref);
                convertConstant(AXIS_X, max);
                this.min = min;
                this.pref = pref;
                this.max = max;
                this.isDefault = isDefault;
            }

            //@Override
            internal override int getMinSize(int axis)
            {
                return convertConstant(axis, min);
            }

            //@Override
            internal override int getPrefSize(int axis)
            {
                return convertConstant(axis, pref);
            }

            //@Override
            internal override int getMaxSize(int axis)
            {
                return convertConstant(axis, max);
            }

            //@Override
            internal override void setSize(int axis, int pos, int size)
            {
            }

            private int convertConstant(int axis, int value)
            {
                if (value >= 0)
                {
                    return value;
                }
                Dimension dim;
                if (value == SMALL_GAP)
                {
                    dim = this._dialogLayout.smallGap;
                }
                else if (value == MEDIUM_GAP)
                {
                    dim = this._dialogLayout.mediumGap;
                }
                else if (value == LARGE_GAP)
                {
                    dim = this._dialogLayout.largeGap;
                }
                else if (value == DEFAULT_GAP)
                {
                    dim = this._dialogLayout.defaultGap;
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
            String name;
            private DialogLayout _dialogLayout;

            public NamedGapSpring(DialogLayout dialogLayout, String name)
            {
                this._dialogLayout = dialogLayout;
                this.name = name;
            }

            //@Override
            internal override int getMaxSize(int axis)
            {
                return getGap().max;
            }

            //@Override
            internal override int getMinSize(int axis)
            {
                return getGap().min;
            }

            //@Override
            internal override int getPrefSize(int axis)
            {
                return getGap().preferred;
            }

            //@Override
            internal override void setSize(int axis, int pos, int size)
            {
            }

            private Gap getGap()
            {
                if (this._dialogLayout.namedGaps != null)
                {
                    return this._dialogLayout.namedGaps.getParameterValue(name, true, typeof(Gap), NO_GAP);
                }
                return NO_GAP;
            }
        }

        public abstract class Group : Spring
        {
            internal List<Spring> springs = new List<Spring>();
            bool alreadyAdded;
            internal DialogLayout _dialogLayout;

            public Group(DialogLayout dialogLayout)
            {
                this._dialogLayout = dialogLayout;
            }

            internal void checkGroup(DialogLayout owner)
            {
                if (this._dialogLayout != owner) {
                    throw new InvalidOperationException("Can't add group from different layout");
                }
                if (alreadyAdded)
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
            public Group addGroup(Group g)
            {
                g.checkGroup(this._dialogLayout);
                g.alreadyAdded = true;
                addSpring(g);
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
            public Group addGroups(params Group[] groups)
            {
                foreach (Group g in groups)
                {
                    addGroup(g);
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
            public Group addWidget(Widget w)
            {
                if (w.getParent() != this._dialogLayout) {
                    this._dialogLayout.add(w);
                }
                WidgetSpring s = this._dialogLayout.widgetSprings[w];
                if (s == null)
                {
                    throw new InvalidOperationException("WidgetSpring for Widget not found: " + w);
                }
                addSpring(s);
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
            public Group addWidget(Widget w, Alignment alignment)
            {
                this.addWidget(w);
                this._dialogLayout.setWidgetAlignment(w, alignment);
                return this;
            }

            /**
             * Adds several widgets to this group. The widget is automatically added as child widget.
             * 
             * @param widgets The widgets which should be added.
             * @return this Group
             */
            public Group addWidgets(params Widget[] widgets)
            {
                foreach (Widget w in widgets)
                {
                    addWidget(w);
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
            public Group addWidgetsWithGap(String gapName, params Widget[] widgets)
            {
                StateKey stateNotFirst = StateKey.Get(gapName + ("NotFirst"));
                StateKey stateNotLast = StateKey.Get(gapName + ("NotLast"));
                for (int i = 0, n = widgets.Length; i < n; i++)
                {
                    if (i > 0)
                    {
                        addGap(gapName);
                    }
                    Widget w = widgets[i];
                    addWidget(w);
                    AnimationState animationState = w.getAnimationState();
                    animationState.setAnimationState(stateNotFirst, i > 0);
                    animationState.setAnimationState(stateNotLast, i < n - 1);
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
            public Group addGap(int min, int pref, int max)
            {
                addSpring(new GapSpring(this._dialogLayout, min, pref, max, false));
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
            public Group addGap(int size)
            {
                addSpring(new GapSpring(this._dialogLayout, size, size, size, false));
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
            public Group addMinGap(int minSize)
            {
                addSpring(new GapSpring(this._dialogLayout, minSize, minSize, short.MaxValue, false));
                return this;
            }

            /**
             * Adds a flexible gap with no minimum size.
             *
             * <p>This is equivalent to {@code addGap(0, 0, Short.MAX_VALUE) }</p>
             * @return this Group
             */
            public virtual Group addGap()
            {
                addSpring(new GapSpring(this._dialogLayout, 0, 0, short.MaxValue, false));
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
            public Group addGap(String name)
            {
                if (name.Length == 0)
                {
                    throw new ArgumentOutOfRangeException("name");
                }
                addSpring(new NamedGapSpring(this._dialogLayout, name));
                return this;
            }

            /**
             * Remove all default gaps from this and child groups
             */
            public void removeDefaultGaps()
            {
                for (int i = springs.Count; i-- > 0;)
                {
                    Spring s = springs[i];
                    if (s is GapSpring)
                    {
                        if (((GapSpring)s).isDefault)
                        {
                            springs.RemoveAt(i);
                        }
                    }
                    else if (s is Group)
                    {
                        ((Group)s).removeDefaultGaps();
                    }
                }
            }

            /**
             * Add a default gap between all children except if the neighbour is already a Gap.
             */
            public virtual void addDefaultGap()
            {
                for (int i = 0; i < springs.Count; i++)
                {
                    Spring s = springs[i];
                    if (s is Group)
                    {
                        ((Group)s).addDefaultGap();
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
            public bool removeGroup(Group g, bool removeWidgets)
            {
                for (int i = 0; i < springs.Count; i++)
                {
                    if (springs[i] == g)
                    {
                        springs.RemoveAt(i);
                        if (removeWidgets)
                        {
                            g.removeWidgets();
                            this._dialogLayout.recheckWidgets();
                        }
                        this._dialogLayout.layoutGroupsChanged();
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
            public void clear(bool bRemoveWidgets)
            {
                if (bRemoveWidgets)
                {
                    removeWidgets();
                }
                springs.Clear();
                if (bRemoveWidgets)
                {
                    this._dialogLayout.recheckWidgets();
                }
                this._dialogLayout.layoutGroupsChanged();
            }

            internal void addSpring(Spring s)
            {
                springs.Add(s);
                this._dialogLayout.layoutGroupsChanged();
            }

            internal void recheckWidgets()
            {
                for (int i = springs.Count; i-- > 0;)
                {
                    Spring s = springs[i];
                    if (s is WidgetSpring)
                    {
                        if (!this._dialogLayout.widgetSprings.ContainsKey(((WidgetSpring)s).w))
                        {
                            springs.RemoveAt(i);
                        }
                    }
                    else if (s is Group)
                    {
                        ((Group)s).recheckWidgets();
                    }
                }
            }

            void removeWidgets()
            {
                for (int i = springs.Count; i-- > 0;)
                {
                    Spring s = springs[i];
                    if (s is WidgetSpring)
                    {
                        this._dialogLayout.removeChild((WidgetSpring)s);
                    }
                    else if (s is Group)
                    {
                        ((Group)s).removeWidgets();
                    }
                }
            }
        }

        class SpringDelta : IComparable<SpringDelta> {
            internal int idx;
            internal int delta;

            internal SpringDelta(int idx, int delta)
            {
                this.idx = idx;
                this.delta = delta;
            }

            public int CompareTo(SpringDelta o)
            {
                return delta - o.delta;
            }
        }

        public class SequentialGroup : Group
        {
            public SequentialGroup(DialogLayout dialogLayout) : base(dialogLayout)
            {
            }

            //@Override
            internal override int getMinSize(int axis)
            {
                int size = 0;
                for (int i = 0, n = springs.Count; i < n; i++)
                {
                    Spring s = springs[i];
                    if (this._dialogLayout.bIncludeInvisibleWidgets || s.isVisible())
                    {
                        size += s.getMinSize(axis);
                    }
                }
                return size;
            }

            //@Override
            internal override int getPrefSize(int axis)
            {
                int size = 0;
                for (int i = 0, n = springs.Count; i < n; i++)
                {
                    Spring s = springs[i];
                    if (this._dialogLayout.bIncludeInvisibleWidgets || s.isVisible())
                    {
                        size += s.getPrefSize(axis);
                    }
                }
                return size;
            }

            //@Override
            internal override int getMaxSize(int axis)
            {
                int size = 0;
                bool hasMax = false;
                for (int i = 0, n = springs.Count; i < n; i++)
                {
                    Spring s = springs[i];
                    if (this._dialogLayout.bIncludeInvisibleWidgets || s.isVisible())
                    {
                        int max = s.getMaxSize(axis);
                        if (max > 0)
                        {
                            size += max;
                            hasMax = true;
                        }
                        else
                        {
                            size += s.getPrefSize(axis);
                        }
                    }
                }
                return hasMax ? size : 0;
            }

            /**
             * Add a default gap between all children except if the neighbour is already a Gap.
             */
            //@Override
            public override void addDefaultGap()
            {
                if (springs.Count > 1)
                {
                    bool wasGap = true;
                    for (int i = 0; i < springs.Count; i++)
                    {
                        Spring s = springs[i];
                        if (this._dialogLayout.bIncludeInvisibleWidgets || s.isVisible())
                        {
                            bool isGap = (s is GapSpring) || (s is NamedGapSpring);
                            if (!isGap && !wasGap)
                            {
                                this.springs.Insert(i++, new GapSpring(this._dialogLayout, DEFAULT_GAP, DEFAULT_GAP, DEFAULT_GAP, true));
                            }
                            wasGap = isGap;
                        }
                    }
                }

                base.addDefaultGap();
            }

            //@Override
            internal override void setSize(int axis, int pos, int size)
            {
                int prefSize = getPrefSize(axis);
                if (size == prefSize)
                {
                    foreach (Spring s in springs)
                    {
                        if (this._dialogLayout.bIncludeInvisibleWidgets || s.isVisible())
                        {
                            int spref = s.getPrefSize(axis);
                            s.setSize(axis, pos, spref);
                            pos += spref;
                        }
                    }
                }
                else if (springs.Count == 1)
                {
                    // no need to check visibility flag
                    Spring s = springs[0];
                    s.setSize(axis, pos, size);
                }
                else if (springs.Count > 1)
                {
                    setSizeNonPref(axis, pos, size, prefSize);
                }
            }

            private void setSizeNonPref(int axis, int pos, int size, int prefSize)
            {
                int delta = size - prefSize;
                bool useMin = delta < 0;
                if (useMin)
                {
                    delta = -delta;
                }

                SpringDelta[] deltas = new SpringDelta[springs.Count];
                int resizeable = 0;
                for (int i = 0; i < springs.Count; i++)
                {
                    Spring s = springs[i];
                    if (this._dialogLayout.bIncludeInvisibleWidgets || s.isVisible())
                    {
                        int sdelta = useMin
                                ? s.getPrefSize(axis) - s.getMinSize(axis)
                                : s.getMaxSize(axis) - s.getPrefSize(axis);
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

                    int[] sizes = new int[springs.Count];

                    int remaining = resizeable;
                    for (int i = 0; i < resizeable; i++)
                    {
                        SpringDelta d = deltas[i];

                        int sdelta = delta / remaining;
                        int ddelta = Math.Min(d.delta, sdelta);
                        delta -= ddelta;
                        remaining--;

                        if (useMin)
                        {
                            ddelta = -ddelta;
                        }
                        sizes[d.idx] = ddelta;
                    }

                    for (int i = 0; i < springs.Count; i++)
                    {
                        Spring s = springs[i];
                        if (this._dialogLayout.bIncludeInvisibleWidgets || s.isVisible())
                        {
                            int ssize = s.getPrefSize(axis) + sizes[i];
                            s.setSize(axis, pos, ssize);
                            pos += ssize;
                        }
                    }
                }
                else
                {
                    foreach (Spring s in springs)
                    {
                        if (this._dialogLayout.bIncludeInvisibleWidgets || s.isVisible())
                        {
                            int ssize;
                            if (useMin)
                            {
                                ssize = s.getMinSize(axis);
                            }
                            else
                            {
                                ssize = s.getMaxSize(axis);
                                if (ssize == 0)
                                {
                                    ssize = s.getPrefSize(axis);
                                }
                            }
                            s.setSize(axis, pos, ssize);
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

            //@Override
            override internal int getMinSize(int axis)
            {
                int size = 0;
                for (int i = 0, n = springs.Count; i < n; i++)
                {
                    Spring s = springs[i];
                    if (this._dialogLayout.bIncludeInvisibleWidgets || s.isVisible())
                    {
                        size = Math.Max(size, s.getMinSize(axis));
                    }
                }
                return size;
            }

            //@Override
            override internal int getPrefSize(int axis)
            {
                int size = 0;
                for (int i = 0, n = springs.Count; i < n; i++)
                {
                    Spring s = springs[i];
                    if (this._dialogLayout.bIncludeInvisibleWidgets || s.isVisible())
                    {
                        size = Math.Max(size, s.getPrefSize(axis));
                    }
                }
                return size;
            }

            //@Override
            override internal int getMaxSize(int axis)
            {
                int size = 0;
                for (int i = 0, n = springs.Count; i < n; i++)
                {
                    Spring s = springs[i];
                    if (this._dialogLayout.bIncludeInvisibleWidgets || s.isVisible())
                    {
                        size = Math.Max(size, s.getMaxSize(axis));
                    }
                }
                return size;
            }

            //@Override
            override internal void setSize(int axis, int pos, int size)
            {
                for (int i = 0, n = springs.Count; i < n; i++)
                {
                    Spring s = springs[i];
                    if (this._dialogLayout.bIncludeInvisibleWidgets || s.isVisible())
                    {
                        s.setSize(axis, pos, size);
                    }
                }
            }

            //@Override
            public override Group addGap()
            {
                getLogger().log(Level.WARNING, "Useless call to addGap() on ParallelGroup", new Exception());
                return this;
            }
        }
    }
}
