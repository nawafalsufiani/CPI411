using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;



//using SharpDX.Direct2D1;

namespace Testing
{
    public class Testing : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch _spriteBatch;
        Model model;
        Matrix world;
        Matrix view;
        Matrix projection;
        SpriteFont font;
        Effect effect;
        bool boolHelp= false;
        bool boolInfo = false;
        //Keyboard current state
        KeyboardState previousKeyboardState;
       

        //camera variable will pass to HLSL ex file 
        Vector3 cameraOffset = new Vector3(0, 0, 0);
        Vector3 cameraPosition = new Vector3(0, 0, 10);

        int selectingTechnique = 0;

        //light position
        Vector3 lightPosition = new Vector3(0f, 0, 0);

        float lightAngle = 0;
        float lightAngle2 = 0;
        float angle = 0;
        float angle2 = 0;
        float distance = 0f;
        float midMouseX = 0f;
        float midMouseY = 0f;



        Vector4 ambient = new Vector4(1, 0.2f, 0.2f, 1);
        float ambientIntensity = 0.9f;

        Vector3 lightDirection = new Vector3(0.5f, 0.6f, 0.4f);

        float specularIntensity = 0.9f;
        Vector4 specularColor = new Vector4(1, 1, 1, 1);
        Vector4 diffuseColor = new Vector4(0.5f, 1f, 0.5f, 1f);
        Vector3 diffuseLightDirection = new Vector3(1, 1, 1);
        float diffuseIntensity = 0.6f;

        float shininess = 7.0f;

        string shaderType = "Gauraud";

        Vector4 lightColor = new Vector4(1, 1, 1, 1);
        float lightIntensity = 1.0f;

        

        // Keyboard and mouse previous state
       
        MouseState previousMouseState;



        public Testing()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            model = Content.Load<Model>("bunny");
            effect = Content.Load<Effect>("TestShader");
            font = Content.Load<SpriteFont>("font");

