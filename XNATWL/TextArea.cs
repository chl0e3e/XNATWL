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
using XNATWL.Renderer;
using XNATWL.Utils;
using XNATWL.TextAreaModel;

namespace XNATWL
{
    public class TextArea : Widget
    {
        public interface WidgetResolver
        {
            Widget resolveWidget(String name, String param);
        }

        public interface ImageResolver
        {
            Image resolveImage(String name);
        }

        public interface Callback
        {
            /**
             * Called when a link has been clicked
             * @param href the href of the link
             */
            void handleLinkClicked(String href);
        }

        public interface Callback2 : Callback
        {
            /**
             * Called for {@code MOUSE_BTNDOWN}, {@code MOUSE_BTNUP} and {@code MOUSE_CLICKED}
             * events performed over elements.
             * 
             * <p>This callback is fired before {@link Callback#handleLinkClicked(java.lang.String) }</p>
             * 
             * <p>This callback should not modify the model - if this feature is required use
             * {@link GUI#invokeLater(java.lang.Runnable) }.</p>
             * 
             * @param evt the mouse event
             * @param element the element under the mouse
             * @see Event.Type#MOUSE_BTNDOWN
             * @see Event.Type#MOUSE_BTNUP
             * @see Event.Type#MOUSE_CLICKED
             */
            void handleMouseButton(Event evt, TextAreaModel.Element element);
        }

        public static StateKey STATE_HOVER = StateKey.Get("hover");

        static char[] EMPTY_CHAR_ARRAY = new char[0];

        private Dictionary<String, Widget> widgets;
        private Dictionary<String, WidgetResolver> widgetResolvers;
        private Dictionary<String, Image> userImages;
        private List<ImageResolver> imageResolvers;

        TextAreaModel.StyleSheetResolver styleClassResolver;
        private Runnable modelCB;
        private TextAreaModel.TextAreaModel model;
        private ParameterMap fonts;
        private ParameterMap images;
        private Font defaultFont;
        private Callback[] callbacks;
        private MouseCursor mouseCursorNormal;
        private MouseCursor mouseCursorLink;
        private DraggableButton.DragListener dragListener;

        private LClip layoutRoot;
        private List<LImage> allBGImages;
        private RenderInfo renderInfo;
        private bool inLayoutCode;
        private bool bForceRelayout;
        private Dimension preferredInnerSize;
        private FontMapper fontMapper;
        private FontMapperCacheEntry[] fontMapperCache;

        private int lastMouseX;
        private int lastMouseY;
        private bool lastMouseInside;
        private bool dragging;
        private int dragStartX;
        private int dragStartY;
        private LElement curLElementUnderMouse;

        public event EventHandler<TextAreaChangedEventArgs> Changed;

        public TextArea()
        {
            this.widgets = new Dictionary<String, Widget>();
            this.widgetResolvers = new Dictionary<String, WidgetResolver>();
            this.userImages = new Dictionary<String, Image>();
            this.imageResolvers = new List<ImageResolver>();
            this.layoutRoot = new LClip(null);
            this.allBGImages = new List<LImage>();
            this.renderInfo = new RenderInfo(getAnimationState());

            //this.modelCB = new Runnable() {
            //    public void run() {
            //       forceRelayout();
            //   }
            //};
        }

        public TextArea(TextAreaModel.TextAreaModel model) : this()
        {
            setModel(model);
        }

        public TextAreaModel.TextAreaModel getModel()
        {
            return model;
        }

        public void setModel(TextAreaModel.TextAreaModel model)
        {
            if (this.model != null)
            {
                this.model.Changed -= Model_Changed;
            }
            this.model = model;
            if (model != null)
            {
                this.model.Changed += Model_Changed;
            }
            forceRelayout();
        }

        private void Model_Changed(object sender, TextAreaChangedEventArgs e)
        {

        }

        public void registerWidget(String name, Widget widget)
        {
            if (name == null)
            {
                throw new NullReferenceException("name");
            }
            if (widget.getParent() != null)
            {
                throw new ArgumentOutOfRangeException("Widget must not have a parent");
            }
            if (widgets.ContainsKey(name) || widgetResolvers.ContainsKey(name))
            {
                throw new ArgumentOutOfRangeException("widget name already in registered");
            }
            if (widgets.ContainsValue(widget))
            {
                throw new ArgumentOutOfRangeException("widget already registered");
            }
            widgets.Add(name, widget);
        }

        public void registerWidgetResolver(String name, WidgetResolver resolver)
        {
            if (name == null)
            {
                throw new NullReferenceException("name");
            }
            if (resolver == null)
            {
                throw new NullReferenceException("resolver");
            }
            if (widgets.ContainsKey(name) || widgetResolvers.ContainsKey(name))
            {
                throw new ArgumentOutOfRangeException("widget name already in registered");
            }
            widgetResolvers.Add(name, resolver);
        }

        public void unregisterWidgetResolver(String name)
        {
            if (name == null)
            {
                throw new NullReferenceException("name");
            }
            widgetResolvers.Remove(name);
        }

        public void unregisterWidget(String name)
        {
            if (name == null)
            {
                throw new NullReferenceException("name");
            }
            Widget w = widgets[name];
            if (w != null)
            {
                int idx = getChildIndex(w);
                if (idx >= 0)
                {
                    base.removeChild(idx);
                    forceRelayout();
                }
            }
        }

        public void unregisterAllWidgets()
        {
            widgets.Clear();
            base.removeAllChildren();
            forceRelayout();
        }

        public void registerImage(String name, Image image)
        {
            if (name == null)
            {
                throw new NullReferenceException("name");
            }
            userImages.Add(name, image);
        }

        public void registerImageResolver(ImageResolver resolver)
        {
            if (resolver == null)
            {
                throw new NullReferenceException("resolver");
            }
            if (!imageResolvers.Contains(resolver))
            {
                imageResolvers.Add(resolver);
            }
        }

        public void unregisterImage(String name)
        {
            userImages.Remove(name);
        }

        public void unregisterImageResolver(ImageResolver imageResolver)
        {
            imageResolvers.Remove(imageResolver);
        }

        public DraggableButton.DragListener getDragListener()
        {
            return dragListener;
        }

        public void setDragListener(DraggableButton.DragListener dragListener)
        {
            this.dragListener = dragListener;
        }

        public TextAreaModel.StyleSheetResolver getStyleClassResolver()
        {
            return styleClassResolver;
        }

        public void setStyleClassResolver(TextAreaModel.StyleSheetResolver styleClassResolver)
        {
            this.styleClassResolver = styleClassResolver;
            forceRelayout();
        }

        /**
         * Sets a default style sheet with the following content:
         * <pre>p, ul {
         *    margin-bottom: 1em
         *}
         *pre {
         *    white-space: pre
         *}</pre>
         */
        public void setDefaultStyleSheet()
        {
            //try
            {
                StyleSheet styleSheet = new StyleSheet();
                styleSheet.Parse("p,ul{margin-bottom:1em}");
                setStyleClassResolver(styleSheet);
            }
            //catch (Exception ex)
            {
            //    Logger.GetLogger(typeof(TextArea)).log(Logger.Level.SEVERE,
             //           "Can't create default style sheet", ex);
            }
        }

        public Rect getElementRect(Element element)
        {
            int[] offset = new int[2];
            LElement le = layoutRoot.find(element, offset);
            if (le != null)
            {
                return new Rect(le.x + offset[0], le.y + offset[1], le.width, le.height);
            }
            else
            {
                return null;
            }
        }

        //@Override
        protected override void applyTheme(ThemeInfo themeInfo)
        {
            base.applyTheme(themeInfo);
            applyThemeTextArea(themeInfo);
        }

        protected void applyThemeTextArea(ThemeInfo themeInfo)
        {
            fonts = themeInfo.GetParameterMap("fonts");
            images = themeInfo.GetParameterMap("images");
            defaultFont = themeInfo.GetFont("font");
            mouseCursorNormal = themeInfo.GetMouseCursor("mouseCursor");
            mouseCursorLink = themeInfo.GetMouseCursor("mouseCursor.link");
            forceRelayout();
        }

        //@Override
        protected override void afterAddToGUI(GUI gui)
        {
            base.afterAddToGUI(gui);
            renderInfo.asNormal.setGUI(gui);
            renderInfo.asHover.setGUI(gui);
        }

        //@Override
        public override void insertChild(Widget child, int index)
        {
            throw new InvalidOperationException("use registerWidget");
        }

        //@Override
        public override void removeAllChildren()
        {
            throw new InvalidOperationException("use registerWidget");
        }

        //@Override
        public override Widget removeChild(int index)
        {
            throw new InvalidOperationException("use registerWidget");
        }

        private void computePreferredInnerSize()
        {
            int prefWidth = -1;
            int prefHeight = -1;

            if (model == null)
            {
                prefWidth = 0;
                prefHeight = 0;

            }
            else if (getMaxWidth() > 0)
            {
                int borderHorizontal = getBorderHorizontal();
                int maxWidth = Math.Max(0, getMaxWidth() - borderHorizontal);
                int minWidth = Math.Max(0, getMinWidth() - borderHorizontal);

                if (minWidth < maxWidth)
                {
                    //System.out.println("Doing preferred size computation");

                    LClip tmpRoot = new LClip(null);
                    startLayout();
                    try
                    {
                        tmpRoot.width = maxWidth;
                        Box box = new Box(this, tmpRoot, 0, 0, 0, false);
                        layoutElements(box, model);
                        box.finish();

                        prefWidth = Math.Max(0, maxWidth - box.minRemainingWidth);
                        prefHeight = box.curY;
                    }
                    finally
                    {
                        endLayout();
                    }
                }
            }
            preferredInnerSize = new Dimension(prefWidth, prefHeight);
        }

        //@Override
        public override int getPreferredInnerWidth()
        {
            if (preferredInnerSize == null)
            {
                computePreferredInnerSize();
            }
            if (preferredInnerSize.X >= 0)
            {
                return preferredInnerSize.X;
            }
            return getInnerWidth();
        }

        //@Override
        public override int getPreferredInnerHeight()
        {
            if (getInnerWidth() == 0)
            {
                if (preferredInnerSize == null)
                {
                    computePreferredInnerSize();
                }
                if (preferredInnerSize.Y >= 0)
                {
                    return preferredInnerSize.Y;
                }
            }
            validateLayout();
            return layoutRoot.height;
        }

        //@Override
        public override int getPreferredWidth()
        {
            int maxWidth = getMaxWidth();
            return computeSize(getMinWidth(), base.getPreferredWidth(), maxWidth);
        }

