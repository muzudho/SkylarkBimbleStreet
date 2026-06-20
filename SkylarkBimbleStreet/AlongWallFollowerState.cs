namespace SkylarkBimbleStreet;

/// <summary>
/// 片手壁伝い法の［壁沿い］状態を表すクラス。
/// </summary>
internal sealed class AlongWallFollowerState : IWallFollowerState
{
    public WallFollowerStateKind Kind => WallFollowerStateKind.AlongWall;

    public void Move(WallFollowerContext context) => context.SetState(this);

    public void Continue(WallFollowerContext context)
    {
        context.SetState(this);
        context.ContinueAlongWall();

        context.SetState(context.IsWallFollowActive() ? this : context.NoReactionState);
    }
}
