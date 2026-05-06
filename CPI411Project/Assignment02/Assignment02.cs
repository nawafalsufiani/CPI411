using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CPI411.SimpleEngine;

namespace Assignment2
{
    
    public class Assignment02 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        //Effect used for using hlsl
        //one for the model without texture and one for the model with texture
        Effect effect,effect2;
        //display the model 3D
        Model model;
        //models used for changing the model
        Model modelBunny, modelTorus, modelCube, modelSphere, modelTeapot, modelHeli;
        //2D texture
        Texture2D texture;
        //Font used for the menu  Help H  and Info ?
        SpriteFont font;
        // screen size 
        int width;
        int height;

        //camera variable will pass to HLSL ex file 
        Vector3 cameraOffset;
        Vector3 cameraPosition;
        //Light angles
        float lightAngleX;
        float lightAngleY;
        //camera angles x,y
        float cameraAngleX;
        float cameraAngleY;
        //the ambient 
        Vector4 ambient;
        //the ambient intensity
        float ambientIntensity;
        //the diffuse color and intensity
        Vector4 diffuseColor;
        //the diffuse intensity
        float diffuseIntensity;
        //the specular color and shininess
        Vector4 specularColor;
        //the shininess
        float shininess;
        //the eta ratio for fresnel effect
        float etaRatio;
        //the fresnel eta ratio for fresnel effect
        Vector3 fresnelEtaRatio;
        //the reflectivity for the model
        float reflectivity;
        //  the fresnel power, scale and bias for fresnel effect
        float fresnelPower;
        // fresnel scale and bias can be used to adjust the fresnel effect, make the effect stronger or weaker
        float fresnelScale;
        float fresnelBias;


        //Use the class from  CPI411 SimpleEngine
        Skybox skybox;
        //model, view, projection
        Matrix world, view, projection;

        //light position
        Vector3 lightPosition = new Vector3(0, 0, 1);
        //angle for the rotation of the model and light
        //float angle, angle2;
        //distance between camera and the model
        float distance;
        //store the previous state of mouse and keyboard for input
        MouseState previousMouseState;
        //store the current state of mouse and keyboard for input
        KeyboardState previousKeyboardState;
        int shaderID;      
        int skyboxIDValue;
        int modelID;
        //boolean variable for showing menu and shader info.
        //Texon is for changing the shader when the model has texture or not
        bool showMenu, showValue, texon;


