using System;
using XNATWL.Model;

namespace XNATWL.Test
{
    public class PropertySheetDemoDialog : FadeFrame
    {
        private ScrollPane _scrollPane;

        public PropertySheetDemoDialog()
        {
            PropertySheet<string> ps = new PropertySheet<string>();
            ps.SetTheme("/table");
            ps.GetPropertyList().AddProperty(new SimpleProperty<String>(typeof(string), "Name", "Hugo"));
            SimplePropertyList spl = new SimplePropertyList("Details");
            spl.AddProperty(new SimpleProperty<string>(typeof(string), "City", "Nowhere"));
            ps.GetPropertyList().AddProperty(spl);

            _scrollPane = new ScrollPane(ps);
            _scrollPane.SetTheme("/tableScrollPane");
                _scrollPane.SetFixed(ScrollPane.Fixed.HORIZONTAL);

                SetTheme("scrollPaneDemoDialog1");
            SetTitle("Property Sheet");
            Add(_scrollPane);
        }
    }
}
