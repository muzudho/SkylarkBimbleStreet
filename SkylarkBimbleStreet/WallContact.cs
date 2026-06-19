namespace SkylarkBimbleStreet;

/// <summary>
/// 接触してる壁の辺
/// </summary>
internal enum WallContactSide
{
    None,
    Top,
    Right,
    Bottom,
    Left,
}

/// <summary>
/// 壁との接触
/// </summary>
internal sealed class WallContact
{
    public static readonly WallContact None = new(-1, WallContactSide.None);

    public WallContact(int wallIndex, WallContactSide side)
    {
        WallIndex = wallIndex;
        Side = side;
    }

    public int WallIndex { get; }

    /// <summary>
    /// 接触している壁の辺
    /// </summary>
    public WallContactSide Side { get; }

    public bool IsValid(int wallCount) => WallIndex >= 0 && WallIndex < wallCount && Side != WallContactSide.None;
}
