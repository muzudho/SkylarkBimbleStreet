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

    private readonly Stage[] _stages =
    [
        new(
            "Stage 1",
            new Vector2(95, 95),
            new Rectangle(VirtualWidth - 150, VirtualHeight - 150, 92, 92),
            new Color(18, 22, 31),
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
            ],
            [
                new(390, 250, 34, 34),
                new(660, 820, 34, 34),
                new(920, 230, 34, 34),
                new(1190, 760, 34, 34),
                new(1450, 230, 34, 34),
            ],
            [
                new(new Rectangle(340, 500, 64, 64), new Vector2(0f, 250f), 410, 760),
                new(new Rectangle(870, 460, 64, 64), new Vector2(0f, -310f), 330, 720),
                new(new Rectangle(1160, 410, 64, 64), new Vector2(0f, 280f), 330, 720),
                new(new Rectangle(1400, 720, 64, 64), new Vector2(300f, 0f), 1360, 1510),
            ]),
        new(
            "Stage 2",
            new Vector2(95, VirtualHeight - 155),
            new Rectangle(VirtualWidth - 150, 58, 92, 92),
            new Color(20, 26, 28),
            [
                new(0, 0, VirtualWidth, 38),
                new(0, VirtualHeight - 38, VirtualWidth, 38),
                new(0, 0, 38, VirtualHeight),
                new(VirtualWidth - 38, 0, 38, VirtualHeight),
                new(250, 120, 38, 750),
                new(430, 870, 600, 38),
                new(560, 120, 38, 610),
                new(820, 300, 38, 730),
                new(1080, 70, 38, 680),
                new(1320, 250, 38, 640),
                new(1520, 120, 38, 620),
                new(1120, 210, 380, 38),
            ],
            [
                new(360, 760, 34, 34),
                new(690, 170, 34, 34),
                new(940, 790, 34, 34),
                new(1210, 140, 34, 34),
                new(1640, 600, 34, 34),
                new(1660, 210, 34, 34),
            ],
            [
                new(new Rectangle(350, 220, 64, 64), new Vector2(0f, 330f), 180, 780),
                new(new Rectangle(650, 740, 64, 64), new Vector2(300f, 0f), 630, 760),
                new(new Rectangle(930, 360, 64, 64), new Vector2(0f, 270f), 330, 840),
                new(new Rectangle(1190, 640, 64, 64), new Vector2(340f, 0f), 1160, 1280),
                new(new Rectangle(1620, 250, 64, 64), new Vector2(0f, 300f), 210, 660),
            ]),
        new(
            "Stage 3",
            new Vector2(95, 520),
            new Rectangle(VirtualWidth - 150, 58, 92, 92),
            new Color(24, 22, 31),
            [
                new(0, 0, VirtualWidth, 38),
                new(0, VirtualHeight - 38, VirtualWidth, 38),
                new(0, 0, 38, VirtualHeight),
                new(VirtualWidth - 38, 0, 38, VirtualHeight),
                new(280, 38, 38, 360),
                new(280, 650, 38, 392),
                new(520, 210, 38, 640),
                new(760, 38, 38, 520),
                new(760, 780, 38, 262),
                new(1000, 220, 38, 280),
                new(1000, 650, 38, 190),
                new(1240, 38, 38, 360),
                new(1240, 650, 38, 392),
                new(1480, 220, 38, 260),
                new(1480, 650, 38, 210),
                new(330, 398, 190, 38),
                new(800, 558, 200, 38),
                new(1038, 840, 202, 38),
            ],
            [
                new(390, 500, 34, 34),
                new(640, 930, 34, 34),
                new(890, 630, 34, 34),
                new(1130, 920, 34, 34),
                new(1350, 500, 34, 34),
                new(1630, 150, 34, 34),
            ],
            [
                new(new Rectangle(390, 430, 64, 64), new Vector2(0f, 290f), 430, 590),
                new(new Rectangle(620, 920, 64, 64), new Vector2(310f, 0f), 600, 710),
                new(new Rectangle(880, 640, 64, 64), new Vector2(0f, 300f), 610, 760),
                new(new Rectangle(1110, 470, 64, 64), new Vector2(330f, 0f), 1060, 1160),
                new(new Rectangle(1370, 430, 64, 64), new Vector2(0f, 280f), 410, 610),
                new(new Rectangle(1600, 130, 64, 64), new Vector2(320f, 0f), 1540, 1710),
            ]),
    ];

    private Rectangle[] _walls = [];
    private Rectangle[] _gemBounds = [];
    private Hazard[] _hazards = [];
    private bool[] _gemsCollected = [];

    private Vector2 _playerPosition;
    private Vector2 _playerStart;
    private Rectangle _exitBounds;
    private KeyboardState _previousKeyboard;
    private GamePadState _previousGamePad;
    private int _currentStageIndex;
    private int _deaths;
    private bool _cleared;
    private double _titleRefreshTimer;
    private double _clearAnimationTime;
    private Color _backgroundColor;

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
        else
        {
            _clearAnimationTime += elapsed;
        }

        UpdateWindowTitle(gameTime);

        _previousKeyboard = keyboard;
        _previousGamePad = gamePad;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_scene);
        GraphicsDevice.Clear(_backgroundColor);

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
        DrawRectangle(new Rectangle(0, 0, VirtualWidth, VirtualHeight), _backgroundColor);
        DrawGrid();

        foreach (var wall in _walls)
        {
            DrawRectangle(wall, new Color(76, 84, 103));
            DrawRectangle(Inset(wall, 5), new Color(104, 116, 140));
        }

        var exitColor = AreAllGemsCollected() ? new Color(74, 205, 116) : new Color(62, 78, 72);
        DrawRectangle(GetExitBounds(), exitColor);
        DrawRectangle(Inset(GetExitBounds(), 12), _backgroundColor);

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
        DrawRectangle(new Rectangle(58, 58, Math.Max(120, 40 + _gemBounds.Length * 76), 22), new Color(44, 51, 67));
        for (var i = 0; i < _gemBounds.Length; i++)
        {
            var color = _gemsCollected[i] ? new Color(245, 198, 80) : new Color(69, 75, 90);
            DrawRectangle(new Rectangle(70 + i * 76, 52, 42, 42), color);
        }

        for (var i = 0; i < Math.Min(_deaths, 8); i++)
        {
            DrawRectangle(new Rectangle(70 + i * 38, 112, 24, 24), new Color(221, 72, 92));
        }

        for (var i = 0; i < _stages.Length; i++)
        {
            var color = i == _currentStageIndex ? new Color(81, 161, 255) : new Color(69, 75, 90);
            DrawRectangle(new Rectangle(VirtualWidth - 190 + i * 54, 58, 38, 38), color);
        }

        if (_cleared)
        {
            DrawClearCelebration();
        }
    }

    private void DrawClearCelebration()
    {
        DrawRectangle(new Rectangle(0, 0, VirtualWidth, VirtualHeight), new Color(6, 8, 14, 176));

        var center = new Vector2(VirtualWidth / 2f, VirtualHeight / 2f);
        var pulse = (float)((Math.Sin(_clearAnimationTime * 5d) + 1d) * 0.5d);
        var glow = new Color(65 + (int)(pulse * 55), 210, 150 + (int)(pulse * 55), 205);

        for (var i = 0; i < 20; i++)
        {
            var angle = (float)(i * Math.PI * 2d / 20d + _clearAnimationTime * 0.35d);
            var length = 360 + (i % 4) * 70;
            DrawLine(center, center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * length, 10, new Color(74, 205, 116, 95));
        }

        DrawConfetti();
        DrawFrame(new Rectangle(540, 300, 840, 480), new Color(245, 198, 80), 18);
        DrawFrame(new Rectangle(590, 350, 740, 380), glow, 12);
        DrawRectangle(new Rectangle(630, 390, 660, 300), new Color(20, 26, 28, 230));

        DrawGem(new Vector2(960, 505), 170, new Color(245, 198, 80), new Color(255, 239, 151));
        DrawGem(new Vector2(760, 545), 86, new Color(81, 161, 255), new Color(197, 228, 255));
        DrawGem(new Vector2(1160, 545), 86, new Color(221, 72, 92), new Color(255, 148, 157));

        for (var i = 0; i < _stages.Length; i++)
        {
            DrawRectangle(new Rectangle(828 + i * 78, 650, 54, 54), new Color(74, 205, 116));
            DrawRectangle(new Rectangle(842 + i * 78, 664, 26, 26), new Color(255, 239, 151));
        }
    }

    private void DrawConfetti()
    {
        Color[] colors =
        [
            new(245, 198, 80),
            new(81, 161, 255),
            new(221, 72, 92),
            new(74, 205, 116),
            new(255, 239, 151),
        ];

        var fall = (int)(_clearAnimationTime * 180d) % VirtualHeight;
        for (var i = 0; i < 72; i++)
        {
            var x = 90 + i * 251 % (VirtualWidth - 180);
            var y = (i * 97 + fall) % VirtualHeight;
            var size = 10 + i % 5 * 4;
            DrawRectangle(new Rectangle(x, y, size, size), colors[i % colors.Length]);
        }
    }

    private void DrawGem(Vector2 center, int size, Color body, Color shine)
    {
        var half = size / 2;
        DrawRectangle(new Rectangle((int)center.X - half, (int)center.Y - size / 4, size, size / 2), body);
        DrawRectangle(new Rectangle((int)center.X - size / 3, (int)center.Y - half, size * 2 / 3, size), body);
        DrawRectangle(new Rectangle((int)center.X - size / 5, (int)center.Y - size / 5, size * 2 / 5, size * 2 / 5), shine);
    }

    private void DrawFrame(Rectangle rectangle, Color color, int thickness)
    {
        DrawRectangle(new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color);
        DrawRectangle(new Rectangle(rectangle.X, rectangle.Bottom - thickness, rectangle.Width, thickness), color);
        DrawRectangle(new Rectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), color);
        DrawRectangle(new Rectangle(rectangle.Right - thickness, rectangle.Y, thickness, rectangle.Height), color);
    }

    private void DrawLine(Vector2 start, Vector2 end, int width, Color color)
    {
        var edge = end - start;
        var angle = (float)Math.Atan2(edge.Y, edge.X);
        _spriteBatch.Draw(_pixel, start, null, color, angle, new Vector2(0f, 0.5f), new Vector2(edge.Length(), width), SpriteEffects.None, 0f);
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
            if (_currentStageIndex + 1 < _stages.Length)
            {
                LoadStage(_currentStageIndex + 1);
                return;
            }

            _cleared = true;
            _clearAnimationTime = 0d;
        }
    }

    private void ResetRun()
    {
        _deaths = 0;
        LoadStage(0);
        RefreshWindowTitle();
    }

    private void LoadStage(int stageIndex)
    {
        _currentStageIndex = stageIndex;
        var stage = _stages[_currentStageIndex];
        _walls = stage.Walls;
        _gemBounds = stage.Gems;
        _hazards = (Hazard[])stage.Hazards.Clone();
        _gemsCollected = new bool[_gemBounds.Length];
        _playerStart = stage.PlayerStart;
        _exitBounds = stage.ExitBounds;
        _backgroundColor = stage.BackgroundColor;
        _cleared = false;
        _clearAnimationTime = 0d;
        ResetPlayerOnly();
        RefreshWindowTitle();
    }

    private void ResetPlayerOnly()
    {
        _playerPosition = _playerStart;
    }

    /// <summary>
    /// ウィンドウ・タイトルを更新します。
    /// </summary>
    /// <param name="gameTime">ゲーム時間の情報</param>
    private void UpdateWindowTitle(GameTime gameTime)
    {
        _titleRefreshTimer -= gameTime.ElapsedGameTime.TotalSeconds;
        if (_titleRefreshTimer > 0)
        {
            return;
        }

        RefreshWindowTitle();
    }

    /// <summary>
    /// ウィンドウ・タイトルを最新表示します。
    /// </summary>
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
        var stage = _stages[_currentStageIndex];
        Window.Title = $"SkylarkBimbleStreet - {stage.Name} - {state} - Gems {collected}/{_gemBounds.Length} - Hits {_deaths}";
    }

    /// <summary>
    /// 移動の入力を取得します。
    /// </summary>
    /// <param name="keyboard">キーボードの状態</param>
    /// <param name="gamePad">ゲームパッドの状態</param>
    /// <returns>移動の入力を表すベクトル</returns>
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

    /// <summary>
    /// プレイヤーの当たり判定を取得します。
    /// </summary>
    /// <returns>プレイヤーの当たり判定を表す矩形</returns>
    private Rectangle GetPlayerBounds() => new((int)_playerPosition.X, (int)_playerPosition.Y, PlayerSize, PlayerSize);

    /// <summary>
    /// 出口の当たり判定を取得します。
    /// </summary>
    /// <returns>出口の当たり判定を表す矩形</returns>
    private Rectangle GetExitBounds() => _exitBounds;

    /// <summary>
    /// 全てのジェムを集めたか。
    /// </summary>
    /// <returns>全てのジェムを集めた場合はtrue、それ以外の場合はfalse</returns>
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

    /// <summary>
    /// 移動先の座標とサイズを指定して、衝突判定に使用する矩形を描画します。
    /// </summary>
    /// <returns>衝突判定に使用する矩形</returns>
    private Rectangle GetDestinationRectangle()
    {
        var viewport = GraphicsDevice.Viewport;
        var scale = Math.Min((float)viewport.Width / VirtualWidth, (float)viewport.Height / VirtualHeight);
        var width = (int)(VirtualWidth * scale);
        var height = (int)(VirtualHeight * scale);
        return new Rectangle((viewport.Width - width) / 2, (viewport.Height - height) / 2, width, height);
    }

    /// <summary>
    /// 矩形の描画
    /// </summary>
    /// <param name="rectangle"></param>
    /// <param name="color"></param>
    private void DrawRectangle(Rectangle rectangle, Color color) => _spriteBatch.Draw(_pixel, rectangle, color);

    /// <summary>
    /// 矩形を内側に縮小した新しい矩形を返す。
    /// </summary>
    /// <param name="rectangle">元の矩形</param>
    /// <param name="inset">縮小する量</param>
    /// <returns>縮小された矩形</returns>
    private static Rectangle Inset(Rectangle rectangle, int inset) => new(
        rectangle.X + inset,
        rectangle.Y + inset,
        Math.Max(1, rectangle.Width - inset * 2),
        Math.Max(1, rectangle.Height - inset * 2));

    /// <summary>
    /// ボタンが押されたか。
    /// </summary>
    /// <param name="keyboard"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    private bool WasPressed(KeyboardState keyboard, Keys key) => keyboard.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);

    /// <summary>
    /// ボタンが押されているか。
    /// </summary>
    /// <param name="current"></param>
    /// <param name="previous"></param>
    /// <returns></returns>
    private static bool WasPressed(ButtonState current, ButtonState previous) => current == ButtonState.Pressed && previous == ButtonState.Released;

    /// <summary>
    /// ステージ（＾▽＾）
    /// </summary>
    private sealed class Stage
    {
        public readonly string Name;
        public readonly Vector2 PlayerStart;
        public readonly Rectangle ExitBounds;
        public readonly Color BackgroundColor;
        public readonly Rectangle[] Walls;
        public readonly Rectangle[] Gems;
        public readonly Hazard[] Hazards;

        public Stage(
            string name,
            Vector2 playerStart,
            Rectangle exitBounds,
            Color backgroundColor,
            Rectangle[] walls,
            Rectangle[] gems,
            Hazard[] hazards)
        {
            Name = name;
            PlayerStart = playerStart;
            ExitBounds = exitBounds;
            BackgroundColor = backgroundColor;
            Walls = walls;
            Gems = gems;
            Hazards = hazards;
        }
    }

    /// <summary>
    /// 障害物（＾▽＾）
    /// </summary>
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
