namespace SkylarkBimbleStreet;

using Microsoft.Xna.Framework;

internal struct Hazard
{
    public Rectangle Bounds;
    public Vector2 Velocity;
    public int Min;
    public int Max;

    public Hazard(Rectangle bounds, Vector2 velocity, int min, int max)
    {
        Bounds = bounds;
        Velocity = velocity;
        Min = min;
        Max = max;
    }
}