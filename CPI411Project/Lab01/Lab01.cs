using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lab01
{
    public class Lab01 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Effect _effect;

        // IMPORTANT: VertexPositionTexture for texture mapping
        private VertexPositionTexture[] _vertices =
        {
            // Position in clip space (-1..1), UV in (0..1)
            new VertexPositionTexture(new Vector3( 0f,  0.8f, 0f), new Vector2(0.5f, 0f)),
            new VertexPositionTexture(new Vector3( 0.8f, -0.6f, 0f), new Vector2(1f,   1f)),
            new VertexPositionTexture(new Vector3(-0.8f, -0.6f, 0f), new Vector2(0f,   1f)),
        };

        public Lab01()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load the effect
            _effect = Content.Load<Effect>("SimpleColorShader");

            // Set shader parameter MyTexture using the handout syntax
            _effect.Parameters["MyTexture"].SetValue(Content.Load<Texture2D>("logo_mg"));
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives(
                    PrimitiveType.TriangleList,
                    _vertices,
                    0,
                    1
                );
            }

            base.Draw(gameTime);
        }
    }
}
