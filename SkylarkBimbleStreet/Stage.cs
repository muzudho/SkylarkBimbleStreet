namespace SkylarkBimbleStreet;

using Microsoft.Xna.Framework;

internal sealed class Stage
{
    public readonly string Name;
    public readonly Vector2 PlayerStart;
    public readonly Rectangle ExitBounds;
    public readonly Rectangle BusStopBounds;
    public readonly Color BackgroundColor;
    public readonly Rectangle[] Walls;
    public readonly Rectangle[] Gems;
    public readonly Hazard[] Hazards;

    public Stage(
        string name,
        Vector2 playerStart,
        Rectangle exitBounds,
        Rectangle busStopBounds,
        Color backgroundColor,
        Rectangle[] walls,
        Rectangle[] gems,
        Hazard[] hazards)
    {
        Name = name;
        PlayerStart = playerStart;
        ExitBounds = exitBounds;
        BusStopBounds = busStopBounds;
        BackgroundColor = backgroundColor;
        Walls = walls;
        Gems = gems;
        Hazards = hazards;
    }
}