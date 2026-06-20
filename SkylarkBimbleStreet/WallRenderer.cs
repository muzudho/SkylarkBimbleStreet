namespace SkylarkBimbleStreet;

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SkylarkBimbleStreet.Movement.WallFollowing;

/// <summary>
/// 壁の描画を担当
/// </summary>
internal sealed class WallRenderer
{
    /// <summary>
    /// 全ての壁を描画する。
    /// </summary>
    /// <param name="walls">描画する壁のコレクション</param>
    /// <param name="mapBounds">壁のマッピング関数</param>
    /// <param name="outerColor">外側の色</param>
    /// <param name="innerColor">内側の色</param>
    /// <param name="drawRectangle">矩形を描画する関数</param>
    /// <param name="inset">矩形を内側に縮小する関数</param>
    /// <param name="innerInsetSelector">内側のインセットを選択する関数</param>
    public void DrawWalls(
        IEnumerable<Wall> walls,
        Func<Rectangle, Rectangle> mapBounds,
        Color outerColor,
        Color innerColor,
        Action<Rectangle, Color> drawRectangle,
        Func<Rectangle, int, Rectangle> inset,
        Func<Rectangle, int> innerInsetSelector)
    {
        foreach (var wall in walls)
        {
            var mapped = mapBounds(wall.Bounds);

            // 外側の矩形を描画
            drawRectangle(mapped, outerColor);

            // 内側の矩形を描画
            drawRectangle(inset(mapped, innerInsetSelector(mapped)), innerColor);
        }
    }

    /// <summary>
    /// 壁追従のハイライトを描画する。
    /// </summary>
    /// <param name="walls">描画する壁のコレクション</param>
    /// <param name="wallFollowWallContact">壁追従の接触情報</param>
    /// <param name="wallFollowHitContact">壁追従のヒット情報</param>
    /// <param name="wallFollowerStateKind">片手壁伝い法の状態</param>
    /// <param name="drawLine">線を描画する関数</param>
    /// <param name="withAlpha">アルファ値を適用する関数</param>
    public void DrawWallFollowWallHighlights(
        Walls walls,
        WallContact wallFollowWallContact,
        WallContact wallFollowHitContact,
        WallFollowerStateKind wallFollowerStateKind,
        Action<Vector2, Vector2, int, Color> drawLine,
        Func<Color, byte, Color> withAlpha)
    {
        if (wallFollowWallContact.IsValid(walls.Count))
        {
            DrawWallFollowWallHighlight(walls[wallFollowWallContact.WallIndex], wallFollowWallContact.Side, GetWallFollowWallColor(wallFollowerStateKind), 10, drawLine, withAlpha);
        }

        if (wallFollowHitContact.IsValid(walls.Count))
        {
            var thickness = wallFollowHitContact.WallIndex == wallFollowWallContact.WallIndex ? 16 : 10;
            DrawWallFollowWallHighlight(walls[wallFollowHitContact.WallIndex], wallFollowHitContact.Side, new Color(255, 174, 72), thickness, drawLine, withAlpha);
        }
    }

    private static Color GetWallFollowWallColor(WallFollowerStateKind stateKind) => stateKind switch
    {
        WallFollowerStateKind.BeforeWall => new Color(255, 174, 72),
        WallFollowerStateKind.FacingWall => new Color(78, 220, 150),
        WallFollowerStateKind.AlongWall => new Color(118, 218, 255),
        _ => new Color(78, 220, 150),
    };

    /// <summary>
    /// プレイヤーの入力に応じた壁のハイライトを描画する。
    /// </summary>
    /// <param name="walls">描画する壁のコレクション</param>
    /// <param name="playerBounds">プレイヤーの矩形範囲</param>
    /// <param name="playerInputDirection">プレイヤーの入力方向</param>
    /// <param name="playerInAmbulance">プレイヤーが救急車に乗っているかどうか</param>
    /// <param name="playerInBus">プレイヤーがバスに乗っているかどうか</param>
    /// <param name="isWallFollowActive">壁追従が有効かどうか</param>
    /// <param name="inputContactWallIndex">入力接触の壁インデックス</param>
    /// <param name="wallContactProbeDistance">壁接触のプローブ距離</param>
    /// <param name="drawLine">線を描画する関数</param>
    /// <param name="withAlpha">アルファ値を適用する関数</param>
    public void DrawInputContactWallHighlight(
        Walls walls,
        Rectangle playerBounds,
        Vector2 playerInputDirection,
        bool playerInAmbulance,
        bool playerInBus,
        bool isWallFollowActive,
        int inputContactWallIndex,
        int wallContactProbeDistance,
        Action<Vector2, Vector2, int, Color> drawLine,
        Func<Color, byte, Color> withAlpha)
    {
        if (playerInAmbulance || playerInBus || playerInputDirection == Vector2.Zero || isWallFollowActive) return;

        var probe = playerBounds;
        probe.Offset((int)(playerInputDirection.X * wallContactProbeDistance), (int)(playerInputDirection.Y * wallContactProbeDistance));
        var color = new Color(118, 218, 255);

        if (inputContactWallIndex >= 0 && walls.IsValidIndex(inputContactWallIndex))
        {
            DrawWallFollowWallHighlight(walls[inputContactWallIndex], GetWallContactSide(playerInputDirection), color, 7, drawLine, withAlpha);
            return;
        }

        for (var i = 0; i < walls.Count; i++)
        {
            if (!probe.Intersects(walls[i].Bounds)) continue;

            DrawWallFollowWallHighlight(walls[i], GetWallContactSide(playerInputDirection), color, 7, drawLine, withAlpha);
            return;
        }
    }

