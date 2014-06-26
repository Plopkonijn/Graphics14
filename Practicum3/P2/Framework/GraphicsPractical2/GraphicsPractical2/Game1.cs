using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace GraphicsPractical2
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        // Often used XNA objects
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private FrameRateCounter frameRateCounter;

        // Game objects and variables
        private Camera camera;
        
        // Model
        private Model model;
        private Material modelMaterial = new Material();

        // Quad
        private VertexPositionNormalTexture[] quadVertices;
        private short[] quadIndices;
        private Matrix quadTransform;
        private Texture2D Brick;

        Effect posteffect;

        //PostProces
        private Camera postprocescam;
        RenderTarget2D texturetarget;

        //Movement
        MouseState mstate;
        KeyboardState kstate;
        Vector3 ViewDir, SideWays, Upways;
        float scaling = 1f;

        public Game1()
        {
            this.graphics = new GraphicsDeviceManager(this);
            this.Content.RootDirectory = "Content";
            // Create and add a frame rate counter
            this.frameRateCounter = new FrameRateCounter(this);
            this.Components.Add(this.frameRateCounter);
        }

        protected override void Initialize()
        {
            //Texturetarget ...
            this.texturetarget = new RenderTarget2D(GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);

            // Copy over the device's rasterizer state to change the current fillMode
            this.GraphicsDevice.RasterizerState = new RasterizerState() { CullMode = CullMode.None };
            // Set up the window
            this.graphics.PreferredBackBufferWidth = 800;
            this.graphics.PreferredBackBufferHeight = 600;
            this.graphics.IsFullScreen = false;
            // Let the renderer draw and update as often as possible
            this.graphics.SynchronizeWithVerticalRetrace = false;
            this.IsFixedTimeStep = true;
            // Flush the changes to the device parameters to the graphics card
            this.graphics.ApplyChanges();
            // Initialize the camera
            this.camera = new Camera(new Vector3(50, 50, 100), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            
            // Initialize the movement vectors
            MoveVectorUpdate();




            this.IsMouseVisible = true;

            modelMaterial.DiffuseColor = Color.Red;
            modelMaterial.AmbientColor = Color.Red;
            modelMaterial.AmbientIntensity = 0.2f;
            //modelMaterial.SpecularColor = Color.White;
            modelMaterial.SpecularIntensity = 2.0f;
            modelMaterial.SpecularPower = 25.0f;

            //setupQuad();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a SpriteBatch object
            this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
            // Load the "Simple" effect
            Effect effect = this.Content.Load<Effect>("Effects/Simple");
            posteffect = this.Content.Load<Effect>("Effects/Postprocessing");
            // Load the model and let it use the "Simple" effect
            this.model = this.Content.Load<Model>("Models/femalehead");
            
            // Load the texture image
            this.Brick = this.Content.Load<Texture2D>("Textures/CobblestonesDiffuse");
           
            this.model.Meshes[0].MeshParts[0].Effect = effect;
            // Setup the quad
            this.setupQuad();
        }

        /// <summary>
        /// Sets up a 2 by 2 quad around the origin.
        /// </summary>
        private void setupQuad()
        {
            float scale = 50.0f;

            // Normal points up
            Vector3 quadNormal = new Vector3(0, 1, 0);

            this.postprocescam = new Camera(new Vector3(0, 0, 72), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            this.quadVertices = new VertexPositionNormalTexture[4];
            // Top left

            this.quadVertices[0].Position = new Vector3(-4, 3, 0);
            this.quadVertices[0].Normal = quadNormal;
            this.quadVertices[0].TextureCoordinate = new Vector2(0, 0); 
            // Top right
            this.quadVertices[1].Position = new Vector3(4, 3, 0);
            this.quadVertices[1].Normal = quadNormal;
            this.quadVertices[1].TextureCoordinate = new Vector2(1,0);
            // Bottom left
            this.quadVertices[2].Position = new Vector3(-4, -3, 0);
            this.quadVertices[2].Normal = quadNormal;
            this.quadVertices[2].TextureCoordinate = new Vector2(0, 1);
            // Bottom right
            this.quadVertices[3].Position = new Vector3(4, -3, 0);
            this.quadVertices[3].Normal = quadNormal;
            this.quadVertices[3].TextureCoordinate = new Vector2(1, 1);

            this.quadIndices = new short[] { 0, 1, 2, 1, 2, 3 };
            this.quadTransform = Matrix.CreateScale(scale);
        }

        int teller = 0;
        bool space_down = false;

        protected override void Update(GameTime gameTime)
        {
            float timeStep = (float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f;

            // Update the window title
            
            this.Window.Title = "XNA Renderer | FPS: " + this.frameRateCounter.FrameRate + "  " + mstate.ScrollWheelValue  ;
            
            
            //Movement
            mstate = Mouse.GetState();
            kstate = Keyboard.GetState();
            Movement(gameTime);
            

            if (kstate.IsKeyDown(Keys.Space))
            {
                if (space_down == false) teller++;
                space_down = true;
            }
            if (kstate.IsKeyUp(Keys.Space)) space_down = false;
            
           

            base.Update(gameTime);
        }

        private void Movement(GameTime gametime)
        {
            //Moving Trough Space
            Keys[] keys = kstate.GetPressedKeys();
            //Left
            for (int i = 0; i < keys.Length; i++)
            {
                switch (keys[i])
                {
                    case Keys.W:
                        this.camera.Eye += ViewDir;
                        this.camera.Focus += ViewDir;
                        goto default;
                    case Keys.A:
                        this.camera.Eye += SideWays;
                        this.camera.Focus += SideWays;
                        goto default;
                    case Keys.S:
                        this.camera.Eye -= ViewDir;
                        this.camera.Focus -= ViewDir;
                        goto default;
                    case Keys.D:
                        this.camera.Eye -= SideWays;
                        this.camera.Focus -= SideWays;
                        goto default;
                    case Keys.LeftControl:
                        this.camera.Eye -= Upways;
                        this.camera.Focus -= Upways;
                        goto default;
                    case Keys.LeftShift:
                         this.camera.Eye += Upways;
                         this.camera.Focus += Upways;
                        goto default;
                    case Keys.Up:
                        this.camera.Focus -= this.camera.Eye;
                        this.camera.Focus = Vector3.Transform(this.camera.Focus, Matrix.CreateFromAxisAngle(SideWays, -0.02f));
                        this.camera.Focus += this.camera.Eye;
                        goto default;
                    case Keys.Down:
                        this.camera.Focus -= this.camera.Eye;
                        this.camera.Focus = Vector3.Transform(this.camera.Focus, Matrix.CreateFromAxisAngle(SideWays, +0.02f));
                        this.camera.Focus += this.camera.Eye;
                        goto default;
                    case Keys.Left:
                        this.camera.Focus -= this.camera.Eye;
                        this.camera.Focus = Vector3.Transform(this.camera.Focus, Matrix.CreateFromAxisAngle(this.camera.UP, +0.02f));
                        this.camera.Focus += this.camera.Eye;
                        goto default;
                    case Keys.Right:
                        this.camera.Focus -= this.camera.Eye;
                        this.camera.Focus = Vector3.Transform(this.camera.Focus, Matrix.CreateFromAxisAngle(this.camera.UP, -0.02f));
                        this.camera.Focus += this.camera.Eye;
                        goto default;
                    default:
                        MoveVectorUpdate();
                        break;

                }
            }
            /*

            
            if (kstate.IsKeyDown(Keys.Up))
            {
                
                MoveVectorUpdate();
            }
            */

        }

        private void MoveVectorUpdate()
        {
            this.ViewDir = this.camera.Focus - this.camera.Eye;
            this.ViewDir.Normalize();

            this.SideWays = SideWays = Vector3.Cross(this.camera.UP, this.ViewDir);
            this.SideWays.Normalize();

            this.Upways = Vector3.Cross(this.ViewDir, this.SideWays);
            this.Upways.Normalize();

            this.ViewDir = Vector3.Multiply(this.ViewDir, scaling);
            this.SideWays = Vector3.Multiply(this.SideWays, scaling);
            this.Upways = Vector3.Multiply(this.Upways, scaling);

        }

        float angle = 0;
        Vector4[] kleuren;
        Vector3[] lights;

        protected override void Draw(GameTime gameTime)
        {

            if (teller == 0) this.GraphicsDevice.SetRenderTarget(texturetarget);  //setrendertarget naar texturetarget voor grayscale
            
            // Clear the screen in a predetermined color and clear the depth buffer
            this.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.DeepSkyBlue, 1.0f, 0);
            
            // Get the model's only mesh
            ModelMesh mesh = this.model.Meshes[0];
            Effect effect = mesh.Effects[0];

            angle = (float)gameTime.TotalGameTime.TotalMilliseconds;

            // Set the effect parameters
            effect.CurrentTechnique = effect.Techniques["Simple"];
            // Matrices for 3D perspective projection
            this.camera.SetEffectParameters(effect);
            effect.Parameters["World"].SetValue(Matrix.CreateScale(1F));
            effect.Parameters["InvTrWorld"].SetValue(Matrix.Transpose(Matrix.Invert(Matrix.CreateScale(1F))));
            
            switch (teller)
            {
                case 0:
                    init_grayscale();
                    break;
                case 1:
                    init_multiple_lights();
                    break;
                case 2:
                    effect.CurrentTechnique = effect.Techniques["Spotlight"];
                    init_grayscale();
                    effect.Parameters["cosine_alpha"].SetValue((float)(Math.Cos(Math.PI / (60+mstate.ScrollWheelValue/120))));
                    effect.Parameters["cosine_beta"].SetValue((float)(Math.Cos(Math.PI / (45+mstate.ScrollWheelValue/120))));
                    break;
                default:
                    teller = 0;
                    break;
            }






            effect.Parameters["SpecularColor"].SetValue(kleuren);
            for(int i = 0 ;i<lights.Length ; i++)
            lights[i] = Vector3.Transform(lights[i],Matrix.CreateRotationY(angle/(800-i*300)));

            effect.Parameters["LightSource"].SetValue(lights);
            effect.Parameters["EyePos"].SetValue(camera.Eye);
            modelMaterial.SetEffectParameters(effect);
            // Draw the model
            mesh.Draw();

            if (teller == 0) draw_grayscale_posteffect();
           base.Draw(gameTime);

        }

        private void init_grayscale()
        {
            lights = new Vector3[] { new Vector3(50, 20, 100) };
            kleuren = new Vector4[] { Color.White.ToVector4() };
        }

        private void init_multiple_lights()
        {
            lights = new Vector3[] { new Vector3(50, 50, 50), new Vector3(-30, 30, 0), new Vector3(50, -50, 50) };
            kleuren = new Vector4[] { Color.White.ToVector4(), Color.Green.ToVector4(), Color.Blue.ToVector4() };
        }

        private void draw_grayscale_posteffect()
        {
            //render to quad

            this.camera.SetEffectParameters(posteffect);
            posteffect.Parameters["World"].SetValue(Matrix.CreateScale(10F));

            this.GraphicsDevice.SetRenderTarget(null);
            this.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Pink, 1.0f, 0);
            this.postprocescam.SetEffectParameters(posteffect);
            posteffect.Parameters["Brick"].SetValue(this.texturetarget);

            //Set the technique so that it will draw the texture on the quad
            posteffect.CurrentTechnique = posteffect.Techniques["TextureQuad"];
            foreach (var pass in posteffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, this.quadVertices, 0, quadVertices.Length,
                    quadIndices, 0, quadIndices.Length / 3);
            }
        }
            
    }
}
