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
using System.Linq;
using System.Text;
using XNATWL.Model;
using XNATWL.Renderer;
using XNATWL.Utils;

namespace XNATWL
{
    public class ColorSelector : DialogLayout
    {
        private static String[] RGBA_NAMES = { "Red", "Green", "Blue", "Alpha" };
        private static String[] RGBA_PREFIX = { "R: ", "G: ", "B: ", "A: " };

        /*ByteBuffer imgData;
        IntBuffer imgDataInt;*/
        Microsoft.Xna.Framework.Color[] _imgData;

        ColorSpace _colorSpace;
        float[] _colorValues;
        ColorValueModel[] _colorValueModels;
        private bool _useColorArea2D = true;
        private bool _showPreview = false;
        private bool _useLabels = true;
        private bool _showHexEditField = false;
        private bool _showNativeAdjuster = true;
        private bool _showRGBAdjuster = true;
        private bool _showAlphaAdjuster = true;
        private ColorModel _model;
        private bool _inModelSetValue;
        int _currentColor;
        private ARGBModel[] _argbModels;
        EditField _hexColorEditField;
        private TintAnimator _previewTintAnimator;
        private bool _recreateLayout;

        public event EventHandler<ColorSelectorColorChangedEventArgs> ColorChanged;

        public ColorSelector(ColorSpace colorSpace)
        {
            // allocate enough space for 2D color areas
            this._imgData = new Microsoft.Xna.Framework.Color[IMAGE_SIZE * IMAGE_SIZE];

            _currentColor = Color.WHITE.ARGB;

            SetColorSpace(colorSpace);
        }

        public ColorSpace GetColorSpace()
        {
            return _colorSpace;
        }

        public void SetColorSpace(ColorSpace colorModel)
        {
            if (colorModel == null)
            {
                throw new ArgumentNullException("colorModel");
            }
            if (this._colorSpace != colorModel)
            {
                bool hasColor = this._colorSpace != null;

                this._colorSpace = colorModel;
                this._colorValues = new float[colorModel.Components];

                if (hasColor)
                {
                    SetColorInt((int) _currentColor);
                }
                else
                {
                    SetDefaultColor();
                }

                _recreateLayout = true;
                InvalidateLayout();
            }
        }

        public ColorModel GetModel()
        {
            return _model;
        }

        public void SetModel(ColorModel model)
        {
            if (this._model != model)
            {
                RemoveModelCallback();
                this._model = model;
                if (model != null)
                {
                    AddModelCallback();
                    ModelValueChanged();
                }
            }
        }

        public Color GetColor()
        {
            return new Color(_currentColor);
        }

        public void SetColor(Color color)
        {
            SetColorInt(color.ARGB);
            UpdateModel();
        }

        public void SetDefaultColor()
        {
            _currentColor = Color.WHITE.ARGB;
            for (int i = 0; i < _colorSpace.Components; i++)
            {
                float oldValue = _colorValues[i];
                _colorValues[i] = _colorSpace.ComponentDefaultValueOf(i);
                //colorValueModels[i].fireCallback(oldValue, colorValues[i]);

                if (_colorValueModels != null && _colorValueModels.Length > i && _colorValueModels[i] != null)
                {
                    _colorValueModels[i].FireCallback(oldValue, _colorValues[i]);
                }
            }

            FireColorChanged();
        }

        public bool IsUseColorArea2D()
        {
            return _useColorArea2D;
        }

        /**
         * Use 2D color areas.
         *
         * Color component 0 is the X axis and component 1 the Y axis of
         * the first 2D area, etc. If the number of color components is
         * odd then the last component is displayed with a 1D.
         *
         * If disabled all components are displayed using 1D color areas.
         *
         * @param useColorArea2D true if 2D areas should be used
         */
        public void SetUseColorArea2D(bool useColorArea2D)
        {
            if (this._useColorArea2D != useColorArea2D)
            {
                this._useColorArea2D = useColorArea2D;
                _recreateLayout = true;
                InvalidateLayout();
            }
        }

        public bool IsShowPreview()
        {
            return _showPreview;
        }