    /// <summary>
    /// 壁の辺に応じたハイライトを描画する。
    /// </summary>
    /// <param name="wall">ハイライトを描画する壁</param>
    /// <param name="side">ハイライトを描画する壁の辺</param>
    /// <param name="color">ハイライトの色</param>
    /// <param name="thickness">ハイライトの太さ</param>
    /// <param name="drawLine">線を描画する関数</param>
    /// <param name="withAlpha">アルファ値を適用する関数</param>
    private static void DrawWallFollowWallHighlight(
        Wall wall,
        WallContactSide side,
        Color color,
        int thickness,
        Action<Vector2, Vector2, int, Color> drawLine,
        Func<Color, byte, Color> withAlpha)
    {
        var alpha = withAlpha(color, 230);
        var glow = withAlpha(color, 90);
        switch (side)
        {
            case WallContactSide.Top:
                drawLine(new Vector2(wall.Left, wall.Top), new Vector2(wall.Right, wall.Top), thickness + 8, glow);
                drawLine(new Vector2(wall.Left, wall.Top), new Vector2(wall.Right, wall.Top), thickness, alpha);
                break;
            case WallContactSide.Right:
                drawLine(new Vector2(wall.Right, wall.Top), new Vector2(wall.Right, wall.Bottom), thickness + 8, glow);
                drawLine(new Vector2(wall.Right, wall.Top), new Vector2(wall.Right, wall.Bottom), thickness, alpha);
                break;
            case WallContactSide.Bottom:
                drawLine(new Vector2(wall.Left, wall.Bottom), new Vector2(wall.Right, wall.Bottom), thickness + 8, glow);
                drawLine(new Vector2(wall.Left, wall.Bottom), new Vector2(wall.Right, wall.Bottom), thickness, alpha);
                break;
            case WallContactSide.Left:
                drawLine(new Vector2(wall.Left, wall.Top), new Vector2(wall.Left, wall.Bottom), thickness + 8, glow);
                drawLine(new Vector2(wall.Left, wall.Top), new Vector2(wall.Left, wall.Bottom), thickness, alpha);
                break;
        }
    }

    /// <summary>
    /// プレイヤーの入力方向に応じた壁の辺を取得する。
    /// </summary>
    /// <param name="contactDirection">プレイヤーの入力方向</param>
    /// <returns>壁の辺</returns>
    private static WallContactSide GetWallContactSide(Vector2 contactDirection)
    {
        var direction = GetAxisDirection(contactDirection);
        if (direction.X > 0f) return WallContactSide.Left;
        if (direction.X < 0f) return WallContactSide.Right;
        if (direction.Y > 0f) return WallContactSide.Top;
        if (direction.Y < 0f) return WallContactSide.Bottom;
        return WallContactSide.None;
    }

    /// <summary>
    /// プレイヤーの入力方向を軸方向に変換する。
    /// </summary>
    /// <param name="delta">プレイヤーの入力方向のベクトル</param>
    /// <returns>軸方向に変換されたベクトル</returns>
    private static Vector2 GetAxisDirection(Vector2 delta)
    {
        if (MathF.Abs(delta.X) >= MathF.Abs(delta.Y))
        {
            return delta.X < 0f ? new Vector2(-1f, 0f) : delta.X > 0f ? new Vector2(1f, 0f) : Vector2.Zero;
        }

        return delta.Y < 0f ? new Vector2(0f, -1f) : delta.Y > 0f ? new Vector2(0f, 1f) : Vector2.Zero;
    }
}
