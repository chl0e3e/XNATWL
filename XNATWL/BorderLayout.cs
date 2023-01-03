using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static XNATWL.TextAreaW;

namespace XNATWL
{

    public class BorderLayout : Widget
    {
        private Dictionary<Location, Widget> widgets;
        private int hgap, vgap;

        /**
         * The location of a widget in the BorderLayout.
         */
        public enum Location
        {
            EAST, WEST, NORTH, SOUTH, CENTER
        }

        public BorderLayout()
        {
            widgets = new Dictionary<BorderLayout.Location, Widget>();
        }

        /**
         * Adds the specific
         * <code>widget</code> to a
         * <code>location</code> in the BorderLayout.
         *
         * @param widget the widget to add
         * @param location the location to set the widget to
         */
        public void add(Widget widget, Location location)
        {
            if (widget == null)
            {
                throw new ArgumentNullException("widget is null");
            }
            if (location == null)
            {
                throw new ArgumentNullException("location is null");
            }
            if (widgets.ContainsKey(location))
            {
                throw new ArgumentOutOfRangeException("a widget was already added to that location: " + location);
            }

            widgets.Add(location, widget);
            try
            {
                base.insertChild(widget, getNumChildren());
            }
            catch (Exception e)
            {
                removeChild(location);
            }

        }

        /**
         * @param location the location to look retrieve
         * @return the child at the specific
         * <code>location</code> or null if there is no child.
         */
        public Widget getChild(Location location)
        {
            if (location == null)
            {
                throw new ArgumentNullException("location is null");
            }
            return widgets[location];
        }

        /**
         * Remove the child at the specific
         * <code>location</code>.
         *
         * @param location the location to remove
         * @return the removed widget or null if there is no child.
         */
        public Widget removeChild(Location location)
        {
            if (location == null)
            {
                throw new ArgumentNullException("location is null");
            }
            Widget w = widgets[location];
            widgets.Remove(location);
            if (w != null)
            {
                removeChild(w);
            }

            return w;
        }

        /**
         * Adds the widget to the center location of the layout.
         */
        public override void add(Widget child)
        {
            add(child, Location.CENTER);
        }

        /**
         * This is not supproted in the BorderLayout.
         *
         * @throws UnsupportedOperationException
         */
        public override void insertChild(Widget child, int index)
        {
            throw new InvalidOperationException("insert child is not supported by the BorderLayout");
        }

        protected override void childRemoved(Widget exChild)
        {
            foreach (Location loc in widgets.Keys)
            {
                if (widgets[loc] == exChild)
                {
                    widgets.Remove(loc);
                    break;
                }
            }
            base.childRemoved(exChild);
        }

        protected override void allChildrenRemoved()
        {
            widgets.Clear();
            base.allChildrenRemoved();
        }

        protected override void applyTheme(ThemeInfo themeInfo)
        {
            hgap = themeInfo.getParameter("hgap", 0);
            vgap = themeInfo.getParameter("vgap", 0);

            base.applyTheme(themeInfo);
        }

        protected override void layout()
        {
            int top = getInnerY();
            int bottom = getInnerBottom();
            int left = getInnerX();
            int right = getInnerRight();
            Widget w;

            if ((w = widgets[Location.NORTH]) != null)
            {
                w.setPosition(left, top);
                w.setSize(Math.Max(right - left, 0), Math.Max(w.getPreferredHeight(), 0));
                top += w.getPreferredHeight() + vgap;
            }
            if ((w = widgets[Location.SOUTH]) != null)
            {
                w.setPosition(left, bottom - w.getPreferredHeight());
                w.setSize(Math.Max(right - left, 0), Math.Max(w.getPreferredHeight(), 0));
                bottom -= w.getPreferredHeight() + vgap;
            }
            if ((w = widgets[Location.EAST]) != null)
            {
                w.setPosition(right - w.getPreferredWidth(), top);
                w.setSize(Math.Max(w.getPreferredWidth(), 0), Math.Max(bottom - top, 0));
                right -= w.getPreferredWidth() + hgap;
            }
            if ((w = widgets[Location.WEST]) != null)
            {
                w.setPosition(left, top);
                w.setSize(Math.Max(w.getPreferredWidth(), 0), Math.Max(bottom - top, 0));
                left += w.getPreferredWidth() + hgap;
            }
            if ((w = widgets[Location.CENTER]) != null)
            {
                w.setPosition(left, top);
                w.setSize(Math.Max(right - left, 0), Math.Max(bottom - top, 0));
            }
        }

        public override int getMinWidth()
        {
            return computeMinWidth();
        }

        public override int getMinHeight()
        {
            return computeMinHeight();
        }

        public override int getPreferredInnerWidth()
        {
            return computePrefWidth();
        }

        public override int getPreferredInnerHeight()
        {
            return computePrefHeight();
        }

        private int computeMinWidth()
        {
            int size = 0;

            size += getChildMinWidth(widgets[Location.EAST], hgap);
            size += getChildMinWidth(widgets[Location.WEST], hgap);
            size += getChildMinWidth(widgets[Location.CENTER], 0);

            size = Math.Max(size, getChildMinWidth(widgets[Location.NORTH], 0));
            size = Math.Max(size, getChildMinWidth(widgets[Location.SOUTH], 0));

            return size;
        }

        private int computeMinHeight()
        {
            int size = 0;

            size = Math.Max(size, getChildMinHeight(widgets[Location.EAST], 0));
            size = Math.Max(size, getChildMinHeight(widgets[Location.WEST], 0));
            size = Math.Max(size, getChildMinHeight(widgets[Location.CENTER], 0));

            size += getChildMinHeight(widgets[Location.NORTH], vgap);
            size += getChildMinHeight(widgets[Location.SOUTH], vgap);

            return size;
        }

        private int computePrefWidth()
        {
            int size = 0;

            size += getChildPrefWidth(widgets[Location.EAST], hgap);
            size += getChildPrefWidth(widgets[Location.WEST], hgap);
            size += getChildPrefWidth(widgets[Location.CENTER], 0);

            size = Math.Max(size, getChildPrefWidth(widgets[Location.NORTH], 0));
            size = Math.Max(size, getChildPrefWidth(widgets[Location.SOUTH], 0));

            return size;
        }

        private int computePrefHeight()
        {
            int size = 0;

            size = Math.Max(size, getChildPrefHeight(widgets[Location.EAST], 0));
            size = Math.Max(size, getChildPrefHeight(widgets[Location.WEST], 0));
            size = Math.Max(size, getChildPrefHeight(widgets[Location.CENTER], 0));

            size += getChildPrefHeight(widgets[Location.NORTH], vgap);
            size += getChildPrefHeight(widgets[Location.SOUTH], vgap);

            return size;
        }

        // return 0 since a child of the BorderLayout can be null
        private int getChildMinWidth(Widget w, int gap)
        {
            if (w != null)
            {
                return w.getMinWidth() + gap;
            }
            return 0;
        }

        private int getChildMinHeight(Widget w, int gap)
        {
            if (w != null)
            {
                return w.getMinHeight() + gap;
            }
            return 0;
        }

        private int getChildPrefWidth(Widget w, int gap)
        {
            if (w != null)
            {
                return computeSize(w.getMinWidth(), w.getPreferredWidth(), w.getMaxWidth()) + gap;
            }
            return 0;
        }

        private int getChildPrefHeight(Widget w, int gap)
        {
            if (w != null)
            {
                return computeSize(w.getMinHeight(), w.getPreferredHeight(), w.getMaxHeight()) + gap;
            }
            return 0;
        }
    }
}
