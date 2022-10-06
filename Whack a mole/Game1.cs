using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Whack_a_mole.Game_objects;
using System.Linq;
using System.Media;
using System.Threading;
using System.Windows.Forms;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using System.Drawing;
using Color = Microsoft.Xna.Framework.Color;
using System;
using Whack_a_mole.Enums;
using Point = Microsoft.Xna.Framework.Point;
using SharpDX.Direct2D1;
using SpriteBatch = Microsoft.Xna.Framework.Graphics.SpriteBatch;
using SharpDX;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Whack_a_mole
{
    public class Game1 : Game
    {
        // Texture2D
        public Texture2D HoleTex;
        public Texture2D MoleTexNormal;
        public Texture2D MoleTexHurt;
        public Texture2D ForegroundTex;
        public Texture2D BackgroundTex;
        public Texture2D StoneSpriteSheetTex;
        public Texture2D MalletTex;

        // Int
        int Height;
        int Width;
        int TimeLeft;
        int HoleAmountX = 1;
        int HoleAmountY = 1;
        int PlayerScore = 0;
        int PlayerLives = 5;
        int StartLives = 5;
        int StartTime = 45;
        int MoleHitScore = 10;
        int textDrawLayer = 1;

        // Float
        public float DeltaTime;
        public float BackgroundScale = 1.55f;
        public float HoleMoleScale;
        float MoleMinDelay = 0.5f;
        float MoleMaxDelay = 1.5f;

        // Array
        Hole[,] Holes;
        Mole[,] Moles;

        // Timer
        Timer TimeLeftInterval;
        Timer MoleDelay;

        // Other
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        GameState CurrentState;
        SpriteFont GameFont;
        bool IsMousePressed;
        Stone MenuStone;
        Mallet CursorMallet;
        Color Textcolor = Color.White;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            HoleTex = Content.Load<Texture2D>("hole");
            MoleTexNormal = Content.Load<Texture2D>("mole");
            MoleTexHurt = Content.Load<Texture2D>("mole_hit");
            ForegroundTex = Content.Load<Texture2D>("hole_foreground");
            BackgroundTex = Content.Load<Texture2D>("background");
            MalletTex = Content.Load<Texture2D>("mallet");
            StoneSpriteSheetTex = Content.Load<Texture2D>("spritesheet_stone");
            GameFont = Content.Load<SpriteFont>("gamefont");

            Width = (int)(BackgroundTex.Width * BackgroundScale);
            Height = (int)(BackgroundTex.Height * 4 * BackgroundScale);

            graphics.PreferredBackBufferWidth = Width;
            graphics.PreferredBackBufferHeight = Height;
            graphics.ApplyChanges();

            HoleMoleScale = (float)Width / (float)HoleAmountX / (float)HoleTex.Width;

            if (HoleMoleScale > 1)
                HoleMoleScale = 1;

            CurrentState = GameState.Menu;
            IsMousePressed = false;

            MenuStone = new(StoneSpriteSheetTex, new Vector2(Width / 10, 0), new Vector2(0, 5), new Vector2(0, Height), 1.5f, .05f);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Mouse.GetState().LeftButton == ButtonState.Released)
                IsMousePressed = false;

            switch (CurrentState)
            {
                case GameState.Menu:
                    MenuStone.Update(DeltaTime);
                    if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                        CurrentState = GameState.DifficultySelect;
                    break;
                case GameState.DifficultySelect:
                    KeyboardState keyboard = Keyboard.GetState();
                    Keys[] validInputs = { Keys.D1, Keys.D2, Keys.D3 };
                    Keys[] input = keyboard.GetPressedKeys();

                    // Changes the amount of holes and delays, i.e changing the difficult, based upon what key is pressed
                    if (input.Any(k => validInputs.Contains(k)) || keyboard.GetPressedKeyCount() == 1)
                        switch (input[0])
                        {
                            // Easy
                            case Keys.D1:
                                HoleAmountX = 3;
                                HoleAmountY = 3;
                                MoleMinDelay = 0.75f;
                                MoleMaxDelay = 1.25f;
                                StartGame();
                                break;
                            // Medium
                            case Keys.D2:
                                HoleAmountX = 5;
                                HoleAmountY = 5;
                                MoleMinDelay = 0.5f;
                                MoleMaxDelay = 1.25f;
                                StartGame();
                                break;
                            // Hard
                            case Keys.D3:
                                HoleAmountX = 7;
                                HoleAmountY = 7;
                                MoleMinDelay = 0.5f;
                                MoleMaxDelay = 1.0f;
                                StartGame();
                                break;
                        }
                    break;
                case GameState.GameStart:
                    UpdateGameObjects(DeltaTime, new Vector2(Mouse.GetState().X, Mouse.GetState().Y), IsMousePressed);
                    TimeLeftInterval.Update(DeltaTime);
                    MoleDelay.Update(DeltaTime);
                    if (TimeLeftInterval.IsDone())
                    {
                        if (TimeLeft <= 0)
                        {
                            EndGame();
                        }
                        else
                        {
                            TimeLeft--;
                            TimeLeftInterval.StartTimer(1);
                        }
                    }

                    // Activates a random mole which isn't idle
                    if (MoleDelay.IsDone())
                    {
                        Random randomizer = new();
                        double delay = randomizer.NextDouble(MoleMinDelay, MoleMaxDelay);

                        Mole randomMole = Moles[randomizer.Next(0, HoleAmountY - 1), randomizer.Next(0, HoleAmountX - 1)];

                        while (randomMole.CurrentState != MoleState.Idle)
                            randomMole = Moles[randomizer.Next(0, HoleAmountY - 1), randomizer.Next(0, HoleAmountX - 1)];

                        randomMole.Activate();

                        MoleDelay.StartTimer(delay);
                    }

                    if (Mouse.GetState().LeftButton == ButtonState.Pressed && IsMousePressed == false)
                    {
                        IsMousePressed = true;
                        Vector2 mousePos = new(Mouse.GetState().X, Mouse.GetState().Y);

                        for (int x = 0; x < HoleAmountY; x++)
                            for (int y = 0; y < HoleAmountX; y++)
                            {
                                if (Moles[x, y].HitBox.Contains(mousePos))
                                    if (mousePos.Y < Moles[x, y].GroundPos.Y)
                                    {
                                        if (Moles[x, y].HealthPoints == -1)
                                        {
                                            Moles[x, y].Reset();
                                            LoseLife();
                                            return;
                                        }

                                        else
                                        {
                                            if (Moles[x, y].CurrentState != MoleState.Hit)
                                                PlayerScore += MoleHitScore;

                                            Moles[x, y].Hit();
                                            return;
                                        }
                                    }
                            }
                    }

                    break;
                case GameState.GameOver:
                    CursorMallet.Update(new Vector2(-1000, -1000), IsMousePressed);
                    if (Keyboard.GetState().IsKeyDown(Keys.Y))
                        CurrentState = GameState.DifficultySelect;
                    else if (Keyboard.GetState().IsKeyDown(Keys.N))
                        Exit();

                    break;
            }

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(111, 209, 73));

            spriteBatch.Begin(SpriteSortMode.FrontToBack);

            switch (CurrentState)
            {
                case GameState.Menu:
                    CreateMenuAnimation(spriteBatch);
                    DrawMenu();
                    break;
                case GameState.DifficultySelect:
                    DrawDifficultySelect();
                    break;
                case GameState.GameStart:
                    DrawSprites(spriteBatch);
                    DrawHUD();
                    break;
                case GameState.GameOver:
                    DrawGameOver();
                    break;
            }
            DrawSprites(spriteBatch);

            spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        /// <summary>
        /// Creates holes and moles and adds them to their respective arrays
        /// </summary>
        public void CreateBoard()
        {
            Holes = new Hole[HoleAmountY, HoleAmountX];
            Moles = new Mole[HoleAmountY, HoleAmountX];

            for (int y = 0; y < HoleAmountY; y++)
            {
                float yPos = (Height - BackgroundTex.Height * BackgroundScale) / HoleAmountY * y + BackgroundTex.Height * BackgroundScale;
                yPos += (Height - BackgroundTex.Height * BackgroundScale) / HoleAmountY / 2;

                for (int x = 0; x < HoleAmountX; x++)
                {
                    Vector2 pos = new(Width / HoleAmountX * x, yPos);
                    Hole newHole = new(HoleTex, ForegroundTex, pos, HoleMoleScale);
                    Mole newMole = new(MoleTexNormal, MoleTexHurt, pos, HoleMoleScale);

                    Holes[y, x] = newHole;
                    Moles[y, x] = newMole;
                }
            }

            CursorMallet = new(MalletTex, new Vector2(Mouse.GetState().X, Mouse.GetState().Y), 1f, 1f);
        }

        /// <summary>
        /// Does an animation of a boulder rolling on the menu screen
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void CreateMenuAnimation(SpriteBatch spriteBatch)
        {
            MenuStone.Draw(spriteBatch);
        }

        /// <summary>
        /// Draws all holes and moles
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void DrawSprites(SpriteBatch spriteBatch)
        {
            if (CurrentState != GameState.Menu && CurrentState != GameState.DifficultySelect)
            {
                CursorMallet.Draw(spriteBatch);
                spriteBatch.Draw(BackgroundTex, Vector2.Zero, null, Color.White, 0f, new Vector2(), BackgroundScale, 0f, 0);

                int multiple = 1000000;
                float drawLayer = 3f / multiple;


                for (int x = 0; x < HoleAmountY; x++)
                    for (int y = 0; y < HoleAmountX; y++)
                    {
                        if (CurrentState == GameState.GameStart)
                        {
                            Holes[x, y].Draw(spriteBatch, drawLayer, multiple);
                            Moles[x, y].Draw(spriteBatch, drawLayer, multiple);
                        }
                        drawLayer += 3f / multiple;
                    }
            }

        }

        /// <summary>
        /// Draws for main menu
        /// </summary>
        public void DrawMenu()
        {
            string title = "Whack a Mole";
            string prompt = "Press Enter to Start";

            Vector2 titleSize = Vector2.Multiply(GameFont.MeasureString(title), 1.75f);
            Vector2 promptSize = GameFont.MeasureString(prompt);

            Vector2 titlePos = new(Width / 2 - titleSize.X / 2, Height / 3 - titleSize.Y / 2);
            Vector2 promptPos = new(Width / 2 - promptSize.X / 2, Height / 2 - promptSize.Y / 2);

            spriteBatch.DrawString(GameFont, title, titlePos, Textcolor, 0f, new Vector2(), 1.75f, new SpriteEffects(), textDrawLayer);
            spriteBatch.DrawString(GameFont, prompt, promptPos, Textcolor, 0f, new Vector2(), 1f, new SpriteEffects(), textDrawLayer);
        }

        /// <summary>
        /// Draws the text for difficulty select screen
        /// </summary>
        public void DrawDifficultySelect()
        {
            string easyPromptText = "Press 1 for Easy";
            string mediumPromptText = "Press 2 for Medium";
            string hardPromptText = "Press 3 for Hard";

            Vector2 easyPromptSize = GameFont.MeasureString(easyPromptText);
            Vector2 mediumPromptSize = GameFont.MeasureString(mediumPromptText);
            Vector2 hardPromptSize = GameFont.MeasureString(hardPromptText);

            Vector2 easyPromptPos = new Vector2(Width / 2 - easyPromptSize.X / 2, Height / 2 - easyPromptSize.Y / 2);
            Vector2 mediumPromptPos = new Vector2(Width / 2 - mediumPromptSize.X / 2, easyPromptPos.Y + mediumPromptSize.Y);
            Vector2 hardPromptPos = new Vector2(Width / 2 - hardPromptSize.X / 2, mediumPromptPos.Y + hardPromptSize.Y);

            spriteBatch.DrawString(GameFont, easyPromptText, easyPromptPos, Textcolor, 0f, new Vector2(), 1f, new SpriteEffects(), textDrawLayer);
            spriteBatch.DrawString(GameFont, mediumPromptText, mediumPromptPos, Textcolor, 0f, new Vector2(), 1f, new SpriteEffects(), textDrawLayer);
            spriteBatch.DrawString(GameFont, hardPromptText, hardPromptPos, Textcolor, 0f, new Vector2(), 1f, new SpriteEffects(), textDrawLayer);
        }

        /// <summary>
        /// Draws the HUD while playing the game
        /// </summary>
        public void DrawHUD()
        {
            string timeLeftText = $"{TimeLeft}";
            string scoreText = $"Score: {PlayerScore}";
            string livesLeftText = $"Lives: {PlayerLives}";

            Vector2 timeLeftSize = GameFont.MeasureString(timeLeftText);
            Vector2 livesLeftSize = GameFont.MeasureString(livesLeftText);

            Vector2 timeLeftPos = new (Width / 2 - timeLeftSize.X / 2, 0);
            Vector2 scorePos = Vector2.Zero;
            Vector2 livesLeftPos = new (Width - livesLeftSize.X, 0);

            spriteBatch.DrawString(GameFont, timeLeftText, timeLeftPos, Textcolor, 0f, new Vector2(), 1f, new SpriteEffects(), textDrawLayer);
            spriteBatch.DrawString(GameFont, scoreText, scorePos, Textcolor, 0f, new Vector2(), 1f, new SpriteEffects(), textDrawLayer);
            spriteBatch.DrawString(GameFont, livesLeftText, livesLeftPos, Textcolor, 0f, new Vector2(), 1f, new SpriteEffects(), textDrawLayer);

        }

        /// <summary>
        /// Draws the game over screen
        /// </summary>
        public void DrawGameOver()
        {
            string gameOverText = "Game Over";
            string scoreText = $"Score: {PlayerScore}";
            string promptText = "Want To Try Again? (Y/N)";

            Vector2 gameOverSize = Vector2.Multiply(GameFont.MeasureString(gameOverText), 1.5f);
            Vector2 scoreSize = GameFont.MeasureString(scoreText);
            Vector2 promptSize = GameFont.MeasureString(promptText);

            Vector2 gameOverPos = new (Width / 2 - gameOverSize.X / 2, Height / 3 - gameOverSize.Y / 2);
            Vector2 scorePos = new(Width / 2 - scoreSize.X / 2, Height / 3 - scoreSize.Y / 2 + gameOverSize.Y);
            Vector2 promptPos = new(Width / 2 - promptSize.X / 2, Height / 2 - promptSize.Y / 2 + promptSize.Y);

            spriteBatch.DrawString(GameFont, gameOverText, gameOverPos, Textcolor, 0f, new Vector2(), 1.5f, new SpriteEffects(), textDrawLayer);
            spriteBatch.DrawString(GameFont, scoreText, scorePos, Textcolor, 0f, new Vector2(), 1f, new SpriteEffects(), textDrawLayer);
            spriteBatch.DrawString(GameFont, promptText, promptPos, Textcolor, 0f, new Vector2(), 1f, new SpriteEffects(), textDrawLayer);
        }

        /// <summary>
        /// Updates all the moles
        /// </summary>
        /// <param name="deltaTime"></param>
        public void UpdateGameObjects(double deltaTime, Vector2 mousePos, bool isMousePressed)
        {
            CursorMallet.Update(mousePos, isMousePressed);

            for (int x = 0; x < HoleAmountY; x++)
                for (int y = 0; y < HoleAmountX; y++)
                {
                    Moles[x, y].Update(deltaTime);
                }
        }

        /// <summary>
        /// Starts a game
        /// </summary>
        public void StartGame()
        {
            PlayerLives = StartLives;
            CurrentState = GameState.GameStart;
            CreateBoard();
            TimeLeft = StartTime;

            TimeLeftInterval = new();
            TimeLeftInterval.StartTimer(1);

            MoleDelay = new();
            MoleDelay.StartTimer(1);
        }

        /// <summary>
        /// Ends the current game
        /// </summary>
        public void EndGame()
        {
            CurrentState = GameState.GameOver;
            Moles = new Mole[0, 0];
            Holes = new Hole[0, 0];
        }

        /// <summary>
        /// Takes away a playerLife, otherwise ends the current game
        /// </summary>
        public void LoseLife()
        {
            PlayerLives--;
            if (PlayerLives == 0)
                EndGame();
        }
    }
}