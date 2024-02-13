using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Physarum.AgentLogic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;

namespace Physarum.Components
{
    internal class Label : DrawableGameComponent
    {
        Game game;
        SpriteBatch spriteBatch;
        


        bool isVisible = true;

        private string text;   
        protected SpriteFont font;

        private Color color;
        private Rectangle labelSpacing;
        private Texture2D labelTexture;
        
        private Vector2 location;
        private Vector2 textLocation;
        public Vector2 Location
        {
            get { return location; }
            set { location = value; }
        }
        private GraphicsDevice device
        {
            get { return GraphicsDevice; }
        }
        public bool IsVisible
        {
            get { return isVisible; }
            set { isVisible = value; }
        }
        public Label(Game game, string Text, Vector2 Location, Rectangle Size, SpriteFont Font, Color Color) : base(game)
        {
            this.game = game;
            
            text = Text;

            if (null == text)
                throw new ArgumentNullException("text", "text can't be null");

            location = Location;
            font = Font;
            color = Color;
            labelSpacing = Size;
        }
        public override void Initialize()
        {
            Vector2 TextSize = font.MeasureString(text);
            labelTexture = new Texture2D(device, labelSpacing.Width + (int)TextSize.X, labelSpacing.Height + (int)TextSize.Y);
            int offset = labelSpacing.Width;
            for (int i = 0; i < labelTexture.Width; i++)
            {
                for (int j = 0; j < labelTexture.Height; j++)
                {
                    labelTexture.SetData(new Color[] { color }, offset * j + i, 1);
                }
            }
            textLocation = new Vector2(location.X + labelTexture.Width / 2f - TextSize.X / 2f, location.Y + labelTexture.Height / 2f - TextSize.Y / 2f);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(device);
            base.LoadContent();
        }

        protected override void UnloadContent()
        {

            base.UnloadContent();
        }
        public override void Update(GameTime gameTime)
        {

            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            if (!isVisible)
                return;
            Color color = this.color;

            spriteBatch.Begin();
            spriteBatch.Draw(labelTexture, location, Color.White);
            spriteBatch.DrawString(font, text, textLocation, color);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