        //@Override
        public override void setMaxSize(int width, int height)
        {
            if (width != getMaxWidth())
            {
                preferredInnerSize = null;
                invalidateLayout();
            }
            base.setMaxSize(width, height);
        }

        //@Override
        public override void setMinSize(int width, int height)
        {
            if (width != getMinWidth())
            {
                preferredInnerSize = null;
                invalidateLayout();
            }
            base.setMinSize(width, height);
        }

        //@Override
        protected override void layout()
        {
            int targetWidth = getInnerWidth();

            //System.out.println(this+" minWidth="+getMinWidth()+" width="+getWidth()+" maxWidth="+getMaxWidth()+" targetWidth="+targetWidth+" preferredInnerSize="+preferredInnerSize);

            // only recompute the layout when it has changed
            if (layoutRoot.width != targetWidth || bForceRelayout)
            {
                var old = layoutRoot.width;
                layoutRoot.width = targetWidth;
                inLayoutCode = true;
                bForceRelayout = false;
                int requiredHeight;

                startLayout();
                try
                {
                    clearLayout();
                    Box box = new Box(this, layoutRoot, 0, 0, 0, true);
                    if (model != null)
                    {
                        layoutElements(box, model);

                        box.finish();

                        // set position & size of all widget elements
                        layoutRoot.adjustWidget(getInnerX(), getInnerY());
                        layoutRoot.collectBGImages(0, 0, allBGImages);
                    }
                    updateMouseHover();
                    requiredHeight = box.curY;
                }
                finally
                {
                    inLayoutCode = false;
                    endLayout();
                }

                if (layoutRoot.height != requiredHeight)
                {
                    layoutRoot.height = requiredHeight;
                    if (getInnerHeight() != requiredHeight)
                    {
                        // call outside of inLayoutCode range
                        invalidateLayout();
                    }
                }
            }
        }

        //@Override
        protected override void paintWidget(GUI gui)
        {
            List<LImage> bi = allBGImages;
            RenderInfo ri = renderInfo;
            ri.offsetX = getInnerX();
            ri.offsetY = getInnerY();
            ri.renderer = gui.getRenderer();

            for (int i = 0, n = bi.Count; i < n; i++)
            {
                bi[i].draw(ri);
            }

            layoutRoot.draw(ri);
        }

        //@Override
        protected override void sizeChanged()
        {
            if (!inLayoutCode)
            {
                invalidateLayout();
            }
        }

        //@Override
        protected override void childAdded(Widget child)
        {
            // always ignore
        }

        //@Override
        protected override void childRemoved(Widget exChild)
        {
            // always ignore
        }

        //@Override
        protected override void allChildrenRemoved()
        {
            // always ignore
        }

        //@Override
        public override void destroy()
        {
            base.destroy();
            clearLayout();
            forceRelayout();
        }

        //@Override
        public override bool handleEvent(Event evt)
        {
            if (base.handleEvent(evt))
            {
                return true;
            }

            if (evt.isMouseEvent())
            {
                EventType eventType = evt.getEventType();

                if (dragging)
                {
                    if (eventType == EventType.MOUSE_DRAGGED)
                    {
                        if (dragListener != null)
                        {
                            dragListener.dragged(evt.getMouseX() - dragStartX, evt.getMouseY() - dragStartY);
                        }
                    }
                    if (evt.isMouseDragEnd())
                    {
                        if (dragListener != null)
                        {
                            dragListener.dragStopped();
                        }
                        dragging = false;
                        updateMouseHover(evt);
                    }
                    return true;
                }

                updateMouseHover(evt);

                if (eventType == EventType.MOUSE_WHEEL)
                {
                    return false;
                }

                if (eventType == EventType.MOUSE_BTNDOWN)
                {
                    dragStartX = evt.getMouseX();
                    dragStartY = evt.getMouseY();
                }

                if (eventType == EventType.MOUSE_DRAGGED)
                {
                    System.Diagnostics.Debug.Assert(!dragging);
                    dragging = true;
                    if (dragListener != null)
                    {
                        dragListener.dragStarted();
                    }
                    return true;
                }

                if (curLElementUnderMouse != null && (
                        eventType == EventType.MOUSE_CLICKED ||
                        eventType == EventType.MOUSE_BTNDOWN ||
                        eventType == EventType.MOUSE_BTNUP))
                {
                    Element e = curLElementUnderMouse.element;
                    if (callbacks != null)
                    {
                        foreach (Callback l in callbacks)
                        {
                            if (l is Callback2)
                            {
                                ((Callback2)l).handleMouseButton(evt, e);
                            }
                        }
                    }
                }

                if (eventType == EventType.MOUSE_CLICKED)
                {
                    if (curLElementUnderMouse != null && curLElementUnderMouse.href != null)
                    {
                        String href = curLElementUnderMouse.href;
                        if (callbacks != null)
                        {
                            foreach (Callback l in callbacks)
                            {
                                l.handleLinkClicked(href);
                            }
                        }
                    }
                }

                return true;
            }

            return false;
        }

        //@Override
        internal override Object getTooltipContentAt(int mouseX, int mouseY)
        {
            if (curLElementUnderMouse != null)
            {
                if (curLElementUnderMouse.element is ImageElement)
                {
                    return ((ImageElement)curLElementUnderMouse.element).GetToolTip();
                }
            }
            return base.getTooltipContentAt(mouseX, mouseY);
        }

        private void updateMouseHover(Event evt)
        {
            lastMouseInside = isMouseInside(evt);
            lastMouseX = evt.getMouseX();
            lastMouseY = evt.getMouseY();
            updateMouseHover();
        }

        private void updateMouseHover()
        {
            LElement le = null;
            if (lastMouseInside)
            {
                le = layoutRoot.find(lastMouseX - getInnerX(), lastMouseY - getInnerY());
            }
            if (curLElementUnderMouse != le)
            {
                curLElementUnderMouse = le;
                layoutRoot.setHover(le);
                renderInfo.asNormal.resetAnimationTime(STATE_HOVER);
                renderInfo.asHover.resetAnimationTime(STATE_HOVER);
                updateTooltip();
            }

            if (le != null && le.href != null)
            {
                setMouseCursor(mouseCursorLink);
            }
            else
            {
                setMouseCursor(mouseCursorNormal);
            }

            getAnimationState().setAnimationState(STATE_HOVER, lastMouseInside);
        }

        void forceRelayout()
        {
            bForceRelayout = true;
            preferredInnerSize = null;
            invalidateLayout();
        }

        private void clearLayout()
        {
            layoutRoot.destroy();
            allBGImages.Clear();
            base.removeAllChildren();
        }

        private void startLayout()
        {
            if (styleClassResolver != null)
            {
                styleClassResolver.StartLayout();
            }

            GUI gui = getGUI();
            fontMapper = (gui != null) ? gui.getRenderer().FontMapper : null;
            fontMapperCache = null;
        }

        private void endLayout()
        {
            if (styleClassResolver != null)
            {
                styleClassResolver.LayoutFinished();
            }
            fontMapper = null;
            fontMapperCache = null;
        }

        private void layoutElements(Box box, IEnumerable<Element> elements)
        {
            foreach (Element e in elements)
            {
                layoutElement(box, e);
            }
        }

        private void layoutElement(Box box, Element e)
        {
            box.clearFloater(e.GetStyle().Get(StyleAttribute.CLEAR, styleClassResolver));

            if (e is TextElement)
            {
                layoutTextElement(box, (TextElement)e);
            }
            else if (e is LineBreakElement)
            {
                box.nextLine(true);
            }
            else
            {
                if (box.wasPreformatted)
                {
                    box.nextLine(false);
                    box.wasPreformatted = false;
                }
                if (e is ParagraphElement)
                {
                    layoutParagraphElement(box, (ParagraphElement)e);
                }
                else if (e is ImageElement)
                {
                    layoutImageElement(box, (ImageElement)e);
                }
                else if (e is WidgetElement)
                {
                    layoutWidgetElement(box, (WidgetElement)e);
                }
                else if (e is ListElement)
                {
                    layoutListElement(box, (ListElement)e);
                }
                else if (e is OrderedListElement)
                {
                    layoutOrderedListElement(box, (OrderedListElement)e);
                }
                else if (e is BlockElement)
                {
                    layoutBlockElement(box, (BlockElement)e);
                }
                else if (e is TableElement)
                {
                    layoutTableElement(box, (TableElement)e);
                }
                else if (e is LinkElement)
                {
                    layoutLinkElement(box, (LinkElement)e);
                }
                else if (e is ContainerElement)
                {
                    layoutContainerElement(box, (ContainerElement)e);
                }
                else
                {
                    Logger.GetLogger(typeof(TextArea)).log(Logger.Level.SEVERE, "Unknown Element subclass: {0}" + e.GetType().FullName);
                }
            }
        }

        private void layoutImageElement(Box box, ImageElement ie)
        {
            Image image = selectImage(ie.GetImageName());
            if (image == null)
            {
                return;
            }

            LImage li = new LImage(ie, image);
            li.href = box.href;
            layout(box, ie, li);
        }

        private void layoutWidgetElement(Box box, WidgetElement we)
        {
            Widget widget = widgets[we.GetWidgetName()];
            if (widget == null)
            {
                WidgetResolver resolver = widgetResolvers[we.GetWidgetName()];
                if (resolver != null)
                {
                    widget = resolver.resolveWidget(we.GetWidgetName(), we.GetWidgetParam());
                }
                if (widget == null)
                {
                    return;
                }
            }

            if (widget.getParent() != null)
            {
                Logger.GetLogger(typeof(TextArea)).log(Logger.Level.SEVERE, "Widget already added: " + widget.getThemePath());
                return;
            }

            base.insertChild(widget, getNumChildren());
            widget.adjustSize();

            LWidget lw = new LWidget(we, widget);
            lw.width = widget.getWidth();
            lw.height = widget.getHeight();

            layout(box, we, lw);
        }

