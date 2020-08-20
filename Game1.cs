using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using MonoGame.Extended.ViewportAdapters;

namespace Repro
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private TiledMapRenderer _tiledMapRenderer;
        private TiledMap _map;
        private OrthographicCamera _camera;

        private TiledMapTileLayer _borders;
        private TiledMapObjectLayer _objects;
        private Vector2 _startPosition;
        private Vector2 _playerPosition;

        private Texture2D _playerTexture;

        private float speed = 100f;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            var vp = GraphicsDevice.Viewport;
            var vpAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, vp.Width, vp.Height);
            _camera = new OrthographicCamera(vpAdapter);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            _playerTexture = Content.Load<Texture2D>("ball");
            _map = Content.Load<TiledMap>("Plazas/sample");
            _tiledMapRenderer = new TiledMapRenderer(GraphicsDevice, _map);
            _borders = _map.GetLayer<TiledMapTileLayer>("Obstacles");
            _objects = _map.GetLayer<TiledMapObjectLayer>("Objects");

            var pos = _objects?.Objects?.FirstOrDefault(o =>
            {
                o.Properties.TryGetValue("player", out string playerPos);
                return int.Parse(playerPos) == 1;
            });
            _startPosition = pos.Position;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            var initialPos = _playerPosition;
            // TODO: Add your update logic here 
            var padState = GamePad.GetState(PlayerIndex.One);
            _tiledMapRenderer.Update(gameTime);
            if (_playerPosition == Vector2.Zero)
            {
                _playerPosition = _startPosition;
            }
            Vector2 finalPos = MovePlayerWithPad(gameTime);
            var height = _borders.TileHeight;
            var width = _borders.TileWidth;
            var tx = (_playerPosition.X - 20f) / width;
            var ty = (_playerPosition.Y + 20f) / height;
            _borders.TryGetTile((ushort)tx, (ushort)ty, out TiledMapTile? tile);
            if (tile.HasValue)
            {
                var rect = new Rectangle(tile.Value.X, tile.Value.Y, width, height);
                if (rect.Contains(_playerPosition))
                {
                    _playerPosition = initialPos;
                }
                else
                {
                    _playerPosition = finalPos;
                }

            }
            else
            {
                _playerPosition = finalPos;
            }
            MoveCamera(_playerPosition);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _camera.LookAt(_playerPosition);
            // TODO: Add your drawing code here
            _spriteBatch.Begin(transformMatrix: _camera.GetViewMatrix(), samplerState: SamplerState.PointClamp);
            _tiledMapRenderer.Draw(_camera.GetViewMatrix());
            _spriteBatch.Draw(_playerTexture, _playerPosition, Color.White);
            _spriteBatch.End();
            base.Draw(gameTime);
        }


        private void MoveCamera(Vector2 finalPos)
        {
            var cameraPos = _camera.WorldToScreen(finalPos);
            _camera.Move(cameraPos);
        }

        private Vector2 MovePlayerWithPad(GameTime gameTime)
        {
            var padState = GamePad.GetState(PlayerIndex.One);
            var dir = padState.ThumbSticks.Left * new Vector2(1.0f, -1.0f);
            var ellapsed = gameTime.ElapsedGameTime.TotalSeconds;
            var finalPos = _playerPosition + dir * ((speed * 0.05f) + (float)ellapsed);
            return finalPos;
        }
    }
}
