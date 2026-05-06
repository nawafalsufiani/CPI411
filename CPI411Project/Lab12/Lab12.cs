using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lab12
{
    public class Lab12 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private Matrix world;
        private Matrix view;
        private Matrix projection;

        private Model model;

        private Effect depthNormalEffect;
        private Effect ssaoEffect;

        private RenderTarget2D renderTarget;
        private Texture2D depthAndNormalMap;
        private Texture2D randomNormalMap;

        private VertexPositionTexture[] vertices =
        {
            new VertexPositionTexture(new Vector3( 1,  1, 0), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3( 1, -1, 0), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 1)),

            new VertexPositionTexture(new Vector3(-1,  1, 0), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3( 1,  1, 0), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 1))
        };

        private float angle = 0f;
        private float angle2 = 0f;
        private MouseState previousMouseState;

        private float offset = 800f / 256f;
        private float ssaoRad = 0.01f;

        private SpriteFont font;
        private bool showDepthNormalMap = false;
        private KeyboardState previousKeyboardState;

        public Lab12()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            world = Matrix.Identity;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            model = Content.Load<Model>("objects");
            depthNormalEffect = Content.Load<Effect>("DepthAndNormal");
            ssaoEffect = Content.Load<Effect>("SSAO");
            randomNormalMap = Content.Load<Texture2D>("noise");
            font = Content.Load<SpriteFont>("font");

            view = Matrix.CreateLookAt(new Vector3(0, 0, 20), Vector3.Zero, Vector3.UnitY);
            projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(90f),
                GraphicsDevice.Viewport.AspectRatio,
                0.1f,
                100f
            );

            PresentationParameters pp = GraphicsDevice.PresentationParameters;
            renderTarget = new RenderTarget2D(
                GraphicsDevice,
                pp.BackBufferWidth,
                pp.BackBufferHeight,
                false,
                SurfaceFormat.Color,
                DepthFormat.Depth24
            );
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboard = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                keyboard.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            if (keyboard.IsKeyDown(Keys.Up) && previousKeyboardState.IsKeyUp(Keys.Up))
                ssaoRad += 0.001f;

            if (keyboard.IsKeyDown(Keys.Down) && previousKeyboardState.IsKeyUp(Keys.Down))
                ssaoRad -= 0.001f;

            if (ssaoRad < 0.001f)
                ssaoRad = 0.001f;

            if (keyboard.IsKeyDown(Keys.Space) && previousKeyboardState.IsKeyUp(Keys.Space))
                showDepthNormalMap = !showDepthNormalMap;

            if (mouse.LeftButton == ButtonState.Pressed &&
                previousMouseState.LeftButton == ButtonState.Pressed)
            {
                float offsetX = 0.01f * (mouse.X - previousMouseState.X);
                float offsetY = 0.01f * (mouse.Y - previousMouseState.Y);

                angle += offsetX;
                angle2 += offsetY;
            }

            Vector3 camera = Vector3.Transform(
                new Vector3(0, 0, 20),
                Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));

            view = Matrix.CreateLookAt(camera, Vector3.Zero, Vector3.UnitY);

            previousMouseState = mouse;
            previousKeyboardState = keyboard;

            base.Update(gameTime);
        }

        private void DrawDepthAndNormalMap()
        {
            depthNormalEffect.CurrentTechnique = depthNormalEffect.Techniques[0];

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    Matrix meshWorld = mesh.ParentBone.Transform;

                    depthNormalEffect.Parameters["World"].SetValue(meshWorld);
                    depthNormalEffect.Parameters["View"].SetValue(view);
                    depthNormalEffect.Parameters["Projection"].SetValue(projection);

                    Matrix worldInverseTransposeMatrix =
                        Matrix.Transpose(Matrix.Invert(meshWorld));
                    depthNormalEffect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTransposeMatrix);

                    foreach (EffectPass pass in depthNormalEffect.CurrentTechnique.Passes)
                    {
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
        }

        private void DrawSSAO()
        {
            ssaoEffect.CurrentTechnique = ssaoEffect.Techniques[0];

            ssaoEffect.Parameters["RandomNormalTexture"].SetValue(randomNormalMap);
            ssaoEffect.Parameters["DepthAndNormalTexture"].SetValue(depthAndNormalMap);
            ssaoEffect.Parameters["offset"].SetValue(offset);
            ssaoEffect.Parameters["rad"].SetValue(ssaoRad);

            foreach (EffectPass pass in ssaoEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.DrawUserPrimitives(
                    PrimitiveType.TriangleList,
                    vertices,
                    0,
                    2
                );
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            // ===== PASS 1: render Depth + Normal =====
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

            DrawDepthAndNormalMap();

            GraphicsDevice.SetRenderTarget(null);
            depthAndNormalMap = renderTarget;

            // ===== PASS 2: show result =====
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.DarkSlateBlue, 1.0f, 0);

            if (showDepthNormalMap)
            {
                // draw texture + text
                spriteBatch.Begin();

                spriteBatch.Draw(depthAndNormalMap, Vector2.Zero, Color.White);

                spriteBatch.DrawString(font,
                    "Showing Depth+Normal Map (SPACE to toggle)",
                    new Vector2(10, 10),
                    Color.White);

                spriteBatch.DrawString(font,
                    $"Radius: {ssaoRad:0.000}",
                    new Vector2(10, 35),
                    Color.White);

                spriteBatch.End();
            }
            else
            {
                // draw SSAO
                GraphicsDevice.DepthStencilState = DepthStencilState.None;
                DrawSSAO();

                // reset for UI text
                GraphicsDevice.BlendState = BlendState.AlphaBlend;

                spriteBatch.Begin();

                spriteBatch.DrawString(font,
                    "Showing SSAO (SPACE to toggle)",
                    new Vector2(10, 10),
                    Color.White);

                spriteBatch.DrawString(font,
                    $"Radius: {ssaoRad:0.000}",
                    new Vector2(10, 35),
                    Color.White);

                spriteBatch.DrawString(font,
                    "UP/DOWN = change radius",
                    new Vector2(10, 60),
                    Color.White);

                spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}