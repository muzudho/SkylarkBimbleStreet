namespace SkylarkBimbleStreet;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;

internal static class StageLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static Stage[] LoadStages()
    {
        var stageDirectory = Path.Combine(AppContext.BaseDirectory, "Stages");
        if (!Directory.Exists(stageDirectory))
        {
            throw new InvalidOperationException($"Stage directory '{stageDirectory}' does not exist.");
        }

        var stageFiles = Directory.GetFiles(stageDirectory, "stage-*.json").OrderBy(static path => path).ToArray();
        if (stageFiles.Length == 0)
        {
            throw new InvalidOperationException($"Stage directory '{stageDirectory}' has no stage-*.json files.");
        }

        var stages = new List<Stage>(stageFiles.Length);
        foreach (var stageFile in stageFiles)
        {
            var stageIndex = GetStageIndex(stageFile);
            if (stageIndex != stages.Count)
            {
                throw new InvalidOperationException($"Stage file '{stageFile}' skips a stage number. Expected stage-{stages.Count + 1:000}.json.");
            }

            stages.Add(LoadStage(stageFile));
        }

        return stages.ToArray();
    }

    private static Stage LoadStage(string stageFile)
    {
        try
        {
            var json = File.ReadAllText(stageFile);
            var data = JsonSerializer.Deserialize<StageData>(json, JsonOptions)
                ?? throw new InvalidOperationException("JSON root is empty.");
            return ToStage(data, stageFile);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException($"Failed to parse stage file '{stageFile}'.", exception);
        }
    }

    private static Stage ToStage(StageData data, string stageFile)
    {
        var items = LoadItems(data, stageFile);
        return new Stage(
            name: Require(data.Name, stageFile, "name"),
            playerStart: ToVector2(Require(data.PlayerStart, stageFile, "playerStart")),
            exitBounds: ToRectangle(Require(data.ExitBounds, stageFile, "exitBounds")),
            busStopBounds: ToRectangle(Require(data.BusStopBounds, stageFile, "busStopBounds")),
            hospitalBounds: ToRectangle(Require(data.HospitalBounds, stageFile, "hospitalBounds")),
            backgroundColor: ToColor(Require(data.BackgroundColor, stageFile, "backgroundColor")),
            walls: Require(data.Walls, stageFile, "walls").Select(ToRectangle).ToArray(),
            ticketPieces: items.TicketPieces,
            gems: items.Gems,
            jets: items.Jets,
            gemBagCapacity: data.GemBagCapacity ?? CalculateDefaultGemBagCapacity(items.Gems),
            hazards: Require(data.Hazards, stageFile, "hazards").Select(hazard => ToHazard(hazard, stageFile)).ToArray());
    }

    private static StageItems LoadItems(StageData data, string stageFile)
    {
        if (data.Items is not null)
        {
            return LoadExplicitItems(data.Items, stageFile);
        }

        var collectibles = Require(data.Collectibles, stageFile, "items or collectibles").Select(ToRectangle).ToArray();
        var ticketPieceIndexes = ChooseTicketPieceIndexes(collectibles.Length);
        var ticketPieces = new List<Rectangle>(ticketPieceIndexes.Length);
        var gems = new List<Rectangle>(Math.Max(0, collectibles.Length - ticketPieceIndexes.Length));

        for (var i = 0; i < collectibles.Length; i++)
        {
            if (ticketPieceIndexes.Contains(i))
            {
                ticketPieces.Add(collectibles[i]);
            }
            else
            {
                gems.Add(collectibles[i]);
            }
        }

        return new StageItems(ticketPieces.ToArray(), gems.ToArray(), []);
    }

    private static StageItems LoadExplicitItems(ItemData[] items, string stageFile)
    {
        var ticketPieces = new List<Rectangle>();
        var gems = new List<Rectangle>();
        var jets = new List<Rectangle>();
        for (var i = 0; i < items.Length; i++)
        {
            var item = Require(items[i], stageFile, $"items[{i}]");
            var bounds = ToRectangle(Require(item.Bounds, stageFile, $"items[{i}].bounds"));
            switch (Require(item.Kind, stageFile, $"items[{i}].kind"))
            {
                case "ticketPiece":
                case "ticket piece":
                    ticketPieces.Add(bounds);
                    break;
                case "gem":
                    gems.Add(bounds);
                    break;
                case "jet":
                    jets.Add(bounds);
                    break;
                default:
                    throw new InvalidOperationException($"Stage file '{stageFile}' has unknown item kind '{item.Kind}' at items[{i}].kind.");
            }
        }

        return new StageItems(ticketPieces.ToArray(), gems.ToArray(), jets.ToArray());
    }

    private static int CalculateDefaultGemBagCapacity(Rectangle[] gems)
    {
        var total = gems.Sum(GetGemShardValue);
        return Math.Max(4, (int)MathF.Ceiling(total * 0.6f));
    }

    private static int GetGemShardValue(Rectangle gem)
    {
        var widthUnits = Math.Max(1, (int)MathF.Round(gem.Width / 17f));
        var heightUnits = Math.Max(1, (int)MathF.Round(gem.Height / 17f));
        return widthUnits * heightUnits;
    }
    private static int GetStageIndex(string stageFile)
    {
        var fileName = Path.GetFileNameWithoutExtension(stageFile);
        var numberText = fileName.Split('-').LastOrDefault();
        if (!int.TryParse(numberText, out var stageNumber) || stageNumber <= 0)
        {
            throw new InvalidOperationException($"Stage file '{stageFile}' must be named like stage-001.json.");
        }

        return stageNumber - 1;
    }

    private static Rectangle ToRectangle(RectangleData data) => new(data.X, data.Y, data.Width, data.Height);

    private static Vector2 ToVector2(Vector2Data data) => new(data.X, data.Y);

    private static Color ToColor(ColorData data) => new(data.R, data.G, data.B);

    private static Hazard ToHazard(HazardData data, string stageFile) => new(
        bounds: ToRectangle(Require(data.Bounds, stageFile, "hazards[].bounds")),
        velocity: ToVector2(Require(data.Velocity, stageFile, "hazards[].velocity")),
        min: data.Min,
        max: data.Max);

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

    private static T Require<T>(T value, string stageFile, string propertyName)
    {
        if (value is null)
        {
            throw new InvalidOperationException($"Stage file '{stageFile}' is missing '{propertyName}'.");
        }

        return value;
    }

    private readonly record struct StageItems(Rectangle[] TicketPieces, Rectangle[] Gems, Rectangle[] Jets);
}
