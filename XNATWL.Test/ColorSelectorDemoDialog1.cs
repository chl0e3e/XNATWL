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
        ColorSelector _cs;
        public ColorSelectorDemoDialog1()
        {
            _cs = new ColorSelector(new ColorSpaceHSL());

            ToggleButton btnUse2D = new ToggleButton();
            btnUse2D.SetActive(_cs.IsUseColorArea2D());
            btnUse2D.Action += (sender, e) =>
            {
                _cs.SetUseColorArea2D(btnUse2D.IsActive());
            };
            Label labelUse2D = new Label("Use 2D color area");
            labelUse2D.SetLabelFor(btnUse2D);

            ToggleButton btnUseLabels = new ToggleButton();
            btnUseLabels.SetActive(_cs.IsUseColorArea2D());
            btnUseLabels.Action += (sender, e) =>
            {
                _cs.SetUseLabels(btnUseLabels.IsActive());
            };
            Label labelUseLabels = new Label("show labels for adjusters");
            labelUseLabels.SetLabelFor(btnUseLabels);

            ToggleButton btnShowPreview = new ToggleButton();
            btnShowPreview.SetActive(_cs.IsShowPreview());
            btnShowPreview.Action += (sender, e) =>
            {
                _cs.SetShowPreview(btnShowPreview.IsActive());
            };
            Label labelShowPreview = new Label("show color preview");
            labelShowPreview.SetLabelFor(btnShowPreview);

            ToggleButton btnShowHexEditField = new ToggleButton();
            btnShowHexEditField.SetActive(_cs.IsShowHexEditField());
            btnShowHexEditField.Action += (sender, e) =>
            {
                _cs.SetShowHexEditField(btnShowHexEditField.IsActive());
            };
            Label labelShowHexEditField = new Label("show hex edit field");
            labelShowHexEditField.SetLabelFor(btnShowHexEditField);

            ToggleButton btnShowNativeAdjuster = new ToggleButton();
            btnShowNativeAdjuster.Action += (sender, e) =>
            {
                _cs.SetShowNativeAdjuster(btnShowNativeAdjuster.IsActive());
            };
            btnShowNativeAdjuster.SetActive(_cs.IsShowNativeAdjuster());
            Label labelShowNativeAdjuster = new Label("show native (HSL) adjuster");
            labelShowNativeAdjuster.SetLabelFor(btnShowNativeAdjuster);

            ToggleButton btnShowRGBAdjuster = new ToggleButton();
            btnShowRGBAdjuster.Action += (sender, e) =>
            {
                _cs.SetShowRGBAdjuster(btnShowRGBAdjuster.IsActive());
            };
            btnShowRGBAdjuster.SetActive(_cs.IsShowRGBAdjuster());

            Label labelShowRGBAdjuster = new Label("show RGB adjuster");
            labelShowRGBAdjuster.SetLabelFor(btnShowRGBAdjuster);

            ToggleButton btnShowAlphaAdjuster = new ToggleButton();
            btnShowAlphaAdjuster.SetActive(_cs.IsShowAlphaAdjuster());
            btnShowAlphaAdjuster.Action += (sender, e) =>
            {
                _cs.SetShowAlphaAdjuster(btnShowAlphaAdjuster.IsActive());
            };

            Label labelShowAlphaAdjuster = new Label("show alpha adjuster");
            labelShowAlphaAdjuster.SetLabelFor(btnShowAlphaAdjuster);

            TintAnimator tintAnimator = new TintAnimator(new TintAnimator.GUITimeSource(this));

            Label testDisplay = new Label("This is a test display");
            testDisplay.SetTheme("testDisplay");
            testDisplay.GetTintAnimator(tintAnimator);

            Label testDisplay2 = new Label("This is a test display");
            testDisplay2.SetTheme("testDisplay2");
            testDisplay2.GetTintAnimator(tintAnimator);

            _cs.ColorChanged += (x, y) =>
            {
                tintAnimator.SetColor(_cs.GetColor());
            };
            tintAnimator.SetColor(_cs.GetColor());

            DialogLayout dl = new DialogLayout();
            dl.SetHorizontalGroup(dl.CreateParallelGroup()
                    .AddWidgets(_cs, testDisplay, testDisplay2)
                    .AddGroup(dl.CreateSequentialGroup().AddGap().AddWidgets(labelUse2D, btnUse2D))
                    .AddGroup(dl.CreateSequentialGroup().AddGap().AddWidgets(labelUseLabels, btnUseLabels))
                    .AddGroup(dl.CreateSequentialGroup().AddGap().AddWidgets(labelShowPreview, btnShowPreview))
                    .AddGroup(dl.CreateSequentialGroup().AddGap().AddWidgets(labelShowHexEditField, btnShowHexEditField))
                    .AddGroup(dl.CreateSequentialGroup().AddGap().AddWidgets(labelShowNativeAdjuster, btnShowNativeAdjuster))
                    .AddGroup(dl.CreateSequentialGroup().AddGap().AddWidgets(labelShowRGBAdjuster, btnShowRGBAdjuster))
                    .AddGroup(dl.CreateSequentialGroup().AddGap().AddWidgets(labelShowAlphaAdjuster, btnShowAlphaAdjuster)));
            dl.SetVerticalGroup(dl.CreateSequentialGroup()
                    .AddWidget(_cs)
                    .AddGap(DialogLayout.MEDIUM_GAP)
                    .AddWidget(testDisplay).AddGap(0).AddWidget(testDisplay2)
                    .AddGap(DialogLayout.MEDIUM_GAP)
                    .AddGroup(dl.CreateParallelGroup(labelUse2D, btnUse2D))
                    .AddGroup(dl.CreateParallelGroup(labelUseLabels, btnUseLabels))
                    .AddGroup(dl.CreateParallelGroup(labelShowPreview, btnShowPreview))
                    .AddGroup(dl.CreateParallelGroup(labelShowHexEditField, btnShowHexEditField))
                    .AddGroup(dl.CreateParallelGroup(labelShowNativeAdjuster, btnShowNativeAdjuster))
                    .AddGroup(dl.CreateParallelGroup(labelShowRGBAdjuster, btnShowRGBAdjuster))
                    .AddGroup(dl.CreateParallelGroup(labelShowAlphaAdjuster, btnShowAlphaAdjuster)));

            SetTheme("colorSelectorDemoFrame");
            SetTitle("Color Selector Demo");
            Add(dl);
        }
    }
}