        /**
         * Show the currently selected color in a preview widget.
         * Default is false.
         *
         * @param showPreview true if the preview widget should be displayed
         */
        public void SetShowPreview(bool showPreview)
        {
            if (this._showPreview != showPreview)
            {
                this._showPreview = showPreview;
                _recreateLayout = true;
                InvalidateLayout();
            }
        }

        public bool IsShowHexEditField()
        {
            return _showHexEditField;
        }

        /**
         * Includes an edit field which allows to edit the color hex values in ARGB.
         * Default is false.
         *
         * @param showHexEditField true if the edit field should be shown
         */
        public void SetShowHexEditField(bool showHexEditField)
        {
            if (this._showHexEditField != showHexEditField)
            {
                this._showHexEditField = showHexEditField;
                _recreateLayout = true;
                InvalidateLayout();
            }
        }

        public bool IsShowAlphaAdjuster()
        {
            return _showAlphaAdjuster;
        }

        public void SetShowAlphaAdjuster(bool showAlphaAdjuster)
        {
            if (this._showAlphaAdjuster != showAlphaAdjuster)
            {
                this._showAlphaAdjuster = showAlphaAdjuster;
                _recreateLayout = true;
                InvalidateLayout();
            }
        }

        public bool IsShowNativeAdjuster()
        {
            return _showNativeAdjuster;
        }

        /**
         * Includes adjuster for each clor component of the specified color space.
         * Default is true.
         *
         * @param showNativeAdjuster true if the native adjuster should be displayed
         */
        public void SetShowNativeAdjuster(bool showNativeAdjuster)
        {
            if (this._showNativeAdjuster != showNativeAdjuster)
            {
                this._showNativeAdjuster = showNativeAdjuster;
                _recreateLayout = true;
                InvalidateLayout();
            }
        }

        public bool IsShowRGBAdjuster()
        {
            return _showRGBAdjuster;
        }

        public void SetShowRGBAdjuster(bool showRGBAdjuster)
        {
            if (this._showRGBAdjuster != showRGBAdjuster)
            {
                this._showRGBAdjuster = showRGBAdjuster;
                _recreateLayout = true;
                InvalidateLayout();
            }
        }

        public bool IsUseLabels()
        {
            return _useLabels;
        }

        /**
         * Show labels infront of the value adjusters for the color components.
         * Default is true.
         *
         * @param useLabels true if labels should be displayed
         */
        public void SetUseLabels(bool useLabels)
        {
            if (this._useLabels != useLabels)
            {
                this._useLabels = useLabels;
                _recreateLayout = true;
                InvalidateLayout();
            }
        }

        /*public void addCallback(Runnable cb)
        {
            callbacks = CallbackSupport.addCallbackToList(callbacks, cb, typeof(Runnable));
        }

        public void removeCallback(Runnable cb)
        {
            callbacks = CallbackSupport.removeCallbackFromList(callbacks, cb);
        }*/

        protected void UpdateModel()
        {
            if (_model != null)
            {
                _inModelSetValue = true;
                try
                {
                    _model.Value = GetColor();
                }
                finally
                {
                    _inModelSetValue = false;
                }
            }
        }

        protected void FireColorChanged()
        {
            int oldV = (int)_currentColor;
            _currentColor = ((_currentColor & (0xFF << 24)) | _colorSpace.RGB(_colorValues));
            if (this.ColorChanged != null)
            {
                this.ColorChanged.Invoke(this, new ColorSelectorColorChangedEventArgs());
            }
            UpdateModel();
            if (_argbModels != null)
            {
                foreach (ARGBModel m in _argbModels)
                {
                    m.FireCallback(oldV, (int)_currentColor);
                }
            }
            if (_previewTintAnimator != null)
            {
                _previewTintAnimator.SetColor(GetColor());
            }
            UpdateHexEditField();
        }

        protected void SetColorInt(int argb)
        {
            _currentColor = argb;
            float[] oldValues = _colorValues;
            _colorValues = _colorSpace.FromRGB(argb & 0x00FFFFFF);
            for (int i = 0; i < _colorSpace.Components; i++)
            {
                _colorValueModels[i].FireCallback(oldValues[i], _colorValues[i]);
            }
            FireColorChanged();
        }

        protected int GetNumComponents()
        {
            return _colorSpace.Components;
        }

