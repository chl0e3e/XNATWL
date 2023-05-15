namespace XNATWL.Test
{
    public class ScrollPaneDemoDialog1 : FadeFrame
    {
        private ScrollPane _scrollPane;

        public ScrollPaneDemoDialog1()
        {
            Widget scrolledWidget = new Widget();
            scrolledWidget.SetTheme("/scrollPaneDemoContent");

            _scrollPane = new ScrollPane(scrolledWidget);
            _scrollPane.SetTheme("/scrollpane");

            SetTheme("scrollPaneDemoDialog1");
            SetTitle("ScrollPane");
            Add(_scrollPane);
        }

        public void centerScrollPane()
        {
            _scrollPane.UpdateScrollbarSizes();
            _scrollPane.SetScrollPositionX(_scrollPane.GetMaxScrollPosX() / 2);
            _scrollPane.SetScrollPositionY(_scrollPane.GetMaxScrollPosY() / 2);
        }
    }
}
