using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using XNATWL.IO;
using XNATWL.Model;
using XNATWL.Renderer.XNA;
using XNATWL.TextAreaModel;
using XNATWL.Theme;

namespace XNATWL.Test
{
    public class SimpleTest
    {
        public static String WITH_TITLE = "resizableframe-title";
        public static String WITHOUT_TITLE = "resizableframe";

        public class StyleItem
        {
            public String Theme;
            public String Name;

            public StyleItem(String theme, String name)
            {
                this.Theme = theme;
                this.Name = name;
            }

            public override String ToString()
            {
                return Name;
            }
        }

        private static String[] THEME_FILES = {
            "simple_demo.xml",
            "guiTheme.xml"
        };

        protected DisplayMode desktopMode;
        protected bool closeRequested;
        protected ThemeManager theme;
        protected XNARenderer renderer;
        protected internal GUI gui;
        protected PersistentIntegerModel curThemeIdx;
        protected Preferences preferences;
        private GraphicsDevice graphicsDevice;
        private FileSystemObject themeRootFso;

        public SimpleTest(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            this.preferences = new Preferences("");
            this.themeRootFso = new IO.FileSystemObject(IO.FileSystemObject.FileSystemObjectType.Directory, "D:\\FortressCraft\\XNATWL\\XNATWL\\XNATWL.Test\\Theme\\");
            curThemeIdx = new PersistentIntegerModel(
                    this.preferences,
                    "currentThemeIndex", 0, THEME_FILES.Length, 0);
            this.Setup();
        }

        private void LoadTheme()
        {
            //renderer.syncViewportSize();
            Console.WriteLine("width=" + renderer.Width + " height=" + renderer.Height);

            long startTime = DateTime.Now.Ticks;
            // NOTE: this code destroys the old theme manager (including it's cache context)
            // after loading the new theme with a new cache context.
            // This allows easy reloading of a theme for development.
            // If you want fast theme switching without reloading then use the existing
            // cache context for loading the new theme and don't destroy the old theme.
            ThemeManager newTheme = ThemeManager.CreateThemeManager(new FileSystemObject(this.themeRootFso, THEME_FILES[curThemeIdx.Value]), renderer);
            long duration = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Loaded theme in " + (duration / 1000) + " us");

            if (theme != null)
            {
                theme.Destroy();
            }
            theme = newTheme;

            gui.SetSize();
            gui.ApplyTheme(theme);
            gui.SetBackground(theme.GetImageNoWarning("gui.background"));
        }

