
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CPI411.SimpleEngine;
//using System.Drawing;

namespace Lab10
{
    public class Lab10 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Effect effect;
        SpriteFont font;
        Matrix world = Matrix.Identity;
        Matrix view = Matrix.CreateLookAt(
            new Vector3(0, 0, 20),
            new Vector3(0, 0, 0),
            Vector3.UnitY);

        Matrix projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(45),
            800f / 600f,
            0.1f,
            100f);

        Vector3 cameraPosition, cameraTarget, lightPosition;

        float angle = 0;
        float angle2 = 0;
        float angleL = 0;
        float angleL2 = 0;
        float distance = 20;

        MouseState preMouse;

        Model model;
        Texture2D texture;

        Matrix lightView;
        Matrix lightProjection;

        Matrix invertCamera;
        ParticleManager particleManager;
        Vector3 particlePosition;
        System.Random random;

        public Lab10()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            cameraTarget = Vector3.Zero;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            effect = Content.Load<Effect>("ParticleShader");
            texture = Content.Load<Texture2D>("fire");
            model = Content.Load<Model>("torus");
            font = Content.Load<SpriteFont>("font");
            random = new System.Random();
            particleManager = new ParticleManager(GraphicsDevice, 100);
            particlePosition = new Vector3(0, 0, 0);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.Left)) angleL += 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Right)) angleL -= 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Up)) angleL2 += 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Down)) angleL2 -= 0.02f;

            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                angle = angle2 = angleL = angleL2 = 0;
                distance = 30;
                cameraTarget = Vector3.Zero;
            }

            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                angle -= (Mouse.GetState().X - preMouse.X) / 100f;
                angle2 += (Mouse.GetState().Y - preMouse.Y) / 100f;
            }

            if (Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                distance += (Mouse.GetState().X - preMouse.X) / 100f;
            }

            if (Mouse.GetState().MiddleButton == ButtonState.Pressed)
            {
                Vector3 viewRight = Vector3.Transform(
                    Vector3.UnitX,
                    Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));

                Vector3 viewUp = Vector3.Transform(
                    Vector3.UnitY,
                    Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));

                cameraTarget -= viewRight * (Mouse.GetState().X - preMouse.X) / 10f;
                cameraTarget += viewUp * (Mouse.GetState().Y - preMouse.Y) / 10f;
            }

            preMouse = Mouse.GetState();

            cameraPosition = Vector3.Transform(
                new Vector3(0, 0, distance),
                Matrix.CreateRotationX(angle2) *
                Matrix.CreateRotationY(angle) *
                Matrix.CreateTranslation(cameraTarget));

            view = Matrix.CreateLookAt(
                cameraPosition,
                cameraTarget,
                Vector3.Transform(
                    Vector3.UnitY,
                    Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle)));

            lightPosition = Vector3.Transform(
                new Vector3(0, 0, 10),
                Matrix.CreateRotationX(angleL2) * Matrix.CreateRotationY(angleL));

            lightView = Matrix.CreateLookAt(lightPosition, Vector3.Zero, Vector3.Up);
            lightProjection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver2, 1f, 1f, 100f);

            if (Keyboard.GetState().IsKeyDown(Keys.P))
            {
                Particle particle = particleManager.getNext();
                particle.Position = particlePosition;

                particle.Velocity = new Vector3(
                    (float)(random.NextDouble() * 2.0 - 1.0f),
                    (float)(random.NextDouble() * 2.0f),
                    (float)(random.NextDouble() * 2.0 - 1.0f));

                particle.Acceleration = new Vector3(0, -0.8f, 0);
                particle.MaxAge = 1.0f;
                particle.Init();
            }

            particleManager.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            invertCamera = Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = new DepthStencilState();

            model.Draw(world, view, projection);

            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            effect.Parameters["World"].SetValue(world);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            effect.Parameters["InverseCamera"].SetValue(invertCamera);
            effect.Parameters["Texture"].SetValue(texture);

            effect.CurrentTechnique = effect.Techniques[0];
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                particleManager.Draw(GraphicsDevice);
            }


            spriteBatch.Begin();

            spriteBatch.DrawString(
                font,
                "Press P to show particles",
                new Vector2(20, 20),
                Color.White
            );

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}