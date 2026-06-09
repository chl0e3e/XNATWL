using System;
using Microsoft.Xna.Framework;
using XNATWL;
using XNATWL.IO;
using XNATWL.Renderer.XNA;
using XNATWL.Test;
using XNATWL.Theme;

// Browser FNA: SDL2 backend (Emscripten port) + FNA3D's OpenGL driver (→ WebGL2).
Environment.SetEnvironmentVariable("FNA_PLATFORM_BACKEND", "SDL2");
Environment.SetEnvironmentVariable("FNA3D_FORCE_DRIVER", "OpenGL");

Console.WriteLine("XNATWL.Wasm: starting FNA game");

using var game = new WasmGame();
game.Run();

// An FNA game that hosts the full XNATWL demo (the same dialogs as the desktop SimpleTest harness),
// rendered via XNARenderer onto the WebGL2 canvas. Theme assets are loaded from /Theme in the
// Emscripten VFS (written there by main.js before the game runs).
class WasmGame : Game
{
    // Mirrors the desktop SimpleTest harness. simple_demo.xml includes simple.xml and defines every
    // demo-specific theme; guiTheme.xml is the alternate look. Both ship in wwwroot/Theme.
    private static readonly string[] ThemeFiles = { "simple_demo.xml", "guiTheme.xml" };

    private readonly GraphicsDeviceManager _gdm;
    private XNARenderer _renderer;
    private GUI _gui;
    private ThemeManager _theme;
    private DemoRootPane _root;
    private FileSystemObject _themeRoot;
    private Preferences _prefs;
    private int _themeIdx = 0;

