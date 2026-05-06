using CPI411.SimpleEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lab06
{
    public class Lab06 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // ***** Lab 5 (Skybox) *****
        Skybox skybox;
        string[] skyboxImageFiles =
        {"SunsetPNG2", "SunsetPNG1","SunsetPNG4", "SunsetPNG3","SunsetPNG6", "SunsetPNG5"
        };

        // ***** Lab 3 / Lab 6 *****
        Model model;
        Texture2D texture;
        Effect effect;

        Matrix world = Matrix.Identity;
        Matrix view;
        Matrix projection;

        Vector3 lightPosition = new Vector3(10, 10, 10); 
        Vector3 cameraPosition;

        float angle = 0;
        float angle2 = 0;
        float distance = 10;
        MouseState previousMouseState;

        /*
         * 
         * Constructor
         * */
        public Lab06()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // ***** For Shader File *****
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        }

        protected override void Initialize()
        {
            previousMouseState = Mouse.GetState();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Skybox
            skybox = new Skybox(skyboxImageFiles, Content, _graphics.GraphicsDevice);

            // Helicopter model + texture
            model = Content.Load<Model>("Helicopter");
            texture = Content.Load<Texture2D>("HelicopterTexture");

            // Reflection shader
            effect = Content.Load<Effect>("Reflection");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            /* Mouse orbit controls (same style as your screenshot)
             
             improtant to contole the mpuse
             */
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                angle += (previousMouseState.X - Mouse.GetState().X) / 100f;
                angle2 += (previousMouseState.Y - Mouse.GetState().Y) / 100f;
            }

            if (Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                distance += (previousMouseState.X - Mouse.GetState().X) / 100f;
                distance = MathHelper.Clamp(distance, 2f, 100f);
            }

            previousMouseState = Mouse.GetState();

            cameraPosition = Vector3.Transform(
                new Vector3(0, 0, distance),
                Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));

            Vector3 cameraUp = Vector3.Transform(
                Vector3.Up,
                Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));

            view = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, cameraUp);

            projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(90),
                1.33f,
                0.1f,
                100);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Step1: Set up the CullMode as None
            RasterizerState originalRasterizerState = _graphics.GraphicsDevice.RasterizerState;
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            _graphics.GraphicsDevice.RasterizerState = rasterizerState;

            /* Step2: Draw Skybox
             it is method on skyclass primary to draw the skybox to the screen from skybox class
            */
            skybox.Draw(view, projection, cameraPosition);

            // Step3: Reset the saved RasterizerState
            _graphics.GraphicsDevice.RasterizerState = originalRasterizerState;

            /* Step4: Draw 3D model

            We use the  DrawModelWithEffect to draw the 3D model insteal of adding drawing code here because it wil be messy and harder

            inshort it is a method to draw 3D model in it

            */

            DrawModelWithEffect();

            base.Draw(gameTime);
        }

        //Method to draw 3D model along th effect then be called in the Draw method
        void DrawModelWithEffect()
        {
            effect.CurrentTechnique = effect.Techniques[0];

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        effect.Parameters["World"].SetValue(mesh.ParentBone.Transform);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);

                        Matrix worldInverseTranspose =
                            Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform));

                        effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);
                        effect.Parameters["CameraPosition"].SetValue(cameraPosition);

                        /* for Part C: helicopter texture
                         *   Adding texture to shader
                         * 
                         * */
                        effect.Parameters["decalMap"].SetValue(texture);

                        /* 
                         *  adding maping to shader
                         * Cube map for reflection
                        */
                        effect.Parameters["environmentMap"].SetValue(skybox.skyBoxTexture);

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