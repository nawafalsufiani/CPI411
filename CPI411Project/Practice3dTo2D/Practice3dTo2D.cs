using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Practice3dTo2D
{
    public class Practice3dTo2D : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Model model;
        Matrix world = Matrix.Identity;
        Matrix view;
        Matrix projection;
        Effect effect;

        Vector3 cameraOffset = new Vector3(0, 0, 0);
        Vector3 cameraPosition = new Vector3(0, 0, 10);
        Vector3 lightPosition = new Vector3(1, 1, 1);
        float lightAngle = 0;
        float lightAngle2 = 0;
        float angle = 0;
        float angle2 = 0;
        float distance = 10;

        Vector4 ambient = new Vector4(0, 0, 0, 0);
        float ambientIntensity = 0.1f;
        Vector4 diffuseColor = new Vector4(1, 1, 1, 1);
        Vector3 diffuseLightDirection = new Vector3(1, 1, 1);
        float diffuseIntensity = 0.3f;

        Vector4 specularColor = new Vector4(1, 1, 1, 1);
        float shininess = 10.0f;

        MouseState previousMouseState;


        SpriteFont font;

        RenderTarget2D renderTarget;

        public Practice3dTo2D()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
        }

        protected override void Initialize()
        {


            base.Initialize();
        }

        protected override void LoadContent()
        {

            spriteBatch = new SpriteBatch(GraphicsDevice);
            model = Content.Load<Model>("bunny");
            effect = Content.Load<Effect>("Diffuse");
            font = Content.Load<SpriteFont>("font");

            world = Matrix.Identity;
            view = Matrix.CreateLookAt(new Vector3(0, 0, 20), Vector3.Zero, Vector3.UnitY);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90),
                                        GraphicsDevice.Viewport.AspectRatio,
                                        0.1f, 100);


            renderTarget = new RenderTarget2D(GraphicsDevice,
             2048,
             2048,
             false,
             SurfaceFormat.Single, DepthFormat.Depth24,
             0,
             RenderTargetUsage.PlatformContents);

        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Mouse.GetState().LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Pressed)
            {
                float offsetx = 0.01f * (Mouse.GetState().X - previousMouseState.X);
                float offsety = 0.01f * (Mouse.GetState().Y - previousMouseState.Y);
                angle += offsetx;
                angle2 += offsety;
            }

            Vector3 camera = Vector3.Transform(
                new Vector3(0, 0, 20), Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));
            view = Matrix.CreateLookAt(camera, Vector3.Zero, Vector3.UnitY);

            effect.Parameters["View"].SetValue(view);

            previousMouseState = Mouse.GetState();


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {


            // PASS 1: draw 3D model into RenderTarget2D
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            effect.CurrentTechnique = effect.Techniques[0];

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        Matrix meshWorld = mesh.ParentBone.Transform;

                        effect.Parameters["World"].SetValue(meshWorld);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);

                        effect.Parameters["AmbientColor"].SetValue(ambient);
                        effect.Parameters["AmbientIntensity"].SetValue(ambientIntensity);
                        effect.Parameters["DiffuseLightDirection"].SetValue(diffuseLightDirection);
                        effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
                        effect.Parameters["DiffuseIntensity"].SetValue(diffuseIntensity);

                        Matrix wit = Matrix.Transpose(Matrix.Invert(meshWorld));
                        effect.Parameters["WorldInverseTranspose"].SetValue(wit);

                        pass.Apply();

                        GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                        GraphicsDevice.Indices = part.IndexBuffer;

                        GraphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            part.VertexOffset,
                            0,
                            part.NumVertices,
                            part.StartIndex,
                            part.PrimitiveCount
                        );
                    }
                }
            }

            // PASS 2: draw RenderTarget2D to the screen
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            spriteBatch.Begin();
            spriteBatch.Draw(
                renderTarget,
                new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                Color.White
            );

            spriteBatch.DrawString(font, $"angle: {angle:0.00}", new Vector2(12, 12), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

    }
}
