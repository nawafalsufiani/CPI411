using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.DirectWrite;
using System.Linq;


//using SharpDX.Direct2D1;
//using SharpDX.Direct3D9;

namespace Change2DImageColorHlsl
{
    public class ChangeImageColorPractice : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont font;
        private Texture2D _image;
        private Effect effect;

        // change these while running
        private Vector4 _tint = new Vector4(1f, 0f, 0f, 1f); // red
        private float _strength = 1f;

        private KeyboardState _prev;

        public ChangeImageColorPractice()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _image = Content.Load<Texture2D>("robot");     // add robot.png to Content
            effect = Content.Load<Effect>("effect");   // ColorTint.fx in Content
            font = Content.Load<SpriteFont>("font");      // add font.spritefont to Content
        }

        protected override void Update(GameTime gameTime)
        {
           
            base.Update(gameTime);
        }

        private bool Pressed(KeyboardState k, Keys key)
            => k.IsKeyDown(key) && !_prev.IsKeyDown(key);

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);


            _spriteBatch.Begin(effect: effect);
            //_spriteBatch.Begin();
            _spriteBatch.Draw(_image, new Vector2(0,0), Color.White); //send it to sheder now wile adding image,position,color
            _spriteBatch.DrawString(font, "Hello MonoGame!", new Vector2(50, 200), Color.Yellow);

            _spriteBatch.End();


           
            _spriteBatch.Begin();
          
            _spriteBatch.DrawString(font, "Hello MonoGame!", new Vector2(50, 200), Color.Yellow);

            _spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}