        public void Setup()
        {
            RootPane root = new RootPane();
            renderer = new XNARenderer(this.graphicsDevice);
            //renderer = new LWJGLEffectsRenderer();
            renderer.SetUseSWMouseCursors(true);
            gui = new GUI(root, renderer);

            LoadTheme();

            WidgetsDemoDialog1 dlg1 = new WidgetsDemoDialog1();
            root._desk.Add(dlg1);
            dlg1.AdjustSize();
            dlg1.Center(0.35f, 0.5f);

            GraphDemoDialog1 fMS = new GraphDemoDialog1();
            root._desk.Add(fMS);
            fMS.AdjustSize();
            fMS.Center(1f, 0.8f);

            TextAreaDemoDialog1 fInfo = new TextAreaDemoDialog1(new FileSystemObject(this.themeRootFso, "license.html"));
            root._desk.Add(fInfo);
            fInfo.SetSize(gui.GetWidth() * 2 / 3, gui.GetHeight() * 2 / 3);
            fInfo.Center(0.5f, 0.5f);
            fInfo.AddCloseCallback();

            TextAreaDemoDialog2 fTextAreaTest = new TextAreaDemoDialog2();
            fTextAreaTest.SetHardVisible(false);
            root._desk.Add(fTextAreaTest);
            fTextAreaTest.SetSize(gui.GetWidth() * 2 / 3, gui.GetHeight() * 2 / 3);
            fTextAreaTest.Center(0.5f, 0.5f);
            fTextAreaTest.AddCloseCallback();

            ScrollPaneDemoDialog1 fScroll = new ScrollPaneDemoDialog1();
            root._desk.Add(fScroll);
            fScroll.AdjustSize();
            fScroll.Center(0f, 0f);
            fScroll.AddCloseCallback();
            fScroll.centerScrollPane();

            PropertySheetDemoDialog fPropertySheet = new PropertySheetDemoDialog();
            fPropertySheet.SetHardVisible(false);
            root._desk.Add(fPropertySheet);
            fPropertySheet.SetSize(400, 400);
            fPropertySheet.Center(0f, 0.25f);
            fPropertySheet.AddCloseCallback();

            ColorSelectorDemoDialog1 fCS = new ColorSelectorDemoDialog1();
            fCS.SetHardVisible(false);
            root._desk.Add(fCS);
            fCS.AdjustSize();
            fCS.Center(0.5f, 0.5f);
            fCS.AddCloseCallback();

            TableDemoDialog1 fTable = new TableDemoDialog1();
            root._desk.Add(fTable);
            fTable.AdjustSize();
            fTable.Center(0f, 0.5f);
            
            TreeTableDemoDialog1 fTreeTable = new TreeTableDemoDialog1(this.preferences);
            root._desk.Add(fTreeTable);
            fTable.AdjustSize();
            fTable.Center(0f, 0.5f);
            //fTable.addCloseCallback();
            /*GraphDemoDialog1 fMS = new GraphDemoDialog1();
            root.desk.add(fMS);
            fMS.adjustSize();
            fMS.center(1f, 0.8f);

            TableDemoDialog1 fTable = new TableDemoDialog1();
            root.desk.add(fTable);
            fTable.adjustSize();
            fTable.center(0f, 0.5f);
            //fTable.addCloseCallback();


            ColorSelectorDemoDialog1 fCS = new ColorSelectorDemoDialog1();
            fCS.setHardVisible(false);
            root.desk.add(fCS);
            fCS.adjustSize();
            fCS.center(0.5f, 0.5f);
            fCS.addCloseCallback();

            PopupWindow settingsDlg = new PopupWindow(root);
            VideoSettings settings = new VideoSettings(
                    this.preferences,
                    desktopMode);
            settingsDlg.setTheme("settingdialog");
            settingsDlg.add(settings);
            settingsDlg.setCloseOnClickedOutside(false);
            settings.setTheme("settings");*/
            /*settings.addCallback(new CallbackWithReason<VideoSettings.CallbackReason>() {
                public void callback(VideoSettings.CallbackReason reason) {
                    vidDlgCloseReason = reason;
                    settingsDlg.closePopup();
                }
            });*/

            root.AddButton("Exit", () => {
                closeRequested = true;
            });
            root.AddButton("Info", "Shows TWL license", () => {
                if (fInfo.IsVisible())
                {
                    fInfo.Hide();
                }
                else
                {
                    fInfo.Show();
                }
            }).SetTooltipContent(MakeComplexTooltip());
            root.AddButton("TA", "Shows a text area test", () => {
                if (fTextAreaTest.IsVisible())
                {
                    fTextAreaTest.Hide();
                }
                else
                {
                    fTextAreaTest.Show();
                }
            });
            root.AddButton("Toggle Theme", () => {
                curThemeIdx.Value = ((curThemeIdx.Value + 1) % THEME_FILES.Length);
                try
                {
                    System.Diagnostics.Debug.WriteLine("Load theme: " + curThemeIdx.Value);
                    LoadTheme();
                }
                catch (IOException ex)
                {
                    System.Diagnostics.Debug.Write(ex);
                }
            });
            root.AddButton("ScrollPane", () => {
                if (fScroll.IsVisible())
                {
                    fScroll.Hide();
                }
                else
                {
                    fScroll.Show();
                }
            });
            root.AddButton("Properties", () => {
                if (fPropertySheet.IsVisible())
                {
                    fPropertySheet.Hide();
                }
                else
                {
                    fPropertySheet.Show();
                }
            });
            root.AddButton("Color", () => {
                if (fCS.IsVisible())
                {
                    fCS.Hide();
                }
                else
                {
                    fCS.Show();
                }
            });
            /*if (!isApplet)
            {
                root.addButton("Settings", "Opens a dialog which might be used to change video settings", () => {
                    settings.readSettings();
                    settingsDlg.openPopupCentered();
                });
            }
            root.addButton("Color", () => {
                if (fCS.isVisible())
                {
                    fCS.hide();
                }
                else
                {
                    fCS.show();
                }
            }));*/

            /*root.addButton("Game", new Runnable() {
                public void run() {
                    BlockGame game = new BlockGame();
                    game.setTheme("/blockgame");
                    PopupWindow popup = new PopupWindow(root);
                    popup.setTheme("settingdialog");
                    popup.add(game);
                    popup.openPopupCentered();
                }
            });*/

            //fInfo.requestKeyboardFocus();

            /*while(!Display.isCloseRequested() && !closeRequested) {
                GL11.glClear(GL11.GL_COLOR_BUFFER_BIT);

                gui.update();
                Display.update();

                if(root.reduceLag) {
                    TestUtils.reduceInputLag();
                }

                if(!isApplet && vidDlgCloseReason == VideoSettings.CallbackReason.ACCEPT) {
                    settings.storeSettings();
                    VideoMode vm = settings.getSelectedVideoMode();
                    gui.destroy();
                    renderer.getActiveCacheContext().destroy();
                    Display.destroy();
                    createDisplay(vm);
                    loadTheme();
                }
                vidDlgCloseReason = null;

                if(!Display.isActive()) {
                    gui.clearKeyboardState();
                    gui.clearMouseState();
                }
                
                if(!Display.isVisible()) {
                    try {
                        Thread.sleep(100);
                    } catch (InterruptedException unused) {
                        Thread.currentThread().interrupt();
                    }
                }
            }*/
        }