        public Assignment02()
        {
            //initialize the variables 
            world = Matrix.Identity;
             view = Matrix.CreateLookAt( new Vector3(0, 0, 20),Vector3.Zero,Vector3.Up);
             projection = Matrix.CreatePerspectiveFieldOfView( MathHelper.ToRadians(90), 800f / 600f, 0.1f, 1000f);
             lightPosition = new Vector3(0, 0, 1);
             distance = 10.0f;
             shaderID = 0;
             skyboxIDValue = 0;
             modelID = 0;
             diffuseColor = new Vector4(1, 1, 1, 1);
             diffuseIntensity = 1.0f;
             specularColor = new Vector4(1, 1, 1, 1);
             shininess = 100.0f;
             etaRatio = 0.658f;
             fresnelEtaRatio = new Vector3(0.1f, 0.1f, 0.1f);
             reflectivity = 0.5f;
             fresnelPower = 2;
             fresnelScale = 15;
             fresnelBias = 0.5f;
             width = 1800;
             height = 1200;
            cameraAngleX = 0;
             cameraAngleY = 0;
            lightAngleX = 0;
            lightAngleY = 0;
            cameraOffset = new Vector3(0, 0, 0);
            cameraPosition = new Vector3(0, 0, 10);
            ambient = new Vector4(0.1f, 0.1f, 0.1f, 1);
            ambientIntensity = 0.1f;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.PreferredBackBufferWidth = width;
            graphics.PreferredBackBufferHeight = height;

           
            skyboxIDValue = 4;

        }

      
        protected override void Initialize()
        {
            

            base.Initialize();
        }

        
        protected override void LoadContent()
        {
             
            spriteBatch = new SpriteBatch(GraphicsDevice);
            //load the model, texture, effect and font used in the game
            font = Content.Load<SpriteFont>("Info");
            modelBunny = Content.Load<Model>("bunny");
            modelTorus = Content.Load<Model>("Torus");
            modelCube = Content.Load<Model>("Box");
            modelTeapot = Content.Load<Model>("Teapot");
            modelSphere = Content.Load<Model>("Sphere");
            modelHeli = Content.Load<Model>("Helicopter");
            //load the shader file for the model with texture and without texture
            effect = Content.Load<Effect>("Shader");
            effect2= Content.Load<Effect>("Reflection");
            model = Content.Load<Model>("cube");
            texture = Content.Load<Texture2D>("HelicopterTexture");

            model = modelCube;
            texon = false;

        }
  
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            //use the keyboard to control the light position, camera position, shader, skybox texture,
            //model and the parameters in shader file
            bool shift = false;
            //check if the shift key is pressed for changing the parameter faster
            Keys[] pressedKeys = Keyboard.GetState().GetPressedKeys();
            //if shift is pressed, the parameter will change faster than normal
            foreach (Keys key in pressedKeys)
                if (key == Keys.LeftShift || key == Keys.RightShift) shift = true;
            //use arrow key to control the light position, and use S key to reset the camera position and light position
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {

                lightAngleX += 1.0f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down)) 
            { lightAngleX -= 1.0f; 
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                lightAngleY += 1.0f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            { lightAngleY -= 1.0f; 
            }

            if (Keyboard.GetState().IsKeyDown(Keys.S)) 
            { 
                distance = 10; 
                cameraAngleX = cameraAngleY = lightAngleX = lightAngleY = 0; 
            }
            //use number key to change the model, 1 - 6 to change the shader,
            //7-0 and P to change the skybox texture
            if (Keyboard.GetState().IsKeyDown(Keys.D1)) 
            { model = modelCube; texon = false; 
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D2)) 
            { model = modelSphere; 
                texon = false; 
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D3)) 
            { model = modelTorus; 
                texon = false; 
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D4)) 
            { model = modelTeapot;
                texon = false; 
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D5)) 
            {
                model = modelBunny; 
                texon = false;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D6))
            { model = modelHeli;
                texon = true; 
            }
            //use plus and minus key to change the reflectivity of the model in shader file
            if (Keyboard.GetState().IsKeyDown(Keys.OemPlus))
            {
                reflectivity += 0.01f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.OemMinus))
            {
                reflectivity -= 0.01f;
            }
            //use F7-F10 to change the shader for the model with texture and without texture
            if (Keyboard.GetState().IsKeyDown(Keys.F7))
            {
                shaderID = 0;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F8)) 
            { 
                shaderID = 1;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F9))
            { 
                shaderID = 2; 
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F10))
            {
                shaderID = 3;
            }
            // use 7-0 and P key to change the skybox texture
            if (Keyboard.GetState().IsKeyDown(Keys.D7))
            {
                skyboxIDValue = 0;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D8))
            { skyboxIDValue = 1; 
            }

            if (Keyboard.GetState().IsKeyDown(Keys.D9))
            {
                skyboxIDValue = 2;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D0))
            {
                skyboxIDValue = 3;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.P))
            {
                skyboxIDValue = 4;
            }
            //use R,G,B key to change the fresnel eta ratio in shader file, and use Q,W,E key to change the fresnel power, scale and bias in shader file.
            //Shift key can be used to decrease the parameter faster
            if (Keyboard.GetState().IsKeyDown(Keys.R) && !shift)
            {
                fresnelEtaRatio.X += 0.04f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.G) && !shift)
            {
                fresnelEtaRatio.Y += 0.04f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.B) && !shift)
            {
                fresnelEtaRatio.Z += 0.04f;
            }
            //if shift key is pressed, the parameter will decrease faster than normal
            if (Keyboard.GetState().IsKeyDown(Keys.R) && shift)
            {
                fresnelEtaRatio.X -= 0.04f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.G) && shift)
            {
                fresnelEtaRatio.Y -= 0.04f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.B) && shift)
            {
                fresnelEtaRatio.Z -= 0.04f;
            }
            //use Q,W,E key to change the fresnel power, scale and bias in shader file.
            //Shift key can be used to decrease the parameter faster
            if (Keyboard.GetState().IsKeyDown(Keys.Q) && !shift)
            {
                fresnelPower += 0.2f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.W) && !shift)
            {
                fresnelScale += 1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.E) && !shift) 
            { 
                fresnelBias += 0.02f;
            }

            //if shift key is pressed, the parameter will decrease faster than normal
            if (Keyboard.GetState().IsKeyDown(Keys.Q) && shift) fresnelPower -= 0.2f;
            if (Keyboard.GetState().IsKeyDown(Keys.W) && shift) fresnelScale -= 1f;
            if (Keyboard.GetState().IsKeyDown(Keys.E) && shift) fresnelBias -= 0.02f;

            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.H) && previousKeyboardState.IsKeyUp(Keys.H))
                showValue = !showValue;

            if (keyboardState.IsKeyDown(Keys.OemQuestion) && previousKeyboardState.IsKeyUp(Keys.OemQuestion))
                showMenu = !showMenu;

            
            previousKeyboardState = keyboardState;

            if (previousMouseState.RightButton == ButtonState.Pressed && Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                cameraOffset.Z += (Mouse.GetState().X - previousMouseState.X) / 100f;
                distance += (Mouse.GetState().X - previousMouseState.X) / 100f;
            }
            if (previousMouseState.LeftButton == ButtonState.Pressed && Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                cameraAngleX += (previousMouseState.X - Mouse.GetState().X);
                cameraAngleY += (previousMouseState.Y - Mouse.GetState().Y);
            }
            if (previousMouseState.MiddleButton == ButtonState.Pressed && Mouse.GetState().MiddleButton == ButtonState.Pressed)
            {
                cameraOffset.X += (Mouse.GetState().X - previousMouseState.X) / 100f;
                cameraOffset.Y -= (Mouse.GetState().Y - previousMouseState.Y) / 100f;
            }
            cameraPosition = Vector3.Transform(new Vector3(0, 0, distance),
                Matrix.CreateRotationX(MathHelper.ToRadians(cameraAngleY)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(cameraAngleX)));

            view = Matrix.CreateLookAt(
                cameraPosition,
                Vector3.Zero,
                Vector3.Transform(
                    Vector3.UnitY,
                    Matrix.CreateRotationX(MathHelper.ToRadians(cameraAngleY)) *
                    Matrix.CreateRotationY(MathHelper.ToRadians(cameraAngleX))
                    )
                );

            lightPosition = Vector3.Transform(new Vector3(0, 0, 1),
                Matrix.CreateRotationX(MathHelper.ToRadians(lightAngleX)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(lightAngleY)));

            previousMouseState = Mouse.GetState();

            base.Update(gameTime);
        }

         
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = new DepthStencilState();
            RasterizerState originalRasterizerState = graphics.GraphicsDevice.RasterizerState;

            RasterizerState ras = new RasterizerState();
            ras.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = ras;

            if (skyboxIDValue == 0)
            {
                string[] skyboxTextures =
                {
                    "test_posx", "test_negx",
                    "test_posy", "test_negy",
                    "test_posz", "test_negz",
                };
                skybox = new Skybox(skyboxTextures, Content, GraphicsDevice);
            }
            else if (skyboxIDValue == 1)
            {
                string[] skyboxTextures =
                {
                    "arch_posx", "arch_negx",
                    "arch_posy", "arch_negy",
                    "arch_posz", "arch_negz",
                };
                skybox = new Skybox(skyboxTextures, Content, GraphicsDevice);
            }
            else if (skyboxIDValue == 2)
            {
                string[] skyboxTextures =
                {
                    "grandcanyon_posx", "grandcanyon_negx",
                    "grandcanyon_posy", "grandcanyon_negy",
                    "grandcanyon_posz", "grandcanyon_negz",
                };
                skybox = new Skybox(skyboxTextures, Content, GraphicsDevice);
            }
            else if (skyboxIDValue == 3)
            {
                string[] skyboxTextures =
                {
                    "hills_posx", "hills_negx",
                    "hills_posy", "hills_negy",
                    "hills_posz", "hills_negz",
                };
                skybox = new Skybox(skyboxTextures, Content, GraphicsDevice);
            }
            else if (skyboxIDValue == 4)
            {
                string[] skyboxTextures =
                {
                    "nvlobby_new_posx", "nvlobby_new_negx",
                    "nvlobby_new_posy", "nvlobby_new_negy",
                    "nvlobby_new_posz", "nvlobby_new_negz",
                };
                skybox = new Skybox(skyboxTextures, Content, GraphicsDevice);
            }

            skybox.Draw(view, projection, cameraPosition);
            graphics.GraphicsDevice.RasterizerState = originalRasterizerState;
            DrawModelWithEffect();
            spriteBatch.Begin();
            if (showMenu) showHelp();
            if (showValue) showInfo();
            spriteBatch.End();

            base.Draw(gameTime);
        }
        //Menu Help
        public string HelpMenu()
        {
            string helpText =
            "Mouse: Views\n" +
            "Arrow Keys: Light Position\n" +
            "Type S Key: Reset Camera\n" +
            "Type 1,2,3,4,5,6: Model Change\n" +
            "Type 7,8,9,0,P: Skybox Texture change\n" +
            "Type F7,F8,F9,F10: Shader Change\n" +
            "Type plus, minus: Reflectivity Change\n" +
            "Type R, G, B Keys (+shift decreases): Color Change\n" +
            "Type Q, W, E Keys(+shift decreases): Fresnel Power, Scale, Bias change\n" +
            "Type H Key: Display Parameters\n" +
            "Type ? Key: Help Menu";

            return helpText;
        }

        //Shader info
        public string ShaderInfo()
        {
            string shaderText =
            "Light Angles X(" + lightAngleX + "), Y(" + lightAngleY + ")\n" +
            "Light Pos " + display(lightPosition) + "\n" +
            "Camera Angles X(" + cameraAngleX + "), Y(" + cameraAngleY + ")\n" +
            "Camera Pos " + display(cameraPosition) + "\n" +
            "Shader: " + shaderID + "\n" +
            "Reflectivity: " + reflectivity + "\n" +
            "Fresnel Power: " + fresnelPower + "\n" +
            "Fresnel Bias: " + fresnelBias + "\n" +
            "Fresnel Scale: " + fresnelScale + "\n" +
            "Fresnel etaRatio: " + display(fresnelEtaRatio);

            return shaderText;
        }


        // show help menu and shader info on the screen
        private void showHelp()
        {
            spriteBatch.DrawString(font, HelpMenu(), new Vector2(0, 0), Color.White);
        }

        private void showInfo()
        {
            spriteBatch.DrawString(font, ShaderInfo(), new Vector2(600, 0), Color.White);
        }

       
        private string display(Vector3 v)
        {
            return "(" + v.X.ToString("0.00") + "," + v.Y.ToString("0.00") + "," + v.Z.ToString("0.00") + ")";
        }
        void DrawModelWithEffect()
        {
            Effect activeEffect;

            if (texon)
            {
                activeEffect = effect2;
                activeEffect.CurrentTechnique = activeEffect.Techniques[0];
            }
            else
            {
                activeEffect = effect;
                activeEffect.CurrentTechnique = activeEffect.Techniques[shaderID];
            }

            foreach (EffectPass pass in activeEffect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        activeEffect.Parameters["World"].SetValue(mesh.ParentBone.Transform);
                        activeEffect.Parameters["View"].SetValue(view);
                        activeEffect.Parameters["Projection"].SetValue(projection);

                        Matrix wit = Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform));

                        if (activeEffect.Parameters["WorldInverseTranspose"] != null)
                            activeEffect.Parameters["WorldInverseTranspose"].SetValue(wit);

                        if (activeEffect.Parameters["CameraPosition"] != null)
                            activeEffect.Parameters["CameraPosition"].SetValue(cameraPosition);

                        if (activeEffect.Parameters["environmentMap"] != null)
                            activeEffect.Parameters["environmentMap"].SetValue(skybox.skyBoxTexture);

                        if (texon)
                        {
                            if (activeEffect.Parameters["decalMap"] != null)
                                activeEffect.Parameters["decalMap"].SetValue(texture);
                        }
                        else
                        {
                            if (activeEffect.Parameters["DiffuseLightDirection"] != null)
                                activeEffect.Parameters["DiffuseLightDirection"].SetValue(lightPosition);

                            if (activeEffect.Parameters["AmbientColor"] != null)
                                activeEffect.Parameters["AmbientColor"].SetValue(ambient);

                            if (activeEffect.Parameters["AmbientIntensity"] != null)
                                activeEffect.Parameters["AmbientIntensity"].SetValue(ambientIntensity);

                            if (activeEffect.Parameters["DiffuseColor"] != null)
                                activeEffect.Parameters["DiffuseColor"].SetValue(diffuseColor);

                            if (activeEffect.Parameters["DiffuseIntensity"] != null)
                                activeEffect.Parameters["DiffuseIntensity"].SetValue(diffuseIntensity);

                            if (activeEffect.Parameters["SpecularColor"] != null)
                                activeEffect.Parameters["SpecularColor"].SetValue(specularColor);

                            if (activeEffect.Parameters["SpecularIntensity"] != null)
                                activeEffect.Parameters["SpecularIntensity"].SetValue(shininess);

                            if (activeEffect.Parameters["EtaRatio"] != null)
                                activeEffect.Parameters["EtaRatio"].SetValue(etaRatio);

                            if (activeEffect.Parameters["FresnelEtaRatio"] != null)
                                activeEffect.Parameters["FresnelEtaRatio"].SetValue(fresnelEtaRatio);

                            if (activeEffect.Parameters["Reflectivity"] != null)
                                activeEffect.Parameters["Reflectivity"].SetValue(reflectivity);

                            if (activeEffect.Parameters["FresnelBias"] != null)
                                activeEffect.Parameters["FresnelBias"].SetValue(fresnelBias);

                            if (activeEffect.Parameters["FresnelScale"] != null)
                                activeEffect.Parameters["FresnelScale"].SetValue(fresnelScale);

                            if (activeEffect.Parameters["FresnelPower"] != null)
                                activeEffect.Parameters["FresnelPower"].SetValue(fresnelPower);
                        }

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
    }
}
