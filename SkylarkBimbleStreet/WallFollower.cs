namespace SkylarkBimbleStreet;

using System;
using Microsoft.Xna.Framework;

internal delegate bool MoveWithoutRollerDelegate(Vector2 delta, out WallContact hitWallContact);

/// <summary>
///     <pre>
/// ［片手壁伝い法］のロジック。［壁追従］ともいう。
/// 
/// ここでは、方向は４方向。斜め入力の場合は上下が優先されるものとする。
/// 
///     - 上
///     - 右
///     - 下
///     - 左
/// 
/// プレイヤーの進行方向は２種類ある。
/// 
///     - ［入力進行方向］：　コントローラーの入力そのものの方向。
///     - ［壁追従進行方向］：　壁に沿って進む方向。
///     - ［親壁方向］：　プレイヤーからみて、プレイヤーが沿っている壁が有る方向。
/// 
/// ［先行ゴースト］という概念があり、種類がある。（ゴーストとは、プレイヤーのコピー）
/// 
///     - ［入力進行方向ゴースト］：　［入力進行方向］に１フレーム進んだ先の位置する。
///     - ［壁追従進行方向ゴースト］：　［壁追従進行方向］に１フレーム進んだ先に位置する。
///     - ［未来予測ゴースト］：　［壁追従進行方向ゴースト］の［親壁方向］に１フレーム進んだ先の位置。
/// 
/// 壁には種類がある。
/// 
///     - ［入力進行方向壁］：　［入力進行方向ゴースト］が食い込んだ壁。
///     - ［触壁］：　［入力進行方向壁］のうち、［壁追従進行方向ゴースト］が食い込んでいない壁（親壁）。
///     - ［蹴壁］：　［入力進行方向壁］でも［触壁］でもないが、プレイヤーが沿う壁（親壁）。
/// 
/// いくつか状態がある。
/// 
///     - ［無反応］：　［入力進行方向壁］、［触壁］、［蹴壁］のいずれも無い。
///     - ［触壁直前］：　親壁が無く、［次壁］はある。
///     - ［触壁正面］：　［触壁］がある。
///     - ［触壁追従］：　［触壁］に沿って移動している。
///         - ［触壁追従　＞　初蹴壁直前］：　［未来予測ゴースト］がどの壁にも食い込まない状態。
///     - ［蹴壁追従］：　［蹴壁］に沿って移動している。
///         - ［蹴壁追従　＞　再蹴壁直前］：　［未来予測ゴースト］がどの壁にも食い込まない状態。
///     </pre>
/// </summary>
internal sealed class WallFollower
{
    private readonly Walls _walls;
    private readonly MoveWithoutRollerDelegate _tryMoveWithoutRoller;
    private readonly Func<Rectangle> _getPlayerBounds;
    private readonly int _rollerWallProbeDistance;
    private readonly int _wallContactProbeDistance;
    private readonly float _rollerSlideMultiplier;
    private readonly float _rollerCornerTurnMultiplier;
    private readonly float _basicWallFollowCornerTurnMultiplier;
    private readonly float _playerGhostPreviewFrames;
    private Vector2 _rollerContactDirection;
    private Vector2 _rollerSlideDirection;
    private int _rollerWallFollowTurnDirection = 1;
    private Vector2 _lastWallParallelContactDirection;
    private Vector2 _lastWallParallelMoveDirection;
    private Vector2 _basicWallFollowContactDirection;
    private Vector2 _basicWallFollowSlideDirection;
    private int _basicWallFollowTurnDirection = 1;
    private bool _wallFollowMovedThisFrame;

    public WallFollower(
        Walls walls,
        MoveWithoutRollerDelegate tryMoveWithoutRoller,
        Func<Rectangle> getPlayerBounds,
        int rollerWallProbeDistance,
        int wallContactProbeDistance,
        float rollerSlideMultiplier,
        float rollerCornerTurnMultiplier,
        float basicWallFollowCornerTurnMultiplier,
        float playerGhostPreviewFrames)
    {
        _walls = walls;
        _tryMoveWithoutRoller = tryMoveWithoutRoller;
        _getPlayerBounds = getPlayerBounds;
        _rollerWallProbeDistance = rollerWallProbeDistance;
        _wallContactProbeDistance = wallContactProbeDistance;
        _rollerSlideMultiplier = rollerSlideMultiplier;
        _rollerCornerTurnMultiplier = rollerCornerTurnMultiplier;
        _basicWallFollowCornerTurnMultiplier = basicWallFollowCornerTurnMultiplier;
        _playerGhostPreviewFrames = playerGhostPreviewFrames;
    }

