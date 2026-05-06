using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lab08A
{
    public class Lab08A : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D texture;
        private Texture2D filter;
        private Effect effect;

        public Lab08A()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // texture = Content.Load<Texture2D>("Honolulu");
            texture = Content.Load<Texture2D>("bunny");
            effect = Content.Load<Effect>("ImageProcessing");
           // filter = Content.Load<Texture2D>("filter");

            effect.Parameters["modelTexture"].SetValue(texture);
            effect.Parameters["imageWidth"].SetValue((float)texture.Width);
            effect.Parameters["imageHeight"].SetValue((float)texture.Height);

            Matrix projection = Matrix.CreateOrthographicOffCenter(
                0, texture.Width,
                texture.Height, 0,
                0, 1);

            Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);

            effect.Parameters["MatrixTransform"].SetValue(halfPixelOffset * projection);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(0, null, null, null, null, effect);

            _spriteBatch.Draw(
                texture, Vector2.Zero, null,Color.White, 0f,Vector2.Zero, 0.5f,SpriteEffects.None, 0f);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}