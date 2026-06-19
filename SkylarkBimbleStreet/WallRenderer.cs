namespace SkylarkBimbleStreet;

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

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
}
