#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
using Winterday.MonoGame.Graphics;
using Winterday.MonoGame.Graphics.Widgets;
#endregion

namespace Sandbox.Windows
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager _graphics;
        KyuBatch _batch;

        Graph _graph;

        KeyboardState _lastKeyState;

        public Game1()
            : base()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _batch = new KyuBatch(GraphicsDevice);
            _graph = new Graph(GraphicsDevice, GraphType.Bars, 50);

            var vp = GraphicsDevice.Viewport;
            
            _graph.Position = new Vector2(vp.Width / 2, 0);
            _graph.Size = new Vector2(vp.Width / 2, vp.Height);

            var rand = new Random();

            for (int i = 0; i < 50; i++)
            {
                _graph[i] = (float)rand.NextDouble();
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            var keyState = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyState.IsKeyDown(Keys.Escape))
                Exit();

            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (keyState.IsKeyUp(Keys.R) && _lastKeyState.IsKeyDown(Keys.R))
            {
                var rand = new Random();

                for (int i = 0; i < _graph.PointCount; i++)
                {
                    _graph[i] = (float)rand.NextDouble();
                }
            }


            _graph.Update(delta);

            base.Update(gameTime);

            _lastKeyState = keyState;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            var piOver4 = (float)Math.PI / 4;

            _batch.Begin();
            _batch.Draw(new Vector3(128, 128, 0), Vector3.Zero, new Vector2(64, 64), Vector2.One, Vector4.Zero, Color.White);
            _batch.Draw(new Vector3(128, 256, 0), new Vector3(piOver4, piOver4, piOver4), new Vector2(64, 64), Vector2.One, Vector4.Zero, Color.White);
            _batch.End();

            _graph.Draw(Color.Black);

            base.Draw(gameTime);
        }
    }
}
