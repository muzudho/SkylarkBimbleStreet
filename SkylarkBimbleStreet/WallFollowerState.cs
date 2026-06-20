namespace SkylarkBimbleStreet;

using Microsoft.Xna.Framework;

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
    public bool TryMove(out WallContact hitWallContact) => Owner.TryMoveForState(Delta, out hitWallContact);

    public void SetState(IWallFollowerState state) => Owner.SetState(state);

    public void RememberWallParallelMove() => Owner.RememberWallParallelMoveForState(Delta);

    public void MoveBeforeWall(WallContact hitWallContact) => Owner.MoveBeforeWallForState(this, hitWallContact);

    public void MoveFacingWallState() => Owner.MoveFacingWallStateForState(this);

    public void MoveFacingWall() => Owner.MoveFacingWallForState(this);

    public void ContinueAlongWall() => Owner.ContinueAlongWallForState(this);

    public bool IsWallFollowActive() => Owner.IsWallFollowActive(RollerActive);

    public IWallFollowerState NoReactionState => Owner.NoReactionState;

    public IWallFollowerState AlongWallState => Owner.AlongWallState;
}