        private void layout(Box box, Element e, LElement le)
        {
            Style style = e.GetStyle();

            FloatPosition floatPosition = style.Get(StyleAttribute.FLOAT_POSITION, styleClassResolver);
            Display display = style.Get(StyleAttribute.DISPLAY, styleClassResolver);

            le.marginTop = (short)convertToPX0(style, StyleAttribute.MARGIN_TOP, box.boxWidth);
            le.marginLeft = (short)convertToPX0(style, StyleAttribute.MARGIN_LEFT, box.boxWidth);
            le.marginRight = (short)convertToPX0(style, StyleAttribute.MARGIN_RIGHT, box.boxWidth);
            le.marginBottom = (short)convertToPX0(style, StyleAttribute.MARGIN_BOTTOM, box.boxWidth);

            int autoHeight = le.height;
            int width = convertToPX(style, StyleAttribute.WIDTH, box.boxWidth, le.width);
            if (width > 0)
            {
                if (le.width > 0)
                {
                    autoHeight = width * le.height / le.width;
                }
                le.width = width;
            }

            int height = convertToPX(style, StyleAttribute.HEIGHT, le.height, autoHeight);
            if (height > 0)
            {
                le.height = height;
            }

            layout(box, e, le, floatPosition, display);
        }

        private void layout(Box box, Element e, LElement le, FloatPosition floatPos, Display display)
        {
            bool leftRight = (floatPos != FloatPosition.NONE);

            if (leftRight || display != Display.INLINE)
            {
                box.nextLine(false);
                if (!leftRight)
                {
                    box.curY = box.computeTopPadding(le.marginTop);
                    box.checkFloaters();
                }
            }

            box.advancePastFloaters(le.width, le.marginLeft, le.marginRight);
            if (le.width > box.lineWidth)
            {
                le.width = box.lineWidth;
            }

            if (leftRight)
            {
                if (floatPos == FloatPosition.RIGHT)
                {
                    le.x = box.computeRightPadding(le.marginRight) - le.width;
                    box.objRight.Add(le);
                }
                else
                {
                    le.x = box.computeLeftPadding(le.marginLeft);
                    box.objLeft.Add(le);
                }
            }
            else if (display == Display.INLINE)
            {
                if (box.getRemaining() < le.width && !box.isAtStartOfLine())
                {
                    box.nextLine(false);
                }
                le.x = box.getXAndAdvance(le.width);
            }
            else
            {
                switch (e.GetStyle().Get(StyleAttribute.HORIZONTAL_ALIGNMENT, styleClassResolver))
                {
                    case TextAreaModel.HAlignment.CENTER:
                    case TextAreaModel.HAlignment.JUSTIFY:
                        le.x = box.lineStartX + (box.lineWidth - le.width) / 2;
                        break;

                    case TextAreaModel.HAlignment.RIGHT:
                        le.x = box.computeRightPadding(le.marginRight) - le.width;
                        break;

                    default:
                        le.x = box.computeLeftPadding(le.marginLeft);
                        break;
                }
            }

            box.layout.Add(le);

            if (leftRight)
            {
                System.Diagnostics.Debug.Assert(box.lineStartIdx == box.layout.Count - 1);
                box.lineStartIdx++;
                le.y = box.computeTopPadding(le.marginTop);
                box.computePadding();
            }
            else if (display != Display.INLINE)
            {
                box.accountMinRemaining(Math.Max(0, box.lineWidth - le.width));
                box.nextLine(false);
            }
        }

        private static int DEFAULT_FONT_SIZE = 14;

        int convertToPX(Style style, StyleAttribute<Value> attribute, int full, int auto)
        {
            style = style.Resolve(attribute, styleClassResolver);
            Value valueUnit = style.GetNoResolve(attribute, styleClassResolver);

            Font font = null;
            if (valueUnit.UnitOfValue.FontBased)
            {
                if (attribute == StyleAttribute.FONT_SIZE)
                {
                    style = style.Parent;
                    if (style == null)
                    {
                        return DEFAULT_FONT_SIZE;
                    }
                }
                font = selectFont(style);
                if (font == null)
                {
                    return 0;
                }
            }

            float value = valueUnit.FloatValue;
            if (valueUnit.UnitOfValue == Value.Unit.EM)
            {
                value *= font.MWidth;
            }
            else if (valueUnit.UnitOfValue == Value.Unit.EX)
            {
                value *= font.xWidth;
            }
            else if (valueUnit.UnitOfValue == Value.Unit.PERCENT)
            {
                value *= full * 0.01f;
            }
            else if (valueUnit.UnitOfValue == Value.Unit.PT)
            {
                value *= 1.33f; // 96 DPI
            }
            else if (valueUnit.UnitOfValue == Value.Unit.AUTO)
            {
                return auto;
            }

            if (value >= short.MaxValue)
            {
                return short.MaxValue;
            }
            if (value <= short.MinValue)
            {
                return short.MinValue;
            }
            return (int)Math.Round(value);
        }

        int convertToPX0(Style style, StyleAttribute<Value> attribute, int full)
        {
            return Math.Max(0, convertToPX(style, attribute, full, 0));
        }

        private Font selectFont(Style style)
        {
            List<string> fontFamilies = style.Get(StyleAttribute.FONT_FAMILIES, styleClassResolver);
            if (fontFamilies != null)
            {
                if (fontMapper != null)
                {
                    Font font = selectFontMapper(style, fontMapper, fontFamilies);
                    if (font != null)
                    {
                        return font;
                    }
                }

                if (fonts != null)
                {
                    foreach(string fontFamily in fontFamilies)
                    {
                        Font font = fonts.GetFont(fontFamily);
                        if (font != null)
                        {
                            return font;
                        }
                    }
                }
            }
            return defaultFont;
        }

        private static StateSelect HOVER_STATESELECT =
                new StateSelect(new Check(STATE_HOVER));

        private static int FONT_MAPPER_CACHE_SIZE = 16;

        public class FontMapperCacheEntry
        {
            internal int fontSize;
            internal int fontStyle;
            internal List<string> fontFamilies;
            internal TextDecoration tdNormal;
            internal TextDecoration tdHover;
            internal int hashCode;
            internal Font font;
            internal FontMapperCacheEntry next;

            internal FontMapperCacheEntry(int fontSize, int fontStyle, List<string> fontFamilies, TextDecoration tdNormal, TextDecoration tdHover, int hashCode, Font font)
            {
                this.fontSize = fontSize;
                this.fontStyle = fontStyle;
                this.fontFamilies = fontFamilies;
                this.tdNormal = tdNormal;
                this.tdHover = tdHover;
                this.hashCode = hashCode;
                this.font = font;
            }
        }

        private Font selectFontMapper(Style style, FontMapper fontMapper, List<string> fontFamilies)
        {
            int fontSize = convertToPX(style, StyleAttribute.FONT_SIZE, DEFAULT_FONT_SIZE, DEFAULT_FONT_SIZE);
            int fontStyle = 0;
            if (style.Get(StyleAttribute.FONT_WEIGHT, styleClassResolver) >= 550)
            {
                fontStyle |= FontMapperStatics.STYLE_BOLD;
            }
            if (style.Get(StyleAttribute.FONT_ITALIC, styleClassResolver))
            {
                fontStyle |= FontMapperStatics.STYLE_ITALIC;
            }

            TextDecoration textDecoration = (TextDecoration) style.GetAsObject(StyleAttribute.TEXT_DECORATION, styleClassResolver);
            TextDecoration textDecorationHover =  (TextDecoration) style.GetAsObject(StyleAttribute.TEXT_DECORATION_HOVER, styleClassResolver);

            int hashCode = fontSize;
            hashCode = hashCode * 67 + fontStyle;
            hashCode = hashCode * 67 + fontFamilies.GetHashCode();
            hashCode = hashCode * 67 + textDecoration.GetHashCode();
            hashCode = hashCode * 67 + ((textDecorationHover != null) ? textDecorationHover.GetHashCode() : 0);

            int cacheIdx = hashCode & (FONT_MAPPER_CACHE_SIZE - 1);

            if (fontMapperCache != null)
            {
                for (FontMapperCacheEntry cache = fontMapperCache[cacheIdx]; cache != null; cache = cache.next)
                {
                    if (cache.hashCode == hashCode &&
                            cache.fontSize == fontSize &&
                            cache.fontStyle == fontStyle &&
                            cache.tdNormal == textDecoration &&
                            cache.tdHover == textDecorationHover &&
                            cache.fontFamilies.Equals(fontFamilies))
                    {
                        return cache.font;
                    }
                }
            }
            else
            {
                fontMapperCache = new FontMapperCacheEntry[FONT_MAPPER_CACHE_SIZE];
            }

            FontParameter fpNormal = createFontParameter(textDecoration);

            StateSelect select;
            FontParameter[] parameters;

            if (textDecorationHover != null)
            {
                FontParameter fpHover = createFontParameter(textDecorationHover);

                select = HOVER_STATESELECT;
                parameters = new FontParameter[] { fpHover, fpNormal };
            }
            else
            {
                select = StateSelect.EMPTY;
                parameters = new FontParameter[] { fpNormal };
            }

            Font font = fontMapper.GetFont(fontFamilies, fontSize, fontStyle, select, parameters);

            FontMapperCacheEntry ce = new FontMapperCacheEntry(fontSize, fontStyle,
                    fontFamilies, textDecoration, textDecorationHover, hashCode, font);
            ce.next = fontMapperCache[cacheIdx];
            fontMapperCache[cacheIdx] = ce;

            return font;
        }

        private static FontParameter createFontParameter(TextDecoration deco)
        {
            FontParameter fp = new FontParameter();
            fp.Put(FontParameter.UNDERLINE, deco == TextDecoration.UNDERLINE);
            fp.Put(FontParameter.LINETHROUGH, deco == TextDecoration.LINE_THROUGH);
            return fp;
        }

        private FontData createFontData(Style style)
        {
            Font font = selectFont(style);
            if (font == null)
            {
                return null;
            }

            return new FontData(font,
                    style.Get(StyleAttribute.COLOR, styleClassResolver),
                    style.Get(StyleAttribute.COLOR_HOVER, styleClassResolver));
        }

        private Image selectImage(Style style, StyleAttribute<String> element)
        {
            String imageName = style.Get(element, styleClassResolver);
            if (imageName != null)
            {
                return selectImage(imageName);
            }
            else
            {
                return null;
            }
        }

        private Image selectImage(String name)
        {
            Image image = null;
            if (userImages.ContainsKey(name))
            {
                image = userImages[name];
            }
            if (image != null)
            {
                return image;
            }
            for (int i = 0; i < imageResolvers.Count; i++)
            {
                image = imageResolvers[i].resolveImage(name);
                if (image != null)
                {
                    return image;
                }
            }
            if (images != null)
            {
                return images.GetImage(name);
            }
            return null;
        }

