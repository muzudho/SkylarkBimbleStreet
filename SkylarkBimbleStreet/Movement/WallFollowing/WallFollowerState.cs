namespace SkylarkBimbleStreet.Movement.WallFollowing;

using Microsoft.Xna.Framework;
using SkylarkBimbleStreet;

internal enum WallFollowerStateKind
{
    NoReaction,
    BeforeWall,
    FacingWall,
    AlongWall,
}

internal interface IWallFollowerState
{
    WallFollowerStateKind Kind { get; }

    void Move(WallFollowerContext context);

    void Continue(WallFollowerContext context);
}

internal readonly record struct WallFollowerContext(WallFollower Owner, Vector2 Delta, float Amount, bool RollerActive, WallContact HitWallContact)
{
    public bool TryMoveInputDirection(out WallContact hitWallContact) => Owner.TryMoveInputDirection(Delta, out hitWallContact);

    public void SetState(IWallFollowerState state) => Owner.SetState(state);

    public void RememberWallParallelMove() => Owner.RememberWallParallelMoveForState(Delta);

    public void EnterBeforeWall(WallContact hitWallContact) => Owner.EnterBeforeWall(this, hitWallContact);

    public void EnterFacingWall() => Owner.EnterFacingWall(this);

    public void StartWallFollow() => Owner.StartWallFollow(this);

    public void ContinueAlongWall() => Owner.ContinueAlongWallForState(this);

    public bool IsWallFollowActive() => Owner.IsWallFollowActive(RollerActive);

    public void SetNoReactionState() => Owner.SetState(Owner.NoReactionState);

    public void SetAlongWallState() => Owner.SetState(Owner.AlongWallState);
}
