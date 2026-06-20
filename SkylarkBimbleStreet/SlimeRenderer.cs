namespace SkylarkBimbleStreet;

using System;
using Microsoft.Xna.Framework;

/// <summary>
/// 壁追従中のプレイヤーをスライム状に描画します。
/// </summary>
internal static class SlimeRenderer
{
    // 壁追従中のプレイヤー表示。判定矩形は変えず、描画だけスライム状に変える。
    // Squash: 壁方向へ潰す量。Spread: 壁と平行方向へ広げる量。Stick: 壁側へ貼り付く帯の太さ。
    private const int WallFollowVisualSquash = 20;
    private const int WallFollowVisualSpread = 34;
    private const int WallFollowVisualStick = 22;

    public static Rectangle Draw(
        Rectangle player,
        Vector2 contactDirection,
        Color bodyColor,
        Color innerColor,
        Color shineColor,
        int playerSize,
        Action<Rectangle, Color> drawRectangle)
    {
        var visualPlayer = GetWallFollowVisualPlayer(player, contactDirection);
        var attachment = GetWallFollowAttachmentBounds(player, visualPlayer, contactDirection);

        // 後景：壁に押しつぶされたスライム部分。
        drawRectangle(GetWallFollowSlimeShadowBounds(visualPlayer, contactDirection), WithAlpha(shineColor, 55));
        drawRectangle(visualPlayer, WithAlpha(bodyColor, 175));
        drawRectangle(attachment, WithAlpha(bodyColor, 205));

        // 前景：元の四角いプレイヤーの一部。凸の字の出っ張りとして残す。
        var cap = GetWallFollowSquareCapBounds(player, contactDirection);
        drawRectangle(cap, bodyColor);
        if (!cap.IsEmpty)
        {
            drawRectangle(Inset(cap, Math.Max(4, playerSize / 7)), innerColor);
        }

        return visualPlayer;
    }

    private static Rectangle GetWallFollowVisualPlayer(Rectangle player, Vector2 contactDirection)
    {
        // 親壁方向へ潰し、親壁と平行な方向へ広げる。
        if (contactDirection.X > 0f) return new Rectangle(player.X + WallFollowVisualSquash, player.Y - WallFollowVisualSpread / 2, player.Width - WallFollowVisualSquash, player.Height + WallFollowVisualSpread);
        if (contactDirection.X < 0f) return new Rectangle(player.X, player.Y - WallFollowVisualSpread / 2, player.Width - WallFollowVisualSquash, player.Height + WallFollowVisualSpread);
        if (contactDirection.Y > 0f) return new Rectangle(player.X - WallFollowVisualSpread / 2, player.Y + WallFollowVisualSquash, player.Width + WallFollowVisualSpread, player.Height - WallFollowVisualSquash);
        if (contactDirection.Y < 0f) return new Rectangle(player.X - WallFollowVisualSpread / 2, player.Y, player.Width + WallFollowVisualSpread, player.Height - WallFollowVisualSquash);
        return player;
    }

    private static Rectangle GetWallFollowAttachmentBounds(Rectangle player, Rectangle visualPlayer, Vector2 contactDirection)
    {
        // 親壁側に足す接着帯。平べったい本体と壁の隙間を埋める。
        if (contactDirection.X > 0f) return new Rectangle(player.Right - WallFollowVisualStick, visualPlayer.Y + 4, WallFollowVisualStick + 4, Math.Max(1, visualPlayer.Height - 8));
        if (contactDirection.X < 0f) return new Rectangle(player.X - 4, visualPlayer.Y + 4, WallFollowVisualStick + 4, Math.Max(1, visualPlayer.Height - 8));
        if (contactDirection.Y > 0f) return new Rectangle(visualPlayer.X + 4, player.Bottom - WallFollowVisualStick, Math.Max(1, visualPlayer.Width - 8), WallFollowVisualStick + 4);
        if (contactDirection.Y < 0f) return new Rectangle(visualPlayer.X + 4, player.Y - 4, Math.Max(1, visualPlayer.Width - 8), WallFollowVisualStick + 4);
        return Rectangle.Empty;
    }

    private static Rectangle GetWallFollowSlimeShadowBounds(Rectangle visualPlayer, Vector2 contactDirection)
    {
        // スライム本体の外側に薄くにじませる部分。
        if (contactDirection.X != 0f) return new Rectangle(visualPlayer.X - 6, visualPlayer.Y - 8, visualPlayer.Width + 12, visualPlayer.Height + 16);
        if (contactDirection.Y != 0f) return new Rectangle(visualPlayer.X - 8, visualPlayer.Y - 6, visualPlayer.Width + 16, visualPlayer.Height + 12);
        return Rectangle.Empty;
    }

    private static Rectangle GetWallFollowSquareCapBounds(Rectangle player, Vector2 contactDirection)
    {
        // 元の四角の一部を中央の出っ張りとして残す。壁に向かって凸の字が回転する。
        var capWidth = Math.Max(1, player.Width / 2);
        var capHeight = Math.Max(1, player.Height / 2);
        var capStick = Math.Max(2, WallFollowVisualStick / 2);
        if (contactDirection.X > 0f) return new Rectangle(player.X, player.Center.Y - capHeight / 2, capWidth + capStick, capHeight);
        if (contactDirection.X < 0f) return new Rectangle(player.Right - capWidth - capStick, player.Center.Y - capHeight / 2, capWidth + capStick, capHeight);
        if (contactDirection.Y > 0f) return new Rectangle(player.Center.X - capWidth / 2, player.Y, capWidth, capHeight + capStick);
        if (contactDirection.Y < 0f) return new Rectangle(player.Center.X - capWidth / 2, player.Bottom - capHeight - capStick, capWidth, capHeight + capStick);
        return Rectangle.Empty;
    }

    private static Color WithAlpha(Color color, byte alpha) => new(color.R, color.G, color.B, alpha);

    private static Rectangle Inset(Rectangle rectangle, int inset) => new(
        rectangle.X + inset,
        rectangle.Y + inset,
        Math.Max(1, rectangle.Width - inset * 2),
        Math.Max(1, rectangle.Height - inset * 2));
}
