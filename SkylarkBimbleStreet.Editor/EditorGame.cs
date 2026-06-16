namespace SkylarkBimbleStreet.Editor;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

internal sealed class EditorGame : Game
{
    private const int VirtualWidth = 1920;
    private const int VirtualHeight = 1080;
    private const int HandleSize = 16;
    private const int MinItemSize = 8;
    private const int ToolButtonSize = 30;
    private const int ToolButtonGap = 8;
    private const int SnapGridSize = 40;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly GraphicsDeviceManager _graphics;
    private readonly List<EditableItem> _items = [];
    private SpriteBatch _spriteBatch = null!;
    private Texture2D _pixel = null!;
    private FileInfo[] _stageFiles = [];
    private StageData _stage = null!;
    private int _stageIndex;
    private int _selectedIndex = -1;
    private MouseState _previousMouse;
    private KeyboardState _previousKeyboard;
    private bool _dragging;
    private bool _snapToGrid;
    private DragMode _dragMode;
    private AddTool _addTool = AddTool.None;
    private Point _dragOffset;
    private string _status = "Ready";

    public EditorGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.SynchronizeWithVerticalRetrace = true;
        Window.AllowUserResizing = true;
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        LoadStageFiles();
        LoadStage(0);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
    }

    protected override void UnloadContent()
    {
        _pixel.Dispose();
        base.UnloadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var mouse = Mouse.GetState();

        if (keyboard.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        if (WasPressed(keyboard, Keys.PageUp))
        {
            LoadStage(Math.Max(0, _stageIndex - 1));
        }
        else if (WasPressed(keyboard, Keys.PageDown))
        {
            LoadStage(Math.Min(_stageFiles.Length - 1, _stageIndex + 1));
        }
        else if (WasPressed(keyboard, Keys.S))
        {
            SaveStage();
        }
        else if (WasPressed(keyboard, Keys.R))
        {
            LoadStage(_stageIndex);
        }
        else if (WasPressed(keyboard, Keys.G))
        {
            _snapToGrid = !_snapToGrid;
            _status = _snapToGrid ? $"Snap {SnapGridSize}px on" : "Snap off";
        }
        else if (WasPressed(keyboard, Keys.Delete))
        {
            DeleteSelectedItem();
        }
        else if (WasPressed(keyboard, Keys.T))
        {
            ToggleSelectedItemKind();
        }
        else if (WasPressed(keyboard, Keys.D1))
        {
            AddWall(mouse.Position);
        }
        else if (WasPressed(keyboard, Keys.D2))
        {
            AddItem(mouse.Position, "gem");
        }
        else if (WasPressed(keyboard, Keys.D3))
        {
            AddItem(mouse.Position, "ticketPiece");
        }
        else if (WasPressed(keyboard, Keys.D4))
        {
            AddHazard(mouse.Position, horizontal: false);
        }
        else if (WasPressed(keyboard, Keys.D5))
        {
            AddHazard(mouse.Position, horizontal: true);
        }
        else if (WasPressed(keyboard, Keys.A))
        {
            AdjustSelectedHazardRangeStart(-EditStep());
        }
        else if (WasPressed(keyboard, Keys.D))
        {
            AdjustSelectedHazardRangeStart(EditStep());
        }
        else if (WasPressed(keyboard, Keys.J))
        {
            AdjustSelectedHazardRangeEnd(-EditStep());
        }
        else if (WasPressed(keyboard, Keys.L))
        {
            AdjustSelectedHazardRangeEnd(EditStep());
        }

        UpdateMouse(mouse);
        UpdateWindowTitle();
        _previousKeyboard = keyboard;
        _previousMouse = mouse;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(14, 16, 20));
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        var map = GetMapRectangle();
        DrawRectangle(map, new Color(22, 25, 31));
        DrawGrid(map);
        DrawStage(map);
        DrawAddToolbar(map);
        DrawFrame(map, new Color(150, 160, 174), 2);

        _spriteBatch.End();
        base.Draw(gameTime);
    }

    private void UpdateMouse(MouseState mouse)
    {
        var map = GetMapRectangle();
        var world = ScreenToWorld(mouse.Position, map);
        var leftPressed = mouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released;
        var leftReleased = mouse.LeftButton == ButtonState.Released && _previousMouse.LeftButton == ButtonState.Pressed;

        if (leftPressed && TrySelectAddTool(mouse.Position, map))
        {
            return;
        }

        if (leftPressed && map.Contains(mouse.Position))
        {
            var resizeHandlePressed = IsSelectedResizeHandle(mouse.Position, map);
            if (!resizeHandlePressed)
            {
                _selectedIndex = FindItemAt(world);
            }

            if (_selectedIndex < 0 && _addTool != AddTool.None)
            {
                AddSelectedTool(mouse.Position);
                return;
            }

            if (_selectedIndex >= 0)
            {
                var bounds = _items[_selectedIndex].GetBounds();
                _dragging = true;
                _dragMode = resizeHandlePressed ? DragMode.Resize : DragMode.Move;
                _dragOffset = new Point(world.X - bounds.X, world.Y - bounds.Y);
                _status = _dragMode == DragMode.Resize
                    ? $"Resizing {_items[_selectedIndex].Kind}"
                    : $"Selected {_items[_selectedIndex].Kind}";
            }
            else
            {
                _dragging = false;
                _dragMode = DragMode.None;
                _status = "No selection";
            }
        }

        if (leftReleased)
        {
            _dragging = false;
            _dragMode = DragMode.None;
        }

        if (!_dragging || _selectedIndex < 0 || mouse.LeftButton != ButtonState.Pressed)
        {
            return;
        }

        var item = _items[_selectedIndex];
        var draggedBounds = item.GetBounds();
        if (_dragMode == DragMode.Resize)
        {
            ResizeSelectedItem(item, draggedBounds, world);
            return;
        }

        draggedBounds.X = Math.Clamp(world.X - _dragOffset.X, 0, VirtualWidth - draggedBounds.Width);
        draggedBounds.Y = Math.Clamp(world.Y - _dragOffset.Y, 0, VirtualHeight - draggedBounds.Height);
        if (_snapToGrid)
        {
            draggedBounds.X = Math.Clamp(Snap(draggedBounds.X), 0, VirtualWidth - draggedBounds.Width);
            draggedBounds.Y = Math.Clamp(Snap(draggedBounds.Y), 0, VirtualHeight - draggedBounds.Height);
        }

        item.SetBounds(draggedBounds);
    }

    private void ResizeSelectedItem(EditableItem item, Rectangle bounds, Point world)
    {
        var right = Math.Clamp(world.X, bounds.X + MinItemSize, VirtualWidth);
        var bottom = Math.Clamp(world.Y, bounds.Y + MinItemSize, VirtualHeight);
        if (_snapToGrid)
        {
            right = Math.Clamp(Snap(right), bounds.X + MinItemSize, VirtualWidth);
            bottom = Math.Clamp(Snap(bottom), bounds.Y + MinItemSize, VirtualHeight);
        }

        bounds.Width = right - bounds.X;
        bounds.Height = bottom - bounds.Y;
        item.SetBounds(bounds);
    }

    private void LoadStageFiles()
    {
        var stagesDirectory = FindStagesDirectory();
        _stageFiles = stagesDirectory.GetFiles("stage-*.json").OrderBy(static file => file.Name).ToArray();
        if (_stageFiles.Length == 0)
        {
            throw new InvalidOperationException($"No stage-*.json files in {stagesDirectory.FullName}.");
        }
    }

    private void LoadStage(int index)
    {
        _stageIndex = Math.Clamp(index, 0, _stageFiles.Length - 1);
        var json = File.ReadAllText(_stageFiles[_stageIndex].FullName);
        _stage = JsonSerializer.Deserialize<StageData>(json, JsonOptions)
            ?? throw new InvalidOperationException($"JSON root is empty: {_stageFiles[_stageIndex].FullName}");
        NormalizeItems();
        RebuildItems();
        _selectedIndex = -1;
        _dragging = false;
        _status = "Loaded";
    }

    private void NormalizeItems()
    {
        if (_stage.Items is not null)
        {
            _stage.Collectibles = null!;
            return;
        }

        var collectibles = _stage.Collectibles ?? [];
        var ticketPieceIndexes = ChooseTicketPieceIndexes(collectibles.Length);
        _stage.Items = new ItemData[collectibles.Length];
        for (var i = 0; i < collectibles.Length; i++)
        {
            _stage.Items[i] = new ItemData
            {
                Kind = ticketPieceIndexes.Contains(i) ? "ticketPiece" : "gem",
                Bounds = collectibles[i],
            };
        }

        _stage.Collectibles = null!;
    }

    private void SaveStage()
    {
        CreateBackup();
        var json = JsonSerializer.Serialize(_stage, JsonOptions).Replace("\n", "\r\n");
        File.WriteAllText(_stageFiles[_stageIndex].FullName, json + "\r\n", new System.Text.UTF8Encoding(false));
        _status = "Saved";
    }

    private void CreateBackup()
    {
        var stageFile = _stageFiles[_stageIndex];
        var backupDirectory = Path.Combine(stageFile.DirectoryName ?? ".", "Backups");
        Directory.CreateDirectory(backupDirectory);

        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss-fff");
        var backupFile = Path.Combine(backupDirectory, $"{Path.GetFileNameWithoutExtension(stageFile.Name)}-{timestamp}.json");
        File.Copy(stageFile.FullName, backupFile, overwrite: false);
    }

    private void DeleteSelectedItem()
    {
        if (_selectedIndex < 0)
        {
            _status = "No selection";
            return;
        }

        var item = _items[_selectedIndex];
        if (!item.CanDelete)
        {
            _status = $"Cannot delete {item.Kind}";
            return;
        }

        var nextSelection = Math.Min(_selectedIndex, _items.Count - 2);
        item.Delete();
        RebuildItems();
        _selectedIndex = Math.Clamp(nextSelection, -1, _items.Count - 1);
        _dragging = false;
        _status = $"Deleted {item.Kind}";
    }

    private void ToggleSelectedItemKind()
    {
        if (_selectedIndex < 0)
        {
            _status = "No selection";
            return;
        }

        var item = _items[_selectedIndex];
        if (!item.CanToggleKind)
        {
            _status = $"Cannot change {item.Kind}";
            return;
        }

        item.ToggleKind();
        RebuildItems();
        _selectedIndex = Math.Clamp(_selectedIndex, -1, _items.Count - 1);
        _dragging = false;
        _status = $"Changed to {_items[_selectedIndex].Kind}";
    }


    private void AddSelectedTool(Point screenPosition)
    {
        switch (_addTool)
        {
            case AddTool.Wall:
                AddWall(screenPosition);
                return;
            case AddTool.Gem:
                AddItem(screenPosition, "gem");
                return;
            case AddTool.TicketPiece:
                AddItem(screenPosition, "ticketPiece");
                return;
            case AddTool.HazardVertical:
                AddHazard(screenPosition, horizontal: false);
                return;
            case AddTool.HazardHorizontal:
                AddHazard(screenPosition, horizontal: true);
                return;
            default:
                return;
        }
    }

    private void AddWall(Point screenPosition)
    {
        var bounds = NewBounds(screenPosition, 160, 38);
        _stage.Walls = AppendItem(_stage.Walls, FromRectangle(bounds));
        RebuildItems();
        _selectedIndex = 4 + _stage.Walls.Length - 1;
        _status = "Added wall";
    }

    private void AddItem(Point screenPosition, string kind)
    {
        var item = new ItemData
        {
            Kind = kind,
            Bounds = FromRectangle(NewBounds(screenPosition, 34, 34)),
        };
        _stage.Items = AppendItem(_stage.Items, item);
        RebuildItems();
        _selectedIndex = 4 + _stage.Walls.Length + _stage.Items.Length - 1;
        _status = $"Added {GetItemDisplayName(item)}";
    }

    private void AddHazard(Point screenPosition, bool horizontal)
    {
        var bounds = NewBounds(screenPosition, 64, 64);
        var hazard = new HazardData
        {
            Bounds = FromRectangle(bounds),
            Velocity = horizontal ? new Vector2Data { X = 250, Y = 0 } : new Vector2Data { X = 0, Y = 250 },
            Min = horizontal
                ? Math.Clamp(bounds.X - 120, 0, VirtualWidth - bounds.Width)
                : Math.Clamp(bounds.Y - 120, 0, VirtualHeight - bounds.Height),
            Max = horizontal
                ? Math.Clamp(bounds.X + 120, 0, VirtualWidth - bounds.Width)
                : Math.Clamp(bounds.Y + 120, 0, VirtualHeight - bounds.Height),
        };
        if (hazard.Max < hazard.Min)
        {
            hazard.Max = hazard.Min;
        }

        _stage.Hazards = AppendItem(_stage.Hazards, hazard);
        RebuildItems();
        _selectedIndex = 4 + _stage.Walls.Length + _stage.Items.Length + _stage.Hazards.Length - 1;
        _status = horizontal ? "Added horizontal hazard" : "Added vertical hazard";
    }

    private void AdjustSelectedHazardRangeStart(int delta)
    {
        var hazard = GetSelectedHazard();
        if (hazard is null)
        {
            _status = "Select hazard first";
            return;
        }

        var bounds = ToRectangle(hazard.Bounds);
        var limit = GetHazardRangeLimit(hazard, bounds);
        hazard.Min = Math.Clamp(hazard.Min + delta, 0, Math.Min(hazard.Max, limit));
        _status = $"Hazard range min {hazard.Min}";
    }

    private void AdjustSelectedHazardRangeEnd(int delta)
    {
        var hazard = GetSelectedHazard();
        if (hazard is null)
        {
            _status = "Select hazard first";
            return;
        }

        var bounds = ToRectangle(hazard.Bounds);
        var limit = GetHazardRangeLimit(hazard, bounds);
        hazard.Max = Math.Clamp(hazard.Max + delta, Math.Max(0, hazard.Min), limit);
        _status = $"Hazard range max {hazard.Max}";
    }

    private HazardData GetSelectedHazard()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _items.Count || _items[_selectedIndex].Kind != "hazard")
        {
            return null;
        }

        var hazardIndex = _selectedIndex - 4 - _stage.Walls.Length - _stage.Items.Length;
        if (hazardIndex < 0 || hazardIndex >= _stage.Hazards.Length)
        {
            return null;
        }

        return _stage.Hazards[hazardIndex];
    }

    private int EditStep() => _snapToGrid ? SnapGridSize : 10;

    private static int GetHazardRangeLimit(HazardData hazard, Rectangle bounds) => IsHorizontalHazard(hazard) ? VirtualWidth - bounds.Width : VirtualHeight - bounds.Height;

    private Rectangle NewBounds(Point screenPosition, int width, int height)
    {
        var map = GetMapRectangle();
        var world = map.Contains(screenPosition) ? ScreenToWorld(screenPosition, map) : new Point(VirtualWidth / 2, VirtualHeight / 2);
        var x = world.X - width / 2;
        var y = world.Y - height / 2;
        if (_snapToGrid)
        {
            x = Snap(x);
            y = Snap(y);
        }

        return new Rectangle(
            Math.Clamp(x, 0, VirtualWidth - width),
            Math.Clamp(y, 0, VirtualHeight - height),
            width,
            height);
    }

    private void RebuildItems()
    {
        _items.Clear();
        _items.Add(new EditableItem("player start", () => CenteredBounds(_stage.PlayerStart, 46, 46), bounds => SetCenter(_stage.PlayerStart, bounds)));
        _items.Add(new EditableItem("exit", () => ToRectangle(_stage.ExitBounds), bounds => FromRectangle(_stage.ExitBounds, bounds)));
        _items.Add(new EditableItem("bus stop", () => ToRectangle(_stage.BusStopBounds), bounds => FromRectangle(_stage.BusStopBounds, bounds)));
        _items.Add(new EditableItem("hospital", () => ToRectangle(_stage.HospitalBounds), bounds => FromRectangle(_stage.HospitalBounds, bounds)));

        foreach (var wall in _stage.Walls)
        {
            _items.Add(new EditableItem("wall", () => ToRectangle(wall), bounds => FromRectangle(wall, bounds), () => _stage.Walls = RemoveItem(_stage.Walls, wall)));
        }

        foreach (var item in _stage.Items)
        {
            _items.Add(new EditableItem(
                GetItemDisplayName(item),
                () => ToRectangle(item.Bounds),
                bounds => FromRectangle(item.Bounds, bounds),
                () => _stage.Items = RemoveItem(_stage.Items, item),
                () => CycleItemKind(item)));
        }

        foreach (var hazard in _stage.Hazards)
        {
            _items.Add(new EditableItem("hazard", () => ToRectangle(hazard.Bounds), bounds => MoveHazard(hazard, bounds), () => _stage.Hazards = RemoveItem(_stage.Hazards, hazard)));
        }
    }

    private void DrawStage(Rectangle map)
    {
        DrawRectangle(Map(_stage.ExitBounds, map), new Color(72, 196, 112));
        DrawFrame(Map(_stage.ExitBounds, map), Color.White, 2);
        DrawRectangle(Map(_stage.BusStopBounds, map), new Color(70, 150, 230));
        DrawFrame(Map(_stage.BusStopBounds, map), Color.White, 2);
        DrawRectangle(Map(_stage.HospitalBounds, map), new Color(220, 220, 235));
        DrawFrame(Map(_stage.HospitalBounds, map), new Color(80, 120, 220), 2);

        foreach (var wall in _stage.Walls)
        {
            DrawRectangle(Map(wall, map), new Color(108, 116, 132));
        }

        foreach (var item in _stage.Items)
        {
            var mapped = Map(item.Bounds, map);
            if (IsTicketPiece(item))
            {
                DrawRectangle(mapped, new Color(172, 116, 255));
                DrawFrame(mapped, new Color(230, 210, 255), 2);
                continue;
            }

            if (IsJet(item))
            {
                DrawRectangle(mapped, new Color(81, 161, 255));
                DrawFrame(mapped, new Color(197, 228, 255), 2);
                continue;
            }

            DrawRectangle(mapped, new Color(246, 202, 76));
            DrawFrame(mapped, new Color(255, 250, 170), 2);
        }

        foreach (var hazard in _stage.Hazards)
        {
            var range = GetHazardRange(hazard);
            DrawFrame(Map(range, map), new Color(180, 70, 80), 1);
            DrawHazardRangeEndpoints(hazard, map);
            DrawRectangle(Map(hazard.Bounds, map), new Color(220, 76, 92));
        }

        var player = Map(CenteredBounds(_stage.PlayerStart, 46, 46), map);
        DrawRectangle(player, new Color(86, 168, 255));
        DrawFrame(player, Color.White, 2);

        DrawOverlapWarnings(map);

        if (_selectedIndex >= 0)
        {
            var selected = Map(_items[_selectedIndex].GetBounds(), map);
            DrawFrame(Inflate(selected, 4), new Color(255, 255, 255), 3);
            DrawRectangle(new Rectangle(selected.Right - HandleSize / 2, selected.Bottom - HandleSize / 2, HandleSize, HandleSize), Color.White);
        }
    }


    private void DrawAddToolbar(Rectangle map)
    {
        foreach (var button in GetAddToolButtons(map))
        {
            DrawRectangle(button.Bounds, new Color(24, 28, 34, 230));
            DrawRectangle(Inflate(button.Bounds, -5), GetToolColor(button.Tool));
            DrawToolGlyph(button);
            DrawFrame(button.Bounds, button.Tool == _addTool ? Color.White : new Color(96, 106, 124), button.Tool == _addTool ? 3 : 2);
        }
    }

    private void DrawToolGlyph(ToolButton button)
    {
        var inner = Inflate(button.Bounds, -8);
        switch (button.Tool)
        {
            case AddTool.HazardVertical:
                DrawRectangle(new Rectangle(inner.Center.X - 2, inner.Y, 4, inner.Height), Color.White);
                break;
            case AddTool.HazardHorizontal:
                DrawRectangle(new Rectangle(inner.X, inner.Center.Y - 2, inner.Width, 4), Color.White);
                break;
        }
    }

    private void DrawOverlapWarnings(Rectangle map)
    {
        var overlappedIndexes = new HashSet<int>();
        for (var i = 0; i < _items.Count; i++)
        {
            var first = _items[i].GetBounds();
            for (var j = i + 1; j < _items.Count; j++)
            {
                var second = _items[j].GetBounds();
                if (!first.Intersects(second))
                {
                    continue;
                }

                overlappedIndexes.Add(i);
                overlappedIndexes.Add(j);
                DrawRectangle(Map(Rectangle.Intersect(first, second), map), new Color(255, 170, 30, 120));
            }
        }

        foreach (var index in overlappedIndexes)
        {
            DrawFrame(Inflate(Map(_items[index].GetBounds(), map), 2), new Color(255, 190, 40), 3);
        }
    }

    private void DrawHazardRangeEndpoints(HazardData hazard, Rectangle map)
    {
        var bounds = ToRectangle(hazard.Bounds);
        Rectangle minBounds;
        Rectangle maxBounds;
        if (IsHorizontalHazard(hazard))
        {
            minBounds = new Rectangle(hazard.Min, bounds.Y, bounds.Width, bounds.Height);
            maxBounds = new Rectangle(hazard.Max, bounds.Y, bounds.Width, bounds.Height);
        }
        else
        {
            minBounds = new Rectangle(bounds.X, hazard.Min, bounds.Width, bounds.Height);
            maxBounds = new Rectangle(bounds.X, hazard.Max, bounds.Width, bounds.Height);
        }

        DrawFrame(Inflate(Map(minBounds, map), 2), new Color(70, 210, 255), 3);
        DrawFrame(Inflate(Map(maxBounds, map), 2), new Color(255, 90, 220), 3);
    }

    private void DrawGrid(Rectangle map)
    {
        for (var x = 0; x <= VirtualWidth; x += SnapGridSize)
        {
            var screenX = map.X + (int)(x * map.Width / (float)VirtualWidth);
            var color = x % 120 == 0 ? new Color(44, 50, 62) : new Color(32, 36, 44);
            DrawRectangle(new Rectangle(screenX, map.Y, 1, map.Height), color);
        }

        for (var y = 0; y <= VirtualHeight; y += SnapGridSize)
        {
            var screenY = map.Y + (int)(y * map.Height / (float)VirtualHeight);
            var color = y % 120 == 0 ? new Color(44, 50, 62) : new Color(32, 36, 44);
            DrawRectangle(new Rectangle(map.X, screenY, map.Width, 1), color);
        }
    }

    private int FindItemAt(Point world)
    {
        for (var i = _items.Count - 1; i >= 0; i--)
        {
            if (_items[i].GetBounds().Contains(world))
            {
                return i;
            }
        }

        return -1;
    }

    private bool IsSelectedResizeHandle(Point screenPosition, Rectangle map)
    {
        if (_selectedIndex < 0 || _selectedIndex >= _items.Count)
        {
            return false;
        }

        var selected = Map(_items[_selectedIndex].GetBounds(), map);
        var handle = new Rectangle(selected.Right - HandleSize / 2, selected.Bottom - HandleSize / 2, HandleSize, HandleSize);
        return handle.Contains(screenPosition);
    }


    private bool TrySelectAddTool(Point screenPosition, Rectangle map)
    {
        foreach (var button in GetAddToolButtons(map))
        {
            if (!button.Bounds.Contains(screenPosition))
            {
                continue;
            }

            _addTool = _addTool == button.Tool ? AddTool.None : button.Tool;
            _dragging = false;
            _dragMode = DragMode.None;
            _status = _addTool == AddTool.None ? "Add tool off" : $"Add {GetToolName(_addTool)}";
            return true;
        }

        return false;
    }

    private IEnumerable<ToolButton> GetAddToolButtons(Rectangle map)
    {
        var x = map.X + 12;
        var y = map.Y + 12;
        yield return new ToolButton(AddTool.Wall, new Rectangle(x, y, ToolButtonSize, ToolButtonSize));
        yield return new ToolButton(AddTool.Gem, new Rectangle(x + (ToolButtonSize + ToolButtonGap), y, ToolButtonSize, ToolButtonSize));
        yield return new ToolButton(AddTool.TicketPiece, new Rectangle(x + (ToolButtonSize + ToolButtonGap) * 2, y, ToolButtonSize, ToolButtonSize));
        yield return new ToolButton(AddTool.HazardVertical, new Rectangle(x + (ToolButtonSize + ToolButtonGap) * 3, y, ToolButtonSize, ToolButtonSize));
        yield return new ToolButton(AddTool.HazardHorizontal, new Rectangle(x + (ToolButtonSize + ToolButtonGap) * 4, y, ToolButtonSize, ToolButtonSize));
    }

    private static Color GetToolColor(AddTool tool) => tool switch
    {
        AddTool.Wall => new Color(108, 116, 132),
        AddTool.Gem => new Color(246, 202, 76),
        AddTool.TicketPiece => new Color(172, 116, 255),
        AddTool.HazardVertical => new Color(220, 76, 92),
        AddTool.HazardHorizontal => new Color(220, 76, 92),
        _ => new Color(70, 76, 88),
    };

    private static string GetToolName(AddTool tool) => tool switch
    {
        AddTool.Wall => "wall",
        AddTool.Gem => "gem",
        AddTool.TicketPiece => "ticket piece",
        AddTool.HazardVertical => "vertical hazard",
        AddTool.HazardHorizontal => "horizontal hazard",
        _ => "none",
    };

    private void UpdateWindowTitle()
    {
        var selected = _selectedIndex >= 0 ? _items[_selectedIndex].Kind : "none";
        var snap = _snapToGrid ? $"snap {SnapGridSize}px" : "snap off";
        var overlaps = CountOverlaps();
        var overlapStatus = overlaps > 0 ? $" - overlaps {overlaps}" : string.Empty;
        var addTool = _addTool == AddTool.None ? "add off" : $"add {GetToolName(_addTool)}";
        Window.Title = $"SkylarkBimbleStreet Editor - {_stageFiles[_stageIndex].Name} - {_stage.Name} - selected {selected} - {addTool} - {snap}{overlapStatus} - {_status} - toolbar choose/add, drag handle resize, PageUp/PageDown stage, S save, R reload, G snap, T kind, 1 wall, 2 gem, 3 ticket, 4 hazard V, 5 hazard H, A/D min, J/L max, Delete remove";
    }

    private int CountOverlaps()
    {
        var count = 0;
        for (var i = 0; i < _items.Count; i++)
        {
            var first = _items[i].GetBounds();
            for (var j = i + 1; j < _items.Count; j++)
            {
                if (first.Intersects(_items[j].GetBounds()))
                {
                    count++;
                }
            }
        }

        return count;
    }

    private Rectangle GetMapRectangle()
    {
        var viewport = GraphicsDevice.Viewport;
        var scale = Math.Min(viewport.Width / (float)VirtualWidth, viewport.Height / (float)VirtualHeight);
        var width = (int)(VirtualWidth * scale);
        var height = (int)(VirtualHeight * scale);
        return new Rectangle((viewport.Width - width) / 2, (viewport.Height - height) / 2, width, height);
    }

    private static DirectoryInfo FindStagesDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "SkylarkBimbleStreet", "Stages");
            if (Directory.Exists(candidate))
            {
                return new DirectoryInfo(candidate);
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find SkylarkBimbleStreet/Stages from the editor output directory.");
    }

    private static Rectangle ToRectangle(RectangleData data) => new(data.X, data.Y, data.Width, data.Height);

    private static string GetItemDisplayName(ItemData item) => IsTicketPiece(item) ? "ticket piece" : item.Kind;

    private static bool IsTicketPiece(ItemData item) => item.Kind is "ticketPiece" or "ticket piece";

    private static bool IsJet(ItemData item) => item.Kind is "jet";

    private static void CycleItemKind(ItemData item)
    {
        item.Kind = item.Kind switch
        {
            "gem" => "ticketPiece",
            "ticketPiece" or "ticket piece" => "jet",
            _ => "gem",
        };
    }

    private static int[] ChooseTicketPieceIndexes(int collectibleCount)
    {
        var ticketPieceCount = Math.Min(3, collectibleCount);
        if (ticketPieceCount == 0)
        {
            return [];
        }

        if (ticketPieceCount == 1)
        {
            return [0];
        }

        if (ticketPieceCount == 2)
        {
            return [0, collectibleCount - 1];
        }

        return [0, collectibleCount / 2, collectibleCount - 1];
    }

    private static RectangleData FromRectangle(Rectangle rectangle) => new()
    {
        X = rectangle.X,
        Y = rectangle.Y,
        Width = rectangle.Width,
        Height = rectangle.Height,
    };

    private static void FromRectangle(RectangleData data, Rectangle rectangle)
    {
        data.X = rectangle.X;
        data.Y = rectangle.Y;
        data.Width = rectangle.Width;
        data.Height = rectangle.Height;
    }

    private static Rectangle CenteredBounds(Vector2Data center, int width, int height) => new((int)center.X, (int)center.Y, width, height);

    private static void SetCenter(Vector2Data position, Rectangle bounds)
    {
        position.X = bounds.X;
        position.Y = bounds.Y;
    }

    private static bool IsHorizontalHazard(HazardData hazard) => Math.Abs(hazard.Velocity.X) >= Math.Abs(hazard.Velocity.Y);

    private static void MoveHazard(HazardData hazard, Rectangle bounds)
    {
        var currentBounds = ToRectangle(hazard.Bounds);
        var deltaX = bounds.X - currentBounds.X;
        var deltaY = bounds.Y - currentBounds.Y;
        FromRectangle(hazard.Bounds, bounds);

        if (IsHorizontalHazard(hazard))
        {
            hazard.Min = Math.Clamp(hazard.Min + deltaX, 0, VirtualWidth - bounds.Width);
            hazard.Max = Math.Clamp(hazard.Max + deltaX, hazard.Min, VirtualWidth - bounds.Width);
            return;
        }

        hazard.Min = Math.Clamp(hazard.Min + deltaY, 0, VirtualHeight - bounds.Height);
        hazard.Max = Math.Clamp(hazard.Max + deltaY, hazard.Min, VirtualHeight - bounds.Height);
    }

    private static Rectangle GetHazardRange(HazardData hazard)
    {
        var bounds = ToRectangle(hazard.Bounds);
        if (IsHorizontalHazard(hazard))
        {
            return new Rectangle(hazard.Min, bounds.Y, hazard.Max - hazard.Min + bounds.Width, bounds.Height);
        }

        return new Rectangle(bounds.X, hazard.Min, bounds.Width, hazard.Max - hazard.Min + bounds.Height);
    }

    private static Rectangle Map(RectangleData source, Rectangle map) => Map(ToRectangle(source), map);

    private static Rectangle Map(Rectangle source, Rectangle map)
    {
        var scaleX = map.Width / (float)VirtualWidth;
        var scaleY = map.Height / (float)VirtualHeight;
        return new Rectangle(
            map.X + (int)(source.X * scaleX),
            map.Y + (int)(source.Y * scaleY),
            Math.Max(1, (int)Math.Ceiling(source.Width * scaleX)),
            Math.Max(1, (int)Math.Ceiling(source.Height * scaleY)));
    }

    private static Point ScreenToWorld(Point screen, Rectangle map)
    {
        var x = (screen.X - map.X) * VirtualWidth / Math.Max(1, map.Width);
        var y = (screen.Y - map.Y) * VirtualHeight / Math.Max(1, map.Height);
        return new Point(Math.Clamp(x, 0, VirtualWidth), Math.Clamp(y, 0, VirtualHeight));
    }

    private static Rectangle Inflate(Rectangle rectangle, int amount) => new(rectangle.X - amount, rectangle.Y - amount, rectangle.Width + amount * 2, rectangle.Height + amount * 2);

    private static int Snap(int value) => (int)Math.Round(value / (double)SnapGridSize) * SnapGridSize;

    private static T[] RemoveItem<T>(T[] items, T item) where T : class => items.Where(candidate => !ReferenceEquals(candidate, item)).ToArray();

    private static T[] AppendItem<T>(T[] items, T item) => [.. items, item];

    private void DrawRectangle(Rectangle rectangle, Color color) => _spriteBatch.Draw(_pixel, rectangle, color);

    private void DrawFrame(Rectangle rectangle, Color color, int thickness)
    {
        DrawRectangle(new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color);
        DrawRectangle(new Rectangle(rectangle.X, rectangle.Bottom - thickness, rectangle.Width, thickness), color);
        DrawRectangle(new Rectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), color);
        DrawRectangle(new Rectangle(rectangle.Right - thickness, rectangle.Y, thickness, rectangle.Height), color);
    }

    private bool WasPressed(KeyboardState keyboard, Keys key) => keyboard.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);


    private enum AddTool
    {
        None,
        Wall,
        Gem,
        TicketPiece,
        HazardVertical,
        HazardHorizontal,
    }

    private readonly record struct ToolButton(AddTool Tool, Rectangle Bounds);
    private enum DragMode
    {
        None,
        Move,
        Resize,
    }

    private sealed class EditableItem
    {
        private readonly Func<Rectangle> _getBounds;
        private readonly Action<Rectangle> _setBounds;
        private readonly Action _delete;
        private readonly Action _toggleKind;

        public EditableItem(string kind, Func<Rectangle> getBounds, Action<Rectangle> setBounds, Action delete = null, Action toggleKind = null)
        {
            Kind = kind;
            _getBounds = getBounds;
            _setBounds = setBounds;
            _delete = delete ?? NoOp;
            _toggleKind = toggleKind ?? NoOp;
            CanDelete = delete is not null;
            CanToggleKind = toggleKind is not null;
        }

        public string Kind { get; }

        public bool CanDelete { get; }

        public bool CanToggleKind { get; }

        public Rectangle GetBounds() => _getBounds();

        public void SetBounds(Rectangle bounds) => _setBounds(bounds);

        public void Delete() => _delete();

        public void ToggleKind() => _toggleKind();

        private static void NoOp()
        {
        }
    }
}
