using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Model;
using XNATWL.Utils;

namespace XNATWL.Test
{

    public class ColorSelectorDemoDialog1 : FadeFrame
    {
        ColorSelector cs;
        public ColorSelectorDemoDialog1()
        {
            cs = new ColorSelector(new ColorSpaceHSL());

            ToggleButton btnUse2D = new ToggleButton();
            btnUse2D.setActive(cs.isUseColorArea2D());
            btnUse2D.Action += (sender, e) =>
            {
                cs.setUseColorArea2D(btnUse2D.isActive());
            };
            Label labelUse2D = new Label("Use 2D color area");
            labelUse2D.setLabelFor(btnUse2D);

            ToggleButton btnUseLabels = new ToggleButton();
            btnUseLabels.setActive(cs.isUseColorArea2D());
            btnUseLabels.Action += (sender, e) =>
            {
                cs.setUseLabels(btnUseLabels.isActive());
            };
            Label labelUseLabels = new Label("show labels for adjusters");
            labelUseLabels.setLabelFor(btnUseLabels);

            ToggleButton btnShowPreview = new ToggleButton();
            btnShowPreview.setActive(cs.isShowPreview());
            btnShowPreview.Action += (sender, e) =>
            {
                cs.setShowPreview(btnShowPreview.isActive());
            };
            Label labelShowPreview = new Label("show color preview");
            labelShowPreview.setLabelFor(btnShowPreview);

            ToggleButton btnShowHexEditField = new ToggleButton();
            btnShowHexEditField.setActive(cs.isShowHexEditField());
            btnShowHexEditField.Action += (sender, e) =>
            {
                cs.setShowHexEditField(btnShowHexEditField.isActive());
            };
            Label labelShowHexEditField = new Label("show hex edit field");
            labelShowHexEditField.setLabelFor(btnShowHexEditField);

            ToggleButton btnShowNativeAdjuster = new ToggleButton();
            btnShowNativeAdjuster.Action += (sender, e) =>
            {
                cs.setShowNativeAdjuster(btnShowNativeAdjuster.isActive());
            };
            btnShowNativeAdjuster.setActive(cs.isShowNativeAdjuster());
            Label labelShowNativeAdjuster = new Label("show native (HSL) adjuster");
            labelShowNativeAdjuster.setLabelFor(btnShowNativeAdjuster);

            ToggleButton btnShowRGBAdjuster = new ToggleButton();
            btnShowRGBAdjuster.Action += (sender, e) =>
            {
                cs.setShowRGBAdjuster(btnShowRGBAdjuster.isActive());
            };
            btnShowRGBAdjuster.setActive(cs.isShowRGBAdjuster());

            Label labelShowRGBAdjuster = new Label("show RGB adjuster");
            labelShowRGBAdjuster.setLabelFor(btnShowRGBAdjuster);

            ToggleButton btnShowAlphaAdjuster = new ToggleButton();
            btnShowAlphaAdjuster.setActive(cs.isShowAlphaAdjuster());
            btnShowAlphaAdjuster.Action += (sender, e) =>
            {
                cs.setShowAlphaAdjuster(btnShowAlphaAdjuster.isActive());
            };

            Label labelShowAlphaAdjuster = new Label("show alpha adjuster");
            labelShowAlphaAdjuster.setLabelFor(btnShowAlphaAdjuster);

            TintAnimator tintAnimator = new TintAnimator(new TintAnimator.GUITimeSource(this));

            Label testDisplay = new Label("This is a test display");
            testDisplay.setTheme("testDisplay");
            testDisplay.setTintAnimator(tintAnimator);

            Label testDisplay2 = new Label("This is a test display");
            testDisplay2.setTheme("testDisplay2");
            testDisplay2.setTintAnimator(tintAnimator);

            cs.ColorChanged += (x, y) =>
            {
                tintAnimator.SetColor(cs.getColor());
            };
            tintAnimator.SetColor(cs.getColor());

            DialogLayout dl = new DialogLayout();
            dl.setHorizontalGroup(dl.createParallelGroup()
                    .addWidgets(cs, testDisplay, testDisplay2)
                    .addGroup(dl.createSequentialGroup().addGap().addWidgets(labelUse2D, btnUse2D))
                    .addGroup(dl.createSequentialGroup().addGap().addWidgets(labelUseLabels, btnUseLabels))
                    .addGroup(dl.createSequentialGroup().addGap().addWidgets(labelShowPreview, btnShowPreview))
                    .addGroup(dl.createSequentialGroup().addGap().addWidgets(labelShowHexEditField, btnShowHexEditField))
                    .addGroup(dl.createSequentialGroup().addGap().addWidgets(labelShowNativeAdjuster, btnShowNativeAdjuster))
                    .addGroup(dl.createSequentialGroup().addGap().addWidgets(labelShowRGBAdjuster, btnShowRGBAdjuster))
                    .addGroup(dl.createSequentialGroup().addGap().addWidgets(labelShowAlphaAdjuster, btnShowAlphaAdjuster)));
            dl.setVerticalGroup(dl.createSequentialGroup()
                    .addWidget(cs)
                    .addGap(DialogLayout.MEDIUM_GAP)
                    .addWidget(testDisplay).addGap(0).addWidget(testDisplay2)
                    .addGap(DialogLayout.MEDIUM_GAP)
                    .addGroup(dl.createParallelGroup(labelUse2D, btnUse2D))
                    .addGroup(dl.createParallelGroup(labelUseLabels, btnUseLabels))
                    .addGroup(dl.createParallelGroup(labelShowPreview, btnShowPreview))
                    .addGroup(dl.createParallelGroup(labelShowHexEditField, btnShowHexEditField))
                    .addGroup(dl.createParallelGroup(labelShowNativeAdjuster, btnShowNativeAdjuster))
                    .addGroup(dl.createParallelGroup(labelShowRGBAdjuster, btnShowRGBAdjuster))
                    .addGroup(dl.createParallelGroup(labelShowAlphaAdjuster, btnShowAlphaAdjuster)));

            setTheme("colorSelectorDemoFrame");
            setTitle("Color Selector Demo");
            add(dl);
        }
    }
}
