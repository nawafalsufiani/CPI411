using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lab04
{
    /*
     Notic the variable which assigned to the sheder parameter has the same type
     Dont forgrt this because will use it in the assignment 01
     * */
    public class Lab04 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Model model;
        Effect effect;

        Matrix view, projection;

        SpriteFont font;
        // Camera
        Vector3 camera;
        //angle
        float angle = 0f;
        //angle 2
        float angle2 = 0f;
        // mouse state
        MouseState previousMouseState;

        // Light (Point Light Position)
        Vector3 lightPosition = new Vector3(2f, 2f, 3f);

        // Material / Lighting
        Vector4 ambientColor = new Vector4(0.2f, 0.2f, 0.2f, 1f);
        float ambientIntensity = 1.0f;
        //diffuse color 
        Vector4 diffuseColor = new Vector4(1f, 1f, 1f, 1f);
        //diffuse intensity color vlause
        float diffuseIntensity = 0.7f;
        //specular color
        Vector4 specularColor = new Vector4(1f, 1f, 1f, 1f);
        //specular intensity
        float specularIntensity = 0.6f;
        //shininess
        float shininess = 2.0f;
        //initialize toggle technique value to 1 (Phong);  menas 0 = Gouraud means 1 = Phong
        int toggleTechnique = 1; 

        //constructor
        public Lab04()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
        }
        //load method
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            model = Content.Load<Model>("Torus");
            effect = Content.Load<Effect>("SimpleShader");
            font = Content.Load<SpriteFont>("font");
            projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(90f),
                GraphicsDevice.Viewport.AspectRatio,
                0.1f,
                100f);

            camera = new Vector3(0, 0, 20);
            view = Matrix.CreateLookAt(camera, Vector3.Zero, Vector3.UnitY);

            System.Diagnostics.Debug.WriteLine("Techniques: " + effect.Techniques.Count);
        }
        //infinite loop update method similar to unity's update method
        protected override void Update(GameTime gameTime)
        {
            // Keyboard state
            var kb = Keyboard.GetState();
            // Mouse state
            var ms = Mouse.GetState();

            if (kb.IsKeyDown(Keys.Escape))
                Exit();

            // Toggle shader: 0 / 1
            if (kb.IsKeyDown(Keys.D0) || kb.IsKeyDown(Keys.NumPad0)) toggleTechnique = 0;
            if (kb.IsKeyDown(Keys.D1) || kb.IsKeyDown(Keys.NumPad1)) toggleTechnique = 1;

            // Shininess control
            if (kb.IsKeyDown(Keys.Left)) shininess = MathHelper.Max(0.1f, shininess - 0.05f);
            if (kb.IsKeyDown(Keys.Right)) shininess += 0.05f;

            // Mouse drag rotate camera
            if (ms.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Pressed)
            {
                float offsetx = 0.01f * (ms.X - previousMouseState.X);
                float offsety = 0.01f * (ms.Y - previousMouseState.Y);
                angle += offsetx;
                angle2 += offsety;
            }

            camera = Vector3.Transform(new Vector3(0, 0, 20),
                Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));

            view = Matrix.CreateLookAt(camera, Vector3.Zero, Vector3.UnitY);

            previousMouseState = ms;

            base.Update(gameTime);
        }
        // Draw method to display the model to the screen
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // assignthe technique to the effect from the toggleTechnique variable
            effect.CurrentTechnique = effect.Techniques[toggleTechnique];
            //loop to draw the model
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    Matrix world = mesh.ParentBone.Transform;
                    Matrix worldInverseTranspose = Matrix.Transpose(Matrix.Invert(world));

                    // Matrices paramenter assign to the sheder
                    effect.Parameters["World"].SetValue(world);
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);
                    effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);

                    // Lighting assign to the shader
                    effect.Parameters["AmbientColor"].SetValue(ambientColor);
                    effect.Parameters["AmbientIntensity"].SetValue(ambientIntensity);

                    effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
                    effect.Parameters["DiffuseIntensity"].SetValue(diffuseIntensity);

                    effect.Parameters["SpecularColor"].SetValue(specularColor);
                    effect.Parameters["SpecularIntensity"].SetValue(specularIntensity);
                    effect.Parameters["Shininess"].SetValue(shininess);
                    //light position assign to the shader
                    effect.Parameters["LightPosition"].SetValue(lightPosition);
                    // Camera position assign to the shader
                    effect.Parameters["CameraPosition"].SetValue(camera);

                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
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
            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            spriteBatch.Begin();

           

            spriteBatch.DrawString(
                font,
                "Camera:" + "{X:" + camera.X +
                " Y:" + camera.Y +
                " Z:" + camera.Z + "}",
                Vector2.UnitX + Vector2.UnitY * 28,
                Color.White
            );

            spriteBatch.DrawString(
                font,
                "Shiness:" + shininess,
                Vector2.UnitX + Vector2.UnitY * 44,
                Color.White
            );

            spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}