        private void layoutParagraphElement(Box box, ParagraphElement pe)
        {
            Style style = pe.GetStyle();
            Font font = selectFont(style);

            doMarginTop(box, style);
            LElement anchor = box.addAnchor(pe);
            box.setupTextParams(style, font, true);

            layoutElements(box, pe);

            if (box.textAlignment == TextAreaModel.HAlignment.JUSTIFY)
            {
                box.textAlignment = TextAreaModel.HAlignment.LEFT;
            }
            box.nextLine(false);
            box.inParagraph = false;

            anchor.height = box.curY - anchor.y;
            doMarginBottom(box, style);
        }

        private void layoutTextElement(Box box, TextElement te)
        {
            String text = te.GetText();
            Style style = te.GetStyle();
            FontData fontData = createFontData(style);
            bool pre = style.Get(StyleAttribute.PREFORMATTED, styleClassResolver);

            if (fontData == null)
            {
                return;
            }

            bool inheritHover;
            object inheritHoverStyle = style.Resolve(StyleAttribute.INHERIT_HOVER, styleClassResolver).GetRawAsObject(StyleAttribute.INHERIT_HOVER);
            if (inheritHoverStyle != null)
            {
                inheritHover = (bool) inheritHoverStyle;
            }
            else
            {
                inheritHover = (box.style != null) && (box.style == style.Parent);
            }

            box.setupTextParams(style, fontData.font, false);

            if (pre && !box.wasPreformatted)
            {
                box.nextLine(false);
            }

            if (pre)
            {
                int idx = 0;
                while (idx < text.Length)
                {
                    int end = TextUtil.IndexOf(text, '\n', idx);
                    layoutTextPre(box, te, fontData, text, idx, end, inheritHover);
                    if (end < text.Length && text[end] == '\n')
                    {
                        end++;
                        box.nextLine(true);
                    }
                    idx = end;
                }
            }
            else
            {
                layoutText(box, te, fontData, text, 0, text.Length, inheritHover);
            }

            box.wasPreformatted = pre;
        }

        private void layoutText(Box box, TextElement te, FontData fontData,
                String text, int textStart, int textEnd, bool inheritHover)
        {
            int idx = textStart;
            // trim start
            while (textStart < textEnd && isSkip(text[textStart]))
            {
                textStart++;
            }
            // trim end
            bool endsWithSpace = false;
            while (textEnd > textStart && isSkip(text[textEnd - 1]))
            {
                endsWithSpace = true;
                textEnd--;
            }

            Font font = fontData.font;

            // check if we skipped white spaces and the previous element in this
            // row was not a text cell
            if (textStart > idx && box.prevOnLineEndsNotWithSpace())
            {
                box.curX += font.SpaceWidth;
            }

            object breakWord = null;    // lazy lookup

            idx = textStart;
            while (idx < textEnd)
            {
                System.Diagnostics.Debug.Assert(!isSkip(text[idx]));

                int end = idx;
                int visibleEnd = idx;
                if (box.textAlignment != TextAreaModel.HAlignment.JUSTIFY)
                {
                    end = idx + font.ComputeVisibleGlyphs(text, idx, textEnd, box.getRemaining());
                    visibleEnd = end;

                    if (end < textEnd)
                    {
                        // if we are at a punctuation then walk backwards until we hit
                        // the word or a break. This ensures that the punctuation stays
                        // at the end of a word
                        while (end > idx && isPunctuation(text[end]))
                        {
                            end--;
                        }

                        // if we are not at the end of this text element
                        // and the next character is not a space
                        if (!isBreak(text[end]))
                        {
                            // then we walk backwards until we find spaces
                            // this prevents the line ending in the middle of a word
                            while (end > idx && !isBreak(text[end - 1]))
                            {
                                end--;
                            }
                        }
                    }

                    // now walks backwards until we hit the end of the previous word
                    while (end > idx && isSkip(text[end - 1]))
                    {
                        end--;
                    }
                }

                bool advancePastFloaters = false;

                // if we found no word that fits
                if (end == idx)
                {
                    // we may need a new line
                    if (box.textAlignment != TextAreaModel.HAlignment.JUSTIFY && box.nextLine(false))
                    {
                        continue;
                    }
                    if (breakWord == null)
                    {
                        breakWord = te.GetStyle().Get(StyleAttribute.BREAKWORD, styleClassResolver);
                    }
                    if ((bool)breakWord)
                    {
                        if (visibleEnd == idx)
                        {
                            end = idx + 1;  // ensure progress
                        }
                        else
                        {
                            end = visibleEnd;
                        }
                    }
                    else
                    {
                        // or we already are at the start of a line
                        // just put the word there even if it doesn't fit
                        while (end < textEnd && !isBreak(text[end]))
                        {
                            end++;
                        }
                        // some characters need to stay at the end of a word
                        while (end < textEnd && isPunctuation(text[end]))
                        {
                            end++;
                        }
                    }
                    advancePastFloaters = true;
                }

                if (idx < end)
                {
                    LText lt = new LText(te, fontData, text, idx, end, box.doCacheText);
                    if (advancePastFloaters)
                    {
                        box.advancePastFloaters(lt.width, box.marginLeft, box.marginRight);
                    }
                    if (box.textAlignment == TextAreaModel.HAlignment.JUSTIFY && box.getRemaining() < lt.width)
                    {
                        box.nextLine(false);
                    }

                    int width = lt.width;
                    if (end < textEnd && isSkip(text[end]))
                    {
                        width += font.SpaceWidth;
                    }

                    lt.x = box.getXAndAdvance(width);
                    lt.marginTop = (short)box.marginTop;
                    lt.href = box.href;
                    lt.inheritHover = inheritHover;
                    box.layout.Add(lt);
                }

                // find the start of the next word
                idx = end;
                while (idx < textEnd && isSkip(text[idx]))
                {
                    idx++;
                }
            }

            if (!box.isAtStartOfLine() && endsWithSpace)
            {
                box.curX += font.SpaceWidth;
            }
        }

        private void layoutTextPre(Box box, TextElement te, FontData fontData,
                String text, int textStart, int textEnd, bool inheritHover)
        {
            Font font = fontData.font;
            int idx = textStart;
            for (; ; )
            {
                while (idx < textEnd)
                {
                    if (text[idx] == '\t')
                    {
                        idx++;
                        int tabX = box.computeNextTabStop(te.GetStyle(), font);
                        if (tabX < box.lineWidth)
                        {
                            box.curX = tabX;
                        }
                        else if (!box.isAtStartOfLine())
                        {
                            break;
                        }
                    }

                    int tabIdx = text.IndexOf('\t', idx);
                    int end = textEnd;
                    if (tabIdx >= 0 && tabIdx < textEnd)
                    {
                        end = tabIdx;
                    }

                    if (end > idx)
                    {
                        int count = font.ComputeVisibleGlyphs(text, idx, end, box.getRemaining());
                        if (count == 0 && !box.isAtStartOfLine())
                        {
                            break;
                        }

                        end = idx + Math.Max(1, count);

                        LText lt = new LText(te, fontData, text, idx, end, box.doCacheText);
                        lt.x = box.getXAndAdvance(lt.width);
                        lt.marginTop = (short)box.marginTop;
                        lt.inheritHover = inheritHover;
                        box.layout.Add(lt);
                    }

                    idx = end;
                }

                if (idx >= textEnd)
                {
                    break;
                }

                box.nextLine(false);
            }
        }

        private void doMarginTop(Box box, Style style)
        {
            int marginTop = convertToPX0(style, StyleAttribute.MARGIN_TOP, box.boxWidth);
            box.nextLine(false);    // need to complete line before computing targetY
            box.advanceToY(box.computeTopPadding(marginTop));
        }

        private void doMarginBottom(Box box, Style style)
        {
            int marginBottom = convertToPX0(style, StyleAttribute.MARGIN_BOTTOM, box.boxWidth);
            box.setMarginBottom(marginBottom);
        }

        private void layoutContainerElement(Box box, ContainerElement ce)
        {
            Style style = ce.GetStyle();
            doMarginTop(box, style);
            box.addAnchor(ce);
            layoutElements(box, ce);
            doMarginBottom(box, style);
        }

        private void layoutLinkElement(Box box, LinkElement le)
        {
            String oldHref = box.href;
            box.href = le.GetHREF();

            Style style = le.GetStyle();
            Display display = style.Get(StyleAttribute.DISPLAY, styleClassResolver);
            if (display == Display.BLOCK)
            {
                layoutBlockElement(box, le);
            }
            else
            {
                layoutContainerElement(box, le);
            }

            box.href = oldHref;
        }

        private void layoutListElement(Box box, ListElement le)
        {
            Style style = le.GetStyle();

            doMarginTop(box, style);

            Image image = selectImage(style, StyleAttribute.LIST_STYLE_IMAGE);
            if (image != null)
            {
                LImage li = new LImage(le, image);
                li.marginRight = (short)convertToPX0(style, StyleAttribute.PADDING_LEFT, box.boxWidth);
                layout(box, le, li, FloatPosition.LEFT, Display.BLOCK);

                int imageHeight = li.height;
                li.height = short.MaxValue;

                layoutElements(box, le);

                li.height = imageHeight;

                box.objLeft.Remove(li);
                box.advanceToY(li.bottom());
                box.computePadding();
            }
            else
            {
                layoutElements(box, le);
                box.nextLine(false);
            }

            doMarginBottom(box, style);
        }

        private void layoutOrderedListElement(Box box, OrderedListElement ole)
        {
            Style style = ole.GetStyle();
            FontData fontData = createFontData(style);

            if (fontData == null)
            {
                return;
            }

            doMarginTop(box, style);
            LElement anchor = box.addAnchor(ole);

            int start = Math.Max(1, ole.GetStart());
            int count = ole.Count;
            OrderedListType type = style.Get(StyleAttribute.LIST_STYLE_TYPE, styleClassResolver);

            String[] labels = new String[count];
            int maxLabelWidth = convertToPX0(style, StyleAttribute.PADDING_LEFT, box.boxWidth);
            for (int i = 0; i < count; i++)
            {
                labels[i] = type.Format(start + i) + ". ";
                int width = fontData.font.ComputeTextWidth(labels[i]);
                maxLabelWidth = Math.Max(maxLabelWidth, width);
            }

            for (int i = 0; i < count; i++)
            {
                String label = labels[i];
                Element li = ole.ElementAt(i);
                Style liStyle = li.GetStyle();
                doMarginTop(box, liStyle);

                LText lt = new LText(ole, fontData, label, 0, label.Length, box.doCacheText);
                int labelWidth = lt.width;
                int labelHeight = lt.height;

                lt.width += convertToPX0(liStyle, StyleAttribute.PADDING_LEFT, box.boxWidth);
                layout(box, ole, lt, FloatPosition.LEFT, Display.BLOCK);
                lt.x += Math.Max(0, maxLabelWidth - labelWidth);
                lt.height = short.MaxValue;

                layoutElement(box, li);

                lt.height = labelHeight;

                box.objLeft.Remove(lt);
                box.advanceToY(lt.bottom());
                box.computePadding();

                doMarginBottom(box, liStyle);
            }

            anchor.height = box.curY - anchor.y;
            doMarginBottom(box, style);
        }