    public WallContact WallFollowWallContact { get; private set; } = WallContact.None;

    public WallContact WallFollowHitContact { get; private set; } = WallContact.None;

    public int InputContactWallIndex { get; private set; } = -1;

    public Vector2 PlayerGhostVelocity { get; private set; }

    public void BeginMove()
    {
        _wallFollowMovedThisFrame = false;
        InputContactWallIndex = -1;
    }

    public void ResetForNoMove()
    {
        PlayerGhostVelocity = Vector2.Zero;
        InputContactWallIndex = -1;
        WallFollowHitContact = WallContact.None;
        ClearActiveWallFollow();
    }

    public void Move(Vector2 delta, bool rollerActive)
    {
        if (!_tryMoveWithoutRoller(delta, out var hitWallContact))
        {
            InputContactWallIndex = hitWallContact.WallIndex;
            WallFollowWallContact = hitWallContact;
            WallFollowHitContact = WallContact.None;
            if (rollerActive)
            {
                TryRollerSlide(delta);
            }
            else
            {
                TryBasicWallFollowSlide(delta);
            }

            return;
        }

        RememberWallParallelMove(delta);
    }

    public void ContinueBasicWallFollow(float amount)
    {
        if (amount <= 0f || _basicWallFollowContactDirection == Vector2.Zero || _basicWallFollowSlideDirection == Vector2.Zero) return;

        if (HasWallNear(_basicWallFollowContactDirection, _rollerWallProbeDistance))
        {
            if (_wallFollowMovedThisFrame) return;

            if (!_tryMoveWithoutRoller(_basicWallFollowSlideDirection * amount, out var hitWallContact))
            {
                if (TryBasicWallFollowTurnInnerCorner(_basicWallFollowSlideDirection, amount, _basicWallFollowTurnDirection, hitWallContact)) return;

                ClearBasicWallFollow();
                return;
            }

            _wallFollowMovedThisFrame = true;
            if (HasWallNear(_basicWallFollowContactDirection, _rollerWallProbeDistance)) return;
        }

        if (TryContinueWallFollowByGhostHit(amount, _basicWallFollowTurnDirection, false)) return;

        TryBasicWallFollowTurnCorner(_basicWallFollowSlideDirection, _basicWallFollowContactDirection, amount);
    }

    public void ContinueRollerWallFollow(float amount)
    {
        if (amount <= 0f || _rollerContactDirection == Vector2.Zero || _rollerSlideDirection == Vector2.Zero) return;

        if (HasWallNear(_rollerContactDirection, _rollerWallProbeDistance))
        {
            if (_wallFollowMovedThisFrame) return;

            if (!_tryMoveWithoutRoller(_rollerSlideDirection * amount, out var hitWallContact))
            {
                if (TryRollerTurnInnerCorner(_rollerSlideDirection, amount, _rollerWallFollowTurnDirection, hitWallContact)) return;

                ClearRollerWallFollow();
                return;
            }

            _wallFollowMovedThisFrame = true;
            if (HasWallNear(_rollerContactDirection, _rollerWallProbeDistance)) return;
        }

        if (TryContinueWallFollowByGhostHit(amount, _rollerWallFollowTurnDirection, true)) return;

        TryRollerTurnCorner(_rollerSlideDirection, _rollerContactDirection, amount * _rollerCornerTurnMultiplier);
    }

    public void UpdateGhostState(Vector2 velocity, bool rollerActive)
    {
        PlayerGhostVelocity = GetPlayerGhostVelocity(velocity, rollerActive);
        RefreshPlayerGhostHitContact();
    }

    public bool IsBasicWallFollowActive() => _basicWallFollowContactDirection != Vector2.Zero && _basicWallFollowSlideDirection != Vector2.Zero;

    public bool IsRollerWallFollowActive() => _rollerContactDirection != Vector2.Zero && _rollerSlideDirection != Vector2.Zero;

    public bool IsWallFollowActive(bool rollerActive) => rollerActive ? IsRollerWallFollowActive() : IsBasicWallFollowActive();