    public WasmGame()
    {
        _gdm = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1280,
            PreferredBackBufferHeight = 720,
        };
        IsMouseVisible = true;
    }

    protected override void LoadContent()
    {
        // In-memory only — never Read()/Write() on wasm (no real filesystem to persist to).
        _prefs = new Preferences("");
        // Theme assets live at /Theme in the Emscripten virtual filesystem (see main.js).
        _themeRoot = new FileSystemObject(FileSystemObject.FileSystemObjectType.Directory, "/Theme");

        _renderer = new XNARenderer(GraphicsDevice);
        _renderer.SetUseSWMouseCursors(true);

        _root = new DemoRootPane();
        _gui = new GUI(_root, _renderer);

        LoadTheme();
        BuildDialogs();

        Console.WriteLine("XNATWL.Wasm: GUI initialized with demo dialogs");
    }

    private void LoadTheme()
    {
        ThemeManager newTheme = ThemeManager.CreateThemeManager(
            new FileSystemObject(_themeRoot, ThemeFiles[_themeIdx]), _renderer);
        if (_theme != null)
        {
            _theme.Destroy();
        }
        _theme = newTheme;

        _gui.SetSize();
        _gui.ApplyTheme(_theme);
        _gui.SetBackground(_theme.GetImageNoWarning("gui.background"));
    }

    // Ports the dialog setup from XNATWL.Test/SimpleTest.Setup().
    private void BuildDialogs()
    {
        DesktopArea desk = _root.Desk;

        WidgetsDemoDialog1 dlgWidgets = new WidgetsDemoDialog1();
        desk.Add(dlgWidgets);
        dlgWidgets.AdjustSize();
        dlgWidgets.Center(0.35f, 0.5f);

        GraphDemoDialog1 dlgGraph = new GraphDemoDialog1();
        desk.Add(dlgGraph);
        dlgGraph.AdjustSize();
        dlgGraph.Center(1f, 0.8f);

        TextAreaDemoDialog1 dlgInfo = new TextAreaDemoDialog1(new FileSystemObject(_themeRoot, "license.html"));
        desk.Add(dlgInfo);
        dlgInfo.SetSize(_gui.GetWidth() * 2 / 3, _gui.GetHeight() * 2 / 3);
        dlgInfo.Center(0.5f, 0.5f);
        dlgInfo.AddCloseCallback();

        TextAreaDemoDialog2 dlgTextArea = new TextAreaDemoDialog2();
        dlgTextArea.SetHardVisible(false);
        desk.Add(dlgTextArea);
        dlgTextArea.SetSize(_gui.GetWidth() * 2 / 3, _gui.GetHeight() * 2 / 3);
        dlgTextArea.Center(0.5f, 0.5f);
        dlgTextArea.AddCloseCallback();

        ScrollPaneDemoDialog1 dlgScroll = new ScrollPaneDemoDialog1();
        desk.Add(dlgScroll);
        dlgScroll.AdjustSize();
        dlgScroll.Center(0f, 0f);
        dlgScroll.AddCloseCallback();
        dlgScroll.centerScrollPane();

        PropertySheetDemoDialog dlgProps = new PropertySheetDemoDialog();
        dlgProps.SetHardVisible(false);
        desk.Add(dlgProps);
        dlgProps.SetSize(400, 400);
        dlgProps.Center(0f, 0.25f);
        dlgProps.AddCloseCallback();

        ColorSelectorDemoDialog1 dlgColor = new ColorSelectorDemoDialog1();
        dlgColor.SetHardVisible(false);
        desk.Add(dlgColor);
        dlgColor.AdjustSize();
        dlgColor.Center(0.5f, 0.5f);
        dlgColor.AddCloseCallback();

        TableDemoDialog1 dlgTable = new TableDemoDialog1();
        desk.Add(dlgTable);
        dlgTable.AdjustSize();
        dlgTable.Center(0f, 0.5f);

        TreeTableDemoDialog1 dlgTreeTable = new TreeTableDemoDialog1(_prefs);
        desk.Add(dlgTreeTable);
        dlgTreeTable.AdjustSize();
        dlgTreeTable.Center(0.8f, 0.2f);

        _root.AddButton("Info", () => Toggle(dlgInfo));
        _root.AddButton("TA", () => Toggle(dlgTextArea));
        _root.AddButton("ScrollPane", () => Toggle(dlgScroll));
        _root.AddButton("Properties", () => Toggle(dlgProps));
        _root.AddButton("Color", () => Toggle(dlgColor));
        _root.AddButton("Toggle Theme", () =>
        {
            _themeIdx = (_themeIdx + 1) % ThemeFiles.Length;
            LoadTheme();
        });
    }

    private static void Toggle(FadeFrame frame)
    {
        if (frame.IsVisible())
        {
            frame.Hide();
        }
        else
        {
            frame.Show();
        }
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        _gui.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);
        _gui.Draw();
        base.Draw(gameTime);
    }
}

// Root pane: a DesktopArea hosting the draggable demo frames, plus a bottom button bar that toggles
// the hideable dialogs. Mirrors XNATWL.Test/SimpleTest.RootPane.
class DemoRootPane : Widget
{
    public DesktopArea Desk { get; private set; }
    private readonly BoxLayout _btnBox;

    public DemoRootPane()
    {
        SetTheme("");

        Desk = new DesktopArea();
        Desk.SetTheme("");

        _btnBox = new BoxLayout(BoxLayout.Direction.Horizontal);
        _btnBox.SetTheme("buttonBox");

        Add(Desk);
        Add(_btnBox);
    }

    public Button AddButton(string text, Action cb)
    {
        Button btn = new Button(text);
        btn.Action += (sender, e) => cb();
        _btnBox.Add(btn);
        InvalidateLayout();
        return btn;
    }

    protected override void Layout()
    {
        Desk.SetSize(GetParent().GetWidth(), GetParent().GetHeight());
        _btnBox.AdjustSize();
        _btnBox.SetPosition(0, GetParent().GetHeight() - _btnBox.GetHeight());
    }

    protected override void AfterAddToGUI(GUI gui)
    {
        base.AfterAddToGUI(gui);
        ValidateLayout();
    }
}
