using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalProject
{
    public class FinalProject : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Matrix world;
        Matrix view;
        Matrix projection;

        KeyboardState previousKeyboardState;
        MouseState previousMouseState;

        Model model;
        Effect effect, comicEffect, normalBufferEffect, depthBufferEffect;

        SpriteFont font;

        RenderTarget2D renderTarget;
        RenderTarget2D normalBufferRender2D;
        RenderTarget2D depthBufferRender2D;

       // Texture2D background;

        // Camera
        Vector3 cameraPosition = new Vector3(0, 0, 20);
        float angle = 0f;
        float angle2 = 0f;
        float distance = 20;
        float midMouseX = 0f;
        float midMouseY = 0f;

        // Light
        Vector3 lightPosition = new Vector3(5f, 5f, 5f);
        float lightAngle = 0f;
        float lightAngle2 = 0f;

        // UI
        bool boolHelp = true;
        bool boolInfo = true;
        string shaderType = "Lambert";

        // Material
        Vector4 ambient = new Vector4(1f, 1f, 1f, 1f);
        float ambientIntensity = 0.10f;

        Vector4 diffuseColor = new Vector4(1f, 1f, 1f, 1f);
        float diffuseIntensity = 1.0f;

        Vector4 specularColor = new Vector4(1f, 1f, 1f, 1f);
        float specularIntensity = 1.0f;
        float shininess = 32.0f;

        public FinalProject()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.PreferredBackBufferWidth = 1980;
            graphics.PreferredBackBufferHeight = 1080;
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            model = Content.Load<Model>("Box");
            effect = Content.Load<Effect>("SimpleShader");
            comicEffect = Content.Load<Effect>("ComicShader");
            normalBufferEffect = Content.Load<Effect>("NormalBufferShader");
            depthBufferEffect = Content.Load<Effect>("DepthBufferShader");

           // background = Content.Load<Texture2D>("backgroundImage");
            font = Content.Load<SpriteFont>("Demo");

            world = Matrix.Identity;

            cameraPosition = new Vector3(0, 0, 20);
            view = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);

            projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45f),
                GraphicsDevice.Viewport.AspectRatio,
                0.1f,
                100f);

            renderTarget = new RenderTarget2D(
                GraphicsDevice,
                GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height,
                false,
                SurfaceFormat.Color,
                DepthFormat.Depth24);

            normalBufferRender2D = new RenderTarget2D(
                GraphicsDevice,
                GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height,
                false,
                SurfaceFormat.Color,
                DepthFormat.Depth24);

            depthBufferRender2D = new RenderTarget2D(
                GraphicsDevice,
                GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height,
                false,
                SurfaceFormat.Color,
                DepthFormat.Depth24);
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

            bool shift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
            bool ctrl = keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl);

            // Light rotation
            if (keyboard.IsKeyDown(Keys.Left))
                lightAngle -= 0.02f;
            if (keyboard.IsKeyDown(Keys.Right))
                lightAngle += 0.02f;
            if (keyboard.IsKeyDown(Keys.Up))
                lightAngle2 += 0.02f;
            if (keyboard.IsKeyDown(Keys.Down))
                lightAngle2 -= 0.02f;

            lightPosition = Vector3.Transform(
                new Vector3(5f, 5f, 5f),
                Matrix.CreateRotationX(lightAngle2) * Matrix.CreateRotationY(lightAngle));

            // Reset
            if (keyboard.IsKeyDown(Keys.S))
            {
                angle = 0f;
                angle2 = 0f;
                distance = 20f;
                midMouseX = 0f;
                midMouseY = 0f;

                lightAngle = 0f;
                lightAngle2 = 0f;
                lightPosition = new Vector3(5f, 5f, 5f);

                ambient = new Vector4(1f, 1f, 1f, 1f);
                ambientIntensity = 0.10f;

                diffuseColor = new Vector4(1f, 1f, 1f, 1f);
                diffuseIntensity = 1.0f;

                specularColor = new Vector4(1f, 1f, 1f, 1f);
                specularIntensity = 1.0f;
                shininess = 32.0f;
            }

            // Camera rotate
            if (mouse.LeftButton == ButtonState.Pressed &&
                previousMouseState.LeftButton == ButtonState.Pressed)
            {
                angle += 0.01f * (mouse.X - previousMouseState.X);
                angle2 += 0.01f * (mouse.Y - previousMouseState.Y);
            }

            // Camera zoom
            if (mouse.RightButton == ButtonState.Pressed &&
                previousMouseState.RightButton == ButtonState.Pressed)
            {
                distance += 0.1f * (mouse.Y - previousMouseState.Y);
            }

            // Camera pan
            if (mouse.MiddleButton == ButtonState.Pressed &&
                previousMouseState.MiddleButton == ButtonState.Pressed)
            {
                midMouseX += 0.03f * (mouse.X - previousMouseState.X);
                midMouseY -= 0.03f * (mouse.Y - previousMouseState.Y);
            }

            Matrix cameraTransform =
                Matrix.CreateTranslation(midMouseX, midMouseY, distance - 20f) *
                Matrix.CreateRotationX(angle2) *
                Matrix.CreateRotationY(angle);

            cameraPosition = Vector3.Transform(new Vector3(0, 0, 20), cameraTransform);
            view = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);

            // Model switching
            if (keyboard.IsKeyDown(Keys.D1))
                model = Content.Load<Model>("Box");
            if (keyboard.IsKeyDown(Keys.D2))
                model = Content.Load<Model>("Sphere");
            if (keyboard.IsKeyDown(Keys.D3))
                model = Content.Load<Model>("Torus");
            if (keyboard.IsKeyDown(Keys.D4))
                model = Content.Load<Model>("Teapot");
            if (keyboard.IsKeyDown(Keys.D5))
                model = Content.Load<Model>("bunny");

            // Ambient controls
            if (keyboard.IsKeyDown(Keys.L))
            {
                if (shift)
                    ambientIntensity -= 0.02f;
                else
                    ambientIntensity += 0.02f;
            }

            if (keyboard.IsKeyDown(Keys.R))
            {
                if (shift)
                    ambient.X -= 0.02f;
                else
                    ambient.X += 0.02f;
            }

            if (keyboard.IsKeyDown(Keys.G))
            {
                if (shift)
                    ambient.Y -= 0.02f;
                else
                    ambient.Y += 0.02f;
            }

            if (keyboard.IsKeyDown(Keys.B))
            {
                if (shift)
                    ambient.Z -= 0.02f;
                else
                    ambient.Z += 0.02f;
            }

            // Specular / shininess
            if (keyboard.IsKeyDown(Keys.OemPlus) || keyboard.IsKeyDown(Keys.Add))
            {
                if (ctrl)
                    shininess += 0.5f;
                else
                    specularIntensity += 0.05f;
            }

            if (keyboard.IsKeyDown(Keys.OemMinus) || keyboard.IsKeyDown(Keys.Subtract))
            {
                if (ctrl)
                    shininess -= 0.5f;
                else
                    specularIntensity -= 0.05f;
            }

            // Clamp
            ambientIntensity = MathHelper.Clamp(ambientIntensity, 0f, 1f);
            ambient.X = MathHelper.Clamp(ambient.X, 0f, 1f);
            ambient.Y = MathHelper.Clamp(ambient.Y, 0f, 1f);
            ambient.Z = MathHelper.Clamp(ambient.Z, 0f, 1f);

            specularIntensity = MathHelper.Clamp(specularIntensity, 0f, 5f);
            shininess = MathHelper.Clamp(shininess, 1f, 128f);

            // Toggle menus
            if (keyboard.IsKeyDown(Keys.H) && previousKeyboardState.IsKeyUp(Keys.H))
                boolInfo = !boolInfo;

            if (keyboard.IsKeyDown(Keys.OemQuestion) && previousKeyboardState.IsKeyUp(Keys.OemQuestion))
                boolHelp = !boolHelp;

            previousKeyboardState = keyboard;
            previousMouseState = mouse;

            base.Update(gameTime);
        }

        private void DrawModelWithEffect(Effect currentEffect)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    Matrix meshWorld = mesh.ParentBone.Transform * world;
                    Matrix worldInverseTranspose = Matrix.Transpose(Matrix.Invert(meshWorld));

                    if (currentEffect.Parameters["World"] != null)
                        currentEffect.Parameters["World"].SetValue(meshWorld);

                    if (currentEffect.Parameters["View"] != null)
                        currentEffect.Parameters["View"].SetValue(view);

                    if (currentEffect.Parameters["Projection"] != null)
                        currentEffect.Parameters["Projection"].SetValue(projection);

                    if (currentEffect.Parameters["WorldInverseTranspose"] != null)
                        currentEffect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);

                    if (currentEffect.Parameters["AmbientColor"] != null)
                        currentEffect.Parameters["AmbientColor"].SetValue(ambient);

                    if (currentEffect.Parameters["AmbientIntensity"] != null)
                        currentEffect.Parameters["AmbientIntensity"].SetValue(ambientIntensity);

                    if (currentEffect.Parameters["DiffuseColor"] != null)
                        currentEffect.Parameters["DiffuseColor"].SetValue(diffuseColor);

                    if (currentEffect.Parameters["DiffuseIntensity"] != null)
                        currentEffect.Parameters["DiffuseIntensity"].SetValue(diffuseIntensity);

                    if (currentEffect.Parameters["SpecularColor"] != null)
                        currentEffect.Parameters["SpecularColor"].SetValue(specularColor);

                    if (currentEffect.Parameters["SpecularIntensity"] != null)
                        currentEffect.Parameters["SpecularIntensity"].SetValue(specularIntensity);

                    if (currentEffect.Parameters["Shininess"] != null)
                        currentEffect.Parameters["Shininess"].SetValue(shininess);

                    if (currentEffect.Parameters["LightPosition"] != null)
                        currentEffect.Parameters["LightPosition"].SetValue(lightPosition);

                    if (currentEffect.Parameters["CameraPosition"] != null)
                        currentEffect.Parameters["CameraPosition"].SetValue(cameraPosition);

                    part.Effect = currentEffect;

                    foreach (EffectPass pass in currentEffect.CurrentTechnique.Passes)
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
                            part.PrimitiveCount);
                    }
                }
            }
        }

        public string ShaderInfo()
        {
            return
                "Light Position: " + lightPosition +
                "\nLight Angle: " + lightAngle.ToString("0.00") +
                "\nLight Angle2: " + lightAngle2.ToString("0.00") +
                "\nDistance: " + distance.ToString("0.00") +
                "\nPan X: " + midMouseX.ToString("0.00") +
                "\nPan Y: " + midMouseY.ToString("0.00") +
                "\nAmbient: " + ambient +
                "\nAmbient Intensity: " + ambientIntensity.ToString("0.00") +
                "\nSpecular Intensity: " + specularIntensity.ToString("0.00") +
                "\nShininess: " + shininess.ToString("0.00") +
                "\nShader Type: " + shaderType +
                "\nCamera Position: " + cameraPosition +
                "\nCamera Angle: " + angle.ToString("0.00") +
                "\nCamera Angle2: " + angle2.ToString("0.00");
        }

        public string HelpMenu()
        {
            return
                "Camera Controls:\n" +
                " Left Mouse Drag : Rotate Camera\n" +
                " Right Mouse Drag : Zoom\n" +
                " Middle Mouse Drag : Pan\n\n" +

                "Lighting Controls:\n" +
                " Arrow Keys : Rotate Light\n" +
                " S : Reset Camera / Light\n\n" +

                "Select Model:\n" +
                " 1 : Box\n" +
                " 2 : Sphere\n" +
                " 3 : Torus\n" +
                " 4 : Teapot\n" +
                " 5 : Bunny\n\n" +

                "Ambient:\n" +
                " L : Increase Ambient Intensity\n" +
                " Shift + L : Decrease Ambient Intensity\n" +
                " R/G/B : Increase Ambient Color\n" +
                " Shift + R/G/B : Decrease Ambient Color\n\n" +

                "Specular:\n" +
                " + : Increase Specular Intensity\n" +
                " - : Decrease Specular Intensity\n" +
                " Ctrl + + : Increase Shininess\n" +
                " Ctrl + - : Decrease Shininess\n\n" +

                "Menus:\n" +
                " ? : Show / Hide Help Menu\n" +
                " H : Show / Hide Shader Info\n";
        }




        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            // PASS 1: COLOR BUFFER
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.White);

            //send it to pass one to draw the 3D Model
            effect.CurrentTechnique = effect.Techniques["shader"];
            //draw the model using DrawModelWithEffect by using effect
            DrawModelWithEffect(effect);

            // PASS 2: NORMAL BUFFER
            GraphicsDevice.SetRenderTarget(normalBufferRender2D);
            GraphicsDevice.Clear(Color.Black);
            // add the render to normal buffer
            normalBufferEffect.CurrentTechnique = normalBufferEffect.Techniques["NormalTechnique"];
            DrawModelWithEffect(normalBufferEffect);

            // PASS 3: DEPTH BUFFER
            GraphicsDevice.SetRenderTarget(depthBufferRender2D);
            GraphicsDevice.Clear(Color.White);

            depthBufferEffect.CurrentTechnique = depthBufferEffect.Techniques["DepthTechnique"];
            DrawModelWithEffect(depthBufferEffect);

            // PASS 4: FINAL COMIC / EDGE PASS
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.White);
            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            comicEffect.CurrentTechnique = comicEffect.Techniques["Shader"];
            comicEffect.Parameters["modelTexture"].SetValue(renderTarget);
            comicEffect.Parameters["normalTexture"].SetValue(normalBufferRender2D);
            comicEffect.Parameters["depthTexture"].SetValue(depthBufferRender2D);
            comicEffect.Parameters["imageWidth"].SetValue((float)renderTarget.Width);
            comicEffect.Parameters["imageHeight"].SetValue((float)renderTarget.Height);

            Matrix projection2 = Matrix.CreateOrthographicOffCenter(
                0, renderTarget.Width,
                renderTarget.Height, 0,
                0, 1);

            Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);
            comicEffect.Parameters["MatrixTransform"].SetValue(halfPixelOffset * projection2);

            spriteBatch.Begin(effect: comicEffect);

            spriteBatch.Draw(
                renderTarget,
                new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                Color.White);

            spriteBatch.End();

            spriteBatch.Begin();

            if (boolHelp)
                spriteBatch.DrawString(font, HelpMenu(), new Vector2(10, 10), Color.Black);

            if (boolInfo)
                spriteBatch.DrawString(font, ShaderInfo(), new Vector2(1500, 10), Color.Black);

            spriteBatch.End();

            base.Draw(gameTime);
        }


    }
}