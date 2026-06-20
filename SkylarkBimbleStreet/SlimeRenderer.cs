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
///    c     |    |
///       +--+    +--+
///    d  |          |
///       +----------+
///        b
///        
/// b > a
/// 
/// 横幅 a, b と、高さ c, d を元サイズとの割合で指定したいぜ（＾～＾） つまり、 a, c は帽子の部分な（＾▽＾）
/// こうすると、サイズを指定しやすいぜ（＾▽＾）
///     </pre>
/// </summary>
internal static class SlimeRenderer
{
    // 壁追従中のプレイヤー表示。判定矩形は変えず、描画だけスライム状に変える。
    // 図の a, b, c, d。元プレイヤーサイズに対する倍率なので、1 より大きい値も使える。
    private const float SlimeCapWidthRatio = 0.88f;     // a
    private const float SlimeBaseWidthRatio = 1.32f;    // b
    private const float SlimeCapHeightRatio = 0.62f;    // c
    private const float SlimeBaseHeightRatio = 0.42f;   // d

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
        var baseWidth = GetParallelSize(player, contactDirection, SlimeBaseWidthRatio);
        var baseHeight = GetDepthSize(player, contactDirection, SlimeBaseHeightRatio);
        if (contactDirection.X > 0f) return new Rectangle(player.Right - baseHeight, player.Center.Y - baseWidth / 2, baseHeight, baseWidth);
        if (contactDirection.X < 0f) return new Rectangle(player.X, player.Center.Y - baseWidth / 2, baseHeight, baseWidth);
        if (contactDirection.Y > 0f) return new Rectangle(player.Center.X - baseWidth / 2, player.Bottom - baseHeight, baseWidth, baseHeight);
        if (contactDirection.Y < 0f) return new Rectangle(player.Center.X - baseWidth / 2, player.Y, baseWidth, baseHeight);
        return player;
    }

    private static Rectangle GetSlimeCapBounds(Rectangle player, Vector2 contactDirection)
    {
        // 帽子部分は、親壁と反対側の中央へ置く。
        var capWidth = GetParallelSize(player, contactDirection, SlimeCapWidthRatio);
        var capHeight = GetDepthSize(player, contactDirection, SlimeCapHeightRatio);
        var baseHeight = GetDepthSize(player, contactDirection, SlimeBaseHeightRatio);
        if (contactDirection.X > 0f) return new Rectangle(player.Right - baseHeight - capHeight, player.Center.Y - capWidth / 2, capHeight, capWidth);
        if (contactDirection.X < 0f) return new Rectangle(player.X + baseHeight, player.Center.Y - capWidth / 2, capHeight, capWidth);
        if (contactDirection.Y > 0f) return new Rectangle(player.Center.X - capWidth / 2, player.Bottom - baseHeight - capHeight, capWidth, capHeight);
        if (contactDirection.Y < 0f) return new Rectangle(player.Center.X - capWidth / 2, player.Y + baseHeight, capWidth, capHeight);
        return Rectangle.Empty;
    }

    private static int GetParallelSize(Rectangle player, Vector2 contactDirection, float ratio)
    {
        var size = contactDirection.X != 0f ? player.Height : player.Width;
        return Math.Max(1, (int)MathF.Round(size * ratio));
    }

    private static int GetDepthSize(Rectangle player, Vector2 contactDirection, float ratio)
    {
        var size = contactDirection.X != 0f ? player.Width : player.Height;
        return Math.Max(1, (int)MathF.Round(size * ratio));
    }

    private static Color WithAlpha(Color color, byte alpha) => new(color.R, color.G, color.B, alpha);

    private static Rectangle Inset(Rectangle rectangle, int inset) => new(
        rectangle.X + inset,
        rectangle.Y + inset,
        Math.Max(1, rectangle.Width - inset * 2),
        Math.Max(1, rectangle.Height - inset * 2));
}
