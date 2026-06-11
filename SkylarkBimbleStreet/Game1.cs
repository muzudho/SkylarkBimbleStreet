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
    private int _selectedStageIndex;
    private int _pauseSelectionIndex;
    private int _deaths;
    private bool _cleared;
    private bool _stageSelectOpen;
    private bool _paused;
    private double _titleRefreshTimer;
    private double _clearAnimationTime;
    private double _stageSelectAnimationTime;
    private double _pauseAnimationTime;
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
        _deaths = 0;
        LoadStage(0);
        OpenStageSelect();
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
        var elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (gamePad.Buttons.Back == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        if (_stageSelectOpen)
        {
            _stageSelectAnimationTime += elapsed;
            UpdateStageSelect(keyboard, gamePad);
            UpdateWindowTitle(gameTime);
            _previousKeyboard = keyboard;
            _previousGamePad = gamePad;
            base.Update(gameTime);
            return;
        }

        if (_paused)
        {
            _pauseAnimationTime += elapsed;
            UpdatePauseMenu(keyboard, gamePad);
            UpdateWindowTitle(gameTime);
            _previousKeyboard = keyboard;
            _previousGamePad = gamePad;
            base.Update(gameTime);
            return;
        }

        if (!_cleared && (WasPressed(keyboard, Keys.Enter) || WasPressed(gamePad.Buttons.Start, _previousGamePad.Buttons.Start)))
        {
            OpenPauseMenu();
        }
        else if (WasPressed(keyboard, Keys.Tab))
        {
            OpenStageSelect();
        }
        else if (WasPressed(keyboard, Keys.R))
        {
            ResetRun();
        }

        if (!_stageSelectOpen && !_paused && !_cleared)
        {
            MovePlayer(GetMoveInput(keyboard, gamePad), elapsed);
            UpdateHazards(elapsed);
            CheckGemCollection();
            CheckHazardCollision();
            CheckExit();
        }
        else if (_cleared)
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

        if (_stageSelectOpen)
        {
            DrawStageSelect();
        }

        if (_paused)
        {
            DrawPauseMenu();
        }
    }

    private void DrawPauseMenu()
    {
        DrawRectangle(new Rectangle(0, 0, VirtualWidth, VirtualHeight), new Color(4, 6, 10, 185));

        var pulse = (float)((Math.Sin(_pauseAnimationTime * 5d) + 1d) * 0.5d);
        var panel = new Rectangle(500, 320, 920, 420);
        DrawRectangle(panel, new Color(18, 24, 34, 238));
        DrawFrame(panel, new Color(245, 198, 80), 16);
        DrawFrame(Inset(panel, 38), new Color(81, 161, 255, 150), 8);

        for (var i = 0; i < 3; i++)
        {
            DrawPauseOption(i, i == _pauseSelectionIndex, pulse);
        }
    }

    private void DrawPauseOption(int optionIndex, bool selected, float pulse)
    {
        var width = selected ? 210 + (int)(pulse * 12f) : 180;
        var height = selected ? 210 + (int)(pulse * 12f) : 180;
        var centerX = 690 + optionIndex * 270;
        var centerY = selected ? 530 - (int)(pulse * 5f) : 540;
        var card = new Rectangle(centerX - width / 2, centerY - height / 2, width, height);
        var body = selected ? new Color(31, 37, 50, 250) : new Color(20, 26, 34, 235);
        var frame = selected ? new Color(255, 239, 151) : new Color(104, 116, 140);

        DrawRectangle(card, body);
        DrawFrame(card, frame, selected ? 12 : 8);

        if (optionIndex == 0)
        {
            DrawArrow(new Rectangle(card.X + 54, card.Y + 62, card.Width - 108, card.Height - 124), true, new Color(74, 205, 116));
        }
        else if (optionIndex == 1)
        {
            DrawFrame(new Rectangle(card.X + 54, card.Y + 54, card.Width - 108, card.Height - 108), new Color(221, 72, 92), 14);
            DrawLine(new Vector2(card.X + 70, card.Bottom - 70), new Vector2(card.Right - 70, card.Y + 70), 16, new Color(255, 148, 157));
        }
        else
        {
            for (var i = 0; i < 3; i++)
            {
                DrawRectangle(new Rectangle(card.X + 48 + i * 42, card.Y + 58, 30, 30), i == _selectedStageIndex ? new Color(245, 198, 80) : new Color(81, 161, 255));
            }

            DrawFrame(new Rectangle(card.X + 48, card.Y + 112, card.Width - 96, 46), new Color(74, 205, 116), 8);
        }

        if (selected)
        {
            DrawFrame(new Rectangle(card.X - 18, card.Y - 18, card.Width + 36, card.Height + 36), new Color(245, 198, 80, 160), 6);
        }
    }

    private void DrawStageSelect()
    {
        DrawRectangle(new Rectangle(0, 0, VirtualWidth, VirtualHeight), new Color(5, 7, 12, 210));

        var center = new Vector2(VirtualWidth / 2f, VirtualHeight / 2f);
        var pulse = (float)((Math.Sin(_stageSelectAnimationTime * 5d) + 1d) * 0.5d);
        for (var i = 0; i < 18; i++)
        {
            var angle = (float)(i * Math.PI * 2d / 18d - _stageSelectAnimationTime * 0.25d);
            DrawLine(center, center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 560f, 6, new Color(81, 161, 255, 58));
        }

        DrawArrow(new Rectangle(160, 490, 150, 100), false, new Color(245, 198, 80));
        DrawArrow(new Rectangle(VirtualWidth - 310, 490, 150, 100), true, new Color(245, 198, 80));

        for (var i = 0; i < _stages.Length; i++)
        {
            DrawStageCard(i, i == _selectedStageIndex, pulse);
        }
    }

    private void DrawStageCard(int stageIndex, bool selected, float pulse)
    {
        var width = selected ? 380 + (int)(pulse * 14f) : 320;
        var height = selected ? 500 + (int)(pulse * 14f) : 420;
        var gap = 430;
        var centerX = VirtualWidth / 2 + (stageIndex - 1) * gap;
        var y = selected ? 290 - (int)(pulse * 7f) : 335;
        var card = new Rectangle(centerX - width / 2, y, width, height);
        var frameColor = selected ? new Color(245, 198, 80) : new Color(69, 75, 90);
        var bodyColor = selected ? new Color(31, 37, 50, 245) : new Color(20, 26, 34, 230);

        DrawRectangle(card, bodyColor);
        DrawFrame(card, frameColor, selected ? 16 : 10);
        DrawFrame(Inset(card, 34), selected ? new Color(74, 205, 116) : new Color(76, 84, 103), selected ? 8 : 6);

        var preview = Inset(card, 70);
        DrawRectangle(new Rectangle(preview.X, preview.Y, preview.Width, 10), new Color(104, 116, 140));
        DrawRectangle(new Rectangle(preview.X, preview.Bottom - 10, preview.Width, 10), new Color(104, 116, 140));
        DrawRectangle(new Rectangle(preview.X, preview.Y, 10, preview.Height), new Color(104, 116, 140));
        DrawRectangle(new Rectangle(preview.Right - 10, preview.Y, 10, preview.Height), new Color(104, 116, 140));

        for (var i = 0; i <= stageIndex; i++)
        {
            DrawGem(new Vector2(card.X + 82 + i * 54, card.Y + 76), 36, new Color(245, 198, 80), new Color(255, 239, 151));
        }

        for (var i = 0; i < 3 + stageIndex; i++)
        {
            var x = preview.X + 48 + i * 42;
            var barHeight = 78 + (i % 2) * 58;
            DrawRectangle(new Rectangle(x, preview.Y + 36, 14, barHeight), new Color(76, 84, 103));
        }

        DrawRectangle(new Rectangle(preview.Right - 70, preview.Y + 34, 40, 40), new Color(74, 205, 116));
        DrawRectangle(new Rectangle(preview.Right - 58, preview.Y + 46, 16, 16), bodyColor);
        DrawRectangle(new Rectangle(preview.X + 30, preview.Bottom - 70, 36, 36), new Color(81, 161, 255));
        DrawRectangle(new Rectangle(preview.X + 40, preview.Bottom - 60, 16, 16), new Color(197, 228, 255));

        if (selected)
        {
            DrawFrame(new Rectangle(card.X - 22, card.Y - 22, card.Width + 44, card.Height + 44), new Color(255, 239, 151, 160), 8);
        }
    }

    private void DrawArrow(Rectangle bounds, bool right, Color color)
    {
        var head = right ? bounds.Right - 42 : bounds.X + 42;
        var tail = right ? bounds.X + 32 : bounds.Right - 32;
        DrawLine(new Vector2(tail, bounds.Center.Y), new Vector2(head, bounds.Center.Y), 18, color);
        DrawLine(new Vector2(head, bounds.Center.Y), new Vector2(right ? head - 36 : head + 36, bounds.Y + 18), 18, color);
        DrawLine(new Vector2(head, bounds.Center.Y), new Vector2(right ? head - 36 : head + 36, bounds.Bottom - 18), 18, color);
    }

    private void DrawClearCelebration()
    {
        DrawRectangle(new Rectangle(0, 0, VirtualWidth, VirtualHeight), new Color(6, 8, 14, 176));

        var center = new Vector2(VirtualWidth / 2f, VirtualHeight / 2f);
        var pulse = (float)((Math.Sin(_clearAnimationTime * 5d) + 1d) * 0.5d);
        var glow = new Color(65 + (int)(pulse * 55), 210, 150 + (int)(pulse * 55), 205);

        for (var i = 0; i < 24; i++)
        {
            var angle = (float)(i * Math.PI * 2d / 24d + _clearAnimationTime * 0.42d);
            var length = 340 + (i % 4) * 80 + (int)(pulse * 35f);
            DrawLine(center, center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * length, 10, new Color(74, 205, 116, 95));
        }

        DrawConfetti();
        DrawFrame(new Rectangle(540, 300, 840, 480), new Color(245, 198, 80), 18);
        DrawFrame(new Rectangle(590, 350, 740, 380), glow, 12);
        DrawRectangle(new Rectangle(630, 390, 660, 300), new Color(20, 26, 28, 230));

        DrawOrbitingGems(center);

        var mainBob = (float)Math.Sin(_clearAnimationTime * 3.8d) * 16f;
        var sideBob = (float)Math.Sin(_clearAnimationTime * 4.6d + Math.PI) * 10f;
        DrawGem(new Vector2(960, 505 + mainBob), 170 + (int)(pulse * 12f), new Color(245, 198, 80), new Color(255, 239, 151));
        DrawGem(new Vector2(760, 545 + sideBob), 86, new Color(81, 161, 255), new Color(197, 228, 255));
        DrawGem(new Vector2(1160, 545 - sideBob), 86, new Color(221, 72, 92), new Color(255, 148, 157));

        for (var i = 0; i < _stages.Length; i++)
        {
            var checkPulse = i == _currentStageIndex ? (int)(pulse * 8f) : 0;
            DrawRectangle(new Rectangle(828 + i * 78 - checkPulse / 2, 650 - checkPulse / 2, 54 + checkPulse, 54 + checkPulse), new Color(74, 205, 116));
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

        var fall = (int)(_clearAnimationTime * 210d) % VirtualHeight;
        for (var i = 0; i < 112; i++)
        {
            var layer = i % 2;
            var xDrift = (int)(Math.Sin(_clearAnimationTime * (1.2d + layer) + i) * (18 + layer * 16));
            var x = 90 + i * 251 % (VirtualWidth - 180) + xDrift;
            var y = (i * 97 + fall + layer * 45) % VirtualHeight;
            var width = 10 + i % 5 * 4;
            var height = layer == 0 ? width : Math.Max(6, width / 2);
            DrawRectangle(new Rectangle(x, y, width, height), colors[i % colors.Length]);
        }
    }

    private void DrawOrbitingGems(Vector2 center)
    {
        Color[] colors =
        [
            new(245, 198, 80),
            new(81, 161, 255),
            new(221, 72, 92),
            new(74, 205, 116),
        ];

        for (var i = 0; i < 12; i++)
        {
            var angle = (float)(i * Math.PI * 2d / 12d - _clearAnimationTime * 1.4d);
            var radius = 235 + (i % 3) * 34;
            var position = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
            var size = 24 + i % 4 * 6;
            DrawGem(position, size, colors[i % colors.Length], new Color(255, 239, 151));
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

    private void UpdatePauseMenu(KeyboardState keyboard, GamePadState gamePad)
    {
        if (WasPressed(keyboard, Keys.Left) || WasPressed(keyboard, Keys.A) || WasPressed(gamePad.DPad.Left, _previousGamePad.DPad.Left) || WasThumbstickPressedLeft(gamePad))
        {
            _pauseSelectionIndex = (_pauseSelectionIndex + 2) % 3;
        }

        if (WasPressed(keyboard, Keys.Right) || WasPressed(keyboard, Keys.D) || WasPressed(gamePad.DPad.Right, _previousGamePad.DPad.Right) || WasThumbstickPressedRight(gamePad))
        {
            _pauseSelectionIndex = (_pauseSelectionIndex + 1) % 3;
        }

        if (WasPressed(keyboard, Keys.Enter) || WasPressed(gamePad.Buttons.Start, _previousGamePad.Buttons.Start))
        {
            ClosePauseMenu();
            return;
        }

        if (WasPressed(keyboard, Keys.Space) || WasPressed(gamePad.Buttons.A, _previousGamePad.Buttons.A))
        {
            SelectPauseOption();
        }
    }

    private void OpenPauseMenu()
    {
        _paused = true;
        _pauseSelectionIndex = 0;
        _pauseAnimationTime = 0d;
        RefreshWindowTitle();
    }

    private void ClosePauseMenu()
    {
        _paused = false;
        _pauseAnimationTime = 0d;
        RefreshWindowTitle();
    }

    private void SelectPauseOption()
    {
        if (_pauseSelectionIndex == 0)
        {
            ClosePauseMenu();
            return;
        }

        if (_pauseSelectionIndex == 1)
        {
            _paused = false;
            _pauseAnimationTime = 0d;
            LoadStage(_currentStageIndex);
            return;
        }

        _paused = false;
        _pauseAnimationTime = 0d;
        OpenStageSelect();
    }
    private void UpdateStageSelect(KeyboardState keyboard, GamePadState gamePad)
    {
        if (WasPressed(keyboard, Keys.Left) || WasPressed(keyboard, Keys.A) || WasPressed(gamePad.DPad.Left, _previousGamePad.DPad.Left) || WasThumbstickPressedLeft(gamePad))
        {
            _selectedStageIndex = (_selectedStageIndex + _stages.Length - 1) % _stages.Length;
        }

        if (WasPressed(keyboard, Keys.Right) || WasPressed(keyboard, Keys.D) || WasPressed(gamePad.DPad.Right, _previousGamePad.DPad.Right) || WasThumbstickPressedRight(gamePad))
        {
            _selectedStageIndex = (_selectedStageIndex + 1) % _stages.Length;
        }

        if (WasPressed(keyboard, Keys.Enter)
            || WasPressed(keyboard, Keys.Space)
            || WasPressed(gamePad.Buttons.Start, _previousGamePad.Buttons.Start)
            || WasPressed(gamePad.Buttons.A, _previousGamePad.Buttons.A))
        {
            StartSelectedStage();
        }
    }

    private void OpenStageSelect()
    {
        _paused = false;
        _stageSelectOpen = true;
        _selectedStageIndex = _currentStageIndex;
        _stageSelectAnimationTime = 0d;
        RefreshWindowTitle();
    }

    private void StartSelectedStage()
    {
        _deaths = 0;
        LoadStage(_selectedStageIndex);
        _stageSelectOpen = false;
        _paused = false;
        _stageSelectAnimationTime = 0d;
        RefreshWindowTitle();
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
        _stageSelectOpen = false;
        _paused = false;
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

        var state = _stageSelectOpen ? "STAGE SELECT - Left/Right choose - Enter/Space/Start play" : _paused ? "PAUSE - Left/Right choose - Space/A select - Start/Enter resume" : _cleared ? "CLEAR - Press R / Start to retry - Tab for stage select" : "Collect all gems and reach the green exit - Start/Enter pause - Tab for stage select";
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

        if (keyboard.IsKeyDown(Keys.Left) || keyboard.IsKeyDown(Keys.A) || gamePad.DPad.Left == ButtonState.Pressed)
        {
            move.X -= 1f;
        }

        if (keyboard.IsKeyDown(Keys.Right) || keyboard.IsKeyDown(Keys.D) || gamePad.DPad.Right == ButtonState.Pressed)
        {
            move.X += 1f;
        }

        if (keyboard.IsKeyDown(Keys.Up) || keyboard.IsKeyDown(Keys.W) || gamePad.DPad.Up == ButtonState.Pressed)
        {
            move.Y -= 1f;
        }

        if (keyboard.IsKeyDown(Keys.Down) || keyboard.IsKeyDown(Keys.S) || gamePad.DPad.Down == ButtonState.Pressed)
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

    private bool WasThumbstickPressedLeft(GamePadState gamePad) => gamePad.ThumbSticks.Left.X < -0.55f && _previousGamePad.ThumbSticks.Left.X >= -0.55f;

    private bool WasThumbstickPressedRight(GamePadState gamePad) => gamePad.ThumbSticks.Left.X > 0.55f && _previousGamePad.ThumbSticks.Left.X <= 0.55f;

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
