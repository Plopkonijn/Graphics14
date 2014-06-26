using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GraphicsPractical2
{
    /// <summary>
    /// This class keeps track of the framerate of the game.
    /// </summary>
    class FrameRateCounter : DrawableGameComponent
    {
        private int frameRate, frameCounter, secondsPassed;

        public FrameRateCounter(Game game)
            : base(game)
        {
            this.frameRate = 0;
            this.frameCounter = 0;
            this.secondsPassed = 0;
        }

        public override void Update(GameTime gameTime)
        {
            // If a second has passed, count the frames we have seen during that second and reset the counter.
            if (this.secondsPassed != gameTime.TotalGameTime.Seconds)
            {
                this.frameRate = this.frameCounter;
                this.secondsPassed = gameTime.TotalGameTime.Seconds;
                this.frameCounter = 0;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            // Update the counter by one every draw call.
            this.frameCounter++;
        }

        /// <summary>
        /// Returns the current framerate of the game.
        /// </summary>
        public int FrameRate
        {
            get { return this.frameRate; }
        }
    }
}