            /*
             * 
             * We use this code so the CPU use the custom shader instead of the CPU default shader
             */
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {

                    
                    part.Effect = effect.Clone();
                }
            }
        }

        protected override void Initialize()
        {
            world = Matrix.Identity;
            view = Matrix.CreateLookAt(
                new Vector3(0, 0, 20),
                Vector3.Zero,
                Vector3.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(90),
                GraphicsDevice.Viewport.AspectRatio,
                0.1f,
                100);



            base.Initialize();
        }


        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            bool shift = false;
            KeyboardState keb = Keyboard.GetState();
            if (keb.IsKeyDown(Keys.LeftShift) || keb.IsKeyDown(Keys.RightShift))
            {
                shift = true;
            }


            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                lightAngle -= 0.02f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                lightAngle += 0.02f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                lightAngle2 += 0.02f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                lightAngle2 -= 0.02f;
            }
            lightPosition = Vector3.Transform(new Vector3(0.5f, 1, 1), Matrix.CreateRotationX(lightAngle2) * Matrix.CreateRotationY(lightAngle));

            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                view = Matrix.CreateLookAt(
                new Vector3(0, 0, 20),
                Vector3.Zero,
                Vector3.Up);
                lightPosition = new Vector3(0.5f, 1, 1);
                angle = 0;
                angle2 = 0;
                lightAngle = 0;
                lightAngle2 = 0;
                distance = 0f;
                midMouseX = 0f;
                midMouseY = 0f;
                ambient = new Vector4(1, 0.2f, 0.2f, 1);
            }


            if (Mouse.GetState().LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Pressed)
            {

                angle += 0.01f * (Mouse.GetState().X - previousMouseState.X);
                angle2 += 0.01f * (Mouse.GetState().Y - previousMouseState.Y);
                Vector3 camera = Vector3.Transform(
                new Vector3(0, 0, 20), Matrix.CreateTranslation(0, 0, distance) * Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));
                view = Matrix.CreateLookAt(camera, Vector3.Zero, Vector3.UnitY);
            }
            if (Mouse.GetState().RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Pressed)
            {
                distance += 0.1f * (Mouse.GetState().Y - previousMouseState.Y);
                Vector3 camera = Vector3.Transform(
                    new Vector3(0, 0, 20), Matrix.CreateTranslation(0, 0, distance) * Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));
                view = Matrix.CreateLookAt(camera, Vector3.Zero, Vector3.UnitY);

            }
            if (Mouse.GetState().MiddleButton == ButtonState.Pressed && previousMouseState.MiddleButton == ButtonState.Pressed)
            {
                midMouseX = 1f * (Mouse.GetState().X - previousMouseState.X);
                midMouseY = 1f * (Mouse.GetState().Y - previousMouseState.Y);
                Vector3 camera = Vector3.Transform(
                new Vector3(0, 0, 20), Matrix.CreateTranslation(midMouseX, midMouseY, distance) * Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));
                view = Matrix.CreateLookAt(camera, Vector3.Zero, Vector3.UnitY);
            }

            if (keb.IsKeyDown(Keys.L))
            {
                if (shift)
                {
                    lightIntensity -= 0.02f;
                }
                else
                {
                    lightIntensity += 0.02f;
                }
            }


            if (keb.IsKeyDown(Keys.R))
            {
                if (shift)
                {
                    lightColor.X -= 0.02f;
                }
                else
                {
                    lightColor.X += 0.02f;
                }
            }

            if (keb.IsKeyDown(Keys.G))
            {
                if (shift)
                {
                    lightColor.Y -= 0.02f;
                }
                else
                {
                    lightColor.Y += 0.02f;
                }
            }

            if (keb.IsKeyDown(Keys.B))
            {
                if (shift)
                {
                    lightColor.Z -= 0.02f;
                }
                else
                {
                    lightColor.Z += 0.02f;
                }
            }

            lightIntensity = MathHelper.Clamp(lightIntensity, 0f, 5f);

            lightColor.X = MathHelper.Clamp(lightColor.X, 0f, 1f);
            lightColor.Y = MathHelper.Clamp(lightColor.Y, 0f, 1f);
            lightColor.Z = MathHelper.Clamp(lightColor.Z, 0f, 1f);


            effect.Parameters["LightColor"].SetValue(lightColor);
            effect.Parameters["LightIntensity"].SetValue(lightIntensity);


            //pass the variable to HLSL to is match parameter. BTW 

            effect.Parameters["View"].SetValue(view);
            effect.Parameters["World"].SetValue(world);
            effect.Parameters["LightPosition"].SetValue(lightPosition);


            //model changing when pressing 1 to 5
            if (Keyboard.GetState().IsKeyDown(Keys.D1))
            {
                model = Content.Load<Model>("Box");
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D2))
            {
                model = Content.Load<Model>("Sphere");
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D3))
            {
                model = Content.Load<Model>("Torus");
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D4))
            {
                model = Content.Load<Model>("Teapot");
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D5))
            {
                model = Content.Load<Model>("bunny");
            }


            /*
              F1.Gouraud(Phong per vertex)(5 Pts)
            • F2.Phong per pixel(5 Pts)
            • F3.PhongBlinn(5 Pts)
            • F4.Schlick(5 Pts)
            • F5.Toon(5 Pts)
            2
            • F6.HalfLife(5 Pts)

             */

            /*
             * 
            0.Gouraud(Phong per vertex)(5 Pts)
          • 1.Phong per pixel(5 Pts)
          • 2.PhongBlinn(5 Pts)
          • 3.Schlick(5 Pts)
          • 4.Toon(5 Pts)
          
          • 5.HalfLife(5 Pts)

           */


            /*
             * 
             shaderType we change it to match the user interface
             */

            //Gouraud
            if (Keyboard.GetState().IsKeyDown(Keys.F1))
            {

                selectingTechnique = 0;
                shaderType = "Gouraud shader";
            }

            //Phong
            if (Keyboard.GetState().IsKeyDown(Keys.F2))
            {
                selectingTechnique = 1;
                shaderType = "Phong shader";
            }

            //PhongBlinn
            if (Keyboard.GetState().IsKeyDown(Keys.F3))
            {
                selectingTechnique = 2;
                shaderType = "PhongBlinn shader";
            }

            //Schlick
            if (Keyboard.GetState().IsKeyDown(Keys.F4))
            {
                selectingTechnique = 3;
                shaderType = "Schlick shader";
            }


            //Toon
            if (Keyboard.GetState().IsKeyDown(Keys.F5))
            {
                selectingTechnique = 4;
                shaderType = "Toon shader";

            }

            //HalfLife
            if (Keyboard.GetState().IsKeyDown(Keys.F6))
            {
                selectingTechnique = 5;
                shaderType = "HalfLife";
            }




            if (Keyboard.GetState().IsKeyDown(Keys.L))
            {
                if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift))
                {
                    ambientIntensity -= 0.02f;
                }
                else
                {
                    ambientIntensity += 0.02f;
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift))
                {
                    ambient -= new Vector4(0.02f, 0, 0, 0);
                }
                else
                {
                    ambient += new Vector4(0.02f, 0, 0, 0);
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.G))
            {
                if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift))
                {
                    ambient -= new Vector4(0, 0.02f, 0, 0);
                }
                else
                {
                    ambient += new Vector4(0, 0.02f, 0, 0);
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.B))
            {
                if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift))
                {
                    ambient -= new Vector4(0, 0, 0.02f, 0);
                }
                else
                {
                    ambient += new Vector4(0, 0, 0.02f, 0);
                }
            }

            if (Keyboard.GetState().IsKeyDown(Keys.OemPlus))
            {
                if (Keyboard.GetState().IsKeyDown(Keys.LeftControl))
                {
                    shininess += 0.02f;
                }
                else
                {
                    specularIntensity += 0.02f;
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.OemMinus))
            {
                if (Keyboard.GetState().IsKeyDown(Keys.LeftControl))
                {
                    shininess -= 0.02f;
                }
                else
                {
                    specularIntensity -= 0.02f;
                }

            }

            KeyboardState currentKeyboard = Keyboard.GetState();



            // pressing  H ke
            if (currentKeyboard.IsKeyDown(Keys.H) && previousKeyboardState.IsKeyUp(Keys.H))
            {
                boolInfo = !boolInfo;
            }

            // pressing ? key
            if (currentKeyboard.IsKeyDown(Keys.OemQuestion) && previousKeyboardState.IsKeyUp(Keys.OemQuestion))
            {
                boolHelp = !boolHelp;
            }

            // Save keyboard state for next frame
            previousKeyboardState = currentKeyboard;

          
            previousMouseState = Mouse.GetState();
            base.Update(gameTime);
        }


        public string HelpMenu()
        {
            string shaderInfoText =
              "Camera Controls:\n" +
        " Rotate Camera: Left Mouse Drag\n" +
        " Zoom (Change Distance): Right Mouse Drag\n" +
        " Pan (Translate Camera): Middle Mouse Drag\n\n" +

        "Lighting Controls:\n" +
        "  Light Rotation: Arrow Keys\n" +
        " Reset Camera/ Light: S \n\n" +

        "Select a Model:\n" +
        "1 : Box\n" +
        "2 : Sphere\n" +
        "3 : Torus\n" +
        "4 : Teapot\n" +
        "5 : Bunny\n\n" +

        "Select a Shader:\n" +
        "F1 key : Gouraud Shader\n" +
        "F2 key : Phong per pixel Shader\n" +
        "F3 key : PhongBlinn Shader\n" +
        "F4 key : Schlick Shader\n" +
        "F5 key : Toon Shader\n" +
        "F6 key : HalfLife Shader\n\n" +

        "Light:\n" +
        "L : Increase Light Intensity (+ Shift to decrease)\n" +
        "R : Increase Red   (+ Shift to decrease)\n" +
        "G : Increase Green (+ Shift to decrease)\n" +
        "B : Increase Blue  (+ Shift to decrease)\n\n" +

        "Material:\n" +
        "+ : Increase Specular Intensity\n" +
        "- : Decrease Specular Intensity\n" +
        "Ctrl + : Increase Shininess\n" +
        "Ctrl - : Decrease Shininess\n\n" +

        "Menu hid/show:\n" +
        " ? : Show -> Hide Help Menu\n" +
        " H : Show -> Hide Shader Information\n";
            return shaderInfoText;
        }


        public string ShaderInfo()
        {
            string helpText =
             "Light Angle: " + lightAngle.ToString("0.00") +
             "\nLight Angle2: " + lightAngle2.ToString("0.00") +
             "\nDistance: " + distance +
    "\nMidMouseX: " + midMouseX.ToString("0.00") +
    "\nMidMouseY: " + midMouseY.ToString("0.00") +
    "\nAmbient: " + ambient +
    "\nAmbient Intensity: " + ambientIntensity +
    "\nSpecular Intensity: " + specularIntensity +
    "\nShininess: " + shininess +
    
    "\nCamera Angle: " + angle.ToString("0.00") +
    "\nCamera Angle2: " + angle2.ToString("0.00");
            return helpText;
        }



        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            if (boolHelp)
            {
                _spriteBatch.DrawString(font, HelpMenu(), new Vector2(10, 10), Color.Black);
            }

            if (boolInfo)
            {
                _spriteBatch.DrawString(font, ShaderInfo(), new Vector2(900, 10), Color.Black);
            }

            _spriteBatch.End();



            effect.CurrentTechnique = effect.Techniques[selectingTechnique];
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {

                        effect.Parameters["World"].SetValue(mesh.ParentBone.Transform);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);
                        effect.Parameters["AmbientColor"].SetValue(ambient);
                        effect.Parameters["AmbientIntensity"].SetValue(ambientIntensity);
                        effect.Parameters["LightPosition"].SetValue(lightPosition);
                        effect.Parameters["SpecularColor"].SetValue(specularColor);
                        effect.Parameters["SpecularIntensity"].SetValue(specularIntensity);
                        effect.Parameters["Shininess"].SetValue(shininess);
                        effect.Parameters["CameraPosition"].SetValue(cameraPosition);
                        effect.Parameters["DiffuseLightDirection"].SetValue(diffuseLightDirection);
                        effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
                        effect.Parameters["DiffuseIntensity"].SetValue(diffuseIntensity);


                        pass.Apply();
                        Matrix worldInverseTranspose = Matrix.Transpose(
                            Matrix.Invert(mesh.ParentBone.Transform));
                        effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);

                        GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                        GraphicsDevice.Indices = part.IndexBuffer;

                        GraphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList, part.VertexOffset, 0,
                            part.NumVertices, part.StartIndex, part.PrimitiveCount);

                    }
                }
            }

            base.Draw(gameTime);
        }


    }
}
