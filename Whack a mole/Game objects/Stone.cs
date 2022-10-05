using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Whack_a_mole.Game_objects
{
    public class Stone
    {
        // Vector2
        public Vector2 Pos;
        public Vector2 StopPos;
        Vector2 Vel;

        // Float
        float Scale;
        float RefreshTime;

        // Other
        Texture2D Tex;
        Rectangle[] SpriteFrames;
        int CurrentFrame;
        Timer FrameTimer;

        public Stone (Texture2D tex, Vector2 pos, Vector2 vel, Vector2 stopPos, float scale, float refreshTime)
        {
            Tex = tex;
            Pos = pos;
            Vel = vel;
            StopPos = stopPos;
            Scale = scale;
            RefreshTime = refreshTime;
            CurrentFrame = 0;

            int spriteFrameRow = (int) (Tex.Height * .75f);
            int spriteFrameHeight = (int) (Tex.Height * .25f);
            int spriteFrameWidth = (int) (Tex.Width * .236f);

            SpriteFrames = new Rectangle[4];

            SpriteFrames[0] = new Rectangle(0, spriteFrameRow, spriteFrameWidth, spriteFrameHeight);
            SpriteFrames[1] = new Rectangle((int) (Tex.Width * .25f), spriteFrameRow, spriteFrameWidth, spriteFrameHeight);
            SpriteFrames[2] = new Rectangle((int) (Tex.Width * .50f), spriteFrameRow, spriteFrameWidth, spriteFrameHeight);
            SpriteFrames[3] = new Rectangle((int) (Tex.Width * .75f), spriteFrameRow, spriteFrameWidth, spriteFrameHeight);

            FrameTimer = new();
            FrameTimer.StartTimer(RefreshTime);
        }

        public void Update(float deltaTime)
        {
            FrameTimer.Update(deltaTime);

            if (FrameTimer.IsDone())
            {
                if (CurrentFrame == SpriteFrames.Length-1)
                    CurrentFrame = 0;
                else
                    CurrentFrame++;

                FrameTimer.StartTimer(RefreshTime);
                if (Pos.Y < StopPos.Y)
                    Pos += Vel;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Tex, Pos, SpriteFrames[CurrentFrame], Color.White, 0f, new Vector2(), Scale, SpriteEffects.None, 0f);
        }
    }
}