        private Box layoutBox(LClip clip, int continerWidth, int paddingLeft, int paddingRight, ContainerElement ce, String href, bool doCacheText)
        {
            Style style = ce.GetStyle();
            int paddingTop = convertToPX0(style, StyleAttribute.PADDING_TOP, continerWidth);
            int paddingBottom = convertToPX0(style, StyleAttribute.PADDING_BOTTOM, continerWidth);
            int marginBottom = convertToPX0(style, StyleAttribute.MARGIN_BOTTOM, continerWidth);

            Box box = new Box(this, clip, paddingLeft, paddingRight, paddingTop, doCacheText);
            box.href = href;
            box.style = style;
            layoutElements(box, ce);
            box.finish();

            int contentHeight = box.curY + paddingBottom;
            int boxHeight = Math.Max(contentHeight, convertToPX(style, StyleAttribute.HEIGHT, contentHeight, contentHeight));
            if (boxHeight > contentHeight)
            {
                int amount = 0;
                TextAreaModel.VAlignment vAlign = style.Get(StyleAttribute.VERTICAL_ALIGNMENT, styleClassResolver);
                if (vAlign == TextAreaModel.VAlignment.BOTTOM)
                {
                    amount = boxHeight - contentHeight;
                }
                else if (vAlign == TextAreaModel.VAlignment.FILL || vAlign == TextAreaModel.VAlignment.MIDDLE)
                {
                    amount = (boxHeight - contentHeight) / 2;
                }

                if (amount > 0)
                {
                    clip.moveContentY(amount);
                }
            }

            clip.height = boxHeight;
            clip.marginBottom = (short)Math.Max(marginBottom, box.marginBottomAbs - box.curY);
            return box;
        }

        private void layoutBlockElement(Box box, ContainerElement be)
        {
            box.nextLine(false);

            Style style = be.GetStyle();
            FloatPosition floatPosition = style.Get(StyleAttribute.FLOAT_POSITION, styleClassResolver);

            LImage bgImage = createBGImage(box, be);

            int marginTop = convertToPX0(style, StyleAttribute.MARGIN_TOP, box.boxWidth);
            int marginLeft = convertToPX0(style, StyleAttribute.MARGIN_LEFT, box.boxWidth);
            int marginRight = convertToPX0(style, StyleAttribute.MARGIN_RIGHT, box.boxWidth);

            int bgX = box.computeLeftPadding(marginLeft);
            int bgY = box.computeTopPadding(marginTop);
            int bgWidth;

            int remaining = Math.Max(0, box.computeRightPadding(marginRight) - bgX);
            int paddingLeft = convertToPX0(style, StyleAttribute.PADDING_LEFT, box.boxWidth);
            int paddingRight = convertToPX0(style, StyleAttribute.PADDING_RIGHT, box.boxWidth);

            if (floatPosition == FloatPosition.NONE)
            {
                bgWidth = convertToPX(style, StyleAttribute.WIDTH, remaining, remaining);
            }
            else
            {
                bgWidth = convertToPX(style, StyleAttribute.WIDTH, box.boxWidth, int.MinValue);
                if (bgWidth == int.MinValue)
                {
                    LClip dummy = new LClip(null);
                    dummy.width = Math.Max(0, box.lineWidth - paddingLeft - paddingRight);

                    Box dummyBox = layoutBox(dummy, box.boxWidth, paddingLeft, paddingRight, be, null, false);
                    dummyBox.nextLine(false);

                    bgWidth = Math.Max(0, dummy.width - dummyBox.minRemainingWidth);
                }
            }

            bgWidth = Math.Max(0, bgWidth) + paddingLeft + paddingRight;

            if (floatPosition != FloatPosition.NONE)
            {
                box.advancePastFloaters(bgWidth, marginLeft, marginRight);

                bgX = box.computeLeftPadding(marginLeft);
                bgY = Math.Max(bgY, box.curY);
                remaining = Math.Max(0, box.computeRightPadding(marginRight) - bgX);
            }

            bgWidth = Math.Min(bgWidth, remaining);

            if (floatPosition == FloatPosition.RIGHT)
            {
                bgX = box.computeRightPadding(marginRight) - bgWidth;
            }

            LClip clip = new LClip(be);
            clip.x = bgX;
            clip.y = bgY;
            clip.width = bgWidth;
            clip.marginLeft = (short)marginLeft;
            clip.marginRight = (short)marginRight;
            clip.href = box.href;
            box.layout.Add(clip);

            Box clipBox = layoutBox(clip, box.boxWidth, paddingLeft, paddingRight, be, box.href, box.doCacheText);

            // sync main box with layout
            box.lineStartIdx = box.layout.Count;

            if (floatPosition == FloatPosition.NONE)
            {
                box.advanceToY(bgY + clip.height);
                box.setMarginBottom(clip.marginBottom);
                box.accountMinRemaining(clipBox.minRemainingWidth);
            }
            else
            {
                if (floatPosition == FloatPosition.RIGHT)
                {
                    box.objRight.Add(clip);
                }
                else
                {
                    box.objLeft.Add(clip);
                }
                box.computePadding();
            }

            if (bgImage != null)
            {
                bgImage.x = bgX;
                bgImage.y = bgY;
                bgImage.width = bgWidth;
                bgImage.height = clip.height;
                bgImage.hoverSrc = clip;
            }
        }

        private void computeTableWidth(TableElement te,
                int maxTableWidth, int[] columnWidth, int[] columnSpacing, bool[] columnsWithFixedWidth)
        {
            int numColumns = te.GetNumColumns();
            int numRows = te.GetNumRows();
            int cellSpacing = te.GetCellSpacing();
            int cellPadding = te.GetCellPadding();

            Dictionary<int, int> colspanWidths = null;

            for (int col = 0; col < numColumns; col++)
            {
                int width = 0;
                int marginLeft = 0;
                int marginRight = 0;
                bool hasFixedWidth = false;

                for (int row = 0; row < numRows; row++)
                {
                    TableCellElement cell = te.GetCell(row, col);
                    if (cell != null)
                    {
                        Style cellStyle = cell.GetStyle();
                        int colspan = cell.GetColspan();
                        int cellWidth = convertToPX(cellStyle, StyleAttribute.WIDTH, maxTableWidth, int.MinValue);
                        if (cellWidth == int.MinValue && (colspan > 1 || !hasFixedWidth))
                        {
                            int paddingLeft = Math.Max(cellPadding, convertToPX0(cellStyle, StyleAttribute.PADDING_LEFT, maxTableWidth));
                            int paddingRight = Math.Max(cellPadding, convertToPX0(cellStyle, StyleAttribute.PADDING_RIGHT, maxTableWidth));

                            LClip dummy = new LClip(null);
                            dummy.width = maxTableWidth;
                            Box dummyBox = layoutBox(dummy, maxTableWidth, paddingLeft, paddingRight, cell, null, false);
                            dummyBox.finish();

                            cellWidth = maxTableWidth - dummyBox.minRemainingWidth;
                        }
                        else if (colspan == 1 && cellWidth >= 0)
                        {
                            hasFixedWidth = true;
                        }

                        if (colspan > 1)
                        {
                            if (colspanWidths == null)
                            {
                                colspanWidths = new Dictionary<int, int>();
                            }
                            int key = (col << 16) + colspan;

                            int? value;

                            if (colspanWidths.ContainsKey(key))
                            {
                                value = colspanWidths[key];
                            }
                            else
                            {
                                value = null;
                            }

                            if (value == null || cellWidth > value)
                            {
                                colspanWidths.Add(key, cellWidth);
                            }
                        }
                        else
                        {
                            width = Math.Max(width, cellWidth);
                            marginLeft = Math.Max(marginLeft, convertToPX(cellStyle, StyleAttribute.MARGIN_LEFT, maxTableWidth, 0));
                            marginRight = Math.Max(marginRight, convertToPX(cellStyle, StyleAttribute.MARGIN_LEFT, maxTableWidth, 0));
                        }
                    }
                }

                columnsWithFixedWidth[col] = hasFixedWidth;
                columnWidth[col] = width;
                columnSpacing[col] = Math.Max(columnSpacing[col], marginLeft);
                columnSpacing[col + 1] = Math.Max(cellSpacing, marginRight);
            }

            if (colspanWidths != null)
            {
                foreach (int key in colspanWidths.Keys)
                {
                    int col = BitOperations.RightMove(key, 16);
                    int colspan = key & 0xFFFF;
                    int width = colspanWidths[key];
                    int remainingCols = colspan;

                    for (int i = 0; i < colspan; i++)
                    {
                        if (columnsWithFixedWidth[col + i])
                        {
                            width -= columnWidth[col + i];
                            remainingCols--;
                        }
                    }

                    if (width > 0)
                    {
                        for (int i = 0; i < colspan && remainingCols > 0; i++)
                        {
                            if (!columnsWithFixedWidth[col + i])
                            {
                                int colWidth = width / remainingCols;
                                columnWidth[col + i] = Math.Max(columnWidth[col + i], colWidth);
                                width -= colWidth;
                                remainingCols--;
                            }
                        }
                    }
                }
            }
        }