        protected override void Layout()
        {
            if (_recreateLayout)
            {
                CreateColorAreas();
            }
            base.Layout();
        }

        public override int GetMinWidth()
        {
            if (_recreateLayout)
            {
                CreateColorAreas();
            }
            return base.GetMinWidth();
        }

        public override int GetMinHeight()
        {
            if (_recreateLayout)
            {
                CreateColorAreas();
            }
            return base.GetMinHeight();
        }

        public override int GetPreferredInnerWidth()
        {
            if (_recreateLayout)
            {
                CreateColorAreas();
            }
            return base.GetPreferredInnerWidth();
        }

        public override int GetPreferredInnerHeight()
        {
            if (_recreateLayout)
            {
                CreateColorAreas();
            }
            return base.GetPreferredInnerHeight();
        }

        protected void CreateColorAreas()
        {
            _recreateLayout = false;
            SetVerticalGroup(null); // stop layout engine while we create new rules
            RemoveAllChildren();

            // recreate models to make sure that no callback is left over
            _argbModels = new ARGBModel[4];
            _argbModels[0] = new ARGBModel(this, 16);
            _argbModels[1] = new ARGBModel(this, 8);
            _argbModels[2] = new ARGBModel(this, 0);
            _argbModels[3] = new ARGBModel(this, 24);

            int numComponents = GetNumComponents();

            Group horzAreas = CreateSequentialGroup().AddGap();
            Group vertAreas = CreateParallelGroup();

            Group horzLabels = null;
            Group horzAdjuster = CreateParallelGroup();
            Group horzControlls = CreateSequentialGroup();

            if (_useLabels)
            {
                horzLabels = CreateParallelGroup();
                horzControlls.AddGroup(horzLabels);
            }
            horzControlls.AddGroup(horzAdjuster);

            Group[] vertAdjuster = new Group[4 + numComponents];
            int numAdjuters = 0;

            for (int i = 0; i < vertAdjuster.Length; i++)
            {
                vertAdjuster[i] = CreateParallelGroup();
            }

            _colorValueModels = new ColorValueModel[numComponents];
            for (int componentI = 0; componentI < numComponents; componentI++)
            {
                _colorValueModels[componentI] = new ColorValueModel(this, componentI);

                if (_showNativeAdjuster)
                {
                    ValueAdjusterFloat vaf = new ValueAdjusterFloat(_colorValueModels[componentI]);

                    if (_useLabels)
                    {
                        Label label = new Label(_colorSpace.ComponentNameOf(componentI));
                        label.SetLabelFor(vaf);
                        horzLabels.AddWidget(label);
                        vertAdjuster[numAdjuters].AddWidget(label);
                    }
                    else
                    {
                        vaf.SetDisplayPrefix(_colorSpace.ComponentShortNameOf(componentI) + ": ");
                        vaf.SetTooltipContent(_colorSpace.ComponentNameOf(componentI));
                    }

                    horzAdjuster.AddWidget(vaf);
                    vertAdjuster[numAdjuters].AddWidget(vaf);
                    numAdjuters++;
                }
            }

            for (int i = 0; i < _argbModels.Length; i++)
            {
                if ((i == 3 && _showAlphaAdjuster) || (i < 3 && _showRGBAdjuster))
                {
                    ValueAdjusterInt vai = new ValueAdjusterInt(_argbModels[i]);

                    if (_useLabels)
                    {
                        Label label = new Label(RGBA_NAMES[i]);
                        label.SetLabelFor(vai);
                        horzLabels.AddWidget(label);
                        vertAdjuster[numAdjuters].AddWidget(label);
                    }
                    else
                    {
                        vai.SetDisplayPrefix(RGBA_PREFIX[i]);
                        vai.SetTooltipContent(RGBA_NAMES[i]);
                    }

                    horzAdjuster.AddWidget(vai);
                    vertAdjuster[numAdjuters].AddWidget(vai);
                    numAdjuters++;
                }
            }

            int component = 0;

            if (_useColorArea2D)
            {
                for (; component + 1 < numComponents; component += 2)
                {
                    ColorArea2D area = new ColorArea2D(this, component, component + 1);
                    area.SetTooltipContent(_colorSpace.ComponentNameOf(component) +
                            " / " + _colorSpace.ComponentNameOf(component + 1));

                    horzAreas.AddWidget(area);
                    vertAreas.AddWidget(area);
                }
            }

            for (; component < numComponents; component++)
            {
                ColorArea1D area = new ColorArea1D(this, component);
                area.SetTooltipContent(_colorSpace.ComponentNameOf(component));

                horzAreas.AddWidget(area);
                vertAreas.AddWidget(area);
            }

            if (_showHexEditField && _hexColorEditField == null)
            {
                CreateHexColorEditField();
            }

            if (_showPreview)
            {
                if (_previewTintAnimator == null)
                {
                    _previewTintAnimator = new TintAnimator(this, GetColor());
                }

                Widget previewArea = new Widget();
                previewArea.SetTheme("colorarea");
                previewArea.GetTintAnimator(_previewTintAnimator);

                Widget preview = new Container();
                preview.SetTheme("preview");
                preview.Add(previewArea);

                Label label = new Label();
                label.SetTheme("previewLabel");
                label.SetLabelFor(preview);

                Group horz = CreateParallelGroup();
                Group vert = CreateSequentialGroup();

                horzAreas.AddGroup(horz.AddWidget(label).AddWidget(preview));
                vertAreas.AddGroup(vert.AddGap().AddWidget(label).AddWidget(preview));

                if (_showHexEditField)
                {
                    horz.AddWidget(_hexColorEditField);
                    vert.AddGap().AddWidget(_hexColorEditField);
                }
            }

            Group horzMainGroup = CreateParallelGroup()
                    .AddGroup(horzAreas.AddGap())
                    .AddGroup(horzControlls);
            Group vertMainGroup = CreateSequentialGroup()
                    .AddGroup(vertAreas);

            for (int i = 0; i < numAdjuters; i++)
            {
                vertMainGroup.AddGroup(vertAdjuster[i]);
            }

            if (_showHexEditField)
            {
                if (_hexColorEditField == null)
                {
                    CreateHexColorEditField();
                }

                if (!_showPreview)
                {
                    horzMainGroup.AddWidget(_hexColorEditField);
                    vertMainGroup.AddWidget(_hexColorEditField);
                }

                UpdateHexEditField();
            }
            SetHorizontalGroup(horzMainGroup);
            SetVerticalGroup(vertMainGroup.AddGap());
        }

