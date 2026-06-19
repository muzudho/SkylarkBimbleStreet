namespace SkylarkBimbleStreet;

using Microsoft.Xna.Framework;

/// <summary>
/// 壁
/// </summary>
internal sealed class Wall
{
    public Wall(int id, Rectangle bounds)
    {
        Id = id;
        Bounds = bounds;
    }

    public int Id { get; }

    public Rectangle Bounds { get; }

    public int Left => Bounds.Left;

    public int Right => Bounds.Right;

    public int Top => Bounds.Top;

    public int Bottom => Bounds.Bottom;
}
