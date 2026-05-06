using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using CPI411.SimpleEngine;

namespace Assignment04
{
    public class Assignment04 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Effect effect;

        int currentTexture = 0; // -1 no texture (Phong), 0 smoke, 1 water, 2 fire
        Matrix world = Matrix.Identity;
        Matrix view;
        Matrix projection;

        Vector3 cameraPosition;
        Vector3 cameraTarget = Vector3.Zero;
        Vector3 lightPosition;

        KeyboardState preKey;
        MouseState preMouse;

        float angle = 0f;
        float angle2 = 0f;
        float angleL = 3.18f;
        float angleL2 = 1.68f;
        float distance = 30f;

        Matrix lightView;
        Matrix lightProjection;
        Matrix invertCamera;

        ParticleManager particleManager;
        Vector3 particlePosition;
        Random random;

        Texture2D[] textures;
        Model plane;

        int emitterShape = 0;   // 0 single, 1 sine, 2 circle, 3 square
        int fountainType = 0;   // 0 basic, 1 medium, 2 advanced
        int counter = 0;
        int time = 0;

        float bounciness = 1f;
        float friction = 0.5f;
        Vector3 wind = Vector3.Zero;
        bool usingWind = false;

        float gravity = -8f;
        float randomness = 5f;
        bool basicUpwards = true;
        bool usingRandomness = true;

        Vector3 velocityOverride = Vector3.Zero;
        bool usingUserVelocity = false;

        int lifeSpan = 4;
        int age = 0;
        bool usingAge = false;

        bool showHelp = true;
        bool showInformation = true;

        public Assignment04()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.GraphicsProfile = GraphicsProfile.HiDef;

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.ApplyChanges();

            projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45f),
                1280f / 720f,
                0.1f,
                100f);
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            textures = new Texture2D[3];
            textures[0] = Content.Load<Texture2D>("smoke");
            textures[1] = Content.Load<Texture2D>("water");
            textures[2] = Content.Load<Texture2D>("fire");

            effect = Content.Load<Effect>("ParticleShader");
            font = Content.Load<SpriteFont>("font");
            plane = Content.Load<Model>("Plane");

            random = new Random();
            particleManager = new ParticleManager(GraphicsDevice, 1000);
            particlePosition = Vector3.Zero;
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState key = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                key.IsKeyDown(Keys.Escape))
                Exit();

            // Light controls
            if (key.IsKeyDown(Keys.Left)) angleL += 0.02f;
            if (key.IsKeyDown(Keys.Right)) angleL -= 0.02f;
            if (key.IsKeyDown(Keys.Up)) angleL2 += 0.02f;
            if (key.IsKeyDown(Keys.Down)) angleL2 -= 0.02f;

            // Reset
            if (key.IsKeyDown(Keys.S) && !preKey.IsKeyDown(Keys.S))
            {
                angle = 0f;
                angle2 = 0f;
                angleL = 3.18f;
                angleL2 = 1.68f;
                distance = 30f;
                cameraTarget = Vector3.Zero;

                counter = 0;
                time = 0;
                bounciness = 1f;
                friction = 0.5f;
                wind = Vector3.Zero;
                usingWind = false;
                gravity = -8f;
                randomness = 5f;
                basicUpwards = true;
                usingRandomness = true;
                velocityOverride = Vector3.Zero;
                usingUserVelocity = false;
                lifeSpan = 4;
                age = 0;
                usingAge = false;
            }

            // Mouse camera controls
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
            lightProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1f, 1f, 100f);

            // Spawn particles
            if (key.IsKeyDown(Keys.P))
            {
                SpawnParticles();
            }

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            particleManager.Update(dt);

            // Fountain physics
            if (fountainType == 1)
            {
                foreach (Particle particle in particleManager.particles)
                {
                    if (!particle.IsActive()) continue;

                    if (particle.Position.Y < 1.75f)
                    {
                        particle.Position = new Vector3(particle.Position.X, 1.75f, particle.Position.Z);
                        particle.Velocity = new Vector3(particle.Velocity.X, 0f, particle.Velocity.Z);
                        particle.Acceleration = new Vector3(particle.Acceleration.X, 0f, particle.Acceleration.Z);
                    }

                    if (particle.Position.Y == 1.75f)
                    {
                        particle.Velocity = new Vector3(
                            particle.Velocity.X * (float)Math.Pow(1.0 - friction, 0.03),
                            0f,
                            particle.Velocity.Z * (float)Math.Pow(1.0 - friction, 0.03));
                    }
                }
            }
            else if (fountainType == 2)
            {
                foreach (Particle particle in particleManager.particles)
                {
                    if (!particle.IsActive()) continue;

                    particle.Acceleration = new Vector3(particle.Acceleration.X, gravity, particle.Acceleration.Z);

                    if (particle.Position.Y < 1.75f)
                    {
                        particle.Position = new Vector3(particle.Position.X, 1.75f, particle.Position.Z);
                        particle.Velocity = new Vector3(
                            particle.Velocity.X,
                            -particle.Velocity.Y * bounciness,
                            particle.Velocity.Z);
                    }
                }
            }

            // Wind
            if (usingWind)
            {
                foreach (Particle particle in particleManager.particles)
                {
                    if (!particle.IsActive()) continue;

                    if (usingRandomness && time % 60 == 0)
                    {
                        wind = new Vector3(
                            (float)((random.NextDouble() - 0.5) * 0.05) * randomness,
                            0f,
                            (float)((random.NextDouble() - 0.5) * 0.05) * randomness);
                    }

                    particle.Position += wind;
                }
            }

            time++;

            // Override age
            if (usingAge)
            {
                foreach (Particle particle in particleManager.particles)
                {
                    if (particle.IsActive())
                        particle.Age = age;
                }
            }

            // Texture controls
            if (key.IsKeyDown(Keys.D1)) currentTexture = -1; // no texture = Phong
            if (key.IsKeyDown(Keys.D2)) currentTexture = 0;  // smoke
            if (key.IsKeyDown(Keys.D3)) currentTexture = 1;  // water
            if (key.IsKeyDown(Keys.D4)) currentTexture = 2;  // fire

            // Fountain controls
            if (key.IsKeyDown(Keys.F1)) fountainType = 0;
            if (key.IsKeyDown(Keys.F2)) fountainType = 1;
            if (key.IsKeyDown(Keys.F3)) fountainType = 2;
            if (key.IsKeyDown(Keys.F4) && !preKey.IsKeyDown(Keys.F4))
                emitterShape = (emitterShape + 1) % 4;

            // Parameter controls
            if (key.IsKeyDown(Keys.F))
            {
                friction = ShiftHeld(key)
                    ? MathHelper.Clamp(friction + 0.005f, 0f, 1f)
                    : MathHelper.Clamp(friction - 0.005f, 0f, 1f);
            }

            if (key.IsKeyDown(Keys.B))
            {
                bounciness = ShiftHeld(key)
                    ? MathHelper.Clamp(bounciness + 0.005f, 0f, 1f)
                    : MathHelper.Clamp(bounciness - 0.005f, 0f, 1f);
            }

            if (key.IsKeyDown(Keys.G))
            {
                gravity = ShiftHeld(key)
                    ? MathHelper.Clamp(gravity + 0.05f, -20f, 20f)
                    : MathHelper.Clamp(gravity - 0.05f, -20f, 20f);
            }

            if (key.IsKeyDown(Keys.R))
            {
                randomness = ShiftHeld(key)
                    ? MathHelper.Clamp(randomness + 0.05f, 0f, 20f)
                    : MathHelper.Clamp(randomness - 0.05f, 0f, 20f);
            }

            if (key.IsKeyDown(Keys.X))
            {
                wind.X = ShiftHeld(key)
                    ? MathHelper.Clamp(wind.X + 0.005f, -0.1f, 0.1f)
                    : MathHelper.Clamp(wind.X - 0.005f, -0.1f, 0.1f);
            }

            if (key.IsKeyDown(Keys.Y))
            {
                wind.Y = ShiftHeld(key)
                    ? MathHelper.Clamp(wind.Y + 0.005f, -0.1f, 0.1f)
                    : MathHelper.Clamp(wind.Y - 0.005f, -0.1f, 0.1f);
            }

            if (key.IsKeyDown(Keys.Z))
            {
                wind.Z = ShiftHeld(key)
                    ? MathHelper.Clamp(wind.Z + 0.005f, -0.1f, 0.1f)
                    : MathHelper.Clamp(wind.Z - 0.005f, -0.1f, 0.1f);
            }

            if (key.IsKeyDown(Keys.U))
            {
                velocityOverride.X = ShiftHeld(key)
                    ? MathHelper.Clamp(velocityOverride.X + 0.05f, -5f, 5f)
                    : MathHelper.Clamp(velocityOverride.X - 0.05f, -5f, 5f);
            }

            if (key.IsKeyDown(Keys.I))
            {
                velocityOverride.Y = ShiftHeld(key)
                    ? MathHelper.Clamp(velocityOverride.Y + 0.05f, -5f, 5f)
                    : MathHelper.Clamp(velocityOverride.Y - 0.05f, -5f, 5f);
            }

            if (key.IsKeyDown(Keys.O))
            {
                velocityOverride.Z = ShiftHeld(key)
                    ? MathHelper.Clamp(velocityOverride.Z + 0.05f, -5f, 5f)
                    : MathHelper.Clamp(velocityOverride.Z - 0.05f, -5f, 5f);
            }

            if (key.IsKeyDown(Keys.T) && !preKey.IsKeyDown(Keys.T))
                usingRandomness = !usingRandomness;

            if (key.IsKeyDown(Keys.D) && !preKey.IsKeyDown(Keys.D))
                basicUpwards = !basicUpwards;

            if (key.IsKeyDown(Keys.W) && !preKey.IsKeyDown(Keys.W))
                usingWind = !usingWind;

            if (key.IsKeyDown(Keys.V) && !preKey.IsKeyDown(Keys.V))
                usingUserVelocity = !usingUserVelocity;

            if (key.IsKeyDown(Keys.L) && !preKey.IsKeyDown(Keys.L))
            {
                lifeSpan = ShiftHeld(key)
                    ? (int)MathHelper.Clamp(lifeSpan + 1, 0, 20)
                    : (int)MathHelper.Clamp(lifeSpan - 1, 0, 20);
            }

            if (key.IsKeyDown(Keys.A) && !preKey.IsKeyDown(Keys.A))
            {
                age = ShiftHeld(key)
                    ? MathHelper.Clamp(age + 1, 0, lifeSpan)
                    : MathHelper.Clamp(age - 1, 0, lifeSpan);
            }

            age = MathHelper.Clamp(age, 0, lifeSpan);

            if (key.IsKeyDown(Keys.Q) && !preKey.IsKeyDown(Keys.Q))
                usingAge = !usingAge;

            if (key.IsKeyDown(Keys.OemQuestion) && !preKey.IsKeyDown(Keys.OemQuestion))
                showHelp = !showHelp;

            if (key.IsKeyDown(Keys.H) && !preKey.IsKeyDown(Keys.H))
                showInformation = !showInformation;

            invertCamera = Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle);

            preKey = key;
            base.Update(gameTime);
        }

        private bool ShiftHeld(KeyboardState key)
        {
            return key.IsKeyDown(Keys.LeftShift) || key.IsKeyDown(Keys.RightShift);
        }

        private void SpawnParticles()
        {
            Particle particle = null;

            if (emitterShape == 0)
            {
                particle = particleManager.getNext();
                particle.Position = particlePosition;
                particle.MaxAge = lifeSpan;
                particle.Velocity = Vector3.Zero;
                ApplyFountainBehavior(particle);
                particle.Init();
            }
            else if (emitterShape == 1)
            {
                particle = particleManager.getNext();
                particle.Position = particlePosition;
                particle.MaxAge = lifeSpan;
                particle.Position += Vector3.UnitX * 2f *
                                     (float)Math.Sin(MathHelper.ToRadians(5f * counter));
                particle.Velocity = Vector3.Zero;
                counter++;
                ApplyFountainBehavior(particle);
                particle.Init();
            }
            else if (emitterShape == 2)
            {
                for (int i = 0; i < 60; i++)
                {
                    particle = particleManager.getNext();
                    particle.Position = particlePosition;
                    particle.MaxAge = lifeSpan;
                    particle.Velocity = new Vector3(
                        10f * (float)Math.Sin(MathHelper.ToRadians(i * 6f)),
                        0f,
                        10f * (float)Math.Cos(MathHelper.ToRadians(i * 6f)));
                    ApplyFountainBehavior(particle);
                    particle.Init();
                }
            }
            else if (emitterShape == 3)
            {
                for (int x = 0; x <= 31; x++)
                {
                    for (int z = 0; z <= 31; z++)
                    {
                        bool edge =
                            x == 0 || x == 31 ||
                            (z == 0 && x > 0 && x < 31) ||
                            (z == 31 && x > 0 && x < 31);

                        if (!edge) continue;

                        particle = particleManager.getNext();
                        particle.Position = particlePosition;
                        particle.MaxAge = lifeSpan;
                        particle.Velocity = new Vector3(x - 16, 0f, z - 16);
                        ApplyFountainBehavior(particle);
                        particle.Init();
                    }
                }
            }
        }

        private void ApplyFountainBehavior(Particle particle)
        {
            if (fountainType == 0)
            {
                particle.Velocity += Vector3.UnitY * 5f * (basicUpwards ? 1f : -1f);
                particle.Acceleration = Vector3.Zero;
            }
            else if (fountainType == 1 || fountainType == 2)
            {
                particle.Velocity += new Vector3(
                    (float)((random.NextDouble() - 0.5) * 2.0) * randomness,
                    (float)((random.NextDouble() - 0.5) * 2.0) * randomness,
                    (float)((random.NextDouble() - 0.5) * 2.0) * randomness);
                particle.Acceleration = Vector3.UnitY * gravity;
            }

            if (usingUserVelocity)
                particle.Velocity = velocityOverride;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = new DepthStencilState();

            plane.Draw(world, view, projection);

            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            effect.CurrentTechnique = effect.Techniques[0];

            effect.Parameters["World"].SetValue(world);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            effect.Parameters["InverseCamera"].SetValue(invertCamera);

            // Phong shader parameters
            effect.Parameters["WorldInverseTranspose"]
                .SetValue(Matrix.Transpose(Matrix.Invert(world)));

            effect.Parameters["LightPosition"].SetValue(lightPosition);
            effect.Parameters["CameraPosition"].SetValue(cameraPosition);

            effect.Parameters["AmbientColor"].SetValue(new Vector4(1f, 1f, 1f, 1f));
            effect.Parameters["AmbientIntensity"].SetValue(0.2f);

            effect.Parameters["DiffuseColor"].SetValue(new Vector4(1f, 1f, 1f, 1f));
            effect.Parameters["DiffuseIntensity"].SetValue(0.8f);

            effect.Parameters["SpecularColor"].SetValue(new Vector4(1f, 1f, 1f, 1f));
            effect.Parameters["SpecularIntensity"].SetValue(1f);
            effect.Parameters["Shininess"].SetValue(16f);

            if (currentTexture >= 0 && currentTexture < textures.Length)
            {
                effect.Parameters["Texture"].SetValue(textures[currentTexture]);
                effect.Parameters["UseTexture"].SetValue(true);
            }
            else
            {
                effect.Parameters["UseTexture"].SetValue(false);
            }

            effect.CurrentTechnique.Passes[0].Apply();
            particleManager.Draw(GraphicsDevice);

            drawHelp();

            base.Draw(gameTime);
        }

        private void drawHelp()
        {
            if (!showHelp && !showInformation)
                return;

            spriteBatch.Begin();

            int y = 10;

            if (showInformation)
            {
                string fountainName = fountainType == 0 ? "Basic" :
                                      fountainType == 1 ? "Medium" : "Advanced";

                string emitterName = emitterShape == 0 ? "Single Particle" :
                                     emitterShape == 1 ? "Sine Curve" :
                                     emitterShape == 2 ? "Circle" : "Square";

                spriteBatch.DrawString(font, "Fountain: " + fountainName, new Vector2(10, y), Color.White); y += 20;
                spriteBatch.DrawString(font, "Emitter: " + emitterName, new Vector2(10, y), Color.White); y += 20;
                spriteBatch.DrawString(font, "Camera: " + cameraPosition, new Vector2(10, y), Color.White); y += 20;
                spriteBatch.DrawString(font, "Light: " + lightPosition, new Vector2(10, y), Color.White); y += 20;
                spriteBatch.DrawString(font, "Friction: " + friction.ToString("0.000"), new Vector2(10, y), Color.White); y += 20;
                spriteBatch.DrawString(font, "Bounciness: " + bounciness.ToString("0.000"), new Vector2(10, y), Color.White); y += 20;
                spriteBatch.DrawString(font, "Gravity: " + gravity.ToString("0.00"), new Vector2(10, y), Color.White); y += 20;
                spriteBatch.DrawString(font, "Randomness: " + randomness.ToString("0.00"), new Vector2(10, y), Color.White); y += 20;
                spriteBatch.DrawString(font, "Wind: " + wind, new Vector2(10, y), Color.White); y += 20;
                spriteBatch.DrawString(font, "Velocity Override: " + velocityOverride, new Vector2(10, y), Color.White); y += 20;
                spriteBatch.DrawString(font, "LifeSpan: " + lifeSpan, new Vector2(10, y), Color.White); y += 20;
                spriteBatch.DrawString(font, "Age Override: " + age, new Vector2(10, y), Color.White); y += 20;
            }

            if (showHelp)
            {
                y += 10;
                spriteBatch.DrawString(font, "Keys:", new Vector2(10, y), Color.Yellow); y += 20;
                spriteBatch.DrawString(font, "P = Spawn Particles", new Vector2(10, y), Color.White); y += 20;
                spriteBatch.DrawString(font, "D1 no texture (Phong), D2 smoke, D3 water, D4 fire", new Vector2(10, y), Color.White); y += 20;
                spriteBatch.DrawString(font, "F1 basic, F2 medium, F3 advanced, F4 change emitter", new Vector2(10, y), Color.White); y += 20;
                spriteBatch.DrawString(font, "W wind, V velocity override, T randomness toggle, D up/down toggle", new Vector2(10, y), Color.White); y += 20;
                spriteBatch.DrawString(font, "F friction, B bounce, G gravity, R randomness", new Vector2(10, y), Color.White); y += 20;
                spriteBatch.DrawString(font, "X/Y/Z wind axis, U/I/O velocity axis", new Vector2(10, y), Color.White); y += 20;
                spriteBatch.DrawString(font, "L life span, A age, Q age override", new Vector2(10, y), Color.White); y += 20;
                spriteBatch.DrawString(font, "? help on/off, H info on/off, S reset", new Vector2(10, y), Color.White); y += 20;
                spriteBatch.DrawString(font, "Mouse: Left rotate, Right zoom, Middle pan", new Vector2(10, y), Color.White); y += 20;
            }

            spriteBatch.End();
        }
    }
}