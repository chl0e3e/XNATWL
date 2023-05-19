using XNATWL.TextAreaModel;

namespace XNATWL.Test
{
    public class TextAreaDemoDialog2 : FadeFrame
    {
        public TextAreaDemoDialog2()
        {
            SimpleTextAreaModel tam = new SimpleTextAreaModel();
            tam.Text = "This is a small test message. It's not too long.\n" +
                    "\tThis is a small test message. It's not too long.\n" +
                    "This\tis a small test message. It's not too long.\n" +
                    "This is\ta small test message. It's not too long.\n" +
                    "This is a\tsmall test message. It's not too long.\n" +
                    "This is a small\ttest message. It's not too long.\n" +
                    "This is a small test\tmessage. It's not too long.\n" +
                    "This is a small test message.\tIt's not too long.\n" +
                    "This is a small test message. It's\tnot too long.\n" +
                    "This is a small test message. It's not\ttoo long.\n" +
                    "This is a small test message. It's not too\tlong.";

            TextArea scrolledWidget2 = new TextArea(tam);
            scrolledWidget2.SetTheme("textarea");

            ScrollPane scrollPane2 = new ScrollPane(scrolledWidget2);
            scrollPane2.SetTheme("scrollpane");
            scrollPane2.SetFixed(ScrollPane.Fixed.HORIZONTAL);

            SetTheme("textAreaTestFrame");
            SetTitle("TextArea tab test");
            Add(scrollPane2);
        }
    }
}