    private void TryBasicWallFollowSlide(Vector2 blockedDelta)
    {
        var amount = MathF.Max(MathF.Abs(blockedDelta.X), MathF.Abs(blockedDelta.Y));
        if (amount <= 0f) return;

        var contactDirection = GetAxisDirection(blockedDelta);
        var slideDirection = GetWallFollowSlideDirection(contactDirection);
        if (slideDirection == Vector2.Zero)
        {
            ClearBasicWallFollow();
            return;
        }

        var turnDirection = GetWallFollowTurnDirection(contactDirection, slideDirection);
        if (!_tryMoveWithoutRoller(slideDirection * amount, out var hitWallContact))
        {
            ClearBasicWallFollow();
            return;
        }

        if (!HasWallNear(contactDirection, _rollerWallProbeDistance))
        {
            ClearBasicWallFollow();
            return;
        }

        _wallFollowMovedThisFrame = true;
        _basicWallFollowContactDirection = contactDirection;
        _basicWallFollowSlideDirection = slideDirection;
        _basicWallFollowTurnDirection = turnDirection;
        RememberWallFollowWall(contactDirection);
    }

    private void TryBasicWallFollowTurnCorner(Vector2 slideDirection, Vector2 contactDirection, float amount)
    {
        if (amount <= 0f) return;

        if (!_tryMoveWithoutRoller(contactDirection * amount, out _))
        {
            ClearBasicWallFollow();
            return;
        }

        _wallFollowMovedThisFrame = true;
        var newContactDirection = -slideDirection;
        if (!HasWallNear(newContactDirection, _rollerWallProbeDistance))
        {
            _tryMoveWithoutRoller(newContactDirection * _rollerWallProbeDistance, out _);
        }

        _basicWallFollowContactDirection = newContactDirection;
        _basicWallFollowSlideDirection = GetWallFollowSlideDirection(newContactDirection, _basicWallFollowTurnDirection);
        RememberWallFollowWall(newContactDirection);
    }

    private bool TryBasicWallFollowTurnInnerCorner(Vector2 slideDirection, float amount, int turnDirection, WallContact hitWallContact)
    {
        var newContactDirection = slideDirection;
        var newSlideDirection = GetWallFollowSlideDirection(newContactDirection, turnDirection);
        if (newSlideDirection == Vector2.Zero)
        {
            return false;
        }

        if (!TryMoveWithoutRollerStepped(newSlideDirection * amount))
        {
            return false;
        }

        _wallFollowMovedThisFrame = true;
        _basicWallFollowContactDirection = newContactDirection;
        _basicWallFollowSlideDirection = newSlideDirection;
        _basicWallFollowTurnDirection = turnDirection;
        RememberWallFollowWall(newContactDirection, hitWallContact);
        return true;
    }

    private void TryRollerSlide(Vector2 blockedDelta)
    {
        var amount = MathF.Max(MathF.Abs(blockedDelta.X), MathF.Abs(blockedDelta.Y)) * _rollerSlideMultiplier;
        if (amount <= 0f) return;

        var contactDirection = GetAxisDirection(blockedDelta);
        var slideDirection = GetWallFollowSlideDirection(contactDirection);
        if (slideDirection == Vector2.Zero)
        {
            ClearRollerWallFollow();
            return;
        }

        var turnDirection = GetWallFollowTurnDirection(contactDirection, slideDirection);
        var slide = slideDirection * amount;
        if (!_tryMoveWithoutRoller(slide, out var hitWallContact))
        {
            ClearRollerWallFollow();
            return;
        }

        if (!HasWallNear(contactDirection, _rollerWallProbeDistance))
        {
            ClearRollerWallFollow();
            return;
        }

        _wallFollowMovedThisFrame = true;
        _rollerContactDirection = contactDirection;
        _rollerSlideDirection = slideDirection;
        _rollerWallFollowTurnDirection = turnDirection;
        RememberWallFollowWall(contactDirection);
    }

    private void TryRollerTurnCorner(Vector2 slideDirection, Vector2 contactDirection, float amount)
    {
        if (amount <= 0f) return;

        var turn = contactDirection * amount;
        if (!_tryMoveWithoutRoller(turn, out _)) return;

        _wallFollowMovedThisFrame = true;
        var newContactDirection = -slideDirection;
        if (!HasWallNear(newContactDirection, _rollerWallProbeDistance))
        {
            _tryMoveWithoutRoller(newContactDirection * _rollerWallProbeDistance, out _);
        }

        _rollerContactDirection = newContactDirection;
        _rollerSlideDirection = GetWallFollowSlideDirection(newContactDirection, _rollerWallFollowTurnDirection);
        RememberWallFollowWall(newContactDirection);
    }

