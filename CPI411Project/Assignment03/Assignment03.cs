using CPI411.SimpleEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Assignment03
{
    public class Assignment03 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Model model;
        int pic;
        Texture2D texture;
        Effect effect;
        Skybox skybox;

        KeyboardState previousKeyboardState;
        MouseState previousMouseState;

        float bumpHeight = 4.0f;
        float etaRatio = 0.658f;

        Matrix world = Matrix.Identity;
        Matrix view = Matrix.CreateLookAt(
            new Vector3(0, 0, 20),
            Vector3.Zero,
            Vector3.Up);

        Matrix projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(90), 800f / 600f, 0.1f, 100f);

        Vector3 cameraTarget = Vector3.Zero;
        float cameraDistance = 20.0f;

        Vector3 cameraPosition = new Vector3(0, 0, 10);
        Vector3 lightPosition = new Vector3(0, 1, 0);

        float angle, angle2;
        float angleL, angleL2;

        int shaderMode = 1;

        float normalMapRepeatU = 1.82f;
        float normalMapRepeatV = 2.00f;

        int mipmap = 0;
        int selfShadow = 0;
        int normalizeTangent = 0;
        int normalizeNormalMap = 0;
        int splitNormalization = 0;

        bool boolHelp = true;
        bool boolInfo = true;

        SpriteFont font;

        public Assignment03()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("font");

            string[] skyboxTextures =
            {
                "nvlobby_new_posx", "nvlobby_new_negx",
                "nvlobby_new_posy", "nvlobby_new_negy",
                "nvlobby_new_posz", "nvlobby_new_negz",
            };

            skybox = new Skybox(skyboxTextures, Content, GraphicsDevice);

            model = Content.Load<Model>("Torus");
            effect = Content.Load<Effect>("BumpNomal");
            texture = Content.Load<Texture2D>("crossHatch");
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboard = Keyboard.GetState();
            MouseState currentMouseState = Mouse.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                keyboard.IsKeyDown(Keys.Escape))
            {
                Exit();
            }
 
            if (currentMouseState.LeftButton == ButtonState.Pressed &&
                previousMouseState.LeftButton == ButtonState.Pressed)
            {
                angle -= (previousMouseState.X - currentMouseState.X) / 100f;
                angle2 -= (previousMouseState.Y - currentMouseState.Y) / 100f;
            }
 
            if (currentMouseState.RightButton == ButtonState.Pressed &&
                previousMouseState.RightButton == ButtonState.Pressed)
            {
                cameraDistance += (previousMouseState.Y - currentMouseState.Y) * 0.1f;
                cameraDistance = MathHelper.Clamp(cameraDistance, 2.0f, 100.0f);
            }

            
            if (currentMouseState.MiddleButton == ButtonState.Pressed &&
                previousMouseState.MiddleButton == ButtonState.Pressed)
            {
                float moveX = (currentMouseState.X - previousMouseState.X) * 0.05f;
                float moveY = (currentMouseState.Y - previousMouseState.Y) * 0.05f;

                cameraTarget += new Vector3(-moveX, moveY, 0);
            }

            if (keyboard.IsKeyDown(Keys.F1) && previousKeyboardState.IsKeyUp(Keys.F1))
            {
                effect = Content.Load<Effect>("BumpNomal");
                shaderMode = 1;
                pic = 0;
                splitNormalization = 0;
            }

            if (keyboard.IsKeyDown(Keys.F2) && previousKeyboardState.IsKeyUp(Keys.F2))
            {
                effect = Content.Load<Effect>("NormalRGB");
                shaderMode = 2;
                pic = 0;
                splitNormalization = 0;
            }

            if (keyboard.IsKeyDown(Keys.F3) && previousKeyboardState.IsKeyUp(Keys.F3))
            {
                effect = Content.Load<Effect>("BumpMap");
                shaderMode = 3;
                pic = 0;
                splitNormalization = 0;
            }

            if (keyboard.IsKeyDown(Keys.F4) && previousKeyboardState.IsKeyUp(Keys.F4))
            {
                effect = Content.Load<Effect>("BumpReflect");

                if (effect.Parameters["environmentMap"] != null)
                    effect.Parameters["environmentMap"].SetValue(skybox.skyBoxTexture);

                shaderMode = 4;
                pic = 0;
                splitNormalization = 0;
            }

            if (keyboard.IsKeyDown(Keys.F5) && previousKeyboardState.IsKeyUp(Keys.F5))
            {
                effect = Content.Load<Effect>("BumpRefract");

                if (effect.Parameters["environmentMap"] != null)
                    effect.Parameters["environmentMap"].SetValue(skybox.skyBoxTexture);

                if (effect.Parameters["EtaRatio"] != null)
                    effect.Parameters["EtaRatio"].SetValue(etaRatio);

                shaderMode = 5;
                pic = 0;
                splitNormalization = 0;
            }

            if (keyboard.IsKeyDown(Keys.F6) && previousKeyboardState.IsKeyUp(Keys.F6))
            {
                effect = Content.Load<Effect>("BumpMap");
                shaderMode = 6;
                normalizeTangent = 0;       
                normalizeNormalMap = 1;    
                splitNormalization = 0;
                pic = 0;
            }

            if (keyboard.IsKeyDown(Keys.F7) && previousKeyboardState.IsKeyUp(Keys.F7))
            {
                effect = Content.Load<Effect>("BumpMap");
                shaderMode = 7;
                normalizeTangent = 0;      
                normalizeNormalMap = 0;    
                splitNormalization = 0;
                pic = 0;
            }

            if (keyboard.IsKeyDown(Keys.F8) && previousKeyboardState.IsKeyUp(Keys.F8))
            {
                effect = Content.Load<Effect>("BumpMap");
                shaderMode = 8;
                normalizeTangent = 1;     
                normalizeNormalMap = 0;    
                splitNormalization = 0;
                pic = 0;
            }

            if (keyboard.IsKeyDown(Keys.F9) && previousKeyboardState.IsKeyUp(Keys.F9))
            {
                effect = Content.Load<Effect>("BumpMap");
                shaderMode = 9;
                normalizeTangent = 1;       
                normalizeNormalMap = 1;
                splitNormalization = 1;    
                pic = 0;
            }

            if (keyboard.IsKeyDown(Keys.F10) && previousKeyboardState.IsKeyUp(Keys.F10))
            {
                shaderMode = 10;
                pic = 5;
                splitNormalization = 0;
            }

            if (keyboard.IsKeyDown(Keys.Up)) angleL += 2.0f;
            if (keyboard.IsKeyDown(Keys.Down)) angleL -= 1.0f;
            if (keyboard.IsKeyDown(Keys.Left)) angleL2 += 2.0f;
            if (keyboard.IsKeyDown(Keys.Right)) angleL2 -= 1.0f;

            if (keyboard.IsKeyDown(Keys.D1)) texture = Content.Load<Texture2D>("art");
            if (keyboard.IsKeyDown(Keys.D2)) texture = Content.Load<Texture2D>("BumpTest");
            if (keyboard.IsKeyDown(Keys.D3)) texture = Content.Load<Texture2D>("crossHatch");
            if (keyboard.IsKeyDown(Keys.D4)) texture = Content.Load<Texture2D>("monkey");

            if (keyboard.IsKeyDown(Keys.D5))
            {
                try
                {
                    texture = Content.Load<Texture2D>("nm");
                }
                catch
                {
                    texture = Content.Load<Texture2D>("crossHatch");
                }
            }

            if (keyboard.IsKeyDown(Keys.D6)) texture = Content.Load<Texture2D>("round");
            if (keyboard.IsKeyDown(Keys.D7)) texture = Content.Load<Texture2D>("saint");
            if (keyboard.IsKeyDown(Keys.D8)) texture = Content.Load<Texture2D>("science");
            if (keyboard.IsKeyDown(Keys.D9)) texture = Content.Load<Texture2D>("square");

            bool shiftDown = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);

            float changeU = shiftDown ? -0.01f : 0.01f;
            float changeV = shiftDown ? -0.01f : 0.01f;
            float changeW = shiftDown ? -0.25f : 0.25f;

            if (keyboard.IsKeyDown(Keys.U))
                normalMapRepeatU += changeU;

            if (keyboard.IsKeyDown(Keys.V))
                normalMapRepeatV += changeV;

            if (keyboard.IsKeyDown(Keys.W))
                bumpHeight += changeW;

            normalMapRepeatU = MathHelper.Clamp(normalMapRepeatU, 0.01f, 20.0f);
            normalMapRepeatV = MathHelper.Clamp(normalMapRepeatV, 0.01f, 20.0f);
            bumpHeight = MathHelper.Clamp(bumpHeight, 0.0f, 20.0f);

            if (keyboard.IsKeyDown(Keys.M) && previousKeyboardState.IsKeyUp(Keys.M))
                mipmap = (mipmap == 0) ? 1 : 0;

            if (keyboard.IsKeyDown(Keys.P) && previousKeyboardState.IsKeyUp(Keys.P))
                selfShadow = (selfShadow == 0) ? 1 : 0;

            if (keyboard.IsKeyDown(Keys.T) && previousKeyboardState.IsKeyUp(Keys.T))
                normalizeTangent = (normalizeTangent == 0) ? 1 : 0;

            if (keyboard.IsKeyDown(Keys.N) && previousKeyboardState.IsKeyUp(Keys.N))
                normalizeNormalMap = (normalizeNormalMap == 0) ? 1 : 0;

            if (keyboard.IsKeyDown(Keys.S))
            {
                angle = 0;
                angle2 = 0;
                angleL = 0;
                angleL2 = 0;
                cameraTarget = Vector3.Zero;
                cameraDistance = 20.0f;
            }

            Matrix cameraRotation =
                Matrix.CreateRotationX(angle2) *
                Matrix.CreateRotationY(angle);

            cameraPosition = Vector3.Transform(
                new Vector3(0, 0, cameraDistance),
                cameraRotation) + cameraTarget;

            view = Matrix.CreateLookAt(
                cameraPosition,
                cameraTarget,
                Vector3.Up);

            lightPosition = Vector3.Transform(
                new Vector3(0, 0, 10),
                Matrix.CreateRotationX(MathHelper.ToRadians(angleL)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(angleL2)));

            if (effect.Parameters["height"] != null)
                effect.Parameters["height"].SetValue(bumpHeight);

            if (effect.Parameters["EtaRatio"] != null)
                effect.Parameters["EtaRatio"].SetValue(etaRatio);

            if (effect.Parameters["environmentMap"] != null)
                effect.Parameters["environmentMap"].SetValue(skybox.skyBoxTexture);

            if (effect.Parameters["UVScale"] != null)
                effect.Parameters["UVScale"].SetValue(new Vector2(normalMapRepeatU, normalMapRepeatV));

            if (effect.Parameters["useSelfShadowing"] != null)
                effect.Parameters["useSelfShadowing"].SetValue(selfShadow == 1);

            if (effect.Parameters["normalizeTangent"] != null)
                effect.Parameters["normalizeTangent"].SetValue(normalizeTangent == 1);

            if (effect.Parameters["normalizeNormalMap"] != null)
                effect.Parameters["normalizeNormalMap"].SetValue(normalizeNormalMap == 1);

            if (effect.Parameters["splitNormalization"] != null)
                effect.Parameters["splitNormalization"].SetValue(splitNormalization == 1);

            if (keyboard.IsKeyDown(Keys.H) && previousKeyboardState.IsKeyUp(Keys.H))
            {
                boolInfo = !boolInfo;
            }

            if (keyboard.IsKeyDown(Keys.OemQuestion) && previousKeyboardState.IsKeyUp(Keys.OemQuestion))
            {
                boolHelp = !boolHelp;
            }

            previousMouseState = currentMouseState;
            previousKeyboardState = keyboard;

            base.Update(gameTime);
        }

        public string ShaderInfo()
        {
            string shaderInfoText =
                "Shader Mode (F1/F2/F3/F4/F5/F6/F7/F8/F9/F10): " + shaderMode + "\n" +
                "BumpHeight (W): " + bumpHeight.ToString("0.00") + "\n" +
                "NormalMapRepeatU (U): " + normalMapRepeatU.ToString("0.00") + "\n" +
                "NormalMapRepeatV (V): " + normalMapRepeatV.ToString("0.00") + "\n" +
                "Mipmap (M): " + mipmap + "\n" +
                "SelfShadow (P): " + selfShadow + "\n" +
                "NormalizeTangent (T): " + normalizeTangent + "\n" +
                "NormalizeNormalMap (N): " + normalizeNormalMap + "\n" +
                "SplitNormalization (F9): " + splitNormalization;

            return shaderInfoText;
        }

        public string HelpMenu()
        {
            string helpText =
                "Camera Controls:\n" +
                " Left Mouse Drag  : Rotate Camera\n" +
                " Right Mouse Drag : Change Camera Distance\n" +
                " Middle Mouse Drag: Translate Camera\n\n" +

                "Lighting Controls:\n" +
                " Arrow Keys : Rotate Light\n" +
                " S          : Reset Camera and Light\n\n" +

                "Texture/Image Loader:\n" +
                " 1 - 9 : Switch Textures\n\n" +

                "Shader Modes:\n" +
                " F1  : Visualize Normals in Tangent Space\n" +
                " F2  : Visualize Normals in World Space\n" +
                " F3  : Bump Mapping\n" +
                " F4  : Reflective Bump Mapping\n" +
                " F5  : Refractive Bump Mapping\n" +
                " F6  : Unnorm Tangent, Norm Normal Map\n" +
                " F7  : Unnorm Tangent, Unnorm Normal Map\n" +
                " F8  : Norm Tangent, Unnorm Normal Map\n" +
                " F9  : Norm Tangent, Split Normalization\n" +
                " F10 : Display Normal Map Image\n\n" +

                "Bump Mapping Controls:\n" +
                " M       : Enable/Disable Mip-Mapping\n" +
                " U       : Change U Tiling (+ Shift to decrease)\n" +
                " V       : Change V Tiling (+ Shift to decrease)\n" +
                " W       : Change Bump Height (+ Shift to decrease)\n" +
                " P       : Toggle Self Shadow\n" +
                " T       : Toggle Normalize Tangent\n" +
                " N       : Toggle Normalize Normal Map\n\n" +

                "Menu Show/Hide:\n" +
                " ? : Show/Hide Help Menu\n" +
                " H : Show/Hide Shader Information\n";

            return helpText;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = new DepthStencilState();

            RasterizerState originalRasterizerState = GraphicsDevice.RasterizerState;

            RasterizerState ras = new RasterizerState();
            ras.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = ras;

            skybox.Draw(view, projection, cameraPosition);

            GraphicsDevice.RasterizerState = originalRasterizerState;

            effect.CurrentTechnique = effect.Techniques["Technique1"];

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        Matrix meshWorld = mesh.ParentBone.Transform;
 
                        effect.Parameters["World"]?.SetValue(meshWorld);
                        effect.Parameters["View"]?.SetValue(view);
                        effect.Parameters["Projection"]?.SetValue(projection);

                        Matrix worldInverseTransposeMatrix =
                            Matrix.Transpose(Matrix.Invert(meshWorld));
                        effect.Parameters["WorldInverseTranspose"]?.SetValue(worldInverseTransposeMatrix);

                        
                        effect.Parameters["CameraPosition"]?.SetValue(cameraPosition);
                        effect.Parameters["LightPosition"]?.SetValue(lightPosition);

                       
                        effect.Parameters["DiffuseIntensity"]?.SetValue(1.0f);
                        effect.Parameters["DiffuseColor"]?.SetValue(new Vector4(1, 1, 1, 1));

                        effect.Parameters["SpecularIntensity"]?.SetValue(1.0f);
                        effect.Parameters["SpecularColor"]?.SetValue(new Vector4(1, 1, 1, 1));
                        effect.Parameters["Shininess"]?.SetValue(100.0f);
                         
                        if (effect.Parameters["normalMap"] != null)
                            effect.Parameters["normalMap"].SetValue(texture);

                        if (effect.Parameters["decalMap"] != null)
                            effect.Parameters["decalMap"].SetValue(texture);

                        if (effect.Parameters["environmentMap"] != null)
                            effect.Parameters["environmentMap"].SetValue(skybox.skyBoxTexture);

                       
                        effect.Parameters["height"]?.SetValue(bumpHeight);

                        effect.Parameters["UVScale"]?.SetValue(
                            new Vector2(normalMapRepeatU, normalMapRepeatV));

                        effect.Parameters["MipMap"]?.SetValue(mipmap);

                        effect.Parameters["useSelfShadowing"]?.SetValue(selfShadow == 1);
                        effect.Parameters["normalizeTangent"]?.SetValue(normalizeTangent == 1);
                        effect.Parameters["normalizeNormalMap"]?.SetValue(normalizeNormalMap == 1);
                        effect.Parameters["splitNormalization"]?.SetValue(splitNormalization == 1);

                        if (effect.Parameters["EtaRatio"] != null)
                            effect.Parameters["EtaRatio"].SetValue(etaRatio);

                        
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

            
            if (pic == 5)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(
                    texture,
                    new Rectangle(
                        0,
                        0,
                        GraphicsDevice.Viewport.Width,
                        GraphicsDevice.Viewport.Height),
                    Color.White);
                spriteBatch.End();
            }

            
            spriteBatch.Begin();

            if (boolHelp)
                spriteBatch.DrawString(font, HelpMenu(), new Vector2(10, 10), Color.White);

            if (boolInfo)
                spriteBatch.DrawString(font, ShaderInfo(), new Vector2(900, 10), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }


    }
}