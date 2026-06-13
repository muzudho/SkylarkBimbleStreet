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

    public static Stage[] LoadStagesOrFallback(Stage[] fallbackStages)
    {
        var stageDirectory = Path.Combine(AppContext.BaseDirectory, "Stages");
        if (!Directory.Exists(stageDirectory))
        {
            return fallbackStages;
        }

        var stageFiles = Directory.GetFiles(stageDirectory, "stage-*.json").OrderBy(static path => path).ToArray();
        if (stageFiles.Length == 0)
        {
            return fallbackStages;
        }

        var stages = new List<Stage>(fallbackStages);
        foreach (var stageFile in stageFiles)
        {
            var stage = LoadStage(stageFile);
            var stageIndex = GetStageIndex(stageFile);
            if (stageIndex < stages.Count)
            {
                stages[stageIndex] = stage;
                continue;
            }

            if (stageIndex == stages.Count)
            {
                stages.Add(stage);
                continue;
            }

            throw new InvalidOperationException($"Stage file '{stageFile}' skips a stage number. Expected stage-{stages.Count + 1:000}.json or earlier.");
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

    private static Stage ToStage(StageData data, string stageFile) => new(
        name: Require(data.Name, stageFile, "name"),
        playerStart: ToVector2(Require(data.PlayerStart, stageFile, "playerStart")),
        exitBounds: ToRectangle(Require(data.ExitBounds, stageFile, "exitBounds")),
        busStopBounds: ToRectangle(Require(data.BusStopBounds, stageFile, "busStopBounds")),
        hospitalBounds: ToRectangle(Require(data.HospitalBounds, stageFile, "hospitalBounds")),
        backgroundColor: ToColor(Require(data.BackgroundColor, stageFile, "backgroundColor")),
        walls: Require(data.Walls, stageFile, "walls").Select(ToRectangle).ToArray(),
        collectibles: Require(data.Collectibles, stageFile, "collectibles").Select(ToRectangle).ToArray(),
        hazards: Require(data.Hazards, stageFile, "hazards").Select(hazard => ToHazard(hazard, stageFile)).ToArray());

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

    private static T Require<T>(T value, string stageFile, string propertyName)
    {
        if (value is null)
        {
            throw new InvalidOperationException($"Stage file '{stageFile}' is missing '{propertyName}'.");
        }

        return value;
    }
}