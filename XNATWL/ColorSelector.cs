using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static XNATWL.Theme.AnimatedImage;
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
        Microsoft.Xna.Framework.Color[] imgData;

        ColorSpace colorSpace;
        float[] colorValues;
        ColorValueModel[] colorValueModels;
        private bool useColorArea2D = true;
        private bool showPreview = false;
        private bool useLabels = true;
        private bool showHexEditField = false;
        private bool showNativeAdjuster = true;
        private bool showRGBAdjuster = true;
        private bool showAlphaAdjuster = true;
        private Runnable[] callbacks;
        private ColorModel model;
        private Runnable modelCallback;
        private bool inModelSetValue;
        uint currentColor;
        private ARGBModel[] argbModels;
        EditField hexColorEditField;
        private TintAnimator previewTintAnimator;
        private bool recreateLayout;

        public event EventHandler<ColorSelectorColorChangedEventArgs> ColorChanged;

        public ColorSelector(ColorSpace colorSpace)
        {
            // allocate enough space for 2D color areas
            this.imgData = new Microsoft.Xna.Framework.Color[IMAGE_SIZE * IMAGE_SIZE];

            currentColor = (uint) Color.WHITE.ARGB;

            setColorSpace(colorSpace);
        }

        public ColorSpace getColorSpace()
        {
            return colorSpace;
        }

        public void setColorSpace(ColorSpace colorModel)
        {
            if (colorModel == null)
            {
                throw new ArgumentNullException("colorModel");
            }
            if (this.colorSpace != colorModel)
            {
                bool hasColor = this.colorSpace != null;

                this.colorSpace = colorModel;
                this.colorValues = new float[colorModel.Components];

                if (hasColor)
                {
                    setColorInt((int) currentColor);
                }
                else
                {
                    setDefaultColor();
                }

                recreateLayout = true;
                invalidateLayout();
            }
        }

        public ColorModel getModel()
        {
            return model;
        }

        public void setModel(ColorModel model)
        {
            if (this.model != model)
            {
                removeModelCallback();
                this.model = model;
                if (model != null)
                {
                    addModelCallback();
                    modelValueChanged();
                }
            }
        }

        public Color getColor()
        {
            return new Color(currentColor);
        }

        public void setColor(Color color)
        {
            setColorInt(color.ARGB);
            updateModel();
        }

        public void setDefaultColor()
        {
            currentColor = (uint)Color.WHITE.ARGB;
            for (int i = 0; i < colorSpace.Components; i++)
            {
                float oldValue = colorValues[i];
                colorValues[i] = colorSpace.ComponentDefaultValueOf(i);
                colorValueModels[i].fireCallback(oldValue, colorValues[i]);
            }
            colorChanged();
        }

        public bool isUseColorArea2D()
        {
            return useColorArea2D;
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
        public void setUseColorArea2D(bool useColorArea2D)
        {
            if (this.useColorArea2D != useColorArea2D)
            {
                this.useColorArea2D = useColorArea2D;
                recreateLayout = true;
                invalidateLayout();
            }
        }

        public bool isShowPreview()
        {
            return showPreview;
        }

        /**
         * Show the currently selected color in a preview widget.
         * Default is false.
         *
         * @param showPreview true if the preview widget should be displayed
         */
        public void setShowPreview(bool showPreview)
        {
            if (this.showPreview != showPreview)
            {
                this.showPreview = showPreview;
                recreateLayout = true;
                invalidateLayout();
            }
        }

        public bool isShowHexEditField()
        {
            return showHexEditField;
        }

        /**
         * Includes an edit field which allows to edit the color hex values in ARGB.
         * Default is false.
         *
         * @param showHexEditField true if the edit field should be shown
         */
        public void setShowHexEditField(bool showHexEditField)
        {
            if (this.showHexEditField != showHexEditField)
            {
                this.showHexEditField = showHexEditField;
                recreateLayout = true;
                invalidateLayout();
            }
        }

        public bool isShowAlphaAdjuster()
        {
            return showAlphaAdjuster;
        }

        public void setShowAlphaAdjuster(bool showAlphaAdjuster)
        {
            if (this.showAlphaAdjuster != showAlphaAdjuster)
            {
                this.showAlphaAdjuster = showAlphaAdjuster;
                recreateLayout = true;
                invalidateLayout();
            }
        }

        public bool isShowNativeAdjuster()
        {
            return showNativeAdjuster;
        }

        /**
         * Includes adjuster for each clor component of the specified color space.
         * Default is true.
         *
         * @param showNativeAdjuster true if the native adjuster should be displayed
         */
        public void setShowNativeAdjuster(bool showNativeAdjuster)
        {
            if (this.showNativeAdjuster != showNativeAdjuster)
            {
                this.showNativeAdjuster = showNativeAdjuster;
                recreateLayout = true;
                invalidateLayout();
            }
        }

        public bool isShowRGBAdjuster()
        {
            return showRGBAdjuster;
        }

        public void setShowRGBAdjuster(bool showRGBAdjuster)
        {
            if (this.showRGBAdjuster != showRGBAdjuster)
            {
                this.showRGBAdjuster = showRGBAdjuster;
                recreateLayout = true;
                invalidateLayout();
            }
        }

        public bool isUseLabels()
        {
            return useLabels;
        }

        /**
         * Show labels infront of the value adjusters for the color components.
         * Default is true.
         *
         * @param useLabels true if labels should be displayed
         */
        public void setUseLabels(bool useLabels)
        {
            if (this.useLabels != useLabels)
            {
                this.useLabels = useLabels;
                recreateLayout = true;
                invalidateLayout();
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

        protected void updateModel()
        {
            if (model != null)
            {
                inModelSetValue = true;
                try
                {
                    model.Value = getColor();
                }
                finally
                {
                    inModelSetValue = false;
                }
            }
        }

        protected void colorChanged()
        {
            int oldV = (int)currentColor;
            currentColor = (uint)((currentColor & (0xFF << 24)) | colorSpace.RGB(colorValues));
            this.ColorChanged.Invoke(this, new ColorSelectorColorChangedEventArgs());
            updateModel();
            if (argbModels != null)
            {
                foreach (ARGBModel m in argbModels)
                {
                    m.fireCallback(oldV, (int)currentColor);
                }
            }
            if (previewTintAnimator != null)
            {
                previewTintAnimator.setColor(getColor());
            }
            updateHexEditField();
        }

        protected void setColorInt(int argb)
        {
            currentColor = (uint)argb;
            float[] oldValues = colorValues;
            colorValues = colorSpace.FromRGB(argb & 0xFFFFFF);
            for (int i = 0; i < colorSpace.Components; i++)
            {
                colorValueModels[i].fireCallback(oldValues[i], colorValues[i]);
            }
            colorChanged();
        }

        protected int getNumComponents()
        {
            return colorSpace.Components;
        }

        protected override void layout()
        {
            if (recreateLayout)
            {
                createColorAreas();
            }
            base.layout();
        }

        public override int getMinWidth()
        {
            if (recreateLayout)
            {
                createColorAreas();
            }
            return base.getMinWidth();
        }

        public override int getMinHeight()
        {
            if (recreateLayout)
            {
                createColorAreas();
            }
            return base.getMinHeight();
        }

        public override int getPreferredInnerWidth()
        {
            if (recreateLayout)
            {
                createColorAreas();
            }
            return base.getPreferredInnerWidth();
        }

        public override int getPreferredInnerHeight()
        {
            if (recreateLayout)
            {
                createColorAreas();
            }
            return base.getPreferredInnerHeight();
        }

        protected void createColorAreas()
        {
            recreateLayout = false;
            setVerticalGroup(null); // stop layout engine while we create new rules
            removeAllChildren();

            // recreate models to make sure that no callback is left over
            argbModels = new ARGBModel[4];
            argbModels[0] = new ARGBModel(this, 16);
            argbModels[1] = new ARGBModel(this, 8);
            argbModels[2] = new ARGBModel(this, 0);
            argbModels[3] = new ARGBModel(this, 24);

            int numComponents = getNumComponents();

            Group horzAreas = createSequentialGroup().addGap();
            Group vertAreas = createParallelGroup();

            Group horzLabels = null;
            Group horzAdjuster = createParallelGroup();
            Group horzControlls = createSequentialGroup();

            if (useLabels)
            {
                horzLabels = createParallelGroup();
                horzControlls.addGroup(horzLabels);
            }
            horzControlls.addGroup(horzAdjuster);

            Group[] vertAdjuster = new Group[4 + numComponents];
            int numAdjuters = 0;

            for (int i = 0; i < vertAdjuster.Length; i++)
            {
                vertAdjuster[i] = createParallelGroup();
            }

            colorValueModels = new ColorValueModel[numComponents];
            for (int componentI = 0; componentI < numComponents; componentI++)
            {
                colorValueModels[componentI] = new ColorValueModel(this, componentI);

                if (showNativeAdjuster)
                {
                    ValueAdjusterFloat vaf = new ValueAdjusterFloat(colorValueModels[componentI]);

                    if (useLabels)
                    {
                        Label label = new Label(colorSpace.ComponentNameOf(componentI));
                        label.setLabelFor(vaf);
                        horzLabels.addWidget(label);
                        vertAdjuster[numAdjuters].addWidget(label);
                    }
                    else
                    {
                        vaf.setDisplayPrefix(colorSpace.ComponentShortNameOf(componentI) + ": ");
                        vaf.setTooltipContent(colorSpace.ComponentNameOf(componentI));
                    }

                    horzAdjuster.addWidget(vaf);
                    vertAdjuster[numAdjuters].addWidget(vaf);
                    numAdjuters++;
                }
            }

            for (int i = 0; i < argbModels.Length; i++)
            {
                if ((i == 3 && showAlphaAdjuster) || (i < 3 && showRGBAdjuster))
                {
                    ValueAdjusterInt vai = new ValueAdjusterInt(argbModels[i]);

                    if (useLabels)
                    {
                        Label label = new Label(RGBA_NAMES[i]);
                        label.setLabelFor(vai);
                        horzLabels.addWidget(label);
                        vertAdjuster[numAdjuters].addWidget(label);
                    }
                    else
                    {
                        vai.setDisplayPrefix(RGBA_PREFIX[i]);
                        vai.setTooltipContent(RGBA_NAMES[i]);
                    }

                    horzAdjuster.addWidget(vai);
                    vertAdjuster[numAdjuters].addWidget(vai);
                    numAdjuters++;
                }
            }

            int component = 0;

            if (useColorArea2D)
            {
                for (; component + 1 < numComponents; component += 2)
                {
                    ColorArea2D area = new ColorArea2D(this, component, component + 1);
                    area.setTooltipContent(colorSpace.ComponentNameOf(component) +
                            " / " + colorSpace.ComponentNameOf(component + 1));

                    horzAreas.addWidget(area);
                    vertAreas.addWidget(area);
                }
            }

            for (; component < numComponents; component++)
            {
                ColorArea1D area = new ColorArea1D(this, component);
                area.setTooltipContent(colorSpace.ComponentNameOf(component));

                horzAreas.addWidget(area);
                vertAreas.addWidget(area);
            }

            if (showHexEditField && hexColorEditField == null)
            {
                createHexColorEditField();
            }

            if (showPreview)
            {
                if (previewTintAnimator == null)
                {
                    previewTintAnimator = new TintAnimator(this, getColor());
                }

                Widget previewArea = new Widget();
                previewArea.setTheme("colorarea");
                previewArea.setTintAnimator(previewTintAnimator);

                Widget preview = new Container();
                preview.setTheme("preview");
                preview.add(previewArea);

                Label label = new Label();
                label.setTheme("previewLabel");
                label.setLabelFor(preview);

                Group horz = createParallelGroup();
                Group vert = createSequentialGroup();

                horzAreas.addGroup(horz.addWidget(label).addWidget(preview));
                vertAreas.addGroup(vert.addGap().addWidget(label).addWidget(preview));

                if (showHexEditField)
                {
                    horz.addWidget(hexColorEditField);
                    vert.addGap().addWidget(hexColorEditField);
                }
            }

            Group horzMainGroup = createParallelGroup()
                    .addGroup(horzAreas.addGap())
                    .addGroup(horzControlls);
            Group vertMainGroup = createSequentialGroup()
                    .addGroup(vertAreas);

            for (int i = 0; i < numAdjuters; i++)
            {
                vertMainGroup.addGroup(vertAdjuster[i]);
            }

            if (showHexEditField)
            {
                if (hexColorEditField == null)
                {
                    createHexColorEditField();
                }

                if (!showPreview)
                {
                    horzMainGroup.addWidget(hexColorEditField);
                    vertMainGroup.addWidget(hexColorEditField);
                }

                updateHexEditField();
            }
            setHorizontalGroup(horzMainGroup);
            setVerticalGroup(vertMainGroup.addGap());
        }

        protected override void afterAddToGUI(GUI gui)
        {
            base.afterAddToGUI(gui);
            addModelCallback();
        }

        protected override void beforeRemoveFromGUI(GUI gui)
        {
            removeModelCallback();
            base.beforeRemoveFromGUI(gui);
        }

        private void removeModelCallback()
        {
            if (model != null)
            {
                model.Changed -= Model_Changed;
            }
        }

        private void addModelCallback()
        {
            if (model != null && getGUI() != null)
            {
                /*if(modelCallback == null) {
                    modelCallback = new Runnable() {
                        public void run() {
                            modelValueChanged();
                        }
                    };
                }*/
                model.Changed += Model_Changed;
            }
        }

        private void Model_Changed(object sender, ColorChangedEventArgs e)
        {
            modelValueChanged();
        }

        private void createHexColorEditField()
        {
            /*hexColorEditField = new EditField() {
                protected override void insertChar(char ch) {
                    if(isValid(ch)) {
                        base.insertChar(ch);
                    }
                }

                public override void insertText(String str) {
                    for(int i=0,n=str.length() ; i<n ; i++) {
                        if(!isValid(str.charAt(i))) {
                            StringBuilder sb = new StringBuilder(str);
                            for(int j=n ; j-- >= i ;) {
                                if(!isValid(sb.charAt(j))) {
                                    sb.deleteCharAt(j);
                                }
                            }
                            str = sb.toString();
                            break;
                        }
                    }
                    base.insertText(str);
                }

                private bool isValid(char ch) {
                    int digit = Character.digit(ch, 16);
                    return digit >= 0 && digit < 16;
                }
            };
            hexColorEditField.setTheme("hexColorEditField");
            hexColorEditField.setColumns(8);
            hexColorEditField.addCallback(new EditField.Callback() {
                public void callback(int key) {
                    if(key == Event.KEY_ESCAPE) {
                        updateHexEditField();
                        return;
                    }
                    Color color = null;
                    try {
                        color = Color.parserColor("#".concat(hexColorEditField.getText()));
                        hexColorEditField.setErrorMessage(null);
                    } catch(Exception ex) {
                        hexColorEditField.setErrorMessage("Invalid color format");
                    }
                    if(key == Event.KEY_RETURN && color != null) {
                        setColor(color);
                    }
                }
            });*/
        }

        void updateHexEditField()
        {
            if (hexColorEditField != null)
            {
                hexColorEditField.setText(String.Format("{0:x8}", currentColor));
            }
        }

        void modelValueChanged()
        {
            if (!inModelSetValue && model != null)
            {
                // don't call updateModel here
                setColorInt(model.Value.ARGB);
            }
        }

        private static int IMAGE_SIZE = 64;

        protected internal class ColorValueModel : AbstractFloatModel
        {
            private int component;
            private ColorSelector colorSelector;

            protected internal ColorValueModel(ColorSelector colorSelector, int component)
            {
                this.colorSelector = colorSelector;
                this.component = component;
            }

            public override float Value
            {
                get
                {
                    return this.colorSelector.colorValues[component];
                }
                set
                {
                    float oldValue = this.colorSelector.colorValues[component];
                    this.colorSelector.colorValues[component] = value;
                    this.Changed.Invoke(this.colorSelector, new FloatChangedEventArgs(oldValue, value));
                    this.colorSelector.colorChanged();
                }
            }

            public override float MinValue
            {
                get
                {
                    return this.colorSelector.colorSpace.ComponentMinValueOf(component);
                }
            }

            public override float MaxValue
            {
                get
                {
                    return this.colorSelector.colorSpace.ComponentMaxValueOf(component);
                }
            }

            public override event EventHandler<FloatChangedEventArgs> Changed;

            protected internal void fireCallback(float oldValue, float newValue)
            {
                this.Changed.Invoke(this.colorSelector, new FloatChangedEventArgs(oldValue, newValue));
            }
        }

        protected internal class ARGBModel : AbstractIntegerModel
        {
            private int startBit;
            private ColorSelector colorSelector;

            protected internal ARGBModel(ColorSelector colorSelector, int startBit)
            {
                this.colorSelector = colorSelector;
                this.startBit = startBit;
            }

            public override int Value
            {
                get
                {
                    return (int)(this.colorSelector.currentColor >> startBit) & 255;
                }

                set
                {
                    this.colorSelector.setColorInt((int)((this.colorSelector.currentColor & ~(255 << startBit)) | (value << startBit)));
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

            protected internal void fireCallback(int oldV, int newV)
            {
                this.Changed.Invoke(this.colorSelector, new IntegerChangedEventArgs(oldV, newV));
            }
        }

        protected internal abstract class ColorArea : Widget
        {
            protected internal DynamicImage img;
            protected internal Image cursorImage;
            protected internal bool needsUpdate;

            protected override void applyTheme(ThemeInfo themeInfo)
            {
                base.applyTheme(themeInfo);
                cursorImage = themeInfo.getImage("cursor");
            }

            public abstract void createImage(GUI gui);
            public abstract void updateImage();
            public abstract void handleMouse(int x, int y);

            protected override void paintWidget(GUI gui)
            {
                if (img == null)
                {
                    createImage(gui);
                    needsUpdate = true;
                }
                if (img != null)
                {
                    if (needsUpdate)
                    {
                        updateImage();
                    }
                    img.Draw(getAnimationState(), getInnerX(), getInnerY(), getInnerWidth(), getInnerHeight());
                }
            }

            public override void destroy()
            {
                base.destroy();
                if (img != null)
                {
                    img.Dispose();
                    img = null;
                }
            }

            public override bool handleEvent(Event evt)
            {
                if (evt.getEventType() == Event.EventType.MOUSE_BTNDOWN || evt.getEventType() == Event.EventType.MOUSE_DRAGGED)
                {
                    handleMouse(evt.getMouseX() - getInnerX(), evt.getMouseY() - getInnerY());
                    return true;
                }
                else if (evt.getEventType() == Event.EventType.MOUSE_WHEEL)
                {
                    return false;
                }
                else
                {
                    if (evt.isMouseEvent())
                    {
                        return true;
                    }
                }

                return base.handleEvent(evt);
            }

            public void run()
            {
                needsUpdate = true;
            }
        }

        protected internal class ColorArea1D : ColorArea
        {
            int component;
            private ColorSelector colorSelector;

            protected internal ColorArea1D(ColorSelector colorSelector, int component)
            {
                this.colorSelector = colorSelector;
                this.component = component;

                for (int i = 0, n = this.colorSelector.getNumComponents(); i < n; i++)
                {
                    if (i != component)
                    {
                        this.colorSelector.colorValueModels[i].Changed += ColorArea1D_Changed;
                    }
                }
            }

            private void ColorArea1D_Changed(object sender, FloatChangedEventArgs e)
            {
                this.run();
            }

            protected override void paintWidget(GUI gui)
            {
                base.paintWidget(gui);
                if (cursorImage != null)
                {
                    float minValue = this.colorSelector.colorSpace.ComponentMinValueOf(component);
                    float maxValue = this.colorSelector.colorSpace.ComponentMaxValueOf(component);
                    int pos = (int)((this.colorSelector.colorValues[component] - maxValue) * (getInnerHeight() - 1) / (minValue - maxValue) + 0.5f);
                    cursorImage.Draw(getAnimationState(), getInnerX(), getInnerY() + pos, getInnerWidth(), 1);
                }
            }

            public override void createImage(GUI gui)
            {
                img = gui.getRenderer().CreateDynamicImage(1, IMAGE_SIZE);
            }

            public override void updateImage()
            {
                float[] temp = (float[]) this.colorSelector.colorValues.Clone();
                Microsoft.Xna.Framework.Color[] buf = this.colorSelector.imgData;
                ColorSpace cs = this.colorSelector.colorSpace;

                float x = cs.ComponentMaxValueOf(component);
                float dx = (cs.ComponentMinValueOf(component) - x) / (IMAGE_SIZE - 1);

                for (int i = 0; i < IMAGE_SIZE; i++)
                {
                    temp[component] = x;
                    Color twlColor = new Color(cs.RGB(temp));
                    buf[i] = new Microsoft.Xna.Framework.Color(twlColor.RedF, twlColor.GreenF, twlColor.BlueF);
                    x += dx;
                }

                img.Update(buf);
                needsUpdate = false;
            }

            public override void handleMouse(int x, int y)
            {
                float minValue = this.colorSelector.colorSpace.ComponentMinValueOf(component);
                float maxValue = this.colorSelector.colorSpace.ComponentMaxValueOf(component);
                int innerHeight = getInnerHeight();
                int pos = Math.Max(0, Math.Min(innerHeight, y));
                float value = maxValue + (minValue - maxValue) * pos / innerHeight;
                this.colorSelector.colorValueModels[component].Value = value;
            }
        }

        protected internal class ColorArea2D : ColorArea
        {
            private int componentX;
            private int componentY;

            private ColorSelector colorSelector;

            protected internal ColorArea2D(ColorSelector colorSelector, int componentX, int componentY)
            {
                this.colorSelector = colorSelector;

                this.componentX = componentX;
                this.componentY = componentY;

                for (int i = 0, n = this.colorSelector.getNumComponents(); i < n; i++)
                {
                    if (i != componentX && i != componentY)
                    {
                        this.colorSelector.colorValueModels[i].Changed += ColorArea2D_Changed;
                    }
                }
            }

            private void ColorArea2D_Changed(object sender, FloatChangedEventArgs e)
            {
                this.run();
            }

            protected override void paintWidget(GUI gui)
            {
                base.paintWidget(gui);
                if (cursorImage != null)
                {
                    float minValueX = this.colorSelector.colorSpace.ComponentMinValueOf(componentX);
                    float maxValueX = this.colorSelector.colorSpace.ComponentMaxValueOf(componentX);
                    float minValueY = this.colorSelector.colorSpace.ComponentMinValueOf(componentY);
                    float maxValueY = this.colorSelector.colorSpace.ComponentMaxValueOf(componentY);
                    int posX = (int)((this.colorSelector.colorValues[componentX] - maxValueX) * (getInnerWidth() - 1) / (minValueX - maxValueX) + 0.5f);
                    int posY = (int)((this.colorSelector.colorValues[componentY] - maxValueY) * (getInnerHeight() - 1) / (minValueY - maxValueY) + 0.5f);
                    cursorImage.Draw(getAnimationState(), getInnerX() + posX, getInnerY() + posY, 1, 1);
                }
            }

            public override void createImage(GUI gui)
            {
                img = gui.getRenderer().CreateDynamicImage(IMAGE_SIZE, IMAGE_SIZE);
            }

            public override void updateImage()
            {
                float[] temp = (float[])this.colorSelector.colorValues.Clone();
                Microsoft.Xna.Framework.Color[] buf = this.colorSelector.imgData;
                ColorSpace cs = this.colorSelector.colorSpace;

                float x0 = cs.ComponentMaxValueOf(componentX);
                float dx = (cs.ComponentMinValueOf(componentX) - x0) / (IMAGE_SIZE - 1);

                float y = cs.ComponentMaxValueOf(componentY);
                float dy = (cs.ComponentMinValueOf(componentY) - y) / (IMAGE_SIZE - 1);

                for (int i = 0, idx = 0; i < IMAGE_SIZE; i++)
                {
                    temp[componentY] = y;
                    float x = x0;
                    for (int j = 0; j < IMAGE_SIZE; j++)
                    {
                        temp[componentX] = x;
                        Color twlColor = new Color(cs.RGB(temp));
                        buf[idx++] = new Microsoft.Xna.Framework.Color(twlColor.RedF, twlColor.GreenF, twlColor.BlueF);
                        x += dx;
                    }
                    y += dy;
                }

                img.Update(buf);
                needsUpdate = false;
            }

            public override void handleMouse(int x, int y)
            {
                float minValueX = this.colorSelector.colorSpace.ComponentMinValueOf(componentX);
                float maxValueX = this.colorSelector.colorSpace.ComponentMaxValueOf(componentX);
                float minValueY = this.colorSelector.colorSpace.ComponentMinValueOf(componentY);
                float maxValueY = this.colorSelector.colorSpace.ComponentMaxValueOf(componentY);
                int innerWidtht = getInnerWidth();
                int innerHeight = getInnerHeight();
                int posX = Math.Max(0, Math.Min(innerWidtht, x));
                int posY = Math.Max(0, Math.Min(innerHeight, y));
                float valueX = maxValueX + (minValueX - maxValueX) * posX / innerWidtht;
                float valueY = maxValueY + (minValueY - maxValueY) * posY / innerHeight;
                this.colorSelector.colorValueModels[componentX].Value = valueX;
                this.colorSelector.colorValueModels[componentY].Value = valueY;
            }
        }
    }

    public class ColorSelectorColorChangedEventArgs
    {
    }
}
