namespace SkylarkBimbleStreet;

using System;
using Microsoft.Xna.Framework;

/// <summary>
/// ゴースト表示の見た目をまとめます。
/// </summary>
internal static class Ghosts
{
    // プレイヤーのサイズを単位 1 としたときの、各ゴーストの表示サイズ。
    // 1.0f ならプレイヤーと同じ大きさ、1.4f ならプレイヤーの 1.4 倍の大きさ。
    public static readonly GhostStyle InputDirection = new(new Color(80, 255, 120), 1.68f, 0.04f, 0.09f);   // 入力方向ゴースト（緑）
    public static readonly GhostStyle MoveDirection = new(new Color(118, 218, 255), 1.45f, 0.06f, 0.09f);   // 進行方向ゴースト（青）
    public static readonly GhostStyle OuterCorner = new(new Color(255, 220, 72), 1.22f, 0.08f, 0.09f);  // 外角判定ゴースト（黄）

    public static Rectangle GetFrameBounds(Rectangle playerGhost, GhostStyle style)
    {
        var width = Math.Max(1, (int)MathF.Round(playerGhost.Width * style.SizeRatio));
        var height = Math.Max(1, (int)MathF.Round(playerGhost.Height * style.SizeRatio));
        return new Rectangle(playerGhost.Center.X - width / 2, playerGhost.Center.Y - height / 2, width, height);
    }

    public static Rectangle GetHaloBounds(Rectangle frame, Rectangle playerGhost, GhostStyle style)
    {
        var spread = Math.Max(1, (int)MathF.Round(Math.Max(playerGhost.Width, playerGhost.Height) * style.HaloSpreadRatio));
        return new Rectangle(frame.X - spread, frame.Y - spread, frame.Width + spread * 2, frame.Height + spread * 2);
    }

    public static int GetFrameThickness(Rectangle playerGhost, GhostStyle style)
        => Math.Max(1, (int)MathF.Round(Math.Max(playerGhost.Width, playerGhost.Height) * style.ThicknessRatio));
}

internal readonly record struct GhostStyle(Color FrameColor, float SizeRatio, float ThicknessRatio, float HaloSpreadRatio);