    private bool TryRollerTurnInnerCorner(Vector2 slideDirection, float amount, int turnDirection, WallContact hitWallContact)
    {
        var newContactDirection = slideDirection;
        var newSlideDirection = GetWallFollowSlideDirection(newContactDirection, turnDirection);
        if (newSlideDirection == Vector2.Zero)
        {
            return false;
        }

        if (!TryMoveWithoutRollerStepped(newSlideDirection * amount))
        {
            return false;
        }

        _wallFollowMovedThisFrame = true;
        _rollerContactDirection = newContactDirection;
        _rollerSlideDirection = newSlideDirection;
        _rollerWallFollowTurnDirection = turnDirection;
        RememberWallFollowWall(newContactDirection, hitWallContact);
        return true;
    }

    private void RememberWallParallelMove(Vector2 delta)
    {
        var moveDirection = GetAxisDirection(delta);
        if (moveDirection == Vector2.Zero) return;

        var contactDirections = new[] { Vector2.UnitX, -Vector2.UnitX, Vector2.UnitY, -Vector2.UnitY };
        foreach (var contactDirection in contactDirections)
        {
            if (!ArePerpendicular(moveDirection, contactDirection) || !HasWallNear(contactDirection, _wallContactProbeDistance)) continue;

            _lastWallParallelContactDirection = contactDirection;
            _lastWallParallelMoveDirection = moveDirection;
            return;
        }
    }

    private Vector2 GetWallFollowSlideDirection(Vector2 contactDirection)
    {
        if (_lastWallParallelMoveDirection != Vector2.Zero
            && _lastWallParallelContactDirection == contactDirection
            && ArePerpendicular(_lastWallParallelMoveDirection, contactDirection))
        {
            return _lastWallParallelMoveDirection;
        }

        return GetWallFollowSlideDirection(contactDirection, 1);
    }

    private static Vector2 GetWallFollowSlideDirection(Vector2 contactDirection, int turnDirection) => GetRightOfDirection(contactDirection) * Math.Sign(turnDirection == 0 ? 1 : turnDirection);

    private static int GetWallFollowTurnDirection(Vector2 contactDirection, Vector2 slideDirection)
    {
        var right = GetRightOfDirection(contactDirection);
        return Vector2.Dot(right, slideDirection) >= 0f ? 1 : -1;
    }

    private static Vector2 GetAxisDirection(Vector2 delta)
    {
        if (MathF.Abs(delta.X) >= MathF.Abs(delta.Y))
        {
            return delta.X < 0f ? new Vector2(-1f, 0f) : delta.X > 0f ? new Vector2(1f, 0f) : Vector2.Zero;
        }

        return delta.Y < 0f ? new Vector2(0f, -1f) : delta.Y > 0f ? new Vector2(0f, 1f) : Vector2.Zero;
    }

    private static Vector2 GetRightOfDirection(Vector2 direction)
    {
        if (direction == Vector2.Zero)
        {
            return Vector2.Zero;
        }

        return new Vector2(-direction.Y, direction.X);
    }

    private static Vector2 GetDirectionFromWallContactSide(WallContactSide side) => side switch
    {
        WallContactSide.Left => Vector2.UnitX,
        WallContactSide.Right => -Vector2.UnitX,
        WallContactSide.Top => Vector2.UnitY,
        WallContactSide.Bottom => -Vector2.UnitY,
        _ => Vector2.Zero,
    };

    private static WallContact CreateWallContact(int wallIndex, Vector2 contactDirection) => new(wallIndex, GetWallContactSide(contactDirection));

    private static WallContactSide GetWallContactSide(Vector2 contactDirection)
    {
        var direction = GetAxisDirection(contactDirection);
        if (direction.X > 0f) return WallContactSide.Left;
        if (direction.X < 0f) return WallContactSide.Right;
        if (direction.Y > 0f) return WallContactSide.Top;
        if (direction.Y < 0f) return WallContactSide.Bottom;
        return WallContactSide.None;
    }

    private static bool ArePerpendicular(Vector2 a, Vector2 b) => a != Vector2.Zero && b != Vector2.Zero && MathF.Abs(Vector2.Dot(a, b)) < 0.001f;

