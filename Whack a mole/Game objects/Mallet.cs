using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Whack_a_mole.Game_objects
{
    public class Mallet
    {
        // Float
        float MaxRotation;
        float CurrentRotation;
        float Scale;

        // Other
        Texture2D Tex;
        Vector2 Pos;
        
        public Mallet(Texture2D tex, Vector2 pos, float rotation, float scale)
        {
            Tex = tex;
            Pos = pos;
            Scale = scale;
            MaxRotation = rotation;
        }

        public void Update(Vector2 pos, bool isMouseDown)
        {
            Pos = pos;

            if (isMouseDown)
                CurrentRotation = 0;
            else
                CurrentRotation = MaxRotation;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Tex, Pos, null, Color.White, CurrentRotation, new Vector2(Tex.Width/5,Tex.Height), Scale, SpriteEffects.None, 1);
        }

    }
}
