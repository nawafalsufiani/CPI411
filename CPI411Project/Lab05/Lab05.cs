using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CPI411.SimpleEngine;

/*
    Added reference of SimpleEngine under lab05 then right click depenencies 
and select add refirences and select simpleEngine

*/



namespace Lab05
{
    
    public class Lab05 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Effect effect;
        Model model;
        Texture2D texture;
        // *** Lab 05 **
        Skybox skybox;
        Matrix world = Matrix.Identity;
        Matrix view = Matrix.CreateLookAt(
                new Vector3(0, 0, 20),
                Vector3.Zero,
                Vector3.Up);
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(90), 800f / 600f, 0.1f, 100f);
        Vector3 cameraPosition = new Vector3(0, 0, 20);
        float angle, angle2;
        float distance = 20;
        MouseState previousMouseState;

        public Lab05()
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

            string[] skyboxTextures =
            {
                "SunsetPNG1", "SunsetPNG2",
                "SunsetPNG3", "SunsetPNG4",
                "SunsetPNG5", "SunsetPNG6",
            };
            skybox = new Skybox(skyboxTextures, Content, GraphicsDevice);
            effect = Content.Load<Effect>("Skybox");
        }



        protected override void UnloadContent()
        {
            
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                angle -= 0.03f;

            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                angle += 0.03f;

            }
            cameraPosition = distance * new Vector3((float)System.Math.Sin(angle), 0, (float)System.Math.Cos(angle));
            view = Matrix.CreateLookAt(cameraPosition, new Vector3(0, 0, 0), Vector3.UnitY);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["CameraPosition"].SetValue(cameraPosition);

            base.Update(gameTime);
        }

        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = new DepthStencilState();

            RasterizerState ras = new RasterizerState();
            ras.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = ras;

            skybox.Draw(view, projection, cameraPosition);


            base.Draw(gameTime);
        }
    }
}