        protected override void AfterAddToGUI(GUI gui)
        {
            base.AfterAddToGUI(gui);
            AddModelCallback();
        }

        protected override void BeforeRemoveFromGUI(GUI gui)
        {
            RemoveModelCallback();
            base.BeforeRemoveFromGUI(gui);
        }

        private void RemoveModelCallback()
        {
            if (_model != null)
            {
                _model.Changed -= Model_Changed;
            }
        }

        private void AddModelCallback()
        {
            if (_model != null && GetGUI() != null)
            {
                /*if(modelCallback == null) {
                    modelCallback = new Runnable() {
                        public void run() {
                            modelValueChanged();
                        }
                    };
                }*/
                _model.Changed += Model_Changed;
            }
        }

        private void Model_Changed(object sender, ColorChangedEventArgs e)
        {
            ModelValueChanged();
        }

        class HexColorEditField : EditField
        {
            protected override void InsertChar(char ch)
            {
                if (IsValid(ch))
                {
                    base.InsertChar(ch);
                }
            }

            public override void InsertText(String str)
            {
                for (int i = 0, n = str.Length; i < n; i++)
                {
                    if (!IsValid(str[i]))
                    {
                        StringBuilder sb = new StringBuilder(str);
                        for (int j = n; j-- >= i;)
                        {
                            if (!IsValid(sb[j]))
                            {
                                sb.Remove(j, 1);
                            }
                        }
                        str = sb.ToString();
                        break;
                    }
                }

                base.InsertText(str);
            }

            private bool IsValid(char ch)
            {
                int digit = CharUtil.Digit(ch, 16);
                return digit >= 0 && digit < 16;
            }
        }

