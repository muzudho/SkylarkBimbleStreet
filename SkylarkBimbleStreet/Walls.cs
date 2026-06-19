namespace SkylarkBimbleStreet;

using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

/// <summary>
/// 全部の壁
/// </summary>
internal sealed class Walls : IEnumerable<Wall>
{
    private readonly Wall[] _items;

    public Walls(Wall[] items)
    {
        _items = items ?? [];
    }

    /// <summary>
    /// 壁の数
    /// </summary>
    public int Count => _items.Length;

    public Wall this[int index] => _items[index];

    public bool IsValidIndex(int index) => index >= 0 && index < _items.Length;

    /// <summary>
    /// ぶつかった先頭の壁のインデックス
    /// </summary>
    /// <param name="bounds"></param>
    /// <returns></returns>
    public int FindCollidingIndex(Rectangle bounds)
    {
        for (var i = 0; i < _items.Length; i++)
        {
            if (bounds.Intersects(_items[i].Bounds)) return i;
        }

        return -1;
    }

    /// <summary>
    /// ぶつかった壁があるか。
    /// </summary>
    /// <param name="bounds"></param>
    /// <returns></returns>
    public bool Collides(Rectangle bounds) => FindCollidingIndex(bounds) >= 0;

    /// <summary>
    /// 少し広めに取って、ぶつかった壁のインデックス
    /// </summary>
    /// <param name="bounds"></param>
    /// <param name="direction"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public int FindNearIndex(Rectangle bounds, Vector2 direction, int distance)
    {
        if (direction == Vector2.Zero) return -1;

        var probe = bounds;
        probe.Offset((int)(direction.X * distance), (int)(direction.Y * distance));
        return FindCollidingIndex(probe);
    }

    public IEnumerator<Wall> GetEnumerator() => ((IEnumerable<Wall>)_items).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
