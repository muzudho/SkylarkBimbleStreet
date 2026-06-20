namespace SkylarkBimbleStreet;

/// <summary>
/// 片手壁伝い法の［壁正面］状態を表すクラス。
/// </summary>
internal sealed class FacingWallFollowerState : IWallFollowerState
{
    public WallFollowerStateKind Kind => WallFollowerStateKind.FacingWall;

    public void Move(WallFollowerContext context)
    {
        context.SetState(this);
        context.StartWallFollow();

        if (context.IsWallFollowActive())
        {
            context.SetAlongWallState();
        }
    }

    public void Continue(WallFollowerContext context) => context.SetState(this);
}