        private void layoutTableElement(Box box, TableElement te)
        {
            int numColumns = te.GetNumColumns();
            int numRows = te.GetNumRows();
            int cellSpacing = te.GetCellSpacing();
            int cellPadding = te.GetCellPadding();
            Style tableStyle = te.GetStyle();

            if (numColumns == 0 || numRows == 0)
            {
                return;
            }

            doMarginTop(box, tableStyle);
            LElement anchor = box.addAnchor(te);

            int left = box.computeLeftPadding(convertToPX0(tableStyle, StyleAttribute.MARGIN_LEFT, box.boxWidth));
            int right = box.computeRightPadding(convertToPX0(tableStyle, StyleAttribute.MARGIN_RIGHT, box.boxWidth));
            int maxTableWidth = Math.Max(0, right - left);
            int tableWidth = Math.Min(maxTableWidth, convertToPX(tableStyle, StyleAttribute.WIDTH, box.boxWidth, int.MinValue));
            bool autoTableWidth = tableWidth == int.MinValue;

            if (tableWidth <= 0)
            {
                tableWidth = maxTableWidth;
            }

            int[] columnWidth = new int[numColumns];
            int[] columnSpacing = new int[numColumns + 1];
            bool[] columnsWithFixedWidth = new bool[numColumns];

            columnSpacing[0] = Math.Max(cellSpacing, convertToPX0(tableStyle, StyleAttribute.PADDING_LEFT, box.boxWidth));

            computeTableWidth(te, tableWidth, columnWidth, columnSpacing, columnsWithFixedWidth);

            columnSpacing[numColumns] = Math.Max(columnSpacing[numColumns],
                    convertToPX0(tableStyle, StyleAttribute.PADDING_RIGHT, box.boxWidth));

            int columnSpacingSum = 0;
            foreach (int spacing in columnSpacing)
            {
                columnSpacingSum += spacing;
            }

            int columnWidthSum = 0;
            foreach (int width in columnWidth)
            {
                columnWidthSum += width;
            }

            if (autoTableWidth)
            {
                tableWidth = Math.Min(maxTableWidth, columnWidthSum + columnSpacingSum);
            }

            int availableColumnWidth = Math.Max(0, tableWidth - columnSpacingSum);
            if (availableColumnWidth != columnWidthSum && columnWidthSum > 0)
            {
                int available = availableColumnWidth;
                int toDistribute = columnWidthSum;
                int remainingCols = numColumns;

                for (int col = 0; col < numColumns; col++)
                {
                    if (columnsWithFixedWidth[col])
                    {
                        int width = columnWidth[col];
                        available -= width;
                        toDistribute -= width;
                        remainingCols--;
                    }
                }

                bool allColumns = false;
                if (availableColumnWidth < 0)
                {
                    available = availableColumnWidth;
                    toDistribute = columnWidthSum;
                    remainingCols = numColumns;
                    allColumns = true;
                }

                for (int col = 0; col < numColumns && remainingCols > 0; col++)
                {
                    if (allColumns || !columnsWithFixedWidth[col])
                    {
                        int width = columnWidth[col];
                        int newWidth = (toDistribute > 0) ? width * available / toDistribute : 0;
                        columnWidth[col] = newWidth;
                        available -= newWidth;
                        toDistribute -= width;
                    }
                }
            }

            LImage tableBGImage = createBGImage(box, te);

            box.textAlignment = TextAreaModel.HAlignment.LEFT;
            box.curY += Math.Max(cellSpacing, convertToPX0(tableStyle, StyleAttribute.PADDING_TOP, box.boxWidth));

            LImage[] bgImages = new LImage[numColumns];

            for (int row = 0; row < numRows; row++)
            {
                if (row > 0)
                {
                    box.curY += cellSpacing;
                }

                LImage rowBGImage = null;
                Style rowStyle = te.GetRowStyle(row);
                if (rowStyle != null)
                {
                    int marginTop = convertToPX0(rowStyle, StyleAttribute.MARGIN_TOP, tableWidth);
                    box.curY = box.computeTopPadding(marginTop);

                    Image image = selectImage(rowStyle, StyleAttribute.BACKGROUND_IMAGE);
                    if (image == null)
                    {
                        image = createBackgroundColor(rowStyle);
                    }
                    if (image != null)
                    {
                        rowBGImage = new LImage(te, image);
                        rowBGImage.y = box.curY;
                        rowBGImage.x = left;
                        rowBGImage.width = tableWidth;
                        box.clip.bgImages.Add(rowBGImage);
                    }

                    box.curY += convertToPX0(rowStyle, StyleAttribute.PADDING_TOP, tableWidth);
                    box.minLineHeight = convertToPX0(rowStyle, StyleAttribute.HEIGHT, tableWidth);
                }

                int x = left;
                for (int col = 0; col < numColumns; col++)
                {
                    x += columnSpacing[col];
                    TableCellElement cell = te.GetCell(row, col);
                    int width = columnWidth[col];
                    if (cell != null)
                    {
                        for (int c = 1; c < cell.GetColspan(); c++)
                        {
                            width += columnSpacing[col + c] + columnWidth[col + c];
                        }

                        Style cellStyle = cell.GetStyle();

                        int paddingLeft = Math.Max(cellPadding, convertToPX0(cellStyle, StyleAttribute.PADDING_LEFT, tableWidth));
                        int paddingRight = Math.Max(cellPadding, convertToPX0(cellStyle, StyleAttribute.PADDING_RIGHT, tableWidth));

                        LClip clip = new LClip(cell);
                        LImage bgImage = createBGImage(box, cell);
                        if (bgImage != null)
                        {
                            bgImage.x = x;
                            bgImage.width = width;
                            bgImage.hoverSrc = clip;
                            bgImages[col] = bgImage;
                        }

                        clip.x = x;
                        clip.y = box.curY;
                        clip.width = width;
                        clip.marginTop = (short)convertToPX0(cellStyle, StyleAttribute.MARGIN_TOP, tableWidth);
                        box.layout.Add(clip);

                        layoutBox(clip, tableWidth, paddingLeft, paddingRight, cell, null, box.doCacheText);

                        col += Math.Max(0, cell.GetColspan() - 1);
                    }
                    x += width;
                }
                box.nextLine(false);

                for (int col = 0; col < numColumns; col++)
                {
                    LImage bgImage = bgImages[col];
                    if (bgImage != null)
                    {
                        bgImage.height = box.curY - bgImage.y;
                        bgImages[col] = null;   // clear for next row
                    }
                }

                if (rowStyle != null)
                {
                    box.curY += convertToPX0(rowStyle, StyleAttribute.PADDING_BOTTOM, tableWidth);

                    if (rowBGImage != null)
                    {
                        rowBGImage.height = box.curY - rowBGImage.y;
                    }

                    doMarginBottom(box, rowStyle);
                }
            }

            box.curY += Math.Max(cellSpacing, convertToPX0(tableStyle, StyleAttribute.PADDING_BOTTOM, box.boxWidth));
            box.checkFloaters();
            box.accountMinRemaining(Math.Max(0, box.lineWidth - tableWidth));

            if (tableBGImage != null)
            {
                tableBGImage.height = box.curY - tableBGImage.y;
                tableBGImage.x = left;
                tableBGImage.width = tableWidth;
            }

            // anchor.y already set (by addAnchor)
            anchor.x = left;
            anchor.width = tableWidth;
            anchor.height = box.curY - anchor.y;

            doMarginBottom(box, tableStyle);
        }

        private LImage createBGImage(Box box, Element element)
        {
            Style style = element.GetStyle();
            Image image = selectImage(style, StyleAttribute.BACKGROUND_IMAGE);
            if (image == null)
            {
                image = createBackgroundColor(style);
            }
            if (image != null)
            {
                LImage bgImage = new LImage(element, image);
                bgImage.y = box.curY;
                box.clip.bgImages.Add(bgImage);
                return bgImage;
            }
            return null;
        }

        private Image createBackgroundColor(Style style)
        {
            Color color = style.Get(StyleAttribute.BACKGROUND_COLOR, styleClassResolver);
            if (color.Alpha != 0)
            {
                Image white = selectImage("white");
                if (white != null)
                {
                    Image image = white.CreateTintedVersion(color);
                    Color colorHover = style.Get(StyleAttribute.BACKGROUND_COLOR_HOVER, styleClassResolver);
                    if (colorHover != null)
                    {
                        return new Theme.StateSelectImage(HOVER_STATESELECT, null,
                                white.CreateTintedVersion(colorHover), image);
                    }
                    return image;
                }
            }
            return null;
        }

        static bool isSkip(char ch)
        {
            return CharUtil.IsWhitespace(ch);
        }

        static bool isPunctuation(char ch)
        {
            return ":;,.-!?".IndexOf(ch) >= 0;
        }

        static bool isBreak(char ch)
        {
            return CharUtil.IsWhitespace(ch) || isPunctuation(ch) || (ch == 0x3001) || (ch == 0x3002);
        }

        internal class Box
        {
            internal LClip clip;
            internal List<LElement> layout;
            internal List<LElement> objLeft = new List<LElement>();
            internal List<LElement> objRight = new List<LElement>();
            internal StringBuilder lineInfo = new StringBuilder();
            internal int boxLeft;
            internal int boxWidth;
            internal int boxMarginOffsetLeft;
            internal int boxMarginOffsetRight;
            internal bool doCacheText;
            internal int curY;
            internal int curX;
            internal int lineStartIdx;
            internal int lastProcessedAnchorIdx;
            internal int marginTop;
            internal int marginLeft;
            internal int marginRight;
            internal int marginBottomAbs;
            internal int marginBottomNext;
            internal int lineStartX;
            internal int lineWidth;
            internal int fontLineHeight;
            internal int minLineHeight;
            internal int lastLineEnd;
            internal int lastLineBottom;
            internal int minRemainingWidth;
            internal bool inParagraph;
            internal bool wasAutoBreak;
            internal bool wasPreformatted;
            internal TextAreaModel.HAlignment textAlignment;
            internal String href;
            internal TextAreaModel.Style style;
            internal TextArea textAreaW;

            internal Box(TextArea textAreaW, LClip clip, int paddingLeft, int paddingRight, int paddingTop, bool doCacheText)
            {
                this.textAreaW = textAreaW;
                this.clip = clip;
                this.layout = clip.layout;
                this.boxLeft = paddingLeft;
                this.boxWidth = Math.Max(0, clip.width - paddingLeft - paddingRight);
                this.boxMarginOffsetLeft = paddingLeft;
                this.boxMarginOffsetRight = paddingRight;
                this.doCacheText = doCacheText;
                this.curX = paddingLeft;
                this.curY = paddingTop;
                this.lineStartX = paddingLeft;
                this.lineWidth = boxWidth;
                this.minRemainingWidth = boxWidth;
                this.textAlignment = TextAreaModel.HAlignment.LEFT;
                System.Diagnostics.Debug.Assert(layout.Count == 0);
            }

            internal void computePadding()
            {
                int left = computeLeftPadding(marginLeft);
                int right = computeRightPadding(marginRight);

                lineStartX = left;
                lineWidth = Math.Max(0, right - left);

                if (isAtStartOfLine())
                {
                    curX = lineStartX;
                }

                accountMinRemaining(getRemaining());
            }

