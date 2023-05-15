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
            btnX.SetTheme("/button");

            TextArea textArea = new TextArea(tam);
            textArea.SetTheme("textarea");
            textArea.RegisterWidget("test", btnX);
            textArea.SetDefaultStyleSheet();

            ScrollPane scrollPane2 = new ScrollPane(textArea);
            scrollPane2.SetTheme("scrollpane");
            scrollPane2.SetFixed(ScrollPane.Fixed.HORIZONTAL);

            SetTheme("licenseFrame");
            SetTitle("TWL License");
            Add(scrollPane2);
        }
    }
}
