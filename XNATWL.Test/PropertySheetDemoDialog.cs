using System;
using XNATWL.Model;

namespace XNATWL.Test
{
    public class PropertySheetDemoDialog : FadeFrame
    {
        private ScrollPane scrollPane;

        public PropertySheetDemoDialog()
        {
            PropertySheet<string> ps = new PropertySheet<string>();
            ps.setTheme("/table");
            ps.getPropertyList().AddProperty(new SimpleProperty<String>(typeof(string), "Name", "Hugo"));
            SimplePropertyList spl = new SimplePropertyList("Details");
            spl.AddProperty(new SimpleProperty<string>(typeof(string), "City", "Nowhere"));
            ps.getPropertyList().AddProperty(spl);

            scrollPane = new ScrollPane(ps);
            scrollPane.setTheme("/tableScrollPane");
                scrollPane.setFixed(ScrollPane.Fixed.HORIZONTAL);

                setTheme("scrollPaneDemoDialog1");
            setTitle("Property Sheet");
            add(scrollPane);
        }
    }
}
