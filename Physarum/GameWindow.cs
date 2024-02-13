using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Physarum.Components;
using System.Diagnostics;

namespace Physarum
{
    public class GameWindow : Game
    {
        #region Propeties
        private GraphicsDevice _device
        {
            get { return GraphicsDevice; }
            set { }
        }
        #endregion
        #region Variables
        PhysarumComponent component;

        int WindowHeight = 600;
        int WindowWidth = 800;
        #endregion
        #region Constructor
        public GameWindow() 
        {
            var GraphicsDeviceManager = new GraphicsDeviceManager(this);

            IsMouseVisible = true;

            IsFixedTimeStep = false;
            if (Debugger.IsAttached)
            {
                //IsFixedTimeStep = true;
                //TargetElapsedTime = TimeSpan.FromSeconds(1 / 2f);
            }

            Content.RootDirectory = @"Content\"
;
            GraphicsDeviceManager.PreferredBackBufferWidth = WindowWidth;
            GraphicsDeviceManager.PreferredBackBufferHeight = WindowHeight;
        }
        #endregion
        #region ProgramBody
        protected override void Initialize()
        {

            component = new(this);
            
            Components.Add(component);
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            
            base.UnloadContent();
        }


        protected override void Update(GameTime gameTime)
        {
            KeyboardState keysState = Keyboard.GetState();
            foreach (var item in keysState.GetPressedKeys())
            {
                switch (item)
                {
                    case Keys.Escape:
                        Exit();
                        continue;
                    case Keys.NumPad1:
                        component.RandomizeShaderColor();
                        continue;
                    case Keys.NumPad2:
                        component.RandomizeGameRules();
                        continue;
                    case Keys.NumPad3:
                        component.RandomizeAgentRules();
                        continue;
                    case Keys.NumPad4:
                        component.RandomizeSensors();
                        continue;
                    default:
                        break;
                }
            }
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
        #endregion
    }
}
