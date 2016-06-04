/* ©2016 Hathor Gaia 
* http://HathorsLove.com
* 
* Source code licensed under GPL-3
* Assets licensed seperately (see LICENSE.md)
*/

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Ruge.ViewportAdapters;
using MonoGame.Ruge.DragonDrop;

namespace OpenSolitaire.Classic
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class OpenSolitaireClassic : Game
    {

        private const string _version = "v 0.9.5";

        private SpriteBatch _spriteBatch;

        private BoxingViewportAdapter _viewport;

        private const int _windowWidth = 1035;
        private const int _windowHeight = 666;

        private Texture2D _cardSlot;
        private Texture2D _cardBack;
        private Texture2D _refreshMe;
        private Texture2D _newGame;
        private Texture2D _metaSmug;
        private Texture2D _debug;
        private Rectangle _newGameRect;
        private Rectangle _debugRect;
        private Color _newGameColor;
        private Color _debugColor;

        private TableClassic _table;

        private DragonDrop<IDragonDropItem> _dragonDrop;

        private MouseState _prevMouseState;
        private SpriteFont _debugFont;

        private List<SoundEffect> _soundFx;


        public OpenSolitaireClassic()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = _windowWidth,
                PreferredBackBufferHeight = _windowHeight
            };

            // set the screen resolution
            Content.RootDirectory = "Content";

            Window.Title = "Open Solitaire Classic";
            Window.AllowUserResizing = true;

            IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {

            _viewport = new BoxingViewportAdapter(Window, GraphicsDevice, _windowWidth, _windowHeight);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {

            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _cardSlot = Content.Load<Texture2D>("card_slot");
            _cardBack = Content.Load<Texture2D>("card_back_green");
            _refreshMe = Content.Load<Texture2D>("refresh");
            _newGame = Content.Load<Texture2D>("new_game");
            _metaSmug = Content.Load<Texture2D>("smug-logo");
            _debug = Content.Load<Texture2D>("debug");
            _debugFont = Content.Load<SpriteFont>("Arial");

            _soundFx = new List<SoundEffect> {
                Content.Load<SoundEffect>("table-animation"),
                Content.Load<SoundEffect>("card-parent"),
                Content.Load<SoundEffect>("card-play"),
                Content.Load<SoundEffect>("card-restack"),
                Content.Load<SoundEffect>("game-win")
            };

            _dragonDrop = new DragonDrop<IDragonDropItem>(this, _viewport);


            // table creates a fresh table.deck
            _table = new TableClassic(_spriteBatch, _dragonDrop, _cardBack, _cardSlot, 20, 30, _soundFx);

            // load up the card assets for the new deck
            foreach (var card in _table.DrawPile.Cards)
            {

                var location = card.Suit.ToString() + card.Rank.ToString();
                card.SetTexture(Content.Load<Texture2D>(location));

            }

            _table.InitializeTable();

            _table.SetTable();

            Components.Add(_dragonDrop);

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent() { }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var mouseState = Mouse.GetState();
            var point = _viewport.PointToScreen(mouseState.X, mouseState.Y);

            _newGameRect = new Rectangle(310, 20, _newGame.Width, _newGame.Height);
            _newGameColor = Color.White;


            if (_table.IsSetup && !_table.IsSnapAnimating)
            {

                if (_newGameRect.Contains(point))
                {

                    _newGameColor = Color.Aqua;

                    if (mouseState.LeftButton == ButtonState.Pressed
                        && _prevMouseState.LeftButton == ButtonState.Released)
                    {

                        _table.NewGame();


                        // load up the card assets for the new deck
                        foreach (var card in _table.DrawPile.Cards)
                        {

                            var location = card.Suit.ToString() + card.Rank.ToString();
                            card.SetTexture(Content.Load<Texture2D>(location));

                        }

                        _table.SetTable();

                    }
                }

#if DEBUG
                _debugRect = new Rectangle(310, 80, _newGame.Width, _newGame.Height);
                _debugColor = Color.White;


                if (_debugRect.Contains(point))
                {

                    _debugColor = Color.Aqua;

                    if (mouseState.LeftButton == ButtonState.Pressed && _prevMouseState.LeftButton == ButtonState.Released)
                    {

                        foreach (var stack in _table.Stacks) stack.Debug();

                    }
                }
#endif
            }

            _prevMouseState = mouseState;

            _table.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.Clear(Color.SandyBrown);

            _spriteBatch.Begin(transformMatrix: _viewport.GetScaleMatrix(), samplerState: SamplerState.LinearWrap);

            var logoVect = new Vector2(10, _windowHeight - _metaSmug.Height - 10);

            // todo: please comment out the line below if you're going to distribute the game
            _spriteBatch.Draw(_metaSmug, logoVect, Color.White);

            _spriteBatch.Draw(_newGame, _newGameRect, _newGameColor);


            _spriteBatch.Draw(_refreshMe, new Vector2(35, 50), Color.White);



            var versionSize = _debugFont.MeasureString(_version);
            var versionPos = new Vector2(_windowWidth - versionSize.X - 10, _windowHeight - versionSize.Y - 10);
            _spriteBatch.DrawString(_debugFont, _version, versionPos, Color.Black);




#if DEBUG

            foreach (var stack in _table.Stacks)
            {

                var slot = stack.Slot;
                var textWidth = _debugFont.MeasureString(slot.Name);
                var x = slot.Position.X + slot.Texture.Width / 2f - textWidth.X / 2f;
                var y = slot.Position.Y - 16;
                var textPos = new Vector2(x, y);

                _spriteBatch.DrawString(_debugFont, slot.Name, textPos, Color.Black);

            }

            _spriteBatch.Draw(_debug, _debugRect, _debugColor);
#endif



            _table.Draw(gameTime);


            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