        private void CreateHexColorEditField()
        {
            _hexColorEditField = new HexColorEditField();
            _hexColorEditField.SetTheme("hexColorEditField");
            _hexColorEditField.SetColumns(8);
            _hexColorEditField.Callback += (sender, e) =>
            {
                if (e.Key == Event.KEY_ESCAPE)
                {
                    UpdateHexEditField();
                    return;
                }
                Color color = null;
                try
                {
                    color = Color.Parse("#" + _hexColorEditField.GetText());
                    _hexColorEditField.SetErrorMessage(null);
                }
                catch (Exception ex)
                {
                    _hexColorEditField.SetErrorMessage("Invalid color format");
                }
                if (e.Key == Event.KEY_RETURN && color != null)
                {
                    SetColor(color);
                }
            };
        }

        void UpdateHexEditField()
        {
            if (_hexColorEditField != null)
            {
                _hexColorEditField.SetText(String.Format("{0:x8}", _currentColor));
            }
        }

        void ModelValueChanged()
        {
            if (!_inModelSetValue && _model != null)
            {
                // don't call updateModel here
                SetColorInt(_model.Value.ARGB);
            }
        }

        private static int IMAGE_SIZE = 64;

        protected internal class ColorValueModel : AbstractFloatModel
        {
            private int _component;
            private ColorSelector _colorSelector;

            protected internal ColorValueModel(ColorSelector colorSelector, int component)
            {
                this._colorSelector = colorSelector;
                this._component = component;
            }

            public override float Value
            {
                get
                {
                    return this._colorSelector._colorValues[_component];
                }
                set
                {
                    float oldValue = this._colorSelector._colorValues[_component];
                    this._colorSelector._colorValues[_component] = value;
                    this.Changed.Invoke(this._colorSelector, new FloatChangedEventArgs(oldValue, value));
                    this._colorSelector.FireColorChanged();
                }
            }

            public override float MinValue
            {
                get
                {
                    return this._colorSelector._colorSpace.ComponentMinValueOf(_component);
                }
            }

            public override float MaxValue
            {
                get
                {
                    return this._colorSelector._colorSpace.ComponentMaxValueOf(_component);
                }
            }

            public override event EventHandler<FloatChangedEventArgs> Changed;

            protected internal void FireCallback(float oldValue, float newValue)
            {
                this.Changed.Invoke(this._colorSelector, new FloatChangedEventArgs(oldValue, newValue));
            }
        }

        protected internal class ARGBModel : AbstractIntegerModel
        {
            private int _startBit;
            private ColorSelector _colorSelector;

            protected internal ARGBModel(ColorSelector colorSelector, int startBit)
            {
                this._colorSelector = colorSelector;
                this._startBit = startBit;
            }

            public override int Value
            {
                get
                {
                    return (int)(this._colorSelector._currentColor >> _startBit) & 255;
                }

                set
                {
                    string x = this._colorSelector._currentColor.ToString("X");

                    int xy = ~(255 << _startBit);
                    System.Diagnostics.Debug.WriteLine("a:  " + xy.ToString("X"));
                    int xyy = this._colorSelector._currentColor & xy;
                    System.Diagnostics.Debug.WriteLine("ya:  " + xyy.ToString("X"));
                    int xy4 = (value << _startBit);
                    System.Diagnostics.Debug.WriteLine("xy4:  " + xy4.ToString("X"));
                    this._colorSelector.SetColorInt((int)((this._colorSelector._currentColor & xy) | (value << _startBit)));
                    string x2 = this._colorSelector._currentColor.ToString("X");
                    System.Diagnostics.Debug.WriteLine("x:  " + x + " | x2: " + x2);
                }
            }

            public override int MinValue
            {
                get
                {
                    return 0;
                }
            }

            public override int MaxValue
            {
                get
                {
                    return 255;
                }
            }

            public override event EventHandler<IntegerChangedEventArgs> Changed;

            protected internal void FireCallback(int oldV, int newV)
            {
                this.Changed.Invoke(this._colorSelector, new IntegerChangedEventArgs(oldV, newV));
            }
        }

        protected internal abstract class ColorArea : Widget
        {
            protected internal DynamicImage _img;
            protected internal Image _cursorImage;
            protected internal bool _needsUpdate;

            protected override void ApplyTheme(ThemeInfo themeInfo)
            {
                base.ApplyTheme(themeInfo);
                _cursorImage = themeInfo.GetImage("cursor");
            }

