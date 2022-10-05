using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whack_a_mole.Game_objects
{
    public class Hole
    {
        // Texture2D
        Texture2D HoleTex;
        Texture2D ForeGroundTex;

        // Other
        Vector2 Pos;
        float Scale;

        public Hole(Texture2D hole, Texture2D foreGround, Vector2 pos, float scale)
        {
            HoleTex = hole;
            ForeGroundTex = foreGround;
            Pos = pos;
            Scale = scale;
        }

        public void Draw(SpriteBatch spriteBatch, float drawLayer, float multiple)
        {
            spriteBatch.Draw(HoleTex, Pos, null, Color.White, 0f, new Vector2(), Scale, SpriteEffects.None, drawLayer - 2f / multiple);
            spriteBatch.Draw(ForeGroundTex, Pos, null, Color.White, 0f, new Vector2(), Scale, SpriteEffects.None, drawLayer);
        }

    }
}
