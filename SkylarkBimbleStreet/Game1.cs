namespace SkylarkBimbleStreet;

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class Game1 : Game
{
    private const int VirtualWidth = 1920;
    private const int VirtualHeight = 1080;
    private const float PlayerSpeed = 520f;
    private const float JetPlayerSpeedMultiplier = 1.55f;
    private const float RollerSlideMultiplier = 0.82f;
    private const float RollerCornerTurnMultiplier = 0.92f;
    private const float BasicWallFollowCornerTurnMultiplier = 0.92f;
    private const int RollerWallProbeDistance = 4;
    private const int WallContactProbeDistance = 28;
    private const int PlayerSize = 46;
    private const int SmallPlayerSize = 30;
    private const float SmallPlayerSpeedMultiplier = 0.5f;
    private const int GemShardPixelSize = 17;
    private const float RespawnInvincibleSeconds = 0.8f;
    private const float StageStartNormalPulseSeconds = 0.58f;
    private const float ExitOpenDelaySeconds = 0.32f;
    private const float ExitOpenFlashSeconds = 0.55f;
    private const float GemCollectEffectSeconds = 0.34f;
    private const float GemBagFullNudgeSeconds = 0.22f;
    private const float GemBagFullSoundCooldownSeconds = 0.36f;
    private const float DeathEffectSeconds = 3.0f;
    private const float BusStopWaitSeconds = 2.0f;
    private const float BusPassageSeconds = 1.6f;
    private const float BusArrivalSeconds = 1.35f;
    private const float BadgeAwardEffectSeconds = 2.4f;
    private const float StageSelectFocusStartOffset = 12f;
    private const float StageSelectFocusExpandFastRate = 22f;
    private const float StageSelectFocusExpandSlowRate = 8f;
    private const float StageSelectFocusShrinkRate = 12f;
    private const float StageSelectFocusMoveShrinkRate = StageSelectFocusExpandFastRate * 2f;
    private const float StageSelectQuickMoveSeconds = 0.22f;
    private const float StageSelectCardGap = 430f;
    private const float StageSelectChainMoveOffset = StageSelectCardGap * 0.58f;
    private const float StageSelectSlideReturnRate = 3.0f;
    private const float StageSelectMoveRepeatSeconds = 0.28f;
    private const int HudBandHeight = 150;
    private const int HudPadding = 42;
    private const int PauseOptionCount = 5;
    private const int SoundTestEntryOption = 4;
    private const int StageMoveSoundVariantCount = 3;
    private const int SoundTestStationCount = 12;
    private const int SoundTestTerminalIndex = SoundTestStationCount;

    private readonly GraphicsDeviceManager _graphics;
    private readonly List<PlayEvent> _playEvents = [];
    private readonly List<GemCollectEffect> _gemCollectEffects = [];
    private readonly List<DeathEffect> _deathEffects = [];
    private readonly List<DeathStopMark> _deathStopLines = [];
    private readonly List<BadgeAwardEffect> _badgeAwardEffects = [];
    private SpriteBatch _spriteBatch = null!;
    private Texture2D _pixel = null!;
    private RenderTarget2D _scene = null!;
    private SoundEffect _gemSound = null!;
    private SoundEffect _gemBagFullSound = null!;
    private SoundEffect _deathSound = null!;
    private SoundEffect _clearSound = null!;
    private SoundEffect _exitOpenSound = null!;
    private SoundEffect _stageMoveSound = null!;
    private SoundEffect[] _stageMoveSounds = [];
    private SoundEffect[] _soundTestSounds = [];
    private SoundEffect _confirmSound = null!;
    private SoundEffect _pauseSound = null!;
    private SoundEffect _ambulanceSirenSound = null!;
    private SoundEffect _ambulanceDoorSound = null!;
    private SoundEffect _ambulanceBrakeSound = null!;

    private readonly Stage[] _stages = StageDefinitions.CreateStages();

    private readonly GamePalette[] _palettes =
    [
        new("Normal", new Color(18, 22, 31), new Color(31, 37, 50), new Color(76, 84, 103), new Color(104, 116, 140), new Color(81, 161, 255), new Color(197, 228, 255), new Color(255, 239, 151), new Color(245, 198, 80), new Color(255, 239, 151), new Color(221, 72, 92), new Color(255, 148, 157), new Color(74, 205, 116), new Color(62, 78, 72), new Color(44, 51, 67), new Color(69, 75, 90), new Color(81, 161, 255)),
        new("Accessible", new Color(16, 18, 22), new Color(42, 45, 52), new Color(92, 99, 110), new Color(134, 144, 158), new Color(0, 170, 210), new Color(214, 248, 255), new Color(255, 255, 255), new Color(245, 210, 65), new Color(255, 252, 190), new Color(210, 82, 36), new Color(255, 180, 120), new Color(0, 154, 120), new Color(76, 88, 86), new Color(48, 52, 60), new Color(78, 84, 94), new Color(0, 170, 210)),
        new("High Contrast", new Color(8, 8, 10), new Color(50, 50, 56), new Color(128, 128, 136), new Color(190, 190, 198), new Color(60, 220, 255), new Color(235, 252, 255), new Color(255, 255, 255), new Color(255, 230, 80), new Color(255, 255, 210), new Color(255, 115, 55), new Color(255, 205, 170), new Color(90, 255, 170), new Color(72, 86, 78), new Color(38, 38, 44), new Color(92, 92, 102), new Color(60, 220, 255)),
        new("Mono Check", new Color(18, 18, 18), new Color(42, 42, 42), new Color(88, 88, 88), new Color(132, 132, 132), new Color(210, 210, 210), new Color(246, 246, 246), new Color(255, 255, 255), new Color(176, 176, 176), new Color(232, 232, 232), new Color(108, 108, 108), new Color(190, 190, 190), new Color(236, 236, 236), new Color(70, 70, 70), new Color(46, 46, 46), new Color(92, 92, 92), new Color(214, 214, 214)),
    ];

    private Rectangle[] _walls = [];
    private Rectangle[] _ticketPieceBounds = [];
    private Rectangle[] _gemBounds = [];
    private Rectangle[] _jetBounds = [];
    private Rectangle[] _rollerBounds = [];
    private Rectangle[] _smallBounds = [];
    private Hazard[] _hazards = [];
    private bool[] _ticketPiecesCollected = [];
    private bool[] _gemsCollected = [];
    private bool[] _jetsCollected = [];
    private bool[] _rollersCollected = [];
    private bool[] _smallsCollected = [];
    private bool _jetActive;
    private bool _rollerActive;
    private Vector2 _rollerContactDirection;
    private Vector2 _rollerSlideDirection;
    private int _rollerWallFollowTurnDirection = 1;
    private bool _smallActive;
    private bool[] _stagesCleared = [];
    private bool[] _stagePassRecords = [];
    private bool[] _stageNoDamageRecords = [];
    private int[] _stageDeathCounts = [];
    private int[] _stageGemCounts = [];
    private int[] _stageBestGemCounts = [];
    private double[] _stageElapsedSeconds = [];
    private double _runElapsedSeconds;
    private double _currentStageElapsedSeconds;
    private int _pauseCount;
    private int _retryCount;
    private bool _startedFromStageSelect;

    private Vector2 _playerPosition;
    private Vector2 _playerStart;
    private Rectangle _exitBounds;
    private Rectangle _busStopBounds;
    private Rectangle _hospitalBounds;
    private KeyboardState _previousKeyboard;
    private GamePadState _previousGamePad;
    private int _currentStageIndex;
    private int _selectedStageIndex;
    private int _pauseSelectionIndex;
    private int _paletteIndex;
    private int _stageMoveSoundIndex = 2;
    private int _soundTestSelectionIndex;
    private int _runStartStageIndex;
    private int _clearRank;
    private int _deaths;
    private bool _cleared;
    private bool _exitOpen;
    private bool _stageSelectOpen;
    private bool _paused;
    private bool _deathRespawnPending;
    private bool _playerInAmbulance;
    private bool _playerInBus;
    private bool _busPassagePending;
    private bool _busArrivalPending;
    private bool _deathRespawnCompleted;
    private bool _ambulancePickupDoorPlayed;
    private bool _ambulanceDropoffDoorPlayed;
    private bool _ambulancePickupBrakePlayed;
    private bool _ambulanceDropoffBrakePlayed;
    private bool _soundTestOpen;
    private double _titleRefreshTimer;
    private double _clearAnimationTime;
    private double _stageSelectAnimationTime;
    private double _pauseAnimationTime;
    private float _stageSelectSlideOffset;
    private float _stageSelectSlideDelay;
    private float _stageSelectFocusAmount = 1f;
    private int _stageSelectPendingMoveDirection;
    private int _stageSelectQueuedMoveDirection;
    private int _stageSelectHeldMoveDirection;
    private float _stageSelectQuickMoveTimeRemaining;
    private float _stageSelectMoveRepeatTimeRemaining;
    private float _invincibleTimeRemaining;
    private float _stageStartNormalPulseRemaining;
    private float _deathRespawnTimeRemaining;
    private float _exitOpenDelayRemaining;
    private float _exitOpenFlashRemaining;
    private float _busStopWaitProgress;
    private float _gemBagFullNudgeRemaining;
    private float _gemBagFullSoundCooldownRemaining;
    private float _busPassageTimeRemaining;
    private float _busArrivalTimeRemaining;
    private Vector2 _lastMoveDirection = Vector2.UnitX;
    private Vector2 _playerFacingDirection = Vector2.UnitX;
    private Vector2 _playerInputDirection;
    private Vector2 _playerGhostVelocity;
    private Vector2 _lastWallParallelContactDirection;
    private Vector2 _lastWallParallelMoveDirection;
    private Vector2 _basicWallFollowContactDirection;
    private Vector2 _basicWallFollowSlideDirection;
    private int _basicWallFollowTurnDirection = 1;
    private bool _wallFollowMovedThisFrame;
    private WallContact _wallFollowWallContact = WallContact.None;
    private WallContact _wallFollowHitContact = WallContact.None;
    private int _inputContactWallIndex = -1;
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
        _stagesCleared = new bool[_stages.Length];
        _stagePassRecords = new bool[_stages.Length];
        _stageNoDamageRecords = new bool[_stages.Length];
        _stageBestGemCounts = new int[_stages.Length];
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
        CreateSoundEffects();
    }

    protected override void UnloadContent()
    {
        DisposeSoundEffects();
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
            UpdateStageSelect(keyboard, gamePad, elapsed);
            UpdateStageSelectSlide(elapsed);
            UpdateBadgeAwardEffects(elapsed);
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

        if (_cleared)
        {
            if (WasPressed(keyboard, Keys.Tab)
                || WasPressed(keyboard, Keys.Enter)
                || WasPressed(keyboard, Keys.Space)
                || WasPressed(gamePad.Buttons.Start, _previousGamePad.Buttons.Start)
                || WasPressed(gamePad.Buttons.A, _previousGamePad.Buttons.A))
            {
                OpenStageSelect();
            }
            else if (WasPressed(keyboard, Keys.R))
            {
                ResetRun();
            }
        }
        else if (WasPressed(keyboard, Keys.Enter) || WasPressed(gamePad.Buttons.Start, _previousGamePad.Buttons.Start))
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
            _stageStartNormalPulseRemaining = Math.Max(0f, _stageStartNormalPulseRemaining - elapsed);
            _exitOpenFlashRemaining = Math.Max(0f, _exitOpenFlashRemaining - elapsed);
            _gemBagFullNudgeRemaining = Math.Max(0f, _gemBagFullNudgeRemaining - elapsed);
            _gemBagFullSoundCooldownRemaining = Math.Max(0f, _gemBagFullSoundCooldownRemaining - elapsed);
            UpdateGemCollectEffects(elapsed);
            UpdateBadgeAwardEffects(elapsed);
            UpdateDeathEffects(elapsed);
            UpdateDeathRespawn(elapsed);
            UpdateBusPassage(elapsed);
            UpdateBusArrival(elapsed);
            UpdateExitOpening(elapsed);
            UpdateHazards(elapsed);

            if (!_deathRespawnPending && !_busPassagePending && !_busArrivalPending)
            {
                MovePlayer(GetMoveInput(keyboard, gamePad), elapsed);
                CheckGemCollection();
                CheckHazardCollision();
                if (!_deathRespawnPending)
                {
                    CheckBusStop(elapsed);
                    CheckExit();
                }
            }
        }
        else if (_cleared)
        {
            _clearAnimationTime += elapsed;
            UpdateBadgeAwardEffects(elapsed);
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

        _spriteBatch.Begin(blendState: BlendState.NonPremultiplied, samplerState: SamplerState.PointClamp, transformMatrix: GetPlayfieldTransform());
        DrawPlayfield();
        _spriteBatch.End();

        _spriteBatch.Begin(blendState: BlendState.NonPremultiplied, samplerState: SamplerState.PointClamp);
        DrawHud();
        _spriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(blendState: BlendState.NonPremultiplied, samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_scene, GetDestinationRectangle(), Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawPlayfield()
    {
        DrawRectangle(new Rectangle(0, 0, VirtualWidth, VirtualHeight), _backgroundColor);
        DrawGrid();

        foreach (var wall in _walls)
        {
            DrawRectangle(wall, CurrentPalette.WallOuter);
            DrawRectangle(Inset(wall, 5), CurrentPalette.WallInner);
        }

        DrawWallFollowWallHighlights();
        DrawInputContactWallHighlight();

        DrawExit(GetExitBounds());
        DrawHospital(_hospitalBounds);
        DrawBusStop(_busStopBounds);
        DrawDeathStopLines();

        for (var i = 0; i < _ticketPieceBounds.Length; i++)
        {
            if (_ticketPiecesCollected[i]) continue;

            DrawTicketPiece(_ticketPieceBounds[i], i, CurrentPalette.ExitOpen, CurrentPalette.GemShine);
        }

        for (var i = 0; i < _jetBounds.Length; i++)
        {
            if (_jetsCollected[i]) continue;

            DrawJet(_jetBounds[i]);
        }

        for (var i = 0; i < _rollerBounds.Length; i++)
        {
            if (_rollersCollected[i]) continue;

            DrawRoller(_rollerBounds[i], 0f);
        }

        for (var i = 0; i < _smallBounds.Length; i++)
        {
            if (_smallsCollected[i]) continue;

            DrawSmall(_smallBounds[i]);
        }

        for (var i = 0; i < _gemBounds.Length; i++)
        {
            if (_gemsCollected[i]) continue;

            DrawGem(_gemBounds[i].Center.ToVector2(), _gemBounds[i].Width, CurrentPalette.Gem, CurrentPalette.GemShine);
        }

        DrawGemCollectEffects();

        foreach (var hazard in _hazards)
        {
            DrawHazard(hazard.Bounds);
        }

        DrawPlayerFacingLight();
        DrawPlayerGhost();
        DrawPlayer();
        DrawDeathEffects();
        DrawBusPassage();
        DrawBusArrival();
        DrawExitOpenRays(GetExitBounds());
    }


    private void DrawHospital(Rectangle bounds)
    {
        var outline = CurrentPalette.WallInner;
        var detail = CurrentPalette.PlayerInner;
        var soft = WithAlpha(CurrentPalette.GemShine, 90);

        DrawFrame(bounds, outline, 5);
        DrawFrame(Inset(bounds, 9), soft, 3);

        var roof = new Rectangle(bounds.X + 10, bounds.Y + 8, bounds.Width - 20, 24);
        DrawFrame(roof, outline, 3);
        DrawLine(new Vector2(roof.X + 10, roof.Center.Y), new Vector2(roof.Right - 10, roof.Center.Y), 4, soft);

        for (var row = 0; row < 2; row++)
        {
            for (var column = 0; column < 4; column++)
            {
                var window = new Rectangle(bounds.X + 20 + column * 34, bounds.Y + 42 + row * 28, 22, 16);
                DrawFrame(window, detail, 2);
                DrawLine(new Vector2(window.X + 4, window.Bottom - 4), new Vector2(window.Right - 4, window.Y + 4), 2, soft);
            }
        }

        var bay = new Rectangle(bounds.Center.X - 48, bounds.Bottom - 44, 96, 38);
        DrawFrame(bay, outline, 4);
        DrawLine(new Vector2(bay.X + 10, bay.Y + 12), new Vector2(bay.Right - 10, bay.Y + 12), 4, soft);
        DrawRectangle(new Rectangle(bay.X + 12, bay.Bottom - 9, 18, 10), detail);
        DrawRectangle(new Rectangle(bay.Right - 30, bay.Bottom - 9, 18, 10), detail);

        var driveway = new Rectangle(bounds.Center.X - 58, bounds.Bottom - 6, 116, 10);
        DrawFrame(driveway, outline, 2);
    }

    private void DrawBusStop(Rectangle bounds)
    {
        var playerWaiting = !_busPassagePending && GetPlayerBounds().Intersects(bounds);
        var accent = playerWaiting ? CurrentPalette.GemShine : CurrentPalette.WallInner;
        var poleX = bounds.X + 54;
        var circleCenter = new Vector2(poleX, bounds.Y + 32);

        DrawLine(new Vector2(poleX, bounds.Y + 56), new Vector2(poleX, bounds.Bottom - 18), 10, CurrentPalette.WallInner);
        DrawRectangle(new Rectangle(bounds.X + 20, bounds.Bottom - 22, bounds.Width - 40, 8), accent);

        DrawGem(circleCenter, 62, CurrentPalette.HudBackground, accent);
        var busIcon = new Rectangle((int)circleCenter.X - 25, (int)circleCenter.Y - 12, 50, 24);
        DrawRectangle(busIcon, CurrentPalette.PlayerInner);
        DrawFrame(busIcon, accent, 3);
        DrawRectangle(new Rectangle(busIcon.X + 7, busIcon.Y + 6, 13, 8), CurrentPalette.GemShine);
        DrawRectangle(new Rectangle(busIcon.X + 24, busIcon.Y + 6, 13, 8), CurrentPalette.GemShine);
        DrawRectangle(new Rectangle(busIcon.Right - 10, busIcon.Y + 6, 6, 12), CurrentPalette.WallInner);
        DrawRectangle(new Rectangle(busIcon.X + 8, busIcon.Bottom - 2, 8, 8), CurrentPalette.WallInner);
        DrawRectangle(new Rectangle(busIcon.Right - 16, busIcon.Bottom - 2, 8, 8), CurrentPalette.WallInner);

        var board = new Rectangle(bounds.X + 86, bounds.Y + 24, bounds.Width - 104, 76);
        DrawRectangle(board, CurrentPalette.HudBackground);
        DrawFrame(board, accent, 4);
        for (var i = 0; i < 4; i++)
        {
            DrawRectangle(new Rectangle(board.X + 12, board.Y + 12 + i * 14, board.Width - 24, 5), i == 0 ? CurrentPalette.GemShine : CurrentPalette.PlayerInner);
        }

        if (_busStopWaitProgress <= 0f || _busPassagePending) return;

        var progress = MathHelper.Clamp(_busStopWaitProgress / BusStopWaitSeconds, 0f, 1f);
        var bar = new Rectangle(bounds.X, bounds.Y - 18, bounds.Width, 10);
        DrawRectangle(bar, CurrentPalette.HudBackground);
        DrawRectangle(new Rectangle(bar.X, bar.Y, (int)(bar.Width * progress), bar.Height), CurrentPalette.GemShine);
        DrawFrame(bar, accent, 2);
    }
    private void DrawExit(Rectangle bounds)
    {
        var open = _exitOpen;
        var body = open ? CurrentPalette.ExitOpen : CurrentPalette.ExitClosed;
        var detail = open ? CurrentPalette.GemShine : CurrentPalette.WallInner;

        DrawFrame(bounds, body, 14);
        DrawFrame(Inset(bounds, 20), detail, 8);
        DrawRectangle(Inset(bounds, 34), _backgroundColor);

        var gate = Inset(bounds, 24);
        if (open)
        {
            DrawRectangle(new Rectangle(gate.X + 8, gate.Y + 8, 8, gate.Height - 16), detail);
            DrawRectangle(new Rectangle(gate.Right - 16, gate.Y + 8, 8, gate.Height - 16), detail);
            DrawExitOpenFlash(bounds);
            return;
        }

        DrawRectangle(new Rectangle(gate.X + gate.Width / 3 - 4, gate.Y + 8, 8, gate.Height - 16), detail);
        DrawRectangle(new Rectangle(gate.X + gate.Width * 2 / 3 - 4, gate.Y + 8, 8, gate.Height - 16), detail);
        DrawExitOpenFlash(bounds);
    }


    private void DrawExitOpenFlash(Rectangle bounds)
    {
        if (_exitOpenFlashRemaining <= 0f) return;

        var progress = 1f - _exitOpenFlashRemaining / ExitOpenFlashSeconds;
        var fade = 1f - progress;
        var pulse = (float)Math.Sin(progress * Math.PI);
        for (var i = 0; i < 5; i++)
        {
            var spread = 10 + i * 14 + (int)(progress * (24f + i * 9f));
            var alpha = (byte)((115f - i * 16f) * fade * (0.65f + 0.35f * pulse));
            var color = i % 2 == 0 ? CurrentPalette.GemShine : CurrentPalette.ExitOpen;
            DrawFrame(new Rectangle(bounds.X - spread, bounds.Y - spread, bounds.Width + spread * 2, bounds.Height + spread * 2), WithAlpha(color, alpha), 3);
        }

        DrawFrame(new Rectangle(bounds.X - 6, bounds.Y - 6, bounds.Width + 12, bounds.Height + 12), WithAlpha(CurrentPalette.GemShine, (byte)(150f * fade)), 4);
    }


    private void DrawExitOpenRays(Rectangle bounds)
    {
        if (_exitOpenFlashRemaining <= 0f) return;

        var progress = 1f - _exitOpenFlashRemaining / ExitOpenFlashSeconds;
        var fade = 1f - progress;
        var center = bounds.Center.ToVector2();
        for (var i = 0; i < 16; i++)
        {
            var angle = (float)(i * Math.PI * 2d / 16d + progress * 0.28f);
            var direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            var start = center + direction * (92f + progress * 38f);
            var end = center + direction * (220f + progress * 150f);
            DrawLine(start, end, Math.Max(3, (int)(9f * fade)), WithAlpha(CurrentPalette.GemShine, (byte)(220f * fade)));
        }

        for (var i = 0; i < 8; i++)
        {
            var angle = (float)(i * Math.PI * 2d / 8d - progress * 0.55f);
            var direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            var position = center + direction * (150f + progress * 120f);
            var size = Math.Max(5, (int)(14f * fade));
            DrawRectangle(new Rectangle((int)position.X - size / 2, (int)position.Y - size / 2, size, size), WithAlpha(CurrentPalette.ExitOpen, (byte)(210f * fade)));
        }
    }

    private void DrawHazard(Rectangle bounds)
    {
        DrawRectangle(bounds, CurrentPalette.Hazard);
        DrawFrame(bounds, CurrentPalette.PlayerInvincible, 5);
        DrawRectangle(Inset(bounds, 12), CurrentPalette.HazardInner);

        var stripe = Math.Max(5, bounds.Height / 10);
        DrawRectangle(new Rectangle(bounds.X + 12, bounds.Y + 14, bounds.Width - 24, stripe), CurrentPalette.PlayerInvincible);
        DrawRectangle(new Rectangle(bounds.X + 12, bounds.Center.Y - stripe / 2, bounds.Width - 24, stripe), CurrentPalette.PlayerInvincible);
        DrawRectangle(new Rectangle(bounds.X + 12, bounds.Bottom - 14 - stripe, bounds.Width - 24, stripe), CurrentPalette.PlayerInvincible);
    }

    private void DrawMissMark(Rectangle bounds)
    {
        DrawFrame(bounds, CurrentPalette.Hazard, 4);
        DrawLine(new Vector2(bounds.X + 4, bounds.Y + 4), new Vector2(bounds.Right - 4, bounds.Bottom - 4), 5, CurrentPalette.HazardInner);
        DrawLine(new Vector2(bounds.Right - 4, bounds.Y + 4), new Vector2(bounds.X + 4, bounds.Bottom - 4), 5, CurrentPalette.HazardInner);
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
        DrawRectangle(new Rectangle(0, 0, VirtualWidth, HudBandHeight), WithAlpha(CurrentPalette.HudBackground, 226));
        DrawRectangle(new Rectangle(0, HudBandHeight - 5, VirtualWidth, 5), WithAlpha(CurrentPalette.WallInner, 190));

        const int sectionY = 32;
        const int sectionHeight = 82;
        const int sectionGap = 28;
        var ticketHudWidth = Math.Max(148, 42 + _ticketPieceBounds.Length * 58);
        var ticketSection = new Rectangle(HudPadding, sectionY, ticketHudWidth, sectionHeight);
        DrawRectangle(ticketSection, WithAlpha(CurrentPalette.HudBackground, 210));
        DrawFrame(ticketSection, WithAlpha(CurrentPalette.WallInner, 180), 3);
        for (var i = 0; i < _ticketPieceBounds.Length; i++)
        {
            var body = _ticketPiecesCollected[i] ? CurrentPalette.ExitOpen : CurrentPalette.HudInactive;
            var detail = _ticketPiecesCollected[i] ? CurrentPalette.GemShine : CurrentPalette.WallInner;
            DrawTicketPiece(new Rectangle(ticketSection.X + 22 + i * 58, ticketSection.Y + 22, 34, 38), i, body, detail);
        }

        var gemBagSection = new Rectangle(ticketSection.Right + sectionGap, sectionY, 250, sectionHeight);
        DrawRectangle(gemBagSection, WithAlpha(CurrentPalette.HudBackground, 210));
        DrawFrame(gemBagSection, WithAlpha(CurrentPalette.WallInner, 180), 3);
        DrawGemBag(new Rectangle(gemBagSection.X + 26, gemBagSection.Y + 17, 198, 48), CountCollectedGemShards(), GetCurrentGemBagCapacity(), true);

        const int stageIndicatorSize = 34;
        const int stageIndicatorGap = 14;
        var stageIndicatorWidth = _stages.Length * (stageIndicatorSize + stageIndicatorGap) - stageIndicatorGap;
        var stageSection = new Rectangle(VirtualWidth - HudPadding - stageIndicatorWidth - 42, sectionY, stageIndicatorWidth + 42, sectionHeight);
        DrawRectangle(stageSection, WithAlpha(CurrentPalette.HudBackground, 210));
        DrawFrame(stageSection, WithAlpha(CurrentPalette.WallInner, 180), 3);
        var stageIndicatorStartX = stageSection.X + 21;
        for (var i = 0; i < _stages.Length; i++)
        {
            var color = i == _currentStageIndex ? CurrentPalette.StageCurrent : CurrentPalette.HudInactive;
            DrawRectangle(new Rectangle(stageIndicatorStartX + i * (stageIndicatorSize + stageIndicatorGap), stageSection.Y + 24, stageIndicatorSize, stageIndicatorSize), color);
        }

        var missSectionX = gemBagSection.Right + sectionGap;
        var missSectionWidth = Math.Max(118, stageSection.X - missSectionX - sectionGap);
        var missSection = new Rectangle(missSectionX, sectionY, missSectionWidth, sectionHeight);
        DrawRectangle(missSection, WithAlpha(CurrentPalette.HudBackground, 170));
        DrawFrame(missSection, WithAlpha(CurrentPalette.WallInner, 140), 3);
        var maxVisibleMisses = Math.Min(8, Math.Max(0, (missSection.Width - 32) / 38));
        for (var i = 0; i < Math.Min(_deaths, maxVisibleMisses); i++)
        {
            DrawMissMark(new Rectangle(missSection.X + 18 + i * 38, missSection.Y + 29, 24, 24));
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

    private void DrawGemBag(Rectangle bounds, int collectedShards, int capacity, bool showEmpty)
    {
        if (showEmpty && _gemBagFullNudgeRemaining > 0f)
        {
            var progress = 1f - _gemBagFullNudgeRemaining / GemBagFullNudgeSeconds;
            var wobble = (int)(MathF.Sin(progress * MathF.PI * 6f) * (1f - progress) * 8f);
            bounds.Offset(wobble, 0);
        }

        if (capacity <= 0 || (!showEmpty && collectedShards <= 0)) return;

        var clamped = Math.Clamp(collectedShards, 0, capacity);
        var full = clamped >= capacity;
        var body = new Rectangle(bounds.X + 8, bounds.Y + 11, bounds.Width - 16, bounds.Height - 13);
        var neck = new Rectangle(bounds.Center.X - 24, bounds.Y + 3, 48, 12);
        var fillHeight = Math.Max(0, body.Height * clamped / capacity);
        var fill = new Rectangle(body.X + 5, body.Bottom - 5 - fillHeight, body.Width - 10, fillHeight);
        var frame = full ? CurrentPalette.GemShine : CurrentPalette.WallInner;

        DrawRectangle(body, CurrentPalette.HudBackground);
        DrawRectangle(neck, CurrentPalette.HudBackground);
        DrawFrame(body, frame, 4);
        DrawFrame(neck, frame, 3);

        if (fill.Height > 0)
        {
            DrawRectangle(fill, full ? CurrentPalette.GemShine : CurrentPalette.Gem);
            DrawRectangle(new Rectangle(fill.X + 7, fill.Y + 4, Math.Max(8, fill.Width / 4), Math.Max(3, fill.Height / 5)), CurrentPalette.GemShine);
        }

        for (var i = 1; i <= 3; i++)
        {
            var x = body.X + body.Width * i / 4;
            var markColor = clamped * 4 >= capacity * i ? CurrentPalette.GemShine : CurrentPalette.HudInactive;
            DrawRectangle(new Rectangle(x - 2, body.Y + 7, 4, body.Height - 14), markColor);
        }

        if (full)
        {
            DrawGem(new Vector2(bounds.Right - 16, bounds.Y + 12), 22, CurrentPalette.Gem, CurrentPalette.GemShine);
        }
    }
    private void DrawPauseMenu()
    {
        DrawRectangle(new Rectangle(0, 0, VirtualWidth, VirtualHeight), WithAlpha(CurrentPalette.Background, 185));

        if (_soundTestOpen)
        {
            DrawSoundTestMenu();
            return;
        }

        var pulse = (float)((Math.Sin(_pauseAnimationTime * 5d) + 1d) * 0.5d);
        var panel = new Rectangle(360, 320, 1200, 420);
        DrawRectangle(panel, WithAlpha(CurrentPalette.HudBackground, 238));
        DrawFrame(panel, CurrentPalette.Gem, 16);
        DrawFrame(Inset(panel, 38), WithAlpha(CurrentPalette.StageCurrent, 150), 8);

        for (var i = 0; i < PauseOptionCount; i++)
        {
            DrawPauseOption(i, i == _pauseSelectionIndex, pulse);
        }
    }

    private void DrawPauseOption(int optionIndex, bool selected, float pulse)
    {
        var width = selected ? 180 + (int)(pulse * 10f) : 150;
        var height = selected ? 180 + (int)(pulse * 10f) : 150;
        var centerX = 520 + optionIndex * 220;
        var centerY = selected ? 530 - (int)(pulse * 5f) : 540;
        var card = new Rectangle(centerX - width / 2, centerY - height / 2, width, height);
        var body = selected ? WithAlpha(CurrentPalette.Grid, 250) : WithAlpha(CurrentPalette.HudBackground, 235);
        var frame = selected ? CurrentPalette.GemShine : CurrentPalette.WallInner;

        DrawRectangle(card, body);
        DrawFrame(card, frame, selected ? 12 : 8);

        if (optionIndex == 0)
        {
            DrawArrow(new Rectangle(card.X + 30, card.Y + 50, card.Width - 60, card.Height - 100), true, CurrentPalette.ExitOpen);
        }
        else if (optionIndex == 1)
        {
            DrawFrame(new Rectangle(card.X + 36, card.Y + 42, card.Width - 72, card.Height - 84), CurrentPalette.Hazard, 10);
            DrawLine(new Vector2(card.X + 42, card.Bottom - 46), new Vector2(card.Right - 42, card.Y + 46), 12, CurrentPalette.HazardInner);
        }
        else if (optionIndex == 2)
        {
            for (var i = 0; i < 3; i++)
            {
                DrawRectangle(new Rectangle(card.X + 28 + i * 28, card.Y + 44, 20, 20), i == _selectedStageIndex ? CurrentPalette.Gem : CurrentPalette.StageCurrent);
            }

            DrawFrame(new Rectangle(card.X + 28, card.Y + 92, card.Width - 56, 34), CurrentPalette.ExitOpen, 6);
        }
        else if (optionIndex == 3)
        {
            DrawPaletteSwatches(new Rectangle(card.X + 30, card.Y + 38, card.Width - 60, card.Height - 76));
        }
        else
        {
            DrawWaveIcon(new Rectangle(card.X + 34, card.Y + 38, card.Width - 68, card.Height - 76), selected ? CurrentPalette.GemShine : CurrentPalette.StageCurrent);
        }

        if (selected)
        {
            DrawFrame(new Rectangle(card.X - 18, card.Y - 18, card.Width + 36, card.Height + 36), WithAlpha(CurrentPalette.Gem, 160), 6);
        }
    }

    private void DrawStageSelect()
    {
        DrawRectangle(new Rectangle(0, 0, VirtualWidth, VirtualHeight), WithAlpha(CurrentPalette.Background, 210));

        var center = new Vector2(VirtualWidth / 2f, VirtualHeight / 2f);
        var pulse = (float)((Math.Sin(_stageSelectAnimationTime * 5d) + 1d) * 0.5d);
        for (var i = 0; i < 18; i++)
        {
            var angle = (float)(i * Math.PI * 2d / 18d - _stageSelectAnimationTime * 0.25d);
            DrawLine(center, center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 560f, 6, WithAlpha(CurrentPalette.StageCurrent, 58));
        }

        DrawArrow(new Rectangle(160, 490, 150, 100), false, CurrentPalette.Gem);
        DrawArrow(new Rectangle(VirtualWidth - 310, 490, 150, 100), true, CurrentPalette.Gem);

        for (var i = 0; i < _stages.Length; i++)
        {
            DrawStageCard(i, i == _selectedStageIndex, pulse);
        }
    }

    private float GetStageCardOffset(int stageIndex) => stageIndex - _selectedStageIndex;

    private void DrawStageCard(int stageIndex, bool selected, float pulse)
    {
        var focusAmount = selected ? _stageSelectFocusAmount : 0f;
        var width = 320 + (int)(60f * focusAmount);
        var height = 420 + (int)(80f * focusAmount);
        var centerOffset = GetStageCardOffset(stageIndex);
        var centerX = VirtualWidth / 2 + (int)(centerOffset * StageSelectCardGap + _stageSelectSlideOffset);
        var y = 335 - (int)(45f * focusAmount);
        var card = new Rectangle(centerX - width / 2, y, width, height);
        var frameColor = selected ? CurrentPalette.Gem : CurrentPalette.HudInactive;
        var bodyColor = selected ? WithAlpha(CurrentPalette.Grid, 245) : WithAlpha(CurrentPalette.HudBackground, 230);

        DrawRectangle(card, bodyColor);
        DrawFrame(card, frameColor, 10 + (int)(6f * focusAmount));
        DrawFrame(Inset(card, 34), selected ? CurrentPalette.ExitOpen : CurrentPalette.WallOuter, 6 + (int)(2f * focusAmount));

        var preview = new Rectangle(card.X + 54, card.Y + 122, card.Width - 108, card.Height - 245);
        DrawStageMiniMap(preview, _stages[stageIndex], bodyColor);
        DrawStageGemCollection(new Rectangle(card.X + 54, card.Bottom - 112, card.Width - 108, 24), stageIndex);
        DrawStageRecordStats(new Rectangle(card.X + 54, card.Bottom - 80, card.Width - 108, 36), stageIndex);

        if (_stagesCleared.Length > stageIndex && _stagesCleared[stageIndex])
        {
            DrawStageClearedMark(new Rectangle(card.Right - 94, card.Y + 48, 56, 56));
        }

        if (focusAmount > 0.01f)
        {
            var glowAlpha = (byte)(110f + 80f * focusAmount);
            DrawFrame(new Rectangle(card.X - 24, card.Y - 24, card.Width + 48, card.Height + 48), WithAlpha(CurrentPalette.GemShine, glowAlpha), 6 + (int)(4f * focusAmount));
            DrawFrame(new Rectangle(card.X - 38, card.Y - 38, card.Width + 76, card.Height + 76), WithAlpha(CurrentPalette.ExitOpen, (byte)(40f + 65f * focusAmount)), 3 + (int)(2f * focusAmount));
        }
    }

    private void DrawStageClearedMark(Rectangle bounds)
    {
        DrawFrame(bounds, CurrentPalette.ExitOpen, 6);
        DrawLine(new Vector2(bounds.X + 12, bounds.Center.Y + 4), new Vector2(bounds.X + 24, bounds.Bottom - 14), 7, CurrentPalette.GemShine);
        DrawLine(new Vector2(bounds.X + 24, bounds.Bottom - 14), new Vector2(bounds.Right - 10, bounds.Y + 12), 7, CurrentPalette.GemShine);
    }

    private void DrawStageMiniMap(Rectangle bounds, Stage stage, Color cardBody)
    {
        var scale = Math.Min(bounds.Width / (float)VirtualWidth, bounds.Height / (float)VirtualHeight);
        var mapWidth = Math.Max(1, (int)(VirtualWidth * scale));
        var mapHeight = Math.Max(1, (int)(VirtualHeight * scale));
        var map = new Rectangle(bounds.Center.X - mapWidth / 2, bounds.Center.Y - mapHeight / 2, mapWidth, mapHeight);

        DrawRectangle(new Rectangle(map.X - 10, map.Y - 10, map.Width + 20, map.Height + 20), WithAlpha(CurrentPalette.HudBackground, 190));
        DrawFrame(new Rectangle(map.X - 10, map.Y - 10, map.Width + 20, map.Height + 20), CurrentPalette.WallInner, 5);
        DrawRectangle(map, WithAlpha(CurrentPalette.Background, 235));

        foreach (var wall in stage.Walls)
        {
            var mapped = MapStageRectangle(wall, map);
            DrawRectangle(mapped, CurrentPalette.WallOuter);
            DrawRectangle(Inset(mapped, Math.Max(1, mapped.Width > mapped.Height ? mapped.Height / 4 : mapped.Width / 4)), CurrentPalette.WallInner);
        }

        var hospital = MapStageRectangle(stage.HospitalBounds, map);
        DrawRectangle(hospital, CurrentPalette.HudBackground);
        DrawFrame(hospital, CurrentPalette.GemShine, Math.Max(2, hospital.Width / 6));

        var busStop = MapStageRectangle(stage.BusStopBounds, map);
        DrawRectangle(busStop, CurrentPalette.StageCurrent);
        DrawFrame(busStop, CurrentPalette.GemShine, Math.Max(2, busStop.Width / 6));

        var exit = MapStageRectangle(stage.ExitBounds, map);
        DrawRectangle(exit, CurrentPalette.ExitClosed);
        DrawFrame(exit, CurrentPalette.ExitOpen, Math.Max(2, exit.Width / 5));
        DrawRectangle(Inset(exit, Math.Max(3, exit.Width / 3)), cardBody);

        foreach (var ticketPiece in stage.TicketPieces)
        {
            var mapped = MapStageRectangle(ticketPiece, map);
            DrawTicketPiece(mapped, 0, CurrentPalette.ExitOpen, CurrentPalette.GemShine);
        }

        foreach (var gem in stage.Gems)
        {
            var mapped = MapStageRectangle(gem, map);
            DrawGem(mapped.Center.ToVector2(), Math.Max(8, mapped.Width + 4), CurrentPalette.Gem, CurrentPalette.GemShine);
        }

        foreach (var jet in stage.Jets)
        {
            var mapped = MapStageRectangle(jet, map);
            DrawJet(mapped);
        }

        foreach (var roller in stage.Rollers)
        {
            var mapped = MapStageRectangle(roller, map);
            DrawRoller(mapped, 0f);
        }

        foreach (var small in stage.Smalls)
        {
            var mapped = MapStageRectangle(small, map);
            DrawSmall(mapped);
        }

        foreach (var hazard in stage.Hazards)
        {
            var mapped = MapStageRectangle(hazard.Bounds, map);
            DrawRectangle(mapped, CurrentPalette.Hazard);
            DrawFrame(mapped, CurrentPalette.PlayerInvincible, Math.Max(2, mapped.Width / 6));
        }

        var playerSize = Math.Max(10, (int)(76 * scale));
        var player = new Rectangle(
            map.X + (int)(stage.PlayerStart.X * scale) - playerSize / 2,
            map.Y + (int)(stage.PlayerStart.Y * scale) - playerSize / 2,
            playerSize,
            playerSize);
        DrawFrame(new Rectangle(player.X - 3, player.Y - 3, player.Width + 6, player.Height + 6), CurrentPalette.GemShine, Math.Max(2, playerSize / 6));
        DrawRectangle(player, CurrentPalette.Player);
        DrawRectangle(Inset(player, Math.Max(2, playerSize / 4)), CurrentPalette.PlayerInner);
    }

    private void DrawStageGemCollection(Rectangle bounds, int stageIndex)
    {
        if (stageIndex < 0 || stageIndex >= _stages.Length) return;

        var collected = _stageBestGemCounts.Length > stageIndex ? _stageBestGemCounts[stageIndex] : 0;
        DrawGemBag(bounds, collected, _stages[stageIndex].GemBagCapacity, false);
    }
    private void DrawStageRecordStats(Rectangle bounds, int stageIndex)
    {
        Span<StageRecordKind> records = stackalloc StageRecordKind[2];
        var count = 0;
        if (HasRecord(_stagePassRecords, stageIndex))
        {
            records[count++] = StageRecordKind.Pass;
        }



        if (HasRecord(_stageNoDamageRecords, stageIndex))
        {
            records[count++] = StageRecordKind.NoDamage;
        }

        if (count == 0) return;

        var badgeSize = Math.Min(bounds.Height, 34);
        var gap = 8;
        var totalWidth = count * badgeSize + (count - 1) * gap;
        var startX = bounds.Center.X - totalWidth / 2;
        for (var i = 0; i < count; i++)
        {
            var badge = new Rectangle(startX + i * (badgeSize + gap), bounds.Center.Y - badgeSize / 2, badgeSize, badgeSize);
            DrawStageRecordBadge(badge, records[i]);
            DrawBadgeAwardEffect(badge, stageIndex, records[i]);
        }
    }

    private void DrawStageRecordBadge(Rectangle bounds, StageRecordKind kind)
    {
        var color = CurrentPalette.GemShine;
        var detail = CurrentPalette.ExitOpen;
        DrawFrame(bounds, color, 3);
        DrawRectangle(new Rectangle(bounds.Right - 8, bounds.Y + 4, 5, 5), CurrentPalette.GemShine);

        var icon = Inset(bounds, 7);
        switch (kind)
        {
            case StageRecordKind.Pass:
                DrawRecordBus(icon, color, detail, true);
                break;

            case StageRecordKind.NoDamage:
                DrawRecordShield(icon, color, detail, true);
                break;
        }
    }


    private void DrawBadgeAwardEffect(Rectangle bounds, int stageIndex, StageRecordKind kind)
    {
        foreach (var effect in _badgeAwardEffects)
        {
            if (effect.StageIndex != stageIndex || effect.Kind != kind) continue;

            var progress = 1f - effect.TimeRemaining / effect.Duration;
            var pulse = (float)Math.Sin(progress * Math.PI * 6f) * 0.5f + 0.5f;
            var spread = 4 + (int)(12f * progress);
            var alpha = (byte)(210f * (1f - progress));
            DrawFrame(new Rectangle(bounds.X - spread, bounds.Y - spread, bounds.Width + spread * 2, bounds.Height + spread * 2), WithAlpha(CurrentPalette.GemShine, alpha), 3 + (int)(pulse * 3f));
            DrawRectangle(new Rectangle(bounds.Center.X - 3, bounds.Y - 10 - (int)(10f * progress), 6, 6), WithAlpha(CurrentPalette.GemShine, alpha));
            DrawRectangle(new Rectangle(bounds.X - 9 - (int)(7f * progress), bounds.Center.Y - 3, 6, 6), WithAlpha(CurrentPalette.ExitOpen, alpha));
            DrawRectangle(new Rectangle(bounds.Right + 3 + (int)(7f * progress), bounds.Center.Y - 3, 6, 6), WithAlpha(CurrentPalette.ExitOpen, alpha));
            return;
        }
    }
    private void DrawRecordBus(Rectangle bounds, Color color, Color detail, bool achieved)
    {
        var body = new Rectangle(bounds.X + 2, bounds.Center.Y - 7, bounds.Width - 4, 16);
        if (achieved)
        {
            DrawRectangle(body, detail);
        }

        DrawFrame(body, color, 3);
        DrawRectangle(new Rectangle(body.X + 6, body.Y + 5, 10, 5), color);
        DrawRectangle(new Rectangle(body.X + 20, body.Y + 5, 10, 5), color);
        DrawRectangle(new Rectangle(body.X + 7, body.Bottom - 1, 6, 6), color);
        DrawRectangle(new Rectangle(body.Right - 14, body.Bottom - 1, 6, 6), color);
    }

    private void DrawRecordShield(Rectangle bounds, Color color, Color detail, bool achieved)
    {
        var top = new Vector2(bounds.Center.X, bounds.Y + 2);
        var right = new Vector2(bounds.Right - 4, bounds.Y + bounds.Height / 3f);
        var bottom = new Vector2(bounds.Center.X, bounds.Bottom - 2);
        var left = new Vector2(bounds.X + 4, bounds.Y + bounds.Height / 3f);
        if (achieved)
        {
            DrawLine(left, right, Math.Max(8, bounds.Height / 3), WithAlpha(detail, 150));
        }

        DrawLine(top, right, 4, color);
        DrawLine(right, bottom, 4, color);
        DrawLine(bottom, left, 4, color);
        DrawLine(left, top, 4, color);
    }

    private static bool HasRecord(bool[] records, int stageIndex) => records.Length > stageIndex && records[stageIndex];
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
    private void DrawSoundTestMenu()
    {
        var panel = new Rectangle(260, 230, 1400, 620);
        DrawRectangle(panel, WithAlpha(CurrentPalette.HudBackground, 240));
        DrawFrame(panel, CurrentPalette.Gem, 16);
        DrawFrame(Inset(panel, 38), WithAlpha(CurrentPalette.StageCurrent, 150), 8);
        DrawWaveIcon(new Rectangle(panel.X + 60, panel.Y + 52, 110, 90), CurrentPalette.GemShine);

        for (var i = 0; i < SoundTestTerminalIndex; i++)
        {
            var start = GetSoundTestStationPosition(i);
            var end = GetSoundTestStationPosition(i + 1);
            DrawLine(start, end, 12, WithAlpha(CurrentPalette.WallInner, 220));
        }

        for (var i = 0; i <= SoundTestTerminalIndex; i++)
        {
            DrawSoundTestStation(i, i == _soundTestSelectionIndex);
        }
    }

    private void DrawSoundTestStation(int stationIndex, bool selected)
    {
        var position = GetSoundTestStationPosition(stationIndex);
        var active = stationIndex == SoundTestTerminalIndex || stationIndex == _soundTestSelectionIndex || (stationIndex >= 4 && stationIndex <= 6 && stationIndex - 4 == _stageMoveSoundIndex);
        var size = selected ? 72 : 58;
        var station = new Rectangle((int)position.X - size / 2, (int)position.Y - size / 2, size, size);
        var color = active ? CurrentPalette.ExitOpen : CurrentPalette.StageCurrent;

        DrawRectangle(station, active ? WithAlpha(CurrentPalette.Grid, 245) : WithAlpha(CurrentPalette.HudBackground, 235));
        DrawFrame(station, selected ? CurrentPalette.Gem : color, selected ? 10 : 6);

        if (stationIndex == SoundTestTerminalIndex)
        {
            DrawArrow(Inset(station, 18), false, selected ? CurrentPalette.GemShine : color);
            return;
        }

        DrawSoundTestIcon(Inset(station, 14), stationIndex, selected ? CurrentPalette.GemShine : color);
    }

    private Vector2 GetSoundTestStationPosition(int stationIndex)
    {
        var column = stationIndex % 5;
        var row = stationIndex / 5;
        return new Vector2(520 + column * 220, 455 + row * 180);
    }

    private void DrawSoundTestIcon(Rectangle bounds, int stationIndex, Color color)
    {
        if (stationIndex == 0)
        {
            DrawGem(bounds.Center.ToVector2(), Math.Min(bounds.Width, bounds.Height), CurrentPalette.Gem, CurrentPalette.GemShine);
            return;
        }

        if (stationIndex == 1)
        {
            DrawMissMark(bounds);
            return;
        }

        if (stationIndex == 2)
        {
            DrawFrame(bounds, color, 5);
            DrawLine(new Vector2(bounds.X + 8, bounds.Center.Y), new Vector2(bounds.Center.X, bounds.Bottom - 8), 6, color);
            DrawLine(new Vector2(bounds.Center.X, bounds.Bottom - 8), new Vector2(bounds.Right - 6, bounds.Y + 8), 6, color);
            return;
        }

        if (stationIndex == 3)
        {
            DrawFrame(bounds, CurrentPalette.ExitOpen, 5);
            DrawRectangle(new Rectangle(bounds.X + 8, bounds.Y + 8, 6, bounds.Height - 16), color);
            DrawRectangle(new Rectangle(bounds.Right - 14, bounds.Y + 8, 6, bounds.Height - 16), color);
            return;
        }

        if (stationIndex >= 4 && stationIndex <= 6)
        {
            DrawWaveIcon(bounds, color);
            var pipCount = stationIndex - 3;
            for (var i = 0; i < pipCount; i++)
            {
                DrawRectangle(new Rectangle(bounds.X + 4 + i * 11, bounds.Bottom - 6, 7, 7), color);
            }

            return;
        }

        if (stationIndex == 7)
        {
            DrawGem(bounds.Center.ToVector2(), Math.Min(bounds.Width, bounds.Height) - 4, CurrentPalette.Player, CurrentPalette.PlayerInner);
            return;
        }

        if (stationIndex == 8)
        {
            DrawWaveIcon(bounds, color);
            DrawRectangle(new Rectangle(bounds.Center.X - 4, bounds.Bottom - 15, 8, 8), color);
            return;
        }

        if (stationIndex == 9)
        {
            DrawRectangle(new Rectangle(bounds.X + 6, bounds.Center.Y - 5, bounds.Width - 12, 18), CurrentPalette.PlayerInner);
            DrawFrame(new Rectangle(bounds.X + 6, bounds.Center.Y - 5, bounds.Width - 12, 18), color, 3);
            DrawRectangle(new Rectangle(bounds.X + bounds.Width / 2 - 6, bounds.Center.Y - 13, 12, 8), color);
            DrawRectangle(new Rectangle(bounds.X + 13, bounds.Center.Y + 11, 8, 8), color);
            DrawRectangle(new Rectangle(bounds.Right - 21, bounds.Center.Y + 11, 8, 8), color);
            DrawLine(new Vector2(bounds.X + 8, bounds.Y + 11), new Vector2(bounds.X + 25, bounds.Y + 4), 4, color);
            DrawLine(new Vector2(bounds.Right - 8, bounds.Y + 11), new Vector2(bounds.Right - 25, bounds.Y + 4), 4, color);
            return;
        }

        DrawFrame(bounds, color, 4);
        DrawLine(new Vector2(bounds.Center.X + 3, bounds.Y + 8), new Vector2(bounds.Center.X + 3, bounds.Bottom - 8), 4, color);
        DrawRectangle(new Rectangle(bounds.Center.X - 13, bounds.Center.Y - 2, 7, 7), color);
    }

    private void DrawWaveIcon(Rectangle bounds, Color color)
    {
        var midY = bounds.Center.Y;
        var step = Math.Max(12, bounds.Width / 4);
        for (var i = 0; i < 3; i++)
        {
            var x = bounds.X + i * step;
            DrawLine(new Vector2(x, midY), new Vector2(x + step / 2f, bounds.Y + 8), 7, color);
            DrawLine(new Vector2(x + step / 2f, bounds.Y + 8), new Vector2(x + step, midY), 7, color);
            DrawLine(new Vector2(x + step, midY), new Vector2(x + step + step / 2f, bounds.Bottom - 8), 7, color);
        }
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
        DrawRectangle(new Rectangle(0, 0, VirtualWidth, VirtualHeight), WithAlpha(CurrentPalette.Background, 176));

        var center = new Vector2(VirtualWidth / 2f, VirtualHeight / 2f);
        var pulse = (float)((Math.Sin(_clearAnimationTime * 5d) + 1d) * 0.5d);
        var rankBody = GetClearRankBodyColor();
        var rankShine = GetClearRankShineColor();
        var glow = Color.Lerp(rankBody, rankShine, pulse);

        for (var i = 0; i < 24; i++)
        {
            var angle = (float)(i * Math.PI * 2d / 24d + _clearAnimationTime * 0.42d);
            var length = 340 + (i % 4) * 80 + (int)(pulse * 35f);
            DrawLine(center, center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * length, 10, WithAlpha(CurrentPalette.ExitOpen, 95));
        }

        DrawConfetti();
        DrawFrame(new Rectangle(540, 300, 840, 480), rankBody, 18);
        DrawFrame(new Rectangle(590, 350, 740, 380), glow, 12);
        DrawRectangle(new Rectangle(630, 390, 660, 300), WithAlpha(CurrentPalette.HudBackground, 230));

        DrawOrbitingGems(center);

        var mainBob = (float)Math.Sin(_clearAnimationTime * 3.8d) * 16f;
        var sideBob = (float)Math.Sin(_clearAnimationTime * 4.6d + Math.PI) * 10f;
        DrawGem(new Vector2(960, 505 + mainBob), 170 + (int)(pulse * 12f), rankBody, rankShine);
        DrawGem(new Vector2(760, 545 + sideBob), 86, CurrentPalette.Player, CurrentPalette.PlayerInner);
        DrawGem(new Vector2(1160, 545 - sideBob), 86, CurrentPalette.Hazard, CurrentPalette.HazardInner);

        for (var i = 0; i < _stages.Length; i++)
        {
            var checkPulse = i == _currentStageIndex ? (int)(pulse * 8f) : 0;
            DrawRectangle(new Rectangle(828 + i * 78 - checkPulse / 2, 650 - checkPulse / 2, 54 + checkPulse, 54 + checkPulse), CurrentPalette.ExitOpen);
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

    private void DrawTicketPiece(Rectangle bounds, int variant, Color body, Color detail)
    {
        var inset = Math.Max(2, Math.Min(bounds.Width, bounds.Height) / 8);
        DrawRectangle(bounds, body);
        DrawFrame(bounds, detail, Math.Max(2, inset / 2));

        var notchSize = Math.Max(4, Math.Min(bounds.Width, bounds.Height) / 5);
        DrawRectangle(new Rectangle(bounds.X - 1, bounds.Center.Y - notchSize / 2, notchSize, notchSize), _backgroundColor);
        DrawRectangle(new Rectangle(bounds.Right - notchSize + 1, bounds.Center.Y - notchSize / 2, notchSize, notchSize), _backgroundColor);

        var lineX = bounds.X + bounds.Width / 2 + (variant % 2 == 0 ? -2 : 2);
        DrawLine(new Vector2(lineX, bounds.Y + inset), new Vector2(lineX, bounds.Bottom - inset), Math.Max(2, inset / 2), detail);
        DrawRectangle(new Rectangle(bounds.X + inset, bounds.Y + inset, Math.Max(4, bounds.Width / 4), Math.Max(3, bounds.Height / 8)), detail);
    }
    private void DrawSmall(Rectangle bounds)
    {
        DrawRectangle(bounds, CurrentPalette.PlayerInner);
        DrawFrame(bounds, CurrentPalette.GemShine, Math.Max(3, bounds.Width / 8));
        DrawRectangle(Inset(bounds, Math.Max(6, bounds.Width / 4)), CurrentPalette.Background);
    }

    private void DrawRoller(Rectangle bounds, float angle)
    {
        DrawRectangle(bounds, CurrentPalette.WallInner);
        DrawFrame(bounds, CurrentPalette.GemShine, Math.Max(3, bounds.Width / 8));
        DrawRollerBar(bounds, angle, CurrentPalette.GemShine);
    }

    private void DrawRollerBar(Rectangle bounds, float angle, Color color)
    {
        var direction = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
        var halfLength = Math.Max(8f, Math.Min(bounds.Width, bounds.Height) * 0.36f);
        var center = bounds.Center.ToVector2();
        DrawLine(center - direction * halfLength, center + direction * halfLength, Math.Max(4, Math.Min(bounds.Width, bounds.Height) / 9), color);
    }

    private void DrawJet(Rectangle bounds)
    {
        var flame = new Rectangle(bounds.X + bounds.Width / 8, bounds.Bottom - bounds.Height / 4, bounds.Width * 3 / 4, bounds.Height / 3);
        DrawRectangle(bounds, CurrentPalette.StageCurrent);
        DrawFrame(bounds, CurrentPalette.GemShine, Math.Max(3, bounds.Width / 8));
        DrawLine(new Vector2(bounds.Center.X, bounds.Y + bounds.Height / 6), new Vector2(bounds.Center.X, bounds.Bottom - bounds.Height / 5), Math.Max(4, bounds.Width / 7), CurrentPalette.PlayerInner);
        DrawRectangle(flame, CurrentPalette.GemShine);
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
        if (_soundTestOpen)
        {
            UpdateSoundTestMenu(keyboard, gamePad);
            return;
        }

        if (WasPressed(keyboard, Keys.Left) || WasPressed(keyboard, Keys.A) || WasPressed(gamePad.DPad.Left, _previousGamePad.DPad.Left) || WasThumbstickPressedLeft(gamePad))
        {
            _pauseSelectionIndex = (_pauseSelectionIndex + PauseOptionCount - 1) % PauseOptionCount;
            PlayStageMoveSound();
        }

        if (WasPressed(keyboard, Keys.Right) || WasPressed(keyboard, Keys.D) || WasPressed(gamePad.DPad.Right, _previousGamePad.DPad.Right) || WasThumbstickPressedRight(gamePad))
        {
            _pauseSelectionIndex = (_pauseSelectionIndex + 1) % PauseOptionCount;
            PlayStageMoveSound();
        }

        if (WasPressed(keyboard, Keys.Enter)
            || WasPressed(keyboard, Keys.Space)
            || WasPressed(gamePad.Buttons.Start, _previousGamePad.Buttons.Start)
            || WasPressed(gamePad.Buttons.A, _previousGamePad.Buttons.A))
        {
            SelectPauseOption();
        }
    }


    private void UpdateSoundTestMenu(KeyboardState keyboard, GamePadState gamePad)
    {
        if (WasPressed(keyboard, Keys.Left) || WasPressed(keyboard, Keys.A) || WasPressed(gamePad.DPad.Left, _previousGamePad.DPad.Left) || WasThumbstickPressedLeft(gamePad))
        {
            _soundTestSelectionIndex = (_soundTestSelectionIndex + SoundTestTerminalIndex) % (SoundTestTerminalIndex + 1);
            PlayStageMoveSound();
        }

        if (WasPressed(keyboard, Keys.Right) || WasPressed(keyboard, Keys.D) || WasPressed(gamePad.DPad.Right, _previousGamePad.DPad.Right) || WasThumbstickPressedRight(gamePad))
        {
            _soundTestSelectionIndex = (_soundTestSelectionIndex + 1) % (SoundTestTerminalIndex + 1);
            PlayStageMoveSound();
        }

        if (WasPressed(keyboard, Keys.Enter)
            || WasPressed(keyboard, Keys.Space)
            || WasPressed(gamePad.Buttons.Start, _previousGamePad.Buttons.Start)
            || WasPressed(gamePad.Buttons.A, _previousGamePad.Buttons.A))
        {
            SelectSoundTestStation();
        }
    }

    private void OpenSoundTestMenu()
    {
        _soundTestOpen = true;
        _soundTestSelectionIndex = 0;
        PlaySound(_confirmSound);
    }

    private void SelectSoundTestStation()
    {
        if (_soundTestSelectionIndex == SoundTestTerminalIndex)
        {
            _soundTestOpen = false;
            PlaySound(_confirmSound);
            return;
        }

        if (_soundTestSelectionIndex >= 4 && _soundTestSelectionIndex <= 6)
        {
            SelectStageMoveSound(_soundTestSelectionIndex - 4);
            return;
        }

        PlaySound(_soundTestSounds[_soundTestSelectionIndex]);
    }
    private void OpenPauseMenu()
    {
        _pauseCount++;
        LogPlayEvent(PlayEventKind.Pause, _currentStageIndex, GetPlayerBounds().Center.ToVector2(), _currentStageElapsedSeconds, _pauseCount);
        _paused = true;
        _soundTestOpen = false;
        PlaySound(_pauseSound);
        _pauseSelectionIndex = 0;
        _pauseAnimationTime = 0d;
        RefreshWindowTitle();
    }

    private void ClosePauseMenu()
    {
        _paused = false;
        _soundTestOpen = false;
        PlaySound(_pauseSound);
        _pauseAnimationTime = 0d;
        RefreshWindowTitle();
    }

    private void SelectPauseOption()
    {
        if (_pauseSelectionIndex == SoundTestEntryOption)
        {
            OpenSoundTestMenu();
            return;
        }

        PlaySound(_confirmSound);
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

    private void UpdateStageSelectSlide(float elapsed)
    {
        if (_stageSelectPendingMoveDirection != 0)
        {
            _stageSelectFocusAmount = MathHelper.Lerp(_stageSelectFocusAmount, 0f, Math.Min(1f, elapsed * StageSelectFocusMoveShrinkRate));
            if (_stageSelectFocusAmount <= 0.01f)
            {
                var direction = _stageSelectPendingMoveDirection;
                _stageSelectPendingMoveDirection = 0;
                _stageSelectFocusAmount = 0f;
                ApplyStageSelectionMove(direction);
            }

            return;
        }

        if (_stageSelectSlideDelay > 0f)
        {
            _stageSelectSlideDelay = Math.Max(0f, _stageSelectSlideDelay - elapsed);
            _stageSelectFocusAmount = 0f;
            return;
        }

        var wasSliding = _stageSelectSlideOffset != 0f;
        _stageSelectSlideOffset = MathHelper.Lerp(_stageSelectSlideOffset, 0f, Math.Min(1f, elapsed * StageSelectSlideReturnRate));
        if (Math.Abs(_stageSelectSlideOffset) < 0.5f)
        {
            _stageSelectSlideOffset = 0f;
        }

        if (wasSliding && _stageSelectSlideOffset == 0f)
        {
            _stageSelectQuickMoveTimeRemaining = StageSelectQuickMoveSeconds;
        }

        if (_stageSelectQueuedMoveDirection != 0)
        {
            _stageSelectFocusAmount = 0f;
            if (Math.Abs(_stageSelectSlideOffset) <= StageSelectChainMoveOffset)
            {
                var direction = _stageSelectQueuedMoveDirection;
                _stageSelectQueuedMoveDirection = 0;
                ApplyStageSelectionMove(direction, true);
            }

            return;
        }

        _stageSelectQuickMoveTimeRemaining = Math.Max(0f, _stageSelectQuickMoveTimeRemaining - elapsed);

        var focusTarget = Math.Abs(_stageSelectSlideOffset) <= StageSelectFocusStartOffset && _stageSelectSlideDelay == 0f ? 1f : 0f;
        var focusRate = focusTarget > _stageSelectFocusAmount
            ? MathHelper.Lerp(StageSelectFocusExpandFastRate, StageSelectFocusExpandSlowRate, _stageSelectFocusAmount)
            : StageSelectFocusShrinkRate;
        _stageSelectFocusAmount = MathHelper.Lerp(_stageSelectFocusAmount, focusTarget, Math.Min(1f, elapsed * focusRate));
        if (Math.Abs(_stageSelectFocusAmount - focusTarget) < 0.01f)
        {
            _stageSelectFocusAmount = focusTarget;
        }
    }

    private void MoveStageSelection(int direction)
    {
        if (_stageSelectPendingMoveDirection != 0) return;

        if (_stageSelectSlideDelay > 0f || _stageSelectSlideOffset != 0f)
        {
            _stageSelectQueuedMoveDirection = direction;
            return;
        }

        var nextStageIndex = Math.Clamp(_selectedStageIndex + direction, 0, _stages.Length - 1);
        if (nextStageIndex == _selectedStageIndex) return;

        if (_stageSelectFocusAmount < 1f || _stageSelectQuickMoveTimeRemaining > 0f)
        {
            _stageSelectFocusAmount = 0f;
            _stageSelectQuickMoveTimeRemaining = 0f;
            ApplyStageSelectionMove(direction);
            return;
        }

        _stageSelectPendingMoveDirection = direction;
    }

    private void ApplyStageSelectionMove(int direction, bool preserveCurrentPosition = false)
    {
        var nextStageIndex = Math.Clamp(_selectedStageIndex + direction, 0, _stages.Length - 1);
        if (nextStageIndex == _selectedStageIndex) return;

        _selectedStageIndex = nextStageIndex;
        _stageSelectSlideOffset = preserveCurrentPosition
            ? _stageSelectSlideOffset + direction * StageSelectCardGap
            : direction * StageSelectCardGap;
        _stageSelectSlideDelay = preserveCurrentPosition ? 0f : 0.08f;
        _stageSelectFocusAmount = 0f;
        _stageSelectQuickMoveTimeRemaining = 0f;
        PlayStageMoveSound();
    }

    private void UpdateStageSelect(KeyboardState keyboard, GamePadState gamePad, float elapsed)
    {
        UpdateStageSelectMovementInput(keyboard, gamePad, elapsed);

        if (WasPressed(keyboard, Keys.Enter)
            || WasPressed(keyboard, Keys.Space)
            || WasPressed(gamePad.Buttons.Start, _previousGamePad.Buttons.Start)
            || WasPressed(gamePad.Buttons.A, _previousGamePad.Buttons.A))
        {
            StartSelectedStage();
        }
    }

    private void UpdateStageSelectMovementInput(KeyboardState keyboard, GamePadState gamePad, float elapsed)
    {
        var direction = GetStageSelectMoveDirection(keyboard, gamePad);
        if (direction == 0)
        {
            _stageSelectHeldMoveDirection = 0;
            _stageSelectMoveRepeatTimeRemaining = 0f;
            return;
        }

        if (direction != _stageSelectHeldMoveDirection)
        {
            _stageSelectHeldMoveDirection = direction;
            _stageSelectMoveRepeatTimeRemaining = StageSelectMoveRepeatSeconds;
            MoveStageSelection(direction);
            return;
        }

        _stageSelectMoveRepeatTimeRemaining = Math.Max(0f, _stageSelectMoveRepeatTimeRemaining - elapsed);
        if (_stageSelectMoveRepeatTimeRemaining > 0f) return;

        _stageSelectMoveRepeatTimeRemaining = StageSelectMoveRepeatSeconds;
        MoveStageSelection(direction);
    }

    private static int GetStageSelectMoveDirection(KeyboardState keyboard, GamePadState gamePad)
    {
        var thumbstickX = gamePad.ThumbSticks.Left.X;
        if (keyboard.IsKeyDown(Keys.Left) || keyboard.IsKeyDown(Keys.A) || gamePad.DPad.Left == ButtonState.Pressed || thumbstickX < -0.55f)
        {
            return -1;
        }

        if (keyboard.IsKeyDown(Keys.Right) || keyboard.IsKeyDown(Keys.D) || gamePad.DPad.Right == ButtonState.Pressed || thumbstickX > 0.55f)
        {
            return 1;
        }

        return 0;
    }

    private void OpenStageSelect()
    {
        LogPlayEvent(PlayEventKind.StageSelect, _currentStageIndex, GetPlayerBounds().Center.ToVector2(), _currentStageElapsedSeconds, _selectedStageIndex);
        _paused = false;
        _stageSelectOpen = true;
        _selectedStageIndex = _currentStageIndex;
        _stageSelectAnimationTime = 0d;
        _stageSelectSlideOffset = 0f;
        _stageSelectSlideDelay = 0f;
        _stageSelectFocusAmount = 1f;
        _stageSelectPendingMoveDirection = 0;
        _stageSelectQueuedMoveDirection = 0;
        _stageSelectHeldMoveDirection = 0;
        _stageSelectQuickMoveTimeRemaining = 0f;
        _stageSelectMoveRepeatTimeRemaining = 0f;
        RefreshWindowTitle();
    }

    private void StartSelectedStage()
    {
        PlaySound(_confirmSound);
        _deaths = 0;
        _runStartStageIndex = _selectedStageIndex;
        StartStatsRun(_selectedStageIndex != 0);
        LoadStage(_selectedStageIndex);
        _stageSelectOpen = false;
        _paused = false;
        _stageSelectAnimationTime = 0d;
        _stageSelectSlideOffset = 0f;
        _stageSelectSlideDelay = 0f;
        _stageSelectFocusAmount = 1f;
        _stageSelectPendingMoveDirection = 0;
        _stageSelectQueuedMoveDirection = 0;
        _stageSelectHeldMoveDirection = 0;
        _stageSelectQuickMoveTimeRemaining = 0f;
        _stageSelectMoveRepeatTimeRemaining = 0f;
        RefreshWindowTitle();
    }

    private void MovePlayer(Vector2 move, float elapsed)
    {
        if (move == Vector2.Zero)
        {
            _playerInputDirection = Vector2.Zero;
            _playerGhostVelocity = Vector2.Zero;
            _inputContactWallIndex = -1;
            ClearActiveWallFollow();
            return;
        }

        move.Normalize();
        _lastMoveDirection = move;
        _playerFacingDirection = GetCardinalDirection(move);
        _playerInputDirection = _playerFacingDirection;
        _inputContactWallIndex = -1;
        var speed = PlayerSpeed;
        if (_jetActive)
        {
            speed *= JetPlayerSpeedMultiplier;
        }

        if (_smallActive)
        {
            speed *= SmallPlayerSpeedMultiplier;
        }

        var velocity = move * speed * elapsed;
        _playerGhostVelocity = GetPlayerGhostVelocity(velocity);
        _wallFollowMovedThisFrame = false;
        var maxMoveAmount = MathF.Max(MathF.Abs(velocity.X), MathF.Abs(velocity.Y));
        if (_rollerActive && IsRollerWallFollowActive())
        {
            ContinueRollerWallFollow(maxMoveAmount * RollerSlideMultiplier);
            return;
        }

        if (!_rollerActive && IsBasicWallFollowActive())
        {
            ContinueBasicWallFollow(maxMoveAmount * BasicWallFollowCornerTurnMultiplier);
            return;
        }

        TryMove(new Vector2(velocity.X, 0f));
        TryMove(new Vector2(0f, velocity.Y));
        if (_rollerActive)
        {
            ContinueRollerWallFollow(maxMoveAmount * RollerSlideMultiplier);
        }
        else
        {
            ContinueBasicWallFollow(maxMoveAmount * BasicWallFollowCornerTurnMultiplier);
        }
    }

    private void DrawWallFollowWallHighlights()
    {
        if (_wallFollowWallContact.IsValid(_walls.Length))
        {
            DrawWallFollowWallHighlight(_wallFollowWallContact, new Color(78, 220, 150), 10);
        }

        if (_wallFollowHitContact.IsValid(_walls.Length))
        {
            var thickness = _wallFollowHitContact.WallIndex == _wallFollowWallContact.WallIndex ? 16 : 10;
            DrawWallFollowWallHighlight(_wallFollowHitContact, new Color(255, 174, 72), thickness);
        }
    }

    private void DrawWallFollowWallHighlight(WallContact contact, Color color, int thickness)
    {
        var wall = _walls[contact.WallIndex];
        var alpha = WithAlpha(color, 230);
        var glow = WithAlpha(color, 90);
        switch (contact.Side)
        {
            case WallContactSide.Top:
                DrawLine(new Vector2(wall.Left, wall.Top), new Vector2(wall.Right, wall.Top), thickness + 8, glow);
                DrawLine(new Vector2(wall.Left, wall.Top), new Vector2(wall.Right, wall.Top), thickness, alpha);
                break;
            case WallContactSide.Right:
                DrawLine(new Vector2(wall.Right, wall.Top), new Vector2(wall.Right, wall.Bottom), thickness + 8, glow);
                DrawLine(new Vector2(wall.Right, wall.Top), new Vector2(wall.Right, wall.Bottom), thickness, alpha);
                break;
            case WallContactSide.Bottom:
                DrawLine(new Vector2(wall.Left, wall.Bottom), new Vector2(wall.Right, wall.Bottom), thickness + 8, glow);
                DrawLine(new Vector2(wall.Left, wall.Bottom), new Vector2(wall.Right, wall.Bottom), thickness, alpha);
                break;
            case WallContactSide.Left:
                DrawLine(new Vector2(wall.Left, wall.Top), new Vector2(wall.Left, wall.Bottom), thickness + 8, glow);
                DrawLine(new Vector2(wall.Left, wall.Top), new Vector2(wall.Left, wall.Bottom), thickness, alpha);
                break;
        }
    }

    private void DrawInputContactWallHighlight()
    {
        if (_playerInAmbulance || _playerInBus || _playerInputDirection == Vector2.Zero) return;

        var probe = GetPlayerBounds();
        probe.Offset((int)(_playerInputDirection.X * WallContactProbeDistance), (int)(_playerInputDirection.Y * WallContactProbeDistance));
        var color = new Color(118, 218, 255);

        if (_inputContactWallIndex >= 0 && _inputContactWallIndex < _walls.Length)
        {
            DrawWallFollowWallHighlight(CreateWallContact(_inputContactWallIndex, _playerInputDirection), color, 7);
            return;
        }

        for (var i = 0; i < _walls.Length; i++)
        {
            if (!probe.Intersects(_walls[i])) continue;

            DrawWallFollowWallHighlight(CreateWallContact(i, _playerInputDirection), color, 7);
            return;
        }
    }

    private void DrawPlayerFacingLight()
    {
        if (_playerInAmbulance || _playerInBus) return;

        var player = GetPlayerBounds();
        var size = GetCurrentPlayerSize();
        var direction = _playerFacingDirection;
        var pulse = (float)((Math.Sin(_currentStageElapsedSeconds * 7.0d) + 1d) * 0.5d);
        var segmentCount = 6;
        var segmentLength = Math.Max(8, size / 2);
        var beamWidth = Math.Max(12, size - 10);
        var perpendicular = new Vector2(-direction.Y, direction.X);

        for (var i = segmentCount - 1; i >= 0; i--)
        {
            var distance = size * 0.55f + segmentLength * (i + 0.5f);
            var center = player.Center.ToVector2() + direction * distance;
            var fade = 1f - i / (float)segmentCount;
            var alpha = (byte)((22f + 52f * pulse) * fade);
            var halfLength = segmentLength / 2f;
            var halfWidth = beamWidth * (0.36f + fade * 0.34f);
            var bounds = new Rectangle(
                (int)(center.X - MathF.Abs(direction.X) * halfLength - MathF.Abs(perpendicular.X) * halfWidth),
                (int)(center.Y - MathF.Abs(direction.Y) * halfLength - MathF.Abs(perpendicular.Y) * halfWidth),
                Math.Max(1, (int)(MathF.Abs(direction.X) * segmentLength + MathF.Abs(perpendicular.X) * halfWidth * 2f)),
                Math.Max(1, (int)(MathF.Abs(direction.Y) * segmentLength + MathF.Abs(perpendicular.Y) * halfWidth * 2f)));

            DrawRectangle(bounds, WithAlpha(CurrentPalette.StageCurrent, alpha));
        }

        var markerCenter = player.Center.ToVector2() + direction * size;
        var marker = new Rectangle(
            (int)markerCenter.X - size / 2,
            (int)markerCenter.Y - size / 2,
            size,
            size);
        DrawFrame(marker, WithAlpha(CurrentPalette.StageCurrent, (byte)(72f + 28f * pulse)), 2);
    }

    private void DrawPlayerGhost()
    {
        if (_playerInAmbulance || _playerInBus || _playerGhostVelocity == Vector2.Zero) return;

        var ghost = GetPlayerBounds();
        ghost.Offset((int)MathF.Round(_playerGhostVelocity.X), (int)MathF.Round(_playerGhostVelocity.Y));
        var hitWallIndex = FindCollidingWallIndex(ghost);
        var frameColor = hitWallIndex >= 0 ? new Color(255, 174, 72) : new Color(118, 218, 255);
        DrawRectangle(ghost, WithAlpha(frameColor, 45));
        DrawFrame(ghost, WithAlpha(frameColor, 220), 3);
        DrawFrame(new Rectangle(ghost.X - 4, ghost.Y - 4, ghost.Width + 8, ghost.Height + 8), WithAlpha(frameColor, 120), 2);

        if (hitWallIndex >= 0)
        {
            DrawWallFollowWallHighlight(CreateWallContact(hitWallIndex, _playerGhostVelocity), frameColor, 6);
        }
    }

    private void DrawPlayer()
    {
        if (_playerInAmbulance || _playerInBus) return;

        if (_invincibleTimeRemaining > 0f && (int)(_invincibleTimeRemaining * 16f) % 2 == 0)
        {
            DrawRectangle(GetPlayerBounds(), CurrentPalette.PlayerInvincible);
            DrawRectangle(Inset(GetPlayerBounds(), 12), CurrentPalette.Player);
            return;
        }

        var player = GetPlayerBounds();
        DrawRectangle(player, _cleared ? CurrentPalette.ExitOpen : CurrentPalette.Player);
        DrawRectangle(Inset(player, Math.Max(6, GetCurrentPlayerSize() / 5)), _jetActive ? CurrentPalette.GemShine : CurrentPalette.PlayerInner);
        if (_smallActive)
        {
            DrawFrame(new Rectangle(player.X - 5, player.Y - 5, player.Width + 10, player.Height + 10), CurrentPalette.PlayerInner, 3);
        }
        if (_rollerActive)
        {
            var angle = (float)(_currentStageElapsedSeconds * 8.0d);
            DrawRollerBar(player, angle, CurrentPalette.GemShine);
        }
        if (_jetActive)
        {
            DrawFrame(new Rectangle(player.X - 7, player.Y - 7, player.Width + 14, player.Height + 14), CurrentPalette.StageCurrent, 4);
            DrawLine(new Vector2(player.X - 18, player.Center.Y), new Vector2(player.X - 4, player.Center.Y), 5, CurrentPalette.GemShine);
        }

        DrawStageStartNormalPulse(player);
    }

    private void DrawStageStartNormalPulse(Rectangle player)
    {
        if (_stageStartNormalPulseRemaining <= 0f || _jetActive || _rollerActive || _smallActive) return;

        var progress = 1f - _stageStartNormalPulseRemaining / StageStartNormalPulseSeconds;
        var spread = 8 + (int)(22f * progress);
        var alpha = (byte)(190f * (1f - progress));
        DrawFrame(new Rectangle(player.X - spread, player.Y - spread, player.Width + spread * 2, player.Height + spread * 2), WithAlpha(CurrentPalette.PlayerInner, alpha), 4);
        DrawFrame(new Rectangle(player.X - 4, player.Y - 4, player.Width + 8, player.Height + 8), WithAlpha(CurrentPalette.GemShine, (byte)(120f * (1f - progress))), 3);
    }

    private Vector2 GetPlayerGhostVelocity(Vector2 velocity)
    {
        var amount = MathF.Max(MathF.Abs(velocity.X), MathF.Abs(velocity.Y));
        if (_rollerActive && IsRollerWallFollowActive()) return _rollerSlideDirection * amount * RollerSlideMultiplier;
        if (!_rollerActive && IsBasicWallFollowActive()) return _basicWallFollowSlideDirection * amount * BasicWallFollowCornerTurnMultiplier;
        return velocity;
    }

    private void TryMove(Vector2 delta)
    {
        _playerPosition += delta;
        var player = GetPlayerBounds();

        for (var i = 0; i < _walls.Length; i++)
        {
            if (!player.Intersects(_walls[i])) continue;

            _inputContactWallIndex = i;
            _wallFollowWallContact = CreateWallContact(i, delta);
            _wallFollowHitContact = WallContact.None;
            _playerPosition -= delta;
            if (_rollerActive)
            {
                TryRollerSlide(delta);
            }
            else
            {
                TryBasicWallFollowSlide(delta);
            }

            return;
        }

        RememberWallParallelMove(delta);
    }

    private void TryBasicWallFollowSlide(Vector2 blockedDelta)
    {
        var amount = MathF.Max(MathF.Abs(blockedDelta.X), MathF.Abs(blockedDelta.Y));
        if (amount <= 0f) return;

        var contactDirection = GetAxisDirection(blockedDelta);
        var slideDirection = GetWallFollowSlideDirection(contactDirection);
        if (slideDirection == Vector2.Zero)
        {
            ClearBasicWallFollow();
            return;
        }

        var turnDirection = GetWallFollowTurnDirection(contactDirection, slideDirection);
        if (!TryMoveWithoutRoller(slideDirection * amount, out var hitWallContact))
        {
            _wallFollowHitContact = hitWallContact;
            ClearBasicWallFollow();
            return;
        }

        if (!HasWallNear(contactDirection))
        {
            ClearBasicWallFollow();
            return;
        }

        _wallFollowMovedThisFrame = true;
        _basicWallFollowContactDirection = contactDirection;
        _basicWallFollowSlideDirection = slideDirection;
        _basicWallFollowTurnDirection = turnDirection;
        RememberWallFollowWall(contactDirection);
    }

    private void ContinueBasicWallFollow(float amount)
    {
        if (amount <= 0f || _basicWallFollowContactDirection == Vector2.Zero || _basicWallFollowSlideDirection == Vector2.Zero) return;

        if (HasWallNear(_basicWallFollowContactDirection))
        {
            if (_wallFollowMovedThisFrame) return;

            if (!TryMoveWithoutRoller(_basicWallFollowSlideDirection * amount, out var hitWallContact))
            {
                _wallFollowHitContact = hitWallContact;
                if (TryBasicWallFollowTurnInnerCorner(_basicWallFollowSlideDirection, amount, _basicWallFollowTurnDirection, hitWallContact)) return;

                ClearBasicWallFollow();
                return;
            }

            _wallFollowMovedThisFrame = true;
            if (HasWallNear(_basicWallFollowContactDirection)) return;
        }

        TryBasicWallFollowTurnCorner(_basicWallFollowSlideDirection, _basicWallFollowContactDirection, amount);
    }

    private void TryBasicWallFollowTurnCorner(Vector2 slideDirection, Vector2 contactDirection, float amount)
    {
        if (amount <= 0f) return;

        if (!TryMoveWithoutRoller(contactDirection * amount))
        {
            ClearBasicWallFollow();
            return;
        }

        _wallFollowMovedThisFrame = true;
        var newContactDirection = -slideDirection;
        if (!HasWallNear(newContactDirection))
        {
            TryMoveWithoutRoller(newContactDirection * RollerWallProbeDistance);
        }

        _basicWallFollowContactDirection = newContactDirection;
        _basicWallFollowSlideDirection = GetWallFollowSlideDirection(newContactDirection, _basicWallFollowTurnDirection);
        RememberWallFollowWall(newContactDirection);
    }

    private bool TryBasicWallFollowTurnInnerCorner(Vector2 slideDirection, float amount, int turnDirection, WallContact hitWallContact)
    {
        var newContactDirection = slideDirection;
        var newSlideDirection = GetWallFollowSlideDirection(newContactDirection, turnDirection);
        if (newSlideDirection == Vector2.Zero)
        {
            return false;
        }

        if (!TryMoveWithoutRollerStepped(newSlideDirection * amount))
        {
            return false;
        }

        _wallFollowMovedThisFrame = true;
        _basicWallFollowContactDirection = newContactDirection;
        _basicWallFollowSlideDirection = newSlideDirection;
        _basicWallFollowTurnDirection = turnDirection;
        RememberWallFollowWall(newContactDirection, hitWallContact);
        return true;
    }

    private void TryRollerSlide(Vector2 blockedDelta)
    {
        var amount = MathF.Max(MathF.Abs(blockedDelta.X), MathF.Abs(blockedDelta.Y)) * RollerSlideMultiplier;
        if (amount <= 0f) return;

        var contactDirection = GetAxisDirection(blockedDelta);
        var slideDirection = GetWallFollowSlideDirection(contactDirection);
        if (slideDirection == Vector2.Zero)
        {
            ClearRollerWallFollow();
            return;
        }

        var turnDirection = GetWallFollowTurnDirection(contactDirection, slideDirection);
        var slide = slideDirection * amount;
        if (!TryMoveWithoutRoller(slide, out var hitWallContact))
        {
            _wallFollowHitContact = hitWallContact;
            ClearRollerWallFollow();
            return;
        }

        if (!HasWallNear(contactDirection))
        {
            ClearRollerWallFollow();
            return;
        }

        _wallFollowMovedThisFrame = true;
        _rollerContactDirection = contactDirection;
        _rollerSlideDirection = slideDirection;
        _rollerWallFollowTurnDirection = turnDirection;
        RememberWallFollowWall(contactDirection);
    }

    private void ContinueRollerWallFollow(float amount)
    {
        if (amount <= 0f || _rollerContactDirection == Vector2.Zero || _rollerSlideDirection == Vector2.Zero) return;

        if (HasWallNear(_rollerContactDirection))
        {
            if (_wallFollowMovedThisFrame) return;

            if (!TryMoveWithoutRoller(_rollerSlideDirection * amount, out var hitWallContact))
            {
                _wallFollowHitContact = hitWallContact;
                if (TryRollerTurnInnerCorner(_rollerSlideDirection, amount, _rollerWallFollowTurnDirection, hitWallContact)) return;

                ClearRollerWallFollow();
                return;
            }

            _wallFollowMovedThisFrame = true;
            if (HasWallNear(_rollerContactDirection)) return;
        }

        TryRollerTurnCorner(_rollerSlideDirection, _rollerContactDirection, amount * RollerCornerTurnMultiplier);
    }

    private void TryRollerTurnCorner(Vector2 slideDirection, Vector2 contactDirection, float amount)
    {
        if (amount <= 0f) return;

        var turn = contactDirection * amount;
        if (!TryMoveWithoutRoller(turn)) return;

        _wallFollowMovedThisFrame = true;
        var newContactDirection = -slideDirection;
        if (!HasWallNear(newContactDirection))
        {
            TryMoveWithoutRoller(newContactDirection * RollerWallProbeDistance);
        }

        _rollerContactDirection = newContactDirection;
        _rollerSlideDirection = GetWallFollowSlideDirection(newContactDirection, _rollerWallFollowTurnDirection);
        RememberWallFollowWall(newContactDirection);
    }

    private bool TryRollerTurnInnerCorner(Vector2 slideDirection, float amount, int turnDirection, WallContact hitWallContact)
    {
        var newContactDirection = slideDirection;
        var newSlideDirection = GetWallFollowSlideDirection(newContactDirection, turnDirection);
        if (newSlideDirection == Vector2.Zero)
        {
            return false;
        }

        if (!TryMoveWithoutRollerStepped(newSlideDirection * amount))
        {
            return false;
        }

        _wallFollowMovedThisFrame = true;
        _rollerContactDirection = newContactDirection;
        _rollerSlideDirection = newSlideDirection;
        _rollerWallFollowTurnDirection = turnDirection;
        RememberWallFollowWall(newContactDirection, hitWallContact);
        return true;
    }

    private void RememberWallParallelMove(Vector2 delta)
    {
        var moveDirection = GetAxisDirection(delta);
        if (moveDirection == Vector2.Zero) return;

        var contactDirections = new[] { Vector2.UnitX, -Vector2.UnitX, Vector2.UnitY, -Vector2.UnitY };
        foreach (var contactDirection in contactDirections)
        {
            if (!ArePerpendicular(moveDirection, contactDirection) || !HasWallNear(contactDirection, WallContactProbeDistance)) continue;

            _lastWallParallelContactDirection = contactDirection;
            _lastWallParallelMoveDirection = moveDirection;
            return;
        }
    }

    private Vector2 GetWallFollowSlideDirection(Vector2 contactDirection)
    {
        if (_lastWallParallelMoveDirection != Vector2.Zero
            && _lastWallParallelContactDirection == contactDirection
            && ArePerpendicular(_lastWallParallelMoveDirection, contactDirection))
        {
            return _lastWallParallelMoveDirection;
        }

        return GetWallFollowSlideDirection(contactDirection, 1);
    }

    private static Vector2 GetWallFollowSlideDirection(Vector2 contactDirection, int turnDirection) => GetRightOfDirection(contactDirection) * Math.Sign(turnDirection == 0 ? 1 : turnDirection);

    private static int GetWallFollowTurnDirection(Vector2 contactDirection, Vector2 slideDirection)
    {
        var right = GetRightOfDirection(contactDirection);
        return Vector2.Dot(right, slideDirection) >= 0f ? 1 : -1;
    }

    private static Vector2 GetAxisDirection(Vector2 delta)
    {
        if (MathF.Abs(delta.X) >= MathF.Abs(delta.Y))
        {
            return delta.X < 0f ? new Vector2(-1f, 0f) : delta.X > 0f ? new Vector2(1f, 0f) : Vector2.Zero;
        }

        return delta.Y < 0f ? new Vector2(0f, -1f) : delta.Y > 0f ? new Vector2(0f, 1f) : Vector2.Zero;
    }

    private static Vector2 GetRightOfDirection(Vector2 direction)
    {
        if (direction == Vector2.Zero)
        {
            return Vector2.Zero;
        }

        return new Vector2(-direction.Y, direction.X);
    }

    private static WallContact CreateWallContact(int wallIndex, Vector2 contactDirection) => new(wallIndex, GetWallContactSide(contactDirection));

    private static WallContactSide GetWallContactSide(Vector2 contactDirection)
    {
        var direction = GetAxisDirection(contactDirection);
        if (direction.X > 0f) return WallContactSide.Left;
        if (direction.X < 0f) return WallContactSide.Right;
        if (direction.Y > 0f) return WallContactSide.Top;
        if (direction.Y < 0f) return WallContactSide.Bottom;
        return WallContactSide.None;
    }

    private static bool ArePerpendicular(Vector2 a, Vector2 b) => a != Vector2.Zero && b != Vector2.Zero && MathF.Abs(Vector2.Dot(a, b)) < 0.001f;

    private bool IsBasicWallFollowActive() => _basicWallFollowContactDirection != Vector2.Zero && _basicWallFollowSlideDirection != Vector2.Zero;

    private bool IsRollerWallFollowActive() => _rollerContactDirection != Vector2.Zero && _rollerSlideDirection != Vector2.Zero;

    private void ClearActiveWallFollow()
    {
        if (_rollerActive)
        {
            ClearRollerWallFollow();
            return;
        }

        ClearBasicWallFollow();
    }

    private void RememberWallFollowWall(Vector2 contactDirection, WallContact fallbackContact = null)
    {
        var wallIndex = FindWallNear(contactDirection);
        _wallFollowWallContact = IsValidWallIndex(wallIndex)
            ? CreateWallContact(wallIndex, contactDirection)
            : fallbackContact ?? WallContact.None;
    }

    private void ClearWallFollowWallIndexes()
    {
        _wallFollowWallContact = WallContact.None;
        _wallFollowHitContact = WallContact.None;
    }

    private bool IsValidWallIndex(int wallIndex) => wallIndex >= 0 && wallIndex < _walls.Length;

    private void ClearBasicWallFollow()
    {
        _basicWallFollowContactDirection = Vector2.Zero;
        _basicWallFollowSlideDirection = Vector2.Zero;
        _basicWallFollowTurnDirection = 1;
        ClearWallFollowWallIndexes();
    }

    private void ClearRollerWallFollow()
    {
        _rollerContactDirection = Vector2.Zero;
        _rollerSlideDirection = Vector2.Zero;
        _rollerWallFollowTurnDirection = 1;
        ClearWallFollowWallIndexes();
        ClearBasicWallFollow();
    }

    private bool HasWallNear(Vector2 direction) => HasWallNear(direction, RollerWallProbeDistance);

    private bool HasWallNear(Vector2 direction, int distance) => FindWallNear(direction, distance) >= 0;

    private int FindWallNear(Vector2 direction) => FindWallNear(direction, RollerWallProbeDistance);

    private int FindWallNear(Vector2 direction, int distance)
    {
        if (direction == Vector2.Zero) return -1;

        var player = GetPlayerBounds();
        player.Offset((int)(direction.X * distance), (int)(direction.Y * distance));
        return FindCollidingWallIndex(player);
    }

    private bool TryMoveWithoutRollerStepped(Vector2 delta)
    {
        if (TryMoveWithoutRoller(delta))
        {
            return true;
        }

        foreach (var scale in new[] { 0.5f, 0.25f, 0.125f })
        {
            if (TryMoveWithoutRoller(delta * scale))
            {
                return true;
            }
        }

        var direction = GetAxisDirection(delta);
        return direction != Vector2.Zero && TryMoveWithoutRoller(direction);
    }

    private bool TryMoveWithoutRoller(Vector2 delta) => TryMoveWithoutRoller(delta, out _);

    private bool TryMoveWithoutRoller(Vector2 delta, out WallContact hitWallContact)
    {
        _playerPosition += delta;
        var player = GetPlayerBounds();
        var hitWallIndex = FindCollidingWallIndex(player);
        if (hitWallIndex >= 0)
        {
            hitWallContact = CreateWallContact(hitWallIndex, delta);
            _playerPosition -= delta;
            return false;
        }

        hitWallContact = WallContact.None;
        return true;
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
        for (var i = 0; i < _ticketPieceBounds.Length; i++)
        {
            if (!_ticketPiecesCollected[i] && player.Intersects(_ticketPieceBounds[i]))
            {
                _ticketPiecesCollected[i] = true;
                LogPlayEvent(PlayEventKind.TicketPiece, _currentStageIndex, _ticketPieceBounds[i].Center.ToVector2(), _currentStageElapsedSeconds, i);
                PlaySound(_gemSound);
                AddGemCollectEffect(_ticketPieceBounds[i].Center.ToVector2(), i);
                StartExitOpenDelayIfReady();
            }
        }

        for (var i = 0; i < _jetBounds.Length; i++)
        {
            if (_jetsCollected[i] || !player.Intersects(_jetBounds[i])) continue;

            _jetsCollected[i] = true;
            _jetActive = true;
            LogPlayEvent(PlayEventKind.Jet, _currentStageIndex, _jetBounds[i].Center.ToVector2(), _currentStageElapsedSeconds, i);
            PlaySound(_gemSound, 0.72f);
            AddGemCollectEffect(_jetBounds[i].Center.ToVector2(), i + _ticketPieceBounds.Length + _gemBounds.Length);
        }

        for (var i = 0; i < _rollerBounds.Length; i++)
        {
            if (_rollersCollected[i] || !player.Intersects(_rollerBounds[i])) continue;

            _rollersCollected[i] = true;
            _rollerActive = true;
            LogPlayEvent(PlayEventKind.Roller, _currentStageIndex, _rollerBounds[i].Center.ToVector2(), _currentStageElapsedSeconds, i);
            PlaySound(_gemSound, 0.78f);
            AddGemCollectEffect(_rollerBounds[i].Center.ToVector2(), i + _ticketPieceBounds.Length + _gemBounds.Length + _jetBounds.Length);
        }

        for (var i = 0; i < _smallBounds.Length; i++)
        {
            if (_smallsCollected[i] || !player.Intersects(_smallBounds[i])) continue;

            _smallsCollected[i] = true;
            ActivateSmall(_smallBounds[i].Center.ToVector2());
            LogPlayEvent(PlayEventKind.Small, _currentStageIndex, _smallBounds[i].Center.ToVector2(), _currentStageElapsedSeconds, i);
            PlaySound(_gemSound, 0.68f);
            AddGemCollectEffect(_smallBounds[i].Center.ToVector2(), i + _ticketPieceBounds.Length + _gemBounds.Length + _jetBounds.Length + _rollerBounds.Length);
        }

        for (var i = 0; i < _gemBounds.Length; i++)
        {
            if (_gemsCollected[i] || !player.Intersects(_gemBounds[i])) continue;

            if (IsGemBagFull())
            {
                ReactToFullGemBag();
                return;
            }

            _gemsCollected[i] = true;
            var shardValue = GetGemShardValue(_gemBounds[i]);
            _stageGemCounts[_currentStageIndex] += shardValue;
            if (_stageBestGemCounts.Length > _currentStageIndex)
            {
                _stageBestGemCounts[_currentStageIndex] = Math.Max(_stageBestGemCounts[_currentStageIndex], CountCollectedGemShards());
            }

            LogPlayEvent(PlayEventKind.Gem, _currentStageIndex, _gemBounds[i].Center.ToVector2(), _currentStageElapsedSeconds, shardValue);
            PlaySound(_gemSound);
            AddGemCollectEffect(_gemBounds[i].Center.ToVector2(), i + _ticketPieceBounds.Length);
        }
    }

    private void ActivateSmall(Vector2 itemCenter)
    {
        if (_smallActive) return;

        var oldBounds = GetPlayerBounds();
        _smallActive = true;
        _playerPosition = oldBounds.Center.ToVector2() - new Vector2(SmallPlayerSize / 2f, SmallPlayerSize / 2f);
        if (CollidesWithWall(GetPlayerBounds()))
        {
            _playerPosition = oldBounds.Center.ToVector2() - new Vector2(SmallPlayerSize / 2f, SmallPlayerSize / 2f);
        }
    }

    private bool CollidesWithWall(Rectangle bounds) => FindCollidingWallIndex(bounds) >= 0;

    private int FindCollidingWallIndex(Rectangle bounds)
    {
        for (var i = 0; i < _walls.Length; i++)
        {
            if (bounds.Intersects(_walls[i])) return i;
        }

        return -1;
    }
    private bool IsGemBagFull() => CountCollectedGemShards() >= GetCurrentGemBagCapacity();

    private void ReactToFullGemBag()
    {
        _gemBagFullNudgeRemaining = GemBagFullNudgeSeconds;
        if (_gemBagFullSoundCooldownRemaining > 0f) return;

        PlaySound(_gemBagFullSound, 0.62f);
        _gemBagFullSoundCooldownRemaining = GemBagFullSoundCooldownSeconds;
    }

    private void StartExitOpenDelayIfReady()
    {
        if (_exitOpen || _exitOpenDelayRemaining > 0f || !AreAllTicketPiecesCollected()) return;

        _exitOpenDelayRemaining = ExitOpenDelaySeconds;
    }

    private void UpdateExitOpening(float elapsed)
    {
        if (_exitOpen || _exitOpenDelayRemaining <= 0f) return;

        _exitOpenDelayRemaining = Math.Max(0f, _exitOpenDelayRemaining - elapsed);
        if (_exitOpenDelayRemaining > 0f) return;

        _exitOpen = true;
        _exitOpenFlashRemaining = ExitOpenFlashSeconds;
        PlaySound(_exitOpenSound);
    }


    private void UpdateGemCollectEffects(float elapsed)
    {
        for (var i = _gemCollectEffects.Count - 1; i >= 0; i--)
        {
            var effect = _gemCollectEffects[i];
            var timeRemaining = effect.TimeRemaining - elapsed;
            if (timeRemaining <= 0f)
            {
                _gemCollectEffects.RemoveAt(i);
                continue;
            }

            _gemCollectEffects[i] = effect with { TimeRemaining = timeRemaining };
        }
    }

    private void AddGemCollectEffect(Vector2 position, int seed)
    {
        _gemCollectEffects.Add(new GemCollectEffect(position, GemCollectEffectSeconds, GemCollectEffectSeconds, seed));
    }

    private void DrawGemCollectEffects()
    {
        foreach (var effect in _gemCollectEffects)
        {
            var progress = 1f - effect.TimeRemaining / effect.Duration;
            var fade = 1f - progress;
            var blink = ((int)(progress * 18f) + effect.Seed) % 2 == 0;
            for (var y = -2; y <= 2; y++)
            {
                for (var x = -2; x <= 2; x++)
                {
                    if (Math.Abs(x) + Math.Abs(y) > 3 || (!blink && (x + y + effect.Seed) % 2 == 0)) continue;

                    var local = new Vector2(x * 11f, y * 11f);
                    var drift = Vector2.Normalize(local == Vector2.Zero ? new Vector2(0f, -1f) : local) * progress * (18f + (x * x + y * y) * 2f);
                    var position = effect.Position + local + drift;
                    var size = Math.Max(3, (int)(12f * fade));
                    var color = (x + y + effect.Seed) % 3 == 0 ? CurrentPalette.GemShine : CurrentPalette.Gem;
                    DrawRectangle(new Rectangle((int)position.X - size / 2, (int)position.Y - size / 2, size, size), WithAlpha(color, (byte)(210f * fade)));
                }
            }

            for (var i = 0; i < 10; i++)
            {
                var angle = (float)((i * Math.PI * 2d / 10d) + effect.Seed * 0.47d);
                var direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                var distance = 18f + progress * (62f + i % 3 * 9f);
                var position = effect.Position + direction * distance;
                var size = Math.Max(3, (int)((10f - i % 3 * 2f) * fade));
                var color = i % 2 == 0 ? CurrentPalette.GemShine : CurrentPalette.Gem;
                DrawRectangle(new Rectangle((int)position.X - size / 2, (int)position.Y - size / 2, size, size), WithAlpha(color, (byte)(220f * fade)));
            }
        }
    }

    private void CheckHazardCollision()
    {
        if (_invincibleTimeRemaining > 0f) return;

        var player = GetPlayerBounds();
        foreach (var hazard in _hazards)
        {
            if (player.Intersects(hazard.Bounds))
            {
                _deaths++;
                _stageDeathCounts[_currentStageIndex]++;
                LogPlayEvent(PlayEventKind.Death, _currentStageIndex, player.Center.ToVector2(), _currentStageElapsedSeconds, _deaths);
                PlaySound(_deathSound);
                AddDeathEffect(player.Center.ToVector2(), _deaths);
                AddDeathStopLine(player.Center.ToVector2(), _lastMoveDirection);
                StartDeathRespawnDelay();
                return;
            }
        }
    }

    private void StartDeathRespawnDelay()
    {
        _deathRespawnPending = true;
        _playerInAmbulance = false;
        _deathRespawnCompleted = false;
        _ambulancePickupDoorPlayed = false;
        _ambulanceDropoffDoorPlayed = false;
        _ambulancePickupBrakePlayed = false;
        _ambulanceDropoffBrakePlayed = false;
        _deathRespawnTimeRemaining = DeathEffectSeconds;
        _invincibleTimeRemaining = 0f;
        _jetActive = false;
        _rollerActive = false;
        ClearRollerWallFollow();
        _smallActive = false;
        PlaySound(_ambulanceSirenSound, 0.72f);
    }

    private void UpdateDeathRespawn(float elapsed)
    {
        if (!_deathRespawnPending) return;

        _deathRespawnTimeRemaining = Math.Max(0f, _deathRespawnTimeRemaining - elapsed);
        var progress = 1f - _deathRespawnTimeRemaining / DeathEffectSeconds;
        if (!_ambulancePickupBrakePlayed && progress >= 0.22f)
        {
            _ambulancePickupBrakePlayed = true;
            PlaySound(_ambulanceBrakeSound, 0.58f);
        }

        if (!_ambulancePickupDoorPlayed && progress >= 0.30f)
        {
            _ambulancePickupDoorPlayed = true;
            PlaySound(_ambulanceDoorSound, 0.68f);
        }

        if (!_playerInAmbulance && progress >= 0.30f)
        {
            _playerInAmbulance = true;
        }

        if (!_ambulanceDropoffBrakePlayed && progress >= 0.66f)
        {
            _ambulanceDropoffBrakePlayed = true;
            PlaySound(_ambulanceBrakeSound, 0.52f);
        }

        if (!_ambulanceDropoffDoorPlayed && progress >= 0.84f)
        {
            _ambulanceDropoffDoorPlayed = true;
            PlaySound(_ambulanceDoorSound, 0.62f);
        }

        if (!_deathRespawnCompleted && progress >= 0.84f)
        {
            _deathRespawnCompleted = true;
            _playerInAmbulance = false;
            ResetPlayerAtHospital(true);
        }

        if (_deathRespawnTimeRemaining > 0f) return;

        _deathRespawnPending = false;
        _playerInAmbulance = false;
    }

    private void UpdateDeathEffects(float elapsed)
    {
        for (var i = _deathEffects.Count - 1; i >= 0; i--)
        {
            var effect = _deathEffects[i];
            var timeRemaining = effect.TimeRemaining - elapsed;
            if (timeRemaining <= 0f)
            {
                _deathEffects.RemoveAt(i);
                continue;
            }

            _deathEffects[i] = effect with { TimeRemaining = timeRemaining };
        }
    }

    private void AddDeathEffect(Vector2 position, int seed)
    {
        _deathEffects.Add(new DeathEffect(position, DeathEffectSeconds, DeathEffectSeconds, seed));
    }

    private void DrawDeathEffects()
    {
        foreach (var effect in _deathEffects)
        {
            var progress = 1f - effect.TimeRemaining / effect.Duration;
            var fade = 1f - progress;
            DrawAmbulance(effect.Position, progress, fade);

            for (var i = 0; i < 8; i++)
            {
                var angle = (float)(i * Math.PI * 2d / 8d + effect.Seed * 0.31d);
                var direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                var distance = 18f + progress * (46f + i % 3 * 8f);
                var position = effect.Position + direction * distance;
                var width = Math.Max(3, (int)((10f - i % 3 * 2f) * fade));
                var height = Math.Max(2, width / 2);
                DrawRectangle(new Rectangle((int)position.X - width / 2, (int)position.Y - height / 2, width, height), WithAlpha(CurrentPalette.GemShine, (byte)(120f * fade)));
            }
        }
    }

    private void AddDeathStopLine(Vector2 position, Vector2 direction)
    {
        if (direction == Vector2.Zero)
        {
            direction = Vector2.UnitX;
        }

        direction.Normalize();
        _deathStopLines.Add(new DeathStopMark(position, direction));
    }

    private void DrawDeathStopLines()
    {
        foreach (var mark in _deathStopLines)
        {
            DrawDeathStopLine(mark.Position, mark.Direction, 1f);
        }
    }

    private void DrawDeathStopLine(Vector2 position, Vector2 direction, float fade)
    {
        if (direction == Vector2.Zero)
        {
            direction = Vector2.UnitX;
        }

        direction.Normalize();
        var perp = new Vector2(-direction.Y, direction.X);
        var alpha = (byte)(145f * fade);
        var fill = WithAlpha(CurrentPalette.HudBackground, alpha);
        var outline = WithAlpha(CurrentPalette.WallInner, (byte)(185f * fade));
        var dot = WithAlpha(CurrentPalette.HudInactive, (byte)(150f * fade));
        var center = position + direction * 8f;

        DrawLine(center - direction * 58f, center + direction * 58f, 48, outline);
        DrawLine(center - direction * 54f, center + direction * 54f, 40, fill);
        DrawLine(center - perp * 18f, center + perp * 18f, 34, WithAlpha(CurrentPalette.Background, (byte)(95f * fade)));
        DrawLine(center - perp * 17f, center + perp * 17f, 3, WithAlpha(CurrentPalette.WallOuter, (byte)(120f * fade)));

        for (var i = 0; i < 6; i++)
        {
            var along = -38f + i % 3 * 15f;
            var side = i / 3 == 0 ? -13f : 13f;
            DrawBandageDot(center + direction * along + perp * side, dot);
            DrawBandageDot(center - direction * along - perp * side, dot);
        }
    }

    private void DrawBandageDot(Vector2 position, Color color)
    {
        DrawRectangle(new Rectangle((int)position.X - 2, (int)position.Y - 2, 5, 5), color);
    }
    private void DrawRoundedRectangleApprox(Rectangle bounds, Color fill, Color outline, int radius)
    {
        DrawRectangle(new Rectangle(bounds.X + radius, bounds.Y, bounds.Width - radius * 2, bounds.Height), fill);
        DrawRectangle(new Rectangle(bounds.X, bounds.Y + radius, bounds.Width, bounds.Height - radius * 2), fill);

        DrawRectangle(new Rectangle(bounds.X + radius, bounds.Y, bounds.Width - radius * 2, 3), outline);
        DrawRectangle(new Rectangle(bounds.X + radius, bounds.Bottom - 3, bounds.Width - radius * 2, 3), outline);
        DrawRectangle(new Rectangle(bounds.X, bounds.Y + radius, 3, bounds.Height - radius * 2), outline);
        DrawRectangle(new Rectangle(bounds.Right - 3, bounds.Y + radius, 3, bounds.Height - radius * 2), outline);

        DrawRectangle(new Rectangle(bounds.X + 3, bounds.Y + 3, radius, 3), outline);
        DrawRectangle(new Rectangle(bounds.Right - radius - 3, bounds.Y + 3, radius, 3), outline);
        DrawRectangle(new Rectangle(bounds.X + 3, bounds.Bottom - 6, radius, 3), outline);
        DrawRectangle(new Rectangle(bounds.Right - radius - 3, bounds.Bottom - 6, radius, 3), outline);
    }

    private void DrawAmbulance(Vector2 target, float progress, float fade)
    {
        var pickup = new Vector2(target.X, target.Y - 42f);
        var dropoff = GetHospitalDropoffPoint();
        var ambulance = GetAmbulancePosition(progress, pickup, dropoff);
        var y = ambulance.Y + (float)Math.Sin(progress * Math.PI * 14f) * 2f;
        var body = new Rectangle((int)ambulance.X - 56, (int)y - 26, 112, 48);
        var cab = new Rectangle(body.Right - 34, body.Y + 8, 28, 30);
        var alpha = (byte)235;

        DrawRectangle(body, WithAlpha(CurrentPalette.GemShine, alpha));
        DrawFrame(body, WithAlpha(CurrentPalette.WallInner, alpha), 4);
        DrawRectangle(cab, WithAlpha(CurrentPalette.PlayerInner, alpha));
        DrawRectangle(new Rectangle(body.X + 16, body.Y + 14, 52, 12), WithAlpha(CurrentPalette.Hazard, alpha));
        DrawRectangle(new Rectangle(body.X + 18, body.Y + 30, 42, 7), WithAlpha(CurrentPalette.StageCurrent, alpha));
        DrawRectangle(new Rectangle(body.X + 18, body.Bottom - 8, 20, 10), WithAlpha(CurrentPalette.WallInner, alpha));
        DrawRectangle(new Rectangle(body.Right - 34, body.Bottom - 8, 20, 10), WithAlpha(CurrentPalette.WallInner, alpha));

        var lightOn = (int)(progress * 34f) % 2 == 0;
        var lightColor = lightOn ? CurrentPalette.Hazard : CurrentPalette.Gem;
        DrawRectangle(new Rectangle(body.X + 14, body.Y - 8, 22, 8), WithAlpha(lightColor, alpha));

        DrawAmbulancePassenger(target, pickup, dropoff, progress);
    }

    private static Vector2 GetAmbulancePosition(float progress, Vector2 pickup, Vector2 dropoff)
    {
        if (progress < 0.24f)
        {
            return new Vector2(MathHelper.Lerp(-170f, pickup.X, progress / 0.24f), pickup.Y);
        }

        if (progress < 0.36f)
        {
            return pickup;
        }

        if (progress < 0.50f)
        {
            return new Vector2(MathHelper.Lerp(pickup.X, VirtualWidth + 170f, (progress - 0.36f) / 0.14f), pickup.Y);
        }

        if (progress < 0.68f)
        {
            return new Vector2(MathHelper.Lerp(-170f, dropoff.X, (progress - 0.50f) / 0.18f), dropoff.Y);
        }

        if (progress < 0.84f)
        {
            return dropoff;
        }

        return new Vector2(MathHelper.Lerp(dropoff.X, VirtualWidth + 170f, (progress - 0.84f) / 0.16f), dropoff.Y);
    }

    private void DrawAmbulancePassenger(Vector2 missPosition, Vector2 pickup, Vector2 dropoff, float progress)
    {
        if (progress >= 0.30f && progress < 0.72f) return;

        Vector2 position;
        if (progress < 0.30f)
        {
            var lift = MathHelper.Clamp((progress - 0.24f) / 0.06f, 0f, 1f);
            position = Vector2.Lerp(missPosition - new Vector2(PlayerSize / 2f, PlayerSize / 2f), pickup - new Vector2(PlayerSize / 2f, 18f), lift);
        }
        else
        {
            var drop = MathHelper.Clamp((progress - 0.72f) / 0.12f, 0f, 1f);
            position = Vector2.Lerp(dropoff - new Vector2(PlayerSize / 2f, 18f), GetHospitalRespawnPosition(), drop);
        }

        var player = new Rectangle((int)position.X, (int)position.Y, PlayerSize, PlayerSize);
        DrawRectangle(player, CurrentPalette.PlayerInvincible);
        DrawRectangle(Inset(player, 10), CurrentPalette.PlayerInner);
    }

    private void CheckBusStop(float elapsed)
    {
        if (_busPassagePending) return;

        if (!GetPlayerBounds().Intersects(_busStopBounds))
        {
            _busStopWaitProgress = Math.Max(0f, _busStopWaitProgress - elapsed * 1.5f);
            return;
        }

        _busStopWaitProgress = Math.Min(BusStopWaitSeconds, _busStopWaitProgress + elapsed);
        if (_busStopWaitProgress < BusStopWaitSeconds) return;

        StartBusPassage();
    }

    private void StartBusPassage()
    {
        _busPassagePending = true;
        _playerInBus = true;
        _busPassageTimeRemaining = BusPassageSeconds;
        _busStopWaitProgress = BusStopWaitSeconds;
        PlaySound(_confirmSound, 0.72f);
    }

    private void UpdateBusPassage(float elapsed)
    {
        if (!_busPassagePending) return;

        _busPassageTimeRemaining = Math.Max(0f, _busPassageTimeRemaining - elapsed);
        if (_busPassageTimeRemaining > 0f) return;

        CompleteBusPassage();
    }

    private void CompleteBusPassage()
    {
        var playerCenter = _busStopBounds.Center.ToVector2();
        LogPlayEvent(PlayEventKind.Pass, _currentStageIndex, playerCenter, _currentStageElapsedSeconds, _stageGemCounts[_currentStageIndex]);
        PlaySound(_stageMoveSound, 0.72f);
        _stagesCleared[_currentStageIndex] = true;
        UpdateStageRecords();

        if (_currentStageIndex + 1 < _stages.Length)
        {
            LoadStage(_currentStageIndex + 1, true);
            return;
        }

        LogPlayEvent(PlayEventKind.FullClear, _currentStageIndex, playerCenter, _runElapsedSeconds, _deaths);
        _clearRank = CalculateClearRank();
        _cleared = true;
        _clearAnimationTime = 0d;
        _busPassagePending = false;
        _playerInBus = false;
    }


    private void UpdateBusArrival(float elapsed)
    {
        if (!_busArrivalPending) return;

        _busArrivalTimeRemaining = Math.Max(0f, _busArrivalTimeRemaining - elapsed);
        if (_busArrivalTimeRemaining > 0f) return;

        _busArrivalPending = false;
        _playerInBus = false;
        _playerPosition = GetBusArrivalDropoffPosition();
        _invincibleTimeRemaining = RespawnInvincibleSeconds;
    }

    private void StartBusArrival()
    {
        _busArrivalPending = true;
        _playerInBus = true;
        _busArrivalTimeRemaining = BusArrivalSeconds;
        _playerPosition = GetBusArrivalDropoffPosition();
        PlaySound(_stageMoveSound, 0.58f);
    }

    private void DrawBusArrival()
    {
        if (!_busArrivalPending) return;

        var progress = 1f - _busArrivalTimeRemaining / BusArrivalSeconds;
        var stop = GetBusStopPassengerPoint();
        var start = new Vector2(-150f, stop.Y);
        var position = progress < 0.62f
            ? Vector2.Lerp(start, stop, progress / 0.62f)
            : stop;

        DrawBus(position, progress);
        DrawBusArrivalPassenger(stop, progress);
    }

    private void DrawBusArrivalPassenger(Vector2 stop, float progress)
    {
        if (progress < 0.58f) return;

        var step = MathHelper.Clamp((progress - 0.58f) / 0.28f, 0f, 1f);
        var start = stop - new Vector2(PlayerSize / 2f, PlayerSize / 2f);
        var end = GetBusArrivalDropoffPosition();
        var position = Vector2.Lerp(start, end, step);
        var player = new Rectangle((int)position.X, (int)position.Y, PlayerSize, PlayerSize);
        DrawRectangle(player, CurrentPalette.PlayerInvincible);
        DrawRectangle(Inset(player, 10), CurrentPalette.PlayerInner);
    }
    private void DrawBusPassage()
    {
        if (!_busPassagePending) return;

        var progress = 1f - _busPassageTimeRemaining / BusPassageSeconds;
        var stop = _busStopBounds.Center.ToVector2();
        var start = new Vector2(-150f, stop.Y);
        var end = new Vector2(VirtualWidth + 150f, stop.Y);
        var position = progress < 0.45f
            ? Vector2.Lerp(start, stop, progress / 0.45f)
            : Vector2.Lerp(stop, end, (progress - 0.45f) / 0.55f);

        DrawBus(position, progress);
    }

    private void DrawBus(Vector2 position, float progress)
    {
        var bounce = (float)Math.Sin(progress * Math.PI * 16f) * 2f;
        var body = new Rectangle((int)position.X - 68, (int)(position.Y + bounce) - 28, 136, 56);
        DrawRectangle(body, CurrentPalette.StageCurrent);
        DrawFrame(body, CurrentPalette.GemShine, 4);
        DrawRectangle(new Rectangle(body.X + 14, body.Y + 10, 30, 18), CurrentPalette.PlayerInner);
        DrawRectangle(new Rectangle(body.X + 50, body.Y + 10, 30, 18), CurrentPalette.PlayerInner);
        DrawRectangle(new Rectangle(body.X + 86, body.Y + 10, 30, 18), CurrentPalette.PlayerInner);
        DrawRectangle(new Rectangle(body.X + 96, body.Y + 28, 16, 24), CurrentPalette.WallInner);
        DrawRectangle(new Rectangle(body.X + 20, body.Bottom - 8, 18, 18), CurrentPalette.WallInner);
        DrawRectangle(new Rectangle(body.Right - 38, body.Bottom - 8, 18, 18), CurrentPalette.WallInner);
    }
    private void CheckExit()
    {
        if (_exitOpen && _exitOpenFlashRemaining <= 0f && GetPlayerBounds().Intersects(GetExitBounds()))
        {
            LogPlayEvent(PlayEventKind.Clear, _currentStageIndex, GetPlayerBounds().Center.ToVector2(), _currentStageElapsedSeconds, _stageGemCounts[_currentStageIndex]);
            PlaySound(_clearSound);
            _stagesCleared[_currentStageIndex] = true;
            UpdateStageRecords();

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


    private void UpdateStageRecords()
    {
        if (_currentStageIndex < 0 || _currentStageIndex >= _stages.Length) return;

        AwardStageRecord(StageRecordKind.Pass, _stagePassRecords, true);
        AwardStageRecord(StageRecordKind.NoDamage, _stageNoDamageRecords, _stageDeathCounts.Length > _currentStageIndex && _stageDeathCounts[_currentStageIndex] == 0);
    }

    private void AwardStageRecord(StageRecordKind kind, bool[] records, bool achieved)
    {
        if (!achieved || records.Length <= _currentStageIndex || records[_currentStageIndex]) return;

        records[_currentStageIndex] = true;
        _badgeAwardEffects.Add(new BadgeAwardEffect(_currentStageIndex, kind, BadgeAwardEffectSeconds, BadgeAwardEffectSeconds));
    }

    private void UpdateBadgeAwardEffects(float elapsed)
    {
        for (var i = _badgeAwardEffects.Count - 1; i >= 0; i--)
        {
            var effect = _badgeAwardEffects[i];
            var timeRemaining = effect.TimeRemaining - elapsed;
            if (timeRemaining <= 0f)
            {
                _badgeAwardEffects.RemoveAt(i);
                continue;
            }

            _badgeAwardEffects[i] = effect with { TimeRemaining = timeRemaining };
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
        if (_stageElapsedSeconds.Length == 0) return;

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

    private void LoadStage(int stageIndex, bool arriveByBus = false)
    {
        _currentStageIndex = stageIndex;
        var stage = _stages[_currentStageIndex];
        _walls = stage.Walls;
        _ticketPieceBounds = stage.TicketPieces;
        _gemBounds = stage.Gems;
        _jetBounds = stage.Jets;
        _rollerBounds = stage.Rollers;
        _smallBounds = stage.Smalls;
        _hazards = (Hazard[])stage.Hazards.Clone();
        _ticketPiecesCollected = new bool[_ticketPieceBounds.Length];
        _gemsCollected = new bool[_gemBounds.Length];
        _jetsCollected = new bool[_jetBounds.Length];
        _rollersCollected = new bool[_rollerBounds.Length];
        _smallsCollected = new bool[_smallBounds.Length];
        _jetActive = false;
        _rollerActive = false;
        ClearRollerWallFollow();
        _smallActive = false;
        _stageStartNormalPulseRemaining = StageStartNormalPulseSeconds;
        _gemCollectEffects.Clear();
        _gemBagFullNudgeRemaining = 0f;
        _gemBagFullSoundCooldownRemaining = 0f;
        _deathEffects.Clear();
        _deathStopLines.Clear();
        _deathRespawnPending = false;
        _deathRespawnTimeRemaining = 0f;
        _playerInAmbulance = false;
        _deathRespawnCompleted = false;
        _ambulancePickupDoorPlayed = false;
        _ambulanceDropoffDoorPlayed = false;
        _ambulancePickupBrakePlayed = false;
        _ambulanceDropoffBrakePlayed = false;
        _playerStart = stage.PlayerStart;
        _exitBounds = stage.ExitBounds;
        _busStopBounds = stage.BusStopBounds;
        _hospitalBounds = stage.HospitalBounds;
        _backgroundColor = CurrentPalette.Background;
        _stageSelectOpen = false;
        _paused = false;
        _cleared = false;
        _exitOpen = false;
        _exitOpenDelayRemaining = 0f;
        _exitOpenFlashRemaining = 0f;
        _busStopWaitProgress = 0f;
        _busPassageTimeRemaining = 0f;
        _busArrivalTimeRemaining = 0f;
        _playerInBus = false;
        _busPassagePending = false;
        _busArrivalPending = false;
        _clearRank = 0;
        _clearAnimationTime = 0d;
        _currentStageElapsedSeconds = 0d;
        ResetPlayerOnly(false);
        if (arriveByBus)
        {
            StartBusArrival();
        }

        LogPlayEvent(PlayEventKind.StageStart, _currentStageIndex, GetPlayerBounds().Center.ToVector2(), 0d, _stageGemCounts[_currentStageIndex]);
        RefreshWindowTitle();
    }

    private void ResetPlayerOnly(bool grantInvincibility)
    {
        _playerPosition = _playerStart;
        _playerFacingDirection = Vector2.UnitX;
        _playerInputDirection = Vector2.Zero;
        _playerGhostVelocity = Vector2.Zero;
        _lastWallParallelContactDirection = Vector2.Zero;
        _lastWallParallelMoveDirection = Vector2.Zero;
        ClearBasicWallFollow();
        _inputContactWallIndex = -1;
        _lastMoveDirection = Vector2.UnitX;
        _invincibleTimeRemaining = grantInvincibility ? RespawnInvincibleSeconds : 0f;
    }

    private void ResetPlayerAtHospital(bool grantInvincibility)
    {
        _playerPosition = GetHospitalRespawnPosition();
        _playerFacingDirection = Vector2.UnitX;
        _playerInputDirection = Vector2.Zero;
        _playerGhostVelocity = Vector2.Zero;
        _lastWallParallelContactDirection = Vector2.Zero;
        _lastWallParallelMoveDirection = Vector2.Zero;
        ClearBasicWallFollow();
        _inputContactWallIndex = -1;
        _lastMoveDirection = Vector2.UnitX;
        _invincibleTimeRemaining = grantInvincibility ? RespawnInvincibleSeconds : 0f;
    }

    private Vector2 GetHospitalRespawnPosition() => new(
        _hospitalBounds.Center.X - PlayerSize / 2f,
        _hospitalBounds.Bottom - PlayerSize - 14f);

    private Vector2 GetHospitalDropoffPoint() => new(_hospitalBounds.Center.X, _hospitalBounds.Bottom + 42f);

    private Vector2 GetBusStopPassengerPoint() => new(_busStopBounds.Center.X, _busStopBounds.Bottom + 22f);

    private Vector2 GetBusArrivalDropoffPosition() => new(
        _busStopBounds.Center.X - PlayerSize / 2f,
        _busStopBounds.Bottom + 28f);

    /// <summary>
    /// ウィンドウ・タイトルを更新します。
    /// </summary>
    /// <param name="gameTime">ゲーム時間の情報</param>
    private void UpdateWindowTitle(GameTime gameTime)
    {
        _titleRefreshTimer -= gameTime.ElapsedGameTime.TotalSeconds;
        if (_titleRefreshTimer > 0) return;

        RefreshWindowTitle();
    }

    /// <summary>
    /// ウィンドウ・タイトルを最新表示します。
    /// </summary>
    private void RefreshWindowTitle()
    {
        _titleRefreshTimer = 0.2;
        var collected = CountCollectedGemShards();

        var state = _stageSelectOpen ? "STAGE SELECT - Left/Right choose - Enter/Space/Start play" : _paused ? "PAUSE - Left/Right choose - Enter/Space/Start/A select" : _cleared ? "CLEAR - Enter/Space/Start/A stage select - R retry" : "Collect ticket pieces, then reach the green exit - Start/Enter pause - Tab for stage select";
        var stage = _stages[_currentStageIndex];
        var capacity = GetCurrentGemBagCapacity();
        var bagState = capacity > 0 && collected >= capacity ? "Bag full" : $"Bag {Math.Min(collected, capacity)}/{capacity} shards";
        Window.Title = $"SkylarkBimbleStreet - {stage.Name} - Palette {CurrentPalette.Name} - {state} - {bagState} - Hits {_deaths} - {GetStatsSummary()}";
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

    private static Vector2 GetCardinalDirection(Vector2 direction)
    {
        if (MathF.Abs(direction.X) >= MathF.Abs(direction.Y))
        {
            return direction.X >= 0f ? Vector2.UnitX : -Vector2.UnitX;
        }

        return direction.Y >= 0f ? Vector2.UnitY : -Vector2.UnitY;
    }

    /// <summary>
    /// プレイヤーの当たり判定を取得します。
    /// </summary>
    /// <returns>プレイヤーの当たり判定を表す矩形</returns>
    private int GetCurrentPlayerSize() => _smallActive ? SmallPlayerSize : PlayerSize;

    private Rectangle GetPlayerBounds()
    {
        var size = GetCurrentPlayerSize();
        return new Rectangle((int)_playerPosition.X, (int)_playerPosition.Y, size, size);
    }

    /// <summary>
    /// 出口の当たり判定を取得します。
    /// </summary>
    /// <returns>出口の当たり判定を表す矩形</returns>
    private Rectangle GetExitBounds() => _exitBounds;

    private int CountCollectedGemShards()
    {
        var collected = 0;
        for (var i = 0; i < _gemsCollected.Length; i++)
        {
            if (_gemsCollected[i])
            {
                collected += GetGemShardValue(_gemBounds[i]);
            }
        }

        return collected;
    }

    private int GetCurrentGemBagCapacity() => _currentStageIndex >= 0 && _currentStageIndex < _stages.Length ? _stages[_currentStageIndex].GemBagCapacity : 0;

    private static int GetGemShardValue(Rectangle gem)
    {
        var widthUnits = Math.Max(1, (int)MathF.Round(gem.Width / (float)GemShardPixelSize));
        var heightUnits = Math.Max(1, (int)MathF.Round(gem.Height / (float)GemShardPixelSize));
        return widthUnits * heightUnits;
    }

    private bool AreAllTicketPiecesCollected()
    {
        foreach (var ticketPieceCollected in _ticketPiecesCollected)
        {
            if (!ticketPieceCollected)
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
    private Matrix GetPlayfieldTransform()
    {
        var scale = (float)(VirtualHeight - HudBandHeight) / VirtualHeight;
        var scaledWidth = VirtualWidth * scale;
        var offsetX = (VirtualWidth - scaledWidth) * 0.5f;
        return Matrix.CreateScale(scale, scale, 1f) * Matrix.CreateTranslation(offsetX, HudBandHeight, 0f);
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
    private static Color WithAlpha(Color color, byte alpha) => new(color.R, color.G, color.B, alpha);

    private static Rectangle Inset(Rectangle rectangle, int inset) => new(
        rectangle.X + inset,
        rectangle.Y + inset,
        Math.Max(1, rectangle.Width - inset * 2),
        Math.Max(1, rectangle.Height - inset * 2));

    private static Rectangle MapStageRectangle(Rectangle source, Rectangle map)
    {
        var scaleX = map.Width / (float)VirtualWidth;
        var scaleY = map.Height / (float)VirtualHeight;
        return new Rectangle(
            map.X + (int)(source.X * scaleX),
            map.Y + (int)(source.Y * scaleY),
            Math.Max(1, (int)Math.Ceiling(source.Width * scaleX)),
            Math.Max(1, (int)Math.Ceiling(source.Height * scaleY)));
    }

    private void CreateSoundEffects()
    {
        _gemSound = CreateGemCollectSound();
        _gemBagFullSound = CreateTone(260f, 190f, 0.11f, 0.16f, WaveShape.Triangle);
        _deathSound = CreateTone(170f, 74f, 0.18f, 0.34f, WaveShape.Square);
        _clearSound = CreateArpeggio([660f, 880f, 1175f, 1568f], 0.07f, 0.28f);
        _exitOpenSound = CreateArpeggio([523f, 659f, 784f, 1047f, 1319f], 0.055f, 0.24f);
        _stageMoveSounds = [
            CreateTone(430f, 360f, 0.045f, 0.18f, WaveShape.Square),
            CreateTone(620f, 510f, 0.052f, 0.16f, WaveShape.Triangle),
            CreateArpeggio([520f, 695f], 0.032f, 0.16f),
        ];
        _stageMoveSound = _stageMoveSounds[_stageMoveSoundIndex];
        _confirmSound = CreateTone(540f, 760f, 0.075f, 0.24f, WaveShape.Sine);
        _pauseSound = CreateTone(260f, 410f, 0.08f, 0.22f, WaveShape.Triangle);
        _ambulanceSirenSound = CreateAmbulanceSirenSound();
        _ambulanceDoorSound = CreateAmbulanceDoorSound();
        _ambulanceBrakeSound = CreateAmbulanceBrakeSound();
        _soundTestSounds = [_gemSound, _deathSound, _clearSound, _exitOpenSound, _stageMoveSounds[0], _stageMoveSounds[1], _stageMoveSounds[2], _confirmSound, _pauseSound, _ambulanceSirenSound, _ambulanceDoorSound, _ambulanceBrakeSound];
    }

    private void DisposeSoundEffects()
    {
        _gemSound.Dispose();
        _gemBagFullSound.Dispose();
        _deathSound.Dispose();
        _clearSound.Dispose();
        _exitOpenSound.Dispose();
        foreach (var stageMoveSound in _stageMoveSounds)
        {
            stageMoveSound.Dispose();
        }

        _confirmSound.Dispose();
        _pauseSound.Dispose();
        _ambulanceSirenSound.Dispose();
        _ambulanceDoorSound.Dispose();
        _ambulanceBrakeSound.Dispose();
    }

    private static void PlaySound(SoundEffect sound, float volume = 1f)
    {
        sound.Play(volume, 0f, 0f);
    }


    private void SelectStageMoveSound(int variantIndex)
    {
        if (_stageMoveSounds.Length == 0) return;

        _stageMoveSoundIndex = Math.Clamp(variantIndex, 0, Math.Min(StageMoveSoundVariantCount, _stageMoveSounds.Length) - 1);
        _stageMoveSound = _stageMoveSounds[_stageMoveSoundIndex];
        PlayStageMoveSound();
    }

    private void PlayStageMoveSound()
    {
        PlaySound(_stageMoveSound);
    }
    private static SoundEffect CreateAmbulanceSirenSound()
    {
        const int sampleRate = 44100;
        const float seconds = 2.75f;
        var sampleCount = (int)(sampleRate * seconds);
        var samples = new float[sampleCount];

        for (var i = 0; i < 5; i++)
        {
            var start = i * 0.52f;
            AddToneBurst(samples, sampleRate, start, 0.22f, 880f, 1160f, 0.20f, WaveShape.Sine);
            AddToneBurst(samples, sampleRate, start + 0.23f, 0.24f, 650f, 520f, 0.22f, WaveShape.Triangle);
        }

        var buffer = new byte[sampleCount * 2];
        for (var i = 0; i < sampleCount; i++)
        {
            WriteSample(buffer, i, samples[i]);
        }

        return new SoundEffect(buffer, sampleRate, AudioChannels.Mono);
    }

    private static SoundEffect CreateAmbulanceDoorSound()
    {
        const int sampleRate = 44100;
        const float seconds = 0.30f;
        var sampleCount = (int)(sampleRate * seconds);
        var samples = new float[sampleCount];

        AddToneBurst(samples, sampleRate, 0.000f, 0.070f, 132f, 58f, 0.55f, WaveShape.Square);
        AddToneBurst(samples, sampleRate, 0.030f, 0.120f, 78f, 42f, 0.42f, WaveShape.Triangle);
        AddToneBurst(samples, sampleRate, 0.115f, 0.090f, 62f, 36f, 0.24f, WaveShape.Square);
        AddNoiseBurst(samples, sampleRate, 0.010f, 0.115f, 0.22f, 41);
        AddNoiseBurst(samples, sampleRate, 0.105f, 0.065f, 0.12f, 73);

        var buffer = new byte[sampleCount * 2];
        for (var i = 0; i < sampleCount; i++)
        {
            WriteSample(buffer, i, samples[i]);
        }

        return new SoundEffect(buffer, sampleRate, AudioChannels.Mono);
    }

    private static SoundEffect CreateAmbulanceBrakeSound()
    {
        const int sampleRate = 44100;
        const float seconds = 0.30f;
        var sampleCount = (int)(sampleRate * seconds);
        var samples = new float[sampleCount];

        AddToneBurst(samples, sampleRate, 0.000f, 0.175f, 1760f, 720f, 0.18f, WaveShape.Square);
        AddToneBurst(samples, sampleRate, 0.055f, 0.150f, 1180f, 520f, 0.12f, WaveShape.Sine);
        AddNoiseBurst(samples, sampleRate, 0.000f, 0.220f, 0.10f, 97);
        AddNoiseBurst(samples, sampleRate, 0.170f, 0.085f, 0.07f, 131);

        var buffer = new byte[sampleCount * 2];
        for (var i = 0; i < sampleCount; i++)
        {
            WriteSample(buffer, i, samples[i]);
        }

        return new SoundEffect(buffer, sampleRate, AudioChannels.Mono);
    }
    private static SoundEffect CreateGemCollectSound()
    {
        const int sampleRate = 44100;
        const float seconds = 0.26f;
        var sampleCount = (int)(sampleRate * seconds);
        var samples = new float[sampleCount];

        AddToneBurst(samples, sampleRate, 0.00f, 0.10f, 1080f, 1580f, 0.30f, WaveShape.Sine);
        AddToneBurst(samples, sampleRate, 0.075f, 0.045f, 920f, 780f, 0.16f, WaveShape.Triangle);
        AddToneBurst(samples, sampleRate, 0.112f, 0.040f, 760f, 640f, 0.13f, WaveShape.Triangle);
        AddToneBurst(samples, sampleRate, 0.150f, 0.038f, 610f, 520f, 0.11f, WaveShape.Triangle);
        AddToneBurst(samples, sampleRate, 0.190f, 0.034f, 490f, 420f, 0.09f, WaveShape.Square);

        var buffer = new byte[sampleCount * 2];
        for (var i = 0; i < sampleCount; i++)
        {
            WriteSample(buffer, i, samples[i]);
        }

        return new SoundEffect(buffer, sampleRate, AudioChannels.Mono);
    }

    private static void AddToneBurst(float[] samples, int sampleRate, float startSeconds, float seconds, float startFrequency, float endFrequency, float volume, WaveShape shape)
    {
        var startSample = Math.Clamp((int)(startSeconds * sampleRate), 0, samples.Length);
        var sampleCount = Math.Max(1, (int)(seconds * sampleRate));
        var endSample = Math.Min(samples.Length, startSample + sampleCount);
        var phase = 0d;

        for (var i = startSample; i < endSample; i++)
        {
            var t = (i - startSample) / (float)Math.Max(1, sampleCount - 1);
            var frequency = MathHelper.Lerp(startFrequency, endFrequency, t);
            phase += frequency / sampleRate;
            phase -= Math.Floor(phase);
            samples[i] += GetWaveSample(phase, shape) * GetEnvelope(t) * volume;
        }
    }
    private static void AddNoiseBurst(float[] samples, int sampleRate, float startSeconds, float seconds, float volume, int seed)
    {
        var random = new Random(seed);
        var startSample = Math.Clamp((int)(startSeconds * sampleRate), 0, samples.Length);
        var sampleCount = Math.Max(1, (int)(seconds * sampleRate));
        var endSample = Math.Min(samples.Length, startSample + sampleCount);

        for (var i = startSample; i < endSample; i++)
        {
            var t = (i - startSample) / (float)Math.Max(1, sampleCount - 1);
            var scrape = (float)(random.NextDouble() * 2d - 1d);
            samples[i] += scrape * GetEnvelope(t) * volume;
        }
    }
    private static SoundEffect CreateTone(float startFrequency, float endFrequency, float seconds, float volume, WaveShape shape)
    {
        const int sampleRate = 44100;
        var sampleCount = Math.Max(1, (int)(sampleRate * seconds));
        var buffer = new byte[sampleCount * 2];
        var phase = 0d;

        for (var i = 0; i < sampleCount; i++)
        {
            var t = i / (float)Math.Max(1, sampleCount - 1);
            var frequency = MathHelper.Lerp(startFrequency, endFrequency, t);
            phase += frequency / sampleRate;
            phase -= Math.Floor(phase);
            var envelope = GetEnvelope(t);
            var sample = GetWaveSample(phase, shape) * envelope * volume;
            WriteSample(buffer, i, sample);
        }

        return new SoundEffect(buffer, sampleRate, AudioChannels.Mono);
    }

    private static SoundEffect CreateArpeggio(float[] frequencies, float noteSeconds, float volume)
    {
        const int sampleRate = 44100;
        var samplesPerNote = Math.Max(1, (int)(sampleRate * noteSeconds));
        var sampleCount = samplesPerNote * frequencies.Length;
        var buffer = new byte[sampleCount * 2];
        var phase = 0d;

        for (var i = 0; i < sampleCount; i++)
        {
            var noteIndex = Math.Min(frequencies.Length - 1, i / samplesPerNote);
            var noteSample = i % samplesPerNote;
            var t = noteSample / (float)Math.Max(1, samplesPerNote - 1);
            phase += frequencies[noteIndex] / sampleRate;
            phase -= Math.Floor(phase);
            var sample = GetWaveSample(phase, WaveShape.Sine) * GetEnvelope(t) * volume;
            WriteSample(buffer, i, sample);
        }

        return new SoundEffect(buffer, sampleRate, AudioChannels.Mono);
    }

    private static float GetEnvelope(float t)
    {
        var attack = MathHelper.Clamp(t / 0.08f, 0f, 1f);
        var release = MathHelper.Clamp((1f - t) / 0.18f, 0f, 1f);
        return attack * release;
    }

    private static float GetWaveSample(double phase, WaveShape shape) => shape switch
    {
        WaveShape.Square => phase < 0.5d ? 0.72f : -0.72f,
        WaveShape.Triangle => (float)(1d - 4d * Math.Abs(phase - 0.5d)),
        _ => (float)Math.Sin(phase * Math.PI * 2d),
    };

    private static void WriteSample(byte[] buffer, int sampleIndex, float sample)
    {
        var value = (short)(MathHelper.Clamp(sample, -1f, 1f) * short.MaxValue);
        var offset = sampleIndex * 2;
        buffer[offset] = (byte)(value & 0xff);
        buffer[offset + 1] = (byte)((value >> 8) & 0xff);
    }
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

    private readonly record struct GemCollectEffect(Vector2 Position, float TimeRemaining, float Duration, int Seed);
    private readonly record struct DeathEffect(Vector2 Position, float TimeRemaining, float Duration, int Seed);
    private readonly record struct DeathStopMark(Vector2 Position, Vector2 Direction);
    private readonly record struct BadgeAwardEffect(int StageIndex, StageRecordKind Kind, float TimeRemaining, float Duration);

    private enum WallContactSide
    {
        None,
        Top,
        Right,
        Bottom,
        Left,
    }

    private sealed class WallContact
    {
        public static readonly WallContact None = new(-1, WallContactSide.None);

        public WallContact(int wallIndex, WallContactSide side)
        {
            WallIndex = wallIndex;
            Side = side;
        }

        public int WallIndex { get; }

        public WallContactSide Side { get; }

        public bool IsValid(int wallCount) => WallIndex >= 0 && WallIndex < wallCount && Side != WallContactSide.None;
    }

    private enum WaveShape
    {
        Sine,
        Square,
        Triangle,
    }

    private enum StageRecordKind
    {
        Pass,
        NoDamage,
    }

    private enum PlayEventKind
    {
        StageStart,
        TicketPiece,
        Gem,
        Jet,
        Roller,
        Small,
        Death,
        Clear,
        FullClear,
        Pause,
        Retry,
        StageSelect,
        Pass,
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
}
