namespace SkylarkBimbleStreet;

internal sealed class StageData
{
    public int Version { get; set; } = 1;
    public string Name { get; set; } = null!;
    public Vector2Data PlayerStart { get; set; } = null!;
    public RectangleData ExitBounds { get; set; } = null!;
    public RectangleData BusStopBounds { get; set; } = null!;
    public RectangleData HospitalBounds { get; set; } = null!;
    public ColorData BackgroundColor { get; set; } = null!;
    public RectangleData[] Walls { get; set; } = null!;
    public RectangleData[] Collectibles { get; set; } = null!;
    public HazardData[] Hazards { get; set; } = null!;
}

internal sealed class RectangleData
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

internal sealed class Vector2Data
{
    public float X { get; set; }
    public float Y { get; set; }
}

internal sealed class ColorData
{
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }
}

internal sealed class HazardData
{
    public RectangleData Bounds { get; set; } = null!;
    public Vector2Data Velocity { get; set; } = null!;
    public int Min { get; set; }
    public int Max { get; set; }
}