    private bool TryContinueWallFollowByGhostHit(float amount, int turnDirection, bool rollerActive)
    {
        if (!WallFollowHitContact.IsValid(_walls.Count)) return false;

        var newContactDirection = GetDirectionFromWallContactSide(WallFollowHitContact.Side);
        var newSlideDirection = GetWallFollowSlideDirection(newContactDirection, turnDirection);
        if (newContactDirection == Vector2.Zero || newSlideDirection == Vector2.Zero) return false;

        if (!TryMoveWithoutRollerStepped(newSlideDirection * amount)) return false;

        _wallFollowMovedThisFrame = true;
        if (rollerActive)
        {
            _rollerContactDirection = newContactDirection;
            _rollerSlideDirection = newSlideDirection;
            _rollerWallFollowTurnDirection = turnDirection;
        }
        else
        {
            _basicWallFollowContactDirection = newContactDirection;
            _basicWallFollowSlideDirection = newSlideDirection;
            _basicWallFollowTurnDirection = turnDirection;
        }

        WallFollowWallContact = CreateWallContact(WallFollowHitContact.WallIndex, newContactDirection);
        return true;
    }

    private bool TryMoveWithoutRollerStepped(Vector2 delta)
    {
        if (_tryMoveWithoutRoller(delta, out _))
        {
            return true;
        }

        foreach (var scale in new[] { 0.5f, 0.25f, 0.125f })
        {
            if (_tryMoveWithoutRoller(delta * scale, out _))
            {
                return true;
            }
        }

        var direction = GetAxisDirection(delta);
        return direction != Vector2.Zero && _tryMoveWithoutRoller(direction, out _);
    }

    private bool HasWallNear(Vector2 direction, int distance) => _walls.HasCollisionNear(_getPlayerBounds(), direction, distance);

    private void ClearActiveWallFollow()
    {
        ClearRollerWallFollow();
        ClearBasicWallFollow();
    }

    private void ClearBasicWallFollow()
    {
        _basicWallFollowContactDirection = Vector2.Zero;
        _basicWallFollowSlideDirection = Vector2.Zero;
        _basicWallFollowTurnDirection = 1;
        ClearWallFollowWallIndexes();
    }

    private void ClearRollerWallFollow()
    {
        _rollerContactDirection = Vector2.Zero;
        _rollerSlideDirection = Vector2.Zero;
        _rollerWallFollowTurnDirection = 1;
        ClearWallFollowWallIndexes();
    }

    private void ClearWallFollowWallIndexes()
    {
        WallFollowWallContact = WallContact.None;
        WallFollowHitContact = WallContact.None;
    }

    private void RememberWallFollowWall(Vector2 contactDirection, WallContact? fallbackContact = null)
    {
        var wallIndex = _walls.FindNearIndex(_getPlayerBounds(), contactDirection, _rollerWallProbeDistance);
        WallFollowWallContact = _walls.IsValidIndex(wallIndex)
            ? CreateWallContact(wallIndex, contactDirection)
            : fallbackContact ?? WallContact.None;
    }

    private void RefreshPlayerGhostHitContact()
    {
        if (PlayerGhostVelocity == Vector2.Zero)
        {
            WallFollowHitContact = WallContact.None;
            return;
        }

        var hitWallIndex = _walls.FindCollidingIndex(GetPlayerGhostBounds());
        WallFollowHitContact = hitWallIndex >= 0
            ? CreateWallContact(hitWallIndex, PlayerGhostVelocity)
            : WallContact.None;
    }

    private Rectangle GetPlayerGhostBounds()
    {
        var ghost = _getPlayerBounds();
        ghost.Offset((int)MathF.Round(PlayerGhostVelocity.X * _playerGhostPreviewFrames), (int)MathF.Round(PlayerGhostVelocity.Y * _playerGhostPreviewFrames));
        return ghost;
    }

    private Vector2 GetPlayerGhostVelocity(Vector2 velocity, bool rollerActive)
    {
        var amount = MathF.Max(MathF.Abs(velocity.X), MathF.Abs(velocity.Y));
        if (rollerActive && IsRollerWallFollowActive()) return _rollerSlideDirection * amount * _rollerSlideMultiplier;
        if (!rollerActive && IsBasicWallFollowActive()) return _basicWallFollowSlideDirection * amount * _basicWallFollowCornerTurnMultiplier;
        return velocity;
    }
}
