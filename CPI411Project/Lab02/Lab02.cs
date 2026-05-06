using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lab02
{
    public class Lab02 : Game
    {
        GraphicsDeviceManager graphics;
        Effect effect;

        float angle;
        float distance = 2.0f;

        Matrix world = Matrix.Identity;
        Matrix view;
        Matrix projection;

        VertexPositionTexture[] vertices =
        {
            new VertexPositionTexture(new Vector3(0, 1, 0),  new Vector2(0.5f, 0)),
            new VertexPositionTexture(new Vector3(1, 0, 0),  new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(-1, 0, 0), new Vector2(0, 1))
        };

        public Lab02()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // ok to keep HiDef, but your shader uses level_9_1 anyway (safe)
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
        }

        protected override void LoadContent()
        {
            
            effect = Content.Load<Effect>("SimplestShader");


            effect.Parameters["MyTexture"].SetValue(Content.Load<Texture2D>("logo_mg"));
            effect.CurrentTechnique = effect.Techniques["MyTechnique"];
        }

        protected override void Update(GameTime gameTime)
        {
            var kb = Keyboard.GetState();
            if (kb.IsKeyDown(Keys.Escape)) Exit();

            
            if (kb.IsKeyDown(Keys.Left)) angle += 0.02f;
            if (kb.IsKeyDown(Keys.Right)) angle -= 0.02f;

            
            if (kb.IsKeyDown(Keys.Up)) distance += 0.02f;
            if (kb.IsKeyDown(Keys.Down)) distance -= 0.02f;

           
            distance = MathHelper.Clamp(distance, 0.5f, 20f);

           
            Vector3 offset = new Vector3(
                (float)System.Math.Cos(angle),
                (float)System.Math.Sin(angle),
                0f);

          

            effect.Parameters["offset"]?.SetValue(offset);

            
            projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(90f),
                GraphicsDevice.Viewport.AspectRatio,
                0.1f, 100f);

            
            Vector3 cameraPosition = distance * new Vector3(
                (float)System.Math.Sin(angle),
                0f,
                (float)System.Math.Cos(angle));

            view = Matrix.CreateLookAt(
                cameraPosition,
                Vector3.Zero,
                Vector3.Up);

            effect.Parameters["World"]?.SetValue(world);
            effect.Parameters["View"]?.SetValue(view);
            effect.Parameters["Projection"]?.SetValue(projection);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives(
                    PrimitiveType.TriangleList,
                    vertices,
                    0,
                    1); 
            }
            base.Draw(gameTime);
        }
    }
}
