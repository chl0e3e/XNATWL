using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace XNATWL.Test
{
    internal class TestGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _batch;
        private KeyboardState _oldKeyState;
        private GamePadState _oldPadState;
        private SpriteFont _font;

        private Texture2D _circleSprite;
        private Texture2D _groundSprite;

        // Simple camera controls
        private Matrix _view;
        private Vector2 _cameraPosition;
        private Vector2 _screenCenter;

        // Farseer expects objects to be scaled to MKS (meters, kilos, seconds)
        // 1 meters equals 64 pixels here
        // (Objects should be scaled to be between 0.1 and 10 meters in size)
        private const float MeterInPixels = 64f;

        //private GUI twlGui;
        //private XNARenderer twlRenderer;
        private SimpleTest simpleTest;

        public TestGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1800;
            _graphics.PreferredBackBufferHeight = 1000;

            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Initialize camera controls
            _view = Matrix.Identity;
            _cameraPosition = Vector2.Zero;

            _screenCenter = new Vector2(_graphics.GraphicsDevice.Viewport.Width / 2f,
                                                _graphics.GraphicsDevice.Viewport.Height / 2f);

            _batch = new SpriteBatch(_graphics.GraphicsDevice);
            //_font = Content.Load<SpriteFont>("font");

            // Load sprites
            //_circleSprite = Content.Load<Texture2D>("circleSprite"); //  96px x 96px => 1.5m x 1.5m
            //_groundSprite = Content.Load<Texture2D>("groundSprite"); // 512px x 64px =>   8m x 1m

            /*this.twlRenderer = new XNARenderer(_graphics.GraphicsDevice);
            this.twlGui = new GUI(new ChatDemo(), this.twlRenderer);

            ThemeManager theme = ThemeManager.createThemeManager(new IO.FileSystemObject(IO.FileSystemObject.FileSystemObjectType.FILE, "D:\\FortressCraft\\XNATWL\\XNATWL\\XNATWL.Test\\Theme\\chat.xml"), this.twlRenderer);
            this.twlGui.applyTheme(theme);*/
            this.simpleTest = new SimpleTest(_graphics.GraphicsDevice);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            HandleGamePad();
            HandleKeyboard();

            base.Update(gameTime);

            this.simpleTest.update(gameTime);
        }

        private void HandleGamePad()
        {
            GamePadState padState = GamePad.GetState(0);

            if (padState.IsConnected)
            {
                if (padState.Buttons.Back == ButtonState.Pressed)
                    Exit();

                _cameraPosition.X -= padState.ThumbSticks.Right.X;
                _cameraPosition.Y += padState.ThumbSticks.Right.Y;

                _view = Matrix.CreateTranslation(new Vector3(_cameraPosition - _screenCenter, 0f)) * Matrix.CreateTranslation(new Vector3(_screenCenter, 0f));

                _oldPadState = padState;
            }
        }

        private void HandleKeyboard()
        {
            KeyboardState state = Keyboard.GetState();

            // Switch between circle body and camera control
            if (state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift))
            {
                // Move camera
                if (state.IsKeyDown(Keys.A))
                    _cameraPosition.X += 1.5f;

                if (state.IsKeyDown(Keys.D))
                    _cameraPosition.X -= 1.5f;

                if (state.IsKeyDown(Keys.W))
                    _cameraPosition.Y += 1.5f;

                if (state.IsKeyDown(Keys.S))
                    _cameraPosition.Y -= 1.5f;

                _view = Matrix.CreateTranslation(new Vector3(_cameraPosition - _screenCenter, 0f)) *
                        Matrix.CreateTranslation(new Vector3(_screenCenter, 0f));
            }
            else
            {
                // other keyboard updates
            }

            if (state.IsKeyDown(Keys.Escape))
                Exit();

            _oldKeyState = state;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //

            /*_batch.Begin();

            // Display instructions
            _batch.DrawString(_font, Text, new Vector2(14f, 14f), Color.Black);
            _batch.DrawString(_font, Text, new Vector2(12f, 12f), Color.White);

            _batch.End();*/

            base.Draw(gameTime);
            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);
            this.simpleTest.draw();
        }
    }
}