            internal int computeLeftPadding(int marginLeft)
            {
                int left = boxLeft + Math.Max(0, marginLeft - boxMarginOffsetLeft);

                for (int i = 0, n = objLeft.Count; i < n; i++)
                {
                    LElement e = objLeft[i];
                    left = Math.Max(left, e.x + e.width + Math.Max(e.marginRight, marginLeft));
                }

                return left;
            }

            internal int computeRightPadding(int marginRight)
            {
                int right = boxLeft + boxWidth - Math.Max(0, marginRight - boxMarginOffsetRight);

                for (int i = 0, n = objRight.Count; i < n; i++)
                {
                    LElement e = objRight[i];
                    right = Math.Min(right, e.x - Math.Max(e.marginLeft, marginRight));
                }

                return right;
            }

            internal int computePaddingWidth(int marginLeft, int marginRight)
            {
                return Math.Max(0, computeRightPadding(marginRight) - computeLeftPadding(marginLeft));
            }

            internal int computeTopPadding(int marginTop)
            {
                return Math.Max(marginBottomAbs, curY + marginTop);
            }

            internal void setMarginBottom(int marginBottom)
            {
                if (isAtStartOfLine())
                {
                    marginBottomAbs = Math.Max(marginBottomAbs, curY + marginBottom);
                }
                else
                {
                    marginBottomNext = Math.Max(marginBottomNext, marginBottom);
                }
            }

            internal int getRemaining()
            {
                return Math.Max(0, lineWidth - curX + lineStartX);
            }

            internal void accountMinRemaining(int remaining)
            {
                minRemainingWidth = Math.Min(minRemainingWidth, remaining);
            }

            internal int getXAndAdvance(int amount)
            {
                int x = curX;
                curX = x + amount;
                return x;
            }

            internal bool isAtStartOfLine()
            {
                return lineStartIdx == layout.Count;
            }

            internal bool prevOnLineEndsNotWithSpace()
            {
                int layoutSize = layout.Count;
                if (lineStartIdx < layoutSize)
                {
                    LElement le = layout[layoutSize - 1];
                    if (le is LText)
                    {
                        LText lt = (LText)le;
                        return !isSkip(lt.text[lt.end - 1]);
                    }
                    return true;
                }
                return false;
            }

            internal void checkFloaters()
            {
                removeObjFromList(objLeft);
                removeObjFromList(objRight);
                computePadding();
                // curX is set by computePadding()
            }

            internal void clearFloater(Clear clear)
            {
                if (clear != Clear.NONE)
                {
                    int targetY = -1;
                    if (clear == Clear.LEFT || clear == Clear.BOTH)
                    {
                        for (int i = 0, n = objLeft.Count; i < n; ++i)
                        {
                            LElement le = objLeft[i];
                            if (le.height != short.MaxValue)
                            {  // special case for list elements
                                targetY = Math.Max(targetY, le.y + le.height);
                            }
                        }
                    }
                    if (clear == Clear.RIGHT || clear == Clear.BOTH)
                    {
                        for (int i = 0, n = objRight.Count; i < n; ++i)
                        {
                            LElement le = objRight[i];
                            targetY = Math.Max(targetY, le.y + le.height);
                        }
                    }
                    if (targetY >= 0)
                    {
                        advanceToY(targetY);
                    }
                }
            }

            internal void advanceToY(int targetY)
            {
                nextLine(false);
                if (targetY > curY)
                {
                    curY = targetY;
                    checkFloaters();
                }
            }

            internal void advancePastFloaters(int requiredWidth, int marginLeft, int marginRight)
            {
                if (computePaddingWidth(marginLeft, marginRight) < requiredWidth)
                {
                    nextLine(false);
                    do
                    {
                        int targetY = int.MaxValue;
                        if (objLeft.Count != 0)
                        {
                            LElement le = objLeft[objLeft.Count - 1];
                            if (le.height != short.MaxValue)
                            {  // special case for list elements
                                targetY = Math.Min(targetY, le.bottom());
                            }
                        }
                        if (objRight.Count != 0)
                        {
                            LElement le = objRight[objRight.Count - 1];
                            targetY = Math.Min(targetY, le.bottom());
                        }
                        if (targetY == int.MaxValue || targetY < curY)
                        {
                            return;
                        }
                        curY = targetY;
                        checkFloaters();
                    } while (computePaddingWidth(marginLeft, marginRight) < requiredWidth);
                }
            }

            internal bool nextLine(bool force)
            {
                if (isAtStartOfLine() && (wasAutoBreak || !force))
                {
                    wasAutoBreak = !force;
                    return false;
                }

                accountMinRemaining(getRemaining());

                int targetY = curY;
                int lineHeight = minLineHeight;

                if (isAtStartOfLine())
                {
                    lineHeight = Math.Max(lineHeight, fontLineHeight);
                }
                else
                {
                    for (int idx = lineStartIdx; idx < layout.Count; idx++)
                    {
                        LElement le = layout[idx];
                        lineHeight = Math.Max(lineHeight, le.height);
                    }

                    LElement lastElement = layout[layout.Count - 1];
                    int remaining = (lineStartX + lineWidth) - (lastElement.x + lastElement.width);

                    switch (textAlignment)
                    {
                        case TextAreaModel.HAlignment.RIGHT:
                            {
                                for (int idx = lineStartIdx; idx < layout.Count; idx++)
                                {
                                    LElement le = layout[idx];
                                    le.x += remaining;
                                }
                                break;
                            }
                        case TextAreaModel.HAlignment.CENTER:
                            {
                                int offset = remaining / 2;
                                for (int idx = lineStartIdx; idx < layout.Count; idx++)
                                {
                                    LElement le = layout[idx];
                                    le.x += offset;
                                }
                                break;
                            }
                        case TextAreaModel.HAlignment.JUSTIFY:
                            if (remaining < lineWidth / 4)
                            {
                                int num = layout.Count - lineStartIdx;
                                for (int i = 1; i < num; i++)
                                {
                                    LElement le = layout[lineStartIdx + i];
                                    int offset = remaining * i / (num - 1);
                                    le.x += offset;
                                }
                            }
                            break;
                    }

                    for (int idx = lineStartIdx; idx < layout.Count; idx++)
                    {
                        LElement le = layout[idx];
                        switch (le.element.GetStyle().Get(StyleAttribute.VERTICAL_ALIGNMENT, textAreaW.styleClassResolver))
                        {
                            case TextAreaModel.VAlignment.BOTTOM:
                                le.y = lineHeight - le.height;
                                break;
                            case TextAreaModel.VAlignment.TOP:
                                le.y = 0;
                                break;
                            case TextAreaModel.VAlignment.MIDDLE:
                                le.y = (lineHeight - le.height) / 2;
                                break;
                            case TextAreaModel.VAlignment.FILL:
                                le.y = 0;
                                le.height = lineHeight;
                                break;
                        }
                        targetY = Math.Max(targetY, computeTopPadding(le.marginTop - le.y));
                        marginBottomNext = Math.Max(marginBottomNext, le.bottom() - lineHeight);
                    }

                    for (int idx = lineStartIdx; idx < layout.Count; idx++)
                    {
                        LElement le = layout[idx];
                        le.y += targetY;
                    }
                }

                processAnchors(targetY, lineHeight);

                minLineHeight = 0;
                lineStartIdx = layout.Count;
                wasAutoBreak = !force;
                curY = targetY + lineHeight;
                marginBottomAbs = Math.Max(marginBottomAbs, curY + marginBottomNext);
                marginBottomNext = 0;
                marginTop = 0;
                checkFloaters();
                // curX is set by computePadding() inside checkFloaters()
                return true;
            }

            internal void finish()
            {
                nextLine(false);
                clearFloater(Clear.BOTH);
                processAnchors(curY, 0);
                int lineInfoLength = lineInfo.Length;
                clip.lineInfo = new char[lineInfoLength];
                clip.lineInfo = lineInfo.ToString(0, lineInfoLength).ToCharArray();
            }

            internal int computeNextTabStop(Style style, Font font)
            {
                int em = font.MWidth;
                int tabSize = style.Get(StyleAttribute.TAB_SIZE, textAreaW.styleClassResolver);
                if (tabSize <= 0 || em <= 0)
                {
                    // replace with single space when tabs are disabled
                    return curX + font.SpaceWidth;
                }
                int tabSizePX = Math.Min(tabSize, short.MaxValue / em) * em;
                int x = curX - lineStartX + font.SpaceWidth;
                return curX + tabSizePX - (x % tabSizePX);
            }

            private void removeObjFromList(List<LElement> list)
            {
                for (int i = list.Count; i-- > 0;)
                {
                    LElement e = list[i];
                    if (e.bottom() <= curY)
                    {
                        // can't update marginBottomAbs here - results in layout error for text
                        list.RemoveAt(i);
                    }
                }
            }

            internal void setupTextParams(Style style, Font font, bool isParagraphStart)
            {
                if (font != null)
                {
                    fontLineHeight = font.LineHeight;
                }
                else
                {
                    fontLineHeight = 0;
                }

                if (isParagraphStart)
                {
                    nextLine(false);
                    inParagraph = true;
                }

                if (isParagraphStart || (!inParagraph && isAtStartOfLine()))
                {
                    marginLeft = textAreaW.convertToPX0(style, StyleAttribute.MARGIN_LEFT, boxWidth);
                    marginRight = textAreaW.convertToPX0(style, StyleAttribute.MARGIN_RIGHT, boxWidth);
                    textAlignment = style.Get(StyleAttribute.HORIZONTAL_ALIGNMENT, textAreaW.styleClassResolver);
                    computePadding();
                    curX = Math.Max(0, lineStartX + textAreaW.convertToPX(style, StyleAttribute.TEXT_INDENT, boxWidth, 0));
                }

                marginTop = textAreaW.convertToPX0(style, StyleAttribute.MARGIN_TOP, boxWidth);
            }

            internal LElement addAnchor(Element e)
            {
                LElement le = new LElement(e);
                le.y = curY;
                le.x = boxLeft;
                le.width = boxWidth;
                clip.anchors.Add(le);
                return le;
            }

