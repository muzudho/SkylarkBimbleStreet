namespace SkylarkBimbleStreet;

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class Game1 : Game
{
    private const int VirtualWidth = 1920;
    private const int VirtualHeight = 1080;
    private const float PlayerSpeed = 520f;
    private const int PlayerSize = 46;

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private Texture2D _pixel = null!;
    private RenderTarget2D _scene = null!;

    private readonly Rectangle[] _walls =
    [
        new(0, 0, VirtualWidth, 38),
        new(0, VirtualHeight - 38, VirtualWidth, 38),
        new(0, 0, 38, VirtualHeight),
        new(VirtualWidth - 38, 0, 38, VirtualHeight),
        new(260, 190, 38, 700),
        new(520, 150, 38, 620),
        new(780, 310, 38, 700),
        new(1040, 140, 38, 660),
        new(1300, 280, 38, 650),
        new(1560, 150, 38, 560),
        new(300, 850, 520, 38),
        new(1080, 820, 520, 38),
    ];

    private readonly Rectangle[] _gemBounds =
    [
        new(390, 250, 34, 34),
        new(660, 820, 34, 34),
        new(920, 230, 34, 34),
        new(1190, 760, 34, 34),
        new(1450, 230, 34, 34),
    ];

    private readonly Hazard[] _hazards =
    [
        new(new Rectangle(340, 500, 64, 64), new Vector2(0f, 250f), 410, 760),
        new(new Rectangle(870, 460, 64, 64), new Vector2(0f, -310f), 330, 720),
        new(new Rectangle(1160, 410, 64, 64), new Vector2(0f, 280f), 330, 720),
        new(new Rectangle(1400, 720, 64, 64), new Vector2(300f, 0f), 1360, 1510),
    ];

    private readonly bool[] _gemsCollected = new bool[5];

    private Vector2 _playerPosition;
    private KeyboardState _previousKeyboard;
    private GamePadState _previousGamePad;
    private int _deaths;
    private bool _cleared;
    private double _titleRefreshTimer;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.SynchronizeWithVerticalRetrace = true;

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        Window.Title = "SkylarkBimbleStreet - Action Puzzle Prototype";
    }

    protected override void Initialize()
    {
        ResetRun();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
        _scene = new RenderTarget2D(GraphicsDevice, VirtualWidth, VirtualHeight);
    }

    protected override void UnloadContent()
    {
        _scene.Dispose();
        _pixel.Dispose();
        base.UnloadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var gamePad = GamePad.GetState(PlayerIndex.One);

        if (gamePad.Buttons.Back == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        if (WasPressed(keyboard, Keys.R) || WasPressed(gamePad.Buttons.Start, _previousGamePad.Buttons.Start))
        {
            ResetRun();
        }

        var elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (!_cleared)
        {
            MovePlayer(GetMoveInput(keyboard, gamePad), elapsed);
            UpdateHazards(elapsed);
            CheckGemCollection();
            CheckHazardCollision();
            CheckExit();
        }

        UpdateWindowTitle(gameTime);

        _previousKeyboard = keyboard;
        _previousGamePad = gamePad;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_scene);
        GraphicsDevice.Clear(new Color(18, 22, 31));

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        DrawScene();
        _spriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_scene, GetDestinationRectangle(), Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawScene()
    {
        DrawRectangle(new Rectangle(0, 0, VirtualWidth, VirtualHeight), new Color(18, 22, 31));
        DrawGrid();

        foreach (var wall in _walls)
        {
            DrawRectangle(wall, new Color(76, 84, 103));
            DrawRectangle(Inset(wall, 5), new Color(104, 116, 140));
        }

        var exitColor = AreAllGemsCollected() ? new Color(74, 205, 116) : new Color(62, 78, 72);
        DrawRectangle(GetExitBounds(), exitColor);
        DrawRectangle(Inset(GetExitBounds(), 12), new Color(18, 22, 31));

        for (var i = 0; i < _gemBounds.Length; i++)
        {
            if (_gemsCollected[i])
            {
                continue;
            }

            DrawRectangle(_gemBounds[i], new Color(245, 198, 80));
            DrawRectangle(Inset(_gemBounds[i], 8), new Color(255, 239, 151));
        }

        foreach (var hazard in _hazards)
        {
            DrawRectangle(hazard.Bounds, new Color(221, 72, 92));
            DrawRectangle(Inset(hazard.Bounds, 12), new Color(255, 148, 157));
        }

        DrawRectangle(GetPlayerBounds(), _cleared ? new Color(90, 220, 160) : new Color(81, 161, 255));
        DrawRectangle(Inset(GetPlayerBounds(), 10), new Color(197, 228, 255));
        DrawHud();
    }

    private void DrawGrid()
    {
        var color = new Color(31, 37, 50);
        for (var x = 0; x < VirtualWidth; x += 120)
        {
            DrawRectangle(new Rectangle(x, 0, 2, VirtualHeight), color);
        }

        for (var y = 0; y < VirtualHeight; y += 120)
        {
            DrawRectangle(new Rectangle(0, y, VirtualWidth, 2), color);
        }
    }

    private void DrawHud()
    {
        DrawRectangle(new Rectangle(58, 58, 420, 22), new Color(44, 51, 67));
        for (var i = 0; i < _gemBounds.Length; i++)
        {
            var color = _gemsCollected[i] ? new Color(245, 198, 80) : new Color(69, 75, 90);
            DrawRectangle(new Rectangle(70 + i * 76, 52, 42, 42), color);
        }

        for (var i = 0; i < Math.Min(_deaths, 8); i++)
        {
            DrawRectangle(new Rectangle(70 + i * 38, 112, 24, 24), new Color(221, 72, 92));
        }

        if (_cleared)
        {
            DrawRectangle(new Rectangle(610, 430, 700, 220), new Color(36, 48, 46));
            DrawRectangle(new Rectangle(650, 470, 620, 140), new Color(74, 205, 116));
        }
    }

    private void MovePlayer(Vector2 move, float elapsed)
    {
        if (move == Vector2.Zero)
        {
            return;
        }

        move.Normalize();
        var velocity = move * PlayerSpeed * elapsed;
        TryMove(new Vector2(velocity.X, 0f));
        TryMove(new Vector2(0f, velocity.Y));
    }

    private void TryMove(Vector2 delta)
    {
        _playerPosition += delta;
        var player = GetPlayerBounds();

        foreach (var wall in _walls)
        {
            if (!player.Intersects(wall))
            {
                continue;
            }

            _playerPosition -= delta;
            return;
        }
    }

    private void UpdateHazards(float elapsed)
    {
        for (var i = 0; i < _hazards.Length; i++)
        {
            var hazard = _hazards[i];
            hazard.Bounds.X += (int)(hazard.Velocity.X * elapsed);
            hazard.Bounds.Y += (int)(hazard.Velocity.Y * elapsed);

            if (hazard.Velocity.Y != 0f && (hazard.Bounds.Y < hazard.Min || hazard.Bounds.Y > hazard.Max))
            {
                hazard.Velocity.Y *= -1f;
                hazard.Bounds.Y = Math.Clamp(hazard.Bounds.Y, hazard.Min, hazard.Max);
            }

            if (hazard.Velocity.X != 0f && (hazard.Bounds.X < hazard.Min || hazard.Bounds.X > hazard.Max))
            {
                hazard.Velocity.X *= -1f;
                hazard.Bounds.X = Math.Clamp(hazard.Bounds.X, hazard.Min, hazard.Max);
            }

            _hazards[i] = hazard;
        }
    }

    private void CheckGemCollection()
    {
        var player = GetPlayerBounds();
        for (var i = 0; i < _gemBounds.Length; i++)
        {
            if (!_gemsCollected[i] && player.Intersects(_gemBounds[i]))
            {
                _gemsCollected[i] = true;
            }
        }
    }

    private void CheckHazardCollision()
    {
        var player = GetPlayerBounds();
        foreach (var hazard in _hazards)
        {
            if (player.Intersects(hazard.Bounds))
            {
                _deaths++;
                ResetPlayerOnly();
                return;
            }
        }
    }

    private void CheckExit()
    {
        if (AreAllGemsCollected() && GetPlayerBounds().Intersects(GetExitBounds()))
        {
            _cleared = true;
        }
    }

    private void ResetRun()
    {
        Array.Fill(_gemsCollected, false);
        _deaths = 0;
        _cleared = false;
        ResetPlayerOnly();
        ResetHazards();
        RefreshWindowTitle();
    }

    private void ResetPlayerOnly()
    {
        _playerPosition = new Vector2(95, 95);
    }

    private void ResetHazards()
    {
        _hazards[0] = new Hazard(new Rectangle(340, 500, 64, 64), new Vector2(0f, 250f), 410, 760);
        _hazards[1] = new Hazard(new Rectangle(870, 460, 64, 64), new Vector2(0f, -310f), 330, 720);
        _hazards[2] = new Hazard(new Rectangle(1160, 410, 64, 64), new Vector2(0f, 280f), 330, 720);
        _hazards[3] = new Hazard(new Rectangle(1400, 720, 64, 64), new Vector2(300f, 0f), 1360, 1510);
    }

    private void UpdateWindowTitle(GameTime gameTime)
    {
        _titleRefreshTimer -= gameTime.ElapsedGameTime.TotalSeconds;
        if (_titleRefreshTimer > 0)
        {
            return;
        }

        RefreshWindowTitle();
    }

    private void RefreshWindowTitle()
    {
        _titleRefreshTimer = 0.2;
        var collected = 0;
        foreach (var gemCollected in _gemsCollected)
        {
            if (gemCollected)
            {
                collected++;
            }
        }

        var state = _cleared ? "CLEAR - Press R / Start to retry" : "Collect all gems and reach the green exit";
        Window.Title = $"SkylarkBimbleStreet - {state} - Gems {collected}/{_gemBounds.Length} - Hits {_deaths}";
    }

    private Vector2 GetMoveInput(KeyboardState keyboard, GamePadState gamePad)
    {
        var move = Vector2.Zero;

        if (keyboard.IsKeyDown(Keys.Left) || keyboard.IsKeyDown(Keys.A))
        {
            move.X -= 1f;
        }

        if (keyboard.IsKeyDown(Keys.Right) || keyboard.IsKeyDown(Keys.D))
        {
            move.X += 1f;
        }

        if (keyboard.IsKeyDown(Keys.Up) || keyboard.IsKeyDown(Keys.W))
        {
            move.Y -= 1f;
        }

        if (keyboard.IsKeyDown(Keys.Down) || keyboard.IsKeyDown(Keys.S))
        {
            move.Y += 1f;
        }

        var thumbstick = gamePad.ThumbSticks.Left;
        move.X += thumbstick.X;
        move.Y -= thumbstick.Y;
        return move;
    }

    private Rectangle GetPlayerBounds() => new((int)_playerPosition.X, (int)_playerPosition.Y, PlayerSize, PlayerSize);

    private static Rectangle GetExitBounds() => new(VirtualWidth - 150, VirtualHeight - 150, 92, 92);

    private bool AreAllGemsCollected()
    {
        foreach (var gemCollected in _gemsCollected)
        {
            if (!gemCollected)
            {
                return false;
            }
        }

        return true;
    }

    private Rectangle GetDestinationRectangle()
    {
        var viewport = GraphicsDevice.Viewport;
        var scale = Math.Min((float)viewport.Width / VirtualWidth, (float)viewport.Height / VirtualHeight);
        var width = (int)(VirtualWidth * scale);
        var height = (int)(VirtualHeight * scale);
        return new Rectangle((viewport.Width - width) / 2, (viewport.Height - height) / 2, width, height);
    }

    private void DrawRectangle(Rectangle rectangle, Color color) => _spriteBatch.Draw(_pixel, rectangle, color);

    private static Rectangle Inset(Rectangle rectangle, int inset) => new(
        rectangle.X + inset,
        rectangle.Y + inset,
        Math.Max(1, rectangle.Width - inset * 2),
        Math.Max(1, rectangle.Height - inset * 2));

    private bool WasPressed(KeyboardState keyboard, Keys key) => keyboard.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);

    private static bool WasPressed(ButtonState current, ButtonState previous) => current == ButtonState.Pressed && previous == ButtonState.Released;

    private struct Hazard
    {
        public Rectangle Bounds;
        public Vector2 Velocity;
        public readonly int Min;
        public readonly int Max;

        public Hazard(Rectangle bounds, Vector2 velocity, int min, int max)
        {
            Bounds = bounds;
            Velocity = velocity;
            Min = min;
            Max = max;
        }
    }
}
