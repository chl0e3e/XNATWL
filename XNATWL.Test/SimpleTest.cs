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
            public String theme;
            public String name;

            public StyleItem(String theme, String name)
            {
                this.theme = theme;
                this.name = name;
            }

            public override String ToString()
            {
                return name;
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
            this.themeRootFso = new IO.FileSystemObject(IO.FileSystemObject.FileSystemObjectType.DIRECTORY, "D:\\FortressCraft\\XNATWL\\XNATWL\\XNATWL.Test\\Theme\\");
            curThemeIdx = new PersistentIntegerModel(
                    this.preferences,
                    "currentThemeIndex", 0, THEME_FILES.Length, 0);
            this.setup();
        }

        private void loadTheme()
        {
            //renderer.syncViewportSize();
            Console.WriteLine("width=" + renderer.Width + " height=" + renderer.Height);

            long startTime = DateTime.Now.Ticks;
            // NOTE: this code destroys the old theme manager (including it's cache context)
            // after loading the new theme with a new cache context.
            // This allows easy reloading of a theme for development.
            // If you want fast theme switching without reloading then use the existing
            // cache context for loading the new theme and don't destroy the old theme.
            ThemeManager newTheme = ThemeManager.createThemeManager(new FileSystemObject(this.themeRootFso, THEME_FILES[curThemeIdx.Value]), renderer);
            long duration = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Loaded theme in " + (duration / 1000) + " us");

            if (theme != null)
            {
                theme.destroy();
            }
            theme = newTheme;

            gui.setSize();
            gui.applyTheme(theme);
            gui.setBackground(theme.getImageNoWarning("gui.background"));
        }

        public void setup()
        {
            RootPane root = new RootPane();
            renderer = new XNARenderer(this.graphicsDevice);
            //renderer = new LWJGLEffectsRenderer();
            renderer.SetUseSWMouseCursors(true);
            gui = new GUI(root, renderer);

            loadTheme();

            WidgetsDemoDialog1 dlg1 = new WidgetsDemoDialog1();
            root.desk.add(dlg1);
            dlg1.adjustSize();
            dlg1.center(0.35f, 0.5f);

            GraphDemoDialog1 fMS = new GraphDemoDialog1();
            root.desk.add(fMS);
            fMS.adjustSize();
            fMS.center(1f, 0.8f);

            TextAreaDemoDialog1 fInfo = new TextAreaDemoDialog1(new FileSystemObject(this.themeRootFso, "license.html"));
            root.desk.add(fInfo);
            fInfo.setSize(gui.getWidth() * 2 / 3, gui.getHeight() * 2 / 3);
            fInfo.center(0.5f, 0.5f);
            fInfo.addCloseCallback();

            TextAreaDemoDialog2 fTextAreaTest = new TextAreaDemoDialog2();
            fTextAreaTest.setHardVisible(false);
            root.desk.add(fTextAreaTest);
            fTextAreaTest.setSize(gui.getWidth() * 2 / 3, gui.getHeight() * 2 / 3);
            fTextAreaTest.center(0.5f, 0.5f);
            fTextAreaTest.addCloseCallback();

            ScrollPaneDemoDialog1 fScroll = new ScrollPaneDemoDialog1();
            root.desk.add(fScroll);
            fScroll.adjustSize();
            fScroll.center(0f, 0f);
            fScroll.addCloseCallback();
            fScroll.centerScrollPane();
            /*GraphDemoDialog1 fMS = new GraphDemoDialog1();
            root.desk.add(fMS);
            fMS.adjustSize();
            fMS.center(1f, 0.8f);

            TableDemoDialog1 fTable = new TableDemoDialog1();
            root.desk.add(fTable);
            fTable.adjustSize();
            fTable.center(0f, 0.5f);
            //fTable.addCloseCallback();

            PropertySheetDemoDialog fPropertySheet = new PropertySheetDemoDialog();
            fPropertySheet.setHardVisible(false);
            root.desk.add(fPropertySheet);
            fPropertySheet.setSize(400, 400);
            fPropertySheet.center(0f, 0.25f);
            fPropertySheet.addCloseCallback();


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

            root.addButton("Exit", () => {
                closeRequested = true;
            });
            root.addButton("Info", "Shows TWL license", () => {
                if (fInfo.isVisible())
                {
                    fInfo.hide();
                }
                else
                {
                    fInfo.show();
                }
            }).setTooltipContent(makeComplexTooltip());
            root.addButton("TA", "Shows a text area test", () => {
                if (fTextAreaTest.isVisible())
                {
                    fTextAreaTest.hide();
                }
                else
                {
                    fTextAreaTest.show();
                }
            });
            root.addButton("Toggle Theme", () => {
                curThemeIdx.Value = ((curThemeIdx.Value + 1) % THEME_FILES.Length);
                try
                {
                    System.Diagnostics.Debug.WriteLine("Load theme: " + curThemeIdx.Value);
                    loadTheme();
                }
                catch (IOException ex)
                {
                    System.Diagnostics.Debug.Write(ex);
                }
            });
            root.addButton("ScrollPane", () => {
                if (fScroll.isVisible())
                {
                    fScroll.hide();
                }
                else
                {
                    fScroll.show();
                }
            });
            /*if (!isApplet)
            {
                root.addButton("Settings", "Opens a dialog which might be used to change video settings", () => {
                    settings.readSettings();
                    settingsDlg.openPopupCentered();
                });
            }
            root.addButton("Properties", () => {
                if (fPropertySheet.isVisible())
                {
                    fPropertySheet.hide();
                }
                else
                {
                    fPropertySheet.show();
                }
            });
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

        private Object makeComplexTooltip()
        {
            HTMLTextAreaModel tam = new HTMLTextAreaModel();
            tam.setHtml("Hello <img src=\"twl-logo\" alt=\"logo\"/> World");
            TextArea ta = new TextArea(tam);
            ta.setTheme("/htmlTooltip");
            return ta;
        }

        internal void update(GameTime gameTime)
        {
            this.gui.update(gameTime);
        }

        internal void draw()
        {
            this.gui.draw();
        }

        class RootPane : Widget
        {
            protected internal DesktopArea desk;
            BoxLayout btnBox;
            BoxLayout vsyncBox;
            bool reduceLag = true;

            public RootPane()
            {
                setTheme("");

                desk = new DesktopArea();
                desk.setTheme("");

                btnBox = new BoxLayout(BoxLayout.Direction.HORIZONTAL);
                btnBox.setTheme("buttonBox");

                vsyncBox = new BoxLayout(BoxLayout.Direction.HORIZONTAL);
                vsyncBox.setTheme("buttonBox");

                SimpleBooleanModel vsyncModel = new SimpleBooleanModel(true);
                /*vsyncModel.addCallback(new Runnable() {
                    public void run() {
                        Display.setVSyncEnabled(vsyncModel.getValue());
                    }
                });*/

                ToggleButton vsyncBtn = new ToggleButton(vsyncModel);
                vsyncBtn.setTheme("checkbox");
                Label l = new Label("VSync");
                l.setLabelFor(vsyncBtn);

                vsyncBox.add(l);
                vsyncBox.add(vsyncBtn);

                add(desk);
                add(btnBox);
                add(vsyncBox);
            }

            public Button addButton(String text, Action cb)
            {
                Button btn = new Button(text);
                btn.Action += (sender, e) => {
                    cb();
                };
                btnBox.add(btn);
                invalidateLayout();
                return btn;
            }

            public Button addButton(String text, String ttolTip, Action cb)
            {
                Button btn = addButton(text, cb);
                btn.setTooltipContent(ttolTip);
                return btn;
            }

            protected override void layout()
            {
                btnBox.adjustSize();
                btnBox.setPosition(0, getParent().getHeight() - btnBox.getHeight());
                desk.setSize(getParent().getWidth(), getParent().getHeight());
                vsyncBox.adjustSize();
                vsyncBox.setPosition(
                        getParent().getWidth() - vsyncBox.getWidth(),
                        getParent().getHeight() - vsyncBox.getHeight());
            }

            protected override void afterAddToGUI(GUI gui)
            {
                base.afterAddToGUI(gui);
                validateLayout();
            }

            public override bool handleEvent(Event evt)
            {
                if (evt.getEventType() == EventType.KEY_PRESSED &&
                        evt.getKeyCode() == Event.KEY_L &&
                        (evt.getModifiers() & Event.MODIFIER_CTRL) != 0 &&
                        (evt.getModifiers() & Event.MODIFIER_SHIFT) != 0)
                {
                    reduceLag ^= true;
                    Console.WriteLine("reduceLag = " + reduceLag);
                }

                return base.handleEvent(evt);
            }
        }
    }
}