        private Object MakeComplexTooltip()
        {
            HTMLTextAreaModel tam = new HTMLTextAreaModel();
            tam.SetHtml("Hello <img src=\"twl-logo\" alt=\"logo\"/> World");
            TextArea ta = new TextArea(tam);
            ta.SetTheme("/htmlTooltip");
            return ta;
        }

        internal void Update(GameTime gameTime)
        {
            this.gui.Update(gameTime);
        }

        internal void Draw()
        {
            this.gui.Draw();
        }

        class RootPane : Widget
        {
            protected internal DesktopArea _desk;
            BoxLayout _btnBox;
            BoxLayout _vsyncBox;
            bool _reduceLag = true;

            public RootPane()
            {
                SetTheme("");

                _desk = new DesktopArea();
                _desk.SetTheme("");

                _btnBox = new BoxLayout(BoxLayout.Direction.Horizontal);
                _btnBox.SetTheme("buttonBox");

                _vsyncBox = new BoxLayout(BoxLayout.Direction.Horizontal);
                _vsyncBox.SetTheme("buttonBox");

                SimpleBooleanModel vsyncModel = new SimpleBooleanModel(true);
                /*vsyncModel.addCallback(new Runnable() {
                    public void run() {
                        Display.setVSyncEnabled(vsyncModel.getValue());
                    }
                });*/

                ToggleButton vsyncBtn = new ToggleButton(vsyncModel);
                vsyncBtn.SetTheme("checkbox");
                Label l = new Label("VSync");
                l.SetLabelFor(vsyncBtn);

                _vsyncBox.Add(l);
                _vsyncBox.Add(vsyncBtn);

                Add(_desk);
                Add(_btnBox);
                Add(_vsyncBox);
            }

            public Button AddButton(String text, Action cb)
            {
                Button btn = new Button(text);
                btn.Action += (sender, e) => {
                    cb();
                };
                _btnBox.Add(btn);
                InvalidateLayout();
                return btn;
            }

            public Button AddButton(String text, String ttolTip, Action cb)
            {
                Button btn = AddButton(text, cb);
                btn.SetTooltipContent(ttolTip);
                return btn;
            }

            protected override void Layout()
            {
                _btnBox.AdjustSize();
                _btnBox.SetPosition(0, GetParent().GetHeight() - _btnBox.GetHeight());
                _desk.SetSize(GetParent().GetWidth(), GetParent().GetHeight());
                _vsyncBox.AdjustSize();
                _vsyncBox.SetPosition(
                        GetParent().GetWidth() - _vsyncBox.GetWidth(),
                        GetParent().GetHeight() - _vsyncBox.GetHeight());
            }

            protected override void AfterAddToGUI(GUI gui)
            {
                base.AfterAddToGUI(gui);
                ValidateLayout();
            }

            public override bool HandleEvent(Event evt)
            {
                if (evt.GetEventType() == EventType.KEY_PRESSED &&
                        evt.GetKeyCode() == Event.KEY_L &&
                        (evt.GetModifiers() & Event.MODIFIER_CTRL) != 0 &&
                        (evt.GetModifiers() & Event.MODIFIER_SHIFT) != 0)
                {
                    _reduceLag ^= true;
                    Console.WriteLine("reduceLag = " + _reduceLag);
                }

                return base.HandleEvent(evt);
            }
        }
    }
}