            private void processAnchors(int y, int height)
            {
                while (lastProcessedAnchorIdx < clip.anchors.Count)
                {
                    LElement le = clip.anchors[lastProcessedAnchorIdx++];
                    if (le.height == 0)
                    {
                        le.y = y;
                        le.height = height;
                    }
                }
                if (lineStartIdx > lastLineEnd)
                {
                    lineInfo.Append((char)0).Append((char)(lineStartIdx - lastLineEnd));
                }
                if (y > lastLineBottom)
                {
                    lineInfo.Append((char)y).Append((char)0);
                }
                lastLineBottom = y + height;
                lineInfo.Append((char)lastLineBottom).Append((char)(layout.Count - lineStartIdx));
                lastLineEnd = layout.Count;
            }
        }

        public class RenderInfo
        {
            internal int offsetX;
            internal int offsetY;
            internal Renderer.Renderer renderer;
            internal AnimationState asNormal;
            internal AnimationState asHover;

            public RenderInfo(AnimationState parent)
            {
                asNormal = new AnimationState(parent);
                asNormal.setAnimationState(STATE_HOVER, false);
                asHover = new AnimationState(parent);
                asHover.setAnimationState(STATE_HOVER, true);
            }

            internal AnimationState getAnimationState(bool isHover)
            {
                return isHover ? asHover : asNormal;
            }
        }

        public class LElement
        {
            internal Element element;
            internal int x;
            internal int y;
            internal int width;
            internal int height;
            internal short marginTop;
            internal short marginLeft;
            internal short marginRight;
            internal short marginBottom;
            internal String href;
            internal bool isHover;
            internal bool inheritHover;

            public LElement(Element element)
            {
                this.element = element;
            }

            internal virtual void adjustWidget(int offX, int offY) { }
            internal virtual void collectBGImages(int offX, int offY, List<LImage> allBGImages) { }
            internal virtual void draw(RenderInfo ri) { }
            internal virtual void destroy() { }

            internal virtual bool isInside(int x, int y)
            {
                return (x >= this.x) && (x < this.x + this.width) &&
                        (y >= this.y) && (y < this.y + this.height);
            }
            internal virtual LElement find(int x, int y)
            {
                return this;
            }
            internal virtual LElement find(Element element, int[] offset)
            {
                if (this.element == element)
                {
                    return this;
                }
                return null;
            }
            internal virtual bool setHover(LElement le)
            {
                isHover = (this == le) || (le != null && element == le.element);
                return isHover;
            }

            internal virtual int bottom()
            {
                return y + height + marginBottom;
            }
        }

        public class FontData
        {
            internal Font font;
            Color color;
            Color colorHover;

            internal FontData(Font font, Color color, Color colorHover)
            {
                if (colorHover == null)
                {
                    colorHover = color;
                }
                this.font = font;
                this.color = maskWhite(color);
                this.colorHover = maskWhite(colorHover);
            }

            public Color getColor(bool isHover)
            {
                return isHover ? colorHover : color;
            }

            private static Color maskWhite(Color c)
            {
                return Color.WHITE.Equals(c) ? null : c;
            }
        }

        public class LText : LElement
        {
            internal FontData fontData;
            internal String text;
            internal int start;
            internal int end;
            internal FontCache cache;

            internal LText(Element element, FontData fontData, String text, int start, int end, bool doCache) : base(element)
            {
                Font font = fontData.font;
                this.fontData = fontData;
                this.text = text;
                this.start = start;
                this.end = end;
                Color c = fontData.getColor(isHover);
                this.cache = doCache ? font.CacheText(null, text, start, end) : null;
                this.height = font.LineHeight;

                if (cache != null)
                {
                    this.width = cache.Width;
                }
                else
                {
                    this.width = font.ComputeTextWidth(text, start, end);
                }
            }

            //@Override
            internal override void draw(RenderInfo ri)
            {
                Color c = fontData.getColor(isHover);
                if (c != null)
                {
                    drawTextWithColor(ri, c);
                }
                else
                {
                    drawText(Color.BLACK, ri);
                }
            }

            private void drawTextWithColor(RenderInfo ri, Color c)
            {
                drawText(c, ri);
            }

            private void drawText(Color c, RenderInfo ri)
            {
                AnimationState animationState = ri.getAnimationState(isHover);
                if (cache != null)
                {
                    cache.Draw(animationState, x + ri.offsetX, y + ri.offsetY);
                }
                else
                {
                    fontData.font.DrawText(animationState, x + ri.offsetX, y + ri.offsetY, text, start, end);
                }
            }

            //@Override
            internal override void destroy()
            {
                if (cache != null)
                {
                    cache.Dispose();
                    cache = null;
                }
            }
        }

        public class LWidget : LElement
        {
            Widget widget;

            internal LWidget(Element element, Widget widget) : base(element)
            {
                this.widget = widget;
            }

            //@Override
            internal override void adjustWidget(int offX, int offY)
            {
                widget.setPosition(x + offX, y + offY);
                widget.setSize(width, height);
            }
        }

        public class LImage : LElement
        {
            internal Image img;
            internal LElement hoverSrc;

            internal LImage(Element element, Image img) : base(element)
            {
                this.img = img;
                this.width = img.Width;
                this.height = img.Height;
                this.hoverSrc = this;
            }

            //@Override
            internal override void draw(RenderInfo ri)
            {
                img.Draw(ri.getAnimationState(hoverSrc.isHover),
                        x + ri.offsetX, y + ri.offsetY, width, height);
            }
        }

        public class LClip : LElement
        {
            internal List<LElement> layout;
            internal List<LImage> bgImages;
            internal List<LElement> anchors;
            internal char[] lineInfo;

            public LClip(Element element) : base(element)
            {
                this.layout = new List<LElement>();
                this.bgImages = new List<LImage>();
                this.anchors = new List<LElement>();
                this.lineInfo = EMPTY_CHAR_ARRAY;
            }

            //@Override
            internal override void draw(RenderInfo ri)
            {
                ri.offsetX += x;
                ri.offsetY += y;
                ri.renderer.ClipEnter(ri.offsetX, ri.offsetY, width, height);
                try
                {
                    if (!ri.renderer.ClipIsEmpty())
                    {
                        List<LElement> ll = layout;
                        for (int i = 0, n = ll.Count; i < n; i++)
                        {
                            ll[i].draw(ri);
                        }
                    }
                }
                finally
                {
                    ri.renderer.ClipLeave();
                    ri.offsetX -= x;
                    ri.offsetY -= y;
                }
            }

            //@Override
            internal override void adjustWidget(int offX, int offY)
            {
                offX += x;
                offY += y;
                for (int i = 0, n = layout.Count; i < n; i++)
                {
                    layout[i].adjustWidget(offX, offY);
                }
            }

            //@Override
            internal override void collectBGImages(int offX, int offY, List<LImage> allBGImages)
            {
                offX += x;
                offY += y;
                for (int i = 0, n = bgImages.Count; i < n; i++)
                {
                    LImage img = bgImages[i];
                    img.x += offX;
                    img.y += offY;
                    allBGImages.Add(img);
                }
                for (int i = 0, n = layout.Count; i < n; i++)
                {
                    layout[i].collectBGImages(offX, offY, allBGImages);
                }
            }

            //@Override
            internal override void destroy()
            {
                for (int i = 0, n = layout.Count; i < n; i++)
                {
                    layout[i].destroy();
                }
                layout.Clear();
                bgImages.Clear();
                lineInfo = EMPTY_CHAR_ARRAY;
            }

            //@Override
            internal override LElement find(int x, int y)
            {
                x -= this.x;
                y -= this.y;
                int lineTop = 0;
                int layoutIdx = 0;
                for (int lineIdx = 0; lineIdx < lineInfo.Length && y >= lineTop;)
                {
                    int lineBottom = lineInfo[lineIdx++];
                    int layoutCount = lineInfo[lineIdx++];
                    if (layoutCount > 0)
                    {
                        if (lineBottom == 0 || y < lineBottom)
                        {
                            for (int i = 0; i < layoutCount; i++)
                            {
                                LElement le = layout[layoutIdx + i];
                                if (le.isInside(x, y))
                                {
                                    return le.find(x, y);
                                }
                            }
                            if (lineBottom > 0 && x >= layout[layoutIdx].x)
                            {
                                LElement prev = null;
                                for (int i = 0; i < layoutCount; i++)
                                {
                                    LElement le = layout[layoutIdx + i];
                                    if (le.x >= x && (prev == null || prev.element == le.element))
                                    {
                                        return le;
                                    }
                                    prev = le;
                                }
                            }
                        }
                        layoutIdx += layoutCount;
                    }
                    if (lineBottom > 0)
                    {
                        lineTop = lineBottom;
                    }
                }
                return this;
            }

            //@Override
            override internal LElement find(Element element, int[] offset)
            {
                if (this.element == element)
                {
                    return this;
                }
                LElement match = find(layout, element, offset);
                if (match == null)
                {
                    match = find(anchors, element, offset);
                }
                return match;
            }

            private LElement find(List<LElement> l, Element e, int[] offset)
            {
                for (int i = 0, n = l.Count; i < n; i++)
                {
                    LElement match = l[i].find(e, offset);
                    if (match != null)
                    {
                        if (offset != null)
                        {
                            offset[0] += this.x;
                            offset[1] += this.y;
                        }
                        return match;
                    }
                }
                return null;
            }

            //@Override
            override internal bool setHover(LElement le)
            {
                bool childHover = false;
                for (int i = 0, n = layout.Count; i < n; i++)
                {
                    childHover |= layout[i].setHover(le);
                }
                if (childHover)
                {
                    isHover = true;
                }
                else
                {
                    base.setHover(le);
                }
                for (int i = 0, n = layout.Count; i < n; i++)
                {
                    LElement child = layout[i];
                    if (child.inheritHover)
                    {
                        child.isHover = isHover;
                    }
                }
                return isHover;
            }

            internal void moveContentY(int amount)
            {
                for (int i = 0, n = layout.Count; i < n; i++)
                {
                    layout[i].y += amount;
                }
                if (lineInfo.Length > 0)
                {
                    if (lineInfo[1] == 0)
                    {
                        lineInfo[0] += (char)amount;
                    }
                    else
                    {
                        int n = lineInfo.Length;
                        char[] tmpLineInfo = new char[n + 2];
                        tmpLineInfo[0] = (char)amount;
                        for (int i = 0; i < n; i += 2)
                        {
                            int lineBottom = lineInfo[i];
                            if (lineBottom > 0)
                            {
                                lineBottom += amount;
                            }
                            tmpLineInfo[i + 2] = (char)lineBottom;
                            tmpLineInfo[i + 3] = lineInfo[i + 1];
                        }
                        lineInfo = tmpLineInfo;
                    }
                }
            }
        }
    }

}
