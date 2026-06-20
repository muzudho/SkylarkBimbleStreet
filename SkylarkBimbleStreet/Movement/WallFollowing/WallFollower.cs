namespace SkylarkBimbleStreet.Movement.WallFollowing;

using System;
using Microsoft.Xna.Framework;
using SkylarkBimbleStreet;

internal delegate bool MoveWithoutRollerDelegate(Vector2 delta, out WallContact hitWallContact);

/// <summary>
///     <pre>
/// ［片手壁伝い法］のロジック。
/// 
///     ［入力方向］を受け取り、通常はその向きへ進み、壁に当たると［親壁方向］に沿う［進行方向］へ切り替える。［壁追従］ともいう。
/// 
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
///     - ［入力方向］：　コントローラーの入力そのものの方向。
///     - ［進行方向］：　プレイヤーが進む方向。
///     - ［親壁方向］：　プレイヤーからみて、プレイヤーが沿っている壁が有る方向。
/// 
/// ［先行ゴースト］という概念があり、種類がある。（ゴーストとは、プレイヤーのコピー）
/// 
///     - ［入力方向ゴースト］：　［入力方向］に１フレーム進んだ先の位置する。
///     - ［進行方向ゴースト］：　［進行方向］に１フレーム進んだ先に位置する。
///     - ［外角判定ゴースト］：　［進行方向ゴースト］から見てさらに［親壁方向］に１フレーム進んだ先の位置。
/// 
/// 壁には２種類ある。
/// 
///     - ［進行方向壁］：　［進行方向ゴースト］が食い込んだ壁。
///     - ［親壁］：　プレイヤーが沿っている壁。
/// 
/// いくつか状態がある。
/// 
///     - ［無反応］：　［進行方向壁］、［親壁］のいずれも無い。
///     - ［壁直前］：　［進行方向壁］が有り、［親壁］が無い。
///     - ［壁正面］：　［進行方向壁］、［親壁］のどちらも有る。
///     - ［壁沿い］：　［親壁］が有る。（※［親壁］に沿って移動している）
///         - ［壁沿い　＞　辺］　：　［進行方向壁］が無い状態。
///         - ［壁沿い　＞　外角］：　さらに、［外角判定ゴースト］がどの壁にも食い込まない状態。
///         - ［壁沿い　＞　内角］：　［進行方向壁］が有る状態。
///     </pre>
/// </summary>
internal sealed class WallFollower
{
    private readonly NoReactionWallFollowerState _noReactionState = new();
    private readonly BeforeWallFollowerState _beforeWallState = new();
    private readonly FacingWallFollowerState _facingWallState = new();
    private readonly AlongWallFollowerState _alongWallState = new();
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
    private WallContact _pendingWallFollowContact = WallContact.None;
    private Vector2 _pendingWallFollowContactDirection;
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

    public WallFollowerStateKind StateKind { get; private set; } = WallFollowerStateKind.NoReaction;

    public WallFollowerStateKind GetStateKind() => StateKind;

    internal IWallFollowerState NoReactionState => _noReactionState;

    internal IWallFollowerState AlongWallState => _alongWallState;

    internal bool TryMoveInputDirection(Vector2 delta, out WallContact hitWallContact) => _tryMoveWithoutRoller(delta, out hitWallContact);

    internal void SetState(IWallFollowerState state) => StateKind = state.Kind;

    internal void RememberWallParallelMoveForState(Vector2 delta) => RememberWallParallelMove(delta);

    internal void EnterBeforeWall(WallFollowerContext context, WallContact hitWallContact)
    {
        SetState(_beforeWallState);
        _beforeWallState.Move(context with { HitWallContact = hitWallContact });
    }

    internal void EnterFacingWall(WallFollowerContext context) => _facingWallState.Move(context);

