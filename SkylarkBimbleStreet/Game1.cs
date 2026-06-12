namespace SkylarkBimbleStreet;

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class Game1 : Game
{
    private const int VirtualWidth = 1920;
    private const int VirtualHeight = 1080;
    private const float PlayerSpeed = 520f;
    private const int PlayerSize = 46;
    private const float RespawnInvincibleSeconds = 0.8f;

    private readonly GraphicsDeviceManager _graphics;
    private readonly List<PlayEvent> _playEvents = [];
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
        new(
            "Stage 4",
            new Vector2(95, 95),
            new Rectangle(VirtualWidth - 150, VirtualHeight - 150, 92, 92),
            new Color(22, 20, 28),
            [
                new(0, 0, VirtualWidth, 38),
                new(0, VirtualHeight - 38, VirtualWidth, 38),
                new(0, 0, 38, VirtualHeight),
                new(VirtualWidth - 38, 0, 38, VirtualHeight),
                new(360, 38, 38, 300),
                new(360, 520, 38, 522),
                new(660, 250, 38, 250),
                new(660, 760, 38, 282),
                new(960, 38, 38, 430),
                new(960, 690, 38, 352),
                new(1260, 250, 38, 250),
                new(1260, 760, 38, 282),
                new(1560, 38, 38, 240),
                new(1560, 650, 38, 392),
                new(398, 330, 262, 38),
                new(698, 650, 262, 38),
                new(998, 210, 262, 38),
                new(1298, 490, 262, 38),
            ],
            [
                new(470, 440, 34, 34),
                new(770, 150, 34, 34),
                new(850, 820, 34, 34),
                new(1080, 560, 34, 34),
                new(1370, 360, 34, 34),
                new(1680, 740, 34, 34),
                new(1710, 210, 34, 34),
            ],
            [
                new(new Rectangle(230, 410, 64, 64), new Vector2(0f, 310f), 170, 760),
                new(new Rectangle(510, 500, 64, 64), new Vector2(320f, 0f), 430, 590),
                new(new Rectangle(800, 140, 64, 64), new Vector2(0f, 300f), 110, 220),
                new(new Rectangle(1110, 560, 64, 64), new Vector2(330f, 0f), 1040, 1160),
                new(new Rectangle(1400, 360, 64, 64), new Vector2(0f, 320f), 330, 450),
                new(new Rectangle(1680, 720, 64, 64), new Vector2(0f, 300f), 650, 850),
            ]),
        new(
            "Stage 5",
            new Vector2(95, 95),
            new Rectangle(VirtualWidth - 150, VirtualHeight - 150, 92, 92),
            new Color(18, 25, 32),
            [
                new(0, 0, VirtualWidth, 38),
                new(0, VirtualHeight - 38, VirtualWidth, 38),
                new(0, 0, 38, VirtualHeight),
                new(VirtualWidth - 38, 0, 38, VirtualHeight),
                new(300, 38, 38, 240),
                new(300, 500, 38, 542),
                new(560, 250, 38, 360),
                new(560, 820, 38, 222),
                new(820, 38, 38, 520),
                new(820, 760, 38, 282),
                new(1080, 38, 38, 280),
                new(1080, 500, 38, 542),
                new(1340, 38, 38, 640),
                new(1340, 860, 38, 182),
                new(1600, 38, 38, 350),
                new(1600, 760, 38, 282),
                new(338, 500, 150, 38),
                new(598, 610, 150, 38),
                new(858, 318, 150, 38),
                new(1118, 678, 150, 38),
                new(1378, 388, 150, 38),
            ],
            [
                new(190, 350, 34, 34),
                new(430, 160, 34, 34),
                new(690, 660, 34, 34),
                new(950, 380, 34, 34),
                new(1210, 760, 34, 34),
                new(1470, 470, 34, 34),
                new(1710, 890, 34, 34),
            ],
            [
                new(new Rectangle(180, 320, 64, 64), new Vector2(0f, 300f), 260, 430),
                new(new Rectangle(420, 250, 64, 64), new Vector2(320f, 0f), 370, 500),
                new(new Rectangle(690, 620, 64, 64), new Vector2(0f, 300f), 590, 720),
                new(new Rectangle(950, 370, 64, 64), new Vector2(330f, 0f), 900, 1020),
                new(new Rectangle(1210, 730, 64, 64), new Vector2(0f, 310f), 700, 810),
                new(new Rectangle(1470, 460, 64, 64), new Vector2(330f, 0f), 1420, 1540),
                new(new Rectangle(1700, 800, 64, 64), new Vector2(0f, 280f), 760, 910),
            ]),
    ];

    private readonly GamePalette[] _palettes =
    [
        new("Normal", new Color(18, 22, 31), new Color(31, 37, 50), new Color(76, 84, 103), new Color(104, 116, 140), new Color(81, 161, 255), new Color(197, 228, 255), new Color(255, 239, 151), new Color(245, 198, 80), new Color(255, 239, 151), new Color(221, 72, 92), new Color(255, 148, 157), new Color(74, 205, 116), new Color(62, 78, 72), new Color(44, 51, 67), new Color(69, 75, 90), new Color(81, 161, 255)),
        new("Accessible", new Color(16, 18, 22), new Color(42, 45, 52), new Color(92, 99, 110), new Color(134, 144, 158), new Color(0, 170, 210), new Color(214, 248, 255), new Color(255, 255, 255), new Color(245, 210, 65), new Color(255, 252, 190), new Color(210, 82, 36), new Color(255, 180, 120), new Color(0, 154, 120), new Color(76, 88, 86), new Color(48, 52, 60), new Color(78, 84, 94), new Color(0, 170, 210)),
        new("High Contrast", new Color(8, 8, 10), new Color(50, 50, 56), new Color(128, 128, 136), new Color(190, 190, 198), new Color(60, 220, 255), new Color(235, 252, 255), new Color(255, 255, 255), new Color(255, 230, 80), new Color(255, 255, 210), new Color(255, 115, 55), new Color(255, 205, 170), new Color(90, 255, 170), new Color(72, 86, 78), new Color(38, 38, 44), new Color(92, 92, 102), new Color(60, 220, 255)),
        new("Mono Check", new Color(18, 18, 18), new Color(42, 42, 42), new Color(88, 88, 88), new Color(132, 132, 132), new Color(210, 210, 210), new Color(246, 246, 246), new Color(255, 255, 255), new Color(176, 176, 176), new Color(232, 232, 232), new Color(108, 108, 108), new Color(190, 190, 190), new Color(236, 236, 236), new Color(70, 70, 70), new Color(46, 46, 46), new Color(92, 92, 92), new Color(214, 214, 214)),
    ];

    private Rectangle[] _walls = [];
    private Rectangle[] _gemBounds = [];
    private Hazard[] _hazards = [];
    private bool[] _gemsCollected = [];
    private int[] _stageDeathCounts = [];
    private int[] _stageGemCounts = [];
    private double[] _stageElapsedSeconds = [];
    private double _runElapsedSeconds;
    private double _currentStageElapsedSeconds;
    private int _pauseCount;
    private int _retryCount;
    private bool _startedFromStageSelect;

    private Vector2 _playerPosition;
    private Vector2 _playerStart;
    private Rectangle _exitBounds;
    private KeyboardState _previousKeyboard;
    private GamePadState _previousGamePad;
    private int _currentStageIndex;
    private int _selectedStageIndex;
    private int _pauseSelectionIndex;
    private int _paletteIndex;
    private int _runStartStageIndex;
    private int _clearRank;
    private int _deaths;
    private bool _cleared;
    private bool _stageSelectOpen;
    private bool _paused;
    private double _titleRefreshTimer;
    private double _clearAnimationTime;
    private double _stageSelectAnimationTime;
    private double _pauseAnimationTime;
    private float _invincibleTimeRemaining;
    private Color _backgroundColor;

    private GamePalette CurrentPalette => _palettes[_paletteIndex];

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
        _runStartStageIndex = 0;
        StartStatsRun(false);
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
            _runElapsedSeconds += elapsed;
            _currentStageElapsedSeconds += elapsed;
            _stageElapsedSeconds[_currentStageIndex] += elapsed;
            _invincibleTimeRemaining = Math.Max(0f, _invincibleTimeRemaining - elapsed);
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
            DrawRectangle(wall, CurrentPalette.WallOuter);
            DrawRectangle(Inset(wall, 5), CurrentPalette.WallInner);
        }

        var exitColor = AreAllGemsCollected() ? CurrentPalette.ExitOpen : CurrentPalette.ExitClosed;
        DrawRectangle(GetExitBounds(), exitColor);
        DrawRectangle(Inset(GetExitBounds(), 12), _backgroundColor);

        for (var i = 0; i < _gemBounds.Length; i++)
        {
            if (_gemsCollected[i])
            {
                continue;
            }

            DrawRectangle(_gemBounds[i], CurrentPalette.Gem);
            DrawRectangle(Inset(_gemBounds[i], 8), CurrentPalette.GemShine);
        }

        foreach (var hazard in _hazards)
        {
            DrawRectangle(hazard.Bounds, CurrentPalette.Hazard);
            DrawRectangle(Inset(hazard.Bounds, 12), CurrentPalette.HazardInner);
        }

        DrawPlayer();
        DrawHud();
    }

    private void DrawGrid()
    {
        var color = CurrentPalette.Grid;
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
        DrawRectangle(new Rectangle(58, 58, Math.Max(120, 40 + _gemBounds.Length * 76), 22), CurrentPalette.HudBackground);
        for (var i = 0; i < _gemBounds.Length; i++)
        {
            var color = _gemsCollected[i] ? CurrentPalette.Gem : CurrentPalette.HudInactive;
            DrawRectangle(new Rectangle(70 + i * 76, 52, 42, 42), color);
        }

        for (var i = 0; i < Math.Min(_deaths, 8); i++)
        {
            DrawRectangle(new Rectangle(70 + i * 38, 112, 24, 24), CurrentPalette.Hazard);
        }

        var stageIndicatorStartX = VirtualWidth - 58 - (_stages.Length * 54 - 16);
        for (var i = 0; i < _stages.Length; i++)
        {
            var color = i == _currentStageIndex ? CurrentPalette.StageCurrent : CurrentPalette.HudInactive;
            DrawRectangle(new Rectangle(stageIndicatorStartX + i * 54, 58, 38, 38), color);
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
        var panel = new Rectangle(360, 320, 1200, 420);
        DrawRectangle(panel, new Color(18, 24, 34, 238));
        DrawFrame(panel, new Color(245, 198, 80), 16);
        DrawFrame(Inset(panel, 38), new Color(81, 161, 255, 150), 8);

        for (var i = 0; i < 4; i++)
        {
            DrawPauseOption(i, i == _pauseSelectionIndex, pulse);
        }
    }

    private void DrawPauseOption(int optionIndex, bool selected, float pulse)
    {
        var width = selected ? 190 + (int)(pulse * 12f) : 160;
        var height = selected ? 190 + (int)(pulse * 12f) : 160;
        var centerX = 600 + optionIndex * 240;
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
        else if (optionIndex == 2)
        {
            for (var i = 0; i < 3; i++)
            {
                DrawRectangle(new Rectangle(card.X + 40 + i * 38, card.Y + 48, 28, 28), i == _selectedStageIndex ? CurrentPalette.Gem : CurrentPalette.StageCurrent);
            }

            DrawFrame(new Rectangle(card.X + 40, card.Y + 100, card.Width - 80, 42), CurrentPalette.ExitOpen, 8);
        }
        else
        {
            DrawPaletteSwatches(new Rectangle(card.X + 35, card.Y + 42, card.Width - 70, card.Height - 84));
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
        var centerOffset = stageIndex - (_stages.Length - 1) / 2f;
        var centerX = VirtualWidth / 2 + (int)(centerOffset * gap);
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

    private void DrawPaletteSwatches(Rectangle bounds)
    {
        var swatchWidth = bounds.Width / 2 - 6;
        var swatchHeight = bounds.Height / 3 - 6;
        DrawRectangle(new Rectangle(bounds.X, bounds.Y, swatchWidth, swatchHeight), CurrentPalette.Player);
        DrawRectangle(new Rectangle(bounds.X + swatchWidth + 12, bounds.Y, swatchWidth, swatchHeight), CurrentPalette.Gem);
        DrawRectangle(new Rectangle(bounds.X, bounds.Y + swatchHeight + 9, swatchWidth, swatchHeight), CurrentPalette.Hazard);
        DrawRectangle(new Rectangle(bounds.X + swatchWidth + 12, bounds.Y + swatchHeight + 9, swatchWidth, swatchHeight), CurrentPalette.ExitOpen);
        DrawRectangle(new Rectangle(bounds.X, bounds.Y + (swatchHeight + 9) * 2, bounds.Width, swatchHeight), CurrentPalette.WallInner);
        DrawFrame(bounds, CurrentPalette.GemShine, 4);
    }

    private void CyclePalette()
    {
        _paletteIndex = (_paletteIndex + 1) % _palettes.Length;
        _backgroundColor = CurrentPalette.Background;
        RefreshWindowTitle();
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
        var rankBody = GetClearRankBodyColor();
        var rankShine = GetClearRankShineColor();
        var glow = Color.Lerp(rankBody, rankShine, pulse);

        for (var i = 0; i < 24; i++)
        {
            var angle = (float)(i * Math.PI * 2d / 24d + _clearAnimationTime * 0.42d);
            var length = 340 + (i % 4) * 80 + (int)(pulse * 35f);
            DrawLine(center, center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * length, 10, new Color(74, 205, 116, 95));
        }

        DrawConfetti();
        DrawFrame(new Rectangle(540, 300, 840, 480), rankBody, 18);
        DrawFrame(new Rectangle(590, 350, 740, 380), glow, 12);
        DrawRectangle(new Rectangle(630, 390, 660, 300), new Color(20, 26, 28, 230));

        DrawOrbitingGems(center);

        var mainBob = (float)Math.Sin(_clearAnimationTime * 3.8d) * 16f;
        var sideBob = (float)Math.Sin(_clearAnimationTime * 4.6d + Math.PI) * 10f;
        DrawGem(new Vector2(960, 505 + mainBob), 170 + (int)(pulse * 12f), rankBody, rankShine);
        DrawGem(new Vector2(760, 545 + sideBob), 86, new Color(81, 161, 255), new Color(197, 228, 255));
        DrawGem(new Vector2(1160, 545 - sideBob), 86, new Color(221, 72, 92), new Color(255, 148, 157));

        for (var i = 0; i < _stages.Length; i++)
        {
            var checkPulse = i == _currentStageIndex ? (int)(pulse * 8f) : 0;
            DrawRectangle(new Rectangle(828 + i * 78 - checkPulse / 2, 650 - checkPulse / 2, 54 + checkPulse, 54 + checkPulse), new Color(74, 205, 116));
            DrawRectangle(new Rectangle(842 + i * 78, 664, 26, 26), rankShine);
        }

        DrawClearRankMarks(rankBody, rankShine, pulse);
    }

    private void DrawClearRankMarks(Color body, Color shine, float pulse)
    {
        var markCount = _clearRank + 1;
        var startX = 960 - (markCount - 1) * 48;
        for (var i = 0; i < markCount; i++)
        {
            var size = 48 + (int)(pulse * 8f);
            DrawGem(new Vector2(startX + i * 96, 345 - pulse * 8f), size, body, shine);
        }
    }

    private Color GetClearRankBodyColor() => _clearRank switch
    {
        2 => new Color(245, 198, 80),
        1 => new Color(184, 205, 224),
        _ => new Color(182, 116, 66),
    };

    private Color GetClearRankShineColor() => _clearRank switch
    {
        2 => new Color(255, 239, 151),
        1 => new Color(236, 247, 255),
        _ => new Color(238, 171, 106),
    };

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
            _pauseSelectionIndex = (_pauseSelectionIndex + 3) % 4;
        }

        if (WasPressed(keyboard, Keys.Right) || WasPressed(keyboard, Keys.D) || WasPressed(gamePad.DPad.Right, _previousGamePad.DPad.Right) || WasThumbstickPressedRight(gamePad))
        {
            _pauseSelectionIndex = (_pauseSelectionIndex + 1) % 4;
        }

        if (WasPressed(keyboard, Keys.Enter)
            || WasPressed(keyboard, Keys.Space)
            || WasPressed(gamePad.Buttons.Start, _previousGamePad.Buttons.Start)
            || WasPressed(gamePad.Buttons.A, _previousGamePad.Buttons.A))
        {
            SelectPauseOption();
        }
    }

    private void OpenPauseMenu()
    {
        _pauseCount++;
        LogPlayEvent(PlayEventKind.Pause, _currentStageIndex, GetPlayerBounds().Center.ToVector2(), _currentStageElapsedSeconds, _pauseCount);
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
            _retryCount++;
            LogPlayEvent(PlayEventKind.Retry, _currentStageIndex, GetPlayerBounds().Center.ToVector2(), _currentStageElapsedSeconds, _retryCount);
            _paused = false;
            _pauseAnimationTime = 0d;
            LoadStage(_currentStageIndex);
            return;
        }

        if (_pauseSelectionIndex == 2)
        {
            _paused = false;
            _pauseAnimationTime = 0d;
            OpenStageSelect();
            return;
        }

        CyclePalette();
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
        LogPlayEvent(PlayEventKind.StageSelect, _currentStageIndex, GetPlayerBounds().Center.ToVector2(), _currentStageElapsedSeconds, _selectedStageIndex);
        _paused = false;
        _stageSelectOpen = true;
        _selectedStageIndex = _currentStageIndex;
        _stageSelectAnimationTime = 0d;
        RefreshWindowTitle();
    }

    private void StartSelectedStage()
    {
        _deaths = 0;
        _runStartStageIndex = _selectedStageIndex;
        StartStatsRun(_selectedStageIndex != 0);
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

    private void DrawPlayer()
    {
        if (_invincibleTimeRemaining > 0f && (int)(_invincibleTimeRemaining * 16f) % 2 == 0)
        {
            DrawRectangle(GetPlayerBounds(), CurrentPalette.PlayerInvincible);
            DrawRectangle(Inset(GetPlayerBounds(), 12), CurrentPalette.Player);
            return;
        }

        DrawRectangle(GetPlayerBounds(), _cleared ? CurrentPalette.ExitOpen : CurrentPalette.Player);
        DrawRectangle(Inset(GetPlayerBounds(), 10), CurrentPalette.PlayerInner);
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
                _stageGemCounts[_currentStageIndex]++;
                LogPlayEvent(PlayEventKind.Gem, _currentStageIndex, _gemBounds[i].Center.ToVector2(), _currentStageElapsedSeconds, i);
            }
        }
    }

    private void CheckHazardCollision()
    {
        if (_invincibleTimeRemaining > 0f)
        {
            return;
        }

        var player = GetPlayerBounds();
        foreach (var hazard in _hazards)
        {
            if (player.Intersects(hazard.Bounds))
            {
                _deaths++;
                _stageDeathCounts[_currentStageIndex]++;
                LogPlayEvent(PlayEventKind.Death, _currentStageIndex, player.Center.ToVector2(), _currentStageElapsedSeconds, _deaths);
                ResetPlayerOnly(true);
                return;
            }
        }
    }

    private void CheckExit()
    {
        if (AreAllGemsCollected() && GetPlayerBounds().Intersects(GetExitBounds()))
        {
            LogPlayEvent(PlayEventKind.Clear, _currentStageIndex, GetPlayerBounds().Center.ToVector2(), _currentStageElapsedSeconds, _stageGemCounts[_currentStageIndex]);

            if (_currentStageIndex + 1 < _stages.Length)
            {
                LoadStage(_currentStageIndex + 1);
                return;
            }

            LogPlayEvent(PlayEventKind.FullClear, _currentStageIndex, GetPlayerBounds().Center.ToVector2(), _runElapsedSeconds, _deaths);
            _clearRank = CalculateClearRank();
            _cleared = true;
            _clearAnimationTime = 0d;
        }
    }

    private void StartStatsRun(bool startedFromStageSelect)
    {
        _playEvents.Clear();
        _stageDeathCounts = new int[_stages.Length];
        _stageGemCounts = new int[_stages.Length];
        _stageElapsedSeconds = new double[_stages.Length];
        _runElapsedSeconds = 0d;
        _currentStageElapsedSeconds = 0d;
        _pauseCount = 0;
        _retryCount = 0;
        _startedFromStageSelect = startedFromStageSelect;
    }

    private void LogPlayEvent(PlayEventKind kind, int stageIndex, Vector2 position, double stageElapsedSeconds, int detail)
    {
        if (_stageElapsedSeconds.Length == 0)
        {
            return;
        }

        _playEvents.Add(new PlayEvent(kind, stageIndex, _runElapsedSeconds, stageElapsedSeconds, position, detail));
    }

    private string GetStatsSummary()
    {
        if (_stageDeathCounts.Length == 0)
        {
            return "Stats none";
        }

        var route = _startedFromStageSelect ? "practice" : "run";
        return $"Stats {route} T{_runElapsedSeconds:0.0}s D[{string.Join('/', _stageDeathCounts)}] G[{string.Join('/', _stageGemCounts)}] P{_pauseCount} R{_retryCount} E{_playEvents.Count}";
    }

    private int CalculateClearRank()
    {
        var startedFromFirstStage = _runStartStageIndex == 0;
        if (startedFromFirstStage && _deaths == 0)
        {
            return 2;
        }

        if (_deaths <= (startedFromFirstStage ? 3 : 0))
        {
            return 1;
        }

        return 0;
    }

    private void ResetRun()
    {
        _deaths = 0;
        _runStartStageIndex = 0;
        StartStatsRun(false);
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
        _backgroundColor = CurrentPalette.Background;
        _stageSelectOpen = false;
        _paused = false;
        _cleared = false;
        _clearRank = 0;
        _clearAnimationTime = 0d;
        _currentStageElapsedSeconds = 0d;
        ResetPlayerOnly(false);
        LogPlayEvent(PlayEventKind.StageStart, _currentStageIndex, GetPlayerBounds().Center.ToVector2(), 0d, _stageGemCounts[_currentStageIndex]);
        RefreshWindowTitle();
    }

    private void ResetPlayerOnly(bool grantInvincibility)
    {
        _playerPosition = _playerStart;
        _invincibleTimeRemaining = grantInvincibility ? RespawnInvincibleSeconds : 0f;
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

        var state = _stageSelectOpen ? "STAGE SELECT - Left/Right choose - Enter/Space/Start play" : _paused ? "PAUSE - Left/Right choose - Enter/Space/Start/A select" : _cleared ? "CLEAR - Press R / Start to retry - Tab for stage select" : "Collect all gems and reach the green exit - Start/Enter pause - Tab for stage select";
        var stage = _stages[_currentStageIndex];
        Window.Title = $"SkylarkBimbleStreet - {stage.Name} - Palette {CurrentPalette.Name} - {state} - Gems {collected}/{_gemBounds.Length} - Hits {_deaths} - {GetStatsSummary()}";
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

    private enum PlayEventKind
    {
        StageStart,
        Gem,
        Death,
        Clear,
        FullClear,
        Pause,
        Retry,
        StageSelect,
    }

    private readonly record struct PlayEvent(
        PlayEventKind Kind,
        int StageIndex,
        double RunElapsedSeconds,
        double StageElapsedSeconds,
        Vector2 Position,
        int Detail);
    private readonly record struct GamePalette(
        string Name,
        Color Background,
        Color Grid,
        Color WallOuter,
        Color WallInner,
        Color Player,
        Color PlayerInner,
        Color PlayerInvincible,
        Color Gem,
        Color GemShine,
        Color Hazard,
        Color HazardInner,
        Color ExitOpen,
        Color ExitClosed,
        Color HudBackground,
        Color HudInactive,
        Color StageCurrent);
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