            public abstract void CreateImage(GUI gui);
            public abstract void UpdateImage();
            public abstract void HandleMouse(int x, int y);

            protected override void PaintWidget(GUI gui)
            {
                if (_img == null)
                {
                    CreateImage(gui);
                    _needsUpdate = true;
                }
                if (_img != null)
                {
                    if (_needsUpdate)
                    {
                        UpdateImage();
                    }
                    _img.Draw(GetAnimationState(), GetInnerX(), GetInnerY(), GetInnerWidth(), GetInnerHeight());
                }
            }

            public override void Destroy()
            {
                base.Destroy();
                if (_img != null)
                {
                    _img.Dispose();
                    _img = null;
                }
            }

            public override bool HandleEvent(Event evt)
            {
                if (evt.GetEventType() == EventType.MOUSE_BTNDOWN || evt.GetEventType() == EventType.MOUSE_DRAGGED)
                {
                    HandleMouse(evt.GetMouseX() - GetInnerX(), evt.GetMouseY() - GetInnerY());
                    return true;
                }
                else if (evt.GetEventType() == EventType.MOUSE_WHEEL)
                {
                    return false;
                }
                else
                {
                    if (evt.IsMouseEvent())
                    {
                        return true;
                    }
                }

                return base.HandleEvent(evt);
            }

            public void Run()
            {
                _needsUpdate = true;
            }
        }

        protected internal class ColorArea1D : ColorArea
        {
            int _component;
            private ColorSelector _colorSelector;

            protected internal ColorArea1D(ColorSelector colorSelector, int component)
            {
                this._colorSelector = colorSelector;
                this._component = component;

                for (int i = 0, n = this._colorSelector.GetNumComponents(); i < n; i++)
                {
                    if (i != component)
                    {
                        this._colorSelector._colorValueModels[i].Changed += ColorArea1D_Changed;
                    }
                }
            }

            private void ColorArea1D_Changed(object sender, FloatChangedEventArgs e)
            {
                this.Run();
            }

            protected override void PaintWidget(GUI gui)
            {
                base.PaintWidget(gui);
                if (_cursorImage != null)
                {
                    float minValue = this._colorSelector._colorSpace.ComponentMinValueOf(_component);
                    float maxValue = this._colorSelector._colorSpace.ComponentMaxValueOf(_component);
                    int pos = (int)((this._colorSelector._colorValues[_component] - maxValue) * (GetInnerHeight() - 1) / (minValue - maxValue) + 0.5f);
                    _cursorImage.Draw(GetAnimationState(), GetInnerX(), GetInnerY() + pos, GetInnerWidth(), 1);
                }
            }

            public override void CreateImage(GUI gui)
            {
                _img = gui.GetRenderer().CreateDynamicImage(1, IMAGE_SIZE);
            }

            public override void UpdateImage()
            {
                float[] temp = (float[]) this._colorSelector._colorValues.Clone();
                Microsoft.Xna.Framework.Color[] buf = this._colorSelector._imgData;
                ColorSpace cs = this._colorSelector._colorSpace;

                float x = cs.ComponentMaxValueOf(_component);
                float dx = (cs.ComponentMinValueOf(_component) - x) / (IMAGE_SIZE - 1);

                for (int i = 0; i < IMAGE_SIZE; i++)
                {
                    temp[_component] = x;
                    Color twlColor = new Color(cs.RGB(temp));
                    buf[i] = new Microsoft.Xna.Framework.Color(twlColor.RedF, twlColor.GreenF, twlColor.BlueF);
                    x += dx;
                }

                _img.Update(buf);
                _needsUpdate = false;
            }

            public override void HandleMouse(int x, int y)
            {
                float minValue = this._colorSelector._colorSpace.ComponentMinValueOf(_component);
                float maxValue = this._colorSelector._colorSpace.ComponentMaxValueOf(_component);
                int innerHeight = GetInnerHeight();
                int pos = Math.Max(0, Math.Min(innerHeight, y));
                float value = maxValue + (minValue - maxValue) * pos / innerHeight;
                this._colorSelector._colorValueModels[_component].Value = value;
            }
        }