    internal void StartWallFollow(WallFollowerContext context)
    {
        InputContactWallIndex = context.HitWallContact.WallIndex;
        WallFollowWallContact = context.HitWallContact;
        WallFollowHitContact = WallContact.None;
        _pendingWallFollowContact = context.HitWallContact;
        _pendingWallFollowContactDirection = GetPendingWallFollowContactDirection(context.HitWallContact, context.Delta);
    }

    internal void ContinueAlongWallForState(WallFollowerContext context)
    {
        if (context.RollerActive)
        {
            ContinueRollerWallFollowCore(context.Amount);
        }
        else
        {
            ContinueBasicWallFollowCore(context.Amount);
        }
    }

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
        SetState(_noReactionState);
    }

    public void Move(Vector2 delta, bool rollerActive)
    {
        if (TryStartPendingWallFollow(delta, rollerActive)) return;

        _noReactionState.Move(new WallFollowerContext(this, delta, 0f, rollerActive, WallContact.None));
    }

    public void ContinueBasicWallFollow(float amount)
    {
        if (!IsBasicWallFollowActive())
        {
            _noReactionState.Continue(new WallFollowerContext(this, Vector2.Zero, amount, false, WallContact.None));
            return;
        }

        _alongWallState.Continue(new WallFollowerContext(this, Vector2.Zero, amount, false, WallContact.None));
    }

    private void ContinueBasicWallFollowCore(float amount)
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
        if (!IsRollerWallFollowActive())
        {
            _noReactionState.Continue(new WallFollowerContext(this, Vector2.Zero, amount, true, WallContact.None));
            return;
        }

        _alongWallState.Continue(new WallFollowerContext(this, Vector2.Zero, amount, true, WallContact.None));
    }

    private void ContinueRollerWallFollowCore(float amount)
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

    private bool TryStartPendingWallFollow(Vector2 delta, bool rollerActive)
    {
        if (!_pendingWallFollowContact.IsValid(_walls.Count) || _pendingWallFollowContactDirection == Vector2.Zero) return false;

        var slideDirection = GetAxisDirection(delta);
        if (slideDirection == Vector2.Zero) return false;

        if (slideDirection == -_pendingWallFollowContactDirection)
        {
            ClearPendingWallFollow();
            return false;
        }

        if (!ArePerpendicular(slideDirection, _pendingWallFollowContactDirection))
        {
            SetState(_facingWallState);
            return true;
        }

        if (!HasWallNear(_pendingWallFollowContactDirection, _rollerWallProbeDistance))
        {
            ClearPendingWallFollow();
            return false;
        }

        if (rollerActive)
        {
            StartRollerWallFollow(_pendingWallFollowContactDirection, slideDirection);
        }
        else
        {
            StartBasicWallFollow(_pendingWallFollowContactDirection, slideDirection);
        }

        WallFollowWallContact = _pendingWallFollowContact;
        ClearPendingWallFollow();
        SetState(_alongWallState);
        return true;
    }

    private void StartBasicWallFollow(Vector2 contactDirection, Vector2 slideDirection)
    {
        _basicWallFollowContactDirection = contactDirection;
        _basicWallFollowSlideDirection = slideDirection;
        _basicWallFollowTurnDirection = GetWallFollowTurnDirection(contactDirection, slideDirection);
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

    private void StartRollerWallFollow(Vector2 contactDirection, Vector2 slideDirection)
    {
        _rollerContactDirection = contactDirection;
        _rollerSlideDirection = slideDirection;
        _rollerWallFollowTurnDirection = GetWallFollowTurnDirection(contactDirection, slideDirection);
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

    private static Vector2 GetPendingWallFollowContactDirection(WallContact contact, Vector2 fallbackDelta)
    {
        var contactDirection = GetDirectionFromWallContactSide(contact.Side);
        return contactDirection != Vector2.Zero ? contactDirection : GetAxisDirection(fallbackDelta);
    }

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
        ClearPendingWallFollow();
    }

    private void ClearPendingWallFollow()
    {
        _pendingWallFollowContact = WallContact.None;
        _pendingWallFollowContactDirection = Vector2.Zero;
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
