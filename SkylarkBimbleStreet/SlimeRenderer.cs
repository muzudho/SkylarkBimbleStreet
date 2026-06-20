namespace SkylarkBimbleStreet;

using System;
using Microsoft.Xna.Framework;

/// <summary>
///     <pre>
/// 壁追従中のプレイヤーをスライム状に描画します。
/// 
/// TODO: よく分からんから、以下のようにしてほしい（＾～＾）
/// 
/// 👇　もとのプレイヤー（石）の縦横サイズは変わることがあるから、ここでは単位１とするぜ（＾～＾）
/// 
///     1 width
///   +----+
/// 1 |    |
///   |    |
///   +----+
/// height
/// 
/// 
/// 👇　プレイヤー（石）を下側の壁に押し付けたとするぜ（＾～＾）
/// 
///            a
///          +----+
///    b     |    |
///       +--+    +--+
/// 1 - b |          |
///       +----------+
///        1
/// 
/// 横幅 a と、高さ b の割合を指定したいぜ（＾～＾） つまり、 a, b は帽子の部分な（＾▽＾）
/// こうすると、サイズを指定しやすいぜ（＾▽＾）
///     </pre>
/// </summary>
internal static class SlimeRenderer
{
    // 壁追従中のプレイヤー表示。判定矩形は変えず、描画だけスライム状に変える。
    // CapWidthRatio: 図の a。帽子部分の横幅。CapHeightRatio: 図の b。帽子部分の高さ。
    private const float SlimeCapWidthRatio = 0.48f;
    private const float SlimeCapHeightRatio = 0.42f;

    public static Rectangle Draw(
        Rectangle player,
        Vector2 contactDirection,
        Color bodyColor,
        Color innerColor,
        Color shineColor,
        int playerSize,
        Action<Rectangle, Color> drawRectangle)
    {
        var baseBounds = GetSlimeBaseBounds(player, contactDirection);
        var capBounds = GetSlimeCapBounds(player, contactDirection);

        // 後景：壁に押しつぶされた台座部分。
        drawRectangle(baseBounds, WithAlpha(bodyColor, 175));

        // 前景：元の四角いプレイヤーの一部。凸の字の帽子部分として残す。
        drawRectangle(capBounds, bodyColor);
        if (!capBounds.IsEmpty)
        {
            drawRectangle(Inset(capBounds, Math.Max(4, playerSize / 7)), innerColor);
        }

        return player;
    }

    private static Rectangle GetSlimeBaseBounds(Rectangle player, Vector2 contactDirection)
    {
        // 台座部分は、親壁に接している側へ寄せる。
        var baseThickness = GetBaseThickness(player, contactDirection);
        if (contactDirection.X > 0f) return new Rectangle(player.Right - baseThickness, player.Y, baseThickness, player.Height);
        if (contactDirection.X < 0f) return new Rectangle(player.X, player.Y, baseThickness, player.Height);
        if (contactDirection.Y > 0f) return new Rectangle(player.X, player.Bottom - baseThickness, player.Width, baseThickness);
        if (contactDirection.Y < 0f) return new Rectangle(player.X, player.Y, player.Width, baseThickness);
        return player;
    }

    private static Rectangle GetSlimeCapBounds(Rectangle player, Vector2 contactDirection)
    {
        // 帽子部分は、親壁と反対側の中央へ置く。
        var capWidth = GetCapWidth(player, contactDirection);
        var capHeight = GetCapHeight(player, contactDirection);
        if (contactDirection.X > 0f) return new Rectangle(player.X, player.Center.Y - capHeight / 2, capWidth, capHeight);
        if (contactDirection.X < 0f) return new Rectangle(player.Right - capWidth, player.Center.Y - capHeight / 2, capWidth, capHeight);
        if (contactDirection.Y > 0f) return new Rectangle(player.Center.X - capWidth / 2, player.Y, capWidth, capHeight);
        if (contactDirection.Y < 0f) return new Rectangle(player.Center.X - capWidth / 2, player.Bottom - capHeight, capWidth, capHeight);
        return Rectangle.Empty;
    }

    private static int GetBaseThickness(Rectangle player, Vector2 contactDirection)
    {
        var size = contactDirection.X != 0f ? player.Width : player.Height;
        return Math.Max(1, size - GetCapDepth(player, contactDirection));
    }

    private static int GetCapWidth(Rectangle player, Vector2 contactDirection)
    {
        var size = contactDirection.X != 0f ? player.Height : player.Width;
        return Math.Clamp((int)MathF.Round(size * SlimeCapWidthRatio), 1, size);
    }

    private static int GetCapHeight(Rectangle player, Vector2 contactDirection) => GetCapDepth(player, contactDirection);

    private static int GetCapDepth(Rectangle player, Vector2 contactDirection)
    {
        var size = contactDirection.X != 0f ? player.Width : player.Height;
        return Math.Clamp((int)MathF.Round(size * SlimeCapHeightRatio), 1, size);
    }

    private static Color WithAlpha(Color color, byte alpha) => new(color.R, color.G, color.B, alpha);

    private static Rectangle Inset(Rectangle rectangle, int inset) => new(
        rectangle.X + inset,
        rectangle.Y + inset,
        Math.Max(1, rectangle.Width - inset * 2),
        Math.Max(1, rectangle.Height - inset * 2));
}