        protected internal class ColorArea2D : ColorArea
        {
            private int _componentX;
            private int _componentY;

            private ColorSelector _colorSelector;

            protected internal ColorArea2D(ColorSelector colorSelector, int componentX, int componentY)
            {
                this._colorSelector = colorSelector;

                this._componentX = componentX;
                this._componentY = componentY;

                for (int i = 0, n = this._colorSelector.GetNumComponents(); i < n; i++)
                {
                    if (i != componentX && i != componentY)
                    {
                        this._colorSelector._colorValueModels[i].Changed += ColorArea2D_Changed;
                    }
                }
            }

            private void ColorArea2D_Changed(object sender, FloatChangedEventArgs e)
            {
                this.Run();
            }

            protected override void PaintWidget(GUI gui)
            {
                base.PaintWidget(gui);
                if (_cursorImage != null)
                {
                    float minValueX = this._colorSelector._colorSpace.ComponentMinValueOf(_componentX);
                    float maxValueX = this._colorSelector._colorSpace.ComponentMaxValueOf(_componentX);
                    float minValueY = this._colorSelector._colorSpace.ComponentMinValueOf(_componentY);
                    float maxValueY = this._colorSelector._colorSpace.ComponentMaxValueOf(_componentY);
                    int posX = (int)((this._colorSelector._colorValues[_componentX] - maxValueX) * (GetInnerWidth() - 1) / (minValueX - maxValueX) + 0.5f);
                    int posY = (int)((this._colorSelector._colorValues[_componentY] - maxValueY) * (GetInnerHeight() - 1) / (minValueY - maxValueY) + 0.5f);
                    _cursorImage.Draw(GetAnimationState(), GetInnerX() + posX, GetInnerY() + posY, 1, 1);
                }
            }

            public override void CreateImage(GUI gui)
            {
                _img = gui.GetRenderer().CreateDynamicImage(IMAGE_SIZE, IMAGE_SIZE);
            }

            public override void UpdateImage()
            {
                float[] temp = (float[])this._colorSelector._colorValues.Clone();
                Microsoft.Xna.Framework.Color[] buf = this._colorSelector._imgData;
                ColorSpace cs = this._colorSelector._colorSpace;

                float x0 = cs.ComponentMaxValueOf(_componentX);
                float dx = (cs.ComponentMinValueOf(_componentX) - x0) / (IMAGE_SIZE - 1);

                float y = cs.ComponentMaxValueOf(_componentY);
                float dy = (cs.ComponentMinValueOf(_componentY) - y) / (IMAGE_SIZE - 1);

                for (int i = 0, idx = 0; i < IMAGE_SIZE; i++)
                {
                    temp[_componentY] = y;
                    float x = x0;
                    for (int j = 0; j < IMAGE_SIZE; j++)
                    {
                        temp[_componentX] = x;
                        Color twlColor = new Color(cs.RGB(temp));
                        buf[idx++] = new Microsoft.Xna.Framework.Color(twlColor.RedF, twlColor.GreenF, twlColor.BlueF);
                        x += dx;
                    }
                    y += dy;
                }

                _img.Update(buf);
                _needsUpdate = false;
            }

            public override void HandleMouse(int x, int y)
            {
                float minValueX = this._colorSelector._colorSpace.ComponentMinValueOf(_componentX);
                float maxValueX = this._colorSelector._colorSpace.ComponentMaxValueOf(_componentX);
                float minValueY = this._colorSelector._colorSpace.ComponentMinValueOf(_componentY);
                float maxValueY = this._colorSelector._colorSpace.ComponentMaxValueOf(_componentY);
                int innerWidtht = GetInnerWidth();
                int innerHeight = GetInnerHeight();
                int posX = Math.Max(0, Math.Min(innerWidtht, x));
                int posY = Math.Max(0, Math.Min(innerHeight, y));
                float valueX = maxValueX + (minValueX - maxValueX) * posX / innerWidtht;
                float valueY = maxValueY + (minValueY - maxValueY) * posY / innerHeight;
                this._colorSelector._colorValueModels[_componentX].Value = valueX;
                this._colorSelector._colorValueModels[_componentY].Value = valueY;
            }
        }
    }

    public class ColorSelectorColorChangedEventArgs
    {
    }
}
