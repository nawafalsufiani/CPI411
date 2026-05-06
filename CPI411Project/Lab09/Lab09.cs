using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lab09
{
    public class Lab09 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Effect effect;

        Matrix world = Matrix.Identity;
        Matrix view = Matrix.CreateLookAt( new Vector3(0, 0, 20),new Vector3(0, 0, 0),Vector3.UnitY);

        Matrix projection = Matrix.CreatePerspectiveFieldOfView( MathHelper.ToRadians(45),800f / 600f, 0.1f,100f);

        Vector3 cameraPosition;
        Vector3 cameraTarget;
        Vector3 lightPosition;

        float angle = 0;
        float angle2 = 0;
        float angleL = 0;
        float angleL2 = 0;
        float distance = 20;

        MouseState preMouse;

        Model[] models;
        Texture2D texture;

        Matrix lightView;
        Matrix lightProjection;
        /*
         * 
         *  We store the information of the scene before display it to the screen
         */
        RenderTarget2D renderTarget;
        Texture2D shadowMap;

        public Lab09()
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

            models = new Model[2];
            models[0] = Content.Load<Model>("Plane");
            models[1] = Content.Load<Model>("torus");

            effect = Content.Load<Effect>("ShadowShader1");
            texture = Content.Load<Texture2D>("Checker");

            renderTarget = new RenderTarget2D( GraphicsDevice,
                2048,
                2048,
                false,
                SurfaceFormat.Single,DepthFormat.Depth24,
                0,
                RenderTargetUsage.PlatformContents);
        }

        protected override void UnloadContent()
        {
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

            MouseState mouse = Mouse.GetState();

            if (mouse.LeftButton == ButtonState.Pressed)
            {
                angle -= (mouse.X - preMouse.X) / 100f;
                angle2 += (mouse.Y - preMouse.Y) / 100f;
            }

            if (mouse.RightButton == ButtonState.Pressed)
            {
                distance += (mouse.X - preMouse.X) / 100f;
            }

            if (mouse.MiddleButton == ButtonState.Pressed)
            {
                Vector3 viewRight = Vector3.Transform(
                    Vector3.UnitX,
                    Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));

                Vector3 viewUp = Vector3.Transform(
                    Vector3.UnitY,
                    Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));

                cameraTarget -= viewRight * (mouse.X - preMouse.X) / 10f;
                cameraTarget += viewUp * (mouse.Y - preMouse.Y) / 10f;
            }

            preMouse = mouse;

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
                MathHelper.PiOver2,
                1f,
                1f,
                100f);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

            DrawShadowMap();

            GraphicsDevice.SetRenderTarget(null);
            shadowMap = renderTarget;

            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer,
                Color.DarkSlateBlue, 1.0f, 0);

            DrawShadowScene();

            base.Draw(gameTime);
        }

        private void DrawShadowMap()
        {
            effect.CurrentTechnique = effect.Techniques["ShadowMapShader"];

            foreach (Model model in models)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    Matrix meshWorld = mesh.ParentBone.Transform;

                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        effect.Parameters["World"].SetValue(meshWorld);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);
                        effect.Parameters["WorldInverseTranspose"]
                            .SetValue(Matrix.Transpose(Matrix.Invert(meshWorld)));
                        effect.Parameters["CameraPosition"].SetValue(cameraPosition);
                        effect.Parameters["LightPosition"].SetValue(lightPosition);
                        effect.Parameters["LightViewMatrix"].SetValue(lightView);
                        effect.Parameters["LightProjectionMatrix"].SetValue(lightProjection);

                        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();

                            GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                            GraphicsDevice.Indices = part.IndexBuffer;
                            GraphicsDevice.DrawIndexedPrimitives(
                                PrimitiveType.TriangleList,
                                part.VertexOffset,
                                part.StartIndex,
                                part.PrimitiveCount);
                        }
                    }
                }
            }
        }

        private void DrawShadowScene()
        {
            effect.CurrentTechnique = effect.Techniques["ShadowedScene"];

            foreach (Model model in models)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    Matrix meshWorld = mesh.ParentBone.Transform;

                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        effect.Parameters["World"].SetValue(meshWorld);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);
                        effect.Parameters["WorldInverseTranspose"]
                            .SetValue(Matrix.Transpose(Matrix.Invert(meshWorld)));
                        effect.Parameters["CameraPosition"].SetValue(cameraPosition);
                        effect.Parameters["LightPosition"].SetValue(lightPosition);
                        effect.Parameters["LightViewMatrix"].SetValue(lightView);
                        effect.Parameters["LightProjectionMatrix"].SetValue(lightProjection);
                        effect.Parameters["ShadowMap"].SetValue(shadowMap);

                        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();

                            GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                            GraphicsDevice.Indices = part.IndexBuffer;
                            GraphicsDevice.DrawIndexedPrimitives(
                                PrimitiveType.TriangleList,
                                part.VertexOffset,
                                part.StartIndex,
                                part.PrimitiveCount);
                        }
                    }
                }
            }
        }
    }
}