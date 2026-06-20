namespace SkylarkBimbleStreet.Movement.WallFollowing;
/// <summary>
/// 片手壁伝い法の［壁直前］状態を表すクラス。
/// </summary>
internal sealed class BeforeWallFollowerState : IWallFollowerState
{
    public WallFollowerStateKind Kind => WallFollowerStateKind.BeforeWall;

    public void Move(WallFollowerContext context)
    {
        context.SetState(this);
        context.EnterFacingWall();
    }

    public void Continue(WallFollowerContext context) => context.SetState(this);
}
