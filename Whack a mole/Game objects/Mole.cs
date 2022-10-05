using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whack_a_mole.Enums;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Whack_a_mole.Game_objects
{
    public class Mole
    {
        // Texture2D
        Texture2D NormTex;
        Texture2D HurtTex;
        Texture2D CurrentSprite;

        // Vector2
        public Vector2 NormalPos;
        public Vector2 GroundPos;
        Vector2 CurrentPos;
        Vector2 MaxPos;
        Vector2 Vel;
        Vector2 RetreatVel;
        Vector2 HitVel;

        // Float
        float Scale;
        float MinDelay = 0.6f;
        float MaxDelay = 1.0f;
        float HitDelay = .5f;

        // Int
        int MinSpeed = 2;
        int MaxSpeed = 5;
        public int HealthPoints;

        // Other
        public MoleState CurrentState;
        public Rectangle HitBox;
        Timer RetreatTimer;
        Timer HitDelayTimer;

        public Mole(Texture2D normTex, Texture2D hurtTex, Vector2 pos, float scale)
        {
            Scale = scale;
            CurrentState = MoleState.Idle;
            HealthPoints = 1;

            NormTex = normTex;
            HurtTex = hurtTex;
            CurrentSprite = NormTex;

            NormalPos = pos;
            CurrentPos = NormalPos;
            RetreatVel = new(0, 5);
            HitVel = new(0, 10);
            MaxPos = Vector2.Subtract(NormalPos, new Vector2(0, NormTex.Height * Scale *.7f));
            GroundPos = new(NormalPos.X, NormalPos.Y + NormTex.Height * Scale * .2f);
            
            int hitboxHeight = (int)(NormTex.Height * Scale * .8f);
            HitBox = new((int)CurrentPos.X, (int) CurrentPos.Y, (int) (NormTex.Width * Scale), hitboxHeight);
            
            RetreatTimer = new();
            HitDelayTimer = new();
        }

        public void Update(double deltatime)
        {
            RetreatTimer.Update(deltatime);
            HitDelayTimer.Update(deltatime);

            HitBox.X = (int)CurrentPos.X;
            HitBox.Y = (int)CurrentPos.Y + CurrentSprite.Height - HitBox.Height;

            switch (CurrentState)
            {
                case MoleState.Idle:
                    CurrentSprite = NormTex;
                    break;
                case MoleState.Peaking:
                    CurrentPos -= Vel;
                    if (CurrentPos.Y <= MaxPos.Y)
                    {
                        CurrentPos = MaxPos;
                        CurrentState = MoleState.OutOfHole;
                        RetreatTimer.StartTimer(new Random().NextDouble(MinDelay,MaxDelay));
                    }
                    break;
                case MoleState.OutOfHole:
                    if (RetreatTimer.IsDone())
                        CurrentState = MoleState.Retreating;
                    break;
                case MoleState.Hit:
                    if (HitDelayTimer.IsDone())
                    {
                        CurrentPos += HitVel;
                        if (CurrentPos.Y >= NormalPos.Y)
                            Reset();
                    }
                    break;
                case MoleState.Retreating:
                    CurrentPos += RetreatVel;
                    if (CurrentPos.Y >= NormalPos.Y)
                    {
                        Reset();
                    } 
                    break;
            }
        }

        public void Draw(SpriteBatch spriteBatch, float drawLayer, int multiple)
        {
            Color drawColor;
            switch (HealthPoints)
            {
                case -1:
                    drawColor = Color.Black;
                    break;
                case 2:
                    drawColor = Color.Red;
                    break;
                case 3:
                    drawColor = Color.DarkRed;
                    break;
                default:
                    drawColor = Color.White;
                    break;
            }
            spriteBatch.Draw(CurrentSprite, CurrentPos, null, drawColor, 0f, new Vector2(), Scale, SpriteEffects.None, drawLayer - 1f / multiple);
        }

        /// <summary>
        /// Causes the mole to peek out the hole
        /// </summary>
        public void Activate()
        {
            if (CurrentState == MoleState.Idle || CurrentState != MoleState.Hit)
            {
                CurrentState = MoleState.Peaking;

                Vel = new(0, new Random().Next(MinSpeed, MaxSpeed));

                int randomNum = new Random().Next(0, 12);

                if (randomNum >= 11)
                    HealthPoints = -1;
                else if (randomNum >= 10)
                    HealthPoints = 3;
                else if (randomNum >= 8)
                    HealthPoints = 2;
                else
                    HealthPoints = 1;
            } 
            
        }

        public void Hit()
        {
            if (HealthPoints > 0)
                HealthPoints--;

            if (HealthPoints == 0 && CurrentState != MoleState.Hit)
            {
                CurrentState = MoleState.Hit;
                CurrentSprite = HurtTex;
                HitDelayTimer.StartTimer(HitDelay);
            }
            else if (CurrentState == MoleState.Hit)
                HitDelayTimer.StartTimer(0);
        }

        /// <summary>
        /// Resets Mole to idle
        /// </summary>
        public void Reset()
        {
            CurrentPos = NormalPos;
            Vel = Vector2.Zero;
            CurrentState = MoleState.Idle;
            HealthPoints = 0;
        }
    }
}
