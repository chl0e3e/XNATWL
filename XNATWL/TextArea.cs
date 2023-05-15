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
            Widget ResolveWidget(String name, String param);
        }

        public interface ImageResolver
        {
            Image ResolveImage(String name);
        }

        public interface Callback
        {
            /**
             * Called when a link has been clicked
             * @param href the href of the link
             */
            void HandleLinkClicked(String href);
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
            void HandleMouseButton(Event evt, TextAreaModel.Element element);
        }

        public static StateKey STATE_HOVER = StateKey.Get("hover");

        static char[] EMPTY_CHAR_ARRAY = new char[0];

        private Dictionary<String, Widget> _widgets;
        private Dictionary<String, WidgetResolver> _widgetResolvers;
        private Dictionary<String, Image> _userImages;
        private List<ImageResolver> _imageResolvers;

        TextAreaModel.StyleSheetResolver _styleClassResolver;
        private TextAreaModel.TextAreaModel _model;
        private ParameterMap _fonts;
        private ParameterMap _images;
        private Font _defaultFont;
        private Callback[] _callbacks;
        private MouseCursor _mouseCursorNormal;
        private MouseCursor _mouseCursorLink;
        private DraggableButton.DragListener _dragListener;

        private LClip _layoutRoot;
        private List<LImage> _allBGImages;
        private RenderInfo _renderInfo;
        private bool _inLayoutCode;
        private bool _bForceRelayout;
        private Dimension _preferredInnerSize;
        private FontMapper _fontMapper;
        private FontMapperCacheEntry[] _fontMapperCache;

        private int _lastMouseX;
        private int _lastMouseY;
        private bool _lastMouseInside;
        private bool _dragging;
        private int _dragStartX;
        private int _dragStartY;
        private LElement _curLElementUnderMouse;

        public event EventHandler<TextAreaChangedEventArgs> Changed;

        public TextArea()
        {
            this._widgets = new Dictionary<String, Widget>();
            this._widgetResolvers = new Dictionary<String, WidgetResolver>();
            this._userImages = new Dictionary<String, Image>();
            this._imageResolvers = new List<ImageResolver>();
            this._layoutRoot = new LClip(null);
            this._allBGImages = new List<LImage>();
            this._renderInfo = new RenderInfo(GetAnimationState());

            //this.modelCB = new Runnable() {
            //    public void run() {
            //       forceRelayout();
            //   }
            //};
        }

        public TextArea(TextAreaModel.TextAreaModel model) : this()
        {
            SetModel(model);
        }

        public TextAreaModel.TextAreaModel GetModel()
        {
            return _model;
        }

        public void SetModel(TextAreaModel.TextAreaModel model)
        {
            if (this._model != null)
            {
                this._model.Changed -= Model_Changed;
            }
            this._model = model;
            if (model != null)
            {
                this._model.Changed += Model_Changed;
            }
            ForceRelayout();
        }

        private void Model_Changed(object sender, TextAreaChangedEventArgs e)
        {

        }

        public void RegisterWidget(String name, Widget widget)
        {
            if (name == null)
            {
                throw new NullReferenceException("name");
            }
            if (widget.GetParent() != null)
            {
                throw new ArgumentOutOfRangeException("Widget must not have a parent");
            }
            if (_widgets.ContainsKey(name) || _widgetResolvers.ContainsKey(name))
            {
                throw new ArgumentOutOfRangeException("widget name already in registered");
            }
            if (_widgets.ContainsValue(widget))
            {
                throw new ArgumentOutOfRangeException("widget already registered");
            }
            _widgets.Add(name, widget);
        }

        public void RegisterWidgetResolver(String name, WidgetResolver resolver)
        {
            if (name == null)
            {
                throw new NullReferenceException("name");
            }
            if (resolver == null)
            {
                throw new NullReferenceException("resolver");
            }
            if (_widgets.ContainsKey(name) || _widgetResolvers.ContainsKey(name))
            {
                throw new ArgumentOutOfRangeException("widget name already in registered");
            }
            _widgetResolvers.Add(name, resolver);
        }

        public void UnregisterWidgetResolver(String name)
        {
            if (name == null)
            {
                throw new NullReferenceException("name");
            }
            _widgetResolvers.Remove(name);
        }

        public void UnregisterWidget(String name)
        {
            if (name == null)
            {
                throw new NullReferenceException("name");
            }
            Widget w = _widgets[name];
            if (w != null)
            {
                int idx = GetChildIndex(w);
                if (idx >= 0)
                {
                    base.RemoveChild(idx);
                    ForceRelayout();
                }
            }
        }

        public void UnregisterAllWidgets()
        {
            _widgets.Clear();
            base.RemoveAllChildren();
            ForceRelayout();
        }

        public void RegisterImage(String name, Image image)
        {
            if (name == null)
            {
                throw new NullReferenceException("name");
            }
            _userImages.Add(name, image);
        }

        public void RegisterImageResolver(ImageResolver resolver)
        {
            if (resolver == null)
            {
                throw new NullReferenceException("resolver");
            }
            if (!_imageResolvers.Contains(resolver))
            {
                _imageResolvers.Add(resolver);
            }
        }

        public void UnregisterImage(String name)
        {
            _userImages.Remove(name);
        }

        public void UnregisterImageResolver(ImageResolver imageResolver)
        {
            _imageResolvers.Remove(imageResolver);
        }

        public DraggableButton.DragListener GetDragListener()
        {
            return _dragListener;
        }

        public void SetDragListener(DraggableButton.DragListener dragListener)
        {
            this._dragListener = dragListener;
        }

        public TextAreaModel.StyleSheetResolver GetStyleClassResolver()
        {
            return _styleClassResolver;
        }

        public void SetStyleClassResolver(TextAreaModel.StyleSheetResolver styleClassResolver)
        {
            this._styleClassResolver = styleClassResolver;
            ForceRelayout();
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
        public void SetDefaultStyleSheet()
        {
            //try
            {
                StyleSheet styleSheet = new StyleSheet();
                styleSheet.Parse("p,ul{margin-bottom:1em}");
                SetStyleClassResolver(styleSheet);
            }
            //catch (Exception ex)
            {
                //    Logger.GetLogger(typeof(TextArea)).log(Logger.Level.SEVERE,
                //           "Can't create default style sheet", ex);
            }
        }

        public Rect GetElementRect(Element element)
        {
            int[] offset = new int[2];
            LElement le = _layoutRoot.Find(element, offset);
            if (le != null)
            {
                return new Rect(le._x + offset[0], le._y + offset[1], le._width, le._height);
            }
            else
            {
                return null;
            }
        }

        //@Override
        protected override void ApplyTheme(ThemeInfo themeInfo)
        {
            base.ApplyTheme(themeInfo);
            ApplyThemeTextArea(themeInfo);
        }

        protected void ApplyThemeTextArea(ThemeInfo themeInfo)
        {
            _fonts = themeInfo.GetParameterMap("fonts");
            _images = themeInfo.GetParameterMap("images");
            _defaultFont = themeInfo.GetFont("font");
            _mouseCursorNormal = themeInfo.GetMouseCursor("mouseCursor");
            _mouseCursorLink = themeInfo.GetMouseCursor("mouseCursor.link");
            ForceRelayout();
        }

        //@Override
        protected override void AfterAddToGUI(GUI gui)
        {
            base.AfterAddToGUI(gui);
            _renderInfo._asNormal.SetGUI(gui);
            _renderInfo._asHover.SetGUI(gui);
        }

        //@Override
        public override void InsertChild(Widget child, int index)
        {
            throw new InvalidOperationException("use registerWidget");
        }

        //@Override
        public override void RemoveAllChildren()
        {
            throw new InvalidOperationException("use registerWidget");
        }

        //@Override
        public override Widget RemoveChild(int index)
        {
            throw new InvalidOperationException("use registerWidget");
        }

        private void ComputePreferredInnerSize()
        {
            int prefWidth = -1;
            int prefHeight = -1;

            if (_model == null)
            {
                prefWidth = 0;
                prefHeight = 0;

            }
            else if (GetMaxWidth() > 0)
            {
                int borderHorizontal = GetBorderHorizontal();
                int maxWidth = Math.Max(0, GetMaxWidth() - borderHorizontal);
                int minWidth = Math.Max(0, GetMinWidth() - borderHorizontal);

                if (minWidth < maxWidth)
                {
                    //System.out.println("Doing preferred size computation");

                    LClip tmpRoot = new LClip(null);
                    StartLayout();
                    try
                    {
                        tmpRoot._width = maxWidth;
                        Box box = new Box(this, tmpRoot, 0, 0, 0, false);
                        LayoutElements(box, _model);
                        box.Finish();

                        prefWidth = Math.Max(0, maxWidth - box._minRemainingWidth);
                        prefHeight = box._curY;
                    }
                    finally
                    {
                        EndLayout();
                    }
                }
            }
            _preferredInnerSize = new Dimension(prefWidth, prefHeight);
        }

        //@Override
        public override int GetPreferredInnerWidth()
        {
            if (_preferredInnerSize == null)
            {
                ComputePreferredInnerSize();
            }
            if (_preferredInnerSize.X >= 0)
            {
                return _preferredInnerSize.X;
            }
            return GetInnerWidth();
        }

        //@Override
        public override int GetPreferredInnerHeight()
        {
            if (GetInnerWidth() == 0)
            {
                if (_preferredInnerSize == null)
                {
                    ComputePreferredInnerSize();
                }
                if (_preferredInnerSize.Y >= 0)
                {
                    return _preferredInnerSize.Y;
                }
            }
            ValidateLayout();
            return _layoutRoot._height;
        }

        //@Override
        public override int GetPreferredWidth()
        {
            int maxWidth = GetMaxWidth();
            return ComputeSize(GetMinWidth(), base.GetPreferredWidth(), maxWidth);
        }

        //@Override
        public override void SetMaxSize(int width, int height)
        {
            if (width != GetMaxWidth())
            {
                _preferredInnerSize = null;
                InvalidateLayout();
            }
            base.SetMaxSize(width, height);
        }

        //@Override
        public override void SetMinSize(int width, int height)
        {
            if (width != GetMinWidth())
            {
                _preferredInnerSize = null;
                InvalidateLayout();
            }
            base.SetMinSize(width, height);
        }

        //@Override
        protected override void Layout()
        {
            int targetWidth = GetInnerWidth();

            //System.out.println(this+" minWidth="+getMinWidth()+" width="+getWidth()+" maxWidth="+getMaxWidth()+" targetWidth="+targetWidth+" preferredInnerSize="+preferredInnerSize);

            // only recompute the layout when it has changed
            if (_layoutRoot._width != targetWidth || _bForceRelayout)
            {
                var old = _layoutRoot._width;
                _layoutRoot._width = targetWidth;
                _inLayoutCode = true;
                _bForceRelayout = false;
                int requiredHeight;

                StartLayout();
                try
                {
                    ClearLayout();
                    Box box = new Box(this, _layoutRoot, 0, 0, 0, true);
                    if (_model != null)
                    {
                        LayoutElements(box, _model);

                        box.Finish();

                        // set position & size of all widget elements
                        _layoutRoot.AdjustWidget(GetInnerX(), GetInnerY());
                        _layoutRoot.CollectBGImages(0, 0, _allBGImages);
                    }
                    UpdateMouseHover();
                    requiredHeight = box._curY;
                }
                finally
                {
                    _inLayoutCode = false;
                    EndLayout();
                }

                if (_layoutRoot._height != requiredHeight)
                {
                    _layoutRoot._height = requiredHeight;
                    if (GetInnerHeight() != requiredHeight)
                    {
                        // call outside of inLayoutCode range
                        InvalidateLayout();
                    }
                }
            }
        }

        //@Override
        protected override void PaintWidget(GUI gui)
        {
            List<LImage> bi = _allBGImages;
            RenderInfo ri = _renderInfo;
            ri._offsetX = GetInnerX();
            ri._offsetY = GetInnerY();
            ri._renderer = gui.GetRenderer();

            for (int i = 0, n = bi.Count; i < n; i++)
            {
                bi[i].Draw(ri);
            }

            _layoutRoot.Draw(ri);
        }

        //@Override
        protected override void SizeChanged()
        {
            if (!_inLayoutCode)
            {
                InvalidateLayout();
            }
        }

        //@Override
        protected override void ChildAdded(Widget child)
        {
            // always ignore
        }

        //@Override
        protected override void ChildRemoved(Widget exChild)
        {
            // always ignore
        }

        //@Override
        protected override void AllChildrenRemoved()
        {
            // always ignore
        }

        //@Override
        public override void Destroy()
        {
            base.Destroy();
            ClearLayout();
            ForceRelayout();
        }

        //@Override
        public override bool HandleEvent(Event evt)
        {
            if (base.HandleEvent(evt))
            {
                return true;
            }

            if (evt.IsMouseEvent())
            {
                EventType eventType = evt.GetEventType();

                if (_dragging)
                {
                    if (eventType == EventType.MOUSE_DRAGGED)
                    {
                        if (_dragListener != null)
                        {
                            _dragListener.Dragged(evt.GetMouseX() - _dragStartX, evt.GetMouseY() - _dragStartY);
                        }
                    }
                    if (evt.IsMouseDragEnd())
                    {
                        if (_dragListener != null)
                        {
                            _dragListener.DragStopped();
                        }
                        _dragging = false;
                        UpdateMouseHover(evt);
                    }
                    return true;
                }

                UpdateMouseHover(evt);

                if (eventType == EventType.MOUSE_WHEEL)
                {
                    return false;
                }

                if (eventType == EventType.MOUSE_BTNDOWN)
                {
                    _dragStartX = evt.GetMouseX();
                    _dragStartY = evt.GetMouseY();
                }

                if (eventType == EventType.MOUSE_DRAGGED)
                {
                    System.Diagnostics.Debug.Assert(!_dragging);
                    _dragging = true;
                    if (_dragListener != null)
                    {
                        _dragListener.DragStarted();
                    }
                    return true;
                }

                if (_curLElementUnderMouse != null && (
                        eventType == EventType.MOUSE_CLICKED ||
                        eventType == EventType.MOUSE_BTNDOWN ||
                        eventType == EventType.MOUSE_BTNUP))
                {
                    Element e = _curLElementUnderMouse._element;
                    if (_callbacks != null)
                    {
                        foreach (Callback l in _callbacks)
                        {
                            if (l is Callback2)
                            {
                                ((Callback2)l).HandleMouseButton(evt, e);
                            }
                        }
                    }
                }

                if (eventType == EventType.MOUSE_CLICKED)
                {
                    if (_curLElementUnderMouse != null && _curLElementUnderMouse._href != null)
                    {
                        String href = _curLElementUnderMouse._href;
                        if (_callbacks != null)
                        {
                            foreach (Callback l in _callbacks)
                            {
                                l.HandleLinkClicked(href);
                            }
                        }
                    }
                }

                return true;
            }

            return false;
        }

        //@Override
        internal override Object GetTooltipContentAt(int mouseX, int mouseY)
        {
            if (_curLElementUnderMouse != null)
            {
                if (_curLElementUnderMouse._element is ImageElement)
                {
                    return ((ImageElement)_curLElementUnderMouse._element).GetToolTip();
                }
            }
            return base.GetTooltipContentAt(mouseX, mouseY);
        }

        private void UpdateMouseHover(Event evt)
        {
            _lastMouseInside = IsMouseInside(evt);
            _lastMouseX = evt.GetMouseX();
            _lastMouseY = evt.GetMouseY();
            UpdateMouseHover();
        }

        private void UpdateMouseHover()
        {
            LElement le = null;
            if (_lastMouseInside)
            {
                le = _layoutRoot.Find(_lastMouseX - GetInnerX(), _lastMouseY - GetInnerY());
            }
            if (_curLElementUnderMouse != le)
            {
                _curLElementUnderMouse = le;
                _layoutRoot.SetHover(le);
                _renderInfo._asNormal.ResetAnimationTime(STATE_HOVER);
                _renderInfo._asHover.ResetAnimationTime(STATE_HOVER);
                UpdateTooltip();
            }

            if (le != null && le._href != null)
            {
                SetMouseCursor(_mouseCursorLink);
            }
            else
            {
                SetMouseCursor(_mouseCursorNormal);
            }

            GetAnimationState().SetAnimationState(STATE_HOVER, _lastMouseInside);
        }

        void ForceRelayout()
        {
            _bForceRelayout = true;
            _preferredInnerSize = null;
            InvalidateLayout();
        }

        private void ClearLayout()
        {
            _layoutRoot.Destroy();
            _allBGImages.Clear();
            base.RemoveAllChildren();
        }

        private void StartLayout()
        {
            if (_styleClassResolver != null)
            {
                _styleClassResolver.StartLayout();
            }

            GUI gui = GetGUI();
            _fontMapper = (gui != null) ? gui.GetRenderer().FontMapper : null;
            _fontMapperCache = null;
        }

        private void EndLayout()
        {
            if (_styleClassResolver != null)
            {
                _styleClassResolver.LayoutFinished();
            }
            _fontMapper = null;
            _fontMapperCache = null;
        }

        private void LayoutElements(Box box, IEnumerable<Element> elements)
        {
            foreach (Element e in elements)
            {
                LayoutElement(box, e);
            }
        }

        private void LayoutElement(Box box, Element e)
        {
            box.ClearFloater(e.GetStyle().Get(StyleAttribute.CLEAR, _styleClassResolver));

            if (e is TextElement)
            {
                LayoutTextElement(box, (TextElement)e);
            }
            else if (e is LineBreakElement)
            {
                box.NextLine(true);
            }
            else
            {
                if (box._wasPreformatted)
                {
                    box.NextLine(false);
                    box._wasPreformatted = false;
                }
                if (e is ParagraphElement)
                {
                    LayoutParagraphElement(box, (ParagraphElement)e);
                }
                else if (e is ImageElement)
                {
                    LayoutImageElement(box, (ImageElement)e);
                }
                else if (e is WidgetElement)
                {
                    LayoutWidgetElement(box, (WidgetElement)e);
                }
                else if (e is ListElement)
                {
                    LayoutListElement(box, (ListElement)e);
                }
                else if (e is OrderedListElement)
                {
                    LayoutOrderedListElement(box, (OrderedListElement)e);
                }
                else if (e is BlockElement)
                {
                    LayoutBlockElement(box, (BlockElement)e);
                }
                else if (e is TableElement)
                {
                    LayoutTableElement(box, (TableElement)e);
                }
                else if (e is LinkElement)
                {
                    LayoutLinkElement(box, (LinkElement)e);
                }
                else if (e is ContainerElement)
                {
                    LayoutContainerElement(box, (ContainerElement)e);
                }
                else
                {
                    Logger.GetLogger(typeof(TextArea)).Log(Logger.Level.SEVERE, "Unknown Element subclass: {0}" + e.GetType().FullName);
                }
            }
        }

        private void LayoutImageElement(Box box, ImageElement ie)
        {
            Image image = SelectImage(ie.GetImageName());
            if (image == null)
            {
                return;
            }

            LImage li = new LImage(ie, image);
            li._href = box._href;
            Layout(box, ie, li);
        }

        private void LayoutWidgetElement(Box box, WidgetElement we)
        {
            Widget widget = _widgets[we.GetWidgetName()];
            if (widget == null)
            {
                WidgetResolver resolver = _widgetResolvers[we.GetWidgetName()];
                if (resolver != null)
                {
                    widget = resolver.ResolveWidget(we.GetWidgetName(), we.GetWidgetParam());
                }
                if (widget == null)
                {
                    return;
                }
            }

            if (widget.GetParent() != null)
            {
                Logger.GetLogger(typeof(TextArea)).Log(Logger.Level.SEVERE, "Widget already added: " + widget.GetThemePath());
                return;
            }

            base.InsertChild(widget, GetNumChildren());
            widget.AdjustSize();

            LWidget lw = new LWidget(we, widget);
            lw._width = widget.GetWidth();
            lw._height = widget.GetHeight();

            Layout(box, we, lw);
        }

        private void Layout(Box box, Element e, LElement le)
        {
            Style style = e.GetStyle();

            FloatPosition floatPosition = style.Get(StyleAttribute.FLOAT_POSITION, _styleClassResolver);
            Display display = style.Get(StyleAttribute.DISPLAY, _styleClassResolver);

            le._marginTop = (short)ConvertToPX0(style, StyleAttribute.MARGIN_TOP, box._boxWidth);
            le._marginLeft = (short)ConvertToPX0(style, StyleAttribute.MARGIN_LEFT, box._boxWidth);
            le._marginRight = (short)ConvertToPX0(style, StyleAttribute.MARGIN_RIGHT, box._boxWidth);
            le._marginBottom = (short)ConvertToPX0(style, StyleAttribute.MARGIN_BOTTOM, box._boxWidth);

            int autoHeight = le._height;
            int width = ConvertToPX(style, StyleAttribute.WIDTH, box._boxWidth, le._width);
            if (width > 0)
            {
                if (le._width > 0)
                {
                    autoHeight = width * le._height / le._width;
                }
                le._width = width;
            }

            int height = ConvertToPX(style, StyleAttribute.HEIGHT, le._height, autoHeight);
            if (height > 0)
            {
                le._height = height;
            }

            Layout(box, e, le, floatPosition, display);
        }

        private void Layout(Box box, Element e, LElement le, FloatPosition floatPos, Display display)
        {
            bool leftRight = (floatPos != FloatPosition.NONE);

            if (leftRight || display != Display.INLINE)
            {
                box.NextLine(false);
                if (!leftRight)
                {
                    box._curY = box.ComputeTopPadding(le._marginTop);
                    box.CheckFloaters();
                }
            }

            box.AdvancePastFloaters(le._width, le._marginLeft, le._marginRight);
            if (le._width > box._lineWidth)
            {
                le._width = box._lineWidth;
            }

            if (leftRight)
            {
                if (floatPos == FloatPosition.RIGHT)
                {
                    le._x = box.ComputeRightPadding(le._marginRight) - le._width;
                    box._objRight.Add(le);
                }
                else
                {
                    le._x = box.ComputeLeftPadding(le._marginLeft);
                    box._objLeft.Add(le);
                }
            }
            else if (display == Display.INLINE)
            {
                if (box.GetRemaining() < le._width && !box.IsAtStartOfLine())
                {
                    box.NextLine(false);
                }
                le._x = box.GetXAndAdvance(le._width);
            }
            else
            {
                switch (e.GetStyle().Get(StyleAttribute.HORIZONTAL_ALIGNMENT, _styleClassResolver))
                {
                    case TextAreaModel.HAlignment.CENTER:
                    case TextAreaModel.HAlignment.JUSTIFY:
                        le._x = box._lineStartX + (box._lineWidth - le._width) / 2;
                        break;

                    case TextAreaModel.HAlignment.RIGHT:
                        le._x = box.ComputeRightPadding(le._marginRight) - le._width;
                        break;

                    default:
                        le._x = box.ComputeLeftPadding(le._marginLeft);
                        break;
                }
            }

            box._layout.Add(le);

            if (leftRight)
            {
                System.Diagnostics.Debug.Assert(box._lineStartIdx == box._layout.Count - 1);
                box._lineStartIdx++;
                le._y = box.ComputeTopPadding(le._marginTop);
                box.ComputePadding();
            }
            else if (display != Display.INLINE)
            {
                box.AccountMinRemaining(Math.Max(0, box._lineWidth - le._width));
                box.NextLine(false);
            }
        }

        private static int DEFAULT_FONT_SIZE = 14;

        int ConvertToPX(Style style, StyleAttribute<Value> attribute, int full, int auto)
        {
            style = style.Resolve(attribute, _styleClassResolver);
            Value valueUnit = style.GetNoResolve(attribute, _styleClassResolver);

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
                font = SelectFont(style);
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

        int ConvertToPX0(Style style, StyleAttribute<Value> attribute, int full)
        {
            return Math.Max(0, ConvertToPX(style, attribute, full, 0));
        }

        private Font SelectFont(Style style)
        {
            List<string> fontFamilies = style.Get(StyleAttribute.FONT_FAMILIES, _styleClassResolver);
            if (fontFamilies != null)
            {
                if (_fontMapper != null)
                {
                    Font font = SelectFontMapper(style, _fontMapper, fontFamilies);
                    if (font != null)
                    {
                        return font;
                    }
                }

                if (_fonts != null)
                {
                    foreach (string fontFamily in fontFamilies)
                    {
                        Font font = _fonts.GetFont(fontFamily);
                        if (font != null)
                        {
                            return font;
                        }
                    }
                }
            }
            return _defaultFont;
        }

        private static StateSelect HOVER_STATESELECT = new StateSelect(new Check(STATE_HOVER));

        private static int FONT_MAPPER_CACHE_SIZE = 16;

        public class FontMapperCacheEntry
        {
            internal int _fontSize;
            internal int _fontStyle;
            internal List<string> _fontFamilies;
            internal TextDecoration _tdNormal;
            internal TextDecoration _tdHover;
            internal int _hashCode;
            internal Font _font;
            internal FontMapperCacheEntry _next;

            internal FontMapperCacheEntry(int fontSize, int fontStyle, List<string> fontFamilies, TextDecoration tdNormal, TextDecoration tdHover, int hashCode, Font font)
            {
                this._fontSize = fontSize;
                this._fontStyle = fontStyle;
                this._fontFamilies = fontFamilies;
                this._tdNormal = tdNormal;
                this._tdHover = tdHover;
                this._hashCode = hashCode;
                this._font = font;
            }
        }

        private Font SelectFontMapper(Style style, FontMapper fontMapper, List<string> fontFamilies)
        {
            int fontSize = ConvertToPX(style, StyleAttribute.FONT_SIZE, DEFAULT_FONT_SIZE, DEFAULT_FONT_SIZE);
            int fontStyle = 0;
            if (style.Get(StyleAttribute.FONT_WEIGHT, _styleClassResolver) >= 550)
            {
                fontStyle |= FontMapperStatics.STYLE_BOLD;
            }
            if (style.Get(StyleAttribute.FONT_ITALIC, _styleClassResolver))
            {
                fontStyle |= FontMapperStatics.STYLE_ITALIC;
            }

            TextDecoration textDecoration = (TextDecoration)style.GetAsObject(StyleAttribute.TEXT_DECORATION, _styleClassResolver);
            TextDecoration textDecorationHover = (TextDecoration)style.GetAsObject(StyleAttribute.TEXT_DECORATION_HOVER, _styleClassResolver);

            int hashCode = fontSize;
            hashCode = hashCode * 67 + fontStyle;
            hashCode = hashCode * 67 + fontFamilies.GetHashCode();
            hashCode = hashCode * 67 + textDecoration.GetHashCode();
            hashCode = hashCode * 67 + ((textDecorationHover != null) ? textDecorationHover.GetHashCode() : 0);

            int cacheIdx = hashCode & (FONT_MAPPER_CACHE_SIZE - 1);

            if (_fontMapperCache != null)
            {
                for (FontMapperCacheEntry cache = _fontMapperCache[cacheIdx]; cache != null; cache = cache._next)
                {
                    if (cache._hashCode == hashCode &&
                            cache._fontSize == fontSize &&
                            cache._fontStyle == fontStyle &&
                            cache._tdNormal == textDecoration &&
                            cache._tdHover == textDecorationHover &&
                            cache._fontFamilies.Equals(fontFamilies))
                    {
                        return cache._font;
                    }
                }
            }
            else
            {
                _fontMapperCache = new FontMapperCacheEntry[FONT_MAPPER_CACHE_SIZE];
            }

            FontParameter fpNormal = CreateFontParameter(textDecoration);

            StateSelect select;
            FontParameter[] parameters;

            if (textDecorationHover != null)
            {
                FontParameter fpHover = CreateFontParameter(textDecorationHover);

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
            ce._next = _fontMapperCache[cacheIdx];
            _fontMapperCache[cacheIdx] = ce;

            return font;
        }

        private static FontParameter CreateFontParameter(TextDecoration deco)
        {
            FontParameter fp = new FontParameter();
            fp.Put(FontParameter.UNDERLINE, deco == TextDecoration.UNDERLINE);
            fp.Put(FontParameter.LINETHROUGH, deco == TextDecoration.LINE_THROUGH);
            return fp;
        }

        private FontData CreateFontData(Style style)
        {
            Font font = SelectFont(style);
            if (font == null)
            {
                return null;
            }

            return new FontData(font,
                    style.Get(StyleAttribute.COLOR, _styleClassResolver),
                    style.Get(StyleAttribute.COLOR_HOVER, _styleClassResolver));
        }

        private Image SelectImage(Style style, StyleAttribute<String> element)
        {
            String imageName = style.Get(element, _styleClassResolver);
            if (imageName != null)
            {
                return SelectImage(imageName);
            }
            else
            {
                return null;
            }
        }

        private Image SelectImage(String name)
        {
            Image image = null;
            if (_userImages.ContainsKey(name))
            {
                image = _userImages[name];
            }
            if (image != null)
            {
                return image;
            }
            for (int i = 0; i < _imageResolvers.Count; i++)
            {
                image = _imageResolvers[i].ResolveImage(name);
                if (image != null)
                {
                    return image;
                }
            }
            if (_images != null)
            {
                return _images.GetImage(name);
            }
            return null;
        }

        private void LayoutParagraphElement(Box box, ParagraphElement pe)
        {
            Style style = pe.GetStyle();
            Font font = SelectFont(style);

            DoMarginTop(box, style);
            LElement anchor = box.AddAnchor(pe);
            box.SetupTextParams(style, font, true);

            LayoutElements(box, pe);

            if (box._textAlignment == TextAreaModel.HAlignment.JUSTIFY)
            {
                box._textAlignment = TextAreaModel.HAlignment.LEFT;
            }
            box.NextLine(false);
            box._inParagraph = false;

            anchor._height = box._curY - anchor._y;
            DoMarginBottom(box, style);
        }

        private void LayoutTextElement(Box box, TextElement te)
        {
            String text = te.GetText();
            Style style = te.GetStyle();
            FontData fontData = CreateFontData(style);
            bool pre = style.Get(StyleAttribute.PREFORMATTED, _styleClassResolver);

            if (fontData == null)
            {
                return;
            }

            bool inheritHover;
            object inheritHoverStyle = style.Resolve(StyleAttribute.INHERIT_HOVER, _styleClassResolver).GetRawAsObject(StyleAttribute.INHERIT_HOVER);
            if (inheritHoverStyle != null)
            {
                inheritHover = (bool)inheritHoverStyle;
            }
            else
            {
                inheritHover = (box._style != null) && (box._style == style.Parent);
            }

            box.SetupTextParams(style, fontData._font, false);

            if (pre && !box._wasPreformatted)
            {
                box.NextLine(false);
            }

            if (pre)
            {
                int idx = 0;
                while (idx < text.Length)
                {
                    int end = TextUtil.IndexOf(text, '\n', idx);
                    LayoutTextPre(box, te, fontData, text, idx, end, inheritHover);
                    if (end < text.Length && text[end] == '\n')
                    {
                        end++;
                        box.NextLine(true);
                    }
                    idx = end;
                }
            }
            else
            {
                LayoutText(box, te, fontData, text, 0, text.Length, inheritHover);
            }

            box._wasPreformatted = pre;
        }

        private void LayoutText(Box box, TextElement te, FontData fontData,
                String text, int textStart, int textEnd, bool inheritHover)
        {
            int idx = textStart;
            // trim start
            while (textStart < textEnd && IsSkip(text[textStart]))
            {
                textStart++;
            }
            // trim end
            bool endsWithSpace = false;
            while (textEnd > textStart && IsSkip(text[textEnd - 1]))
            {
                endsWithSpace = true;
                textEnd--;
            }

            Font font = fontData._font;

            // check if we skipped white spaces and the previous element in this
            // row was not a text cell
            if (textStart > idx && box.PrevOnLineEndsNotWithSpace())
            {
                box._curX += font.SpaceWidth;
            }

            object breakWord = null;    // lazy lookup

            idx = textStart;
            while (idx < textEnd)
            {
                System.Diagnostics.Debug.Assert(!IsSkip(text[idx]));

                int end = idx;
                int visibleEnd = idx;
                if (box._textAlignment != TextAreaModel.HAlignment.JUSTIFY)
                {
                    end = idx + font.ComputeVisibleGlyphs(text, idx, textEnd, box.GetRemaining());
                    visibleEnd = end;

                    if (end < textEnd)
                    {
                        // if we are at a punctuation then walk backwards until we hit
                        // the word or a break. This ensures that the punctuation stays
                        // at the end of a word
                        while (end > idx && IsPunctuation(text[end]))
                        {
                            end--;
                        }

                        // if we are not at the end of this text element
                        // and the next character is not a space
                        if (!IsBreak(text[end]))
                        {
                            // then we walk backwards until we find spaces
                            // this prevents the line ending in the middle of a word
                            while (end > idx && !IsBreak(text[end - 1]))
                            {
                                end--;
                            }
                        }
                    }

                    // now walks backwards until we hit the end of the previous word
                    while (end > idx && IsSkip(text[end - 1]))
                    {
                        end--;
                    }
                }

                bool advancePastFloaters = false;

                // if we found no word that fits
                if (end == idx)
                {
                    // we may need a new line
                    if (box._textAlignment != TextAreaModel.HAlignment.JUSTIFY && box.NextLine(false))
                    {
                        continue;
                    }
                    if (breakWord == null)
                    {
                        breakWord = te.GetStyle().Get(StyleAttribute.BREAKWORD, _styleClassResolver);
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
                        while (end < textEnd && !IsBreak(text[end]))
                        {
                            end++;
                        }
                        // some characters need to stay at the end of a word
                        while (end < textEnd && IsPunctuation(text[end]))
                        {
                            end++;
                        }
                    }
                    advancePastFloaters = true;
                }

                if (idx < end)
                {
                    LText lt = new LText(te, fontData, text, idx, end, box._doCacheText);
                    if (advancePastFloaters)
                    {
                        box.AdvancePastFloaters(lt._width, box._marginLeft, box._marginRight);
                    }
                    if (box._textAlignment == TextAreaModel.HAlignment.JUSTIFY && box.GetRemaining() < lt._width)
                    {
                        box.NextLine(false);
                    }

                    int width = lt._width;
                    if (end < textEnd && IsSkip(text[end]))
                    {
                        width += font.SpaceWidth;
                    }

                    lt._x = box.GetXAndAdvance(width);
                    lt._marginTop = (short)box._marginTop;
                    lt._href = box._href;
                    lt._inheritHover = inheritHover;
                    box._layout.Add(lt);
                }

                // find the start of the next word
                idx = end;
                while (idx < textEnd && IsSkip(text[idx]))
                {
                    idx++;
                }
            }

            if (!box.IsAtStartOfLine() && endsWithSpace)
            {
                box._curX += font.SpaceWidth;
            }
        }

        private void LayoutTextPre(Box box, TextElement te, FontData fontData,
                String text, int textStart, int textEnd, bool inheritHover)
        {
            Font font = fontData._font;
            int idx = textStart;
            for (; ; )
            {
                while (idx < textEnd)
                {
                    if (text[idx] == '\t')
                    {
                        idx++;
                        int tabX = box.ComputeNextTabStop(te.GetStyle(), font);
                        if (tabX < box._lineWidth)
                        {
                            box._curX = tabX;
                        }
                        else if (!box.IsAtStartOfLine())
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
                        int count = font.ComputeVisibleGlyphs(text, idx, end, box.GetRemaining());
                        if (count == 0 && !box.IsAtStartOfLine())
                        {
                            break;
                        }

                        end = idx + Math.Max(1, count);

                        LText lt = new LText(te, fontData, text, idx, end, box._doCacheText);
                        lt._x = box.GetXAndAdvance(lt._width);
                        lt._marginTop = (short)box._marginTop;
                        lt._inheritHover = inheritHover;
                        box._layout.Add(lt);
                    }

                    idx = end;
                }

                if (idx >= textEnd)
                {
                    break;
                }

                box.NextLine(false);
            }
        }

        private void DoMarginTop(Box box, Style style)
        {
            int marginTop = ConvertToPX0(style, StyleAttribute.MARGIN_TOP, box._boxWidth);
            box.NextLine(false);    // need to complete line before computing targetY
            box.AdvanceToY(box.ComputeTopPadding(marginTop));
        }

        private void DoMarginBottom(Box box, Style style)
        {
            int marginBottom = ConvertToPX0(style, StyleAttribute.MARGIN_BOTTOM, box._boxWidth);
            box.SetMarginBottom(marginBottom);
        }

        private void LayoutContainerElement(Box box, ContainerElement ce)
        {
            Style style = ce.GetStyle();
            DoMarginTop(box, style);
            box.AddAnchor(ce);
            LayoutElements(box, ce);
            DoMarginBottom(box, style);
        }

        private void LayoutLinkElement(Box box, LinkElement le)
        {
            String oldHref = box._href;
            box._href = le.GetHREF();

            Style style = le.GetStyle();
            Display display = style.Get(StyleAttribute.DISPLAY, _styleClassResolver);
            if (display == Display.BLOCK)
            {
                LayoutBlockElement(box, le);
            }
            else
            {
                LayoutContainerElement(box, le);
            }

            box._href = oldHref;
        }

        private void LayoutListElement(Box box, ListElement le)
        {
            Style style = le.GetStyle();

            DoMarginTop(box, style);

            Image image = SelectImage(style, StyleAttribute.LIST_STYLE_IMAGE);
            if (image != null)
            {
                LImage li = new LImage(le, image);
                li._marginRight = (short)ConvertToPX0(style, StyleAttribute.PADDING_LEFT, box._boxWidth);
                Layout(box, le, li, FloatPosition.LEFT, Display.BLOCK);

                int imageHeight = li._height;
                li._height = short.MaxValue;

                LayoutElements(box, le);

                li._height = imageHeight;

                box._objLeft.Remove(li);
                box.AdvanceToY(li.Bottom());
                box.ComputePadding();
            }
            else
            {
                LayoutElements(box, le);
                box.NextLine(false);
            }

            DoMarginBottom(box, style);
        }

        private void LayoutOrderedListElement(Box box, OrderedListElement ole)
        {
            Style style = ole.GetStyle();
            FontData fontData = CreateFontData(style);

            if (fontData == null)
            {
                return;
            }

            DoMarginTop(box, style);
            LElement anchor = box.AddAnchor(ole);

            int start = Math.Max(1, ole.GetStart());
            int count = ole.Count;
            OrderedListType type = style.Get(StyleAttribute.LIST_STYLE_TYPE, _styleClassResolver);

            String[] labels = new String[count];
            int maxLabelWidth = ConvertToPX0(style, StyleAttribute.PADDING_LEFT, box._boxWidth);
            for (int i = 0; i < count; i++)
            {
                labels[i] = type.Format(start + i) + ". ";
                int width = fontData._font.ComputeTextWidth(labels[i]);
                maxLabelWidth = Math.Max(maxLabelWidth, width);
            }

            for (int i = 0; i < count; i++)
            {
                String label = labels[i];
                Element li = ole.ElementAt(i);
                Style liStyle = li.GetStyle();
                DoMarginTop(box, liStyle);

                LText lt = new LText(ole, fontData, label, 0, label.Length, box._doCacheText);
                int labelWidth = lt._width;
                int labelHeight = lt._height;

                lt._width += ConvertToPX0(liStyle, StyleAttribute.PADDING_LEFT, box._boxWidth);
                Layout(box, ole, lt, FloatPosition.LEFT, Display.BLOCK);
                lt._x += Math.Max(0, maxLabelWidth - labelWidth);
                lt._height = short.MaxValue;

                LayoutElement(box, li);

                lt._height = labelHeight;

                box._objLeft.Remove(lt);
                box.AdvanceToY(lt.Bottom());
                box.ComputePadding();

                DoMarginBottom(box, liStyle);
            }

            anchor._height = box._curY - anchor._y;
            DoMarginBottom(box, style);
        }

        private Box LayoutBox(LClip clip, int continerWidth, int paddingLeft, int paddingRight, ContainerElement ce, String href, bool doCacheText)
        {
            Style style = ce.GetStyle();
            int paddingTop = ConvertToPX0(style, StyleAttribute.PADDING_TOP, continerWidth);
            int paddingBottom = ConvertToPX0(style, StyleAttribute.PADDING_BOTTOM, continerWidth);
            int marginBottom = ConvertToPX0(style, StyleAttribute.MARGIN_BOTTOM, continerWidth);

            Box box = new Box(this, clip, paddingLeft, paddingRight, paddingTop, doCacheText);
            box._href = href;
            box._style = style;
            LayoutElements(box, ce);
            box.Finish();

            int contentHeight = box._curY + paddingBottom;
            int boxHeight = Math.Max(contentHeight, ConvertToPX(style, StyleAttribute.HEIGHT, contentHeight, contentHeight));
            if (boxHeight > contentHeight)
            {
                int amount = 0;
                TextAreaModel.VAlignment vAlign = style.Get(StyleAttribute.VERTICAL_ALIGNMENT, _styleClassResolver);
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
                    clip.MoveContentY(amount);
                }
            }

            clip._height = boxHeight;
            clip._marginBottom = (short)Math.Max(marginBottom, box._marginBottomAbs - box._curY);
            return box;
        }

        private void LayoutBlockElement(Box box, ContainerElement be)
        {
            box.NextLine(false);

            Style style = be.GetStyle();
            FloatPosition floatPosition = style.Get(StyleAttribute.FLOAT_POSITION, _styleClassResolver);

            LImage bgImage = CreateBGImage(box, be);

            int marginTop = ConvertToPX0(style, StyleAttribute.MARGIN_TOP, box._boxWidth);
            int marginLeft = ConvertToPX0(style, StyleAttribute.MARGIN_LEFT, box._boxWidth);
            int marginRight = ConvertToPX0(style, StyleAttribute.MARGIN_RIGHT, box._boxWidth);

            int bgX = box.ComputeLeftPadding(marginLeft);
            int bgY = box.ComputeTopPadding(marginTop);
            int bgWidth;

            int remaining = Math.Max(0, box.ComputeRightPadding(marginRight) - bgX);
            int paddingLeft = ConvertToPX0(style, StyleAttribute.PADDING_LEFT, box._boxWidth);
            int paddingRight = ConvertToPX0(style, StyleAttribute.PADDING_RIGHT, box._boxWidth);

            if (floatPosition == FloatPosition.NONE)
            {
                bgWidth = ConvertToPX(style, StyleAttribute.WIDTH, remaining, remaining);
            }
            else
            {
                bgWidth = ConvertToPX(style, StyleAttribute.WIDTH, box._boxWidth, int.MinValue);
                if (bgWidth == int.MinValue)
                {
                    LClip dummy = new LClip(null);
                    dummy._width = Math.Max(0, box._lineWidth - paddingLeft - paddingRight);

                    Box dummyBox = LayoutBox(dummy, box._boxWidth, paddingLeft, paddingRight, be, null, false);
                    dummyBox.NextLine(false);

                    bgWidth = Math.Max(0, dummy._width - dummyBox._minRemainingWidth);
                }
            }

            bgWidth = Math.Max(0, bgWidth) + paddingLeft + paddingRight;

            if (floatPosition != FloatPosition.NONE)
            {
                box.AdvancePastFloaters(bgWidth, marginLeft, marginRight);

                bgX = box.ComputeLeftPadding(marginLeft);
                bgY = Math.Max(bgY, box._curY);
                remaining = Math.Max(0, box.ComputeRightPadding(marginRight) - bgX);
            }

            bgWidth = Math.Min(bgWidth, remaining);

            if (floatPosition == FloatPosition.RIGHT)
            {
                bgX = box.ComputeRightPadding(marginRight) - bgWidth;
            }

            LClip clip = new LClip(be);
            clip._x = bgX;
            clip._y = bgY;
            clip._width = bgWidth;
            clip._marginLeft = (short)marginLeft;
            clip._marginRight = (short)marginRight;
            clip._href = box._href;
            box._layout.Add(clip);

            Box clipBox = LayoutBox(clip, box._boxWidth, paddingLeft, paddingRight, be, box._href, box._doCacheText);

            // sync main box with layout
            box._lineStartIdx = box._layout.Count;

            if (floatPosition == FloatPosition.NONE)
            {
                box.AdvanceToY(bgY + clip._height);
                box.SetMarginBottom(clip._marginBottom);
                box.AccountMinRemaining(clipBox._minRemainingWidth);
            }
            else
            {
                if (floatPosition == FloatPosition.RIGHT)
                {
                    box._objRight.Add(clip);
                }
                else
                {
                    box._objLeft.Add(clip);
                }
                box.ComputePadding();
            }

            if (bgImage != null)
            {
                bgImage._x = bgX;
                bgImage._y = bgY;
                bgImage._width = bgWidth;
                bgImage._height = clip._height;
                bgImage._hoverSrc = clip;
            }
        }

        private void ComputeTableWidth(TableElement te, int maxTableWidth, int[] columnWidth, int[] columnSpacing, bool[] columnsWithFixedWidth)
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
                        int cellWidth = ConvertToPX(cellStyle, StyleAttribute.WIDTH, maxTableWidth, int.MinValue);
                        if (cellWidth == int.MinValue && (colspan > 1 || !hasFixedWidth))
                        {
                            int paddingLeft = Math.Max(cellPadding, ConvertToPX0(cellStyle, StyleAttribute.PADDING_LEFT, maxTableWidth));
                            int paddingRight = Math.Max(cellPadding, ConvertToPX0(cellStyle, StyleAttribute.PADDING_RIGHT, maxTableWidth));

                            LClip dummy = new LClip(null);
                            dummy._width = maxTableWidth;
                            Box dummyBox = LayoutBox(dummy, maxTableWidth, paddingLeft, paddingRight, cell, null, false);
                            dummyBox.Finish();

                            cellWidth = maxTableWidth - dummyBox._minRemainingWidth;
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
                            marginLeft = Math.Max(marginLeft, ConvertToPX(cellStyle, StyleAttribute.MARGIN_LEFT, maxTableWidth, 0));
                            marginRight = Math.Max(marginRight, ConvertToPX(cellStyle, StyleAttribute.MARGIN_LEFT, maxTableWidth, 0));
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

        private void LayoutTableElement(Box box, TableElement te)
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

            DoMarginTop(box, tableStyle);
            LElement anchor = box.AddAnchor(te);

            int left = box.ComputeLeftPadding(ConvertToPX0(tableStyle, StyleAttribute.MARGIN_LEFT, box._boxWidth));
            int right = box.ComputeRightPadding(ConvertToPX0(tableStyle, StyleAttribute.MARGIN_RIGHT, box._boxWidth));
            int maxTableWidth = Math.Max(0, right - left);
            int tableWidth = Math.Min(maxTableWidth, ConvertToPX(tableStyle, StyleAttribute.WIDTH, box._boxWidth, int.MinValue));
            bool autoTableWidth = tableWidth == int.MinValue;

            if (tableWidth <= 0)
            {
                tableWidth = maxTableWidth;
            }

            int[] columnWidth = new int[numColumns];
            int[] columnSpacing = new int[numColumns + 1];
            bool[] columnsWithFixedWidth = new bool[numColumns];

            columnSpacing[0] = Math.Max(cellSpacing, ConvertToPX0(tableStyle, StyleAttribute.PADDING_LEFT, box._boxWidth));

            ComputeTableWidth(te, tableWidth, columnWidth, columnSpacing, columnsWithFixedWidth);

            columnSpacing[numColumns] = Math.Max(columnSpacing[numColumns],
                    ConvertToPX0(tableStyle, StyleAttribute.PADDING_RIGHT, box._boxWidth));

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

            LImage tableBGImage = CreateBGImage(box, te);

            box._textAlignment = TextAreaModel.HAlignment.LEFT;
            box._curY += Math.Max(cellSpacing, ConvertToPX0(tableStyle, StyleAttribute.PADDING_TOP, box._boxWidth));

            LImage[] bgImages = new LImage[numColumns];

            for (int row = 0; row < numRows; row++)
            {
                if (row > 0)
                {
                    box._curY += cellSpacing;
                }

                LImage rowBGImage = null;
                Style rowStyle = te.GetRowStyle(row);
                if (rowStyle != null)
                {
                    int marginTop = ConvertToPX0(rowStyle, StyleAttribute.MARGIN_TOP, tableWidth);
                    box._curY = box.ComputeTopPadding(marginTop);

                    Image image = SelectImage(rowStyle, StyleAttribute.BACKGROUND_IMAGE);
                    if (image == null)
                    {
                        image = CreateBackgroundColor(rowStyle);
                    }
                    if (image != null)
                    {
                        rowBGImage = new LImage(te, image);
                        rowBGImage._y = box._curY;
                        rowBGImage._x = left;
                        rowBGImage._width = tableWidth;
                        box._clip._bgImages.Add(rowBGImage);
                    }

                    box._curY += ConvertToPX0(rowStyle, StyleAttribute.PADDING_TOP, tableWidth);
                    box._minLineHeight = ConvertToPX0(rowStyle, StyleAttribute.HEIGHT, tableWidth);
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

                        int paddingLeft = Math.Max(cellPadding, ConvertToPX0(cellStyle, StyleAttribute.PADDING_LEFT, tableWidth));
                        int paddingRight = Math.Max(cellPadding, ConvertToPX0(cellStyle, StyleAttribute.PADDING_RIGHT, tableWidth));

                        LClip clip = new LClip(cell);
                        LImage bgImage = CreateBGImage(box, cell);
                        if (bgImage != null)
                        {
                            bgImage._x = x;
                            bgImage._width = width;
                            bgImage._hoverSrc = clip;
                            bgImages[col] = bgImage;
                        }

                        clip._x = x;
                        clip._y = box._curY;
                        clip._width = width;
                        clip._marginTop = (short)ConvertToPX0(cellStyle, StyleAttribute.MARGIN_TOP, tableWidth);
                        box._layout.Add(clip);

                        LayoutBox(clip, tableWidth, paddingLeft, paddingRight, cell, null, box._doCacheText);

                        col += Math.Max(0, cell.GetColspan() - 1);
                    }
                    x += width;
                }
                box.NextLine(false);

                for (int col = 0; col < numColumns; col++)
                {
                    LImage bgImage = bgImages[col];
                    if (bgImage != null)
                    {
                        bgImage._height = box._curY - bgImage._y;
                        bgImages[col] = null;   // clear for next row
                    }
                }

                if (rowStyle != null)
                {
                    box._curY += ConvertToPX0(rowStyle, StyleAttribute.PADDING_BOTTOM, tableWidth);

                    if (rowBGImage != null)
                    {
                        rowBGImage._height = box._curY - rowBGImage._y;
                    }

                    DoMarginBottom(box, rowStyle);
                }
            }

            box._curY += Math.Max(cellSpacing, ConvertToPX0(tableStyle, StyleAttribute.PADDING_BOTTOM, box._boxWidth));
            box.CheckFloaters();
            box.AccountMinRemaining(Math.Max(0, box._lineWidth - tableWidth));

            if (tableBGImage != null)
            {
                tableBGImage._height = box._curY - tableBGImage._y;
                tableBGImage._x = left;
                tableBGImage._width = tableWidth;
            }

            // anchor.y already set (by addAnchor)
            anchor._x = left;
            anchor._width = tableWidth;
            anchor._height = box._curY - anchor._y;

            DoMarginBottom(box, tableStyle);
        }

        private LImage CreateBGImage(Box box, Element element)
        {
            Style style = element.GetStyle();
            Image image = SelectImage(style, StyleAttribute.BACKGROUND_IMAGE);
            if (image == null)
            {
                image = CreateBackgroundColor(style);
            }
            if (image != null)
            {
                LImage bgImage = new LImage(element, image);
                bgImage._y = box._curY;
                box._clip._bgImages.Add(bgImage);
                return bgImage;
            }
            return null;
        }

        private Image CreateBackgroundColor(Style style)
        {
            Color color = style.Get(StyleAttribute.BACKGROUND_COLOR, _styleClassResolver);
            if (color.Alpha != 0)
            {
                Image white = SelectImage("white");
                if (white != null)
                {
                    Image image = white.CreateTintedVersion(color);
                    Color colorHover = style.Get(StyleAttribute.BACKGROUND_COLOR_HOVER, _styleClassResolver);
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

        static bool IsSkip(char ch)
        {
            return CharUtil.IsWhitespace(ch);
        }

        static bool IsPunctuation(char ch)
        {
            return ":;,.-!?".IndexOf(ch) >= 0;
        }

        static bool IsBreak(char ch)
        {
            return CharUtil.IsWhitespace(ch) || IsPunctuation(ch) || (ch == 0x3001) || (ch == 0x3002);
        }

        internal class Box
        {
            internal LClip _clip;
            internal List<LElement> _layout;
            internal List<LElement> _objLeft = new List<LElement>();
            internal List<LElement> _objRight = new List<LElement>();
            internal StringBuilder _lineInfo = new StringBuilder();
            internal int _boxLeft;
            internal int _boxWidth;
            internal int _boxMarginOffsetLeft;
            internal int _boxMarginOffsetRight;
            internal bool _doCacheText;
            internal int _curY;
            internal int _curX;
            internal int _lineStartIdx;
            internal int _lastProcessedAnchorIdx;
            internal int _marginTop;
            internal int _marginLeft;
            internal int _marginRight;
            internal int _marginBottomAbs;
            internal int _marginBottomNext;
            internal int _lineStartX;
            internal int _lineWidth;
            internal int _fontLineHeight;
            internal int _minLineHeight;
            internal int _lastLineEnd;
            internal int _lastLineBottom;
            internal int _minRemainingWidth;
            internal bool _inParagraph;
            internal bool _wasAutoBreak;
            internal bool _wasPreformatted;
            internal TextAreaModel.HAlignment _textAlignment;
            internal String _href;
            internal TextAreaModel.Style _style;
            internal TextArea _textAreaW;

            internal Box(TextArea textAreaW, LClip clip, int paddingLeft, int paddingRight, int paddingTop, bool doCacheText)
            {
                this._textAreaW = textAreaW;
                this._clip = clip;
                this._layout = clip._layout;
                this._boxLeft = paddingLeft;
                this._boxWidth = Math.Max(0, clip._width - paddingLeft - paddingRight);
                this._boxMarginOffsetLeft = paddingLeft;
                this._boxMarginOffsetRight = paddingRight;
                this._doCacheText = doCacheText;
                this._curX = paddingLeft;
                this._curY = paddingTop;
                this._lineStartX = paddingLeft;
                this._lineWidth = _boxWidth;
                this._minRemainingWidth = _boxWidth;
                this._textAlignment = TextAreaModel.HAlignment.LEFT;
                System.Diagnostics.Debug.Assert(_layout.Count == 0);
            }

            internal void ComputePadding()
            {
                int left = ComputeLeftPadding(_marginLeft);
                int right = ComputeRightPadding(_marginRight);

                _lineStartX = left;
                _lineWidth = Math.Max(0, right - left);

                if (IsAtStartOfLine())
                {
                    _curX = _lineStartX;
                }

                AccountMinRemaining(GetRemaining());
            }

            internal int ComputeLeftPadding(int marginLeft)
            {
                int left = _boxLeft + Math.Max(0, marginLeft - _boxMarginOffsetLeft);

                for (int i = 0, n = _objLeft.Count; i < n; i++)
                {
                    LElement e = _objLeft[i];
                    left = Math.Max(left, e._x + e._width + Math.Max(e._marginRight, marginLeft));
                }

                return left;
            }

            internal int ComputeRightPadding(int marginRight)
            {
                int right = _boxLeft + _boxWidth - Math.Max(0, marginRight - _boxMarginOffsetRight);

                for (int i = 0, n = _objRight.Count; i < n; i++)
                {
                    LElement e = _objRight[i];
                    right = Math.Min(right, e._x - Math.Max(e._marginLeft, marginRight));
                }

                return right;
            }

            internal int ComputePaddingWidth(int marginLeft, int marginRight)
            {
                return Math.Max(0, ComputeRightPadding(marginRight) - ComputeLeftPadding(marginLeft));
            }

            internal int ComputeTopPadding(int marginTop)
            {
                return Math.Max(_marginBottomAbs, _curY + marginTop);
            }

            internal void SetMarginBottom(int marginBottom)
            {
                if (IsAtStartOfLine())
                {
                    _marginBottomAbs = Math.Max(_marginBottomAbs, _curY + marginBottom);
                }
                else
                {
                    _marginBottomNext = Math.Max(_marginBottomNext, marginBottom);
                }
            }

            internal int GetRemaining()
            {
                return Math.Max(0, _lineWidth - _curX + _lineStartX);
            }

            internal void AccountMinRemaining(int remaining)
            {
                _minRemainingWidth = Math.Min(_minRemainingWidth, remaining);
            }

            internal int GetXAndAdvance(int amount)
            {
                int x = _curX;
                _curX = x + amount;
                return x;
            }

            internal bool IsAtStartOfLine()
            {
                return _lineStartIdx == _layout.Count;
            }

            internal bool PrevOnLineEndsNotWithSpace()
            {
                int layoutSize = _layout.Count;
                if (_lineStartIdx < layoutSize)
                {
                    LElement le = _layout[layoutSize - 1];
                    if (le is LText)
                    {
                        LText lt = (LText)le;
                        return !IsSkip(lt._text[lt._end - 1]);
                    }
                    return true;
                }
                return false;
            }

            internal void CheckFloaters()
            {
                RemoveObjFromList(_objLeft);
                RemoveObjFromList(_objRight);
                ComputePadding();
                // curX is set by computePadding()
            }

            internal void ClearFloater(Clear clear)
            {
                if (clear != Clear.NONE)
                {
                    int targetY = -1;
                    if (clear == Clear.LEFT || clear == Clear.BOTH)
                    {
                        for (int i = 0, n = _objLeft.Count; i < n; ++i)
                        {
                            LElement le = _objLeft[i];
                            if (le._height != short.MaxValue)
                            {  // special case for list elements
                                targetY = Math.Max(targetY, le._y + le._height);
                            }
                        }
                    }
                    if (clear == Clear.RIGHT || clear == Clear.BOTH)
                    {
                        for (int i = 0, n = _objRight.Count; i < n; ++i)
                        {
                            LElement le = _objRight[i];
                            targetY = Math.Max(targetY, le._y + le._height);
                        }
                    }
                    if (targetY >= 0)
                    {
                        AdvanceToY(targetY);
                    }
                }
            }

            internal void AdvanceToY(int targetY)
            {
                NextLine(false);
                if (targetY > _curY)
                {
                    _curY = targetY;
                    CheckFloaters();
                }
            }

            internal void AdvancePastFloaters(int requiredWidth, int marginLeft, int marginRight)
            {
                if (ComputePaddingWidth(marginLeft, marginRight) < requiredWidth)
                {
                    NextLine(false);
                    do
                    {
                        int targetY = int.MaxValue;
                        if (_objLeft.Count != 0)
                        {
                            LElement le = _objLeft[_objLeft.Count - 1];
                            if (le._height != short.MaxValue)
                            {  // special case for list elements
                                targetY = Math.Min(targetY, le.Bottom());
                            }
                        }
                        if (_objRight.Count != 0)
                        {
                            LElement le = _objRight[_objRight.Count - 1];
                            targetY = Math.Min(targetY, le.Bottom());
                        }
                        if (targetY == int.MaxValue || targetY < _curY)
                        {
                            return;
                        }
                        _curY = targetY;
                        CheckFloaters();
                    } while (ComputePaddingWidth(marginLeft, marginRight) < requiredWidth);
                }
            }

            internal bool NextLine(bool force)
            {
                if (IsAtStartOfLine() && (_wasAutoBreak || !force))
                {
                    _wasAutoBreak = !force;
                    return false;
                }

                AccountMinRemaining(GetRemaining());

                int targetY = _curY;
                int lineHeight = _minLineHeight;

                if (IsAtStartOfLine())
                {
                    lineHeight = Math.Max(lineHeight, _fontLineHeight);
                }
                else
                {
                    for (int idx = _lineStartIdx; idx < _layout.Count; idx++)
                    {
                        LElement le = _layout[idx];
                        lineHeight = Math.Max(lineHeight, le._height);
                    }

                    LElement lastElement = _layout[_layout.Count - 1];
                    int remaining = (_lineStartX + _lineWidth) - (lastElement._x + lastElement._width);

                    switch (_textAlignment)
                    {
                        case TextAreaModel.HAlignment.RIGHT:
                            {
                                for (int idx = _lineStartIdx; idx < _layout.Count; idx++)
                                {
                                    LElement le = _layout[idx];
                                    le._x += remaining;
                                }
                                break;
                            }
                        case TextAreaModel.HAlignment.CENTER:
                            {
                                int offset = remaining / 2;
                                for (int idx = _lineStartIdx; idx < _layout.Count; idx++)
                                {
                                    LElement le = _layout[idx];
                                    le._x += offset;
                                }
                                break;
                            }
                        case TextAreaModel.HAlignment.JUSTIFY:
                            if (remaining < _lineWidth / 4)
                            {
                                int num = _layout.Count - _lineStartIdx;
                                for (int i = 1; i < num; i++)
                                {
                                    LElement le = _layout[_lineStartIdx + i];
                                    int offset = remaining * i / (num - 1);
                                    le._x += offset;
                                }
                            }
                            break;
                    }

                    for (int idx = _lineStartIdx; idx < _layout.Count; idx++)
                    {
                        LElement le = _layout[idx];
                        switch (le._element.GetStyle().Get(StyleAttribute.VERTICAL_ALIGNMENT, _textAreaW._styleClassResolver))
                        {
                            case TextAreaModel.VAlignment.BOTTOM:
                                le._y = lineHeight - le._height;
                                break;
                            case TextAreaModel.VAlignment.TOP:
                                le._y = 0;
                                break;
                            case TextAreaModel.VAlignment.MIDDLE:
                                le._y = (lineHeight - le._height) / 2;
                                break;
                            case TextAreaModel.VAlignment.FILL:
                                le._y = 0;
                                le._height = lineHeight;
                                break;
                        }
                        targetY = Math.Max(targetY, ComputeTopPadding(le._marginTop - le._y));
                        _marginBottomNext = Math.Max(_marginBottomNext, le.Bottom() - lineHeight);
                    }

                    for (int idx = _lineStartIdx; idx < _layout.Count; idx++)
                    {
                        LElement le = _layout[idx];
                        le._y += targetY;
                    }
                }

                ProcessAnchors(targetY, lineHeight);

                _minLineHeight = 0;
                _lineStartIdx = _layout.Count;
                _wasAutoBreak = !force;
                _curY = targetY + lineHeight;
                _marginBottomAbs = Math.Max(_marginBottomAbs, _curY + _marginBottomNext);
                _marginBottomNext = 0;
                _marginTop = 0;
                CheckFloaters();
                // curX is set by computePadding() inside checkFloaters()
                return true;
            }

            internal void Finish()
            {
                NextLine(false);
                ClearFloater(Clear.BOTH);
                ProcessAnchors(_curY, 0);
                int lineInfoLength = _lineInfo.Length;
                _clip._lineInfo = new char[lineInfoLength];
                _clip._lineInfo = _lineInfo.ToString(0, lineInfoLength).ToCharArray();
            }

            internal int ComputeNextTabStop(Style style, Font font)
            {
                int em = font.MWidth;
                int tabSize = style.Get(StyleAttribute.TAB_SIZE, _textAreaW._styleClassResolver);
                if (tabSize <= 0 || em <= 0)
                {
                    // replace with single space when tabs are disabled
                    return _curX + font.SpaceWidth;
                }
                int tabSizePX = Math.Min(tabSize, short.MaxValue / em) * em;
                int x = _curX - _lineStartX + font.SpaceWidth;
                return _curX + tabSizePX - (x % tabSizePX);
            }

            private void RemoveObjFromList(List<LElement> list)
            {
                for (int i = list.Count; i-- > 0;)
                {
                    LElement e = list[i];
                    if (e.Bottom() <= _curY)
                    {
                        // can't update marginBottomAbs here - results in layout error for text
                        list.RemoveAt(i);
                    }
                }
            }

            internal void SetupTextParams(Style style, Font font, bool isParagraphStart)
            {
                if (font != null)
                {
                    _fontLineHeight = font.LineHeight;
                }
                else
                {
                    _fontLineHeight = 0;
                }

                if (isParagraphStart)
                {
                    NextLine(false);
                    _inParagraph = true;
                }

                if (isParagraphStart || (!_inParagraph && IsAtStartOfLine()))
                {
                    _marginLeft = _textAreaW.ConvertToPX0(style, StyleAttribute.MARGIN_LEFT, _boxWidth);
                    _marginRight = _textAreaW.ConvertToPX0(style, StyleAttribute.MARGIN_RIGHT, _boxWidth);
                    _textAlignment = style.Get(StyleAttribute.HORIZONTAL_ALIGNMENT, _textAreaW._styleClassResolver);
                    ComputePadding();
                    _curX = Math.Max(0, _lineStartX + _textAreaW.ConvertToPX(style, StyleAttribute.TEXT_INDENT, _boxWidth, 0));
                }

                _marginTop = _textAreaW.ConvertToPX0(style, StyleAttribute.MARGIN_TOP, _boxWidth);
            }

            internal LElement AddAnchor(Element e)
            {
                LElement le = new LElement(e);
                le._y = _curY;
                le._x = _boxLeft;
                le._width = _boxWidth;
                _clip._anchors.Add(le);
                return le;
            }

            private void ProcessAnchors(int y, int height)
            {
                while (_lastProcessedAnchorIdx < _clip._anchors.Count)
                {
                    LElement le = _clip._anchors[_lastProcessedAnchorIdx++];
                    if (le._height == 0)
                    {
                        le._y = y;
                        le._height = height;
                    }
                }
                if (_lineStartIdx > _lastLineEnd)
                {
                    _lineInfo.Append((char)0).Append((char)(_lineStartIdx - _lastLineEnd));
                }
                if (y > _lastLineBottom)
                {
                    _lineInfo.Append((char)y).Append((char)0);
                }
                _lastLineBottom = y + height;
                _lineInfo.Append((char)_lastLineBottom).Append((char)(_layout.Count - _lineStartIdx));
                _lastLineEnd = _layout.Count;
            }
        }

        public class RenderInfo
        {
            internal int _offsetX;
            internal int _offsetY;
            internal Renderer.Renderer _renderer;
            internal AnimationState _asNormal;
            internal AnimationState _asHover;

            public RenderInfo(AnimationState parent)
            {
                _asNormal = new AnimationState(parent);
                _asNormal.SetAnimationState(STATE_HOVER, false);
                _asHover = new AnimationState(parent);
                _asHover.SetAnimationState(STATE_HOVER, true);
            }

            internal AnimationState GetAnimationState(bool isHover)
            {
                return isHover ? _asHover : _asNormal;
            }
        }

        public class LElement
        {
            internal Element _element;
            internal int _x;
            internal int _y;
            internal int _width;
            internal int _height;
            internal short _marginTop;
            internal short _marginLeft;
            internal short _marginRight;
            internal short _marginBottom;
            internal String _href;
            internal bool _isHover;
            internal bool _inheritHover;

            public LElement(Element element)
            {
                this._element = element;
            }

            internal virtual void AdjustWidget(int offX, int offY) { }
            internal virtual void CollectBGImages(int offX, int offY, List<LImage> allBGImages) { }
            internal virtual void Draw(RenderInfo ri) { }
            internal virtual void Destroy() { }

            internal virtual bool IsInside(int x, int y)
            {
                return (x >= this._x) && (x < this._x + this._width) &&
                        (y >= this._y) && (y < this._y + this._height);
            }

            internal virtual LElement Find(int x, int y)
            {
                return this;
            }

            internal virtual LElement Find(Element element, int[] offset)
            {
                if (this._element == element)
                {
                    return this;
                }
                return null;
            }

            internal virtual bool SetHover(LElement le)
            {
                _isHover = (this == le) || (le != null && _element == le._element);
                return _isHover;
            }

            internal virtual int Bottom()
            {
                return _y + _height + _marginBottom;
            }
        }

        public class FontData
        {
            internal Font _font;
            Color _color;
            Color _colorHover;

            internal FontData(Font font, Color color, Color colorHover)
            {
                if (colorHover == null)
                {
                    colorHover = color;
                }
                this._font = font;
                this._color = MaskWhite(color);
                this._colorHover = MaskWhite(colorHover);
            }

            public Color GetColor(bool isHover)
            {
                return isHover ? _colorHover : _color;
            }

            private static Color MaskWhite(Color c)
            {
                return Color.WHITE.Equals(c) ? null : c;
            }
        }

        public class LText : LElement
        {
            internal FontData _fontData;
            internal String _text;
            internal int _start;
            internal int _end;
            internal FontCache _cache;

            internal LText(Element element, FontData fontData, String text, int start, int end, bool doCache) : base(element)
            {
                Font font = fontData._font;
                this._fontData = fontData;
                this._text = text;
                this._start = start;
                this._end = end;
                Color c = fontData.GetColor(_isHover);
                this._cache = doCache ? font.CacheText(null, text, start, end) : null;
                this._height = font.LineHeight;

                if (_cache != null)
                {
                    this._width = _cache.Width;
                }
                else
                {
                    this._width = font.ComputeTextWidth(text, start, end);
                }
            }

            internal override void Draw(RenderInfo ri)
            {
                Color c = _fontData.GetColor(_isHover);
                if (c != null)
                {
                    DrawTextWithColor(ri, c);
                }
                else
                {
                    DrawText(Color.BLACK, ri);
                }
            }

            private void DrawTextWithColor(RenderInfo ri, Color c)
            {
                DrawText(c, ri);
            }

            private void DrawText(Color c, RenderInfo ri)
            {
                AnimationState animationState = ri.GetAnimationState(_isHover);
                if (_cache != null)
                {
                    _cache.Draw(animationState, _x + ri._offsetX, _y + ri._offsetY);
                }
                else
                {
                    _fontData._font.DrawText(animationState, _x + ri._offsetX, _y + ri._offsetY, _text, _start, _end);
                }
            }

            internal override void Destroy()
            {
                if (_cache != null)
                {
                    _cache.Dispose();
                    _cache = null;
                }
            }
        }

        public class LWidget : LElement
        {
            Widget _widget;

            internal LWidget(Element element, Widget widget) : base(element)
            {
                this._widget = widget;
            }

            internal override void AdjustWidget(int offX, int offY)
            {
                _widget.SetPosition(_x + offX, _y + offY);
                _widget.SetSize(_width, _height);
            }
        }

        public class LImage : LElement
        {
            internal Image _img;
            internal LElement _hoverSrc;

            internal LImage(Element element, Image img) : base(element)
            {
                this._img = img;
                this._width = img.Width;
                this._height = img.Height;
                this._hoverSrc = this;
            }

            //@Override
            internal override void Draw(RenderInfo ri)
            {
                _img.Draw(ri.GetAnimationState(_hoverSrc._isHover),
                        _x + ri._offsetX, _y + ri._offsetY, _width, _height);
            }
        }

        public class LClip : LElement
        {
            internal List<LElement> _layout;
            internal List<LImage> _bgImages;
            internal List<LElement> _anchors;
            internal char[] _lineInfo;

            public LClip(Element element) : base(element)
            {
                this._layout = new List<LElement>();
                this._bgImages = new List<LImage>();
                this._anchors = new List<LElement>();
                this._lineInfo = EMPTY_CHAR_ARRAY;
            }

            internal override void Draw(RenderInfo ri)
            {
                ri._offsetX += _x;
                ri._offsetY += _y;
                ri._renderer.ClipEnter(ri._offsetX, ri._offsetY, _width, _height);
                try
                {
                    if (!ri._renderer.ClipIsEmpty())
                    {
                        List<LElement> ll = _layout;
                        for (int i = 0, n = ll.Count; i < n; i++)
                        {
                            ll[i].Draw(ri);
                        }
                    }
                }
                finally
                {
                    ri._renderer.ClipLeave();
                    ri._offsetX -= _x;
                    ri._offsetY -= _y;
                }
            }

            internal override void AdjustWidget(int offX, int offY)
            {
                offX += _x;
                offY += _y;
                for (int i = 0, n = _layout.Count; i < n; i++)
                {
                    _layout[i].AdjustWidget(offX, offY);
                }
            }

            internal override void CollectBGImages(int offX, int offY, List<LImage> allBGImages)
            {
                offX += _x;
                offY += _y;
                for (int i = 0, n = _bgImages.Count; i < n; i++)
                {
                    LImage img = _bgImages[i];
                    img._x += offX;
                    img._y += offY;
                    allBGImages.Add(img);
                }
                for (int i = 0, n = _layout.Count; i < n; i++)
                {
                    _layout[i].CollectBGImages(offX, offY, allBGImages);
                }
            }

            internal override void Destroy()
            {
                for (int i = 0, n = _layout.Count; i < n; i++)
                {
                    _layout[i].Destroy();
                }
                _layout.Clear();
                _bgImages.Clear();
                _lineInfo = EMPTY_CHAR_ARRAY;
            }

            internal override LElement Find(int x, int y)
            {
                x -= this._x;
                y -= this._y;
                int lineTop = 0;
                int layoutIdx = 0;
                for (int lineIdx = 0; lineIdx < _lineInfo.Length && y >= lineTop;)
                {
                    int lineBottom = _lineInfo[lineIdx++];
                    int layoutCount = _lineInfo[lineIdx++];
                    if (layoutCount > 0)
                    {
                        if (lineBottom == 0 || y < lineBottom)
                        {
                            for (int i = 0; i < layoutCount; i++)
                            {
                                LElement le = _layout[layoutIdx + i];
                                if (le.IsInside(x, y))
                                {
                                    return le.Find(x, y);
                                }
                            }
                            if (lineBottom > 0 && x >= _layout[layoutIdx]._x)
                            {
                                LElement prev = null;
                                for (int i = 0; i < layoutCount; i++)
                                {
                                    LElement le = _layout[layoutIdx + i];
                                    if (le._x >= x && (prev == null || prev._element == le._element))
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

            override internal LElement Find(Element element, int[] offset)
            {
                if (this._element == element)
                {
                    return this;
                }
                LElement match = Find(_layout, element, offset);
                if (match == null)
                {
                    match = Find(_anchors, element, offset);
                }
                return match;
            }

            private LElement Find(List<LElement> l, Element e, int[] offset)
            {
                for (int i = 0, n = l.Count; i < n; i++)
                {
                    LElement match = l[i].Find(e, offset);
                    if (match != null)
                    {
                        if (offset != null)
                        {
                            offset[0] += this._x;
                            offset[1] += this._y;
                        }
                        return match;
                    }
                }
                return null;
            }

            override internal bool SetHover(LElement le)
            {
                bool childHover = false;
                for (int i = 0, n = _layout.Count; i < n; i++)
                {
                    childHover |= _layout[i].SetHover(le);
                }
                if (childHover)
                {
                    _isHover = true;
                }
                else
                {
                    base.SetHover(le);
                }
                for (int i = 0, n = _layout.Count; i < n; i++)
                {
                    LElement child = _layout[i];
                    if (child._inheritHover)
                    {
                        child._isHover = _isHover;
                    }
                }
                return _isHover;
            }

            internal void MoveContentY(int amount)
            {
                for (int i = 0, n = _layout.Count; i < n; i++)
                {
                    _layout[i]._y += amount;
                }
                if (_lineInfo.Length > 0)
                {
                    if (_lineInfo[1] == 0)
                    {
                        _lineInfo[0] += (char)amount;
                    }
                    else
                    {
                        int n = _lineInfo.Length;
                        char[] tmpLineInfo = new char[n + 2];
                        tmpLineInfo[0] = (char)amount;
                        for (int i = 0; i < n; i += 2)
                        {
                            int lineBottom = _lineInfo[i];
                            if (lineBottom > 0)
                            {
                                lineBottom += amount;
                            }
                            tmpLineInfo[i + 2] = (char)lineBottom;
                            tmpLineInfo[i + 3] = _lineInfo[i + 1];
                        }
                        _lineInfo = tmpLineInfo;
                    }
                }
            }
        }
    }
}
