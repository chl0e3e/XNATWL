using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static XNATWL.TabbedPane;
using XNATWL.TextAreaModel;
using XNATWL;
using XNATWL.IO;
using System.IO;

namespace XNATWL.Test
{
    public class TextAreaDemoDialog1 : FadeFrame
    {
        public TextAreaDemoDialog1(FileSystemObject fso)
        {
            HTMLTextAreaModel tam = new HTMLTextAreaModel();
            FileStream fileStream = File.OpenRead(fso.Path);
            tam.ParseXHTML(fileStream);
            fileStream.Close();

            Button btnX = new Button("Blub!");
            btnX.setTheme("/button");

            TextArea textArea = new TextArea(tam);
            textArea.setTheme("textarea");
            textArea.registerWidget("test", btnX);
            textArea.setDefaultStyleSheet();

            ScrollPane scrollPane2 = new ScrollPane(textArea);
            scrollPane2.setTheme("scrollpane");
            scrollPane2.setFixed(ScrollPane.Fixed.HORIZONTAL);

            setTheme("licenseFrame");
            setTitle("TWL License");
            add(scrollPane2);
        }
    }
}
